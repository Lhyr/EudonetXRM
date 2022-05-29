using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Render pour la gestion des droits de traitements
    /// </summary>
    public class eAdminRightsDialogGridWidgetRenderer : eAdminRightsDialogRenderer
    {

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nDescId"></param>
        protected eAdminRightsDialogGridWidgetRenderer(ePref pref, Int32 nDescId, Int32 fileId) : base(pref, nDescId)
        {

        }

        public static eAdminRightsDialogGridWidgetRenderer CreateAdminRightsDialogGridWidgetRenderer(ePref pref, Int32 nDescId, Int32 nFileId)
        {
            return new eAdminRightsDialogGridWidgetRenderer(pref, nDescId, nFileId);
        }

        protected override bool Init()
        {


            //Function = "Voir la grille";
            LstTreatmentTypes = new HashSet<eTreatmentType>() { eTreatmentType.VIEW };
            //ListOnly = true;
            return base.Init();
        }

        protected override void CreateTreatmentsButton()
        {

        }
    }
}