using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldsRGPDListRendererForExport : eAdminFieldsRGPDListRenderer
    {

        private List<eAdminFieldInfos> ExportListFields
        {
            get { return _listFields.Where(f => _descAdv.GetAdvInfoValue(f.DescId, DESCADV_PARAMETER.RGPD_ENABLED, "0") == "1").ToList(); }
        }

        public List<eAdminFieldsListRenderer.ListCol> ListColumns
        {
            get { return _listColumns; }
        }

        public eAdminFieldsRGPDListRendererForExport(ePref pref, int tab) : base(pref, tab)
        {

        }


        public List<string[]> GetExportLine()
        {
            List<string[]> lines = new List<string[]>();
            List<List<string>> Infos = new List<List<string>>();
            List<DESCADV_PARAMETER> _params = new List<DESCADV_PARAMETER> {
                DESCADV_PARAMETER.RGPD_NATURE
                , DESCADV_PARAMETER.RGPD_PERSONNAL_CATEGORY
                , DESCADV_PARAMETER.RGPD_CATEGORY_PRECISION
                , DESCADV_PARAMETER.RGPD_DATA_PURPOSE
                , DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_ENABLED
                , DESCADV_PARAMETER.RGPD_RESPONSIBLE_1
                , DESCADV_PARAMETER.RGPD_RESPONSIBLE_2
                , DESCADV_PARAMETER.RGPD_RESPONSIBLE_3
            };


            // Nom SQL du champ
            Infos.Add(ExportListFields.Select(f => f.FieldName).ToList());

            //Libellé
            Infos.Add(ExportListFields.Select(f => f.Labels[Pref.LangId]).ToList());


            foreach (DESCADV_PARAMETER item in _params)
            {
                Infos.Add(GetFieldInfos(item));
            }

            if (this.ListColumns?.Count > 0 && Infos.Count == this.ListColumns?.Count)
            {


                for (int i = 0; i < this.ListColumns?.Count; i++)
                {
                    lines.Add(new string[Infos[i].Count + 1]);
                    lines[i][0] = this.ListColumns[i].Label;
                    for (int j = 0; j < Infos[i].Count; j++)
                    {
                        lines[i][j + 1] = Infos[i][j];
                    }



                }
            }
            return lines;
        }

        protected override bool Init()
        {
            if (base.Init())
            {

                return true;
            }

            return false;
        }

        protected override bool Build()
        {
            return true;
        }


        private List<string> GetFieldInfos(DESCADV_PARAMETER param)
        {
            List<string> Info = new List<string>();
            List<List<string>> Infos = new List<List<string>>();

            switch (param)
            {

                case DESCADV_PARAMETER.RGPD_NATURE:

                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Nature
                        string natureLabel = String.Empty;
                        string nature = _descAdv.GetAdvInfoValue(field.DescId, param, ((int)DESCADV_RGPD_DEFAULT_VALUES.NATURE).ToString());
                        DESCADV_RGPD_NATURE natureValue;
                        if (DESCADV_RGPD_NATURE.TryParse(nature, out natureValue))
                            natureLabel = _dicoNatureLabels[natureValue];

                        Info.Add(natureLabel);
                    }

                    break;
                case DESCADV_PARAMETER.RGPD_PERSONNAL_CATEGORY:
                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Catégorie
                        string natureLabel = String.Empty;
                        string categoryLabel = String.Empty;
                        string nature = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_NATURE, ((int)DESCADV_RGPD_DEFAULT_VALUES.NATURE).ToString());
                        DESCADV_RGPD_NATURE natureValue;
                        DESCADV_RGPD_NATURE.TryParse(nature, out natureValue);

                        if (natureValue == DESCADV_RGPD_NATURE.PERSONAL)
                        {
                            string category = _descAdv.GetAdvInfoValue(field.DescId, param, ((int)DESCADV_RGPD_DEFAULT_VALUES.PERSONAL_CATEGORY).ToString());
                            DESCADV_RGPD_PERSONNAL_CATEGORY categoryValue;
                            if (DESCADV_RGPD_PERSONNAL_CATEGORY.TryParse(category, out categoryValue))
                                categoryLabel = _dicoPersonalCatLabels[categoryValue];
                        }
                        else if (natureValue == DESCADV_RGPD_NATURE.SENSITIVE)
                        {
                            string category = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_SENSIBLE_CATEGORY, ((int)DESCADV_RGPD_DEFAULT_VALUES.SENSITIVE_CATEGORY).ToString());
                            DESCADV_RGPD_SENSITIVE_CATEGORY categoryValue;
                            if (DESCADV_RGPD_SENSITIVE_CATEGORY.TryParse(category, out categoryValue))
                                categoryLabel = _dicoSensitiveCatLabels[categoryValue];
                        }

                        Info.Add(categoryLabel);
                    }
                    break;
                case DESCADV_PARAMETER.RGPD_CATEGORY_PRECISION:

                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Catégorie Autre
                        string categoryPrecision = _descAdv.GetAdvInfoValue(field.DescId, param, String.Empty);
                        Info.Add(categoryPrecision);
                    }
                    break;
                case DESCADV_PARAMETER.RGPD_DATA_PURPOSE:
                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Utilisation
                        string dataPurpose = _descAdv.GetAdvInfoValue(field.DescId, param, String.Empty);
                        Info.Add(dataPurpose);
                    }
                    break;
                case DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_ENABLED:
                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Pseudonymisation
                        bool pseudoEnabled = _descAdv.GetAdvInfoValue(field.DescId, param, "0") == "1";
                        Info.Add(pseudoEnabled ? eResApp.GetRes(Pref, 58) : eResApp.GetRes(Pref, 59));
                    }
                    break;

                case DESCADV_PARAMETER.RGPD_RESPONSIBLE_1:
                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Responsable traitement 1
                        Info.Add(GetUserGroupLabel(field.DescId, param));
                    }
                    break;
                case DESCADV_PARAMETER.RGPD_RESPONSIBLE_2:
                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Responsable traitement 2
                        Info.Add(GetUserGroupLabel(field.DescId, param));
                    }
                    break;
                case DESCADV_PARAMETER.RGPD_RESPONSIBLE_3:
                    foreach (eAdminFieldInfos field in ExportListFields)
                    {
                        //Responsable traitement 3
                        Info.Add(GetUserGroupLabel(field.DescId, param));
                    }
                    break;


                default:
                    break;
            }

            return Info;
        }


        /// <summary>
        ///  Retourne le nom de la table
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns>string</returns>
        public static string GetTableName(ePref pref, Int32 nTab)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                eTableLiteWithLib tab = new eTableLiteWithLib(nTab, pref.Lang);
                string err = string.Empty;
                dal?.OpenDatabase();
                tab.ExternalLoadInfo(dal, out err);
                return tab.Libelle;
            }
            finally
            {
                dal?.CloseDatabase();
            }


        }
    }

    public class Line
    {
        public string Libelle { get; set; }
        public List<string> Value { get; set; }
    }
}