using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Linq;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminMainMenuManager
    /// </summary>
    public class eAdminMainMenuManager : eAdminManager
    {


        Int32 _nActiveTab;

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        protected override void ProcessManager()
        {

            //Initialisation
            //

            String sPart = "";
            string[] openedBlocks = null;

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out _nActiveTab);

            if (_requestTools.AllKeys.Contains("part") && !String.IsNullOrEmpty(_context.Request.Form["part"]))
                sPart = _context.Request.Form["part"].ToString();

            if (_requestTools.AllKeys.Contains("openedblocks") && !String.IsNullOrEmpty(_context.Request.Form["openedblocks"]))
                openedBlocks = _context.Request.Form["openedblocks"].ToString().Split(';');

            //A enrichir pour obtenir des parties isolées du menu de droite
            // Si enrichi --> transformer sPart en enum

            eAdminTableInfos tab = new eAdminTableInfos(_pref, _nActiveTab);

            // pour l'instant, on désactive "en dur" la table PAYMENTTRANSACTION, ceci étant temporaire
            if ((tab.TabHiddenProduct || _nActiveTab == (int)TableType.PAYMENTTRANSACTION) && _pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                throw new EudoAdminInvalidRightException();

            eAdminRenderer rdr = null;
            if (sPart.Length == 0)
            {
                rdr = eAdminRendererFactory.CreateAdminTabRightMenuRenderer(_pref, _nActiveTab, openedBlocks);
            }
            else
            {
                if (sPart == "RelationsPart")
                {
                    if (_nActiveTab < (int)TableType.HISTO)
                    {
                        rdr = eAdminRendererFactory.CreateAdminRelationsRenderer(_pref, tab, eResApp.GetRes(_pref, 1117), "", openedBlocks?.Contains("RelationsPart") ?? false);
                    }
                }
                else if (sPart == "FiltersSearchesDuplicatesPart")
                {
                    rdr = eAdminRendererFactory.CreateAdminFiltersAndDuplicatesRenderer(_pref, tab, eResApp.GetRes(_pref, 6813));
                }
            }

            if (rdr != null)
                RenderResultHTML(rdr.PgContainer);
        }


    }
}