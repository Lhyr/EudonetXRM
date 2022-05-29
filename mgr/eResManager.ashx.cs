using Com.Eudonet.Internal;
using System;

using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Construction d'un bloc javascript définissant des variables des ressources de l'application
    ///  sous la forme '_res_#resid# = '"resource"';
    /// </summary>
    public class eResManager : eEudoManager
    {
        /// <summary>
        /// resmanager peut être utilisé sans pref et sans session
        /// </summary>
        protected override void LoadSession()
        {
            //
            _requestTools = new eRequestTools(_context);
        }

        /// <summary>
        /// Generation du rendu JS des Ressources
        /// </summary>
        protected override void ProcessManager()
        {
            HttpContext context = HttpContext.Current;
            string langue = "0";

            int nLangId;
            if (!_requestTools.AllKeysQS.Contains("l") || !int.TryParse(context.Request.QueryString["l"], out nLangId))
            {

                langue = eTools.GetCookie("langue", context.Request);
                if (langue.Length == 0)
                    langue = "LANG_00";

                Regex myLANG = new Regex(eLibConst.LANG_FORMAT);
                if (!myLANG.IsMatch(langue))
                    langue = "LANG_00";

                langue = langue.ToUpper();


                if (!int.TryParse(langue.Replace("LANG_", ""), out nLangId))
                    nLangId = 0;
            }






            string sReturnValue = string.Empty;
            string error = string.Empty;
            try
            {
                StringBuilder sb = new StringBuilder();

                // défini désormais sur eMain.aspx en fonction du niveau de l'utilisateur connecté
                //sb.Append("top._newsLetterNum =   '';").AppendLine("");
                //sb.Append("top._newsLetterUrl =  '").Append(eConst.NEWSLETTER_USR_URL).Append("';").AppendLine("");

                sb.Append("_userLangId = ").Append(nLangId.ToString()).Append(";").AppendLine("");

                foreach (int nResId in eResApp.GetListResIds())
                {
                    sb.Append("_res_").Append(nResId).Append(" = '").Append(eResApp.GetRes(nLangId, nResId).Replace("'", @"\'")).AppendLine("';");
                }

                sReturnValue = sb.ToString();


            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
            finally
            {


            }

            if (!string.IsNullOrEmpty(error))
            {
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, string.Concat("Erreur non gérée : ", Environment.NewLine, error)), _pref);
                RenderResult(RequestContentType.TEXT, delegate() { return eResApp.GetRes(_pref, 72); });
            }
            else
                RenderResult(RequestContentType.SCRIPT, delegate() { return sReturnValue; });
        }


    }

}