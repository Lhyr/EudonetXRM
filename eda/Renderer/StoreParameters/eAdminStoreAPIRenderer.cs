using System.Collections.Generic;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Web.UI.WebControls;
using System;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model.api;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu de l'écran de paramétrage de l'extension API
    /// </summary>
    public class eAdminStoreAPIRenderer : eAdminStoreFileRenderer
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreAPIRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// Méthode statique qui permet  de générer le Rendu de l'écran de paramétrage de l'extension API
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext">Objet représentant l'extension</param>
        /// <returns></returns>
        public static eAdminStoreAPIRenderer CreateAdminStoreAPIRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            eAdminStoreAPIRenderer rdr = new eAdminStoreAPIRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// initialisation du composant
        /// Vérification de l'activation de l'api
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.API_ENABLED
                    });

                return true;
            }
            return false;
        }


        /// <summary>
        /// écran de paramétrage
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();



            APIParams paramAPI = APIParams.GetAPIParams(_eRegisteredExt.Param, _ePref);

            string sURLAPI = eLibTools.GetServerConfig("apiurl", "");

            if (eLibTools.TestURL(sURLAPI) != System.Net.HttpStatusCode.OK)
            {
                sURLAPI = eModelTools.GetBaseUrl().TrimEnd('/') + "/eudoapi/";
                if (eLibTools.TestURL(sURLAPI) != System.Net.HttpStatusCode.OK)
                {
                    sURLAPI = "<URL Inaccessible>";
                }
            }

            //Information(s) générale(s)
            List<Tuple<string, string, eAdminField>> lstInfos = new List<Tuple<string, string, eAdminField>>();
            lstInfos.Add(new Tuple<string, string, eAdminField>("URL API", sURLAPI, null));
            lstInfos.Add(new Tuple<string, string, eAdminField>(eResApp.GetRes(_ePref, 2313), _eRegisteredExt?.DateEnabled.ToString("yyyy/MM/dd HH:mm") ?? "", null));

            if (_ePref.User.UserLevel < 100)
            {
                lstInfos.Add(new Tuple<string, string, eAdminField>(eResApp.GetRes(_ePref, 2674), paramAPI.CALL_BY_MIN.ToString(), null));
            }

            ExtensionParametersContainer.Controls.Add(GetInfosSection("apinfos", eResApp.GetRes(_ePref, 5080), "", lstInfos));

            if (_ePref.User.UserLevel >= 100)
            {
                //Paramétrage
                Panel section = GetModuleSection(Extension.Module.ToString(), eResApp.GetRes(_ePref, 2675));

                ExtensionParametersContainer.Controls.Add(section);

                Panel targetPanel = new Panel();
                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];

                Dictionary<int, string> itemList = new Dictionary<int, string>();


                int selectedValue = 60;


                itemList.Add(60, "60");
                itemList.Add(90, "90");
                itemList.Add(120, "120");
                itemList.Add(240, "240");
                itemList.Add(360, "360");



                Panel apiParamPanel = new Panel();
                targetPanel.Controls.Add(apiParamPanel);
                apiParamPanel.CssClass = "field";
                apiParamPanel.ID = String.Concat("API_", "LIMITS");
                HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
                fieldLabel.InnerText = eResApp.GetRes(_ePref, 2674);
                apiParamPanel.Controls.Add(fieldLabel);

                DropDownList ddl = new DropDownList();
                apiParamPanel.Controls.Add(ddl);


                ddl.ID = "apicallbymin";

                if (itemList.ContainsKey(paramAPI.CALL_BY_MIN))
                    ddl.SelectedValue = paramAPI.CALL_BY_MIN.ToString();

                ddl.Attributes.Add("onchange", "nsAdminApi.changeRate(this)");

                ddl.DataSource = itemList;
                ddl.DataTextField = "Value";
                ddl.DataValueField = "Key";
                ddl.DataBind();
            }

        }



    }
}


