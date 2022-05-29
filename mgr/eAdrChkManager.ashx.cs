using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Data;
using System.Text;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eAdrChkManager
    /// </summary>
    public class eAdrChkManager : eEngineMgr
    {

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            StringBuilder sbError = new StringBuilder();

            Boolean bFound;
            String sNotFoundMsg = eResApp.GetRes(_pref, 278);

            eRes res = new eRes(_pref, String.Concat((int)TableType.PP, ",", (int)TableType.PM));

            String sLabel200 = res.GetRes((int)TableType.PP, out bFound);
            Int32? nPPID = _requestTools.GetRequestFormKeyI("ppid");
            if (!bFound || nPPID == null || nPPID == 0)
                sbError.AppendLine(sNotFoundMsg.Replace("<TAB>", sLabel200));

            String sLabel300 = res.GetRes(TableType.PM.GetHashCode(), out bFound);
            Int32? nPMID = _requestTools.GetRequestFormKeyI("pmid");
            if (!bFound || nPMID == null || nPMID == 0)
                sbError.AppendLine(sNotFoundMsg.Replace("<TAB>", sLabel300));

            if (sbError.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", sbError.ToString());
            }
            else
            {
                DoAsk(nPPID.Value, nPMID.Value, sLabel200, sLabel300);
            }
            LaunchError();

            RenderResult(RequestContentType.XML, delegate() { return _xmlResult.OuterXml; });
        }

        private void DoAsk(Int32 nPPID, Int32 nPMID, String sLabel200, String sLabel300)
        {
            String error = String.Empty;

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();

            try
            {
                String sql = "SELECT CASE WHEN EXISTS(SELECT ADRID FROM ADDRESS WHERE PPID = @ppid AND PMID = @pmid) THEN 1 ELSE 0 END AS ADREXST";
                RqParam rq = new RqParam(sql);
                rq.AddInputParameter("@ppid", System.Data.SqlDbType.Int, nPPID);
                rq.AddInputParameter("@pmid", System.Data.SqlDbType.Int, nPMID);

                String sResult = "0";
                DataTableReaderTuned dr = dal.Execute(rq, out error);
                try
                {
                    if (error.Length != 0)
                        throw new Exception(error); // l'exception sera récupérée ci-dessous pour être gérée

                    if (dr != null && dr.Read())
                        sResult = dr.GetString("ADREXST");
                }
                finally
                {
                    if (dr != null)
                        dr.Dispose();
                }
                _xmlResult = new XmlDocument();
                XmlNode _xmlroot = _xmlResult.CreateElement("Root");
                _xmlResult.AppendChild(_xmlroot);

                XmlNode _xmlDoesAdrExist = _xmlResult.CreateElement("AdrExists");
                _xmlDoesAdrExist.AppendChild(_xmlResult.CreateTextNode(sResult));
                _xmlroot.AppendChild(_xmlDoesAdrExist);

                XmlNode _xmlLabel200 = _xmlResult.CreateElement("Lbl200");
                _xmlLabel200.AppendChild(_xmlResult.CreateTextNode(sLabel200));
                _xmlroot.AppendChild(_xmlLabel200);

                XmlNode _xmlLabel300 = _xmlResult.CreateElement("Lbl300");
                _xmlLabel300.AppendChild(_xmlResult.CreateTextNode(sLabel300));
                _xmlroot.AppendChild(_xmlLabel300);
            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    ex.Message,
                    eResApp.GetRes(_pref, 72),
                    ex.StackTrace
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {
                dal.CloseDatabase();
            }
        }
    }
}