using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Page des options des fiches marquées
    /// </summary>
    public partial class eMarkedFileDialog : eEudoPage
    {
        /// <summary>niveau de l'utilisateur en cours</summary>
        public int userLevel = 0;

        /// <summary>contrôle liste affiché sur la page</summary>
        public string pageOutput;
        /// <summary>code JS affiché en haut de la page</summary>
        public string pageHeaderJSOutput;
        /// <summary>code JS affiché en bas de la page</summary>
        public string pageFooterJSOutput;
        private Int32 nTab = 0;
        private Int32 nUserId = 0;


        private Int32 nType = -1;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eMarkedFile");
            PageRegisters.AddCss("eCatalog");

            #endregion

            #region


            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eMarkedFile");
            PageRegisters.AddScript("eModalDialog");
            #endregion

            userLevel = _pref.User.UserLevel;
            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            nType = -1;
            if (allKeys.Contains("type") && !String.IsNullOrEmpty(Request.Form["type"]))
                Int32.TryParse(Request.Form["type"].ToString(), out nType);


            MarkedFilesSelection markedSel = null;

            // Table de la sélection
            nTab = _pref.Context.Paging.Tab;
            nUserId = _pref.User.UserId;
            _pref.Context.MarkedFiles.TryGetValue(nTab, out markedSel);




            if (nTab > 0 && nUserId > 0)
            {
                StringBuilder _sbFooterJSOutput = new StringBuilder();
                StringBuilder _sbHeaderJSOutput = new StringBuilder();
                divCatalogFinal.Attributes.Add("class", "eMarkedFileEditor");
                HtmlGenericControl divHeader = new HtmlGenericControl();

                // Valeures du catalogue
                HtmlGenericControl divValues = new HtmlGenericControl("div");

                if (nType == 0)
                    divHeader = CreateHtmlHeaderMarkedFile();

                eMarkedFiles eMF = new eMarkedFiles(_pref);
                try
                {
                    eMF.LoadMarkedFiles();
                }
                catch (Exception exp)
                {
                    //Avec exception
                    String sDevMsg = String.Concat("Erreur sur eMakedFileDialog -> LoadMarkedFiles : \n");
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", exp.Message,
                        Environment.NewLine, "Exception StackTrace :", exp.StackTrace
                        );


                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        String.Concat(sDevMsg)

                        );

                    LaunchError();
                }

                CreateSelectionRenderer(eMF, out _sbFooterJSOutput, out divValues);

                //*****  On force le rendu HTML avant qu'il s'affiche dans la page pour le XML  //
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                System.IO.StringWriter sw = new System.IO.StringWriter(sb);
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                //**********************************************//

                // On ajoute les differentes div entetes valeures .. au div principal
                if (nType == 0)
                    divCatalogFinal.Controls.Add(divHeader);
                HtmlGenericControl divSeparateur = new HtmlGenericControl("DIV");
                divSeparateur.InnerHtml = "&nbsp;";
                divCatalogFinal.Controls.Add(divSeparateur);
                divCatalogFinal.Controls.Add(divValues);
            }
            else
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    String.Concat("Erreur sur eMakedFileDialog -PageLoad Lectures des paramètres = nTab ->", nTab, "<- ")

                );
                LaunchError();
            }
        }





        /// <summary>
        /// /
        /// </summary>
        /// <param name="p_MarkedFile"></param>
        /// <param name="sbAdditionalJSOutput"></param>
        /// <param name="divCatalogWindow"></param>
        private void CreateSelectionRenderer(eMarkedFiles p_MarkedFile, out StringBuilder sbAdditionalJSOutput, out HtmlGenericControl divCatalogWindow)
        {
            divCatalogWindow = new HtmlGenericControl("DIV");

            //**DIV contenant le tableau avec les valeurs du catalogue**//
            // Validation de la popup par double-clic : l'objet JS est situé sur le parent (car le tableau est situé dans l'iframe interne de eModalDialog)
            divCatalogWindow.Attributes.Add("class", "eMarkedFileEditValues");
            divCatalogWindow.ID = "eCEDValues";

            //**  Table avec les valeures du catalogue* **//
            HtmlTable tbCatValues = new HtmlTable();
            tbCatValues.ID = "markedFileTab";
            tbCatValues.Attributes.Add("cellpadding", "0");
            tbCatValues.Attributes.Add("cellspacing", "0");
            tbCatValues.Attributes.Add("class", "eMarkedFileEditValues");
            // StringBuilder contenant du code JS à ajouter éventuellement à l'existant en haut de page
            sbAdditionalJSOutput = new StringBuilder();


            if (nType == 0)
                pageFooterJSOutput = String.Concat(pageFooterJSOutput, "document.getElementById('txtMarkedFileName').focus();", Environment.NewLine);


            int _ligne = 0; //Utilisé pour ecrire 1 ligne sur 2

            HtmlTableRow _rowsValues = new HtmlTableRow();
            foreach (MarkedFilesSelection _MF_value in p_MarkedFile.Values)
            {

                string _sClass = "eMarkedFileEditValues";
                if ((_ligne % 2) == 1)
                    _sClass = " eMarkedFileEditValues2";


                if (p_MarkedFile.markedSel != null && p_MarkedFile.markedSel.Id == _MF_value.Id)
                {
                    _sClass = " eMarkedFileSelected";
                    pageFooterJSOutput = String.Concat(pageFooterJSOutput, "var nSelectedId =", _MF_value.Id.ToString(), ";", Environment.NewLine);
                }

                _rowsValues = new HtmlTableRow();
                _rowsValues.ID = string.Concat("eMfId", _MF_value.Id.ToString());
                _rowsValues.Attributes.Add("class", _sClass);
                _rowsValues.Attributes.Add("index", _ligne.ToString());
                _rowsValues.Attributes.Add("ednval", _MF_value.Name);
                _rowsValues.Attributes.Add("ednid", _MF_value.Id.ToString());
                _rowsValues.Attributes.Add("value", _MF_value.Name);

                HtmlTableCell tbCellValues = new HtmlTableCell();
                tbCellValues.ID = String.Concat("eCatalogEditorLabel", _ligne.ToString());
                //  tbCellValues.ColSpan = 2;

                // Colonne Valeurs
                if (nType == 1) // type ouvrir -> option rename/delete/select activé
                {
                    tbCellValues.Attributes.Add("class", "eMarkedFilegEditValuesLbl edit cell");
                    tbCellValues.Attributes.Add("onDblClick", string.Concat("goSelect(", _MF_value.Id, ")"));
                    tbCellValues.Attributes.Add("onClick", string.Concat("switchSelected(", _MF_value.Id, ");"));
                }
                else
                {
                    tbCellValues.Attributes.Add("class", "eMarkedFilegEditValuesLbl edit cell");
                }


                tbCellValues.InnerHtml = _MF_value.Name;

                _rowsValues.Cells.Add(tbCellValues);

                //Demande #19407 : Boutons modif et supprimé doivent apparraitre même en enregistrement
                if ((nType == 1) || (nType == 0))
                {
                    //Derniere colonne 
                    // boutons modifier supprimer et tooltip
                    tbCellValues = new HtmlTableCell();
                    tbCellValues.Attributes.Add("class", "eMarkedFileEditBtn");
                    HtmlGenericControl divbtnValues = new HtmlGenericControl("div");
                    divbtnValues.Attributes.Add("class", "eMarkedFileBtnValues");
                    HtmlGenericControl ulBtnValues = new HtmlGenericControl("ul");
                    ulBtnValues.Attributes.Add("class", "eMarkedFileEdiBtnUl");

                    // Bouton modifier
                    HyperLink aModif = new HyperLink();
                    aModif.ToolTip = eResApp.GetRes(_pref, 151);
                    HtmlGenericControl liBtnValuesModif = new HtmlGenericControl("li");
                    liBtnValuesModif.Attributes.Add("class", "eMarkedFileEdiBtnUl eMarkedFileBtnModif icon-edn-pen");
                    liBtnValuesModif.Attributes.Add("onclick", string.Concat("eMFEObject.initEditors(); eMFEObject.eMarkedFile_LblEditor.onClick(document.getElementById('eCatalogEditorLabel", _ligne.ToString(), "'), this);"));
                    aModif.Attributes.Add("href", "#");


                    liBtnValuesModif.Controls.Add(aModif);

                    //Bouton Supprimer
                    HyperLink aDelete = new HyperLink();
                    aDelete.ToolTip = eResApp.GetRes(_pref, 19);
                    HtmlGenericControl liBtnValuesDelete = new HtmlGenericControl("li");
                    liBtnValuesDelete.Attributes.Add("class", "eMarkedFileEdiBtnUl eMarkedFileBtnDelete icon-delete");
                    aDelete.Attributes.Add("href", "#");

                    liBtnValuesDelete.Attributes.Add("onclick", string.Concat("eConfirm(1, top._res_806, top._res_6273,'',500,200,function(){eMFEObject.delMarkedFileSel(", _MF_value.Id, " ); },function(){});return false;"));

                    //aDelete.Attributes.Add("onclick", string.Concat("delSelect(", _MF_value.Id, " ); return false;"));
                    //aDelete.Attributes.Add("onclick", string.Concat("delSelect(", _MF_value.Id, " ); return false;"));

                    liBtnValuesDelete.Controls.Add(aDelete);



                    ulBtnValues.Controls.Add(liBtnValuesModif);
                    ulBtnValues.Controls.Add(liBtnValuesDelete);
                    divbtnValues.Controls.Add(ulBtnValues);

                    tbCellValues.Controls.Add(divbtnValues);

                }


                _rowsValues.Cells.Add(tbCellValues);

                // On insere la ligne générée (<tr>) dans le tableau 
                tbCatValues.Rows.Add(_rowsValues);

                _ligne++;
            }

            if (p_MarkedFile.Values.Count == 0)
            {
                HtmlTableRow rowsValuesnull;
                HtmlTableCell tbCellValues;
                if (p_MarkedFile.Values.Count == 0)
                {

                    rowsValuesnull = new HtmlTableRow();
                    rowsValuesnull.ID = string.Concat("eCatalogEditorValue", _ligne.ToString());
                    rowsValuesnull.Attributes.Add("class", "eMarkedFileEditValues");
                    rowsValuesnull.Attributes.Add("index", _ligne.ToString());

                    tbCellValues = new HtmlTableCell();
                    tbCellValues.ColSpan = 2;
                    tbCellValues.Attributes.Add("class", "eMarkedFileMenuItemNoRes eMarkedFilegEditValuesLbl edit cell");
                    tbCellValues.Attributes.Add("id", string.Concat("eCatalogEditorLabel", _ligne.ToString()));
                    tbCellValues.InnerHtml = eResApp.GetRes(_pref, 6195);

                    rowsValuesnull.Cells.Add(tbCellValues);
                    tbCatValues.Rows.Add(rowsValuesnull);
                    _ligne++;
                }
            }


            // On ajoute la table au div
            divCatalogWindow.Controls.Add(tbCatValues);
        }



        /// <summary>
        /// Creation d'un DIV avec la zone de texte permettant d'ajouter une nouvelel sélection
        /// </summary>
        /// <returns>On renvoi un DIV complet</returns>
        private HtmlGenericControl CreateHtmlHeaderMarkedFile()
        {
            //**  DIV Entete contenant tout le rendu html **//
            HtmlGenericControl divCatalogHeader = new HtmlGenericControl("DIV");

            //**  Table Entete l **//
            HtmlTable tbheader = new HtmlTable();
            tbheader.Attributes.Add("cellpadding", "0");
            tbheader.Attributes.Add("cellspacing", "0");
            // Entete avec la zone de recherche et la loupe pour rechercher

            divCatalogHeader.Attributes.Add("class", "eMarkedFileEdit");

            HtmlTableRow rows = new HtmlTableRow();
            HtmlTableCell tbCell = new HtmlTableCell();
            tbCell.InnerHtml = eResApp.GetRes(_pref, 6219);
            rows.Cells.Add(tbCell);

            tbCell = new HtmlTableCell();
            HtmlGenericControl divSearch = new HtmlGenericControl("DIV");
            divSearch.Style.Value = "float:right";
            HtmlInputText inputSearch = new HtmlInputText("text");
            inputSearch.ID = "txtMarkedFileName";
            inputSearch.Name = "txtMarkedFileName";
            divSearch.Controls.Add(inputSearch);
            tbCell.Controls.Add(divSearch);

            rows.Cells.Add(tbCell);
            tbCell = new HtmlTableCell();
            tbCell.ID = "tdResult";
            rows.Cells.Add(tbCell);

            tbheader.Rows.Add(rows);
            divCatalogHeader.Controls.Add(tbheader);

            return divCatalogHeader;
        }

    }
}