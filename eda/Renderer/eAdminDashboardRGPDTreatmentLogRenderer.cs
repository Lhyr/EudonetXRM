using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminDashboardRGPDTreatmentLogRenderer : eAdminModuleRenderer
    {
        private eAdminDashboardRGPDTreatmentLogRenderer(ePref pref, int w, int h) : base(pref)
        {
            _width = w;
            _height = h;
            _tab = 0;
        }

        public static eAdminDashboardRGPDTreatmentLogRenderer CreateAdminDashboardRGPDTreatmentLogRenderer(ePref pref, int w, int h)
        {
            return new eAdminDashboardRGPDTreatmentLogRenderer(pref, w, h);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminRGPD))
                    return false;

                eudoDAL eDal = eLibTools.GetEudoDAL(Pref);
                try
                {
                    eDal.OpenDatabase();

                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    eDal.CloseDatabase();
                }

                return true;
            }


            return false;
        }


        /// <summary>
        /// Lance la construction de la grille
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            Control contents = null;

            eAdminRGPDTreatmentLogListRenderer rdr = eAdminRGPDTreatmentLogListRenderer.CreateAdminRGPDTreatmentLogListRenderer(Pref, true, 1, 24, _height, _width);
            if (rdr.ErrorMsg.Length > 0 || rdr.InnerException != null)
                throw rdr.InnerException ?? new Exception(rdr.ErrorMsg);

            DicContent = rdr.DicContent;

            //Main Content
            contents = (rdr != null ? rdr.PgContainer : null);

            if (rdr == null || contents == null)
            {
                _sErrorMsg = "Renderer de module admin non implémenté";
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT; // TODO/TOCHECK
                _eException = null;
            }

            else if (rdr.ErrorMsg.Length > 0)
            {
                _sErrorMsg = rdr.ErrorMsg;
                _nErrorNumber = rdr.ErrorNumber;
                _eException = rdr.InnerException;
            }
            else
            {
                AddCallBackScript(rdr.GetCallBackScript);


                // pour un mode Liste, la liste (mainListContent) doit impérativement être ajouté au conteneur racine mainDiv
                _pgContainer.Controls.Add(contents);

            }

            return true;
        }
    }
}