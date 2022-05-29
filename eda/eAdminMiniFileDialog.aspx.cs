using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Threading;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Modal dialog de l'admin des miniche
    /// </summary>
    public partial class eAdminMiniFileDialog : eAdminPage
    {
        protected int _nTab;
        private MiniFileType _miniFileType = MiniFileType.File;

        protected MiniFileType MiniFileType
        {
            get
            {
                return _miniFileType;
            }

            set
            {
                _miniFileType = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PageRegisters.RegisterFromRoot = true;
            #region Ajout des css
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eKanban");
            #endregion

            #region Ajout des js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.RegisterAdminIncludeScript("eAdminMiniFile");
            #endregion

            #region Paramètres
            _nTab = 0;

            try
            {
                _miniFileType = MiniFileType.File;

                _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                int type = _requestTools.GetRequestFormKeyI("type") ?? 0;
                int widgetId = _requestTools.GetRequestFormKeyI("wid") ?? 0;

                if (Enum.IsDefined(typeof(MiniFileType), type))
                    _miniFileType = (MiniFileType)type;

                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminMiniFileDialogRenderer(_pref, _nTab, _miniFileType, widgetId);
                if (!String.IsNullOrEmpty(renderer.ErrorMsg))
                {
                    if (renderer.InnerException != null)
                        throw new Exception(renderer.ErrorMsg, renderer.InnerException);
                    else
                        throw new Exception(renderer.ErrorMsg);
                }

                formAdminMiniFile.Controls.Add(renderer.PgContainer);
            }
            catch (eEndResponseException)
            {
            }
            catch (ThreadAbortException)
            { }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminMiniFileDialog : ", exc.Message, Environment.NewLine, exc.StackTrace)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {

            }

            #endregion
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