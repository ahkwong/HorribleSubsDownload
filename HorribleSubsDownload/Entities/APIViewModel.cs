using System.Collections.Generic;

namespace HorribleSubsDownload.Entities
{
    public class APIViewModel
    {
        public Schedule Schedule { get; set; }
        public string TimeZone { get; set; }
    }

    public class Schedule
    {
        public List<Anime> Monday { get; set; }
        public List<Anime> Tuesday { get; set; }
        public List<Anime> Wednesday { get; set; }
        public List<Anime> Thursday { get; set; }
        public List<Anime> Friday { get; set; }
        public List<Anime> Saturday { get; set; }
        public List<Anime> Sunday { get; set; }
    }

    public class Anime
    {
        public string ImgURL { get; set; }
        public string Page { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
    }
}
