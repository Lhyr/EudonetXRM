using System;
using Com.Eudonet.Merge;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>LoadQueryStringMailing</className>
    /// <summary>Représente les données transmis en QueryString à la page tracking de l'appli</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-09-22</date>
    public class LoadQueryStringMailing : LoadQueryString<ExternalUrlTokenMailing, ExternalUrlParamMailing>
    {
        /// <summary>
        /// indique si le mail est un mail unitaire
        /// </summary>
        public bool IsUnitMail { get; set; }

        /// <summary>
        /// Constructeur - Charge et analyse la querystring
        /// </summary>
        /// <param name="uid">identifiant de la base</param>
        /// <param name="csCrypt">token de sécurité</param>
        /// <param name="paramCrypt">paramètres</param>
        public LoadQueryStringMailing(string uid, string csCrypt, string paramCrypt)
            : base(uid, csCrypt, paramCrypt)
        {
            // Données non identiques, on stop le chargement
            if (ParamData.MailTabDescId == 0 || ParamData.MailTabDescId != CsData.MailTabDescId
                || ParamData.MailId == 0 || ParamData.MailId != CsData.MailId)
            {
#if DEBUG
                eModelTools.EudoTraceLog(String.Concat("LoadQueryString >> Mauvais MailTabDescId et/ou MailId : ",
                    ParamData.MailTabDescId, " != ", CsData.MailTabDescId, " / ",
                    ParamData.MailId, " != ", CsData.MailId));
#endif

                _invalidQueryString = true;
            }
        }

        /// <summary>
        /// Charge le token
        /// </summary>
        /// <param name="cs">token crypté venant de la querystring</param>
        protected override void LoadSetTokenSecu(string cs)
        {
            this.CsData = ExternalUrlTokenMailing.LoadToken(cs);
        }

        /// <summary>
        /// Charge des paramètres
        /// </summary>
        /// <param name="param">param crypté venant de la querystring</param>
        protected override void LoadSetParam(string param)
        {
            this.ParamData = (ExternalUrlParamMailing)ExternalUrlTools.LoadParam(param);
        }
    }
}