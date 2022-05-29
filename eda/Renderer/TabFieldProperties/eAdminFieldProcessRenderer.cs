using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Linq;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldProcessRenderer : eAdminBlockRenderer
    {
        private Int32 _descid;
        private eAdminFieldInfos _field;

        private eAdminFieldProcessRenderer(ePref pref, eAdminFieldInfos field)
            : base(pref, null, eResApp.GetRes(pref, 295), idBlock: "Process")
        {
            _descid = field.DescId;
            _field = field;
        }

        public static eAdminFieldProcessRenderer CreateAdminFieldProcessRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldProcessRenderer features = new eAdminFieldProcessRenderer(pref, field);
            return features;
        }

        /// <summary>Construction du bloc "Catalogue associé"</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            #region Génération auto de la valeur
            List<ListItem> list = CreateGeneratorChoices();
            if (_field.FieldName == "EVT01" || _field.FieldName == "ADR01")
            {
                if (_field.FieldName == "ADR01")
                {
                    var empty = list.SingleOrDefault(v => v.Value == "");
                    if (empty != null)
                        list.Remove(empty);
                }

                eAdminDropdownField field = new eAdminDropdownField(_field.Table.DescId, eResApp.GetRes(Pref, 8165),
                    eAdminUpdateProperty.CATEGORY.DESC,
                    eLibConst.DESC.AUTOBUILDNAME.GetHashCode(),
                    list.ToArray(),
                    value: _field.Table.AutoBuildName, sortItemsByLabel: false, valueFormat: FieldFormat.TYP_CHAR,
                    renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE);
                field.IsOptional = _field.FieldName != "ADR01";
                field.Generate(_panelContent);
            }
            #endregion

            #region Construction des Liens
            if (Pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
            {
                // Automatismes avancés
                eAdminField btnAutomatismes = new eAdminButtonField(eResApp.GetRes(Pref, 7114), "btnAutomatismes", eResApp.GetRes(Pref, 7115), String.Format("nsAdmin.confAdvancedAutomatisms({0});", _descid));
                btnAutomatismes.Generate(_panelContent);
            }
            #endregion

            // Annulation de la dernière saisie autorisée
            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.File_CancelLastEntries))
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("1", eResApp.GetRes(Pref, 8255));
                dic.Add("0", eResApp.GetRes(Pref, 8256));
                eAdminField rb = new eAdminRadioButtonField(_field.DescId, eResApp.GetRes(Pref, 8238), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.CANCELLASTVALUE.GetHashCode(), "rbCancelValue", dic,
                     tooltiptext: eResApp.GetRes(Pref, 8239), value: _field.CancelLastValueAllowed ? "1" : "0");
                rb.IsLabelBefore = true;
                rb.Generate(_panelContent);
            }


            return true;
        }

        /// <summary>
        /// Génère la liste des choix possibles pour l'AutoBuildName
        /// </summary>
        /// <returns></returns>
        List<ListItem> CreateGeneratorChoices()
        {
            String error = "";
            eAdminTableInfos tabInfos = _field.Table;
            int tableDescid = tabInfos.DescId;

            int ppDid = TableType.PP.GetHashCode();
            int pmDid = TableType.PM.GetHashCode();
            int pp01Did = TableType.PP.GetHashCode() + 1;
            int pm01Did = TableType.PM.GetHashCode() + 1;
            int evt95Did = tableDescid + AllField.DATE_CREATE.GetHashCode();

            // Récupération des RES
            StringBuilder sb = new StringBuilder();
            sb.Append((evt95Did));
            //if (tabInfos.InterPP)
            sb.Append(";").Append((TableType.PP.GetHashCode()));
            sb.Append(";").Append((TableType.PP.GetHashCode() + 1));
            //if (tabInfos.InterPM)
            sb.Append(";").Append((TableType.PM.GetHashCode()));
            sb.Append(";").Append((TableType.PM.GetHashCode() + 1));
            Dictionary<int, String> dicRes = eLibTools.GetRes(Pref, sb.ToString(), Pref.Lang, out error);

            List<ListItem> list = new List<ListItem>();


            list.Add(new ListItem(String.Concat("<", eResApp.GetRes(Pref, 8166), ">"), "")); // Aucune valeur

            // Nom de l’onglet.Créé le 
            list.Add(new ListItem(
                String.Concat("<", tabInfos.TableLabel, ".", dicRes[evt95Did], ">"),
                String.Concat("$", AllField.DATE_CREATE.GetHashCode(), "$")));

            if (tabInfos.InterPP)
                // Contact.Nom
                list.Add(new ListItem(String.Concat("<", dicRes[ppDid], ".", dicRes[pp01Did], ">"),
                    String.Concat("$", pp01Did, "$")));

            if (tabInfos.InterPM)
                // Société.Raison sociale
                list.Add(new ListItem(String.Concat("<", dicRes[pmDid], ".", dicRes[pm01Did], ">"), String.Concat("$", pm01Did, "$")));

            if (tabInfos.InterPM && tabInfos.InterPP)
            {
                // « Contact.Nom » (PP.PP01) - « Société.Raison sociale » (PM.PM01)
                list.Add(new ListItem(String.Concat("<", dicRes[ppDid], ".", dicRes[pp01Did], ">", " - ", "<", dicRes[pmDid], ".", dicRes[pm01Did], ">"),
                    String.Concat("$", pp01Did, "$", " - ", "$", pm01Did, "$")));

                // « Société.Raison sociale » (PM.PM01) - « Contact.Nom » (PP.PP01)
                list.Add(new ListItem(String.Concat("<", dicRes[pmDid], ".", dicRes[pm01Did], ">", " - ", "<", dicRes[ppDid], ".", dicRes[pp01Did], ">"),
                    String.Concat("$", pm01Did, "$", " - ", "$", pp01Did, "$")));
            }

            if (tabInfos.InterPP)
                // « Nom de l’onglet.Créé le » - « Contact.Nom » (PP.PP01)
                list.Add(new ListItem(String.Concat("<", tabInfos.TableLabel, ".", dicRes[evt95Did], ">", " - ", "<", dicRes[ppDid], ".", dicRes[pp01Did], ">"),
                    String.Concat("$", AllField.DATE_CREATE.GetHashCode(), "$", " - ", "$", pp01Did, "$")));

            if (tabInfos.InterPM)
                // « Nom de l’onglet.Créé le » - « Société.Raison sociale » (PM.PM01)
                list.Add(new ListItem(String.Concat("<", tabInfos.TableLabel, ".", dicRes[evt95Did], ">", " - ", "<", dicRes[pmDid], ".", dicRes[pm01Did], ">"),
              String.Concat("$", AllField.DATE_CREATE.GetHashCode(), "$", " - ", "$", pm01Did, "$")));

            if (tabInfos.InterPM && tabInfos.InterPP)
            {
                // « Nom de l’onglet.Créé le » - « Contact.Nom » (PP.PP01) - « Société.Raison sociale » (PM.PM01)
                list.Add(new ListItem(String.Concat("<", tabInfos.TableLabel, ".", dicRes[evt95Did], ">", " - ", "<", dicRes[ppDid], ".", dicRes[pp01Did], ">", " - ", "<", dicRes[pmDid], ".", dicRes[pm01Did], ">"),
                String.Concat("$", AllField.DATE_CREATE.GetHashCode(), "$", " - ", "$", pp01Did, "$", " - ", "$", pm01Did, "$")));

                // « Nom de l’onglet.Créé le » - « Société.Raison sociale » (PM.PM01) - « Contact.Nom » (PP.PP01)
                list.Add(new ListItem(String.Concat("<", tabInfos.TableLabel, ".", dicRes[evt95Did], ">", " - ", "<", dicRes[pmDid], ".", dicRes[pm01Did], ">", " - ", "<", dicRes[ppDid], ".", dicRes[pp01Did], ">"),
                     String.Concat("$", AllField.DATE_CREATE.GetHashCode(), "$", " - ", "$", pm01Did, "$", " - ", "$", pp01Did, "$")));
            }

            return list;
        }


    }
}