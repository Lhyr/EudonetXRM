using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Form appeler pour selectionner la date de début et la date de fin pour le filtre express
    /// </summary>
    public partial class eDateSelect : eEudoPage
    {
        /// <summary>variables js</summary>
        public string jsStringVars;

        /// <summary>Nom javascript de la modale</summary>
        public String _modalVarNameStart = string.Empty;
        public String _modalVarNameEnd = string.Empty;
        /// <summary>Nom javascript de la modale</summary>
        public String _modalVarName = string.Empty;

        string strDateStart = string.Empty;
        string userDateStart = string.Empty;

        string strDateEnd = string.Empty;
        string userDateEnd = string.Empty;

        string strHourStart = string.Empty;
        string strMinStart = string.Empty;

        string strHourEnd = string.Empty;
        string strMinEnd = string.Empty;

        /// <summary>
        /// Text box contenant la date de debut
        /// </summary>
        public HtmlInputText tbDateFrom;

        /// <summary>
        /// Text box contenant la date de fin
        /// </summary>
        public HtmlInputText tbDateEnd;

        private Int32 _nTab;
        private Int32 _nTabFrom;
        private Int32 _nIdFrom;
        private Int32 _nDescId;

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

            PageRegisters.AddCss("ecalendar");

            #endregion


            #region ajout de js

            PageRegisters.AddScript("eCalendar");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eTools");

            #endregion

            #region Variables de session

            Int32 _groupId = _pref.User.UserGroupId;
            Int32 _userLevel = _pref.User.UserLevel;

            String _lang = _pref.Lang;

            Int32 _userId = _pref.User.UserId;
            String _instance = _pref.GetSqlInstance;
            String _baseName = _pref.GetBaseName;

            #endregion

            DateTime dtUserDate = DateTime.Now;
            dtUserDate = eLibTools.ClearHour(dtUserDate);
            DateTime dtDate = dtUserDate;
            int nHideHourField = 0;
            int nHideNoDate = 0;
            string strHour = string.Empty;
            string strMin = string.Empty;


            if (Request.Form["HideHourField"] != null)
            {
                Int32.TryParse(Request.Form["HideHourField"].ToString(), out nHideHourField);
            }

            if (Request.Form["HideNoDate"] != null)
            {
                Int32.TryParse(Request.Form["HideNoDate"].ToString(), out nHideNoDate);
            }


            StringBuilder sbJs = new StringBuilder();

            sbJs.AppendLine(string.Concat("var strDateStart = '", dtDate.ToString("dd/MM/yyyy HH:mm"), "';"));
            sbJs.AppendLine(string.Concat("var userDateStart = '", dtUserDate.ToString("dd/MM/yyyy"), "';"));

            sbJs.AppendLine(string.Concat("var strDateEnd = '", dtDate.ToString("dd/MM/yyyy HH:mm"), "';"));
            sbJs.AppendLine(string.Concat("var userDateEnd = '", dtUserDate.ToString("dd/MM/yyyy"), "';"));
            sbJs.AppendLine(string.Concat("var nHideHourField = '", nHideHourField, "';"));
            sbJs.AppendLine(string.Concat("var nHideNoDate = '", nHideNoDate, "';"));
            sbJs.AppendLine(string.Concat("var strHour = '", strHour, "';"));
            sbJs.AppendLine(string.Concat("var strMin = '", strMin, "';"));

            jsStringVars = sbJs.ToString();
        }

    }


}