using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Net;
using System.Configuration;
using System.Globalization;
using System.Text;
using Com.Eudonet.Xrm;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

/// <summary>
/// Regroupe les les methodes utiles pour interroger BingMaps
/// </summary>
public class BingRequest
{
    /// <summary>Clé de licence pour Microsoft Bing Maps v8 - Ajoutée après appel à CanRunBigMapsAutoSuggest() par eMain
    /// Indispensable pour utiliser les fonctionnalités Bing Maps, doit être valide (non expirée) et activée sur le site de Microsoft
    /// ATTENTION, CETTE CLE DOIT ÊTRE EGALEMENT MISE A JOUR DANS Internal.eLibConst !</summary>
    public const string BING_MAPS_KEY = "Aia9V-TFKUb44CNZsVp_oxYGgszFUgksJal8-_IW1SSbodepQ4didGSMVp4UiSwR";

    /// <summary>
    /// Envoie la requete et retourne un fichier XML 
    /// </summary>
    /// <param name="requestUrl">URL de la requette</param>
    /// <param name="_error">l'erreur en cas d'echec</param>
    /// <returns>Un document XML contenant les informations trouvées</returns>
    public static XmlDocument MakeRequest(string requestUrl, out string _error)
    {
        _error = string.Empty;
      
        try
        {
            //   Uri url = new Uri(requestUrl);

            HttpWebResponse response = eDataTools.ProcessRequest(requestUrl, out _error);

            if (_error.Length > 0)
                return null;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(response.GetResponseStream());
            return (xmlDoc);

        }
        catch (Exception e)
        {
            _error = e.Message;
            return null;
        }
    }
   

    /// <summary>
    /// Construit la requete à envoyer pour récuperer les coordonnées 
    /// </summary>
    /// <param name="_queryString">l'adresse à rechercher</param>
    /// <param name="_key">Clé de licence Bing Maps</param>
    /// <returns>retourne l'url complete</returns>
    public static string CreateRequest(string _queryString, string _key)
    {
        //     queryString = queryString.Replace(" ", "%20");
        // Erreur 404 en cas d'espace à la fin de l'adresse
        _queryString = _queryString.TrimEnd(' ');
        string UrlRequest = string.Concat("http://dev.virtualearth.net/REST/v1/Locations/",
                             _queryString,
                             "?maxResults=1",
                             "&o=xml",
                             " &key=", _key);

        return (UrlRequest);
    }



    /// <summary>
    /// Géolocalisation d'une adresse physique
    /// </summary>
    /// <param name="_adresse">Adresse à géolocaliser</param>
    /// <param name="bIsFranceAdr">Indiquer s'il faut forcer le pays à France pour toutes les adresses</param>
    /// <param name="_latitude">Latitude retournée</param>
    /// <param name="_longitude">Longitude retournée</param>
    /// <returns></returns>
    public static string GeolocaliseAdresse(string _adresse, bool bIsFranceAdr, out string _latitude, out string _longitude)
    {
        _latitude = string.Empty;
        _longitude = string.Empty;
        string _error = string.Empty;

        if (_adresse.Length < 1)
            return "Adresse incorrecte";

        if (bIsFranceAdr)
            _adresse = string.Concat(_adresse, ", France");
        try
        {
            string req = string.Empty;

            req = BingRequest.CreateRequest(_adresse, BingRequest.BING_MAPS_KEY);

            XmlDocument xmlResponse = BingRequest.MakeRequest(req, out _error);

            if (xmlResponse != null)
            {
                //Create namespace manager
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlResponse.NameTable);
                nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");

                XmlNodeList locationElements = xmlResponse.SelectNodes("//rest:Point", nsmgr);

                // Recup des coordonnées Lambert
                foreach (XmlNode location in locationElements)
                {
                    if (location.SelectSingleNode("//rest:Latitude", nsmgr) != null)
                        _latitude = location.SelectSingleNode(".//rest:Latitude", nsmgr).InnerText;

                    if (location.SelectSingleNode("//rest:Longitude", nsmgr) != null)
                        _longitude = location.SelectSingleNode(".//rest:Longitude", nsmgr).InnerText;

                    return string.Empty;
                }
            }
            else
            {
                return "Coordonnées non trouvées";
            }
            return "Coordonnées non trouvées";
        }
        catch (Exception ex2)
        {
            return ex2.Message;
        }

    }



}
