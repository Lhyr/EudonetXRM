using EudoGraphTeams.Factories;
using EudoGraphTeams.Helpers;
using EudoGraphTeams.Models;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Identity.Web;
using System.Threading.Tasks;
using System.Security.Claims;
using EudoGraphTeams;
using TimeZoneConverter;
using Microsoft.Owin.Security.OpenIdConnect;
using EudoGraphTeams.Authentication;
using System.Web;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model.Teams;
using Com.Eudonet.Core.Model;
using Newtonsoft.Json;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{

    /// <summary>
    /// controller d'interaction avec le calendrier teams
    /// </summary>
    public class GraphTeamsEventController : BaseController
    {
        // GET: api/GraphEvent/5
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Met à jour une information
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> SaveEvent(eTeamsRequest request)
        {
            SessionTokenStore tokenStore = new SessionTokenStore(null, HttpContext.Current, ClaimsPrincipal.Current);

            if (!tokenStore.HasUser())
                return Ok(JsonConvert.SerializeObject(new { IsAuthenticated = false }));


            if (!(request.FileId > 0))
                throw new EudoFileNotFoundException("The FileId is 0, the file doesn't seem to have been created in database yet. Please press 'Save' before trying to create teams meeting ");

            #region informations de connection azure
            CachedUser user = tokenStore.GetUserDetails();
            GraphServiceClient graphServiceClient = GraphHelper.GetAuthenticatedClient();
            #endregion


            #region récupération des valeurs en base
            eTeamsData teamsData = eTeamsData.GetTeamsData(_pref, request.Tab, request.FileId);
            if (!(teamsData?.IsFileOk ?? false))
                throw new EudoFileNotFoundException("The file to be set as a date in teams cannot be found");
            #endregion


            //récapitulatif des informations simplifiées de l'évènement
            GraphTeamsEvent evt = new GraphTeamsEvent()
            {
                Start = teamsData.GetStart,
                End = teamsData.GetEnd,
                Attendees = teamsData.GetAttendees,
                Subject = teamsData.GetSubject,
                Body = teamsData.GetBody,
                Id = teamsData.GetMsEventId,
            };

            //Transformation Microsoft.graph.Event qui comprend les informations exhaustives nécessaire à la création du rdv
            Event msGraphEvent = EventFactory.GetGraphEvent(evt, user.TimeZone ?? TimeZoneInfo.Local.Id);
            Event msReturnedGraphEvent;
            try
            {
                //Appel de l'api pour créer le rendez vous dans teams
                //récupération des informations enregistrées en sus.
                msReturnedGraphEvent = await EventFactory.SaveEvent(msGraphEvent, graphServiceClient);
            }
            catch (ServiceException ex)
            {
                //todo : gérer Code: ErrorDuplicateTransactionId Message: Your request can't be completed. The TransactionId specified in the request has already been used to create a different event.

                if (eLibTools.IsLocalOrEudoMachine(HttpContext.Current))
                    throw;
                else
                    throw new EudoException("An Error has occured during Teams meeting recording", innerExcp: ex);
            }


            #region enregistrement des valeurs de références en base

            Engine.Engine eng = eModelTools.GetEngine(_pref, request.Tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = request.FileId;


            //si pas de mapping ça aura planté avant
            if (teamsData.GetMapping().TeamsEventIdDescid > 0 && !String.IsNullOrEmpty(msReturnedGraphEvent.Id))
                eng.AddNewValue(teamsData.GetMapping().TeamsEventIdDescid, msReturnedGraphEvent.Id, middleFormulaOkForceUpdate: true, readOnly: false, ignoreTestRights: true);

            if (teamsData.GetMapping().URLMeetingDescid > 0 && !String.IsNullOrEmpty(msReturnedGraphEvent.OnlineMeeting?.JoinUrl))
                eng.AddNewValue(teamsData.GetMapping().URLMeetingDescid, msReturnedGraphEvent.OnlineMeeting?.JoinUrl ?? "", middleFormulaOkForceUpdate: true, readOnly: false, ignoreTestRights: true);

            eng.AddParam("fieldTrigger", request.FieldTrigger ?? "");

            eng.EngineProcess(new StrategyCruXrm(XrmCruAction.UPDATE), new ResultXrmCru());

            IEngineReturnModel engResultModel = EngineReturnFactory.initEngineReturnFactory(eng.Result).GetEngineResult();


            #endregion

            var oreturn = new {
                IsAuthenticated = true,
                Result = engResultModel,
                DataExceptions = teamsData.Exceptions
            };


            return Ok(JsonConvert.SerializeObject(oreturn));
        }

        // DELETE: api/GraphEvent/5
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}
