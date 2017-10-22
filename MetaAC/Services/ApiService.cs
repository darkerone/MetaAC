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
    public abstract class ApiService<T>
    {
        protected string _parameters;
        protected string _url;

        /// <summary>
        /// Lance une recherche basée sur le titre et l'artiste des metadatas provenant d'une musique
        /// </summary>
        /// <param name="metadatasFromMusique">Metadatas d'une musique (fromFile, fromFileName, fromUser, fromInternet...)</param>
        /// <returns>Dans metadatas, Status peut valoir : NeedValidation, NoConnection, NoResult.</returns>
        public abstract Metadatas search(Metadatas metadatasFromMusique);

        /// <summary>
        /// Convertis les métadonnées recues du serveur en des métadonnées "communes" à l'application
        /// </summary>
        /// <param name="metadatasMusixmatch">Métadonnées à convertir</param>
        /// <returns></returns>
        protected abstract Metadatas convertToMetadatas(T metadatas);

        /// <summary>
        /// Envoie une requête à l'API pour récupérer les métadatas à partir d'une recherche
        /// </summary>
        /// <param name="recherche">Peut être un nom de musique, d'artiste,...</param>
        /// <returns>Status peut valoir : NeedValidation, NoConnection, NoResult.</returns>
        protected Metadatas request(string search)
        {
            Metadatas metadatas;
            WebResponse response;

            // Create a request for the URL. 
            WebRequest request = WebRequest.Create(_url + "?" + _parameters + "&" + search);
            try
            {
                string responseFromServer = "";
                // Get the response.
                response = request.GetResponse();
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                // Clean up the streams and the response.
                reader.Close();
                response.Close();

                metadatas = convertToMetadatas(JsonConvert.DeserializeObject<T>(responseFromServer));
            }
            catch
            {
                // Si l'API n'a pas pu être contactée

                metadatas = new Metadatas();
                metadatas.Status = Status.NoConnetion;
            }

            return metadatas;
        }

        /// <summary>
        /// Elimine les caractères qui ne passent pas dans l'url
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected string escape(string text)
        {
            string pattern = @"&";

            text = Regex.Replace(text, pattern, "");

            return text;
        }
    }
}
