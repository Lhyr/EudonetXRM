using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eListMainRenderer" />
    public class eListRGPDTreatmentLogRenderer : eListMainRenderer
    {
        /// <summary>
        /// Constructeur par défaut avec uniquement pref
        /// Base des classe dérivées
        /// </summary>
        /// <param name="pref"></param>
        private eListRGPDTreatmentLogRenderer(ePref pref) : base(pref)
        {
            _rType = RENDERERTYPE.ListRendererMain;
        }

        /// <summary>
        /// test les droits et retourne un eListUserRenderer
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static eListRGPDTreatmentLogRenderer GetListRGPDTreatmentLogRenderer(ePref p)
        {

            if (p.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            return new eListRGPDTreatmentLogRenderer(p);
        }

        /// <summary>
        /// Indique si le champ de recherche doit être affiché
        /// </summary>
        public override Boolean DrawSearchField
        {
            get { return (_list != null); }
        }



        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {
            return;
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