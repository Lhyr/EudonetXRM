using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminConditionsListDialogRenderer
    /// </summary>
    public class eAdminConditionsListDialogRenderer : eAdminRightsRenderer
    {


        #region Propriétés



        /// <summary>
        /// Id de la modal du renderer
        /// </summary>
        private string _sIdModal = "";

        private eRules.ConditionsFiltersConcerning _typeFilter = eRules.ConditionsFiltersConcerning.ALL;

        /// <summary>
        /// Table "parente" pour le cas du paramétrage des signets
        /// </summary>
        private Int32 _nParentTab;

        /// <summary>
        /// Info sur la table de la conditions
        /// </summary>
        private eAdminTableInfos _tabInfos;


        #endregion

        #region Constructeur

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nDescId"></param>
        /// <param name="nParentTab">Id table parente</param>
        /// <param name="sIdModal"></param>
        /// <param name="typ"></param>
        private eAdminConditionsListDialogRenderer(ePref pref, int nDescId, int nParentTab, string sIdModal, eRules.ConditionsFiltersConcerning tp = eRules.ConditionsFiltersConcerning.CURRENT_TAB) : base(pref, nDescId)
        {
            Pref = pref;
            _tab = nDescId - nDescId % 100;
            _descid = nDescId;
            _sIdModal = sIdModal;
            _nParentTab = nParentTab;
            _typeFilter = tp;
            if (_descid != _tab)
                _typeFilter = eRules.ConditionsFiltersConcerning.FIELDS;

            _tabInfos = new eAdminTableInfos(Pref, _tab);
        }


        /// <summary>
        /// Accessur vers le construteur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nDescId"></param>
        /// <param name="nParentTab"></param>
        /// <param name="typ"></param>
        /// <param name="sIdModal">Id de la modal - utilisé pour le JS</param>
        /// <param name="nFilter">Filtre sur les droits</param>
        /// <returns></returns>
        public static eAdminConditionsListDialogRenderer GetAdminConditionsListDialogRenderer(ePref pref, int nDescId, int nParentTab, string sIdModal, eRules.ConditionsFiltersConcerning nFilter = eRules.ConditionsFiltersConcerning.CURRENT_TAB)
        {
            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            return new eAdminConditionsListDialogRenderer(pref, nDescId, nParentTab, sIdModal, nFilter);

        }
        #endregion


        #region Override




        protected override bool Build()
        {

            _pgContainer.ID = "rightsAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");


            try
            {
                if (!ListOnly)
                {
                    CreateHeaderFilters();
                    CreateTableFilters();
                    CreateActionsButtons();
                }

                CreateListRight();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }


            return true;
        }

        protected override void CreateTableFilters()
        {

        }

        protected override void CreateActionsButtons()
        {

        }

        protected override void CreateListRight()
        {
            String tidList = String.Empty;
            Panel wrapper = new Panel();
            wrapper.ID = "tableWrapper";

            System.Web.UI.WebControls.Table tblList = new System.Web.UI.WebControls.Table();
            tblList.ID = "tableFilters";
            tblList.CssClass = "admCondintionList";

            TableHeaderRow trHead = new TableHeaderRow();
            trHead.TableSection = TableRowSection.TableHeader;
            trHead.CssClass = "tHeadCSS";
            tblList.Rows.Add(trHead);

            #region titres colonnes

            // Description du comportement conditionnels
            TableHeaderCell tcType = new TableHeaderCell();
            trHead.Cells.Add(tcType);
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = "Libellé"; // Libellé
            tcType.Controls.Add(label);



            // Elément sur lequel porte les comportement conditionnels
            TableHeaderCell tcTab = new TableHeaderCell();
            trHead.Cells.Add(tcTab);
            label = new HtmlGenericControl("label");
            label.InnerText = "Elément"; // élément
            tcTab.Controls.Add(label);


            //Comportement
            TableHeaderCell tcFrom = new TableHeaderCell();
            trHead.Cells.Add(tcFrom);
            label = new HtmlGenericControl("label");
            label.InnerText = "Comportement"; //  
            tcFrom.Controls.Add(label);
            #endregion


            // Compteur pour l'alternance des lignes
            int count = 0;
            //List<Tuple<string, eRules>> le = GetListRules();

            List<Tuple<string, eRules>> le = eRules.GetAllRules(_typeFilter, Pref, new List<int>() { _descid });



            #region ressources tables liées

            string sLstRes = _tab.ToString();

            if (_tabInfos.InterPM)
            {
                if (sLstRes.Length > 0)
                    sLstRes = String.Concat(sLstRes, ",");

                sLstRes = "300";
            }
            if (_tabInfos.InterPP)
            {
                if (sLstRes.Length > 0)
                    sLstRes = String.Concat(sLstRes, ",");

                sLstRes = String.Concat(sLstRes, "200");
            }

            if (_tabInfos.InterEVT)
            {
                if (sLstRes.Length > 0)
                    sLstRes = String.Concat(sLstRes, ",");

                sLstRes = String.Concat(sLstRes, _tabInfos.InterEVTDescid);
            }
            eRes res = new eRes(Pref, sLstRes);

            #endregion


            #region lignes de rules

            //Parcour des rules
            foreach (Tuple<string, eRules> tpl in le)
            {

                eRules currRules = tpl.Item2;

                if (currRules.AllRules.Count > 0)
                    currRules.CheckRules();

                string sLabel = tpl.Item1;

                //07/12/06  vu avec rma, finalement, on n'affiche pas toutes les conditons depuis la liste d'un bkm
                if (_nParentTab != 0 && _nParentTab != _tab && currRules.ParentTab != _nParentTab)
                    continue;

                //07/12/06  vu avec rma, finalement, on n'affiche pas les conditons type bkm depuis la liste d'un onglet
                if (_nParentTab == 0 && currRules.ParentTab != _nParentTab && currRules.ParentTab != 0)
                    continue;


                TableRow tr = new TableRow();
                tr.TableSection = TableRowSection.TableBody;
                tr.CssClass = (count % 2 != 0) ? "alternateLine lineEven" : "lineOdd";
                tblList.Rows.Add(tr);


                #region cellule description + bouton actions

                // Description du comportement conditionnels
                TableCell tcTabV = new TableCell();
                tr.Cells.Add(tcTabV);


                //Description

                label = new HtmlGenericControl("label");

                string sLabelRules = GetLabel(currRules);
                if (currRules.ParentTab > 0 && currRules.ParentTab != currRules.Tab)
                {
                    //    sLabel = sLabel.Replace("<PARENTTAB>", re.ParentTab.ToString());
                    if (currRules.Tab != _tab)
                        sLabelRules = sLabelRules.Replace("<PARENTTAB>", _tabInfos.TableLabel);
                    else
                    {
                        bool bfound;
                        sLabelRules = sLabelRules.Replace("<PARENTTAB>", res.GetRes(currRules.ParentTab, out bfound));
                    }
                }
                label.InnerHtml = sLabelRules;
                Panel pnDesc = new Panel();
                pnDesc.CssClass = "divDescConditionsList";
                pnDesc.Controls.Add(label);



                //bouton action
                Panel pnButtons = new Panel();
                pnButtons.CssClass = "divActionConditionsList logo_modifs";
                Panel pnBtn = new Panel();
                pnButtons.Controls.Add(pnBtn);
                pnBtn.ToolTip = eResApp.GetRes(Pref, 1229);
                pnBtn.CssClass = "icon-edn-pen";


                pnBtn.Attributes.Add("onclick", String.Concat("top.nsAdmin.confConditions(", (int)currRules.Type, ",", tpl.Item2.DescId, ",", currRules.ParentTab, ",'LIST') "));


                //ajout des élément
                tcTabV.Controls.Add(pnButtons);
                tcTabV.Controls.Add(pnDesc);


                #endregion


                #region cellule élément concerné
                //Onglet
                TableCell tcTypeV = new TableCell();
                tr.Cells.Add(tcTypeV);
                label = new HtmlGenericControl("label");
                label.InnerText = sLabel;
                tcTypeV.Controls.Add(label);

                #endregion


                #region cellule comportement
                //Comportement
                TableCell tcFromV = new TableCell();
                tr.Cells.Add(tcFromV);
                label = new HtmlGenericControl("label");
                label.InnerText = GetBehavior(currRules);
                tcFromV.Controls.Add(label);
                #endregion

                tidList = String.Concat(!String.IsNullOrEmpty(tidList) ? tidList + ";" : "", currRules.Type);

                count++;

            }

            #endregion


            HiddenField hidTidList = new HiddenField();
            hidTidList.ID = "hidTidList";

            wrapper.Controls.Add(hidTidList);
            wrapper.Controls.Add(tblList);



            _pgContainer.Controls.Add(wrapper);
        }


        /// <summary>Génération des filtres "Onglet" et "Type"</summary>
        protected override void CreateHeaderFilters()
        {
            Panel panel = new Panel();
            panel.ID = "headerFilters";

            // Onglets
            Panel field = new Panel();
            field.CssClass = "field";
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 264), " :");
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlListTabs";
            ddl.SelectedValue = _tab.ToString();
            ddl.Attributes.Add("onchange", "nsConditions.refreshConditionsLaunch();");

            Dictionary<int, String> tabs = eSqlDesc.LoadTabs(Pref, new int[] { (int)TableType.DOUBLONS });
            ddl.DataSource = tabs;
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.DataBind();

            field.Controls.Add(label);
            field.Controls.Add(ddl);

            panel.Controls.Add(field);

            // Type
            field = new Panel();
            field.CssClass = "field";
            label = new HtmlGenericControl("label");
            label.InnerText = "Concerne :";
            ddl = new DropDownList();
            ddl.ID = "ddlListTypes";
            ddl.Attributes.Add("onchange", "nsConditions.refreshConditionsLaunch();");


            field.Controls.Add(label);
            field.Controls.Add(ddl);


            //
            List<ListItem> lst = new List<ListItem>();
            lst.Add(new ListItem("Les signets", ((int)eRules.ConditionsFiltersConcerning.BKMS).ToString()));
            lst.Add(new ListItem("Les rubriques", ((int)eRules.ConditionsFiltersConcerning.FIELDS).ToString()));
            lst.Add(new ListItem("L'onglet", ((int)eRules.ConditionsFiltersConcerning.CURRENT_TAB).ToString()));
            lst.Add(new ListItem("Tous", ((int)eRules.ConditionsFiltersConcerning.ALL).ToString()));
            ddl.Items.AddRange(lst.ToArray()); ddl.SelectedIndex = ddl.Items.IndexOf(ddl.Items.FindByValue(((int)_typeFilter).ToString()));
            panel.Controls.Add(field);

            _pgContainer.Controls.Add(panel);
        }


        /// <summary>
        /// retourne le libelle de la condition
        /// </summary>
        /// <param name="re"></param>
        /// <returns></returns>
        private string GetLabel(eRules re)
        {

            bool bHasRules = re.AllRules.Count > 0;

            string sLabel = "";

            switch (re.Type)
            {
                case TypeTraitConditionnal.Undefined:
                    break;
                case TypeTraitConditionnal.Update:

                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7498);  //"La fiche est modifiable selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7499);  //"Dans tous les cas, la fiche est modifiable";

                    break;
                case TypeTraitConditionnal.Delete:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7500);  //"La fiche est supprimable selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7501);// "Dans tous les cas, la fiche est supprimable";
                    break;
                case TypeTraitConditionnal.Color:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7502);// "Un pictogragmme est affiché de manière conditionnelle";
                    else
                        sLabel = eResApp.GetRes(Pref, 7503);// "Dans tous les cas, le pictogramme par défaut est affiché";
                    break;
                case TypeTraitConditionnal.Header_View:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7504);// "Les entête de la fiche sont visibles selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7505);//  "Dans tous les cas, les entête de la fiche sont visibles";
                    break;
                case TypeTraitConditionnal.Header_Update:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7506);//  "Les entête de la fiche sont modifiables selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7507);// "Dans tous les cas, les entête de la fiche sont modifiables";
                    break;
                case TypeTraitConditionnal.Notification:
                    break;
                case TypeTraitConditionnal.Export:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7508);//"L'export est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7509);//" Dans tous les cas, l'export est autorisé";
                    break;
                case TypeTraitConditionnal.Mailing:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7510);//"Le mailing est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7511); //"Dans tous les cas, le mailing est autorisé";
                    break;

                case TypeTraitConditionnal.Merge:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7512); // "Le publipostable est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7513); // "Dans tous les cas, le publipostage est autorisé";
                    break;
                case TypeTraitConditionnal.Faxing:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7514); //"Le faxing est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7515); // "Dans tous les cas, le faxing est autorisé";
                    break;
                case TypeTraitConditionnal.Voicing:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7516); //"Le voicing est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7517); // "Dans tous les cas, le faxing est autorisé";
                    break;
                case TypeTraitConditionnal.BkmView:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7518); // "Le signet est visible depuis <PARENTTAB> selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7519); //"Dans tous les cas, le signet est visible depuis '<PARENTTAB>'";
                    break;
                case TypeTraitConditionnal.BkmAdd:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7520); // "L'ajout en signet depuis '<PARENTTAB>' est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7521); // "Dans tous les cas, il est possible d'ajouter des fiches depuis '<PARENTTAB>'";
                    break;
                case TypeTraitConditionnal.BkmDelete:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7522); //  "La suppression en signet depuis ? est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7523); //  "Dans tous les cas, il est possible de supprimer des fiches depuis '<PARENTTAB>'";
                    break;
                case TypeTraitConditionnal.BkmUpdate:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7524); //  "La mise à jour en signet depuis <TAB> est autorisé selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7525); //   "Dans tous les cas, il est possible de mettre des fiches depuis '<PARENTTAB>'";
                    break;
                case TypeTraitConditionnal.FieldView:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7526); // "Le champ est visible selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7527); // "Dans tous les cas, le champ est visible";
                    break;
                case TypeTraitConditionnal.FieldObligat:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7528); // "La saisie du champ est obligatoire selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7529); // "Dans tous les cas, la saisie du champ est obligatoire/facultative";
                    break;
                case TypeTraitConditionnal.FieldUpdate:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7530); // "Le champ est modifiable selon une condition";
                    else
                        sLabel = eResApp.GetRes(Pref, 7531); //  "Dans tous les cas, le champ est modifiable";
                    break;
                case TypeTraitConditionnal.FieldForbidClone:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7933); // La rubrique est duplicable selon une condition
                    else
                        sLabel = eResApp.GetRes(Pref, 7934); // Dans tous les cas, la rubrique est duplicable
                    break;
                case TypeTraitConditionnal.FieldForbidImport:
                    if (bHasRules)
                        sLabel = eResApp.GetRes(Pref, 7949); // L'import d'une valeur dans la rubrique est autorisé selon une condition
                    else
                        sLabel = eResApp.GetRes(Pref, 7950); // Dans tous les cas, l'import d'une valeur dans la rubrique est autorisé
                    break;
                default:
                    sLabel = eResApp.GetRes(Pref, 7532); //  "Condition inconnue";
                    break;
            }


            if (re.ParentTab > 0 && re.ParentTab != re.Tab)
            {
                //    sLabel = sLabel.Replace("<PARENTTAB>", re.ParentTab.ToString());
            }

            return sLabel;

        }


        /// <summary>
        /// Retourne le nom du comportement de la règle
        /// </summary>
        /// <param name="re"></param>
        /// <returns></returns>
        private string GetBehavior(eRules re)
        {
            string sBehav = "";

            switch (re.Type)
            {

                case TypeTraitConditionnal.Update:
                    sBehav = eResApp.GetRes(Pref, 7533); //  "Modification de la fiche";
                    break;
                case TypeTraitConditionnal.Delete:
                    sBehav = eResApp.GetRes(Pref, 7534); //"Suppression de la fiche";
                    break;
                case TypeTraitConditionnal.Color:
                    sBehav = eResApp.GetRes(Pref, 7535); //"Pictogramme";
                    break;
                case TypeTraitConditionnal.Header_View:
                    sBehav = eResApp.GetRes(Pref, 7536); // "Affichage entête";
                    break;
                case TypeTraitConditionnal.Header_Update:
                    sBehav = eResApp.GetRes(Pref, 7537); // "Modification de l'entête";
                    break;
                case TypeTraitConditionnal.Notification:
                    break;
                case TypeTraitConditionnal.Export:
                    sBehav = eResApp.GetRes(Pref, 7538); // "Export";
                    break;
                case TypeTraitConditionnal.Mailing:
                    sBehav = eResApp.GetRes(Pref, 7539); // "Mailing";
                    break;
                case TypeTraitConditionnal.Merge:
                    sBehav = eResApp.GetRes(Pref, 7540); //"Publipostage";
                    break;
                case TypeTraitConditionnal.Faxing:
                    sBehav = eResApp.GetRes(Pref, 7541); // "Faxing";
                    break;
                case TypeTraitConditionnal.Voicing:
                    sBehav = eResApp.GetRes(Pref, 7542); //"Voicing";
                    break;
                case TypeTraitConditionnal.BkmView:
                    sBehav = eResApp.GetRes(Pref, 7543); // "Visualisation en signet";
                    break;
                case TypeTraitConditionnal.BkmAdd:
                    sBehav = eResApp.GetRes(Pref, 7544); // "Ajout en signet";
                    break;
                case TypeTraitConditionnal.BkmDelete:
                    sBehav = eResApp.GetRes(Pref, 7545); // "Suppression en signet";
                    break;
                case TypeTraitConditionnal.BkmUpdate:
                    sBehav = eResApp.GetRes(Pref, 7546); //"Mise à jour en signet";
                    break;
                case TypeTraitConditionnal.FieldView:
                    sBehav = eResApp.GetRes(Pref, 7547); // "Visualisation du champ";
                    break;
                case TypeTraitConditionnal.FieldObligat:
                    sBehav = eResApp.GetRes(Pref, 7548); // "Saisie obligatoire";
                    break;
                case TypeTraitConditionnal.FieldUpdate:
                    sBehav = eResApp.GetRes(Pref, 7549); // "Mise à jour du champ";
                    break;
                case TypeTraitConditionnal.FieldForbidClone:
                    sBehav = eResApp.GetRes(Pref, 7932); // "Duplication d'une valeur"
                    break;
                case TypeTraitConditionnal.FieldForbidImport:
                    sBehav = eResApp.GetRes(Pref, 7948); // "Import d'une valeur"
                    break;
                default:
                    sBehav = eResApp.GetRes(Pref, 7550); // "comportement inconnu";
                    break;
            }

            return sBehav;

        }

        #endregion
    }




}