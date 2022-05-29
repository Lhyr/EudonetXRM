using System;
using System.Text;
using System.Web.UI;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de gestion du calendrier
    /// </summary>
    public partial class eCalendar : eEudoPage
    {

        /// <summary>variables js</summary>
        public string jsStringVars;

        /// <summary>Nom javascript de la modale</summary>
        public String _modalVarName = string.Empty;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// page d'affichage du calendrier
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eCalendar");

            #endregion


            //add scripts
            PageRegisters.AddScript("eCalendar");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eTools");

            DateTime dtUserDate = DateTime.Now;
            dtUserDate = eLibTools.ClearHour(dtUserDate);
            DateTime dtDate = dtUserDate;
            int nHideHourField = 0;
            int nHideNoDate = 0;
            string calOperator = "0";
            string nodeId = "";
            string strJsOkButtonAction = string.Empty;
            string strParentJsValidFunction = string.Empty;
            string strHour = string.Empty;
            string strMin = string.Empty;


            if (Request.Form["date"] != null)
            {
                String strDate = Request.Form["date"].ToString();

                if (!DateTime.TryParse(strDate, out dtDate))
                    dtDate = dtUserDate;
            }

            if (Request.Form["HideHourField"] != null)
            {
                Int32.TryParse(Request.Form["HideHourField"].ToString(), out nHideHourField);
            }

            if (Request.Form["HideNoDate"] != null)
            {
                Int32.TryParse(Request.Form["HideNoDate"].ToString(), out nHideNoDate);
            }

            if (Request.Form["JsValidAction"] != null)
            {
                strParentJsValidFunction = Request.Form["JsValidAction"].ToString().Replace("'", "\\'");
            }
            if (Request.Form["JsOkButtonAction"] != null)
            {
                strJsOkButtonAction = Request.Form["JsOkButtonAction"].ToString().Replace("'", "\\'");
            }

            if (Request.Form["operator"] != null)
            {
                calOperator = Request.Form["operator"].ToString().Replace("'", "\\'");
            }

            if (Request.Form["nodeId"] != null)
            {
                nodeId = Request.Form["nodeId"].ToString().Replace("'", "\\'");
            }

            if (Request.Form["modalVarName"] != null)
            {
                _modalVarName = Request.Form["modalVarName"].ToString();
            }

            if (nHideHourField != 1)
            {
                int nHour = dtDate.Hour;
                int nMin = dtDate.Minute;
                if (nHour + nMin != 0)
                {
                    if (nHour < 10)
                        strHour = string.Concat("0", nHour);
                    else
                        strHour = nHour.ToString();

                    if (nMin < 10)
                        strMin = "0" + nMin;
                    else
                        strMin = nMin.ToString();

                }
            }

            StringBuilder sbJs = new StringBuilder();
            if (!string.IsNullOrEmpty(strJsOkButtonAction))
            {

                if (Request.Form["parentframeid"] != null && !String.IsNullOrEmpty(Request.Form["parentframeid"].ToString()))
                {
                    String parentframeid = String.Empty;
                    parentframeid = Request.Form["parentframeid"].ToString();
                    sbJs.AppendLine(string.Concat("var parentFrame = top.document.getElementById('", parentframeid, "').contentWindow ;"));

                    // ASY du a la refacto de eCallendar.js : ajout de eCalendarControl  et pas de parametres a Valid
                    sbJs.AppendLine(string.Concat("parentFrame.", strJsOkButtonAction, "= function(){eCalendarControl.Valid();};"));

                }
                else
                {
                    sbJs.AppendLine(string.Concat("var parentFrame = top;"));
                    sbJs.AppendLine(string.Concat("parentFrame.", strJsOkButtonAction, "= function(){eCalendarControl.Valid();};"));

                }

            }

            if (Request.Form["frmId"] != null && !String.IsNullOrEmpty(Request.Form["frmId"].ToString()))
            {
                sbJs.AppendLine(string.Concat("var frmId = '", Request.Form["frmId"], "';"));

                sbJs.AppendLine(string.Concat("var parentFrame = top.document.getElementById(frmId).contentWindow; "));
                // ASY du a la refacto de eCallendar.js : ajout de eCalendarControl
                sbJs.AppendLine(string.Concat("parentFrame.", strJsOkButtonAction, "= function(){eCalendarControl.Valid();};"));


            }

            sbJs.AppendLine(string.Concat("var strParentJsValidFunction = '", strParentJsValidFunction, "';"));
            sbJs.AppendLine(string.Concat("var nodeId = '", nodeId, "';"));
            sbJs.AppendLine(string.Concat("var operator = '", calOperator, "';"));
            sbJs.AppendLine(string.Concat("var strDate = '", dtDate.ToString("dd/MM/yyyy HH:mm"), "';"));
            sbJs.AppendLine(string.Concat("var userDate = '", dtUserDate.ToString("dd/MM/yyyy"), "';"));
            sbJs.AppendLine(string.Concat("var nHideHourField = '", nHideHourField, "';"));
            sbJs.AppendLine(string.Concat("var nHideNoDate = '", nHideNoDate, "';"));
            sbJs.AppendLine(string.Concat("var strHour = '", strHour, "';"));
            sbJs.AppendLine(string.Concat("var strMin = '", strMin, "';"));

            jsStringVars = sbJs.ToString();
        }

    }
}