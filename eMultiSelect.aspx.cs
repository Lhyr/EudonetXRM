using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Fenêtre de multi-sélection pour des source de données hétérogènes
    /// </summary>
    public partial class eMultiSelect : eEudoPage
    {

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eFieldsSelect");

            #endregion

            #region ajout des js

            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eDrag");
            PageRegisters.AddScript("eTabsFieldsSelect");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("multiselect/eMultiSelectInternal");
            #endregion

            try
            {
                string param = _requestTools.GetRequestFormKeyS("param");
                eMultiSelectType multiSelectType = _requestTools.GetRequestFormEnum<eMultiSelectType>("type", true);

                eMultiSelectDataSourceInterface multiSelectDataSource = eMultiSelectFactory.CreateMultiSelectDataSource(multiSelectType, _pref, param);
                multiSelectDataSource.Generate();

                multiSelectTitle.InnerText = multiSelectDataSource.GetHeaderTitle();
                sourceItemsTitle.InnerText = multiSelectDataSource.GetSourceItemsTitle();
                targetItemsTitle.InnerText = multiSelectDataSource.GetTargetItemsTitle();
                           
                // TODO Créer un renderer    
                TdSourceList.Controls.Add(FillSourceList(multiSelectDataSource));
                TdTargetList.Controls.Add(FillTargetList(multiSelectDataSource));

            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }
            catch (eMultiSelectException exp) { ErrorContainer = exp.GetErrorContainer(_pref); }
            catch (Exception exp)
            {
                String sDevMsg = String.Concat("Erreur sur MultiSelect: ",
                    Environment.NewLine, "Message : ", exp.Message,
                    Environment.NewLine, " StackTrace :", exp.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),
                    eResApp.GetRes(_pref, 72),
                    sDevMsg);
            }

            try { LaunchError(); }
            catch (eEndResponseException) { }
        }


        /// <summary>
        /// Créée un container des elements sélectionnés
        /// </summary>
        private HtmlGenericControl FillTargetList(eMultiSelectDataSourceInterface multiSelectDataSource)
        {
            HtmlGenericControl lst = new HtmlGenericControl("div");
            lst.ID = "TabSelectedList";
            lst.Attributes.Add("class", "ItemList");
            lst.Attributes.Add("onclick", "doInitSearch(this, event);");
            lst.Attributes.Add("onmouseup", "doOnMouseUp(this);");

            String optCss = "cell";
            HtmlGenericControl itm = null;
            foreach (eMultiSelectItem item in multiSelectDataSource.GetTargetItems())
            {

                itm = new HtmlGenericControl("div");
                if (optCss.Equals("cell"))
                    optCss = "cell2";
                else
                    optCss = "cell";
                itm.Attributes.Add("class", optCss);
                itm.Attributes.Add("oldCss", optCss);

                itm.Attributes.Add("item", item.Id.ToString());
                itm.Attributes.Add("title", item.Tooltip);
                itm.Attributes.Add("onclick", "setElementSelected(this);");
                itm.InnerHtml = HttpUtility.HtmlEncode(item.Title);

                itm.ID = "item_" + item.Id.ToString();

                itm.Attributes.Add("onmouseover", "doOnMouseOver(this);");
                itm.Attributes.Add("onmousedown", "strtDrag(event);");

                lst.Controls.Add(itm);
            }


            // Création du guide de déplacement
            itm = new HtmlGenericControl("div");
            itm.ID = "SelectedListElmntGuidTS";
            itm.Attributes.Add("class", "dragGuideTab");
            itm.Attributes.Add("syst", "");
            itm.Style.Add("display", "none");
            lst.Controls.Add(itm);

            // Actions Javascript
            lst.Attributes.Add("ondblclick", "SelectItem('TabSelectedList','AllTabList');");

            return lst;
        }

        /// <summary>
        /// Créer un container des élement disponibles
        /// </summary>
        private HtmlGenericControl FillSourceList(eMultiSelectDataSourceInterface multiSelectDataSource)
        {

            String error = String.Empty;
            HtmlGenericControl lst = new HtmlGenericControl("div");
            lst.ID = "AllTabList";
            lst.Attributes.Add("class", "ItemList");
            lst.Attributes.Add("onclick", "doInitSearch(this, event);");

            String optCss = "cell";
            HtmlGenericControl itm = null;
            foreach (eMultiSelectItem item in multiSelectDataSource.GetSourceItems())
            {

                itm = new HtmlGenericControl("div");
                if (optCss.Equals("cell"))
                    optCss = "cell2";
                else
                    optCss = "cell";
                itm.Attributes.Add("class", optCss);
                itm.Attributes.Add("oldCss", optCss);

                itm.Attributes.Add("item", item.Id.ToString());
                itm.Attributes.Add("title", item.Tooltip);
                itm.Attributes.Add("onclick", "setElementSelected(this);");
                itm.InnerHtml = HttpUtility.HtmlEncode(item.Title);
                itm.ID = "item_" + item.Id.ToString();

                itm.Attributes.Add("onmouseover", "doOnMouseOver(this);");
                itm.Attributes.Add("onmousedown", "strtDrag(event);");

                lst.Controls.Add(itm);
            }

            // Création du guide de déplacement
            itm = new HtmlGenericControl("div");
            itm.ID = "AllListElmntGuidTS";
            itm.Attributes.Add("class", "dragGuideTab");
            itm.Attributes.Add("syst", "");
            itm.Style.Add("display", "none");
            lst.Controls.Add(itm);

            //Actions Javascript
            lst.Attributes.Add("ondblclick", "SelectItem('AllTabList','TabSelectedList');");

            return lst;
        }

        /// <summary>
        /// Récupération des ressources
        /// </summary>
        /// <param name="resId">Id de la ressource</param>
        /// <returns>Ressource dans la langue des préferences</returns>
        public string GetRes(int resId)
        {
            return eResApp.GetRes(_pref, resId);
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