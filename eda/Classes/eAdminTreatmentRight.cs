using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Com.Eudonet.Xrm.eda
{



    /// <summary>
    /// Classe eAdminTreatmentRight : droit de traitement
    /// </summary>
    /// <author>CRU</author>
    /// 
    /// <remarks>05/02/2016</remarks>
    public class eAdminTreatmentRight
    {


        #region Propriétés

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
                    // Ajouter de puis une autre fiche
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
                    //ajouter une valeur dans un catalogue
                    case 52:
                    //modifier une valeur dans un catalogue
                    case 53:
                    //retirer une valeur dans un catalogue
                    case 54:
                        //catalogue avancé ?
                        return eTreatmentType.OTHER;
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


        public static String GetResType(eTreatmentType t, ePrefLite p)
        {

            switch (t)
            {
                case eTreatmentType.ATOMIC:

                    //return eResApp.GetRes(p, 0);

                    return "Action unitaire";

                case eTreatmentType.MASS:
                    return "Traitement de masse";
                case eTreatmentType.COMMUNICATION:
                    return eResApp.GetRes(p, 6854); //communucation
                case eTreatmentType.ANALYZE:
                    return eResApp.GetRes(p, 6606); //Analyse

                case eTreatmentType.PREF:
                    return eResApp.GetRes(p, 445); //Préférence
                case eTreatmentType.PJ:

                    return eResApp.GetRes(p, 5042); //annexes
                case eTreatmentType.OTHER:
                    return eResApp.GetRes(p, 75); //Autre
                case eTreatmentType.VIEW:
                    return eResApp.GetRes(p, 6594); //Visu

                case eTreatmentType.CHANGE:
                    return eResApp.GetRes(p, 805); //Modif
                case eTreatmentType.CATALOG:
                    return eResApp.GetRes(p, 225); //Catalogue
                default:
                    return "UNDEFINED";

            }
        }


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

        public eLibConst.TREATMENTLOCATION TreatLoc = eLibConst.TREATMENTLOCATION.None;

        /// <summary>
        /// préférence 
        /// </summary>
        ePref _pref;


        eudoDAL _eDal = null;
        /// <summary>
        /// Id du traitement
        /// </summary>
        int _nTraitID;


        /// <summary>
        /// Id du droit de traitement
        /// </summary>
        public int TraitID
        {
            get { return _nTraitID; }
        }

        /// <summary>
        /// Id du droit de traitement
        /// </summary>
        public int DescID = 0;

        /// <summary>
        /// Id de traitment dans EUDORES..TRAIT
        /// </summary>
        Int32 _nGlobalTraitId;

        public Int32 GlobalTraitId
        {
            get { return _nGlobalTraitId; }
        }

        /// <summary>
        /// Certains droits de traitement sont associés à une règles (les envois conditionnels )
        /// </summary>
        Int32 _nRightId = 0;

        /// <summary>
        /// Table du traitement
        /// </summary>
        Int32 _nTabDescId = 0;


        public String TabFromLabel { get; set; }
        public String FieldLabel { get; set; }

        /// <summary>
        /// Permission appliqué au traitement
        /// </summary>
        ePermission _perm;

        public ePermission Perm
        {
            get { return _perm; }
            set { _perm = value; }
        }


        private String _sTraitTabel;

        /// <summary>
        /// Libelle du droit de traitement
        /// </summary>
        public String TraitLabel
        {
            //get { return _sTraitTabel.Length > 0 ? _sTraitTabel : GetResFromTreatLocation(TreatLoc); }
            get { return _sTraitTabel; }
            set { _sTraitTabel = value; }
        }

        private String _sTabLabel;

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
        /// Retourne la requete de base de lecture des droits de traitements avec gestion de la langue
        /// </summary>
        /// <param name="pref"></param>
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

        public eAdminTreatmentRight(ePref pref)
        {
            _pref = pref;
        }

        #region oldcode 22/11/2016

        ///// <summary>
        ///// Retourne un droit de traitement par son ID
        ///// </summary>
        ///// <param name="pref"></param>
        ///// <param name="nTraitId"></param>
        ///// <param name="bLoad">Récupère-t-on les infos du droit de traitement ?</param>
        ///// <returns></returns>
        //public static eAdminTreatmentRight GetTreatmentRight(ePref pref, Int32 nTraitId)
        //{

        //    eAdminTreatmentRight oMyTreatRight = new eAdminTreatmentRight(pref, nTraitId);

        //    try
        //    {

        //        oMyTreatRight.LoadTreatment();
        //    }
        //    catch
        //    {

        //        throw;
        //    }
        //    finally
        //    {

        //        if (oMyTreatRight != null && oMyTreatRight._eDal != null)
        //            oMyTreatRight._eDal.CloseDatabase();
        //    }


        //    return oMyTreatRight;
        //}



        ///// <summary>
        ///// Retourne un objet de traitement
        ///// </summary>
        ///// <param name="nTraitID">Id du traitement</param>
        ///// <param name="nGlobalTraitId">Id 'globale', servant a caractérisé le traitment. cf eudores..trait </param>
        ///// <param name="nTab">Descid de la table</param>
        ///// <param name="perm">Permission du droit</param>
        ///// <param name="nRulesId">Règle du traitement</param>
        ///// <returns></returns>
        //public static eAdminTreatmentRight CreateTreatmentRight(Int32 nTraitID, Int32 nGlobalTraitId, Int32 nTab, ePermission perm = null, Int32 nRulesId = 0, String sTraitLabel = "", String sTabLabel = "")
        //{

        //    eAdminTreatmentRight a = new eAdminTreatmentRight(nTraitID, nGlobalTraitId, nTab, perm, nRulesId);
        //    a.TraitLabel = sTraitLabel;
        //    a.TabLabel = sTabLabel;

        //    return a;
        //}



        ///// <summary>
        ///// Charge le traitement
        ///// </summary>
        //protected void LoadTreatment()
        //{
        //    //if (_nTraitID <= 0)
        //    //    throw new Exception("Id de traitement invalide.");

        //    try
        //    {
        //        _eDal = eLibTools.GetEudoDAL(_pref);
        //        _eDal.OpenDatabase();

        //        StringBuilder sbSQL = new StringBuilder(ApplyLanguage(_pref));
        //        RqParam rq = new RqParam();
        //        if (_nTraitID > 0)
        //        {
        //            sbSQL.Append(" WHERE RIGHTS.[TRAITID] = @TRAITID");
        //            rq.AddInputParameter("@TRAITID", SqlDbType.Int, _nTraitID);
        //        }
        //        else if (
        //            _treatLoc == eLibConst.TREATMENTLOCATION.ViewPermId
        //            || _treatLoc == eLibConst.TREATMENTLOCATION.UpdatePermId
        //            || _treatLoc == eLibConst.TREATMENTLOCATION.BkmViewPermId_100
        //            || _treatLoc == eLibConst.TREATMENTLOCATION.BkmViewPermId_200
        //            || _treatLoc == eLibConst.TREATMENTLOCATION.BkmViewPermId_300
        //            )
        //        {

        //        }
        //        rq.SetQuery(sbSQL.ToString());

        //        String sError = String.Empty;
        //        DataTableReaderTuned dtr = _eDal.Execute(rq, out sError);

        //        if (sError.Length > 0)
        //        {
        //            if (_eDal.InnerException != null)
        //                throw _eDal.InnerException;
        //            else
        //                throw new Exception(sError);
        //        }

        //        if (dtr == null || !dtr.Read())
        //            throw new Exception("Pas de traitement pour cet ID.");



        //        // récupération de la permission
        //        Int32 nPermId = dtr.GetEudoNumeric("PERMID");
        //        _perm = new ePermission(0, ePermission.PermissionMode.MODE_NONE, 0, "");
        //        if (nPermId > 0)
        //        {
        //            Int32 nPermMode = dtr.GetEudoNumeric("MODE");
        //            ePermission.PermissionMode pmode = ePermission.PermissionMode.MODE_NONE;
        //            if (Enum.TryParse(nPermMode.ToString(), out pmode))
        //                _perm = new ePermission(nPermId, pmode, dtr.GetEudoNumeric("LEVEL"), dtr.GetString("USER"));
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (_eDal != null)
        //            _eDal.CloseDatabase();
        //    }
        //}


        ///// <summary>
        ///// Retourne une liste de droits de traitement en fonction de son type et/ou sa table.
        ///// Passé null/0 pour ignorer le critère
        ///// </summary>
        ///// <param name="pref">préférence</param>
        ///// <param name="nType">Liste des types de traitement - TODO : maj l'énum et voir rma/tge pour l'affectation de chaque traitement à son type</param>
        ///// <param name="nTabId">Table du droit de traitement</param>
        ///// <returns></returns>
        //public static List<eAdminTreatmentRight> GetListTreament(ePref pref, HashSet<eTreatmentType> lstType = null, Int32 nTabId = 0)
        //{

        //    if (pref == null)
        //        throw new Exception("ePref non fourni");
        //    List<eAdminTreatmentRight> lst = new List<eAdminTreatmentRight>();

        //    //Si pas de filtre sur type, init de la var
        //    if (lstType == null)
        //        lstType = new HashSet<eTreatmentType>();

        //    eudoDAL eDal = null;
        //    try
        //    {
        //        eDal = eLibTools.GetEudoDAL(pref);
        //        eDal.OpenDatabase();

        //        String sSQL = "";
        //        sSQL = ApplyLanguageOnQuery(pref, sSQL);

        //        //Le filtre sur la table est ajouté sur la rq
        //        if (nTabId != 0)
        //            sSQL = String.Concat(sSQL, " WHERE ([DESCID] = @TAB )");

        //        RqParam rq = new RqParam(sSQL);
        //        rq.AddInputParameter("@TAB", SqlDbType.Int, nTabId);



        //        String sError = String.Empty;
        //        DataTableReaderTuned dtr = eDal.Execute(rq, out sError);

        //        if (sError.Length > 0)
        //        {

        //            if (eDal.InnerException != null)
        //                throw eDal.InnerException;
        //            else
        //                throw new Exception(sError);
        //        }

        //        if (dtr == null)
        //            throw new Exception("Erreur de récupération des traitements");

        //        try
        //        {

        //            while (dtr.Read())
        //            {

        //                Int32 nGlobalId = 0;

        //                nGlobalId = dtr.GetEudoNumeric("g_TraitId");
        //                if (nGlobalId == 0)
        //                    continue;

        //                String sTabLabel = dtr.GetString("TabLabel");
        //                String sTraitLabel = dtr.GetString("TraitLabel");

        //                if (sTraitLabel.Length == 0)
        //                    continue;


        //                //Filtre sur les types choisis
        //                if (lstType != null && lstType.Count > 0 && !lstType.Contains(eAdminTreatmentRight.GetType(nGlobalId)))
        //                    continue;

        //                Int32 nTraitId = 0;
        //                Int32 nTab = 0;

        //                nTraitId = dtr.GetEudoNumeric("l_TraitId");


        //                Int32 nPermId = 0;
        //                ePermission perm = new ePermission(0, ePermission.PermissionMode.MODE_NONE, 0, "");
        //                ePermission.PermissionMode pMode = ePermission.PermissionMode.MODE_NONE;



        //                //permission
        //                nPermId = dtr.GetEudoNumeric("PERMID");
        //                if (nPermId > 0)
        //                {

        //                    String sUser = dtr.GetString("USER");
        //                    Int32 nMode = dtr.GetEudoNumeric("MODE");

        //                    Int32 nLevel = dtr.GetEudoNumeric("LEVEL");
        //                    if (!Enum.TryParse(nMode.ToString(), out pMode))
        //                        pMode = ePermission.PermissionMode.MODE_NONE;

        //                    perm = new ePermission(nPermId, pMode, nLevel, sUser);
        //                    if (perm.PermUser.Length > 0)
        //                        perm.LoadUserPermLabel(eDal);

        //                }

        //                lst.Add(eAdminTreatmentRight.CreateTreatmentRight(
        //                    nTraitId,
        //                    nGlobalId,
        //                    nTab,
        //                    perm,
        //                    0,
        //                    sTraitLabel,
        //                    sTabLabel
        //                    )
        //                  );
        //            }
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        finally
        //        {
        //            dtr?.Dispose();
        //        }
        //        return lst;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        eDal?.CloseDatabase();
        //    }
        //}

        ///// <summary>
        ///// Met à jour une liste de droit avec la une permission
        ///// </summary>
        ///// <param name="pref">pref</param>
        ///// <param name="lstTreat">liste des traitements à appliquer</param>
        ///// <param name="perm">nouvelle permission </param>
        //public static void UpdateTreatmentRight(ePref pref, List<eAdminTreatmentRight> lstTreat, ePermission perm)
        //{

        //    foreach (eAdminTreatmentRight e in lstTreat)
        //    {
        //        Int32 treatId = e._nTraitID;
        //    }

        //}


        ///// <summary>
        ///// Création d'un droit de traitement à partir de ces paramètres
        ///// </summary>
        ///// <param name="nTraitID">Id du traitement</param>
        ///// <param name="nGlobalTraitId">Id 'globale', servant a caractérisé le traitment. cf eudores..trait </param>
        ///// <param name="nTab">Descid de la table</param>
        ///// <param name="perm">Permission du droit</param>
        ///// <param name="nRulesId">Règle du traitement</param>
        //protected eAdminTreatmentRight(Int32 nTraitID, Int32 nGlobalTraitId, Int32 nTab, ePermission perm, Int32 nRulesId)
        //{
        //    _nGlobalTraitId = nGlobalTraitId;
        //    _nTraitID = nTraitID;
        //    _nTabDescId = nTab;
        //    _perm = perm;
        //    _nRightId = nRulesId;
        //}

        ///// <summary>
        ///// Constructeur : initialisation des propriétés
        ///// </summary>
        ///// <param name="pref">ePref</param>
        ///// <param name="eDal">Connexion ouverte</param>
        ///// <param name="traitId">ID du traitement</param>
        //protected eAdminTreatmentRight(ePref pref, int traitId)
        //{
        //    _nTraitID = traitId;
        //    _pref = pref;
        //    _perm = null;
        //}

        ///// <summary>Chargement de la permission correspondant au droit de traitement : PermId, Mode, Level, User </summary>
        ///// <returns>Booléen</returns>
        //public Boolean LoadTreatmentPermission()
        //{
        //    String error = String.Empty;

        //    String sql = @"SELECT p.PermissionId, ISNULL(Mode, 0), ISNULL([Level], 1), ISNULL([User], '') FROM PERMISSION p
        //        LEFT JOIN TRAIT t ON t.PermId = p.PermissionId
        //        WHERE TraitId = @traitID";
        //    RqParam rq = new RqParam(sql);
        //    rq.AddInputParameter("@traitID", System.Data.SqlDbType.Int, _nTraitID);
        //    DataTableReaderTuned dtr = _eDal.Execute(rq, out error);
        //    try
        //    {
        //        if (String.IsNullOrEmpty(error) && dtr != null && dtr.Read())
        //        {
        //            ePermission.PermissionMode pMode = (ePermission.PermissionMode)dtr.GetEudoNumeric("Mode");

        //            int level = eLibTools.GetNum(dtr.GetEudoNumeric("Level").ToString());

        //            String users = dtr.GetString("User").ToString();

        //            _perm = new ePermission(dtr.GetEudoNumeric("PermissionId"), pMode, level, users);
        //        }
        //        else
        //        {
        //            eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), _pref);
        //            return false;
        //        }
        //    }
        //    finally
        //    {
        //        dtr?.Dispose();
        //    }

        //    return true;
        //}

        /// <summary>Enregistre la permission et met à jour le traitement si nécessaire</summary>
        /// <param name="level">Niveau d'utilisateur</param>
        /// <param name="users">Utilisateurs ou groupes concernés</param>
        /// <returns>Booléen</returns>
        //public Boolean UpdatePermission(int level = 0, String users = "")
        //{
        //    Boolean savePermId = false;
        //    if (_perm != null)
        //    {
        //        _perm.PermLevel = level;
        //        _perm.PermUser = users;
        //        _perm.SetPermMode(); // Permet d'affecter le mode par rapport au Level et au User

        //        if (_perm.PermId == 0)
        //        {
        //            savePermId = true;
        //        }

        //        return _perm.Save(_pref, savePermId, TraitID);

        //    }
        //    return false;
        //}


        ///// <summary>Récupère l'ID de la table (si le droit est lié à une table)</summary>
        //public int LoadTreatmentTable()
        //{
        //    return 0;
        //}
        #endregion

        ///// <summary>
        ///// Le libellé du droit en fonction de la provenance de l'info (Trait ou Desc)
        ///// </summary>
        ///// <param name="tl"></param>
        ///// <returns></returns>
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
                default:
                    break;
            }


            if (iResId == 0)
                return "";

            return eResApp.GetRes(pref, iResId);

        }

    }


    /// <summary>
    /// type possible des traitements
    /// </summary>
    public enum eTreatmentType
    {

        /// <summary>Action unitaire (création/suppression unitaire) /// </summary>
        ATOMIC = 0,

        /// <summary>Action de traitement de masse (tous les traitements de masse de maj, duppli, suppression...)</summary>
        MASS = 1,

        /// <summary>Action liée aux options de communication</summary>
        COMMUNICATION,

        /// <summary>Action liées aux options d'analyse (raport...) </summary>
        ANALYZE,

        /// <summary>Action liées à la manipulation des pref (crée/définir des sélections) </summary>
        PREF,

        /// <summary>Action liée aux pj </summary>
        PJ,

        /// <summary>Autres...</summary>
        OTHER,

        /// <summary>Visualisation </summary>
        VIEW,

        /// <summary>Modification </summary>
        CHANGE,

        /// <summary>Catalogues </summary>
        CATALOG


    }

    public class eAdminTreatmentRightList
    {
        /// <summary>Filtre sur "Depuis" (Signet depuis event, PP, PM)</summary>
        public Int32 From = 0;
        /// <summary>Filtre sur "Onglet"</summary>
        public Int32 Tab = 0;
        /// <summary>Filtre sur "Rubrique"</summary>
        public Int32 Field = 0;
        /// <summary>Filtre sur le nom de la fonction</summary>
        public string Function = "";
        /// <summary>Filtre sur "Type de traitement"</summary>
        public HashSet<eTreatmentType> TreatTypes = new HashSet<eTreatmentType>();


        public List<eAdminTreatmentRight> RightsList = new List<eAdminTreatmentRight>();

        private ePref _pref;

        public eAdminTreatmentRightList(ePref pref)
        {
            _pref = pref;
        }

        /// <summary>
        /// Retourne une liste de droits de traitement en fonction de son type et/ou sa table.
        /// Passé null/0 pour ignorer le critère
        /// </summary>
        /// <returns></returns>
        public void LoadTreamentsList()
        {

            if (_pref == null)
                throw new Exception("ePref non fourni");

            //List<eAdminTreatmentRight> lst = new List<eAdminTreatmentRight>();

            ////Si pas de filtre sur type, init de la var
            //if (lstType == null)
            //    lstType = new HashSet<eTreatmentType>();

            eudoDAL eDal = null;
            try
            {
                eDal = eLibTools.GetEudoDAL(_pref);
                eDal.OpenDatabase();

                #region on rajoute les enregistrements potentiellement manquants dans FileDataParam et dans trait
                String sError;

                eSqlTrait.AddTableProcessRights(_pref, eDal, Tab, out sError);
                if (sError.Length > 0)
                {
                    eFeedbackXrm.LaunchFeedbackXrm(
                        eErrorContainer.GetDevError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            String.Concat("eAdminTreatmentRight.LoadTreatmentList --> eSqlTrait.AddTableProcessRights : ", Environment.NewLine, sError)
                            )
                        , _pref
                        );
                }

                eSqlFiledataParam.CreateMissing(eDal, Tab, out sError);
                if (sError.Length > 0)
                {
                    eFeedbackXrm.LaunchFeedbackXrm(
                        eErrorContainer.GetDevError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            String.Concat("eAdminTreatmentRight.LoadTreatmentList : ", Environment.NewLine, sError)
                            )
                        , _pref
                        );
                }
                #endregion

                StringBuilder sbSQL = new StringBuilder();
                RqParam rq = new RqParam();
                List<String> lstSubRequest = new List<String>();

                //droits de traitements tels qu'ils existaient en V7
                lstSubRequest.Add(TraitSubQuery);

                //Droits de Visu (DESC.ViewPermid)
                eLibConst.TREATMENTLOCATION tl = eLibConst.TREATMENTLOCATION.ViewPermId;
                string sViewQuery = GetViewQuery(tl);
                lstSubRequest.Add(sViewQuery);

                rq.AddInputParameter("@ViewFieldLabel", SqlDbType.VarChar, eResApp.GetRes(_pref, 7392));
                rq.AddInputParameter("@ViewTabLabel", SqlDbType.VarChar, eResApp.GetRes(_pref, 7391));

                //Droits de Visu (DESC.UpdatePermid)
                tl = eLibConst.TREATMENTLOCATION.UpdatePermId;
                string sUpdateQuery = GetUpdateQuery(eLibConst.TREATMENTLOCATION.UpdatePermId);
                lstSubRequest.Add(sUpdateQuery);

                //Droits de visu du signet depuis un event
                tl = eLibConst.TREATMENTLOCATION.BkmViewPermId_100;
                string sBkmViewEventQuery = GetBkmViewEventQuery(tl);
                lstSubRequest.Add(sBkmViewEventQuery);

                //Droits de visu du signet depuis une PP
                tl = eLibConst.TREATMENTLOCATION.BkmViewPermId_200;
                string sBkmViewPpQuery = GetBkmViewPpPmQuery(TableType.PP, tl);
                lstSubRequest.Add(sBkmViewPpQuery);

                //Droits de visu du signet depuis une PM
                tl = eLibConst.TREATMENTLOCATION.BkmViewPermId_300;
                string sBkmViewPmQuery = GetBkmViewPpPmQuery(TableType.PM, tl);
                lstSubRequest.Add(sBkmViewPmQuery);

                rq.AddInputParameter("@tabADR", SqlDbType.Int, (int)TableType.ADR);

                foreach (eLibConst.TREATMENTLOCATION tltmp in Enum.GetValues(typeof(eLibConst.TREATMENTLOCATION)))
                {
                    rq.AddInputParameter(String.Concat("@", tltmp), SqlDbType.Int, (int)tltmp);
                    rq.AddInputParameter(String.Concat("@", tltmp, "label"), SqlDbType.VarChar, eAdminTreatmentRight.GetResFromTreatLocation(_pref, tltmp));
                }

                #region droits sur les signets
                // lors de l'exécution des filtres, en fonctions des critères toutes les requêtes ne sont pas nécessaires
                if (From > 0)
                {
                    lstSubRequest.Clear();
                    if (From == (Int32)TableType.PP)
                    {
                        //Droits de visu du signet depuis une PP
                        lstSubRequest.Add(sBkmViewPpQuery);
                    }
                    else if (From == (Int32)TableType.PM)
                    {
                        //Droits de visu du signet depuis une PM
                        lstSubRequest.Add(sBkmViewPmQuery);
                    }
                    else
                    {
                        //Droits de visu du signet depuis un event
                        lstSubRequest.Add(GetBkmViewEventQuery(eLibConst.TREATMENTLOCATION.BkmViewPermId_100));
                    }
                }
                else if (From == -1) // Aucun
                {
                    //Droits de visu du signet depuis un event
                    lstSubRequest.Remove(sBkmViewEventQuery);

                    //Droits de visu du signet depuis une PP
                    lstSubRequest.Remove(sBkmViewPpQuery);

                    //Droits de visu du signet depuis une PM
                    lstSubRequest.Remove(sBkmViewPmQuery);
                }
                #endregion

                // si on affiche tous les onglets, on n'affiche pas les propriétés des rubriques 
                // sinon la requête est beaucoup trop longue
                // CRU : Sauf pour les catalogues
                //if (Tab == 0 || From > 0)
                //    Field = -1;

                #region Droits sur les rubriques
                if (Field > 0)
                { // si une rubrique a été sélectionnée on ne montre que les droits de visu et de modif
                    lstSubRequest.RemoveRange(0, lstSubRequest.Count);

                    //Droits de Visu (DESC.ViewPermid)
                    lstSubRequest.Add(sViewQuery);

                    //Droits de Visu (DESC.UpdatePermid)
                    lstSubRequest.Add(sUpdateQuery);
                }
                else if (Field == -1) // aucune rubrique : on ne consulte update permid que pour les rubriques
                {
                    //Droits de Visu (DESC.UpdatePermid)
                    lstSubRequest.Remove(sUpdateQuery);
                }
                #endregion

                #region Droits sur les catalogues
                if (Field > -1)
                {
                    //dans le cas où l'on souhaite n'accéder qu'aux paramétres des catalogues, toutes les autres requêtes sont inutiles (performances)
                    if (TreatTypes.Count == 1 && TreatTypes.Contains(eTreatmentType.CATALOG))
                        lstSubRequest.RemoveRange(0, lstSubRequest.Count);

                    //droits sur les catalogues si le filtre ne porte pas sur aucune rubrique.
                    tl = eLibConst.TREATMENTLOCATION.AddPermission;
                    lstSubRequest.Add(GetBaseFileDataParamQuery(tl));

                    tl = eLibConst.TREATMENTLOCATION.UpdatePermission;
                    lstSubRequest.Add(GetBaseFileDataParamQuery(tl));

                    tl = eLibConst.TREATMENTLOCATION.DeletePermission;
                    lstSubRequest.Add(GetBaseFileDataParamQuery(tl));

                    tl = eLibConst.TREATMENTLOCATION.SynchroPermission;
                    lstSubRequest.Add(GetBaseFileDataParamQuery(tl));

                    rq.AddInputParameter("@popupnone", SqlDbType.Int, (int)PopupType.NONE);
                }
                #endregion


                #region droits globaux : on ne garde que les droits globaux
                if (Tab == -1) {
                    lstSubRequest.Clear();
                    //droits de traitements tels qu'ils existaient en V7
                    lstSubRequest.Add(TraitSubQuery);
                }
                #endregion

                String sSeparator = String.Concat(Environment.NewLine, "UNION", Environment.NewLine);

                String sSubRequests = eLibTools.Join<string>(sSeparator, lstSubRequest);

                sbSQL.Append(_sMainSqlSelect)
                    .Append(sSubRequests).AppendLine()
                    .AppendLine(_sMainSqlEnd);

                StringBuilder sbSQLWhere = new StringBuilder();
                //Le filtre sur la table ou la rubrique est ajouté sur la rq
                if (Tab == -1) //Droits Globaux
                {
                    sbSQLWhere.Append(" WHERE (RIGHTS.[DESCID] = 0)");
                }
                //else if (Tab == 0 && Field == 0) //tous les onglets => aucune rubrique
                //{
                //    sbSQLWhere.Append(" WHERE (RIGHTS.[DESCID] % 100 = 0)");
                //    //// rq.AddInputParameter("@descid", SqlDbType.Int, Tab);
                //}
                else if (Field == 0 && Tab > 0) //Field == 0 : toutes les rubriques
                {
                    sbSQLWhere.Append(" WHERE (RIGHTS.[DESCID] BETWEEN @descid AND @Descid + 99)");
                    rq.AddInputParameter("@descid", SqlDbType.Int, Tab);
                }
                else if (Field > 0 || Field == -1) //Field == -1 :  Aucune rubrique
                {
                    sbSQLWhere.Append(" WHERE (RIGHTS.[DESCID] = @descid)");
                    rq.AddInputParameter("@descid", SqlDbType.Int, Field > 0 ? Field : Tab);
                }

                if (Function.Length > 0)
                {
                    if (sbSQLWhere.Length > 0)
                    {
                        sbSQLWhere.AppendLine().Append("AND ");
                    }
                    else
                    {
                        sbSQLWhere.AppendLine().Append("WHERE ");
                    }

                    sbSQLWhere.Append("RIGHTS.TraitLabel = @function");
                    rq.AddInputParameter("@function", SqlDbType.VarChar, Function);
                }

                sbSQL.Append(sbSQLWhere);
                if (Tab == 0 && Field == 0)
                    sbSQL.AppendLine().Append("ORDER BY RIGHTS.DESCID");
                sbSQL.Replace("@@LNG@@", _pref.User.UserLang);

                rq.SetQuery(sbSQL.ToString());

                sError = String.Empty;
                DataTableReaderTuned dtr = eDal.Execute(rq, out sError);

                if (sError.Length > 0)
                {
                    if (eDal.InnerException != null)
                        throw eDal.InnerException;
                    else
                        throw new Exception(sError);
                }

                if (dtr == null)
                    throw new Exception("Erreur de récupération des traitements");

                try
                {
                    eAdminTreatmentRight adminRight = null;
                    while (dtr.Read())
                    {
                        adminRight = new eAdminTreatmentRight(_pref);
                        adminRight.TreatLoc = (eLibConst.TREATMENTLOCATION)dtr.GetInt32("TraitLocation");

                        if (adminRight.TreatLoc == eLibConst.TREATMENTLOCATION.Trait)
                        {
                            //Propriétés en provenance de Trait
                            adminRight.SetTrait(nTraitID: dtr.GetEudoNumeric("l_TraitId"), nGlobalTraitId: dtr.GetEudoNumeric("g_TraitId"));

                            //Filtre sur les types choisis
                            if (TreatTypes != null && TreatTypes.Count > 0 && !TreatTypes.Contains(adminRight.Type))
                                continue;
                        }
                        else
                        {
                            //Filtre sur les types choisis
                            if (TreatTypes != null && TreatTypes.Count > 0 && !TreatTypes.Contains(eAdminTreatmentRight.GetType(adminRight.TreatLoc)))
                                continue;

                            //Propriétés en provenance de DESC
                            adminRight.SetFromDesc(dtr.GetString("FromLabel"), dtr.GetString("FieldLabel"));
                        }

                        adminRight.DescID = dtr.GetEudoNumeric("DESCID");
                        if (adminRight.DescID > 0)
                        {
                            adminRight.TabLabel = dtr.GetString("TabLabel");
                        }
                        else
                        {
                            adminRight.TabLabel = eResApp.GetRes(_pref, 7611);
                        }
                        adminRight.TraitLabel = dtr.GetString("TraitLabel");

                        switch (adminRight.TreatLoc)
                        {
                            case eLibConst.TREATMENTLOCATION.None:
                            case eLibConst.TREATMENTLOCATION.Trait:
                            case eLibConst.TREATMENTLOCATION.ViewPermId:
                            case eLibConst.TREATMENTLOCATION.UpdatePermId:
                            case eLibConst.TREATMENTLOCATION.BkmViewPermId_100:
                            case eLibConst.TREATMENTLOCATION.BkmViewPermId_200:
                            case eLibConst.TREATMENTLOCATION.BkmViewPermId_300:
                                adminRight.DescID = dtr.GetEudoNumeric("DESCID");
                                break;
                            case eLibConst.TREATMENTLOCATION.AddPermission:
                            case eLibConst.TREATMENTLOCATION.UpdatePermission:
                            case eLibConst.TREATMENTLOCATION.DeletePermission:
                            case eLibConst.TREATMENTLOCATION.SynchroPermission:
                                adminRight.DescID = dtr.GetEudoNumeric("FdpDescId");

                                break;
                            default:
                                break;
                        }

                        if (adminRight.TraitLabel.Length == 0)
                            continue;

                        // dans le cas des signets "Affaires depuis Affaires" et "Annexes" il faut intervertir des libellés
                        if (adminRight.DescID % 100 == (int)AllField.ATTACHMENT || adminRight.DescID % 100 == (int)AllField.BKM_PM_EVENT)
                        {
                            adminRight.TabFromLabel = adminRight.TabLabel;
                            adminRight.TabLabel = adminRight.FieldLabel;
                            adminRight.FieldLabel = "";
                        }
                        ePermission.PermissionMode pMode = ePermission.PermissionMode.MODE_NONE;

                        //permission
                        Int32 nPermId = dtr.GetEudoNumeric("PERMID");
                        if (nPermId > 0)
                        {

                            String sUser = dtr.GetString("USER");
                            Int32 nMode = dtr.GetEudoNumeric("MODE");

                            Int32 nLevel = dtr.GetEudoNumeric("LEVEL");
                            if (!Enum.TryParse(nMode.ToString(), out pMode))
                                pMode = ePermission.PermissionMode.MODE_NONE;

                            adminRight.Perm = new ePermission(nPermId, pMode, nLevel, sUser);
                            if (adminRight.Perm.PermUser.Length > 0)
                                adminRight.Perm.LoadUserPermLabel(eDal);
                        }
                        else
                        {
                            adminRight.Perm = new ePermission(0, ePermission.PermissionMode.MODE_NONE, 0, "");
                        }



                        RightsList.Add(adminRight);


                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    dtr?.Dispose();
                }
                //return lst;
            }
            catch
            {
                throw;
            }
            finally
            {
                eDal?.CloseDatabase();
            }

        }


        /// <summary>Select de la requête globale  </summary>
        private static string _sMainSqlSelect = String.Concat("SELECT RIGHTS.*  ", Environment.NewLine,
                                                                "    , IsNull(PERMISSION.PermissionId, 0) PERMID  ", Environment.NewLine,
                                                                "    , IsNull(PERMISSION.Mode, -1) MODE  ", Environment.NewLine,
                                                                "    , IsNull(PERMISSION.LEVEL, 0) LEVEL  ", Environment.NewLine,
                                                                "    , IsNull(PERMISSION.[User], '')[USER]  ", Environment.NewLine,
                                                                "FROM(  ", Environment.NewLine
                                                                );
        /// <summary>Select de la requête globale  </summary>
        private static string _sMainSqlEnd = String.Concat("	) AS RIGHTS  ", Environment.NewLine,
                                                             "LEFT JOIN PERMISSION ON RIGHTS.PERMID = PERMISSION.PermissionId  ", Environment.NewLine
                                                                );

        /// <summary>
        /// Requete sql de query sur la table Trait
        /// </summary>      
        protected static String TraitSubQuery = String.Concat(@"	/*Droits de traitements globaux*/", Environment.NewLine,
                                                                     "	SELECT ISNULL(RES.ResId,0) AS DescId  ", Environment.NewLine,
                                                                     "		,RES.@@LNG@@ TabLabel  ", Environment.NewLine,
                                                                     "		,'' FromLabel  ", Environment.NewLine,
                                                                     "		,'' FieldLabel  ", Environment.NewLine,
                                                                     "		,@", eLibConst.TREATMENTLOCATION.Trait, " TraitLocation  ", Environment.NewLine,
                                                                     "		,gt.traitid g_TraitId  ", Environment.NewLine,
                                                                     "		,lt.TRAITID l_TraitId  ", Environment.NewLine,
                                                                     "		,gt.@@LNG@@ TraitLabel  ", Environment.NewLine,
                                                                     "		,IsNull(PERMID, 0) PermId  ", Environment.NewLine,
                                                                     "      , 0 FdpDescId  ", Environment.NewLine,
                                                                     "	FROM TRAIT lt  ", Environment.NewLine,
                                                                     "	LEFT JOIN RES ON (lt.TRAITID - lt.TRAITID % 100) = ISNULL(RES.ResId,0)  ", Environment.NewLine,
                                                                     "	LEFT JOIN EUDORES..TRAIT gt ON (  ", Environment.NewLine,
                                                                     "			    (  ", Environment.NewLine,
                                                                     "				CASE   ", Environment.NewLine,
                                                                     "					WHEN lt.TraitId < 100  ", Environment.NewLine,
                                                                     "						THEN gt.TraitId  ", Environment.NewLine,
                                                                     "					WHEN gt.TraitId BETWEEN 100  ", Environment.NewLine,
                                                                     "							AND 199  ", Environment.NewLine,
                                                                     "						THEN (gt.TraitId - 100) + ISNULL(RES.ResId,0)  ", Environment.NewLine,
                                                                     "					WHEN gt.TraitId < 100  ", Environment.NewLine,
                                                                     "						OR gt.TraitId >= 200  ", Environment.NewLine,
                                                                     "						THEN gt.TraitId  ", Environment.NewLine,
                                                                     "					END  ", Environment.NewLine,
                                                                     "				) = lt.TraitId  ", Environment.NewLine,
                                                                     "	    )  ", Environment.NewLine,
                                                                     "	WHERE gt.TraitId > 0", Environment.NewLine,
                                                                     "	    AND NOT lt.TraitId between 51 AND 54", Environment.NewLine,
                                                                     ""

                                                                    );
        /// <summary>Requête SQL pour les Droits de Visu sur l'onglet et les rubriques</summary>
        /// <param name="tl">ViewPermid ou UpdatePermid</param>
        /// <returns></returns>
        protected static string GetViewQuery(eLibConst.TREATMENTLOCATION tl)
        {
            return String.Concat("	/*Droits de Visu sur l'onglet, les rubriques et les signets*/", Environment.NewLine,
                                  "	  SELECT DescId  ", Environment.NewLine,
                                  "        , TABRES.@@LNG@@ TabLabel  ", Environment.NewLine,
                                  "        , '' FromLabel  ", Environment.NewLine,
                                  "        , CASE WHEN TABRES.ResId < RES.ResId THEN RES.@@LNG@@ ELSE '' END FieldLabel  ", Environment.NewLine,
                                  "        , @", tl, " TraitLocation  ", Environment.NewLine,
                                  "        , 0 gtraitid  ", Environment.NewLine,
                                  "        , 0 ltrait  ", Environment.NewLine,
                                  "        , CASE  ", Environment.NewLine,
                                  "            WHEN Descid % 100 IN(  ", Environment.NewLine,
                                  "                    91  ", Environment.NewLine,
                                  "                    , 87  ", Environment.NewLine,
                                  "                    )  ", Environment.NewLine,
                                  "                THEN @BkmViewPermId_100label  ", Environment.NewLine,
                                  "            WHEN DescId % 100 > 0  ", Environment.NewLine,
                                  "                THEN @ViewFieldLabel  ", Environment.NewLine,
                                  "            ELSE @ViewTabLabel  ", Environment.NewLine,
                                  "            END TraitLabel  ", Environment.NewLine,
                                  "        , IsNull(", tl, ", 0) PermId  ", Environment.NewLine,
                                  "        , 0 FdpDescId  ", Environment.NewLine,
                                  "   FROM [DESC] d  ", Environment.NewLine,
                                  "   LEFT JOIN RES ON d.DescId = RES.ResId  ", Environment.NewLine,
                                  "   LEFT JOIN RES TABRES ON TABRES.ResId + (RES.ResId % 100) = RES.ResId ", Environment.NewLine
                    );
        }

        /// <summary>Requête SQL pour les Droits de Visu sur l'onglet et les rubriques</summary>
        /// <param name="tl">ViewPermid ou UpdatePermid</param>
        /// <returns></returns>
        protected static string GetUpdateQuery(eLibConst.TREATMENTLOCATION tl)
        {
            return String.Concat("	/*Droits de Visu ou de mise à jour sur les rubriques*/", Environment.NewLine,
                                  "	  SELECT DescId  ", Environment.NewLine,
                                  "        , TABRES.@@LNG@@ TabLabel  ", Environment.NewLine,
                                  "        , '' FromLabel  ", Environment.NewLine,
                                  "        , RES.@@LNG@@ FieldLabel  ", Environment.NewLine,
                                  "        , @", tl, " TraitLocation  ", Environment.NewLine,
                                  "        , 0 gtraitid  ", Environment.NewLine,
                                  "        , 0 ltrait  ", Environment.NewLine,
                                  "        , @", tl, "label TraitLabel  ", Environment.NewLine,
                                  "        , IsNull(", tl, ", 0) PermId  ", Environment.NewLine,
                                  "        , 0 FdpDescId  ", Environment.NewLine,
                                  "   FROM [DESC] d  ", Environment.NewLine,
                                  "   LEFT JOIN RES ON d.DescId = RES.ResId  ", Environment.NewLine,
                                  "   LEFT JOIN RES TABRES ON TABRES.ResId + (RES.ResId % 100) = RES.ResId  ", Environment.NewLine,
                                  "   WHERE Descid % 100 > 0  AND DescId % 100 Not In (91, 87)"
                    );
        }


        /// <summary>Requête pour les Droits de Visu sur le signet depuis event</summary>
        protected static string GetBkmViewEventQuery(eLibConst.TREATMENTLOCATION tl)
        {
            return String.Concat("	/*Droits de Visu ou de mise à jour sur le signet depuis event*/", Environment.NewLine,
                                                                        "	 SELECT DescId  ", Environment.NewLine,
                                                                        "        , RES.@@LNG@@ TabLabel  ", Environment.NewLine,
                                                                        "        , FROMRES.@@LNG@@ FromLabel  ", Environment.NewLine,
                                                                        "        , '' FieldLabel  ", Environment.NewLine,
                                                                        "        , @", eLibConst.TREATMENTLOCATION.BkmViewPermId_100, " TraitLocation  ", Environment.NewLine,
                                                                        "        , 0 gtraitid  ", Environment.NewLine,
                                                                        "        , 0 ltrait  ", Environment.NewLine,
                                                                        "        , @", tl, "label TraitLabel  ", Environment.NewLine,
                                                                        "        , IsNull(CAST(BkmViewPermId_100 AS NUMERIC), 0) PermId  ", Environment.NewLine,
                                                                        "        , 0 FdpDescId  ", Environment.NewLine,
                                                                        "    FROM [DESC] d  ", Environment.NewLine,
                                                                        "    LEFT JOIN RES ON d.DescId = RES.ResId  ", Environment.NewLine,
                                                                        "    LEFT JOIN RES FROMRES ON(FROMRES.ResId = (d.InterEventNum + 10) * 100)  ", Environment.NewLine,
                                                                        "        OR(  ", Environment.NewLine,
                                                                        "            FROMRES.ResId = 100  ", Environment.NewLine,
                                                                        "            AND InterEventNum = 0  ", Environment.NewLine,
                                                                        "            )  ", Environment.NewLine,
                                                                        "    WHERE DescId % 100 = 0  ", Environment.NewLine,
                                                                        "        AND d.InterEvent = 1"
                                                            );
        }

        protected static string GetBkmViewPpPmQuery(TableType type, eLibConst.TREATMENTLOCATION tl)
        {
            return String.Concat("	/*Droits de Visu ou de mise à jour sur le signet depuis ", type, "*/", Environment.NewLine,
                                "	 SELECT DescId  ", Environment.NewLine,
                                "        , RES.@@LNG@@ TabLabel  ", Environment.NewLine,
                                "        , FROMRES.@@LNG@@ FromLabel  ", Environment.NewLine,
                                "        , '' FieldLabel  ", Environment.NewLine,
                                "        , @BkmViewPermId_", (int)type, " TraitLocation  ", Environment.NewLine,
                                "        , 0 gtraitid  ", Environment.NewLine,
                                "        , 0 ltrait  ", Environment.NewLine,
                                "        , @", tl, "label TraitLabel  ", Environment.NewLine,
                                "        , IsNull(CAST(BkmViewPermId_", (int)type, " AS NUMERIC), 0) PermId  ", Environment.NewLine,
                                "        , 0 FdpDescId  ", Environment.NewLine,
                                "    FROM [DESC] d  ", Environment.NewLine,
                                "    LEFT JOIN RES ON d.DescId = RES.ResId  ", Environment.NewLine,
                                "    LEFT JOIN RES FROMRES ON FROMRES.ResId = ", (int)type, "  ", Environment.NewLine,
                                "    WHERE DescId % 100 = 0  ", Environment.NewLine,
                                "        AND (", Environment.NewLine,
                                "           (d.Inter", type, " = 1", type == TableType.PM ? " OR EXISTS (SELECT PrefId FROM Pref where d.DescId = Pref.tab and UserId = 0 And AdrJoin = 1))" : ") ", Environment.NewLine,
                                "            OR DescId = @tabADR", Environment.NewLine,
                                "        ) "
                    );
        }

        /// <summary>Requête SQL pour les Droits de Visu sur l'onglet et les rubriques</summary>
        /// <param name="tl">ViewPermid ou UpdatePermid</param>
        /// <returns></returns>
        protected static string GetBaseFileDataParamQuery(eLibConst.TREATMENTLOCATION tl)
        {
            return String.Concat("	/*Droits sur les catalogues : ", tl, "*/", Environment.NewLine,
                                  "	  SELECT d.DescId DescId ", Environment.NewLine,
                                  "        , TABRES.@@LNG@@ TabLabel  ", Environment.NewLine,
                                  "        , '' FromLabel  ", Environment.NewLine,
                                  "        , RES.@@LNG@@ FieldLabel  ", Environment.NewLine,
                                  "        , @", tl, " TraitLocation  ", Environment.NewLine,
                                  "        , 0 gtraitid  ", Environment.NewLine,
                                  "        , 0 ltrait  ", Environment.NewLine,
                                  "        , @", tl, "label TraitLabel  ", Environment.NewLine,
                                  "        , IsNull(", tl, ", 0) PermId  ", Environment.NewLine,
                                  "        , fdp.DescId FdpDescId  ", Environment.NewLine,
                                  "   FROM [DESC] d  ", Environment.NewLine,
                                  "   LEFT JOIN [FILEDATAPARAM] fdp ON d.PopupDescId = fdp.DescId  ", Environment.NewLine,
                                  "   LEFT JOIN RES ON d.DescId = RES.ResId  ", Environment.NewLine,
                                  "   LEFT JOIN RES TABRES ON TABRES.ResId + (RES.ResId % 100) = RES.ResId  ", Environment.NewLine,
                                  "   WHERE d.POPUP > @popupnone "
                    );
        }




    }

}