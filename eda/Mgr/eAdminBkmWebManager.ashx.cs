using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de Handler1
    /// </summary>
    public class eAdminBkmWebManager : eAdminManager
    {

        /// <summary>
        /// type d'action pour le manager
        /// </summary>
        public enum BkmWebAction
        {
            /// <summary>action indéfini </summary>
            UNDEFINED = 0,
            /// <summary>Rendu html de la table d'édition des propriétés d'un sigent web </summary>
            GETINFOS = 1,

            /// <summary>Utilisation d'une Specif dans le signet Web </summary>
            TOSPECIF = 2,

            /// <summary>Utilisation d'une URL Externe </summary>
            TOEXTERN = 3,

            /// <summary>
            /// Supprime
            /// </summary>
            DELETE = 4
        }


        private BkmWebAction action = BkmWebAction.UNDEFINED;


        /// <summary>
        /// Gestion de l'action a effectuer
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 iParentTab = 0, iBkm = 0;

            //Action - paramètre obligatoire
            if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
            {
                if (!Enum.TryParse(_context.Request.Form["action"], out action))
                    action = BkmWebAction.UNDEFINED;
            }

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"], out iParentTab);

            if (_requestTools.AllKeys.Contains("bkm") && !String.IsNullOrEmpty(_context.Request.Form["bkm"]))
                Int32.TryParse(_context.Request.Form["bkm"], out iBkm);

            //tab et spécif obligatoire
            if (iParentTab == 0 || iBkm == 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Paramètres non renseignés.");
                LaunchError();
            }

            String sError = "";

            switch (action)
            {
                case BkmWebAction.UNDEFINED:
                    throw new EudoException("Aucune action définie.");


                case BkmWebAction.GETINFOS:
                    //création du rendu
                    eAdminRenderer rdr = eAdminRendererFactory.CreateAdminBkmWebParameterRenderer(_pref, iParentTab, iBkm);
                    RenderResultHTML(rdr.PgContainer);

                    break;
                case BkmWebAction.TOSPECIF:

                    Int32 iSpecId = eSqlSpecif.CreateSpecifFromBkmWeb(_pref, _pref.User, iBkm, out sError);

                    if (sError.Length > 0)
                    {
                        throw new EudoException(sError);
                    }

                    eAdminDesc ad = new eAdminDesc(iBkm);
                    ad.SetDesc(eLibConst.DESC.DEFAULT, iSpecId.ToString());
                    ad.Save(_pref, out sError);


                    break;
                case BkmWebAction.TOEXTERN:
                    break;
                case BkmWebAction.DELETE:

                    EdnType ednType = EdnType.FILE_GRID;
                    eAdminTableInfos tb = new eAdminTableInfos(_pref, iBkm);


                    if (tb.EdnType == EdnType.FILE_GRID)
                    {
                        var result = eAdminCreateTable.DropGridTab(_pref, iBkm);
                    }
                    else if (tb.EdnType == EdnType.FILE_BKMWEB)
                    {
                        eAdminCreateTableBkmWeb createTable = eAdminCreateTableBkmWeb.GetCreateTable(_pref, iBkm);

                        var result = createTable.Drop();
                    }

                    break;
                default:
                    break;
            }

        }
    }
}