using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminRelationsSQLFiltersDialog : eAdminPage
    {
        protected int _nTab;
        protected String _sInterEventNum;

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdminMenu");
            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            //PageRegisters.AddScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript("eAdminRelations");
            #endregion

            #region Paramètres
            _nTab = 0;
            _sInterEventNum = String.Empty;

            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            if (allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                _nTab = eLibTools.GetNum(Request.Form["tab"].ToString());
            if (allKeys.Contains("interEventNum") && !String.IsNullOrEmpty(Request.Form["interEventNum"]))
                _sInterEventNum = Request.Form["interEventNum"].ToString();

            #endregion

            String error = String.Empty;
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

            try
            {
                eDal.OpenDatabase();

                eAdminTableInfos tabInfos = new eAdminTableInfos(_pref, _nTab);
                int nParentTab = TableLite.CalculInterEvtNumToDid(_sInterEventNum);

                if (!String.IsNullOrEmpty(error))
                    throw new Exception("eAdminTableInfos.ExternalLoadInfo => " + error);

                eBkmPref bkmPref = new eBkmPref(_pref, nParentTab, _nTab);
                String sValue = bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.ADDEDBKMWHERE);

                eAdminRelationsSQLFiltersDialogRenderer renderer = eAdminRendererFactory.CreateAdminRelationsSQLFiltersDialogRenderer(_pref, tabInfos, nParentTab, sValue);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }


                formAdminRelationsSQLFilters.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminRelationsSQLFiltersDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }
    }
}