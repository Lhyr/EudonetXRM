
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Fenêtre de paramétrage des droits de traitement
    /// </summary>
    public partial class eAdminRightsDialog : eAdminPage
    {
        int _descid = 0;
        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("noUiSlider");
            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            //PageRegisters.AddScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript();
            PageRegisters.RegisterAdminIncludeScript("eAdminRights");
            PageRegisters.RegisterAdminIncludeScript("noUiSlider.min");
            #endregion

            #region Paramètres
            int nFrom = 0;
            string rightFunction = string.Empty;
            // Pour l'enregistrement des droits sur une fiche (page d'accueil xrm ou grille)
            int nPageId = 0, nGridId = 0;

            HashSet<eTreatmentType> lstTreatTypes = new HashSet<eTreatmentType>();
            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);



            _descid = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            nFrom = _requestTools.GetRequestFormKeyI("from") ?? 0;
            rightFunction = _requestTools.GetRequestFormKeyS("fct") ?? "";

            // [dev #55029] droits sur la page d'accueil 
            nPageId = _requestTools.GetRequestFormKeyI("pageid") ?? 0;

            // [dev #55029] droits sur la grille
            nGridId = _requestTools.GetRequestFormKeyI("gridid") ?? 0;

            if (allKeys.Contains("types") && !String.IsNullOrEmpty(Request.Form["types"]))
            {
                String[] sTypes = Request.Form["Types"].ToString().Split(';');
                foreach (String s in sTypes)
                {
                    Int32 i = 0;
                    if (!Int32.TryParse(s, out i))
                        continue;

                    eTreatmentType treatType;

                    if (!Enum.TryParse<eTreatmentType>(i.ToString(), out treatType))
                        continue;

                    if (!lstTreatTypes.Contains(treatType))
                        lstTreatTypes.Add(treatType);
                }
            }
            #endregion


            try
            {
                formAdminRights.Action = Request.Url.AbsoluteUri;

                eAdminRightsRenderer renderer = eAdminRendererFactory.CreateAdminRightsDialogRenderer(_pref, _descid, nPageId, nGridId, true);
                renderer.From = nFrom;
                renderer.LstTreatmentTypes = lstTreatTypes;
                renderer.Function = rightFunction;
                renderer.ExportHandler = new EventHandler(ExportButton_Handle);
                renderer.Generate();
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }
                formAdminRights.Controls.Add(renderer.PgContainer);
            }
            catch (EudoException exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        exc,
                        eResApp.GetRes(_pref, 72)
                    );

                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchError();
                }
                catch (eEndResponseException)
                {
                }
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminRightsDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchError();
                }
                catch (eEndResponseException)
                {
                }
            }
            finally
            {

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


        //private void FillDropDownLists()
        //{
        //    // Onglets
        //    ExtendedDictionary<int, String> tabs = eSqlDesc.LoadTabs(_eDal);
        //    ddlListTabs.DataSource = tabs;
        //    ddlListTabs.DataTextField = "Value";
        //    ddlListTabs.DataValueField = "Key";
        //    ddlListTabs.DataBind();
        //}

        void ExportButton_Handle(object sender, EventArgs e)
        {
            int type = _requestTools.GetRequestFormKeyI("ddlListTypes") ?? 99;
            HashSet<eTreatmentType> typesSet = new HashSet<eTreatmentType>();
            if (type != 99)
            {
                typesSet.Add((eTreatmentType)type);
            }

            eAdminRightsExport.ExportList(Context, _pref, new eAdminRightsFilters()
            {
                Tab = _requestTools.GetRequestFormKeyI("hidTab") ?? 0,
                From = _requestTools.GetRequestFormKeyI("ddlListFrom") ?? 0,
                Field = _requestTools.GetRequestFormKeyI("ddlListFields") ?? 0,
                TreatTypes = typesSet,
                Function = _requestTools.GetRequestFormKeyS("hidFct") ?? ""
            });


        }
    }
}