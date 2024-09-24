using System;
namespace Aurora_Comics.Classes
{
    public class ComicService
    {
        private const string ComicKey = "MyComics";
        private List<MyComic> comics;

        public ComicService()
        {
            var json = Preferences.Get(ComicKey, string.Empty);
            comics = string.IsNullOrEmpty(json) ? new List<MyComic>() : System.Text.Json.JsonSerializer.Deserialize<List<MyComic>>(json);
        }

        public MyComic GetComic(string title, string category)
        {
            return comics.FirstOrDefault(c => c.Title == title && c.Category == category);
        }

        public void AddComic(MyComic comic)
        {
            comics.Add(comic);
            SaveComics();
        }

        private void SaveComics()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(comics);
            Preferences.Set(ComicKey, json);
        }
    }

}

