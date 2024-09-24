using System;
using SQLite;

namespace Aurora_Comics.Classes
{
    public class Comics
    {
       
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PostLink { get; set; }
        public string ImageUrl { get; set; }
    }
}

