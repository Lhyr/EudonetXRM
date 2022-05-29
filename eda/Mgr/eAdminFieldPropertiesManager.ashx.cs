using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminFieldPropertiesManager
    /// </summary>
    public class eAdminFieldPropertiesManager : eAdminManager
    {
        /// <summary>
        /// type d'action pour le manager
        /// </summary>
        public enum FieldManagerAction
        {
            /// <summary>action indéfini </summary>
            UNDEFINED = 0,
            /// <summary>Rendu html des propriétés de la rubrique</summary>
            GETINFOS = 1,
            /// <summary>Creation de Champs</summary>
            CREATEFIELD = 2,
            /// <summary>Suppression de Champs</summary>
            DROPFIELD = 3,
            /// <summary>Résolution des conflits d'emplacement (disporder identique)</summary>
            RESOLVECONFLICT = 4
        }

        private FieldManagerAction action = FieldManagerAction.UNDEFINED;
        private Int32 _iDescId;

        private eAdminResult result = null;

        /// <summary>
        /// Processes the manager.
        /// </summary>
        /// <exception cref="System.Exception">
        /// </exception>
        protected override void ProcessManager()
        {

            if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
            {
                if (!Enum.TryParse(_context.Request.Form["action"], out action))
                    action = FieldManagerAction.UNDEFINED;
            }
            eudoDAL dal;
            switch (action)
            {
                case FieldManagerAction.UNDEFINED:
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Aucune action définie.");
                    LaunchError();
                    break;
                case FieldManagerAction.GETINFOS:

                    String idpart = "";
                    bool isSys = false;

                    if (_requestTools.AllKeys.Contains("descid") && !String.IsNullOrEmpty(_context.Request.Form["descid"]))
                        Int32.TryParse(_context.Request.Form["descid"], out _iDescId);

                    if (_requestTools.AllKeys.Contains("idpart") && !String.IsNullOrEmpty(_context.Request.Form["idpart"]))
                        idpart = _context.Request.Form["idpart"];

                    isSys = _requestTools.GetRequestFormKeyB("sys") ?? false;


                    //tab et spécif obligatoire
                    //if (_iDescId == 0)
                    //{
                    //    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 7875));
                    //    LaunchError();
                    //}

                    // Création du rendu

                    if (String.IsNullOrEmpty(idpart))
                    {
                        int tabId = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                        //On recupere les champs en entete, ainsi que les tab id (cas des champs de liaison type PP et PM)
                        //Si champ d'entête alors on met descid a 0 pour faire en sorte de ne pas pouvoir administrer les champs d'en tête
                        Dictionary<String, String> headerPref = _pref.GetPrefDefault(tabId, new List<String> { "HEADER_300", "HEADER_200" });
                        string header300 = headerPref["HEADER_300"].Trim();
                        if (header300.EndsWith(";"))
                            header300 = header300.Substring(0, header300.Length - 1).Trim();

                        string header200 = headerPref["HEADER_200"].Trim();
                        if (header200.EndsWith(";"))
                            header200 = header200.Substring(0, header200.Length - 1).Trim();

                        List<Int32> lHeader300 = !String.IsNullOrEmpty(header300) ? header300.Split(';').Select(Int32.Parse).ToList().ConvertAll(x => x / 100 * 100) : new List<int>();
                        List<Int32> lHeader200 = !String.IsNullOrEmpty(header200) ? header200.Split(';').Select(Int32.Parse).ToList().ConvertAll(x => x / 100 * 100) : new List<int>();
                        if (header200.Contains(_iDescId.ToString()) || header300.Contains(_iDescId.ToString()) ||
                            lHeader200.Contains(_iDescId) || lHeader300.Contains(_iDescId))
                        {
                            _iDescId = 0;
                        }

                        eAdminRenderer rdr = eAdminRendererFactory.CreateAdminFieldsParamsRenderer(_pref, _iDescId, isSys);
                        if (String.IsNullOrEmpty(rdr.ErrorMsg))
                        {
                            RenderResultHTML(rdr.PgContainer);
                        }
                        else
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 8174), eResApp.GetRes(_pref, 8176), eResApp.GetRes(_pref, 8177), rdr.ErrorMsg);
                            eFeedbackXrm.LaunchFeedbackXrm(ErrorContainer, _pref);
                            LaunchError();

                        }
                    }
                    else
                    {
                        eAdminFieldInfos fieldInfos = eAdminFieldInfos.GetAdminFieldInfos(_pref, _iDescId);

                        eAdminRenderer rdr = null;

                        switch (idpart)
                        {
                            case "blockFeatures": rdr = eAdminRendererFactory.CreateAdminFieldFeaturesRenderer(_pref, fieldInfos); break;
                            case "blockRightsAndRulesPart": rdr = eAdminRendererFactory.CreateAdminFieldRightsAndRulesRenderer(_pref, fieldInfos); break;
                            case "blockLayout": rdr = eAdminRendererFactory.CreateAdminFieldLayoutRenderer(_pref, fieldInfos); break;
                            case "blockCatalog": rdr = eAdminRendererFactory.CreateAdminFieldCatalogRenderer(_pref, fieldInfos); break;
                            case "blockRelations": rdr = eAdminRendererFactory.CreateAdminFieldRelationsRenderer(_pref, fieldInfos); break;
                            case "blockTrace": rdr = eAdminRendererFactory.CreateAdminFieldTraceabilityRenderer(_pref, fieldInfos); break;
                            case "blockTranslations": rdr = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(_pref, fieldInfos); break;
                        }

                        if (rdr != null)
                        {
                            RenderResultHTML(rdr.GetContents());
                        }
                    }

                    break;

                case FieldManagerAction.CREATEFIELD:
                    #region Creation de Champ

                    int iDisporder = _requestTools.GetRequestFormKeyI("disporder") ?? 0;
                    int nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                    int iCol = _requestTools.GetRequestFormKeyI("col") ?? 0;
                    string sLabel = _requestTools.GetRequestFormKeyS("label") ?? String.Empty;
                    _iDescId = _requestTools.GetRequestFormKeyI("descid") ?? nTab;

                    Int32 nSysLimite = 0;


                    if (iDisporder <= nSysLimite)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6882), eResApp.GetRes(_pref, 6883), "", String.Concat("iDisporder : ", iDisporder)));
                        return;
                    }

                    eAdminFieldInfos fieldCreate = new eAdminFieldInfos(_pref, _iDescId);
                    if (sLabel.Trim().Length > 0)
                        fieldCreate.SetLabelIfEmpty(sLabel);
                    fieldCreate.Disporder = iDisporder;
                    fieldCreate.Col = iCol;
                    Regex re = new Regex("^[0-9]+_[0-9]+$");

                    List<DescAdvObj> liDescAdv = new List<DescAdvObj>();

                    int iTabSource = 0;
                    foreach (String key in _requestTools.AllKeys)
                    {
                        if (!re.IsMatch(key))
                            continue;

                        string[] aKey = key.Split('_');
                        eAdminUpdateProperty.CATEGORY category;
                        if (!Enum.TryParse<eAdminUpdateProperty.CATEGORY>(aKey[0], out category))
                            continue;

                        String sValue = _context.Request.Form[key].ToString();
                        switch (category)
                        {
                            case eAdminUpdateProperty.CATEGORY.DESC:
                                #region propriétés dans [DESC]
                                eLibConst.DESC property;
                                if (!Enum.TryParse<eLibConst.DESC>(aKey[1], out property))
                                    continue;

                                switch (property)
                                {
                                    case eLibConst.DESC.FORMAT:
                                        FieldFormat fldFormat;

                                        if (!Enum.TryParse<FieldFormat>(sValue, out fldFormat))
                                            continue;

                                        fieldCreate.Format = fldFormat;

                                        break;
                                    case eLibConst.DESC.LENGTH:
                                        Int32 iLength = 0;
                                        if (!Int32.TryParse(sValue, out iLength))
                                            continue;
                                        fieldCreate.Length = iLength;
                                        break;
                                    case eLibConst.DESC.CASE:
                                        CaseField caseFld;

                                        if (!Enum.TryParse<CaseField>(sValue, out caseFld))
                                            continue;

                                        fieldCreate.Case = caseFld;

                                        break;
                                    case eLibConst.DESC.TOOLTIPTEXT:
                                        fieldCreate.ToolTipText = sValue;
                                        break;
                                    case eLibConst.DESC.DEFAULT:
                                        fieldCreate.Default = sValue;
                                        break;
                                    case eLibConst.DESC.HISTORIC:
                                        fieldCreate.Historic = sValue == "1";
                                        break;
                                    case eLibConst.DESC.OBLIGAT:

                                        //todo implementer Obligat dans eAdminFieldInfos
                                        //field.Obligat = sValue == "1";
                                        break;
                                    case eLibConst.DESC.MULTIPLE:
                                        fieldCreate.Multiple = sValue == "1";
                                        break;
                                    case eLibConst.DESC.RELATION:
                                        fieldCreate.Relation = sValue == "1";
                                        break;
                                    case eLibConst.DESC.POPUP:
                                        PopupType ptype;

                                        if (!Enum.TryParse<PopupType>(sValue, out ptype))
                                            continue;

                                        fieldCreate.PopupType = ptype;

                                        break;
                                    case eLibConst.DESC.POPUPDESCID:
                                        Int32 iPDescid;
                                        if (Int32.TryParse(sValue, out iPDescid))
                                            fieldCreate.PopupDescId = iPDescid;
                                        break;
                                    case eLibConst.DESC.BOUNDDESCID:
                                        Int32 iBDescid;
                                        if (Int32.TryParse(sValue, out iBDescid))
                                            fieldCreate.BoundDescId = iBDescid;
                                        break;
                                    case eLibConst.DESC.ROWSPAN:
                                        Int32 irowspan;
                                        if (Int32.TryParse(sValue, out irowspan))
                                            fieldCreate.Rowspan = irowspan;
                                        break;
                                    case eLibConst.DESC.COLSPAN:
                                        Int32 icolspan;
                                        if (Int32.TryParse(sValue, out icolspan))
                                            fieldCreate.Colspan = icolspan;
                                        break;
                                    case eLibConst.DESC.DISPORDER:
                                        Int32 ido;
                                        if (Int32.TryParse(sValue, out ido))
                                            fieldCreate.Disporder = ido;
                                        break;
                                    case eLibConst.DESC.TABINDEX:
                                        Int32 iti;
                                        if (Int32.TryParse(sValue, out iti))
                                            fieldCreate.TabIndex = iti;

                                        break;
                                    case eLibConst.DESC.BOLD:
                                        fieldCreate.Bold = sValue == "1";
                                        break;
                                    case eLibConst.DESC.ITALIC:
                                        fieldCreate.Italic = sValue == "1";
                                        break;
                                    case eLibConst.DESC.UNDERLINE:
                                        fieldCreate.UnderLine = sValue == "1";
                                        break;
                                    case eLibConst.DESC.FLAT:
                                        fieldCreate.Flat = sValue == "1";
                                        break;
                                    case eLibConst.DESC.FORECOLOR:
                                        fieldCreate.ForeColor = sValue;

                                        break;
                                    case eLibConst.DESC.READONLY:
                                        fieldCreate.ReadOnly = sValue == "1";

                                        break;
                                    case eLibConst.DESC.HTML:
                                        fieldCreate.IsHTML = sValue == "1";
                                        break;
                                    case eLibConst.DESC.STORAGE:
                                        ImageStorage s;

                                        if (!Enum.TryParse<ImageStorage>(sValue, out s))
                                            continue;

                                        fieldCreate.Storage = s;

                                        break;
                                    case eLibConst.DESC.UNICODE:
                                        fieldCreate.Unicode = sValue == "1";
                                        break;
                                    case eLibConst.DESC.NODEFAULTCLONE:
                                        fieldCreate.NoDefaultClone = sValue == "1";
                                        break;

                                    default:
                                        break;
                                }
                                break;

                            #endregion
                            case eAdminUpdateProperty.CATEGORY.RES:
                                Int32 iLangId;
                                if (Int32.TryParse(aKey[1], out iLangId))
                                    fieldCreate.Labels[iLangId] = sValue;
                                break;
                            case eAdminUpdateProperty.CATEGORY.DESCADV:
                                try
                                {
                                    DESCADV_PARAMETER p;
                                    if (!Enum.TryParse<DESCADV_PARAMETER>(aKey[1], out p))
                                        continue;

                                    //on retient la table source pour recopier les traductions du libellé
                                    if (p == DESCADV_PARAMETER.ALIAS_RELATION)
                                        iTabSource = eLibTools.GetNum(sValue);
                                    int iValue = 0;
                                    Int32.TryParse(sValue, out iValue);
                                    switch (p)
                                    {
                                        case DESCADV_PARAMETER.SUPERADMINONLY:
                                            fieldCreate.SuperAdminOnly = sValue == "1";
                                            break;
                                        case DESCADV_PARAMETER.FIELDSTYLE:
                                            fieldCreate.FieldStyle = (FIELD_DISPLAY_STYLE)iValue;
                                            break;
                                        case DESCADV_PARAMETER.VALUECOLOR:
                                            fieldCreate.ValueColor = sValue;
                                            break;
                                        case DESCADV_PARAMETER.LABELHIDDEN:
                                            fieldCreate.LabelHidden = sValue == "1";
                                            break;
                                        case DESCADV_PARAMETER.BUTTONCOLOR:
                                            fieldCreate.ButtonColor = sValue;
                                            break;
                                        case DESCADV_PARAMETER.MAXIMIZEVALUE:
                                            fieldCreate.MaximizeValue = sValue == "1";
                                            break;
                                        case DESCADV_PARAMETER.IS_UNIQUE:
                                            fieldCreate.IsUnique = sValue == "1";
                                            break;
                                    }

                                    liDescAdv.Add(DescAdvObj.GetSingle(_iDescId, p, sValue));
                                }
                                catch (Exception)
                                {

                                }
                                break;
                            default:
                                continue;

                        }


                    }

                    String sError = "";

                    fieldCreate.Confirmed = _requestTools.AllKeys.Contains("confirm") && _context.Request["confirm"].ToString() == "1";


                    dal = eLibTools.GetEudoDAL(_pref);
                    dal.OpenDatabase();
                    try
                    {
                        if (fieldCreate.Format == FieldFormat.TYP_ALIASRELATION && iTabSource > 0)
                        {
                            fieldCreate.SetLabelFromRes(iTabSource, dal, out sError);
                        }

                        //ALISTER Demande / Request 83814 Check if there is a link-type field for alias fields
                        if (fieldCreate.Format == FieldFormat.TYP_ALIAS)
                        {
                            bool bIsLink = false;
                            List<eFieldRecord> lstFields = new List<eFieldRecord>();
                            eRecord recBlank = eFileLayout.GetBlankRecord(_pref, nTab, out sError);
                            if (sError.Length > 0)
                            {
                                //sError = "eAdminFieldPropertiesManager.GetBlankRecord : " + sError;
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 7042), eResApp.GetRes(_pref, 8875), result.DebugErrorMessage);
                                LaunchError();
                            }

                            lstFields = recBlank.GetFields.FindAll(
                                    delegate (eFieldRecord fld)
                                    {
                                        return fld.FldInfo.Table.Alias == recBlank.ViewTab.ToString();
                                    }

                                    );

                            // on retire les champs systèmes
                            lstFields.RemoveAll(delegate (eFieldRecord fld)
                            {
                                return fld.FldInfo.Descid % 100 >= eLibConst.MAX_NBRE_FIELD
                                    && !(fld.FldInfo.Descid % 100 == (int)AllField.AVATAR && fld.FldInfo.PosDisporder > 0);
                            });

                            lstFields.Sort(eFieldRecord.CompareByDisporder);

                            for(int i = 0; i < lstFields.Count; i++)
                            {

                                eAdminFieldInfos fld = eAdminFieldInfos.GetAdminFieldInfos(_pref, lstFields[i].FldInfo.Descid);
                                if (fld.IsSpecialCatalog) {
                                    bIsLink = true;
                                }
                            }

                            if (!bIsLink)
                            {
                                sError = eResApp.GetRes(_pref, 8874);
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 8873), sError, eResApp.GetRes(_pref, 8875));
                                LaunchError();
                            }
                            //fieldCreate.SetLabelFromRes(iTabSource, dal, out sError);
                        }

                        result = fieldCreate.Create(dal, out sError);

                        if ((!result.Success && result.DebugErrorMessage.Length > 0) || sError.Length > 0)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), result.UserErrorMessage, eResApp.GetRes(_pref, 8875), result.DebugErrorMessage);
                            LaunchError();
                        }
                        else
                        {

                            foreach (DescAdvObj item in liDescAdv)
                            {
                                item.Key.DescID = fieldCreate.DescId;
                            }

                            eAdminDescAdv descAdv = new eAdminDescAdv(_pref, liDescAdv, true);
                            descAdv.SaveDescAdv(dal);

                            bool bOnlyOneFieldOnTab = eSqlDesc.GetNbCustomFieldsOnTab(_pref, dal, nTab, out sError) == 1;
                            if (sError.Length > 0)
                                throw new Exception(sError);
                            bool bIsCoordLayoutEnabled = eSqlDesc.IsCoordLayoutEnabled(_pref, dal, nTab, out sError);
                            if (sError.Length > 0)
                                throw new Exception(sError);
                            if (bOnlyOneFieldOnTab || bIsCoordLayoutEnabled)
                            {
                                if (sError.Length > 0)
                                    throw new Exception(sError);
                                eFileLayout.UpdateFieldsPositions(_pref, nTab, out sError, eFileLayout.UpdateLayoutDirection.FromDisporderToCoord);
                            }
                            if (sError.Length > 0)
                                throw new Exception(sError);


                        }
                    }
                    catch { throw; }
                    finally
                    {
                        dal.CloseDatabase();
                    }

                    RenderResult(RequestContentType.TEXT, delegate ()
                    {
                        return JsonConvert.SerializeObject(result);
                    });
                    #endregion


                    break;
                case FieldManagerAction.DROPFIELD:
                    if (_requestTools.AllKeys.Contains("descid") && !String.IsNullOrEmpty(_context.Request.Form["descid"]))
                        Int32.TryParse(_context.Request.Form["descid"], out _iDescId);

                    //tab et spécif obligatoire
                    if (_iDescId == 0)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Paramètres non renseignés.");
                        LaunchError();
                    }




                    eAdminFieldInfos fieldDrop = eAdminFieldInfos.GetAdminFieldInfos(_pref, _iDescId);
                    if (fieldDrop != null && fieldDrop.Error.Length > 0)
                        throw new Exception(fieldDrop.Error);

                    // On ne supprime pas un champs spécial
                    if (fieldDrop.IsSpecialField())
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "Ce champs est utilisé par le système", "Cette suppression n'est pas autorisée !");
                        LaunchError();
                    }

                    // On ne supprime pas un champs utilisé par l'orm
                    OrmMappingInfo ormInfos = eLibTools.OrmLoadAndGetMapAdv(_pref, new OrmGetParams() { ExceptionMode = OrmMappingExceptionMode.SAFE  });
                    if (ormInfos.GetAllMappedDescid.Contains(fieldDrop.DescId))
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "Ce champs est utilisé par l'ORM", "Cette suppression n'est pas autorisée !");
                        LaunchError();
                    }

                    fieldDrop.Confirmed = _requestTools.AllKeys.Contains("confirm") && _context.Request.Form["confirm"].ToString() == "1";

                    result = fieldDrop.Drop(out sError);
                    if (result != null && !result.Success && result.DebugErrorMessage.Length > 0 && !result.NeedConfirm)
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), result.UserErrorMessage, "La suppression du Champ a échoué", result.DebugErrorMessage);
                        LaunchError();
                    }
                    else
                    {
                        RenderResult(RequestContentType.TEXT, delegate ()
                        {
                            return JsonConvert.SerializeObject(result);
                        });
                    }

                    break;
                case FieldManagerAction.RESOLVECONFLICT:
                    nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;

                    dal = eLibTools.GetEudoDAL(_pref);

                    dal.OpenDatabase();
                    try
                    {
                        eSqlDesc.ResolveFieldsDisporderConflict(_pref, dal, nTab, out sError);
                        if (sError.Length > 0)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 7656), eResApp.GetRes(_pref, 7657), title: eResApp.GetRes(_pref, 72), devMsg: sError);
                            LaunchError();
                        }

                        if (eSqlDesc.IsCoordLayoutEnabled(_pref, dal, nTab, out sError))
                        {
                            eFileLayout.UpdateFieldsPositions(_pref, nTab, out sError, eFileLayout.UpdateLayoutDirection.FromDisporderToCoord);
                        }

                    }
                    catch { throw; }
                    finally
                    {
                        dal.CloseDatabase();
                    }

                    break;

            }
        }


    }
}