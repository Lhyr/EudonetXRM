using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Memo
    /// </summary>
    public class MemoFieldInfos : FldTypedInfosModel, IMultiLine
    {
        /// <summary>
        /// Indique si le champ Mémo est au format HTML (sinon : texte brut)
        /// </summary>
        public bool IsHtml;
        public int Rowspan { get; set; }

        internal MemoFieldInfos(Field f) : base(f)
        {
            Format = FieldType.Memo;
            IsHtml = f.IsHtml;
            Rowspan = f.PosRowSpan;
        }

    }
}