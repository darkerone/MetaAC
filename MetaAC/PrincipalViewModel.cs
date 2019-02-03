using MetaAC.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MetaAC
{
    
    public class PrincipalViewModel : INotifyPropertyChanged
    {
        const string RECHERCHE_EN_COURS_LABEL = "Recherche en cours...";
        const string RECHERCHER_TOUS_LABEL = "Chercher les métadonnées de la sélection";

        ObservableCollection<Musique> _listMusiques = new ObservableCollection<Musique>();
        bool _listMusiquesNotEmpty = false;
        bool _rechercheEnCours = false;
        bool _boutonsActifs = true;
        bool _detailsActif = false;
        string _texteBoutonFindMetadatas = RECHERCHER_TOUS_LABEL;
        int _avancement = 0;
        int _avancementMax = 100;
        Visibility _loadingVisibility = Visibility.Hidden;
        ObservableCollection<Musique> _listAlreadyCompletedMusiques = new ObservableCollection<Musique>();
        ObservableCollection<Musique> _listCanceledMusiques = new ObservableCollection<Musique>();
        ObservableCollection<Musique> _listMusiquesNoConnection = new ObservableCollection<Musique>();
        ObservableCollection<Musique> _listMusiquesNoResult = new ObservableCollection<Musique>();
        Musique _selectedMusique;
        bool _completedFilesIncluded = false;
        ServicesManager _serviceManager;
        bool _allChecked = false;

        Player _player;

        /// <summary>
        /// Constructeur
        /// </summary>
        public PrincipalViewModel()
        {
            _serviceManager = new ServicesManager();
            _player = new Player();
        }

        #region Commands

        public ICommand AddMusiques
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    // Créé une boite de dialogue de recherche de fichier
                    Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                    // Permet de sélectionner plusieurs fichiers
                    openFileDialog.Multiselect = true;
                    // Permet de filtrer les types de fichiers
                    openFileDialog.Filter = "Music files (*.mp3)|*.mp3";
                    AvancementMax = 0;
                    // Si l'utilisateur clique sur "ok" de la boite de dialogue
                    if (openFileDialog.ShowDialog() == true)
                    {
                        addMusiques(openFileDialog.FileNames);
                    }
                    if (ListMusiques.Count != 0)
                    {
                        ListMusiquesNotEmpty = true;
                        SelectedMusique = ListMusiques.FirstOrDefault();
                    }

                    // Si on choisit d'inclure les musiques qui possèdent déja des métadonnée
                    // Lors de l'ajout de nouvelles musiques, on coche ces musiques
                    CompletedFilesIncluded = CompletedFilesIncluded;
                    manageCheckAll();
                });
            }
        }

        public ICommand ClearList
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    resetAll();
                    manageCheckAll();
                });
            }
        }

        public ICommand RemoveMusiques
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    bool hasRemovedMusique = false;
                    // Pour toutes les musiques cochées
                    foreach(Musique musique in ListMusiques.Where(m => m.IsChecked).ToList())
                    {
                        ListMusiques.Remove(musique);
                        hasRemovedMusique = true;
                    }

                    // On ne sélectionne la première musique que si des musiques ont été supprimées
                    if(hasRemovedMusique)
                    {
                        SelectedMusique = ListMusiques.FirstOrDefault();
                    }

                    manageCheckAll();
                });
            }
        }

        public ICommand FindMetadatas
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if(ListMusiques.Any(m => m.IsChecked))
                    {
                        RechercheEnCours = true;
                        Avancement = 0;

                        foreach (Musique musique in ListMusiques.Where(m => m.IsChecked).ToList())
                        {
                            BackgroundWorker worker = new BackgroundWorker();
                            worker.WorkerReportsProgress = true;
                            worker.RunWorkerCompleted += SearchFinished;
                            worker.DoWork += (obj, e) => DoSearch(musique);
                            worker.RunWorkerAsync();

                            //Avancement++;
                            //// Signal à l'application de lire ses évenements pour raffraichir la vue
                            //DoEvents();
                        }
                    }
                });
            }
        }
        
        public ICommand SaveCheckedMetadatasFromInternet
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if(ListMusiques.Any(m => m.IsChecked))
                    {
                        string status = null;

                        foreach (Musique musique in ListMusiques.Where(m => m.IsChecked).ToList())
                        {
                            status = saveMetaForMusique(musique, musique.MetaFromInternet);
                        }

                        ShowSavedMusiqueDialog(ListMusiques.Where(m => m.IsChecked).ToList(), status);
                    }
                });
            }
        }

        public ICommand FromFileSearch
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    RechercheEnCours = true;
                    SelectedMusique.MetaFromInternet = _serviceManager.searchFromFileMeta(SelectedMusique);
                    RechercheEnCours = false;
                });
            }
        }

        public ICommand FromFileNameSearch
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    RechercheEnCours = true;
                    SelectedMusique.MetaFromInternet = _serviceManager.searchFromFileNameMeta(SelectedMusique);
                    RechercheEnCours = false;
                });
            }
        }

        public ICommand FromUserSearch
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    RechercheEnCours = true;
                    SelectedMusique.MetaFromInternet = _serviceManager.searchFromUserMeta(SelectedMusique);
                    RechercheEnCours = false;
                });
            }
        }

        public ICommand SaveMetadatasFromInternet
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    string status = saveMetaForMusique(SelectedMusique, SelectedMusique.MetaFromInternet);

                    ShowSavedMusiqueDialog(new List<Musique> { SelectedMusique }, status);
                });
            }
        }

        public ICommand SaveMetadatasFromUser
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    string status = saveMetaForMusique(SelectedMusique, SelectedMusique.MetaFromUser);

                    ShowSavedMusiqueDialog(new List<Musique>{SelectedMusique}, status);
                });
            }
        }

        public ICommand CheckAll
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    bool allMusiquesChecked = true;

                    // On regarde si toutes les musiques sont cochées
                    foreach(Musique musique in ListMusiques)
                    {
                        // Si une musique n'est pas cochée
                        if(!musique.IsChecked)
                        {
                            allMusiquesChecked = false;
                        }
                    }

                    // Si toutes les musiques sont cochées
                    if(allMusiquesChecked)
                    {
                        // On les décoches
                        checkAllMusiques(false);
                    }
                    else
                    {
                        // On les coches
                        checkAllMusiques(true);
                    }
                    
                });
            }
        }

        public ICommand CheckMusique
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    manageCheckAll();

                });
            }
        }

        public ICommand CopyImageFromInternet
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if(SelectedMusique.MetaFromInternet.AlbumCoverDisplay != null)
                    {
                        SelectedMusique.MetaFromUser.AlbumCoverDisplay = SelectedMusique.MetaFromInternet.AlbumCoverDisplay;
                    }
                });
            }
        }

        public ICommand CopyImageFromMetadatas
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (SelectedMusique.MetaFromFile.AlbumCoverDisplay != null)
                    {
                        SelectedMusique.MetaFromUser.AlbumCoverDisplay = SelectedMusique.MetaFromFile.AlbumCoverDisplay;
                    }
                });
            }
        }

        #endregion

        public void DoSearch(Musique musique)
        {
            musique.MetaFromInternet = _serviceManager.search(musique);
        }

        public void SearchFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            // Si l'une des musiques cochées est en cours de recherche
            if (ListMusiques.Where(m => m.IsChecked).ToList().Any(m => m.IsInSearch))
            {
                RechercheEnCours = true;
            }
            else
            {
                RechercheEnCours = false;
            }
        }

        /// <summary>
        /// Rétablit les variables comme au lancement de l'application.
        /// </summary>
        public void resetAll()
        {
            ListAlreadyCompletedMusiques.Clear();
            ListCanceledMusiques.Clear();
            ListMusiquesNoConnection.Clear();
            ListMusiquesNoResult.Clear();
            ListMusiques.Clear();
            SelectedMusique = null;
            ListMusiquesNotEmpty = false;
            Avancement = 0;
            RechercheEnCours = false;
        }

        /// <summary>
        /// Signal à l'application de lire ses évenements (pour raffraichir la vue par exemple)
        /// </summary>
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        /// <summary>
        /// Ajoute plusieurs musiques à la liste.
        /// </summary>
        /// <param name="fileNames">Tableau de chemins d'accès aux musiques.</param>
        /// <returns>Liste des musiques ajoutées</returns>
        public List<Musique> addMusiques(string[] fileNames)
        {
            List<Musique> musiqueList = new List<Musique>();
            foreach (string f in fileNames)
            {
                musiqueList.Add(addMusique(f));
                AvancementMax++;
            }
            return musiqueList;
        }

        /// <summary>
        /// Ajoute une musique à la liste.
        /// </summary>
        /// <param name="fileName">Chemin complet d'accès à la musique.</param>
        /// <returns>Musique ajoutée</returns>
        public Musique addMusique(string fileName)
        {
            Musique musique = new Musique(fileName);
            ListMusiques.Add(musique);
            return musique;
        }

        /// <summary>
        /// Affiche le rapport détaillé des musiques dont les métadatas étaient déjà présentes ou dont la recherche des métadatas a été annulée.
        /// </summary>
        public void showReport()
        {
            int nbAlreadyCompletedMusiques = ListAlreadyCompletedMusiques.Count;
            int nbCanceledMusiques = ListCanceledMusiques.Count;
            int nbNoConnection = ListMusiquesNoConnection.Count;
            string report = "";

            if (nbAlreadyCompletedMusiques != 0)
            {
                report += "Les musiques suivantes possaidaient déjà des métadonnées (" + nbAlreadyCompletedMusiques + "). Leur nom a été mis à jour selon leurs métadonnées :\n";

                for (int i = 0; i < nbAlreadyCompletedMusiques; i++)
                {
                    report += "- " + ListAlreadyCompletedMusiques[i].Meta.Tag.Performers[0] + " - " + ListAlreadyCompletedMusiques[i].Meta.Tag.Title + "\n";
                }
            }

            if (nbCanceledMusiques != 0)
            {
                report += "La recherche des métadonnées des musiques suivantes a été annulée (" + nbCanceledMusiques + ") :\n";

                for (int i = 0; i < nbCanceledMusiques; i++)
                {
                    report += "- " + ListCanceledMusiques[i].Name + "\n";
                }
            }

            if (nbNoConnection != 0)
            {
                report += "Connexion au serveur impossible pour les musiques suivantes (" + nbNoConnection + ") :\n";

                for (int i = 0; i < nbNoConnection; i++)
                {
                    report += "- " + ListMusiquesNoConnection[i].Name + "\n";
                }
            }

            if ((nbAlreadyCompletedMusiques == 0) && (nbCanceledMusiques == 0) && (nbNoConnection == 0))
            {
                report = "Métadonnnées ajoutées aux musiques avec succès !";
            }
            MessageBoxResult result = System.Windows.MessageBox.Show(report,
                "Rapport", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        /// <summary>
        /// Coche ou décoche toutes les musiques
        /// </summary>
        /// <param name="checkAll">True pour tout checker, false pour tout unchecker</param>
        public void checkAllMusiques(bool checkAll)
        {
            AllChecked = checkAll;
            foreach (Musique musique in ListMusiques)
            {
                musique.IsChecked = checkAll;
            }
        }

        /// <summary>
        /// Coche ou décoche la case checkAll selon que toules les musiques sont cochées ou pas
        /// </summary>
        public void manageCheckAll()
        {
            if(ListMusiques.Count != 0)
            {
                AllChecked = true;

                // On vérifie que toutes les musiques sont cochées
                foreach (Musique musique in ListMusiques)
                {
                    //Si une musique n'est pas cochée
                    if (!musique.IsChecked)
                    {
                        // On coche la case checkAll
                        AllChecked = false;
                    }
                }
            }
            else
            {
                AllChecked = false;
            }
        }

        /// <summary>
        /// Sauvegarde les metadonnées de la musique dans le fichier
        /// </summary>
        /// <param name="musique"></param>
        /// <param name="metadatas"></param>
        /// <returns>Message d'erreur si la sauvegarde a échouée</returns>
        public string saveMetaForMusique(Musique musique, Metadatas metadatas)
        {
            Player.setMusique(null);

            string status = null;

            try
            {
                musique.WriteMetadatas(metadatas);
            }
            catch(Exception ex)
            {
                status = ex.Message;
            }
            

            //// Récupère le nouvel emplacement de la musique
            //string newPathNameExtension = musique.PathNameExtension;
            //// Memorise la position de la musique dans la liste
            //int oldMusiqueIndex = ListMusiques.IndexOf(musique);

            //// Retire la musique de la liste
            //ListMusiques.Remove(musique);
            //// Ajoute la musique avec le nouvel emplacement à la liste
            //Musique newMusique = addMusique(newPathNameExtension);

            //// Deplace la musique nouvellement ajoutée à la position mémorisée
            //int newMusiqueIndex = ListMusiques.IndexOf(newMusique);
            //ListMusiques.Move(newMusiqueIndex, oldMusiqueIndex);

            Player.setMusique(SelectedMusique);
            AllChecked = false;

            return status;
        }

        /// <summary>
        /// Affiche une boite de dialogue après l'enregistrement des métadonnées
        /// </summary>
        /// <param name="musiqueList"></param>
        /// <param name="message">Message d'erreur, null si pas d'erreur</param>
        public void ShowSavedMusiqueDialog(List<Musique> musiqueList, string message)
        {
            string report = "";
            MessageBoxImage messageBoxImage = MessageBoxImage.Information;

            if (musiqueList.Count == 1)
            {
                report = "Musique mise à jour !";
            }
            else
            {
                report = "Musiques mises à jour !";
            }

            if (message != null)
            {
                report = "Erreur : " + message;
                messageBoxImage = MessageBoxImage.Error;
            }

            MessageBoxResult result = System.Windows.MessageBox.Show(report,
                    "Rapport", MessageBoxButton.OK, messageBoxImage, MessageBoxResult.OK);
        }

        #region Propriétés

        public ObservableCollection<Musique> ListMusiques
        {
            get { return _listMusiques; }
        }

        public ObservableCollection<Musique> ListAlreadyCompletedMusiques
        {
            get { return _listAlreadyCompletedMusiques; }
        }

        public ObservableCollection<Musique> ListCanceledMusiques
        {
            get { return _listCanceledMusiques; }
        }

        public ObservableCollection<Musique> ListMusiquesNoConnection
        {
            get { return _listMusiquesNoConnection; }
        }

        public ObservableCollection<Musique> ListMusiquesNoResult
        {
            get { return _listMusiquesNoResult; }
        }

        public bool ListMusiquesNotEmpty
        {
            get { return _listMusiquesNotEmpty; }
            set
            {
                _listMusiquesNotEmpty = value;
                if (value)
                {
                    BoutonRechercherActif = true;
                }
                else
                {
                    BoutonRechercherActif = false;
                }
                    
                RaisePropertyChanged("ListMusiquesNonVide");
            }
        }

        public bool BoutonRechercherActif
        {
            get { return _rechercheEnCours; }
            set
            {
                _rechercheEnCours = value;
                RaisePropertyChanged("BoutonRechercherActif");
            }
        }

        public bool RechercheEnCours
        {
            get { return _rechercheEnCours; }
            set
            {
                _rechercheEnCours = value;
                if(value)
                {
                    TexteBoutonFindMetadatas = RECHERCHE_EN_COURS_LABEL;
                    LoadingVisibility = Visibility.Visible;
                    BoutonsActifs = false;
                    BoutonRechercherActif = false;
                    DetailsActif = false;
                }
                else
                {
                    TexteBoutonFindMetadatas = RECHERCHER_TOUS_LABEL;
                    LoadingVisibility = Visibility.Hidden;
                    BoutonsActifs = true;
                    BoutonRechercherActif = true;
                    DetailsActif = true;
                }
                DoEvents();
                RaisePropertyChanged("RechercheEnCours");
            }
        }

        public bool BoutonsActifs
        {
            get { return _boutonsActifs; }
            set
            {
                _boutonsActifs = value;
                RaisePropertyChanged("BoutonsActifs");
            }
        }

        public bool DetailsActif
        {
            get { return _detailsActif; }
            set
            {
                _detailsActif = value;
                RaisePropertyChanged("DetailsActif");
            }
        }

        public int Avancement
        {
            get { return _avancement; }
            set
            {
                _avancement = value;
                RaisePropertyChanged("Avancement");
            }
        }

        public int AvancementMax
        {
            get { return _avancementMax; }
            set
            {
                _avancementMax = value;
                RaisePropertyChanged("AvancementMax");
            }
        }

        public Visibility LoadingVisibility
        {
            get { return _loadingVisibility; }
            set
            {
                _loadingVisibility = value;
                RaisePropertyChanged("LoadingVisibility");
            }
        }

        public string TexteBoutonFindMetadatas
        {
            get { return _texteBoutonFindMetadatas; }
            set
            {
                _texteBoutonFindMetadatas = value;
                RaisePropertyChanged("TexteBoutonFindMetadatas");
            }
        }

        public Musique SelectedMusique
        {
            get { return _selectedMusique; }
            set
            {
                _selectedMusique = value;
                if(value != null)
                {
                    DetailsActif = true;
                }
                else
                {
                    DetailsActif = false;
                }
                Player.setMusique(value);
                RaisePropertyChanged("SelectedMusique");
            }
        }
        
        public bool CompletedFilesIncluded
        {
            get { return _completedFilesIncluded; }
            set
            {
                _completedFilesIncluded = value;
                if(value)
                {
                    foreach(Musique musique in ListMusiques.Where(m => m.MetaCompleted).ToList())
                    {
                        musique.IsChecked = true;
                    }
                }
                else
                {
                    foreach (Musique musique in ListMusiques.Where(m => m.MetaCompleted).ToList())
                    {
                        musique.IsChecked = false;
                    }
                }
                RaisePropertyChanged("CompletedFilesIncluded");
            }
        }

        public bool AllChecked
        {
            get { return _allChecked; }
            set
            {
                _allChecked = value;
                RaisePropertyChanged("AllChecked");
            }
        }

        public Player Player
        {
            get { return _player; }
            set
            {
                _player = value;
                RaisePropertyChanged("Player");
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
