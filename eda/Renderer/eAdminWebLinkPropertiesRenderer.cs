using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Génération d'un bloc de paramétrage d'un lien web
    /// </summary>
    public class eAdminWebLinkPropertiesRenderer : eAdminSpecifPropertiesRenderer
    {
        bool _planningTab = false;

        private eAdminWebLinkPropertiesRenderer(ePref pref, int tab, int specifID) :
            base(pref, eAdminUpdateProperty.CATEGORY.SPECIFS, tab, specifID,
                new List<eLibConst.SPECIF_TYPE>() { eLibConst.SPECIF_TYPE.TYP_FILE, eLibConst.SPECIF_TYPE.TYP_LIST, eLibConst.SPECIF_TYPE.TYP_SPECIF_CALENDAR_MODE }, "partWebLink")
        {
            _blockTitle = eResApp.GetRes(Pref, 7809);
        }

        /// <summary>
        /// Génération d'un bloc de paramétrage d'un lien web
        /// </summary>
        /// <param name="pref">ePref.</param>
        /// <param name="tab">Descid de la table</param>
        /// <param name="specifID">ID de la spécif</param>
        /// <returns></returns>
        public static eAdminWebLinkPropertiesRenderer CreateAdminWebLinkPropertiesRenderer(ePref pref, int tab, int specifID)
        {
            return new eAdminWebLinkPropertiesRenderer(pref, tab, specifID);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                eAdminTableInfos tabInfos = eAdminTableInfos.GetAdminTableInfos(_ePref, _tab);
                _planningTab = tabInfos.EdnType == EudoQuery.EdnType.FILE_PLANNING;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Définit les libellés des champs
        /// </summary>
        protected override void SetLabels()
        {
            base.SetLabels();

            _labelName = eResApp.GetRes(Pref, 7808);
            _labelAdminURL = eResApp.GetRes(Pref, 7812);
        }

        /// <summary>
        /// Définit les modes d'ouverture de spécif
        /// </summary>
        protected override void SetAvailableOpenModes()
        {
            _openModes = new List<eLibConst.SPECIF_OPENMODE> { eLibConst.SPECIF_OPENMODE.MODAL, eLibConst.SPECIF_OPENMODE.NEW_WINDOW };
        }

        /// <summary>
        /// Génère le contenu
        /// </summary>
        protected override void BuildParametersContent(Panel panel = null)
        {
            base.BuildParametersContent(panel);
        }


        /// <summary>
        /// Création de champ spécifique au renderer
        /// </summary>
        protected override void CreateAdditionalFields()
        {
            // Emplacement du lien web
            Dictionary<String, String> items = new Dictionary<string, string>();
            items.Add(eLibConst.SPECIF_TYPE.TYP_FILE.GetHashCode().ToString(), eResApp.GetRes(Pref, 190)); // Fiche
            items.Add(eLibConst.SPECIF_TYPE.TYP_LIST.GetHashCode().ToString(), eResApp.GetRes(Pref, 179)); // Liste
            if (_planningTab)
                items.Add(eLibConst.SPECIF_TYPE.TYP_SPECIF_CALENDAR_MODE.GetHashCode().ToString(), eResApp.GetRes(Pref, 135)); // Calendrier

            Dictionary<string, string> attr = new Dictionary<string, string>();
            attr.Add("fid", _specifID.ToString());

            eAdminField field = new eAdminRadioButtonField(
                descid: _tab,
                label: eResApp.GetRes(Pref, 7813),
                propCat: eAdminUpdateProperty.CATEGORY.SPECIFS,
                propCode: eLibConst.SPECIFS.SPECIFTYPE.GetHashCode(),
                groupName: "rbSpecifType",
                items: items,
                value: _specif.Type.GetHashCode().ToString(),
                customRadioButtonAttributes: attr

                );

            field.IsLabelBefore = true;

            field.Generate(_panelContent);


            // Type spécif Produits/Classique
            items = new Dictionary<string, string>();
            items.Add(eLibConst.SPECIF_SOURCE.SRC_XRM.GetHashCode().ToString(), eResApp.GetRes(_ePref, 1874)); // XRM : CLassique
            items.Add(eLibConst.SPECIF_SOURCE.SRC_EXT.GetHashCode().ToString(), eResApp.GetRes(_ePref, 1875)); // PRODUITD : Produits



            field = new eAdminRadioButtonField(
                descid: _tab,
                label: eResApp.GetRes(_ePref, 1873),
                propCat: eAdminUpdateProperty.CATEGORY.SPECIFS,
                propCode: (int)eLibConst.SPECIFS.SOURCE,
                groupName: "rbSpecifSource",
                items: items,
                value: ((int)_specif.Source).ToString(),
                customRadioButtonAttributes: attr

                );

            field.IsLabelBefore = true;

            field.Generate(_panelContent);


            // Type spécif Produits/Classique
            var itemTokens = new Dictionary<string, string>();
            itemTokens.Add("0", "POST"); // POST
            itemTokens.Add("1", "GET"); // GET

 

            eAdminRadioButtonField fieldToken = new eAdminRadioButtonField(
                descid: _tab,
                label: "Type de lancement",
                propCat: eAdminUpdateProperty.CATEGORY.SPECIFS,
                propCode: (int)eLibConst.SPECIFS.ISSTATIC,
                groupName: "rbSpecifStatic",
                items: itemTokens,
                value: ((_specif?.IsStatic ?? false) ? "1" : "0"),
                customRadioButtonAttributes: attr

                );

            fieldToken.IsLabelBefore = true;

            if (this.Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
                fieldToken.Generate(_panelContent);
        }
    }



}