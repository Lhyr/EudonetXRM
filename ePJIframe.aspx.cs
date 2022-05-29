using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Affichage d'une iframe permettant d'afficher la PJ sécurisée : permet d'afficher le titre de la page
    /// </summary>
    public partial class ePJIframe : eExternalPage<LoadQueryStringPJ>, System.Web.SessionState.IRequiresSessionState
    {

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessPage()
        {


            try
            {
                errorContainer.Visible = false;
                errorTitle1.InnerText = eResApp.GetRes(_pref, 416);
                errorTitle2.InnerText = eResApp.GetRes(_pref, 7175);
                PageRegisters.AddCss("ePJ");

                ePJ pj;

                LoadInfosPj(out pj);
                if (pj != null)
                {

                    this.PageTitle = Path.GetFileNameWithoutExtension(pj.FileName);

                    CreateIframe();
                }


            }
            catch (PjExp exp)
            {
                // Utile pour le message d'erreur à l'utilisateur
                _panelErrorMsg = exp.Message;
            }

            if (RendType == eExternalPage<LoadQueryStringPJ>.ExternalPageRendType.ERROR)
            {
                errorContainer.Visible = true;
            }


        }

        /// <summary>
        /// Charge les tokens du tracking de la queryString
        /// </summary>
        protected override void LoadQueryString()
        {
            DataParam = new LoadQueryStringPJ(_pageQueryString.UID, _pageQueryString.Cs, _pageQueryString.P);
        }

        /// <summary>
        /// Type d'external page
        /// </summary>
        protected override eExternal.ExternalPageType PgTyp { get { return eExternal.ExternalPageType.PJ; } }

        /// <summary>
        /// Retourne le type (nom) de la page pour reconstruire l'UR
        /// </summary>
        /// <returns></returns>
        protected override ExternalUrlTools.PageName GetRedirectPageName()
        {
            return ExternalUrlTools.PageName.AT;
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Creates the iframe.
        /// </summary>
        void CreateIframe()
        {
            //formPJIFrame

            HtmlIframe iframe = new HtmlIframe();
            iframe.ID = "pjIframe";
            iframe.Src = String.Concat("pj.aspx?", this.Context.Request.QueryString.ToString());
            iframe.Attributes.Add("frameBorder", "0");
            formPJIFrame.Controls.Add(iframe);
        }


        /// <summary>
        /// Recupere un object pj a partir des informations recuperer de la query string
        /// </summary>
        /// <param name="pjToDisplay">Objet pj a chargé</param>
        void LoadInfosPj(out ePJ pjToDisplay)
        {
            int pjId = DataParam.ParamData.PjId;

            //Si pas d id fourni on retourne un message d'erreur
            if (pjId == 0)
            {
                throw new PjExp(eResApp.GetRes(_pref, 8261));
            }
            else
            {
                //Si la pj n'a pas été trouvé en base on leve un message
                pjToDisplay = ePJ.CreatePJ(_pref, pjId);
                if (pjToDisplay == null)
                    throw new PjExp(eResApp.GetRes(_pref, 8262));
            }
        }

        /// <summary>
        /// Gestion de l'affichage du message d'erreur à l'utilisateur si une erreur c'est produite
        /// </summary>
        protected override void RendTitleAndErrorMsg()
        {
            // Si _panelErrorMsg est déjà défini alors c'est un message bien identifié pour l'utilisateur
            if (string.IsNullOrEmpty(_panelErrorMsg))
            {
                base.RendTitleAndErrorMsg();
            }
            else
            {
                // On active le panneau d'erreur
                RendType = ExternalPageRendType.ERROR;
            }
        }
    }
}