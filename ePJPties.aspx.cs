using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using System;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// page permettant d'afficher les propriétés de l'annexe
    /// </summary>
    public partial class ePJPties : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }



        /// <summary>
        /// traitement à lancer au chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("ePjPties", "all");


            #endregion


            #region ajout des js

            PageRegisters.AddScript("eFile");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("ePJPties");
            PageRegisters.AddScript("eSchedule");


            #endregion


            Int32 iPjid = 0;
            string sError = string.Empty;

            if (!_allKeys.Contains("pjid")
                    || Request.Form["pjid"] == null
                    || string.IsNullOrEmpty(Request.Form["pjid"])
                    || !Int32.TryParse(Request.Form["pjid"].ToString(), out iPjid)
                    || !(iPjid > 0))
            {
                string sDevMsg = "ePjPties.aspx : Aucun identifiant d'annexe n'a été fourni.";
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6631), "", devMsg: sDevMsg));
            }


            bool bRo = (_allKeys.Contains("ro") && Request.Form["ro"].ToString() == "1");
            bool bSupp = (!_allKeys.Contains("sup") || Request.Form["sup"].ToString() == "1");

            ePJ pj = ePJ.CreatePJ(_pref, iPjid);

            if (pj.ErrorContainer != null)
            {
                /*
                //Pas de message dev : il a été envoyé directement via ePJ
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    ""
                    );
                */

                LaunchError(pj.ErrorContainer);

                return;
            }


            bool bFound = false;
            // 102011 : Onglet
            // 102012 : Fiche
            eRes res = new eRes(_pref, string.Concat("102012,102011"));
            lblEudoFile.InnerHtml = res.GetRes(102012, out bFound);


            if (pj.EudoTabType == EudoQuery.EdnType.FILE_MAIN)
            {
                tdEudoFile.InnerHtml = pj.EudoFileName;
            }
            else
            {
                tdEudoFile.InnerHtml = "-";
            }

            lblEudoTable.InnerHtml = res.GetRes(102011, out bFound);
            tdEudoTable.InnerHtml = pj.EudoTabName;

            //Nom du fichier
            lblFileName.InnerText = eResApp.GetRes(_pref, 103);
            inptFileName.Value = pj.FileName;
            //if (!pj.IsPjUpdatable)
            //    inptFileName.Disabled = true;
            oldFileName.Value = pj.FileName;


            // type de fichier
            lblType.InnerText = eResApp.GetRes(_pref, 105);
            tdFileType.InnerText = pj.Type;

            // emplacement ?? à conserver ?
            lblPath.InnerText = eResApp.GetRes(_pref, 128);
            tdPath.InnerText = "";

            //Taille
            lblSize.InnerText = eResApp.GetRes(_pref, 106);
            tdSize.InnerText = eLibTools.GetSizeString(_pref, pj.Size);

            lblCrea.InnerText = eResApp.GetRes(_pref, 113);
            tdCrea.InnerText = pj.CreationDate.ToString("F");
            dtCrea.Value = eDate.ConvertBddToDisplay(_pref.CultureInfo, pj.CreationDate.ToShortDateString());
            /*
             KHA le 29/01/2014 - je n'ai pas mis en place la rubrique "modifiée le" présente sur la maquette car aucun champ n'est prévu à cet effet en base
             */
            lblLimitDate.InnerText = eResApp.GetRes(_pref, 8250);
            string valueLimitDate = "";
            if (pj.ExpireDay == null)
                valueLimitDate = "";
            else
                valueLimitDate = eDate.ConvertBddToDisplay(_pref.CultureInfo, pj.ExpireDay.Value.ToShortDateString());

            inptLimitDate.Value = valueLimitDate;

            lblTip.InnerText = eResApp.GetRes(_pref, 130);
            inptTip.Value = pj.ToolTip;

            lblDsc.InnerText = eResApp.GetRes(_pref, 104);
            inptDsc.Value = pj.Description;


            if (pj.PJType == EudoQuery.PjType.FILE)
            {
                PjBuildParam paramPj = new PjBuildParam()
                {
                    AppExternalUrl = eLibTools.GetAppUrl(Request),
                    Uid = _pref.DatabaseUid,
                    TabDescId = pj.EudoTabDescId,
                    PjId = pj.PJId,
                    UserId = _pref.UserId,
                    UserLangId = _pref.LangId
                };

                string sLink = ExternalUrlTools.GetLinkPJ(paramPj);
                lblExtLnk.InnerText = eResApp.GetRes(_pref, 6079);
                inptExtLnk.Value = sLink;
            }
            else
            {
                trExtLnk.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            if (!pj.IsPjUpdatable)
                bRo = true;

            if (!pj.IsPjDeletable)
                bSupp = false;

            ro.Value = bRo ? "1" : "0";
            supp.Value = bSupp ? "1" : "0";

            if (bRo)
            {
                inptTip.Disabled = true;
                inptFileName.Disabled = true;
                inptExtLnk.Disabled = true;
                inptDsc.Disabled = true;
            }
        }
    }
}