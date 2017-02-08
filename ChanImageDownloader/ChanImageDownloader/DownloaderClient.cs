using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ChanImageDownloader
{
    public class DownloaderClient
    {
        private WebClient _client;
        private MainWindow _mWindow;
        private int _downloadedImagesCount = 0;
        private Uri[] _imageUrisGuess = new Uri[1000];
        private Uri[] _imageUrisFinal;

        // http://boards.4chan.org/pol/thread/111395819/a-stable-multicultural-liberal-democracy-with

        public DownloaderClient() { }

        public DownloaderClient(MainWindow mainWindow)
        {
            _mWindow = mainWindow;
            _client = new WebClient();
            _client.Encoding = Encoding.Default;
            _client.Headers.Add("Accept-Language", " en-US");
            _client.Headers.Add("Accept", " text/html, application/xhtml+xml, */*");
            _client.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");
        }

        public void DownloadHtml(string url)     // Downloads the source of the html page -- enters AS main page
        {
            _mWindow.UpdateText("Fetching page source");

            string htmlResponse = "";

            Console.WriteLine(url);

            CookieContainer container = new CookieContainer();
            container.Add(new Uri("http://ylilauta.org"), new Cookie("cookieconsent", "true"));
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                webReq.CookieContainer = container;
                webReq.Method = "GET";
                webReq.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
                    "Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
                    ".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
                    "InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";
                WebResponse webRes = webReq.GetResponse();
                Stream stream = webRes.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                htmlResponse = sr.ReadToEnd();
                Console.WriteLine("HTML response received!\n");
              //  Console.WriteLine(htmlResponse);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown, evacuate the facility!\n\n{0}", ex);
            }

            ParseImagePage(htmlResponse);
        }

        private void ParseImagePage(string htmlString)
        {
            string identifier = "<a href=\"//is2.4chan.org";
            int linkStartIndex = 0;
            int linkEndIndex = 0;
            bool lineFound = false;
            string imageLink = "";
            Uri imageUri = null;
            int counter = 0;
            for (int i = 0; i < htmlString.Length - 24; i++)
            {
                string sub = htmlString.Substring(i, identifier.Length);
                if (sub == identifier)
                {
                    //   Console.WriteLine("Line found! \n{0}", sub);
                    linkStartIndex = i + 11;       // identifier start + 11 = is2...
                    lineFound = true;
                }
                if (lineFound == true)
                {
                    for (int j = linkStartIndex; j < htmlString.Length; j++)
                    {
                        if (htmlString.Substring(j, 1) == "\"")
                        {
                            linkEndIndex = j;
                            lineFound = false;
                            imageLink = htmlString.Substring(linkStartIndex, linkEndIndex - linkStartIndex);
                            Console.WriteLine("\nIMAGE LINK: {0}\n", imageLink);
                            imageUri = new Uri("http://"+ RemoveWhiteSpace(imageLink));
                            _imageUrisGuess[counter] = imageUri;
                            counter++;
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("{0} links found!", counter);
            _imageUrisFinal = new Uri[counter];
            for (int i = 0; i < counter; i++)
            {
                if (_imageUrisGuess[i] != null)
                {
                    _imageUrisFinal[i] = _imageUrisGuess[i];
                }
            }
            _mWindow.UpdateText("Downloading files");
            DownloadImage(_imageUrisFinal);
        }


        private void DownloadImage(Uri[] linkUris)
        {
            string filePath = _mWindow.PathBox.Text;
            string fileSuffix = @"\file" + _downloadedImagesCount + ".jpg";

            if (_client.IsBusy == false)
            {
                if (_client == null)
                {
                    _client = new WebClient();
                }
                _downloadedImagesCount++;
                _client.DownloadFileAsync(linkUris[_downloadedImagesCount], filePath + fileSuffix);
                _client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
                _client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            }
        }

        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            Console.WriteLine(downloadProgressChangedEventArgs.ProgressPercentage);
            _mWindow.UpdateProgress(downloadProgressChangedEventArgs.ProgressPercentage);
        }

        private void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            Console.WriteLine("Download finished!");
            Console.WriteLine("Images downloaded: {0}", _downloadedImagesCount);
            if (_downloadedImagesCount < _imageUrisFinal.Length-1 )
            {
                DownloadImage(_imageUrisFinal);
            }
            else _mWindow.UpdateText("Downloads finished!");
        }

        private string RemoveWhiteSpace(string input)
        {
            char[] inputArr = input.ToCharArray();
            char[] outputArr = new char[inputArr.Length];
            for (int i = 0; i < inputArr.Length; i++)
            {
                if (char.IsWhiteSpace(inputArr[i]) == false)
                {
                    outputArr[i] = inputArr[i];
                }
            }
            return new string(outputArr).Trim();
        }
    }
}