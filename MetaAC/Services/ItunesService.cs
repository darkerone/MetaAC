using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MetaAC.Services
{
    public class ItunesService : ApiService<MetadatasItunes>
    {
        public override MetadatasSourceEnum Source { get { return MetadatasSourceEnum.Itunes; } }

        public ItunesService()
        {
            string country = "fr";
            string media = "music";
            string limit = "1";

            _url = "https://itunes.apple.com/search";
            _parameters = "&country=" + country
                        + "&media=" + media
                        + "&limit=" + limit;
        }
        
        override public Metadatas search(Metadatas metadatasFromMusique)
        {
            string recherche = "term=" + escape(Tools.UpperFirstLetters(metadatasFromMusique.ArtistName) + "-" + metadatasFromMusique.Title);
            Metadatas metadatas;

            metadatas = request(recherche);

            return metadatas;
        }

        public override Metadatas search(string text)
        {
            string recherche = "term=" + escape(Tools.UpperFirstLetters(text));
            Metadatas metadatas;

            metadatas = request(recherche);

            return metadatas;
        }

        /// <summary>
        /// Convertit les métadonnées provenant d'Itunes en Metadatas.
        /// </summary>
        /// <param name="metadatasItunes">Métadonnées provenant d'Itunes</param>
        /// <returns>Status peut valoir : NeedValidation, NoResult.</returns>
        override protected Metadatas convertToMetadatas(MetadatasItunes metadatasItunes)
        {
            Metadatas metadatas = new Metadatas();
            metadatas.Valid = false;
            if (metadatasItunes.resultCount != 0)
            {
                metadatas.AlbumName = metadatasItunes.results.First().collectionName;
                metadatas.ArtistName = metadatasItunes.results.First().artistName;
                DateTime tmpDate = Convert.ToDateTime(metadatasItunes.results.First().releaseDate);
                metadatas.ReleaseDate = tmpDate.Year.ToString();
                metadatas.Title = metadatasItunes.results.First().trackName;
                
                metadatas.AlbumCoverStream = GetStreamFromUrl(metadatasItunes.results.First().artworkUrl100);
                metadatas.AlbumCover = GetBitmapImageFromStream(metadatas.AlbumCoverStream);
                metadatas.AlbumCoverDisplay = metadatas.AlbumCover as BitmapSource;

                metadatas.checkValidity();
            }
            else
            {
                metadatas.Status = Status.NoResult;
            }

            return metadatas;
        }

        private MemoryStream GetStreamFromUrl(string url)
        {
            
            byte[] buffer = new byte[1024];

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Timeout = 30000;
            httpRequest.Method = "GET";
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:12.0) Gecko/20100101 Firefox/12.0";
            httpRequest.Accept = "image/png,image/*;q=0.8,*/*;q=0.5";

            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                using (Stream responseStream = httpResponse.GetResponseStream())
                {
                    MemoryStream memStream = new MemoryStream();
                    int bytesRead;
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }
                    memStream.Seek(0, SeekOrigin.Begin);

                    return memStream;
                    
                }
            }
        }

        private BitmapImage GetBitmapImageFromStream(MemoryStream memStream)
        {
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.StreamSource = memStream;
            bmpImage.CacheOption = BitmapCacheOption.OnLoad;
            bmpImage.EndInit();

            // Necessaire pour le multithreading
            // http://www.ridgesolutions.ie/index.php/2012/01/26/net-wpf-set-bitmap-must-create-dependencysource-on-same-thread-as-the-dependencyobject-error/
            bmpImage.Freeze();

            return bmpImage;
        }
    }
}
