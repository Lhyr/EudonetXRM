using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{




    /// <summary>
    /// Description résumée de eAdminConditionListManager
    /// </summary>
    public class eAdminConditionListManager : eAdminManager
    {

        private int _nParentTab;
        private int _nDescId;
        private int _nType;
        private int _nAction;
        private string _sIdModal;


        protected override void ProcessManager()
        {


            _nParentTab = _requestTools.GetRequestFormKeyI("parenttab") ?? 0;
            _nDescId = _requestTools.GetRequestFormKeyI("descid") ?? 0;
            _nType = _requestTools.GetRequestFormKeyI("typefilter") ?? 0;

            _nAction = _requestTools.GetRequestFormKeyI("action") ?? 0;

            if (_nDescId <= 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                      eLibConst.MSG_TYPE.CRITICAL,
                      eResApp.GetRes(_pref, 72),
                      eResApp.GetRes(_pref, 6236),
                      eResApp.GetRes(_pref, 72),
                     String.Concat("Paramètres 'descid' invalide ou absent")
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            _sIdModal = _requestTools.GetRequestFormKeyS("_parentiframeid");


            if (_sIdModal.Length <= 0 || !_sIdModal.StartsWith("frm_"))
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                      eLibConst.MSG_TYPE.CRITICAL,
                      eResApp.GetRes(_pref, 72),
                      eResApp.GetRes(_pref, 6236),
                      eResApp.GetRes(_pref, 72),
                     String.Concat("Paramètres '_parentiframeid' invalide ou absent")
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            _sIdModal = _sIdModal.Substring("frm_".Length);


            eRules.ConditionsFiltersConcerning type = eLibTools.GetEnumFromCode<eRules.ConditionsFiltersConcerning>(_nType);
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminConditionsListDialogRenderer(_pref, _nDescId, _nParentTab, _sIdModal, type, true);


            JSONReturnHTMLContent res = new Xrm.JSONReturnHTMLContent();

            if (renderer.ErrorMsg.Length > 0)
            {
                res.Success = false;
                res.ErrorMsg = renderer.ErrorMsg;

            }
            else
            {

                res.Success = true;
                res.Html = GetResultHTML(renderer.PgContainer, true);

                res.CallBack = "";
            }
            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });




        }



    }
}