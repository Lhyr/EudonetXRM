using System;
using Com.Eudonet.Merge;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>LoadQueryStringPJ</className>
    /// <summary>Représente les données transmis en QueryString à la page d'affichage de pj</summary>
    /// <purpose></purpose>
    /// <authors>BBA</authors>
    /// <date>2017-07-31</date>
    public class LoadQueryStringPJ : LoadQueryString<ExternalUrlTokenPj, ExternalUrlParamPj>
    {
        /// <summary>
        /// Constructeur - Charge et analyse la querystring
        /// </summary>
        /// <param name="uid">identifiant de la base</param>
        /// <param name="csCrypt">token de sécurité</param>
        /// <param name="paramCrypt">paramètres</param>
        public LoadQueryStringPJ(string uid, string csCrypt, string paramCrypt)
            : base(uid, csCrypt, paramCrypt)
        {
            // Données non identiques, on stop le chargement
            if (ParamData.TabDescId == 0 || ParamData.TabDescId != CsData.TabDescId
                || ParamData.PjId == 0 || ParamData.PjId != CsData.PjId
                || ParamData.UserId == 0 || ParamData.UserId != CsData.UserId
                || ParamData.UserLangId != CsData.UserLangId
                || ParamData.Ticks == 0)
            {
#if DEBUG
                eModelTools.EudoTraceLog(String.Concat("LoadQueryString >> Mauvais TabDescId et/ou PjId et/ou UserId et/ou UserLangId ou Ticks vide : ",
                    ParamData.TabDescId, " != ", CsData.TabDescId, " / ",
                    ParamData.PjId, " != ", CsData.PjId, " / ",
                    ParamData.UserId, " != ", CsData.UserId, " / ",
                    ParamData.UserLangId, " != ", CsData.UserLangId));
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
            this.CsData = ExternalUrlTokenPj.LoadToken(cs);
        }

        /// <summary>
        /// Charge des paramètres
        /// </summary>
        /// <param name="param">param crypté venant de la querystring</param>
        protected override void LoadSetParam(string param)
        {
            this.ParamData = (ExternalUrlParamPj)ExternalUrlTools.LoadParam(param);
        }
    }
}