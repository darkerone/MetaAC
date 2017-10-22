using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaAC.Services
{
    public class MusixmatchService : ApiService<MetadatasMusixmatch>
    {

        public MusixmatchService()
        {
            string apikey = "e4a6998275c7ea7dcc464f80fc6539e5";
            _url = "http://api.musixmatch.com/ws/1.1/track.search";
            _parameters = "apikey=" + apikey;
        }
        
        override public Metadatas search(Metadatas metadatasFromMusique)
        {
            string recherche = "&q_track=" + escape(metadatasFromMusique.ArtistName + "-" + @metadatasFromMusique.Title);
            Metadatas metadatas;

            metadatas = request(recherche);


            return metadatas;
        }

        /// <summary>
        /// Convertit les métadonnées provenant de MusixMatch en Metadatas.
        /// </summary>
        /// <param name="metadatasMusixmatch">Métadonnées provenant de MusixMatch</param>
        /// <returns>Status peut valoir : NeedValidation, NoResult.</returns>
        /// <returns></returns>
        override protected Metadatas convertToMetadatas(MetadatasMusixmatch metadatasMusixmatch)
        {
            Metadatas metadatas = new Metadatas();

            if ((metadatasMusixmatch != null) && (metadatasMusixmatch.Track != null))
            {
                metadatas.AlbumName = metadatasMusixmatch.Track.album_name;
                metadatas.ArtistName = metadatasMusixmatch.Track.artist_name;
                metadatas.ReleaseDate = metadatasMusixmatch.Track.first_release_date;
                metadatas.Title = metadatasMusixmatch.Track.track_name;
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
