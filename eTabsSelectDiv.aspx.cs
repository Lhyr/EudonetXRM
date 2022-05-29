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
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Fenêtre de sélection de table pour onglets/sélection
    /// </summary>
    public partial class eTabsSelectDiv : eEudoPage
    {

        /// <summary>
        /// Nom js de la modal hébergeant cette page
        /// </summary>
        public String _modalName = String.Empty;

        /// <summary>
        /// JS a injecter
        /// </summary>
        public String _sJsRes = String.Empty;

        class TabSelectProperties
        {
            public String Libelle { get; set; }
            public Boolean Selected { get; set; }
        }

        private Dictionary<Int32, TabSelectProperties> _tabs = null;

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

            #region Variables de session

            String lang = _pref.Lang;
            Int32 userId = _pref.User.UserId;
            String instance = _pref.GetSqlInstance;
            String baseName = _pref.GetBaseName;

            #endregion

            String action = "";
            if (Request.Form["action"] != null)
                action = Request.Form["action"].ToString();

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            try
            {
                String err = string.Empty;
                String errUser = String.Empty;
                dal.OpenDatabase();

                switch (action)
                {
                    case "loadselection":
                        int selId = 0;
                        if (Request.Form["selid"] != null)
                        {
                            if (!Int32.TryParse(Request.Form["selid"].ToString(), out selId))
                                Response.End();
                        }

                        RqParam rq = new RqParam("Select TabOrder From [selections] where SelectId=@SelectId");
                        rq.AddInputParameter("@SelectId", SqlDbType.Int, selId);

                        err = string.Empty;
                        DataTableReaderTuned dtr = dal.Execute(rq, out err);
                        try
                        {
                            if (dtr.Read())
                            {
                                Response.Clear();
                                Response.ClearContent();

                                LoadTabs(dal, dtr.GetString("TabOrder"));

                                Control div1;
                                StringBuilder sb1 = new StringBuilder();

                                //Source
                                div1 = FillSourceList();
                                div1.RenderControl(new HtmlTextWriter(new StringWriter(sb1)));
                                Response.Write(sb1.ToString());
                                Response.Write("$$!!$$");

                                Control div2;
                                StringBuilder sb2 = new StringBuilder();
                                div2 = FillSelectedList(dtr.GetString("TabOrder"));
                                div2.RenderControl(new HtmlTextWriter(new StringWriter(sb2)));

                                Response.Write(sb2.ToString());
                            }
                        }
                        finally
                        {
                            if (dtr != null)
                                dtr.Dispose();
                        }

                        if (err.Length > 0 || errUser.Length > 0)
                        {
                            if (errUser.Length < 0)
                                errUser = String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544));
                            if (err.Length > 0)
                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, errUser, String.Empty, eResApp.GetRes(_pref, 422), err));
                            else
                                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, errUser, String.Empty));
                        }
                        else
                            Response.End();
                        break;

                    case "createempty":
                        Int32 _NewId = 0;
                        String _selectionName = String.Empty;
                        if (Request.Form["selectionname"] != null)
                        {
                            _selectionName = Request.Form["selectionname"].ToString();
                        }

                        err = String.Empty;

                        //Teste si le nom existe ou pas

                        rq = new RqParam("select count(selectid)  From [Selections] where userid=@userid and label=@label and tab=0 ");

                        rq.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);
                        rq.AddInputParameter("@label", SqlDbType.VarChar, _selectionName);


                        DataTableReaderTuned _dtrlbl = dal.Execute(rq, out err);
                        try
                        {
                            if (String.IsNullOrEmpty(err))
                            {
                                if (_dtrlbl.Read() && _dtrlbl.GetEudoNumeric(0) > 0)
                                {
                                    errUser = eResApp.GetRes(_pref, 1647);
                                }
                                else
                                {
                                    rq = new RqParam("Insert Into [Selections]([tab],[userid],[label]) values(0,@userid,@label);  SELECT NewID = SCOPE_IDENTITY ()");
                                    rq.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);
                                    rq.AddInputParameter("@label", SqlDbType.VarChar, _selectionName);
                                    DataTableReaderTuned _dtrNewId = dal.Execute(rq, out err);
                                    if (String.IsNullOrEmpty(err) && _dtrNewId.Read())
                                    {
                                        _NewId = _dtrNewId.GetEudoNumeric(0);
                                    }

                                }
                            }
                        }
                        finally
                        {
                            if (_dtrlbl != null)
                                _dtrlbl.Dispose();
                        }

                        XmlDocument xmlResult = new XmlDocument();
                        XmlNode maintNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                        xmlResult.AppendChild(maintNode);
                        XmlNode detailsNode = xmlResult.CreateElement("result");
                        xmlResult.AppendChild(detailsNode);


                        XmlNode _successNode = xmlResult.CreateElement("success");
                        detailsNode.AppendChild(_successNode);




                        if (err.Length > 0 || errUser.Length > 0)
                        {
                            if (errUser.Length < 0)
                                errUser = String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544));
                            if (err.Length > 0)
                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, errUser, String.Empty, eResApp.GetRes(_pref, 422), err));
                            else
                                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, errUser, String.Empty));
                        }
                        else
                        {
                            _successNode.InnerText = "1";

                            XmlNode _newIdNode = xmlResult.CreateElement("newid");
                            detailsNode.AppendChild(_newIdNode);
                            _newIdNode.InnerText = _NewId.ToString();

                            XmlNode _newNameNode = xmlResult.CreateElement("newname");
                            detailsNode.AppendChild(_newNameNode);
                            _newNameNode.InnerText = _selectionName;


                            XmlNode _newSelNode = xmlResult.CreateElement("newselectionlist");
                            detailsNode.AppendChild(_newSelNode);
                            Control _newsel = FillSelectionList(dal, _NewId);

                            StringBuilder sb1 = new StringBuilder();


                            _newsel.RenderControl(new HtmlTextWriter(new StringWriter(sb1)));

                            _newSelNode.InnerText = sb1.ToString();

                        }
                        RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });

                        break;
                    case "delete":



                        Int32 tabOrderId = 0;
                        if (Request.Form["selid"] != null)
                        {
                            tabOrderId = eLibTools.GetNum(Request.Form["selid"].ToString());
                        }

                        err = String.Empty;

                        //Teste si c'est la seule sélection ou pas

                        rq = new RqParam("select count(selectid) from selections where userid=@userid and tab = 0");
                        rq.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);

                        int _newSelId = 0;
                        int nbr_ = 0;
                        dtr = dal.Execute(rq, out err);
                        try
                        {
                            dtr.Read();

                            _newSelId = 0;
                            nbr_ = dtr.GetEudoNumeric(0);

                            if (dtr.GetEudoNumeric(0) <= 1)
                            {
                                //Impossible de supprimer la dernière sélection
                                errUser = eResApp.GetRes(_pref, 90);
                            }
                            else
                            {
                                rq = new RqParam("delete from [Selections] where userid=@userid and selectId = @selectid");
                                rq.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);
                                rq.AddInputParameter("@selectid", SqlDbType.VarChar, tabOrderId);
                                dal.ExecuteNonQuery(rq, out err);

                                if (String.IsNullOrEmpty(err))
                                {
                                    //Affectation d'une nouvelle sélection (première dans la liste)
                                    rq = new RqParam("select top 1 selectId from selections where userid=@userid and tab = 0");
                                    rq.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);
                                    dtr = dal.Execute(rq, out err);
                                    dtr.Read();

                                    _newSelId = dtr.GetEudoNumeric(0);
                                    nbr_--;

                                    //Enregistrement dans config
                                    _pref.SetConfigScalar(eLibConst.PREF_CONFIG.TABORDERID, _newSelId.ToString());

                                    _pref.LoadTabs();

                                }
                            }
                        }
                        finally
                        {
                            if (dtr != null)
                                dtr.Dispose();
                        }

                        xmlResult = new XmlDocument();
                        maintNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                        xmlResult.AppendChild(maintNode);
                        detailsNode = xmlResult.CreateElement("result");
                        xmlResult.AppendChild(detailsNode);


                        _successNode = xmlResult.CreateElement("success");
                        detailsNode.AppendChild(_successNode);


                        if (err.Length > 0 || errUser.Length > 0)
                        {
                            if (errUser.Length < 0)
                                errUser = String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544));
                            if (err.Length > 0)
                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, errUser, String.Empty, eResApp.GetRes(_pref, 422), err));
                            else
                                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, errUser, String.Empty));
                        }
                        else
                        {
                            _successNode.InnerText = "1";

                            XmlNode _seliddNode = xmlResult.CreateElement("selid");
                            _seliddNode.InnerText = tabOrderId.ToString();
                            detailsNode.AppendChild(_seliddNode);


                            XmlNode _newseliddNode = xmlResult.CreateElement("newselid");
                            _newseliddNode.InnerText = _newSelId.ToString();
                            detailsNode.AppendChild(_newseliddNode);

                            XmlNode _nbrNode = xmlResult.CreateElement("nbr");
                            _nbrNode.InnerText = nbr_.ToString();
                            detailsNode.AppendChild(_nbrNode);


                        }

                        RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });


                        break;

                    case "rename":


                        tabOrderId = 0;
                        String selName = String.Empty;
                        if (Request.Form["selid"] != null)
                            tabOrderId = eLibTools.GetNum(Request.Form["selid"].ToString());

                        if (Request.Form["selname"] != null)
                            selName = Request.Form["selname"].ToString();

                        errUser = String.Empty;
                        err = String.Empty;

                        //Teste si c'est le nom existe  

                        rq = new RqParam("select count(*) from selections where userid=@userid and tab = 0 and label=@selname");
                        rq.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);
                        rq.AddInputParameter("@selname", SqlDbType.VarChar, selName);

                        dtr = dal.Execute(rq, out err);
                        try
                        {
                            dtr.Read();

                            if (dtr.GetEudoNumeric(0) >= 1)
                            {
                                //Impossible d'enregistrer - Le nom existe
                                errUser = string.Concat(eResApp.GetRes(_pref, 91), ". ", eResApp.GetRes(_pref, 1647));
                            }
                            else
                            {
                                rq = new RqParam("update [Selections] set label = @selname where userid=@userid and selectId = @selectid");
                                rq.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);
                                rq.AddInputParameter("@selectid", SqlDbType.Int, tabOrderId);
                                rq.AddInputParameter("@selname", SqlDbType.VarChar, selName);
                                dal.ExecuteNonQuery(rq, out err);

                                _pref.LoadTabs();
                            }
                        }
                        finally
                        {
                            if (dtr != null)
                                dtr.Dispose();
                        }

                        xmlResult = new XmlDocument();
                        maintNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                        xmlResult.AppendChild(maintNode);
                        detailsNode = xmlResult.CreateElement("result");
                        xmlResult.AppendChild(detailsNode);


                        _successNode = xmlResult.CreateElement("success");
                        detailsNode.AppendChild(_successNode);




                        if (err.Length > 0 || errUser.Length > 0)
                        {
                            if (errUser.Length < 0)
                                errUser = String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544));
                            if (err.Length > 0)
                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, errUser, String.Empty, eResApp.GetRes(_pref, 422), err));
                            else
                                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, errUser, String.Empty));
                        }
                        else
                        {
                            _successNode.InnerText = "1";

                            XmlNode _seliddNode = xmlResult.CreateElement("selid");
                            _seliddNode.InnerText = tabOrderId.ToString();
                            detailsNode.AppendChild(_seliddNode);


                            XmlNode _selnamedNode = xmlResult.CreateElement("newselname");
                            _selnamedNode.InnerText = selName;
                            detailsNode.AppendChild(_selnamedNode);

                        }

                        RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });

                        break;
                    default:
                        String _tabList = String.Empty;
                        tabOrderId = 0;
                        if (Request.Form["selid"] != null)
                        {
                            tabOrderId = eLibTools.GetNum(Request.Form["selid"].ToString());
                        }
                        else
                        {
                            tabOrderId = eLibTools.GetNum(_pref.GetConfig(eLibConst.PREF_CONFIG.TABORDERID));
                            _tabList = _pref.GetTabs(ePrefConst.PREF_SELECTION_TAB.TABORDER);
                        }

                        DivSelectionListResult.Controls.Add(FillSelectionList(dal, tabOrderId));
                        LoadTabs(dal, _tabList);
                        TdSourceList.Controls.Add(FillSourceList());
                        TdTargetList.Controls.Add(FillSelectedList(_tabList));
                        if (Request.Form["modalname"] != null)
                            _modalName = Request.Form["modalname"].ToString();
                        break;

                }
            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
            catch (Exception ex)
            {
                LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            String.Concat("Erreur non gérée eTabsSelectDiv.aspx : ", Environment.NewLine, ex.ToString())));
            }
            finally
            {
                dal.CloseDatabase();
            }
        }

        private void LoadTabs(eudoDAL dal, String tabList)
        {
            try
            {
                string error = String.Empty;

                StringBuilder sqlTabs = new StringBuilder();
                sqlTabs.Append("SELECT [desc].[descid] ")
                    .Append("FROM [desc] left join [res] on [desc].[descid] = [res].[resid] ")
                    .Append("WHERE (isnull([desc].[activetab], 0) <> 0 ")
                    .Append("   AND (([desc].[descid] >= 1000 AND [desc].[descid] like '%00') OR [desc].[descid] in (100,200,300, 400,102000,116000)))   ");

                ISet<int> allTabs = new HashSet<int>();
                DataTableReaderTuned dtr = dal.Execute(new RqParam(sqlTabs.ToString()), out error);
                try
                {
                    if (error.Length != 0 || dtr == null)
                        throw new Exception(String.Concat("eTabsSelect - LoadTabs", error));

                    while (dtr.Read())
                        allTabs.Add(dtr.GetEudoNumeric(0));

                }
                finally
                {
                    dtr?.Dispose();
                }

                // Vérif des droits de visu, recup le libelle, indique si la table est selectionnée
                TabSelectProperties properties = null;
                string tabListContains = String.Concat(";", tabList, ";");



                //st.Stop();
                //long t2 = st.ElapsedMilliseconds;
                using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(_pref.User.UserId, _pref.User.UserLevel, _pref.User.UserGroupId, _pref.User.UserLang, allTabs, dal))
                {
                    _tabs = new Dictionary<Int32, TabSelectProperties>();

                    int tabDescId;
                    while (dtrRights.Read())
                    {
                        tabDescId = dtrRights.GetEudoNumeric("DescId");
                        if (tabDescId == 0)
                            continue;

                        if (!eLibDataTools.GetTabViewRight(dtrRights))
                            continue;

                        if (
                            tabDescId == (int)TableType.NOTIFICATION
                            || tabDescId == (int)TableType.NOTIFICATION_TRIGGER
                            || tabDescId == (int)TableType.XRMGRID
                            || tabDescId == (int)TableType.XRMWIDGET
                            )
                            continue;

                        if (tabDescId == (int)TableType.XRMPRODUCT
                            && !eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.AdminProduct))
                            continue;

                        properties = new TabSelectProperties();
                        properties.Libelle = dtrRights.GetString("libelle");
                        properties.Selected = tabListContains.Contains(String.Concat(";", tabDescId, ";"));
                        _tabs.Add(tabDescId, properties);
                    }
                }

            }
            catch (eEndResponseException) { Response.End(); return; }
            catch (ThreadAbortException) { return; }    // Laisse passer le response.end du RenderResult
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private HtmlGenericControl FillSelectedList(String tabList)
        {
            try
            {
                String error = String.Empty;
                HtmlGenericControl lst = new HtmlGenericControl("div");
                lst.ID = "TabSelectedList";
                lst.Attributes.Add("class", "ItemList");
                lst.Attributes.Add("onclick", "doInitSearch(this, event);");
                lst.Attributes.Add("onmouseup", "doOnMouseUp(this);");

                String optCss = "cell";
                HtmlGenericControl itm = null;
                Int32 tabDescId = 0;
                TabSelectProperties properties;
                String[] tabs = tabList.Split(";");
                foreach (String tab in tabs)
                {
                    tabDescId = eLibTools.GetNum(tab);
                    if (tabDescId == 0)
                        continue;

                    if (!_tabs.TryGetValue(tabDescId, out properties))
                        continue;

                    itm = new HtmlGenericControl("div");
                    if (optCss.Equals("cell"))
                        optCss = "cell2";
                    else
                        optCss = "cell";
                    itm.Attributes.Add("class", optCss);
                    itm.Attributes.Add("oldCss", optCss);

                    itm.Attributes.Add("DescId", tabDescId.ToString());
                    itm.Attributes.Add("onclick", "setElementSelected(this);");
                    itm.InnerHtml = HttpUtility.HtmlEncode(properties.Libelle);
                    itm.ID = "descId_" + tabDescId.ToString();



                    itm.Attributes.Add("onmouseover", "doOnMouseOver(this);");
                    //_itm.Attributes.Add("onmouseup", "doOnMouseUp();");
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
            catch (eEndResponseException) { Response.End(); return null; }
            catch (ThreadAbortException) { return null; }    // Laisse passer le response.end du RenderResult
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private HtmlGenericControl FillSourceList()
        {
            try
            {
                String error = String.Empty;
                HtmlGenericControl lst = new HtmlGenericControl("div");
                lst.ID = "AllTabList";
                lst.Attributes.Add("class", "ItemList");
                lst.Attributes.Add("onclick", "doInitSearch(this, event);");

                String optCss = "cell";
                HtmlGenericControl itm = null;
                foreach (KeyValuePair<Int32, TabSelectProperties> keyValue in _tabs)
                {
                    if (keyValue.Value.Selected)
                        continue;

                    itm = new HtmlGenericControl("div");
                    if (optCss.Equals("cell"))
                        optCss = "cell2";
                    else
                        optCss = "cell";
                    itm.Attributes.Add("class", optCss);
                    itm.Attributes.Add("oldCss", optCss);

                    itm.Attributes.Add("DescId", keyValue.Key.ToString());
                    itm.Attributes.Add("onclick", "setElementSelected(this);");
                    itm.InnerHtml = HttpUtility.HtmlEncode(keyValue.Value.Libelle);
                    itm.ID = "descId_" + keyValue.Key.ToString();

                    itm.Attributes.Add("onmouseover", "doOnMouseOver(this);");
                    //_itm.Attributes.Add("onmouseup", "doOnMouseUp();");
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
            catch (eEndResponseException) { Response.End(); return null; }
            catch (ThreadAbortException) { return null; }    // Laisse passer le response.end du RenderResult
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Création de la table contenant la liste des sélections
        /// </summary>
        /// <param name="dal">Objet d'accès aux données. La bdd doit etre ouverte</param>
        /// <param name="userSelectId">Id de la sélection a surligné</param>
        /// <returns></returns>
        private Control FillSelectionList(eudoDAL dal, int userSelectId)
        {
            try
            {
                HtmlGenericControl oDiv = new HtmlGenericControl("table");
                oDiv.ID = "DivSelectionListResultList";
                oDiv.Attributes.Add("SelectedSel", userSelectId.ToString());
                //oDiv.Attributes.Add("class", "SelectionList");
                oDiv.Attributes.Add("cellspacing", "0");
                oDiv.Attributes.Add("cellpadding", "0");
                oDiv.Attributes.Add("width", "100%");


                String _sqlSel = String.Concat("select [SelectId], [Label] from [Selections] where [UserId] = "
                                , _pref.User.UserId
                                , " and [Tab] = 0 order by [label]");

                String _err = String.Empty;
                int _inbr_dtr = 0;
                DataTableReaderTuned _dtr = dal.Execute(new RqParam(_sqlSel), out _inbr_dtr, out _err);
                try
                {
                    if (!string.IsNullOrEmpty(_err))
                    {
                        throw new Exception("FillSelectionList : " + _err);
                    }

                    String _class = "cell";
                    int i = 0;
                    if (_dtr != null)
                    {
                        while (_dtr.Read())
                        {
                            Int32 _selectId = _dtr.GetEudoNumeric(0);

                            String _lbl = _dtr.GetString(1);

                            HtmlGenericControl oTr = new HtmlGenericControl("tr");
                            oTr.ID = "tr_sel_" + _selectId;
                            oDiv.Controls.Add(oTr);

                            HtmlGenericControl oTd = new HtmlGenericControl("td");
                            oTd.ID = "td_sel_" + _selectId;
                            oTd.Attributes.Add("width", "80%");
                            oTd.Attributes.Add("height", "21px");
                            oTr.Controls.Add(oTd);

                            if (i % 2 == 0)
                            {

                                if (userSelectId == _selectId)
                                    _class = "SelectedItem";
                                else
                                    _class = "cell2";
                            }
                            else
                            {
                                if (userSelectId == _selectId)
                                    _class = "SelectedItem";
                                else
                                    _class = "cell";
                            }

                            oTr.Attributes.Add("class", _class);
                            oTr.Attributes.Add("oldCss", _class);

                            if (userSelectId == _selectId)
                            {
                                _class += " selected";
                                oTr.Attributes.Add("selected", "1");
                                oTr.Attributes.Add("name", "selected");
                                //   _class = "SelectedItem";
                            }


                            oTd.Attributes.Add("selid", _selectId.ToString());
                            oTd.Attributes.Add("onclick", "doOnSelectionclick('" + _selectId + "');");
                            oTd.InnerHtml = HttpUtility.HtmlEncode(_lbl);

                            HtmlGenericControl oTd2 = new HtmlGenericControl("td");
                            oTd2.Attributes.Add("height", "21px");
                            oTr.Controls.Add(oTd2);

                            var oDivBtn = new HtmlGenericControl("div");
                            oDivBtn.Attributes.Add("class", "ViewsEditBtn");
                            oTd2.Controls.Add(oDivBtn);

                            HtmlGenericControl oUl = new HtmlGenericControl("ul");
                            oDivBtn.Controls.Add(oUl);
                            oUl.Attributes.Add("class", "fldBtnUl");

                            HtmlGenericControl oLi = new HtmlGenericControl("li");
                            oUl.Controls.Add(oLi);
                            oLi.Attributes.Add("class", "icon-edn-pen");
                            oLi.Attributes.Add("onclick", "RenView('" + _selectId + "',this);");
                            oLi.Attributes.Add("title", eResApp.GetRes(_pref, 151));



                            if (_inbr_dtr > 1)
                            {
                                oLi = new HtmlGenericControl("li");
                                oLi.ID = string.Concat("DelView_", _selectId);
                                oUl.Controls.Add(oLi);

                                oLi.Attributes.Add("class", "icon-delete");
                                oLi.Attributes.Add("onclick", "cfmDelView('" + _selectId + "','" + _lbl.Replace("'", @"\'") + "');");
                                oLi.Attributes.Add("title", eResApp.GetRes(_pref, 19));


                            }

                            i++;
                        }
                    }
                }
                finally
                {
                    _dtr?.Dispose();
                }
                return oDiv;
            }
            catch (eEndResponseException) { Response.End(); return null; }
            catch (ThreadAbortException) { return null; }    // Laisse passer le response.end du RenderResult
            catch
            {
                throw;
            }
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
    }
}