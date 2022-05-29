using static Com.Eudonet.Xrm.IRISBlack.Model.ListDetailModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;
using Syncfusion.XPS;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using System.Threading.Tasks;
using System.Text;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// 
    /// </summary>
    public class StructureModelForListFactory
    {

        #region propriétés
        private int PosX { get; set; } = 0;
        private int PosY { get; set; } = 0;

        protected eList objList { get; set; }
        protected ePref pref { get; set; }
        protected List<IFldTypedInfosModel> LstStructFields { get; set; } = new List<IFldTypedInfosModel>();
        protected string WebDataPath { get; set; }
        protected int MainFieldId { get; set; }
        protected int ViewMainTab { get; set; }

        protected bool ExpressFilterActivated { get; set; }
        protected EdnType StructType { get; set; }
        protected string Label { get; set; }

        /// <summary>Libellé pour un seul résultat dans le compte d'enregistrement</summary>
        protected string SingularResultLabel { get; set; }
        /// <summary>Libellé pour plusieurs résultats dans le compte d'enregistrement</summary>
        protected string PluralResultsLabel { get; set; }

        /// <summary>Libellé du filtre avancé actif sur la liste</summary>
        protected string AdvancedFilterLabel { get; set; }
        /// <summary>Libellé de la sélection de fiches marquées actives sur la liste</summary>
        protected string MarkedFilesSelectionLabel { get; set; }

        #endregion

        #region construteurs
        /// <summary>
        /// constructeur pour l'objet
        /// </summary>
        protected StructureModelForListFactory(eList l, ePref p)
        {
            objList = l;
            pref = p;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Retourne une instance de la classe.
        /// </summary>
        /// <param name="l">objet list</param>
        /// <param name="pref">objet des prefs</param>
        /// <returns></returns>
        public static StructureModelForListFactory InitListDetailModelFactory(eList l, ePref pref)
        {
            return new StructureModelForListFactory(l, pref); ;
        }
        #endregion

        #region privées

        private async Task Load()
        {
            WebDataPath = eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.ROOT, pref.GetBaseName);
            Label = objList.ViewMainTable.Libelle;
            MainFieldId = objList.MainField?.Descid ?? 0;
            ViewMainTab = objList.ViewMainTable?.DescId ?? objList.CalledTabDescId;

            if (objList.ListRecords != null && objList.ListRecords.Count > 0)
            {
                // on retire les informations propres aux champs de liaison. les informations sur le champs lui-même étant suffisantes
                /** TODO : Gros goulet d'étranglement. A mettre en asynchrone, on passe de 10 à 3s en debug. A voir en prod si c'est du même genre. GLA */

                var ieFields = objList.ListRecords[0].GetFields.Where(f => f.FldInfo.PermViewAll);

                var ieDescId = ieFields
                    .Select(fld => fld.FldInfo.Descid)
                    .Union(ieFields.Where(fld => fld.FldInfo.AliasSourceField != null).Select(fld => fld.FldInfo.AliasSourceField.Descid))
                    .Union(ieFields.Select(fld => eLibTools.GetTabFromDescId(fld.FldInfo.Descid)));

                if (ieDescId.Count() < 1)
                {
                    ieDescId = Enumerable.Range(ViewMainTab, eLibConst.MAX_NBRE_FIELD);
                }

                var oFldAdv = FieldsAdvancedCplFactory.InitFieldsAdvancedCplFactory(pref, ieDescId);
                var oRes = oFldAdv.GetResInternal();
                var oDescAdv = oFldAdv.GetDescAdvDataSet();
                var rgpdData = RGPDFactory.initRGPDFactory(pref).GetRGPDData(ieDescId);

                await Task.Run(() => LstStructFields.AddRange(ieFields.AsParallel()
                    .Select(fr => FldTypedInfosFactory.InitFldTypedInfosFactory(fr.FldInfo, pref, dataFiller: objList).GetFldTypedInfos(oRes, oDescAdv, rgpdData))));

                //KHA du fait du passage en asynchrone les champs ne sont plus ordonnés correctement.
                //on les réordonne donc ensuite selon l'ordre de selection
                List<int> lstOrderDescId = objList.ListRecords[0].GetFields.Select(f => f.FldInfo.Descid).ToList();
                LstStructFields.Sort((a, b) => lstOrderDescId.IndexOf(a.DescId).CompareTo(lstOrderDescId.IndexOf(b.DescId)));

            };

            setExpressFilter();
            setStructType();
            setAdvancedLabels();
        }

        /// <summary>
        /// vérifie s'il existe des filtres express activés par le user sur la liste
        /// </summary>
        protected virtual void setExpressFilter()
        {
            eListMain list = objList as eListMain;
            ExpressFilterActivated = list.LstActiveExpressFilter.Count > 0;
        }

        /// <summary>
        /// set le type de la table appelée
        /// </summary>
        protected virtual void setStructType()
        {
            StructType = objList.ViewMainTable?.EdnType ?? EdnType.FILE_UNDEFINED;
        }

        /// <summary>
        /// set les libellés
        /// </summary>
        protected void setAdvancedLabels()
        {
            int tab = objList.CalledTabDescId;
            bool bFound = false;
            eResInternal res = new eResInternal(pref, tab.ToString());
            eListMain list = objList as eListMain;
            List<FilterTipInfo> lstFilter = list.FilterTipInfo;

            AdvancedFilterLabel = lstFilter?.FirstOrDefault(f => f.Type == FilterTipType.ADVANCED)?.Label;
            MarkedFilesSelectionLabel = lstFilter?.FirstOrDefault(f => f.Type == FilterTipType.MARKEDFILE)?.Label;
            SingularResultLabel = res.GetResAdv(new KeyResADV(eLibConst.RESADV_TYPE.RESULT_LABEL_SINGULAR, tab, pref.LangId), out bFound);
            PluralResultsLabel = res.GetResAdv(new KeyResADV(eLibConst.RESADV_TYPE.RESULT_LABEL_PLURAL, tab, pref.LangId), out bFound);
        }

        #endregion

        #region public
        /// <summary>
        /// Crée une intance de ListDetailModel.StructureModel et la retourne.
        /// </summary>
        /// <returns></returns>
        public async Task<StructureModel> GetListStructureModel()
        {
            await Load();

            return new StructureModel
            {
                LstStructFields = LstStructFields ?? new List<IFldTypedInfosModel>(),
                ExpressFilterActivated = ExpressFilterActivated,
                MainFieldId = MainFieldId,
                ViewMainTab = ViewMainTab,
                StructType = StructType,
                WebDataPath = WebDataPath,
                Label = Label,
                PluralResultsLabel = PluralResultsLabel,
                SingularResultLabel = SingularResultLabel,
                AdvancedFilterLabel = AdvancedFilterLabel,
                MarkedFilesSelectionLabel = MarkedFilesSelectionLabel
            };
        }
        #endregion
    }
}