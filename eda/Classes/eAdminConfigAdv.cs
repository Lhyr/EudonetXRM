using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.xrm.eda;
using EudoEnum = Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe métier de maj de config adev
    /// </summary>
    public class eAdminConfigAdv
    {
        ePref _pref;

        /// <summary>
        /// Exception éventuelle
        /// </summary>
        public eAdminConfigException Exception { get; private set; }

        /// <summary>
        /// constructeur standard
        /// </summary>
        /// <param name="pref">pref admin</param>
        public eAdminConfigAdv(ePref pref)
        {
            _pref = pref;

        }

        internal eAdminResult SetParam(List<SetCAdvParam> listConfig)
        {
            eAdminResult result = new eAdminResult();

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            StringBuilder sbError = new StringBuilder();

            bool majUnicode = false;
            bool toUnicode = false;

            try
            {
                dal.OpenDatabase();
                foreach (SetCAdvParam p in listConfig)
                {
                    try
                    {

                        //validation
                        if (p.CAdvCategory == eLibConst.CONFIGADV_CATEGORY.MAIN && p.Option == Common.Enumerations.CONFIGADV.LICENSEKEY && !string.IsNullOrEmpty(p.Value))
                        {
                            try
                            {
                                var clientInfos = eClientInfos.DecryptLicenseKey(_pref, p.Value);
                            }
                            catch
                            {
                                result.Success = false;
                                result.UserErrorMessage = "License invalide. La mise à jour n'a pas été effectuée.";
                                return result;
                            }
                        }

                        if (p.Option == Common.Enumerations.CONFIGADV.FULL_UNICODE)
                        {
                            if (_pref.User.UserLevel < 99)
                                throw new EudoAdminInvalidRightException();

                            majUnicode = true;
                            toUnicode = p.Value == "1";

                        }

                        eSqlConfigAdv.SetConfigAdv(dal, p.Option, p.Value, p.CAdvCategory);
                    }
                    catch (eSqlConfigAdvException e)
                    {
                        sbError.AppendLine("****").AppendLine(e.Message).AppendLine(e.StackTrace);
                    }
                }


                if (majUnicode)
                {
                    bool bSuccess = false;
                    eAdminUnicodeTransform eTransform = eAdminUnicodeTransform.GetAdminTransform(_pref);

                    bSuccess = eTransform.Update(toUnicode);
                    if (!bSuccess)
                    {
                        eSqlConfigAdv.SetConfigAdv(dal, Common.Enumerations.CONFIGADV.FULL_UNICODE, "0", eLibConst.CONFIGADV_CATEGORY.SYSTEM);

                        result.Success = false;
                        //  result.InnerException = Exception;
                        result.UserErrorMessage = "Une Erreur est survenue lors de la converion des champs.";



                        string sError = "";
                        //Erreur - 
                        var l = eTransform.LogRq.Where(log => !log.Succes);
                        
                            result.UserMessage = "Des erreurs sont survenue, voir la liste détaillée";

                        if (_pref.User.UserLevel > 99)
                            result.DebugErrorMessage = SerializerTools.JsonSerialize(l);



                        return result;
                    }
                    else
                    {


                       



                        _pref.IsUnicode = toUnicode;

                        result.Success = true;
                        result.UserMessage = "Traitement terminé.";
                        if (_pref.User.UserLevel > 99)
                            result.UserFullMessage = SerializerTools.JsonSerialize(eTransform.LogRq);

                        return result;
                    }
                }


                if (sbError.Length == 0)
                {
                    result.Success = true;
                }
                else
                {
                    Exception = new eAdminConfigException(sbError.ToString());
                    result.Success = false;
                    result.InnerException = Exception;
                    result.UserErrorMessage = "Une Erreur est survenue durant la mise à jour de ConfigAdv";
                    result.DebugErrorMessage = sbError.ToString();
                }
            }
            catch (Exception e)
            {
                Exception = new eAdminConfigException("Une Erreur est survenue durant la mise à jour de ConfigAdv", e);
                result.Success = false;
                result.InnerException = Exception;
                result.UserErrorMessage = "Une Erreur est survenue durant la mise à jour de ConfigAdv";
                result.DebugErrorMessage = e.Message;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return result;
        }


    }


    public class eAdminConfigException : Exception
    {
        public eAdminConfigException(String msg, Exception e) : base(msg, e) { }
        public eAdminConfigException(String msg) : base(msg) { }

    }
}