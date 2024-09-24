using System;
using SQLite;

namespace Aurora_Comics.Classes
{
    public class MyComic
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string ComicBase64 { get; set; }
    }

    public class ImageData
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Indexed]
        public string ComicTitle { get; set; }

        public string Data { get; set; } // Base64 String
    }
}