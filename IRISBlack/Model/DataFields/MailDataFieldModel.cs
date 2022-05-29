using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools.filler;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type Adresse email
    /// </summary>
    public class MailDataFieldModel : DataFieldWithValueModel
    {
        /// <summary>
        /// Statut Eudonet
        /// </summary>
        public int MailStatusEudo { get; private set; }

        /// <summary>
        /// Statut Technique
        /// </summary>
        public int MailStatusTech { get; private set; }

        /// <summary>
        /// Sous Statut Technique
        /// </summary>
        public int MailStatusSubTech { get; private set; }



        internal MailDataFieldModel (eFieldRecord f) : base(f)
        {
            var prop = f.ExtendedProperties as ExtendedMailStatus;

            if (prop == null)
                return;

            MailStatusEudo = prop.MailStatusEudo;
            MailStatusTech = prop.MailStatusTech;
            MailStatusSubTech = prop.MailStatusSubTech;
        }

    }
}