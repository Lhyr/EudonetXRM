using System;
using System.Collections.Generic;


namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class FormularParamsModel
    {
        /// <summary>
        /// Liste des langue disponible
        /// </summary>
        public Dictionary<String, String> AvailableLanguages = new Dictionary<String, String>();
        
        /// <summary>
        /// La liste des champs de fusion
        /// </summary>
        public string MergeFields { get; set; }

        /// <summary>
        /// La liste des champs de fusion de type site web
        /// </summary>
        public string HyperLinkMergeFields { get; set; }
        /// <summary>
        /// Information utilisateur
        /// </summary>
        public AdvFormularUserInfoModel UserInfos { get; set; }

        /// <summary>
        /// Champs de fusion sans cibles étendues
        /// </summary>
        public string MergeFieldsWithoutExtendedFields { get; set; }

        /// <summary>
        /// test if the extension worldline payement is activated
        /// </summary>
        public bool IsWorldLineExtensionIsActivated { get; set; }
    }

}