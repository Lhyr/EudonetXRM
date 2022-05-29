using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Model pour les UserValueField.
    /// </summary>
    public class UserValueFieldModel
    {
        public bool IsFound { get; set; }
        [DefaultValue(0)]
        public int NumDesc { get; set; }
        public string ValDesc { get; set; }
        public string DisplayDesc { get; set; }
    }
}