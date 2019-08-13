using HorribleSubsDownload.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

namespace HorribleSubsDownload
{
    public partial class MainWindow : Window
    {
        const string res720p = "http://www.horriblesubs.info/rss.php?res=720";
        const string res1080p = "http://www.horriblesubs.info/rss.php?res=1080";
        const string currentSeason = "https://horriblesubs.info/current-season/";
        public ObservableCollection<Title> ListOfTitles { get; set; }
        SyndicationFeed feed = new SyndicationFeed();
        Timer Timer = null;

        public MainWindow()
        {
            InitializeComponent();
            LoadAppSettings();
            LoadTitles();
            LoadRSS();
        }

        private void LoadAppSettings()
        {
            MySettings.Get();

            if (MySettings.TitleDictionary == null)
            {
                MySettings.TitleDictionary = new Dictionary<string, string>();
            }

            if (string.IsNullOrEmpty(MySettings.Resolution))
            {
                MySettings.Resolution = res720p;
            }

            Url.Text = MySettings.Resolution;
            Res720.IsChecked = MySettings.Resolution == res720p;
            Res1080.IsChecked = MySettings.Resolution == res1080p;
            AutoDownloadMinutes.Text = MySettings.AutoDownloadMinutes.ToString();
            Timer = new Timer(MySettings.AutoDownloadMinutes * 60 * 1000);
            Timer.Elapsed += new ElapsedEventHandler(AutoDownloadTimer);
            AutoDownload.IsChecked = MySettings.AutoDownload;
        }

        private void LoadTitles()
        {
            var web = new HtmlWeb();
            var doc = web.Load(currentSeason);
            var items = doc.DocumentNode.SelectNodes("//div[@class='ind-show']/a");
            ListOfTitles = new ObservableCollection<Title>();
            foreach (var item in items)
            {
                string name = WebUtility.HtmlDecode(item.GetAttributeValue("title", item.InnerText).Trim());
                ListOfTitles.Add(new Title
                {
                    Name = name,
                    IsChecked = MySettings.TitleDictionary.ContainsKey(name.ReplaceSpecialCharacters())
                });
            }
            DataContext = this;
        }

        private void LoadRSS()
        {
            using (XmlReader reader = XmlReader.Create(Url.Text))
            {
                feed = SyndicationFeed.Load(reader);
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            feed.Items = feed.Items.Reverse();
            foreach (SyndicationItem item in feed.Items)
            {
                string subject = item.Title.Text;
                string link = item.Links[0].Uri.ToString();

                string name = subject;
                name = name.Replace("[HorribleSubs]", "");
                name = Regex.Replace(name, @"- \d+(\.\d+)? \[(720p|1080p)\].mkv", "");
                name = name.ReplaceSpecialCharacters();

                string number = Regex.Match(subject, @"- \d+(\.\d+)? \[(720p|1080p)\].mkv").Value;
                number = Regex.Match(number, @"\d+(\.\d+)?").Value;
                number = number.Trim();
                string numberValue = number;

                if (MySettings.TitleDictionary.ContainsKey(name))
                {
                    bool savedNumberConvertSuccess = decimal.TryParse(MySettings.TitleDictionary[name], out decimal savedNumber);
                    bool newNumberConvertSuccess = decimal.TryParse(numberValue, out decimal newNumber);
                    if (savedNumberConvertSuccess && newNumberConvertSuccess)
                    {
                        if (savedNumber < newNumber)
                        {
                            MySettings.TitleDictionary[name] = numberValue;
                            MySettings.Save();
                            Process.Start(link);
                        }
                    }
                    else
                    {
                        if (MySettings.TitleDictionary[name] != numberValue)
                        {
                            MySettings.TitleDictionary[name] = numberValue;
                            MySettings.Save();
                            Process.Start(link);
                        }
                    }
                }
            }
        }

        private void Res720_Checked(object sender, RoutedEventArgs e)
        {
            if (Url.Text != res720p)
            {
                Res1080.IsChecked = false;
                Url.Text = res720p;
                ResetTitleValues();
                MySettings.Resolution = res720p;
                MySettings.Save();
            }
        }

        private void Res1080_Checked(object sender, RoutedEventArgs e)
        {
            if (Url.Text != res1080p)
            {
                Res720.IsChecked = false;
                Url.Text = res1080p;
                ResetTitleValues();
                MySettings.Resolution = res1080p;
                MySettings.Save();
            }
        }

        private void ResetTitleValues()
        {
            foreach (var title in MySettings.TitleDictionary.Keys.ToList())
            {
                MySettings.TitleDictionary[title] = "-1";
            }
        }

        //AutoDownload
        private void AutoDownload_Checked(object sender, RoutedEventArgs e)
        {
            Timer.Enabled = true;
            AutoDownloadMinutes.IsEnabled = true;
            MySettings.AutoDownload = true;
            MySettings.Save();
        }
        private void AutoDownload_Unchecked(object sender, RoutedEventArgs e)
        {
            Timer.Enabled = false;
            AutoDownloadMinutes.IsEnabled = false;
            MySettings.AutoDownload = false;
            MySettings.Save();
        }

        private void AutoDownloadTimer(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Timer.Interval = MySettings.AutoDownloadMinutes * 60 * 1000;
                LoadRSS();
                Download_Click(null, null);
            }), DispatcherPriority.ContextIdle);
        }

        private void AutoDownloadMinutes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Int32.TryParse(AutoDownloadMinutes.Text, out int minutes))
            {
                if (minutes >= 1 && minutes <= 60)
                {
                    MySettings.AutoDownloadMinutes = minutes;
                    MySettings.Save();
                    return;
                }
            }
            AutoDownloadMinutes.Text = MySettings.AutoDownloadMinutes.ToString();
        }

        //Check Uncheck Titles
        private void TitleChecked(object sender, RoutedEventArgs e)
        {
            CheckBox title = (CheckBox)sender;
            string name = title.Content.ToString().ReplaceSpecialCharacters();
            if (!MySettings.TitleDictionary.ContainsKey(name))
            {
                MySettings.TitleDictionary.Add(name, "-1");
            }
            MySettings.Save();
        }

        private void TitleUnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox show = (CheckBox)sender;
            string name = show.Content.ToString().ReplaceSpecialCharacters();
            if (MySettings.TitleDictionary.ContainsKey(name))
            {
                MySettings.TitleDictionary.Remove(name);
            }
            MySettings.Save();
        }

        //Minimize to Tray
        private void TrayIconDoubleClick(object sender, RoutedEventArgs e)
        {
            Open(sender, e);
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            base.OnStateChanged(e);
        }
    }
}
