using System;
using System.Text;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eListUserRenderer : eListMainRenderer
    {


        /// <summary>
        /// Constructeur par défaut avec uniquement pref
        /// Base des classe dérivées
        /// </summary>
        /// <param name="pref"></param>
        private eListUserRenderer(ePref pref) : base(pref)
        {
            _rType = RENDERERTYPE.ListRendererMain;
        }

        /// <summary>
        /// test les droits et retourne un eListUserRenderer
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static eListUserRenderer GetListUserRenderer(ePref p)
        {

            if (p.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            return new eListUserRenderer(p);
        }


        public override Boolean DrawSearchField
        {
            get { return (_list != null); }
        }



        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        /// <param name="iWidth">largeur utilisée par la cellule</param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {

        }



        /// <summary>
        /// Gestion du rendu des champs spéciaux de user (déclaré en un type donné mais rendu différent : ex, level est du char mais doit représente le level qui est du int)
        /// </summary>
        /// <param name="row">Ligne de l'enregistrement</param>
        /// <param name="fieldRow">Champ a afficher</param>
        /// <param name="ednWebControl">COntrole eudo de représentation</param>
        /// <param name="sbClass">CSS</param>
        /// <param name="sClassAction">Action JS</param>
        /// <returns></returns>
        protected override bool RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            WebControl webControl = ednWebControl.WebCtrl;

            //Le user level doit avoir une dbv pour que les filtres express fonctionne
            if (
                    fieldRow.FldInfo.Descid == (int)UserField.LEVEL 
                || fieldRow.FldInfo.Descid == (int)UserField.PASSWORD_POLICIES_ALGO
                || fieldRow.FldInfo.Descid == (int)UserField.Product
                )
                webControl.Attributes.Add("dbv", fieldRow.Value);

            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }


        /// <summary>
        /// Ajoute dans le rang de donnée la check box permettant d'effectuer une selection
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, String sAltLineCss)
        {
            return;
        }

    }
}