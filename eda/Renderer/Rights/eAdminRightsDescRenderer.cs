using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer pour la gestion des droits standard sur les tables et les rubriques 
    /// Désormais on ajoute la liste des pages d'accueil xrm dans la sélection d'onglet
    /// </summary>
    public class eAdminRightsDescRenderer : eAdminRightsRenderer
    {

        /// <summary>
        /// Constructeur de la fenêtre des droits
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nDescId"></param>
        public eAdminRightsDescRenderer(ePref pref, Int32 nDescId) : base(pref, nDescId) { }


        /// <summary>
        /// Ajout de l'ensemble des pages d'accueil définies dans la base au choix des onglets
        /// </summary>
        /// <param name="ddl"></param>
        protected override void FillTabList(DropDownList ddl)
        {

            // Ajout des autre onglets
            base.FillTabList(ddl);

            // Recupération des pages d'accueil
            List<eRecord> pages = LoadPages();

            if (pages == null || pages.Count == 0)
                return;        

            // Ajout de la liste des pages d'accueil xrm
            String sSep = "-----------------------";

            //Séparateur
            ListItem li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            //Ajout des pages d'accueil
            ListItem item;
            foreach (eRecord r in pages)
            {
                item = new ListItem(r.GetFieldByAlias((int)TableType.XRMHOMEPAGE + "_" + (int)XrmHomePageField.Title).DisplayValue, (int)TableType.XRMHOMEPAGE + "_" + r.MainFileid);               
                ddl.Items.Insert(0, item);
                item.Attributes.Add("title", r.GetFieldByAlias((int)TableType.XRMHOMEPAGE + "_" + (int)XrmHomePageField.Tooltip).DisplayValue);
             
            }
        }

        /// <summary>
        /// Récupère l'ensemble des pages d'accueil
        /// </summary>
        /// <returns></returns>
        private List<eRecord> LoadPages()
        {
            eDataFillerGeneric dtf = new eDataFillerGeneric(Pref, (int)TableType.XRMHOMEPAGE, ViewQuery.CUSTOM);
            dtf.EudoqueryComplementaryOptions =
                delegate (EudoQuery.EudoQuery eq)
                {
                    eq.SetListCol = string.Concat((int)XrmHomePageField.Title , ";" , (int)XrmHomePageField.Tooltip, ";");    
                    eq.AddCustomFieldSort((int)XrmHomePageField.Title, false);
                };

            dtf.Generate();
            return dtf.ListRecords;
        }     
    }
}