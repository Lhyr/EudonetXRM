using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtendedTargetMappingsDialogRenderer : eAdminRenderer
    {
        #region propriétés
        private Int32 _nTab;
        private String _tabName;
        private eAdminTableInfos _tabInfos;

        private List<eAdminFieldInfos> _listFieldsInfos;
        private List<int> _listMappingDescid;
        private Dictionary<int, KeyValuePair<string, Tuple<FieldFormat, int>>> _listMappingFields;

        private Panel _panelMainDiv;
        private Panel _panelSectionDiv;
        #endregion

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        public eAdminExtendedTargetMappingsDialogRenderer(ePref pref, int nTab)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = nTab;
            _tabName = _tabInfos.TableLabel;
        }

        public static eAdminExtendedTargetMappingsDialogRenderer CreateAdminExtendedTargetMappingsDialogRenderer(ePref pref, int nTab)
        {
            return new eAdminExtendedTargetMappingsDialogRenderer(pref, nTab);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                Dictionary<int, string> dicoFieldsNames;
                Dictionary<int, int> dicoFieldsFormats;

                _tabInfos.GetFields(Pref, out dicoFieldsNames, out dicoFieldsFormats);


                _listFieldsInfos = new List<eAdminFieldInfos>();

                _listMappingDescid = new List<int>();


                //Liste des champs de la table  au format/descid compatible avec un mapping
                List<Int32> lst = dicoFieldsFormats.Where(f =>
                    (f.Value == (int)FieldFormat.TYP_CHAR || f.Value == (int)FieldFormat.TYP_EMAIL || f.Value == (int)FieldFormat.TYP_PHONE)
                    && (f.Key % 100 < eLibConst.MAX_NBRE_FIELD)).Select(aa => aa.Key).ToList();

                //Information détaillées sur ces champs
                _listFieldsInfos = eAdminFieldInfos.GetAdminFieldsInfos(Pref, lst);

                //Uniquement les champs flaggé Cibles Etendues
                _listFieldsInfos.RemoveAll(zz => zz.ProspectEnabled < 1);

                // Tri ?
                _listFieldsInfos.Sort(eAdminFieldInfos.CompareByDescId);

                //Descid source des champs mappé sur la table
                _listMappingDescid = _listFieldsInfos.Where(zz => (zz.ProspectEnabled ?? 0) > 1).Select(zz => zz.ProspectEnabled.Value).ToList();

                // On a déjà tous les champs, inutile de refaire une requêtre
                //_listMappingDescid = _tabInfos.GetExtendedTargetMappingList(Pref);

                //Champ disponible pour être mappé sur PP/PM/ADR
                _listMappingFields = _tabInfos.GetTargetMappingInfos(Pref);

                return true;
            }
            else
                return false;
        }

        protected override bool Build()
        {
            if (base.Build())
            {
                _pgContainer.ID = "extendedTargetMappingsAdminModalContent";
                _pgContainer.Attributes.Add("class", "adminModalContent");

                CreateMainDiv();

                return true;
            }
            else
                return false;
        }

        private void CreateMainDiv()
        {
            _panelMainDiv = new Panel();
            _panelMainDiv.ID = "edaExtmpMainDiv";

            // Champs des mappings
            CreateSection();
            CreateTitle(eResApp.GetRes(Pref, 7625));

            foreach (eAdminFieldInfos fieldInfos in _listFieldsInfos)
            {
                string label = fieldInfos.Labels[Pref.LangId];
                if (String.IsNullOrEmpty(label))
                    label = String.Concat(eResApp.GetRes(Pref, 7624), " (", fieldInfos.DescId, ")");

                if (fieldInfos.ProspectEnabled.HasValue)
                    CreateRubriques(String.Concat(label, " :"), fieldInfos, _listMappingFields);
            }

            _pgContainer.Controls.Add(_panelMainDiv);
        }

        private void CreateSection()
        {
            _panelSectionDiv = new Panel();
            _panelSectionDiv.CssClass = "edaExtmpSection";
            _panelMainDiv.Controls.Add(_panelSectionDiv);
        }

        private enum COLUMN
        {
            LABEL = 1,
            LIST = 2
        }

        private HtmlGenericControl GetSpan(COLUMN columm)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");

            switch (columm)
            {
                case COLUMN.LABEL:
                    span.Attributes.Add("class", "edaExtmpLabel");
                    break;
                case COLUMN.LIST:
                    span.Attributes.Add("class", "edaExtmpList");
                    break;
                default:
                    break;
            }

            return span;
        }

        private void CreateTitle(string text)
        {
            Panel field = new Panel();
            field.CssClass = "title";
            _panelSectionDiv.Controls.Add(field);

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerText = text;
            field.Controls.Add(span);
        }

        private void CreateRubriques(string label, eAdminFieldInfos fieldInfos, Dictionary<int, KeyValuePair<string, Tuple<FieldFormat, int>>> datasource)
        {
            Panel field = new Panel();
            field.ID = String.Concat("field", fieldInfos.DescId);
            field.CssClass = "field";

            HtmlGenericControl spanLabel = GetSpan(COLUMN.LABEL);
            HtmlGenericControl spanList = GetSpan(COLUMN.LIST);
            field.Controls.Add(spanLabel);
            field.Controls.Add(spanList);

            string selectName = String.Concat("ddl", fieldInfos.DescId);

            HtmlGenericControl lbl = new HtmlGenericControl("label");
            lbl.Attributes.Add("id", String.Concat("lbl", fieldInfos.DescId));
            lbl.Attributes.Add("for", selectName);
            lbl.InnerText = label;
            spanLabel.Controls.Add(lbl);

            HtmlGenericControl ddl = new HtmlGenericControl("select");
            ddl.Attributes.Add("id", selectName);
            ddl.Attributes.Add("name", selectName);
            ddl.Attributes.Add("did", fieldInfos.DescId.ToString());

            spanList.Controls.Add(ddl);


            int selectedValue = fieldInfos.ProspectEnabled ?? 1;

            // SPH 26/09/2017 : pour l'instant on laisse le disabled
            //ajoute de fieldInfos.Format == FieldFormat.TYP_CHAR pour permettre de choisir un autre mapping 
            // que celui par défaut : sur la v7, il était possible de choisir à la création, contrairement à XRM


            //BSE:#59 784
            //if (selectedValue > 1 /*&& fieldInfos.Format == FieldFormat.TYP_CHAR*/)
            //    ddl.Attributes.Add("disabled", "disabled");
            //else
            {
                ddl.Attributes.Add("onchange", "nsAdmin.confirmAdminExtendedTargetMappingsToggleField(this);");
                ddl.Attributes.Add("edaExtmpOldvalue", selectedValue.ToString());
                ddl.Attributes.Add("edaFieldLenght", GetFieldLength(fieldInfos).ToString());
                ddl.Attributes.Add("edaFieldName", label.Replace(":", ""));
            }

            HtmlGenericControl option = new HtmlGenericControl("option");
            option.Attributes.Add("value", "1");
            option.InnerText = eResApp.GetRes(Pref, 6211);
            if (selectedValue == 1)
                option.Attributes.Add("selected", "selected");

            ddl.Controls.Add(option);

            foreach (KeyValuePair<int, KeyValuePair<string, Tuple<FieldFormat, int>>> kvp in datasource)
            {
                option = new HtmlGenericControl("option");
                option.Attributes.Add("value", kvp.Key.ToString());

                //Pour les champs "system" de cibles étendues, on ne propose de mappé que les champs du même type
                // ne concernce que les champ tél & mail qui ne sont plus paramétrage à la création (contrairement à la v7)
                if (fieldInfos.DescId % 100 < 14 && fieldInfos.Format != kvp.Value.Value.Item1)
                    continue;

                option.InnerText = kvp.Value.Key;
                option.Attributes.Add("edaFieldLenght", kvp.Value.Value.Item2.ToString());
                if (selectedValue == kvp.Key)
                    option.Attributes.Add("selected", "selected");

                if (selectedValue != kvp.Key && _listMappingDescid.Contains(kvp.Key))
                    option.Attributes.Add("disabled", "disabled");

                ddl.Controls.Add(option);
            }

            _panelSectionDiv.Controls.Add(field);
        }

        /// <summary>
        /// Récuperer la taille SQL de la rubrique
        /// </summary>
        /// <param name="fieldInfos"></param>
        /// <returns></returns>
        private int GetFieldLength(eAdminFieldInfos fieldInfos)
        {
            int fieldLenght = fieldInfos.Length;
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {
                dal?.OpenDatabase();
                fieldLenght = eLibTools.CheckFieldSize(dal, fieldInfos.Table.TabName, fieldInfos.FieldName, fieldLenght, false);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                dal?.CloseDatabase();
            }

            return fieldLenght;
        }

    }
}