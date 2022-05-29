using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Text.RegularExpressions;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer pour la liste des filtres pour les invitations
    /// </summary>
    public class eInvitFilterListRenderer : eFilterListRenderer
    {

        private Boolean _bDeleteMode = false;


        /// <summary>
        /// Mode suppression
        /// </summary>
        public Boolean DeleteMode
        {
            get { return _bDeleteMode; }

        }


        /// <summary>
        /// Retourne un renderer pour la liste des filtres en mode invitation (++/xx)
        /// </summary>
        /// <param name="pref">préférence utilisateur</param>
        /// <param name="height">Hauteur du div de rendu</param>
        /// <param name="width">Largeur</param>
        /// <param name="nTab">Table des invitations</param>
        /// <param name="bDelete">Mode suppression</param>
        /// <returns></returns>
        public static eInvitFilterListRenderer GetInvitFilterListRenderer(ePref pref, Int32 height, Int32 width, Int32 nTab, Boolean bDelete = false)
        {
            eInvitFilterListRenderer myRenderer = new eInvitFilterListRenderer(pref, height, width);
            myRenderer._tab = nTab;
            myRenderer._bDeleteMode = bDelete;

            return myRenderer;
        }

        /// <summary>
        /// Constructeur privé du renderer
        /// Pour obtenir une instance de l'objet, utiliser GetFilterReportListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        private eInvitFilterListRenderer(ePref pref, Int32 height, Int32 width)
            : base(pref, height, width)
        {

        }


        /// <summary>
        /// Génération de la liste des filtres pour les invitations
        /// </summary>
        protected override void GenerateList()
        {
            FilterType = EudoQuery.TypeFilter.USER;
            _list = eListFactory.CreateInvitFilterList(Pref, _tab, _bDeleteMode);


        }



        /// <summary>
        /// Paging pour l'instant désactivé ici
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (_list.ErrorMsg.Length == 0 && _list.ListRecords != null)
                {
                    _rows = _list.ListRecords.Count;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Ajoute les spécifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, Int32 idxLine)
        {
            trRow.Attributes.Add("onclick", "selectLine(this);");
            trRow.Attributes.Add("ondblclick", "selectLine(this);StepClick('2');return false;");

            try
            {
                //lorsqu'on liste les filtres depuis un assistant ++ on a besoin de connaitre la table à laquelle le filtre appartient.
                String sFilterTabAlias = String.Concat(TableType.FILTER.GetHashCode(), "_", FilterField.TAB.GetHashCode());
                
                eFieldRecord efFilterTab = row.GetFieldByAlias(sFilterTabAlias);
                trRow.Attributes.Add("eft", efFilterTab?.Value ?? ""); // pour suppression en xx, il n'y a pas de filtertab.

                eRecordFilter rowFilter = (eRecordFilter)row;
                Regex reQuestion = new Regex("question_[0-9]_[0-9]=1");
                if (reQuestion.IsMatch(rowFilter.Param))
                    trRow.Attributes.Add("iq", "1");
            }
            catch { }
        }

    }
}