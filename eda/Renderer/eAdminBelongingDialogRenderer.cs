using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminBelongingDialogRenderer : eAdminRenderer
    {
        int _nTableHeight;
        int _nColsWidth;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminBelongingDialogRenderer(ePref pref, Int32 nTab, int nWidth, int nHeight) 
        {
            Pref = pref;
            _tab = nTab;
            _nTableHeight = nHeight - 170;
            _nColsWidth = (nWidth - 100) / 3;
        }

        public static eAdminBelongingDialogRenderer CreateAdminBelongingDialogRenderer(ePref pref, Int32 nTab, int nWidth, int nHeight)
        {
            return new eAdminBelongingDialogRenderer(pref, nTab, nWidth, nHeight);
        }

        protected override bool Build()
        {
            if (base.Build())
            {
                // Champ caché
                this._divHidden = new HtmlGenericControl();
                HtmlInputHidden hidden = new HtmlInputHidden();
                hidden.ID = "hidPref";
                hidden.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.DEFAULTOWNER.GetHashCode()));
                this._divHidden.Controls.Add(hidden);
                PgContainer.Controls.Add(_divHidden);

                #region Panel haut
                Panel pnlHeader = new Panel();
                pnlHeader.ID = "pnlHeader";
                
                eAdminButtonField field = new eAdminButtonField(eResApp.GetRes(Pref, 7402), "btnDefaultValues", eResApp.GetRes(Pref, 7404), "nsAdminBelonging.setDefaultValues()");
                field.Generate(pnlHeader);

                field = new eAdminButtonField(eResApp.GetRes(Pref, 7403), "btnPublicFiles", eResApp.GetRes(Pref, 7405), "nsAdminBelonging.setAllPublic()");
                field.Generate(pnlHeader);

                Panel searchBar = eAdminTools.CreateSearchBar("eFSContainerBelongings", "tableBelongings");
                pnlHeader.Controls.Add(searchBar);
                #endregion

                eAdminRenderer rdr = eAdminUsersBelongingRenderer.CreateAdminUsersBelongingRenderer(Pref, _tab, _nTableHeight, _nColsWidth);
                rdr.Generate();

                PgContainer.Controls.Add(pnlHeader);
                PgContainer.Controls.Add(rdr.PgContainer);

                return true;
            }
            return false;
        }
    }
}