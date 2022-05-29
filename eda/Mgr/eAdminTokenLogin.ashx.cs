using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminTokenLogin
    /// </summary>
    public class eAdminTokenLogin : eAdminManager
    {

        /// <summary>
        /// Classe de login pour se connecter "en tant que"
        /// </summary>
        protected override void ProcessManager()
        {

         
            JSONReturnToken res = new Mgr.JSONReturnToken();
            res.Success = false;


            int nUserId = _requestTools.GetRequestFormKeyI("uid") ?? 0;



            try
            {

                if (nUserId <= 0)
                    throw new EudoException("UserId Non Valide", "UserId Non Valide");

                
                eLoginOL login = eLoginOL.GetLoginObjectForProfilId(_pref, nUserId);
                eLoginOL.LogLogout(); // Log le logout admin

                // réinitialise la session
                _context.Session.Clear();

                // set la session
                login.SetSessionVars();


                res.Success = true;
            }
            catch (EudoException ee)
            {
                //Message d'erreur explicite pour le user
                res.ErrorMsg = ee.UserMessage;
                res.Success = false;

            }
            catch (Exception e)
            {

#if DEBUG
                res.ErrorMsg = e.Message;
#else
                res.ErrorMsg = "";
#endif
                res.Success = false;

                //Sendfeedback

            }

            RenderResult(RequestContentType.SCRIPT, delegate () {
                return SerializerTools.JsonSerialize(res); });
        }


    }


    public class JSONReturnToken : JSONReturnHTMLContent
    {

        public string Token = "TEST";
    }
}