using System.Net.Http.Json;
using Aurora_Comics.Classes;

namespace Aurora_Comics.Service
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(Constants.BaseUrl)
            };
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<List<Comics>> SearchComics(string searchTerm)
        {
            return await GetAsync<List<Comics>>($"{Constants.SearchComicsEndpoint}/{searchTerm}");
        }

        public async Task<string> GetDownloadLink(string postLink)
        {
            return await GetAsync<string>($"{Constants.GetDownloadLinkEndpoint}?postLink={postLink}");
        }
        
        public async Task<downloadedFile> DownloadFile(string downloadLink, IProgress<DownloadProgressInfo> progress = null)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(downloadLink, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var contentLength = response.Content.Headers.ContentLength ?? -1L;
                    var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[8192];
                        var totalBytesRead = 0L;
                        var bytesRead = 0;
                        var fileBytes = new List<byte>();

                        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                        {
                            fileBytes.AddRange(buffer.Take(bytesRead));
                            totalBytesRead += bytesRead;

                            if (progress != null)
                            {
                                progress.Report(new DownloadProgressInfo
                                {
                                    Progress = (double)totalBytesRead / contentLength,
                                    Speed = CalculateSpeed(totalBytesRead, DateTime.Now.TimeOfDay)
                                });
                            }
                        }

                        return new downloadedFile
                        {
                            fileName = fileName ?? "unknown",
                            fileBytes = fileBytes.ToArray()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return null;
            }
        }

        private double CalculateSpeed(long bytesRead, TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds == 0)
                return 0;
            return (bytesRead / 1024.0 / 1024.0) / elapsed.TotalSeconds; // MB/s
        }
    }
}