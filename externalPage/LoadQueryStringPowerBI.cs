using System;
using Com.Eudonet.Merge;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>LoadQueryStringPowerBI</className>
    /// <summary>Représente les données transmises en QueryString à la page d'affichage de PowerBI</summary>
    /// <purpose></purpose>
    /// <authors>MAB</authors>
    /// <date>2018-03-05</date>
    public class LoadQueryStringPowerBI : LoadQueryString<ExternalUrlTokenPowerBI, ExternalUrlParamPowerBI>
    {
        /// <summary>
        /// Constructeur - Charge et analyse la querystring
        /// </summary>
        /// <param name="uid">identifiant de la base</param>
        /// <param name="csCrypt">token de sécurité</param>
        /// <param name="paramCrypt">paramètres</param>
        public LoadQueryStringPowerBI(string uid, string csCrypt, string paramCrypt)
            : base(uid, csCrypt, paramCrypt)
        {
            // Données non identiques, on stoppe le chargement
            if (ParamData == null || CsData == null ||
                ParamData.TabDescId == 0 || ParamData.TabDescId != CsData.TabDescId ||
                ParamData.ReportId == 0 || ParamData.ReportId != CsData.ReportId ||
                ParamData.UserId == 0 || ParamData.UserId != CsData.UserId ||
                ParamData.UserLangId != CsData.UserLangId ||
                ParamData.Ticks == 0
            )
            {
#if DEBUG
                eModelTools.EudoTraceLog(
                    String.Concat(
                        "LoadQueryStringPowerBI >> Mauvais TabDescId et/ou ReportId et/ou UserId et/ou UserLangId ou Ticks vide : ",
                        ParamData != null && CsData != null ?
                            String.Concat(
                                ParamData.TabDescId, " != ", CsData.TabDescId, " / ",
                                ParamData.ReportId, " != ", CsData.ReportId, " / ",
                                ParamData.UserId, " != ", CsData.UserId, " / ",
                                ParamData.UserLangId, " != ", CsData.UserLangId
                            ) : String.Empty
                    )
                );
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
            this.CsData = ExternalUrlTokenPowerBI.LoadToken(cs);
        }

        /// <summary>
        /// Charge des paramètres
        /// </summary>
        /// <param name="param">param crypté venant de la querystring</param>
        protected override void LoadSetParam(string param)
        {
            this.ParamData = (ExternalUrlParamPowerBI)ExternalUrlTools.LoadParam(param);
        }
    }
}