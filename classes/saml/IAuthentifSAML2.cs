
/*
using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using Com.Eudonet.Internal;
using SAML2;
using SAML2.Actions;
using SAML2.Identity;
using SAML2.Protocol;
namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe d'authentification SAML - Utilisé par les webhandler lié a SAML
    /// </summary>
    public class XRMSAMLAuthentification : IAction
    {
        #region Implementation of IAction

        private string _name = "XRMSAMLAuthentification";

        /// <summary>
        /// Nom de l'action
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Action performed during login.
        /// </summary>
        /// <param name="handler">The handler initiating the call.</param>
        /// <param name="context">The current http context.</param>
        /// <param name="assertion">The saml assertion of the currently logged in user.</param>
        public void SignOnAction(AbstractEndpointHandler handler, HttpContext context, Saml20Assertion assertion)
        {

            try
            {

                //                throw new ArgumentException("SAML Response did not contain the identifier attribute.");

                // Ensure that the necessary attributes have been returned
                if (!Saml20Identity.Current.HasAttribute("email"))
                {
                    throw new ArgumentException("SAML Response did not contain the identifier attribute.");
                }



                // Ensure that there is a value passed back for identifier
                if (!Saml20Identity.Current["email"].Any(x => x.AttributeValue.Length > 0))
                {
                    throw new FormatException("SAML Response contained the identifierattribute, but did not include a value.");
                }

                try
                {
                    string identifier = Saml20Identity.Current["email"].FirstOrDefault(x => x.AttributeValue.Length > 0).AttributeValue.FirstOrDefault();

                    string sError = "";
                    eLoginOL login;
                    string sDBID = eTools.GetServerConfig("EXTERNALAUTHDEFAULTBASEID", "");

                    if (sDBID.Length == 0)
                    {
                        context.Response.Redirect("eADFSERROR.html?e=basenotfound");
                    }


                    AUTH_USER_RES resADFS = eLoginOL.GetLoginObjectForExternalLogin(sDBID, identifier, out login, out sError);
                    if (resADFS == AUTH_USER_RES.SUCCESS)
                    {
                        login.SetSessionVars();
                        //context.Response.Redirect("eADFSERROR.html?e=YEAH");
                    }
                    else
                    {
                        context.Response.Redirect("eADFSERROR.html?e=opensession");
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception e)
                {
                    context.Response.Redirect("eADFSERROR.html?e=loadsession");

                }

            }
            catch (Exception e)
            {
                context.Response.Redirect("eADFSERROR.html?e=other");
            }



        }


        /// <summary>
        /// Action performed during logout.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="context">The context.</param>
        /// <param name="IdPInitiated">During IdP initiated logout some actions such as redirecting should not be performed</param>
        public void LogoutAction(AbstractEndpointHandler handler, HttpContext context, bool IdPInitiated)
        {
            context.Session.Clear();
        }



        #endregion

    }
}
*/