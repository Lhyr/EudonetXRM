using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminNewTabDialogManager
    /// </summary>
    public class eAdminNewTabDialogManager : eAdminManager
    {
        /// <summary>
        /// action a effectuer
        /// </summary>
        public enum Action
        {
            /// <summary>Créaton d'un nouveau onglet</summary>
            NEW = 0,
            /// <summary>Suppression signet</summary>
            DELETE = 1,

            /// <summary>Suppression signet web</summary>
            DELETEWEBTAB = 2
        }
        protected override void ProcessManager()
        {
            String sNewTabName = "";
            EdnType ednNewTabType = EdnType.FILE_MAIN;
            String sError;
            eAdminFieldCreateResult result = new eAdminFieldCreateResult();

            Action _action = Action.NEW;
            if (_requestTools.AllKeys.Contains("action"))
            {
                _action = _requestTools.GetRequestFormEnum<Action>("action");
            }

            if (_action == Action.NEW)
            {
                try
                {
                    ednNewTabType = _requestTools.GetRequestFormEnum<EdnType>("newTabType");
                }
                catch (Exception ex)
                {
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), "Veuillez réitérer l'opération.", devMsg: String.Concat("Erreur de conversion NewTabType : ", _context.Request.Form["newTabType"], " - ", ex.ToString())));
                }

                if (_context.Request.Form["newTabName"] != null && _context.Request.Form["newTabName"].ToString() != String.Empty)
                    sNewTabName = _context.Request.Form["newTabName"].ToString();


                #region old debut de code par mab
                //eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                //r.TabId = eSqlDesc.CreateTab(_pref, dal, sNewTabName, ednNewTabType, out sError);

                //if (sError.Length > 0)
                //{
                //    r.Success = false;
                //    r.Error = sError;
                //}
                //else
                //{
                //    r.Success = true;
                //}
                #endregion

                if (ednNewTabType == EdnType.FILE_GRID)
                {
                    //eLibConst.SPECIF_TYPE specType = eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL;
                    //if (_context.Request.Form["specType"] != null)
                    //    specType = _requestTools.GetRequestFormEnum<eLibConst.SPECIF_TYPE>("specType");

                    result = eAdminCreateTable.CreateGridTable(_pref, sNewTabName, eModelTools.GetRootPhysicalDatasPath(), _pref.ModeDebug);
                    if (!result.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                result.UserErrorMessage,
                                "",
                                result.DebugErrorMessage
                            )
                        );


                    }

                }
                else {
                    eAdminCreateTable createTable = eAdminCreateTable.GetCreateTable(_pref, sNewTabName, ednNewTabType);
                    result = createTable.Create();
                    if (!result.Success)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                result.UserErrorMessage,
                                "",
                                result.DebugErrorMessage
                            )
                        );


                    }
                }
            }
            else if (_action == Action.DELETE)
            {
                Int32 nDescId = 0;
                TableType tabType = TableType.EVENT;
                EdnType ednType = _requestTools.GetRequestFormEnum<EdnType>("edntype");
                if (!(_requestTools.AllKeys.Contains("tab") && Int32.TryParse(_context.Request.Form["tab"], out nDescId)
                    && _requestTools.AllKeys.Contains("tabtype") && Enum.TryParse<TableType>(_context.Request.Form["tabtype"], out tabType)
                    && _requestTools.AllKeys.Contains("edntype")))
                {
                    result.Success = false;
                    result.UserErrorMessage = "Des informations manquent pour la suppression de la table.";
                    result.DebugErrorMessage = "le paramètre tab ne semble pas avoir été fournis";
                    LaunchError(
                        eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            result.UserErrorMessage,
                            "",
                            result.DebugErrorMessage
                        )
                    );
                }


                if (ednType == EdnType.FILE_GRID)
                {
                    result = eAdminCreateTable.DropGridTab(_pref, nDescId);
                }
                else if(ednType == EdnType.FILE_BKMWEB)
                {
                    eAdminCreateTableBkmWeb createTable = eAdminCreateTableBkmWeb.GetCreateTable(_pref, nDescId);

                    result = createTable.Drop();
                }
                else {
                    eAdminCreateTable createTable = eAdminCreateTable.GetExistingTable(_pref, nDescId, tabType, out sError);
                    if (sError.Length > 0)
                    {
                        result.Success = false;
                        result.UserErrorMessage = eResApp.GetRes(_pref, 7253);
                        result.DebugErrorMessage = sError;

                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                result.UserErrorMessage,
                                "",
                                result.DebugErrorMessage
                            )
                        );
                    }

                    result = createTable.Drop();
                }

                if (!result.Success)
                {
                    eLibConst.MSG_TYPE mtype = (eLibConst.MSG_TYPE)result.Criticity;
                    if (result.DebugErrorMessage.Length > 0)
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                mtype,
                                eResApp.GetRes(_pref, mtype == eLibConst.MSG_TYPE.CRITICAL ? 72 : 69),
                                result.UserErrorMessage,
                                title: " ",
                                devMsg: result.DebugErrorMessage
                            )
                        );
                    else
                        LaunchError(
                            eErrorContainer.GetUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                result.UserErrorMessage,
                                ""
                            )
                        );

                }

            }

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(result);
            });

        }

        public class Result
        {
            public Boolean Success;
            public Int32 TabId;
            public String Error;
        }
    }
}