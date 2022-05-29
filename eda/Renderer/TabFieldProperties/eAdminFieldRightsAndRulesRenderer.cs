using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldRightsAndRulesRenderer : eAdminRightsAndRulesRenderer
    {

        private eAdminFieldInfos _field;

        private eAdminFieldRightsAndRulesRenderer(ePref pref, eAdminFieldInfos field)
            : base(pref, field.Table, idBlock: "blockRightsAndRulesPart")
        {
            _descid = field.DescId;
            _field = field;
            _tab = field.Table.DescId;
        }

        public static eAdminFieldRightsAndRulesRenderer CreateAdminFieldRightsAndRulesLayoutRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldRightsAndRulesRenderer features = new eAdminFieldRightsAndRulesRenderer(pref, field);
            return features;
        }



        protected override bool Build()
        {
            BuildMainContent();

            // administrer les droits
            BuildAdminRights();

            // Le champ 87 est destiné au signet de l'onglet du même nom
            if (_descid % 100 != AllField.BKM_PM_EVENT.GetHashCode())
            {
                //Comportements conditionnels
                BuildConditionalBehaviors();

                //Automatismes
                BuildAutomatismPart();

                // administrer les règles
                BuildAdminRules();

                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Field_Association))
                {
                    //associer une rubrique
                    BuildAssociateFieldParam();
                }

            }

            return true;
        }

        /// <summary>
        /// Paramètre de Rubrique associée
        /// </summary>
        protected void BuildAssociateFieldParam()
        {
            if (!IsAssociateFieldValidFormat(_field.Format))
                return;

            Panel panelRules = new Panel();
            Panel panelRulesSub = new Panel();
            CreateCollapsibleMenu(out panelRules, out panelRulesSub, false, eResApp.GetRes(Pref, 8186), bRights: true);
            panelRules.CssClass = "btnLink";
            panelRules.ID = "adminAssociateField";
            _panelContent.Controls.Add(panelRules);
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {
                #region Onglets Disponibles

                dal.OpenDatabase();
                RqParam rq = eSqlDesc.GetRqLiaison(_field.Table.DescId, Pref.LangId);
                string sError = "";

                DataTableReaderTuned dtr = dal.Execute(rq, out sError);

                if (sError.Length > 0)
                    throw new Exception("BuildAssociateFieldParam (File) : " + sError);

                List<ListItem> li = new List<ListItem>();
                ListItem item;
                int iAliasSourceFileDid = eLibTools.GetTabFromDescId(_field.AssociateField.AliasSourceFieldDid), iAliasRelationFieldDid = _field.AssociateField.AliasRelationFieldDid;
                String sLabelCplt = "", sRelationFieldDescId = "";
                eRes res = new eRes(Pref, ((int)TableType.ADR).ToString());
                bool bFound;
                String sLabelAddress = res.GetRes((int)TableType.ADR, out bFound);
                while (dtr.Read())
                {

                    // pour adresse on fait la liaison via contact
                    if (dtr.GetInt32("RelationFileDescId") == (int)TableType.ADR)
                        continue;

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
                    if (li.Exists(delegate (ListItem i) { return i.Value == item.Value; }))
                        continue;

                    li.Add(item);
                    if (dtr.GetInt32("RelationFileDescId") == iAliasSourceFileDid
                        && (dtr.GetInt32("LinkFieldDescId") == iAliasRelationFieldDid
                            || (iAliasRelationFieldDid % 100 == 0 && dtr.GetInt32("LinkFieldDescId") == _field.Table.DescId)
                        ))
                        item.Selected = true;

                    //dans le cas d'une relation avec contact on rajoute une relation avec Address
                    if (dtr.GetInt32("RelationFileDescId") == (int)TableType.PP)
                    {
                        item = new ListItem(String.Format("{0}{1}", sLabelAddress, sLabelCplt), String.Format("{0}_{1}", sRelationFieldDescId, (int)TableType.ADR));

                        li.Add(item);
                        if ((int)TableType.ADR == iAliasSourceFileDid
                            && (dtr.GetInt32("LinkFieldDescId") == iAliasRelationFieldDid
                                || (iAliasRelationFieldDid % 100 == 0 && dtr.GetInt32("LinkFieldDescId") == _field.Table.DescId)
                            ))
                            item.Selected = true;
                    }
                }

                if (li.Count == 0)
                    li.Add(new ListItem(eResApp.GetRes(Pref, 436), ""));

                eAdminDropdownField ddlFiles = new eAdminDropdownField(descid: _field.DescId,
                   label: eResApp.GetRes(Pref, 7992),
                   customLabelCSSClasses: "info",
                   propCat: eAdminUpdateProperty.CATEGORY.DESCADV,
                   propCode: (int)DESCADV_PARAMETER.ASSOCIATE_FIELD,
                   items: li.ToArray(),
                   renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                   valueFormat: FieldFormat.TYP_CHAR);

                ddlFiles.SetFieldControlID("ddlAssociateFiles");
                ddlFiles.Generate(panelRulesSub);

                DropDownList ddl = ((DropDownList)ddlFiles.FieldControl);
                if (ddl != null)
                {
                    //le paramètre ALIASLINKFIELD est enregistré via la ddl des champs pour que les deux informations soient enregistrées en meme temps
                    //afin de ne pas avoir d'informations discordantes
                    ddl.Attributes.Remove("dsc");
                    ddl.Attributes.Add("onChange", "nsAdminField.refreshAssociateFields();");
                    ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 436), ""));
                }
                #endregion

                #region Rubriques associables
                Dictionary<int, string> dicFields = eAdminFieldAliasRelationRenderer.GetLinkedFields(Pref, dal, iAliasSourceFileDid, out sError, context: eAdminFieldAliasRelationRenderer.GetLinkedFieldsContext.AssociateField, field: _field);
                //Key = DescId, Value = Libelle du champ
                if (sError.Length > 0)
                    throw new Exception("BuildAssociateFieldParam (Field) : " + sError);

                li = new List<ListItem>();

                foreach (KeyValuePair<int, string> kvp in dicFields)
                {
                    item = new ListItem(kvp.Value, String.Format("[{0}]_[{1}]", iAliasRelationFieldDid, kvp.Key.ToString()));

                    if (li.Exists(delegate (ListItem i) { return i.Value == item.Value; }))
                        continue;

                    li.Add(item);

                    if (kvp.Key == _field.AssociateField.AliasSourceFieldDid)
                        item.Selected = true;
                }

                if (li.Count == 0)
                    li.Add(new ListItem(eResApp.GetRes(Pref, 436), ""));

                eAdminDropdownField ddlFields = new eAdminDropdownField(descid: _field.DescId,
                   label: eResApp.GetRes(Pref, 8186),
                   customLabelCSSClasses: "info",
                   propCat: eAdminUpdateProperty.CATEGORY.DESCADV,
                   propCode: (int)DESCADV_PARAMETER.ASSOCIATE_FIELD,
                   items: li.ToArray(),
                   renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                   valueFormat: FieldFormat.TYP_CHAR);

                ddlFields.SetFieldControlID("ddlAssociateFields");
                ddlFields.Generate(panelRulesSub);
                ddl = ((DropDownList)ddlFields.FieldControl);

                if (ddl.Items.Count > 1)
                    ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 436), ""));

                #endregion
            }
            catch
            {
                throw;
            }
            finally
            {
                dal?.CloseDatabase();
            }
        }


        /// <summary>
        /// indique si le format est compatible avec les Rubriques associées
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool IsAssociateFieldValidFormat(FieldFormat format)
        {

            switch (format)
            {
                case FieldFormat.TYP_HIDDEN:
                case FieldFormat.TYP_AUTOINC:
                case FieldFormat.TYP_BITBUTTON:
                case FieldFormat.TYP_ALIAS:
                case FieldFormat.TYP_ALIASRELATION:
                case FieldFormat.TYP_TITLE:
                case FieldFormat.TYP_CHART:
                case FieldFormat.TYP_FILE:
                case FieldFormat.TYP_IFRAME:
                case FieldFormat.TYP_PASSWORD:
                    return false;
                    break;
                default:
                    return true;
                    break;
            }
        }

        /// <summary>
        /// Comportement conditionnels des fields
        /// </summary>
        protected override void BuildConditionalBehaviors()
        {

            // administrer les conditions
            //Comportements conditionnels
            Panel panelRules = new Panel();
            Panel panelRulesSub = new Panel();
            CreateCollapsibleMenu(out panelRules, out panelRulesSub, false, eResApp.GetRes(Pref, 7355), bRights: true);
            panelRules.CssClass = "btnLink";
            panelRules.ID = "adminRules";
            _panelContent.Controls.Add(panelRules);

            int iShortid = this._descid % 100;

            //champs non modifiables
            bool bLockedField = iShortid == (int)AllField.DATE_CREATE
                || iShortid == (int)AllField.USER_CREATE
                || iShortid == (int)AllField.DATE_MODIFY
                || iShortid == (int)AllField.USER_MODIFY
                || _field.Format == FieldFormat.TYP_PASSWORD;
            string sLabel;

            //Visibilité de la rubrique            
            sLabel = String.Concat(eResApp.GetRes(Pref, 7407), " ", GetRulesInfos(TypeTraitConditionnal.FieldView));
            eAdminButtonField button = new eAdminButtonField(sLabel, "buttonAdminConditions", onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.FieldView + "," + _field.DescId + ")");
            button.Generate(panelRulesSub);

            if (!bLockedField)
            {

                //Modification de la rubrique
                sLabel = String.Concat(eResApp.GetRes(Pref, 7408), " ", GetRulesInfos(TypeTraitConditionnal.FieldUpdate));
                button = new eAdminButtonField(sLabel, "buttonAdminConditions", onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.FieldUpdate + "," + _field.DescId + ")");
                button.Generate(panelRulesSub);


                //Saisie obligatoire d'une valeur
                sLabel = String.Concat(eResApp.GetRes(Pref, 7409), " ", GetRulesInfos(TypeTraitConditionnal.FieldObligat));
                button = new eAdminButtonField(sLabel, "buttonAdminConditions", onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.FieldObligat + "," + _field.DescId + ")");
                button.Generate(panelRulesSub);

                //Duplication de la valeur            
                sLabel = eResApp.GetRes(Pref, 7932);
                if (_field.NoDefaultClone)
                    sLabel = String.Concat(sLabel, " (1)");
                button = new eAdminButtonField(sLabel, "buttonAdminConditions", onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.FieldForbidClone + "," + _field.DescId + ")");
                button.Generate(panelRulesSub);

                //Import d'une valeur dans la rubrique
                sLabel = eResApp.GetRes(Pref, 7948);
                if (eSqlSync.IsFieldImportForbidden(Pref, _field.DescId))
                    sLabel = String.Concat(sLabel, " (1)");
                button = new eAdminButtonField(sLabel, "buttonAdminConditions", onclick: "nsAdmin.confConditions(" + (int)TypeTraitConditionnal.FieldForbidImport + "," + _field.DescId + ")");
                button.Generate(panelRulesSub);


                //Liste des comportements conditionnels
                sLabel = String.Concat(eResApp.GetRes(Pref, 7412));
                button = new eAdminButtonField(sLabel, "buttonAdminConditions", onclick: "nsAdmin.listConditions(" + _field.DescId + "," + 0 + ")");
                button.Generate(panelRulesSub);
            }
        }


        /// <summary>
        /// Panneau automatisme
        /// </summary>
        /// <param name="nId"></param>
        /// <returns></returns>
        protected override Panel BuildAutomatismPart()
        {

            Panel panelRulesSecond = new Panel();

            bool adminLevel = Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN;
            bool notifEnabled = NotifConst.NotifEnabled(_ePref, _ePref.User, eModelTools.GetRootPhysicalDatasPath());

            // Pas de niveau admin et pas de notif alors pas de menu Automatismes
            if (!adminLevel && !notifEnabled)
                return panelRulesSecond;

            if ((adminLevel && _field.Format != FieldFormat.TYP_PASSWORD) || notifEnabled)
            {
                Panel panelRulesSecondSub = new Panel();
                CreateCollapsibleMenu(out panelRulesSecond, out panelRulesSecondSub, false, eResApp.GetRes(Pref, 7344), bRights: true);
                panelRulesSecond.CssClass = "btnLink";
                panelRulesSecond.ID = "adminAutomatismes";
                _panelContent.Controls.Add(panelRulesSecond);

                eAdminButtonField button;
                if (notifEnabled)
                {
                    //Ajouter une notification pour la rubrique
                    button = new eAdminButtonField(eResApp.GetRes(Pref, 7356), "buttonAddAdminAutomations", eResApp.GetRes(Pref, 7766), onclick: "nsAdmin.createNotif(" + _tab + ", " + _descid + ", " + AutomationType.NOTIFICATION.GetHashCode() + ")");
                    button.Generate(panelRulesSecondSub);

                    //Liste des notifications pour la rubrique
                    button = new eAdminButtonField(eResApp.GetRes(Pref, 7485).Replace("<COUNT>", GetNotificationNumber(_tab, _descid)), "btnListAutomation_" + _descid, eResApp.GetRes(Pref, 7767), onclick: "nsAdmin.confShowAutomationList(" + _tab + ", " + _descid + ", " + AutomationType.NOTIFICATION.GetHashCode() + ")");
                    button.Generate(panelRulesSecondSub);
                }

                //Automatismes Avancés //BSE: US765 bloquer automatisme pour rubrique type mot de passe
                if (adminLevel && _field.Format != FieldFormat.TYP_PASSWORD)
                {
                    button = new eAdminButtonField(eResApp.GetRes(Pref, 7114), "buttonAdminAdvAutomations", "", onclick: "nsAdmin.confAdvancedAutomatisms(" + _descid + ")");
                    //js : F10 = 121
                    //button.SetControlAttribute("keycode", "121");
                    //button.SetControlAttribute("ctlkey", "1");              
                    button.Generate((Panel)panelRulesSecondSub);
                }

            }

            return panelRulesSecond;
        }

    }
}