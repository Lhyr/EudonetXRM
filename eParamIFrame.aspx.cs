using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eParamIFrame</className>
    /// <summary>Classe de gestion du chargement et de modifications des param de l'applicationdans</summary>
    /// <purpose>Permet de charger, lire ou écrire les préférences de la fenêtre param</purpose>
    /// <authors>HLA</authors>
    /// <date>2011-09</date>
    public partial class eParamIFrame : eEudoPage 
    {
        /// <summary>préférences de l'utilisateur</summary>
        private eParam _param;

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
            //Thread.Sleep(20000);

            #region ajout des js



            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eNavBar");
            PageRegisters.AddScript("eParamIFrame");

            #endregion

            // Ouverture des param
            _param = new eParam(_pref);

            // Permier chargement des param
            if (Request.Form.Count == 0 && Request.QueryString.Count == 0)
            {
                try
                {
                    LoadBeginApp();
                }
                catch (eEndResponseException) { Response.End(); }
                catch (ThreadAbortException)
                {
                    //Ne catch pas le LaunchError/RenderResult
                }
                catch (Exception exp)
                {
                    //Avec exception
                    String sDevMsg = String.Concat("Erreur sur eParamIframe.aspx - Page_Load - Impossible de charger les paramètres");
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", exp.Message, Environment.NewLine, "Exception StackTrace :", exp.StackTrace);

                    //Echec du chargement initial des paramètres !
                    // Il s'agit d'un appel direct via une iframe
                    // Il faut appeller le LaunchErrorHTML(true)
                    // Erreur bloquante, renvoie au login
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 5008), "<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        sDevMsg
                        );


                    //KHA : en cas d'erreur dans la construction il faut tout de même avoir le feedback
                    if (_pref == null || _pref.User == null || _pref.User.UserId == 0)
                        ErrorContainer.IsSessionLost = true;

                    try
                    {
                        LaunchErrorHTML(true);
                    }
                    catch (eEndResponseException)
                    {

                    }
                }
            }
            else
            {
                try
                {
                    if (Request.QueryString["action"] == null)
                        throw new Exception("Action vide");

                    switch (Request.QueryString["action"])
                    {
                        case "RefreshMRUFile":
                            RefreshMruFile();
                            break;
                        case "RefreshMRUField":
                            RefreshMruField();
                            break;
                        case "RefreshUserMessage":
                            RefreshUserMessage();
                            break;
                        default:
                            throw new Exception("Action Invalide");

                    }
                }
                catch (eEndResponseException)
                {
                    Response.End();
                }
                catch (ThreadAbortException)
                {
                    //Ne catch pas le LaunchError/RenderResult
                }
                catch (Exception exp)
                {
                    //Avec exception
                    String sDevMsg = String.Concat("Erreur sur eParamIframe.aspx - Page_Load - Opération impossible action : >", Request.QueryString["action"], "<");
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", exp.Message, Environment.NewLine, "Exception StackTrace :", exp.StackTrace);

                    //Echec du chargement initial des paramètres !
                    // Il s'agit d'un appel direct via une iframe
                    // Il faut appeller le LaunchErrorHTML(true)
                    // Erreur bloquante, renvoie au login
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 422), Environment.NewLine, eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        sDevMsg
                        );

                    try
                    {
                        LaunchError();
                    }
                    catch (eEndResponseException)
                    { }
                }
            }
        }

        /// <summary>
        /// Recharge le message utilisateur
        /// </summary>
        private void RefreshUserMessage()
        {
            eRequestTools tools = new eRequestTools(Context);
            String userMessage = HttpUtility.HtmlDecode(tools.GetRequestFormKeyS("um"));

            // le nouveau userMessage en brut
            _pref.User.UserMessage = userMessage;

            if (String.IsNullOrEmpty(userMessage))
            {
                _param.addOrUpParam("UserMessage", String.Empty);

            }
            else
            {
                eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                try
                {
                    dal.OpenDatabase();
                    //on remplace les champs de fusions             
                    _pref.User.UserMessage = eLibTools.CalculUserMessage(dal, _pref.User);
                    //on traite l'avatar
                    eImageTools.ProcessAvatarMergeField(_pref);
                    _param.addOrUpParam("UserMessage", _pref.User.UserMessage);
                }
                catch (Exception ex)
                {
                    throw new Exception("Impossible de recharger le message utilisateur:" + ex.StackTrace);
                }
                finally
                {
                    if (dal != null)
                        dal.CloseDatabase();
                }
            }

            DoResponse();
        }

        /// <summary>
        /// Chargement au départ de l'application
        /// </summary>
        private void LoadBeginApp()
        {
            string error = string.Empty;
            string drawValue = string.Empty;

            if (!_param.LoadParam(out error))
                throw new Exception(error);

            HtmlInputControl inputControl;

            // Parcours des ParamGlobal
            foreach (KeyValuePair<string, string> keyValue in _param.ParamGlobal)
            {
                inputControl = new HtmlInputText();
                inputControl.ID = keyValue.Key;
                if (!String.IsNullOrEmpty(keyValue.Value))
                    inputControl.Value = keyValue.Value;

                this.GLOBAL.Controls.Add(inputControl);
            }

            inputControl = new HtmlInputText();
            inputControl.ID = "sBaseName";
            inputControl.Value = _pref.GetBaseName;
            this.GLOBAL.Controls.Add(inputControl);

            // Parcours des ParamGrid
            foreach (KeyValuePair<string, string> keyValue in _param.ParamGrid)
            {
                inputControl = new HtmlInputText();
                inputControl.ID = keyValue.Key;
                if (!String.IsNullOrEmpty(keyValue.Value))
                    inputControl.Value = keyValue.Value;

                this.GRIDS.Controls.Add(inputControl);
            }

            // Parcours des ParamMruTable
            Panel tabMruPnl;
            foreach (KeyValuePair<int, IDictionary<string, string>> keyTable in _param.ParamMruTable)
            {
                if (keyTable.Value.Count <= 0)
                    continue;

                tabMruPnl = new Panel();
                tabMruPnl.ID = String.Concat("TAB_MRU_", keyTable.Key);
                this.TABS.Controls.Add(tabMruPnl);

                foreach (KeyValuePair<string, string> keyParam in keyTable.Value)
                {
                    inputControl = new HtmlInputText();
                    inputControl.ID = keyParam.Key;
                    if (!String.IsNullOrEmpty(keyParam.Value))
                        inputControl.Value = keyParam.Value;

                    tabMruPnl.Controls.Add(inputControl);
                }
            }

            // Parcours des ParamMruField
            // traitement effectué à la demande

            // Parcours des fichiers mails existants
            foreach (KeyValuePair<int, string> kvp in _param.ParamEmailFiles)
            {
                inputControl = new HtmlInputText();
                inputControl.ID = String.Concat("MLF_", kvp.Key.ToString());
                inputControl.Value = kvp.Value;

                this.MLFiles.Controls.Add(inputControl);
            }

            // Parcours des fichiers SMS existants
            foreach (KeyValuePair<int, string> kvp in _param.ParamSMSFiles)
            {
                inputControl = new HtmlInputText();
                inputControl.ID = String.Concat("SMSF_", kvp.Key.ToString());
                inputControl.Value = kvp.Value;

                this.SMSFiles.Controls.Add(inputControl);
            }

            // Liste des utilisateurs à afficher par défaut en filtre rapides
            StringBuilder sUserList = new StringBuilder();
            foreach (eUser.UserListItem currentULI in _param.ParamQuickUserList)
            {
                if (sUserList.Length > 0)
                    sUserList.Append(SEPARATOR.MRU_LVL1);
                sUserList
                    .Append(currentULI.ItemCode)
                    .Append(SEPARATOR.MRU_LVL2)
                    .Append(Server.HtmlEncode(currentULI.Libelle));

            }

            inputControl = new HtmlInputText();
            inputControl.ID = "QckUsrLstInput";
            inputControl.Value = sUserList.Replace("\"", "&quote;").ToString();
            this.QckUsrLst.Controls.Add(inputControl);

            //Liste des descID qui sont sur la nouvelle ergo.
            inputControl = new HtmlInputText();
            inputControl.ID = "dvIrisBlackInput";
            inputControl.Value = string.Join(";", _param.ParamTabNelleErgo);
            this.dvIrisBlack.Controls.Add(inputControl);


            inputControl = new HtmlInputText();
            inputControl.ID = "dvIrisBlackInputPreview";
            inputControl.Value = string.Join(";", _param.ParamTabNelleErgoPreview);
            this.dvIrisBlack.Controls.Add(inputControl);
            

            //Liste des descID qui sont sur la liste de la nouvelle ergo.
            inputControl = new HtmlInputText();
            inputControl.ID = "dvIrisCrimsonInput";
            inputControl.Value = string.Join(";", _param.ParamTabNelleErgoList);
            this.dvIrisCrimson.Controls.Add(inputControl);

            //Liste des descID qui sont le mode téléguidé avec la nouvelle ergonomie.
            inputControl = new HtmlInputText();
            inputControl.ID = "dvIrisPurpleInput";
            inputControl.Value = string.Join(";", _param.ParamTabNelleErgoGuided);
            this.dvIrisPurple.Controls.Add(inputControl);
        }

        /// <summary>
        /// Met à jour les paramètre d'un ou plusieurs onglets
        /// </summary>
        private void RefreshMruFile()
        {
            String error = String.Empty;

            if (!_allKeys.Contains("newTabs"))
                throw new Exception("Paramètre NewTabs introuvable.");

            // Listes des nouveaux onglets
            String newTabOrder = Request.Form["newTabs"].ToString();
            newTabOrder = eLibTools.CleanIdList(newTabOrder);

            if (newTabOrder.Length == 0)
                throw new Exception("Chargement des MRU impossible. La liste des descid de table est vide.");

            _param.LoadTableMru(null, newTabOrder, out error);
            if (error.Length > 0)
                throw new Exception(String.Concat("Chargement des MRU de table impossible. Description : ", error));

            DoResponse();
        }

        /// <summary>
        /// Chargement des MRU du field demandé
        /// </summary>
        private void RefreshMruField()
        {
            String error = String.Empty;

            if (!_allKeys.Contains("descId"))
                throw new Exception("Paramètre DescId introuvable.");

            String lstDescId = Request.Form["descId"];
            if (lstDescId.Length == 0)
                throw new Exception("Chargement des MRU impossible. La liste des descid de field est vide.");

            _param.LoadFieldMru(null, lstDescId, out error);
            if (error.Length > 0)
                throw new Exception(String.Concat("Chargement des MRU de field impossible. Description : ", error));

            DoResponse();
        }

        private void DoResponse()
        {
            XmlAttribute resultAttribute = null;

            XmlDocument xmlResult = new XmlDocument();
            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(mainNode);

            XmlNode rootNode = xmlResult.CreateElement("root");
            xmlResult.AppendChild(rootNode);

            XmlNode globalNode = xmlResult.CreateElement("Global");
            rootNode.AppendChild(globalNode);
            XmlNode globalGrid = xmlResult.CreateElement("Grid");
            rootNode.AppendChild(globalGrid);
            XmlNode mruTabNode = xmlResult.CreateElement("MruTab");
            rootNode.AppendChild(mruTabNode);
            XmlNode mruNode = xmlResult.CreateElement("MruField");
            rootNode.AppendChild(mruNode);

            #region Parcours des ParamGlobal à mettre à jour

            XmlNode resultNode = null;

            foreach (KeyValuePair<String, String> keyValue in _param.ParamGlobal)
            {
                resultNode = xmlResult.CreateElement("param");

                resultAttribute = xmlResult.CreateAttribute("name");
                resultAttribute.Value = keyValue.Key;
                resultNode.Attributes.Append(resultAttribute);

                resultAttribute = xmlResult.CreateAttribute("value");
                resultAttribute.Value = keyValue.Value;
                resultNode.Attributes.Append(resultAttribute);

                globalNode.AppendChild(resultNode);
            }

            #endregion
            
            #region Parcours des ParamGrid à mettre à jour

            resultNode = null;
            foreach (KeyValuePair<String, String> keyValue in _param.ParamGrid)
            {
                resultNode = xmlResult.CreateElement("param");

                resultAttribute = xmlResult.CreateAttribute("name");
                resultAttribute.Value = keyValue.Key;
                resultNode.Attributes.Append(resultAttribute);

                resultAttribute = xmlResult.CreateAttribute("value");
                resultAttribute.Value = keyValue.Value;
                resultNode.Attributes.Append(resultAttribute);

                globalGrid.AppendChild(resultNode);
            }

            #endregion

            #region Parcours des ParamMruTable

            XmlNode tabNode = null;
            XmlNode resultChildNode = null;

            foreach (KeyValuePair<int, IDictionary<string, string>> keyTable in _param.ParamMruTable)
            {
                if (keyTable.Value.Count <= 0)
                    continue;

                resultAttribute = xmlResult.CreateAttribute("id");
                resultAttribute.Value = String.Concat("TAB_MRU_", keyTable.Key.ToString());

                tabNode = xmlResult.CreateElement("tab");
                tabNode.Attributes.Append(resultAttribute);

                foreach (KeyValuePair<String, String> keyParam in keyTable.Value)
                {
                    resultChildNode = xmlResult.CreateElement("param");

                    resultAttribute = xmlResult.CreateAttribute("name");
                    resultAttribute.Value = keyParam.Key;
                    resultChildNode.Attributes.Append(resultAttribute);

                    if (!String.IsNullOrEmpty(keyParam.Value))
                    {
                        resultAttribute = xmlResult.CreateAttribute("value");

                        //Le encode provient de eParam.getMruTabs et n'est nécessaire que pour le chargement initial
                        // pour le chargement unitaire d'un onglet de mru, il faut retirer ce encode
                        resultAttribute.Value = eLibTools.CleanXMLChar(System.Web.HttpUtility.HtmlDecode(keyParam.Value));

                        resultChildNode.Attributes.Append(resultAttribute);
                    }

                    tabNode.AppendChild(resultChildNode);
                }

                mruTabNode.AppendChild(tabNode);
            }

            #endregion

            #region Parcours des ParamMruField

            XmlNode fieldNode = null;

            foreach (KeyValuePair<Int32, eParam.ParamMruFieldItem> keyValue in _param.ParamMruField)
            {
                eParam.ParamMruFieldItem item = keyValue.Value;

                fieldNode = xmlResult.CreateElement("field");

                // Id
                resultAttribute = xmlResult.CreateAttribute("descId");
                resultAttribute.Value = keyValue.Key.ToString();
                fieldNode.Attributes.Append(resultAttribute);

                // Infos
                foreach (KeyValuePair<String, String> itemAttributes in item.GetList())
                {
                    resultAttribute = xmlResult.CreateAttribute(itemAttributes.Key);
                    resultAttribute.Value = itemAttributes.Value;
                    fieldNode.Attributes.Append(resultAttribute);
                }

                // Valeurs
                resultAttribute = xmlResult.CreateAttribute("value");
                resultAttribute.Value = item.Values;
                fieldNode.Attributes.Append(resultAttribute);

                mruNode.AppendChild(fieldNode);
            }

            #endregion

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }
    }
}
