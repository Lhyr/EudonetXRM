using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Usine de fabrication de valeurs calculées
    /// </summary>
    public class ComputedValueFactory
    {
        /// <summary>
        /// construit les valeurs calculées
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static List<ComputedValueModel> GetValues(Core.Model.ePref pref, ComputedValueRequestModel request)
        {
            List<ComputedValueModel> computedValues = new List<ComputedValueModel>();

            //Paramètres invalides
            if (request.Tab == 0 || request.ListCol?.Length == 0)
            {
                throw new EudoException(
                    "ComputedValueController",
                    String.Concat(eResApp.GetRes(pref, 2024).Replace("<PARAM>", "")),
                    new Exception(
                        String.Concat("Tab: ", request.Tab, Environment.NewLine, "ListCol: ", request.ListCol)
                    )
                );
            }

            Dictionary<string, decimal> dicAliasValue = new Dictionary<string, decimal>();
            HashSet<string> hsFlt = new HashSet<string>();

            List<int> lstDescId = new List<int>();

            //Somme de colonnes depuis un signet
            Boolean bFromBkm = ((request.ParentTab != request.Tab) && request.ParentFileId > 0 && request.ParentTab > 0);

            //Type de Eudoquery à utiliser
            ViewQuery typeQuery = ViewQuery.TREATMENT;
            if (bFromBkm)
                typeQuery = ViewQuery.LIST_BKM;

            int nViewTab = request.Tab;
            if (request.Tab == request.ParentTab + AllField.BKM_PM_EVENT.GetHashCode())
            {
                typeQuery = ViewQuery.LIST_BKM_EVENTPM;
                nViewTab = request.ParentTab;
            }

            EudoQuery.EudoQuery eq = eLibTools.GetEudoQuery(pref, nViewTab, typeQuery);
            if (bFromBkm)
            {
                eq.SetParentDescid = request.ParentTab;
                eq.SetParentFileId = request.ParentFileId;
            }
            else
            {
                //Fiches marquées
                if (pref.Context.MarkedFiles.ContainsKey(request.Tab))
                    eq.SetDisplayMarkedFile = pref.Context.MarkedFiles[request.Tab].Enabled;

                //Filtre Avancé
                if (pref.Context.Filters.ContainsKey(request.Tab))
                    eq.SetFilterId = pref.Context.Filters[request.Tab].FilterSelId;
            }

            eq.SetListCol = String.Join(";", request.ListCol);
            eq.LoadRequest();

            #region gestion erreur
            if (eq.GetError.Length > 0)
            {
                throw new EudoException(
                    "ComputedValueController - EudoQuery",
                    eq.GetError,
                    eq.InnerException
                );
            }
            #endregion

            //Liste des champs rétourné
            List<Field> lstFieldHeader = eq.GetFieldHeaderList;
            List<Table> lstTableHeader = eq.GetTableHeaderList;
            Table mainTable = eq.GetMainTable;

            //Récupération des colonnes à calculer
            foreach (int nDescId in request.ListCol)
            {
                //Recherche le field à calculer
                Predicate<Field> predicate = delegate (Field fld) { return fld.Descid == nDescId; };
                if (lstFieldHeader.Exists(predicate) && !lstDescId.Contains(nDescId))
                {
                    dicAliasValue.Add(lstFieldHeader.Find(predicate).Alias, 0);
                    lstDescId.Add(nDescId);
                }
            }

            //Aucun champ à calculer - Msg user only
            if (dicAliasValue.Count == 0)
            {
                throw new EudoException(eResApp.GetRes(pref, 581), eResApp.GetRes(pref, 581));
            }

            //On doit charger toutes la liste
            eq.SetTopRecord = 0;

            //Construction de la requte
            eq.BuildRequest();

            string sSQL = eq.EqQuery;
            ViewQuery vq = eq.GetQueryType;
            eq.CloseQuery();
            eudoDAL eDal = eLibTools.GetEudoDAL(pref);
            eDal.OpenDatabase();

            string err = String.Empty;
            RqParam rqList = new RqParam(sSQL);
            if (vq == ViewQuery.LIST_BKM_REL)
            {
                sSQL = sSQL.Replace("$#$fileid$#$", "@fileid");
                rqList.SetQuery(sSQL);
                rqList.AddInputParameter("@fileid", SqlDbType.Int, request.ParentFileId);
            }
            else if (vq == ViewQuery.LIST_BKM_EVENTPM)
            {
                sSQL = sSQL.Replace("$#$PMID$#$", "@pmid");
                rqList.SetQuery(sSQL);
                rqList.AddInputParameter("@pmid", SqlDbType.Int, request.ParentPMFileId);
            }
            DataTableReaderTuned dtrStats = eDal.Execute(rqList, out err);
            try
            {
                eDal.CloseDatabase();
                //Gestion erreur
                if (err.Length > 0)
                {
                    throw new EudoException(
                        err,
                        String.Concat(eResApp.GetRes(pref, 422), "<br>", eResApp.GetRes(pref, 544)),  //  Détail : pour améliorer...
                        eDal.InnerException
                    );
                }

                string sIdAlias = String.Concat(mainTable.Alias, "_ID");

                while (dtrStats.Read())
                {
                    foreach (int nDescId in lstDescId)
                    {
                        //Champ à calculer
                        Predicate<Field> predicate = delegate (Field fld) { return fld.Descid == nDescId; };
                        Field fldToCompute = lstFieldHeader.Find(predicate);

                        if (fldToCompute != null)
                        {
                            string sAliasId = String.Concat(fldToCompute.Table.Alias, "_ID");
                            int nFieldFileId;
                            if (!int.TryParse(dtrStats.GetString(sAliasId), out nFieldFileId))
                                continue;

                            string sKeyValue = String.Concat(sAliasId, "_", fldToCompute.Descid, "_", nFieldFileId);

                            if (!hsFlt.Contains(sKeyValue))
                            {
                                hsFlt.Add(sKeyValue);

                                //Droit de visu
                                if (eLibTools.AllowedView(fldToCompute, dtrStats, pref.GroupMode))
                                {
                                    //Construction objet de retour
                                    string aliasName = eTools.GetFieldValueCellName(request.Tab, fldToCompute.Alias);
                                    ComputedValueModel computedValue = computedValues.Find(cv => cv.Alias == aliasName);
                                    if (computedValue == null)
                                    {
                                        computedValue = new ComputedValueModel();
                                        computedValue.Alias = eTools.GetFieldValueCellName(request.Tab, fldToCompute.Alias);
                                        computedValue.DescId = fldToCompute.Descid;
                                        computedValue.DecimalCount = fldToCompute.Length;
                                        computedValues.Add(computedValue);
                                    }
                                    //Mise à jour de la somme de la colonne en cours avec la valeur de l'enregistrement en cours
                                    computedValue.DecimalValue += dtrStats.GetDecimal(fldToCompute.ValueAlias);
                                    computedValue.Value = eNumber.FormatNumber(pref, computedValue.DecimalValue, computedValue.DecimalCount, true);
                                }
                            }
                        }

                    }
                }

            }
            finally
            {
                //Libération ressources
                if (dtrStats != null)
                    dtrStats.Dispose();
            }

            return computedValues;
        }
    }
}