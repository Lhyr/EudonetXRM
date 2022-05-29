using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminUsersBelongingRenderer : eAdminRenderer
    {
        int _nTableH;
        int _nColWidth;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminUsersBelongingRenderer(ePref pref, Int32 nTab, int nTableHeight, int nColsWidth)
        {
            Pref = pref;
            _tab = nTab;
            _nTableH = nTableHeight;
            _nColWidth = nColsWidth;
        }

        public static eAdminUsersBelongingRenderer CreateAdminUsersBelongingRenderer(ePref pref, Int32 nTab, int nTableHeight, int nColsWidth)
        {
            return new eAdminUsersBelongingRenderer(pref, nTab, nTableHeight, nColsWidth);
        }

        protected override bool Build()
        {
            if (base.Build())
            {
                

                eudoDAL eDal = eLibTools.GetEudoDAL(Pref);

                try
                {
                    eDal.OpenDatabase();
                    List<UserBelonging> list = eAdminBelonging.Load(eDal, _tab);

                    if (list.Count == 0)
                    {
                        HtmlGenericControl message = new HtmlGenericControl("p");
                        message.InnerText = "Aucune donnée trouvée.";
                        PgContainer.Controls.Add(message);
                    }
                    else
                    {
                        PgContainer.Controls.Add(CreateTable(list));
                    }
                    
                }
                catch (Exception e)
                {
                   
                }
                finally
                {
                    if (eDal != null)
                        eDal.CloseDatabase();
                }

                

                

                return true;
            }
            return false;
        }


        private HtmlGenericControl CreateTable(List<UserBelonging> list)
        {
            TextBox txtUser;
            int lineNum = 1;
            List<String> listHeaders = new List<String> { eResApp.GetRes(Pref, 195), eResApp.GetRes(Pref, 7416), eResApp.GetRes(Pref, 7402) };

            HtmlGenericControl htmlTable = new HtmlGenericControl("table");
            HtmlGenericControl sectionThead = new HtmlGenericControl("thead");
            HtmlGenericControl sectionTbody = new HtmlGenericControl("tbody");
            HtmlGenericControl tableTr = new HtmlGenericControl("tr");
            HtmlGenericControl tableTh = new HtmlGenericControl("th");
            HtmlGenericControl tableTd = new HtmlGenericControl("td");

            htmlTable.ID = "tableBelongings";
            htmlTable.Attributes.Add("class", "mTab tableWithFixedHeader");

            #region HEADER
            htmlTable.Controls.Add(sectionThead);

            sectionThead.Controls.Add(tableTr);

            foreach (String headerCol in listHeaders)
            {
                tableTh = new HtmlGenericControl("th");
                tableTh.Attributes.Add("class","hdBgCol");
                tableTh.Style.Add("min-width", _nColWidth + "px");
                tableTh.InnerText = headerCol;
                tableTr.Controls.Add(tableTh);
            }
            #endregion

            #region BODY

            htmlTable.Controls.Add(sectionTbody);

            sectionTbody.Style.Add("height", _nTableH + "px");

            String sLevelLabel;

            foreach (UserBelonging u in list)
            {
                sLevelLabel = eAdminTools.GetUserLevelLabel(Pref, u.UserLevel.ToString());

                tableTr = new HtmlGenericControl("tr");
                tableTr.Attributes.Add("class", String.Concat("line", lineNum));
                tableTr.Attributes.Add("data-userid", u.UserId.ToString());
                tableTr.Attributes.Add("data-userdisplayname", u.UserDisplayName);
                tableTr.Attributes.Add("data-userlogin", u.UserLogin);
                tableTr.Attributes.Add("data-username", u.UserName);
                tableTr.Attributes.Add("data-userlevel", sLevelLabel);
                tableTr.Attributes.Add("title", u.UserLogin);
                lineNum = (lineNum == 1) ? 2 : 1;

                tableTd = new HtmlGenericControl("td");
                tableTd.Style.Add("min-width", _nColWidth + "px");
                tableTd.InnerText = u.UserDisplayName;
                tableTr.Controls.Add(tableTd);

                tableTd = new HtmlGenericControl("td");
                tableTd.Style.Add("min-width", _nColWidth + "px");
                tableTd.InnerText = sLevelLabel;
                tableTr.Controls.Add(tableTd);

                tableTd = new HtmlGenericControl("td");
                tableTd.Style.Add("min-width", _nColWidth + "px");
                txtUser = new TextBox();
                txtUser.ReadOnly = true;
                txtUser.Text = (u.DefaultOwner == 0) ? String.Concat("<",eResApp.GetRes(Pref, 53),">") : u.DefaultOwnerName;
                txtUser.ID = String.Concat("txtOwner", u.UserId);
                txtUser.CssClass = "txtOwner";
                txtUser.Attributes.Add("data-userid", u.UserId.ToString());
                txtUser.Attributes.Add("dbv", u.DefaultOwner.ToString());
                txtUser.Attributes.Add("did", _tab.ToString());
                txtUser.Attributes.Add("onclick", "nsAdminField.openUserCat(null, this, this, false)");
                tableTd.Controls.Add(txtUser);
                tableTr.Controls.Add(tableTd);

                sectionTbody.Controls.Add(tableTr);
            }
            #endregion

           
            return htmlTable;
        }
    }

    
}