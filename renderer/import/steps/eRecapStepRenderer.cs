using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Internal.wcfs.data.import;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class pour faire un rendu de l'étape du résulta d'import
    /// </summary>
    public class eRecapStepRenderer : eComunImportRenderer
    {
        public eRecapStepRenderer(ePref pref, eImportWizardParam wizardParam, eImportSourceInfosCallReturn result)
            : base(pref, wizardParam, result)
        {
        }

        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        public override IWizardStepRenderer Init() { return this; }


        /// <summary>
        /// Execute l'opération du rendu
        /// </summary>
        /// <returns></returns>
        public override Panel Render()
        {
            Panel ctn = new Panel();
            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "data-source-step");
            bool addAdr = WizardParam.ImportParams.Tables.Where(t => t.TabInfo.TabDescId == (int)EudoQuery.TableType.PM || t.TabInfo.TabDescId == (int)EudoQuery.TableType.PP).Count() > 0;
            //Si on a au moins une table pour laquelle on a choisi la création ou la mise à jour 
            if (WizardParam.ImportParams.NbTabImportAllowed > 0)
            {
                //Ajouter le nom du modèle d'import s'il existe
                GetTemplateName(container);

                container.Controls.Add(GetHeaderTabLine(eResApp.GetRes(Pref, 8446).Replace("<NBLINE>", eNumber.FormatNumber(this.ResulWWcf.SourceInfos.LineCount, new eNumber.DecimalParam(Pref) { NumberDigitMin = 0 }, new eNumber.SectionParam(Pref)))));
                foreach (Cache.ImportInfo tab in this.ListImportInfo)
                {
                    IEnumerable<ImportTabParams> importTabParams = WizardParam.ImportParams.Tables.Where(t => t.TabInfo.TabInfoId == tab.GetJsKey());
                    if ((importTabParams != null && importTabParams.Count() == 1 && (importTabParams.First().Create || importTabParams.First().Update))
                        //Ajouter adresse dans l'étape de récap si on a mappé au moins PP ou PM et que adresse n'est pas mappé 
                        //=>dans ce cas là, l'adresse reprend le mapping de certaines rubriques de pp/pm => voir la partie moteur pour les rubriques reprises
                        || importTabParams.Count() == 0 && addAdr && (tab.GetJsKey() == (int)EudoQuery.TableType.PP + "_" + (int)EudoQuery.TableType.ADR || tab.GetJsKey() == (int)EudoQuery.TableType.PM + "_" + (int)EudoQuery.TableType.ADR))
                    {
                        HtmlGenericControl renderer = GetOptionsTabLine(tab);
                        if (renderer != null)
                            container.Controls.Add(renderer);
                    }

                }

            }
            else
                container.Controls.Add(GetHeaderTabLine(eResApp.GetRes(Pref, 1834)));


            ctn.Controls.Add(container);
            return ctn;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="cssName"></param>
        /// <returns></returns>
        public override HtmlGenericControl GetOptionsLine(Cache.ImportInfo tab, string cssName)
        {
            HtmlGenericControl header = new HtmlGenericControl("div");
            header.Attributes.Add("class", "headerOptionsLine");
            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", string.Concat(cssName, " iconHeaderOptions"));
            icon.Style.Add(HtmlTextWriterStyle.Color, Pref.ThemeXRM.Color);
            HtmlGenericControl headerTab = new HtmlGenericControl();
            headerTab.Attributes.Add("class", "divTabOptions");
            headerTab.Style.Add(HtmlTextWriterStyle.Color, Pref.ThemeXRM.Color);
            headerTab.InnerHtml = tab.GetLibelle();
            header.Controls.Add(icon);
            header.Controls.Add(headerTab);
            return header;
        }
    }
}
