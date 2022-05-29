using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    ///  gestion des mots de passe oubliés
    /// </summary>
    public partial class eForgotPassword : System.Web.UI.Page
    {
        private HashSet<String> _allKeys;
        private HashSet<String> _allKeysQS;

        public int iLang;

        public String _onLoadBody = String.Empty;

        /// <summary>
        ///  gestion des mots de passe oubliés
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            iLang = 0;
            String sLang;
            //Int32 iLang = 0;
            String sUserLogin = string.Empty;

            SubscriberToken cSubscriberToken = new SubscriberToken();
            String sSubscriberToken = String.Empty;


            String dbToken = String.Empty;
            DbToken cDbToken = new DbToken();

            _allKeys = new HashSet<String>(Request.Form.AllKeys, StringComparer.OrdinalIgnoreCase);
            _allKeysQS = new HashSet<String>(Request.QueryString.AllKeys, StringComparer.OrdinalIgnoreCase);


            String sAction = String.Empty;
            if (_allKeys.Contains("action") && !Request.Form["action"].ToString().Trim().Equals(string.Empty))
                sAction = Request.Form["action"].ToString();
            else if (_allKeysQS.Contains("action") && !Request.QueryString["action"].ToString().Trim().Equals(string.Empty))
                sAction = Request.QueryString["action"].ToString();



            if (sAction.Length == 0)
            {
                WriteResponse("Action non reconnue.");
                return;
            }

            //Récupération de la langue
            sLang = String.Empty;
            if (_allKeys.Contains("lang"))
                sLang = Request.Form["lang"].ToString();
            eLibTools.GetLangFromUserPref(sLang, out sLang, out iLang);

            //Connexion à la base
            if (Request.Form["dbt"] != null)
                dbToken = Request.Form["dbt"].ToString();
            else if (Request.QueryString["t1"] != null)  //Appel depuis email - Token dans t1-t2
                dbToken = Request.QueryString["t1"].ToString();

            cDbToken = new DbToken();
            if (!cDbToken.LoadTokenCrypted(dbToken))
            {
                WriteResponse(eResApp.GetRes(iLang, 516));
                return;
            }

            switch (sAction)
            {
                //Formulaire
                case "forgotpasswordform": // Affichage du formulaire après click sur le lien de la page de login
                    _onLoadBody = "onloadLib();";

                    if (Request.Form["UserLogin"] == null || Request.Form["UserLogin"].ToString().Trim().Equals(string.Empty))
                        WriteResponse(eResApp.GetRes(iLang, 516));


                    if (_allKeys.Contains("st"))
                        sSubscriberToken = Request.Form["st"];

                    cSubscriberToken = new SubscriberToken();
                    if (!cSubscriberToken.LoadTokenCrypted(sSubscriberToken))
                    {
                        WriteResponse(eResApp.GetRes(iLang, 516));
                        return;
                    }


                    sUserLogin = Request.Form["UserLogin"].ToString();



                    /* (re)place les jetons dans les Inpu t*/

                    dbt.Value = cDbToken.GetTokenCrypted();
                    lang.Value = sLang;
                    st.Value = cSubscriberToken.GetTokenCrypted();
                    userlogin.Value = sUserLogin;

                    break;


                //Retour depuis l'email envoyé
                case "validpassword":



                    if (_allKeysQS.Contains("t3"))
                    {
                        sSubscriberToken = Request.QueryString["t3"];
                        //    sSubscriberToken = Server.UrlDec ode(sSubscriberToken);
                        cSubscriberToken = new SubscriberToken();
                        if (!cSubscriberToken.LoadTokenCrypted(sSubscriberToken))
                        {
                            WriteResponse(eResApp.GetRes(iLang, 516));
                            return;
                        }
                    }
                    else
                    {
                        WriteResponse(eResApp.GetRes(iLang, 516));
                        return;
                    }

                    // Réinitialisation du password à partir du lien du mail 
                    String pwdToken = Request.QueryString["t2"].ToString();
                    eLoginOL login = eLoginOL.GetLoginObject(cSubscriberToken);
                    String sErr;

                    if (!login.ValidPassWord(cDbToken, pwdToken, out sErr))
                        WriteResponse(sErr);
                    else
                        WriteResponse(eResApp.GetRes(iLang, 6098));

                    return;

                default:
                    WriteResponse(eResApp.GetRes(iLang, 516));
                    return;

            }
        }






        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void WriteResponse(String msg)
        {

            panelForm.InnerHtml = msg;



            return;
        }

    }
}