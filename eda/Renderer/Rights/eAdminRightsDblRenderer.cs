using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Renderer de la gestion des droits sur les doublons
    /// >
    /// </summary>
    public class eAdminRightsDblRenderer : eAdminRightsDescRenderer
    {

        /// <summary>
        /// Constructeur standard - pour les doublons, ni grille ni page d'accueil
        /// </summary>
        /// <param name="pref"></param>
        public eAdminRightsDblRenderer(ePref pref) : base(pref, (int)TableType.DOUBLONS)
        {
        }

        /// <summary>
        /// Pour les doublons, pas de tab special, l'interface est "verrouillée" sur les doublons
        /// </summary>
        /// <param name="ddl"></param>
        protected override void AddSpecialTab(DropDownList ddl)
        {
            return;
        }

        /// <summary>
        /// Pour les doublons, pas de tab special, l'interface est "verrouillée" sur les doublons
        /// </summary>
        /// <param name="ddl"></param>
        protected override void AddSpecialField(DropDownList ddl)
        {
            return;
        }

        /// <summary>
        /// pour les doublons uniquement view et update
        /// </summary>
        /// <param name="tt"></param>
        /// <returns></returns>
        protected override bool CanManageTreatment(eTreatmentType tt)
        {
            if (tt == eTreatmentType.VIEW)
                return true;
            else
                return false;


        }

        /// <summary>
        /// Remplit la la drop liste avec la liste avec des champs de la table 
        /// </summary>
        /// <param name="ddl"></param>
        protected override void FillFieldList(DropDownList ddl)
        {


            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.Items.Add(
                new ListItem("Doublons", ((int)TableType.DOUBLONS).ToString())
                );
        }

        /// <summary>
        /// Remplit la la drop liste avec la liste des tables
        /// </summary>
        /// <param name="ddl"></param>
        protected override void FillTabList(DropDownList ddl)
        {
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.Items.Add(
                new ListItem("Doublons", ((int)TableType.DOUBLONS).ToString())
                );
        }


        /// <summary>
        /// Retourne la liste des permissions
        /// </summary>
        /// <returns></returns>
        protected override List<IAdminTreatmentRight> GetTreatmentRights()
        {

            eAdminDescTreatmentRightCollection oRightsList = new eAdminDescDblTreatmentRightCollection(Pref);
            oRightsList.TreatTypes = LstTreatmentTypes;
            oRightsList.Function = Function;
            oRightsList.LoadTreamentsList();
            var t = oRightsList.RightsList;
            return t;
        }




    }
}