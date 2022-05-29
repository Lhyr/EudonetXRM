using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Classes
{
    /// <summary>
    /// Class pour vérifier l'utilisation d'une rubrique dans les filtres/ORM/Rapports/Formules
    /// </summary>
    public class eAdminMappingExtendedField
    {

        /// <summary>
        /// Permet de vérifier si la rubrique est utilisée dans les filtres/ORM/Rapports/Formules
        /// </summary>
        /// <param name="pref">Pref de l'utilisateur</param>
        /// <param name="descIdField">Descid de la rubrique à vérifier</param>
        /// <returns></returns>
        public static eAdminResult CheckUsedField(ePref pref, Int32 descIdField)
        {
            eAdminResult result = new eAdminResult();
            StringBuilder sbUserMsg = new StringBuilder();
            StringBuilder sbDebugMsg = new StringBuilder();

            if (pref == null || pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
            {
                throw new EudoAdminInvalidRightException();
            }

            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                dal.OpenDatabase();
                result.Success = true;
                eAdminFieldInfos adminField = eAdminFieldInfos.GetAdminFieldInfos(pref, descIdField);
                result = adminField.CheckInvolving(dal);
                if (!result.Success)
                    sbUserMsg.AppendLine(result.UserErrorMessage);
                result.UserErrorMessage = sbUserMsg.ToString();
            }
            catch (Exception e)
            {
                result.Success = false;
                result.UserErrorMessage = eResApp.GetRes(pref, 8666);
                result.DebugErrorMessage = e.Message;

            }
            finally
            {
                dal?.CloseDatabase();
            }

            return result;
        }

        /// <summary>
        /// Permt de vider le contenu d'une rubrique
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="descIdField"></param>
        /// <returns></returns>
        public static eAdminResult UpdateFieldContent(ePref pref, Int32 descIdField)
        {
            eAdminResult result = new eAdminResult();
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            StringBuilder sb = new StringBuilder("UPDATE ");
            string sError = string.Empty;
            result.Success = true;
            if (pref == null || pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
            {
                throw new EudoAdminInvalidRightException();
            }

            try
            {
                dal.OpenDatabase();
                eAdminFieldInfos adminField = eAdminFieldInfos.GetAdminFieldInfos(pref, descIdField);
                sb.Append(adminField.TableName)
                    .Append(" SET [")
                    .Append(adminField.FieldName)
                    .Append("] = NULL");
                RqParam rq = new RqParam(sb.ToString());

                dal.ExecuteNonQuery(rq, out sError);

                if (sError.Length > 0)
                    throw dal.InnerException ?? new Exception(sError);

            }
            catch (Exception e)
            {
                result.Success = false;
                result.UserErrorMessage = eResApp.GetRes(pref, 8666);
                result.DebugErrorMessage = e.Message;
                result.InnerException = e;

            }
            finally
            {
                dal?.CloseDatabase();
            }

            return result;
        }

    }
}