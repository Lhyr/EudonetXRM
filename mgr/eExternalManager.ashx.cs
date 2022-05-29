using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

 


    /// <className>eEudoManager</className>
    /// <summary>Classe parente des manager, se référer à la classe fille</summary>
    /// <purpose></purpose>
    /// <authors>SPH</authors>
    /// <date>2011-06-18</date>
    public abstract class eExternalManager : eBaseEudoManager
    {
        /// <summary>
        /// Informations de connexion à SQL
        /// </summary>
        protected ePrefSQL _prefSqlClient = null;
        /// <summary>
        /// Objet de communication avec SQL
        /// </summary>
        protected eudoDAL _dalClient = null;


        /// <summary>Informations sur l'URL de la page externalisée</summary>
        internal ExternalPageQueryString _pageQueryString;

        /// <summary>
        /// Type d'external page
        /// </summary>
        protected virtual eExternal.ExternalPageType PgTyp { get { throw new NotImplementedException(); } }


        /// <summary>
        /// Charge les variables de session et le dico de request.form
        /// </summary>
        protected override void LoadSession()
        {
            // Charge les valeurs de request sur ancien système pour conserver une certaine compatibilité
            _allKeys = new HashSet<string>(_context.Request.Form.AllKeys, StringComparer.OrdinalIgnoreCase);
            _allKeysQS = new HashSet<string>(_context.Request.QueryString.AllKeys, StringComparer.OrdinalIgnoreCase);

            _requestTools = new eRequestTools(_context);
            _pageQueryString = ExternalPageQueryString.GetNewByForm(_requestTools);

            if (!ValidateExternalLoad())
                return;

            LoadPref();
        }


        /// <summary>
        /// Valide les informations pour ouvrir la page externe
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateExternalLoad()
        {
            return true;
        }


        private void LoadPref()
        {
            LoadEudoLog dataEudoLog = new LoadEudoLog(_context.Request, _pageQueryString.UID);

            if (dataEudoLog.Error.Length != 0)
            {
                // TODO - DEMANDER A GBO - Message à utilisateur est-il necessaire ?
                // Perte de Session             

                //503; // votre session a expiré...
                //6068; // votre session a expiré...détail
                int iLng = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(iLng,503), eResApp.GetRes(iLng, 6068));
                this.ErrorContainer.IsSessionLost = true;
                LaunchError();
            }

            // Objet temporaire pour la description de connexion à SQL
            _prefSqlClient = ePrefTools.GetDefaultPrefSql(dataEudoLog.Directory);
            _dalClient = eLibTools.GetEudoDAL(_prefSqlClient);

            string error = string.Empty;
            try
            {
                _dalClient.OpenDatabase();
                _pref = eExternal.GetPref(PgTyp, _prefSqlClient, _dalClient, out error);
                if (_pref == null)
                    return;
            }
            catch (Exception exp)
            {
                error = exp.Message;
            }
            finally
            {
                _dalClient.CloseDatabase();

                if (error.Length > 0)
                {
                    switch (PgTyp)
                    {
                        case eExternal.ExternalPageType.TRACKING:
                            throw new TrackExp(error);
                        case eExternal.ExternalPageType.FORMULAR:
                            throw new FormularExp(error);
                    }
                }
            }
        }
    }

 
}