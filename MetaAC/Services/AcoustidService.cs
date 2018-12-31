using AcoustID;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestAcoustid;
using TestAcoustid.Audio;

namespace MetaAC.Services
{
    public class AcoustidService : ApiService<MetadatasAcoustid>
    {
        public override MetadatasSourceEnum Source { get { return MetadatasSourceEnum.AcoustId; } }

        public AcoustidService()
        {
            string apiClientKey = "VSBxZeBBtZ";

            _url = "http://api.acoustid.org/v2/lookup";
            _parameters = "format=" + "json"
                        + "&client=" + apiClientKey
                        + "&meta=" + "recordings+recordingids+releases+releaseids+releasegroups+releasegroupids+tracks+compress+usermeta+sources";
        }

        
        override public Metadatas search(Metadatas metadatasFromMusique)
        {
            string recherche = "duration=" + metadatasFromMusique.Duration
                             + "&fingerprint=" + metadatasFromMusique.Fingerprint;
            Metadatas metadatas;

            metadatas = request(recherche);

            
            return metadatas;
        }

        public override Metadatas search(string text)
        {
            return new Metadatas() {
                Status = Status.NoResult
            };
        }

        /// <summary>
        /// Convertit les métadonnées provenant de acoustid en Metadatas.
        /// </summary>
        /// <param name="metadatasItunes">Métadonnées provenant d'Acoustid</param>
        /// <returns>Status peut valoir : NeedValidation, NoResult.</returns>
        override protected Metadatas convertToMetadatas(MetadatasAcoustid metadatasAcoustid)
        {
            Metadatas metadatas = new Metadatas();
            metadatas.Valid = false;
            if (metadatasAcoustid.results.Count != 0
                && metadatasAcoustid.results[0].recordings.Count != 0
                && metadatasAcoustid.results[0].recordings[0].releasegroups.Count != 0
                && metadatasAcoustid.results[0].recordings[0].releasegroups[0].releases.Count != 0
                && metadatasAcoustid.results[0].recordings[0].releasegroups[0].releases[0].mediums.Count != 0
                && metadatasAcoustid.results[0].recordings[0].releasegroups[0].releases[0].mediums[0].tracks.Count != 0
                && metadatasAcoustid.results[0].recordings[0].releasegroups[0].releases[0].mediums[0].tracks[0].artists.Count != 0)
            {
                
                metadatas.AlbumName = "";
                metadatas.ArtistName = metadatasAcoustid.results[0].recordings[0].releasegroups[0].releases[0].mediums[0].tracks[0].artists[0].name;
                metadatas.ReleaseDate = "";
                metadatas.Title = metadatasAcoustid.results[0].recordings[0].releasegroups[0].releases[0].mediums[0].tracks[0].title;
                metadatas.checkValidity();
            }
            else
            {
                metadatas.Status = Status.NoResult;
            }

            return metadatas;
        }
        
    }
}
