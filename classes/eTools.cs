using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using Microsoft.IdentityModel.Claims;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Newtonsoft.Json;
using Com.Eudonet.Merge;
using Newtonsoft.Json.Linq;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{
    /// <className>eTools</className>
    /// <summary>Classe contenant des fonctions utilitaires pour EudonetXRM</summary>
    /// <authors>R et D</authors>
    /// <date>2011-08-31</date>
    public static class eTools
    {

        /// <summary>
        /// retourne le windows account name fourni par le systeme d'authentification extérieur
        /// </summary>
        /// <returns></returns>
        internal static String GetEudoExternalLoginFromExternalAuth()
        {


            String sTimeStamp = String.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), " : ");

            //Type authentif non adfs
            if (HttpContext.Current.User.Identity.AuthenticationType.ToLower() != "federation")
            {
                eModelTools.EudoTraceLogExternalConnexion(String.Concat("MODE D'AUTHENTIFICATION NON FEDERATION"));
                return String.Empty;
            }

            //Non authentifé
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                eModelTools.EudoTraceLogExternalConnexion(String.Concat("UTILISATEUR NON AUTHENTIFIE"));
                return String.Empty;
            }



            string sLoginClaims = eLibTools.GetServerConfig("ADFSLOGINCLAIMS", "windowsaccountname");

            String sWindowAccoutName = String.Empty;
            String sEmail = String.Empty;
            IClaimsPrincipal claimsPr = (IClaimsPrincipal)(HttpContext.Current.User);

            foreach (var id in claimsPr.Identities)
            {
                foreach (Claim cl in id.Claims)
                {
                    eModelTools.EudoTraceLogExternalConnexion(String.Concat("CLAIMS PRESENT DANS LE RETOUR  => ", cl.ClaimType), eModelConst.TypeLogExternalAuth.INFO);

                    if (cl.ClaimType.ToLower().EndsWith(sLoginClaims))
                    {
                        return cl.Value;
                    }
                }
            }


            if (sWindowAccoutName.Length == 0)
            {
                eModelTools.EudoTraceLogExternalConnexion(String.Concat("CLAIM ", sLoginClaims, " non trouvé"));
            }

            return sWindowAccoutName;
        }




        /// <summary>
        /// Retourne la valeur de la clée du fichier serer.config.
        /// Si la clée n'est pas présente, retourne la valeur par défaut
        /// Les clées de server.config ne sont rechargées par IIS
        /// que lorsque web.config est changé
        /// </summary>
        /// <param name="sKey">Clée à rechercher</param>
        /// <param name="sDefaultValue">Valeur par défaut de la clée</param>
        /// <returns>Valeur de la clée</returns>
        internal static String GetServerConfig(String sKey, String sDefaultValue = "")
        {
            System.Collections.Specialized.NameValueCollection server = (System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("ServerSpecificSettings");

            if (server.AllKeys.Contains(sKey))
                return server[sKey];
            else if (ConfigurationManager.AppSettings.AllKeys.Contains(sKey))
                return ConfigurationManager.AppSettings.Get(sKey);
            else
                return sDefaultValue;

        }



        /// <summary>
        /// Retourne l'id de la fiche d'un champ de liaison
        /// ex si une fiche est un catalogue spécial, retourne l'id de la fiche liée par le catalogue
        /// </summary>
        /// <param name="fieldRow"></param>
        /// <returns></returns>
        internal static String GetLnkId(eFieldRecord fieldRow)
        {
            // MAB - US #1586 - Tâche #3265 - Demande #75 895 - Minifiche sur les champs Alias
            if ((fieldRow.FldInfo.Popup == PopupType.SPECIAL || fieldRow.FldInfo.AliasSourceField?.Popup == PopupType.SPECIAL)
                && fieldRow.Value.Length > 0)
            {
                return fieldRow.Value;
            }
            else if (fieldRow.IsLink && (fieldRow.Value.Length > 0 || fieldRow.FileId != 0))
            {
                return fieldRow.FileId.ToString();
            }

            return "0";
        }



        /// <summary>
        /// Propriété "ename" des cellules sur XRM
        /// Nom de la cellule - commun à toutes la colonne, entête inclus
        /// </summary>
        /// <param name="row">Record</param>
        /// <param name="ef">FieldRecord</param>
        /// <returns></returns>
        internal static String GetFieldValueCellName(eRecord row, eFieldRecord ef)
        {
            return GetFieldValueCellName(row.CalledTab, ef.FldInfo.Alias);
        }

        /// <summary>
        /// Propriété "ename" des cellules sur XRM
        /// Nom de la cellule - commun à toutes la colonne, entête inclus
        /// </summary>
        /// <param name="tabCalledDescId">descid de la tab appelé</param>
        /// <param name="fldAlias">alias du field</param>
        /// <returns></returns>
        internal static String GetFieldValueCellName(Int32 tabCalledDescId, String fldAlias)
        {
            String[] sIdCell = fldAlias.Split("_");
            sIdCell[0] = tabCalledDescId.ToString();
            return String.Concat("COL_", String.Join("_", sIdCell));
        }

        /// <summary>
        /// Propriété "id" des cellules sur XRM
        /// </summary>
        /// <param name="row">Record</param>
        /// <param name="ef">FieldRecord</param>
        /// <param name="lineNum">index de la row</param>
        /// <returns>COL_[Alias du field]_[Id de la fiche principale]_[Id de la fiche du champs]_[Numéro de ligne]</returns>
        internal static String GetFieldValueCellId(eRecord row, eFieldRecord ef, Int32 lineNum = 0)
        {
            return String.Concat(GetFieldValueCellName(row, ef), "_", row.MainFileid, "_", ef.FileId, "_", lineNum);
        }

        /// <summary>
        /// Propriété "id" des cellules sur XRM
        /// </summary>
        /// <param name="colName">colonne name</param>
        /// <param name="masterId">id de la fiche de la row</param>
        /// <param name="fieldFileId">id de la fiche de la rubrique</param>
        /// <param name="lineNum">index de la row</param>
        /// <returns>COL_[Alias du field]_[Id de la fiche principale]_[Id de la fiche du champs]_[Numéro de ligne]</returns>
        internal static String GetFieldValueCellId(String colName, Int32 masterId, Int32 fieldFileId, Int32 lineNum = 0)
        {
            return String.Concat(colName, "_", masterId, "_", fieldFileId, "_", lineNum);
        }


        /// <summary>
        /// Retourne la taille en pixel d'une chaine de caractères
        /// Par défaut, police Verdana 12
        /// </summary>
        /// <param name="value">String à taillé</param>
        /// <param name="font">définit la Font utilisé</param>
        /// <returns></returns>
        public static Int32 MesureString(String value, System.Drawing.Font font = null)
        {
            if (font == null)
                font = new System.Drawing.Font("Verdana", 9);

            value = HttpUtility.UrlDecode(value);

            System.Drawing.SizeF stringSize = new System.Drawing.SizeF();
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(1, 1);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp);
            stringSize = graphics.MeasureString(value, font);
            return (Int32)stringSize.Width;
        }

        /// <summary>
        /// Retourne un color picker
        /// </summary>
        /// <returns></returns>
        public static Panel GetColorPicker(ePref pref, string selectedColor, string divId, string txtId, string eName, bool isUpdatable, bool readOnly)
        {
            Boolean isReadOnly = (!isUpdatable || readOnly);

            Panel pnlReturn = new Panel();
            pnlReturn.CssClass = "pnlColor";

            Panel _divSelected = new Panel();
            pnlReturn.Controls.Add(_divSelected);
            _divSelected.CssClass = "PersColor";
            _divSelected.ID = divId;
            _divSelected.Attributes.Add("ero", isReadOnly ? "1" : "0");

            TextBox _txtSelected = new TextBox();
            _txtSelected.ID = txtId;
            _txtSelected.Style.Add(HtmlTextWriterStyle.Display, "none");
            _txtSelected.Attributes.Add("ename", eName);

            //SPH : le seul appel a getcolorpicker prend en paramètre field.isUpdatable et field.readOnly
            // pour paramètre.
            // Dans ce cas, readOnly est inutile car field.isUpdatable prend déjà en compte field.readOnly
            // Je laisse en l'état pour l'instant au cas ou planning ai une gestion particulière de ces propriétés

            _txtSelected.Attributes.Add("readonly", isReadOnly ? "readonly" : "0");
            _txtSelected.Attributes.Add("ero", isReadOnly ? "1" : "0");

            //CNA - Demande #57670
            if (isUpdatable)
                _txtSelected.Attributes.Add("eaction", "LNKFREETEXT");

            _txtSelected.Attributes.Add("efld", "1");
            _txtSelected.Attributes.Add("data-ehidden", "1");

            if (!String.IsNullOrEmpty(selectedColor))
            {
                _divSelected.Style.Add(System.Web.UI.HtmlTextWriterStyle.BackgroundColor, selectedColor);
                _txtSelected.Text = selectedColor;
            }


            pnlReturn.Controls.Add(_txtSelected);
            if (!isReadOnly)
                _divSelected.Attributes.Add("onclick", "pickColor(this, document.getElementById('" + txtId + "'));");
            else
                _divSelected.ToolTip = eResApp.GetRes(pref, 882);

            _divSelected.Attributes.Add("value", selectedColor);

            return pnlReturn;
        }

        /// <summary>
        /// Enregistre un cookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="exp"></param>
        /// <param name="response"></param>
        /// <param name="ignoreCase">Ignore la case pour le cryptage du cookies</param>
        [Obsolete("Utiliser EudoHelpers.SaveCookie")]
        public static void SaveCookie(string name, string value, DateTime exp, HttpResponse response, Boolean ignoreCase = true)
        {
            EudoCommonHelper.EudoHelpers.SaveCookie(name, value, exp, response, ignoreCase);
        }

        /// <summary>
        /// lit un cookie
        /// </summary>
        /// <param name="name">Nom du cookie</param>
        /// <param name="request">Context ou se trouve le cookies</param>
        /// <param name="ignoreCase">Sensibilité à la case du cryptage</param>
        [Obsolete("Utiliser EudoHelpers.GetCookie ")]
        public static string GetCookie(string name, HttpRequest request, Boolean ignoreCase = true)
        {
            return EudoCommonHelper.EudoHelpers.GetCookie(name, request, ignoreCase);
        }





        /// <summary>
        /// Retourne un contrôle de type bouton radio 
        /// </summary>
        /// <param name="id">ID unique du bouton radio dans le DOM</param>
        /// <param name="name">Nom du groupe HTML duquel doit dépendre le bouton radio, pour décocher les autres boutons du groupe lorsqu'on le sélectionne</param>
        /// <param name="chked">Indique si le bouton doit être coché/sélectionné</param>
        /// <param name="label">Libellé du bouton radio</param>
        /// <param name="visible">Indique si le bouton radio  doit être visible ou non</param>
        /// <param name="sJSOnClick">Code JavaScript à déclencher au clic</param>
        /// <param name="sValue">Valeur optionnelle à affecter au bouton radio</param>
        /// <param name="sTooltip">Info-bulle à afficher sur le libellé et le bouton radio lui-même. Si vide, on utilise le libellé (label) comme info-bulle</param>
        /// <param name="bCreateRealLabel">true pour créer un libellé avec le tag "label" (provoquant le cochage du bouton radio au clic sur le libellé), ou false pour créer un "span"</param>
        /// <returns></returns>
        public static HtmlGenericControl GetRadioButton(String id, String name, bool chked, string label, bool visible = true, String sJSOnClick = "onSelectRadio(this.id)", String sValue = "", String sTooltip = "", bool bCreateRealLabel = false)
        {
            HtmlGenericControl _radioDiv = new HtmlGenericControl("div");
            _radioDiv.ID = "RadioDiv_" + id;
            _radioDiv.Attributes.Add("class", "RadioDiv");
            HtmlGenericControl _radio = new HtmlGenericControl("input");
            _radioDiv.Controls.Add(_radio);
            _radio.ID = id;
            if (chked)
                _radio.Attributes.Add("checked", "checked");
            _radio.Attributes.Add("name", name);
            _radio.Attributes.Add("type", "radio");
            _radio.Attributes.Add("value", sValue);
            _radio.Attributes.Add("onclick", sJSOnClick);

            HtmlGenericControl _lbl = new HtmlGenericControl(bCreateRealLabel ? "label" : "span");
            _lbl.InnerText = label;
            _lbl.Attributes.Add("title", String.IsNullOrEmpty(sTooltip) ? label : sTooltip); // si aucun tooltip précisé : utilisation du libellé
            if (bCreateRealLabel)
                _lbl.Attributes.Add("for", _radio.ID);
            _radioDiv.Controls.Add(_lbl);
            _radioDiv.Style.Add(HtmlTextWriterStyle.Display, visible ? "block" : "none");
            return _radioDiv;
        }

        /// <summary>
        ///  Ajoute des proprité au bouton Afficher l'historique
        /// </summary>
        /// <param name="pref">Preferences utilisateur</param>
        /// <param name="btnContainer">Div du bouton</param>
        /// <param name="enabled">Filtre disponible  </param>
        /// <param name="activated">Filtre activé</param>
        /// <param name="type">type de rendu : finder, mainlist</param>
        public static void BuildHistoBtn(ePref pref, HtmlGenericControl btnContainer, Boolean enabled, Boolean activated, String type)
        {
            if (!enabled)
            {
                btnContainer.Style.Add("display", "none");
                return;
            }

            btnContainer.ID = "histoFilter";
            btnContainer.Attributes.Add("onclick", "doHistoFilter();");
            btnContainer.Attributes.Add("ednLibHide", eResApp.GetRes(pref, 6216));
            btnContainer.Attributes.Add("ednLibShow", eResApp.GetRes(pref, 6217));
            btnContainer.Attributes.Add("type", type);

            //HtmlGenericControl btnLeft = new HtmlGenericControl("div");
            //btnLeft.Attributes.Add("class", "histoLeftBtn");
            //btnContainer.Controls.Add(btnLeft);

            HtmlGenericControl btnCenter = new HtmlGenericControl("div");
            btnCenter.ID = "histoFilterTxt";
            btnCenter.Attributes.Add("class", "historique");
            btnContainer.Controls.Add(btnCenter);

            //HtmlGenericControl btnRight = new HtmlGenericControl("div");
            //btnRight.Attributes.Add("class", "histoRightBtn");
            //btnContainer.Controls.Add(btnRight);

            // Historique activé
            if (activated)
            {
                // On inverse les actions
                btnCenter.InnerHtml = eResApp.GetRes(pref, 6216);
                btnContainer.Attributes.Add("ednval", "1");
                btnContainer.Attributes.Add("class", "histoFilter histoActive");
            }
            else
            {
                btnCenter.InnerHtml = eResApp.GetRes(pref, 6217);
                btnContainer.Attributes.Add("ednval", "0");
                btnContainer.Attributes.Add("class", "histoFilter");
            }
        }

        /// <summary>
        /// retourne un champ hidden de type input
        /// </summary>
        /// <param name="id">Id de l'input</param>
        /// <returns>controle de type input</returns>
        public static Control GetEmptyInput(String id)
        {
            HtmlGenericControl _inpt = new HtmlGenericControl("input");
            _inpt.Attributes.Add("type", "hidden");
            _inpt.ID = id;
            return _inpt;
        }

        /// <summary>
        /// Donne la couleur en fonction de la date pour les fiches Planning
        /// </summary>
        /// <param name="dDate"></param>
        /// <returns></returns>
        public static string GetDateColor(DateTime dDate)
        {
            string sColor = string.Empty;

            if (dDate < DateTime.Now.Date)
                sColor = eConst.COL_DATE_PAST;

            else
                if (dDate >= DateTime.Now.AddDays(1).Date)
                sColor = eConst.COL_DATE_FUTURE;
            else
                sColor = eConst.COL_DATE_TODAY;

            return sColor;
        }

        /// <summary>
        /// Permet de générer un objet EudoQuery directement depuis un objet ePref
        /// </summary>
        /// <param name="pref">info pref</param>
        /// <param name="dal">connexion sql</param>
        /// <param name="nTab">table principale</param>
        /// <param name="viewQuery">type de requête (Enum ViewQuery)</param>
        /// <param name="eqCaches">gestion de caches eudoquery venant de l'exterieur</param>
        /// <returns>retourne un nouvelle objet EudoQuery</returns>
        public static EudoQuery.EudoQuery GetEudoQuery(ePref pref, eudoDAL dal, int nTab, ViewQuery viewQuery, EqCaches eqCaches = null)
        {
            EqUserInfo equi = EqUserInfo.GetNew(
                pref.UserId, pref.User.UserLevel, pref.User.UserGroupId, pref.User.UserGroupLevel, pref.User.GroupPublic);

            return new EudoQuery.EudoQuery(dal, nTab, equi, viewQuery, pref.Lang, pref.LangServ, eqCaches);
        }




        /// <summary>
        /// Savoir si le navigateur est supporte la cartographie bingmap
        /// 
        /// Au : 06/12/2016
        ///  Supported Browsers
        ///  
        ///        Bing
        ///  
        ///        The Bing Maps V8 Web Control is supported with most modern browsers that are HTML5-enabled, specifically:
        ///        Desktop
        ///         The current and previous version of Microsoft Edge(Windows)
        ///         Internet Explorer 11 (Windows)
        ///         The current and previous version of Firefox(Windows, Mac OS X, Linux)  
        ///         The current and previous version of Chrome(Windows, Mac OS X, Linux)
        ///         The current and previous version of Safari(Mac OS X)
        ///  
        ///        Note: Internet Explorer's Compatibility View is not supported.
        ///        https://msdn.microsoft.com/en-us/library/mt712867.aspx
        /// </summary>
        /// <returns></returns>
        public static Boolean BrowserSupportedByBing(HttpRequest request)
        {
            // Version de IE
            Regex regEx = new Regex(@"Trident/(?<IEEngineVer>\d+).?\d*");
            Match match = regEx.Match(request.UserAgent);
            if (match.Success)
            {
                /*
                https://msdn.microsoft.com/en-us/library/ms537503(v=vs.85).aspx
                Token       Description
                Trident/7.0 IE11
                Trident/6.0 Internet Explorer 10
                Trident/5.0 Internet Explorer 9
                Trident/4.0 Internet Explorer 8
                */
                // A partir de IE11
                int ver;
                if (int.TryParse(match.Groups["IEEngineVer"].Value, out ver) && ver >= 7)
                    return true;

                // Old IE
                return false;
            }

            // pour les autres,
            return true;
        }



        /// <summary>
        /// Retourne vrai si IE ou EDGE
        /// </summary>
        /// <returns></returns>
        public static bool IsMSBrowser
        {
            get
            {
                if (HttpContext.Current != null & HttpContext.Current.Request != null)
                {
                    string ua = System.Web.HttpContext.Current.Request.UserAgent ?? "";
                    return ua.ToLower().Contains("edge") || ua.ToLower().Contains("trident");
                }
                return false;
            }
        }


        /// <summary>
        /// Retourne un controle de type select
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dicValues">The dic values.</param>
        /// <param name="disabled">if set to <c>true</c> [disabled].</param>
        /// <param name="css">The CSS.</param>
        /// <param name="onChangeAction">The on change action.</param>
        /// <param name="selOpt">The sel opt.</param>
        /// <param name="isMultiple">if set to <c>true</c> [is multiple].</param>
        /// <param name="allowEmptyString">Permet de rajouter une option "vide"</param>
        /// <returns></returns>
        public static HtmlGenericControl GetSelectCombo(string id, Dictionary<String, String> dicValues, Boolean disabled, String css, string onChangeAction, string selOpt, bool isMultiple = false, bool allowEmptyString = false)
        {
            HtmlGenericControl dv = new HtmlGenericControl("select");
            dv.Attributes.Add("class", css);
            dv.ID = id;
            dv.Attributes.Add("onchange", String.Concat(onChangeAction, "(this)"));
            if (disabled)
                dv.Attributes.Add("disabled", "1");

            List<string> valuesList = new List<string>();
            if (isMultiple)
            {
                dv.Attributes.Add("multiple", "multiple");
                valuesList = selOpt.Split(';').ToList();
            }


            foreach (KeyValuePair<string, string> kvp in dicValues)
            {

                if (kvp.Key.Length == 0 || (kvp.Value.Length == 0 && !allowEmptyString))
                    continue;

                HtmlGenericControl opt = new HtmlGenericControl("option");
                dv.Controls.Add(opt);

                opt.Attributes.Add("value", kvp.Key);

                if (kvp.Key.ToLower() == selOpt.ToLower() ||
                    (isMultiple && valuesList.Contains(kvp.Key)))
                    opt.Attributes.Add("selected", "1");

                opt.Controls.Add(new LiteralControl(kvp.Value));
            }
            return dv;

        }

        //SHA : surcharge de GetSelectCombo pour désactiver certaines options
        /// <summary>
        /// Retourne un controle de type select
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dicValues">The dic values.</param>
        /// <param name="disabled">if set to <c>true</c> [disabled].</param>
        /// <param name="css">The CSS.</param>
        /// <param name="onChangeAction">The on change action.</param>
        /// <param name="selOpt">The sel opt.</param>
        /// <param name="isMultiple">if set to <c>true</c> [is multiple].</param>
        /// <param name="allowEmptyString">Permet de rajouter une option "vide"</param>
        /// <returns></returns>
        public static HtmlGenericControl GetSelectCombo(string id, Dictionary<String, Tuple<bool, string>> dicValues, Boolean disabled, String css, string onChangeAction, string selOpt, bool isMultiple = false, bool allowEmptyString = false)
        {
            HtmlGenericControl dv = new HtmlGenericControl("select");
            dv.Attributes.Add("class", css);
            dv.ID = id;
            dv.Attributes.Add("onchange", String.Concat(onChangeAction, "(this)"));
            if (disabled)
                dv.Attributes.Add("disabled", "1");

            List<string> valuesList = new List<string>();
            if (isMultiple)
            {
                dv.Attributes.Add("multiple", "multiple");
                valuesList = selOpt.Split(';').ToList();
            }

            foreach (KeyValuePair<string, Tuple<bool, string>> kvp in dicValues)
            {

                if (kvp.Key.Length == 0 || (kvp.Value.Item2.Length == 0 && !allowEmptyString))
                    continue;

                HtmlGenericControl opt = new HtmlGenericControl("option");
                dv.Controls.Add(opt);

                opt.Attributes.Add("value", kvp.Key);

                if (kvp.Key.ToLower() == selOpt.ToLower() ||
                    (isMultiple && valuesList.Contains(kvp.Key)))
                    opt.Attributes.Add("selected", "1");

                if (kvp.Value.Item1)
                    opt.Attributes.Add("disabled", "1");

                opt.Controls.Add(new LiteralControl(kvp.Value.Item2));
            }
            return dv;
        }


        /// <summary>
        /// Retourne le HTML d'une option case à côcher
        /// </summary>
        /// <returns></returns>
        public static HtmlGenericControl GetCheckBoxOption(string label, string id, Boolean bChecked, Boolean disabled, String css, string onClickFunctionName, String sSubControl = "div", eFieldRecord fld = null)
        {
            HtmlGenericControl dv = new HtmlGenericControl(sSubControl);
            dv.Attributes.Add("class", css);

            eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(bChecked, disabled);
            chkCtrl.ID = string.Concat("chk_", id);
            if (!disabled)
                chkCtrl.AddClick(string.Concat(onClickFunctionName, "('" + id + "');"));
            chkCtrl.AddText(label);
            //BBA - Ajout du style pour la checkbox periodicite
            if (fld != null)
                eTools.SetHTMLControlStyle(fld, chkCtrl);

            HtmlGenericControl chk = new HtmlGenericControl("input");
            chk.Attributes.Add("type", "checkbox");
            if (bChecked)
                chk.Attributes.Add("checked", "checked");
            chk.ID = id;
            chk.Attributes.Add("name", id);
            chk.Style.Add(HtmlTextWriterStyle.Display, "none");

            dv.Controls.Add(chkCtrl);
            dv.Controls.Add(chk);

            return dv;

        }

        /// <summary>
        /// Set les attributs de style tel que le gras, l italic, le souligne et la coleur
        /// </summary>
        /// <param name="fld">Le field row a afficher</param>
        /// <param name="ctrl">L objet html a setter</param>
        public static void SetHTMLControlStyle(eFieldRecord fld, WebControl ctrl)
        {
            if (fld.FldInfo.StyleForeColor.Length > 0) { ctrl.Style.Add(HtmlTextWriterStyle.Color, fld.FldInfo.StyleForeColor); }
            if (fld.FldInfo.StyleBold) { ctrl.Style.Add(HtmlTextWriterStyle.FontWeight, "bold"); }
            if (fld.FldInfo.StyleItalic) { ctrl.Style.Add(HtmlTextWriterStyle.FontStyle, "italic"); }
            if (fld.FldInfo.StyleUnderline) { ctrl.Style.Add(HtmlTextWriterStyle.TextDecoration, "underline"); }
            if (fld.FldInfo.StyleFlat) { ctrl.Style.Add(HtmlTextWriterStyle.BorderStyle, "thin"); }
        }

        /// <summary>
        /// Set les attributs de style tel que le gras, l italic, le souligne et la coleur
        /// </summary>
        /// <param name="fld">Le field row a afficher</param>
        /// <param name="ctrl">L objet html a setter</param>
        public static void SetHTMLControlStyle(eFieldRecord fld, HtmlGenericControl ctrl)
        {
            if (fld.FldInfo.StyleForeColor.Length > 0) { ctrl.Style.Add(HtmlTextWriterStyle.Color, fld.FldInfo.StyleForeColor); }
            if (fld.FldInfo.StyleBold) { ctrl.Style.Add(HtmlTextWriterStyle.FontWeight, "bold"); }
            if (fld.FldInfo.StyleItalic) { ctrl.Style.Add(HtmlTextWriterStyle.FontStyle, "italic"); }
            if (fld.FldInfo.StyleUnderline) { ctrl.Style.Add(HtmlTextWriterStyle.TextDecoration, "underline"); }
            if (fld.FldInfo.StyleFlat) { ctrl.Style.Add(HtmlTextWriterStyle.BorderStyle, "thin"); }
        }


        /// <summary>
        /// Génére un HtmlGenericControl du type Tag et insére son innerHtml
        /// </summary>
        /// <returns></returns>
        public static HtmlGenericControl GetHtmlGenericControl(String tag, String innerHtml, String cssClass = "")
        {
            HtmlGenericControl ctrl = new HtmlGenericControl(tag);
            ctrl.InnerHtml = innerHtml;
            if (!String.IsNullOrEmpty(cssClass))
                ctrl.Attributes.Add("class", cssClass);
            return ctrl;
        }



        /// <summary>
        /// Retourne le chemin web de l'image vcard de la societe ou du contact
        /// </summary>
        /// <param name="baseName">nom de la base</param>
        /// <param name="didTab">descid de la table</param>
        /// <param name="fileId">file id de la fiche</param>
        /// <returns></returns>
        public static String GetWebVCardPhoto(String baseName, Int32 tab, Int32 fileId)
        {
            return eTools.GetWebVCardPhoto(HttpContext.Current, baseName, tab, fileId);
        }

        /// <summary>
        /// Retourne le chemin web de l'image vcard de la societe ou du contact
        /// </summary>
        /// <param name="context">Context de la request</param>
        /// <param name="baseName">nom de la base</param>
        /// <param name="didTab">descid de la table</param>
        /// <param name="fileId">file id de la fiche</param>
        /// <returns></returns>
        public static string GetWebVCardPhoto(HttpContext context, String baseName, Int32 tab, Int32 fileId)
        {
            string physicDatasPath = eLibTools.GetRootPhysicalDatasPath(context);
            return eLibTools.GetWebVCardPhoto(physicDatasPath, baseName, tab, fileId);
        }

        /// <summary>
        /// Retourne une chaine de caractère comprise entre la string cutBegin et cutEnd.
        /// Si le cutEnd n'est pas trouvé, retour vide.
        /// Si le cutEnd n'est pas trouvé, on récupère la totalité de la chaine à partir du cutBegin.
        /// </summary>
        /// <param name="value">chaine totale à parcourir</param>
        /// <param name="cutBegin">chaine de caractère de début</param>
        /// <param name="cutEnd">chaine de caractère de fin</param>
        /// <returns></returns>
        public static String GetInStr(String value, String cutBegin, String cutEnd)
        {
            if (!value.Contains(cutBegin))
                return String.Empty;

            Int32 idxBegin = value.IndexOf(cutBegin) + cutBegin.Length;
            Int32 idxEnd = value.IndexOf(cutEnd, idxBegin);

            if (idxEnd != -1)
                return value.Substring(idxBegin, idxEnd - idxBegin);
            else
                return value.Substring(idxBegin);
        }

        /// <summary>
        /// Retourne un TimeSpan à partir d'une chaine
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetTimeFromString(String sTime)
        {
            String[] aTime = sTime.Split(':');
            string sHours = String.Empty;
            string sMinutes = String.Empty;
            string sSeconds = String.Empty;
            if (aTime.Length > 0)
                sHours = aTime[0];
            if (aTime.Length > 1)
                sMinutes = aTime[1];
            if (aTime.Length > 2)
                sSeconds = aTime[2];

            return new TimeSpan(eLibTools.GetNum(sHours), eLibTools.GetNum(sMinutes), eLibTools.GetNum(sSeconds));
        }

        /// <summary>
        /// Ecrit le contenu du Buffer dans le fichier défini en paramètre. Ecrase le fichier si déjà existant (eImageDialog et eGoogleImageGet)
        /// </summary>
        /// <param name="strPath">Chemin du fichier à écrire</param>
        /// <param name="Buffer">Source de données (Buffer)</param>
        public static void WriteToFile(string strPath, ref byte[] Buffer)
        {
            if (File.Exists(strPath))
            {
                File.Delete(strPath);
            }
            FileStream newFile = new FileStream(strPath, FileMode.Create);
            newFile.Write(Buffer, 0, Buffer.Length);
            newFile.Close();
            newFile.Dispose();
        }


        /// <summary>
        /// Force le rendu HTML d'un control
        /// </summary>
        /// <param name="ctrl">Controle à décrypter en rendu HTML</param>
        /// <returns></returns>
        public static String GetControlRender(Control ctrl)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter textWriter = new HtmlTextWriter(sw);
            ctrl.RenderControl(textWriter);
            return sb.ToString();
        }


        /// <summary>
        /// Retourne la liste des fichiers (Excel, le reste TODO)
        /// </summary>
        /// <param name="databaseName">Nom SQL de la Base</param>
        /// <param name="extensions">Extensions séparées par des ';'</param>
        /// <returns>Liste des chemin UNC des fichiers</returns>
        public static List<String> GetTemplateFilesList(String databaseName, String extensions)
        {
            List<String> templateList = new List<String>();

            List<String> extensionList = new List<String>(extensions.ToUpper().Split(';'));

            DirectoryInfo templateDir = new DirectoryInfo(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.MODELES, databaseName));
            if (templateDir.Exists)
                foreach (FileInfo templateFile in templateDir.GetFiles())
                    if (extensionList.Contains(templateFile.Extension.ToUpper()))
                        templateList.Add(templateFile.Name);
            return templateList;
        }


        /// <summary>
        ///  Vérification d'upload d'un fichier
        /// </summary>
        /// <param name="fileUploadToCheck">Controle à vérifier</param>
        /// <param name="returnError">Message de retour</param>
        /// <returns>True ou false</returns>
        public static bool CheckFileToUpload(FileUpload fileUploadToCheck, out string returnError)
        {
            return CheckFileToUpload(fileUploadToCheck, 0, null, out returnError);
        }

        /// <summary>
        /// Vérification d'upload d'un fichier, on vérifie la présence d'un fichier, la taille maximum autorisée et les extensions
        /// </summary>
        /// <param name="fileUploadToCheck">Controle à vérifier</param>
        /// <param name="maxLength">Taille maximum en Ko</param>
        /// <param name="lstExttoinclude">tableau de string contenant les extensions à inclure</param>
        /// <param name="messageOut">Massage en cas d'echec</param>
        /// <returns>True ou false</returns>
        public static bool CheckFileToUpload(FileUpload fileUploadToCheck, Int32 maxLength, List<string> lstExttoinclude, out string messageOut)
        {
            messageOut = string.Empty;
            try
            {
                // Vérification de la présence d'un fichier à uploader
                if (!fileUploadToCheck.HasFile)
                {
                    //messageOut = "Aucun fichier à télécharger";
                    messageOut = "";
                    return false;
                }
                HttpPostedFile postedFile = fileUploadToCheck.PostedFile;
                return CheckFileToUpload(postedFile, maxLength, lstExttoinclude, out messageOut);
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Vérification d'upload d'un fichier, on vérifie la présence d'un fichier, la taille maximum autorisée et les extensions
        /// </summary>
        /// <param name="postedFile">Fichier à vérifier</param>
        /// <param name="maxLength">Taille maximum en Ko</param>
        /// <param name="tabExtLstBlanche">tableau de string contenant les extensions à inclure</param>
        /// <param name="messageOut">Massage en cas d'echec</param>
        /// <returns>True ou false</returns>
        public static bool CheckFileToUpload(HttpPostedFile postedFile, Int32 maxLength, List<string> lstExttoinclude, out string messageOut)
        {
            messageOut = string.Empty;
            try
            {
                // Vérification de la présence d'un fichier à uploader
                if (postedFile.ContentLength == 0)
                {
                    //messageOut = "Aucun fichier à télécharger";
                    messageOut = "";
                    return false;
                }
                // Verification de la taille du fichier
                if (maxLength > 0)
                {
                    if (postedFile.ContentLength > maxLength)
                    {
                        messageOut = "Taille du fichier trop volumineuse";
                        return false;
                    }
                }

                // récupération de l'extension du fichier 
                string extFile = Path.GetExtension(postedFile.FileName);

                List<string> lstAllowedExtServer = new List<string>();
                //récupération des Extentions niveau serveur
                string sServeurWideAllowed = string.Empty;

                if (!string.IsNullOrEmpty(eLibTools.GetServerConfig("allowedextensions")))
                {
                    // Extensionss Serveur
                    sServeurWideAllowed = eLibTools.GetServerConfig("allowedextensions");
                    sServeurWideAllowed = CryptoTripleDES.Decrypt(sServeurWideAllowed, CryptographyConst.KEY_CRYPT_LINK1);

                    // FORMAT : .ext
                    sServeurWideAllowed = sServeurWideAllowed.ToLower().Replace("*.", ".");
                    lstAllowedExtServer = sServeurWideAllowed.Split(';').ToList<string>();

                    // Vérification des extensions
                    // Les fichiers doivent répondre au critères serveur et à ceux spécifique
                    // L'echec de l'un ou l'autre abouti au blocage du fichier.
                    if (extFile.Length > 0                                // Les fichiers sans extensions sont autorisés
                     &&
                   (!lstAllowedExtServer.Contains(".*")  // Si on autorise tout côté serveur
                   && !lstAllowedExtServer.Contains(extFile.ToLower()))   // Niveau serveur  : ne contient l'extension du fichier          
                 || (lstExttoinclude != null && lstExttoinclude.Count > 0 && !lstExttoinclude.Contains(extFile.ToLower())
                 && !lstExttoinclude.Contains(".*")))   // Niveau spécifique  : l'extension du fichier n'est pas dans le masque fourni
                    {
                        //On a pas de pref, on tente de récupérer la langue depuis le cookie

                        int idLang = 0;
                        try
                        {
                            idLang = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                        }
                        catch
                        {
                            idLang = 0;

                        }

                        messageOut = eResApp.GetRes(idLang, 1545) + $" ({sServeurWideAllowed})";
                        return false;
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        #region GCH Acces à Nomination - voir v7 si demande d'umplémentation, le code n'est ici pas terminé

        /// <summary>
        /// Appel du WebServices de Nomination pour authentifier l'utilisateur
        /// HLA - Acces à Nomination - Dev #9448
        /// GCH Acces à Nomination - Partenariat rompu, voir v7 si demande d'umplémentation, le code n'est ici pas terminé
        /// </summary>
        /// <param name="strUserMail">Email</param>
        /// <param name="sNominationUserMD5">Clef d'authentification</param>
        /// <param name="err">si une erreur se produit</param>
        /// <returns></returns>
        public static Boolean GetNominationAuthenticate(string strUserMail, string sNominationUserMD5, out string err)
        {
            Boolean retour = true;
            err = string.Empty;
            try
            {


                //                String sPPPKey = eMD5.EncryptMd5(string.Concat(DateTime.Now.ToString("yyyyMMdd"), "ae12f09e6b7b"));
                String sPPPKey = HashMD5.GetHash(string.Concat(DateTime.Now.ToString("yyyyMMdd"), "ae12f09e6b7b"));
                String sUrl = eConst.NOMINATION_HREF_ACCESS
                    .Replace("<USERMAIL>", strUserMail)
                    .Replace("<USERKEY>", sNominationUserMD5)
                    .Replace("<PKEY>", sPPPKey);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(eConst.NOMINATION_HREF_ACCESS);
                request.Method = "QUERY";

                //TODO NOMINATION à vérifier et compléter
                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string content = sr.ReadToEnd();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    retour = false;
                    err = string.Concat("Erreur d'accès à la page : ", response.StatusCode.ToString());
                }
                else
                {
                    switch (content)
                    {
                        case "1":
                            retour = true;
                            break;
                        case "2":
                            retour = false;
                            err = "Utilisateur non trouvé";
                            break;
                        case "3":
                            retour = false;
                            err = "Erreur de calcul authkey";
                            break;
                        case "4":
                            retour = false;
                            err = "Erreur pkey";
                            break;
                        default:
                            retour = false;
                            err = "Erreur non identifiée";
                            break;
                    }
                }


            }
            catch (Exception ex)
            {
                err = ex.ToString();
                retour = false;
            }
            return retour;
        }
        #endregion

        /// <summary>
        /// Retourne la valeur d'une propriété dans le label
        /// </summary>
        /// <param name="strHtmlTag"></param>
        /// <param name="strProp"></param>
        /// <returns></returns>
        public static string GetTagPropValue(string strHtmlTag, string strProp)
        {


            //ajout des ' dans les labels des trackings
            try
            {
                string taragLabel = strHtmlTag.Substring(0, strHtmlTag.IndexOf(">") + 1);
                taragLabel = taragLabel.Replace("'", "");
                taragLabel = taragLabel.Replace("\"", "");
                taragLabel = taragLabel.Replace(eConst.HTM_TAG_BEGIN, "");
                taragLabel = taragLabel.Replace(">", "");

                string[] aParams = taragLabel.Split(' ');

                for (int i = 0; i < aParams.Length; i++)
                {
                    if (aParams[i].Contains("="))
                    {
                        string[] aProp = aParams[i].Split('=');
                        if (aProp[0].ToLower() == strProp)
                        {
                            return aProp[1];
                        }
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }




        /// <summary>
        /// Affectre l'image AVATAR du contact ou de l'orga au controle
        /// </summary>
        /// <param name="vcPhoto">Controle wen de la photo</param>
        /// <param name="pref">pref utilisateur</param>
        /// <param name="tab">table de l'avatar</param>
        /// <param name="isModifyAllowed">Changement d'avatar autorisé</param>
        /// <param name="sFileName">Nom de l'image</param>
        /// <returns></returns>
        public static Boolean SetAvatar(WebControl vcPhoto, ePref pref, Int32 tab, bool isModifyAllowed, String sFileName, int nFileId, Boolean bFromFile = false)
        {
            HtmlImage image = new HtmlImage();
            vcPhoto.Controls.Add(image);

            int nDescid = tab + EudoQuery.AllField.AVATAR.GetHashCode();

            vcPhoto.ID = "vcImg";
            if (tab == EudoQuery.TableType.PM.GetHashCode())
                vcPhoto.Attributes.Add("class", "vcImgPm");
            else
                // MCR/ SPH 40510 remplacer la variable : isModifyAllowed par le parametre optionnel  bFromFile dans le test ; qui est a true si on est en mode fiche. 
                //                positionné a true dans le renderer\eMainFileRenderer.cs dans la methode AddAvatarCellOnTable()
                vcPhoto.Attributes.Add("class", string.Concat("vcImgPp", bFromFile ? "File" : ""));

            vcPhoto.Attributes.Add("tab", tab.ToString());
            vcPhoto.Attributes.Add("fid", nFileId.ToString());
            vcPhoto.Attributes.Add("did", nDescid.ToString());


            //Le changement d'image n'est disponible que depuis la fiche hors VCARD
            if (isModifyAllowed)
            {
                vcPhoto.Attributes.Add("onclick", "doGetImage(this, 'OLD_AVATAR_FIELD');");
                vcPhoto.Attributes.Add("ondragover", "UpFilDragOver(this, event);return false;");
                vcPhoto.Attributes.Add("ondragleave", "UpFilDragLeave(this); return false;");
                vcPhoto.Attributes.Add("ondrop", "UpFilDrop(this,event,null,null,1);return false;");
            }


            if (sFileName.Length == 0)
            {
                if (tab == TableType.PP.GetHashCode())
                    image.Src = "themes/default/images/ui/avatar.png";
                else if (tab == TableType.PM.GetHashCode())
                    image.Src = "themes/default/images/iVCard/unknown_pm.png";

                return false;
            }

            else
            {
                string sFilePath = String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, pref.GetBaseName).TrimEnd('/'), "/", sFileName);
                string sLocalFilePath = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, pref).TrimEnd('\\'), @"\", sFileName);
                // Si l'avatar a été uploadé en 3 versions (dev #48 094), on utilise la version préfixée _thumb.
                // Sinon, on utilise la version originale/antérieure du fichier
                if (File.Exists(sLocalFilePath.Replace(".jpg", String.Concat(eLibConst.THUMB_SUFFIX, ".jpg"))))
                    sFilePath = sFilePath.Replace(".jpg", String.Concat(eLibConst.THUMB_SUFFIX, ".jpg"));

                //vcPhoto.Style.Add("background-image", "url('" + sFilePath.Replace("'", "\\'") + "')");
                image.Src = sFilePath.Replace("'", "\\'");
                return true;
            }


        }


        /// <summary>
        /// Transfert les attributs styles classe et controls internes de wc1 à wc2
        /// </summary>
        /// <param name="wc1"></param>
        /// <param name="wc2"></param>
        public static void TransfertFromTo(WebControl wc1, WebControl wc2)
        {
            while (wc1.Controls.Count > 0)
                wc2.Controls.Add(wc1.Controls[0]);

            wc2.CssClass = wc1.CssClass;

            wc2.ApplyStyle(wc1.ControlStyle);

            wc2.CopyBaseAttributes(wc1);

            wc2.ID = wc1.ID;
        }

        /// <summary>
        /// Transfert les attributs styles classe et controls internes de pn1 à pn2
        /// </summary>
        /// <param name="pn1"></param>
        /// <param name="pn2"></param>
        public static void TransfertFromTo(Panel pn1, Panel pn2)
        {
            TransfertFromTo((WebControl)pn1, (WebControl)pn2);
        }

        /// <summary>
        /// Création du string représentant un eAlert Javascript contenu dans des balises de scripts
        /// Ce message est déstiné à l'utilisateur
        /// Il faut au préalable avoir remplis les differents champs
        /// </summary>
        /// <param name="_errCont">eErrorContainer avec toutes les informations de l'erreur</param>
        /// <param name="widtheAlert">Largeur de la fenetre</param>
        /// <param name="heighteAlert">Hauteur de la fenetre</param>
        /// <param name="fnctReturn">Méthode de retour</param>
        /// <returns>String contenant une eAlert Javascript entourée des balises de scripts</returns>
        public static string GetCompletAlert(eErrorContainer _errCont, int widtheAlert = 0, int heighteAlert = 0, string fnctReturn = "")
        {
            try
            {
                StringBuilder sbReturnHtml = new StringBuilder();
                sbReturnHtml.Append("<html><head>");
                StringBuilder UrlRelative = new StringBuilder();
                UrlRelative.Append("/").Append(System.Web.HttpContext.Current.Request.Url.Segments[1].TrimEnd('/')).Append("/");
                sbReturnHtml.Append("<script src=\"").Append(UrlRelative).Append("scripts/eMain.js?ver=").Append(eConst.VERSION).Append(".").Append(eConst.REVISION).Append("\" type=\"text/javascript\"></script>");
                sbReturnHtml.Append("<script src=\"").Append(UrlRelative).Append("scripts/eModalDialog.js?ver=").Append(eConst.VERSION).Append(".").Append(eConst.REVISION).Append("\" type=\"text/javascript\"></script>");
                sbReturnHtml.Append("<script src=\"").Append(UrlRelative).Append("scripts/eTools.js?ver=").Append(eConst.VERSION).Append(".").Append(eConst.REVISION).Append("\" type=\"text/javascript\"></script>");
                sbReturnHtml.Append("<script src=\"").Append(UrlRelative).Append("scripts/eUpdater.js?ver=").Append(eConst.VERSION).Append(".").Append(eConst.REVISION).Append("\" type=\"text/javascript\"></script>");
                sbReturnHtml.Append("<link rel=\"stylesheet\" media=\"all\" type=\"text/css\" href=\"").Append(UrlRelative).Append("themes/default/css/eModalDialog.css?ver=").Append(eConst.VERSION).Append(".").Append(eConst.REVISION).Append("\" />");
                sbReturnHtml.Append("<link rel=\"stylesheet\" media=\"all\" type=\"text/css\" href=\"").Append(UrlRelative).Append("themes/default/css/eButtons.css?ver=").Append(eConst.VERSION).Append(".").Append(eConst.REVISION).Append("\" />");

                sbReturnHtml.Append("</head><body>");
                sbReturnHtml.Append("<script type=\"text/javascript\" language=\"javascript\">").Append(Environment.NewLine);
                sbReturnHtml.AppendLine("var _res_30 = 'OK';");
                sbReturnHtml.Append(GetJsAlert(_errCont, widtheAlert, heighteAlert, fnctReturn));

                sbReturnHtml.Append("</script>").Append(Environment.NewLine);
                sbReturnHtml.Append("</body></html>");

                return sbReturnHtml.ToString();
            }
            catch (Exception ex)
            {
                return string.Concat("<script type=\"text/javascript\" language=\"javascript\">", Environment.NewLine, " eAlert( 0, \"eAlert\", \"Generation erreur\", \"", ex.Message, "\"); </script>");
            }
        }


        /// <summary>
        /// Création du string représentant un eAlert Javascript
        /// Cette méthode renvoi un code JavaScript sans les balises de scripts
        /// Ce message est déstiné à l'utilisateur
        /// Il faut au préalable avoir remplis les differents champs contenu dans eErrorContainer
        /// </summary>
        /// <param name="_errCont">eErrorContainer avec toutes les informations de l'erreur</param>
        /// <param name="widtheAlert">Largeur de la fenetre</param>
        /// <param name="heighteAlert">Hauteur de la fenetre</param>
        /// <param name="fnctReturn">Méthode de retour</param>
        /// <returns>String contenant une eAlert Javascript </returns>
        public static String GetJsAlert(eErrorContainer _errCont, int widtheAlert = 0, int heighteAlert = 0,
            string fnctReturn = "", string identifiantJsVariables = "")
        {
            if (identifiantJsVariables == null)
                identifiantJsVariables = String.Empty;

            string jsErrorObjName = String.Concat("oErrorObj", identifiantJsVariables);
            string jsOkFctName = String.Concat("okFct", identifiantJsVariables);
            string jsAlertObjName = String.Concat("myErrorAlert", identifiantJsVariables);

            try
            {
                if (_errCont == null)
                    return string.Empty;

                StringBuilder sbeAlert = new StringBuilder();
                string titleFen = string.Empty;
                string messageFen = string.Empty;
                string detailsFen = string.Empty;
                string devFen = string.Empty;

                // Options de eAlert concernant la taile de la modalDialog
                if (widtheAlert < 0)
                    widtheAlert = 0;
                if (heighteAlert < 0)
                    heighteAlert = 0;
                if (fnctReturn == null)
                    fnctReturn = string.Empty;

                // Fonction appelée lors de la validation de la modalDialog
                string validfunction = string.Empty;

                if (_errCont.Title.Length == 0 && _errCont.Msg.Length == 0 && _errCont.Detail.Length == 0)
                    return string.Empty;

                if (_errCont.Title.Length > 0)
                    titleFen = CleanJsAlertParam(_errCont.Title, '\"');

                if (_errCont.Msg.Length > 0)
                    messageFen = CleanJsAlertParam(_errCont.Msg, '\"');

                if (_errCont.Detail.Length > 0)
                    detailsFen = CleanJsAlertParam(_errCont.Detail, '\"');

                if (_errCont.DebugMsg.Length > 0)
                    devFen = CleanJsAlertParam(_errCont.DebugMsg, '\"');
                //eAlert(criticity, title, message, details, width, height, okFct)

                sbeAlert.Append("var ").Append(jsErrorObjName).Append(" = new Object();").AppendLine();

                sbeAlert.Append(jsErrorObjName).Append(".Type = \"").Append(_errCont.TypeCriticity.GetHashCode()).Append("\";");
                sbeAlert.Append(jsErrorObjName).Append(".Title = \"").Append(titleFen).AppendLine("\";");
                sbeAlert.Append(jsErrorObjName).Append(".Msg = \"").Append(messageFen).AppendLine("\";");
                sbeAlert.Append(jsErrorObjName).Append(".DetailMsg =\"").Append(detailsFen).AppendLine("\";");
                sbeAlert.Append(jsErrorObjName).Append(".DetailDev =\"").Append(devFen).AppendLine("\";");

                // remplace le callback intial par un retour à l'accueil

                if (_errCont.IsSessionLost)
                {
                    sbeAlert.Append(jsOkFctName).Append(" = function () {");
                    sbeAlert.Append("top.document.location = \"elogin.aspx\";");
                    sbeAlert.AppendLine("};");
                }
                else
                {

                    sbeAlert.Append(jsOkFctName).Append(" = function () {");
                    if (fnctReturn.Length > 0)
                        sbeAlert.Append(fnctReturn);
                    sbeAlert.AppendLine("};");
                }


                if (heighteAlert == 0 || widtheAlert == 0)
                {

                    sbeAlert.Append("var ").Append(jsAlertObjName)
                        .Append(" = top.eAlertError(").Append(jsErrorObjName).Append(", ").Append(jsOkFctName).Append(");").AppendLine();
                }
                else

                {
                    sbeAlert.Append("var ").Append(jsAlertObjName)
                        .Append(" = top.eAlertError(").Append(jsErrorObjName).Append(", ").Append(jsOkFctName).Append(",").Append(widtheAlert).Append(",").Append(heighteAlert).Append(");").AppendLine();
                }

                return sbeAlert.ToString();
            }
            catch (Exception ex)
            {
                return string.Concat("var ", jsAlertObjName, " =  top.eAlert( 0, \"eAlert\", \"Generation erreur\", \"", ex.Message, "\");", Environment.NewLine);
            }
        }
        /// <summary>
        /// Nettoie une chaine en paramètre pour qu'elle soit utilisée dans une Variable JS en paramètre d'un eAlert
        /// </summary>
        /// <param name="sToClean">chaine à nettoyer</param>
        /// <param name="charSeparator">caractère d'encadrement des chaine</param>
        /// <returns>chaine nettoyée</returns>
        private static String CleanJsAlertParam(String sToClean, Char charSeparator)
        {
            String sBrTag = "$br$";
            return sToClean.Replace(charSeparator.ToString(), String.Concat("\\", charSeparator))
                .Replace(Environment.NewLine, sBrTag)
                .Replace("\n", sBrTag)
                .Replace("\r", sBrTag)
                .Replace(sBrTag, "<br/>");
        }

        /// <summary>
        /// Retourne le rendu html en string d'un WebControl
        /// </summary>
        /// <param name="ectrl">WebControle à transformer</param>
        /// <returns>composant convertit en chaine de caractères</returns>
        public static String GetHtmlRender(Control ectrl)
        {
            String strReturn = string.Empty;
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            ectrl.RenderControl(tw);
            strReturn = sb.ToString();
            return strReturn;
        }



        /// <summary>
        /// Convertit un dictionnaire  en objet javascript,
        /// Si vous passez des objets c#, redefinissez la methode toString()
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="extendedDictionary"></param>
        /// <returns></returns>
        public static String JavaScriptSerialize<TK, TV>(Dictionary<TK, TV> extendedDictionary)
        {
            String varjs = String.Concat("{", String.Join(",", extendedDictionary.Select(kv => kv.Key.ToString() + ":'" + HttpUtility.JavaScriptStringEncode(kv.Value.ToString()) + "'")), "}");

            return varjs;
        }

        /// <summary>
        /// Retourne le numéro de révision de XRM
        /// </summary>
        /// <returns>Numéro de révision de XRM</returns>
        public static String GetVersion()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
            catch { return String.Empty; }
        }

        /// <summary>
        /// Retourne une iFrame avec un traitement sur l'URL s'il s'agit d'une spécif.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="sOrigUrl"></param>
        /// <param name="iTabDescId"></param>
        /// <param name="iDescid"></param>
        /// <param name="iFileId"></param>
        /// <returns></returns>
        public static HtmlGenericControl GetFieldIFrame(ePref pref, String sOrigUrl, Int32 iTabDescId, Int32 iDescid, Int32 iFileId)
        {
            Int32 nSpecifId = 0;
            String sUrl = sOrigUrl;

            if (pref.AdminMode)
            {
                HtmlGenericControl pnl = new HtmlGenericControl("div");
                pnl.Attributes.Add("class", "iframeadmin");

                HtmlGenericControl pnlTxt = new HtmlGenericControl("div");
                pnlTxt.Controls.Add(new LiteralControl("[ URL : " + sOrigUrl + "]"));

                pnl.Controls.Add(pnlTxt);

                return pnl;

            }

            if (Int32.TryParse(sOrigUrl, out nSpecifId))
            {

                string sEncode = ExternalUrlTools.GetCryptEncode(string.Concat("sid=", nSpecifId, "&tab=", iTabDescId, "&fid=", iFileId, "&descid=", iDescid));
                sUrl = String.Concat("eSubmitTokenXRM.aspx?t=", sEncode);
            }
            else
            {
                //RegExp de la spécif
                string sRegSpec = string.Concat("((^https?://([^/]*/)+app)?/?specif/", pref.GetBaseName, "/)(.*)$");
                Regex regExp = new Regex(sRegSpec, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                MatchCollection mc;

                mc = regExp.Matches(sOrigUrl);

                // Url de type Spécif 
                if (mc.Count == 1)
                    sUrl = string.Concat("eExportToV7.aspx?id=", HttpUtility.UrlEncode(sOrigUrl), "&type=", eLibConst.SPECIF_TYPE.TYP_WEBFIELD.GetHashCode(), "&tab=", iTabDescId, "&descid=", iDescid, "&fileid=", iFileId);
            }


            HtmlGenericControl iFrame = new HtmlGenericControl("iframe");
            iFrame.Attributes.Add("src", sUrl);

            return iFrame;
        }



        /// <summary>
        /// Permet de combiner des morceaux de chemins web, chaques paramètres et rajouté dans le chemin retourné séparé par des /
        /// exemple :
        ///     WebPathCombine("a","b","c")
        ///     WebPathCombine("a","b/","c")
        ///     WebPathCombine("a","/b","c")
        ///     WebPathCombine("a","/b/","c")
        ///     => Dans tous les cas cela retourne a/b/c
        /// </summary>
        /// <param name="sUrls"></param>
        /// <returns></returns>
        public static String WebPathCombine(params String[] sUrls)
        {
            String sCombine = String.Empty;
            foreach (String s in sUrls)
            {
                if (sCombine.Length > 0 && sCombine[sCombine.Length - 1] != '/' && (s.Length <= 0 || s[0] != '/'))
                    sCombine = String.Concat(sCombine, '/');
                sCombine = String.Concat(sCombine, s);
            }
            return sCombine;
        }

        /// <summary>
        /// Remplit le select avec les table mail
        /// </summary>
        /// <param name="Pref"></param>
        /// <param name="select"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool FillEmailFiles(ePref Pref, HtmlSelect select, out String err)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            Dictionary<int, string> dicEmailFiles = new Dictionary<int, string>();
            Dictionary<int, string> dicDeletedFiles = new Dictionary<int, string>();
            err = string.Empty;

            try
            {
                dal.OpenDatabase();

                //On recupère tous les fichiers Email                 
                dicEmailFiles = eDataTools.GetEmailFiles(dal, Pref, out err, ref dicDeletedFiles);

                //En cas d'erreur
                if (err.Length > 0)
                    return false;

                //Remplissage du select
                foreach (KeyValuePair<int, string> kv in dicEmailFiles)
                {




                    select.Items.Add(new ListItem(kv.Value, kv.Key.ToString()));

                }

            }
            catch (Exception e)
            {
                err = e.Message;
                return false;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return true;
        }

        /// <summary>
        /// Remplit le select avec les tables de type SMS
        /// </summary>
        /// <param name="Pref"></param>
        /// <param name="select"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool FillSMSFiles(ePref Pref, HtmlSelect select, out String err)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            Dictionary<int, string> dicSMSFiles = new Dictionary<int, string>();
            Dictionary<int, string> dicDeletedFiles = new Dictionary<int, string>();
            err = string.Empty;

            try
            {
                dal.OpenDatabase();

                //On recupère tous les fichiers SMS
                dicSMSFiles = eDataTools.GetSMSFiles(dal, Pref, out err, ref dicDeletedFiles);

                //En cas d'erreur
                if (err.Length > 0)
                    return false;

                //Remplissage du select
                foreach (KeyValuePair<int, string> kv in dicSMSFiles)
                {




                    select.Items.Add(new ListItem(kv.Value, kv.Key.ToString()));

                }

            }
            catch (Exception e)
            {
                err = e.Message;
                return false;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return true;
        }

        public static void SetTableCellColspan(TableCell tc, eFieldRecord fldRec)
        {
            SetTableCellColspan(tc, fldRec.FldInfo.PosColSpan);

        }

        public static void SetTableCellColspan(TableCell tc, Int32 fieldColspan)
        {
            //(eConst.NB_COL_BY_FIELD - 1) corresponds au nombre des cellules système associées : (label, boutons, etc.)
            tc.ColumnSpan = fieldColspan * eConst.NB_COL_BY_FIELD - (eConst.NB_COL_BY_FIELD - 1);

        }


        #region Réseaux Sociaux
        public static string GetSocialNetworkUrl(string url = "", string rootUrl = "")
        {
            string finalUrl = url;

            if (rootUrl != null && rootUrl != "")
            {
                finalUrl = String.Concat(rootUrl, finalUrl);
            }

            return finalUrl;
        }
        #endregion

        #region Meta Réseaux Sociaux
        public enum MetaSocialNetworkType
        {
            TITLE,
            DESCRIPTION,
            IMAGE,
            URL,
            IMAGEWIDTH,
            IMAGEHEIGHT
        }

        public enum SocialNetworkType
        {
            FACEBOOK,
            TWITTER
        }

        /// <summary>
        /// Retourne la liste des métas (open graph et twitter) 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<HtmlMeta> GetHtmlMetaSocialNetwork(MetaSocialNetworkType type, string value)
        {
            List<HtmlMeta> metaList = new List<HtmlMeta>();
            //on ajoute les métas open graph
            HtmlMeta meta = new HtmlMeta();
            meta.Attributes.Add("name", GetMetaSocialNetworkProperty(type));
            meta.Attributes.Add("property", String.Concat("og:", GetMetaSocialNetworkProperty(type)));
            meta.Attributes.Add("content", value.Replace("\"", "&quot;"));
            metaList.Add(meta);

            //on ajoute les métas twitter
            meta = new HtmlMeta();
            meta.Attributes.Add("name", String.Concat("twitter:", GetMetaSocialNetworkProperty(type, SocialNetworkType.TWITTER)));
            meta.Attributes.Add("content", value.Replace("\"", "&quot;"));
            metaList.Add(meta);

            return metaList;
        }

        private static string GetMetaSocialNetworkProperty(MetaSocialNetworkType type, SocialNetworkType network = SocialNetworkType.FACEBOOK)
        {
            switch (type)
            {
                case MetaSocialNetworkType.TITLE:
                    return "title";
                case MetaSocialNetworkType.DESCRIPTION:
                    return "description";
                case MetaSocialNetworkType.IMAGE:
                    if (network == SocialNetworkType.TWITTER)
                        return "image:src";
                    else
                        return "image";
                case MetaSocialNetworkType.URL:
                    return "url";
                case MetaSocialNetworkType.IMAGEHEIGHT:
                    return "image:width";
                case MetaSocialNetworkType.IMAGEWIDTH:
                    return "image:height";
                default:
                    return "";
            }
        }

        public enum MetaTwitterCardType
        {
            SUMMARY_CARD,
            SUMMARY_CARD_W_LARGE_IMG,
            APP_CARD,
            PLAYER_CARD
        }


        public static HtmlMeta GetHtmlMetaTwitterSummaryCard(MetaTwitterCardType type = MetaTwitterCardType.SUMMARY_CARD)
        {
            HtmlMeta meta = new HtmlMeta();
            meta.Attributes.Add("name", "twitter:card");
            meta.Attributes.Add("content", GetMetaTwitterSummaryCardContent(type));

            return meta;
        }

        private static string GetMetaTwitterSummaryCardContent(MetaTwitterCardType type)
        {
            switch (type)
            {
                case MetaTwitterCardType.SUMMARY_CARD:
                    return "summary";
                case MetaTwitterCardType.SUMMARY_CARD_W_LARGE_IMG:
                    return "summary_large_image";
                case MetaTwitterCardType.APP_CARD:
                    return "app";
                case MetaTwitterCardType.PLAYER_CARD:
                    return "player";
                default:
                    return "";
            }
        }

        #endregion

        enum EudologTabName
        {
            CLIENTS,
            DATABASES,
            HASH_TO_DB
        }

        /// <summary>
        /// Permet de récupérer une liste de valeur dans la table CLIENTS de EUDOLOG
        /// </summary>
        /// <param name="pref">Préférence Simplifiée</param>
        /// <param name="listParameter">Nom du paramètre</param>
        /// <returns>Dictionnaire de paramètre/Valeurs demandés</returns>
        public static IDictionary<eLibConst.EUDOLOG_CLIENTS, string> GetEudologClientsValues(ePref pref, IEnumerable<eLibConst.EUDOLOG_CLIENTS> listParameter)
        {
            return GetEudologValues(EudologTabName.CLIENTS, pref, listParameter);
        }

        /// <summary>
        /// Permet de récupérer une liste de valeur dans la table DATABASES de EUDOLOG
        /// </summary>
        /// <param name="pref">Préférence Simplifiée</param>
        /// <param name="listParameter">Nom du paramètre</param>
        /// <returns>Dictionnaire de paramètre/Valeurs demandés</returns>
        public static IDictionary<eLibConst.EUDOLOG_DATABASES, string> GetEudologDatabasesValues(ePref pref, IEnumerable<eLibConst.EUDOLOG_DATABASES> listParameter)
        {
            return GetEudologValues(EudologTabName.DATABASES, pref, listParameter);
        }

        /// <summary>
        /// Permet de récupérer une liste de valeur dans la table DATABASES de EUDOLOG
        /// </summary>
        /// <param name="pref">Préférence Simplifiée</param>
        /// <param name="listParameter">Nom du paramètre</param>
        /// <returns>Dictionnaire de paramètre/Valeurs demandés</returns>
        public static IDictionary<eLibConst.EUDOLOG_HASH_TO_DB, string> GetEudologHashToDbValues(ePref pref, IEnumerable<eLibConst.EUDOLOG_HASH_TO_DB> listParameter)
        {
            return GetEudologValues(EudologTabName.HASH_TO_DB, pref, listParameter);
        }

        /// <summary>
        /// Permet de mettre à jour une liste de valeur dans la table DATABASES de EUDOLOG
        /// </summary>
        /// <param name="pref">Préférence</param>
        /// <param name="hash">hash de la bdd</param>
        public static void InsertHashDatabase(ePref pref, string hash)
        {
            string Error = String.Empty;
            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                RqParam rq = new RqParam(" INSERT INTO EUDOLOG..HASH_TO_DB VALUES (@HashDatabase, @BaseUID)");
                rq.AddInputParameter("HashDatabase", System.Data.SqlDbType.VarChar, hash);
                rq.AddInputParameter("BaseUID", System.Data.SqlDbType.VarChar, pref.DatabaseUid);

                dal.OpenDatabase();
                dal.ExecuteNonQuery(rq, out Error);

                if (!String.IsNullOrEmpty(Error))
                    throw new EudoSqlException("L'insertion du Hash de base de données dans la table HASH_TO_DB à échouché." + Environment.NewLine + Error);
            }
            catch (XRMUpgradeException)
            {
                throw;
            }
            finally
            {
                dal.CloseDatabase();
            }
        }




        /// <summary>
        /// Permet de récupérer une liste de valeur dans la table CLIENTS de EUDOLOG
        /// </summary>
        /// <param name="pref">Préférence Simplifiée</param>
        /// <param name="listParameter">Nom du paramètre</param>
        /// <returns>Dictionnaire de paramètre/Valeurs demandés</returns>
        private static IDictionary<T, string> GetEudologValues<T>(EudologTabName edlTabName, ePref pref, IEnumerable<T> listParameter)
                where T : struct
        {
            string error = string.Empty;

            IDictionary<T, string> dicParamValue = new Dictionary<T, string>();

            // Si il n'y a pas de valeur
            if (listParameter == null)
                return dicParamValue;

            // Selection
            StringBuilder strSqlIn = new StringBuilder();
            foreach (T currentParameter in listParameter)
            {
                if (strSqlIn.Length > 0)
                    strSqlIn.Append(", ");
                strSqlIn.Append("[").Append(currentParameter.ToString()).Append("]");
            }

            // Pas de colonnes !
            if (strSqlIn.Length == 0)
                return dicParamValue;

            string strTargetTable = edlTabName.ToString();

            string strFrom = String.Empty;
            string strWhere = String.Empty;
            switch (edlTabName)
            {
                case EudologTabName.CLIENTS:
                    // From
                    strFrom = String.Concat("from [EUDOLOG]..[", strTargetTable, "] ",
                        " LEFT JOIN [EUDOLOG]..[SUBSCRIBERS] on [EUDOLOG]..[CLIENTS].[ClientId] = [EUDOLOG]..[SUBSCRIBERS].[ClientId] ",
                        " LEFT JOIN [EUDOLOG]..[RELATIONS] on [EUDOLOG]..[SUBSCRIBERS].[LoginId] = [EUDOLOG]..[RELATIONS].[LoginId] ",
                        " LEFT JOIN [EUDOLOG]..[DATABASES] ON [EUDOLOG]..[RELATIONS].[BaseId] = [EUDOLOG]..[DATABASES].[BaseId] ");

                    // Conditions
                    strWhere = " WHERE [EUDOLOG]..[DATABASES].[UID] = @DatabaseUID and [EUDOLOG]..[SUBSCRIBERS].[LOGINID] = @LoginID";

                    break;
                case EudologTabName.DATABASES:
                    // From
                    strFrom = String.Concat("from [EUDOLOG]..[", strTargetTable, "] ");

                    // Conditions
                    strWhere = " WHERE [EUDOLOG]..[DATABASES].[UID] = @DatabaseUID";

                    break;
                case EudologTabName.HASH_TO_DB:
                    // From
                    strFrom = String.Concat("from [EUDOLOG]..[", strTargetTable, "] ");

                    // Conditions
                    strWhere = " WHERE [EUDOLOG]..[HASH_TO_DB].[BaseUID] = @DatabaseUID";

                    break;
                default:
                    return null;
            }

            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            DataTableReaderTuned dtr = null;
            string strSql = String.Concat(" SELECT ", strSqlIn, strFrom, strWhere);

            RqParam rqParam = new RqParam(strSql);
            rqParam.AddInputParameter("DatabaseUID", SqlDbType.VarChar, pref.DatabaseUid);
            rqParam.AddInputParameter("LoginID", SqlDbType.Int, pref.LoginId);

            try
            {
                dal.OpenDatabase();

                dtr = dal.Execute(rqParam, out error);

                if (!String.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("GetEudologValues : ", Environment.NewLine, error));

                if (dtr != null)
                {
                    String val = String.Empty;
                    while (dtr.Read())
                    {
                        foreach (T currentParameter in listParameter)
                            dicParamValue.Add(currentParameter, dtr.GetString(currentParameter.ToString()));
                    }

                    if (dicParamValue.Count <= 0)
                    {
                        foreach (T currentParameter in listParameter)
                            dicParamValue.Add(currentParameter, String.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Concat("GetEudologValues : Exception interne - ", ex.Message), ex);
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();

                if (dal != null)
                    dal.CloseDatabase();
            }

            return dicParamValue;
        }

        /// <summary>
        /// Retourne la liste des font size disponible
        /// </summary>
        /// <returns>Liste des font size disponible a la selection</returns>
        internal static Array GetFontSize()
        {
            return Enum.GetValues(typeof(EudoQuery.FontSize)).Cast<int>().ToArray();
        }

        /// <summary>
        /// A partir d'une taille de police donnée, renvoie le libellé approprié à afficher dans "Mon Eudonet"
        /// </summary>
        /// <param name="pref">Objet Pref pour l'accès en base de données</param>
        /// <param name="fontSize">Taille de la police pour laquelle renvoyer un libellé</param>
        /// <returns>Un libellé si la taille de police est connue, sinon, la taille de police elle-même</returns>
        internal static string GetFontSizeLabel(ePrefBase pref, int fontSize)
        {
            int res = 0;
            switch (fontSize)
            {
                case (int)EudoQuery.FontSize.FS_8_PT: res = 2906; break; // Très petite
                case (int)EudoQuery.FontSize.FS_10_PT: res = 1979; break; // Petite
                case (int)EudoQuery.FontSize.FS_12_PT: res = 1980; break; // Moyenne
                case (int)EudoQuery.FontSize.FS_14_PT: res = 1981; break; // Grande
                case (int)EudoQuery.FontSize.FS_16_PT: res = 2907; break; // Très grande
                case (int)EudoQuery.FontSize.FS_18_PT: res = 2908; break; // Très très grande
            }

            if (res > 0)
                return eResApp.GetRes(pref, res);
            else
                return fontSize.ToString();
        }

        /// <summary>
        /// Retourne le nom de la classe css représenant la fontsize
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        internal static string GetClassNameFontSize(ePref pref)
        {

            if (String.IsNullOrEmpty(pref.FontSize))
            {
                return "fs_8pt";
            }
            else
            {
                return string.Concat("fs_", pref.FontSize, "pt");
            }
        }

        /// <summary>
        /// Retourne la police par defaut de l'utilisateur
        /// </summary>
        /// <param name="pref">Preference utilisateur</param>
        /// <returns>Taille de la police</returns>
        internal static String GetUserFontSize(ePref pref)
        {
            if (String.IsNullOrEmpty(pref.FontSize))
            {
                return "8";
            }
            else
            {
                return pref.FontSize;
            }
        }

        /// <summary>
        /// Vérifie que la classe passée en paramètre commence par "icon-"        
        /// Le rajoute le cas échéant 
        /// </summary>
        /// <param name="sClass"></param>
        internal static string GetEudoFontClass(string sClass)
        {
            const string eudoFontString = "icon-";
            if (!sClass.StartsWith(eudoFontString))
                sClass = String.Concat(eudoFontString, sClass);

            return sClass;
        }

        /// <summary>
        /// Peut-on activer l'autosuggestion BingMaps ?
        /// </summary>
        public static Boolean CanRunBingAutoSuggest(ePref pref, HttpRequest request)
        {
            string provider = eLibTools.GetConfigAdvValues(pref, new List<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF })[eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF];
            eConst.PredictiveAddressesRef eProvider = (eConst.PredictiveAddressesRef)eLibTools.GetNum(provider);
            if (eTools.BrowserSupportedByBing(request)
                && eProvider == eConst.PredictiveAddressesRef.BingMapsV8
                && eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "0")
            {
                return eExtension.IsReady(pref, ExtensionCode.CARTOGRAPHY);
            }

            return false;
        }

        /// <summary>
        /// Affichage de la description du filtre au survol de l'élément
        /// </summary>
        /// <param name="control">Elément HTML</param>
        /// <param name="description">Description du filtre</param>
        public static void DisplayFilterTooltip(WebControl control, string description)
        {
            control.Attributes.Add("onmouseover", String.Concat("shFilterDescription(event, '", HttpUtility.HtmlEncode(description), "');"));
            control.Attributes.Add("onmouseout", "ht();");
        }



        /// <summary>
        /// Charge les informations de la page d'accueil
        /// </summary>
        /// <returns></returns>
        public static eLoginPageInfos LoadLoginPageInfos()
        {


            eLoginPageInfos log = new eLoginPageInfos();

            #region default values
            //Bouton par défaut
            log.btn.Add(new UrlInfos()
            {
                Lang = "LANG_00",
                Text = "Découvrir les nouveautés",
                Url = "https://fr.eudonet.com/essai-gratuit/"
            });

            log.btn.Add(new UrlInfos()
            {
                Lang = "LANG_01",
                Text = "Find out the latest news",
                Url = "https://ca.eudonet.com/en/free-trial/"
            });

            //Ilage par défaut
            log.img.Add(new UrlInfos()
            {
                Lang = "LANG_00",
                Text = "<p>Et si vous passiez à</p><p><span>Eudonet E17</span> ?",
                Url = "themes/default/images/login-background.jpg"
            });

            log.img.Add(new UrlInfos()
            {
                Lang = "LANG_01",
                Text = " <p>Find out more about the new version</p><p><span>Eudonet E17</span> ? ",
                Url = "themes/default/images/login-background.jpg"
            });
            #endregion


            string sL = eLibTools.ReadIISConfigJsonFile("loginpage", "");

            if (!string.IsNullOrEmpty(sL))
            {
                try
                {
                    return JsonConvert.DeserializeObject<eLoginPageInfos>(sL);



                }
                catch (Exception ee)
                {
                    return log;
                }
            }



            return log;
        }

        /// <summary>
        /// Retourne les informations de la newsletter
        /// </summary>
        /// <returns></returns>
        public static eNewsLetters LoadNewsLetterInfos()
        {
            eNewsLetters newinfos = new eNewsLetters();
            newinfos.usermsg.num = (int)eConst.NEWSLETTER_USR;
            newinfos.usermsg.url.Add(new UrlInfos()
            {
                Url = eConst.NEWSLETTER_USR_URL,
                Lang = "LANG_00",
                Text = ""
            });

            newinfos.adminmsg.num = (int)eConst.NEWSLETTER_USR;
            newinfos.adminmsg.url.Add(new UrlInfos()
            {
                Url = eConst.NEWSLETTER_USR_URL,
                Lang = "LANG_00",
                Text = ""
            });



            var mySelection = newinfos;

            string sNLInfos = eLibTools.ReadIISConfigJsonFile("newsletter", "");

            if (!string.IsNullOrEmpty(sNLInfos))
            {
                try
                {
                    mySelection = JsonConvert.DeserializeObject<eNewsLetters>(sNLInfos);
                }
                catch (Exception ee)
                {
                    string sa = ee.Message;
                    mySelection = newinfos;
                }
            }

            return mySelection;
        }

        /// <summary>
        /// Affichage de la description du filtre au survol de l'élément
        /// </summary>
        /// <param name="control">Elément HTML</param>
        /// <param name="description">Description du filtre</param>
        public static void DisplayFilterTooltip(HtmlGenericControl control, string description)
        {
            control.Attributes.Add("onmouseover", String.Concat("shFilterDescription(event, '", HttpUtility.HtmlEncode(description), "');"));
            control.Attributes.Add("onmouseout", "ht();");
        }


        /// <summary>
        /// Clé pour le dictionnaire de la méthode GetUnsubscribeCommonCatalogValues
        ///
        /// </summary>
        public enum InteractionCommonCatalogValuesKeys
        {
            /// <summary>Type d'interaction => Consentement</summary>
            CONSENT,
            /// <summary>Statut du consentement => Opt-in</summary>
            OPTIN,
            /// <summary>Statut du consentement => Opt-out</summary>
            OPTOUT,
            /// <summary>Condition d'obtention => Formulaire web</summary>
            WEBFORM
        }

        /// <summary>
        /// Récupère les valeurs du catalogue qui nous interessent pour la désinscription (Table Interaction, type=>consentement, status=>OptIn et status=>OptOut
        /// </summary>
        /// <returns>Dictionnaire contenant les trois clés "consent", "optin" et "optout"</returns>
        public static Dictionary<InteractionCommonCatalogValuesKeys, int> GetUnsubscribeCommonCatalogValues(ePref Pref)
        {
            List<eCatalog.CatalogValue> catalogInteractionTypes = new List<eCatalog.CatalogValue>();
            List<eCatalog.CatalogValue> catalogInteractionStatusConsent = new List<eCatalog.CatalogValue>();
            List<eCatalog.CatalogValue> catalogInteractionConsentObtainedBy = new List<eCatalog.CatalogValue>();

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {
                dal.OpenDatabase();

                catalogInteractionTypes = new eCatalog(dal, Pref, PopupType.DATA, Pref.User, (int)InteractionField.Type).Values;
                catalogInteractionStatusConsent = new eCatalog(dal, Pref, PopupType.DATA, Pref.User, (int)InteractionField.StatusConsent).Values;
                catalogInteractionConsentObtainedBy = new eCatalog(dal, Pref, PopupType.DATA, Pref.User, (int)InteractionField.ConsentObtainedBy).Values;
            }
            finally
            {
                dal.CloseDatabase();
            }

            Dictionary<InteractionCommonCatalogValuesKeys, int> dico = new Dictionary<InteractionCommonCatalogValuesKeys, int>();

            eCatalog.CatalogValue catalogValueConsent = catalogInteractionTypes.Find(c => c.Data == "consent");
            dico.Add(InteractionCommonCatalogValuesKeys.CONSENT, catalogValueConsent != null ? catalogValueConsent.Id : 0);

            eCatalog.CatalogValue catalogValueOptin = catalogInteractionStatusConsent.Find(c => c.Data == "optin");
            eCatalog.CatalogValue catalogValueOptOut = catalogInteractionStatusConsent.Find(c => c.Data == "optout");

            dico.Add(InteractionCommonCatalogValuesKeys.OPTIN, catalogValueOptin != null ? catalogValueOptin.Id : 0);
            dico.Add(InteractionCommonCatalogValuesKeys.OPTOUT, catalogValueOptOut != null ? catalogValueOptOut.Id : 0);

            eCatalog.CatalogValue catalogValueWebform = catalogInteractionConsentObtainedBy.Find(c => c.Data == "webform");
            dico.Add(InteractionCommonCatalogValuesKeys.WEBFORM, catalogValueWebform != null ? catalogValueWebform.Id : 0);

            if (dico[InteractionCommonCatalogValuesKeys.CONSENT] == 0
                || dico[InteractionCommonCatalogValuesKeys.OPTIN] == 0
                || dico[InteractionCommonCatalogValuesKeys.OPTOUT] == 0
                || dico[InteractionCommonCatalogValuesKeys.WEBFORM] == 0)
                throw new Exception("GetUnsubscribeCommonCatalogValues error: erreur lors de la récupération des valeurs de catalogues");

            return dico;
        }


        /// <summary>
        /// Retourne la version Office configurée
        /// </summary>
        public static ExcelVersion GetOfficeVersion(ePref pref)
        {

            switch (pref.GetConfig(eLibConst.PREF_CONFIG.OFFICERELEASE))
            {
                case "10":
                    return ExcelVersion.Excel2007;
                case "12":
                    return ExcelVersion.Excel2010;
                case "14":
                case "15":
                    return ExcelVersion.Excel2013;
                case "16":
                case "17":
                    return ExcelVersion.Excel2016;
                case "8":
                case "9":
                    return ExcelVersion.Excel97to2003;
                default:
                    return ExcelVersion.Excel2007;
            }


        }

        /// <summary>
        /// Ajoute la nouvelle valeur dans le catalogue et retourne l'ID de la nouvelle valeur
        /// </summary>
        /// <param name="eDal">Objet eudoDAL pour l'accès à la base de données</param>
        /// <param name="pref">Objet ePref donnant le contexte</param>
        /// <param name="updFld">Le champ à mettre à jour</param>
        /// <param name="updFldValueIsLabel">Indique si la valeur indiquée dans l'UpdateField passé en paramètre représente, ou non, le libellé de la valeur à insérer dans le catalogue (cas de l'autocomplétion ou d'EudoSync, par ex.)</param>
        /// <param name="returnEmptyOnDbError">Si le traitement de MAJ en base échoue, renvoyer String.Empty plutôt que l'ID de la nouvelle valeur (si connu). Attention, si l'ajout est refusé pour des raisons fonctionnelles, le retour de la fonction pourra être String.Empty même si ce paramètre est à false</param>
        /// <param name="error">Si le traitement échoue, l'erreur sera renvoyée dans cette variable</param>
        public static string AddNewValueInCatalog(eudoDAL eDal, ePref pref, eUpdateField updFld, bool updFldValueIsLabel, bool returnEmptyOnDbError, out string error)
        {
            return eLibTools.AddNewValueInCatalog(eDal, pref, updFld, updFld.NewValue, updFld.NewDisplay, updFld.BoundValue, updFld.BoundPopup, updFldValueIsLabel, returnEmptyOnDbError, out error);
        }
        /// <summary>
        /// Obtient le site web eudonet de référence en fonction du pays de reference de la base et de la langue utilisateur
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static string GetEudonetWebSite(ePref pref)
        {
            eISOCountry country = eISOCountry.GetCurrentCountry(pref);
            string sDefaultURL = "https://fr.eudonet.com/";

            if (country == null)
                return sDefaultURL;

            switch (country.Alpha3)
            {
                case "FRA":
                    return "https://fr.eudonet.com/";
                case "GBR":
                    return "https://uk.eudonet.com/";
                case "CAN":
                    if (pref.LangId == 0) // si la langue de l'utilisateur est le français
                        return "https://ca.eudonet.com/fr/";
                    else
                        return "https://ca.eudonet.com/en/";
                default:
                    return sDefaultURL;

            }



        }

        /// <summary>
        /// cette fonction permet de vérifier le résultat du test captcha Google
        /// </summary>
        /// <param name="gRecaptchaResponse"></param>
        /// <returns></returns>
        public static bool IsReCaptchValid(string gRecaptchaResponse)
        {
            var result = false;
            var secretKey = "";
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("CaptchaServerKey")))
                secretKey = ConfigurationManager.AppSettings.Get("CaptchaServerKey");
            else
                return false;

            var apiUrl = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";
            var requestUri = string.Format(apiUrl, secretKey, gRecaptchaResponse);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    JObject jResponse = JObject.Parse(stream.ReadToEnd());
                    var isSuccess = jResponse.Value<bool>("success");
                    result = (isSuccess) ? true : false;
                }
            }
            return result;
        }


    }



    #region Classes utiles (ListItemComparer, ...)

    /// <className>ListItemComparer</className>
    /// <summary>Classe de comparaison entre éléments de type ListItem.
    /// Elle permet de trier les objets ListItem d'une liste en fonction de son text ou du retour de la func par ordre croissant ou décroissant défini par l'accesseurs SortOrder
    /// </summary>
    /// <example>
    /// Exemple avec une liste nommé "list" de ListItem avec un ordre décroissant :
    /// <code>
    /// ListItemComparer myComparer = new ListItemComparer();
    /// myComparer.SortOrder = 1;
    /// list.Sort(myComparer);
    /// </code>
    /// 
    /// Exemple avec une liste nommé "list" de ListItem avec un ordre croissant en spécifiant ca propriété testé :
    /// <code>
    /// ListItemComparer myComparer = new ListItemComparer(delegate(ListItem item) { return item.Value; });
    /// list.Sort(myComparer);
    /// </code>
    /// </example>
    /// <authors>HLA</authors>
    /// <date>2011-12-22</date>
    public class ListItemComparer : IComparer<ListItem>
    {
        // Par defaut trie croissant : 0
        private Int32 _sortOrder = 0;

        private Func<ListItem, String> _comparedValueGetter = null;

        /// <summary>
        /// Constructeur par defaut avec la propriété .Text de l'item comme valeur de comparaison
        /// </summary>
        public ListItemComparer()
            : this(delegate (ListItem item) { return item.Text; })
        {
        }

        /// <summary>
        /// Constructeur avec possibilité de spécifié quelle propriété sera utilisé pour le teste de comparaison
        /// </summary>
        /// <param name="comparedValueGetter"></param>
        public ListItemComparer(Func<ListItem, String> comparedValueGetter)
        {
            _comparedValueGetter = comparedValueGetter;
        }

        /// <summary>
        /// Ordre du trie. 1 : pour trie décroissant
        /// </summary>
        public Int32 SortOrder
        {
            set { _sortOrder = value; }
        }

        /// <summary>
        /// Méthode surchargé de comparaison d'élément ListItem
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Int32 Compare(ListItem x, ListItem y)
        {
            Int32 retval;

            if (x == null)
            {
                // If x is null and y is null, they're equal.
                if (y == null)
                    retval = 0;
                // If x is null and y is not null, y is greater.
                else
                    retval = -1;
            }
            // If y is null and x is not null, x is greater.
            else if (y == null)
                retval = 1;
            else
                retval = _comparedValueGetter(x).CompareTo(_comparedValueGetter(y));

            if (_sortOrder.Equals(1))
                retval = retval * -1;

            return retval;
        }
    }

    /// <summary>
    /// Classe représentant une eAlert Javascript présente dans eTools.js
    /// (Methode personnalisée de XRM destinée à remplacer la méthode native alert() en utilisant eModalDialog)
    /// <autheur>NBA</autheur>
    /// <date>Création 06/12/2012</date>
    /// </summary>
    public class eAlert : eMsgContainer
    {
        #region Variable membre

        /// <summary>
        /// dimensions de la popup
        /// </summary>
        internal Int32 widthFen { get; set; }

        /// <summary>
        /// dimensions de la popup
        /// </summary>
        internal Int32 heightFen { get; set; }

        #endregion

        #region Constructeurs

        /// <summary>
        /// Constructeur vide, pensez à renseigner les champs avec les setteurs
        /// </summary>
        public eAlert(eErrorContainer _errCont)
            : base(_errCont.Msg, _errCont.Title, _errCont.Detail, _errCont.TypeCriticity)
        {

        }

        #endregion

        /// <summary>
        /// Création du string représentant un eAlert Javascript
        /// Il faut au préalable avoir remplis les differents champs
        /// </summary>
        /// <param name="balisesJS">ajout la balise d'ouverture et de fermeture de javascript</param>
        /// <returns></returns>
        public String GetJs(Boolean balisesJS = false)
        {
            StringBuilder sbeAlert = new StringBuilder();

            try
            {
                //eAlert(criticity, title, message, details, width, height, okFct)
                sbeAlert.Append("eAlert(")
                    .Append("'").Append(TypeCriticity.GetHashCode()).Append("', ")
                    .Append("'").Append(Title.Replace("'", @"\'")).Append("', ")
                    .Append("'").Append(Msg.Replace("'", @"\'")).Append("', ")
                    .Append("'").Append(Detail.Replace("'", @"\'").Replace(Environment.NewLine, @"\n")).Append("', ")
                    .Append(widthFen == 0 ? "null" : widthFen.ToString()).Append(", ")
                    .Append(heightFen == 0 ? "null" : heightFen.ToString()).Append(");")
                    .AppendLine();
            }
            catch (Exception ex)
            {
                sbeAlert.Length = 0;
                sbeAlert.Append("eAlert(0, 'eAlert', '").Append(Msg.Replace("'", @"\'")).Append("', '").Append(ex.Message.Replace("'", @"\'")).Append("');");
            }

            return GetJsWithBalise(sbeAlert, balisesJS);
        }

        private String GetJsWithBalise(StringBuilder content, Boolean balisesJS)
        {
            StringBuilder sbGlobal = new StringBuilder();

            if (balisesJS)
                sbGlobal.Append("<script type=\"text/javascript\" language=\"javascript\">").AppendLine();

            sbGlobal.Append(content);

            if (balisesJS)
                sbGlobal.Append("</script>").AppendLine();

            return sbGlobal.ToString();
        }
    }


    #endregion

}