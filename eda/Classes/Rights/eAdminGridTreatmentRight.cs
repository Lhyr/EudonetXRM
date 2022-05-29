using Com.Eudonet.Internal;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet gérant un droit pour une grille
    /// </summary>
    public class eAdminGridTreatmentRight : IAdminTreatmentRight
    {

        /// <summary>
        /// Id de la grille dans ce cas
        /// </summary>
        public int TraitID { get; private set; }

        /// <summary>
        /// DescId de la table grille
        /// </summary>
        public int DescID { get; set; }


        /// <summary>
        /// Type de traitement
        /// </summary>
        public eTreatmentType Type { get; private set; }

        /// <summary>
        /// Permission définit sur la table, la rubrique, la grille ou autre 
        /// </summary>
        public ePermission Perm { get; private set; }

        /// <summary>
        /// Libellé de la grille
        /// </summary>
        public string TabLabel { get; private set; }

        /// <summary>
        /// Libellé de vu depuis - reprends le libellé de la pages d'accueil ou de l'onglet
        /// </summary>
        public string TabFromLabel { get; set; }

        /// <summary>
        /// Libellé du champ dans ce cas c'est vide
        /// </summary>
        public string FieldLabel { get; private set; }

        /// <summary>
        /// Libellé du traitement
        /// </summary>
        public string TraitLabel { get; set; }

        /// <summary>
        /// Libelle du type de filtre
        /// </summary>
        public string TypeLabel { get; set; }

        /// <summary>
        /// Localisation du traitement
        /// </summary>
        public eLibConst.TREATMENTLOCATION TreatLoc { get; set; }

        /// <summary>
        /// Constructeur du droit traitement de la grille
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="grid"></param>
        public eAdminGridTreatmentRight(ePref pref, eRecord grid)
        {
            DescID = (int)TableType.XRMGRID;
            TraitID = grid.MainFileid;
            Perm = GetPerm(pref, GetPermId(grid));
            TabLabel = GetTitle(grid);
            TabFromLabel = string.Empty;
            FieldLabel = string.Empty;
            TraitLabel = string.Empty;
            TypeLabel = string.Empty;
            Type = eTreatmentType.VIEW;
            TreatLoc = eLibConst.TREATMENTLOCATION.GridViewPermission;
        }

        /// <summary>
        /// Titre de la grille
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private string GetTitle(eRecord grid)
        {
            eFieldRecord fld = grid.GetFields.Find(f => f.FldInfo.Descid == (int)XrmGridField.Title);
            if (fld == null)
                return string.Empty;

            return fld.DisplayValue;
        }

        /// <summary>
        /// Récupère l'id de la permission
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        private int GetPermId(eRecord grid)
        {
            eFieldRecord fld = grid.GetFields.Find(f => f.FldInfo.Descid == (int)XrmGridField.ViewPermId);
            int i;
            if (fld == null || !Int32.TryParse(fld.Value, out i))
                return 0;

            return i;
        }

        /// <summary>
        /// Récupère la permission avec son ID
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="permId"></param>
        /// <returns></returns>
        private ePermission GetPerm(ePref pref, int permId)
        {
            if (permId == 0)
                return new ePermission(0, ePermission.PermissionMode.MODE_NONE, 0, "");

            using (eudoDAL dal = eLibTools.GetEudoDAL(pref))
            {
                dal.OpenDatabase();
                ePermission perm = new ePermission(permId, dal, pref);

                // C'est moche, Pourquoi ne pas le charger directement à la construction ? 
                if (perm.PermUser.Length > 0)
                    perm.LoadUserPermLabel(dal);

                return perm;
            }
        }

        /// <summary>
        /// Pour ordonner dans une liste suivant TypeLabel puis nom de la grille "TabLabel"
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public int CompareTo(IAdminTreatmentRight that)
        {
            // orderby TypeLabel, TraitLabel
            int result = this.TypeLabel.CompareTo(that.TypeLabel);
            if (result == 1 || result == -1)
                return result;

            return this.TabLabel.CompareTo(that.TabLabel);
        }
    }
}