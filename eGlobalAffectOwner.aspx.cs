using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// cette page permet de répartir les fiches sur plusieurs utilisateurs lors de la mise à jour en masse sur le champs Appartient à (99)
    /// </summary>
    public partial class eGlobalAffectOwner : eEudoPage
    {
        /// <summary>Nombre de fiches à répartir</summary>
        public Int32 TotalFilesNumber;
        /// <summary>
        /// Dictionnaires de RES
        /// </summary>
        public Dictionary<Int32, String> Res;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eList", "all");
            PageRegisters.AddCss("eControl", "all");
            PageRegisters.AddCss("eGlobalAffectOwner", "all");
            PageRegisters.AddCss("eCatalog", "all");

            #endregion

            #region ajout des js



            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eGlobalAffectOwner");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eFieldEditor");

            #endregion

            Res = new Dictionary<Int32, String>();

            Int32[] iRes = { 6424, 6425, 6426, 6427, 950, 6437 };
            foreach (Int32 i in iRes)
            {
                Res.Add(i, eResApp.GetRes(_pref, i));
            }

            Int32 nDescId = 0;
            Int32.TryParse(Request.Form["descid"], out nDescId);

            String sInitValues = String.Empty;

            if (_allKeys.Contains("initvalues"))
                sInitValues = Request.Form["initvalues"];



            TotalFilesNumber = 0;

            if (_allKeys.Contains("nbfiles") && _allKeys.Contains("all"))
            {
                String sNbFiles = CryptoTripleDES.Decrypt(Request.Form["nbfiles"], CryptographyConst.KEY_CRYPT_LINK1);
                Boolean bAllFiles = Request.Form["all"] == "1";

                if (sNbFiles.Contains('#'))
                {
                    String[] aNbFiles = sNbFiles.Split('#');

                    if (bAllFiles)
                        Int32.TryParse(aNbFiles[1], out TotalFilesNumber);
                    else
                        Int32.TryParse(aNbFiles[0], out TotalFilesNumber);


                }

            }

            //Aucune fiche a sélectionner
            if (TotalFilesNumber == 0)
            {
                StringBuilder sDevMsg = new StringBuilder();
                StringBuilder sUserMsg = new StringBuilder();

                sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append("Aucune fiches à traiter");

                sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));


                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    sUserMsg.ToString(),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    sDevMsg.ToString()

                    );

                LaunchError();
                return;
            }

            List<String> selectedIds = new List<String>();
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);


            eCheckBoxCtrl chkBox;

            eDal.OpenDatabase();
            try
            {
                StringBuilder sbError = new StringBuilder();
                //TODOKHA prendre en compte l'option "afficher tous les users" sur le champs à mettre à jour
                Boolean bIsFullUserList = false;

                //Liste des users accessibles en fonction de la gestion de groupe de la base
                eUser user = new eUser(eDal, _pref.User, eUser.ListMode.USERS_AND_GROUPS, _pref.GroupMode, selectedIds);
                user.SortMode = eUser.ListSortMode.MIXED_ALPHABETIC; // # 22 150 / # 22 151 - sur cette fenêtre, les utilisateurs et groupes sont triés alphabétiquement
                List<eUser.UserListItem> userListItems = user.GetUserList(bIsFullUserList, false, String.Empty, sbError);


                Dictionary<String, Int32> dicInit = new Dictionary<String, Int32>();
                try
                {
                    if (sInitValues.Length > 0)
                    {
                        dicInit = sInitValues.Split("$|$").Select(aKV => aKV.Split(":"))
                            .ToDictionary(sKv => sKv[0], sKv => Int32.Parse(sKv[1]));

                        /*
                      .Select(p => new { p, splittedP = p.Split(':') })
                      .Select(p => new KeyValuePair<Int32, Int32>(Int32.Parse(p.splittedP[0]), Int32.Parse(p.splittedP[1]))))
                      .ToList();

                         * */
                    }
                }
                catch (Exception ex)
                {


                    StringBuilder sDevMsg = new StringBuilder();
                    StringBuilder sUserMsg = new StringBuilder();

                    sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append("Valeurs prédéinies en ererur : >").Append(sInitValues).AppendLine("<");
                    sDevMsg.AppendLine(ex.Message).AppendLine(ex.StackTrace);

                    sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));


                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        sUserMsg.ToString(),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        sDevMsg.ToString()

                        );

                    LaunchError();
                }
                System.Web.UI.WebControls.Table tbMain = new System.Web.UI.WebControls.Table();
                GAUserList.Controls.Add(tbMain);
                tbMain.ID = "UsrLstTab";
                tbMain.CssClass = "mTab";
                tbMain.Attributes.Add("cellspacing", "0");
                tbMain.Attributes.Add("cellpadding", "0");

                #region EN TETES DE COLONNES

                //thead
                TableHeaderRow headerRow = new TableHeaderRow();
                tbMain.Rows.Add(headerRow);
                headerRow.TableSection = TableRowSection.TableHeader;
                headerRow.CssClass = "hdBgCol";
                headerRow.VerticalAlign = VerticalAlign.Top;
                headerRow.Style.Add("cursor", "initial");

                String[] aRes = new String[] { "", eResApp.GetRes(_pref, 195), eResApp.GetRes(_pref, 437) };

                foreach (String res in aRes)
                {


                    //th
                    TableHeaderCell cellCol = new TableHeaderCell();
                    headerRow.Cells.Add(cellCol);
                    cellCol.CssClass = "head";
                    cellCol.Attributes.Add("nomove", "1");

                    //tableau à l'intérieur de la cellule d'en tête
                    System.Web.UI.WebControls.Table cellTable = new System.Web.UI.WebControls.Table();
                    cellCol.Controls.Add(cellTable);
                    cellTable.CssClass = "hdTable";
                    cellTable.Attributes.Add("cellspacing", "0");
                    cellTable.Attributes.Add("cellpadding", "0");

                    TableRow row = new TableRow();
                    cellTable.Rows.Add(row);

                    TableCell cell = new TableCell();
                    row.Cells.Add(cell);
                    cell.CssClass = "hdName";
                    //cell.Attributes.Add("ondblclick", "rdc(event);");
                    if (res.Length == 0)
                    {
                        chkBox = new eCheckBoxCtrl(false, false);
                        cell.Controls.Add(chkBox);
                        chkBox.AddClick("selAllUsr(this);");
                        cellCol.Width = 35;

                    }
                    else
                    {
                        cell.Text = res;

                        // Cellule de resize de la colonne
                        TableCell cellResize = new TableCell();
                        row.Cells.Add(cellResize);
                        cellResize.CssClass = "hdResize";
                        //cellResize.ID = String.Concat("RESIZE_", shortColname);
                        //cellResize.RowSpan = 2;
                        //cellResize.Attributes.Add("ondblclick", "rdc(event);");
                        //cellResize.Attributes.Add("onmousedown", "rd(event);");
                        cellResize.Text = "&nbsp;";

                    }
                }

                #endregion

                #region CORPS DE LA LISTE


                Int32 idxRow = 1; // l'index commence à 1 car la ligne 0 est l'entete de colonne
                foreach (eUser.UserListItem userItem in userListItems)
                {
                    TableRow tr = new TableRow();
                    tbMain.Rows.Add(tr);
                    tr.CssClass = String.Concat("line", idxRow % 2 + 1);
                    tr.ID = String.Concat("line", idxRow);
                    //Cellule de sélection du user

                    TableCell tc = new TableCell();
                    tr.Cells.Add(tc);

                    if (userItem.Type == eUser.UserListItem.ItemType.USER)
                    {
                        chkBox = new eCheckBoxCtrl(false, false);
                        tc.Controls.Add(chkBox);
                        chkBox.ID = String.Concat("usr", idxRow);
                        chkBox.AddClick("activUsr(this);");
                        tr.Attributes.Add("u", userItem.ItemCode);

                        if (userItem.Hidden)
                        {
                            tr.Attributes.Add("h", "1");
                            tr.Style.Add("display", "none");
                        }
                        if (dicInit.ContainsKey(userItem.ItemCode))
                        {
                            chkBox.SetChecked(true);
                        }
                    }


                    //Cellule portant le nom du User ou du groupe

                    tc = new TableCell();
                    tr.Cells.Add(tc);
                    tc.Text = userItem.Libelle;
                    if (userItem.Type == eUser.UserListItem.ItemType.GROUP)
                        tc.CssClass = "Grp";
                    else
                        tc.CssClass = "Usr";


                    //Cellule du nbre de fiches à affecter au user

                    tc = new TableCell();
                    tr.Cells.Add(tc);
                    if (dicInit.ContainsKey(userItem.ItemCode))
                    {
                        tc.Text = dicInit[userItem.ItemCode].ToString();
                    }
                    if (userItem.Type == eUser.UserListItem.ItemType.USER)
                    {

                        tc.Attributes.Add("onclick", "repNbEditor.onClick(this);");
                    }
                    idxRow++;

                }
                #endregion

                #region Afficher les utilisateurs masqués
                eCheckBoxCtrl cbCatalogValue = new eCheckBoxCtrl(false, false);
                cbCatalogValue.AddClass("chkAction");
                cbCatalogValue.AddClick("dispUsrMasked(this);");
                cbCatalogValue.ID = "chkUnmsk";
                cbCatalogValue.Attributes.Add("name", "chkUnmsk");
                cbCatalogValue.Style.Add(HtmlTextWriterStyle.Height, "18px");
                cbCatalogValue.AddText(eResApp.GetRes(_pref, 6251));

                dispHidUsr.Controls.Add(cbCatalogValue);

                #endregion

                #region Afficher tous les utilisateurs ou seulement ceux qui sont sélectionnés
                #region Afficher tous les utilisateurs

                Panel divSubBt = new Panel();
                //divSubBt.Attributes.Add("class", "bt-left");

                HtmlInputRadioButton radioBtn = new HtmlInputRadioButton();
                radioBtn.Name = "DisplayAllOrSelUser";
                radioBtn.Value = "All";
                radioBtn.ID = "rbAll";
                radioBtn.Checked = true;
                radioBtn.Attributes.Add("onclick", "dispOnlySelUsr(false);");
                divSubBt.Controls.Add(radioBtn);

                HtmlGenericControl lblLibRadio = new HtmlGenericControl("label");
                lblLibRadio.InnerHtml = eResApp.GetRes(_pref, 6250);
                lblLibRadio.Attributes.Add("for", "rbAll");
                divSubBt.Controls.Add(lblLibRadio);

                dispOptUsr.Controls.Add(divSubBt);

                #endregion

                #region Afficher les utilisateurs sélectionnés

                //Afficher les utilisateurs sélectionnés
                divSubBt = new Panel();
                //divSubBt.Attributes.Add("class", "bt-right");

                radioBtn = new HtmlInputRadioButton();
                radioBtn.Name = "DisplayAllOrSelUser";
                radioBtn.Value = "Sel";
                radioBtn.ID = "rbSel";
                radioBtn.Attributes.Add("onclick", "dispOnlySelUsr(true);");
                divSubBt.Controls.Add(radioBtn);

                lblLibRadio = new HtmlGenericControl("label");
                lblLibRadio.InnerHtml = eResApp.GetRes(_pref, 6249);
                lblLibRadio.Attributes.Add("for", "rbSel");
                divSubBt.Controls.Add(lblLibRadio);

                dispOptUsr.Controls.Add(divSubBt);

                #endregion


                #endregion



            }
            catch (Exception ex)
            {
                StringBuilder sDevMsg = new StringBuilder();
                StringBuilder sUserMsg = new StringBuilder();

                sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append("Pas de descid"); sDevMsg.AppendLine(ex.Message).AppendLine(ex.StackTrace);

                sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));


                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    sUserMsg.ToString(),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    sDevMsg.ToString()

                    );

                LaunchError();
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }
    }
}