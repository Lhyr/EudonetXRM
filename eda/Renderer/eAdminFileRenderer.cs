
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.eda;
using System.Linq;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// renderer du fichier en mode administration
    /// </summary>
    public class eAdminFileRenderer : eMasterFileRenderer
    {
        #region Propriétés
        Dictionary<int, List<eAdminTriggerField>> _dicFormulasTriggers = new Dictionary<int, List<eAdminTriggerField>>();
        int _nbFreeFields = 0;
        protected eAdminTableInfos _tabInfos;
        Boolean _showFileProp = false;
        protected HashSet<int> _specialFields = new HashSet<int>();

        /// <summary>
        /// champ mappé dans l'ORM
        /// </summary>
        protected OrmMappingInfo _ormInfos;

        #endregion

        #region Accesseurs
        /// <summary>
        /// Sur l'admin on affiche tjs le header appartenance et traçabilité (cf spec )
        /// </summary>
        protected override bool HideHeader
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Visibilité de la partie contenu "Appartenance et traçabilité" (partie ouverte)
        /// </summary>
        public bool ShowFileProp
        {
            get
            {
                return _showFileProp;
            }

            set
            {
                _showFileProp = value;
            }
        }

        /// <summary>
        /// Les champs qui ne sont pas supprimables. Pas de modification de type non plus
        /// </summary>
        public HashSet<int> SpecialFields
        {
            get
            {
                return _specialFields;
            }

            set
            {
                _specialFields = value;
            }
        }
        #endregion


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        protected eAdminFileRenderer(ePref pref, eAdminTableInfos tabInfos)
        {

            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            Pref = pref;
            _tab = tabInfos.DescId;
            _rType = RENDERERTYPE.AdminFile;

            _tabInfos = tabInfos;

            SpecialFields = eAdminTools.GetSpecialFields(tabInfos.DescId, tabInfos.EdnType, _tabInfos.IsEventStep);
        }



        /// <summary>
        /// Création d'un renderer mode fiche
        /// </summary>
        /// <param name="pref">pref utilisateur</param>
        /// <param name="nTab">Table à créer</param>
        /// <param name="showFileProp">Indique si la partie "propriété de la fiche" doît être générée</param>        
        /// <returns></returns>
        public static eAdminFileRenderer CreateAdminFileRenderer(ePref pref, Int32 nTab, Boolean showFileProp = false)
        {


            eAdminFileRenderer adminFileRenderer;
            eAdminTableInfos tabInfo = new eAdminTableInfos(pref, nTab);

            // pour l'instant, on désactive "en dur" la table PAYMENTTRANSACTION, ceci étant temporaire
            if ((tabInfo.TabHiddenProduct || nTab == (int)TableType.PAYMENTTRANSACTION) && pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                throw new EudoAdminInvalidRightException();

            switch (tabInfo.TabType)
            {
                // Table adresse
                case TableType.ADR:
                    adminFileRenderer = new eAdminAddressFileRenderer(pref, tabInfo);
                    break;

                // Table historique
                case TableType.HISTO:
                    adminFileRenderer = new eAdminHistoricFileRenderer(pref, tabInfo);
                    break;

                // Table annexes
                case TableType.PJ:
                    adminFileRenderer = new eAdminPjFileRenderer(pref, tabInfo);
                    break;
                case TableType.TEMPLATE:
                    // Table cible etendues
                    if (tabInfo.IsExtendedTargetSubfile)
                        adminFileRenderer = new eAdminTargetFileRenderer(pref, tabInfo);
                    // Table Mail
                    else if (tabInfo.EdnType == EdnType.FILE_MAIL)
                        adminFileRenderer = new eAdminMailFileRenderer(pref, tabInfo);
                    // Table SMS
                    else if (tabInfo.EdnType == EdnType.FILE_SMS)
                        adminFileRenderer = new eAdminSMSFileRenderer(pref, tabInfo);
                    // Table Plannings
                    else if (tabInfo.EdnType == EdnType.FILE_PLANNING)
                        //#55160  Création/modification onglet planning                
                        adminFileRenderer = new eAdminTemplateFileRenderer(pref, tabInfo);
                    // Table Relations
                    else if (tabInfo.EdnType == EdnType.FILE_RELATION)
                        adminFileRenderer = eAdminRelationFileRenderer.CreateAdminRelationFileRenderer(pref, tabInfo);
                    // Table onglet web
                    else if (tabInfo.EdnType == EdnType.FILE_GRID)
                        adminFileRenderer = eAdminWebTabFileRenderer.CreateAdminWebTabFileRenderer(pref, tabInfo);
                    else
                        // les autres templates
                        adminFileRenderer = new eAdminTemplateFileRenderer(pref, tabInfo);
                    break;
                default:
                    // Les autres tables
                    adminFileRenderer = new eAdminFileRenderer(pref, tabInfo);
                    break;
            }

            adminFileRenderer._ormInfos = eLibTools.OrmLoadAndGetMapAdv(pref, new OrmGetParams() { ExceptionMode = OrmMappingExceptionMode.SAFE });
            adminFileRenderer.ShowFileProp = showFileProp; // Bloc système ouvert
            adminFileRenderer.Generate();
            if (adminFileRenderer.ErrorMsg.Length > 0 || adminFileRenderer.InnerException != null)
            {
                if (adminFileRenderer.InnerException is EudoInternalException)
                {
                    //EudoInternalException : A priori, vient des couches rendu/métier. Comporte un errorContainer déjà formée, a renvoyer tel quel

                    throw adminFileRenderer.InnerException;
                }
                else if (adminFileRenderer.InnerException is EudoException)
                {
                    //EudoException : A priori, vient d'EudoQuery. A encapsuler

                    throw EudoInternalException.GetEudoInternalException(
                        sTitle: eResApp.GetRes(pref.User.UserLangServerId, 72),
                        sShortUserMessage: "Chargement de l'interface d'aministration impossible",
                        sDetailUserMessage: ((EudoException)adminFileRenderer.InnerException).UserMessage,
                        sDebugError: ((EudoException)adminFileRenderer.InnerException).Message,
                        ex: adminFileRenderer.InnerException
                        );

                }
                else
                {
                    //Exception inconnues

                    throw EudoInternalException.GetEudoInternalException(
                    sTitle: eResApp.GetRes(pref.User.UserLangServerId, 72),
                    sShortUserMessage: "Chargement de l'interface d'aministration impossible",
                    // sDetailUserMessage: ((EudoException)adminFileRenderer.InnerException).UserMessage,
                    sDebugError: adminFileRenderer.ErrorMsg,
                    ex: adminFileRenderer.InnerException
                    );
                }
            }

            if (adminFileRenderer._myFile != null && adminFileRenderer._myFile is eAdminFile && ((eAdminFile)adminFileRenderer._myFile).NoExecDefault)
            {
                Dictionary<int, string> dic = new Dictionary<int, string>();
                foreach (var z in adminFileRenderer._myFile.GetFileFields)
                {
                    if (z.FldInfo.DefaultFormatInSql && !string.IsNullOrEmpty(z.FldInfo.DefaultValue))
                    {
                        dic[z.FldInfo.Descid] = z.FldInfo.DefaultValue;
                    }
                }

                //Affichage message avertissement formules du haut non lancées
                adminFileRenderer.PgContainer.Attributes.Add("warningdefault", "1");
                adminFileRenderer.PgContainer.Attributes.Add("warningdefaultlst", String.Join(";", dic.Keys.ToList()));
            }

            adminFileRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.ADMIN_FILE.GetHashCode().ToString());
            return adminFileRenderer;
        }

        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            if (Pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            try
            {

                _myFile = eAdminFile.CreateAdminFile(Pref, _tabInfos);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eAdminFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
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

                _dicFormulasTriggers = ((eAdminFile)_myFile).DicFormulasTriggers;
                _nbFreeFields = ((eAdminFile)_myFile).NbFreeFields;

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eAdminFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }




        /// <summary>
        /// Construction des objets HTML
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            _backBoneRdr = GetFileBackBone();

            this._pgContainer = _backBoneRdr.PgContainer;
            this._pgContainer.Attributes.Add("edntype", _myFile.ViewMainTable.EdnType.GetHashCode().ToString());
            this._pgContainer.Attributes.Add("tt", ((int)_myFile.ViewMainTable.TabType).ToString());

            _divHidden = new HtmlGenericControl("div");
            _divHidden.ID = String.Concat("hv_", _myFile.ViewMainTable.DescId);
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _pgContainer.Controls.Add(_divHidden);

            //Code d'accès pour mettre à jour la valeur par défaut d'une rubrique
            HtmlInputHidden inputDefaultDsc = new HtmlInputHidden();
            _divHidden.Controls.Add(inputDefaultDsc);
            inputDefaultDsc.ID = "edfvc"; //edfvc = eudonet default value code
            inputDefaultDsc.Value = String.Concat((Int32)eAdminUpdateProperty.CATEGORY.DESC, "|", (Int32)eLibConst.DESC.DEFAULT.GetHashCode());

            List<eFieldRecord> sortedFields = null;
            SortFields(out sortedFields);
            FillContent(sortedFields);

            return true;
        }

        /// <summary>
        /// Procure les champs à afficher
        /// </summary>
        /// <param name="sortedFields"></param>
        protected override void SortFields(out List<eFieldRecord> sortedFields)
        {
            base.SortFields(out sortedFields);
            if (_layoutMode == eFileLayout.Mode.DISPORDER)
                return;

            eFieldRecord myMemoField = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.DescId + (int)EudoQuery.AllField.MEMO_NOTES));

            if (myMemoField != null && myMemoField.RightIsVisible)
            {
                sortedFields.Add(myMemoField);
                //myMemoField.FldInfo.PosDisporder = 1;
                myMemoField.FldInfo.PosColSpan = _myFile.ViewMainTable.ColByLine;

            }
        }

        /// <summary>
        /// Affiche la table en mode admin fiche
        /// </summary>
        /// <returns></returns>
        protected virtual eFileBackBoneRenderer GetFileBackBone()
        {
            return (eFileBackBoneRenderer)eRendererFactory.CreateFileBackBone(_tab, bPopup: false, bDisplayBookmark: true, isBkmFile: false);
        }



        /// <summary>
        /// Génère une table d'entête
        /// </summary>
        /// <param name="bHasButtons"></param>
        protected override System.Web.UI.WebControls.Table GetHeader(Boolean bHasButtons = true)
        {
            return base.GetHeader(true);
        }

        /// <summary>
        /// Adds the parent head sep.
        /// </summary>
        protected override void AddParentHeadSep()
        {
            return;
        }

        /// <summary>
        /// Adds the admin drop web tab.
        /// </summary>
        protected override void AddAdminDropWebTab()
        {
            HtmlGenericControl header = new HtmlGenericControl("header");
            _backBoneRdr.PnFilePart1.Controls.Add(header);

            //Panel pn = new Panel();
            //header.Controls.Add(pn);
            //pn.CssClass = "fieldsDropArea";

            //HtmlGenericControl p = new HtmlGenericControl("p");
            //pn.Controls.Add(p);

            // barre des onglets web
            eAdminWebTabNavBarRenderer subMenuRenderer = GetAdminWebTabNavBar();
            subMenuRenderer.PgContainer.Attributes.Add("gridType", _tabInfos.ActiveTab ? "tab" : "Bkm");
            header.Controls.Add(subMenuRenderer.PgContainer);
        }


        /// <summary>
        /// Gets the admin web tab nav bar.
        /// </summary>
        /// <returns></returns>
        protected virtual eAdminWebTabNavBarRenderer GetAdminWebTabNavBar()
        {
            return eAdminWebTabNavBarRenderer.GetAdminWebTabNavBarRenderer(Pref, _tab, 0);
        }


        /// <summary>
        /// rajoute une icone en debut de ligne permettant de supprimer une ligne vide
        /// </summary>
        /// <param name="myTr"></param>
        /// <param name="y"></param>
        /// <param name="bLineNotEmpty"></param>
        protected override void AddDropRowIcon(TableRow myTr, int y, bool bLineNotEmpty)
        {
            if (bLineNotEmpty)
            {
                return;
            }

            if (_layoutMode != eFileLayout.Mode.COORD)
                return;

            HtmlGenericControl ctrl = new HtmlGenericControl("span");
            ctrl.Attributes.Add("class", "icon-minus-circle");
            ctrl.Attributes.Add("Title", eResApp.GetRes(Pref, 1911));
            ctrl.Attributes.Add("action", "dropRow");
            ctrl.Attributes.Add("onclick", String.Format("nsAdminMoveField.dropRow(event, {0});", y));
            myTr.Cells[0].Controls.AddAt(0, ctrl);
        }


        /// <summary>
        /// Adds the drop fields area.
        /// </summary>
        /// <param name="maTable">Table</param>
        protected override void AddDropFieldsArea(System.Web.UI.WebControls.Table maTable)
        {

            Int32 y = 0;
            if (maTable.Rows.Count > 0)
                y = eLibTools.GetNum(maTable.Rows[maTable.Rows.Count - 1].Attributes["y"]) + 1;

            TableRow trDropArea = new TableRow();
            trDropArea.Attributes.Add("y", y.ToString());
            for (int x = 0; x < _myFile.ViewMainTable.ColByLine; x++)
            {
                _iLastDisporder++;
                TableCell tcDropArea = new TableCell();
                tcDropArea.ColumnSpan = eConst.NB_COL_BY_FIELD;
                trDropArea.Cells.Add(tcDropArea);
                tcDropArea.CssClass = "free";
                tcDropArea.Attributes.Add("edo", _iLastDisporder.ToString());
                tcDropArea.Attributes.Add("cellpos", String.Concat(x, ';', y));
                // AddAddtionnalControlInValueCell(tcDropArea);
            }

            AddInterTr(maTable, y, _myFile.ViewMainTable.ColByLine);

            maTable.Rows.Add(trDropArea);
        }

        /// <summary>
        /// Ajout de la règle graduée pour régler la largeur des champs
        /// </summary>
        protected override void AddDrawingScale()
        {
            #region Champ caché pour la propriété DESC.ListColWidth

            Control c = eAdminFieldBuilder.BuildField(_backBoneRdr.PnFilePart1, AdminFieldType.ADM_TYPE_HIDDEN, "ColWidths", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.COLUMNS.GetHashCode());
            c.ID = "hidListColWidth";
            _backBoneRdr.PnFilePart1.Controls.Add(c);

            #endregion Champ caché pour la propriété DESC.ListColWidth

            #region Grips pour régler la taille des champs

            Panel pGrips = new Panel();
            pGrips.ID = "ruleGrips";
            pGrips.CssClass = "selectedArea";
            _backBoneRdr.PnFilePart1.Controls.Add(pGrips);

            #endregion Grips pour régler la taille des champs

            #region Règle pour régler la taille des champs

            System.Web.UI.WebControls.Table myTable = new System.Web.UI.WebControls.Table();
            _backBoneRdr.PnFilePart1.Controls.Add(myTable);
            myTable.ID = "adminFieldsContent";
            myTable.CssClass = "mTabFile selectedArea";

            eMasterFileRenderer.AddEmptyCellsInHead(_myFile, myTable);
            myTable.Rows[0].CssClass = "rowForStyle emptyrow";
            myTable.Rows[0].Style.Value = "";

            Int32 nbCols = _myFile.ViewMainTable.ColByLine * eConst.NB_COL_BY_FIELD;

            TableRow tr = new TableRow();
            myTable.Rows.Add(tr);
            tr.ID = "ruleRow";

            String[] aColumnsWidth = _myFile.ViewMainTable.ColumnsDisplay.Split(',');

            int indexCol = 0;

            for (int i = 0; i < nbCols; i++)
            {
                TableCell tc = new TableCell();
                tr.Cells.Add(tc);

                switch (i % eConst.NB_COL_BY_FIELD)
                {
                    case 0:
                        //CNA - Retire la class "table_labels" pour la règle de la taille des champs car cela interfère avec des listener javascript et cause une erreur
                        //tc.CssClass = "table_labels";
                        break;

                    case 1:
                        tc.CssClass = "table_values";
                        break;

                    case 2:
                        tc.CssClass = "btn";
                        break;

                    default:
                        break;
                }

                if (i % eConst.NB_COL_BY_FIELD != 2 && i < aColumnsWidth.Length)
                {
                    HtmlGenericControl span = new HtmlGenericControl();
                    tc.Controls.Add(span);
                    span.Attributes.Add("class", "icon-caret-down");

                    HtmlGenericControl spanColWidth = new HtmlGenericControl();

                    if (aColumnsWidth[i] == "A")
                    {
                        spanColWidth.InnerText = eResApp.GetRes(Pref, 772);
                        spanColWidth.Attributes.Add("title", eResApp.GetRes(Pref, 7284));
                        spanColWidth.Attributes.Add("class", "spanColAuto");

                        HtmlGenericControl iconRefresh = new HtmlGenericControl();
                        iconRefresh.Attributes.Add("class", "icon-refresh");
                        spanColWidth.Controls.Add(iconRefresh);
                    }
                    else
                    {
                        spanColWidth.InnerText = String.Concat(aColumnsWidth[i], "px");
                        spanColWidth.ID = "spanColWidth" + indexCol;
                        spanColWidth.Attributes.Add("class", "spanColWidth");
                        spanColWidth.Attributes.Add("data-col", indexCol.ToString());
                        // Largeur saisissable
                        TextBox txtColWidth = new TextBox();
                        txtColWidth.Attributes.Add("data-col", indexCol.ToString());
                        txtColWidth.ID = "txtColWidth" + indexCol;
                        txtColWidth.CssClass = "txtColWidth hidden";
                        txtColWidth.Text = aColumnsWidth[i];
                        tc.Controls.Add(txtColWidth);
                    }

                    tc.Controls.Add(spanColWidth);

                    indexCol++;
                }
            }

            #endregion Règle pour régler la taille des champs

        }

        /// <summary>
        /// Adds the additional attributes.
        /// </summary>
        /// <param name="liTC">Cells list</param>
        /// <param name="coord">Point</param>
        /// <param name="iDisporder">Disporder</param>
        protected override void AddAdditionalAttributes(List<TableCell> liTC, Point coord, Int32 iDisporder)
        {
            TableCell myLabel = liTC[0];

            myLabel.Attributes.Add("edo", iDisporder.ToString());
            _iLastDisporder = iDisporder;

            foreach (TableCell tc in liTC)
            {
                tc.Attributes.Add("cellpos", String.Concat(coord.X, ";", coord.Y));
            }
        }

        /// <summary>
        /// Ajout de contrôles dans la cellule du libellé
        /// </summary>
        /// <param name="labelCell"></param>
        /// <param name="field"></param>
        protected override void AddAddtionnalControlInLabelCell(TableCell labelCell, Field field = null)
        {
            if (field != null)
            {
                // Pour les champs autres que cases à cocher, l'étoile se trouve après le libellé et avant le contrôle
                if (field.Format != FieldFormat.TYP_BIT)
                {
                    AddFormulaAsterisks(labelCell, field);
                }
            }
        }

        /// <summary>
        /// Adds the addtionnal control in value cell.
        /// </summary>
        /// <param name="myValueCell">My value cell.</param>
        /// <param name="field">The field.</param>
        protected override void AddAddtionnalControlInValueCell(TableCell myValueCell, Field field = null)
        {
            Boolean bSysField = IsSysField(field);
            AddAddtionnalControlInValueCell(myValueCell, isPropertyField: bSysField, field: field);

            if (field != null)
            {
                // Pour les cases à cocher, l'étoile doit se trouver après le libellé et le contrôle
                if (field.Format == FieldFormat.TYP_BIT)
                {
                    AddFormulaAsterisks(myValueCell, field);
                }
            }
        }

        private static Boolean IsSysField(Field field)
        {
            if (field == null)
                return false;

            try
            {
                AllField sysField = (AllField)(field.Descid % 100);
                switch (sysField)
                {
                    case AllField.GEOGRAPHY:
                    case AllField.DATE_CREATE:
                    case AllField.DATE_MODIFY:
                    case AllField.USER_CREATE:
                    case AllField.USER_MODIFY:
                    case AllField.MULTI_OWNER:
                    case AllField.TPL_MULTI_OWNER:
                    case AllField.MEMO_NOTES:
                        return true;
                    default:
                        return false;

                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Permet d'ajouter des controles html suplémentaire dans la cellue contenant les valeur des champs en mode fiche
        /// </summary>
        /// <param name="myValueCell"></param>
        /// <param name="isPropertyField"></param>
        /// <param name="field"></param>
        protected void AddAddtionnalControlInValueCell(TableCell myValueCell, bool isPropertyField, Field field = null)
        {
            Int32 fieldDescid = field?.Descid ?? 0;

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            myValueCell.Controls.Add(ul);
            ul.Attributes.Add("class", "fieldOptions");
            ul.Attributes.Add("did", fieldDescid.ToString());

            HtmlGenericControl li;
            if (field != null)
            {
                li = new HtmlGenericControl("li");
                li.ID = "buttonBold";
                ul.Controls.Add(li);
                li.Attributes.Add("class", "buttonBold " + (field.StyleBold ? "active" : ""));
                li.Attributes.Add("did", fieldDescid.ToString());
                li.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.BOLD.GetHashCode()));
                li.Attributes.Add("onmousedown", "nsAdmin.updateLabelFormat(event, this);");
                li.Attributes.Add("dbvalue", field.StyleBold ? "1" : "0");
                li.InnerText = eResApp.GetRes(Pref, 7606);

                li = new HtmlGenericControl("li");
                li.ID = "buttonItalic";
                ul.Controls.Add(li);
                li.Attributes.Add("class", "buttonItalic " + (field.StyleItalic ? "active" : ""));
                li.Attributes.Add("did", fieldDescid.ToString());
                li.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.ITALIC.GetHashCode()));
                li.Attributes.Add("onmousedown", "nsAdmin.updateLabelFormat(event, this);");
                li.Attributes.Add("dbvalue", field.StyleItalic ? "1" : "0");
                li.InnerText = eResApp.GetRes(Pref, 7607);

                li = new HtmlGenericControl("li");
                li.ID = "buttonUnderline";
                ul.Controls.Add(li);
                li.Attributes.Add("class", "buttonUnderline " + (field.StyleUnderline ? "active" : ""));
                li.Attributes.Add("did", fieldDescid.ToString());
                li.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.UNDERLINE.GetHashCode()));
                li.Attributes.Add("onmousedown", "nsAdmin.updateLabelFormat(event, this);");
                li.Attributes.Add("dbvalue", field.StyleUnderline ? "1" : "0");
                li.InnerText = eResApp.GetRes(Pref, 7608);

                li = new HtmlGenericControl("li");
                ul.Controls.Add(li);
                li.Attributes.Add("class", "icon-cog configOption");
                li.Attributes.Add("title", String.Concat(eResApp.GetRes(Pref, 7817), Environment.NewLine, GetFormulaTooltip(field)));

                /*
                #51 570 : il était initialement proposé de déclencher l'ouverture de la fenêtre d'édition des automatismes au clic sur l'icône Engrenage
                (uniquement si l'utilisateur est super-admin et qu'un automatisme est présent) mais cela rentrait en conflit avec sa fonction d'affichage des
                propriétés de la rubrique sur le bandeau droit. Cette fonctionnalité a donc été désactivée.
                if (Pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode() && HasAutomatism(field))
                {
                 //   li.Attributes.Add("onclick", String.Concat("nsAdmin.confAdvancedAutomatisms(", field.Descid, ");"));
                    li.Attributes.Add("did", field.Descid.ToString());
                }
                */
                if (!isPropertyField && IsAdminAllowed(field))
                    RenderDeleteOption(ul, field);
            }
            else
            {
                RenderDeleteOption(ul, null);
            }



        }

        /// <summary>
        /// Fait un rendu de la corbeil pour suppression du champ
        /// Les champs specifiques aux table système : mail, cible etendu, .. ne sont pas supprimable
        /// du coup ce fonction est redifinit dans le renderer adéquat
        /// </summary>
        /// <param name="ul">The ul.</param>
        /// <param name="field">The field.</param>
        protected virtual void RenderDeleteOption(HtmlGenericControl ul, Field field)
        {
            if (IsFieldDeletable(field))
            {
                HtmlGenericControl li = new HtmlGenericControl("li");
                ul.Controls.Add(li);
                li.Attributes.Add("class", "deleteOption icon-delete");
            }

        }

        /// <summary>
        /// La rubrique est-elle supprimable ?
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected Boolean IsFieldDeletable(Field field)
        {
            if (field != null && (SpecialFields.Contains(field.Descid) || eAdminFieldInfos.IsSystem(field)))
                return false;
            return true;
        }

        /// <summary>
        /// implémente le champ note (94) dans le cas du mode création
        /// </summary>
        protected override void AddMemoField()
        {
            if (_myFile.ViewMainTable.TabType == TableType.TEMPLATE || _myFile.ViewMainTable.TabType == TableType.ADR)
                base.AddMemoField();

        }

        /// <summary>
        /// Ajoute les astérisques matérialisant le fait qu'un champ soit lié directement ou indirectement à des automatismes
        /// </summary>
        /// <param name="targetCell"></param>
        /// <param name="field"></param>
        private void AddFormulaAsterisks(TableCell targetCell, Field field)
        {
            HtmlGenericControl asterix;
            bool asterisksAdded = false;

            int descid = field.Descid;

            if (HasDirectAutomatism(field))
            {
                asterix = new HtmlGenericControl();
                asterix.Attributes.Add("class", "formulaAst");
                asterix.InnerText = "*";
                asterix.Attributes.Add("title", GetFormulaTooltip(field));
                if (Pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
                {
                    asterix.Attributes.Add("onclick", String.Concat("nsAdmin.confAdvancedAutomatisms(", descid, ");"));
                    asterix.Attributes.Add("did", descid.ToString());
                }
                targetCell.Controls.Add(asterix);
                asterisksAdded = true;
            }


            if (_ormInfos != null && _ormInfos.GetAllMappedDescid.Contains(field.Descid))
            {
                asterix = new HtmlGenericControl();
                asterix.Attributes.Add("class", "formulaAstORM");
                asterix.InnerText = "*";

                asterix.Attributes.Add("title", GetFormulaTooltip(field, false));
                targetCell.Controls.Add(asterix);
                asterisksAdded = true;
            }

            if (HasIndirectAutomatism(descid))
            {
                asterix = new HtmlGenericControl();
                asterix.Attributes.Add("class", "formulaExternalAst");
                asterix.Attributes.Add("title", GetFormulaTooltip(field));
                asterix.InnerText = "*";
                targetCell.Controls.Add(asterix);
                asterisksAdded = true;
            }

            // #51 570 - Si on rajoute des astérisques au libellé, on déplace le libellé à la fin de la cellule de tableau pour que l'ellipsis CSS lui soit appliqué
            // en tenant compte de l'espace occupé par les astérisques. Puis on positionne les astérisques à droite avec une classe CSS "withAst" appliquée à la
            // cellule parente qui les repositionnera après le libellé via un float: right.
            // Cette technique permet d'appliquer l'ellipsis sur le libellé pour empêcher le retour à la ligne, tout en maintenant les astérisques visibles derrière
            // l'ellipsis.
            // Source : http://codepen.io/granteagon/pen/pKFyt
            if (asterisksAdded)
            {
                bool labelsMovedToRight = false;
                for (int ctrlIndex = 0; ctrlIndex < targetCell.Controls.Count; ctrlIndex++)
                {
                    if (targetCell.Controls[ctrlIndex] is LiteralControl)
                    {
                        HtmlGenericControl spanLabelContainer = new HtmlGenericControl("span");
                        spanLabelContainer.Attributes.Add("class", "labelWithAst");
                        spanLabelContainer.InnerHtml = ((LiteralControl)targetCell.Controls[ctrlIndex]).Text; // InnerHtml, car .Text peut contenir des entités HTML de type &#xx;
                        targetCell.Controls.Remove(targetCell.Controls[ctrlIndex]);
                        targetCell.Controls.Add(spanLabelContainer);
                        labelsMovedToRight = true;
                    }
                }

                if (labelsMovedToRight)
                    targetCell.Attributes["class"] = String.Concat(targetCell.Attributes["class"], " withAst");
            }
        }

        /// <summary>
        /// Renvoie la description à afficher en infobulle d'un champ lié à une formule de calcul/un automatisme lors du survol des petites étoiles vertes/bleues ou du
        /// bouton Engrenage
        /// ATTENTION : cette version de la fonction effectuera un appel à field.GetInvolvedFieldsWithAutomatisms() pour récupérer les informations des automatismes liés.
        /// Par souci de performances, ne pas l'appeler plusieurs fois avec ce paramètre, mais faire un appel séparé à field.GetInvolvedFieldsWithAutomatisms(), puis
        /// appeler GetFormulaTooltip avec la liste retournée en paramètre.
        /// </summary>
        /// <param name="field">Champ concerné</param>
        /// <param name="eDal">Objet eDAL pour effectuer la connexion à la base, nécessaire pour la recherche des automatismes liés via field.GetInvolvedFieldsWithAutomatisms</param>
        /// <param name="lang">Identifiant de langue (sous la forme LANG_XX) à utiliser pour la récupération des informations via field.GetInvolvedFieldsWithAutomatisms</param>
        /// <returns>La description, sous la forme donnée en exemple</returns>
        /// <example>
        /// - La référence : « Réf : Res.Lang_00 Onglet.Res.Lang_00 Rubrique (DescID Rubrique)» (Exemple : "Réf : EVENT_32.EVT44 (4244)")
        /// - Une ligne pour chaque automatisme avancé présent sur cette rubrique : « Un automatisme … est présent sur cette rubrique »
        ///   où "…" = "Valeur par défaut" OU "Avant enregistrement" OU "Après enregistrement"
        /// - Une ligne pour rappeler les rubriques ayant un automatisme qui utilisent la présente rubrique : « Cette rubrique est utilisée par les automatismes des rubriques suivantes: » suivi de la liste paginée des rubriques sous la forme « Onglet.Rubrique (référence) »
        /// </example>
        public string GetFormulaTooltip(Field field, eudoDAL eDal, String lang = "LANG_00")
        {

            //SPH : NE PAS UTILISER GetInvolvedFieldsWithAutomatisms
            return "";
            //return GetFormulaTooltip(field, field.GetInvolvedFieldsWithAutomatisms(eDal, field, lang));
        }


        /// <summary>
        /// Ajoute la cellule contenant le bouton
        /// Désactive l'affichage en cas d'automatisme par défaut
        /// </summary>
        /// <param name="cellValue">The cell value.</param>
        /// <param name="bDisplayBtn">if set to <c>true</c> [display BTN].</param>
        /// <param name="sIcon">The icon.</param>
        /// <param name="sIconColor">Color of the icon.</param>
        /// <param name="field">Objet field pour information complémentaire</param>
        public override TableCell GetButtonCell(TableCell cellValue, bool bDisplayBtn, string sIcon = "", string sIconColor = "", Field field = null)
        {
            TableCell t = base.GetButtonCell(cellValue, bDisplayBtn, sIcon, sIconColor, field);
            if (field != null && HasDefaultValueAutomatism(field))
            {
                // t.ToolTip = eResApp.GetRes (Pref, 497);// "Un automatisme par défaut bloque l'utilisation d'une valeur par défaut standard";
                t.ToolTip = "";
                t.CssClass = " icon-cross icnFileBtn icon-is-disabled";
                t.Attributes["eaction"] = "";
                t.Attributes.Add("icodisabled", "1");

                t.Attributes.Add("onmouseover", String.Concat("st(event, ' ", eResApp.GetRes(Pref, 497).Replace("'", "&quot;"), "' , 'divTTipcat');"));
                t.Attributes.Add("onmouseout", "ht();");
            }

            return t;
        }


        /// <summary>
        /// Création d'une cellule de tableau pour la valeur d'un champ
        /// </summary>
        /// <param name="row">Ligne d'enregistrement</param>
        /// <param name="lstFieldRecord">Liste de rubrique, rubrique 1 : Rubrique de l'enregistrement et les suivantes juste pour compléter</param>
        /// <param name="idx">Index de la colonne</param>
        /// <param name="Pref">Préférence de l'utilisateur</param>
        /// <param name="themePaths">Gestion de dossier du theme</param>
        /// <param name="colMaxValues">Collection des valeurs max de la colonne</param>
        /// <param name="nbCol">nombre de colonnes qu'occupera la cellule</param>
        /// <param name="nbRow">nombre de lignes qu'occupera la cellule</param>    
        protected override WebControl GetFieldValueCell(eRecord row, List<eFieldRecord> lstFieldRecord, int idx, ePrefLite Pref, ePref.CalculateDynamicTheme themePaths, ExtendedDictionary<string, ListColMaxValues> colMaxValues = null, int nbCol = 1, int nbRow = 1)
        {
            Field f = lstFieldRecord[0].FldInfo;
            lstFieldRecord[0].RightIsUpdatable = lstFieldRecord[0].RightIsUpdatable && lstFieldRecord?.Count > 0 && !HasDefaultValueAutomatism(f);

            WebControl wc = base.GetFieldValueCell(row, lstFieldRecord, idx, Pref, themePaths, colMaxValues, nbCol, nbRow);

            if (lstFieldRecord?.Count > 0 && HasDefaultValueAutomatism(f))
            {

                //wc.ToolTip = eResApp.GetRes(Pref, 497);// "Un automatisme par défaut bloque l'utilisation d'une valeur par défaut standard";
                wc.ToolTip = "";
                wc.Attributes["eaction"] = "";
                wc.Attributes["disabled"] = "1";

                wc.Attributes.Add("onmouseover", String.Concat("st(event, '", eResApp.GetRes(Pref, 497).Replace("'", "&quot;"), "' , 'divTTipcat');"));
                wc.Attributes.Add("onmouseout", "ht();");
            }

            return wc;
        }

        /// <summary>
        /// Renvoie la description à afficher en infobulle d'un champ lié à une formule de calcul/un automatisme lors du survol des petites étoiles vertes/bleues ou du
        /// bouton Engrenage
        /// </summary>
        /// <param name="field">Champ concerné</param>
        /// <param name="showIndirectAutomatisms">if set to <c>true</c> [show indirect automatisms].</param>
        /// <returns>
        /// La description, sous la forme donnée en exemple
        /// </returns>
        /// <example>
        /// - La référence : « Réf : Res.Lang_00 Onglet.Res.Lang_00 Rubrique (DescID Rubrique) » (Exemple : "Réf : EVENT_32.EVT44 (4244)")
        /// - Une ligne pour chaque automatisme avancé présent sur cette rubrique : « Un automatisme … est présent sur cette rubrique »
        /// où "…" = "Valeur par défaut" OU "Avant enregistrement" OU "Après enregistrement"
        /// - Une ligne pour rappeler les rubriques ayant un automatisme qui utilisent la présente rubrique : « Cette rubrique est utilisée par les automatismes des rubriques suivantes : » suivi de la liste paginée des rubriques sous la forme « Onglet.Rubrique (référence) »
        /// </example>
        public string GetFormulaTooltip(Field field, bool showIndirectAutomatisms = true)
        {
            StringBuilder sb = new StringBuilder();

            int descid = field.Descid;

            // Description de la rubrique
            sb.Append(eResApp.GetResWithColon(Pref, 7614))
              .Append(" ").Append(field.Table.TabName)
              .Append(".").Append(field.RealName)
              .Append(" (")
              .Append(descid).Append(")")
              .Append(Environment.NewLine);

            // Présence d'automatismes directs
            string hasAutomatismRes = eResApp.GetRes(Pref, 7648); // Un automatisme … est présent sur cette rubrique
            if (field.HasMidFormula)
                sb.Append(hasAutomatismRes.Replace("<TYPE>", eResApp.GetRes(Pref, 7650))).Append(Environment.NewLine); // Avant enregistrement

            if (!String.IsNullOrEmpty(field.Formula))
                sb.Append(hasAutomatismRes.Replace("<TYPE>", eResApp.GetRes(Pref, 7651))).Append(Environment.NewLine); // Après enregistrement

            if (HasDefaultValueAutomatism(field))
                sb.Append(hasAutomatismRes.Replace("<TYPE>", eResApp.GetRes(Pref, 528))).Append(Environment.NewLine); // Valeur par défaut

            // Description des automatismes indirects
            if (showIndirectAutomatisms && _dicFormulasTriggers.ContainsKey(descid))
            {
                sb.Append(eResApp.GetRes(Pref, 7649)).Append(Environment.NewLine); // "Cette rubrique est utilisée par les automatismes des rubriques suivantes :"
                foreach (eAdminTriggerField f in _dicFormulasTriggers[descid])
                {
                    sb.Append("- ").Append(f.ToString())
                      .Append(" (").Append(f.Descid).Append(")")
                      .Append(Environment.NewLine);
                }
            }


            if (_ormInfos != null && _ormInfos.GetAllMappedDescid.Contains(field.Descid))
            {
                sb.Append(eResApp.GetRes(Pref, 8222)).Append(Environment.NewLine);
            }


            return sb.ToString();
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            Boolean bReturn = base.End();

            if (!bReturn)
                return false;

            List<Field> lstMemo = _myFile.FldFieldsInfos.FindAll(delegate (Field f) { return f.Format.Equals(FieldFormat.TYP_MEMO); });
            foreach (Field field in lstMemo)
            {
                //if (field.Format != FieldFormat.TYP_MEMO)
                //    continue;

                String colName = eTools.GetFieldValueCellName(_myFile.CalledTabDescId, field.Alias);
                String shortColname = colName.Replace("COL_", "");
                HtmlInputHidden inptMemoCss = new HtmlInputHidden();
                inptMemoCss.ID = String.Concat("divct_", shortColname);
                inptMemoCss.Attributes.Add("etype", "css");
                inptMemoCss.Attributes.Add("ecssname", String.Concat("divct_", shortColname));
                inptMemoCss.Attributes.Add("ecssclass", "width:100%;");
                _divHidden.Controls.Add(inptMemoCss);
            }

            return true;
        }



        /// <summary>
        /// propriété de la fiche
        /// </summary>
        protected override void AddFilePropertiesBlock()
        {

            //SPH - voir demande #57987
            // en attendant une refonte plus globale,
            // on ouvre une transaction unique le temps du traitement pour ne pas ouvrir
            //  2 fois le nombre connexions qu'il y a de chaù^s dans la fiche (cf GetInvolvedFieldsWithAutomatisms & AddFormulaAsterisks )
            // TODO : Il faudrait : ne pas avoir d'ouverture de connexion dans un renderer et pas d'ouverture de connexion dans une boucle.
            eudoDAL eDal = eLibTools.GetEudoDAL(Pref);
            eDal.OpenDatabase();


            string sError = "";
            try
            {

                eDal.StartTransaction(out sError);

                if (sError.Length > 0)
                    throw eDal.InnerException ?? new Exception(sError);

                Pref.SetTransDal(eDal);



                System.Web.UI.WebControls.Table tableProp = new System.Web.UI.WebControls.Table();
                tableProp.ID = "tabAdminFileProp";
                tableProp.Attributes.Add("data-active", ShowFileProp ? "1" : "0"); // Visible ou pas

                TableRow tr = new TableRow();

                List<int> listFields = new List<int>();

                // 1ère ligne
                listFields = new List<int>();
                listFields.Add(_myFile.ViewMainTable.GetOwnerDescId());
                listFields.Add(_myFile.ViewMainTable.DescId + AllField.DATE_CREATE.GetHashCode());
                listFields.Add(_myFile.ViewMainTable.DescId + AllField.DATE_MODIFY.GetHashCode());
                GetFilePropertyCell(tr, listFields, 0, true);
                tableProp.Controls.Add(tr);
                // 2ème ligne
                tr = new TableRow();
                listFields = new List<int>();
                listFields.Add(_myFile.ViewMainTable.GetMultiOwnerDescId());
                listFields.Add(_myFile.ViewMainTable.DescId + AllField.USER_CREATE.GetHashCode());
                listFields.Add(_myFile.ViewMainTable.DescId + AllField.USER_MODIFY.GetHashCode());
                GetFilePropertyCell(tr, listFields, 1, true);
                tableProp.Controls.Add(tr);
                // 3ème ligne
                tr = new TableRow();
                listFields = new List<int>();
                listFields.Add(_myFile.ViewMainTable.DescId + AllField.CONFIDENTIAL.GetHashCode());
                if (_myFile.ViewMainTable.TabType != TableType.TEMPLATE)
                    listFields.Add(_myFile.ViewMainTable.DescId + AllField.MEMO_INFOS.GetHashCode());
                else
                    listFields.Add(0);

                listFields.Add(_myFile.ViewMainTable.DescId + AllField.GEOGRAPHY.GetHashCode());

                GetFilePropertyCell(tr, listFields, 2, false);
                tableProp.Controls.Add(tr);

                //4eme ligne : adresse personnelle
                if (_myFile.ViewMainTable.TabType == TableType.ADR)
                {
                    tr = new TableRow();
                    listFields = new List<int>();
                    listFields.Add((int)AdrField.PERSO);
                    GetFilePropertyCell(tr, listFields, 3, false);
                    tableProp.Controls.Add(tr);

                }

                // 4ème ligne
                //TODO déplacer vers la classe fille
                if (_myFile.ViewMainTable.EdnType == EdnType.FILE_PLANNING)
                {
                    tr = new TableRow();
                    listFields = new List<int>();
                    listFields.Add(_myFile.ViewMainTable.DescId + PlanningField.DESCID_CALENDAR_COLOR.GetHashCode());
                    listFields.Add(0);
                    listFields.Add(0);
                    GetFilePropertyCell(tr, listFields, 3);
                    tableProp.Controls.Add(tr);
                }

                _backBoneRdr.PnFilePart1.Controls.Add(tableProp);
            }
            finally
            {
                try
                {
                    bool commitSuccess = false;


                    try
                    {
                        eDal.CommitTransaction(out sError);
                        commitSuccess = sError.Length == 0;
                    }
                    finally
                    {
                        if (!commitSuccess)
                            eDal.RollBackTransaction(out sError);
                    }


                }
                finally
                {
                    eDal.CloseDatabase();
                }
            }

        }

        /// <summary>
        /// Propriétés de la fiche
        /// </summary>
        /// <param name="tcFileOwner"></param>
        protected override void Get99TableCell(TableCell tcFileOwner)
        {
            tcFileOwner.Controls.Add(GetFileLogo());

            HtmlGenericControl spanFile99 = new HtmlGenericControl();
            spanFile99.Attributes.Add("class", "file99Label");
            spanFile99.InnerText = eResApp.GetRes(Pref, 6915);

            HtmlGenericControl spanFileInfo = new HtmlGenericControl();
            spanFileInfo.ID = "ownerTitle";
            spanFileInfo.Attributes.Add("class", "fileOwner");
            spanFileInfo.Controls.Add(spanFile99);

            tcFileOwner.Controls.Add(spanFileInfo);
        }

        /// <summary>
        /// Gets the properties fields.
        /// </summary>
        /// <param name="PtyFieldsDescId">Descid list</param>
        /// <returns></returns>
        protected override List<int> GetPropertiesFields(ref List<int> PtyFieldsDescId)
        {
            if (PtyFieldsDescId == null)
                PtyFieldsDescId = new List<Int32>();

            // Appartient à
            PtyFieldsDescId.Add(_myFile.ViewMainTable.GetOwnerDescId());

            // <fiche> de : - copropriétaires - appartient à multiple
            Int32 multiOwnerDescId = _myFile.ViewMainTable.GetMultiOwnerDescId();
            if (multiOwnerDescId != 0)
                PtyFieldsDescId.Add(multiOwnerDescId);

            // Confidentielle
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.CONFIDENTIAL.GetHashCode());

            // Créé par, Créé le
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.DATE_CREATE.GetHashCode());
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.USER_CREATE.GetHashCode());

            // Modifié le, Modifié par
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.DATE_MODIFY.GetHashCode());
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.USER_MODIFY.GetHashCode());

            // Géolocalisation
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.GEOGRAPHY.GetHashCode());

            return PtyFieldsDescId;
        }



        private void GetFilePropertyCell(TableRow tr, List<Int32> fieldsDescid, int trPos, bool readOnly = false)
        {
            //TableCell tc = new TableCell();
            //tc.CssClass = "table_labels";
            List<Field> fields = _myFile.FldFieldsInfos;

            TableCell tcLabel, tcValue, tcButton;

            int tcPos = 0;
            foreach (Int32 fldDescId in fieldsDescid)
            {
                tcLabel = new TableCell();
                tcValue = new TableCell();
                tcButton = new TableCell();
                tcButton.CssClass = "icnFileBtn";

                Field field = fields.Find(f => f.Descid == fldDescId);

                if (field != null)
                {
                    eFieldRecord fld = _myFile.Record.GetFieldByAlias(field.Alias);

                    AllField sysfld = (AllField)(field.Descid % 100);
                    GetFieldLabelCell(tcLabel, _myFile.Record, fld);
                    tcLabel.Attributes.Add("sys", "1");

                    switch (sysfld)
                    {
                        case AllField.DATE_CREATE:
                        case AllField.DATE_MODIFY:
                        case AllField.USER_CREATE:
                        case AllField.USER_MODIFY:
                        case AllField.MULTI_OWNER:
                        case AllField.TPL_MULTI_OWNER:
                            tcValue = (TableCell)GetFieldValueCell(_myFile.Record, fld, 0, Pref);
                            break;

                        default:

                            tcValue = (TableCell)GetFieldValueCell(_myFile.Record, fld, 0, Pref);
                            if (!HasDefaultValueAutomatism(field))
                                tcButton = GetButtonCell(tcValue, true);

                            break;
                    }

                    AddAddtionnalControlInValueCell(tcValue, true, field);
                }
                if (readOnly && tcValue.Controls.Count > 0)
                    ((WebControl)tcValue.Controls[0]).Attributes.Add("readonly", "readonly");

                tcValue.CssClass += " table_values";
                tcLabel.CssClass += " table_labels";

                tr.Cells.Add(tcLabel);
                tr.Cells.Add(tcValue);
                tr.Cells.Add(tcButton);

                string cellPos = String.Concat("prop", ";", tcPos, ";", trPos);
                tcValue.Attributes.Add("cellpos", cellPos);
                tcLabel.Attributes.Add("cellpos", cellPos);

                ++tcPos;
            }
        }

        /// <summary>
        /// rend le block HTML de tous les signets dans une méthode overridable
        /// </summary>
        /// <returns></returns>
        protected override void GetBookMarkBlock()
        {
            base.GetBookMarkBlock();

            AddDragAndDropAttributes();

        }
        /// <summary>
        /// rajoute les attributs permettant le drag n drop
        /// </summary>
        protected virtual void AddDragAndDropAttributes()
        {
            string sDraggable = "true";

            if (_tabInfos.EudonetXIrisBlackStatus == EUDONETX_IRIS_BLACK_STATUS.ENABLED)
                sDraggable = "false";

           _backBoneRdr.PnBkmBar.Attributes.Add("draggable", sDraggable);
            _backBoneRdr.PnBkmBar.Attributes.Add("ondragstart", "nsAdminMoveBkmBar.onDragStart(event);");
            _backBoneRdr.PnBkmBar.Attributes.Add("ondrag", "nsAdminMoveBkmBar.onDrag(event);");
            _backBoneRdr.PnBkmBar.Attributes.Add("ondragend", "nsAdminMoveBkmBar.onDragEnd(event);");
        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected override void GetRawMemoControl(EdnWebControl ednWebCtrl, String sValue)
        {
            GetHTMLMemoControl(ednWebCtrl, sValue);
        }
        /// <summary>
        /// Prépare un tableau contenant les rubriques à afficher en signet et y deverse les lignes du tableau principal qui sont concernées
        /// </summary>
        /// <param name="fileTabBody"></param>
        /// <param name="nbColByLine"></param>
        /// <param name="nBreakLine"></param>
        protected override System.Web.UI.WebControls.Table SetHtmlTabInBkm(System.Web.UI.WebControls.Table fileTabBody, Int32 nbColByLine, Int32 nBreakLine)
        {
            if (_tabInfos.EudonetXIrisBlackStatus == EUDONETX_IRIS_BLACK_STATUS.ENABLED)
                nBreakLine = 0;

            if (_myFile.ViewMainTable.TabType == TableType.TEMPLATE)
                return new System.Web.UI.WebControls.Table();
            else
                return base.SetHtmlTabInBkm(fileTabBody, nbColByLine, nBreakLine);
        }




        private void GetColorpickerButton()
        {
            TableCell tc = new TableCell();
            tc.Attributes.Add("class", "icon-paint-brush icnFileBtn");
        }

        /// <summary>
        /// Vérifie si un automatisme direct (formule de calcul "du haut/milieu/bas" : Valeur par défaut/Avant enregistrement/Après enregistrement) est présent sur la rubrique
        /// Ne vérifie pas la présence d'automatisme indirect (si la rubrique est utilisée dans l'automatisme d'une autre rubrique)
        /// </summary>
        /// <param name="field">Champ sur lequel effectuer la vérification</param>
        /// <returns>true si un automatisme direct est présent, false sinon</returns>
        private Boolean HasDirectAutomatism(Field field)
        {
            if ((!String.IsNullOrEmpty(field.Formula) || field.HasMidFormula || HasDefaultValueAutomatism(field)))
            {
                return true;
            }
            return false;
        }

        private Boolean HasDefaultValueAutomatism(Field field)
        {
            return field.DefaultFormatInSql && !String.IsNullOrEmpty(field.DefaultValue);
        }

        /// <summary>
        /// Vérifie si un automatisme indirect est présent sur la rubrique (si la rubrique est utilisée dans l'automatisme d'une autre rubrique)
        /// Ne vérifie pas la présence d'automatisme direct (formule de calcul "du haut/milieu/bas" : Valeur par défaut/Avant enregistrement/Après enregistrement sur la rubrique)
        /// </summary>
        /// <param name="descid">Champ sur lequel effectuer la vérification</param>
        /// <returns>true si un automatisme indirect est présent, false sinon</returns>
        private Boolean HasIndirectAutomatism(int descid)
        {
            return _dicFormulasTriggers != null && _dicFormulasTriggers.ContainsKey(descid);
        }

        /// <summary>
        /// Vérifie si un automatisme direct ou indirect est présent sur la rubrique
        /// Automatisme direct : formule de calcul "du haut/milieu/bas" : Valeur par défaut/Avant enregistrement/Après enregistrement sur la rubrique
        /// Automatisme indirect : la rubrique est utilisée dans l'automatisme d'une autre rubrique
        /// </summary>
        /// <param name="field">Champ sur lequel effectuer la vérification</param>
        /// <returns>true si un automatisme direct ou indirect est présent, false sinon</returns>
        private Boolean HasAutomatism(Field field)
        {
            return HasDirectAutomatism(field) || HasIndirectAutomatism(field.Descid);
        }

        /// <summary>
        /// L'utilisateur connecté est-il autorisé à modifier le champ (par rapport à l'administration restreinte) ?
        /// </summary>
        /// <returns></returns>
        protected Boolean IsAdminAllowed(Field field)
        {
            int level = Pref.User.UserLevel;

            if (level < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            if (field.SuperAdminOnly && level < UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ajout de la breakline séparant le résumé
        /// </summary>
        /// <param name="td">Cellule dans laquelle on ajoute l'icône de la breakline de résumé</param>
        protected override void AddResumeBreakLine(System.Web.UI.WebControls.TableCell td)
        {
            if (_numLine > -1)
            {
                Panel div = new Panel();
                div.ID = "resumeBreakline";
                div.Attributes.Add("data-nbline", _numLine.ToString());
                div.Attributes.Add("draggable", "true");
                div.Attributes.Add("ondragstart", "oResume.onDragStart(event)");
                div.Attributes.Add("ondragend", "oResume.onDragEnd(event)");
                div.Attributes.Add("class", "resumeBreakline icon-caret-right");
                div.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESCADV.GetHashCode(), "|", DESCADV_PARAMETER.RESUME_BREAKLINE.GetHashCode()));
                div.ToolTip = eResApp.GetRes(Pref, 8164);
                td.Controls.Add(div);
            }
        }
        /// <summary>
        /// Barre d'outils
        /// </summary>
        /// <returns></returns>
        protected override Panel GetToolBar()
        {
            Panel pnl = new Panel();
            pnl.ID = "nbFreeFields";
            HtmlGenericControl span = new HtmlGenericControl();
            span.InnerText = $"{_nbFreeFields} {eResApp.GetRes(_ePref, 1841)}";
            pnl.Controls.Add(span);
            return pnl;
        }

    }
}