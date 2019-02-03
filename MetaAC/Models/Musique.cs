using AcoustID;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TagLib;
using TestAcoustid;
using TestAcoustid.Audio;

namespace MetaAC
{
    public class Musique : INotifyPropertyChanged
    {
        string _pathNameExtension; // Chemin + nom + extension du fichier
        string _path; // Chemin du fichier uniquement (sans le nom du fichier)
        string _name; // Nom uniquement
        string _extension; // Extension du fichier uniquement
        TagLib.File _meta; // Métadonnées présentes dans le fichier
        string _cleanedName; // _name débarrassé de tous les caractères/mots particuliers
        bool _metaCompleted = false; // La musique contient-elle des métadonnées
        bool _isChecked = true;
        bool _hasMetaFromInternet = false;
        bool _isInSearch = false;
        public int Distance { get; set; }
        
        Metadatas _metaFromFile; // Trouvé dans les métadonnées de la musique
        Metadatas _metaFromFileName; // Extrait du nom du fichier
        Metadatas _metaFromInternet; // Trouvé sur internet
        Metadatas _metaFromUser; // Renseigné par l'utilisateur

        public Musique(string pathNameExtension)
        {
            
            MetaFromFile = new Metadatas();
            MetaFromFileName = new Metadatas();
            MetaFromInternet = new Metadatas();
            MetaFromUser = new Metadatas();
            PathNameExtension = pathNameExtension;
        }
        
        /// <summary>
        /// Nettoie le nom de tous les caractères spéciaux, les parenthèses, les crochets et ce qu'ils contiennent.
        /// </summary>
        /// <param name="name">Nom à nettoyer.</param>
        /// <returns></returns>
        public string CleanName(string name)
        {
            string pattern;
            string cleanedName = name;
            RegexOptions options = RegexOptions.IgnoreCase;
            string[] blackList = { "clip", "official", "original", "version", "mix", "with ", "avec",
                                    "lyrics", "paroles", "hd", " ld", "cover", @"art\.", " art ", "remix", "from",
                                    "sound", "track", "youtube", "music", "musique", "240p", "480p",
                                    "720p", "1080p", "instrumental"};

            // Supprime les mots recensés dans la blacklist
            pattern = "video";
            foreach(string mot in blackList)
            {
                pattern = pattern + "|" + mot;
            }
            cleanedName = Regex.Replace(cleanedName, pattern, String.Empty, options);

            // Décode les caractères codé en html
            cleanedName = System.Net.WebUtility.HtmlDecode(cleanedName);

            // Supprime les parenthèses et ce qu'il y a dedans
            pattern = @"\(.*\)";
            cleanedName = Regex.Replace(cleanedName, pattern, String.Empty);

            // Supprime les crochets et ce qu'il y a dedans
            pattern = @"\[.*\]";
            cleanedName = Regex.Replace(cleanedName, pattern, String.Empty);

            // Supprime les "feat. " et "ft. "
            pattern = @"fe*a*t\.\s";
            cleanedName = Regex.Replace(cleanedName, pattern, String.Empty, options);

            // Supprime tous les caractères spéciaux
            pattern = @"#[^a-zA-Z0-9\s&éèàêâùïüë-]#i";
            cleanedName = Regex.Replace(cleanedName, pattern, String.Empty);

            // Remplace les "_" par des espaces
            pattern = @"_";
            cleanedName = Regex.Replace(cleanedName, pattern, " ");

            return cleanedName;
        }

        /// <summary>
        /// Ecrit les métadatas passés en paramètre dans le fichier.
        /// </summary>
        /// <param name="metadatas">Métadatas à ajouter au fichier.</param>
        public void WriteMetadatas(Metadatas metadatas)
        {
            if(metadatas == null)
            {
                
            }
            else
            {
                // Artiste
                if (metadatas.ArtistName != null)
                {
                    string[] tabArtists = new string[1];
                    tabArtists[0] = metadatas.ArtistName;
                    Meta.Tag.Performers = tabArtists;
                }

                // Année
                if ((metadatas.ReleaseDate != null && metadatas.ReleaseDate != ""))
                {
                    Meta.Tag.Year = UInt32.Parse(metadatas.ReleaseDate.Split('-')[0]);
                }

                // Album
                if (metadatas.AlbumName != null)
                    Meta.Tag.Album = metadatas.AlbumName;

                // Titre
                if (metadatas.Title != null)
                {
                    Meta.Tag.Title = metadatas.Title;
                    // Remixé par
                    if(metadatas.RemixedBy != null && metadatas.RemixedBy.Length > 0)
                    {
                        Meta.Tag.Title += $" ({metadatas.RemixedBy} Remix)";
                    }
                    // Edité par
                    if (metadatas.EditBy != null && metadatas.EditBy.Length > 0)
                    {
                        Meta.Tag.Title += $" ({metadatas.EditBy} Edit)";
                    }
                }
                    

                // Photo
                if (metadatas.AlbumCoverStream != null)
                {
                    Picture picture = new Picture();
                    picture.Type = PictureType.FrontCover;
                    picture.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    picture.Description = "Cover";
                    byte[] tabBytes = metadatas.AlbumCoverStream.ToArray();
                    picture.Data = new ByteVector(tabBytes);
                    Meta.Tag.Pictures = new IPicture[] { picture }; ; 
                }
                    

                // On enregistre les métadatas dans le fichier
                Meta.Save();


                this.UpdateFileName();
            }
            
            
        }

        /// <summary>
        /// Renomme le fichier avec les métadonnées
        /// </summary>
        public void UpdateFileName()
        {
            // On renomme le fichier
            // Pour cela, on supprime les caractères \/:*?"<>| qui sont invalides pour un nom de fichier
            string pattern = @"[\\/:*?" + '"' + "<>|]";

            string performers = Meta.Tag.Performers[0];
            performers = Regex.Replace(performers, pattern, String.Empty);

            string title = Meta.Tag.Title;
            title = Regex.Replace(title, pattern, String.Empty);

            string newPathNameExtension = Path + "\\" + performers + " - " + title + Extension;

            if (!System.IO.File.Exists(newPathNameExtension))
            {
                System.IO.File.Move(PathNameExtension, newPathNameExtension);

                // Mise à jour du chemin
                PathNameExtension = newPathNameExtension;
            }
        }

        /// <summary>
        /// Calcul l'empreinte de la musique et l'enregistre dans les métadatas
        /// </summary>
        /// <returns></returns>
        public void CalculateFingerprint()
        {
            // Initialisation du décodeur pour décompresser la musique
            IAudioDecoder decoder;

            decoder = new NAudioDecoder(PathNameExtension);

            int bits = decoder.Format.BitDepth;
            int channels = decoder.Format.Channels;

            // Initialisation de chromaprint
            ChromaContext context = new ChromaContext(AcoustID.Chromaprint.ChromaprintAlgorithm.TEST1);

            context.Start(decoder.SampleRate, decoder.Channels);
            SetDurationEverywhere(decoder.Format.Duration);
            decoder.Decode(context.Consumer, 120);
            context.Finish();

            decoder.Dispose();

            try
            {
                if (context.GetFingerprint() != null && context.GetFingerprint() != "")
                {
                    SetFingerprintEverywhere(context.GetFingerprint());
                }
            }
            catch
            {

            }
           
        }

        /// <summary>
        /// Récupère les métadonnées contenues dans le fichier
        /// </summary>
        private void FindMetaFromFile()
        {
            MetaFromFile.Title = Meta.Tag.Title;
            MetaFromFile.ArtistName = Meta.Tag.Performers.FirstOrDefault();
            MetaFromFile.AlbumName = Meta.Tag.Album;
            MetaFromFile.ReleaseDate = Meta.Tag.Year.ToString();
            if(Meta.Tag.Pictures != null  && Meta.Tag.Pictures.Count() != 0)
            {
                // Load you image data in MemoryStream
                IPicture pic = Meta.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                MetaFromFile.AlbumCoverDisplay = bitmap as BitmapSource;
            }
            
            
        }

        /// <summary>
        /// Récupère les infos contenues dans le nom du fichier
        /// </summary>
        private void FindMetaFromFileName()
        {
            string[] splittedName = CleanedName.Split('-');
            // Si la recherche ne contient pas de tiret
            if (splittedName.Count() < 2)
            {
                // On considère que l'on a que le titre de la musique
                MetaFromFileName.ArtistName = "";
                MetaFromFileName.Title = splittedName[0];
            }
            else
            {
                MetaFromFileName.ArtistName = splittedName[0];
                MetaFromFileName.Title = splittedName[1];
            }
        }

        private void SetDefaultMetaFromUser()
        {
            if(MetaFromFile.Title != null 
                && MetaFromFile.Title != ""
                && MetaFromFile.ArtistName != null
                && MetaFromFile.ArtistName != "")
            {
                MetaFromUser.Title = MetaFromFile.Title;
                MetaFromUser.ArtistName = MetaFromFile.ArtistName;
            }
            else
            {
                MetaFromUser.Title = MetaFromFileName.Title;
                MetaFromUser.ArtistName = MetaFromFileName.ArtistName;
            }
        }

        private void SetDurationEverywhere(int duration)
        {
            MetaFromFile.Duration = duration;
            MetaFromFileName.Duration = duration;
            MetaFromInternet.Duration = duration;
            MetaFromUser.Duration = duration;
        }

        private void SetFingerprintEverywhere(string fingerprint)
        {
            MetaFromFile.Fingerprint = fingerprint;
            MetaFromFileName.Fingerprint = fingerprint;
            MetaFromInternet.Fingerprint = fingerprint;
            MetaFromUser.Fingerprint = fingerprint;
        }
        
        #region Propriétés

        public string PathNameExtension
        {
            get { return _pathNameExtension; }
            set
            {
                _pathNameExtension = value;
                Path = System.IO.Path.GetDirectoryName(_pathNameExtension);
                Name = System.IO.Path.GetFileNameWithoutExtension(_pathNameExtension);
                Extension = System.IO.Path.GetExtension(_pathNameExtension);
                Meta = TagLib.File.Create(_pathNameExtension);
                CleanedName = CleanName(Name);
                SetDefaultMetaFromUser();
                RaisePropertyChanged("PathNameExtension");
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged("Path");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Extension
        {
            get { return _extension; }
            set
            {
                _extension = value;
                RaisePropertyChanged("Extension");
            }
        }

        public TagLib.File Meta
        {
            get { return _meta; }
            set
            {
                _meta = value;
                // Si un champ des métadatas n'est pas rempli
                if ((Meta.Tag.Performers.Length == 0) || (Meta.Tag.Year == 0) || (Meta.Tag.Album == null) || (Meta.Tag.Title == null))
                    MetaCompleted = false;
                else
                {
                    FindMetaFromFile();
                    MetaCompleted = true;
                }
                _meta.Dispose();
                RaisePropertyChanged("Meta");
            }
        }

        public string CleanedName
        {
            get { return _cleanedName; }
            set
            {
                _cleanedName = value;

                FindMetaFromFileName();

                RaisePropertyChanged("CleanedName");
            }
        }

        /// <summary>
        /// Vrai si toutes les métadonnées sont présentes.
        /// </summary>
        public bool MetaCompleted
        {
            get { return _metaCompleted; }
            set
            {
                _metaCompleted = value;    
                if(value)
                {
                    IsChecked = false;
                }     
                else
                {
                    IsChecked = true;
                }       
                RaisePropertyChanged("MetaCompleted");
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        public Metadatas MetaFromFile
        {
            get { return _metaFromFile; }
            set
            {
                _metaFromFile = value;
                RaisePropertyChanged("MetaFromFile");
            }
        }

        public Metadatas MetaFromFileName
        {
            get { return _metaFromFileName; }
            set
            {
                _metaFromFileName = value;
                RaisePropertyChanged("MetaFromFileName");
            }
        }

        public Metadatas MetaFromInternet
        {
            get { return _metaFromInternet; }
            set
            {
                _metaFromInternet = value;
                if(value.ArtistName != null && value.ArtistName != ""
                    && value.Title != null && value.Title != "")
                {
                    HasMetaFromInternet = true;
                }
                else
                {
                    HasMetaFromInternet = false;
                }
                RaisePropertyChanged("MetaFromInternet");
            }
        }

        public Metadatas MetaFromUser
        {
            get { return _metaFromUser; }
            set
            {
                _metaFromUser = value;
                RaisePropertyChanged("MetaFromUser");
            }
        }
        
        public bool HasMetaFromInternet
        {
            get { return _hasMetaFromInternet; }
            set
            {
                _hasMetaFromInternet = value;
                RaisePropertyChanged("HasMetaFromInternet");
            }
        }

        public bool IsInSearch
        {
            get { return _isInSearch; }
            set
            {
                _isInSearch = value;
                RaisePropertyChanged("IsInSearch");
            }
        }

        #endregion

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
