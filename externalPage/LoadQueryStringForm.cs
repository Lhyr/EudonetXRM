using Com.Eudonet.Merge;

namespace Com.Eudonet.Xrm
{
    /// <className>LoadQueryStringForm</className>
    /// <summary>Représente les données transmis en QueryString à la page formulaire de l'appli</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-09-22</date>
    public class LoadQueryStringForm : LoadQueryString<ExternalUrlTokenForm, ExternalUrlParamForm>
    {
        /// <summary>
        /// Constructeur - Charge et analyse la querystring
        /// </summary>
        /// <param name="uid">identifiant de la base</param>
        /// <param name="csCrypt">token de sécurité</param>
        /// <param name="paramCrypt">paramètres</param>
        public LoadQueryStringForm(string uid, string csCrypt, string paramCrypt)
            : base(uid, csCrypt, paramCrypt)
        {
        }

        /// <summary>
        /// Charge le token
        /// </summary>
        /// <param name="cs">token crypté venant de la querystring</param>
        protected override void LoadSetTokenSecu(string cs)
        {
            this.CsData = ExternalUrlTokenForm.LoadToken(cs);
        }

        /// <summary>
        /// Charge des paramètres
        /// </summary>
        /// <param name="param">param crypté venant de la querystring</param>
        protected override void LoadSetParam(string param)
        {
            this.ParamData = (ExternalUrlParamForm)ExternalUrlTools.LoadParam(param);
        }
    }
}