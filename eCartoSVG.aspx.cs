using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    public partial class eCartoSVG : eEudoPage
    {
        private Int32 _nTab = 0;
        protected String _json;

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des scripts JS

            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eExpressFilter");
            PageRegisters.AddScript("jquery.min");
            PageRegisters.AddScript("mapael/raphael-min");
            PageRegisters.AddScript("mapael/jquery.mapael");
            PageRegisters.AddScript("mapael/maps/france_departments");


            #endregion

            RetrieveParams();
            RunOperation();


        }


        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Récupération des infos postées
        /// </summary>
        private void RetrieveParams()
        {
            //Table sur laquelle on recherche
            if (_requestTools.AllKeys.Contains("tab"))
                _nTab = eLibTools.GetNum(Request.Form["tab"]);
        }


        /// <summary>
        /// Execute l'action demandée 
        /// </summary>
        private void RunOperation()
        {
            CreateMapaelAreasOptions();
        }

        /// <summary>
        /// Génère les options pour les différentes zones
        /// </summary>
        private void CreateMapaelAreasOptions()
        {
            List<PlotContainer> plots = new List<PlotContainer>();
            String query = String.Concat("SELECT LEFT(PM09, 2) CP, COUNT(*) Nb FROM PM WHERE ISNULL(PM09, '') <> '' GROUP BY LEFT(PM09, 2)");
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
            eDal.OpenDatabase();

            String error = String.Empty;
            DataTableReader dtr = eDal.ExecuteDTR(query, out error);
            var obj = new JObject();
            String tooltip = String.Empty;
            if (dtr != null)
            {

                while (dtr.Read())
                {
                    tooltip = String.Concat("Département ", dtr["CP"].ToString(), "<br />", dtr["Nb"].ToString(), " fiches");
                    obj["department-" + dtr["CP"].ToString()] = JToken.FromObject(new Plot(dtr["CP"].ToString(), dtr["Nb"].ToString(), tooltip));

                }
            }
            eDal.CloseDatabase();

            new JsonSerializerSettings() { };
            _json = JsonConvert.SerializeObject(obj);
        }


    }

    public class PlotContainer
    {
        public Plot PlotValues;

        public PlotContainer(Plot values)
        {
            PlotValues = values;
        }
    }

    public class Plot
    {
        //[JsonProperty(PropertyName = "FooBar")]
        //public String CP { get; private set; }
        [JsonProperty(PropertyName = "value")]
        public String Nb { get; private set; }
        //[JsonProperty(PropertyName = "latitude")]
        //public String Latitude { get; private set; }
        //[JsonProperty(PropertyName = "longitude")]
        //public String Longitude { get; private set; }
        [JsonProperty(PropertyName = "href")]
        public String Href { get; private set; }
        [JsonProperty(PropertyName = "tooltip")]
        public Dictionary<String, String> Tooltip { get; private set; }

        public Plot(String cp, String nb, String tooltipText)
        {
            //CP = cp;
            Nb = nb;
            //Latitude = lat;
            //Longitude = lon;
            //Tooltip tooltip = new Tooltip(tooltipText);
            Href = "#";
            Tooltip = new Dictionary<String, String>();
            Tooltip.Add("content", tooltipText);
        }
    }

    public class Tooltip
    {
        [JsonProperty(PropertyName = "content")]
        public String Content;

        public Tooltip(String text)
        {
            Content = text;
        }
    }
}