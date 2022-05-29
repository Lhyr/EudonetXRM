using System;
using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Engine.Result;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager d'action administrateur
    /// </summary>
    public class eAdminAccessPrefManager : eAdminManager
    {


        /// <summary>
        /// Gestion de la demande 
        /// </summary>
        protected override void ProcessManager()
        {
            string error = String.Empty;

            //Initialisation
            int nWidth = _requestTools.GetRequestFormKeyI("width") ?? 800;
            int nHeight = _requestTools.GetRequestFormKeyI("height") ?? 600;


            string strAction = _requestTools.GetRequestFormKeyS("action");

            JSONReturnAccessPref res = new JSONReturnAccessPref();
            res.Success = false;
            bool success = false;
            switch (strAction)
            {
                case "leave":
                    _pref.AdminMode = false;
                    res.Success = true;
                    break;
                case "connect":
                    if (checkXSRF())
                    {
                        if (_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                        {
                            _pref.AdminMode = true;
                            res.Success = true;
                        }
                        else
                            throw new EudoAdminInvalidRightException();
                    }
                    break;

                case "reset":
                    if (!_pref.AdminMode)
                    {
                        res.ErrorMsg = eResApp.GetRes(_pref, 8207);
                        break;
                    }

                    success = eAdminAccessPref.ResetAllUsersPref(_pref, out error);
                    if (!success)
                    {
                        res.ErrorMsg = eResApp.GetRes(_pref, 7715);
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 7715), devMsg: error);
                        LaunchError();
                    }
                    else
                    {
                        res.Success = true;
                    }
                    break;
                case "copy":


                    int nUserId = _requestTools.GetRequestFormKeyI("src") ?? 0;
                    List<string> lstDesc = _requestTools.GetRequestFormKeyS("dst").Split(';').Where(a => { return eLibTools.IsInt(a) || eLibTools.IsGroupFormat(a); }).ToList();

                    if (nUserId == 0 || lstDesc.Count == 0)
                    {
                        res.ErrorMsg = eResApp.GetRes(_pref, 7715);
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237),
                            eResApp.GetRes(_pref, 7715), devMsg: error);
                        LaunchError();
                    }

                    try
                    {
                        eAdminAccessPref.CopyUserAllPrefs(_pref, nUserId, lstDesc);
                        res.Success = true;

                        //Met à jour les préférences de profile utilisateurs
                        //Update user's profile preferences
                        eAdminAccessPref.UpdateUserProfilePrefs(_pref, nUserId, lstDesc);

                    }
                    catch (eEndResponseException) { }
                    catch (EudoException ee)
                    {
                        // erreur catché - affichage du message EUDO
                        res.Success = false;
                        res.ErrorMsg = ee.UserMessage;
                        res.ErrorTitle = eResApp.GetRes(_pref, 92);

                        if (ee.LaunchFeedBack)
                        {
                            eFeedbackXrm.LaunchFeedbackXrm(new eErrorContainer()
                            {
                                AppendDebug = String.Concat(" erreur lors de la recopie des préférences "),
                                AppendDetail = ee.InnerException.Message
                            }, _pref);
                        }
                    }
                    catch (Exception e)
                    {
                        res.ErrorMsg = eResApp.GetRes(_pref, 7715);
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 7715), devMsg: error);

                        LaunchError();

                    }


                    break;
            }

            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
        }


        /// <summary>
        /// Vérifie si la pref user utilisé est de niveau admin et que est en mode admin
        /// </summary>
        protected override void CheckAdminRight()
        {

            // cette page doit permettre de passer en mode admin.
            // on ne vérifie donc pas le pref.adminmode
            if (_pref == null || !_pref.IsAdminEnabled || _pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
            {


                ErrorContainer = eErrorContainer.GetDevUserError(
               eLibConst.MSG_TYPE.CRITICAL,
               eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
               String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
               eResApp.GetRes(_pref, 72),  //   titre
               String.Concat("Droits insuffisants."));

                LaunchError();
            }




        }


        /// <summary>
        /// Vérification anti xsrf
        /// </summary>
        /// <returns></returns>
        private bool checkXSRF()
        {

            //Id du process - Check Anti XSRF - 
            // L'id est par session, il faudrait améliorer cela
            string sIdProcess = _context?.Request?.Headers?["X-EUPDATER-SID"] ?? "";
            string sBase = _context.Session["_uidupdater"]?.ToString() ?? "";
            if (sBase != sIdProcess)
            {
                //TODO : avant de thrower une erreur, s'assuer que le _uiupdater est tjs fourni
                return false;
            }

            return true;

        }

    }

    public class JSONReturnAccessPref : JSONReturnGeneric
    {

    }
}