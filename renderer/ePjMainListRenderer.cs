using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Text;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer pour la liste de pj en mode main liste
    /// </summary>
    public class ePjMainListRenderer : eListMainRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        private ePjMainListRenderer(ePref pref)
            : base(pref)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static ePjMainListRenderer GetPjMainListRenderer(ePref pref)
        {
            return new ePjMainListRenderer(pref);
        }

        /// <summary>
        /// Pour les PJ, pas de sélection
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, String sAltLineCss)
        {
            return;
        }

        /// <summary>
        /// Pour les PJ, pas de sélection
        /// </summary>
        /// <param name="headerRow"></param>
        /// <param name="iWidth">largeur utilisée par la cellule</param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {

        }
 

        /// <summary>
        /// Dans le cas de la liste vers pj, ce type de champ est un lien vers la fiche de la pj
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">classe CSS</param>
        /// <param name="sClassAction">la type d action</param>
        /// <returns></returns>
        protected override bool RenderIDFieldFormat(eRecord row, Internal.eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            eRecordPJ rowPj = (eRecordPJ)row;

            ednWebControl.WebCtrl.Attributes.Add("pjtabtyp", rowPj.PJTabType.GetHashCode().ToString());
            //ednWebControl.WebCtrl.Attributes.Add("lnkdid", rowPj.PJTabDescID.ToString());
            ednWebControl.WebCtrl.Attributes.Add("lnkid", fieldRow.Value); //ID de la fiche

            if (rowPj.PJTabType == EdnType.FILE_MAIN)
                sClassAction = "LNKGOFILE";
            else if (rowPj.PJTabType == EdnType.FILE_PLANNING)
            {
                sClassAction = "LNKOPENCALPUP";
                
                fieldRow.DisplayValue = string.Concat("[", eResApp.GetRes(Pref, 1122), "]");
            }
            else
            {
                sClassAction = "LNKOPENPOPUP";
                fieldRow.DisplayValue = string.Concat("[", eResApp.GetRes(Pref, 1122), "]");
            }

            return base.RenderIDFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }

        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, Int32 idxLine)
        {
            eRecordPJ rowPJ = (eRecordPJ)row;

            trRow.Attributes.Add("fid", rowPJ.PJFileID.ToString());
            trRow.Attributes.Add("tab", rowPJ.PJTabDescID.ToString());
        }

    }
}