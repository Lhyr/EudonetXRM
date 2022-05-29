using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// Classe abstraite de gestion des options utilisateurs 
    public abstract class eUserOptions
    {



        #region properties

        /// <summary>
        /// Inner Exception
        /// </summary>
        protected Exception _eInnerException = null;

        /// <summary>
        /// Exception rencontrée
        /// </summary>
        public Exception InnerException
        {
            get { return _eInnerException; }
        }


        /// <summary>
        /// Message d'erreur
        /// </summary>
        protected String _sErrorMsg = "";
        /// <summary>
        /// 
        /// </summary>
        public String ErrorMsg
        {
            get { return _sErrorMsg; }
        }


        private ePref _ePref;

        /// <summary>
        /// Objet ePref
        /// </summary>
        public ePref Pref
        {
            get { return _ePref; }

        }




        #endregion


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eUserOptions(ePref pref)
        {
            _ePref = pref;

        }



        //public abstract bool UpdateOption(String sOption, String sOptionValue);

    }
}