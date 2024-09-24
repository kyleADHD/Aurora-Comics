namespace Aurora_Comics;
using Aurora_Comics.Internaldb;
public partial class App : Application
{
    static ComicDatabase comicDatabase;

    public static ComicDatabase ComicDatabase
    {
        get
        {
            if (comicDatabase == null)
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                string dbPath = Path.Combine(folderPath, "Comics.db3");
                comicDatabase = new ComicDatabase(dbPath);
            }
            return comicDatabase;
        }
    }
    public App()
    {
        InitializeComponent();

        MainPage = new Shell();
    }
}