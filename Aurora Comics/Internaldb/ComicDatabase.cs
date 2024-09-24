using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aurora_Comics.Classes;

namespace Aurora_Comics.Internaldb
{
    public class ComicDatabase
    {
        readonly SQLiteAsyncConnection _database;

        public ComicDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<MyComic>().Wait();
            _database.CreateTableAsync<ImageData>().Wait();
        }

        public Task<List<MyComic>> GetComicsAsync()
        {
            return _database.Table<MyComic>().ToListAsync();
        }

        public Task<MyComic> GetComicByIdAsync(int id)
        {
            return _database.Table<MyComic>()
                            .Where(i => i.ID == id)
                            .FirstOrDefaultAsync();
        }

        public Task<MyComic> GetComicByFileNameAsync(string fileName)
        {
            return _database.Table<MyComic>()
                            .Where(i => i.FileName == fileName)
                            .FirstOrDefaultAsync();
        }

        public Task<int> SaveComicAsync(MyComic comic)
        {
            if (comic.ID != 0)
            {
                return _database.UpdateAsync(comic);
            }
            else
            {
                return _database.InsertAsync(comic);
            }
        }

        public Task<int> DeleteComicAsync(MyComic comic)
        {
            return _database.DeleteAsync(comic);
        }

        public Task<List<ImageData>> GetImagesByComicTitleAsync(string comicTitle)
        {
            return _database.Table<ImageData>()
                            .Where(i => i.ComicTitle == comicTitle)
                            .ToListAsync();
        }

        public Task<int> SaveImageAsync(ImageData image)
        {
            if (image.ID != 0)
            {
                return _database.UpdateAsync(image);
            }
            else
            {
                return _database.InsertAsync(image);
            }
        }

        public Task<int> DeleteImageAsync(ImageData image)
        {
            return _database.DeleteAsync(image);
        }
    }
}