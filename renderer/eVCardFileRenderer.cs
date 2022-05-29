using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Génération d'une vcard
    /// </summary>
    public class eVCardFileRenderer : eRenderer
    {


        #region PROPRIETES
        /// <summary>Id de la fiche vcard courante (ppid)</summary>
        private Int32 _nFileId = 0;
        /// <summary>Page de VCARD affichée (autant de page que d'adresse)</summary>
        private Int32 _nPage = 0;
        /// <summary>Table courante</summary>
        private Int32 _nTab = 0;
        /// <summary></summary>
        protected eVCardFile _myFile;
        /// <summary>
        /// Indique si on est en mode mouseover ou au click
        /// Ceci est necessaire pour determiner si on affiche ou non la croix
        /// True pour ne pas afficher la croix et false pour l'afficher
        /// </summary>
        private Boolean _bCroix;




        #endregion

        #region ACCESSEURS

        /// <summary>
        /// Indique si on est en mode mouseover ou au click
        /// Ceci est necessaire pour determiner si on affiche ou non la croix
        ///  True pour ne pas afficher la croix et false pour l'afficher
        /// </summary>
        public Boolean BCroix
        {
            get { return _bCroix; }
            set { _bCroix = value; }
        }
        /// <summary>Id de la fiche vcard courante (ppid)</summary>
        protected Int32 FileId
        {
            get { return _nFileId; }
            set { _nFileId = value; }
        }
        /// <summary>Page de VCARD affichée (autant de page que d'adresse)</summary>
        protected Int32 Page
        {
            get { return _nPage; }
            set { _nPage = value; }
        }
        /// <summary>Table courante</summary>
        public Int32 Tab
        {
            get { return _nTab; }
        }
        #endregion

        /// <summary>
        /// Constructeur initilise les propriété de base de VCARD
        /// </summary>
        /// <param name="pref">Preference de l'user en cours</param>
        /// <param name="nFileId">Id de la fiche vcard courante (ppid)</param>
        /// <param name="nPage">Page demandée</param>
        public eVCardFileRenderer(ePref pref, Int32 nFileId, Int32 nPage)
        {
            _nFileId = nFileId;
            _nPage = nPage;
            _nTab = 200;
            Pref = pref;
            _rType = RENDERERTYPE.VCardFile;
        }

        #region METHODES PRINCIPALES DE RENDERER
        /// <summary>
        /// Page permettant d'initialiser le contenu de retour VCARD HTML
        /// </summary>
        /// <returns>
        /// Vrai si tout est OK
        /// Faux si une erreur se produit, l'erreure étant renseignée dans _sErrorMsg
        /// </returns>
        protected override bool Init()
        {
            try
            {
                _myFile = eVCardFile.CreateVCardFiles(Pref, _nFileId, _nPage);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eVCardFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
                    if (_myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eVCardFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }

        private HtmlGenericControl GetVCardLiEmpty(String className)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", className);
            return li;
        }

        private HtmlGenericControl GetVCardLi(eRecord row, String id, String className, eFieldRecord ef)
        {
            return GetVCardLi(row, id, className, new LiElemInfos[] { new LiElemInfos(ef) });
        }

        private HtmlGenericControl GetVCardLi(eRecord row, String id, String className, ICollection<LiElemInfos> colef)
        {
            Boolean existValue = false;

            HtmlGenericControl li = new HtmlGenericControl("li");
            li.ID = id;
            li.Attributes.Add("class", className);

            if (id == "vcName")
            {
                // Ajout de l'icône
                HtmlGenericControl spanIcon = new HtmlGenericControl("span");
                spanIcon.Attributes.Add("class", "icon-avatar");
                li.Controls.Add(spanIcon);
            }

            foreach (LiElemInfos elem in colef)
            {
                if (elem == null || elem.FldRecord == null)
                    continue;

                eFieldRecord ef = elem.FldRecord;

                HtmlGenericControl a = new HtmlGenericControl("a");

                HtmlGenericControl span = new HtmlGenericControl("span");
                span.Attributes.Add("did", ef.FldInfo.Descid.ToString());
                span.ID = eTools.GetFieldValueCellId(row, ef);
                span.InnerText = ef.DisplayValue;

                // Pour les adresses mail, on ajoute un mailto
                if (id == "vcMail")
                {
                    a.Attributes.Add("href", "mailto:" + ef.DisplayValue);
                    a.Controls.Add(span);
                }

                // Cas spécifique pour 201
                if (ef.FldInfo.Descid == 201 && ef.NameOnly)
                    span.Attributes.Add("nameonly", "");

                if (span.InnerText.Length == 0)
                    continue;

                existValue = true;
                if (id == "vcMail")
                    li.Controls.Add(a);
                else
                    li.Controls.Add(span);

                if (elem.AddSpace)
                    li.Controls.Add(new LiteralControl("&nbsp;"));
            }

            return existValue ? li : null;
        }

        class LiElemInfos
        {
            internal eFieldRecord FldRecord { get; private set; }
            internal bool AddSpace { get; set; }

            internal LiElemInfos(eFieldRecord fldRecord) { FldRecord = fldRecord; AddSpace = true; }
        }

        /// <summary>
        /// Construction du HTML des vcard
        /// </summary>
        /// <returns>
        /// Vrai si tout est OK
        /// Faux si une erreur se produit, l'erreure étant renseignée dans _sErrorMsg
        /// </returns>
        protected override bool Build()
        {
            String liClass = String.Empty;
            String liValue = String.Empty;
            String vCardFldAlias = String.Empty;

            eFieldRecord ef = null;
            HtmlGenericControl li = null;

            #region Bloc de div
            // Div principale
            //   Panel pVCMainDiv = new Panel();
            //  pgContainer.Controls.Add(pVCMainDiv);


            _pgContainer.ID = "vcMain";

            Panel container = new Panel();
            container.CssClass = "vcGlbl";
            _pgContainer.Controls.Add(container);
            //_pgContainer.CssClass = "vcGlbl";


            //Div partie haute
            //Panel pVCUp = new Panel();
            //_pgContainer.Controls.Add(pVCUp);
            //pVCUp.ID = "vcBorderTop";
            //pVCUp.CssClass = "vcBorderTop";

            eRecord eRecord = null;
            if (_myFile.ListRecords.Count > 0)
                eRecord = _myFile.ListRecords[0];

            // Si on est en mode click ou mouseover
            //if (_bCroix)
            {
                /*  Croix FERMER*/
                Label titleSpan = new Label();
                container.Controls.Add(titleSpan);
                titleSpan.CssClass = "icon-edn-cross";

                titleSpan.ID = "vcCross";

                titleSpan.Attributes.Add("style", "visibility:" + (_bCroix ? "visible" : "hidden"));
                titleSpan.Attributes.Add("onclick", "top.shvc(top.oVCardCaller, 0);");
                /*******/
            }

            //Div contenu
            Panel pVCContenair = new Panel();
            container.Controls.Add(pVCContenair);
            pVCContenair.ID = "vcMID";
            pVCContenair.CssClass = "vcMiddle";


            //Sous-div de contenu
            Panel pVCSubCont = new Panel();
            pVCContenair.Controls.Add(pVCSubCont);
            pVCSubCont.ID = "vcSubMiddle";
            pVCSubCont.CssClass = "vcSubMiddle";


            //cadre Photo
            Panel pVCCadrePhoto = new Panel();
            pVCCadrePhoto.ID = "vcCadre";
            pVCCadrePhoto.CssClass = "vcCadre";

            // Initialisation de la photo de la VCARD
            Panel vcPhoto = new Panel();
            Boolean bHasPhoto = false;
            if (eRecord != null)
            {
                eFieldRecord f = eRecord.GetFields.Find(fld => fld.FldInfo.Descid == (TableType.PP.GetHashCode() + AllField.AVATAR.GetHashCode()));
                if (f != null && f.DisplayValue.Length > 0 && f.RightIsVisible)
                {
                    pVCSubCont.Controls.Add(pVCCadrePhoto);
                    pVCCadrePhoto.Controls.Add(vcPhoto);

                    eTools.SetAvatar((WebControl)vcPhoto, Pref, Tab, false, f.DisplayValue, f.FileId);
                }
            }

            //Cadre Détails
            Panel pVCDetails = new Panel();
            pVCSubCont.Controls.Add(pVCDetails);
            pVCDetails.ID = "vcDtls";
            pVCDetails.CssClass = String.Concat("vcDtls", bHasPhoto ? String.Empty : " noPhoto");

            //Séparateur bottom
            //Panel pVCSepBottom = new Panel();
            //pVCSepBottom.CssClass = "vcSepBtm";
            //pVCSubCont.Controls.Add(pVCSepBottom);


            //Navig
            Panel pVCNavig = new Panel();
            pVCSubCont.Controls.Add(pVCNavig);
            pVCNavig.ID = "vcNav";
            pVCNavig.CssClass = "";

            Panel pCroixOrNot = new Panel();
            pCroixOrNot.Style.Add("display", "none");
            pCroixOrNot.Attributes.Add("c", _bCroix.ToString().ToLower());
            pCroixOrNot.ID = "Cx";
            pVCNavig.Controls.Add(pCroixOrNot);

            #region pagging vcard
            for (int i = 1; i <= _myFile.NbVCards; i++)
            {
                Image btnNav = new Image();
                pVCNavig.Controls.Add(btnNav);
                btnNav.ID = String.Concat("swVc", i);
                btnNav.ImageUrl = eConst.GHOST_IMG;
                btnNav.Style.Add("border-width", "0px");

                if (i == _nPage)
                {
                    btnNav.CssClass = "icon-circle imgAct";
                }
                else
                {
                    btnNav.Attributes.Add("onclick", String.Concat("switchVCard(", i, ",", _nFileId, ")"));
                    btnNav.CssClass = "icon-circle-thin imgInact";

                }
            }
            #endregion


            //Div partie basse
            Panel pVCBorderBottom = new Panel();
            container.Controls.Add(pVCBorderBottom);
            pVCBorderBottom.ID = "vcBorderBtm";
            pVCBorderBottom.CssClass = "vcBorderBtm";

            #endregion

            if (eRecord != null)
            {
                //


                //Mapping
                String sMapping = _myFile.GetParam<String>("vCardMapping");
                XmlDocument xmlVCard = new XmlDocument();

                if (string.IsNullOrEmpty(sMapping))
                    throw (new Exception("Mapping VCARD manquant."));
                xmlVCard.LoadXml(sMapping);

                //Construction du détail
                HtmlGenericControl mainUL = new HtmlGenericControl("ul");
                mainUL.ID = "vCardUl";
                mainUL.Attributes.Add("class", "vCard");

                pVCDetails.Controls.Add(mainUL);

                // Prénom + Paricule + Nom
                List<LiElemInfos> lstPpName = new List<LiElemInfos>();

                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//firstname"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                lstPpName.Add(new LiElemInfos(ef));

                ef = eRecord.GetFieldByAlias(TableType.PP.GetHashCode() + "_203");
                if (ef != null)
                    lstPpName.Add(new LiElemInfos(ef) { AddSpace = !ef?.DisplayValue.EndsWith("'") ?? true });



                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//lastname"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                lstPpName.Add(new LiElemInfos(ef));

                li = GetVCardLi(eRecord, "vcName", "vcName", lstPpName);
                if (li != null)
                    mainUL.Controls.Add(li);

                // Fonction
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//title"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcFunction", "vcFunction", ef);
                if (li != null)
                    mainUL.Controls.Add(li);

                // Mail
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//email"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcMail", "vcMail", ef);
                if (li != null)
                    mainUL.Controls.Add(li);

                #region informations téléphone

                List<Control> phoneControls = new List<Control>();

                // Phone
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//phone"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcPhone", "vcPhone", ef);
                if (li != null)
                    phoneControls.Add(li);

                // Mobile
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//mobile"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcMobile", "vcPhone", ef);
                if (li != null)
                    phoneControls.Add(li);

                if (phoneControls.Count > 0)
                {
                    // Logo Phone
                    mainUL.Controls.Add(GetVCardLiEmpty("vcLogoPhone icon-phone"));

                    foreach (Control ctrl in phoneControls)
                        mainUL.Controls.Add(ctrl);

                    // Sep
                    mainUL.Controls.Add(GetVCardLiEmpty("vcSep"));
                }

                #endregion

                #region informations postale

                List<Control> postalControls = new List<Control>();

                // Société  
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//company"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcCorporate", "vcCorporate", ef);
                if (li != null)
                    postalControls.Add(li);

                // Rue 1
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//address1"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcAddress1", "vcAddress", ef);
                if (li != null)
                    postalControls.Add(li);

                // Rue 2
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//address2"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcAddress2", "vcAddress", ef);
                if (li != null)
                    postalControls.Add(li);

                // Rue 3
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//address3"));
                ef = eRecord.GetFieldByAlias(vCardFldAlias);
                li = GetVCardLi(eRecord, "vcAddress3", "vcAddress", ef);
                if (li != null)
                    postalControls.Add(li);

                // CP + Ville
                List<LiElemInfos> lstCpVille = new List<LiElemInfos>();
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//postalcode"));
                lstCpVille.Add(new LiElemInfos(eRecord.GetFieldByAlias(vCardFldAlias)));
                vCardFldAlias = GetAliasFromDescId(xmlVCard.SelectSingleNode("//city"));
                lstCpVille.Add(new LiElemInfos(eRecord.GetFieldByAlias(vCardFldAlias)));
                li = GetVCardLi(eRecord, "vcCPVille", "vcAddress", lstCpVille);
                if (li != null)
                    postalControls.Add(li);

                if (postalControls.Count > 0)
                {
                    // Logo Postal
                    mainUL.Controls.Add(GetVCardLiEmpty("vcLogoPostal icon-vc_card_adress"));

                    foreach (Control ctrl in postalControls)
                        mainUL.Controls.Add(ctrl);

                    // Sep 
                    mainUL.Controls.Add(GetVCardLiEmpty("vcSep"));
                }

                #endregion

                // Site
                li = GetVCardLiEmpty("vcSite");
                if (li != null)
                {
                    // Logo Site
                    mainUL.Controls.Add(GetVCardLiEmpty("vcLogoSite icon-site_web"));
                    mainUL.Controls.Add(li);
                    li.InnerText = String.Empty;  //"www.todo.com";
                }

            }
            else
            {

            }

            return true;
        }
        /// <summary>
        /// Appelé à la fin de construction par le renderer Pas de traitement spé pour la VCARD
        /// </summary>
        /// <returns>nothing</returns>
        protected override bool End()
        {
            return true;
        }

        #endregion


        /// <summary>
        /// Permet de récupérer l'alias en fonction du descid dans le mapping
        /// </summary>
        /// <param name="xml">Noeud XML du mapping de la VCARD dont on cherche l'alias</param>
        /// <returns>l'alias en fonction du descid dans le mapping</returns>
        public static String GetAliasFromDescId(XmlNode xml)
        {

            if (xml == null)
                return String.Empty;


            String sFieldDescId = xml.InnerText;
            Int32 nDescId;
            Int32 nTableDescId;
            String sAlias = string.Empty;

            if (Int32.TryParse(sFieldDescId, out nDescId))
            {
                nTableDescId = nDescId - nDescId % 100;

                switch (nTableDescId)
                {
                    case 200:
                    case 400:
                        sAlias = String.Concat(EudoQuery.TableType.PP.GetHashCode(), "_", nDescId.ToString());
                        break;
                    case 300:
                        sAlias = String.Concat(EudoQuery.TableType.PP.GetHashCode(), "_", EudoQuery.TableType.ADR.GetHashCode(), "_", nDescId.ToString());
                        break;
                }

            }

            return sAlias;
        }

    }
}