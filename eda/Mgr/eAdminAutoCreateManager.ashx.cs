using System;
using System.Collections.Generic;
using EudoQuery;
using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Génère une fiche à partir du masque de nouvelle fiche fourni par eudoquery.
    /// </summary>
    public class eAdminAutoCreateManager : eEngineMgr
    {
        /// <summary>
        /// contenu du traitement de création automatique de fiche.
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                if (_pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                    throw new EudoAdminInvalidRightException();

                //Table
                int? nTab = _requestTools.GetRequestFormKeyI("tab");

                //Retour d'erreur si tab n'est pas renseigné
                if (nTab == null)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 6237),
                        eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                        eResApp.GetRes(_pref, 72).Replace(" <PARAM> ", " "),
                        "La table dans laquelle le nouvel enregistrement doit être créé n'a pas été précisée."
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }

                Engine.Engine eng = eModelTools.GetEngine(_pref, nTab.Value, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.EngineProcess(new StrategyCruSimple());
                _engResult = eng.Result;

                // on crée
                if (nTab.Value == (int)TableType.XRMHOMEPAGE)
                {
                    Engine.Engine engGrid = eModelTools.GetEngine(_pref, (int)TableType.XRMGRID, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                    eng.EngineProcess(new StrategyCruSimple());
                }
            }
            catch (eEndResponseException)
            {


            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 2024).Replace("<PARAM>", ""), string.Concat("Autocreate :", ex.Message), ex.ToString());
                LaunchError();
            }

            DoResponse();
        }
    }
}