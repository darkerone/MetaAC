using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAcoustid
{
    public class MetadatasAcoustid
    {
        public string status { get; set; }
        public List<Result> results { get; set; }
    }

    public class Artist
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Track
    {
        public int position { get; set; }
        public List<Artist> artists { get; set; }
        public string id { get; set; }
        public string title { get; set; }
    }

    public class Medium
    {
        public int position { get; set; }
        public List<Track> tracks { get; set; }
        public int track_count { get; set; }
        public string format { get; set; }
    }

    public class Release
    {
        public List<Medium> mediums { get; set; }
        public string id { get; set; }
    }

    public class Releasegroup
    {
        public string id { get; set; }
        public List<Release> releases { get; set; }
    }

    public class Recording
    {
        public int sources { get; set; }
        public List<Releasegroup> releasegroups { get; set; }
        public string id { get; set; }
    }

    public class Result
    {
        public List<Recording> recordings { get; set; }
        public double score { get; set; }
        public string id { get; set; }
    }



}
