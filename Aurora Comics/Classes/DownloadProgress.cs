using System;
namespace Aurora_Comics.Classes
{
    public class DownloadProgress
    {
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public long Speed { get; set; }
    }
}

