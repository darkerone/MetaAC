using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaAC.Services
{
    /// <summary>
    /// Gère l'appel des différents services (API)
    /// </summary>
    public class ServicesManager
    {
        private AcoustidService _acoustidService;
        private ItunesService _itunesService;
        private MusixmatchService _musixmatchService;

        public ServicesManager()
        {
            _acoustidService = new AcoustidService();
            _itunesService = new ItunesService();
            _musixmatchService = new MusixmatchService();
        }

        /// <summary>
        /// Recherche les métadonnées à partir des donnée de la musique, si aucune donnée n'est trouvé, 
        /// recherche à partir du nom du fichier de la musique
        /// </summary>
        /// <param name="musique"></param>
        /// <returns></returns>
        public Metadatas search(Musique musique)
        {
            musique.IsInSearch = true;

            Metadatas metadatas;
            metadatas = searchFromFileMeta(musique);
            if (metadatas.Status == Status.NoConnetion || metadatas.Status == Status.NoResult)
            {
                metadatas = searchFromFileNameMeta(musique);
                if (metadatas.Status == Status.NoConnetion || metadatas.Status == Status.NoResult)
                {
                    musique.IsChecked = false;
                }
            }

            musique.Distance = checkConformityBetween(musique.CleanedName, 
                musique.MetaFromInternet.ArtistName + " - " + musique.MetaFromInternet.Title) ? 1 : 0;

            musique.IsInSearch = false;
            return metadatas;
        }

        /// <summary>
        /// Vérifie si les deux chaines se ressemblent (partiellement au moins)
        /// </summary>
        /// <param name="chaine1"></param>
        /// <param name="chaine2"></param>
        /// <returns>true si elles se ressemblent, false sinon</returns>
        private bool checkConformityBetween(string chaine1, string chaine2)
        {
            int compteur = 0;
            // On récupère tous les mots de la première chaine
            string[] motsChaine1 = chaine1.Split(' ');

            // Pour tous les mots de la chaine
            foreach(string mot in motsChaine1)
            {
                // On regarde s'il est aussi dans l'autre chaine
                if(chaine2.Contains(mot))
                {
                    compteur++;
                }
            }

            // Si au moins 2 mots de la premiere chaine sont dans la seconde
            if(compteur > 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Effectue une recherche à partir des données contenues dans le fichier sur toutes les api tant qu'il n'a pas trouvé les métadonnées
        /// </summary>
        /// <param name="musique">Musique dont les métadonnées doivent être recherchées</param>
        /// <returns></returns>
        public Metadatas searchFromFileMeta(Musique musique)
        {
            Metadatas metadatas;

            // On recherche d'abord sur Acoustid avec l'empreinte de la musique
            musique.CalculateFingerprint();
            metadatas = _acoustidService.search(musique.MetaFromFile);

            // Si la première recherche n'a rien donnée
            if (metadatas.Status == Status.NoConnetion || metadatas.Status == Status.NoResult)
            {
                // On cherche sur Itunes
                metadatas = _itunesService.search(musique.MetaFromFile);

                // Si la seconde recherche n'a rien donnée
                if (metadatas.Status == Status.NoConnetion || metadatas.Status == Status.NoResult)
                {
                    // On cherche sur Musixmatch
                    metadatas = _musixmatchService.search(musique.MetaFromFile);
                }
            }
            else 
            {
                // On cherche sur Itunes avec les données trouvées grace à la requete précédente
                metadatas = _itunesService.search(metadatas);

                // Si la seconde recherche n'a rien donnée
                if (metadatas.Status == Status.NoConnetion || metadatas.Status == Status.NoResult)
                {
                    // On cherche sur Musixmatch
                    metadatas = _musixmatchService.search(metadatas);
                }
            }
            return metadatas;
        }

        /// <summary>
        /// Effectue une recherche à partir des données contenues dans le nom du fichier sur toutes les api tant qu'il n'a pas trouvé les métadonnées
        /// </summary>
        /// <param name="musique"></param>
        /// <returns></returns>
        public Metadatas searchFromUserMeta(Musique musique)
        {
            Metadatas metadatas;

            // On cherche sur Itunes
            metadatas = _itunesService.search(musique.MetaFromUser);

            // Si la seconde recherche n'a rien donnée
            if (metadatas.Status == Status.NoConnetion || metadatas.Status == Status.NoResult)
            {
                // On cherche sur Musixmatch
                metadatas = _musixmatchService.search(musique.MetaFromUser);
            }

            return metadatas;
        }

        /// <summary>
        /// Effectue une recherche de la musique à partir des données fournies par l'utilisateur sur toutes les api tant qu'il n'a pas trouvé les métadonnées
        /// </summary>
        /// <param name="musique"></param>
        /// <returns></returns>
        public Metadatas searchFromFileNameMeta(Musique musique)
        {
            Metadatas metadatas;

            // On cherche sur Itunes
            metadatas = _itunesService.search(musique.MetaFromFileName);

            // Si la seconde recherche n'a rien donnée
            if (metadatas.Status == Status.NoConnetion || metadatas.Status == Status.NoResult)
            {
                // On cherche sur Musixmatch
                metadatas = _musixmatchService.search(musique.MetaFromFileName);
            }

            return metadatas;
        }

    }
}
