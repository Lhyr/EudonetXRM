using System;
using Com.Eudonet.Merge;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>LoadQueryString</className>
    /// <summary>Représente les données transmis en QueryString à la page externe de l'appli</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-09-22</date>
    public abstract class LoadQueryString<UrlToken, UrlParam> : ILoadQueryString
        where UrlToken : ExternalUrlToken where UrlParam : ExternalUrlParam
    {
        /// <summary>token de sécurité du lien de mailing</summary>
        public UrlToken CsData { get; protected set; }
        /// <summary>données du lien tracking</summary>
        public UrlParam ParamData { get; protected set; }

        /// <summary>
        /// Charge le token
        /// </summary>
        /// <param name="cs">token crypté venant de la querystring</param>
        protected abstract void LoadSetTokenSecu(string cs);
        /// <summary>
        /// Charge des paramètres
        /// </summary>
        /// <param name="param">param crypté venant de la querystring</param>
        protected abstract void LoadSetParam(string param);

        /// <summary>Indique si la querystring est cohérente</summary>
        protected bool _invalidQueryString;

        /// <summary>
        /// Indique si la querystring est cohérente
        /// </summary>
        /// <returns>vrai si mauvaise querystring</returns>
        public bool InvalidQueryString()
        {
            return _invalidQueryString;
        }

        /// <summary>
        /// Constructeur - Charge et analyse la querystring
        /// </summary>
        /// <param name="uid">identifiant de la base</param>
        /// <param name="csCrypt">token de sécurité</param>
        /// <param name="paramCrypt">paramètres</param>
        public LoadQueryString(string uid, string csCrypt, string paramCrypt)
        {
            _invalidQueryString = true;

            if (uid == null || csCrypt == null || paramCrypt == null)
            {
#if DEBUG
                eModelTools.EudoTraceLog("LoadQueryString >> uid et/ou csCrypt et/ou paramCrypt vide");
#endif
                return;
            }

            string cs = ExternalUrlTools.GetDecrypt(csCrypt);
            LoadSetTokenSecu(cs);

            // Données non identiques, on stop le chargement
            if (!CsData.UID.Equals(uid))
            {
#if DEBUG
                eModelTools.EudoTraceLog(String.Concat("LoadQueryString >> Mauvais UID : ", CsData.UID, " != ", uid));
#endif
                return;
            }

            string param = ExternalUrlTools.GetDecrypt(paramCrypt);
            LoadSetParam(param);

            _invalidQueryString = false;
        }
    }
}