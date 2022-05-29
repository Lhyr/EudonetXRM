using System;
using System.Collections.Generic;
using System.Text;
using Com.Eudonet.Internal;
using EudoQuery;
using static Com.Eudonet.Internal.eLibConst;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eAdminLanguage
    {

        public int Id { get; private set; }
        public int SysId { get; private set; }
        public Boolean Disabled { get; private set; }
        public String Label { get; private set; }
        public String Code { get; private set; }

        public Boolean IsDefault { get; private set; }

        private eAdminLanguage(int id, int sysId, String label, Boolean disabled, Boolean isDefault)
        {
            Id = id;
            SysId = sysId;
            Label = label;
            Disabled = disabled;
            IsDefault = isDefault;
            Code = String.Concat("LANG_", id.ToString().PadLeft(2, '0'));
        }

        /// <summary>
        /// Chargement de la liste des langues
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="bEnabledOnly">Ne charger seulement que les langues actives</param>
        /// <returns></returns>
        public static List<eAdminLanguage> Load(ePref pref, bool bEnabledOnly = false)
        {
            String query = String.Empty;
            StringBuilder sbQuery = new StringBuilder();
            String error = String.Empty;
            eAdminLanguage[] languages = null;
            List<eAdminLanguage> listLanguages = new List<eAdminLanguage>();
            eAdminLanguage lang;
            DataTableReaderTuned dtr;
            RqParam rqParam = new RqParam();
            eudoDAL eDal = null;

            try
            {
                eDal = eLibTools.GetEudoDAL(pref);
                eDal.OpenDatabase();

                sbQuery.AppendLine("SELECT LANG_ID, LANG_SYSID, LANG_LABEL, [Disabled], LANG_DEFAULT FROM MAPLANG ");
                if (bEnabledOnly)
                    sbQuery.AppendLine("WHERE ISNULL([Disabled], 0) = 0 AND ISNULL(Lang_Label, '') <> '' ");
                sbQuery.AppendLine("ORDER BY LANG_ID ASC");
                query = sbQuery.ToString();
                rqParam = new RqParam(query);
                dtr = eDal.Execute(rqParam, out error);

                if (!String.IsNullOrEmpty(error))
                {
                    throw new EudoAdminSQLException("eAdminLanguage.Load", error);
                }

                while (dtr.Read())
                {
                    lang = new eAdminLanguage(dtr.GetInt32("LANG_ID"), dtr.GetInt32("LANG_SYSID"), dtr.GetString("LANG_LABEL"), dtr.GetBoolean("Disabled"), dtr.GetBoolean("LANG_DEFAULT"));
                    listLanguages.Add(lang);
                }
            }
            //catch (Exception exc)
            //{
            //    String test = exc.Message;
            //}
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();
            }

            return listLanguages;


        }


        public void Add()
        {

        }

        /// <summary>
        /// Suppression d'une langue
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="id"></param>
        public static void Delete(ePref pref, int id)
        {
            eudoDAL eDal = null;
            String query, error;
            RqParam rqParam;

            try
            {
                eDal = eLibTools.GetEudoDAL(pref);
                eDal.OpenDatabase();

                query = String.Concat("UPDATE [MAPLANG] SET LANG_LABEL = NULL, LANG_SYSID = NULL WHERE Lang_ID = @id");
                rqParam = new RqParam(query);
                rqParam.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                eDal.ExecuteNonQuery(rqParam, out error);

                if (!String.IsNullOrEmpty(error))
                {
                    throw new EudoAdminSQLException("eAdminLanguage.Delete", error);
                }

                // TODO : Vider les traductions
            }
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();
            }
        }

        /// <summary>
        /// Mise à jour de la langue
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="id"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public static void Save(ePref pref, int id, MAPLANG_FIELD field, String value)
        {
            eudoDAL eDal = null;
            String query, error;
            RqParam rqParam;

            try
            {
                eDal = eLibTools.GetEudoDAL(pref);
                eDal.OpenDatabase();

                query = String.Concat("UPDATE [MAPLANG] SET ", field.ToString(), " = '", value, "' WHERE Lang_ID = @id");
                rqParam = new RqParam(query);
                rqParam.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                eDal.ExecuteNonQuery(rqParam, out error);

                if (!String.IsNullOrEmpty(error))
                {
                    throw new EudoAdminSQLException("eAdminLanguage.Save", error);
                }
            }
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();
            }

        }
    }
}