using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe permettant d'afficher une carte Kanban
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eRenderer" />
    public class eKanbanCardRenderer : eRenderer
    {
        #region Propriétés privées
        List<eMiniFileParam> _cardParams = null;
        eRes _res = null;
        int _fileID = 0;
        bool _draggable = false;
        /// <summary>
        /// Objet eRecord de la fiche
        /// </summary>
        eRecord _record;
        #endregion

        /// <summary>
        /// Creates the kanban card renderer.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tab">ID de la table</param>
        /// <param name="cardParams">The card parameters.</param>
        /// <param name="record">The record.</param>
        /// <param name="draggable">if set to <c>true</c> [draggable].</param>
        /// <returns></returns>
        public static eKanbanCardRenderer CreateKanbanCardRenderer(ePref pref, int tab, List<eMiniFileParam> cardParams = null, eRecord record = null, bool draggable = false)
        {
            eKanbanCardRenderer rdr = new eKanbanCardRenderer(pref, tab, cardParams, record, draggable);
            return rdr;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="eKanbanCardRenderer" /> class.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tab">ID de la table</param>
        /// <param name="cardParams">The card parameters.</param>
        /// <param name="record">The record.</param>
        /// <param name="draggable">if set to <c>true</c> [draggable].</param>
        private eKanbanCardRenderer(ePref pref, int tab, List<eMiniFileParam> cardParams, eRecord record, bool draggable) : base()
        {
            this.Pref = pref;
            _tab = tab;
            _record = record;
            _fileID = (record != null) ? record.MainFileid : 0;
            _draggable = draggable;
            _cardParams = cardParams;
        }


        /// <summary>
        /// Appel l'objet métier
        /// eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {
                if (_cardParams == null)
                {
                    _cardParams = eMiniFileParam.GetFakeParams();

                    //_displayEmptyField = true;
                }


                if (_cardParams == null || _cardParams.Count == 0)
                {
                    _sErrorMsg = "Aucun mapping de carte Kanban trouvé";
                    return false;
                }


            }
            catch (Exception)
            {
                throw;
            }


            return true;
        }


        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            try
            {
                Panel wrapper;

                _pgContainer.CssClass = "kanbanCard";
                if (_draggable)
                {
                    _pgContainer.Attributes.Add("draggable", "true");
                }
                _pgContainer.Attributes.Add("data-fid", _fileID.ToString());


                // Avatar
                CreateControlForField(_pgContainer, FILEMAP_MINIFILE_TYPE.IMAGE, "cardBlockAvatar");

                wrapper = new Panel();
                wrapper.ID = "cardContentWrapper";
                _pgContainer.Controls.Add(wrapper);

                // Titre
                CreateControlForField(wrapper, FILEMAP_MINIFILE_TYPE.FIELD_TITLE, "cardBlockTitle");

                // Champs
                CreateControlForField(wrapper, FILEMAP_MINIFILE_TYPE.FIELD, "cardBlockFields");


                CreateParentsPart(wrapper);


            }
            catch (Exception exc)
            {
                _eException = exc;
                return false;
            }


            return true;
        }

        private void CreateParentsPart(Panel wrapper)
        {
            List<eMiniFileParam> list = _cardParams.FindAll(p => p.DisplayType == FILEMAP_MINIFILE_TYPE.SEPARATOR && p.Value != 0);

            if (list != null && list.Count > 0)
            {
                Panel panel = new Panel();
                panel.ID = "cardBlockParentFields";
                panel.CssClass = "cardBlock";
                wrapper.Controls.Add(panel);

                foreach (eMiniFileParam p in list)
                {
                    CreateParentTabPart(panel, p.Value, "cardBlockFields" + p.Value);
                }
            }

        }

        /// <summary>
        /// Chargement des ressources nécessaires
        /// </summary>
        private void LoadRes()
        {
            List<eMiniFileParam> paramsList;
            if (_fileID > 0)
                paramsList = _cardParams.FindAll(p => p.DisplayLabel == true && p.DisplayType != FILEMAP_MINIFILE_TYPE.SEPARATOR && p.Value != 0);
            else
                paramsList = _cardParams.FindAll(p => p.DisplayType != FILEMAP_MINIFILE_TYPE.SEPARATOR && p.Value != 0);

            if (paramsList.Count > 0)
            {
                List<int> descidList = paramsList.Select(p => p.Value).ToList();

                _res = new eRes(this.Pref, string.Join(",", descidList));
            }

        }

        /// <summary>
        /// Création du "champ" (libellé et valeur)
        /// </summary>
        /// <param name="container">Panel auquel ajouter le(s) contrôle(s)</param>
        /// <param name="type">Type de mini-fiche</param>
        /// <param name="idBlock">ID du bloc conteneur</param>
        /// <param name="parentTab">ID de la table parente</param>
        private void CreateControlForField(Panel container, FILEMAP_MINIFILE_TYPE type, string idBlock, int parentTab = 0)
        {
            string label, value = string.Empty;
            //bool resFound = false;
            Panel block, field;
            HtmlGenericControl span;
            eFieldRecord fRec = null;

            block = new Panel();
            block.ID = idBlock;
            block.CssClass = "cardBlock";
            if (container == null)
                container = _pgContainer;



            List<eMiniFileParam> paramsList = _cardParams.FindAll(p => p.DisplayType == type && p.ParentTab == parentTab);
            foreach (eMiniFileParam param in paramsList)
            {
                label = string.Empty;
                value = string.Empty;

                if (_record != null)
                {
                    fRec = _record.GetFieldByAlias(String.Concat(_tab, "_", param.Value));
                    if (fRec != null)
                    {
                        label = fRec.FldInfo.Libelle;
                        value = fRec.DisplayValue;
                    }

                }


                // Dans le cas de l'avatar, si la valeur est vide, on ne crée pas de bloc
                if (param.DisplayType == FILEMAP_MINIFILE_TYPE.IMAGE)
                {
                    if (fRec == null || String.IsNullOrEmpty(value))
                        return;
                }

                if (fRec == null)
                    continue;


                //if (_displayEmptyField
                //|| (!_displayEmptyField && !String.IsNullOrEmpty(value))
                //)
                //{
                field = new Panel();
                field.CssClass = "cardField";
                field.Attributes.Add("data-did", fRec.FldInfo.Descid.ToString());

                // Champ obligatoire
                if (fRec.IsMandatory)
                    field.Attributes.Add("data-required", "1");

                if (param.DisplayType == FILEMAP_MINIFILE_TYPE.FIELD_TITLE)
                {
                    field.CssClass = "cardTitle cardField";
                    field.Attributes.Add("data-fid", fRec.FileId.ToString());
                    field.Attributes.Add("data-tab", fRec.FldInfo.Table.DescId.ToString());
                    field.Attributes.Add("data-openpopup", (fRec.FldInfo.Table.EdnType != EdnType.FILE_MAIN) ? "1" : "0");
                    field.Attributes.Add("data-tablabel", fRec.FldInfo.Table.Libelle);

                    if (String.IsNullOrEmpty(value))
                    {
                        value = "_";
                    }
                }
                else if (param.DisplayType == FILEMAP_MINIFILE_TYPE.IMAGE)
                {

                    field.CssClass = "cardAvatar";

                }
                else if (String.IsNullOrEmpty(value))
                    field.Style.Add("display", "none");


                if (param.DisplayType != FILEMAP_MINIFILE_TYPE.IMAGE)
                {
                    #region Libellé/Valeur

                    //if (_fileID == 0)
                    //{
                    //    label = param.Label;
                    //}


                    if (param.DisplayLabel)
                    {
                        span = new HtmlGenericControl();
                        span.InnerText = label + (!String.IsNullOrEmpty(label) ? " :" : "");
                        span.Attributes.Add("title", fRec.FldInfo.ToolTipText);

                        field.Controls.Add(span);
                    }

                    //if (_fileID == 0)
                    //{
                    //    value = param.DisplayValue;
                    //}

                    span = new HtmlGenericControl();
                    span.Attributes.Add("class", "fieldValue");
                    span.Attributes.Add("data-did", fRec.FldInfo.Descid.ToString());
                    span.Attributes.Add("data-label", label);
                    // Les champs texte et mémo sont limités à 100 caractères
                    if (value.Length > 100 && (fRec.FldInfo.Format == FieldFormat.TYP_MEMO || fRec.FldInfo.Format == FieldFormat.TYP_CHAR))
                    {
                        value = String.Concat(value.Substring(0, 100), "...");
                    }
                    else if (fRec.FldInfo.Format == FieldFormat.TYP_BIT || fRec.FldInfo.Format == FieldFormat.TYP_BITBUTTON)
                    {
                        value = (value == "1") ? eResApp.GetRes(_ePref, 58) : eResApp.GetRes(_ePref, 59);
                    }
                    //ALISTER => Concerne la demande 80 032
                    span.InnerText = System.Net.WebUtility.HtmlDecode(value);
                    span.Attributes.Add("title", System.Net.WebUtility.HtmlDecode(fRec.DisplayValue));

                    field.Controls.Add(span);

                    block.Controls.Add(field);

                    #endregion
                }
                else
                {
                    #region Image

                    DisplayCardImage(field, fRec);
                    block.Controls.Add(field);

                    // TODO : en attendant quoi faire des images >.<
                    //HtmlImage img = new HtmlImage();
                    //img.Src = String.Concat("eImage.aspx?did=", param.Value, "&fid=", fRec.FileId, "&w=64&h=62&it=IMAGE_FIELD&ts=", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    //field.Controls.Add(img);
                    //block.Controls.Add(field);

                    #endregion
                }




                //}


            }

            container.Controls.Add(block);
        }

        private void DisplayCardImage(Panel container, eFieldRecord fRec)
        {
            // On ne gère pas le cas des images stockées en base
            if (fRec.FldInfo.ImgStorage == ImageStorage.STORE_IN_DATABASE)
            {
                return;
            }

            string url = string.Empty;

            string thumbnailName = string.Empty;
            string path = string.Empty;
            //int newWidth = 0;
            //int newHeight = 0;

            if (!String.IsNullOrEmpty(fRec.Value))
            {
                if (fRec.FldInfo.ImgStorage == ImageStorage.STORE_IN_FILE)
                {
                    // Si avatar, on essaie de récupérer l'URL de la miniature
                    if (fRec.FldInfo.Descid % 100 == (int)AllField.AVATAR)
                    {
                        thumbnailName = eImageTools.GetThumbNailName(fRec.Value);
                        if (!String.IsNullOrEmpty(thumbnailName))
                        {
                            path = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), @"\", thumbnailName);
                            if (File.Exists(path))
                            {
                                url = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), "/", thumbnailName);
                            }
                        }
                    }

                    if (String.IsNullOrEmpty(url))
                        url = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), "/", fRec.Value);
                }
                else if (fRec.FldInfo.ImgStorage == ImageStorage.STORE_IN_URL)
                {
                    url = fRec.Value;
                }
            }

            if (String.IsNullOrEmpty(url))
                return;

            HtmlImage image = new HtmlImage();
            image.Src = url;
            container.Controls.Add(image);
        }

        /// <summary>
        /// Création de la partie table parente : titre et champs
        /// </summary>
        /// <param name="container">Panel auquel il faut ajouter le contenu</param>
        /// <param name="parentTab">ID de la table parente</param>
        /// <param name="idBlock">ID du bloc</param>
        private void CreateParentTabPart(Panel container, int parentTab, string idBlock)
        {

            Panel panel = new Panel();
            //panel.CssClass = "cardBlockParentFields";
            container.Controls.Add(panel);


            // Titre
            CreateControlForField(panel, FILEMAP_MINIFILE_TYPE.FIELD_TITLE, idBlock, parentTab);

            // Champs
            CreateControlForField(panel, FILEMAP_MINIFILE_TYPE.FIELD, idBlock, parentTab);

        }
    }
}