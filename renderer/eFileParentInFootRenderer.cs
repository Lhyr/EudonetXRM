using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Web.UI;
using EudoExtendedClasses;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// classe de rendu des pied de page sur les template et les popup
    /// </summary>
    /// <authors>SPH/KHA</authors>
    /// <date>2012-09-26</date>
    public class eFileParentInFootRenderer : eFileParentInHeadRenderer
    {

        eMainFileRenderer _mainRdr = null;

        /// <summary>
        /// Renderer des pied de page de fiche
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="efRdr"></param>
        public eFileParentInFootRenderer(ePref pref, eMainFileRenderer efRdr)
            : base(pref, efRdr)
        {
            _rType = RENDERERTYPE.FileParentInFoot;
        }

        /// <summary>
        /// Renderer des pied de page de fiche
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ef"></param>
        public eFileParentInFootRenderer(ePref pref, eFile ef)
            : base(pref, ef)
        {
            _rType = RENDERERTYPE.FileParentInFoot;
        }


        protected override bool Init()
        {
            if (_masterRdr != null)
            {
                try
                {
                    _mainRdr = (eMainFileRenderer)_masterRdr;
                }
                catch (Exception e)
                {
                    _eException = e;
                    _sErrorMsg = "eFileParentInFootRenderer ne peut être utilisé qu'avec un eMainFileRenderer ou une classe héritant de celui-ci";
                    return false;
                }
            }
            return base.Init();
        }

        /// <summary>
        /// Surcharge de Build propre au FileHeader
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _pgContainer.CssClass = "divFootInfo";
            _pgContainer.ID = String.Concat("divPrt_", _myFile.ViewMainTable.Alias);
            _pgContainer.Attributes.Add("fmt", "foot");

            System.Web.UI.WebControls.Table tbFooterParentInfo = new System.Web.UI.WebControls.Table();
            _pgContainer.Controls.Add(tbFooterParentInfo);
            tbFooterParentInfo.ID = String.Concat("ftp_", _myFile.ViewMainTable.Alias);

            // Certaines options de champs de liaison (ex : affichage des notes) ne sont pas prises en compte sur certains types de fichiers
            bool bIsMailRenderer = _myFile.ViewMainTable.EdnType == EdnType.FILE_MAIL;
            bool bIsSmsRenderer = _myFile.ViewMainTable.EdnType == EdnType.FILE_SMS;



            //Affectation globale
            Boolean bIsGlobalAffect = false;
            Boolean bIsGlobalInvit = false;
            Boolean bIsPostBack = false;
            Int32 nParentTab = 0;
            if (this._mainRdr != null && this._mainRdr.DicParams != null)
            {
                this._mainRdr.DicParams.TryGetValueConvert("globalaffect", out bIsGlobalAffect);
                this._mainRdr.DicParams.TryGetValueConvert("globalinvit", out bIsGlobalInvit);
                this._mainRdr.DicParams.TryGetValueConvert("parenttab", out nParentTab);
                this._mainRdr.DicParams.TryGetValueConvert("ispostback", out bIsPostBack);
            }

            //Si on vient d'une création d'invitation, les champs de liaisons ne sont pas modifiable
            if (bIsGlobalInvit)
                return true;

            eFieldRecord fldRec;

            //KHA le 11/05/2015 je commence par rechercher des information concernant l'event parent 
            // Si on effectue un ajout par traitement de masse depuis celui-ci, 
            // par défaut il faut reprendre les PP/PM de se dernier.

            Boolean bGlobalAffectFromEVT = false;
            Boolean bGetPPFromParentEVT = false;
            Boolean bGetPMFromParentEVT = false;
            TableLite tabParentEvent = null;
            if (_myFile.ViewMainTable.InterEVT)
            {
                //Liaison parente (EVT) n'est pas affichée si on fait une Affectation globale de la fiche en cours 
                bGlobalAffectFromEVT = bIsGlobalAffect && _myFile.ViewMainTable.InterEVTDescid == nParentTab;
                if (!bGlobalAffectFromEVT)
                {
                    fldRec = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.InterEVTDescid + 1));
                    fldRec.IsMandatory = _myFile.ViewMainTable.InterEVTNeeded;
                    fldRec.RightIsUpdatable = _myFile.Record.RightIsUpdatable;

                    //Le seul droit sur le champ de liaison est "InterEventHidden" qui est gérée plus haut
                    // cf 32235
                    fldRec.RightIsVisible = true;

                    TableRow tr = bIsSmsRenderer ? GetFooterTR_SMS_label(fldRec) : GetFooterTR(fldRec);
                    tbFooterParentInfo.Rows.AddAt(0, tr);

                    if (bIsSmsRenderer)
                    {
                        tr = GetFooterTR_SMS_field(fldRec);
                        tbFooterParentInfo.Rows.AddAt(1, tr);
                    }

                    //demande 34 399 - Gestion de l'option InterEventHidden qui permet de masquer la liaison event en mode popup.
                    if (_myFile.ViewMainTable.InterEVTHidden)
                    {
                        tr.Style.Add("display", "none");
                    }
                    else if (!bIsMailRenderer && _myFile.GetParam<String>("HasNote100") != null && _myFile.GetParam<String>("HasNote100").Equals("1"))
                    {
                        fldRec = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.InterEVTDescid + AllField.MEMO_NOTES.GetHashCode()));

                        if (bIsSmsRenderer)
                        {
                            tbFooterParentInfo.Rows.AddAt(1, GetFooterTR_SMS_label(fldRec));
                            tbFooterParentInfo.Rows.AddAt(2, GetFooterTR_SMS_field(fldRec));
                        }
                        else
                            tbFooterParentInfo.Rows.AddAt(1, GetFooterTR(fldRec));


                    }
                }
                else
                {
                    String sError;
                    eudoDAL edal = eLibTools.GetEudoDAL(Pref);
                    edal.OpenDatabase();
                    // On doit vérifier les relations parentes de la table parente
                    tabParentEvent = new TableLite(_myFile.ViewMainTable.InterEVTDescid);
                    try
                    {
                        //56402 
                        if (tabParentEvent.ExternalLoadInfo(edal, out sError))
                        {
                            if (!bIsPostBack)
                            {
                                if (tabParentEvent.InterPP && _myFile.ViewMainTable.InterPP)
                                    bGetPPFromParentEVT = true;

                                if (tabParentEvent.InterPM && _myFile.ViewMainTable.InterPM)
                                    bGetPMFromParentEVT = true;
                            }
                            else// if(bGlobalAffectFromEVT) //Vu le if audessus, on est forcément dans ce cas
                            {
                                //
                                if (_myFile.ViewMainTable.InterPP && _myFile.DicValues.ContainsKey((int)TableType.PP))
                                    bGetPPFromParentEVT = _myFile.DicValues[(int)TableType.PP] == "-1";

                                if (_myFile.ViewMainTable.InterPM && _myFile.DicValues.ContainsKey((int)TableType.PM))
                                    bGetPMFromParentEVT = _myFile.DicValues[(int)TableType.PM] == "-1";
                            }
                        }
                        else
                        {
                            eFeedbackXrm.LaunchFeedback(
                                eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL,
                                    String.Concat("eFileParentInFootRenderer.Build() : Erreur lors de la recherche d'infos concernant la table Event parente (variable tabParentEvent, descid ", _myFile.ViewMainTable.InterEVTDescid, ") : ",
                                    sError)
                                    ),
                                Pref,
                                eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT, "")
                                );
                        }
                    }
                    finally
                    {
                        edal.CloseDatabase();
                    }
                }


            }


            //on affiche pas la liaison société pour les addresses personnelles
            if (_myFile.ViewMainTable.TabType == TableType.ADR
                && _myFile.Record.GetFieldByAlias(String.Format("{0}_{1}", _myFile.ViewMainTable.DescId, (int)AdrField.PERSO))?.Value == "1"
                )
            {
            }
            else if (_myFile.ViewMainTable.InterPM)
            {
                //Liaison parente (PM) n'est pas affichée si on fait une Affectation globale de la fiche en cours 
                if (!bIsGlobalAffect || (TableType.PM.GetHashCode() != nParentTab))
                {

                    fldRec = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", TableType.PM.GetHashCode() + 1));

                    if (bGetPMFromParentEVT || (TableType.PP.GetHashCode() == nParentTab && bIsGlobalAffect))
                    {
                        int tabDidLib = 0;
                        if ((int)TableType.PP == nParentTab)
                            tabDidLib = (int)TableType.PP;
                        else
                            tabDidLib = tabParentEvent != null ? tabParentEvent.DescId : 0;

                        if (tabDidLib != 0)
                        {
                            string sLibTab = eLibTools.GetPrefName(Pref, tabDidLib);

                            if (sLibTab.Length != 0)
                            {
                                fldRec.DisplayValue = String.Concat("<", eResApp.GetRes(Pref, 819), " ", sLibTab, ">");
                                fldRec.Value = "-1";
                                fldRec.FileId = -1;
                                fldRec.FldInfo.Case = CaseField.CASE_NONE;
                            }
                        }
                    }

                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPMNeeded;
                    fldRec.RightIsUpdatable = _myFile.Record.RightIsUpdatable;

                    //Il n'y a pas de droit de visu sur les champs de liaison
                    fldRec.RightIsVisible = true;

                    if (bIsSmsRenderer)
                    {
                        tbFooterParentInfo.Rows.AddAt(0, GetFooterTR_SMS_label(fldRec));
                        tbFooterParentInfo.Rows.AddAt(1, GetFooterTR_SMS_field(fldRec));
                    }
                    else
                        tbFooterParentInfo.Rows.AddAt(0, GetFooterTR(fldRec));

                    if (!bIsMailRenderer && _myFile.GetParam<String>("HasNote300") != null && _myFile.GetParam<String>("HasNote300").Equals("1"))
                    {
                        fldRec = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", TableType.PM.GetHashCode() + AllField.MEMO_NOTES.GetHashCode()));

                        if (bIsSmsRenderer)
                        {
                            tbFooterParentInfo.Rows.AddAt(1, GetFooterTR_SMS_label(fldRec));
                            tbFooterParentInfo.Rows.AddAt(2, GetFooterTR_SMS_field(fldRec));
                        }
                        else
                            tbFooterParentInfo.Rows.AddAt(1, GetFooterTR(fldRec));
                    }
                }

            }

            if (_myFile.ViewMainTable.AdrJoin)
            {
                //Liaison parente (PP) n'est pas affichée si on fait une Affectation globale de la fiche en cours 
                if (!bIsGlobalAffect || ((int)TableType.PP != nParentTab && (int)TableType.ADR != nParentTab))
                {
                    eFieldRecord fldRecAdrPerso = _myFile.Record.GetFieldByAlias(String.Format("{0}_{1}", _myFile.ViewMainTable.DescId, (int)AdrField.PERSO));
                    if (fldRecAdrPerso != null && fldRecAdrPerso.Value != "1")
                    {
                        eFieldRecord fldRecPM = _myFile.Record.GetFieldByAlias(String.Format("{0}_{1}_{2}", _myFile.ViewMainTable.DescId, (int)TableType.ADR, (int)TableType.PM + 1));
                        if (fldRecPM != null && fldRecPM.Value != "")
                        {
                            fldRecPM.RightIsUpdatable = false;
                            fldRecPM.IsMandatory = false;

                            if (bIsSmsRenderer)
                            {
                                tbFooterParentInfo.Rows.AddAt(0, GetFooterTR_SMS_label(fldRecPM, true));
                                tbFooterParentInfo.Rows.AddAt(1, GetFooterTR_SMS_field(fldRecPM, true));
                            }
                            else
                                tbFooterParentInfo.Rows.AddAt(0, GetFooterTR(fldRecPM, true));


                        }
                    }

                    fldRec = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", (int)TableType.ADR + 1));
                    if (bIsSmsRenderer)
                    {
                        tbFooterParentInfo.Rows.AddAt(0, GetFooterTR_SMS_label(fldRec));
                        tbFooterParentInfo.Rows.AddAt(1, GetFooterTR_SMS_field(fldRec));
                    }
                    else
                        tbFooterParentInfo.Rows.AddAt(0, GetFooterTR(fldRec));
                }
            }


            if (_myFile.ViewMainTable.InterPP)
            {
                //Liaison parente (PP) n'est pas affichée si on fait une Affectation globale de la fiche en cours 
                if (!bIsGlobalAffect || ((int)TableType.PP != nParentTab && (int)TableType.ADR != nParentTab))
                {
                    fldRec = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", TableType.PP.GetHashCode() + 1));
                    if (bGetPPFromParentEVT || ((int)TableType.PM == nParentTab && bIsGlobalAffect))
                    {
                        int tabDidLib = 0;
                        if ((int)TableType.PM == nParentTab)
                            tabDidLib = (int)TableType.PM;
                        else
                            tabDidLib = tabParentEvent != null ? tabParentEvent.DescId : 0;

                        if (tabDidLib != 0)
                        {
                            string sLibTab = eLibTools.GetPrefName(Pref, tabDidLib);

                            if (sLibTab.Length != 0)
                            {
                                fldRec.DisplayValue = String.Concat("<", eResApp.GetRes(Pref, 819), " ", sLibTab, ">");
                                fldRec.Value = "-1";
                                fldRec.FileId = -1;
                                fldRec.FldInfo.Case = CaseField.CASE_NONE;
                            }
                        }
                    }

                    fldRec.IsMandatory = _myFile.ViewMainTable.InterPPNeeded;
                    fldRec.RightIsUpdatable = _myFile.Record.RightIsUpdatable;

                    //Il n'y a pas de droit de visu sur les champs de liaison
                    fldRec.RightIsVisible = true;

                    if (bIsSmsRenderer)
                    {

                        tbFooterParentInfo.Rows.AddAt(0, GetFooterTR_SMS_label(fldRec));
                        tbFooterParentInfo.Rows.AddAt(1, GetFooterTR_SMS_field(fldRec));
                    }
                    else
                        tbFooterParentInfo.Rows.AddAt(0, GetFooterTR(fldRec));

                    if (!bIsMailRenderer && _myFile.GetParam<String>("HasNote200") != null && _myFile.GetParam<String>("HasNote200").Equals("1"))
                    {
                        fldRec = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", TableType.PP.GetHashCode() + AllField.MEMO_NOTES.GetHashCode()));
                        if (bIsSmsRenderer)
                        {
                            tbFooterParentInfo.Rows.AddAt(0, GetFooterTR_SMS_label(fldRec));
                            tbFooterParentInfo.Rows.AddAt(1, GetFooterTR_SMS_field(fldRec));
                        }
                        else
                            tbFooterParentInfo.Rows.AddAt(1, GetFooterTR(fldRec));
                    }
                }
            }
            if (!bIsSmsRenderer)
            {
                if ((this._myFile.ViewMainTable.EdnType != EdnType.FILE_PLANNING && this._myFile.ViewMainTable.CalendarEnabled) && tbFooterParentInfo.Rows.Count > 0 && tbFooterParentInfo.Rows[0].Cells.Count > 0)
                {
                    String[] sColumnsWidth = this._myFile.ViewMainTable.ColumnsDisplay.Split(",");
                    if (sColumnsWidth.Length > 0)
                    {
                        TableCell tc = tbFooterParentInfo.Rows[0].Cells[0];
                        tc.Style.Add(HtmlTextWriterStyle.Width, String.Concat(sColumnsWidth[0], "px"));
                    }
                }
            }




            return true;

        }

        /// <summary>
        /// fournit chaque ligne d'informations parentes
        /// </summary>
        /// <param name="fldRec">Champ à afficher</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        /// <returns></returns>
        private TableRow GetFooterTR(eFieldRecord fldRec, Boolean forceReadOnly = false)
        {
            TableRow tr = new TableRow();
            AddFieldCellsToTR(fldRec, tr, forceReadOnly: forceReadOnly);

            return tr;

        }

        /// <summary>
        /// fournit chaque ligne d'informations parentes
        /// </summary>
        /// <param name="fldRec">Champ à afficher</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        /// <returns></returns>
        private TableRow GetFooterTR_SMS_label(eFieldRecord fldRec, Boolean forceReadOnly = false)
        {
            TableRow tr = new TableRow();
            AddLabelCellsToTR_SMS(fldRec, tr, forceReadOnly: forceReadOnly);

            return tr;

        }

        // <summary>
        /// fournit chaque ligne d'informations parentes
        /// </summary>
        /// <param name="fldRec">Champ à afficher</param>
        /// <param name="forceReadOnly">Force le rendu de la rubrique en lecture seule</param>
        /// <returns></returns>
        private TableRow GetFooterTR_SMS_field(eFieldRecord fldRec, Boolean forceReadOnly = false)
        {

            TableRow tr = new TableRow();
            AddFieldCellsToTR_SMS(fldRec, tr, forceReadOnly: forceReadOnly);

            return tr;
        }

        /// <summary>
        /// Fait un rendu du champ de type char 
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">classe CSS</param>
        /// <param name="sClassAction">le type d action</param>
        protected override Boolean RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref String sClassAction)
        {
            //#39838 Pour les liaisons de cible etendu en mode création (popup)
            if (fieldRow.IsLink && _myFile.ViewMainTable.ProspectEnabled && _myFile.FileId == 0)
            {
                WebControl webControl = ednWebControl.WebCtrl;

                // Dans le cas de la création d'une cible étendue, on peux rensigner la fiche avec les champs mappés : Nom, Prénom etc. Puis, si l'utilistaeur lie la fiche en cours(id=0) à un PP, les données saisies seront perdues
                // Donc, on doit informer l'utilisateur de ce fait. Pour cela, on ajoute l'attribut "prospect" pour informer le js. celui -i afin qu'il propose une confirmation.
                webControl.Attributes.Add("prospect", "1");
            }

            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }


        protected override void SetTableCellColspan(TableCell tcValue, eFieldRecord fldRec)
        {
            return;

        }
    }
}