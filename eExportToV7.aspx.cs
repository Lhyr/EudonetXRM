using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// DEVELOPPEUR	: SPH
    /// DATE			: 10/06/2013
    /// DESCRIPTION 			: Transfert Spécif XRM -> V7
    ///   	Cette page construit un formulaire et le poste
    ///   	vers la v7. Celui-ci contient le token
    ///   	et les informations nécessaire à l'ouverture en 
    ///   	session d'une spécif v7.
    /// </summary>
    public partial class eExportToV7page : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// Chargement des infos en qs et appel à eSpecifToken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //
            String error = String.Empty;

            //Id de l'url de la spécif
            Int32 nUrlId = 0;

            //Url de la spécif
            String sUrl = String.Empty;

            //type de spécif
            Int32 nType = 0;
            eLibConst.SPECIF_TYPE cType = eLibConst.SPECIF_TYPE.TYP_SPECIF;

            //fileid et table depuis laquelle l'appel a été fait
            Int32 nFileId = 0;
            Int32 nTab = 0;
            Int32 nDescId = 0;


            //URL
            // soit un id, soit une "vrai" url
            if (!_allKeysQS.Contains("id"))
            {

                String sDevMsg = "pas de spécif";

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));

                LaunchError(ErrorContainer, RequestContentType.HTML);
            }


            sUrl = Request.QueryString["id"].ToString();

            //
            if (!(_allKeysQS.Contains("type") && Int32.TryParse(Request.QueryString["type"], out nType) && nType >= 1 && nType <= 8))
            {


                String sDevMsg = "type de spécif invalide";

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));

                LaunchError(ErrorContainer, RequestContentType.HTML);
            }

            cType = (eLibConst.SPECIF_TYPE)nType;

            //Si type spécif classique ou favoiris, 
            if (cType == eLibConst.SPECIF_TYPE.TYP_SPECIF || cType == eLibConst.SPECIF_TYPE.TYP_FAVORITE)
                Int32.TryParse(sUrl, out nUrlId);

            //Table et fileid
            if (_allKeysQS.Contains("tab") && Int32.TryParse(Request.QueryString["tab"], out nTab))
            {
                if (_allKeysQS.Contains("descid") && Int32.TryParse(Request.QueryString["descid"], out nDescId))
                {
                    //ok
                }
                if (_allKeysQS.Contains("fileid") && Int32.TryParse(Request.QueryString["fileid"], out nFileId))
                {
                    //ok
                }
            }


            //Construction du token
            eSpecifToken es = eSpecifToken.GetSpecifTokenV7(_pref, cType, sUrl, nTab, nFileId, nDescId);
            if (es.IsError)
            {

                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (es.InnerException != null)
                {
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", es.InnerException.Message, Environment.NewLine, "Exception StackTrace :", es.InnerException.StackTrace);
                }
                else
                {
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message d'erreur : ", es.ErrorMsg);
                }

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                LaunchError(ErrorContainer, RequestContentType.HTML);
            }


            //token d'information
            token.InnerText = es.Token;

            //type de spécif
            typespecif.Value = cType.GetHashCode().ToString();

            //Url de page v7 de transfert
            exporttov7.Action = String.Concat(es.BaseSiteV7URL, "app/ExportFromXRMDialog.asp");
        }
    }
}