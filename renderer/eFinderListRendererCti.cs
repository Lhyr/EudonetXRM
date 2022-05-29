using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eFinderRenderer</className>
    /// <summary>classe générant le rendu du champ de liaison</summary>
    /// <authors>GCH</authors>
    /// <date>2012-09-07</date>
    public class eFinderListRendererCti : eFinderListRenderer
    {
        private string _pn;

        private string _sSearch;

        private List<UserValueField> _listDisplayedUserValueField;

        /// <summary>
        /// Constructeur de l'extrème
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <param name="nTargetTab">Table sur laquelle on recherche</param>
        /// <param name="nTabFrom">Table appelante</param>
        /// <param name="nFileId">Id de la fiche appelante</param>
        /// <param name="sSearch">The s search.</param>
        /// <param name="bHisto">if set to <c>true</c> [b histo].</param>
        /// <param name="currentSearchMode">Spécifie le mode de recherche choisi (standard, étendue, étendue sur toutes les rubriques affichées)</param>
        /// <param name="bPhoneticSearch">if set to <c>true</c> [b phonetic search].</param>
        /// <param name="bAllRecord">if set to <c>true</c> [b all record].</param>
        /// <param name="nDescid">DescId du champ</param>
        /// <param name="nDispValue">The n disp value.</param>
        /// <param name="listDisplayedUserValueField">Liste des infos des champs affichés pour Uservalue (IsFound$|$Parameter$|$Value$|$Label)</param>
        /// <param name="listCol">Liste des colonnes demandées à l'affichage (vide pour le choix par défaut)</param>
        /// <param name="listColSpec">The list col spec.</param>
        /// <param name="fileMode">Type de champ de liaison
        /// 0 =&gt; Champ de liaison classique (ligne sélectionnable avant validation)
        /// 1 =&gt; Recherche avancée (cliquable et permet de rediriger vers la fiche correspondant à la ligne sélectionnée)
        /// 3 =&gt; MRU (permet de sélectionner au clique)</param>
        /// <param name="pn">The pn.</param>
        public eFinderListRendererCti(ePref pref, Int32 height, Int32 width, Int32 nTargetTab, Int32 nTabFrom, Int32 nFileId, string sSearch, Boolean bHisto, eFinderList.SearchMode currentSearchMode, Boolean bPhoneticSearch, Boolean bAllRecord, Int32 nDescid, List<Int32> nDispValue, List<UserValueField> listDisplayedUserValueField, List<Int32> listCol, List<Int32> listColSpec, eFinderList.Mode fileMode, string pn)
            : base(pref, height, width, nTargetTab, nTabFrom, nFileId, sSearch, bHisto, currentSearchMode, bPhoneticSearch, bAllRecord, nDescid, nDispValue, listDisplayedUserValueField, listCol, listColSpec, fileMode)
        {
            _sSearch = sSearch;
            _pn = pn;
            _listDisplayedUserValueField = listDisplayedUserValueField;
        }

        /// <summary>
        /// MCR/GCH Initialisaton de l object liste
        /// </summary>
        /// <param name="nTabFrom"></param>
        /// <param name="nFileId"></param>
        /// <param name="sSearch"></param>
        /// <param name="bHisto"></param>
        /// <param name="currentSearchMode">Spécifie le mode de recherche choisi (standard, étendue, étendue sur toutes les rubriques affichées)</param>
        /// <param name="bPhoneticSearch"></param>
        /// <param name="bAllRecord"></param>
        /// <param name="nDescid"></param>
        /// <param name="nDispValue"></param>
        /// <param name="listDisplayedUserValueField"></param>
        /// <param name="listCol"></param>
        /// <param name="listColSpec"></param>
        /// <param name="bMulti"></param>
        protected override void initList(Int32 nTabFrom, Int32 nFileId, string sSearch, Boolean bHisto, eFinderList.SearchMode currentSearchMode, Boolean bPhoneticSearch, Boolean bAllRecord, Int32 nDescid, List<Int32> nDispValue, List<UserValueField> listDisplayedUserValueField, List<Int32> listCol, List<Int32> listColSpec, Boolean bMulti = false)
        {
            _pn = sSearch;              // MRC a faire pour conserver la valeur provenant du initList
            _sSearch = sSearch;         // MRC a faire pour conserver la valeur provenant du initList

            _list = eFinderListCti.CreateFinderListCti(Pref, _tab, nTabFrom, nFileId, nDescid, _sSearch, bHisto, currentSearchMode, bPhoneticSearch, bAllRecord, nDispValue,
                                  _listDisplayedUserValueField, listCol, listColSpec, _fileMode, _pn);
            if (!string.IsNullOrEmpty(_list.ErrorMsg))
            {
                _eException = _list.InnerException;
                _sErrorMsg = _list.ErrorMsg;
                return;
            }
        }


        /// <summary>Rendu du haut du champ de liaison</summary>
        /// <returns>Rendu généré</returns>
        public List<eUlCtrl> GetFinderCtiTop()
        {
            List<eUlCtrl> listUL = new List<eUlCtrl>();

            eUlCtrl ulSearch = new eUlCtrl();
            listUL.Add(ulSearch);
            ulSearch.Attributes.Add("class", "LnkTop");

            eLiCtrl li = null;
            eUlCtrl ulOptions = new eUlCtrl();

            ulOptions.Attributes.Add("class", "LnkTopOpt");

            #region Historique
            if (_list.HistoInfo.Has)
            {
                li = new eLiCtrl();
                ulOptions.Controls.Add(li);

                HtmlGenericControl btnContainer = new HtmlGenericControl("div");
                eTools.BuildHistoBtn(this.Pref, btnContainer, _list.HistoInfo.Has, false, "finder");

                li.Controls.Add(btnContainer);
            }
            #endregion

            if (ulOptions.Controls.Count > 0)
                listUL.Add(ulOptions);

            li = null;
            ulSearch = null;

            return listUL;
        }

    }
}