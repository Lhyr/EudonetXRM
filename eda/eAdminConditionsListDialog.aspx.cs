using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using System;
using System.Text;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Administration - Gestion de la modal des traitement conditionnels
    /// </summary>
    public partial class eAdminConditionsListDialog : eAdminPage
    {

        /// <summary>
        /// Table/Champ à administrer
        /// </summary>
        protected int _nDescId;

        /// <summary>
        /// type à filter
        /// </summary>
        protected int _nType;


        /// <summary>
        /// Table parente (pour admin signet)
        /// </summary>
        protected int _nParentTab;


        //Id Modal
        private string _sIdModal;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {


                PageRegisters.RegisterFromRoot = true;
                #region Ajout des css

                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eButtons");
                PageRegisters.AddCss("eControl");
                PageRegisters.AddCss("eAdmin");
                PageRegisters.AddCss("eAdminFile");
                PageRegisters.AddCss("eAdminMenu");

                #endregion

                #region Ajout des js
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eMain");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("eModalDialog");


                PageRegisters.RegisterAdminIncludeScript("eAdminConditions");


                #endregion

                #region récup param

                _nParentTab = _requestTools.GetRequestFormKeyI("parenttab") ?? 0;
                _nDescId = _requestTools.GetRequestFormKeyI("descid") ?? 0;

                _nType = _requestTools.GetRequestFormKeyI("typefilter") ?? 0;



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


                StringBuilder sb = new StringBuilder();
                sb.Append("var myConditionModal = top.eTools.GetModalFromId('").Append(_sIdModal).Append("');");




                #endregion


                #region génération du renderer

                eRules.ConditionsFiltersConcerning type = eLibTools.GetEnumFromCode<eRules.ConditionsFiltersConcerning>(_nType);
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminConditionsListDialogRenderer(_pref, _nDescId, _nParentTab, _sIdModal, type);




                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                    throw renderer.InnerException ?? new Exception(renderer.ErrorMsg);

                //Ajout du callback script
                PageRegisters.RawScrip.AppendLine(sb.ToString());
                PageRegisters.RawScrip.AppendLine(renderer.GetCallBackScript);

                formAdminListConditions.Controls.Add(renderer.PgContainer);


                #endregion

            }
            #region gestion erreur
            catch (eEndResponseException)
            {
                //erreur gérée
            }
            catch (Exception exe)
            {
                //erreur non gérée
                ErrorContainer = eErrorContainer.GetDevUserError(
                               eLibConst.MSG_TYPE.CRITICAL,
                               eResApp.GetRes(_pref, 72),
                               eResApp.GetRes(_pref, 6236),
                               eResApp.GetRes(_pref, 72),
                              String.Concat("Erreur de génération de eAdminConditionsDialog :", exe.Message)
                             );

                try
                {
                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }
                catch (eEndResponseException) { }
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