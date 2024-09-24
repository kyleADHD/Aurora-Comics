using System;
using SQLite;

namespace Aurora_Comics.Classes
{
    public class ComicImages
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ComicTitle { get; set; }
        public string ImagesJson { get; set; }
    }
}

