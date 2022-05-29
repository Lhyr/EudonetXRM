using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldAliasRelationRenderer : eAdminBlockRenderer
    {
        #region variables privées
        eudoDAL _dal;
        eAdminFieldInfos _field;
        #endregion


        #region Constructeur

        /// <summary>
        /// Constructeur interne
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="title"></param>
        /// <param name="titleInfo"></param>
        protected eAdminFieldAliasRelationRenderer(ePref pref, eAdminFieldInfos fieldInfos, string idBlock = "RelationPart")
            : base(pref, fieldInfos.Table, eResApp.GetRes(pref, 1117), "", idBlock: idBlock)
        {
            _field = fieldInfos;
        }


        /// <summary>
        /// Instanciation de l'objet
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="title"></param>
        /// <param name="titleInfo"></param>
        /// <returns></returns>
        public static eAdminFieldAliasRelationRenderer CreateAdminFieldAliasRelationRenderer(ePref pref, eAdminFieldInfos fieldInfos)
        {
            eAdminFieldAliasRelationRenderer features = new eAdminFieldAliasRelationRenderer(pref, fieldInfos);
            features._tab = fieldInfos.Table.DescId; // todo : vérifier pourquoi ce n'est pas fait dans le cas général
            return features;
        }


        #endregion

        #region Construction du panneau

        protected override bool Init()
        {
            _dal = eLibTools.GetEudoDAL(Pref);
            _dal.OpenDatabase();
            return base.Init();
        }
        protected override bool Build()
        {
            bool bReturn = base.Build();
            try
            {
                setLinkedFiles();
                setLinkedFields();
            }
            catch (Exception e)
            {
                _eException = e;
                _sErrorMsg = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
                _dal.CloseDatabase();
                return false;
            }
            return bReturn;
        }

        protected override bool End()
        {
            _dal.CloseDatabase();
            return base.End();
        }
        #endregion

        #region récupération des informations concernant les parents

        private void setLinkedFiles()
        {
            RqParam rq = eSqlDesc.GetRqLiaison(_field.Table.DescId, Pref.LangId);
            string sError = "";
            DataTableReaderTuned dtr = _dal.Execute(rq, out sError);

            if (sError.Length > 0)
                throw new Exception("setLinkedFiles : " + sError);

            String sLabelCplt = "", sRelationFieldDescId = "";
            List<ListItem> li = new List<ListItem>();
            ListItem item;
            int iAliasSourceFileDid = eLibTools.GetTabFromDescId(_field.AliasParam.AliasSourceFieldDid), iAliasRelationFieldDid = _field.AliasParam.AliasRelationFieldDid;
            while (dtr.Read())
            {

                if (dtr.GetInt32("LinkFieldDescId") % 100 > 0)
                {
                    //Liaison par champ custom
                    sLabelCplt = String.Format(" ({0})", dtr.GetString("LinkField"));
                    sRelationFieldDescId = dtr.GetString("LinkFieldDescId");
                }
                else
                {
                    //Liaison Parente
                    sLabelCplt = "";
                    sRelationFieldDescId = dtr.GetString("RelationFileDescId");
                }

                item = new ListItem(String.Format("{0}{1}", dtr.GetString("RelationFile"), sLabelCplt), String.Format("{0}_{1}", sRelationFieldDescId, dtr.GetString("RelationFileDescId")));

                li.Add(item);

                if (dtr.GetInt32("RelationFileDescId") == iAliasSourceFileDid
                    && (dtr.GetInt32("LinkFieldDescId") == iAliasRelationFieldDid
                        || (iAliasRelationFieldDid % 100 == 0 && dtr.GetInt32("LinkFieldDescId") == _field.Table.DescId)
                    ))
                    item.Selected = true;
            }

            eAdminDropdownField ddlFiles = new eAdminDropdownField(descid: _field.DescId,
               label: eResApp.GetRes(Pref, 7992),
               customLabelCSSClasses: "info",
               propCat: eAdminUpdateProperty.CATEGORY.DESCADV,
               propCode: (int)DESCADV_PARAMETER.ALIASSOURCE,
               items: li.ToArray(),
               renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
               valueFormat: FieldFormat.TYP_CHAR);

            ddlFiles.SetFieldControlID("ddlAliasLinkedFiles");
            ddlFiles.Generate(_panelContent);

            DropDownList ddl = ((DropDownList)ddlFiles.FieldControl);
            //le paramètre ALIASLINKFIELD est enregistré via la ddl des champs pour que les deux informations soient enregistrées en meme temps
            //afin de ne pas avoir d'informations discordantes
            ddl.Attributes.Remove("dsc");
            ddl.Attributes.Add("onChange", "nsAdminField.refreshFieldAliasSources();");
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 436), ""));
        }

        private void setLinkedFields()
        {

            String sError = "";
            int iLinkedTab = eLibTools.GetTabFromDescId(_field.AliasParam.AliasSourceFieldDid), iRelationField = _field.AliasParam.AliasRelationFieldDid;

            Dictionary<int, string> dicFields = eAdminFieldAliasRelationRenderer.GetLinkedFields(Pref, _dal, iLinkedTab, out sError);
            //Key = DescId, Value = Libelle du champ

            if (sError.Length > 0)
                throw new Exception("getLinkedFields : " + sError);

            List<ListItem> li = new List<ListItem>();
            ListItem item;

            foreach (KeyValuePair<int, string> kvp in dicFields)
            {
                item = new ListItem(kvp.Value, String.Format("[{0}]_[{1}]", iRelationField, kvp.Key.ToString()));
                li.Add(item);

                if (kvp.Key == _field.AliasParam.AliasSourceFieldDid)
                    item.Selected = true;
            }

            if (li.Count == 0)
                li.Add(new ListItem(eResApp.GetRes(Pref, 436), ""));

            eAdminDropdownField ddlFields = new eAdminDropdownField(descid: _field.DescId,
               label: eResApp.GetRes(Pref, 7991),
               customLabelCSSClasses: "info",
               propCat: eAdminUpdateProperty.CATEGORY.DESCADV,
               propCode: (int)DESCADV_PARAMETER.ALIASSOURCE,
               items: li.ToArray(),
               renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
               valueFormat: FieldFormat.TYP_CHAR);

            ddlFields.SetFieldControlID("ddlAliasLinkedFields");
            ddlFields.Generate(_panelContent);
            DropDownList ddl = ((DropDownList)ddlFields.FieldControl);
            if (li.Count > 0)
                ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 436), ""));
        }


        #endregion


        #region Fonctions statiques publiques

        public enum GetLinkedFieldsContext
        {
            AliasRelation = 0,
            AssociateField = 1
        }

        /// <summary>
        /// retourne un dictionnaire avec pour clé le descid, pour valeur le libellé du champ
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="iLinkedTab"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        internal static Dictionary<int, string> GetLinkedFields(ePref pref, eudoDAL dal, Int32 iLinkedTab, out string sError, GetLinkedFieldsContext context = GetLinkedFieldsContext.AliasRelation, eAdminFieldInfos field = null)
        {
            sError = "";
            Dictionary<int, string> dicFields = new Dictionary<int, string>();

            if (iLinkedTab == 0)
                return dicFields;

            RqParam rq = eSqlDesc.GetRqFields(iLinkedTab, pref.LangId);
            DataTableReaderTuned dtr = dal.Execute(rq, out sError);

            if (sError.Length > 0)
                throw new Exception("getLinkedFields : " + sError);


            while (dtr.Read())
            {
                FieldFormat format = (FieldFormat)dtr.GetInt32("Format");
                if (context == GetLinkedFieldsContext.AliasRelation && !IsValidFieldFormat(format))
                {
                    continue;
                }
                else if (context == GetLinkedFieldsContext.AssociateField && !eAdminFieldRightsAndRulesRenderer.IsAssociateFieldValidFormat(format))
                {
                    continue;
                }

                if (field != null)
                {
                    PopupType popup = (PopupType)dtr.GetInt32("Popup");
                    int iPopupDescId = dtr.GetInt32("PopupDescId");
                    bool bMultiple = dtr.GetBoolean("Multiple");

                    //field = rubrique en cours
                    if (field.Format != format)
                        continue;

                    if (field.PopupType != popup)
                        continue;

                    if (popup != PopupType.NONE && field.PopupDescId != iPopupDescId)
                        continue;

                    if (field.Multiple != bMultiple)
                        continue;

                }

                dicFields.Add(dtr.GetInt32("DescId"), dtr.GetString("Label"));
            }

            return dicFields;
        }

        /// <summary>
        /// indique si le format de champ est valide pour que le champ soit source d'un Alias
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool IsValidFieldFormat(FieldFormat format)
        {
            switch (format)
            {
                case FieldFormat.TYP_CHAR:
                case FieldFormat.TYP_DATE:
                case FieldFormat.TYP_BIT:
                case FieldFormat.TYP_MONEY:
                case FieldFormat.TYP_EMAIL:
                case FieldFormat.TYP_WEB:
                case FieldFormat.TYP_USER:
                case FieldFormat.TYP_MEMO:
                case FieldFormat.TYP_NUMERIC:
                case FieldFormat.TYP_AUTOINC:
                case FieldFormat.TYP_PHONE:
                case FieldFormat.TYP_IMAGE:
                case FieldFormat.TYP_GROUP:
                case FieldFormat.TYP_IFRAME:
                case FieldFormat.TYP_CHART:
                case FieldFormat.TYP_GEOGRAPHY_V2:
                case FieldFormat.TYP_BITBUTTON:
                case FieldFormat.TYP_SOCIALNETWORK:
                    return true;
                    break;
                default:
                    return false;
                    break;
            }


        }
        #endregion
    }
}