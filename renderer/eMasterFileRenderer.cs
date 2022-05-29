
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{


    /// <summary>
    /// eMasterFileRenderer
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eRenderer" />
    public class eMasterFileRenderer : eRenderer
    {


        /// <summary>
        /// Indique le mode d'affichage demandé de la fiche : par défaut, mode résumé ou non
        /// </summary>
        public enum ResumeMode
        {
            /// <summary>
            /// Par défaut, on prend les pref de l'utilisateur 
            /// </summary>
            Default,
            /// <summary>
            /// Yes, si l'utilisateur demande le mode résumé         
            /// </summary>
            Yes,
            /// <summary>
            /// No, si l'utilisateur souhaite l'affichage normal
            /// </summary>
            No
        }


        #region PROPRIETES

        /// <summary>id de la fiche</summary>
        protected int _nFileId = 0;
        /// <summary>Objet d'accès aux données</summary>
        protected eFile _myFile;

        /// <summary>Squelette HTML</summary>
        protected eFileBackBoneRenderer _backBoneRdr;

        /// <summary>Indique si une partie de la fiche est affichée en dessous de la barre des signets </summary>
        protected bool _bFileTabInBkm = false;

        ///// <summary>Dictionnaire représentant les coordonnées des rubriques en identifiant ces rubriques par leur alias</summary>
        //protected IDictionary<string, Point> _diCoordByFldAlias = new Dictionary<string, Point>();

        /// <summary>Dictionnaire de champs ordonnés par coordonnées "x,y"</summary>
        protected IDictionary<Point, eFieldRecord> _diFieldsByCoord = new Dictionary<Point, eFieldRecord>();

        /// <summary>Liste des positions exprimées sous la forme "x,y" indisponibles du tableau, ainsi que leur champ associé</summary>
        protected IDictionary<Point, eFieldRecord> _liUnavailableEmplacement = new Dictionary<Point, eFieldRecord>();

        /// <summary>dernier disporder utilisé</summary>
        protected int _iLastDisporder = 0;

        /// <summary>
        /// Numéro ligne de résumé
        /// </summary>
        protected int _resumeBreakLine = 1;

        /// <summary>
        /// Numéro de la ligne
        /// </summary>
        protected int _numLine = 1;

        /// <summary>Indique si on s'appuie sur les disporder ou sur les coordonnées</summary>
        protected eFileLayout.Mode _layoutMode = eFileLayout.Mode.DISPORDER;

        /// <summary>indique si la mise en page des tables PP PM ADDRESS ont été  </summary>
        private bool _bIsMigratedTab = false;

        /// <summary>
        /// Indique si la fiche est en mode résumé (champs qui passent dans Details)
        /// </summary>
        protected ResumeMode _modeResume = ResumeMode.Default;
        #endregion

        #region ACCESSEURS

        /// <summary>
        /// Objet d'accès aux données
        /// </summary>
        public eFile File
        {
            get { return _myFile; }
        }

        /// <summary>Squelette HTML</summary>
        public eFileBackBoneRenderer BackBoneRdr
        {
            get { return _backBoneRdr; }
        }

        /// <summary>
        /// Indique si la fiche est en mode résumé (champs qui passent dans Details)
        /// </summary>
        public ResumeMode ModeResume
        {
            get { return _modeResume; }
            set { _modeResume = value; }
        }


        #endregion

        #region METHODES

        /// <summary>
        /// Procure les champs à afficher
        /// </summary>
        /// <param name="sortedFields"></param>
        protected virtual void SortFields(out List<eFieldRecord> sortedFields)
        {

            //Tri la liste par disporder
            sortedFields = eMasterFileRenderer.GetSortedFields(_myFile);
            //SHA : backlog #1 104 : ajout affichage From + Destinataire + Corps SMS dans l'affichage de la campagne SMS en mode popup
            if (_rType != RENDERERTYPE.EditMailing && _rType != RENDERERTYPE.EditSMSMailing && _rType != RENDERERTYPE.SMSFile)
            {
                //Les champs en disporder 0 sont des champs system à ne pas afficher
                // la modification de disporder à la source peut avoir des effets de bords,
                // il a donc été décider de le retirer après construction du tableau
                sortedFields.RemoveAll(
                   delegate (eFieldRecord fld)
                   {
                       return fld.FldInfo.PosDisporder == 0;
                   });

                if (_rType != RENDERERTYPE.EditMail && _rType != RENDERERTYPE.EditSMS)
                {
                    sortedFields.RemoveAll(
                        delegate (eFieldRecord fld)
                        {
                            return fld.FldInfo.Descid % 100 >= 70 && fld.FldInfo.Descid % 100 <= 99 && fld.FldInfo.Descid % 100 != (int)AllField.AVATAR;
                        });

                }

            }

            //Si les coordonnées sont renseignées, on s'appuiera sur celles-ci.
            if (sortedFields.Exists(delegate (eFieldRecord fld) { return (fld.FldInfo.X != null || fld.FldInfo.Y != null); }))
                _layoutMode = eFileLayout.Mode.COORD;


        }

        /// <summary>
        /// Gets the sorted fields.
        /// </summary>
        /// <param name="myFile">eFile</param>
        /// <returns></returns>
        public static List<eFieldRecord> GetSortedFields(eFile myFile)
        {

            //Tri la liste par disporder
            List<eFieldRecord> sortedFields = myFile.GetFileFields;
            sortedFields.Sort(eFieldRecord.CompareByDisporder);
            return sortedFields;

        }

        /// <summary>
        /// renseigne le contenu de la fiche
        /// </summary>
        /// <param name="sortedFields">Liste des champs triés</param>
        protected virtual void FillContent(List<eFieldRecord> sortedFields)
        {
            int nbColByLine = _myFile.ViewMainTable.ColByLine;
            int nBreakLine = GetBreakLine();

            // Affichage du "curseur" permettant de déplacer la breakline de résumé
            bool showResumeBL = false;

            // Breakline de résumé
            eudoDAL eDal = null;
            try
            {
                eDal = eLibTools.GetEudoDAL(Pref);
                eDal.OpenDatabase();

                _resumeBreakLine = eLibTools.GetNum(DescAdvDataSet.LoadAndGetAdvParam(eDal, this._tab, DESCADV_PARAMETER.RESUME_BREAKLINE));
                if (_resumeBreakLine == 0)
                    _resumeBreakLine = 1;

                string sError = "";
                _bIsMigratedTab = eSqlDesc.IsCoordLayoutEnabled(Pref, eDal, _myFile.ViewMainTable.DescId, out sError);

            }
            catch (Exception ex)
            {
                throw new EudoException("Une erreur s'est produite lors de l'initialisation du contenu de la page", "Erreur lors de la récupération de la breakline de résumé ou lors de la vérification du mode de mise en page", ex, true);
            }
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();
            }

            AddAdminDropWebTab();

            _backBoneRdr.PnFilePart1.Controls.Add(GetHeader());

            AddFilePropertiesBlock();

            //Création de l'entête
            System.Web.UI.WebControls.Table fileTabSysFields = new System.Web.UI.WebControls.Table();
            System.Web.UI.WebControls.Table fileTabMain = new System.Web.UI.WebControls.Table();
            System.Web.UI.WebControls.Table fileTabInBkm = new System.Web.UI.WebControls.Table();
            List<eFieldRecord> nonSysFields;

            // Si aucune mode demandé, on prend la pref de l'utilisateur (dans uservalue)
            if (_modeResume == ResumeMode.Default)
            {
                _modeResume = (_myFile.IsGlobalSepClosed) ? ResumeMode.Yes : ResumeMode.No;
            }

            // Au dela de la break line, on affiche le reste du tableau en signet
            if (_modeResume == ResumeMode.Yes)
            {
                if (_resumeBreakLine < nBreakLine)
                    nBreakLine = _resumeBreakLine + 1;
            }

            // Si le champ comporte des coordonnées X et Y
            if (sortedFields.Exists(delegate (eFieldRecord fld)
            {
                return (fld.FldInfo.X != null && fld.FldInfo.Y != null)
                            || (fld.FldInfo.Descid % 100 == (int)AllField.AVATAR && fld.FldInfo.PosDisporder > 0);
            }))
            {
                AddParentHead();

                //Ajout de la règle graduée pour régler la largeur des champs (Admin)
                AddDrawingScale();

                fileTabMain = CreateHtmlTable(sortedFields, nbColByLine, string.Concat("SEP_", _myFile.ViewMainTable.DescId, "_0"), 0, bShowResumeBreakline: true);
                fileTabMain.ID = "ftm_" + _tab.ToString();



                if (nBreakLine > 0 || _tab == TableType.CAMPAIGN.GetHashCode())
                    fileTabInBkm = SetHtmlTabInBkm(fileTabMain, nbColByLine, nBreakLine);

                _backBoneRdr.PnFilePart1.Controls.Add(fileTabMain);


            }
            else if ((_tab == EudoQuery.TableType.PM.GetHashCode() || _tab == EudoQuery.TableType.ADR.GetHashCode()) && !_bIsMigratedTab)
            {
                #region Cas particulier : fiche PM

                //Ajout de la règle graduée pour régler la largeur des champs(Admin)
                AddDrawingScale();

                //Création la partie fixe de la fiche pm
                // il s'agit des NB_HEADER_FIELD_PM (rue1, rue2, etc.) champs de pm  dont la position est fixe
                int rangeCount = eFileLayout.NB_HEADER_FIELD_PM > sortedFields.Count ? sortedFields.Count : eFileLayout.NB_HEADER_FIELD_PM;
                List<eFieldRecord> FixedFields = sortedFields.GetRange(0, rangeCount);

                if (_resumeBreakLine < eFileLayout.NB_LINE_HEAD_PM)
                {
                    showResumeBL = true;
                }

                // Partie "Détails <FILE>"
                // Les 12 premiers champs sont des champs system
                // qui s'affichent sur 4 colonnes
                fileTabSysFields = CreateHtmlTable(FixedFields, eFileLayout.NB_COL_HEAD_PM, string.Concat("SEP_", _myFile.ViewMainTable.DescId, "_0"), 0, true, showResumeBL);
                fileTabSysFields.ID = "fts_" + _tab.ToString();
                _backBoneRdr.PnFilePart1.Controls.Add(fileTabSysFields);

                // Si on a déjà affiché la breakline, on ne l'affiche plus
                if (showResumeBL)
                    showResumeBL = false;
                else
                    showResumeBL = true;


                // Partie "Informations Supplémentaires"
                //Collection des champs 

                nonSysFields = sortedFields.GetRange(rangeCount, sortedFields.Count - rangeCount);

                // au dela de la break line , on affiche le reste du tableau en signet
                if (nBreakLine < eFileLayout.NB_LINE_HEAD_PM)
                    nBreakLine = eFileLayout.NB_LINE_HEAD_PM;


                fileTabMain = CreateHtmlTable(nonSysFields, nbColByLine, string.Concat("SEP_", _myFile.ViewMainTable.DescId, "_1"), iDecrement: rangeCount, bShowResumeBreakline: showResumeBL);
                fileTabMain.ID = "ftm_" + _tab.ToString();

                if (nonSysFields.Count > 0)
                {
                    fileTabInBkm = SetHtmlTabInBkm(fileTabMain, nbColByLine, nBreakLine - eFileLayout.NB_LINE_HEAD_PM);
                }

                _backBoneRdr.PnFilePart1.Controls.Add(fileTabMain);

                #endregion
            }
            else if (_tab == EudoQuery.TableType.PP.GetHashCode() && !_bIsMigratedTab)
            {
                #region Cas particulier : fiche PP

                //Ajout de la règle graduée pour régler la largeur des champs(Admin)
                AddDrawingScale();

                if (_resumeBreakLine < eFileLayout.NB_LINE_HEAD_PP)
                {
                    showResumeBL = true;
                }

                //Collection des champs 
                // Les 5 premiers champs sont des champs system
                // qui s'affichent sur 2 colonnes
                List<eFieldRecord> headerFields = sortedFields.GetRange(0, eFileLayout.NB_HEADER_FIELD_PP);
                fileTabSysFields = CreateHtmlTable(headerFields, eFileLayout.NB_COL_HEAD_PP, string.Empty, 0, true, showResumeBL);
                fileTabSysFields.ID = "fts_" + _tab.ToString();
                _backBoneRdr.PnFilePart1.Controls.Add(fileTabSysFields);

                if (showResumeBL)
                    showResumeBL = false;
                else
                    showResumeBL = true;


                nonSysFields = sortedFields.GetRange(eFileLayout.NB_HEADER_FIELD_PP, sortedFields.Count - eFileLayout.NB_HEADER_FIELD_PP);

                // au dela de la break line , on affiche le reste du tableau en signet
                if (nBreakLine < eFileLayout.NB_LINE_HEAD_PP)
                    nBreakLine = eFileLayout.NB_LINE_HEAD_PP;


                fileTabMain = CreateHtmlTable(nonSysFields, nbColByLine, string.Concat("SEP_", _myFile.ViewMainTable.DescId, "_0"), iDecrement: eFileLayout.NB_HEADER_FIELD_PP, bShowResumeBreakline: showResumeBL);
                fileTabMain.ID = "ftm_" + _tab.ToString();

                if (nonSysFields.Count > 0)
                {
                    fileTabInBkm = SetHtmlTabInBkm(fileTabMain, nbColByLine, nBreakLine - eFileLayout.NB_LINE_HEAD_PP);
                }
                _backBoneRdr.PnFilePart1.Controls.Add(fileTabMain);

                //pgContainer.Controls.Add(pnlDetailsBkms);
                #endregion
            }
            else
            {

                #region Cas Général

                // ajout du cadre contenant les informations parentes
                AddParentHead();

                //Ajout de la règle graduée pour régler la largeur des champs (Admin)
                AddDrawingScale();

                fileTabMain = CreateHtmlTable(sortedFields, nbColByLine, string.Concat("SEP_", _myFile.ViewMainTable.DescId, "_0"), 0, bShowResumeBreakline: true);
                fileTabMain.ID = "ftm_" + _tab.ToString();

                // au dela de la break line, on affiche le reste du tableau en signet
                if (nBreakLine > 0 || _tab == TableType.CAMPAIGN.GetHashCode())
                    fileTabInBkm = SetHtmlTabInBkm(fileTabMain, nbColByLine, nBreakLine);

                _backBoneRdr.PnFilePart1.Controls.Add(fileTabMain);

                #endregion

            }


            #region KHA le 26 mai 2014 Pour PP et PM on rajoute un "avatar" et on le rajoute sur tous les tableaux pour que les champs restent alignés
            if ((_myFile.ViewMainTable.TabType == TableType.PP || _myFile.ViewMainTable.TabType == TableType.PM)
                && _layoutMode == eFileLayout.Mode.DISPORDER)
            {
                bool bCellOnly = false;

                bCellOnly = AddAvatarCellOnTable(fileTabSysFields, bCellOnly) || bCellOnly;
                bCellOnly = AddAvatarCellOnTable(fileTabMain, bCellOnly) || bCellOnly;
                bCellOnly = AddAvatarCellOnTable(fileTabInBkm, bCellOnly) || bCellOnly;
            }

            #endregion

            HtmlInputHidden inptFileTabInBkm = new HtmlInputHidden();
            inptFileTabInBkm.ID = "bftbkm";
            inptFileTabInBkm.Value = _bFileTabInBkm ? "1" : "0";
            _divHidden.Controls.Add(inptFileTabInBkm);

            #region Référentiel Sirene
            // Récupération des informations sur la table (côté CONFIGADV/DESCADV/ = non géré par EudoQuery)
            string[] sireneEnabledFields = eSireneMapping.GetSireneEnabledFields(Pref, _tab);
            if (sireneEnabledFields.Length > 0)
                PgContainer.Attributes.Add("sirenefields", string.Join(";", sireneEnabledFields));
            #endregion

            // Appel de fonction de fin de fill content
            EndFillContent();

            AddDropFieldsArea(fileTabInBkm != null && fileTabInBkm.Rows.Count > 0 ? fileTabInBkm : fileTabMain);

            //PgContainer.Controls.Add(_pnlDetailsBkms);

            #region SIGNETS

            //En mode créa et en affichage sous la forme de popup, 
            //le champ note est affiché dans le corps de la fiche, car il n'y a pas de signet
            AddMemoField();

            GetBookMarkBlock();

            #endregion
        }

        /// <summary>
        /// Adds the admin drop web tab.
        /// </summary>
        protected virtual void AddAdminDropWebTab()
        {
            return;
        }

        /// <summary>
        /// Adds the avatar cell on table.
        /// </summary>
        /// <param name="myTable">My table.</param>
        /// <param name="bCellOnly">if set to <c>true</c> [cell only].</param>
        /// <returns></returns>
        protected virtual bool AddAvatarCellOnTable(System.Web.UI.WebControls.Table myTable, bool bCellOnly)
        {
            return false;
        }

        /// <summary>
        /// Ajout de la règle graduée pour régler la largeur des champs
        /// </summary>
        protected virtual void AddDrawingScale()
        {
            return;
        }

        /// <summary>
        /// Adds the parent head.
        /// </summary>
        protected virtual void AddParentHead()
        {
            if (_myFile.GetFileFields.Exists(delegate (eFieldRecord ef) { return ef.FldInfo.Format == FieldFormat.TYP_ALIASRELATION; }))
                return;

            if (_myFile.GetHeaderFieldsLeft.Count == 0 && _myFile.GetHeaderFieldsRight.Count == 0)
            {
                return;
            }

            eRenderer headRenderer = eRendererFactory.CreateParenttInHeadRenderer(Pref, this);
            Panel pgC = null;
            if (headRenderer.ErrorMsg.Length > 0)
                this._sErrorMsg = headRenderer.ErrorMsg;    //On remonte l'erreur
            if (headRenderer != null)
                pgC = headRenderer.PgContainer;
            _backBoneRdr.PnFilePart1.Controls.Add(headRenderer.PgContainer);




        }

        /// <summary>
        /// Adds the parent head sep.
        /// </summary>
        protected virtual void AddParentHeadSep()
        {
            Panel divSep1 = new Panel();
            divSep1.CssClass = "sub_sep";


            Panel divSep2 = new Panel();
            divSep2.CssClass = "separateur_glbl LNKSEP";
            divSep2.Controls.Add(divSep1);
            _backBoneRdr.PnFilePart1.Controls.Add(divSep2);

        }


        /// <summary>
        /// Créée et ajoute au panel une table html de la liste des fiels passé sur le nombre de colonne passé
        /// </summary>
        /// <param name="lstFields">Liste des fields a créé en table</param>
        /// <param name="nbColByLine">Nombre de champ par ligne du tableau</param>
        /// <param name="sIdSep">Id du séparateur</param>
        /// <param name="iDecrement">Nombre de champs précédemment affichés et à déduire du disporder</param>
        /// <param name="bSystemField">Indique si le tableau est le tableau des champs systèmes</param>
        /// <param name="bShowResumeBreakline">Affichage de la breakline de résumé</param>
        public System.Web.UI.WebControls.Table CreateHtmlTable(List<eFieldRecord> lstFields, int nbColByLine, string sIdSep = "", int iDecrement = 0, bool bSystemField = false, bool bShowResumeBreakline = false)
        {
            //bool bAddSystemSep = (iDecrement == 0 && !_bPopupDisplay);

            // Initialisation de la table 
            System.Web.UI.WebControls.Table maTable = new System.Web.UI.WebControls.Table();

            maTable.CssClass = "mTab mTabFile";
            //  maTable.Attributes.Add("border", "1");

            AddEmptyCellsInHead(_myFile, maTable, nbColByLine, bSystemField);

            int nMaxLine = 0;
            if (_layoutMode == eFileLayout.Mode.COORD)
                // utilisation des coordonnées stockées en base
                nMaxLine = eFileLayout.GetFieldsPositionFromCoord(Pref, lstFields, nbColByLine, ref _liUnavailableEmplacement, ref _diFieldsByCoord);
            else
            {
                // calcul de la position des champs
                _diFieldsByCoord.Clear();
                _liUnavailableEmplacement.Clear();
                nMaxLine = eFileLayout.GetFieldsPositionFromDisporder(Pref, lstFields, iDecrement, nbColByLine, ref _liUnavailableEmplacement, ref _diFieldsByCoord);
            }
            // Ajout des objets HTML tr et td dans maTable
            if (nMaxLine > 0)
                DrawInHTMLTable(nbColByLine, nMaxLine, sIdSep, maTable, iDecrement, bShowResumeBreakline: bShowResumeBreakline);


            return maTable;
        }



        /// <summary>
        /// créée et insère dans maTable les objets DataRow et DataCell nécessaire à l'affichage du mode fiche
        /// </summary>
        /// <param name="nbColByLine">The nb col by line.</param>
        /// <param name="nMaxLine">The n maximum line.</param>
        /// <param name="sIdSep">The s identifier sep.</param>
        /// <param name="maTable">The ma table.</param>
        /// <param name="iLastDisporder">The i last disporder.</param>
        /// <param name="bShowResumeBreakline">if set to <c>true</c> [show resume breakline].</param>
        /// <exception cref="System.Exception"></exception>
        protected void DrawInHTMLTable(int nbColByLine, int nMaxLine, string sIdSep, System.Web.UI.WebControls.Table maTable, int iLastDisporder = 0, bool bShowResumeBreakline = false)
        {
            //Création du cartouche d'entête
            TableRow myTr;
            string sTagPageSep = sIdSep;
            int nIdxLastSep = 0;
            bool bDoNotAddTR = false;



            // demande d'evolution #39838 pouvoir editer la fiche à la création si elle n'est pas rattachée a un PP
            eFieldRecord fldPPRec = new eFieldRecord();
            if (_myFile.ViewMainTable.InterPP)
                fldPPRec = _myFile.Record.GetFieldByAlias(string.Concat(_myFile.ViewMainTable.DescId, "_", TableType.PP.GetHashCode() + 1));


            bool bUseTran = this is eda.eAdminFileRenderer;

            eudoDAL eDal = null;
            string sError = "";
            if (bUseTran)
            {
                eDal = eLibTools.GetEudoDAL(Pref);
                eDal.OpenDatabase();
                eDal.StartTransaction(out sError);

                if (sError.Length > 0)
                    throw eDal.InnerException ?? new Exception(sError);

                Pref.SetTransDal(eDal);

            }

            try
            {

                // Parcours des lignes
                for (int y = 0; y < nMaxLine; y++)
                {

                    myTr = new TableRow();
                    myTr.Attributes.Add("y", y.ToString());

                    bool bHasPageSep = false;
                    bool bHasSep = false;
                    bool bLineNotEmpty = false;
                    bool bRowHasOnlySpace = true;

                    // Parcours des colonnes
                    for (int x = 0; x < nbColByLine; x++)
                    {

                        #region Parcours des lignes

                        Point coord = new Point(x, y);

                        //Emplacement occupé par une cell en rowspan/colspan
                        if (_liUnavailableEmplacement.ContainsKey(coord))
                        {
                            bLineNotEmpty = true;
                            continue;
                        }

                        // MAB - On masque également les lignes vides en mode Modification.
                        // Empêche l'affichage de lignes vides même si cela est voulu explicitement en admin (ex : série de champs en haut + un seul champ tout en bas avec champs libres au milieu)
                        /*

                        */

                        TableCell myLabel = new TableCell();
                        TableCell myValue = new TableCell();
                        TableCell myButton = new TableCell();
                        //Liste des cellules composant le champ
                        List<TableCell> liTcField = new List<TableCell>();

                        if (_diFieldsByCoord.ContainsKey(coord))
                        {
                            eFieldRecord myField = _diFieldsByCoord[coord];



                            FieldFormat myFieldFormat = myField.FldInfo.Format;
                            string myFieldIcon = myField.FldInfo.Icon;
                            string myFieldIconColor = myField.FldInfo.IconColor;
                            if (myFieldFormat == FieldFormat.TYP_ALIAS && myField.FldInfo.AliasSourceField != null)
                            {
                                myFieldFormat = myField.FldInfo.AliasSourceField.Format;
                                myFieldIcon = myField.FldInfo.AliasSourceField.Icon;
                                myFieldIconColor = myField.FldInfo.AliasSourceField.IconColor;
                            }

                            if (myField.FldInfo.Descid % 100 == (int)AllField.MEMO_NOTES)
                            {
                                bDoNotAddTR = false;
                                sTagPageSep = "";
                            }

                            bRowHasOnlySpace = false;
                            iLastDisporder = myField.FldInfo.PosDisporder;

                            SetProspectMatchedFields(myField, fldPPRec);

                            #region remplacement de ADR01 par une liaison vers PM dans le cas d'une adresse pro

                            if (SetAddrProfLink(myField, maTable))
                                continue;

                            #endregion

                            bool bDisplayBtn = _bIsEditRenderer
                                && myFieldFormat != FieldFormat.TYP_CHART // BSE :#54 133
                                && (myField.RightIsUpdatable
                                || myFieldFormat == FieldFormat.TYP_WEB
                                || myFieldFormat == FieldFormat.TYP_PHONE
                                || myFieldFormat == FieldFormat.TYP_EMAIL
                                || myFieldFormat == FieldFormat.TYP_SOCIALNETWORK)
                                && ((myField.FldInfo.PopupDataRend == PopupDataRender.STEP && RendererType == RENDERERTYPE.AdminFile) || myField.FldInfo.PopupDataRend != PopupDataRender.STEP); // Affichage du bouton catalogue pour les catalogues étape seulement en admin

                            if (myField.RightIsVisible)
                                bLineNotEmpty = true; // Si une cellule contient un champ, la ligne est considéré comme pleine

                            myLabel.RowSpan = myField.FldInfo.PosRowSpan;
                            myValue.RowSpan = myField.FldInfo.PosRowSpan;

                            // Style du champ
                            if (myField.FldInfo.FieldStyle != FIELD_DISPLAY_STYLE.NORMAL)
                                myLabel.Attributes.Add("fstyle", myField.FldInfo.FieldStyle.ToString());

                            //(eConst.NB_COL_BY_FIELD - 1) corresponds au nombre des cellules système associées : (label, boutons, etc.)
                            //myValue.ColumnSpan = myField.FldInfo.PosColSpan * eConst.NB_COL_BY_FIELD - (eConst.NB_COL_BY_FIELD - 1);

                            // Si le nb de colonnes du champ dépasse le nb max de colonnes de la table, le champ va prendre le nombre de colonnes restant...
                            //if (myField.FldInfo.PosColSpan + x > nbColByLine)
                            //{
                            //    myField.FldInfo.PosColSpan = myField.FldInfo.PosColSpan - x;
                            //}
                            eTools.SetTableCellColspan(myValue, myField);


                            if (myField.FldInfo.Format == FieldFormat.TYP_TITLE && myField.FldInfo.Length == 1)
                            {
                                bDoNotAddTR = !myField.RightIsVisible;
                            }
                            // dans le cas ou le dernier titre séparateur de page est masqué pour une raison ou pour une autre, on affiche pas les lignes qui suivent
                            // on peut donc passer au tour suivant.
                            if (bDoNotAddTR)
                                continue;

                            if (myField.FldInfo.Format == FieldFormat.TYP_TITLE && myField.RightIsVisible)
                            {
                                #region Affichage Titre Séparateur


                                // On teste si le titre séparateur peut prendre toutes les colonnes requises...
                                int titleColSpan = 0;
                                int posX = x;
                                Point p;
                                for (int j = 1; j <= myField.FldInfo.PosColSpan; j++)
                                {
                                    p = new Point(posX, y);
                                    if (_diFieldsByCoord.ContainsKey(p))
                                    {
                                        if (_diFieldsByCoord[p] != myField)
                                        {
                                            break;
                                        }
                                    }
                                    else if (_liUnavailableEmplacement.ContainsKey(p))
                                    {
                                        if (_liUnavailableEmplacement[p] != myField)
                                            break;
                                    }
                                    titleColSpan++;

                                    posX++;
                                }


                                // titre séparateur 
                                //myLabel.ColumnSpan = myValue.ColumnSpan + (eConst.NB_COL_BY_FIELD - 1);
                                myLabel.ColumnSpan = titleColSpan * eConst.NB_COL_BY_FIELD;

                                //GetFieldLabelCell(myLabel, _myFile.Record, myField);

                                myLabel.Height = myValue.RowSpan * eConst.FILE_LINE_HEIGHT;

                                // Taille de police


                                GetSeparator(myLabel, myField, string.Empty, true);
                                bHasSep = true;
                                if (myField.FldInfo.Length == 1)
                                {
                                    sTagPageSep = string.Concat("SEP_", _myFile.ViewMainTable.DescId, '_', myField.FldInfo.Descid);
                                    myLabel.ID = sTagPageSep;
                                    myLabel.Attributes.Add("efld", "1");
                                    myLabel.Attributes.Add("eaction", "LNKSEP");
                                    myLabel.Attributes.Add("eOpen", "1");

                                    bHasPageSep = true;

                                    nIdxLastSep = maTable.Rows.Count;
                                    myTr.Attributes.Add("hasSep", "1");

                                    ListClosedSep(myField);


                                }
                                myTr.Cells.Add(myLabel);
                                liTcField.Add(myLabel);

                                AddAddtionnalControlInValueCell(myLabel, myField.FldInfo);

                                #endregion
                            }
                            else if (myField.RightIsVisible || (myField.FldInfo.DefaultValue.Length > 0 && myField.FldInfo.Format == FieldFormat.TYP_IFRAME))
                            {
                                #region Cas général

                                GetFieldLabelCell(myLabel, _myFile.Record, myField);
                                TableCell myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, myField, 0, Pref, nbCol: myValue.ColumnSpan, nbRow: myValue.RowSpan);
                                if (myField.FldInfo.Format == FieldFormat.TYP_ALIASRELATION)
                                {
                                    try
                                    {
                                        myLabel.Attributes["eltvalid"] = myValueCell.Controls[0].ID;
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                    }


                                }



                                myValueCell.RowSpan = myValue.RowSpan;
                                myValueCell.ColumnSpan = myValue.ColumnSpan;



                                // Visibilité du libellé
                                bool bLabelHidden = myField.FldInfo.LabelHidden;
                                // Largeur de la valeur
                                bool bMaximizeValue = myField.FldInfo.MaximizeValue;

                                if (myField.FldInfo.Descid == 101075 || (bLabelHidden && bMaximizeValue))
                                {
                                    // yolo
                                    myLabel.Style.Add("display", "none");

                                    myValueCell.Attributes["title"] = myLabel.Attributes["title"];

                                    if (myField.FldInfo.Descid == 101075)
                                        myValueCell.ColumnSpan = (eConst.NB_COL_BY_FIELD - 1);
                                    else
                                        myValueCell.ColumnSpan = eConst.NB_COL_BY_FIELD * myField.FldInfo.PosColSpan - 1;
                                }

                                myTr.Cells.Add(myLabel);
                                myTr.Cells.Add(myValueCell);

                                liTcField.Add(myLabel);
                                liTcField.Add(myValueCell);

                                //Spécifique mail : s'il s'agit du premier champ de type E-mail de la table, on retient son ID
                                //afin d'en récupérer la valeur lors d'une création de fiche E-mail depuis le signet E-mail
                                //if (
                                //    myField.FldInfo.Format == FieldFormat.TYP_EMAIL &&
                                //    _sFstMailFieldId.Length == 0 && // et si le champ "Première adresse e-mail n'a pas déjà été renseigné 
                                //    myValueCell.Controls.Count > 0
                                //)
                                //    _sFstMailFieldId = myValueCell.Controls[0].ID ?? string.Empty;

                                //Spécifique mail : si on traite le champ "Corps", on l'ajoute dans la variable référençant les champs Mémo
                                //afin qu'un eMemoEditor puisse être instancié dessus via un appel à initMemoFields() (JS)
                                // TOCHECK SMS : normalement, FILE_SMS n'est pas concerné (pas de CKEditor HTML pour des SMS)
                                if (_myFile.ViewMainTable.EdnType == EdnType.FILE_MAIL && myField.FldInfo.Descid % 100 == MailField.DESCID_MAIL_HTML.GetHashCode())
                                    _sMemoIds.Add(myValueCell.ID);



                                //css spécifique valeurs de champs
                                myValueCell.CssClass += " table_values";

                                // CRU : Si champ obligatoire 
                                // kha : ET si champ vide
                                myValueCell.CssClass += (myField.IsMandatory && string.IsNullOrEmpty(myField.Value)) ? " mandatory" : "";

                                // Couleur valeur
                                if (!string.IsNullOrEmpty(myField.FldInfo.ValueColor))
                                {
                                    myValueCell.CssClass += " useValueColor";
                                    myValueCell.Style.Add(HtmlTextWriterStyle.Color, myField.FldInfo.ValueColor);
                                }

                                // Style du champ
                                if (myField.FldInfo.FieldStyle != FIELD_DISPLAY_STYLE.NORMAL)
                                {
                                    if (myField.FldInfo.Format != FieldFormat.TYP_CHART)
                                        myValueCell.Attributes.Add("fstyle", myField.FldInfo.FieldStyle.ToString());

                                }


                                // Hauteur de la cellule calculée en fonction du nombre de lignes défini en admin, pour les rubriques ayant recours à une iFrame (images, champs Mémo...)
                                int nCellHeight = eConst.FILE_LINE_HEIGHT * myValueCell.RowSpan;
                                myValueCell.Height = nCellHeight;

                                // Lorsque le renderer générique crée le champ Image, il ne connaît pas encore la taille de l'image qui est calculée ci-dessous. Il ajoute alors 
                                // "eImage.aspx" comme simple URL afin que l'image ne soit pas chargée tout de suite, puis celle-ci est remplacée ci-dessous une fois que l'on
                                // dispose de toutes les informations nécessaires
                                if (myValueCell.HasControls() && myValueCell.Controls[0].GetType() == typeof(System.Web.UI.WebControls.Image) && ((System.Web.UI.WebControls.Image)myValueCell.Controls[0]).ImageUrl == "eImage.aspx")
                                {
                                    ((System.Web.UI.WebControls.Image)myValueCell.Controls[0]).ImageUrl = string.Concat("eImage.aspx?did=", myField.FldInfo.Descid, "&fid=", myField.FileId, "&it=IMAGE_FIELD&h=", nCellHeight, "&w=", nCellHeight);
                                }

                                if (!string.IsNullOrEmpty(myField.Value) ||
                                    myField.FldInfo.Format == FieldFormat.TYP_IFRAME ||
                                    myField.FldInfo.Format == FieldFormat.TYP_CHART ||
                                    myField.FldInfo.PosRowSpan > 1
                                    )
                                {
                                    bLineNotEmpty = true;
                                }
                                else if (myField.FldInfo.Format != FieldFormat.TYP_IMAGE && (RendererType == RENDERERTYPE.EditFile || RendererType == RENDERERTYPE.EditFileLite))
                                {
                                    myValueCell.Attributes.Add("eEmpty", "1");
                                }
                                //On affiche le bouton du catalogue seulement si on a le droit de modif sur le champ et qu'on est en mode edition
                                myButton = GetButtonCell(myValueCell, bDisplayBtn, myFieldIcon, myFieldIconColor, myField.FldInfo);
                                myTr.Cells.Add(myButton);
                                liTcField.Add(myButton);


                                AddAddtionnalControlInLabelCell(myLabel, myField.FldInfo);
                                AddAddtionnalControlInValueCell(myValueCell, myField.FldInfo);

                                #endregion
                            }
                            //else if (myField.RightIsVisible && myField.FldInfo.Format == FieldFormat.TYP_BUTTON)
                            //{
                            //    myTr.Cells.Add(myLabel);

                            //    TableCell myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, myField, 0, Pref);
                            //    myTr.Cells.Add(myValueCell);

                            //    myTr.Cells.Add(myButton);

                            //    liTcField.Add(myLabel);
                            //    liTcField.Add(myValue);
                            //    liTcField.Add(myButton);
                            //}
                            else
                            {
                                #region Value vide


                                myTr.Cells.Add(myLabel);
                                myTr.Cells.Add(myValue);
                                myButton = GetButtonCell(myValue, bDisplayBtn, myFieldIcon, myFieldIconColor);
                                myTr.Cells.Add(myButton);

                                liTcField.Add(myLabel);
                                liTcField.Add(myValue);
                                liTcField.Add(myButton);


                                #endregion
                            }

                        }
                        else
                        {
                            iLastDisporder++;

                            //empty cell
                            myLabel.ColumnSpan = eConst.NB_COL_BY_FIELD;
                            myTr.Cells.Add(myLabel);
                            myLabel.CssClass = "free";
                            AddAddtionnalControlInValueCell(myLabel);
                            //myTr.Cells.Add(myValue);
                            //myButton = GetButtonCell(myValue, _bIsEditRenderer);
                            //myTr.Cells.Add(myButton);

                            liTcField.Add(myLabel);
                            //liTcField.Add(myValue);
                            //liTcField.Add(myButton);
                        }
                        AddAdditionalAttributes(liTcField, coord, iLastDisporder);

                        // Ajout de la breakline de résumé
                        if (bShowResumeBreakline && _numLine == _resumeBreakLine && x == 0)
                            AddResumeBreakLine(myLabel);
                    }

                    if (!bHasPageSep && sTagPageSep.Length > 0)
                        myTr.Attributes.Add("ePageSep", sTagPageSep);

                    //#54 212 : On affiche les lignes composés uniquement d espace
                    // if (bRowHasOnlySpace)
                    //    bLineNotEmpty = true;

                    AddTableRow(maTable, myTr, nbColByLine * eConst.NB_COL_BY_FIELD, y, bLineNotEmpty, bHasPageSep, bHasSep, bDoNotAddTR);
                    #endregion

                }
            }
            finally
            {

                if (bUseTran)
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


        }

        /// <summary>
        /// Ajout de la breakline séparant le résumé
        /// </summary>
        /// <param name="tableCell">Cellule dans laquelle on ajoute l'icône de la breakline de résumé</param>
        protected virtual void AddResumeBreakLine(System.Web.UI.WebControls.TableCell tableCell)
        {
        }

        /// <summary>
        /// Rajoute une interligne
        /// </summary>
        /// <param name="maTable"></param>
        /// <param name="y"></param>
        /// <param name="iInterTrColspan"></param>
        protected void AddInterTr(System.Web.UI.WebControls.Table maTable, int y, int iInterTrColspan)
        {
            if (y > 0)
            {
                TableRow interTr = new TableRow();
                interTr.Cells.Add(new TableCell());
                // interTr.Cells[0].ColumnSpan = iInterTrColspan;
                interTr.Cells[0].ColumnSpan = _myFile.ViewMainTable.ColByLine * eConst.NB_COL_BY_FIELD;
                interTr.Attributes.Add("class", "tableSep");
                maTable.Rows.Add(interTr);
            }
        }

        /// <summary>
        /// Adds the table row.
        /// </summary>
        /// <param name="maTable">The ma table.</param>
        /// <param name="myTr">My tr.</param>
        /// <param name="nbMaxCols">The nb maximum cols.</param>
        /// <param name="y">The y.</param>
        /// <param name="bLineNotEmpty">if set to <c>true</c> [line not empty].</param>
        /// <param name="bHasPageSep">if set to <c>true</c> [has page sep].</param>
        /// <param name="bHasSep">if set to <c>true</c> [has sep].</param>
        /// <param name="bDoNotAddTR">if set to <c>true</c> [do not add tr].</param>
        protected virtual void AddTableRow(System.Web.UI.WebControls.Table maTable, TableRow myTr, int nbMaxCols, int y, bool bLineNotEmpty, bool bHasPageSep, bool bHasSep, bool bDoNotAddTR)
        {
            int iInterTrColspan = nbMaxCols;
            if (myTr.Cells.Count == nbMaxCols)
            {
                AddInterTr(maTable, y, nbMaxCols);
            }
            else if (myTr.Cells.Count == 0)
            {
                iInterTrColspan = 0;
            }
            else
            {
                int nbCols = 0;
                foreach (TableCell tc in myTr.Cells)
                {
                    if (tc.ColumnSpan > 0)
                        nbCols += tc.ColumnSpan;
                    else
                        nbCols += 1;
                }

                AddDropRowIcon(myTr, y, bLineNotEmpty);

                if (nbCols == nbMaxCols)
                    AddInterTr(maTable, y, nbMaxCols);

                iInterTrColspan = nbCols;
            }


            myTr.Attributes.Add("data-nbline", _numLine.ToString());
            maTable.Rows.Add(myTr);
            //if (_numLine == _resumeBreakLine && bShowResumeBL)
            //{
            //    AddResumeBreakLine(maTable);
            //}

            _numLine++;
        }

        /// <summary>
        /// Pour l'administration rajoute une icone en debut de ligne permettant de supprimer une ligne vide
        /// </summary>
        /// <param name="myTr"></param>
        /// <param name="y"></param>
        /// <param name="bLineNotEmpty"></param>
        protected virtual void AddDropRowIcon(TableRow myTr, int y, bool bLineNotEmpty)
        {
            return;
        }

        /// <summary>
        /// Adds the addtionnal control in value cell.
        /// </summary>
        /// <param name="myValueCell">My value cell.</param>
        /// <param name="field">The field.</param>
        protected virtual void AddAddtionnalControlInValueCell(TableCell myValueCell, Field field = null)
        {
            return;
        }

        /// <summary>
        /// Ajout de contrôles dans la cellule du libellé
        /// </summary>
        /// <param name="labelCell"></param>
        /// <param name="field"></param>
        protected virtual void AddAddtionnalControlInLabelCell(TableCell labelCell, Field field = null)
        {
            return;
        }

        /// <summary>
        /// Adds the additional attributes.
        /// </summary>
        /// <param name="liTC">The li tc.</param>
        /// <param name="coord">The coord.</param>
        /// <param name="iDisporder">The i disporder.</param>
        protected virtual void AddAdditionalAttributes(List<TableCell> liTC, Point coord, int iDisporder)
        {
            return;
        }

        /// <summary>
        /// Sets the prospect matched fields.
        /// </summary>
        /// <param name="myField">My field.</param>
        /// <param name="fldPPRec">The field pp record.</param>
        protected virtual void SetProspectMatchedFields(eFieldRecord myField, eFieldRecord fldPPRec)
        {
        }

        /// <summary>
        /// Lists the closed sep.
        /// </summary>
        /// <param name="myField">My field.</param>
        protected virtual void ListClosedSep(eFieldRecord myField)
        {
        }

        /// <summary>
        /// Ajoute les relations d'adresse (PP/PM)
        /// </summary>
        /// <param name="myField">eFieldRecord</param>
        /// <param name="maTable">Contrôle Table</param>
        /// <returns></returns>
        protected virtual bool SetAddrProfLink(eFieldRecord myField, System.Web.UI.WebControls.Table maTable)
        {
            return false;
        }


        /// <summary>
        /// Génère un titre séparateur.
        /// </summary>
        /// <param name="Cell"></param>
        /// <param name="fieldRecord">Objet eFieldRecord contenant les paramètres du champ à ajouter (libellé, mise en forme). Peut être null (auquel cas, il est impératif de renseigner sOverrideLabel et bOverridePageSep)</param>
        /// <param name="sOverrideLabel">Si différent de null ou string.Empty, ou si fieldRecord est null, ce libellé se substitue à celui indiqué par fieldRecord.FldInfo.Libelle</param>
        /// <param name="bOverridePageSep">Si fieldRecord est null, indique si le séparateur doit être affiché sur toute la page</param>
        protected void GetSeparator(TableCell Cell, eFieldRecord fieldRecord, string sOverrideLabel, bool bOverridePageSep)
        {
            string sLabel = string.Empty;
            bool bIsPageSep = true;
            string sTooltiptext = string.Empty;
            bool bIsLabelHidden = false;
            if (fieldRecord != null && fieldRecord.FldInfo != null)
            {
                sLabel = fieldRecord.FldInfo.Libelle;
                bIsPageSep = fieldRecord.FldInfo.Length == 1;
                sTooltiptext = fieldRecord.FldInfo.ToolTipText;
                bIsLabelHidden = fieldRecord.FldInfo.LabelHidden;
            }
            else
            {
                bIsPageSep = bOverridePageSep;
            }
            if (!string.IsNullOrEmpty(sOverrideLabel))
                sLabel = sOverrideLabel;

            // MAB - #19 207 : pour que l'ellipsis (...) fonctionne sous IE, il faut qu'elle
            // soit appliquée sur une div elle-même contenue dans une div.
            // D'où l'emploi de div et divContainer
            Panel divContainer = new Panel();
            //Panel div = new Panel();
            //divContainer.Controls.Add(div);

            Label spanSep = new Label();

            if (bIsPageSep)
            {
                HtmlGenericControl imgSep = new HtmlGenericControl("span");
                imgSep.Attributes.Add("class", "icon-title_sep icnFileBtn");
                imgSep.ID = "sepIcon";
                spanSep.Controls.Add(imgSep);
                divContainer.CssClass = "title_sep_page";
            }
            else
            {
                divContainer.CssClass = "title_sep";
            }

            bool bDoTT = Pref.GetConfig(eLibConst.PREF_CONFIG.TOOLTIPTEXTENABLED) == "1";

            divContainer.ToolTip = string.Concat(sLabel, (bDoTT) ? string.Concat(Environment.NewLine, sTooltiptext) : "");

            HtmlGenericControl titleSepLabel = new HtmlGenericControl("div");
            titleSepLabel.Attributes.Add("class", "title_sep_label");
            titleSepLabel.InnerHtml = HttpUtility.HtmlEncode(sLabel);
            spanSep.Controls.Add(titleSepLabel);

            divContainer.Controls.Add(spanSep);

            Panel divSep1 = new Panel();
            divSep1.CssClass = "sub_sep";
            if (fieldRecord != null && fieldRecord.FldInfo != null)
            {
                if (fieldRecord.FldInfo.StyleForeColor.Length > 0)
                {
                    spanSep.Style.Add(HtmlTextWriterStyle.Color, fieldRecord.FldInfo.StyleForeColor);
                    titleSepLabel.Style.Add(HtmlTextWriterStyle.BorderColor, fieldRecord.FldInfo.StyleForeColor);
                }
                if (fieldRecord.FldInfo.StyleBold) { spanSep.Style.Add(HtmlTextWriterStyle.FontWeight, "bold"); }
                if (fieldRecord.FldInfo.StyleItalic) { spanSep.Style.Add(HtmlTextWriterStyle.FontStyle, "italic"); }
                if (fieldRecord.FldInfo.StyleUnderline) { spanSep.Style.Add(HtmlTextWriterStyle.TextDecoration, "underline"); }
                if (fieldRecord.FldInfo.StyleFlat) { spanSep.Style.Add(HtmlTextWriterStyle.BorderStyle, "thin"); }

                Cell.Attributes.Add("did", fieldRecord.FldInfo.Descid.ToString());
            }

            //Panel divSep2 = new Panel();
            //divSep2.CssClass = "separateur_glbl LNKSEP";
            //divSep2.Controls.Add(divSep1);
            if (!bIsPageSep && bIsLabelHidden)
            {
            }
            else
            {
                Cell.Controls.Add(divContainer);
            }
            Cell.Attributes.Add("fld", "1");
            //Cell.Controls.Add(divSep2);
            Cell.Attributes.Add("class", "table_labels");


        }


        /// <summary>
        /// retourne la ligne à partir de laquelle les rubriques doivent passer sous la barre des signets
        /// </summary>
        /// <param name="nBreakLine"></param>
        /// <returns></returns>
        protected virtual int GetBreakLine(int nBreakLine = 0)
        {
            if (nBreakLine > 0)
                return nBreakLine;
            else
                return _myFile.ViewMainTable.BreakLine;

        }


        /// <summary>
        /// Prépare un tableau contenant les rubriques à afficher en signet et y deverse les lignes du tableau principal qui sont concernées
        /// </summary>
        /// <param name="fileTabBody"></param>
        /// <param name="nbColByLine"></param>
        /// <param name="nBreakLine"></param>
        protected virtual System.Web.UI.WebControls.Table SetHtmlTabInBkm(System.Web.UI.WebControls.Table fileTabBody, int nbColByLine, int nBreakLine)
        {
            int y = 0;
            int nEudoLine = 0, nPrevEudoLine = 0;

            for (y = 0; y < fileTabBody.Rows.Count; y++)
            {
                if (fileTabBody.Rows[y].Attributes["y"] == null)
                    continue;

                nPrevEudoLine = nEudoLine;
                nEudoLine = eLibTools.GetNum(fileTabBody.Rows[y].Attributes["y"]) + 1;
                if (nEudoLine >= nBreakLine)
                {
                    break;
                }
            }

            // nombre normal de cellules lorsqu'il n'y a aucun rowspan colspan
            int nbMaxCols = nbColByLine * eConst.NB_COL_BY_FIELD;
            //cpr = cells per row
            fileTabBody.Attributes.Add("cpr", nbMaxCols.ToString());

            if (y >= fileTabBody.Rows.Count)
                return null;

            /*
             * pour transférer les lignes dans le bkm sans prendre de risque de couper une ligne contenant un champ s'étendant sur plusieurs lignes (rowspan), 
             * on compte le nombre de cellules dans la ligne en faisant la somme des columnspan et on le compare au nombre de cellules qu'on est censé obtenir. 

             * Ex: dans une fiche dont les champs sont présentés sur 3 colonnes, on doit obtenir 9 cellules :
             * chaque champ possède un libellé, une Input, et un bouton -> 3 Champs x 3 Cellules = 9

             */

            TableRow tr;


            //nombre de colonne atteints en cumulant les colspan
            int nbCols = 0;

            while (y < fileTabBody.Rows.Count)
            {
                //s'il n'y a pas d'attribut y c'est une interligne : on ne prend pas en compte
                if (fileTabBody.Rows[y].Attributes["y"] == null)
                {
                    y++;
                }

                tr = fileTabBody.Rows[y];

                //modification du nombre de colonne suite au développement des libellés masqués

                //if (tr.Cells.Count == nbMaxCols)
                //{
                //    break;
                //}
                //else
                //{
                nbCols = 0;
                foreach (TableCell tc in tr.Cells)
                {
                    if (tc.Style["display"]?.ToLower() == "none")
                        continue;

                    if (tc.ColumnSpan > 0)
                        nbCols += tc.ColumnSpan;
                    else
                        nbCols += 1;

                }
                if (nbCols == nbMaxCols)
                    break;
                //}

                y++;
            }

            if (y >= fileTabBody.Rows.Count)
                return null;

            System.Web.UI.WebControls.Table fileTabInBkm = new System.Web.UI.WebControls.Table();

            fileTabInBkm.ID = string.Concat("ftdbkm_", _myFile.ViewMainTable.DescId);
            fileTabInBkm.CssClass = fileTabBody.CssClass;
            fileTabInBkm.Attributes.Add("cpr", nbMaxCols.ToString());

            bool bEmpty = true;

            while (y < fileTabBody.Rows.Count)
            {
                tr = fileTabBody.Rows[y];
                fileTabInBkm.Rows.Add(tr);
                fileTabBody.Rows.Remove(tr);

                if (bEmpty)
                {
                    foreach (TableCell tc in tr.Cells)
                    {
                        if (tc.HasControls())
                        {
                            bEmpty = false;
                            break;
                        }
                    }
                }
            }

            if (fileTabInBkm.Rows.Count > 0 && !bEmpty)
            {
                AddEmptyCellsInHead(_myFile, fileTabInBkm);

                _backBoneRdr.PnFilePart2.Controls.Add(fileTabInBkm);

                if (_myFile.ActiveBkm != EudoQuery.ActiveBkm.DISPLAYFIRST.GetHashCode())
                    _backBoneRdr.PnFilePart2.Style.Add("display", "none");

                _bFileTabInBkm = true;
            }
            return fileTabInBkm;
        }


        /// <summary>implémente le champ note (94) dans le cas du mode création</summary>
        protected virtual void AddMemoField()
        {
            if (_layoutMode == eFileLayout.Mode.COORD)
                return;

            eFieldRecord myMemoField = _myFile.Record.GetFieldByAlias(string.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.DescId + EudoQuery.AllField.MEMO_NOTES.GetHashCode()));

            if (myMemoField != null && myMemoField.RightIsVisible)
            {
                myMemoField.FldInfo.PosDisporder = 1;
                myMemoField.FldInfo.PosColSpan = _myFile.ViewMainTable.ColByLine;

                List<eFieldRecord> liFields = new List<eFieldRecord>();
                liFields.Add(myMemoField);

                System.Web.UI.WebControls.Table fileTab = CreateHtmlTable(liFields, _myFile.ViewMainTable.ColByLine, string.Empty);
                fileTab.ID = string.Concat("ftn_", _myFile.ViewMainTable.DescId);
                _backBoneRdr.PnFilePart1.Controls.Add(fileTab);
            }

        }

        /// <summary>
        /// crée une cellule de tableau contenant le logo de la fiche
        /// </summary>
        /// <returns></returns>
        protected Panel GetFileLogo()
        {
            Panel pnFileLogo = new Panel();
            pnFileLogo.CssClass = string.Concat("fileLogo ", "iconDef iconDef_", _myFile.CalledTabDescId);

            //Couleurs conditionnelles

            // Type d'icone
            //  priorité : Historique -> Conditionnel -> Standard
            if (_myFile.Record.IsHisto)
            {
                //pnFileLogo.CssClass = string.Concat(pnFileLogo.CssClass, " iconHisto", _myFile.CalledTabDescId);
                //TODO : resources !selIcon.ToolTip = row.RuleColor.Label;
                pnFileLogo.ToolTip = "HISTO";
                pnFileLogo.Style.Add("background-color", _myFile.HistoInfo.BgColor);
                pnFileLogo.Style.Add("color", _myFile.HistoInfo.Color);
                pnFileLogo.CssClass = string.Concat(pnFileLogo.CssClass, " ", eFontIcons.GetFontClassName(_myFile.HistoInfo.Icon));
            }
            else if (_myFile.Record.RuleColor.HasRuleColor && !string.IsNullOrEmpty(_myFile.Record.RuleColor.Icon))
            {
                // TODO : le row.RuleColor.Idendity n'est pas un id assez fort à priori
                //      2 règles de couleurs vont avoir le même id
                //pnFileLogo.CssClass = string.Concat(pnFileLogo.CssClass, " ", _myFile.Record.RuleColor.Idendity, "_1");
                pnFileLogo.ToolTip = _myFile.Record.RuleColor.Label;
                pnFileLogo.Style.Add("background-color", _myFile.Record.RuleColor.BgColor);
                pnFileLogo.Style.Add("color", _myFile.Record.RuleColor.Color);
                pnFileLogo.CssClass = string.Concat(pnFileLogo.CssClass, " ", eFontIcons.GetFontClassName(_myFile.Record.RuleColor.Icon));
            }
            else
            {
                pnFileLogo.CssClass = string.Concat(pnFileLogo.CssClass, " ", eFontIcons.GetFontClassName(_myFile.ViewMainTable.GetIcon));
                // HLA - Ne pas mettre de couleur par defaut, mais laisser le theme prendre le relais sur la couleur des icones de tables
                if (!string.IsNullOrEmpty(_myFile.ViewMainTable.GetIconColor))
                    pnFileLogo.Style.Add("color", _myFile.ViewMainTable.GetIconColor);
            }

            return pnFileLogo;
        }



        /// <summary>
        /// indique si le header doit être caché
        /// </summary>
        protected virtual bool HideHeader
        {
            get
            {
                return _myFile.ViewMainTable.EdnType == EdnType.FILE_STANDARD;
            }
        }

        /// <summary>
        /// Génère une table d'entête
        /// </summary>       
        /// <param name="bHasButtons"></param>
        protected virtual System.Web.UI.WebControls.Table GetHeader(bool bHasButtons = true)
        {
            System.Web.UI.WebControls.Table myTable = new System.Web.UI.WebControls.Table();


            // dans le cas des sous fichiers de type standard, pas d'en-tête
            //BSE #52691
            if (HideHeader)
                return myTable;


            myTable.ID = string.Concat("fth_", _tab.ToString());

            myTable.Attributes.Add("eid", string.Concat(_tab.ToString() + "_" + _myFile.FileId.ToString()));

            myTable.CssClass = "mTabFile";

            //
            TableRow myTrHeader = new TableRow();
            TableCell myCellHeader = new TableCell();
            myTrHeader.Cells.Add(myCellHeader);

            myTable.Rows.Add(myTrHeader);

            myCellHeader.ColumnSpan = 2;
            myCellHeader.Attributes.Add("class", "fileHeaderCell");

            System.Web.UI.WebControls.Table tbFileCadre = new System.Web.UI.WebControls.Table();
            tbFileCadre.CssClass = "fileCadre";
            myCellHeader.Controls.Add(tbFileCadre);
            if (_myFile.MainField != null)
            {
                HtmlInputHidden inptFileName = new HtmlInputHidden();
                _divHidden.Controls.Add(inptFileName);

                inptFileName.ID = string.Concat("fileName_", _myFile.MainField.Alias);

                string sLabel = _myFile.Record.GetFieldByAlias(_myFile.MainField.Alias).DisplayValue.Trim();
                // Cas particulier PP : Nom + Prénom
                if (_tab == EudoQuery.TableType.PP.GetHashCode())
                    sLabel = _myFile.Record.GetFieldByAlias(_myFile.MainField.Alias).DisplayValuePPName.Trim();

                inptFileName.Value = sLabel;
            }

            TableRow tr = new TableRow();
            tbFileCadre.Rows.Add(tr);

            #region DIV 99

            TableCell tcFileOwner = new TableCell();
            tcFileOwner.CssClass = "file99";

            Get99TableCell(tcFileOwner);

            #endregion

            tr.Cells.Add(tcFileOwner);

            #region légende champs obligatoires


            if (_myFile.Record.HasMandatoryFields && RendererType != RENDERERTYPE.PrintFile)
            {

                /* Légendes (ex : Saisie obligatoire) */

                // tr = new TableRow();
                //         tbFileCadre.Rows.Add(tr);

                TableCell tcHeaderCaptions = new TableCell();
                //tcHeaderCaptions.ColumnSpan = 2;

                // Champs obligatoires
                HtmlGenericControl lblMandatoryFields = new HtmlGenericControl("div");
                tcHeaderCaptions.Controls.Add(lblMandatoryFields);
                lblMandatoryFields.Attributes.Add("class", "divHeadCaption MndAst");
                lblMandatoryFields.InnerText = string.Concat("* ", eResApp.GetRes(Pref, 6304));

                tr.Cells.Add(tcHeaderCaptions);
            }


            #endregion

            #region buttons
            /*  Bloc Boutons */
            if (bHasButtons && RendererType != RENDERERTYPE.PrintFile)
            {
                TableCell tcButtons = new TableCell();
                tcButtons.Attributes.Add("class", "toolBtn");
                tr.Cells.Add(tcButtons);
                // S'il n'y a pas de champs obligatoires sur la fiche, on n'affichera pas la légende "* Saisie Obligatoire"
                // On peut alors étendre l'affichage des boutons sur 2 lignes
                //if (!_myFile.Record.HasMandatoryFields)
                //{
                //    //tcButtons.RowSpan = 2;
                //}
                tcButtons.Controls.Add(GetToolBar());

            }
            #endregion

            return myTable;
        }

        /// <summary>
        /// Construit des objets html annexes/place des appel JS d'apres chargement
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            if (!base.End())
                return false; ;

            //creation d'une input contenant la liste des champs memos de la fiche
            HtmlInputHidden memoDescIds = new HtmlInputHidden();
            memoDescIds.ID = string.Concat("memoIds_", _tab);
            memoDescIds.Attributes.Add("ednmemoIds_" + _tab, "1");

            _divHidden.Controls.Add(memoDescIds);
            memoDescIds.Value = string.Join(";", _sMemoIds.ToArray());
            return true;

        }
        /// <summary>
        /// Construit la fin du contenu de la fiche
        /// </summary>
        protected virtual void EndFillContent()
        {

        }

        /// <summary>
        /// Barre d'outils 
        /// </summary>
        /// <returns></returns>
        protected virtual Panel GetToolBar()
        {
            return new Panel();
        }

        /// <summary>
        /// ajoute en entete de tableau les cellules qui permettent d'étalonner les largeurs.
        /// </summary>
        /// <param name="myFile"></param>
        /// <param name="maTable"></param>
        /// <param name="nbColByLine">Param optionnel indiquant le nb de colonne du tableau</param>
        /// <param name="bSystemField">Param optionnel indiquant si le tableau est de type system</param>
        /// 
        public static void AddEmptyCellsInHead(eFile myFile, System.Web.UI.WebControls.Table maTable, int nbColByLine = 0, bool bSystemField = false)
        {
            //ajout de cellule vide en entête pour caler les largeur des cellules
            TableRow myEmptyRow = new TableRow();
            maTable.Rows.AddAt(0, myEmptyRow);

            myEmptyRow.CssClass = "emptyrow";
            myEmptyRow.Style.Add(System.Web.UI.HtmlTextWriterStyle.Height, "1px");
            string[] sColumnsWidth = myFile.ViewMainTable.ColumnsDisplay.Split(",");
            int nWidth = 0;
            int idxColumnsWidth = 0;
            int colByLine = nbColByLine != 0 ? nbColByLine : myFile.ViewMainTable.ColByLine;

            for (int nCmpt = 0; nCmpt < colByLine; nCmpt++)
            {
                // Libellé                
                TableCell myTc = new TableCell();
                myTc.CssClass = "table_labels";
                myTc.Height = 1;
                myEmptyRow.Cells.Add(myTc);

                idxColumnsWidth = nCmpt * eConst.NB_COL_BY_FIELD;
                // largeur du libellé
                //49 389 : Correction du calcul de la taille des colonnes pour les champs system (nb colonne = 4 et size = auto)
                if (idxColumnsWidth < sColumnsWidth.Length && !bSystemField)
                {
                    if (int.TryParse(sColumnsWidth[idxColumnsWidth], out nWidth) && nWidth > 0)
                        myTc.Style.Add("width", string.Concat(nWidth, "px"));
                }
                //Champ
                myTc = new TableCell();
                myTc.CssClass = "table_values";
                myTc.Height = Unit.Pixel(1);
                myEmptyRow.Cells.Add(myTc);

                //largeur du champ
                idxColumnsWidth++;
                if (idxColumnsWidth < sColumnsWidth.Length)
                {
                    if (int.TryParse(sColumnsWidth[idxColumnsWidth], out nWidth) && nWidth > 0)
                        myTc.Style.Add("width", string.Concat(nWidth, "px"));
                }
                //Bouton
                myTc = new TableCell();
                myTc.CssClass = "btn";
                myTc.Height = 1;
                myEmptyRow.Cells.Add(myTc);

                //Largeur de cellule contenant le bouton
                idxColumnsWidth++;
                if (idxColumnsWidth < sColumnsWidth.Length)
                {
                    if (int.TryParse(sColumnsWidth[idxColumnsWidth], out nWidth) && nWidth > 0)
                        myTc.Style.Add("width", string.Concat(nWidth, "px"));
                }
            }
        }

        /// <summary>
        /// Adds the drop fields area.
        /// </summary>
        /// <param name="maTable">The ma table.</param>
        protected virtual void AddDropFieldsArea(System.Web.UI.WebControls.Table maTable)
        {
            return;
        }


        /// <summary>
        /// rend le block HTML de tous les signets dans une méthode overridable
        /// </summary>
        /// <returns></returns>
        protected virtual void GetBookMarkBlock()
        {

            eRenderer bkmBarRdr = eRendererFactory.CreateBookmarkBarRenderer(Pref, _myFile, _bFileTabInBkm);
            eTools.TransfertFromTo(bkmBarRdr.PgContainer, _backBoneRdr.PnBkmBar);

        }

        /// <summary>
        /// Adds the file properties block.
        /// </summary>
        protected virtual void AddFilePropertiesBlock()
        {
            return;
        }


        /// <summary>
        /// Propriétés de la fiche
        /// </summary>
        /// <param name="tcFileOwner"></param>
        protected virtual void Get99TableCell(TableCell tcFileOwner)
        {
            eFieldRecord fldOwner = _myFile.Record.GetFieldByAlias(string.Concat(_myFile.ViewMainTable.Alias, "_", _myFile.ViewMainTable.GetOwnerDescId()));

            tcFileOwner.Controls.Add(GetFileLogo());

            HtmlGenericControl spanFileOwner = new HtmlGenericControl("span");
            spanFileOwner.ID = "fileInfo";
            spanFileOwner.Attributes.Add("onclick", string.Concat("shPties(", _myFile.ViewMainTable.DescId, ",", _myFile.FileId, ")"));
            spanFileOwner.Attributes.Add("onmouseover", string.Concat("oToolTip.show(this, ", _myFile.ViewMainTable.DescId, ",", _myFile.FileId, ");"));
            spanFileOwner.Attributes.Add("onmouseout", string.Concat("oToolTip.hide();"));
            spanFileOwner.Attributes.Add("class", "fileOwner");

            tcFileOwner.Controls.Add(spanFileOwner);

            if (fldOwner?.RightIsVisible ?? false)
            {

                HtmlGenericControl spanFileOwnerLabel = new HtmlGenericControl("span");
                spanFileOwnerLabel.InnerHtml = string.Concat(HttpUtility.HtmlEncode(fldOwner.FldInfo.Libelle), "&nbsp;:&nbsp;");
                spanFileOwnerLabel.Attributes.Add("class", "file99Label");

                HtmlGenericControl spanFile99Value = new HtmlGenericControl("span");
                spanFile99Value.ID = "file99Value";
                spanFile99Value.Attributes.Add("class", "file99Value");
#if DEBUG
                /* POC
                int userId = 0;
                int.TryParse(fldOwner.Value, out userId);
                HtmlGenericControl avatar = new HtmlGenericControl("img");
                avatar.ID = "file99Avatar";           
                avatar.Attributes.Add("class", "memoAvatar avatar99");
                avatar.Attributes.Add("uid", userId.ToString());
                avatar.Attributes.Add("src", eImageTools.GetAvatar(Pref, true, userId));
                */
#endif
                // Couleur de la valeur
                if (!string.IsNullOrEmpty(fldOwner.FldInfo.ValueColor))
                {
                    spanFile99Value.Style.Add("color", fldOwner.FldInfo.ValueColor);
                }


                // Protection XSS : dans le innerText, pas de HttpUtility.HtmlEncode : sinon, double encodage
                spanFile99Value.InnerText = (fldOwner.DisplayValue);
                spanFileOwner.Controls.Add(spanFileOwnerLabel);
#if DEBUG
                //  POC
                //  spanFileOwner.Controls.Add(avatar);
#endif
                spanFileOwner.Controls.Add(spanFile99Value);
            }
            else
            {
                spanFileOwner.InnerHtml = eResApp.GetRes(Pref, 54);

                /*
                XRM - #59 797 - Pas de tentative de MAJ du champ Appartient à si celui-ci n'est pas affiché (pas de droits de visu)
                On ajoute un champ caché pour indiquer au JS de ne pas afficher de message d'avertissement
                */
                HtmlInputHidden ownerFieldHidden = new HtmlInputHidden();
                ownerFieldHidden.ID = "ownerFieldHidden";
                ownerFieldHidden.Value = "1";
                spanFileOwner.Controls.Add(ownerFieldHidden);

            }

            #region Liens Reseaux Sociaux / SiteWeb
            List<eFieldRecord> lstActionBarRecords = _myFile.Record.GetFields.FindAll(
                fld => fld.FldInfo != null
                && (fld.FldInfo.Format == FieldFormat.TYP_SOCIALNETWORK || fld.FldInfo.Format == FieldFormat.TYP_WEB || fld.FldInfo.Format == FieldFormat.TYP_EMAIL)
                && fld.FldInfo.DisplayInActionBar
                && fld.RightIsVisible
                && !string.IsNullOrEmpty(fld.Value)
                );
            foreach (eFieldRecord fldRecord in lstActionBarRecords)
            {
                HtmlGenericControl pnlSocNet = new HtmlGenericControl("span");

                string url = string.Empty;
                string onclick = string.Empty;
                string title = string.Empty;
                string pnlClass = string.Empty;

                switch (fldRecord.FldInfo.Format)
                {
                    case FieldFormat.TYP_SOCIALNETWORK:
                        url = eTools.GetSocialNetworkUrl(fldRecord.Value, fldRecord.FldInfo.RootURL);
                        onclick = string.Concat("openUrlInNewTab('", url, "');");
                        title = url;
                        if (!string.IsNullOrEmpty(fldRecord.FldInfo.Icon))
                            pnlClass = eTools.GetEudoFontClass(fldRecord.FldInfo.Icon);
                        else
                            pnlClass = "icon-site_web";
                        break;
                    case FieldFormat.TYP_WEB:
                        url = fldRecord.Value;
                        onclick = string.Concat("openUrlInNewTab('", url, "');");
                        title = url;
                        pnlClass = "icon-site_web";
                        break;
                    case FieldFormat.TYP_EMAIL:
                        url = fldRecord.Value;
                        onclick = string.Concat("sendMailOrSMSFromActionBar(this, '", url, "', TypeMailing.MAILING_FROM_BKM);"); // TODO SMS
                        title = url;
                        pnlClass = "icon-email";
                        pnlSocNet.Attributes.Add("did", fldRecord.FldInfo.Descid.ToString()); //nécessaire pour l'envoi d'email
                        break;
                    case FieldFormat.TYP_PHONE:
                        url = fldRecord.Value;
                        onclick = string.Concat("sendMailOrSMSFromActionBar(this, '", url, "', TypeMailing.SMS_MAILING_FROM_BKM);");
                        title = url;
                        pnlClass = "icon-sms";
                        pnlSocNet.Attributes.Add("did", fldRecord.FldInfo.Descid.ToString()); //nécessaire pour l'envoi de SMS
                        break;
                }
                pnlClass = string.Concat(pnlClass, " ", "actionBarLink");

                pnlSocNet.Attributes.Add("onclick", onclick);
                pnlSocNet.Attributes.Add("title", title);
                pnlSocNet.Attributes.Add("class", pnlClass);

                if (fldRecord.FldInfo.Format == FieldFormat.TYP_SOCIALNETWORK)
                {
                    if (!string.IsNullOrEmpty(fldRecord.FldInfo.IconColor))
                        pnlSocNet.Style.Add("color", fldRecord.FldInfo.IconColor);
                }

                tcFileOwner.Controls.Add(pnlSocNet);
            }

            #endregion

            #region   Div logo Infos ou VCard
            HtmlGenericControl pnlLogoInf = new HtmlGenericControl("span");


            /* Div logo VCard - sur PP uniquement */
            if (!string.IsNullOrEmpty(Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.VCARDMAPPING })[eLibConst.CONFIG_DEFAULT.VCARDMAPPING]) && (_tab == EudoQuery.TableType.PP.GetHashCode()))
            {
                HtmlGenericControl vcard = new HtmlGenericControl("span");
                vcard.Attributes.Add("class", "vcard-line");

                // Icône
                pnlLogoInf.Attributes.Add("class", "icon-vcard");
                // Infobulle
                pnlLogoInf.Attributes.Add("title", eResApp.GetRes(Pref, 6300));
                // Action
                if (RendererType != RENDERERTYPE.PrintFile)
                {
                    pnlLogoInf.Attributes.Add("onclick",
                        string.Concat("shvc(this, 1, ", _myFile.FileId, ", null, true);setWindowEventListener('click', function (e) { HideVcard(e); });"));
                    pnlLogoInf.Attributes.Add("onmouseover", string.Concat("shvc(this, 1, ", _myFile.FileId, ");"));
                    pnlLogoInf.Attributes.Add("onmouseout", string.Concat("if(!vcFromClick)shvc(this, 0);"));
                }

                vcard.Controls.Add(pnlLogoInf);

                HtmlGenericControl pnlvCardDownload = new HtmlGenericControl("span");
                // Icône
                pnlvCardDownload.Attributes.Add("class", "icon-import");
                // Infobulle
                pnlvCardDownload.Attributes.Add("title", eResApp.GetRes(Pref.LangId, 6805));// "Télécharger la vCard";
                                                                                            // Action
                if (RendererType != RENDERERTYPE.PrintFile)
                    pnlvCardDownload.Attributes.Add("onclick", "exportVCard('" + CryptoEudonet.Encrypt(_myFile.FileId.ToString(), CryptographyConst.KEY_CRYPT_LINK2) + "');");

                vcard.Controls.Add(pnlvCardDownload);

                tcFileOwner.Controls.Add(vcard);

            }

            pnlLogoInf = new HtmlGenericControl("span");
            eFieldRecord fldInfos = _myFile.Record.GetFieldByAlias(string.Concat(_myFile.ViewMainTable.Alias, "_", _tab + EudoQuery.AllField.MEMO_INFOS));
            bool hasValue = false;
            string cssClass = string.Empty;
            if (fldInfos != null && fldInfos.RightIsVisible)
            {
                if (!string.IsNullOrEmpty(fldInfos.DisplayValue))
                    hasValue = true;

                // Icône
                if ((_tab == EudoQuery.TableType.PP.GetHashCode() || _tab == EudoQuery.TableType.PM.GetHashCode()))
                {
                    cssClass = "icon-info-circle pppm";
                }
                else
                {
                    cssClass = "icon-info-circle evt";
                }

                if (!hasValue)
                    cssClass = string.Concat(cssClass, " disabled");

                pnlLogoInf.Attributes.Add("class", cssClass);
                // Infobulle
                pnlLogoInf.Attributes.Add("title", (fldInfos.FldInfo.ToolTipText.Length > 0 ? fldInfos.FldInfo.ToolTipText : fldInfos.FldInfo.Libelle));
                // Action
                if (RendererType != RENDERERTYPE.PrintFile)
                {
                    pnlLogoInf.Attributes.Add("did", fldInfos.FldInfo.Descid.ToString());
                    pnlLogoInf.Attributes.Add("html", RenderMemoFieldIsHtml(fldInfos) ? "1" : "0");
                    pnlLogoInf.Attributes.Add("name", fldInfos.FldInfo.Libelle);
                    pnlLogoInf.Attributes.Add("onclick", string.Concat("shi(this, ", _myFile.FileId, ");"));
                }

                tcFileOwner.Controls.Add(pnlLogoInf);
            }

            #endregion
        }

        /// <summary>
        /// Gets the properties fields.
        /// </summary>
        /// <param name="PtyFieldsDescId">Fields descid list</param>
        /// <returns></returns>
        protected virtual List<int> GetPropertiesFields(ref List<int> PtyFieldsDescId)
        {
            return new List<int>();
        }


        #endregion


    }

}