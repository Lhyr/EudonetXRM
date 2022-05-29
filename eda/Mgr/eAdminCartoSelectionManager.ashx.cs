using System;
using System.Collections.Generic;
using EudoQuery;
using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Génère une réponse JSON aux differents type de requetes
    /// </summary>
    public class eAdminCartoSelectionManager : eAdminManager
    {
        Func<bool, Func<string>, Func<string>, Func<string>> when = (b, ok, nok) => b ? ok : nok;
        Func<string> result = () => SerializerTools.JsonSerialize(new JSONReturnGeneric() { Success = true });

        /// <summary>
        /// Paramétrage de l'admin widget
        /// </summary>
        protected override void ProcessManager()
        {

            int wid = _requestTools.GetRequestFormKeyI("wid") ?? 0;
            var AdminCartoSelection = eAdminCartoSelection.Create(_pref, wid);

            string action = _requestTools.GetRequestFormKeyS("action");
            switch (action)
            {
                case "load-selection":
                    int selectionTab = _requestTools.GetRequestFormKeyI("selection-tab") ?? 0;
                    if (selectionTab > 0)
                    {
                        result = () => SerializerTools.JsonSerialize(AdminCartoSelection.LoadTabSelection(selectionTab));
                    }
                    else
                    {
                        result = () => SerializerTools.JsonSerialize(new JSONReturnGeneric()
                        {
                            Success = false,
                            ErrorTitle = eResApp.GetRes(_pref, 6524),
                            ErrorMsg = $"{eResApp.GetRes(_pref, 2190)} - ({selectionTab})"
                        });
                    }

                    break;

                case "load-source-fields":
                    int sourceTab = _requestTools.GetRequestFormKeyI("source-tab") ?? 0;
                    result = when(sourceTab > 0,
                          () => SerializerTools.JsonSerialize(AdminCartoSelection.LoadFields(sourceTab)),
                          () => SerializerTools.JsonSerialize(new JSONReturnGeneric()
                          {
                              Success = false,
                              ErrorTitle = eResApp.GetRes(_pref, 6524),
                              ErrorMsg = $"{eResApp.GetRes(_pref, 2191)} - ({sourceTab})"
                          }));
                    break;

                case "save-widget-param":
                    string data = _requestTools.GetRequestFormKeyS("data");
                    result = when(!string.IsNullOrEmpty(data),
                        () => SerializerTools.JsonSerialize(AdminCartoSelection.SaveConfig(data)),
                        () => SerializerTools.JsonSerialize(new JSONReturnGeneric()
                        {
                            Success = false,
                            ErrorTitle = eResApp.GetRes(_pref, 6524),
                            ErrorMsg = eResApp.GetRes(_pref, 2186)
                        }));
                    break;

                case "get-widget-param":
                    result = () => SerializerTools.JsonSerialize(AdminCartoSelection.GetConfig(), true);
                    break;
                case "get-widget-default-param":
                    result = () => SerializerTools.JsonSerialize(CartoSelectionConfigReturn.GetDefaultAsExample(_pref), true);
                    break;
                default:
                    break;
            }

            // Renvoi du flux JSON à l'appelant
            RenderResult(RequestContentType.SCRIPT, () => result());
        }
    }
}