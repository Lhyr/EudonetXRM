using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Modal de la liste des condition (admin)
    /// </summary>
    public partial class eAdminConditionsDialog : eAdminPage
    {

        /// <summary>
        /// Table
        /// </summary>
        protected int _nTab;


        /// <summary>
        /// Table parente (pour admin signet)
        /// </summary>
        protected int _nParentTab;

        /// <summary>
        /// type de condition
        /// </summary>
        protected TypeTraitConditionnal _tType = TypeTraitConditionnal.Undefined;

        /// <summary>
        /// id de la frame de la dialog
        /// </summary>
        private string _sIdModal = string.Empty;

        /// <summary>
        /// Process Id de la demande
        /// </summary>
        private string _sIdProcess = string.Empty;


        /// <summary>
        /// Génération de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

            try
            {


                PageRegisters.RegisterFromRoot = true;
                #region Ajout des css
                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eButtons");
                PageRegisters.AddCss("eControl");
                PageRegisters.AddCss("eAdminFile");
                PageRegisters.AddCss("eAdminMenu");
                PageRegisters.AddCss("eAdminConditions");
                #endregion

                #region Ajout des js
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eMain");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("eModalDialog");


                PageRegisters.RegisterAdminIncludeScript("eAdminConditions");


                #endregion

                #region recup param

                _tType = _requestTools.GetRequestFormEnum<TypeTraitConditionnal>("type");
                if (_tType == TypeTraitConditionnal.Undefined)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                          eLibConst.MSG_TYPE.CRITICAL,
                          eResApp.GetRes(_pref, 72),
                          eResApp.GetRes(_pref, 6236),
                          eResApp.GetRes(_pref, 72),
                         String.Concat("Paramètres 'type' invalide ou absent")
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }


                _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                if (_nTab <= 0)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                          eLibConst.MSG_TYPE.CRITICAL,
                          eResApp.GetRes(_pref, 72),
                          eResApp.GetRes(_pref, 6236),
                          eResApp.GetRes(_pref, 72),
                         String.Concat("Paramètres 'tab' invalide ou absent")
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }



                _nParentTab = _requestTools.GetRequestFormKeyI("parenttab") ?? 0;
                

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


                /*
                PageRegisters.RawScrip.Append("var oTitle = top.document.getElementById('td_title_").Append(_sIdModal).AppendLine("');");
                PageRegisters.RawScrip.Append("oTitle.innerHTML='").Append("").Append("';");
                */

                #endregion


                #region génération du renderer

                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminConditionsDialogRenderer(_pref, _nTab, _nParentTab, _sIdModal, _tType);

                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                    throw renderer.InnerException ?? new Exception(renderer.ErrorMsg);

                //Ajout du callback script
                PageRegisters.RawScrip.AppendLine(renderer.GetCallBackScript);

                formAdminAutomatisms.Controls.Add(renderer.PgContainer);

                #endregion

            }
            #region gestion erreur
            catch (eEndResponseException)
            {
                //erreur gérée
            }
            catch (Exception exe)
            {

                ErrorContainer = eErrorContainer.GetDevUserError(
                               eLibConst.MSG_TYPE.CRITICAL,
                               eResApp.GetRes(_pref, 72),
                               eResApp.GetRes(_pref, 6236),
                               eResApp.GetRes(_pref, 72),
                              String.Concat("Erreur de génération de eAdminConditionsDialog :", exe.Message)
                             );

                try
                {
                    LaunchError(); //Arrete le traitement et envoi l'erreur
                }
                catch (eEndResponseException) { }


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