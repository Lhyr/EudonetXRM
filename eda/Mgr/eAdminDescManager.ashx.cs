using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminDescManager
    /// </summary>
    public class eAdminDescManager : eAdminManager
    {
        protected override void ProcessManager()
        {
            bool success = true;

            eAdminCapsule<eAdminUpdateProperty> caps = null;
            try
            {
                caps = eAdminTools.GetAdminCapsule<eAdminCapsule<eAdminUpdateProperty>, eAdminUpdateProperty>(_context.Request.InputStream);
                caps = CheckCapsule(caps);
            }
            catch (Exception e)
            {
                // TODO: gestion d'erreur
                throw e;
            }

            eAdminResult adminRes = new eAdminResult();

            //répartition des propriétés entre desc et pref
            List<SetParam<ADMIN_PREF>> liPref = new List<SetParam<ADMIN_PREF>>();
            List<SetParam<string>> liPrefDefault = new List<SetParam<string>>();
            List<SetParam<int>> liRes = new List<SetParam<int>>();
            List<SetParam<eLibConst.SPECIFS>> liSpecf = new List<SetParam<eLibConst.SPECIFS>>();
            List<SetParam<ePrefConst.PREF_BKMPREF>> liBkmPref = new List<SetParam<ePrefConst.PREF_BKMPREF>>();
            List<SetParam<eLibConst.FILEDATAPARAM>> liFDParam = new List<SetParam<eLibConst.FILEDATAPARAM>>();
            List<SetParam<KeyResADV>> liResAdv = new List<SetParam<KeyResADV>>();
            List<SetParam<TypeUserValueAdmin>> liUserValue = new List<SetParam<TypeUserValueAdmin>>();
            List<SetParam<TypeUserValueAdmin>> liUserValueSearchAll = new List<SetParam<TypeUserValueAdmin>>();
            List<SetParam<eLibConst.PREF_COLSPREF>> liColsPref = new List<SetParam<eLibConst.PREF_COLSPREF>>();
            List<SetParam<eLibConst.PREF_CONFIG>> listConfig = new List<SetParam<eLibConst.PREF_CONFIG>>();
            List<SetParam<string>> listConfigDefault = new List<SetParam<string>>();
            List<SetParam<eLibConst.TREATID>> liConditionalSend = new List<SetParam<eLibConst.TREATID>>();
            List<RulesDefinition> lstColor = new List<RulesDefinition>();
            List<DescAdvObj> liDescAdv = new List<DescAdvObj>();
            List<SetParam<eConst.SYNC_PARAMETER>> liSyncParam = new List<SetParam<eConst.SYNC_PARAMETER>>();
            List<SetParam<eLibConst.RGPDRuleParam>> liRGPDRuleParam = new List<SetParam<eLibConst.RGPDRuleParam>>();

            // Liste des appartenances user/owner
            List<Tuple<int, int>> liDefaultOwners = new List<Tuple<int, int>>();


            eAdminDesc adminDesc = new eAdminDesc(caps.DescId, caps.ParentTab);
            eAdminTableInfos adminTab = null;
            eAdminTableInfos adminOldTab = null;

            eAdminFieldInfos adminOldField = null;
            eAdminFieldInfos adminField = null;

            bool doGlobalMaJ = false;

            if (caps.DescId % 100 > 0
                && caps.DescId % 100 != AllField.ATTACHMENT.GetHashCode()
                && caps.DescId != (int)TableType.DOUBLONS
                )
            {
                // Cas d'un champ
                adminField = eAdminFieldInfos.GetAdminFieldInfos(_pref, caps.DescId);

                if (adminField != null && adminField.Error.Length > 0)
                    throw new Exception(adminField.Error);

                if (caps.ListProperties.Count == 1
                        && caps.ListProperties[0].Category == eAdminUpdateProperty.CATEGORY.DESC.GetHashCode()
                        && caps.ListProperties[0].Property == eLibConst.DESC.DEFAULT.GetHashCode())
                {
                    // dans le cas ou le manager ne doit traiter qu'une valeur par défaut, on ne
                    // passe pas par tout le système de modification de paramétrage de champs trop
                    // lourd pour pas grand chose

                    //todo verification du format

                    adminDesc.SetDesc(eLibConst.DESC.DEFAULT, caps.ListProperties[0].Value);

                    adminField = null; // pour ne pas retomber dans le Update un peu plus bas
                }
                else
                {
                    adminOldField = eAdminFieldInfos.GetAdminFieldInfos(_pref, caps.DescId);
                }
            }
            else if (caps.DescId > 0)
            {
                // Cas d'une table
                adminOldTab = new eAdminTableInfos(_pref, caps.DescId);
                adminTab = new eAdminTableInfos(_pref, caps.DescId);

                //if (!caps.Confirmed)
                adminDesc.Confirmed = caps.Confirmed;

                //adminDesc.TabInfos = adminTab;
                adminDesc.OldTabInfos = adminOldTab;
            }

            int nPpty = 0;
            string sValue = string.Empty;
            string sError = string.Empty;
            Int32 iValue = 0;

            string autoBuildName = string.Empty;
            bool bAutobuildnameSet = false;
            #region Génération de la liste des propriété a MAJ

            try
            {
                eBkmPrefCleaner cleaner;
                foreach (eAdminUpdateProperty pty in caps.ListProperties)
                {
                    nPpty = pty.Property;
                    sValue = pty.Value;

                    eAdminUpdateProperty.CATEGORY cat = (eAdminUpdateProperty.CATEGORY)pty.Category;


                    switch (cat)
                    {
                        case eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT:
                            listConfigDefault.Add(new SetParam<string>(((eLibConst.CONFIG_DEFAULT)pty.Property).ToString(), pty.Value));
                            break;
                        case eAdminUpdateProperty.CATEGORY.CONFIG:

                            listConfig.Add(new SetParam<eLibConst.PREF_CONFIG>((eLibConst.PREF_CONFIG)pty.Property, pty.Value));
                            break;
                        case eAdminUpdateProperty.CATEGORY.DESC:
                            eLibConst.DESC descField = (eLibConst.DESC)pty.Property;



                            if (caps.DescId % 100 == 0
                                || caps.DescId % 100 == AllField.ATTACHMENT.GetHashCode()
                                || caps.DescId == (int)TableType.DOUBLONS
                                )
                            {
                                #region Propriétés de table

                                //test si une liaison existe déjà
                                if (
                                           (descField == eLibConst.DESC.INTERPP && sValue == "1")
                                    || (descField == eLibConst.DESC.INTERPM && sValue == "1")
                                    || descField == eLibConst.DESC.INTEREVENTNUM
                                        )
                                {

                                    //Check si une relation 1-1 existe déjà entre les tables
                                    int nRelTab = 0;

                                    if (descField == eLibConst.DESC.INTERPP)
                                        nRelTab = 200;
                                    else if (descField == eLibConst.DESC.INTERPM)
                                        nRelTab = 300;
                                    else if (descField == eLibConst.DESC.INTEREVENTNUM)
                                    {
                                        if (Int32.TryParse(sValue, out nRelTab))
                                        {
                                            if (nRelTab == 0)
                                                nRelTab = 100;
                                            else
                                            {
                                                nRelTab = (nRelTab + 10) * 100;
                                            }
                                        }
                                    }


                                    int nStartTab = caps.DescId;
                                    List<eFieldRes> lstLiaison = eSqlDesc.GetSpecialPopupFields(_pref, nRelTab, nStartTab);
                                    foreach (eFieldRes lnk in lstLiaison)
                                    {

                                        //Si champ non principale ou liaison principale vers ma fiche de départ
                                        if (!lnk.MainRel || (lnk.MainRel && lnk.Ord == 1))
                                        {

                                            adminRes.Success = false;
                                            adminRes.UserErrorMessage = eResApp.GetRes(_pref, 2417);

                                            adminRes.UserErrorMessage += Environment.NewLine;
                                            adminRes.UserErrorMessage += string.Join(Environment.NewLine, lstLiaison.Select(a => " - " + a.GetCompleteLabel()));

                                            RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(adminRes); });

                                            return;
                                        }
                                    }
                                }




                                if (!bAutobuildnameSet)
                                {
                                    autoBuildName = adminTab.AutoBuildName;
                                    bAutobuildnameSet = true;
                                }

                                if (descField == eLibConst.DESC.NBCOLS)
                                {
                                    #region Nombre de colonnes

                                    pty.Property = eLibConst.DESC.COLUMNS.GetHashCode();

                                    int nColPerLine = eLibTools.GetNum(pty.Value);
                                    if (nColPerLine <= 0)
                                        continue;

                                    if (nColPerLine == adminTab.ColPerLine)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (nColPerLine > adminTab.ColPerLine)
                                        {
                                            StringBuilder sColumns = new StringBuilder(adminTab.Columns);
                                            for (int i = adminTab.ColPerLine; i < nColPerLine; i++)
                                            {
                                                if (i > 0)
                                                {
                                                    sColumns.Append(",");
                                                }
                                                sColumns.Append("70,A,25");
                                            }
                                            pty.Value = sColumns.ToString();
                                        }
                                        else if (nColPerLine < adminTab.ColPerLine)
                                        {
                                            List<string> sColumns = new List<string>();
                                            sColumns.AddRange(adminTab.Columns.Split(','));

                                            //string sColumns = adminTab.Columns;
                                            sColumns.RemoveRange(nColPerLine * 3, sColumns.Count - (nColPerLine * 3));
                                            pty.Value = eLibTools.Join<string>(",", sColumns);
                                        }
                                    }

                                    #endregion Nombre de colonnes
                                }
                                else if (descField == eLibConst.DESC.INTERPP && caps.Confirmed)
                                {
                                    //traitement sur la modif de interpp/..
                                    if (Int32.TryParse(sValue, out iValue) && iValue == 0 && caps.DescId > 0 && caps.DescId % 100 == 0)
                                    {
                                        cleaner = new eBkmPrefCleaner(_pref, caps.DescId, (int)TableType.PP);
                                        cleaner.UpdateBkmPref();
                                    }

                                }
                                else if (descField == eLibConst.DESC.INTERPM && caps.Confirmed)
                                {
                                    if (Int32.TryParse(sValue, out iValue) && iValue == 0 && caps.DescId > 0 && caps.DescId % 100 == 0)
                                    {
                                        cleaner = new eBkmPrefCleaner(_pref, caps.DescId, (int)TableType.PM);
                                        cleaner.UpdateBkmPref();
                                    }
                                }
                                else if (descField == eLibConst.DESC.INTEREVENTNUM && caps.Confirmed)
                                {
                                    if (Int32.TryParse(sValue, out iValue) && iValue > 0 && caps.DescId > 0 && caps.DescId % 100 == 0)
                                    {
                                        cleaner = new eBkmPrefCleaner(_pref, caps.DescId, caps.DescId);
                                        if (cleaner.InterEventTab != null && cleaner.TabLinkedId != caps.DescId)
                                            cleaner.UpdateBkmPref();
                                    }
                                }
                                else if (descField == eLibConst.DESC.HISTORIC)
                                {
                                    #region Historique de suppression

                                    adminTab.EnableDeletionTrigger(pty.Value == "1");

                                    #endregion Historique de suppression
                                }
                                else if (descField == eLibConst.DESC.INTEREVENT)
                                {
                                    if (pty.Value.ToLower() == "true")
                                        pty.Value = "1";
                                    else if (pty.Value.ToLower() == "true")
                                        pty.Value = "0";
                                }


                                adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value);
                                #endregion Propriétés de table

                            }
                            else if (adminField != null)
                            {
                                #region Propriétés du champ

                                switch (descField)
                                {
                                    case eLibConst.DESC.FORMAT:

                                        // Pas de mise à jour si le champ est spécial
                                        // ce cas ne devrait pas se produire mais on ne sais jamais...
                                        if (adminField.IsSpecialField())
                                            break;

                                        adminField.Format = (FieldFormat)eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.LENGTH:
                                        int length = 0;
                                        if (!int.TryParse(pty.Value, out length))
                                            length = int.MaxValue;
                                        adminField.Length = length;
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.CASE:
                                        adminField.Case = (CaseField)eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.TOOLTIPTEXT:
                                        adminField.ToolTipText = pty.Value;
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.DEFAULT:
                                        adminField.Default = pty.Value;
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.HISTORIC:
                                        adminField.Historic = pty.Value == "1";
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.OBLIGAT:
                                        adminField.Obligat = pty.Value == "1";
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value == "1" ? "1" : "0");
                                        break;

                                    case eLibConst.DESC.MULTIPLE:
                                        adminField.Multiple = pty.Value == "1";
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.RELATION:
                                        adminField.Relation = pty.Value == "1";

                                        if (adminField.Relation)
                                        {

                                            if (adminField.PopupDescId == 201 && adminField.Table.InterPP)
                                            {
                                                adminRes.Success = false;
                                                adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1208);

                                                RenderResult(RequestContentType.TEXT, delegate ()
                                                {
                                                    return JsonConvert.SerializeObject(adminRes);
                                                });

                                                return;
                                            }
                                            else if (adminField.PopupDescId == 301 && adminField.Table.InterPM)
                                            {
                                                adminRes.Success = false;
                                                adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1208);

                                                RenderResult(RequestContentType.TEXT, delegate ()
                                                {
                                                    return JsonConvert.SerializeObject(adminRes);
                                                });

                                                return;
                                            }
                                            else if (adminField.PopupDescId == (adminField.Table.InterEVTDescid + 1) && adminField.Table.InterEVT)
                                            {
                                                adminRes.Success = false;
                                                adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1208);

                                                RenderResult(RequestContentType.TEXT, delegate ()
                                                {
                                                    return JsonConvert.SerializeObject(adminRes);
                                                });

                                                return;
                                            }
                                            else
                                            {
                                                //Check si une relation 1-1 existe déjà entre les tables
                                                int nRelTab = adminField.PopupDescId - adminField.PopupDescId % 100;
                                                int nStartTab = adminField.DescId - adminField.DescId % 100;

                                                List<eFieldRes> lstLiaison = eSqlDesc.GetSpecialPopupFields(_pref, nRelTab, nStartTab);
                                                foreach (eFieldRes lnk in lstLiaison)
                                                {

                                                    if (lnk.MainRel // liaison principale
                                                        || (!lnk.MainRel && lnk.Ord == 2 && lnk.DescId != adminField.DescId) // catalogue relation autre
                                                        || (!lnk.MainRel && lnk.Ord == 1) // catalogue relation sur autre fiche

                                                        )
                                                    {
                                                        adminRes.Success = false;
                                                        adminRes.UserErrorMessage = eResApp.GetRes(_pref, 2417);

                                                        adminRes.UserErrorMessage += Environment.NewLine;
                                                        adminRes.UserErrorMessage += string.Join(Environment.NewLine, lnk.GetCompleteLabel());

                                                        RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(adminRes); });

                                                        return;
                                                    }
                                                }

                                            }
                                        }

                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.POPUP:
                                        adminField.PopupType = (PopupType)eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.POPUPDESCID:
                                        adminField.PopupDescId = eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.BOUNDDESCID:
                                        adminField.BoundDescId = eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.ROWSPAN:
                                        adminField.Rowspan = eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.COLSPAN:
                                        adminField.Colspan = eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.DISPORDER:
                                        adminField.Disporder = eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.TABINDEX:
                                        adminField.TabIndex = eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.BOLD:
                                        adminField.Bold = pty.Value == "1";
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value == "1" ? "1" : "0");
                                        break;

                                    case eLibConst.DESC.ITALIC:
                                        adminField.Italic = pty.Value == "1";
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value == "1" ? "1" : "0");
                                        break;

                                    case eLibConst.DESC.UNDERLINE:
                                        adminField.UnderLine = pty.Value == "1";
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value == "1" ? "1" : "0");
                                        break;

                                    case eLibConst.DESC.FLAT:
                                        adminField.Flat = pty.Value == "1";
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value == "1" ? "1" : "0");
                                        break;

                                    case eLibConst.DESC.FORECOLOR:
                                        adminField.ForeColor = pty.Value;
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value);
                                        break;
                                    case eLibConst.DESC.READONLY:
                                        adminField.ReadOnly = pty.Value == "1";
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value == "1" ? "1" : "0");
                                        break;

                                    case eLibConst.DESC.CHANGERULESID:
                                        adminField.ChangeRulesId = pty.Value;
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value);
                                        break;

                                    case eLibConst.DESC.HTML:
                                        adminField.IsHTML = pty.Value == "1";
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.VIEWPERMID:
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.UPDATEPERMID:
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.OBLIGATRULESID:
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value);
                                        if (pty.Value.Length > 0)
                                            adminDesc.SetDesc(eLibConst.DESC.OBLIGAT, "1");
                                        break;

                                    case eLibConst.DESC.LABELALIGN:
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.STORAGE:
                                        adminField.Storage = (ImageStorage)eLibTools.GetNum(pty.Value);
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.UNICODE:
                                        adminField.Unicode = pty.Value == "1";
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.NODEFAULTCLONE:
                                        adminField.NoDefaultClone = pty.Value == "1";
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.VIEWRULESID:
                                        adminField.ViewRulesId = pty.Value;
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value);
                                        break;

                                    case eLibConst.DESC.PROSPECTENABLED:

                                        //TODO gérer les cibles étendues
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.OBLIGATREADONLY:
                                        doGlobalMaJ = true;
                                        break;

                                    case eLibConst.DESC.SEARCHALL:
                                        doGlobalMaJ = true;
                                        adminField.SearchAll = pty.Value == "1";
                                        break;

                                    case eLibConst.DESC.SEARCHALLBLOCKED:
                                        doGlobalMaJ = true;
                                        adminField.SearchAllBlocked = pty.Value == "1";
                                        break;

                                    case eLibConst.DESC.COMPUTEDFIELDENABLED:
                                        doGlobalMaJ = true;
                                        adminField.ComputedFieldEnabled = pty.Value == "1";
                                        break;

                                    case eLibConst.DESC.SHOWFILEID:
                                        doGlobalMaJ = true;
                                        adminField.IsSysId = (pty.Value == "1");
                                        break;

                                    case eLibConst.DESC.TREEVIEWUSERLIST:
                                        doGlobalMaJ = true;
                                        // HLA - Rubrique Compteur identifiant système : problème de calcul de la valeur - #62445
                                        adminField.TreeViewUserList = pty.Value == "1";
                                        break;

                                    case eLibConst.DESC.PARAMETERS:
                                        adminDesc.SetDesc(eLibConst.DESC.PARAMETERS, pty.Value);
                                        if (adminField.Format == FieldFormat.TYP_CHART)
                                            adminField.SetParameters = pty.Value;
                                        break;

                                    case eLibConst.DESC.SCROLLING:
                                        adminField.Scrolling = pty.Value == "1";
                                        adminDesc.SetDesc((eLibConst.DESC)pty.Property, pty.Value);
                                        break;

                                    default:
                                        doGlobalMaJ = true;
                                        break;
                                }


                                #endregion Propriétés du champ
                            }
                            break;

                        case eAdminUpdateProperty.CATEGORY.COLOR_RULES:

                            lstColor = SerializerTools.JsonDeserialize<List<RulesDefinition>>(pty.Value);
                            if (lstColor.Count == 0)
                            {
                                lstColor.Add(RulesDefinition.GetRulesDefinition(TypeTraitConditionnal.Color, "EMPTY", "", "", "", "", new List<Tuple<AdvFilter, InterOperator>>()));
                            }

                            break;

                        case eAdminUpdateProperty.CATEGORY.PREF:
                            if (((ADMIN_PREF)pty.Property) == ADMIN_PREF.DEFAULTOWNER)
                            {
                                liDefaultOwners.Add(new Tuple<int, int>(pty.UserId, eLibTools.GetNum(pty.Value)));
                            }
                            else
                            {
                                if (((ADMIN_PREF)pty.Property) == ADMIN_PREF.ADRJOIN)
                                {
                                    if (Int32.TryParse(sValue, out iValue) && iValue == 0 && caps.DescId > 0 && caps.DescId % 100 == 0)
                                    {
                                        cleaner = new eBkmPrefCleaner(_pref, caps.DescId, (int)TableType.ADR);
                                        cleaner.UpdateBkmPref();
                                    }
                                }
                                if (this is eObjManager)
                                    liPref.Add(new SetParam<ADMIN_PREF>((ADMIN_PREF)pty.Property, pty.Value));
                                else
                                    liPrefDefault.Add(new SetParam<string>(((ADMIN_PREF)pty.Property).ToString(), pty.Value));
                            }

                            break;

                        case eAdminUpdateProperty.CATEGORY.RES:
                            liRes.Add(new SetParam<int>(pty.Property, pty.Value));

                            // Mettre à jour adminField ?
                            break;

                        case eAdminUpdateProperty.CATEGORY.SPECIFS:
                            liSpecf.Add(new SetParam<eLibConst.SPECIFS>((eLibConst.SPECIFS)pty.Property, pty.Value));
                            break;

                        // TODO/TOCHECK MAB : mise à jour de BKMPREF
                        case eAdminUpdateProperty.CATEGORY.BKMPREF:
                            liBkmPref.Add(new SetParam<ePrefConst.PREF_BKMPREF>((ePrefConst.PREF_BKMPREF)pty.Property, pty.Value));
                            break;

                        case eAdminUpdateProperty.CATEGORY.RESADV:
                            KeyResADV r = new KeyResADV((eLibConst.RESADV_TYPE)pty.Property, caps.DescId, _pref.LangId);
                            liResAdv.Add(new SetParam<KeyResADV>(r, pty.Value));
                            break;

                        case eAdminUpdateProperty.CATEGORY.FILEDATAPARAM:
                            liFDParam.Add(new SetParam<eLibConst.FILEDATAPARAM>((eLibConst.FILEDATAPARAM)pty.Property, pty.Value));
                            break;

                        case eAdminUpdateProperty.CATEGORY.USERVALUE:
                            switch ((TypeUserValueAdmin)pty.Property)
                            {
                                case TypeUserValueAdmin.SEARCHALL_EVT:
                                case TypeUserValueAdmin.SEARCHALLBLOCKED_EVT:
                                case TypeUserValueAdmin.SEARCHALL_PP:
                                case TypeUserValueAdmin.SEARCHALLBLOCKED_PP:
                                case TypeUserValueAdmin.SEARCHALL_PMADR:
                                case TypeUserValueAdmin.SEARCHALLBLOCKED_PMADR:
                                    //if (caps.Confirmed) #71226 - CNA - la confirmation n'est pas vérifié plus loin alors il ne faut pas la vérifier ici 
                                    liUserValueSearchAll.Add(new SetParam<TypeUserValueAdmin>((TypeUserValueAdmin)pty.Property, pty.Value));
                                    break;
                                default:
                                    liUserValue.Add(new SetParam<TypeUserValueAdmin>((TypeUserValueAdmin)pty.Property, pty.Value));
                                    break;
                            }
                            break;

                        case eAdminUpdateProperty.CATEGORY.COLSPREF:
                            liColsPref.Add(new SetParam<eLibConst.PREF_COLSPREF>((eLibConst.PREF_COLSPREF)pty.Property, pty.Value));
                            break;

                        case eAdminUpdateProperty.CATEGORY.CONDITIONALSENDINGRULES:
                            liConditionalSend.Add(new SetParam<eLibConst.TREATID>((eLibConst.TREATID)pty.Property, pty.Value));
                            break;

                        case eAdminUpdateProperty.CATEGORY.DESCADV:
                            liDescAdv.Add(DescAdvObj.GetSingle(caps.DescId, (DESCADV_PARAMETER)pty.Property, pty.Value));
                            if ((DESCADV_PARAMETER)pty.Property == DESCADV_PARAMETER.ALIASSOURCE)
                            {
                                doGlobalMaJ = true;
                                adminField.AliasParam = new AliasParam(pty.Value);
                            }
                            break;

                        case eAdminUpdateProperty.CATEGORY.SYNC:
                            liSyncParam.Add(new SetParam<eConst.SYNC_PARAMETER>((eConst.SYNC_PARAMETER)pty.Property, pty.Value));
                            break;

                        case eAdminUpdateProperty.CATEGORY.RGPDRULEPARAM:
                            liRGPDRuleParam.Add(new SetParam<eLibConst.RGPDRuleParam>((eLibConst.RGPDRuleParam)pty.Property, pty.Value));
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (System.Threading.ThreadAbortException) { return; }
            catch (eEndResponseException) { return; }
            catch (InvalidCastException ex)
            {
                //paramètres non valide
                throw new Exception(string.Concat("Le paramètre  [", nPpty, "] valeur [", sValue, "]  n'est pas valide : ", ex.Message));
            }
            catch (UsedCatalogException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Autre erreurs
                adminRes.Success = false;
                adminRes.UserErrorMessage = "Une erreur est survenue lors de la récupération des paramètres à mettre à jour";
                adminRes.DebugErrorMessage = ex.Message;
                adminRes.InnerException = ex;
                LaunchError(
                    eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        adminRes.UserErrorMessage,
                        "",
                        adminRes.DebugErrorMessage
                    )
                );
            }

            #endregion Génération de la liste des propriété a MAJ

            List<Object> lstResult = new List<object>();

            //Transaction
            eLibTools.SetTran(_pref, eLibTools.GetEudoDAL(_pref), "DescManager");

            try
            {
                #region maj des propriétés globale du champ
                if (doGlobalMaJ && adminField != null)
                {
                    adminField.Confirmed = caps.Confirmed;
                    if (adminOldField != null)
                        adminOldField.Confirmed = caps.Confirmed;




                    //#77 785, KJE: si les types sql sont trop différents, on supprime et on recrée complètement la colonne
                    if (adminField.Confirmed && adminField.CheckIfNeedReCreate(adminOldField))
                    {
                        //on supprime le colone
                        adminRes = adminField.Drop(out sError);
                        if (adminRes != null && !adminRes.Success && adminRes.DebugErrorMessage.Length > 0 && !adminRes.NeedConfirm)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), adminRes.UserErrorMessage, eResApp.GetRes(_pref, 8876), adminRes.DebugErrorMessage);
                            LaunchError();
                        }

                        //on recréee la colonne
                        adminRes = adminField.Create(caps);
                        if ((!adminRes.Success && adminRes.DebugErrorMessage.Length > 0) || sError.Length > 0)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), adminRes.UserErrorMessage, eResApp.GetRes(_pref, 8875), adminRes.DebugErrorMessage);
                            LaunchError();
                        }
                        goto sendFinalResult;
                    }

                    adminRes = adminField.Update(adminOldField);

                    if (adminRes != null && !adminRes.Success && !adminRes.NeedConfirm)
                    {
                        if (adminRes.DebugErrorMessage.Length > 0)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "Une erreur s'est produite lors de la mise à jour de la rubrique",
                                adminRes.DebugErrorMessage
                            );
                        }

                        if (adminRes.UserErrorMessage.Length > 0)
                        {
                            ErrorContainer = eErrorContainer.GetUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "Une erreur s'est produite lors de la mise à jour de la rubrique"
                            );
                        }

                        LaunchError();
                    }

                }

                #endregion


                #region MAJ PREF

                // Mise à jour de PREF
                if (liPref.Count > 0 || liPrefDefault.Count > 0)
                {
                    try
                    {
                        if (liPref.Count > 0)
                            success = _pref.SetPrefFromAdmin(caps.DescId, liPref);

                        if (liPrefDefault.Count > 0)
                            success = _pref.SetPrefDefault(caps.DescId, liPrefDefault);

                        adminRes.Success = success;

                        if (success && adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);
                    }
                    catch (Exception e)
                    {
                        adminRes.Success = false;
                        adminRes.UserErrorMessage = "Mise à jour non effectuée";
                        adminRes.DebugErrorMessage = e.Message;
                        adminRes.InnerException = e;
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                }

                #endregion MAJ PREF

                #region MAJ RES

                //Mise à jour de RES
                if (liRes.Count > 0)
                {
                    eAdminRes res = new eAdminRes(_pref);
                    adminRes = res.SetRes(caps.DescId, liRes);
                    if (!adminRes.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                    else
                    {
                        if (adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);
                    }
                }

                #endregion MAJ RES

                #region MAJ FILEDATAPARAM

                //Mise à jour de Filedataparam
                if (liFDParam.Count > 0)
                {
                    eAdminFiledataParam fdparam = new eAdminFiledataParam(caps.DescId);
                    fdparam.SetDescList(liFDParam);

                    adminRes = fdparam.Save(_pref);
                    if (!adminRes.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                    else
                    {
                        if (adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);
                    }
                }

                #endregion MAJ FILEDATAPARAM

                #region MAJ RESADV

                if (liResAdv.Count > 0)
                {
                    eAdminRes res = new eAdminRes(_pref);
                    adminRes = res.SetResAdv(caps.DescId, liResAdv);
                    if (!adminRes.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                    else
                    {
                        if (adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);
                    }

                    //on rajoute les lignes manquantes pour les traductions

                    List<SetParam<KeyResADV>> lstRes = new List<SetParam<KeyResADV>>();

                    //liste des langues utilisées sur la base
                    IEnumerable<int> langs = eLibTools.GetUsersLang(_pref).Keys.Select(k => eLibTools.GetNum(k));

                    List<eLibConst.RESADV_TYPE> liResAdvType = new List<eLibConst.RESADV_TYPE>() {
                        eLibConst.RESADV_TYPE.WATERMARK,
                        eLibConst.RESADV_TYPE.TOOLTIP,
                        eLibConst.RESADV_TYPE.UNIT
                    };

                    foreach (eLibConst.RESADV_TYPE typ in liResAdvType)
                    {
                        if (liResAdv.Exists(Sp => Sp.Option.GetOption == typ))
                            lstRes.AddRange(
                                langs.Select(iLang =>
                                    new SetParam<KeyResADV>(new KeyResADV(typ, caps.DescId, iLang), "")));

                    }


                    adminRes = res.SetResAdv(caps.DescId, lstRes, true);



                }

                #endregion MAJ RESADV

                #region MAJ DESC

                // Mise à jour de DESC
                if (adminDesc.ListToUpdate.Count > 0)
                {
                    string error = string.Empty;
                    adminRes = adminDesc.Save(_pref, out error);

                    if (!adminRes.Success && !adminRes.NeedConfirm)
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            adminRes.UserErrorMessage,
                            "",
                            adminRes.DebugErrorMessage
                        );
                        LaunchError();
                    }
                    else
                    {
                        if (adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);

                    }
                }

                #endregion MAJ DESC

                #region MAJ BKMPREF

                // Mise à jour de BKMPREF
                if (liBkmPref.Count > 0)
                {
                    eAdminBkmPref bkmPref = new eAdminBkmPref(_pref);
                    List<SetParam<ePrefConst.PREF_BKMPREF>> liBkmPrefFiltered = new List<SetParam<ePrefConst.PREF_BKMPREF>>();
                    int tabId = 0;
                    int bkmId = 0;
                    foreach (SetParam<ePrefConst.PREF_BKMPREF> setParam in liBkmPref)
                    {
                        if (setParam.Option == ePrefConst.PREF_BKMPREF.TAB)
                            tabId = eLibTools.GetNum(setParam.Value);
                        else if (setParam.Option == ePrefConst.PREF_BKMPREF.BKM)
                            bkmId = eLibTools.GetNum(setParam.Value);
                        else
                            liBkmPrefFiltered.Add(setParam);


                    }

                    // Normalement, le setbkmpref defini uniquement des valeurs par defaut (userid=0)
                    adminRes = bkmPref.SetBkmPref(0, bkmId, tabId, liBkmPrefFiltered);

                    if (!adminRes.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                    else
                    {
                        if (adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);
                    }
                }

                #endregion MAJ BKMPREF

                #region MAJ USERVALUE

                //Mise à Jour des UserValue
                if (liUserValue.Count > 0)
                {
                    eAdminUserValue userValue = new eAdminUserValue(caps.DescId);
                    userValue.SetDescList(liUserValue);

                    adminRes = userValue.Save(_pref);
                    if (!adminRes.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                    else
                    {
                        if (adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);
                    }
                }

                //Mise à Jour des UserValueSearchAll
                if (liUserValueSearchAll.Count > 0 && adminTab != null)
                {
                    eAdminUserValueSearchAll userValue = new eAdminUserValueSearchAll(adminTab.DescId);

                    userValue.SetDescList(liUserValueSearchAll);

                    eAdminResult adminResUserValue = userValue.Save(_pref);
                    if (!adminResUserValue.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminResUserValue.UserErrorMessage,
                                "",
                                adminResUserValue.DebugErrorMessage
                            )
                        );
                    }
                    else
                    {
                        if (adminResUserValue.Result.Count > 0)
                            lstResult.AddRange(adminResUserValue.Result);
                    }
                }

                #endregion MAJ USERVALUE

                #region MAJ FINDERPREF

                // Mise à jour des PREF COLSPREF
                if (liColsPref.Count > 0)
                {
                    try
                    {
                        // true car nous cherchons les pref par defaut
                        eColsPref pref = new eColsPref(_pref, caps.DescId, ColPrefType.FINDERPREF, 0);
                        adminRes.Success = pref.SetColsDefaultPref(liColsPref);
                    }
                    catch (Exception e)
                    {
                        adminRes.Success = false;
                        adminRes.UserErrorMessage = "Mise à jour non effectuée";
                        adminRes.DebugErrorMessage = e.Message;
                        adminRes.InnerException = e;
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                }

                #endregion MAJ FINDERPREF

                #region Envoi Conditionnel

                // Mise à jour de ConditionnalSend
                if (liConditionalSend.Count > 0)
                {
                    try
                    {
                        adminRes = eAdminConditionalSend.Save(caps.DescId, liConditionalSend, _pref);

                        if (adminRes.Success && adminRes.Result.Count > 0)
                            lstResult.AddRange(adminRes.Result);
                    }
                    catch (Exception e)
                    {
                        adminRes.Success = false;
                        adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1760); // Mise à jour non effectuée
                        adminRes.DebugErrorMessage = e.Message;
                        adminRes.InnerException = e;
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                }

                #endregion Envoi Conditionnel

                #region MAJ Couleur

                if (lstColor.Count > 0)
                {
                    //Création / Maj des règles puis maj de desc
                    adminRes = RulesDefinition.UpdateColorRules(_pref, caps.DescId, lstColor);
                    if (adminRes.Success && adminRes.Result.Count > 0)
                    {
                        lstResult.AddRange(adminRes.Result);
                    }
                }

                #endregion MAJ Couleur

                #region MAJ PREF - APPARTENANCES (DefaultOwner)

                if (liDefaultOwners.Count > 0)
                {
                    try
                    {
                        foreach (Tuple<int, int> t in liDefaultOwners)
                        {
                            success = _pref.SetPrefFromAdminScalar(caps.DescId, ADMIN_PREF.DEFAULTOWNER, t.Item2.ToString(), t.Item1);
                            if (!success)
                            {
                                throw new Exception(string.Concat("ePref.SetPrefUserScalar() => Erreur mise à jour DefaultOwner pour tab = ", caps.DescId, ", userid = ", t.Item1, ", valeur = ", t.Item2));
                            }
                        }
                        adminRes.Success = true;
                    }
                    catch (Exception e)
                    {
                        adminRes.Success = false;
                        adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1760); // Mise à jour non effectuée
                        adminRes.DebugErrorMessage = e.Message;
                        adminRes.InnerException = e;
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                }

                #endregion MAJ PREF - APPARTENANCES (DefaultOwner)

                #region MAJ CONFIG USER
                if (listConfig.Count > 0)
                {


                    List<int> lstUser = caps.ListUser?.Count() > 0 ? eLibTools.CleanIdList(caps.ListUser).Split(';').Select(valUsr => int.Parse(valUsr)).ToList() : new List<int>();

                    //raz des propriété 

                    //Maj les uer
                    if (lstUser.Count() > 0)
                        if (_pref.SetConfig(listConfig, lstUser))
                        {
                            adminRes.Success = true;
                        }
                        else
                        {
                            adminRes.Success = false;
                        }
                }

                #endregion

                #region MAJ CONFIGDEFAULT 

                //#51627 user 0 pour mettre la première fiche Adresse créée  automatiquement en Principale
                if (listConfigDefault.Count > 0)
                    adminRes.Success = _pref.SetConfigDefault(listConfigDefault);

                #endregion

                #region MAJ DESCADV
                if (liDescAdv.Count > 0)
                {

                    try
                    {
                        eAdminDescAdv descAdv = new eAdminDescAdv(_pref, liDescAdv, caps.Confirmed);
                        adminRes = descAdv.SaveDescAdv();
                    }
                    catch (Exception e)
                    {
                        adminRes.Success = false;
                        adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1760); // Mise à jour non effectuée
                        adminRes.DebugErrorMessage = e.Message;
                        adminRes.InnerException = e;
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                }
                #endregion

                #region MAJ SYNC
                if (liSyncParam.Count > 0)
                {
                    try
                    {
                        foreach (SetParam<eConst.SYNC_PARAMETER> p in liSyncParam)
                        {
                            switch (p.Option)
                            {
                                case eConst.SYNC_PARAMETER.FORBID_IMPORT_FIELD_VALUE:
                                    eSqlSync.UpdateFieldImportForbidden(_pref, caps.DescId, p.Value == "1" ? true : false);
                                    break;
                                default:
                                    break;
                            }
                        }
                        adminRes.Success = true;
                    }
                    catch (Exception e)
                    {
                        adminRes.Success = false;
                        adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1760); // Mise à jour non effectuée
                        adminRes.DebugErrorMessage = e.Message;
                        adminRes.InnerException = e;
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                }
                #endregion

                #region RGPDRuleParam
                if (liRGPDRuleParam.Count > 0)
                {
                    eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

                    try
                    {
                        eDal.OpenDatabase();

                        RGPDRuleType ruleType = RGPDRuleType.Archiving;
                        bool ruleActive = false;
                        int nbMonths = 0;

                        foreach (SetParam<eLibConst.RGPDRuleParam> p in liRGPDRuleParam)
                        {
                            switch (p.Option)
                            {
                                case eLibConst.RGPDRuleParam.RGPDType:
                                    ruleType = (RGPDRuleType)(eLibTools.GetNum(p.Value));
                                    break;
                                case eLibConst.RGPDRuleParam.Active:
                                    ruleActive = p.Value == "1";
                                    break;
                                case eLibConst.RGPDRuleParam.NbMonthsDeadline:
                                    nbMonths = eLibTools.GetNum(p.Value);
                                    break;
                                default:
                                    break;
                            }
                        }

                        eAdminTableInfos tabInfo = eAdminTableInfos.GetAdminTableInfos(_pref, caps.DescId);
                        if (ruleType == RGPDRuleType.Archiving && ruleActive == true && tabInfo.HistoDescId == 0)
                        {
                            adminRes.Success = false;
                            adminRes.UserErrorMessage = eResApp.GetRes(_pref, 8494); //Impossible d'activer la règle d'archivage car l'onglet ne possède aucune rubrique historique
                        }
                        else
                        {
                            eRGPDRuleParam.SaveRGPDRuleParam(eDal, caps.DescId, ruleType, ruleActive, nbMonths);
                            adminRes.Success = true;
                        }
                    }
                    catch (Exception e)
                    {
                        adminRes.Success = false;
                        adminRes.UserErrorMessage = eResApp.GetRes(_pref, 1760); // Mise à jour non effectuée
                        adminRes.DebugErrorMessage = e.Message;
                        adminRes.InnerException = e;
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                adminRes.UserErrorMessage,
                                "",
                                adminRes.DebugErrorMessage
                            )
                        );
                    }
                    finally
                    {
                        if (eDal != null)
                            eDal.CloseDatabase();
                    }
                }
                #endregion
            }
            finally
            {
                eLibTools.CommitOrRollBack(_pref, adminRes.Success, "DescManager");
            }


            sendFinalResult:

            //Objet de retour
            eAdminDescResult result = new eAdminDescResult()
            {
                Success = adminRes.Success,
                UserErrorMessage = adminRes.UserErrorMessage,
                DebugErrorMessage = adminRes.DebugErrorMessage,
                NeedConfirm = adminRes.NeedConfirm,
                Descid = caps.DescId,
                Capsule = caps,
                Criticity = adminRes.Criticity,
                Result = lstResult.Count > 0 ? lstResult : null
            };

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(result);
            });
        }

        /// <summary>
        /// On retire les rubriques appartenant à la table "tabToRemove"
        /// </summary>
        /// <param name="tabToRemove">Table à retirer</param>
        /// <param name="arrHeader300">Rubriques entête gauche</param>
        /// <param name="arrHeader200">Rubriques entête droite</param>
        private void RemoveFieldsFromSelection(int tabToRemove, ref List<string> arrHeader300, ref List<string> arrHeader200)
        {
            arrHeader300.RemoveAll(item => eLibTools.GetNum(item) / 100 * 100 == tabToRemove);
            arrHeader200.RemoveAll(item => eLibTools.GetNum(item) / 100 * 100 == tabToRemove);
        }

        /// <summary>
        /// Vérifie la conformatité de la capsule
        /// </summary>
        /// <param name="caps"></param>
        /// <returns></returns>
        protected virtual eAdminCapsule<eAdminUpdateProperty> CheckCapsule(eAdminCapsule<eAdminUpdateProperty> caps)
        {
            return caps;
        }




        private string RemoveFieldsFromAutoBuildName(int tabToRemove, string autobuildname)
        {
            int field01 = tabToRemove + 1;
            string fieldToRemove = string.Concat("$", field01, "$");

            List<string> listAutobuildname = autobuildname.Split('-').ToList<string>();
            listAutobuildname.RemoveAll(f => f.Trim() == fieldToRemove);

            return string.Join("-", listAutobuildname);
        }

        [DataContract]
        public class eAdminDescResult : eAdminResult
        {
            [DataMember]
            public int Descid;

            [DataMember]
            public eAdminCapsule<eAdminUpdateProperty> Capsule;
        }
    }
}