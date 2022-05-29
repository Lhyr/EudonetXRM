using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using Com.Eudonet.Merge.Eudonet;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.classes
{
    public class eUsrFrm
    {
        private string _sUid = string.Empty;
        private int _nCampaignId = -1;
        private eudoDAL _dalClient = null;
        private string _sAppExternalUrl = string.Empty;
        private ePref _pref = null;

        public eUsrFrm(ePref pref, eudoDAL dalClient, string sUid, int nCampaignId, string sAppExternalUrl)
        {
            _sUid = sUid;
            _nCampaignId = nCampaignId;
            _dalClient = dalClient;
            _sAppExternalUrl = sAppExternalUrl;
            _pref = pref;
        }

        public void GenerateMail(ExternalUrlParamMailingVisu visuParam, string mailTabName, out string bodyCss, out string svisuPnlInnerHtml, out string sPageTitle)
        {
            DataTableReaderTuned dtr = null;

            string sql = new StringBuilder()
                //SHA : tâche #1 941
                    .Append("SELECT camp.[Subject], camp.[Body], camp.[BodyCss], mail.[MergeFields]") //camp.[PreHeader]
                    .Append("   FROM [CAMPAIGN] camp ")
                    .Append("       INNER JOIN [").Append(mailTabName).Append("] mail ")
                    .Append("           ON camp.[CampaignId] = mail.[TPL").Append(MailField.DESCID_MAIL_CAMPAIGNID.GetHashCode()).Append("]")
                    .Append("   WHERE camp.[CampaignId] = @campid and mail.[TplId] = @mailid")
                    .ToString();

            RqParam rq = new RqParam(sql);
            rq.AddInputParameter("@campid", SqlDbType.Int, _nCampaignId);
            rq.AddInputParameter("@mailid", SqlDbType.Int, visuParam.MailId);

            try
            {
                string error = string.Empty;
                dtr = _dalClient.Execute(rq, out error);
                if (error.Length != 0 || dtr == null || !dtr.Read())
                    throw new TrackExp(string.Concat("Contenu de la campagne non trouvé. ", (error.Length != 0) ? error : string.Empty));

                eAnalyzerInfos objSubject = dtr.GetVarBinaryValue<eAnalyzerInfos>(0);
                eAnalyzerInfos objBody = dtr.GetVarBinaryValue<eAnalyzerInfos>(1);
                //SHA : tâche #1 941
                //eAnalyzerInfos objPreheader = dtr.GetVarBinaryValue<eAnalyzerInfos>(4);
                bodyCss = dtr.GetString(2);
                eMailMergeFields mergeFieldsInfo = dtr.GetVarBinaryValue<eMailMergeFields>(3);

                EudonetMailingBuildParam bParam = new EudonetMailingBuildParam()
                {
                    AppExternalUrl = _sAppExternalUrl,
                    Uid = _sUid,
                    MailTabDescId = visuParam.MailTabDescId,
                    MailId = visuParam.MailId,
                    // HLA - Ca n'est pas normal qu'il y est ce test, mergeFields est sensé être renseigné par EudoProcess quoi qu'il arrive !!
                    MergeValues = mergeFieldsInfo != null ? mergeFieldsInfo.dicMergeField : new Dictionary<int, string>()
                };

                try
                {
                    sPageTitle = eMergeTools.GetObjectMerge(objSubject);
                    svisuPnlInnerHtml = eMergeTools.GetBodyMerge_InactiveVisuLink(objBody, bParam);
                }
                catch (Exception exp)
                {
                    throw new TrackExp(string.Concat("Construction de la visu en erreur. ", exp.Message));
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }
        }


        public void UpdateMainTable(ExternalUrlParamMailingTrack lnk, string mailTabName, out int nTplFileId, out int nEvtFileId, out string sErrorMsg)
        {
            sErrorMsg = string.Empty;
            string error = string.Empty;
            DataTableReaderTuned dtr = null;
            eMailMergeFields mergeFieldsInfo = null;
            nTplFileId = -1;
            nEvtFileId = -1;
            string sql = new StringBuilder()
                    .Append("SELECT [MergeFields] FROM [").Append(mailTabName).Append("] WHERE [TplId] = @mailid")
                    .ToString();

            RqParam rq = new RqParam(sql);
            rq.AddInputParameter("@mailid", SqlDbType.Int, lnk.MailId);

            try
            {
                // HLA - Urgence métropole EUDO_06195 - on passe l'erreur en silencieuse
                StringBuilder errMsg = new StringBuilder();

                dtr = _dalClient.Execute(rq, out error);
                if (error.Length != 0 || dtr == null || !dtr.Read())
                {
                    // HLA - Urgence métropole EUDO_06195 - on passe l'erreur en silencieuse
                    //throw new TrackExp(string.Concat("Contenu du mergefield du mail non trouvé. ", (error.Length != 0) ? error : string.Empty));

                    errMsg.Append("UpdateMainTable - Contenu du mergefield du mail non trouvé. ").AppendLine();
                    if (error.Length != 0)
                        errMsg.Append(error).AppendLine();
                    else
                        errMsg.Append("Request : ").Append(rq.GetSqlCommandText()).AppendLine();

                    sErrorMsg = errMsg.ToString();
                    return;
                }

                mergeFieldsInfo = dtr.GetVarBinaryValue<eMailMergeFields>(0);

                if (mergeFieldsInfo == null)
                {
                    // HLA - Urgence métropole EUDO_06195 - on passe l'erreur en silencieuse
                    //throw new TrackExp("Contenu du mergefield du mail non trouvé. mergeFieldsInfo vide. ");

                    errMsg.Append("UpdateMainTable - Contenu du mergefield du mail non trouvé. mergeFieldsInfo vide. ").AppendLine()
                        .Append("Request : ").Append(rq.GetSqlCommandText()).AppendLine();

                    sErrorMsg = errMsg.ToString();
                    return;
                }
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }

            nTplFileId = mergeFieldsInfo.mainFileId;
            nEvtFileId = mergeFieldsInfo.parentFileId;
            if (lnk.DescId == 0 || eLibTools.GetTabFromDescId(lnk.DescId) != mergeFieldsInfo.mainTabId)
                return;

            FieldLite fld = new FieldLite(lnk.DescId);
            fld.ExternalLoadInfo(_dalClient, out error);

            if (error.Length != 0)
            {
                sErrorMsg = new StringBuilder()
                    .Append("UpdateMainTable - Informations sur le Field non accessibles. ").AppendLine()
                    .Append(error).Append(" ").AppendLine()
                    .Append(lnk).ToString();
            }

            if (fld.Format != FieldFormat.TYP_BIT && fld.Format != FieldFormat.TYP_NUMERIC)
                return;
            
            Engine.Engine eng = eModelTools.GetEngine(_pref, mergeFieldsInfo.mainTabId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = mergeFieldsInfo.mainFileId;
            if (fld.Format == FieldFormat.TYP_BIT)
                eng.AddNewValue(lnk.DescId, "1");
            else if (fld.Format == FieldFormat.TYP_NUMERIC)
                eng.AddMetaValue(lnk.DescId, "<INC>1");
            eng.EngineProcess(new StrategyCruSimpleEdnUser());

            // Erreur silencieuse : on envoie un feedback mais on continue le traitement pour la mise à jour des stats
            string engError = null;
            if (eng.Result == null)
                engError = "Pas de retour de l'Engine.";
            else if (!eng.Result.Success)
            {
                if (eng.Result.Error == null)
                    engError = "Erreur sur Engine inconnue.";
                else
                    engError = eng.Result.Error.DebugMsg;
            }

            if (engError != null)
            {
                sErrorMsg = new StringBuilder()
                    .Append("UpdateMainTable - Mise à jour des données de l'invitation (main table) en erreur. ").AppendLine()
                    .Append(engError).Append(" ").AppendLine()
                    .Append(lnk).ToString();
            }
        }
    }
}