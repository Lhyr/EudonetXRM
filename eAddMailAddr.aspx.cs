using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    public partial class eAddMailAddr : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eAddMailAddr");

            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");

            #endregion

            #region ajout des js

            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eAddMailAddr");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");


            #endregion

            Int32 iPpid = 0, iAdrid = 0, iPmid = 0;
            String sSearch = "";
            Int32 iFileType = EdnType.FILE_MAIL.GetHashCode();

            if (_requestTools.AllKeys.Contains("ppid") && !String.IsNullOrEmpty(Request.Form["ppid"]))
                Int32.TryParse(Request.Form["ppid"].ToString(), out iPpid);

            if (_requestTools.AllKeys.Contains("pmid") && !String.IsNullOrEmpty(Request.Form["pmid"]))
                Int32.TryParse(Request.Form["pmid"].ToString(), out iPmid);

            if (_requestTools.AllKeys.Contains("adrid") && !String.IsNullOrEmpty(Request.Form["adrid"]))
                Int32.TryParse(Request.Form["adrid"].ToString(), out iAdrid);

            if (_requestTools.AllKeys.Contains("search") && !String.IsNullOrEmpty(Request.Form["search"]))
                sSearch = Request.Form["search"].ToString();

            if (_requestTools.AllKeys.Contains("filetype") && !String.IsNullOrEmpty(Request.Form["filetype"]))
                Int32.TryParse(Request.Form["filetype"].ToString(), out iFileType);

            eAddMailAddrData data = new eAddMailAddrData(_pref);
            data.PPID = iPpid;
            data.PMID = iPmid;
            data.ADRID = iAdrid;
            data.Search = sSearch;
            data.FileType = iFileType;
            if (data.FileType == EdnType.FILE_SMS.GetHashCode())
            {
                data.RequiredFieldFormats = new List<FieldFormat>() { FieldFormat.TYP_PHONE };
                data.RequiredUserFields = new List<UserField>() { UserField.TEL, UserField.MOBILE };
            }
            else
            {
                data.RequiredFieldFormats = new List<FieldFormat>() { FieldFormat.TYP_EMAIL };
                data.RequiredUserFields = new List<UserField>() { UserField.EMAIL };
            }

            data.LaunchSearch();


            Boolean bOdd = true;
            foreach (String s in data.DataList)
            {
                HtmlGenericControl li = new HtmlGenericControl("li");
                li.InnerText = s;
                ulListAddr.Controls.Add(li);
                if (bOdd)
                {
                    li.Attributes.Add("class", "odd");
                    bOdd = false;
                }
                else
                {
                    bOdd = true;
                }
            }

            adrid.Value = data.ADRID.ToString();
            pmid.Value = data.PMID.ToString();
            ppid.Value = data.PPID.ToString();
            fileType.Value = data.FileType.ToString();
            search.Value = data.Search;
        }
    }
}