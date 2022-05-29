using Com.Eudonet.Core.Model.Teams;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminTeamsMapping
    /// </summary>
    public class eAdminTeamsMapping : eAdminManager
    {

        public enum Action
        {
            Initial = 0,
            Save = 1,
            GetBookmarksFields = 2
        }

        private int _nTab;

        /// <summary>
        /// traitement du contenu de la page
        /// </summary>
        protected override void ProcessManager()
        {
            Action action = Action.Initial;
            Enum.TryParse<Action>(_requestTools.GetRequestFormKeyS("action"), out action);
            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;


            switch (action)
            {
                case Action.Initial:
                    initialDisplay();
                    return;
                    break;
                case Action.Save:
                    save();
                    return;
                    break;
                case Action.GetBookmarksFields:
                    getBookmarkEditorTools();
                    return;
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// traitement du contenu de la page
        /// </summary>
        private void initialDisplay()
        {

            #region Ajout des css
            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eFieldsSelect");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eAdminTeamsMapping");

            BodyCssClass = "bodyWithScroll";

            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eFieldsSelect");
            //PageRegisters.AddScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript("eAdminTeamsMapping");

            AddHeadAndBody = true;

            #endregion



            if (!(_nTab >= 100))
            {
                throw new Exception("Paramètre 'tab' nécessaire à la mise à jour.");
            }

            eAdminRenderer renderer = null;
            try
            {
                renderer = eAdminRendererFactory.CreateTeamsMappingEditorRenderer(_pref, _nTab);
                if (renderer.InnerException != null)
                {
                    Exception exc = renderer.InnerException;

                    ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6236),
                            eResApp.GetRes(_pref, 72),
                            String.Concat("Erreur création du renderer dans eAdminTeamsMapping : ", exc.Message, Environment.NewLine, exc.StackTrace)
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();

                }
                else if (renderer.ErrorMsg?.Length > 0)
                    throw new EudoException(renderer.ErrorMsg);

                RenderResultHTML(renderer.PgContainer);



            }
            catch (eEndResponseException e) { }
            catch (EudoException exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6236),
                    exc);

                LaunchError();
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminTeamsMapping : ", exc.Message, Environment.NewLine, exc.StackTrace)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {

            }

        }

        /// <summary>
        /// enregistrement en base du mapping
        /// </summary>
        private void save()
        {
            try
            {
                string sMapping = _requestTools.GetRequestFormKeyS("mapping");
                DescAdvObj descAdvObj = DescAdvObj.GetSingle(_nTab, DESCADV_PARAMETER.TEAMS_MAPPING, sMapping);
                eAdminDescAdv adminDescAdv = new eAdminDescAdv(_pref, new List<DescAdvObj>() { descAdvObj });
                eAdminResult result = adminDescAdv.SaveDescAdv();
                RenderResult(RequestContentType.TEXT, delegate ()
                {
                    return JsonConvert.SerializeObject(result);
                });


            }
            catch (eEndResponseException e) { }
            catch (EudoException exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6236),
                    exc);

                LaunchError();
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur de sauvegarde dans eAdminTeamsMapping : ", exc.Message, Environment.NewLine, exc.StackTrace)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {

            }
        }

        private void getBookmarkEditorTools()
        {

            eTeamsBookmarkMappingEditorTools bkmEditorTools = eTeamsBookmarkMappingEditorTools.CreateMappingEditorTools(_nTab, _pref);
            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(bkmEditorTools);
            });

        }
    }
}