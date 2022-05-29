using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.renderer
{
    /// <summary>
    /// Rendu des permissions
    /// </summary>
    /// <authors>GCH</authors>
    /// <date>2014-04-24</date>
    public class ePermissionRenderer
    {
        #region Variables
        /// <summary>Préférence de l'utilisateur</summary>
        private ePref _pref = null;
        /// <summary>Libellé de la colonne nom du rapport/Modèle... (dépend du contexte)</summary>
        private string _sLabel = string.Empty;
        /// <summary>Indique si filtre public est coché</summary>
        private bool _bPublic = false;
        /// <summary>Permission de visu</summary>
        private ePermission _viewPerm = null;
        /// <summary>Permission de MAJ</summary>
        private ePermission _updPerm = null;
        /// <summary>libellé à afficher</summary>
        private string _sName = string.Empty;

        private IRightTreatment _eRT = null;

        private bool _bLabelReadOnly = false;

        public enum PermType
        {
            VIEW,
            UPDATE
        }

        public bool DoAddPermOptions = true;
        #endregion

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur</param>
        /// <param name="sLabel">Libellé de la colonne nom du rapport/Modèle... (dépend du contexte)</param>
        /// <param name="sName">Libellé à afficher</param>
        /// <param name="bPublic">Indique si filtre public est coché</param>
        /// <param name="viewPermId">permission de visu : id de perm</param>
        /// <param name="updatePermId">permission de MAJ : id de perm</param>
        /// <param name="viewPermMode">permission de visu : mode de permission choisi</param>
        /// <param name="viewPermUsersId">permission de visu : users/groups possèdant les droits</param>
        /// <param name="viewPermLevel">permission de visu : niveau minimum</param>
        /// <param name="updPermMode">permission de MAJ : mode de permission choisi</param>
        /// <param name="updPermUsersId">permission de MAJ : users/groups possèdant les droits</param>
        /// <param name="updPermLevel">permission de MAJ : niveau minimum</param>
        /// <param name="eRT">Ensemble des droits de traitements correspondants au cas en cours(eRightFilter pour les filtres)</param>
        /// <param name="bLabelReadOnly">droit de modif du libellé </param>
        public ePermissionRenderer(ePref pref, string sLabel = "", string sName = "", bool bPublic = false, Int32 viewPermId = 0, Int32 updatePermId = 0
                , ePermission.PermissionMode viewPermMode = ePermission.PermissionMode.MODE_NONE, string viewPermUsersId = "", Int32 viewPermLevel = 0
                , ePermission.PermissionMode updPermMode = ePermission.PermissionMode.MODE_NONE, string updPermUsersId = "", Int32 updPermLevel = 0
                , IRightTreatment eRT = null, bool bLabelReadOnly = false
            )
        {
            _pref = pref;
            _bPublic = bPublic;
            _sLabel = (sLabel.Length > 0) ? sLabel : eResApp.GetRes(_pref, 6544);
            _sName = sName;
            _bLabelReadOnly = bLabelReadOnly;

            #region Permissions

            #region View
            _viewPerm = new ePermission(viewPermId, _pref);
            if (viewPermUsersId.Length > 0)
                _viewPerm.PermUser = viewPermUsersId;
            if (viewPermLevel > 0)
                _viewPerm.PermLevel = viewPermLevel;
            if (viewPermMode != ePermission.PermissionMode.MODE_NONE)
                _viewPerm.PermMode = viewPermMode;
            _viewPerm.CheckPermMode();
            #endregion

            #region update
            _updPerm = new ePermission(updatePermId, _pref);
            if (updPermUsersId.Length > 0)
                _updPerm.PermUser = updPermUsersId;
            if (updPermLevel > 0)
                _updPerm.PermLevel = updPermLevel;
            if (updPermMode != ePermission.PermissionMode.MODE_NONE)
                _updPerm.PermMode = updPermMode;
            _updPerm.CheckPermMode();
            #endregion

            #endregion

            _eRT = eRT;
        }

        public ePermissionRenderer(ePref pref, bool bPublic, ePermission viewPerm, ePermission updatePerm, IRightTreatment eRT = null)
        {
            _pref = pref;
            _bPublic = bPublic;
            _sLabel = eResApp.GetRes(_pref, 6544);
            _sName = "";

            #region Permissions

            #region View
            if (viewPerm != null)
            {
                _viewPerm = viewPerm;
                _viewPerm.CheckPermMode();
            }
            else
            {
                _viewPerm = new ePermission(0, _pref);
            }
            #endregion

            #region update
            if (updatePerm != null)
            {
                _updPerm = updatePerm;
                _updPerm.CheckPermMode();
            }
            else
            {
                _updPerm = new ePermission(0, _pref);
            }
            #endregion

            #endregion

            _eRT = eRT;
        }


        /// <summary>
        /// Enregistrer sous d'un modèle
        /// </summary>
        public HtmlGenericControl GetSaveAsBlock()
        {
            HtmlGenericControl DivModelName = new HtmlGenericControl("div");
            DivModelName.ID = "DivModeleName";
            DivModelName.Attributes.Add("class", "DivModeleName");

            HtmlGenericControl DivLabel = new HtmlGenericControl("span");
            DivLabel.ID = "DivLabel";
            DivLabel.InnerText = _sLabel;
            DivModelName.Controls.Add(DivLabel);

            if (_bLabelReadOnly)
            {
                Label lb = new Label();
                lb.ID = "PermName";
                lb.Attributes.Add("name", "PermName");
                lb.Text = _sName;
                lb.CssClass = "ModeleLabelName readonly";
                DivModelName.Controls.Add(lb);
            }
            else
            {
                TextBox tb = new TextBox();
                tb.ID = "PermName";
                tb.Attributes.Add("name", "PermName");
                tb.CssClass = _bLabelReadOnly ? "ModeleName" : "ModeleName";
                tb.Text = _sName;
                tb.ReadOnly = _bLabelReadOnly;
                DivModelName.Controls.Add(tb);
            }


            if (DoAddPermOptions)
                DivModelName.Controls.Add(GetSavePermOptions());

            return DivModelName;
        }

        /// <summary>Rendu du bloc Options de permission</summary>
        /// <returns></returns>
        public HtmlGenericControl GetSavePermOptions()
        {
            HtmlGenericControl DivOptBlock = new HtmlGenericControl("div");
            DivOptBlock.ID = "DivOptBlock";
            Label divTitle = new Label();
            divTitle.CssClass = "OptTtlFilt";
            divTitle.Text = eResApp.GetRes(_pref, 6293);
            DivOptBlock.Controls.Add(divTitle);
            Panel divSpace = new Panel();
            divSpace.CssClass = "divSpace";
            divSpace.Controls.Add(new LiteralControl(" "));
            DivOptBlock.Controls.Add(divSpace);

            bool bPublicDisabled = _eRT != null && _eRT.GetType() == typeof(eRightFilter) && !((eRightFilter)_eRT).HasRight(eLibConst.TREATID.PUBLIC_FILTER);
            DivOptBlock.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(_pref, 8420), "OptPublicFilter", _bPublic, bPublicDisabled, "PermOpt", "onCheckOption"));

            GetHtmlRender(_viewPerm, PermType.VIEW, _pref, DivOptBlock);

            GetHtmlRender(_updPerm, PermType.UPDATE, _pref, DivOptBlock);

            //Hiddens pour les perm
            DivOptBlock.Controls.Add(GetInputText("ViewPermId", _viewPerm.PermId.ToString(), false));
            DivOptBlock.Controls.Add(GetInputText("ViewPermUsersId", _viewPerm.PermUser.ToString(), false));
            DivOptBlock.Controls.Add(GetInputText("ViewPermLevel", _viewPerm.PermLevel.ToString(), false));
            DivOptBlock.Controls.Add(GetInputText("ViewPermMode", _viewPerm.PermMode.GetHashCode().ToString(), false));

            DivOptBlock.Controls.Add(GetInputText("UpdatePermId", _updPerm.PermId.ToString(), false));
            DivOptBlock.Controls.Add(GetInputText("UpdatePermUsersId", _updPerm.PermUser.ToString(), false));
            DivOptBlock.Controls.Add(GetInputText("UpdatePermLevel", _updPerm.PermLevel.ToString(), false));
            DivOptBlock.Controls.Add(GetInputText("UpdatePermMode", _updPerm.PermMode.GetHashCode().ToString(), false));

            return DivOptBlock;
        }

        private static HtmlGenericControl GetInputText(string id, string value, bool bVisible)
        {
            HtmlGenericControl inpt = new HtmlGenericControl("input");
            inpt.ID = id;
            inpt.Attributes.Add("value", value);
            inpt.Style.Add(HtmlTextWriterStyle.Display, bVisible ? "block" : "none");
            return inpt;
        }
        /// <summary>
        /// Retourne un rendu de type controle HTMLGenericControl pour cette permission
        /// </summary>
        /// <param name="control">RETOUR : Ajoute au control "control" le HtmlGenericControl sous forme de System.Web.UI.Control</param>
        /// <param name="perm">Information de permission à afficher</param>
        /// <param name="permType">Type de permission sous forme de chaine pour l'utiliser comme identifiant du controle dans l'interface html)</param>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <returns>HtmlGenericControl sous forme de System.Web.UI.Control </returns>
        internal static void GetHtmlRender(ePermission perm, PermType permType, ePref pref, HtmlGenericControl control, string sSubControl = "div")
        {
            string sPermType = permType.ToString().ToCapitalize();
            if (perm == null)
                perm = new ePermission(0, pref);
            if (perm == null)
                perm = new ePermission(0, pref);
            string sLabelPerm = string.Empty;
            switch (permType)
            {
                case PermType.VIEW:
                    sLabelPerm = eResApp.GetRes(pref, 1033);   //Libellé des drtois de visu
                    break;
                case PermType.UPDATE:
                    sLabelPerm = eResApp.GetRes(pref, 1034);   //Libellé des droit de modif
                    break;
            }

            #region Libellé principal

            control.Controls.Add(eTools.GetCheckBoxOption(sLabelPerm, string.Concat("Opt", sPermType, "Filter"), perm.HasPerm, false, "PermOpt", "onCheckOption", sSubControl));

            #endregion

            #region sous partie
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "DivPerm");

            bool bLevelChecked = perm.PermMode == ePermission.PermissionMode.MODE_LEVEL_ONLY
                  || perm.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL;
            /**/
            HtmlGenericControl subDiv = new HtmlGenericControl("div");
            subDiv.Attributes.Add("class", "CheckPermOpt");
            div.Controls.Add(subDiv);
            subDiv.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(pref, 249), string.Concat("OptLevels_", sPermType), bLevelChecked, false, "ChkPerm", "onCheckOption"));

            HtmlGenericControl lvlList = new HtmlGenericControl("select");
            lvlList.ID = string.Concat("LevelLst_", sPermType);
            if (!bLevelChecked)
                lvlList.Disabled = true;
            HtmlGenericControl itm;
            for (int i = 1; i <= 5; i++)
            {
                itm = new HtmlGenericControl("option");
                itm.InnerText = i.ToString();
                itm.Attributes.Add("value", i.ToString());
                if (perm.PermLevel == i)
                    itm.Attributes.Add("selected", "selected");
                lvlList.Controls.Add(itm);
            }

            // TODO SUPERADMIN ?
            itm = new HtmlGenericControl("option");
            itm.InnerText = eLibTools.GetUserLevelLabel(pref, UserLevel.LEV_USR_ADMIN);
            itm.Attributes.Add("value", UserLevel.LEV_USR_ADMIN.GetHashCode().ToString());
            if (perm.PermLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                itm.Attributes.Add("selected", "selected");

            lvlList.Controls.Add(itm);
            subDiv.Controls.Add(lvlList);
            /**/
            /**/
            subDiv = new HtmlGenericControl("div");
            subDiv.Attributes.Add("class", "CheckPermOpt");
            div.Controls.Add(subDiv);
            bool bUserChecked = perm.PermMode == ePermission.PermissionMode.MODE_USER_ONLY
              || perm.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL;
            subDiv.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(pref, 250), string.Concat("OptUsers_", sPermType), bUserChecked, false, "ChkPerm", "onCheckOption"));
            HtmlGenericControl userInput = new HtmlGenericControl("input");
            userInput.ID = string.Concat("TxtUsers_", sPermType);
            if (!bUserChecked)
                userInput.Attributes.Add("style", "display:none;");
            userInput.Attributes.Add("readonly", "readonly");
            IDictionary<string, string> dicoUser = eLibDataTools.GetUserLogin(pref, perm.PermUser);
            perm.PermUser = string.Join(";", dicoUser.Keys);
            userInput.Attributes.Add("value", string.Join(", ", dicoUser.Values));
            userInput.Attributes.Add("ednvalue", perm.PermUser);
            subDiv.Controls.Add(userInput);

            HtmlGenericControl userLnk = new HtmlGenericControl("span");
            userLnk.ID = string.Concat("UsersLink_", sPermType);
            if (!bUserChecked)
                userLnk.Attributes.Add("style", "display:none;");
            userLnk.Attributes.Add("onclick", string.Concat("SetUsersPerm('TxtUsers_", sPermType, "')"));
            userLnk.Attributes.Add("class", "icon-catalog");
            subDiv.Controls.Add(userLnk);
            /**/
            HtmlGenericControl DivOptViewLink = new HtmlGenericControl(sSubControl);
            DivOptViewLink.ID = string.Concat("Opt", sPermType, "FilterLink");
            DivOptViewLink.Attributes.Add("class", "ParamLink");
            DivOptViewLink.Controls.Add(div);
            DivOptViewLink.Style.Add(HtmlTextWriterStyle.Display, perm.HasPerm ? "block" : "none");
            control.Controls.Add(DivOptViewLink);
            #endregion
        }
    }
}