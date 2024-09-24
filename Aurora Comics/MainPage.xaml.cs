using System.Collections.ObjectModel;
using Aurora_Comics.Classes;
using Aurora_Comics.Service;

namespace Aurora_Comics;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;
    public ObservableCollection<Comics> comics { get; set; }
    public bool IsLoading { get; set; }

    public MainPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        comics = new ObservableCollection<Comics>();
        ImageCollectionView.ItemsSource = comics;
        BindingContext = this;
        SetInfo();
    }

    public async void SetInfo()
    {
        IsLoading = true;
        var fetchedComics = await _apiService.GetAsync<List<Comics>>("Comics/GetComicsLanding");
        foreach (var comic in fetchedComics)
        {
            comic.Title = System.Net.WebUtility.HtmlDecode(comic.Title);
            if (comic.Title.IndexOf("Weekly Pack", StringComparison.OrdinalIgnoreCase) < 0)
            {
                comics.Add(comic);
            }
        }
        IsLoading = false;
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var search = e.NewTextValue;
        if (!string.IsNullOrEmpty(search))
        {
            var searchResults = await GetComicsSearch(search);
            if (searchResults != null)
            {
                comics.Clear();
                foreach (var comic in searchResults.Where(c => !c.Title.Contains("Weekly Pack", StringComparison.OrdinalIgnoreCase)))
                {
                    comics.Add(comic);
                }
            }
        }
        else
        {
            SetInfo();
        }
    }

    public async Task<List<Comics>> GetComicsSearch(string search)
    {
        IsLoading = true;

        try
        {
            var comics = await _apiService.GetAsync<List<Comics>>($"Comics/SearchComics/search/{search}");
            return comics;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to search for comics", "OK");
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async void ImageCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Comics selectedComic)
        {
            // Reset the selection
            ((CollectionView)sender).SelectedItem = null;

            await Navigation.PushModalAsync(new EReader(selectedComic.Title, selectedComic.PostLink));
        }
    }
}