using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using static Com.Eudonet.Core.Model.eParam;
using static EudoCommonHelper.EudoHelpers;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Usine à menus du nouveau mode Fiche
    /// </summary>
    public class FileMenuFactory
    {
        #region Propriétés

        #region Propriétés passées en paramètre au contrôleur
        private int DescId { get; set; }
        private int FileId { get; set; }
        #endregion

        #region Propriétés liées à la base de données et aux infos de contexte
        private ePref Pref { get; set; }
        private eudoDAL DAL { get; set; }
        private DataTableReaderTuned DTRTabTreatmentRights { get; set; }
        private TableLite TableInfo { get; set; }
        private eParam ParamInfo { get; set; }
        private IDictionary<eLibConst.TREATID, bool> GlobalRightsFileInfo { get; set; } = null;
        List<LOCATION_PURPLE_ACTIVATED> LocationsPurpleActivated { get; set; }
        #endregion

        #endregion

        #region Construteurs
        /// <summary>
        /// constructeur qui prend en paramètre tout ce dont a besoin la 
        /// factory pour construire FileMenuModel
        /// <param name="pref">Objet Pref</param>
        /// </summary>
        private FileMenuFactory(ePref pref)
        {
            Pref = pref;
        }

        /// <summary>
        /// constructeur qui prend en paramètre tout ce dont a besoin la 
        /// factory pour construire FileMenuModel
        /// <param name="pref">Objet Pref</param>
        /// <param name="nDescId">DescID de l'onglet</param>
        /// <param name="nFileId">ID de la fiche</param>
        /// </summary>
        private FileMenuFactory(ePref pref, int nDescId, int nFileId)
        {
            Pref = pref;
            DescId = nDescId;
            FileId = nFileId;
        }
        #endregion

        #region Static initializer
        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Prend en paramètre les prefs
        /// Retourne une instance de la classe.
        /// <param name="pref">Objet Pref</param>
        /// </summary>
        /// <returns></returns>
        public static FileMenuFactory InitFileMenuFactory(ePref pref) =>
            new FileMenuFactory(pref);

        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Prend en paramètre les prefs
        /// Retourne une instance de la classe.
        /// <param name="pref">Objet Pref</param>
        /// <param name="nDescId">DescID de l'onglet</param>
        /// <param name="nFileId">ID de la fiche</param>
        /// </summary>
        /// <returns></returns>
        public static FileMenuFactory InitFileMenuFactory(ePref pref, int nDescId, int nFileId) =>
            new FileMenuFactory(pref, nDescId, nFileId);
        #endregion

        #region Private 

        #endregion

        #region Public
        /// <summary>
        /// Récupère les informations concernant les droits de traitement depuis la base
        /// Permet de déterminer quelles entrées de menu peuvent être affichées à l'utilisateur en cours
        /// </summary>
        private void GetContextInfo()
        {
            string sError = String.Empty;
            //eConst.eFileType menuType = (eConst.eFileType) 1; // 0 = Liste, 1 = Fiche, 2 = Accueil

            try
            {
                #region Récupération des paramètres en base
                DAL = eLibTools.GetEudoDAL(Pref);
                DAL.OpenDatabase();

                TableInfo = new TableLite(DescId);
                ParamInfo = new eParam(Pref);
                if (!ParamInfo.InitTabNelleErgo(DAL, out sError))
                    throw new Exception(String.Concat(eResApp.GetRes(Pref, 416), " - ", sError)); // "Erreur - XXXX"

                DescAdvDataSet descAdv = new DescAdvDataSet();
                descAdv.LoadAdvParams(eDal: DAL,
                    listDescid: new List<int> { DescId },
                    searchedParams: new List<DESCADV_PARAMETER> { DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM }
                    );

                List<int> lstNoAdmin = eLibTools.GetDescAdvInfo(
                    Pref,
                    new List<int>() { DescId },
                    new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.NOAMDMIN })
                        .Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.NOAMDMIN && dd.Item2 == "1") != null)
                        .Select(t => t.Key)
                        .ToList();
                lstNoAdmin.Add((int)TableType.PAYMENTTRANSACTION);

                bool bNoAdmin = lstNoAdmin != null && lstNoAdmin.Contains(DescId);

                LocationsPurpleActivated = descAdv.GetAdvInfoValue(DescId, DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM)
                      .ConvertToList<LOCATION_PURPLE_ACTIVATED>(",", new Converter<string, LOCATION_PURPLE_ACTIVATED?>(
                          delegate (string s)
                          {
                              LOCATION_PURPLE_ACTIVATED location = LOCATION_PURPLE_ACTIVATED.UNDEFINED;
                              if (!Enum.TryParse<LOCATION_PURPLE_ACTIVATED>(s, out location))
                                  return LOCATION_PURPLE_ACTIVATED.UNDEFINED;

                              return location;
                          }));

                TableInfo.ExternalLoadInfo(DAL, out sError);

                if (sError.Length > 0)
                    throw DAL.InnerException ?? new EudoException(sError);

                // Onglets

                RqParam rqParam = new RqParam();
                /*  Droits de traitements sur la table */
                rqParam = new RqParam();
                rqParam.SetProcedure("xsp_getTableDescFromDescIdV2");
                rqParam.AddInputParameter("@DescId", SqlDbType.Int, DescId);
                rqParam.AddInputParameter("@UserId", SqlDbType.Int, Pref.UserId);
                rqParam.AddInputParameter("@GroupId", SqlDbType.Int, Pref.User.UserGroupId);
                rqParam.AddInputParameter("@UserLevel", SqlDbType.Int, Pref.User.UserLevel);
                rqParam.AddInputParameter("@Lang", SqlDbType.VarChar, Pref.Lang);

                DTRTabTreatmentRights = DAL.Execute(rqParam, out sError);

                //Gestion d'erreur
                if (!string.IsNullOrEmpty(sError) || !DTRTabTreatmentRights.HasRows || !DTRTabTreatmentRights.Read())
                    throw new Exception(String.Concat("Table (", DescId, ")  ou User (", Pref.UserId, ") Inexistant"));

                try
                {
                    GlobalRightsFileInfo = eLibDataTools.GetTreatmentGlobalRight(DAL, Pref.User);
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Chargement des droits globaux de la table ({0}) impossible.", DescId), e);
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Renvoie les éléments du menu sous la forme d'un objet modèle de données
        /// </summary>
        /// <returns></returns>
        public FileMenuModel GetFileMenuModel()
        {
            FileMenuModel fmm = new FileMenuModel();

            try
            {
                #region Récupération des informations de contexte
                GetContextInfo();

                bool isEudonetXThemeEnabled = Pref.ThemeXRM.Version > 1;
                bool isTemplate = TableInfo.TabType == TableType.TEMPLATE;
                #endregion

                #region Création des menus

                #region Menu AJOUTER

                List<FileMenuModel.FileMenuItemAction> addMenuActions = new List<FileMenuModel.FileMenuItemAction>();

                #region Action AJOUTER / NOUVEAU
                /*************** Nouvelle fiche : Mode fiche  *************************/
                if (!isTemplate && DTRTabTreatmentRights.GetString("ADD_LIST_P") == "1")
                {
                    #region On détermine le câblage des actions JS selon les cas
                    string addJSAction = String.Empty;
                    string addWizardJSAction = String.Empty;

                    /* Nouvelle ergo filoguidée G.L */
                    if (
                        ParamInfo.ParamTabNelleErgoGuided.Any(eg => eg == DescId) &&
                        !new List<int> { TableType.PP.GetHashCode(), TableType.PM.GetHashCode() }.Any(elm => elm == DescId)
                    )
                    {
                        addJSAction = "goCreateFile()";
                        addWizardJSAction = "goCreatePurpleFile()";
                    }
                    // Si PM ou PP, on recherche avant de faire un ajout : on appelle da Finder
                    else if (eLibTools.GetTabFromDescId(DescId) == TableType.PP.GetHashCode() || eLibTools.GetTabFromDescId(DescId) == TableType.PM.GetHashCode())
                    {
                        addJSAction = "goCreateFileWithFinder()";
                        addWizardJSAction = "goCreatePurpleFileWithFinder()";
                    }
                    else if (
                        eLibTools.GetTabFromDescId(DescId) == TableType.CAMPAIGN.GetHashCode() ||
                        eLibTools.GetTabFromDescId(DescId) == TableType.PAYMENTTRANSACTION.GetHashCode()
                    )
                    {
                        // Pas de bouton Nouveau dans ces cas
                    }
                    else
                    {
                        //Sur les fichiers de type Principal (Event), pas de bouton Appliquer/Fermer, seulement Valider
                        string strApplyCloseOnly = "false";
                        if (TableInfo.EdnType == EdnType.FILE_MAIN)
                            strApplyCloseOnly = "true";

                        if (TableInfo.AutoCreate)
                            addJSAction = String.Concat("goAutoCreate(", DescId, ")");
                        else
                        {
                            if (TableInfo.TabType != TableType.USER)
                                addJSAction = String.Concat("goCreateMainFile(", strApplyCloseOnly, ")");
                            else
                                addJSAction = "goCreateUserFile()";
                        }
                    }
                    #endregion

                    #region Création des actions si elles sont câblées
                    // Nouveau / Ajouter
                    if (!String.IsNullOrEmpty(addJSAction))
                        addMenuActions.Add(new FileMenuModel.FileMenuItemAction(eResApp.GetRes(Pref, 31), "fa fa-plus-square", addJSAction, String.Empty));

                    // Assistant de création
                    if (!String.IsNullOrEmpty(addWizardJSAction) && isEudonetXThemeEnabled && LocationsPurpleActivated.Contains(LOCATION_PURPLE_ACTIVATED.MENU))
                        addMenuActions.Add(new FileMenuModel.FileMenuItemAction(eResApp.GetRes(Pref, 3005), "fas fa-hat-wizard", addWizardJSAction, String.Empty));
                    #endregion
                }
                #endregion

                #region Action DUPLIQUER
                if (!isTemplate && DTRTabTreatmentRights.GetString("DUPLI_P") == "1" &&
                    TableInfo.TabType != TableType.CAMPAIGN && TableInfo.TabType != TableType.USER && TableInfo.TabType != TableType.PAYMENTTRANSACTION
                )
                    addMenuActions.Add(
                        new FileMenuModel.FileMenuItemAction(
                            eResApp.GetRes(Pref, 534),
                            "fas fa-copy",
                            "goDuplicateFile()",
                            String.Empty
                        )
                    ); // "copy"
                #endregion

                #region Création du menu s'il comporte des actions
                if (addMenuActions.Count > 0)
                    fmm.Items.Add(new FileMenuModel.FileMenuItem(eResApp.GetRes(Pref, 18), addMenuActions)); // Ajouter
                #endregion

                #endregion

                #region Menu AFFICHAGES
                List<FileMenuModel.FileMenuItemAction> displayMenuActions = new List<FileMenuModel.FileMenuItemAction>();

                #region Action VOIR LA LISTE
                displayMenuActions.Add(
                    new FileMenuModel.FileMenuItemAction(
                        eResApp.GetRes(Pref, 23), // Mode Liste - TODORES "Voir la liste"
                        "fas fa-list",
                        "goList()",
                        String.Empty
                    )
                );
                #endregion

                #region Action EPINGLER
                displayMenuActions.Add(
                    new FileMenuModel.FileMenuItemAction(
                        eResApp.GetRes(Pref, 3068), // Epingler
                        "fas fa-thumbtack",
                        "goPinFile()",
                        eResApp.GetRes(Pref, 8902) // Epingler dans un nouvel onglet du navigateur / Pin in a new browser tab
                    )
                );
                #endregion

                #region Création du menu s'il comporte des actions
                if (displayMenuActions.Count > 0)
                    fmm.Items.Add(new FileMenuModel.FileMenuItem(eResApp.GetRes(Pref, 2067), displayMenuActions)); // Affichages
                #endregion

                #endregion

                #region Menu ACTIONS
                List<FileMenuModel.FileMenuItemAction> actionMenuActions = new List<FileMenuModel.FileMenuItemAction>();


                #region Action PROPRIETES
                actionMenuActions.Add(new FileMenuModel.FileMenuItemAction(
                    eResApp.GetRes(Pref, 54), // Propriétés de la fiche
                    "fas fa-user",
                    "goProperties()",
                    String.Empty
                ));
                #endregion

                #region Action MODIFIER AVEC L'ASSISTANT
                if (LocationsPurpleActivated.Contains(LOCATION_PURPLE_ACTIVATED.MENU) && DTRTabTreatmentRights.GetString("MODIF_P") == "1" && TableInfo.TabType != TableType.PAYMENTTRANSACTION)
                    actionMenuActions.Add(new FileMenuModel.FileMenuItemAction(
                        eResApp.GetRes(Pref, 3069), // Modifier avec l'assistant
                        "fas fa-hat-wizard",
                        "goEditPurpleFile()",
                        String.Empty
                    ));
                #endregion

                #region Action SUPPRIMER
                if (DTRTabTreatmentRights.GetString("DEL_P") == "1" && TableInfo.TabType != TableType.PAYMENTTRANSACTION)
                    actionMenuActions.Add(new FileMenuModel.FileMenuItemAction(
                        eResApp.GetRes(Pref, 19), // Supprimer
                        "fas fa-trash-alt",
                        "goDeleteFile()",
                        String.Empty
                    ));
                #endregion

                #region Action IMPRIMER
                if (!isTemplate && GlobalRightsFileInfo[eLibConst.TREATID.PRINT] && TableInfo.DescId != (int)TableType.USER)
                    actionMenuActions.Add(new FileMenuModel.FileMenuItemAction(
                        eResApp.GetRes(Pref, 13), // Imprimer
                        "fas fa-print",
                        "goPrintFile()",
                        String.Empty
                    ));
                #endregion

                #region Action PARTAGER
                actionMenuActions.Add(new FileMenuModel.FileMenuItemAction(
                    eResApp.GetRes(Pref, 8027), // Partager
                    "fas fa-share-alt",
                    "goShareFile()",
                    String.Empty
                ));
                #endregion

                #region Création du menu s'il comporte des actions
                if (actionMenuActions.Count > 0)
                    fmm.Items.Add(new FileMenuModel.FileMenuItem(eResApp.GetRes(Pref, 296), actionMenuActions));
                #endregion

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DTRTabTreatmentRights != null)
                    DTRTabTreatmentRights.Dispose();
                // Fermeture de la connexion
                DAL.CloseDatabase();
            }

            return fmm;
        }
        #endregion
    }
}