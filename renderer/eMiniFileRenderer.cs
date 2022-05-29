using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu d'une mini-fiche
    /// </summary>
    public class eMiniFileRenderer : eRenderer
    {
        #region PROPRIETES
        /// <summary>DescId de la table de la mini fiche courante</summary>
        private Int32 _nTab = 0;
        /// <summary>Id de la mini fiche courante</summary>
        private Int32 _nFileId = 0;

        /// <summary>Objet d'accès aux données</summary>
        protected eMiniFile _myFile;

        /// <summary>
        /// Indique si on est en mode mouseover ou au click
        /// Ceci est necessaire pour determiner si on affiche ou non la croix
        /// True pour ne pas afficher la croix et false pour l'afficher
        /// </summary>
        private Boolean _bCroix;
        #endregion

        #region ACCESSEURS
        /// <summary>DescId de la table de la mini fiche courante</summary>
        public Int32 Tab
        {
            get { return _nTab; }
        }

        /// <summary>Id de la mini fiche courante</summary>
        public Int32 FileId
        {
            get { return _nFileId; }
        }

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
        #endregion

        #region CONSTRUCTEUR
        /// <summary>
        /// Constructeur initilise les propriété de base de VCARD
        /// </summary>
        /// <param name="pref">Preference de l'user en cours</param>
        /// <param name="nTab">DescId de l'onglet</param>
        /// <param name="nFileId">Id de la fiche</param>
        public eMiniFileRenderer(ePref pref, Int32 nTab, Int32 nFileId)
        {
            Pref = pref;
            _nTab = nTab;
            _nFileId = nFileId;
        }
        #endregion

        #region METHODES PRINCIPALES DE RENDERER
        /// <summary>
        /// Page permettant d'initialiser le contenu de retour MiniFiche HTML
        /// </summary>
        /// <returns>
        /// Vrai si tout est OK
        /// Faux si une erreur se produit, l'erreure étant renseignée dans _sErrorMsg
        /// </returns>
        protected override bool Init()
        {
            try
            {
                _myFile = eMiniFile.CreateMiniFiles(Pref, _nTab, _nFileId);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eMiniFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
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
                _sErrorMsg = String.Concat("eMiniFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }



        /// <summary>
        /// Construction du HTML des MiniFiches
        /// </summary>
        /// <returns>
        /// Vrai si tout est OK
        /// Faux si une erreur se produit, l'erreure étant renseignée dans _sErrorMsg
        /// </returns>
        protected override bool Build()
        {
            //Mapping
            List<eFilemapPartner> mappings = _myFile.Mappings;

            String liClass = String.Empty;
            String liValue = String.Empty;
            String vCardFldAlias = String.Empty;

            eFieldRecord ef = null;

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
          //  if (_bCroix)
            {
                /*  Croix FERMER*/
                Label titleSpan = new Label();
                container.Controls.Add(titleSpan);
                titleSpan.CssClass = "icon-edn-cross";
                titleSpan.Attributes.Add("onclick", "top.shvc(top.oVCardCaller, 0);");
                titleSpan.ID = "vcCross";

                titleSpan.Attributes.Add("style", "visibility:" + (_bCroix ? "visible" : "hidden"));
                

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
            eFilemapPartner mappingImage = mappings.Find(efp => efp.SourceDescId == FILEMAP_MINIFILE_TYPE.IMAGE.GetHashCode());
            if (mappingImage != null && eRecord != null)
            {
                string sAlias = GetAliasFromDescId(mappingImage.DescId);
                eFieldRecord f = eRecord.GetFieldByAlias(sAlias);

                if (f != null && f.DisplayValue.Length > 0)
                {
                    pVCSubCont.Controls.Add(pVCCadrePhoto);
                    pVCCadrePhoto.Controls.Add(vcPhoto);

                    eTools.SetAvatar((WebControl)vcPhoto, Pref, Tab, false, f.DisplayValue, _nFileId);
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
            /*
            for (int i = 1; i <= _myFile.NbVCards; i++)
            {
                Image btnNav = new Image();
                pVCNavig.Controls.Add(btnNav);
                btnNav.ID = String.Concat("swVc", i);
                btnNav.ImageUrl = eConst.GHOST_IMG;
                btnNav.Style.Add("border-width", "0px");

                if (i == _nPage)
                {
                    btnNav.CssClass = "imgAct";
                }
                else
                {
                    btnNav.Attributes.Add("onclick", String.Concat("switchVCard(", i, ",", _nFileId, ")"));
                    btnNav.CssClass = "imgInact";

                }
            }
            */
            #endregion


            //Div partie basse
            Panel pVCBorderBottom = new Panel();
            container.Controls.Add(pVCBorderBottom);
            pVCBorderBottom.ID = "vcBorderBtm";
            pVCBorderBottom.CssClass = "vcBorderBtm";

            #endregion

            if (eRecord != null)
            {
                //Construction du détail
                HtmlGenericControl mainUL = new HtmlGenericControl("ul");
                mainUL.ID = "vCardUl";
                mainUL.Attributes.Add("class", "vCard");

                pVCDetails.Controls.Add(mainUL);


                //mappings.Sort(delegate (eFilemapPartner x, eFilemapPartner y) { return x.Order.CompareTo(y.Order); });
                foreach (eFilemapPartner mapping in mappings)
                {
                    if (mapping.SourceDescId == FILEMAP_MINIFILE_TYPE.FIELD.GetHashCode() || mapping.SourceDescId == FILEMAP_MINIFILE_TYPE.FIELD_TITLE.GetHashCode())
                    {
                        string sAlias = GetAliasFromDescId(mapping.DescId);
                        ef = eRecord.GetFieldByAlias(sAlias);

                        if (ef != null)
                        {
                            bool displayLibelle = mapping.SourceType == FILEMAP_MINIFILE_SOURCETYPE.LABEL_DISPLAY.GetHashCode();

                            if (mapping.SourceDescId == FILEMAP_MINIFILE_TYPE.FIELD_TITLE.GetHashCode())
                                mainUL.Controls.Add(GetMiniFileFieldTitle(eRecord, mapping.FieldLabel, displayLibelle, ef));
                            else
                                mainUL.Controls.Add(GetMiniFileField(eRecord, mapping.FieldLabel, displayLibelle, ef));
                        }
                    }
                    else if (mapping.SourceDescId == FILEMAP_MINIFILE_TYPE.TITLE.GetHashCode())
                    {
                        mainUL.Controls.Add(GetMiniFileTitle(mapping.FieldLabel.Length > 0 ? mapping.FieldLabel : mapping.Source));
                    }
                    else if (mapping.SourceDescId == FILEMAP_MINIFILE_TYPE.SEPARATOR.GetHashCode())
                    {
                        mainUL.Controls.Add(GetMiniFileSeparator());
                    }
                }
            }
            else
            {

            }

            return true;
        }

        /// <summary>
        /// Appelé à la fin de construction par le renderer Pas de traitement spé pour la Mini Fiche
        /// </summary>
        /// <returns>nothing</returns>
        protected override bool End()
        {
            return true;
        }

        #endregion



        #region METHODES PRIVÉES
        private HtmlGenericControl GetMiniFileField(eRecord row, String libelle, Boolean displayLibelle, eFieldRecord ef, string className)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", className);

            // ELAIZ - Request 81 950 - Ajoute une classe CSS pour masquer la ligne si cette dernière n'a aucune valeur
            if(ef.DisplayValue == "" || ef.Value == "")
                li.Attributes.Add("class", "vcEmpty");

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.Attributes.Add("did", ef.FldInfo.Descid.ToString());
            span.ID = eTools.GetFieldValueCellId(row, ef);

            if (displayLibelle)
                span.InnerText = String.Concat(libelle, " : ", ef.DisplayValue);
            else
                span.InnerText = ef.DisplayValue;

            // Cas spécifique pour 201
            //if (ef.FldInfo.Descid == 201 && ef.NameOnly)
            //span.Attributes.Add("nameonly", "");

            li.Controls.Add(span);

            return li;
        }

        private HtmlGenericControl GetMiniFileField(eRecord row, String libelle, Boolean displayLibelle, eFieldRecord ef)
        {
            return GetMiniFileField(row, libelle, displayLibelle, ef, "vcMiniFileField");
        }

        private HtmlGenericControl GetMiniFileFieldTitle(eRecord row, String libelle, Boolean displayLibelle, eFieldRecord ef)
        {
            return GetMiniFileField(row, libelle, displayLibelle, ef, "vcMiniFileTitle");
        }

        private HtmlGenericControl GetMiniFileTitle(String libelle)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "vcMiniFileTitle");

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerText = libelle;

            li.Controls.Add(span);

            return li;
        }

        private HtmlGenericControl GetMiniFileSeparator()
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "vcSep");

            return li;
        }

        private String GetAliasFromDescId(int nDescId)
        {
            //Int32 nTableDescId;
            String sAlias = string.Empty;

            //nTableDescId = nDescId - (nDescId % 100);

            //sAlias = String.Concat(nTableDescId, "_", nDescId.ToString());
            sAlias = String.Concat(_nTab, "_", nDescId.ToString());

            return sAlias;
        }
        #endregion
    }
}