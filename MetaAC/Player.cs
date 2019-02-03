using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MetaAC
{
    public class Player : INotifyPropertyChanged
    {
        const string ICON_PLAY = @"pack://application:,,,/Icones/iconPlayer/play.png";
        const string ICON_PAUSE = @"pack://application:,,,/Icones/iconPlayer/pause.png";

        private MediaPlayer _mediaPlayer;
        private bool _isPlaying = false;
        private string _playPauseIcon = ICON_PLAY;

        public Player()
        {
            _mediaPlayer = new MediaPlayer();
        }

        public ICommand PlayPause
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (_mediaPlayer.HasAudio)
                    {
                        if(IsPLaying)
                        {
                            _mediaPlayer.Pause();
                            IsPLaying = false;
                        }
                        else
                        {
                            _mediaPlayer.Play();
                            IsPLaying = true;
                        }
                        
                    }
                });
            }
        }

        public ICommand Stop
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _mediaPlayer.Stop();
                    IsPLaying = false;
                });
            }
        }

        public void setMusique(Musique musique)
        {
            // Commenté pour éviter les problèmes de "fichier utilisé par une autre application"
            // Il faudrait trouver une solution qui libère le fichier
            //_mediaPlayer.Close();
            //if (musique != null)
            //{
            //    _mediaPlayer.Open(new Uri(musique.PathNameExtension));
            //}
            //IsPLaying = false;
        }

        public bool IsPLaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                if(value)
                {
                    PlayPauseIcon = ICON_PAUSE;
                }
                else
                {
                    PlayPauseIcon = ICON_PLAY;
                }
                RaisePropertyChanged("IsPLaying");
            }
        }

        public string PlayPauseIcon
        {
            get { return _playPauseIcon; }
            set
            {
                _playPauseIcon = value;
                RaisePropertyChanged("PlayPauseIcon");
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
