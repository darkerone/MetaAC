using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MetaAC
{
    public class Metadatas : INotifyPropertyChanged
    {
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public string Genre { get; set; }
        public int Duration { get; set; }
        public TagLib.Picture Picture { get; set; }
        public BitmapImage AlbumCover { get; set; }
        public MemoryStream AlbumCoverStream { get; set; }
        public string Fingerprint { get; set; }
        public string RemixedBy { get; set; }

        private ImageSource _albumCoverDisplay;
        public ImageSource AlbumCoverDisplay
        {
            get { return _albumCoverDisplay; }
            set
            {
                _albumCoverDisplay = value;
                RaisePropertyChanged("AlbumCoverDisplay");
            }
        }


        

        public bool Valid { get; set; }
        public Status Status { get; set; }

        public void checkValidity()
        {
            if (ArtistName != null 
                && Title != null 
                && ArtistName != ""
                && Title != ""
                && ArtistName != "-"
                && Title != "-")
            {
                Status = Status.ValidResult;
            }
            else
            {
                Status = Status.NoResult;
            }
        }

        #region Property Change
        // On créé une méthode pour éviter de recopier a chaque fois le if...
        private void RaisePropertyChanged(string propertyName)
        {
            // On vérifie qu'il y ai des abonnés à l'evenement
            if (PropertyChanged != null)
            {
                // On dit que la propriété "propertyName" a été changée
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
