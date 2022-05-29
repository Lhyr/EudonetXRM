using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    public class eWebPageXrmWidgetUI : eAbstractXrmWidgetUI
    {
        /// <summary>
        /// Les fameuses pref users.
        /// </summary>
        public ePref Pref { get; set; }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {

            HtmlGenericControl iframe = new HtmlGenericControl("iframe");

            /** Petite transfo pour les url formatées pour société.com, compass et autres. G.L */
            string sLinkToDecompose = _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.ContentSource)).Value;
            sLinkToDecompose = eModelTools.ReplaceEudoParamInURL(Pref, sLinkToDecompose, GetFieldsValue(_widgetContext.ParentTab, _widgetContext.ParentFileId, sLinkToDecompose));

            iframe.Attributes.Add("src", sLinkToDecompose);            
            widgetContainer.Controls.Add(iframe);
            // iframe.Attributes.Add("sandbox", "allow-same-origin");

            base.Build(widgetContainer);
        }

        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public override void AppendScript(StringBuilder scriptBuilder)
        {
            base.AppendScript(scriptBuilder);
        }

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="scriptBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }

        /// <summary>
        /// Retourne la liste des champs avec leur valeurs
        /// </summary>
        /// <param name="nTabId">id de la table parent du signet</param>
        /// <param name="stringToReplace">l'URL d'origine</param>
        /// <param name="fileId">id de la fiche</param>
        /// <returns>liste des champs avec leur valeurs</returns>
        private IEnumerable<eFieldRecord> GetFieldsValue(int nTabId, int fileId, string stringToReplace)
        {
            Regex Re = new Regex(@"(\$)(\d+)(\$)");

            IEnumerable<int> listDescid = Re.Matches(stringToReplace).OfType<Match>().
                Select(rem => (rem.Groups.Count > 1) ? eLibTools.GetNum(rem.Groups[2].Value) : 0).
                Where(num => num > 0);

            // HLA - Inutile de faire une EudoQuery si on n'a pas trouvé de rubriques !
            if (listDescid == null || listDescid.Count() < 1)
                return new List<eFieldRecord>();

            eDataFillerGeneric filler = new eDataFillerGeneric(Pref, nTabId, ViewQuery.CUSTOM)
            {
                EudoqueryComplementaryOptions = (EudoQuery.EudoQuery eq) =>
                {
                    eq.SetListCol = string.Join(";", listDescid);
                    eq.SetFileId = fileId;
                }
            };
            filler.Generate();

            if (filler.ErrorMsg.Length != 0 || filler.InnerException != null)
                throw new Exception($"{filler.ErrorMsg} {(filler.InnerException == null ? String.Empty : filler.InnerException.Message)}");

            // Recupère l'enregistrement
            eRecord row = filler.GetFirstRow();
            if (row == null)
                throw new Exception($"L'enregistrement n° {fileId} de la table {nTabId} non trouvé.");

            // Recupère la rubrique
            return row.GetFields.Join(listDescid, rw => rw.FldInfo.Descid, lstDsc => lstDsc, (rw, lstDsc) => rw);
        }
    }
}