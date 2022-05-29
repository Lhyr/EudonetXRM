using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    public partial class eImportCatalogDialog : eEudoPage
    {       


        protected void Page_Load (object sender, EventArgs e)
        {
            Dictionary<string, string> usersLang = new Dictionary<string, string>();
            string lang = string.Empty;
            usersLang = eDataTools.GetUsersLangFilter(_pref);
            foreach (KeyValuePair<string,string> item in usersLang)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    lang = lang + string.Concat("LIB_", (Int32.Parse(item.Key) < 10) ? ("LANG_0" + Int32.Parse(item.Key)) : "LANG_" + item.Key.ToString()) + ";";
                    lang = lang + string.Concat("INFOBULLE_", (Int32.Parse(item.Key) < 10) ? ("LANG_0" + Int32.Parse(item.Key)) : "LANG_" + item.Key.ToString()) + ";";
                }
                else
                {
                    lang = lang + string.Concat("LIB_", item.Value.ToUpper()) + ";";
                    lang = lang + string.Concat("INFOBULLE_", item.Value.ToUpper()) + ";";
                }
            }


            lang = lang.Remove(lang.Length - 1);

            eTextImportCat.InnerText = "ID;PARENTID;CODE;DESACTIVE;"+lang;
        }


        public override Control GetHeadPlaceHolder ()
        {
            return scriptHolder;
        }
    }
}