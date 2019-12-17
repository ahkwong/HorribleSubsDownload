using Newtonsoft.Json;
using System.Collections.Generic;

namespace HorribleSubsDownload
{
    public class MySettings
    {
        public static string Resolution { get; set; }
        public static Dictionary<string, string> TitleDictionary { get; set; }
        public static bool AutoDownload { get; set; }
        public static int AutoDownloadMinutes { get; set; }

        public static void Get()
        {
            Resolution = Properties.Settings.Default.Resolution;
            TitleDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Settings.Default.TitleDictionary);
            AutoDownload = Properties.Settings.Default.AutoDownload;
            AutoDownloadMinutes = Properties.Settings.Default.AutoDownloadMinutes;
        }

        public static void Save()
        {
            Properties.Settings.Default.Resolution = Resolution;
            Properties.Settings.Default.TitleDictionary = JsonConvert.SerializeObject(TitleDictionary);
            Properties.Settings.Default.AutoDownload = AutoDownload;
            Properties.Settings.Default.AutoDownloadMinutes = AutoDownloadMinutes;
            Properties.Settings.Default.Save();
            //a comment
        }
    }
}
