using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <className>LoadUnsubInfos</className>
    /// <summary>Charge les informations de catégorie de la campagne et le mail du receveur pour la désinscription</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-03-10</date>
    public class LoadExternalInfosUnsub : LoadExternalInfos
    {

        /// <summary>
        /// Répresente un record du type de média de la campagne
        /// </summary>
        public eFieldRecord FldCampMediaType { get; private set; }
        /// <summary>
        /// Répresente un record de la catégorie de la campagne
        /// </summary>
        public eFieldRecord FldCampCategory { get; private set; }
        /// <summary>
        /// Mail concerné
        /// </summary>
        public eMailAddressInfos FldMailAValue { get; private set; }

        /// <summary>
        /// Liste des erreur rencontrées
        /// </summary>
        public ICollection<Exception> SilentExp { get; protected set; }

        /// <summary>
        /// Id de la fiche PP parente
        /// </summary>
        public int PpId { get; private set; }
        /// <summary>
        /// Id de la fiche PM parente
        /// </summary>
        public int PmId { get; private set; }
        /// <summary>
        /// Id de la fiche ADDRESS parente
        /// </summary>
        public int AdrId { get; private set; }
        /// <summary>
        /// Id de la fiche EVENT parente
        /// </summary>
        public int EvtId { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="dal">connexion à la base</param>
        /// <param name="pref">preflite avec le userid de EDN_TRACKS</param>
        /// <param name="unsubParam">Information sur le lien de désinscription</param>
        /// <param name="mailTabName">Nom de la main table (invitation, pp, etc.)</param>
        /// <param name="campaignId">Id de la campagne</param>
        /// <param name="isEdnUser">Indique que l'utilisateur est un utilisateur système "EDN_xxxx"</param>
        public LoadExternalInfosUnsub(eudoDAL dal, ePrefLite pref, ExternalUrlParamMailing unsubParam, String mailTabName, Int32 campaignId, Boolean isEdnUser)
            : base(dal, pref, unsubParam, campaignId)
        {
            SilentExp = null;

            InitParentIds();

            if (unsubParam.ParamType != Common.Enumerations.EudonetMailingBuildParamType.EXTRANET)
            {
                try
                {
                    // Type de média de la campagne
                    // Remarque : EDN_TRACKS doit avoir les droits de visu sur cette rubrique !
                    FldCampMediaType = GetFieldValue(CampaignField.MEDIATYPE.GetHashCode(), campaignId, isEdnUser);
                }
                catch (Exception exp)
                {
                    if (SilentExp == null)
                        SilentExp = new List<Exception>();
                    SilentExp.Add(exp);
                }

                try
                {
                    // Catégorie de désinscription de la campagne
                    // Remarque : EDN_TRACKS doit avoir les droits de visu sur cette rubrique !
                    FldCampCategory = GetFieldValue(CampaignField.CATEGORY.GetHashCode(), campaignId, isEdnUser);
                }
                catch (Exception exp)
                {
                    if (SilentExp == null)
                        SilentExp = new List<Exception>();
                    SilentExp.Add(exp);
                }

                try
                {
                    // Mail du destinataire
                    FldMailAValue = GetMailValue(mailTabName, unsubParam.MailId);
                }
                catch (Exception exp)
                {
                    if (SilentExp == null)
                        SilentExp = new List<Exception>();
                    SilentExp.Add(exp);
                }
            }
            else
            {

                try
                {
                    // Mail du destinataire
                    FldMailAValue = GetMailValue(mailTabName, unsubParam.MailId, unsubParam.MailDecid, unsubParam.ParamType);
                    if(FldMailAValue == null)
                    {
                        FldMailAValue = new eMailAddressInfos();
                        FldMailAValue.Mail = "-";
                    }
                }
                catch (Exception exp)
                {
                    FldMailAValue = new eMailAddressInfos();
                    FldMailAValue.Mail = "-";

                }


            }


            try
            {
                // Ids Parents
                GetParentIds(mailTabName, unsubParam.MailId, unsubParam.ParamType);
            }
            catch (Exception exp)
            {
                if (SilentExp == null)
                    SilentExp = new List<Exception>();
                SilentExp.Add(exp);
            }
        }

        /// <summary>
        /// Retourne le mail principale des destinataires
        /// </summary>
        /// <returns></returns>
        private eMailAddressInfos GetMailValue(String mailTabName, Int32 mailId, int mailDescid = 0, EudonetMailingBuildParamType paramType = EudonetMailingBuildParamType.STANDARD)
        {
            String sql = "";


            if (paramType != EudonetMailingBuildParamType.EXTRANET)
            {
                sql = new StringBuilder()
                        .Append("SELECT isnull([TPL").Append(MailField.DESCID_MAIL_TO.GetHashCode().ToString("00")).Append("], '')")
                        .Append(" FROM [").Append(mailTabName).Append("]")
                        .Append(" WHERE [TplId] = @mailid")
                        .ToString();
            }
            else
            {
                string sAdrField = "";

                switch (mailDescid - mailDescid % 100)
                {
                    case 400:
                        sAdrField = "ADR" + (mailDescid % 100).ToString("00");
                        break;
                    case 300:
                        sAdrField = "PM" + (mailDescid % 100).ToString("00");
                        break;
                    case 200:
                        sAdrField = "PP" + (mailDescid % 100).ToString("00");
                        break;
                    default:
                        throw new Exception(String.Concat("Mail du destinataire non trouvé : ", mailDescid));

                }

                sql = new StringBuilder()
                           .Append("SELECT TOP 1 isnull([").Append(sAdrField).Append("], '')")
                           .Append(" FROM [ADDRESS]")
                           .Append(" LEFT JOIN [PP] ON [PP].[PPID] = [ADDRESS].[PPID]")
                           .Append(" LEFT JOIN [PM] ON [PM].[PMID] = [ADDRESS].[PMID]")
                           .Append(" WHERE [ADRID] = @mailid")
                           .ToString();
            }


            RqParam rq = new RqParam(sql);
            rq.AddInputParameter("@mailid", SqlDbType.Int, mailId);

            String error = String.Empty;
            String fldMailAValue = _dal.ExecuteScalar<String>(rq, out error);
            if (error.Length != 0)
                throw new Exception(String.Concat("Mail du destinataire non trouvé. ", error));

            eMailAddressConteneur conteneurTo = new eMailAddressConteneur(fldMailAValue);
            if (conteneurTo.AllDestAreEmptyOrInvalid && paramType != EudonetMailingBuildParamType.EXTRANET)
                throw new Exception(String.Concat("Mail du destinataire non trouvé : ", fldMailAValue));

            return conteneurTo.FirstAddress;
        }

        /// <summary>
        /// Initialisation des variables Id parent
        /// </summary>
        private void InitParentIds()
        {
            this.PpId = this.PmId = this.AdrId = this.EvtId = 0;
        }

        /// <summary>
        /// Retourne les id des fiches parentes du mail
        /// </summary>
        /// <param name="mailTabName">Nom de la table Email</param>
        /// <param name="mailId">Id de la fiche Email</param>
        private void GetParentIds(String mailTabName, Int32 mailId, EudonetMailingBuildParamType paramType)
        {
            String sql = "";


            if (paramType != EudonetMailingBuildParamType.EXTRANET)
            {
                sql = new StringBuilder()
                        .Append("SELECT isnull([PpId], 0)")
                        .Append(", isnull([PmId], 0)")
                        .Append(", isnull([AdrId], 0)")
                        .Append(", isnull([EvtId], 0)")
                        .Append(" FROM [").Append(mailTabName).Append("]")
                        .Append(" WHERE [TplId] = @mailid")
                        .ToString();
            }
            else
            {

                sql = new StringBuilder()
                        .Append("SELECT isnull([PpId], 0)")
                        .Append(", isnull([PmId], 0)")
                        .Append(", isnull([AdrId], 0)")
                        .Append(", 0 as EVTID")
                        .Append(" FROM [ADDRESS]")
                        .Append(" WHERE [ADRID] = @mailid")
                        .ToString();

            }

            RqParam rq = new RqParam(sql);
            rq.AddInputParameter("@mailid", SqlDbType.Int, mailId);

            String error = String.Empty;

            DataTableReaderTuned dtr = _dal.Execute(rq, out error);
            try
            {
                if (dtr == null || error.Length != 0)
                    throw new Exception(String.Concat("Id parents du destinataire non trouvé. ", error));

                if (dtr.Read())
                {
                    this.PpId = dtr.GetEudoNumeric(0);
                    this.PmId = dtr.GetEudoNumeric(1);
                    this.AdrId = dtr.GetEudoNumeric(2);
                    this.EvtId = dtr.GetEudoNumeric(3);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dtr?.Dispose();
            }
        }

        /// <summary>
        /// Constructeur pour les mail
        /// </summary>
        /// <param name="sMailValue"></param>
        /// <param name="nCategoryId"></param>
        /// <param name="sCategoryName"></param>
        /// <param name="campaignId"></param>
        public LoadExternalInfosUnsub(eMailAddressInfos sMailValue, Int32 nCategoryId, String sCategoryName, Int32 campaignId)
            : base(null, null, null, campaignId)
        {
            FldMailAValue = sMailValue;
            FldCampCategory = new eFieldRecord();
            FldCampCategory.Value = nCategoryId.ToString();
            FldCampCategory.DisplayValue = sCategoryName;

            InitParentIds();
        }
    }
}