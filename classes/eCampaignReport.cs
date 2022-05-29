using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe permettant de recenser les données pour construire le tableau de bilan de la campagne
    /// </summary>
    public class eCampaignReport
    {
        private Int32 _campaignId = 0;
        private ePref _ePref;
        private eudoDAL _eDal;
        private StringBuilder sbWarning = new StringBuilder();

        /// <summary>contient les valeurs nécessaires à la construction du cadre "bilan de la campagne"</summary>
        public Dictionary<CAMPAIGNSTATS_Category, Int32> DiGlobalStats { get; set; }

        /// <summary>contient les valeurs nécessaires à la construction du cadre "Taux d'ouverture"</summary>
        public Dictionary<String, Int32> DiReadingRate { get; set; }

        /// <summary>contient les valeurs nécessaires à la construction du cadre "Motifs de rejets"</summary>
        public Dictionary<String, Int32> DiUnreceiveCause { get; set; }

        /// <summary>contient les valeurs nécessaires à la construction du cadre "Désinscrits"</summary>
        public Dictionary<String, Int32> DiUnsubscribed { get; set; }

        /// <summary>Nombre de clics par lien</summary>
        public Dictionary<string, int> DiClics { get; set; }

        /// <summary>Nombre de clics par jour</summary>
        public Dictionary<string, int> DiClicsPerDay { get; set; }



        /// <summary>erreur potentiellement rencontrée lors de la construction d' l'objet</summary>
        public Dictionary<String, Exception> Errors { get; set; }
        /// <summary>Permet de transmettre un avertissement au support</summary>
        public String Warning { get { return sbWarning.ToString(); } }

        private eCampaignReport(ePref pref, Int32 nCampaignId)
        {

            _ePref = pref;
            _campaignId = nCampaignId;
            DiGlobalStats = new Dictionary<CAMPAIGNSTATS_Category, Int32>();
        }

        /// <summary>
        /// Classe statique permettant d'obtenir l'objet de données qui fournit les statistiques de la campagne.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nCampaignId"></param>
        /// <returns></returns>
        public static eCampaignReport CreateCampaignReport(ePref pref, Int32 nCampaignId)
        {
            eCampaignReport campaignReport = new eCampaignReport(pref, nCampaignId);

            if (!campaignReport.InitConnection())
                return campaignReport;

            campaignReport.GetGlobalStats();
            campaignReport.GetReadingRateStats();
            campaignReport.GetUnreceiveCauseStats();
            campaignReport.GetUnsubscribeStats();
            campaignReport.GetClics();
            campaignReport.GetClicsPerDay();

            campaignReport.CloseConnection();

            return campaignReport;
        }


        /// <summary>
        /// initialise la connection via eudoDal
        /// </summary>
        private Boolean InitConnection()
        {
            Errors = new Dictionary<string, Exception>();
            try
            {
                _eDal = eLibTools.GetEudoDAL(_ePref);

                _eDal.OpenDatabase();
            }
            catch (Exception e)
            {
                Errors.Add("eCampaignReport.InitConnection() : ", e);
                return false;
            }

            return true;

        }

        /// <summary>
        /// ferme la connexion
        /// </summary>
        private void CloseConnection()
        {

            _eDal.CloseDatabase();
        }

        /// <summary>
        /// Données nécessaires au bilan de la campagne
        /// </summary>
        /// <returns></returns>
        private void GetGlobalStats()
        {

            String strSQL = "SELECT Category, SUM(Number) AS Number FROM CampaignStats WHERE EvtId = @campaignId AND Category NOT IN (@CatNone, @CatNbClicPerDay) GROUP BY Category";
            RqParam rqReport = new RqParam(strSQL);
            rqReport.AddInputParameter("@campaignId", SqlDbType.Int, _campaignId);
            rqReport.AddInputParameter("@CatNone", SqlDbType.Int, CAMPAIGNSTATS_Category.NONE.GetHashCode());
            rqReport.AddInputParameter("@CatNbClicPerDay", SqlDbType.Int, CAMPAIGNSTATS_Category.NB_CLICK_PER_DAY.GetHashCode());


            try
            {
                String sError;
                DataTableReaderTuned dtrReport = _eDal.Execute(rqReport, out sError);
                if (sError.Length > 0)
                    throw new Exception(sError);

                Int32 iCategory = 0;
                CAMPAIGNSTATS_Category category = CAMPAIGNSTATS_Category.NONE;
                Int32 iNumber = 0;
                try
                {
                    while (dtrReport.Read())
                    {
                        if (!Int32.TryParse(dtrReport.GetString("Category"), out iCategory))
                            continue;

                        if (!Int32.TryParse(dtrReport.GetString("Number"), out iNumber))
                            continue;

                        try
                        {
                            category = (CAMPAIGNSTATS_Category)iCategory;
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (DiGlobalStats.ContainsKey(category))
                            sbWarning.Append("Il y a plusieurs lignes dans Statistiques de Campagne pour la catégorie ")
                                .Append(category).Append(" et pour la campagne dont l'id est ")
                                .AppendLine(_campaignId.ToString());
                        else
                            DiGlobalStats.Add(category, iNumber);

                    }
                }
                catch  
                {
                    throw  ;
                }
                finally
                {
                    dtrReport?.Dispose();
                }
            }
            catch (Exception e)
            {
                Errors.Add("eCampaignReport.GetGlobalStats()", e);
            }
        }

        /// <summary>
        /// Données nécessaires à la génération du graph de taux d'ouverture
        /// </summary>
        private void GetReadingRateStats()
        {
            Int32 nbRead = 0, nbTotal = 0;
            DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_VIEW, out nbRead);
            DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_TOTAL, out nbTotal);

            DiReadingRate = new Dictionary<String, Int32>();
            DiReadingRate.Add(eResApp.GetRes(_ePref, 6467), nbRead); //Ouverts
            DiReadingRate.Add(eResApp.GetRes(_ePref, 6473), nbTotal - nbRead); //Non-Ouverts

        }
        /// <summary>
        /// Obtient les Données permettant de générer le graph des motifs de rejets
        /// </summary>
        private void GetUnreceiveCauseStats()
        {
            DiUnreceiveCause = new Dictionary<string, int>();

            String sSql = new StringBuilder("SELECT isnull(RES.[LANG],[SubCategory]),isnull(RES2.[LANG],'') ToolTip, Number ")
                .Append("FROM CampaignStats ")
                .Append("LEFT JOIN eudores..RESAPP RES ON RES.Id_Lang=@lg AND RES.resid = case SubCategory ")
                //Bounce Catgory
                .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.TEMPORARY).Append("' then 6681 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.SOFT).Append("' then 2921 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.HARD).Append("' then 2922 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.BLOCKED_CP).Append("' then 2923 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.NO_BOUNCE_RESULT).Append("' then 2941 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.AUTOREPLAY).Append("' then 2924 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.GENERIC).Append("' then 2925 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.UNKNOWN_CP).Append("' then 6677 ")

                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.INVALID_ADDRESS).Append("' then 6688 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.TEMP_COMMUNICATION_FAILURE).Append("' then 6687 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.TRANSIENT).Append("' then 6686 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.REJECTED).Append("' then 6685 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.BLOCKED).Append("' then 6684 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.UNKNOWN).Append("' then 1747 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.COMPLAINT).Append("' then 6683 ")
                    .Append(" else NULL end ")  //Valeur affichée
                .Append("LEFT JOIN eudores..RESAPP RES2 ON RES2.Id_Lang=@lg AND RES2.resid = case SubCategory ")
                //Bounce Catgory
                .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.TEMPORARY).Append("' then 6681 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.SOFT).Append("' then 2921 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.HARD).Append("' then 2922 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.NO_BOUNCE_RESULT).Append("' then 2941 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.BLOCKED_CP).Append("' then 2923 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.AUTOREPLAY).Append("' then 2924 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.GENERIC).Append("' then 2925 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.UNKNOWN_CP).Append("' then 6677 ")

                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.INVALID_ADDRESS).Append("' then 6682 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.TEMP_COMMUNICATION_FAILURE).Append("' then 6681 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.TRANSIENT).Append("' then 6680 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.REJECTED).Append("' then 6679 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.BLOCKED).Append("' then 6678 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.UNKNOWN).Append("' then 6677 ")
                    .Append(" when '").Append(CAMPAIGNSTATS_SUBCATEGORY.COMPLAINT).Append("' then 6676 ")
                    .Append(" else NULL end ")  //Valeur au survole
                .Append(" WHERE Category = @Bounce AND EvtId = @CampaignId ORDER BY Number DESC").ToString();
            RqParam rqUnreceived = new RqParam(sSql);
            rqUnreceived.AddInputParameter("@CampaignId", SqlDbType.Int, _campaignId);
            rqUnreceived.AddInputParameter("@Bounce", SqlDbType.Int, CAMPAIGNSTATS_Category.NB_BOUNCE);
            rqUnreceived.AddInputParameter("@lg", SqlDbType.Int, _ePref.LangServId);

            try
            {
                String sError;
                DataTableReaderTuned dtrUnreceived = _eDal.Execute(rqUnreceived, out sError);
                if (sError.Length > 0)
                    throw new Exception(sError);

                String sSubCategory = String.Empty;
                Int32 iNumber = 0, nTotalBounce = 0;
                try
                {
                    while (dtrUnreceived.Read())
                    {
                        if (!dtrUnreceived.IsDBNull(0))
                            sSubCategory = dtrUnreceived.GetString(0);
                        if ((!dtrUnreceived.IsDBNull(1)) && dtrUnreceived.GetString(1).Length > 0)
                            sSubCategory = String.Concat(sSubCategory, SEPARATOR.LVL1, dtrUnreceived.GetString(1));

                        if (!Int32.TryParse(dtrUnreceived.GetString("Number"), out iNumber))
                            continue;

                        if (DiUnreceiveCause.ContainsKey(sSubCategory))
                            sbWarning.Append("Il y a plusieurs lignes dans Statistiques de Campagne pour la catégorie Bounce dont le motif de rejet est ")
                                .Append(sSubCategory).Append(" et pour la campagne dont l'id est ")
                                .AppendLine(_campaignId.ToString());
                        else
                            DiUnreceiveCause.Add(sSubCategory, iNumber);
                        nTotalBounce = nTotalBounce + iNumber;
                    }
                    //Afficher Pas de retours d'erreur =NB_TOTAL-NON REMIS
                    Int32 nbTotalWithoutReturn;
                    if (DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_TOTAL, out nbTotalWithoutReturn))
                    {
                        nbTotalWithoutReturn = nbTotalWithoutReturn - nTotalBounce;
                        DiUnreceiveCause.Add(eResApp.GetRes(_ePref, 1751), nbTotalWithoutReturn);
                    }
                }
                catch  
                {
                    throw;
                }
                finally
                {
                    dtrUnreceived?.Dispose();
                }
            }
            catch (Exception e)
            {
                Errors.Add("eCampaignReport.GetUnreceiveCauseStats()", e);
            }


        }

        /// <summary>
        /// Données nécessaires à la génération du graph de taux de désinscriptions
        /// </summary>
        private void GetUnsubscribeStats()
        {
            Int32 nbUnsubscribed = 0, nbTotal = 0, nbBounce = 0;
            DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_UNSUBSCRIBE, out nbUnsubscribed);
            DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_TOTAL, out nbTotal);
            DiGlobalStats.TryGetValue(CAMPAIGNSTATS_Category.NB_BOUNCE, out nbBounce);

            DiUnsubscribed = new Dictionary<String, Int32>();
            DiUnsubscribed.Add(eResApp.GetRes(_ePref, 6466), nbUnsubscribed); //désinscrits
            DiUnsubscribed.Add(String.Empty, nbTotal - nbBounce - nbUnsubscribed); //restant

        }


        /// <summary>
        /// Obtient le nombre de clics par liens
        /// </summary>
        private void GetClics()
        {
            DiClics = new Dictionary<String, Int32>();

            String sSql = "SELECT Label, NumberOfClicks FROM TrackLink where EvtId = @CampaignId ORDER BY NumberOfClicks DESC";
            RqParam rqClics = new RqParam(sSql);
            rqClics.AddInputParameter("@CampaignId", SqlDbType.Int, _campaignId);

            try
            {
                String sError;
                DataTableReaderTuned dtrClics = _eDal.Execute(rqClics, out sError);
                if (sError.Length > 0)
                    throw new Exception(sError);

                String sLabel = String.Empty;
                Int32 iNumber = 0;
                try
                {
                    while (dtrClics.Read())
                    {
                        if (!dtrClics.IsDBNull("Label"))
                            sLabel = dtrClics.GetString("Label");

                        if (!Int32.TryParse(dtrClics.GetString("NumberOfClicks"), out iNumber))
                            continue;

                        Int32 i = 0;
                        while (DiClics.ContainsKey(sLabel))
                        {
                            i++;
                            sLabel = String.Concat(sLabel, " (", i, ")");
                        }

                        DiClics.Add(sLabel, iNumber);

                    }
                }
                catch  
                {
                    throw;
                }
                finally
                {
                    dtrClics?.Dispose();
                }
            }
            catch (Exception e)
            {
                Errors.Add("eCampaignReport.GetClics()", e);
            }


        }

        /// <summary>
        /// Obtient le nombre de clics par liens
        /// </summary>
        private void GetClicsPerDay()
        {
            DiClicsPerDay = new Dictionary<String, Int32>();

            String sSql = "SELECT Convert(varchar(10), Date, 103) as Dt, Number FROM CampaignStats where EvtId = @CampaignId AND Category = @ClicPerDay ORDER BY Date";
            RqParam rqClicsPerDay = new RqParam(sSql);
            rqClicsPerDay.AddInputParameter("@CampaignId", SqlDbType.Int, _campaignId);
            rqClicsPerDay.AddInputParameter("@ClicPerDay", SqlDbType.Int, CAMPAIGNSTATS_Category.NB_CLICK_PER_DAY.GetHashCode());

            try
            {
                String sError;
                DataTableReaderTuned dtrClicsPerDay = _eDal.Execute(rqClicsPerDay, out sError);
                if (sError.Length > 0)
                    throw new Exception(sError);

                String sDate = String.Empty;
                Int32 iNumber = 0;
                try
                {
                    while (dtrClicsPerDay.Read())
                    {
                        if (!dtrClicsPerDay.IsDBNull("Dt") && dtrClicsPerDay.GetString("Dt").Length > 5)
                            sDate = dtrClicsPerDay.GetString("Dt").Substring(0, 5);

                        if (!Int32.TryParse(dtrClicsPerDay.GetString("Number"), out iNumber))
                            continue;


                        if (DiClicsPerDay.ContainsKey(sDate))
                            sbWarning.Append("Il y a plusieurs lignes dans Statistiques de Campagne pour la catégorie Clic Per Day et pour la date du ")
                                .Append(sDate).Append(" et pour la campagne dont l'id est ")
                                .AppendLine(_campaignId.ToString());
                        else
                            DiClicsPerDay.Add(sDate, iNumber);

                    }
                }
                catch  
                {
                    throw;
                }
                finally
                {
                    dtrClicsPerDay?.Dispose();
                }
            }
            catch (Exception e)
            {
                Errors.Add("eCampaignReport.GetClicsPerDay()", e);
            }


        }


    }
}