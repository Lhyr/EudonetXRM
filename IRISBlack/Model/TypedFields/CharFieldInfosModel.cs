using Com.Eudonet.Core.Model;
using System.ComponentModel;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type caractère
    /// </summary>
    public class CharFieldInfos : FldTypedInfosModel
    {
        /// <summary>
        /// indique si l'autocompletion est activée sur le champ
        /// </summary>
        public bool AutoComplete = false;

        /// <summary>
        /// indique si le champ est le champ principal de la fiche
        /// </summary>
        public bool IsMainField = false;

        /// <summary>
        /// format d'affichage du champs caractère
        /// </summary>
        /// <example>Première lettre en capitale.</example>
        /// <example>Lettres en capitale.</example>
        /// <example>Tout en minuscules</example>
        [DefaultValue(CaseField.CASE_NONE)]
        public string DisplayFormat { get; set; } = CaseField.CASE_NONE.ToString();
        internal CharFieldInfos(Field f, ePref pref) : base(f)
        {
            Format = FieldType.Character;
            AutoComplete = f.AutoCompletionEnabled;
            DisplayFormat = f.Case.ToString();
        }

    }
}