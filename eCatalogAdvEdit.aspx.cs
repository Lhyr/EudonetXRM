using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// NBA - 01-2012
    /// Classe qui genere la fenetre de modification ou d'ajout de valeurs des catalogues avancés
    /// Elle effectue une requete en base pour récupérer les langues à modifier ainsi que les libellés et infobulles existant
    /// Elle renvoie au final une table HTML avec les libellés et infobulles pour modification
    /// </summary>
    public partial class eCatalogAdvEdit : eEudoPage
    {
        private int descid = 0;
        private int dataId = 0;
        private bool _dataEnabled = false;
        private bool _dataEditable = false;
        private bool _fromAdmin = false;
        private DataTableReaderTuned _dtrFields = null;
        /// <summary>Tableau contenant les langues utilisées</summary>
        //private HashSet<string> tabLang = new HashSet<string>(); 
        private List<eAdminLanguage> _listLang = null;
        /// <summary>'edit' pour modification et 'add' pour ajout d'une nouvelle valeur</summary>
        private string catAdvAction = string.Empty;
        /// <summary>Nom de la première textbox pour lui donner le focus</summary>
        protected String firstTxtBox = string.Empty;
        private string txtSearch = string.Empty;
        eudoDAL _edal = null;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Se lance au chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string langUsed = string.Empty;

            #region Récupération des variables passées en POST
            try
            {
                //DESCID DU CATALOGUE
                if ((Request.Form["CatDescId"] != null) && (Request.Form["CatDescId"] != ""))
                    descid = int.Parse(Request.Form["CatDescId"].ToString());
                else
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                        eResApp.GetRes(_pref, 72),
                        String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "descid"), " (descid = ", (Request.Form["CatDescId"] != null ? Request.Form["CatDescId"] : "null"), ")")
                    );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }

                //Format attendu : INT
                if ((Request.Form["dataID"] != null) && (Request.Form["dataID"] != ""))
                    dataId = int.Parse(Request.Form["dataID"].ToString());

                //FORMAT attendy STRING : Série de INT séparé par des ";"
                //  les int représente les langue a modifié et doivent donc être compris entre 0 et MAX_LANG
                //if (Request.Form["langUsed"] != null)
                //{
                //    langUsed = Request.Form["langUsed"].ToString();
                //    Int32 nNum;
                //    foreach (String s in langUsed.Split(';'))
                //        if (Int32.TryParse(s, out nNum) && nNum >= 0 && nNum <= eLibConst.MAX_LANG)
                //            tabLang.Add(s);
                //}

                //TYPE D'ACTION SUR LE CATALOGUE
                //  Seule action testée : "edit"
                if (Request.Form["action"] != null)
                    catAdvAction = Request.Form["action"].ToString();

                //AFFICHAGE DU CHAMP DATA
                if (Request.Form["dataEnabled"] != null)
                    _dataEnabled = Request.Form["dataEnabled"].ToString() == "true" ? true : false;

                //EDITION DU CHAMP DATA
                if (Request.Form["dataEdit"] != null)
                    _dataEditable = Request.Form["dataEdit"].ToString() == "true" ? true : false;

                //RECHERCHE A EFFECTUER
                if (Request.Form["TxtSearch"] != null)
                    txtSearch = Request.Form["TxtSearch"].ToString();

                //DEPUIS ADMIN
                if (Request.Form["fromAdmin"] != null)
                    _fromAdmin = Request.Form["fromAdmin"].ToString() == "1" ? true : false;

                InitLanguages();
            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    ex.Message,
                    "Erreur lors du chargement du catalogue : " + ex.StackTrace
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            #endregion

            // Recherche des langues à afficher - uniqement numérique

            //scripts            
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");

            #region CSS

            PageRegisters.AddCss("eModalDialog");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eControl");

            #endregion

            _edal = eLibTools.GetEudoDAL(_pref);
            _edal.OpenDatabase();

            try
            {
                if (catAdvAction.Equals("edit"))
                {
                    #region Appel à Edal - TODO : DEPLACER L'APPEL A DATA ET REVOIR LA CONSTRUCTION DE LA REQUETE

                    // Requete de selection des libellés et des ToolTips
                    StringBuilder _reqlang = new StringBuilder();
                    _reqlang.Append("SELECT [FILEDATA].[DATA], ");
                    string lang = string.Empty;

                    foreach (eAdminLanguage l in _listLang)
                    {
                        lang = l.Id.ToString("00");
                        _reqlang.Append(" [FILEDATA].[Lang_").Append(lang).Append("], [FILEDATA].[Tip_Lang_");
                        _reqlang.Append(lang).Append("], '").Append(lang);
                        _reqlang.Append("' , [FILEDATA].[TIP_Lang_").Append(lang).Append("_Format], ");
                    }

                    _reqlang.Append(" ISNULL([FILEDATA].[Hidden], 0) [DisabledValue], ");

                    _reqlang.Remove(_reqlang.ToString().LastIndexOf(","), 1); // on supprime la derniere virgule
                    string _error = string.Empty;

                    _reqlang.Append(" FROM [FILEDATA]");
                    _reqlang.AppendLine(" WHERE [FILEDATA].[DescId] = @DescId AND [FILEDATA].[DATAID] = @DataId;");

                    RqParam _rqFields = new RqParam(_reqlang.ToString());
                    _rqFields.AddInputParameter("@DescId", System.Data.SqlDbType.Int, descid);
                    _rqFields.AddInputParameter("@DataId", System.Data.SqlDbType.Int, dataId);

                    _dtrFields = _edal.Execute(_rqFields, out _error);
                    try
                    {
                        // On va contruire le flux html
                        if (String.IsNullOrEmpty(_error) && _dtrFields != null && _dtrFields.HasRows)
                        {
                            divCAtAdvEdit.Controls.Add(CreateTableEdit(true));
                        }
                    }
                    finally
                    {
                        if (_dtrFields != null)
                            _dtrFields.Dispose();
                    }
                    #endregion
                }
                else
                    divCAtAdvEdit.Controls.Add(CreateTableEdit(false));
            }
            finally
            {
                if (_edal != null)
                    _edal.CloseDatabase();
            }
        }

        /// <summary>
        /// Chargement des langues actives de la base
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Aucune langue active trouvée</exception>
        private void InitLanguages()
        {
            _listLang = eAdminLanguage.Load(_pref, true);

            if (_listLang == null)
            {
                throw new Exception("Aucune langue active trouvée");
            }
        }

        /// <summary>
        /// Creation d'une HtmlTableRow avec les drapeaux et les input correspondant 
        /// libellé et tooltip
        /// </summary>
        /// <param name="p_class">Nom de la classe pour l'image (dreapeaux)</param>
        /// <param name="p_altImg">Alt de l'image </param>
        /// <param name="p_IDlibelle">Id de l'input test du libellé</param>
        /// <param name="p_lib">Libellé à inserer dans l'input</param>
        /// <param name="p_IDtooltip">ID de l'input du ToolTip</param>
        /// <param name="p_tip">ToolTip à inserer dans l'input</param>
        /// <returns>renvoi une row à inserer dans une htmltablerow</returns>
        private TableRow CreateRowCat(string p_class, string p_altImg, string p_IDlibelle, string p_lib, string p_IDtooltip, string p_tip, String langLabel)
        {
            TableRow _row = new TableRow();
            TableCell _cell = new TableCell();

            // Drapeaux 1 
            _cell.CssClass = p_class;
            _cell.Text = langLabel;
            _row.Cells.Add(_cell);

            // Input de saisie 1 Label
            HtmlInputText inpTxlib = new HtmlInputText();
            inpTxlib.ID = p_IDlibelle;
            inpTxlib.Value = p_lib;
            inpTxlib.Attributes.Add("class", "lib_flag");

            _cell = new TableCell();
            _cell.CssClass = "td_lbl";
            _cell.Controls.Add(inpTxlib);
            _row.Cells.Add(_cell);

            //drapeau 2 
            _cell = new TableCell();
            _cell.CssClass = p_class;
            _cell.Text = langLabel;
            _row.Cells.Add(_cell);

            // Input de saisie 2
            HtmlInputText inpTxTipt = new HtmlInputText();
            inpTxTipt.ID = p_IDtooltip;
            inpTxTipt.Value = p_tip;
            inpTxTipt.Attributes.Add("class", "lib_flag");

            _cell = new TableCell();
            _cell.CssClass = "td_tip";
            _cell.Controls.Add(inpTxTipt);
            _row.Cells.Add(_cell);


            return _row;
        }

        /// <summary>
        /// Rend une table html complete avec les drapeaux et les input
        /// </summary>
        /// <param name="action">true si on edite une valeur et false pour l'ajout d'une nouvelle valeur</param>
        /// <returns>Rend une table html</returns>
        private System.Web.UI.WebControls.Table CreateTableEdit(bool action)
        {
            System.Web.UI.WebControls.Table _tbCatAdvEdit = new System.Web.UI.WebControls.Table();
            TableRow _rowsCatAdvEdit = new TableRow();
            TableCell _cellCatAdvEdit = new TableCell();

            //Format du tool tip html ou text
            //  _cellCatAdvEdit = new TableCell();
            //       _cellCatAdvEdit.Text = "Format HTML : ";
            //  _rowsCatAdvEdit.Cells.Add(_cellCatAdvEdit);
            //--------

            _tbCatAdvEdit.CssClass = "tbCatADVEdit";

            #region Entête du tableau
            TableHeaderRow headerRow = new TableHeaderRow();

            TableHeaderCell headerCell = new TableHeaderCell();
            headerCell.Text = String.Concat(eResApp.GetRes(_pref, 223), " : ");
            headerCell.CssClass = "catAdvEditHlbl";
            headerCell.ColumnSpan = 2;
            headerRow.Cells.Add(headerCell);
            headerCell = new TableHeaderCell();
            headerCell.Text = String.Concat(eResApp.GetRes(_pref, 130), " : ");
            headerCell.CssClass = "catAdvEditHTip";
            headerCell.ColumnSpan = 2;
            headerRow.Cells.Add(headerCell);

            _tbCatAdvEdit.Rows.Add(headerRow);
            #endregion

            HtmlGenericControl ulFooter = new HtmlGenericControl("ul");
            ulFooter.Attributes.Add("class", "ulCatAdvEditFt");

            HtmlInputHidden hiddenDataId = new HtmlInputHidden();
            hiddenDataId.ID = "hDataId";
            hiddenDataId.Value = dataId.ToString();

            #region Parcourt du DataTableReader pour recuperation des données en mode modification

            string _classCss = string.Empty;
            string _idLib = string.Empty;
            string _idTtip = string.Empty;
            string _toolTip = string.Empty;
            string _libelle = string.Empty;
            string _idChkFormat = string.Empty;
            string _toolTipFormat = string.Empty;
            string _codeCat = string.Empty;
            String langLabel = String.Empty;
            Boolean disabledValue = false;

            // Modification
            if (action)
            {
                while (_dtrFields.Read())
                {
                    _codeCat = _dtrFields.GetString(0);
                    disabledValue = _dtrFields.GetBoolean("DisabledValue");
                    for (int colIndex = 1; colIndex < _dtrFields.NbCols - 1; colIndex = colIndex + 4)
                    {
                        _rowsCatAdvEdit = new TableRow();
                        _classCss = string.Concat("rFly flag_", _dtrFields.GetString(colIndex + 2));
                        _idLib = string.Concat("lbl_", _dtrFields.GetString(colIndex + 2));
                        _idTtip = string.Concat("tip_", _dtrFields.GetString(colIndex + 2));
                        //        _idChkFormat = string.Concat("chkFormat_", _dtrFields[colIndex + 2].ToString());
                        _toolTip = _dtrFields.GetString(colIndex + 1);
                        _libelle = _dtrFields.GetString(colIndex);
                        //       _toolTipFormat = _dtrFields[colIndex + 3].ToString();
                        langLabel = eLibTools.GetLangLabel(_edal, eLibTools.GetNum(_dtrFields.GetString(colIndex + 2)));
                        _rowsCatAdvEdit = CreateRowCat(_classCss, "", _idLib, _libelle, _idTtip, _toolTip, langLabel);
                        _tbCatAdvEdit.Rows.Add(_rowsCatAdvEdit);
                    }
                }

            }
            #endregion

            #region Creation des textbox selon les langues utilisée

            else //Mode ajout de nouvelles valeurs 
            {

                int nCmpt = 0;
                String langCode = string.Empty;
                foreach (eAdminLanguage l in _listLang)
                {
                    _rowsCatAdvEdit = new TableRow();
                    // recup du contenu de la textbox de recherche
                    if (nCmpt == 0 && txtSearch != "")
                        _libelle = txtSearch;

                    langCode = l.Id.ToString("00");
                    _classCss = string.Concat("rFly flag_", langCode);
                    _idLib = string.Concat("lbl_", langCode);
                    _idTtip = string.Concat("tip_", langCode);
                    //   _idChkFormat = string.Concat("chkFormat_", tabLang[j].PadLeft(2, '0'));

                    langLabel = eLibTools.GetLangLabel(_edal, l.Id);
                    _rowsCatAdvEdit = CreateRowCat(_classCss, "", _idLib, _libelle, _idTtip, _toolTip, langLabel);
                    _tbCatAdvEdit.Rows.Add(_rowsCatAdvEdit);
                    nCmpt++;
                }
            }
            #endregion

            // On affiche le Champs Code que si l'option en base est activée
            if (_dataEnabled)
            {
                HtmlGenericControl liCodeFooter = new HtmlGenericControl("li");

                liCodeFooter.Attributes.Add("class", "lib_code");

                HtmlInputText inpCatAdvCode = new HtmlInputText("text");

                //si le code est généré par formule ou autoincrémenté, on empêche la modification du champ code en édition.
                if (!_dataEditable)
                {
                    inpCatAdvCode.Attributes.Add("readonly", "readonly");
                    inpCatAdvCode.Attributes.Add("ero", "1");
                    inpCatAdvCode.Disabled = true;
                }

                inpCatAdvCode.Attributes.Add("class", "inp_code");
                inpCatAdvCode.ID = "inpCode";

                liCodeFooter.InnerText = eResApp.GetRes(_pref, 973);
                liCodeFooter.Controls.Add(inpCatAdvCode);
                ulFooter.Controls.Add(liCodeFooter);
                if (action)
                    inpCatAdvCode.Value = _codeCat; // Valeur d code catalogue  
            }

            // On active la case désactivée que si on est en Administration 
            // HLA - Le UserLevel n'indique pas qu'on viens de l'administration !
            if (_fromAdmin)
            {
                // Champ désactivé 
                HtmlGenericControl liChkFooter = new HtmlGenericControl("li");
                liChkFooter.Attributes.Add("class", "chk_code");

                eCheckBoxCtrl chkActiv = new eCheckBoxCtrl(disabledValue, false);
                chkActiv.ID = "chkActiv";
                chkActiv.AddClick("");
                chkActiv.AddText(eResApp.GetRes(_pref, 690));

                liChkFooter.Controls.Add(chkActiv);
                ulFooter.Controls.Add(liChkFooter);
            }

            // Nom de la première textbox pour lui donner le focus
            firstTxtBox = string.Concat("lbl_", _listLang.First().Id.ToString("00"));

            divCatAdvEditF.Controls.Add(ulFooter);
            divCatAdvEditF.Controls.Add(hiddenDataId);

            return _tbCatAdvEdit;
        }
    }
}