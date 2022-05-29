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
    /// Renderer pour la gestion des droits et inclure les grilles 
    /// </summary>
    public class eAdminRightsDescAndGridsRenderer : eAdminRightsDescRenderer
    {

        /// <summary>
        /// Id de la grille 
        /// </summary>
        private int _gridId;

        /// <summary>
        /// Id de la page si table page d'accueil, 0 si autre onglet ou l'option "Tous" est sélectionnée 
        /// </summary>
        private int _pageId;

        /// <summary>    
        /// Page d'accueil sélectionnée dans le choix des onglets
        /// </summary>
        private eRecord _page;

        /// <summary>
        /// Infos desc sur la table en cours
        /// </summary>
        private eAdminTableInfos _tableInfos;

        /// <summary>
        /// La liste représente 
        /// 1 seule grille si nGridId > 0
        /// Liste des grilles associées à la page d'acueill si nPageId > 0 et nGridId = 0
        /// Liste des grilles associées à l'onglet nDescID si nPageId = 0 et nGridId = 0
        /// </summary>
        private IEnumerable<eRecord> _grids;

        /// <summary>
        /// constructeur de la fenetre des droits pour les pages d'accueil XRM
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nDescId">Descid de la table homepage, on le garde pour la sécu</param>
        /// <param name="nPageId">Id de la page d'accueil</param>
        /// <param name="nGridId">ID de la grille</param>
        public eAdminRightsDescAndGridsRenderer(ePref pref, Int32 nDescId, Int32 nPageId, Int32 nGridId) : base(pref, nDescId)
        {
            _pageId = nPageId;
            _gridId = nGridId;
        }

        /// <summary>
        /// Récupèration de la liste des pages d'accueil
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            if (_pageId > 0) // Infos sur la page d'accueil      
                _page = eFileMain.CreateMainFile(Pref, (int)TableType.XRMHOMEPAGE, _pageId, -2).Record;
            else // info sur l'onglet
                _tableInfos = new eAdminTableInfos(Pref, eLibTools.GetTabFromDescId(_descid));

            if (_gridId > 0)  // Context ou on paramèrtre la grille seule
                _grids = eFileMain.CreateMainFile(Pref, (int)TableType.XRMGRID, _gridId, -2).ListRecords;
            else if (_pageId > 0) // Sinon on affiche la liste des grilles associées à la pages
                _grids = eGridList.GetRecordList(Pref, (int)TableType.XRMHOMEPAGE, _pageId);
            else // Sinon on affiche la liste des grilles associées à un onglet
                _grids = eGridList.GetRecordList(Pref, eLibTools.GetTabFromDescId(_descid));

            return base.Init();
        }

        /// <summary>
        /// Ajout des droits de traitement pour les grilles
        /// </summary>
        /// <returns></returns>
        protected override List<IAdminTreatmentRight> GetTreatmentRights()
        {
            List<IAdminTreatmentRight> rights = new List<IAdminTreatmentRight>();

            // On recupère les droits si on vient depuis un onglet autre que Page d'accueil ou onglet web
            if (!IsHomePageType() && _gridId == 0 && !IsWebTabTyp())
                rights = base.GetTreatmentRights();

            // Pour les grilles, il n' y a qu'un seul type : visualisation 
            string viewRes = eAdminTreatmentTypeRes.GetRes(eTreatmentType.VIEW, Pref);
            string traitRes = eResApp.GetRes(Pref, 8212);

            foreach (eRecord grid in _grids)
            {
                rights.Add(new eAdminGridTreatmentRight(Pref, grid)
                {
                    TypeLabel = viewRes,
                    TabFromLabel = GetTableFrom(),
                    TraitLabel = traitRes
                });
            }

            return rights;
        }

        /// <summary>
        /// Onglet de type web
        /// </summary>
        /// <returns></returns>
        private bool IsWebTabTyp()
        {
            if (_tableInfos != null && (_tableInfos.EdnType == EdnType.FILE_WEBTAB || _tableInfos.EdnType == EdnType.FILE_GRID || _tableInfos.EdnType == EdnType.FILE_WIDGET))
                return true;

            return false;
        }

        /// <summary>
        /// Pages d'accueil
        /// </summary>
        /// <returns></returns>
        private bool IsHomePageType()
        {
            if (_descid == (int)TableType.XRMHOMEPAGE && _pageId > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Récupère la valeur de la colonne "Vu depuis"
        /// </summary>
        /// <returns></returns>
        private string GetTableFrom()
        {
            if (_page != null)
            {
                eFieldRecord fld = _page.GetFields.Find(f => f.FldInfo.Descid == (int)XrmHomePageField.Title);
                if (fld == null)
                    return string.Empty;

                return fld.DisplayValue;
            }

            if (_tableInfos != null)
                return _tableInfos.TableLabel;

            return "[[## RECORD_INVALID ##]]";
        }

        /// <summary>
        /// Personalisation de l'option fiche page d'accueil dans le choix des onglet
        /// </summary>
        /// <param name="ddl">DropDownList</param>
        protected override void SelectTabItem(DropDownList ddl)
        {

            if (IsHomePageType())
                ddl.SelectedValue = string.Concat(_descid, "_", _pageId);
            else
                base.SelectTabItem(ddl);
        }

        /// <summary>
        /// Les onglet "vu depuis" sont récupérés que si on est pas en onglet web ni pages d'accueil
        /// </summary>
        /// <param name="ddl">DropDownList</param>
        protected override void FillTabFromList(DropDownList ddl)
        {
            if (!IsHomePageType() && !IsWebTabTyp())
                base.FillTabFromList(ddl);
        }

        /// <summary>
        /// Le type visualisation est disponible exclusivement pour l'onglet web et pages d'accueil
        /// </summary>
        /// <param name="ddl"></param>
        protected override void FillTypeList(DropDownList ddl)
        {
            if (!IsHomePageType() && !IsWebTabTyp())
            {
                base.FillTypeList(ddl);
            }
            else
            {
                ListItem item = new ListItem(eAdminTreatmentTypeRes.GetRes(eTreatmentType.VIEW, Pref), ((int)eTreatmentType.VIEW).ToString());
                item.Selected = true;
                ddl.Items.Add(item);
            }
        }

        /// <summary>
        /// Pas de champ pour les onglets web ni les page d'accueil
        /// </summary>
        /// <param name="ddl"></param>
        protected override void FillFieldList(DropDownList ddl)
        {
            if (!IsHomePageType() && !IsWebTabTyp())
                base.FillFieldList(ddl);
        }
    }
}