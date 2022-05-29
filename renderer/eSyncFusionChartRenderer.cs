using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Linq;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.renderer
{
    public class eSyncFusionChartRenderer : eChartRenderer
    {

        public eSyncFusionChartRenderer(ePref pref, int tab) : base(pref, tab)
        {

        }

        /// <summary>
        /// Retourne le code html du graphique
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="chartId">Id du graphique</param>
        /// <param name="getExportMenu">Indique si on intègre ou pas le menu d'export du graphqiue</param>
        /// <param name="fromHomePage">Indique si on vient de la page d'accueil</param>
        /// <returns></returns>
        public static HtmlGenericControl GetHtmlChart(ePref pref, string chartId, bool getExportMenu = true, bool fromHomePage = true)
        {
            int reportId = 0;
            int.TryParse(chartId.Split("_")[0], out reportId);

            HtmlGenericControl divContainer = new HtmlGenericControl("div");
            divContainer.Attributes.Add("class", "exportChartMenu  ");
            divContainer.Style.Add(System.Web.UI.HtmlTextWriterStyle.Height, "100%");

            HtmlGenericControl divChart = new HtmlGenericControl("div");

            if (getExportMenu)
            {
                HtmlGenericControl divExportChart = new HtmlGenericControl("div");
                divExportChart.ID = "exPortChart" + chartId.ToString();
                divExportChart.Attributes.Add("load", "DivChart" + chartId.ToString() + "_canvas");
                divExportChart.Attributes.Add("class", "icon-ellipsis-v advExportChartMenu  ");
                divExportChart.Attributes.Add("fHome", fromHomePage ? "1" : "0");
                divExportChart.Attributes.Add("onmouseover", "dispExportMenu(this);;");
                divContainer.Controls.Add(divExportChart);
            }

            divChart.ID = "DivChart" + chartId.ToString();
            divChart.Attributes.Add("syncFusionChart", "1");
            divChart.Attributes.Add("class", "SyncFusionChartContainer");
            // Chargement en cours ...
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "xrm-widget-waiter");

            HtmlGenericControl img = new HtmlGenericControl("img");
            img.Attributes.Add("alt", "wait");
            img.Attributes.Add("src", "themes/default/images/wait.gif");
            div.Controls.Add(img);

            divChart.Controls.Add(div);


            divContainer.Controls.Add(divChart);

            return divContainer;
        }



        /// <summary>
        /// Retourne un renderer pour le flux XML d'un chart
        /// </summary>
        /// <param name="pref">Préférence</param>
        /// <param name="nReportId">Id du rapport</param>
        /// <param name="nFileId">Id de la fiche pour les charts en mode fiche</param>
        /// <returns></returns>
        public static eSyncFusionChartRenderer GetStatisticalChartRendererXML(ePref pref, int nTab, int nTabFrom, int nIdFrom, int nDescId, int nField, eCommunChart.TypeAgregatFonction agregat, int nFileId = 0)
        {

            //Construction et initialisation

            eSyncFusionChartRenderer ecRenderer = new eSyncFusionChartRenderer(pref, nTab);
            ecRenderer._eC = eChartStatistique.CreateChartXML(pref, nTab, nTabFrom, nIdFrom, nDescId, nField, agregat, nFileId);


            //Erreur sur le chart
            if (ecRenderer._eC.ErrorMsg.Length > 0)
            {

                ecRenderer._sErrorMsg = ecRenderer._eC.ErrorMsg;
                if (ecRenderer._eC.InnerException != null)
                    ecRenderer._eException = ecRenderer._eC.InnerException;
                ecRenderer.XMLData = GetErrorLuncher(ecRenderer._eC.ErrorMsg);
                return ecRenderer;
            }

            return ecRenderer;
        }


        /// <summary>
        ///  Retourne le nom de la table
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns>string</returns>
        public static string GetTableNameForExportChart(ePref pref, int nTab)
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


        /// <summary>
        ///  Retourne le format d'un champ donné
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nField"></param>
        /// <returns>string</returns>
        public static new FieldFormat GetStatFieldFormat(ePref pref, int nField)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                eFieldLiteWithLib field = new eFieldLiteWithLib(nField, pref.Lang);

                string err = string.Empty;
                dal?.OpenDatabase();
                field.ExternalLoadInfo(dal, out err);
                return field.Format;
            }
            finally
            {
                dal?.CloseDatabase();
            }


        }

        /// <summary>
        /// Récuperer les informations d'une rubrique à partir d'un descid 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nField"></param>
        /// <returns></returns>
        public static Field GetFieldInfoFromDescid(ePref pref, int nTab, int nField)
        {
            EudoQuery.EudoQuery query = eLibTools.GetEudoQuery(pref, nTab, ViewQuery.FILE);
            query.LoadRequest();

            return query.GetFieldHeaderList.Where(f => f.Table.DescId == nTab && f.Descid == nField).First();
        }


    }
}