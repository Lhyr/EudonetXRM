using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eTreatmentFieldOpionManager
    /// </summary>
    public class eTreatmentFieldOptionManager : eEudoManager
    {
        /// <summary>
        /// Retourne le HTML du champ
        /// </summary>
        /// <param name="sType">Type du champ a mettre à jour</param>
        /// <returns>Code html du controle</returns>
        private String GetHTMLFieldControl(String sType, String sBaseId = "")
        {

            StringBuilder sb = new StringBuilder();

            Panel fldCtrl = new Panel();
            HtmlInputText inpt = new HtmlInputText();
            inpt.ID = String.Concat("action_update_withnew_value_vis", sBaseId);

            if (sType.ToLower() == "date")
                inpt.Attributes.Add("class", "trt_UpdWithNew_Date");
            else
                inpt.Attributes.Add("class", "trt_UpdWithNew");

            switch (sType.ToLower())
            {
                case "cat":
                    //Dans le cas d'un catalogue, le input est disabled
                    inpt.Attributes.Add("readonly", "true");
                    inpt.Attributes.Add("disabled", "true");
                    fldCtrl.Controls.Add(inpt);

                    eDivCtrl imgCat = new eDivCtrl();
                    fldCtrl.Controls.Add(imgCat);
                    imgCat.CssClass = "icon-catalog icnFileBtn trt_btn";
                    imgCat.Attributes.Add("onclick", "doAction(this)");
                    imgCat.Attributes.Add("action", "LNKCAT");
                    imgCat.Style.Add("border-width", "0px");
                    break;
                case "lnkfile":
                    //Dans le cas d'un catalogue, le input est disabled
                    inpt.Attributes.Add("readonly", "true");
                    inpt.Attributes.Add("disabled", "true");
                    fldCtrl.Controls.Add(inpt);

                    eDivCtrl imgCatLnk = new eDivCtrl();
                    fldCtrl.Controls.Add(imgCatLnk);
                    imgCatLnk.CssClass = "icon-catalog icnFileBtn trt_btn";
                    imgCatLnk.Attributes.Add("onclick", "doAction(this)");
                    imgCatLnk.Attributes.Add("action", "LNKFILE");
                    imgCatLnk.Style.Add("border-width", "0px");
                    break;
                case "user":

                    //Dans le cas d'un catalogue user, le input est disabled
                    inpt.Attributes.Add("readonly", "true");
                    inpt.Attributes.Add("disabled", "true");

                    fldCtrl.Controls.Add(inpt);
                    eDivCtrl imgCatUser = new eDivCtrl();
                    fldCtrl.Controls.Add(imgCatUser);
                    imgCatUser.CssClass = "icon-catalog icnFileBtn trt_btn";
                    imgCatUser.Attributes.Add("onclick", "doAction(this)");
                    imgCatUser.Attributes.Add("action", "LNKUSER");

                    imgCatUser.Style.Add("border-width", "0px");
                    break;

                case "date":
                    // Input readonly + button
                    inpt.Attributes.Add("readonly", "true");
                    inpt.Attributes.Add("disabled", "true");
                    fldCtrl.Controls.Add(inpt);

                    eDivCtrl imgDate = new eDivCtrl();
                    fldCtrl.Controls.Add(imgDate);
                    imgDate.CssClass = "icon-agenda btnIe8 trt_btn";

                    imgDate.Attributes.Add("onclick", "doAction(this)");
                    imgDate.Attributes.Add("action", "LNKDATE");

                    imgDate.Style.Add("border-width", "0px");
                    break;
                case "varchar":
                    //champs à saisie libre
                    inpt.Attributes.Add("onblur", "doAction(this)");
                    inpt.Attributes.Add("action", "LNKTEXT");
                    fldCtrl.Controls.Add(inpt);

                    break;
                case "mail":
                    inpt.Attributes.Add("onblur", "doAction(this)");
                    inpt.Attributes.Add("action", "LNKMAIL");
                    fldCtrl.Controls.Add(inpt);
                    break;
                case "numeric":

                    inpt.Attributes.Add("onblur", "doAction(this)");
                    inpt.Attributes.Add("action", "LNKNUM");
                    fldCtrl.Controls.Add(inpt);

                    break;
                case "datedecal":
                    //option 2 des date : date décallée : il s'agit de pouvoir décaller la date d'un nombre d'unité (minutes, heures, jours)
                    HtmlInputText datedecal = new HtmlInputText();
                    fldCtrl.Controls.Add(datedecal);
                    datedecal.Attributes.Add("onchange", "updateDateDecal();");
                    datedecal.ID = "action_update_withnew_value_val";
                    datedecal.Attributes.Add("class", "trt_UpdWithNew_datedeaclNpt");
                    datedecal.Value = "1";

                    HtmlSelect optiondecal = new HtmlSelect();
                    optiondecal.ID = "action_update_withnew_value_type";
                    optiondecal.Attributes.Add("onchange", "updateDateDecal();");
                    optiondecal.Attributes.Add("class", "trt_UpdWithNew_datedeacl");
                    optiondecal.Attributes.Add("width", "90px");

                    fldCtrl.Controls.Add(optiondecal);

                    List<ListItem> list = new List<ListItem>();

                    list.Add(new ListItem(eResApp.GetRes(_pref, 855), "Y"));
                    list.Add(new ListItem(eResApp.GetRes(_pref, 854), "M"));
                    list.Add(new ListItem(eResApp.GetRes(_pref, 853), "D"));
                    list.Add(new ListItem(eResApp.GetRes(_pref, 852), "W"));
                    list.Add(new ListItem(eResApp.GetRes(_pref, 851), "H"));
                    list.Add(new ListItem(eResApp.GetRes(_pref, 850), "N"));
                    optiondecal.Items.AddRange(list.ToArray());

                    optiondecal.SelectedIndex = 2;
                    break;
                case "bit":
                case "bitbutton":
                    //La case à coché se sélectionne à travers une liste déroulante avec cochée/décochée
                    HtmlSelect optionCAC = new HtmlSelect();
                    optionCAC.ID = "action_update_withnew_value_type";
                    optionCAC.Attributes.Add("onchange", "updateBit();");
                    optionCAC.Attributes.Add("class", "trt_UpdWithNew_bit");
                    optionCAC.Attributes.Add("width", "90px");

                    //fldCtrl.Controls.Add(optionCAC);

                    List<ListItem> listCAC = new List<ListItem>();
                    if (sType.ToLower() == "bit")
                    {
                        listCAC.Add(new ListItem(eResApp.GetRes(_pref, 308), "1"));   //est cochée
                        listCAC.Add(new ListItem(eResApp.GetRes(_pref, 309), "0"));   //est décochée
                    }
                    else
                    {
                        listCAC.Add(new ListItem(eResApp.GetRes(_pref, 2998), "1"));   //est cliqué
                        listCAC.Add(new ListItem(eResApp.GetRes(_pref, 2997), "0"));   //est décliqué
                    }
                    optionCAC.Items.AddRange(listCAC.ToArray());

                    optionCAC.SelectedIndex = 0;

                    return eRenderer.RenderWebControl(optionCAC);
                default:
                    break;
            }

            return eRenderer.RenderWebControl(fldCtrl);
        }


        /// <summary>
        /// Retourne le code html d'un tableau contenant un control field
        /// </summary>
        /// <param name="nDescId"></param>
        /// <returns></returns>
        private String GetHTMLField(Int32 nDescId)
        {
            eRenderer er = eRenderer.CreateRenderer(RENDERERTYPE.EditFile);
            eRecord erow = new eRecord();
            eFieldRecord efro = new eFieldRecord();

            Int32 nTab = nDescId - nDescId % 100;

            EudoQuery.EudoQuery eq = eLibTools.GetEudoQuery(_pref, nTab, ViewQuery.FIELD_VALUES);
            try
            {
                eq.SetListCol = nDescId.ToString();
                eq.LoadRequest();

                efro.RightIsUpdatable = true;
                efro.RightIsVisible = true;
                string serr = string.Empty;
                efro.FldInfo = eq.GetFieldHeaderList.Find(delegate (Field f) { return f.Descid == nDescId; });
                efro.FileId = 0;
            }
            finally
            {
                eq.CloseQuery();
            }

            erow.AddField(efro);
            erow.CalledTab = efro.FldInfo.Table.DescId;
            erow.MainFileid = efro.FldInfo.Table.MainFieldDescId;
            erow.ViewTab = efro.FldInfo.Table.DescId;


            System.Web.UI.WebControls.Table tab = new System.Web.UI.WebControls.Table();
            TableRow tr = new TableRow();
            TableCell tc = (TableCell)er.GetFieldValueCell(erow, efro, 0, _pref);
            TableCell tb = er.GetButtonCell(tc, true);
            tab.Controls.Add(tr);
            tr.Controls.Add(tc);
            tr.Controls.Add(tb);

            return eRenderer.RenderWebControl(tab);
        }

        /// <summary>
        /// Retourne un select rempli avec les champs qui ont le meme type que le champs qui a nDescid 
        /// </summary>
        /// <param name="nDescId"></param>
        /// <param name="nbFields"></param>
        /// <returns></returns>
        private String GetHTMLFromOtherField(Int32 nDescId, out Int32 nbFields)
        {
            HtmlSelect select = new HtmlSelect();
            select.ID = "action_update_fromexisting_value";
            select.Attributes.Add("class", "selectActionsUpdateFromExisting");
            select.Attributes.Add("width", "90px");

            Dictionary<Int32, String> dic = GetAllFieldsWithSameFormat(_pref, nDescId, true);
            nbFields = dic.Count;
            select.Disabled = nbFields <= 0;
            select.Attributes.Add("readonly", "1");

            foreach (var entry in dic)
                select.Items.Add(new ListItem(entry.Value, entry.Key.ToString()));

            return eRenderer.RenderControl(select);

        }

        /// <summary>
        /// Création des options de traitement de mise à jour d'un filtre
        /// </summary>
        private void GetFieldOptions(int nDescId)
        {
            StringBuilder sDevMsg = new StringBuilder();
            StringBuilder sUserMsg = new StringBuilder();

            if (nDescId == 0)
            {
                sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append("Pas de descid");

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
            else
            {
                try
                {
                    FieldLite fld = null;
                    if (nDescId % 100 == 0)
                    {
                        //liaisons parentes
                        fld = eLibTools.GetFieldInfo(_pref, nDescId + 1, FieldLite.Factory());
                    }
                    else
                    {
                        fld = eLibTools.GetFieldInfo(_pref, nDescId, FieldLite.Factory());
                    }

                    if (fld == null)
                        return;

                    XmlDocument xmlResult = new XmlDocument();

                    XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xmlResult.AppendChild(mainNode);

                    XmlNode xmlNodeRoot = xmlResult.CreateElement("ednResult");
                    xmlResult.AppendChild(xmlNodeRoot);

                    // Indicateur de non success
                    XmlNode xmlNodeSuccess = xmlResult.CreateElement("success");
                    xmlNodeRoot.AppendChild(xmlNodeSuccess);
                    xmlNodeSuccess.InnerText = "1";

                    // Indicateur de non success
                    XmlElement xmlFieldInfos = xmlResult.CreateElement("fieldInfos");
                    xmlNodeRoot.AppendChild(xmlFieldInfos);

                    XmlNode xmlEditorType = xmlResult.CreateElement("editortype");
                    xmlFieldInfos.AppendChild(xmlEditorType);


                    XmlElement optionsNode = xmlResult.CreateElement("options");
                    xmlNodeRoot.AppendChild(optionsNode);

                    XmlElement optionOtherFieldNode = xmlResult.CreateElement("option");
                    optionOtherFieldNode.SetAttribute("label", eResApp.GetRes(_pref, 302)); // A partir des valeurs de la rubriques
                    optionOtherFieldNode.SetAttribute("optionlevel", "2");
                    optionOtherFieldNode.SetAttribute("action", "fromexisting");
                    optionOtherFieldNode.SetAttribute("radioName", "fromexisting");

                    optionsNode.AppendChild(optionOtherFieldNode);

                    XmlElement optCtl = xmlResult.CreateElement("optionControl");
                    Int32 nbOtherFields = 0;
                    String sF = GetHTMLFromOtherField(nDescId, out nbOtherFields);
                    optionOtherFieldNode.AppendChild(optCtl);
                    optCtl.InnerText = sF;

                    if (nbOtherFields <= 0)
                        optionOtherFieldNode.SetAttribute("disabled", "1");

                    //Rendu en fonction du type de champ
                    switch (fld.Format)
                    {
                        case FieldFormat.TYP_HIDDEN:
                            break;

                        case FieldFormat.TYP_DATE:
                            //Pour les champs de type date, il faut afficher :
                            //    1. Pour le champ "a partir de la rubrique" , uniquement les champs de type date
                            //    2. Pour "Avec une nouvelle valeur" :
                            //          * Un champ de sélection de type "date"
                            //          * Un champ de décalage
                            xmlEditorType.InnerText = "LNKDATE";

                            //Option date fixe
                            XmlElement optionFixedDateNode = xmlResult.CreateElement("option");
                            optionsNode.AppendChild(optionFixedDateNode);
                            optionFixedDateNode.SetAttribute("label", eResApp.GetRes(_pref, 369));// date fixe
                            optionFixedDateNode.SetAttribute("radioName", "dateFixed");
                            optionFixedDateNode.SetAttribute("optionlevel", "2");
                            optionFixedDateNode.SetAttribute("action", "withnew");
                            optionFixedDateNode.SetAttribute("onchange", "updateDateDecal();");


                            String sField = GetHTMLFieldControl("date");
                            XmlElement optionControl = xmlResult.CreateElement("optionControl");
                            optionFixedDateNode.AppendChild(optionControl);
                            optionControl.InnerText = sField;

                            //Option décalage
                            XmlElement optionOffsetDateNode = xmlResult.CreateElement("option");
                            optionsNode.AppendChild(optionOffsetDateNode);
                            optionOffsetDateNode.SetAttribute("label", eResApp.GetRes(_pref, 848));// décaler de
                            optionOffsetDateNode.SetAttribute("radioName", "dateDecal");
                            optionOffsetDateNode.SetAttribute("optionlevel", "2");
                            optionOffsetDateNode.SetAttribute("action", "withnewdate");
                            optionOffsetDateNode.SetAttribute("onchange", "updateDateDecal();");

                            sField = GetHTMLFieldControl("datedecal");
                            optionControl = xmlResult.CreateElement("optionControl");
                            optionOffsetDateNode.AppendChild(optionControl);
                            optionControl.InnerText = sField;



                            break;
                        case FieldFormat.TYP_USER:
                            xmlEditorType.InnerText = "LNKUSER";

                            //Option Champ Libre
                            XmlElement optionNewValueUserNode = xmlResult.CreateElement("option");
                            optionNewValueUserNode.SetAttribute("label", eResApp.GetRes(_pref, 301));
                            optionNewValueUserNode.SetAttribute("optionlevel", "2");
                            optionNewValueUserNode.SetAttribute("radioName", "newvalue");
                            optionNewValueUserNode.SetAttribute("action", "withnew");
                            xmlFieldInfos.SetAttribute("multiple", fld.Multiple ? "1" : "0");
                            optionsNode.AppendChild(optionNewValueUserNode);


                            String sFieldUser = GetHTMLFieldControl("user");
                            optionControl = xmlResult.CreateElement("optionControl");
                            optionNewValueUserNode.AppendChild(optionControl);
                            optionControl.InnerText = sFieldUser;

                            //Pour les catalogue a choix multiple
                            if (fld.Multiple)
                            {
                                //Option Ecraser les valeur
                                XmlElement optionEraseValueNode = xmlResult.CreateElement("option");
                                optionEraseValueNode.SetAttribute("optionlevel", "3");
                                optionEraseValueNode.SetAttribute("label", eResApp.GetRes(_pref, 643));
                                optionEraseValueNode.SetAttribute("radioName", "withnew_erase_existing");
                                optionEraseValueNode.SetAttribute("checkbox", "1");

                                optionsNode.AppendChild(optionEraseValueNode);



                                #region Retirer une valeur (pas treeview) - canceled
                                /*
                                if (!fld.IsTreeView)
                                {

                                    String sFieldDefRemove = GetHTMLFieldControl("user", "_remove");
                                    XmlElement optionRemoveValueNode = xmlResult.CreateElement("option");
                                    optionRemoveValueNode.SetAttribute("optionlevel", "2");
                                    optionRemoveValueNode.SetAttribute("remove", "1");
                                    optionRemoveValueNode.SetAttribute("label", eResApp.GetRes(_pref, 1488));
                                    optionRemoveValueNode.SetAttribute("radioName", "removevalue");
                                    optionRemoveValueNode.SetAttribute("action", "removevalue");
                                    optionsNode.AppendChild(optionRemoveValueNode);

                                    XmlElement optionControlDefRemove = xmlResult.CreateElement("optionControl");
                                    optionRemoveValueNode.AppendChild(optionControlDefRemove);
                                    optionControlDefRemove.InnerText = sFieldDefRemove;
                                }
                                 */
                                #endregion
                            }
                            break;

                        case FieldFormat.TYP_BITBUTTON:
                        case FieldFormat.TYP_BIT:
                            xmlEditorType.InnerText = "BIT";

                            //Option Case à cocher
                            XmlElement optionNewValueCACNode = xmlResult.CreateElement("option");
                            optionNewValueCACNode.SetAttribute("label", eResApp.GetRes(_pref, 301));
                            optionNewValueCACNode.SetAttribute("optionlevel", "2");
                            optionNewValueCACNode.SetAttribute("radioName", "newvalue");
                            optionNewValueCACNode.SetAttribute("action", "withnew");
                            optionNewValueCACNode.SetAttribute("onchange", "updateBit();");
                            optionsNode.AppendChild(optionNewValueCACNode);

                            String sFieldCAC = GetHTMLFieldControl(fld.Format == FieldFormat.TYP_BIT ? "bit" : "bitbutton");
                            optionControl = xmlResult.CreateElement("optionControl");
                            optionNewValueCACNode.AppendChild(optionControl);
                            optionControl.InnerText = sFieldCAC;

                            break;
                        case FieldFormat.TYP_CHAR:
                            if (fld.Popup != PopupType.NONE || nDescId % 100 == 0)
                            {
                                PopupType popup = fld.Popup;
                                int iPopupDescId = fld.PopupDescId;
                                /* Info Catalogue */
                                if (nDescId % 100 == 0)
                                {
                                    popup = PopupType.SPECIAL;
                                    iPopupDescId = nDescId + 1;
                                    xmlFieldInfos.SetAttribute("descid", fld.Descid.ToString());
                                }

                                xmlFieldInfos.SetAttribute("popup", ((int)popup).ToString());
                                xmlFieldInfos.SetAttribute("popupdescid", iPopupDescId.ToString());
                                xmlFieldInfos.SetAttribute("descid", nDescId.ToString());
                                xmlFieldInfos.SetAttribute("multiple", fld.Multiple ? "1" : "0");
                                xmlFieldInfos.SetAttribute("treeview", fld.PopupDataRend == PopupDataRender.TREE ? "1" : "0");
                                FieldLite fldBound = fld;
                                if (nDescId != iPopupDescId && nDescId + 1 != iPopupDescId)
                                    fldBound = eLibTools.GetFieldInfo(_pref, iPopupDescId, FieldLite.Factory());

                                bool bSpecialCat = popup == PopupType.SPECIAL;
                                xmlFieldInfos.SetAttribute("special", bSpecialCat ? "1" : "0");

                                // Type catalogue
                                xmlEditorType.InnerText = "LNKCAT";
                                XmlElement optionNewValueNode = xmlResult.CreateElement("option");
                                optionNewValueNode.SetAttribute("optionlevel", "2");
                                optionNewValueNode.SetAttribute("label", eResApp.GetRes(_pref, 301));
                                optionNewValueNode.SetAttribute("radioName", "newvalue");
                                optionNewValueNode.SetAttribute("action", "withnew");
                                optionsNode.AppendChild(optionNewValueNode);

                                String strType = "cat";
                                if (bSpecialCat)
                                    strType = "lnkfile";

                                String sFieldDef = GetHTMLFieldControl(strType);
                                XmlElement optionControlDef = xmlResult.CreateElement("optionControl");
                                optionNewValueNode.AppendChild(optionControlDef);
                                optionControlDef.InnerText = sFieldDef;

                                //Pour les catalogue a choix multiple
                                if (fld.Multiple)
                                {
                                    //Option Ecraser les valeur
                                    XmlElement optionEraseValueNode = xmlResult.CreateElement("option");
                                    optionEraseValueNode.SetAttribute("optionlevel", "3");
                                    optionEraseValueNode.SetAttribute("label", eResApp.GetRes(_pref, 643));
                                    optionEraseValueNode.SetAttribute("radioName", "withnew_erase_existing");
                                    optionEraseValueNode.SetAttribute("checkbox", "1");
                                    optionsNode.AppendChild(optionEraseValueNode);

                                    //Retirer une valeur (pas treeview)
                                    if (fld.PopupDataRend != PopupDataRender.TREE)
                                    {
                                        String sFieldDefRemove = GetHTMLFieldControl(strType, "_remove");
                                        XmlElement optionRemoveValueNode = xmlResult.CreateElement("option");
                                        optionRemoveValueNode.SetAttribute("optionlevel", "2");
                                        optionRemoveValueNode.SetAttribute("label", eResApp.GetRes(_pref, 6865));
                                        optionRemoveValueNode.SetAttribute("radioName", "removevalue");
                                        optionRemoveValueNode.SetAttribute("remove", "1");
                                        optionRemoveValueNode.SetAttribute("action", "removevalue");
                                        optionsNode.AppendChild(optionRemoveValueNode);

                                        XmlElement optionControlDefRemove = xmlResult.CreateElement("optionControl");
                                        optionRemoveValueNode.AppendChild(optionControlDefRemove);
                                        optionControlDefRemove.InnerText = sFieldDefRemove;
                                    }
                                }
                            }
                            else
                            {
                                xmlEditorType.InnerText = "LNKCHAR";

                                //Option Champ Libre
                                XmlElement optionNewValueNode = xmlResult.CreateElement("option");
                                optionNewValueNode.SetAttribute("label", eResApp.GetRes(_pref, 301));
                                optionNewValueNode.SetAttribute("optionlevel", "2");
                                optionNewValueNode.SetAttribute("radioName", "newvalue");
                                optionNewValueNode.SetAttribute("action", "withnew");

                                optionsNode.AppendChild(optionNewValueNode);

                                String sFieldDef = GetHTMLFieldControl("varchar");
                                XmlElement optionControlDef = xmlResult.CreateElement("optionControl");
                                optionNewValueNode.AppendChild(optionControlDef);
                                optionControlDef.InnerText = sFieldDef;
                            }
                            break;
                        case FieldFormat.TYP_NUMERIC:
                        case FieldFormat.TYP_MONEY:
                            xmlEditorType.InnerText = "LNKNUM";

                            //Option Champ Libre
                            XmlElement optionNewValueNumNode = xmlResult.CreateElement("option");
                            optionNewValueNumNode.SetAttribute("label", eResApp.GetRes(_pref, 301));
                            optionNewValueNumNode.SetAttribute("optionlevel", "2");
                            optionNewValueNumNode.SetAttribute("radioName", "newvalue");
                            optionNewValueNumNode.SetAttribute("action", "withnew");

                            optionsNode.AppendChild(optionNewValueNumNode);

                            String sFieldNumDef = GetHTMLFieldControl("numeric");
                            XmlElement optionControlNumDef = xmlResult.CreateElement("optionControl");
                            optionNewValueNumNode.AppendChild(optionControlNumDef);
                            optionControlNumDef.InnerText = sFieldNumDef;

                            break;
                        case FieldFormat.TYP_EMAIL:
                            xmlEditorType.InnerText = "LNKMAIL";

                            //Option valeur email
                            XmlElement optionNewValueMailNode = xmlResult.CreateElement("option");
                            optionNewValueMailNode.SetAttribute("label", eResApp.GetRes(_pref, 301));
                            optionNewValueMailNode.SetAttribute("optionlevel", "2");
                            optionNewValueMailNode.SetAttribute("radioName", "newvalue");
                            optionNewValueMailNode.SetAttribute("action", "withnew");

                            optionsNode.AppendChild(optionNewValueMailNode);

                            String sFieldMailDef = GetHTMLFieldControl("mail");
                            XmlElement optionControlMailDef = xmlResult.CreateElement("optionControl");
                            optionNewValueMailNode.AppendChild(optionControlMailDef);
                            optionControlMailDef.InnerText = sFieldMailDef;

                            break;
                    }

                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });

                }
                catch (eEndResponseException) { }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    sDevMsg.Length = 0;
                    sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine);

                    sDevMsg.Append(sDevMsg).Append(Environment.NewLine).Append("Exception Message : ").Append(ex.Message).Append(Environment.NewLine).Append("Exception StackTrace :").Append(ex.StackTrace);

                    sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));

                    ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                     sUserMsg.ToString(),  //  Détail : pour améliorer...
                       eResApp.GetRes(_pref, 72),  //   titre
                       sDevMsg.ToString());

                    LaunchError();
                }


            }

        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 nDescId = 0;
            String sAction = String.Empty;


            if (_allKeys.Contains("action"))
                sAction = _context.Request.Form["action"].ToString();

            if (_allKeys.Contains("descid"))
                Int32.TryParse(_context.Request.Form["descid"].ToString(), out nDescId);

            String sDevMsg = String.Empty;

            switch (sAction)
            {
                case "getinfos":
                    GetFieldOptions(nDescId);

                    break;
                default:
                    break;
            }

        }


        /// <summary>
        /// TREATMENT - Retourne la liste des champs de même type que le champ passé en paramètre et de la même table (seulement si droit de visu)
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur</param>
        /// <param name="nDescId"></param>
        /// <param name="sortFromDescIdList"></param>
        /// <returns>Liste des champs (clé : descid - valeur : libellé du champ)</returns>
        private static Dictionary<Int32, String> GetAllFieldsWithSameFormat(ePrefLite pref, Int32 nDescId, bool sortFromDescIdList)
        {
            EudoQuery.EudoQuery query = null;
            List<Field> fields = null;
            Int32 tabDescId = eLibTools.GetTabFromDescId(nDescId);

            #region Appel d'EudoQuery pour la gestion des droits
            try
            {
                query = eLibTools.GetEudoQuery(pref, tabDescId, ViewQuery.FILE);
                if (query.GetError.Length != 0)
                    throw new Exception("GetAllFieldsWithSameFormat=> EudoQuery.Init pour " + query.GetError);

                query.LoadRequest();
                if (query.GetError.Length != 0)
                    throw new Exception("GetAllFieldsWithSameFormat=> EudoQuery.LoadRequest pour " + query.GetError);

                fields = query.GetFieldHeaderList;
                if (fields.Count == 0)
                    throw new Exception("GetAllFieldsWithSameFormat=> EudoQuery.GetFieldHeaderList liste de fields vide");
            }
            finally
            {
                if (query != null)
                    query.CloseQuery();
            }

            #endregion

            eFieldLiteTreatment fldSrc = null;
            try
            {
                fldSrc = eLibTools.GetFieldInfo(pref, nDescId, eFieldLiteTreatment.Factory());
                if (fldSrc == null)
                    throw new Exception("Rubrique " + nDescId + " introuvable");
            }
            catch (Exception exp)
            {
                throw new Exception(String.Concat("GetAllFieldsWithSameFormat : ", exp.Message));
            }

            //bool bKeySector = false;  //les clés de secteurs sont un reste de la v8=>v5 donc inutile depuis la v7
            bool bFormatNotAllowed = false;
            bool bFieldNotAllowed = false;
            Boolean bMatched = false;

            Dictionary<Int32, String> dicFields = new Dictionary<Int32, String>();
            foreach (Field fld in fields)
            {

                if ((fld.Table.DescId == tabDescId) && fld.DrawField && fld.PermViewAll)
                {
                    if (fld.Descid == nDescId)
                        continue;

                    //bKeySector = (((tabDescId == TableType.PP.GetHashCode()) && (fld.Descid == 292 || fld.Descid == 290)) || (tabDescId == TableType.PM.GetHashCode() && fld.Descid == 392));
                    bFormatNotAllowed = (fld.Format == FieldFormat.TYP_HIDDEN 
                        || fld.Format == FieldFormat.TYP_TITLE
                        || fld.Format == FieldFormat.TYP_MEMO
                        || fld.Format == FieldFormat.TYP_ALIAS
                        || fld.Format == FieldFormat.TYP_ALIASRELATION
                        || fld.Format == FieldFormat.TYP_IMAGE 
                        || fld.Format == FieldFormat.TYP_PASSWORD);

                    //ASY : Commentaire de la V7 => 'AV le 13/10/2003 : (Suite Mail BV) Remplacement d'adresse principale (412) ne doit pas être possible en masse 
                    //ASY : Commentaire de la V7 => 'SPH 24/07/2009 : Ajout de adresse perso à la liste des champ non modifiable par traitement
                    bFieldNotAllowed = ((fld.Descid == AdrField.PERSO.GetHashCode()) || (fld.Descid == (tabDescId + PlanningField.DESCID_SCHEDULE_ID.GetHashCode())) || (fld.Descid == (AdrField.PRINCIPALE.GetHashCode())) || (fld.Descid == (tabDescId + 86)) || ((fld.Descid == (tabDescId + 1)) && (fld.Table.EdnType == EdnType.FILE_MAIN)))
                                    || ((fld.Descid == (tabDescId + PlanningField.DESCID_CALENDAR_ITEM.GetHashCode())) || (fld.Descid >= (tabDescId + AllField.DATE_CREATE.GetHashCode()) && fld.Descid <= (tabDescId + AllField.USER_MODIFY.GetHashCode())));

                    //ASY : Commentaire de la V7 =>   GCH 06/08/2012 [BUG]#19060 Champs d'e-mail autorisé à la modification
                    bFieldNotAllowed = bFieldNotAllowed || ((fld.Table.EdnType == EdnType.FILE_MAIL || fld.Table.EdnType == EdnType.FILE_SMS) && (!eLibTools.IsMailAllowedReadField(fld.Descid)));

                    //if ( ((!bFormatNotAllowed && !bFieldNotAllowed) || bKeySector) && (!dicFields.ContainsKey(fld.Descid)) )
                    if ((!bFormatNotAllowed && !bFieldNotAllowed) && (!dicFields.ContainsKey(fld.Descid)))
                    {
                                             
                        switch (fld.Format)
                        {
                            #region TYP_CHAR, TYP_EMAIL, TYP_WEB, TYP_PHONE, TYP_SOCIALNETWORK
                            case FieldFormat.TYP_CHAR:
                            case FieldFormat.TYP_EMAIL:
                            case FieldFormat.TYP_WEB:
                            case FieldFormat.TYP_PHONE:
                            case FieldFormat.TYP_SOCIALNETWORK:

                                switch (fldSrc.Format)
                                {
                                    case FieldFormat.TYP_CHAR:
                                    case FieldFormat.TYP_EMAIL:
                                    case FieldFormat.TYP_WEB:
                                    case FieldFormat.TYP_PHONE:
                                    case FieldFormat.TYP_SOCIALNETWORK:
                                        bMatched = true;
                                        break;
                                    default:
                                        bMatched = false;
                                        break;
                                }
                                if (fld.Length > fldSrc.Length) bMatched = false;
                                break;
                            #endregion
                            #region TYP_DATE
                            case FieldFormat.TYP_DATE:
                                switch (fldSrc.Format)
                                {
                                    case FieldFormat.TYP_DATE:
                                        bMatched = true;
                                        break;
                                    default:
                                        bMatched = false;
                                        break;
                                }
                                break;
                            #endregion
                            #region TYP_GEOGRAPHY_V2
                            case FieldFormat.TYP_GEOGRAPHY_V2:
                                bMatched = fldSrc.Format == FieldFormat.TYP_GEOGRAPHY_V2;
                                break;
                            #endregion
                            #region TYP_BIT
                            case FieldFormat.TYP_BIT:
                                switch (fldSrc.Format)
                                {
                                    case FieldFormat.TYP_BIT:
                                        bMatched = true;
                                        break;
                                    default:
                                        bMatched = false;
                                        break;
                                }
                                break;
                            #endregion
                            #region TYP_MONEY, TYP_NUMERIC
                            case FieldFormat.TYP_MONEY:
                            case FieldFormat.TYP_NUMERIC:
                                switch (fldSrc.Format)
                                {
                                    case FieldFormat.TYP_MONEY:
                                    case FieldFormat.TYP_NUMERIC:
                                    case FieldFormat.TYP_CHAR:
                                        bMatched = true;
                                        break;
                                    default:
                                        bMatched = false;
                                        break;
                                }
                                break;
                            #endregion
                            #region TYP_USER
                            case FieldFormat.TYP_USER:
                                switch (fldSrc.Format)
                                {
                                    case FieldFormat.TYP_USER:
                                        if (fld.Multiple == fldSrc.Multiple)
                                            bMatched = true;
                                        else if (fldSrc.Multiple && !fld.Multiple)
                                            bMatched = true;
                                        break;
                                    default:
                                        bMatched = false;
                                        break;
                                }
                                break;
                            #endregion
                            default:
                                break;
                        }

                        if (fldSrc.Format == FieldFormat.TYP_CHAR && fldSrc.Popup == PopupType.DATA)
                        {

                            if (fld.Popup != PopupType.DATA)
                            {
                                bMatched = false;
                            }

                            if (!fldSrc.Multiple && fld.Multiple)
                            {
                                bMatched = false;
                            }

                           
                            bMatched = bMatched &&  (fld.PopupDescId == fldSrc.PopupDescId);

                            
                        }
                        else if (fldSrc.Popup == PopupType.SPECIAL)
                        {
                            bMatched = (fld.Popup == PopupType.SPECIAL && fld.Popup == fldSrc.Popup && fld.PopupDescId == fldSrc.PopupDescId);
                        }
                        else if (fld.Format == FieldFormat.TYP_CHAR && fld.Popup == PopupType.DATA && fldSrc.Popup != PopupType.DATA)
                        {
                            bMatched = false;
                        }

                      
                            
                        if (bMatched)
                            dicFields.Add(fld.Descid, fld.Libelle);
                    }
                }
            }

            return dicFields;
        }
    }

}