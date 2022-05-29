using Com.Eudonet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;


namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de rendu d'une annexe sous forme de fiche
    /// </summary>
    public class ePJFileRenderer : eEditFileRenderer
    {

        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <param name="fileid"></param>
        public ePJFileRenderer(ePref pref, int tab, int fileid) : base(pref, tab, fileid)
        {

        }

        protected override bool End()
        {
            if (!base.End())
                return false;

            eFilePJ filePJ = (eFilePJ)_myFile;
            HtmlInputHidden input = new HtmlInputHidden()
            {
                ID = $"{filePJ.ViewMainTable.DescId}_LinkedFile",
                Value = JsonConvert.SerializeObject(new { LinkedTab = filePJ.GetLinkedTab, LinkedFileId = filePJ.GetLinkedFileId })
            };

            PgContainer.Controls.Add(input);

            return true;

        }

    }
}