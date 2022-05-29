using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Com.Eudonet.Internal.eLibConst;
using static Com.Eudonet.Xrm.IRISBlack.Model.FileDetailModel;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Crée la Structure d'un bkm.
    /// </summary>
    public class StructureBkmModelFactory
    {
        #region propriétés
        eBookmark bkm { get; set; }
        ePref pref { get; set; }

        List<int> lstSpecialBkm = new List<int>() { (int)AllField.MEMO_DESCRIPTION, (int)AllField.MEMO_INFOS, (int)AllField.MEMO_NOTES };
        #endregion

        #region constructeur
        /// <summary>
        /// Constructeur de la classe StructureBkmModelFactory
        /// </summary>
        /// <param name="bookmark"></param>
        /// <param name="_pref"></param>
        private StructureBkmModelFactory(eBookmark bookmark, ePref _pref)
        {
            bkm = bookmark;
            pref = _pref;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour la classe StructureBkmModelFactory.
        /// </summary>
        /// <param name="bookmark"></param>
        /// <param name="_pref"></param>
        /// <returns></returns>
        public static StructureBkmModelFactory InitStructureBkmModelFactory(eBookmark bookmark, ePref _pref)
        {
            return new StructureBkmModelFactory(bookmark, _pref);
        }
        #endregion

        #region public
        /// <summary>
        /// Crée une structure de bookmark pour le front à partir 
        /// des informations bookmark
        /// </summary>
        /// <param name="globalright">Liste des droits</param>
        /// <param name="oRightManager">Liste des droits</param>
        /// <param name="lPinnedBookmarkList">Liste des signets épinglés pour l'onglet principal parent du signet en cours</param>
        /// <returns></returns>
        public StructBkmModel GetStructBkmModel(IDictionary<TREATID, bool> globalright = null, eRightReport oRightManager = null, List<int> lPinnedBookmarkList = null)
        {
            ;
            try
            {

                if (!String.IsNullOrEmpty(bkm.ErrorMsg))
                {

                    return new StructBkmModel
                    {
                        Label = bkm?.Libelle ?? "",
                        DescId = bkm?.CalledTabDescId ?? 0,
                        Error = (eLibTools.IsLocalOrEudoMachine())
                                    ? $"Une erreur est survenue sur le signet {bkm.CalledTabDescId} : Code Erreur EQ : {bkm.ErrorType}, Message : {bkm.ErrorMsg}"
                                    : $"{eResApp.GetRes(pref, 72)} {eResApp.GetRes(pref, 6360)}",
                        RelationFieldDescId = bkm?.RelationFieldDescid ?? 0

                    };
                }

                //on exclut les signets de type notes
                if (lstSpecialBkm.Contains(bkm.CalledTabDescId % 100))
                    return null;

                int nViewMode = 0;

                if (!string.IsNullOrEmpty(bkm.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.VIEWMODE)))
                    nViewMode = int.Parse(bkm.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.VIEWMODE));

                bkm.LoadButtonsToDisplay(globalright, oRightManager);
                return new StructBkmModel()
                {
                    DescId = bkm.CalledTabDescId,
                    Label = bkm.Libelle,
                    TableType = (int)bkm.BkmEdnType,
                    HistoricActived = bkm.HistoInfo.Actived,
                    ExpressFilterActived = !string.IsNullOrEmpty(bkm.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERCOL)),
                    ViewMode = nViewMode,
                    //IsMarkettingStep = bkm.IsMarkettingStep,
                    IsMarkettingStepHold = bkm.IsMarkettingStepHold,
                    IsPinned = bkm.IsPinned,
                    PinnedOrder = bkm.IsPinned && lPinnedBookmarkList != null ? lPinnedBookmarkList.IndexOf(bkm.CalledTabDescId) : -1,
                    RelationFieldDescId = bkm.RelationFieldDescid,
                    ListTargetScenario = bkm.ListTargetScenario,
                    Actions = new StructBkmModel.DisplayButtons()
                    {
                        Add = bkm.Actions.Add,
                        AddFromFilter = bkm.Actions.AddFromFilter,
                        AddPurpleFile = bkm.Actions.AddPurpleFile,
                        Historic = bkm.Actions.Historic,
                        DeleteFromFilter = bkm.Actions.DeleteFromFilter,
                        Import = bkm.Actions.Import,
                        ImportTarget = bkm.Actions.ImportTarget,
                        Export = bkm.Actions.Export,
                        Merge = bkm.Actions.Merge,
                        Chart = false,//bkm.Actions.Chart, //BSE: Ne pas afficher Graphique dans la barre d'actions
                        Print = bkm.Actions.Print,
                        Mailing = bkm.Actions.Mailing,
                        SMS = bkm.Actions.SMS,
                        Formular = bkm.Actions.Formular,
                        //tâche #3 095 : bouton Formulaire Avancé
                        AdvFormular = bkm.Actions.AdvFormular,
                        SwitchViewFile = bkm.Actions.SwitchViewFile,
                        MarketingAutomation = bkm.IsMarkettingStep,

                        //action marketting
                        HoldMarkettingStep = bkm.Actions.HoldMarkettingStep,
                        EventTargetForScenario = bkm.Actions.EventTargetForScenario
}


                };
            }
            catch (Exception e)
            {
                return new StructBkmModel()
                {
                    Error = (eLibTools.IsLocalOrEudoMachine())
                                ? $"Une erreur est survenue sur le signet {bkm.CalledTabDescId} : Code Erreur EQ : {bkm.ErrorType}, Message : {bkm.ErrorMsg}"
                                : $"{eResApp.GetRes(pref, 72)} {eResApp.GetRes(pref, 6360)}"
                };
            }

        }

        /// <summary>
        /// Renvoie la liste des signets épinglés de l'onglet demandé
        /// </summary>
        /// <param name="pref">Objet Pref pour l'accès en base (la méthode étant statique, l'objet doit lui être passé)</param>
        /// <param name="nMainTabDescId">DescID/TabID de l'onglet pour lequel renvoyer la liste</param>
        public static List<int> GetPinnedBookmarkList(ePref pref, int nMainTabDescId)
        {
            List<int> lPinnedBookmark = new List<int>();

            // TOCHECK: doit-on récupérer la liste des signets disponibles et choisis par l'utilisateur pour cet onglet, et s'en servir pour filtrer/limiter les TabIDs pouvant être épinglés ?
            //IEnumerable<StructBkmModel> data = await GetData(Tab, FileId);
            //ListBkmPinned = data.Where(bkm => bkm.IsPinned).Select(bkm => bkm.DescId.ToString()).ToList();

            try
            {
                string sBkmPinned = String.Empty;
                IDictionary<PREFADV, string> dicoPrefAdvValues = eLibTools.GetPrefAdvValues(pref, new List<PREFADV>() { PREFADV.LISTBKMPINNED }, pref.UserId, nMainTabDescId);
                if (dicoPrefAdvValues.ContainsKey(PREFADV.LISTBKMPINNED))
                    dicoPrefAdvValues.TryGetValue(eLibConst.PREFADV.LISTBKMPINNED, out sBkmPinned);
                lPinnedBookmark = sBkmPinned.Split(';')?.Select(s => string.IsNullOrEmpty(s) ? 0 : int.Parse(s)).Where(n => n > 0).ToList();
            }
            catch { }

            return lPinnedBookmark;
        }
        #endregion
    }
}