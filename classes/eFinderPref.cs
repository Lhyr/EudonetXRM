using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model.prefs;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Objet métier pour les signets
    /// </summary>
    public class eFinderPrefOld
    {
        private Dictionary<KeyFinderPref, string> _prefDic = new Dictionary<KeyFinderPref, string>();
        private Dictionary<KeyFinderPref, string> _defaultPrefDic = new Dictionary<KeyFinderPref, string>();

        /// <summary>Préférence de l'utilisateur en cours</summary>
        private ePref _ePref;

        /// <summary>DescId de la table principale</summary>
        private Int32 _nTab = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="epref">Objet ePref du user</param>
        /// <param name="nTab">DescId de la table principale</param>
        private eFinderPrefOld(ePref epref, Int32 nTab)
        {
            _ePref = epref;
            _nTab = nTab;

            if (_nTab > 0) { LoadFinderPref(); }
        }

        /// <summary>
        /// Récupère la pref bkm
        /// </summary>
        /// <param name="pref">Pref recherchée</param>
        /// <returns>Valeur trouvée</returns>
        public String GetFinderPref(ePrefConst.PREF_FINDERPREF pref)
        {
            String value = String.Empty;

            if (_prefDic.TryGetValue(new KeyFinderPref(pref), out value))
                return value;
            else
                return String.Empty;
        }

        /// <summary>
        /// Récupère la pref bkm
        /// </summary>
        /// <param name="pref">Pref recherchée</param>
        /// <returns>Valeur trouvée</returns>
        public String GetFinderDefaultPref(ePrefConst.PREF_FINDERPREF pref)
        {
            String value = String.Empty;

            if (_defaultPrefDic.TryGetValue(new KeyFinderPref(pref), out value))
                return value;
            else
                return String.Empty;
        }

        /// <summary>
        /// Met à jour une préférence de signet
        /// </summary>
        /// <param name="prefsValues">Liste des prefs à maj</param>
        /// <returns></returns>
        public bool SetFinderPref(ICollection<SetParam<ePrefConst.PREF_FINDERPREF>> prefsValues)
        {
            // Si il n'y a pas de valeur
            if (prefsValues == null || prefsValues.Count <= 0 || _nTab == 0)
                return false;

            Int32 nbLine, nbParam = 0;
            RqParam rq = new RqParam();
            String paramName = String.Empty, error = String.Empty;
            StringBuilder sbUpdateSet = new StringBuilder();
            StringBuilder sbInsertInto = new StringBuilder();
            StringBuilder sbInsertValues = new StringBuilder();

            eudoDAL _dal = eLibTools.GetEudoDAL(_ePref);

            try
            {
                _dal.OpenDatabase();

                //paramètre de base de la maj
                rq.AddInputParameter("@userId", SqlDbType.Int, _ePref.User.UserId);
                rq.AddInputParameter("@tab", SqlDbType.Int, _nTab);

                foreach (SetParam<ePrefConst.PREF_FINDERPREF> setParam in prefsValues)
                {
                    paramName = String.Concat("@value", nbParam++);

                    // Mise à jour du dico
                    _prefDic[new KeyFinderPref(setParam.Option)] = setParam.Value;

                    // Mise à jour en base
                    if (nbParam != 1)
                    {
                        sbUpdateSet.Append(", ");
                        sbInsertInto.Append(", ");
                        sbInsertValues.Append(", ");
                    }

                    sbUpdateSet.Append("[").Append(setParam.Option).Append("] = ").Append(paramName);
                    sbInsertInto.Append("[").Append(setParam.Option).Append("]");
                    sbInsertValues.Append(paramName);

                    rq.AddInputParameter(paramName, SqlDbType.NVarChar, String.IsNullOrEmpty(setParam.Value) ? null : setParam.Value);
                }

                rq.SetQuery(String.Concat("UPDATE [COLSPREF] SET ", sbUpdateSet.ToString(), " WHERE [UserId] = @userId And [Tab] = @tab AND [COLSPREFTYPE] = @TYPE AND [BKMTAB] = 0"));
                nbLine = _dal.ExecuteNonQuery(rq, out error);

                if (nbLine == 0)
                {
                    rq.SetQuery(String.Concat("INSERT INTO [COLSPREF] (", sbInsertInto, ", [UserId], [Tab], [COLSPREFTYPE], [BKMTAB]) VALUES (", sbInsertValues, ", @userId, @tab, @TYPE,0) "));
                    nbLine = _dal.ExecuteNonQuery(rq, out error);
                }

                rq.AddInputParameter("@TYPE", SqlDbType.Int, (int)ColPrefType.FINDERPREF);

                if (!String.IsNullOrEmpty(error))
                {
                    if (_dal.InnerException != null)
                        throw _dal.InnerException;

                    return false;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur de sauvegarde des prefs", e);
            }
            finally
            {
                _dal.CloseDatabase();
            }


            return true;
        }

        /// <summary>
        /// Charge les préférences d'un signet
        /// </summary>
        /// <returns></returns>
        private Boolean LoadFinderPref()
        {
            String error = String.Empty;

            // Connexion à la base
            eudoDAL dal = eLibTools.GetEudoDAL(_ePref);
            DataTableReaderTuned dtrPref = null;

            try
            {
                dal.OpenDatabase();

                // Construction du select en fonction de l'enum
                eEnumTools<ePrefConst.PREF_FINDERPREF> eta = new eEnumTools<ePrefConst.PREF_FINDERPREF>();

                String sqlSelect = eLibTools.Join(", ",
                        eta.GetList.Select(opt => String.Concat("ISNULL([UserPref].[", opt, "], [UserDefaultPref].[", opt, "]) AS [", opt, "], [UserDefaultPref].[", opt, "] AS [default_", opt, "]")));

                // Construction de la requête
                StringBuilder sbConfig = new StringBuilder()
                    .Append("SELECT ").Append(sqlSelect).AppendLine(" ")
                    .Append("FROM ")
                    .Append("       (SELECT * FROM [COLSPREF] WHERE [UserId] = @userid AND [tab] = @tab AND [COLSPREFTYPE] = @TYPE  AND [BKMTAB] = 0 ) UserPref ")
                    .Append("   FULL JOIN ")
                    .Append("       (SELECT * FROM [COLSPREF] WHERE [UserId] = 0 AND [tab] = @tab  AND [COLSPREFTYPE] = @TYPE AND [BKMTAB] = 0 ) UserDefaultPref ")
                    .Append("   ON UserPref.[tab] = UserDefaultPref.[tab]").AppendLine(" ");


                RqParam rqBkmPref = new RqParam(sbConfig.ToString()) { LoadColumnsType = true };
                rqBkmPref.AddInputParameter("@userid", SqlDbType.Int, _ePref.User.UserId);
                rqBkmPref.AddInputParameter("@tab", SqlDbType.Int, _nTab);
                rqBkmPref.AddInputParameter("@TYPE", SqlDbType.Int, (int)ColPrefType.FINDERPREF );

                dtrPref = dal.Execute(rqBkmPref, out error);
                dal.CloseDatabase();

                if (error.Length != 0 || dtrPref == null)
                    return false;

                LoadDictionaries(eta, dtrPref, (dtr, col) => new KeyFinderPref(col));

                return true;
            }
            finally
            {
                dtrPref?.Dispose();
                dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Parcours du DataTableReader pour construire un dico de valeurs des options de l'application
        /// Remarque importante : le RqParam utilisé pour l'execute du DTR doit l'option LoadColumnsType d'activée
        /// </summary>
        /// <typeparam name="Enum">Enum utilisé qui enumére les options de la db à charger</typeparam>
        /// <param name="eta">Outil de liste de l'enum</param>
        /// <param name="dtr">Résultat venant de la db</param>
        /// <param name="keyGen">Générateur de la clé</param>
        /// <returns>Retourne le dico des options</returns>
        private void LoadDictionaries<Enum>(
            eEnumTools<Enum> eta,
            DataTableReaderTuned dtr,
            Func<DataTableReaderTuned, Enum, KeyFinderPref> keyGen)
        {
            DataTableReaderTuned.ColVal value = null;
            IDictionary<string, DataTableReaderTuned.ColVal> values;

            while (dtr.Read())
            {
                values = dtr.GetLineValues();

                foreach (Enum col in eta.GetList)
                {
                    if (values.TryGetValue(col.ToString().ToLower(), out value))
                        _prefDic[keyGen(dtr, col)] = value.GetSafeValue();

                    if (values.TryGetValue(String.Concat("default_", col.ToString().ToLower()), out value))
                        _defaultPrefDic[keyGen(dtr, col)] = value.GetSafeValue();
                }
            }
        }
    }
}