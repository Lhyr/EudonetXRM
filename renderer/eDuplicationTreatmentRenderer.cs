using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer de la duplication en masse
    /// </summary>
    public class eDuplicationTreatmentRenderer : eBaseWizardRenderer
    {


        #region construction du rendu
        /// <summary>
        /// Génère un renderer paramétrés pour l'assistant invitation et le retourne
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="tab">Table des invitations</param>
        /// <param name="nTabFrom">Table de l'évènement</param>
        /// <param name="nParentFileId">Id de la fiche event</param>
        /// <param name="bDelete">Mode suppression</param>
        /// <returns>Renderer contenant l'interface graphique de l'assistant</returns>
        public static eDuplicationTreatmentRenderer GetDuplicationTreatmentWizardRenderer(ePref ePref, Int32 tab, Int32 width, Int32 height)
        {
            return new eDuplicationTreatmentRenderer(ePref, tab, width, height);
        }


        /// <summary>
        /// Constructeur du renderer
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nTabFrom"></param>
        /// <param name="nParentFileId"></param>
        /// <param name="bDelete"></param>
        private eDuplicationTreatmentRenderer(ePref ePref, Int32 nTab, Int32 width, Int32 height)
        {
            _nbStep = 2;
            Pref = ePref;
            _tab = nTab;
            PgContainer.ID = "wizard";
            _width = width;
            _height = height;

            //Liste des ressources nécessaires
            _sListRes = _tab.ToString();

            //Etapes du wizard
            lstStep.Add(new WizStep(1, eResApp.GetRes(Pref, 6380), BuildFields)); // Sélection des rubriques
            lstStep.Add(new WizStep(2, eResApp.GetRes(Pref, 6829), BuildBkm)); // sélection des signets


        }

        #endregion



        /// <summary>
        /// JS spécifiques
        /// </summary>
        /// <returns></returns>
        public override string GetInitJS()
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("  var oDuppiWizard = new nsDuplicationTreatment.eDuplicationWizard() ;");
            sb.AppendLine("  var iCurrentStep = 1;");
            sb.AppendLine("  var iTotalSteps = 2;");
            sb.AppendLine("   Init('duplitreat');");


            sb.AppendLine(" function onLoadDupliTreat(){ oDuppiWizard.InitNoClone();};");

            return sb.ToString();

        }




        #region Création des conteneurs d'étapes


        /// <summary>
        /// Création de l'étape de sélection des bookmark à dupliquer
        /// Etape 2
        /// </summary>
        /// <returns></returns>
        private Panel BuildBkm()
        {
            eRenderer e = eRendererFactory.CreateDuplicationBkmsRenderer(Pref, _tab);
            e.PgContainer.ID = "dupliTreatSelectBkm";
            return e.PgContainer;
        }




        /// <summary>
        /// Création de l'étape de sélection des rubriques à dupliquer et de leur valeurs
        /// Etape 1
        /// </summary>
        /// <returns></returns>
        private Panel BuildFields()
        {
            Panel pnSelectFields = new Panel();
            pnSelectFields.ID = "dupliTreatSelectFields";
            pnSelectFields.Attributes.Add("tab", _tab.ToString());
            // Phrase d'intro
            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = eResApp.GetRes(Pref, 6827);
            pnSelectFields.Controls.Add(p);

            Panel pnl = new Panel();
            pnSelectFields.Controls.Add(pnl);
            pnl.Style.Add("height", "400px");
            pnl.Style.Add("overflow", "auto");

            System.Web.UI.WebControls.Table tb = new System.Web.UI.WebControls.Table();
            pnl.Controls.Add(tb);

            #region Entête du tableau
            TableHeaderRow thr = new TableHeaderRow();
            thr.TableSection = TableRowSection.TableHeader;
            thr.CssClass = "tableHeader";
            TableHeaderCell thc = new TableHeaderCell();
            thc.Text = "";
            thc.CssClass = "checkCol";
            thr.Controls.Add(thc);
            thc = new TableHeaderCell();
            thc.Text = eResApp.GetRes(Pref, 222);
            thr.Controls.Add(thc);
            thc = new TableHeaderCell();
            thc.Text = eResApp.GetRes(Pref, 6828);
            thr.Cells.Add(thc);

            tb.Rows.Add(thr);
            #endregion

            //Format  supporté pour la duplication personalisée
            List<FieldFormat> lstFormatAllowed = new List<FieldFormat>(){
                    FieldFormat.TYP_CHAR,
                    FieldFormat.TYP_DATE,
                    FieldFormat.TYP_BIT,
                    FieldFormat.TYP_MONEY,
                    FieldFormat.TYP_EMAIL,
                    FieldFormat.TYP_WEB,
                    FieldFormat.TYP_SOCIALNETWORK,
                    FieldFormat.TYP_USER,
                    FieldFormat.TYP_MEMO,
                    FieldFormat.TYP_NUMERIC,
                    FieldFormat.TYP_PHONE
            };

            //Descid exclu de la duplication personnalisé
            List<Int32> lstDescidExcluded = new List<int>(){

                    //certains champs de campagne
                    CampaignField.PARENTFILEID.GetHashCode(),
                    CampaignField.MAILTEMPLATEID.GetHashCode(),
                    CampaignField.MAILTEMPLATESYSID.GetHashCode(),
                    CampaignField.ADDRESSMAIN.GetHashCode(),
                    CampaignField.ADDRESSACTIVE.GetHashCode(),
                    CampaignField.REQUEST.GetHashCode(),
                    CampaignField.BODYCSS.GetHashCode(),

                    // créé par/le/modifé par/le
                    _tab + AllField.DATE_CREATE.GetHashCode(),
                    _tab + AllField.DATE_MODIFY.GetHashCode(),
                    _tab + AllField.USER_MODIFY.GetHashCode(),
                    _tab + AllField.USER_CREATE.GetHashCode(),

                    //Confidentiel
                    _tab + AllField.CONFIDENTIAL.GetHashCode(),
                    
                    //Champ system de notes                    
                    _tab + AllField.MEMO_INFOS.GetHashCode(),
                    _tab + AllField.MEMO_DESCRIPTION.GetHashCode(),

                    // Geo
                    _tab + AllField.GEOGRAPHY.GetHashCode(),

                    // HLA - Duplication d'adresse - #70971
                    (int)AdrField.PERSO,
            };

            eDataFillerGeneric dtf = new eDataFillerGeneric(Pref, _tab, viewQuery: ViewQuery.FILE);

            dtf.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
            {
                eq.SetTmpValues = true;
            };

            dtf.RqComplementaryParam = (dal, rq, query) => eFileTools.AddParamInFileRequest(dal, query, rq,
                new Dictionary<int, string>(), 0, new eFileTools.eFileContext(new eFileTools.eParentFileId(), Pref.User, _tab, 0, 0));

            dtf.Generate();

            if (dtf.ErrorMsg.Length > 0)
            {
                if (dtf.InnerException != null)
                    throw dtf.InnerException;
                else
                    throw new Exception(dtf.ErrorMsg);
            }

            eRecord eFirstRecord = dtf.GetFirstRow();
            if (eFirstRecord == null)
                throw new Exception("Impossible de simuler un nouvelle enregistrement");

            // Création de la liste des champs dispo (tri : disporder, sauf champ system)
            Int32 nIndex = 0;
            foreach (eFieldRecord fr in eFirstRecord.GetFields.OrderBy(t => t.FldInfo.Descid % 100 < 70 || t.FldInfo.Descid % 100 == 75 ? t.FldInfo.PosDisporder : t.FldInfo.Descid))
            {

                Field f = fr.FldInfo;
                //liaisons parentes
                if (f.Table.DescId != _tab && f.Descid % 100 == 1)
                {
                    f.Libelle = f.Table.Libelle;
                    fr.IsMandatory = false;
                    if (f.Table.DescId == (int)TableType.PP)
                        fr.IsMandatory = dtf.ViewMainTable.InterPPNeeded;
                    else if (f.Table.DescId == (int)TableType.PM)
                        fr.IsMandatory = dtf.ViewMainTable.InterPMNeeded;
                    else if (f.Table.DescId != (int)TableType.ADR)
                        fr.IsMandatory = dtf.ViewMainTable.InterEVTNeeded;

                    //fr.IsLink = true;

                }

                if (!f.DrawField)
                    continue;

                if (!f.PermViewAll || !f.PermUpdateAll)
                    continue;

                if (!lstFormatAllowed.Contains(f.Format) || lstDescidExcluded.Contains(f.Descid))
                    continue;


                TableRow tr = new TableRow();
                tr.CssClass = (nIndex % 2 == 0) ? "list_even" : "list_odd";
                tb.Rows.Add(tr);

                // case à cocher
                TableCell tcCheck = new TableCell();
                tr.Cells.Add(tcCheck);

                HtmlGenericControl chk = eTools.GetCheckBoxOption("", "chkDup" + f.Descid.ToString(), false, false, "", "oDuppiWizard.onCheckFieldDuplicate");
                chk.Attributes.Add("edndescid", f.Descid.ToString());
                chk.Attributes.Add("ednidx", nIndex.ToString());

                if (f.NoDefaultClone)
                {
                    chk.Attributes.Add("ednautoload", "1");
                }

                chk.Attributes.Add("edntype", fr.FldInfo.Format.GetHashCode().ToString());

                chk.Attributes.Add("edndefaultvalue", fr.DisplayValue);
                chk.Attributes.Add("edndefaultvaluedb", fr.Value);

                chk.Attributes.Add("ednobligat", f.Obligat ? "1" : "0");
                chk.Attributes.Add("ednlabel", f.Libelle);
                chk.Attributes.Add("ednbound", f.BoundDescid.ToString());

                tcCheck.Controls.Add(chk);

                // libellé
                TableCell tbLib = new TableCell();
                tr.Cells.Add(tbLib);
                HtmlGenericControl label = new HtmlGenericControl("span");
                label.InnerHtml = HttpUtility.HtmlEncode(f.Libelle);
                tbLib.Controls.Add(label);

                if (fr.IsMandatory)
                {
                    HtmlGenericControl divOb = new HtmlGenericControl("span");
                    divOb.Attributes.Add("class", "MndAst");
                    divOb.InnerHtml = "*";
                    tbLib.Controls.Add(divOb);

                }


                // valeur
                TableCell tbVal = new TableCell();
                tbVal.Attributes.Add("class", "duplival");
                tbVal.ID = String.Concat("td_dupli_val_", nIndex.ToString());
                tr.Cells.Add(tbVal);

                HtmlInputText inpt = new HtmlInputText();
                inpt.ID = "inptDup" + f.Descid.ToString();
                inpt.Disabled = true;
                inpt.Attributes.Add("edndup", "1");
                inpt.Attributes.Add("edndescid", f.Descid.ToString());

                tbVal.Controls.Add(inpt);

                nIndex++;
            }




            return pnSelectFields;
        }


        #endregion
    }




}
