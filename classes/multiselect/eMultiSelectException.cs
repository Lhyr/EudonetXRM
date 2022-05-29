using System;
using System.Runtime.Serialization;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Exception liée au multi-select
    /// </summary>
    internal class eMultiSelectException : Exception
    {
        public string Detail = string.Empty;
        public string DetailDev = string.Empty;

        /// <summary>
        /// Exception non détaillée c'pas bien
        /// </summary>
        /// <param name="message"></param>
        public eMultiSelectException(string message) : base(message)
        {
        }

        /// <summary>
        /// Exception détaillée
        /// </summary>
        /// <param name="message"></param>
        /// <param name="detail"></param>
        /// <param name="dev"></param>
        public eMultiSelectException(string message, string detail, string dev) : this(message)
        {
            Detail = detail;
            DetailDev = dev;
        }

        /// <summary>
        ///  Construit un errorContainer 
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public eErrorContainer GetErrorContainer(ePref pref)
        {
            return eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, Message, Detail, eResApp.GetRes(pref, 72), DetailDev);
        }
    }
}