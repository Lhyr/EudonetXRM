using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.xadminst
{
    public partial class xadmlgn : eEudoPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eudoFont");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");

            Dictionary<String, String> aa = eDataTools.GetUsersLang(_pref);
            lguser.Items.Clear();

            foreach (KeyValuePair<String, string> kv in aa)
                lguser.Items.Add(new ListItem(kv.Value, kv.Key.ToString()));
            

            if (lguser.Items.FindByValue(_pref.LangId.ToString()) != null)
                lguser.Items.FindByValue(_pref.LangId.ToString()).Selected = true; ;
        }


        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }
    }
}