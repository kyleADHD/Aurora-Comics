using System.ComponentModel;
using System.IO.Compression;
using SharpCompress.Archives.Rar;
using Aurora_Comics.Classes;
using Aurora_Comics.Service;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Aurora_Comics
{
    public partial class EReader : ContentPage, INotifyPropertyChanged
    {
        private byte[] fileData;
        private string fileName;
        private string postLink;
        public ComicViewModel ViewModel { get; set; }
        private List<ImageData> imageDataList;
        private double startScale = 1;
        const double MIN_SCALE = 1;
        const double MAX_SCALE = 3;
        private readonly ApiService _apiService;

        public bool IsDownloading { get; set; }
        public DownloadProgressInfo DownloadProgressInfo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EReader(string title, string postLink)
        {
            InitializeComponent();
            this.fileName = title;
            this.postLink = postLink;
            ViewModel = new ComicViewModel();
            _apiService = new ApiService();
            BindingContext = this;
            LoadComic();
        }

        private async void LoadComic()
        {
            var existingComic = await App.ComicDatabase.GetComicByFileNameAsync(fileName);

            if (existingComic != null)
            {
                fileData = Convert.FromBase64String(existingComic.ComicBase64);
                await LoadImages();
            }
            else
            {
                await DownloadComic();
            }
        }

        private async Task DownloadComic()
        {
            IsDownloading = true;
            OnPropertyChanged(nameof(IsDownloading));

            try
            {
                var downloadLink = await _apiService.GetDownloadLink(postLink);
                var progress = new Progress<DownloadProgressInfo>(p =>
                {
                    DownloadProgressInfo = p;
                    OnPropertyChanged(nameof(DownloadProgressInfo));
                });

                var downloadedFile = await _apiService.DownloadFile(downloadLink, progress);

                if (downloadedFile != null)
                {
                    fileData = downloadedFile.fileBytes;
                    fileName = downloadedFile.fileName;

                    var myComic = new MyComic
                    {
                        Title = fileName,
                        FileName = fileName,
                        ComicBase64 = Convert.ToBase64String(fileData)
                    };

                    await App.ComicDatabase.SaveComicAsync(myComic);

                    await LoadImages();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to download the comic", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsDownloading = false;
                OnPropertyChanged(nameof(IsDownloading));
            }
        }

        private async Task LoadImages()
        {
            imageDataList = await App.ComicDatabase.GetImagesByComicTitleAsync(fileName);

            if (imageDataList != null && imageDataList.Count > 0)
            {
                foreach (var imageData in imageDataList)
                {
                    ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageData.Data)));
                    ViewModel.Images.Add(new Images { Name = fileName, MySource = imageSource });
                }
            }
            else if (fileName.EndsWith(".cbr"))
            {
                await ConvertCBRtoImages();
            }
            else if (fileName.EndsWith(".cbz"))
            {
                await ConvertCBZtoImages();
            }

            Collection.Position = 0;
        }

        private async Task ConvertCBRtoImages()
        {
            try
            {
                using (var stream = new MemoryStream(fileData))
                using (var archive = RarArchive.Open(stream))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        string path = entry.Key;
                        if (!path.Contains(".xml"))
                        {
                            using (Stream imageStream = entry.OpenEntryStream())
                            using (MemoryStream mem = new MemoryStream())
                            {
                                await imageStream.CopyToAsync(mem);
                                mem.Position = 0;

                                using (var image = await Image.LoadAsync(mem))
                                using (var imageStreamForBase64 = new MemoryStream())
                                {
                                    await image.SaveAsPngAsync(imageStreamForBase64);
                                    var imageBase64 = Convert.ToBase64String(imageStreamForBase64.ToArray());
                                    var imageData = new ImageData { ComicTitle = fileName, Data = imageBase64 };
                                    await App.ComicDatabase.SaveImageAsync(imageData);
                                    ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageBase64)));
                                    ViewModel.Images.Add(new Images { Name = path, MySource = imageSource });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await DisplayAlert("Error", "Failed to process CBR file", "OK");
            }
        }

        private async Task ConvertCBZtoImages()
        {
            try
            {
                using (var stream = new MemoryStream(fileData))
                using (var archive = new ZipArchive(stream))
                {
                    foreach (var entry in archive.Entries.Where(entry => !string.IsNullOrWhiteSpace(entry.Name)))
                    {
                        string path = entry.FullName;
                        if (!path.Contains(".xml"))
                        {
                            using (Stream imageStream = entry.Open())
                            using (MemoryStream mem = new MemoryStream())
                            {
                                await imageStream.CopyToAsync(mem);
                                mem.Position = 0;

                                using (var image = await Image.LoadAsync(mem))
                                using (var imageStreamForBase64 = new MemoryStream())
                                {
                                    await image.SaveAsPngAsync(imageStreamForBase64);
                                    var imageBase64 = Convert.ToBase64String(imageStreamForBase64.ToArray());
                                    var imageData = new ImageData { ComicTitle = fileName, Data = imageBase64 };
                                    await App.ComicDatabase.SaveImageAsync(imageData);
                                    ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageBase64)));
                                    ViewModel.Images.Add(new Images { Name = path, MySource = imageSource });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await DisplayAlert("Error", "Failed to process CBZ file", "OK");
            }
        }

        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            var image = (Microsoft.Maui.Controls.Image)sender;

            if (e.Status == GestureStatus.Started)
            {
                startScale = image.Scale;
            }
            else if (e.Status == GestureStatus.Running)
            {
                double currentScale = startScale * e.Scale;
                currentScale = Math.Max(MIN_SCALE, currentScale);
                currentScale = Math.Min(currentScale, MAX_SCALE);

                image.Scale = currentScale;
            }
        }

        private void Collection_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            int currentIndex = ViewModel.Images.IndexOf(e.CurrentItem as Images) + 1;
            ViewModel.PageCountString = $"{currentIndex}/{ViewModel.Images.Count}";
        }

        async void BackImage_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ComicViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Images> Images { get; set; } = new List<Images>();

        private string pageCountString;
        public string PageCountString
        {
            get { return pageCountString; }
            set
            {
                if (pageCountString != value)
                {
                    pageCountString = value;
                    OnPropertyChanged(nameof(PageCountString));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Images
    {
        public string Name { get; set; }
        public ImageSource MySource { get; set; }
    }
}