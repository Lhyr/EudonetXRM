using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using EudoQuery;
using Com.Eudonet.Internal.prefs;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// page de selection des signets
    /// </summary>
    public partial class eSelectBkm : eEudoPage
    {
        /// <summary>nom javascript de la fenêtre modale dans laquelle est appelée la page</summary>
        public string _modalName = string.Empty;

        /// <summary>Code javascript a injecter dans la page</summary>
        public string _sJsRes = string.Empty;

        private int _nTab = 0;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

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

            #endregion

            try
            {
                _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;

                eBkmPref myBkmPref = new eBkmPref(_pref, _nTab);
                DescAdvDataSet descAdv = new DescAdvDataSet();
                int[] tabForbidBkmIris = { AllField.MEMO_DESCRIPTION.GetHashCode(), AllField.MEMO_INFOS.GetHashCode(), AllField.MEMO_NOTES.GetHashCode() };

                // TODO SWITCH 3 POSITIONS - A vérifier si le cas doit être pris en charge dans le mode "Prévisualisation"
                EUDONETX_IRIS_BLACK_STATUS eudonetXIrisBlackStatus = EUDONETX_IRIS_BLACK_STATUS.DISABLED;

                if (_nTab > 0)
                {
                    using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
                    {
                        try
                        {
                            dal.OpenDatabase();
                            descAdv.LoadAdvParams(dal, new int[] { _nTab });
                            eudonetXIrisBlackStatus = eLibTools.GetEnumFromCode<EUDONETX_IRIS_BLACK_STATUS>(descAdv.GetAdvInfoValue(_nTab, DESCADV_PARAMETER.ERGONOMICS_IRIS_BLACK, "0"), true);
                            dal.CloseDatabase();
                        }
                        catch (Exception ex)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.INFOS,
                                eResApp.GetRes(_pref, 72),
                                 eResApp.GetRes(_pref, 6342),
                                  eResApp.GetRes(_pref, 366),
                                string.Concat("Page de selection des signets (eSelectBkm.aspx) : ", ex.Message));
                        }
                    }
                }

                var bIrisBlackEnabled = (eudonetXIrisBlackStatus == EUDONETX_IRIS_BLACK_STATUS.ENABLED && _pref.ThemeXRM.Version > 1);


                IDictionary<int, string> linkedBkm = bIrisBlackEnabled
                    ? myBkmPref.GetLinkedBkm()
                        .Where(bkm => !tabForbidBkmIris.Contains(bkm.Key % 100))
                        .ToDictionary(bkm => bkm.Key, bkm => bkm.Value)
                    : myBkmPref.GetLinkedBkm();


                IDictionary<int, string> selectedBkm = myBkmPref.GetSelectedBkm();


                TdSourceList.Controls.Add(FillSourceList(linkedBkm, selectedBkm));
                TdTargetList.Controls.Add(FillSelectedList(selectedBkm, linkedBkm));

            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                     eResApp.GetRes(_pref, 6342),
                      eResApp.GetRes(_pref, 366),
                    string.Concat("Page de selection des signets (eSelectBkm.aspx) : ", ex.Message));

                //Arrete le traitement et envoi l'erreur
                LaunchError();

            }
        }

        /// <summary>
        /// remplit la zone des signets selectionnés
        /// </summary>
        private HtmlGenericControl FillSelectedList(IDictionary<int, string> selectedBkm, IDictionary<int, string> linkedBkm)
        {
            try
            {
                HtmlGenericControl lst = new HtmlGenericControl("div");
                lst.ID = "TabSelectedList";
                lst.Attributes.Add("class", "ItemList");
                lst.Attributes.Add("onclick", "doInitSearch(this, event);");

                string optCss = "cell";
                HtmlGenericControl itm = null;

                lst.Attributes.Add("onmouseup", "doOnMouseUp(this);");


                foreach (KeyValuePair<Int32, String> kvp in selectedBkm)
                {

                    if (!linkedBkm.Contains(kvp))
                        continue;

                    itm = new HtmlGenericControl("div");//_tab.Libelle, _tab.DescId.ToString());

                    itm.Attributes.Add("DescId", kvp.Key.ToString());

                    if (_nTab == EudoQuery.TableType.PM.GetHashCode() && kvp.Key == EudoQuery.TableType.ADR.GetHashCode())
                    {
                        eRes tabRes = new eRes(_pref, EudoQuery.TableType.PP.GetHashCode().ToString());
                        itm.InnerHtml = HttpUtility.HtmlEncode(tabRes.GetRes(EudoQuery.TableType.PP.GetHashCode()));

                    }
                    else
                        itm.InnerHtml = HttpUtility.HtmlEncode(kvp.Value);


                    if (optCss.Equals("cell"))
                        optCss = "cell2";
                    else
                        optCss = "cell";
                    itm.Attributes.Add("class", optCss);
                    itm.Attributes.Add("oldCss", optCss);
                    itm.Attributes.Add("onclick", "setElementSelected(this);");

                    itm.Attributes.Add("onmouseover", "doOnMouseOver(this);");
                    itm.Attributes.Add("onmousedown", "strtDrag(event);");

                    itm.ID = "descId_" + kvp.Key.ToString();
                    lst.Controls.Add(itm);

                }

                // Création du guide de déplacement
                itm = new HtmlGenericControl("div");
                itm.ID = "SelectedListElmntGuidSB";
                itm.Attributes.Add("class", "dragGuideTab");
                itm.Attributes.Add("syst", "");
                itm.Style.Add("display", "none");
                lst.Controls.Add(itm);

                // Actions Javascript
                lst.Attributes.Add("ondblclick", "SelectItem('TabSelectedList','AllTabList');");
                return lst;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Remplt la zone des signets diponibles
        /// </summary>
        private HtmlGenericControl FillSourceList(IDictionary<int, string> linkedBkm, IDictionary<int, string> selectedBkm)
        {

            try
            {
                HtmlGenericControl lst = new HtmlGenericControl("div");
                lst.ID = "AllTabList";
                lst.Attributes.Add("class", "ItemList");
                lst.Attributes.Add("onclick", "doInitSearch(this, event);");

                string optCss = "cell";
                HtmlGenericControl itm = new HtmlGenericControl("div");//_tab.Libelle, _tab.DescId.ToString());



                foreach (KeyValuePair<int, string> kvp in linkedBkm)
                {
                    if (selectedBkm.Contains(kvp))
                        continue;

                    itm = new HtmlGenericControl("div");//_tab.Libelle, _tab.DescId.ToString());
                    if (optCss.Equals("cell"))
                        optCss = "cell2";
                    else
                        optCss = "cell";

                    itm.Attributes.Add("class", optCss);
                    itm.Attributes.Add("oldCss", optCss);

                    itm.Attributes.Add("DescId", kvp.Key.ToString());
                    itm.Attributes.Add("onclick", "setElementSelected(this);");


                    //if (_nTab == EudoQuery.TableType.PM.GetHashCode() && kvp.Key == EudoQuery.TableType.ADR.GetHashCode())
                    //{
                    //    eRes tabRes = new eRes(_pref, EudoQuery.TableType.PP.GetHashCode().ToString());
                    //    itm.InnerHtml = HttpUtility.HtmlEncode(tabRes.GetRes(EudoQuery.TableType.PP.GetHashCode()));

                    //}
                    //else
                    itm.InnerHtml = HttpUtility.HtmlEncode(kvp.Value);


                    itm.ID = "descId_" + kvp.Key.ToString();


                    itm.Attributes.Add("onmouseover", "doOnMouseOver(this);");
                    itm.Attributes.Add("onmousedown", "strtDrag(event);");
                    lst.Controls.Add(itm);


                }

                // Création du guide de déplacement
                itm = new HtmlGenericControl("div");
                itm.ID = "AllListElmntGuidSB";
                itm.Attributes.Add("class", "dragGuideTab");
                itm.Attributes.Add("syst", "");
                itm.Style.Add("display", "none");
                lst.Controls.Add(itm);

                //Actions Javascript
                lst.Attributes.Add("ondblclick", "SelectItem('AllTabList','TabSelectedList');");

                return lst;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Récupère la res pour le code html
        /// </summary>
        public string GetRes(int resId)
        {
            return eResApp.GetRes(_pref, resId);
        }

    }
}