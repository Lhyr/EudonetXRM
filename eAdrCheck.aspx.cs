using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <className>eAdrCheck</className>
    /// <summary>Page de demande de mise à jour des addresses non identiques après mise à jour d'une rubrique postale de PM</summary>
    /// <purpose>Va recherche les fiches consernées et construit le rendu pour pouvoir choisir</purpose>
    /// <authors>HLA</authors>
    /// <date>2012-10-26</date>
    public partial class eAdrCheck : eEudoPage
    {
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
            Table adrTable = null;

            Int32 descid = 0;
            String adrToUpd = String.Empty;
            String adrNoUpd = String.Empty;
            String error = String.Empty;

            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");

 
            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            if (allKeys.Contains("descid") && !String.IsNullOrEmpty(Request.Form["descid"]))
                descid = eLibTools.GetNum(Request.Form["descid"].ToString());

            if (allKeys.Contains("adrtoupd") && !String.IsNullOrEmpty(Request.Form["adrtoupd"]))
                adrToUpd = Request.Form["adrtoupd"].ToString();

            if (allKeys.Contains("adrnoupd") && !String.IsNullOrEmpty(Request.Form["adrnoupd"]))
                adrNoUpd = Request.Form["adrnoupd"].ToString();

            /* Nettoye le contenu d'id invalide */
            adrToUpd = eLibTools.CleanIdList(adrToUpd);
            adrNoUpd = eLibTools.CleanIdList(adrNoUpd);

            hiddenDescid.Value = descid.ToString();
            hiddenAdrtoupd.Value = adrToUpd;

            if (descid == 0 || adrNoUpd.Length == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "descid, adrNoUpd.Length"), " (descid = ", descid, ", adrNoUpd.Length = ", adrNoUpd.Length, ")")
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            eDataFillerGeneric filler = new eDataFillerGeneric(_pref, TableType.ADR.GetHashCode(), ViewQuery.CUSTOM);
            filler.EudoqueryComplementaryOptions =
                delegate(EudoQuery.EudoQuery eqf)
                {
                    IEnumerable<Int32> lstFieldAdrCopy = eLibTools.GetCommonPmAddressPostalField(_pref, descid);
                    eqf.SetListCol = String.Concat("201;",
                        eLibTools.Join(";", lstFieldAdrCopy,
                            delegate(Int32 tmp)
                            {
                                // On n'ajoute pas la rubrique pays "ADR03" - Iso V7
                                if (tmp == 403)
                                    return null;

                                return tmp.ToString();
                            }));


                    eqf.AddCustomFilter(new WhereCustom("ADRID", Operator.OP_IN, adrNoUpd.Replace(";", ",")));

                };


            if (filler.Generate() && filler.ErrorMsg.Length == 0)
            {

                adrTable = filler.ViewMainTable;
                textMsgInfo.InnerText = eResApp.GetRes(_pref, 1425).Replace("<TAB>", adrTable.Libelle);


                Int32 adrId = 0;
                String ppName = String.Empty;
                StringBuilder sbInfos = new StringBuilder();

                #region ajout des css

                PageRegisters.AddCss("eModalDialog");
                PageRegisters.AddCss("eControl");
                PageRegisters.AddCss("eAdrCheck");

                #endregion

                foreach (eRecord er in filler.ListRecords)
                {

                    adrId = er.MainFileid;

                    if (adrId == 0)
                        continue;

                    ppName = String.Empty;
                    sbInfos.Length = 0;


                    foreach (eFieldRecord fld in er.GetFields)
                    {
                        if (!fld.FldInfo.DrawField || !fld.RightIsVisible)
                            continue;

                        if (fld.FldInfo.Descid == 201)
                            ppName = fld.DisplayValue;
                        else
                        {
                            if (sbInfos.Length != 0)
                                sbInfos.Append(" ");
                            sbInfos.Append(fld.DisplayValue);
                        }
                    }

                    if (ppName.Length == 0 && sbInfos.Length == 0)
                        continue;

                    HtmlGenericControl paragraphe = new HtmlGenericControl("p");
                    adrCheckList.Controls.Add(paragraphe);

                    eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(false, false);
                    chkCtrl.ID = adrId.ToString();
                    chkCtrl.Attributes.Add("adri", String.Empty);
                    chkCtrl.AddClick(String.Empty);
                    chkCtrl.AddText(String.Concat(ppName, " (", sbInfos.ToString(), ")"));
                    paragraphe.Controls.Add(chkCtrl);
                }
            }
            else
            {
                StringBuilder sDevMsg = new StringBuilder();
                StringBuilder sUserMsg = new StringBuilder();

                sDevMsg.Append("Erreur sur la page : ").AppendLine(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).AppendLine(filler.ErrorMsg);

                if (filler.InnerException != null)
                    sDevMsg.AppendLine(filler.InnerException.Message).AppendLine(filler.InnerException.StackTrace);

                sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));


                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    sUserMsg.ToString(),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    sDevMsg.ToString()

                    );

                LaunchError();
            }



        }
    }
}