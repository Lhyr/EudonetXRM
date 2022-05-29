using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm
{
    public class eFieldLiteMiniFileAdmin : FieldLite
    {
        /// <summary>
        /// Libelle du field
        /// </summary>
        public string Libelle { get; set; }
        public ImageStorage ImgStorage { get; set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="descId">descid de la rubrique</param>
        public eFieldLiteMiniFileAdmin(int descId, string lang)
            : base(descId)
        {
            _externalInfoAdditionalSelect = new List<string>();
            // Libelle du field
            _externalInfoAdditionalSelect.Add($"[res].[{lang}] as [lib]");
            _externalInfoAdditionalSelect.Add("fld.[Storage] AS [ImgStorage]");
        }

        /// <summary>
        /// Chargement des colonnes complémentaires
        /// </summary>
        /// <param name="param"></param>
        public override void LoadAdditionalInfos(AdditionalParameters param)
        {
            Libelle = param.Dtr.GetString("lib");
            ImgStorage = (ImageStorage)param.Dtr.GetEudoNumeric("ImgStorage");
        }

        public static Func<int, eFieldLiteMiniFileAdmin> Factory(ePrefLite pref)
        {
            return (int did) => new eFieldLiteMiniFileAdmin(did, pref.Lang);
        }
    }
}