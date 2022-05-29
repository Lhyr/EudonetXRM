using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Eudonet.Xrm.Import.Exceptions
{
    public class ImportLineErrorMsgDouble : ImportLineErrorMsg
    {
        public int NbLineDuplicates { get; set; }
        public IEnumerable<eImportContentColumn> Columns { get; set; }

        protected override string GetUserSpecMsg(ePrefLite pref)
        {
            string value;
            StringBuilder sb = new StringBuilder();
            foreach (eImportContentColumn column in Columns)
            {
                if (!column.IsKey)
                    continue;

                value = Line.Values[column.Index];

                if (sb.Length != 0)
                    sb.Append(" / ");
                sb.Append(column.EudoColLabel).Append("=").Append(value.Length == 0 ? String.Concat("{", eResApp.GetRes(pref, 141), "}") : value);
            }

            // 1817 - <NB> enregistrements contenant les mêmes valeurs de déduplication (<COLS>)
            return eResApp.GetRes(pref, 1817)
                .Replace("<NB>", NbLineDuplicates.ToString())
                .Replace("<COLS>", sb.ToString());
        }
    }
}