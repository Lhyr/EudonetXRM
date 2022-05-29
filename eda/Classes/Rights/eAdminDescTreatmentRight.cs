using Com.Eudonet.Internal;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Classe eAdminTreatmentRight : droit de traitement
    /// </summary>
    /// <author>CRU</author>
    /// 
    /// <remarks>05/02/2016</remarks>
    public class eAdminDescTreatmentRight : IAdminTreatmentRight
    {
        /// <summary>
        /// préférence 
        /// </summary>
        private ePref _pref;
        /// <summary>
        /// eudoDAL
        /// </summary>
        private eudoDAL _eDal = null;

        /// <summary>
        /// Certains droits de traitement sont associés à une règles (les envois conditionnels )
        /// </summary>
        private int _nRightId = 0;

        /// <summary>
        /// Table du traitement
        /// </summary>
        private int _nTabDescId = 0;

        /// <summary>
        /// Permission appliqué au traitement
        /// </summary>
        ePermission _perm;

        /// <summary>
        /// Id du traitement
        /// </summary>
        private int _nTraitID;

        private string _sTabLabel;

        private string _sTraitTabel;

        /// <summary>
        /// Id de traitment dans EUDORES..TRAIT
        /// </summary>
        private int _nGlobalTraitId;

        #region Propriétés      

        /// <summary>
        /// Type de traitement
        /// </summary>
        public eTreatmentType Type
        {
            get
            {
                return GetType(_nGlobalTraitId);
            }
        }

        /// <summary>
        /// Libellé du type de traitement
        /// </summary>
        public string TypeLabel
        {
            get
            {
                return GetResType(Type, _pref);
            }
        }

        /// <summary>
        /// Localisation du traitement
        /// </summary>
        public eLibConst.TREATMENTLOCATION TreatLoc { get; set; }


        /// <summary>
        /// Id du droit de traitement
        /// </summary>
        public int TraitID
        {
            get { return _nTraitID; }
        }

        /// <summary>
        /// Descid de la table/rubrique
        /// </summary>
        public int DescID { get; set; }


        /// <summary>
        /// Traitement global
        /// </summary>
        public Int32 GlobalTraitId
        {
            get { return _nGlobalTraitId; }
        }

        /// <summary>
        /// Table vu depuis
        /// </summary>
        public String TabFromLabel { get; set; }

        /// <summary>
        /// Libellé de la rubrique
        /// </summary>
        public String FieldLabel { get; set; }

        /// <summary>
        /// Permission associée
        /// </summary>
        public ePermission Perm
        {
            get { return _perm; }
            set { _perm = value; }
        }

        /// <summary>
        /// Libelle du droit de traitement
        /// </summary>
        public String TraitLabel
        {
            //get { return _sTraitTabel.Length > 0 ? _sTraitTabel : GetResFromTreatLocation(TreatLoc); }
            get { return _sTraitTabel; }
            set { _sTraitTabel = value; }
        }

        /// <summary>
        /// Libelle de la table
        /// </summary>
        public String TabLabel
        {
            get { return _sTabLabel; }
            set { _sTabLabel = value; }
        }

        #endregion


        /// <summary>
        /// surcharge tostring pour le debug
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Concat(
                this.TabLabel, " | ",
                this.TabFromLabel, " | ",
                this.FieldLabel, " | ",
                this.TraitLabel, " | ",
                this.TypeLabel, " | "
                );
        }

        /// <summary>
        /// surcharge gethashcode nécessaire du à la surcharge de ToString()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCodeGenerator.GenericHashCode(TabLabel, TabFromLabel, FieldLabel, TraitLabel, TypeLabel);
        }
        /// <summary>
        /// Retourne la requete de base de lecture des droits de traitements avec gestion de la langue
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="sQuery">Requête</param>
        /// <returns></returns>
        public static String ApplyLanguageOnQuery(ePref pref, string sQuery)
        {
            return sQuery.Replace("@@LNG@@", pref.User.UserLang);

        }

        public void SetTrait(Int32 nTraitID, Int32 nGlobalTraitId)
        {
            _nGlobalTraitId = nGlobalTraitId;
            _nTraitID = nTraitID;
        }



        public void SetFromDesc(string sFromLabel, string sFieldLabel)
        {
            TabFromLabel = sFromLabel;
            FieldLabel = sFieldLabel;

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminDescTreatmentRight(ePref pref)
        {
            _pref = pref;
            TreatLoc = eLibConst.TREATMENTLOCATION.None;
        }


        /// <summary>
        /// Type du traitement
        /// </summary>
        public eTreatmentType GetType(Int32 nGlobalTraitId)
        {
            if (TreatLoc == eLibConst.TREATMENTLOCATION.Trait)
            {
                switch (nGlobalTraitId)
                {

                    case 101:
                    // Ajouter depuis une autre fiche
                    case 102:
                    // Modifier une autre fiche
                    case 103:
                    // Supprimer une autre fiche
                    case 105:
                    // Duppliquer une autre fiche
                    case 115:
                        return eTreatmentType.ATOMIC;
                    case 106:
                    //Ajout en masse
                    case 107:
                    //Modif en masse
                    case 108:
                    //Supp en masse
                    case 109:
                        //Duppli en masse
                        return eTreatmentType.MASS;

                    //Import en masse en onglet
                    //Import en masse en signet
                    case 118:
                    case 119:
                        return eTreatmentType.IMPORT;

                    // Modifier la zone Assistant Eudonet X
                    case 122:
                        return eTreatmentType.CHANGE;

                    case 110:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                        return eTreatmentType.PJ;


                    case 10:
                    //Impression
                    case 11:
                    //Ajout d'un rapport d'impression
                    case 12:
                    //Modif d'un rapport d'impression
                    case 13:
                    // suppression d'un rapport d'impression
                    case 14:
                        // rendre un rapport d'impression public
                        return eTreatmentType.ANALYZE;

                    case 20:
                    //Export
                    case 21:
                    //Ajout
                    case 22:
                    //Modif
                    case 23:
                    //Supp
                    case 24:
                        //Public                        
                        return eTreatmentType.ANALYZE;
                    case 30:
                    //Filtre
                    case 31:
                    //Ajout
                    case 32:
                    //Modif
                    case 33:
                    //Supp
                    case 34:
                        //Public
                        return eTreatmentType.ANALYZE;
                    case 39:
                    //Sms
                    case 40:
                    //emailing
                    case 41:
                    //faxing
                    case 42:
                        //voicing
                        return eTreatmentType.COMMUNICATION;
                    case 43:
                    // envois conditionnels (email)
                    case 44:
                    // envois conditionnels (Fax)
                    case 45:
                    // envois conditionnels (Voice)
                    case 46:
                        // envois conditionnels (Publipostage)
                        return eTreatmentType.OTHER;
                    case 47:
                        //nomination
                        return eTreatmentType.OTHER;
                    /* catalogue */
                    case 51:
                    //Ajouter un modèle de mail
                    case 52:
                    //Modifier un modèle de mail
                    case 53:
                    //Supprimer un modèle de mail
                    case 54:
                        return eTreatmentType.COMMUNICATION;
                    case 55:
                    // formulaire
                    case 56:
                    // ajout
                    case 57:
                    // modif
                    case 58:
                        // supp
                        return eTreatmentType.COMMUNICATION;
                    case 60:
                    // publipostage word
                    case 61:
                    //ajout rapport
                    case 62:
                    // modifier rapport
                    case 63:
                        //supprimer rapport
                        return eTreatmentType.COMMUNICATION;
                    case 70:
                    // publipostage html
                    case 71:
                    //ajout rapport
                    case 72:
                    // modifier rapport
                    case 73:
                        //supprimer rapport
                        return eTreatmentType.COMMUNICATION;
                    case 80:
                    // publipostage pdf
                    case 81:
                    //ajout
                    case 82:
                    // modifier
                    case 83:
                        //supprimer
                        return eTreatmentType.COMMUNICATION;
                    case 90:
                        //fusion
                        return eTreatmentType.OTHER;
                    case 800:
                    //rapport graphique
                    case 801:
                    //ajout
                    case 802:
                    //modif
                    case 803:
                        //supprimer
                        return eTreatmentType.ANALYZE;
                    case 811:
                    //gestion de seuil : export
                    case 812:
                        //gestion de seuil : mail
                        return eTreatmentType.OTHER;
                    case 820:
                    // Droits d'export en format Power BI
                    case 821:
                    // Droit d'ajout d'un rapport Power BI
                    case 822:
                    // Droit de modification d'un rapport Power BI
                    case 823:
                        return eTreatmentType.COMMUNICATION;
                }
            }

            return GetType(TreatLoc);
        }


        /// <summary>
        /// Type du traitement
        /// </summary>
        public static eTreatmentType GetType(eLibConst.TREATMENTLOCATION tl)
        {
            switch (tl)
            {
                case eLibConst.TREATMENTLOCATION.None:
                    break;
                case eLibConst.TREATMENTLOCATION.Trait:
                    break;
                case eLibConst.TREATMENTLOCATION.ViewPermId:
                case eLibConst.TREATMENTLOCATION.BkmViewPermId_100:
                case eLibConst.TREATMENTLOCATION.BkmViewPermId_200:
                case eLibConst.TREATMENTLOCATION.BkmViewPermId_300:
                    return eTreatmentType.VIEW;
                    break;
                case eLibConst.TREATMENTLOCATION.UpdatePermId:
                    return eTreatmentType.CHANGE;
                    break;
                case eLibConst.TREATMENTLOCATION.AddPermission:
                case eLibConst.TREATMENTLOCATION.UpdatePermission:
                case eLibConst.TREATMENTLOCATION.DeletePermission:
                case eLibConst.TREATMENTLOCATION.SynchroPermission:
                    return eTreatmentType.CATALOG;
                default:
                    break;
            }

            return eTreatmentType.OTHER;


        }

        /// <summary>
        /// Retourne la res de type de treatment 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static String GetResType(eTreatmentType type, ePrefLite pref)
        {
            return eAdminTreatmentTypeRes.GetRes(type, pref);
        }

 

        /// <summary>
        /// Le libellé du droit en fonction de la provenance de l'info (Trait ou Desc)
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tl"></param>
        /// <returns></returns>
        public static string GetResFromTreatLocation(ePref pref, eLibConst.TREATMENTLOCATION tl)
        {
            int iResId = 0;
            switch (tl)
            {
                case eLibConst.TREATMENTLOCATION.None:
                    break;
                case eLibConst.TREATMENTLOCATION.Trait:
                    break;
                case eLibConst.TREATMENTLOCATION.ViewPermId:
                    break;
                case eLibConst.TREATMENTLOCATION.UpdatePermId:
                    iResId = 7393;
                    break;
                case eLibConst.TREATMENTLOCATION.BkmViewPermId_100:
                case eLibConst.TREATMENTLOCATION.BkmViewPermId_200:
                case eLibConst.TREATMENTLOCATION.BkmViewPermId_300:
                    iResId = 7399;
                    break;
                case eLibConst.TREATMENTLOCATION.AddPermission:
                    iResId = 1486;
                    break;
                case eLibConst.TREATMENTLOCATION.UpdatePermission:
                    iResId = 1487;
                    break;
                case eLibConst.TREATMENTLOCATION.DeletePermission:
                    iResId = 1488;
                    break;
                case eLibConst.TREATMENTLOCATION.SynchroPermission:
                    iResId = 481;
                    break;
                case eLibConst.TREATMENTLOCATION.GridViewPermission:
                    iResId = 8212; // Voir la grille
                    break;
                default:
                    break;
            }


            if (iResId == 0)
                return "";

            return eResApp.GetRes(pref, iResId);

        }


        /// <summary>
        /// Pour ordonner dans une liste suivant TypeLabel puis TraitLabel
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public int CompareTo(IAdminTreatmentRight that)
        {
            // orderby TypeLabel, TraitLabel
            int result = this.TypeLabel.CompareTo(that.TypeLabel);
            if (result == 1 || result == -1)
                return result;

            return this.TraitLabel.CompareTo(that.TraitLabel);
        }
    }

}