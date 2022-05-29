using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Model pour les VCard
    /// </summary>
    public class VCardShortcutModel : MenuShortcutModel
    {
        /// <summary>
        /// Lien encrypté pour la V-card.
        /// </summary>
        public string EncryptedLink { get; set; }
    }
}