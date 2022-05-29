using System;
using System.IO;
using System.Linq;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Surcharge de la page de tracking de l'application Eudonet pour la gestion des trackings externalisés
    /// Particularité : cette page est appelée depuis l'extérieur sans authentification !
    /// </summary>
    public partial class ednexternal : edn
    {
        private eMailAddressInfos _mailValue = null;
        private int _nCatId = 0;
        private string _sCatName = string.Empty;

        /// <summary>
        /// Chargement de  MailTabName / CampaignId
        /// </summary>
        /// <param name="mailTabName"></param>
        protected override void LoadInfosEdn(out string mailTabName)
        {
            //non relevant pour la gestion externalisé
            mailTabName = "";

            string cpid;
            dicAddedParam.TryGetValue("cid", out cpid);
            _campaignId = eLibTools.GetNum(cpid);

            string mail;
            dicAddedParam.TryGetValue("mail", out mail);
            eMailAddressConteneur conteneurMail = new eMailAddressConteneur(mail);
            if (conteneurMail.AllDestAreEmptyOrInvalid)
                _mailValue = conteneurMail.FirstAddress;

            dicAddedParam.TryGetValue("cat", out _sCatName);

            string scatid;
            dicAddedParam.TryGetValue("catid", out scatid);
            _nCatId = eLibTools.GetNum(scatid);
        }

        /// <summary>
        /// Pour les liens de confirmation de lecture, enregistre l'appel comme pour un tracking
        /// </summary>
        /// <param name="lnk"></param>
        protected override void UpdateRead(Merge.ExternalUrlParamMailingTrack lnk)
        {

        }

        /// <summary>
        /// Retourne un statupdate pour la gestion externalisé
        /// </summary>
        /// <param name="sMailTabName"></param>
        /// <returns></returns>
        protected override Merge.StatsUpdate GetStatsUpdate(string sMailTabName)
        {
            string Serial;
            dicAddedParam.TryGetValue("ser", out Serial);

            Path.GetInvalidFileNameChars().Aggregate("filename", (current, c) => current.Replace(c.ToString(), string.Empty));

            return new Merge.StatsUpdateExternal(_dalClient, _campaignId, Serial,
                _pageQueryString.UID,_pageQueryString.Cs, _pageQueryString.Token, _pageQueryString.P);
        }


        /// <summary>
        /// Construction d'un LoadExternalInfosUnsub  pour la désinscription externalisée
        /// </summary>
        /// <param name="mailTabName"></param>
        /// <returns></returns>
        protected override LoadExternalInfosUnsub GetLoadExternalInfosUnsub(string mailTabName)
        {
            return new LoadExternalInfosUnsub(_mailValue, _nCatId, _sCatName, _campaignId);

            // return new LoadExternalInfosUnsub();
            //public LoadExternalInfosUnsub(string sMailValue, int nCategoryId, string sCategoryName, int campaignId)
        }


        /// <summary>
        /// Dans le cas du tracking externalisé, ne fait rien
        /// Le traitemetn sera réalisé par le site internet d'origine
        /// </summary>
        /// <param name="lnk"></param>
        /// <param name="mailTabName"></param>
        /// <param name="nTplFileId"></param>
        /// <param name="nEvtFileId"></param>
        protected override void UpdateMainTable(Merge.ExternalUrlParamMailingTrack lnk, string mailTabName, out int nTplFileId, out int nEvtFileId)
        {
            //

            nTplFileId = 0;
            nEvtFileId = 0;
            //base.UpdateMainTable(lnk, mailTabName, out nTplFileId, out nEvtFileId);
        }
    }
}