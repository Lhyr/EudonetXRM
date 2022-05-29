using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Handler pour renvoyer la liste des pages d'accueil et des onglets.
    /// </summary>
    public class eAdminListeOngletsEtAccueil : eAdminManager
    {
        /// <summary>
        /// Procedure qui va faire le traitement, en récupérant les données
        /// noms et identifiants des pages d'accueil et des onglets
        /// et qui les renvoie au format JSON.
        /// </summary>
        protected override void ProcessManager()
        {

            var _deletedOrVirtualTabList = new Dictionary<int, string>();
            eudoDAL _dal = null;
            Dictionary<string, string> lstOptionsGrilles = null;

            string error = String.Empty;
            List<EdnType> fileTypes = new List<EdnType>() {
                    EdnType.FILE_ADR,
                    EdnType.FILE_MAIL,
                    EdnType.FILE_MAIN,
                    EdnType.FILE_DISCUSSION,
                    EdnType.FILE_PLANNING,
                    EdnType.FILE_SMS,
                    EdnType.FILE_STANDARD,
                    EdnType.FILE_TARGET,
                    EdnType.FILE_HISTO, /* #51211 */
                    EdnType.FILE_PJ, /*#51 628, #50 700*/                                        
                    EdnType.FILE_BKMWEB, // suprimer de la liste des signet visible. du coup, on peut plus non plus les supprimer
                    EdnType.FILE_GRID
                };

            try {
                _dal = eLibTools.GetEudoDAL(_pref);
                _dal.OpenDatabase();

                lstOptionsGrilles = eDataTools.GetFiles(_dal, _pref, fileTypes, out error, ref _deletedOrVirtualTabList, "", 0, true).
                                        Select(eRec => new { ID = $"DESC_{eRec.Key}", LABEL = eRec.Value }).
                                    Union(eListFactory.CreateMainList(_pref, (int)TableType.XRMHOMEPAGE, 0, 0, true, true).ListRecords.
                                            Select(eRec => new { ID = $"{TableType.XRMHOMEPAGE}_{eRec.MainFileid}", LABEL = eRec.MainFileLabel })).
                                            ToDictionary(eRec => eRec.ID, eRec => eRec.LABEL);
            }
            catch (Exception ex) {
                LaunchError(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 7143),  ex), RequestContentType.SCRIPT);
            }
            finally {
                _dal?.CloseDatabase();
            }

            RenderResult(RequestContentType.SCRIPT, () => SerializerTools.JsonSerialize(lstOptionsGrilles));
        }
    }
}