using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EudoQuery;
using System.Data;
using System.Text;
using EudoExtendedClasses;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// exception spécifique à la gestion des bkmPref
    /// </summary>
    public class eAdminBkmPrefException : Exception
    {
        private string p;
        private Exception e;

        /// <summary>
        /// constructeur idem que Exception
        /// </summary>
        /// <param name="p"></param>
        /// <param name="e"></param>
        public eAdminBkmPrefException(string p, Exception e)
            : base(p, e)
        {
        }
    }

    /// <summary>
    /// classe métier de mise à jour des bkmPref
    /// </summary>
    public class eAdminBkmPref
    {
        public eAdminBkmPrefException Exception { get; private set; }
        ePrefLite _pref = null;


        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminBkmPref(ePref pref)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            _pref = pref;
        }


        /// <summary>
        /// Mise à jour de preférence de signet (BKMPREF). Encapsule eBkmPref.SetBkmPref() et renvoie un eAdminResult
        /// </summary>
        /// <param name="iUserId"></param>
        /// <param name="iBkm"></param>
        /// <param name="iTab"></param>
        /// <param name="bkmPrefValues"></param>
        /// <returns></returns>
        public eAdminResult SetBkmPref(Int32 iUserId, Int32 iBkm, Int32 iTab, ICollection<SetParam<ePrefConst.PREF_BKMPREF>> bkmPrefValues)
        {
            eAdminResult res = new eAdminResult();

            if (bkmPrefValues == null || bkmPrefValues.Count <= 0)
            {
                res.Success = false;
                res.UserErrorMessage = "Aucune donnée à mettre à jour";
                return res;
            }

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            try
            {
                eBkmPref bkmPref = new eBkmPref((ePref)_pref, iTab, iBkm);
                if (iUserId == 0)
                    res.Success = bkmPref.SetBkmPrefDefault(bkmPrefValues);
                else
                    res.Success = bkmPref.SetBkmPref(bkmPrefValues);
            }
            catch (Exception e)
            {
                Exception = new eAdminBkmPrefException("Une erreur est survenue durant la mise à jour des bkmPref", e);
                res.Success = false;
                res.UserErrorMessage = "Une erreur est survenue durant la mise à jour des préférences de signets";
                res.InnerException = Exception;
                res.DebugErrorMessage = e.Message;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return res;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="iBkm"></param>
        /// <param name="iTab"></param>
        /// <param name="bkmPrefValues"></param>
        /// <returns></returns>
        public eAdminResult CleanBkmPref(Int32 iBkm, Int32 iTab, ICollection<SetParam<ePrefConst.PREF_BKMPREF>> bkmPrefValues)
        {
            eAdminResult res = new eAdminResult();
            //Valeur nétoyée à mettre à jour
            String formValue = string.Empty;
            if (bkmPrefValues == null || bkmPrefValues.Count <= 0)
            {
                res.Success = false;
                res.UserErrorMessage = "Aucune donnée à mettre à jour";
                return res;
            }

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            try
            {
                foreach (SetParam<ePrefConst.PREF_BKMPREF> item in bkmPrefValues)
                {
                    int mainField = 0;
                    int tab = 0;
                    if (Int32.TryParse(item.Value, out tab) && tab % 100 == 0)
                    {
                        mainField = tab + 1;
                        //eBkmPref bkmPref = new eBkmPref((ePref)_pref, iTab, iBkm);
                        ISet<int> listCol;
                        HashSet<int> listColAndColFixed;
                        List<SetParam<ePrefConst.PREF_BKMPREF>> prefBkm = new List<SetParam<ePrefConst.PREF_BKMPREF>>();
                        string dbContent;
                        ColFilterOptions colFiltOpts;
                        ColFilterOptionsOldFormat colFiltOptsOldFormat;
                        ColWidthOptions colWidOpts;
                        ColWidthOptionsOldFormat colWidOptsOldFormat;

                        eBkmPref bkmPref = new eBkmPref((ePref)_pref, iTab, iBkm);

                        #region colwidth - Chargement des anciennes valeurs
                        listCol = new HashSet<int>(bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOL).ConvertToListInt(";"));

                        // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                        listColAndColFixed = new HashSet<int>();
                        if (mainField != 0 && !listCol.Contains(mainField))
                            listColAndColFixed.Add(mainField);
                        listColAndColFixed.UnionWith(listCol);

                        // Charge les valeurs actuelles en db
                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                            String.Join(";", listColAndColFixed),
                            bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH));
                        ColWidthOptions colWidOpts_Old = colWidOptsOldFormat.GetOptions();
                        #endregion

                        listCol = new HashSet<int>(formValue.ConvertToListInt(";"));

                        // Ajout des colonnes a ne jamais netoyer (liaisons, rubriques 01, etc.)
                        listColAndColFixed = new HashSet<int>();
                        if (mainField != 0 && !listCol.Contains(mainField))
                            listColAndColFixed.Add(mainField);
                        listColAndColFixed.UnionWith(listCol);

                        #region colwidth
                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(
                            String.Join(";", listColAndColFixed), String.Empty);
                        colWidOpts = colWidOptsOldFormat.GetOptions();

                        colWidOpts.RestaureWidths(colWidOpts_Old);

                        colWidOptsOldFormat = ColWidthOptionsOldFormat.GetNew(colWidOpts.Options);

                        // Verifie la taille du champ
                        eLibTools.CheckFieldSize(dal, "BKMPREF", ePrefConst.PREF_BKMPREF.BKMCOLWIDTH.ToString(), colWidOptsOldFormat.Width.Length, true);
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMCOLWIDTH, colWidOptsOldFormat.Width));
                        #endregion

                        #region expressfilter
                        // On recupère les valeurs existantes en base
                        colFiltOptsOldFormat = ColFilterOptionsOldFormat.GetNew(
                                    bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERCOL),
                                    bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTEROP),
                                    bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERVALUE));
                        colFiltOpts = colFiltOptsOldFormat.GetOptions();

                        // On supprime les valeurs non utile
                        colFiltOpts.ClearNotOptionInList(listColAndColFixed);

                        colFiltOptsOldFormat = ColFilterOptionsOldFormat.GetNew(colFiltOpts.Options);

                        // Filtre express
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTERCOL, colFiltOptsOldFormat.FilterCol));
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTEROP, colFiltOptsOldFormat.FilterOp));
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMFILTERVALUE, colFiltOptsOldFormat.FilterValue));
                        #endregion

                        dbContent = eLibTools.Join(";", listCol);
                        // Verifie la taille du champ
                        eLibTools.CheckFieldSize(dal, "BKMPREF", ePrefConst.PREF_BKMPREF.BKMCOL.ToString(), dbContent.Length, true);
                        prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.BKMCOL, dbContent));

                        if (prefBkm.Count > 0)
                            res.Success = bkmPref.SetBkmPref(prefBkm);
                    }


                }



            }
            catch (Exception e)
            {
                Exception = new eAdminBkmPrefException("Une erreur est survenue durant la mise à jour des bkmPref", e);
                res.Success = false;
                res.UserErrorMessage = "Une erreur est survenue durant la mise à jour des préférences de signets";
                res.InnerException = Exception;
                res.DebugErrorMessage = e.Message;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return res;
        }



        public void Load(Int32 iBkm, Int32 iTab)
        {

        }

    }
}