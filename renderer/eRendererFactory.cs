using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.import;
using Com.Eudonet.Xrm.list;
using Com.Eudonet.Xrm.renderer;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Classe d'appel des renderer
    /// </summary>
    public static class eRendererFactory
    {
        #region RENDERER LIST



        #region Bookmark

        /// <summary>
        /// Renderer pour la barre des signets
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        /// <param name="bFileTabInBkm">indique si une partie des rubriques de la fiche est déplacée en signet (facultatif si fileDetail est différent de null)</param>
        /// <returns></returns>
        public static eRenderer CreateBookmarkBarRenderer(ePref pref, int nTab, int nFileId, Boolean bFileTabInBkm = false)
        {
            eFile ef = eFileForBkm.CreateFileForBkmBar(pref, nTab, nFileId);
            return eRendererFactory.CreateBookmarkBarRenderer(pref, ef, bFileTabInBkm);
        }


        /// <summary>
        /// Création du bloc signet avec les entêtes
        /// Fait appel a CreateBookmarkRenderer(ePref ePref,  List eBookmark  bkmList, Boolean bDisplayAllRecord);
        /// </summary>
        /// <param name="pref">Préférence utilisateur en cours</param>
        /// <param name="ef">Objet eFile contenant les données à afficher dans les signets</param>
        /// <param name="bDisplayAll">Indique si tous les signets doivent être affichés</param>
        /// <param name="divFileDetailsInBkm">div contenant les rubriques à afficher en signet</param>
        /// <param name="bFileTabInBkm">indique si une partie des rubriques de la fiche est déplacée en signet (facultatif si fileDetail est différent de null)</param>
        /// <returns></returns>
        public static eRenderer CreateBookmarkWithHeaderRenderer(ePref pref, eFile ef, Boolean bDisplayAll, Panel divFileDetailsInBkm = null, Boolean bFileTabInBkm = false)
        {


            if (divFileDetailsInBkm != null && divFileDetailsInBkm.Controls.Count > 0)
                bFileTabInBkm = true;


            //container global
            eRenderer eRenderHeaderBkm = eRenderer.CreateRenderer();
            eRenderHeaderBkm.PgContainer.ID = "blockBkms";
            eRenderHeaderBkm.PgContainer.CssClass = "blockBkms";

            #region BLOCK BARRE DES SIGNETS


            Panel pnl = CreateBookmarkBarRenderer(pref, ef, bFileTabInBkm).PgContainer;
            eRenderHeaderBkm.PgContainer.Controls.Add(pnl);

            #endregion


            #region BLOCK BOOKMARK

            Panel divBkmPres = new Panel();
            divBkmPres.ID = "divBkmPres";
            divBkmPres.Attributes.Add("class", "divBkmPres");
            eRenderHeaderBkm.PgContainer.Controls.Add(divBkmPres);

            if (divFileDetailsInBkm != null && divFileDetailsInBkm.Controls.Count > 0)
            {
                divBkmPres.Controls.Add(divFileDetailsInBkm);
            }

            Panel pnBkmsListContainer;
            if (ef.FileId == 0 && ef.ViewMainTable.TabType == TableType.PP)
            {
                string sPMValue = string.Empty;
                ef.DicValues.TryGetValue(TableType.PM.GetHashCode(), out sPMValue);

                if (string.IsNullOrEmpty(sPMValue) && ef.FileContext != null && ef.FileContext.ParentFileId != null && ef.FileContext.ParentFileId.ParentPmId > 0)
                {
                    if (ef.DicValues.ContainsKey(TableType.PM.GetHashCode()))
                        ef.DicValues[TableType.PM.GetHashCode()] = ef.FileContext.ParentFileId.ParentPmId.ToString();
                    else
                        ef.DicValues.Add(TableType.PM.GetHashCode(), ef.FileContext.ParentFileId.ParentPmId.ToString());

                    sPMValue = ef.FileContext.ParentFileId.ParentPmId.ToString();


                }

                pnBkmsListContainer = eRendererFactory.CreateAddressCreationRenderer(pref, ef.DicValues, !string.IsNullOrEmpty(sPMValue)).PgContainer;
            }
            else
            {
                eRenderer rndBkms = eRendererFactory.CreateBookmarkListRenderer(pref, ef, bDisplayAll);

                //En erreur
                if (rndBkms.ErrorMsg.Length > 0)
                    return rndBkms;

                pnBkmsListContainer = rndBkms.PgContainer;
            }

            if (ef.ActiveBkm == EudoQuery.ActiveBkm.DISPLAYFIRST.GetHashCode() && bFileTabInBkm)
                pnBkmsListContainer.Style.Add("display", "none");

            divBkmPres.Controls.Add(pnBkmsListContainer);


            #endregion


            /*  Ajout des signets */


            return eRenderHeaderBkm;
        }

        /// <summary>
        /// Fais un rendu 0
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="id"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        internal static eRenderer CreateXrmHomePageRenderer(ePref pref, int id, int w, int h)
        {
            eRenderer ren = new eXrmHomePageRenderer(pref, id, w, h);
            ren.Generate();
            return ren;
        }


        /// <summary>
        /// Création du block de signets passé en paramètres
        /// Fait appel à (ePref ePref, eBookmark eBkm, Boolean bDisplayAllRecord)
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="ef"></param>
        /// <param name="bDisplayAll">Indique si tous les signets doivent être affichés</param>
        /// <param name="bForceDisplay">Indique si les signs</param>
        /// <param name="bIsForPrint">Indique si on est en mode impression</param>
        /// <returns></returns>
        public static eRenderer CreateBookmarkListRenderer(ePref ePref, eFile ef, Boolean bDisplayAll = false, Boolean bForceDisplay = false, Boolean bIsForPrint = false)
        {

            //Initialise l'objet eError au cas ou
            eError err = eError.getError();
            err.SetPref(ePref);

            List<eBookmark> bkmList = ef.LstBookmark;
            eRenderer eRenderListBkm = eRenderer.CreateRenderer();

            eRenderListBkm.PgContainer.ID = "divBkmCtner";
            eRenderListBkm.PgContainer.CssClass = "divBkmCtner";


            StringBuilder sBkmToInit = new StringBuilder();

            foreach (eBookmark bkm in bkmList)
            {

                // Pour de grille dans le signet tous
                if (bDisplayAll && bkm.BkmEdnType == EdnType.FILE_GRID)
                    continue;

                // Si en erreur, on feedback et on passe à la suite
                if (bkm.ErrorMsg.Length > 0)
                {
                    //KHA le 25/05/2015 les erreurs concernant l'absence de filtres doublons ne doivent pas être affichées comme des erreurs.
                    //Il s'agit d'un cas de configuration normale
                    if (bkm.ErrorType == QueryErrorType.ERROR_NUM_FILTER_DBL_NOT_FOUND)
                    {
                        continue;
                    }
                    Panel pnErr = new Panel();
                    pnErr.Controls.Add(new LiteralControl(eResApp.GetRes(ePref, 72)));
                    eRenderListBkm.PgContainer.Controls.Add(pnErr);


                    string sErrMsg = eResApp.GetRes(ePref, 6432)
                        .Replace("<BKMRES>",
                            (bkm.ViewMainTable != null) ? bkm.ViewMainTable.Libelle : ""
                            );
                    string sTitle = "";
                    string sDevMsg = "";
                    if (bkm.InnerException != null)
                    {
                        sTitle = bkm.InnerException.Message;
                        sDevMsg = string.Concat(bkm.InnerException.Source, Environment.NewLine, bkm.InnerException.StackTrace);
                    }
                    err.Container = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, sErrMsg, bkm.ErrorMsg, sTitle, sDevMsg);
                    err.LaunchFeedBack();

                    continue;
                }
                //Si non loadé, on ignore
                if (bkm.State != FILLERSTATE.END && !bkm.IsMemo)
                    continue;

                // Si option "masqué si vide" et vide, on ignore
                if (bkm.HideWhenEmpty && bkm.ListRecords.Count == 0 && !bForceDisplay)
                    continue;

                if (!bDisplayAll && ef.ActiveBkm != bkm.CalledTabDescId)
                    continue;



                eRenderer eBkmRend = eRendererFactory.CreateBookmarkRenderer(ePref, bkm, bDisplayAll, bIsForPrint);
                if (eBkmRend.ErrorMsg.Length == 0 && eBkmRend.InnerException == null)
                {
                    eRenderListBkm.PgContainer.Controls.Add(eBkmRend.PgContainer);

                    //Ajout du bkm dans la liste des bkm à initialiser
                    if (sBkmToInit.Length > 0)
                        sBkmToInit.Append(";");

                    sBkmToInit.Append(bkm.CalledTabDescId);
                    if (!bDisplayAll)
                    {
                        break;
                    }
                }
                else
                {
                    //  eRenderListBkm._sErrorMsg = string.Concat(eRenderListBkm.ErrorMsg, eBkmRend.ErrorMsg);
                    //  eRenderListBkm._nErrorNumber = 2;
                    eRenderListBkm.PgContainer.Controls.Add(eBkmRend.PnlError);


                    string sErrMsg = eResApp.GetRes(ePref, 6432).Replace("<BKMRES>", bkm.ViewMainTable != null ? bkm.ViewMainTable.Libelle : "");
                    string sTitle = "";
                    string sDevMsg = string.Concat("Signet : ", bkm.ViewTabDescId, Environment.NewLine, eBkmRend.ErrorMsg);
                    if (eBkmRend.InnerException != null)
                    {
                        sTitle = eBkmRend.InnerException.Message;
                        sDevMsg += string.Concat(eBkmRend.InnerException.Source, Environment.NewLine, eBkmRend.InnerException.StackTrace);
                    }
                    err.Container = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, sErrMsg, eBkmRend.ErrorMsg, sTitle, sDevMsg);
                    err.LaunchFeedBack();


                }

            }
            eRenderListBkm.PgContainer.Attributes.Add("BkmToInit", sBkmToInit.ToString());


            return eRenderListBkm;

        }




        /// <summary>
        /// Créer une instance du renderer des grilles
        /// </summary>
        /// <param name="pref">pref utilisateur en cours</param>
        /// <param name="nGridId">Id de la grille</param>
        /// <param name="nWidth">largeur</param>
        /// <param name="nHeight">hauteur</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// un renderer de la grille
        /// </returns>
        public static eRenderer CreateXrmGridRenderer(ePref pref, int nGridId, int nWidth, int nHeight, eXrmWidgetContext context = null)
        {
            eRenderer ren = new eXrmHomePageGridRenderer(pref, nGridId, nWidth, nHeight, context);
            ren.Generate();
            return ren;
        }


        /// <summary>
        /// Créer une instance du renderer de la page d'accueil
        /// </summary>
        /// <param name="pref">pref utilisateur en cours</param>
        /// <param name="nPage">The n page.</param>
        /// <param name="nRows">The n rows.</param>
        /// <param name="nWidth">largeur</param>
        /// <param name="nHeight">hauteur</param>
        /// <returns>
        /// un renderer de la grille
        /// </returns>
        public static eRenderer CreateXrmHomePageListRenderer(ePref pref, int nPage, int nRows, int nWidth, int nHeight)
        {
            eRenderer ren = eListMainXrmHomPageRenderer.CreateXrmHomPageListRenderer(pref, nPage, nRows, nWidth, nHeight);
            ren.Generate();
            return ren;
        }


        /// <summary>
        /// Render d'un signet unique à partir d'un descid
        /// Utilisé pour paging, filtre express, et modification des rubriques
        /// Ne passe pas par un efile
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nParentTab"></param>
        /// <param name="nParentFileId"></param>
        /// <param name="nBkmDescId"></param>
        /// <param name="nPage"></param>
        /// <param name="nRows"></param>
        /// <param name="bDisplayAllRecord">Indique si l'on doit afficher tous les enregistrements du signet indépendamment des paramètres de paging actuels (bouton Afficher tout)</param>
        /// <param name="nBkmFileId">Pour les modes fiches en signet : indique le fileid de la fiche à présenter</param>
        /// <returns></returns>
        public static eRenderer CreateBookmarkRenderer(ePref ePref, int nParentTab, int nParentFileId, int nBkmDescId, int nPage, int nRows, Boolean bDisplayAllRecord, int nBkmFileId = 0, int nBkmFilePos = 0)
        {
            if (nBkmFilePos > 0)
            {
                //Gestion nRows = 0
                nPage = (nBkmFilePos / (nRows <= 0 ? eLibConst.MAX_ROWS : nRows)) + 1;
            }
            eBookmark eBkm = (eBookmark)eBookmark.CreateBookmark(ePref, nParentTab, nParentFileId, nBkmDescId, nPage, nRows, bDisplayAllRecord, eBookmark.LoadMode.LOAD);
            eBkm.BkmFileId = nBkmFileId;
            eBkm.BkmFilePos = nBkmFilePos;
            eRenderer BkmRenderer = eRendererFactory.CreateBookmarkRenderer(ePref, eBkm, false, false);

            return BkmRenderer;
        }


        /// <summary>
        /// Génération d'un signet de type discussion
        /// </summary>
        /// <param name="ePref">préférences user</param>
        /// <param name="bkm">signet</param>
        /// <returns>Renderer générique</returns>
        public static eRenderer CreateBkmDiscCommRenderer(ePref ePref, eBkmDiscComm bkm)
        {
            eBkmDiscCommRenderer bkmRdr = new eBkmDiscCommRenderer(ePref, bkm);
            bkmRdr.Generate();
            return bkmRdr;

        }

        /// <summary>
        /// Création du rendu graphique d'un signet unique
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="eBkm">l'objet eBkm est construit au préalable du renderer</param>
        /// <param name="bDisplayAllBkm"></param>
        /// <param name="bIsForPrint"></param>
        /// <returns></returns>
        private static eRenderer CreateBookmarkRenderer(ePref ePref, eBookmark eBkm, Boolean bDisplayAllBkm, Boolean bIsForPrint)
        {


            eRenderer BkmRenderer = null;


            //Si le bookmark est en erreur
            if (eBkm.ErrorMsg.Length > 0)
            {

                BkmRenderer = eBookmarkRenderer.CreateErrorBookmarkRenderer(ePref, eBkm);

                if (eBkm.ErrorType == QueryErrorType.ERROR_NUM_BKM_NOT_LINKED)
                {
                    //Bookmark non disponible (droits ou liaison)

                    BkmRenderer.PgContainer.ID = string.Concat("bkm_", eBkm.CalledTabDescId);
                    BkmRenderer.PgContainer.Attributes.Add("class", "bkmdiv");
                    return BkmRenderer;
                }
                else
                {
                    BkmRenderer.PgContainer.ID = string.Concat("bkm_", eBkm.CalledTabDescId);
                    BkmRenderer.PgContainer.Attributes.Add("class", "bkmdiv");
                    return BkmRenderer;
                }
            }



            if (eBkm.CalledTabDescId % 100 == AllField.MEMO_NOTES.GetHashCode() || eBkm.CalledTabDescId % 100 == AllField.MEMO_DESCRIPTION.GetHashCode())
            {
                BkmRenderer = new eNotesBookmarkRenderer(ePref, eBkm, bIsForPrint);
            }
            else if (eBkm.ViewMainTable.EdnType == EdnType.FILE_BKMWEB)
            {
                eWebBookmark eWebBkm = new eWebBookmark(ePref, eBkm);
                BkmRenderer = new eWebBookmarkRenderer(ePref, eWebBkm);
                if (!bDisplayAllBkm)
                    BkmRenderer.PgContainer.Style.Add(HtmlTextWriterStyle.Height, "100%");
            }
            else if (eBkm.BkmEdnType == EdnType.FILE_GRID)
            {
                BkmRenderer = new eBookmarkGridSubMenu(ePref, eBkm);
            }
            else if (eBkm.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.VIEWMODE) == ePrefConst.BKMVIEWMODE.FILE.GetHashCode().ToString() && eBkm.BkmEdnType != EdnType.FILE_RELATION && !bIsForPrint)
            {
                BkmRenderer = new eBookmarkFileRenderer(ePref, eBkm);
            }
            else if (eBkm.ViewMainTable.EdnType == EdnType.FILE_DISCUSSION)
            {
                BkmRenderer = new eBkmDiscussionRenderer(ePref, eBkm, bDisplayAllBkm, bIsForPrint);
                //if (!bDisplayAllBkm)
                //    BkmRenderer.PgContainer.Style.Add(HtmlTextWriterStyle.Height, "100%");
            }

            else
            {
                BkmRenderer = new eBookmarkRenderer(ePref, eBkm, bDisplayAllBkm, bIsForPrint);
            }

            if (BkmRenderer.ErrorMsg.Length > 0 || !BkmRenderer.Generate())
                return BkmRenderer;

            BkmRenderer.PgContainer.ID = string.Concat("bkm_", eBkm.CalledTabDescId);
            BkmRenderer.PgContainer.Attributes.Add("class", "bkmdiv");

            //BSE #50 364 , Récuperer le nombre de page à afficher pour le signet en mode TOUS
            BkmRenderer.PgContainer.Attributes.Add(string.Concat("bkm_Rows_", eBkm.CalledTabDescId), eBkm.RowsByPage.ToString());
            //Drag and Drop sur les signets annexes
            if (eBkm.CalledTabDescId % 100 == AllField.ATTACHMENT.GetHashCode())
            {
                if (eBkm.IsAddAllowed)
                {
                    BkmRenderer.PgContainer.Attributes.Add("ondragover", "UpFilDragOver(this, event);return false;");
                    BkmRenderer.PgContainer.Attributes.Add("ondragleave", "UpFilDragLeave(this); return false;");
                    BkmRenderer.PgContainer.Attributes.Add("ondrop", "UpFilDrop(this,event);return false;");
                }

            }

            if (eBkm.CalledTabDescId == (int)TableType.BOUNCEMAIL && eBkm.ParentTab == (int)TableType.CAMPAIGN)
            {

                BkmRenderer.PgContainer.Attributes.Add("ondragover", "UpFilDragOver(this, event);return false;");
                BkmRenderer.PgContainer.Attributes.Add("ondragleave", "UpFilDragLeave(this); return false;");
                BkmRenderer.PgContainer.Attributes.Add("ondrop", "UpFilDrop(this,event,null,null,2);return false;");


            }

            return BkmRenderer;
        }


        /// <summary>
        /// Création du block de signets de eFile sauf un
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        /// <param name="nBkm"></param>
        /// <returns></returns>
        public static eRenderer CreateBookmarkListAllButOneRenderer(ePref ePref, int nTab, int nFileId, int nBkm)
        {
            eFile ef = eFileForBkm.CreateFileForAllBkmButOne(ePref, nTab, nFileId, nBkm);

            eRenderer rdr = eRendererFactory.CreateBookmarkListRenderer(ePref, ef, true);

            rdr.PgContainer.ID = "tmpctner";

            return rdr;
        }


        #endregion



        /// <summary>
        /// Création d'une liste de type Homepage MRU
        /// </summary>
        public static eRenderer CreateHomePageRenderer(eHomePageListRenderer hpgRend)
        {
            hpgRend.Generate();
            return hpgRend;
        }

        /// <summary>
        /// Retourne une liste de filtre
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nPage"></param>
        /// <param name="typeFilter"></param>
        /// <param name="deselectAllowed">Possibilité de désélectionner un filtre</param>
        /// <param name="adminMode">Mode administration d'un filtre et non l'utilisation directe d'un filtre</param>
        /// <param name="selectFilterMode">Mode selection d'un filtre sans l'appliquer</param>
        /// <returns></returns>
        public static eRenderer CreateFilterListRenderer(ePref ePref, int nTab, IRightTreatment oRightManager, EudoQuery.TypeFilter typeFilter, int nPage = 1,
            bool deselectAllowed = false, bool adminMode = false, bool selectFilterMode = false)
        {

            eFilterListRenderer filterListRenderer = eFilterListRenderer.GetFilterListRenderer(ePref, 800, 600, nTab, oRightManager, nPage,
                deselectAllowed: deselectAllowed, adminMode: adminMode, selectFilterMode: selectFilterMode);
            filterListRenderer.FilterType = (EudoQuery.TypeFilter)typeFilter;
            filterListRenderer.Generate();
            return filterListRenderer;
        }


        /// <summary>
        /// Retourne une liste de formulaires
        /// </summary>
        /// <param name="ePref">Préférence Utilisateur</param>
        /// <param name="nTab">Table pour laquelle on veut les formulaires</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="width">Largeur de la fenêtre</param>
        /// <param name="height">Hauteur de la fenêtre</param>
        /// <param name="oRightManager">Gestionnaire de droits</param>
        /// <param name="bHideActionBtn">Masquer les actions qui ne doivent pas être accessible dans certains modes</param>
        /// <param name="filter">Filtre sur la liste</param>
        /// <returns></returns>
        public static eRenderer CreateFormularListRenderer(ePref ePref, int width, int height, int nTab, IRightTreatment oRightManager, eFormularListFilterParams filter, int nPage = 1, Boolean bHideActionBtn = false)
        {
            eFormularListRenderer formularListRenderer = eFormularListRenderer.GetFormularListRenderer(ePref, height, width, nTab, oRightManager, filter, nPage, bHideActionBtn: bHideActionBtn);

            formularListRenderer.Generate();
            return formularListRenderer;


        }

        /// <summary>
        /// Retourne une liste de filtre pour les wizard++/xx
        /// </summary>
        /// <param name="ePref">Préférence utilisateur</param>
        /// <param name="nTab">Tab des filtres</param>
        /// <param name="bDelete">Mode suppression</param>
        /// <returns></returns>
        public static eRenderer CreateInvitFilterListRenderer(ePref ePref, int width, int height, int nTab, Boolean bDelete)
        {
            eInvitFilterListRenderer filterListRenderer = eInvitFilterListRenderer.GetInvitFilterListRenderer(ePref, height, width, nTab, bDelete);

            filterListRenderer.Generate();

            return filterListRenderer;
        }

        /// <summary>
        /// Retourne une liste de rapports
        /// </summary>
        /// <param name="ePref">Préférence de l'utilisateur</param>
        /// <param name="nTab">Table Utilisateur ciblée</param>
        /// <param name="reportType">Type de rapport</param>
        /// <returns>Renderer pour la liste des rapports</returns>
        public static eRenderer CreateReportListRenderer(ePref ePref, int width, int height, int nTab, EudoQuery.TypeReport reportType, IRightTreatment oRightManager, int nFileId = 0, bool deselectAllowed = false)
        {
            eReportListRenderer reportListRenderer = eReportListRenderer.GetReportListRenderer(ePref, width, height, nTab, oRightManager, deselectAllowed: deselectAllowed);
            reportListRenderer.ReportType = (EudoQuery.TypeReport)reportType;
            reportListRenderer.FileId = nFileId;
            reportListRenderer.Generate();

            return reportListRenderer;
        }



        /// <summary>
        /// Retourne une liste des modèles d'import
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="ePref">Préférence de l'utilisateur</param>
        /// <param name="nTab">Table Utilisateur ciblée</param>
        /// <param name="nPage">Numéro de page</param>
        /// <returns></returns>
        public static eRenderer CreateImportTemplateListRenderer(ePref ePref, int width, int height, int nTab, int nPage = 1)
        {
            eImportTemplateListRenderer reportListRenderer = eImportTemplateListRenderer.GetImportTemplateListRenderer(ePref, width, height, nTab, nPage);
            reportListRenderer.Generate();

            return reportListRenderer;
        }


        /// <summary>
        /// Retourne une liste des modeles d'emailing
        /// </summary>
        /// <param name="ePref">Préférence de l'utilisateur</param>
        /// <param name="nTabFilter">Table Utilisateur ciblée</param>
        /// <returns>Renderer pour la liste des rapports</returns> 
        public static eRenderer CreateMailTemplateListRenderer(ePref ePref, int width, int height, int nTabFilter, TypeMailTemplate nType, IRightTreatment oRightManager, int nPage = 1)
        {

            eMailTemplateListRendrer MailTemplateListRenderer = eMailTemplateListRendrer.GetMailTemplateListRenderer(ePref, width, height, nTabFilter, nType, oRightManager, nPage);


            MailTemplateListRenderer.Generate();

            return MailTemplateListRenderer;
        }

        /// <summary>
        /// Création d'une liste de type Champ de liaison
        /// </summary>
        public static eRenderer CreateFinderRenderer(eFinderListRenderer finderRend)
        {
            finderRend.Generate();

            return finderRend;
        }

        /// <summary>
        /// Finder multiple : Renderer affichant les fiches sélectionnées
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="sListCol"></param>
        /// <param name="liSelIds"></param>
        /// <returns></returns>
        public static eRenderer CreateFinderSelectionRenderer(ePref pref, int nTab, string sListCol, List<int> liSelIds)
        {
            eFinderSelectionRenderer rdr = eFinderSelectionRenderer.GetFinderSelectionRenderer(pref, nTab, sListCol, liSelIds);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Créé le renderer pour l'assistant Reporting
        /// </summary>
        /// <param name="ePref">Préférence de l'utilisateur</param>
        /// <param name="nTab">Table Utilisateur ciblée</param>
        /// <param name="reportType">Type de reporting</param>
        /// <param name="report">Modèle de reporting</param>
        /// <returns>Renderer pour l'assistant Reporting</returns>
        public static eRenderer CreateReportWizardRenderer(ePref ePref, int width, int height, int nTab, TypeReport reportType, eReport report = null)
        {
            if (reportType == TypeReport.CHARTS)
                return CreateChartWizardRenderer(ePref, nTab, width, height, reportType, report);

            // #33 098, #32 972, #33 601, #33 620 - Redimensionnement des listes de rubriques - La taille de la fenêtre passée au renderer doit être la même que celle demandée par le JavaScript dans eFilterReportListDialog.AddReport/editReport
            eReportWizardRenderer wizardRenderer = eReportWizardRenderer.GetReportWizardRenderer(ePref, width, height, reportType, nTab);
            if (report != null)
                wizardRenderer.Report = report;


            if (!wizardRenderer.Generate())
                return wizardRenderer;

            wizardRenderer.PgContainer.ID = "wizard";

            return wizardRenderer;

        }

        /// <summary>
        /// Créé le renderer pour l'assistant Mailing
        /// </summary>
        /// <param name="ePref">Préférence de l'utilisateur</param>
        /// <param name="nTab">Table Utilisateur ciblée</param>
        /// <param name="nHeight">Hauteur du renderer</param>
        /// <param name="nWidth">Largeur du renderer</param>
        /// <param name="mailingType">Type de mailing</param>
        /// <param name="mailing">Modèle de mailing</param>
        /// <returns>Renderer pour l'assistant Mailing</returns>
        /// <param name="_iWizardTotalStepNumber">Nombre d'étape du wizard</param>
        public static eRenderer CreateMailingWizardRenderer(ePref ePref, int nTab, int nHeight, int nWidth, TypeMailing mailingType, out int _iWizardTotalStepNumber, eMailing mailing = null)
        {
            eMailingWizardRenderer wizardRenderer;


            if (mailingType == TypeMailing.SMS_MAILING_FROM_BKM)
            {
                wizardRenderer = eSmsMailingFileWizardRenderer.GetSmsMailingFileWizardRenderer(ePref, nHeight, nWidth, mailing, mailingType, nTab, out _iWizardTotalStepNumber);
            }
            else
            {
                wizardRenderer = eMailingFileWizardRenderer.GetMailingFileWizardRenderer(ePref, nHeight, nWidth, mailing, mailingType, nTab, out _iWizardTotalStepNumber);

                // Backlog : 585
                //Si on est dans sur une offre xrm ou un navigateur IE/Edge, alors le wizard à une étape de moins
                //Tel quel, les wizard ne peuvent pas gèrer correctement ce type de cas.
                // Le nombre d'étape est défini par le type de wizard et il n'y a pas de notion d'étape optionnel
                // il faudrait revoir plus en profondeur le fonctionnement des wizards, ce qui n'est pas possible au vu des délais
                // Du coup, on retourne une étape vide (null) et on décrémente après coup le nombre d'étape total
                if (
                            ePref.ClientInfos.ClientOffer == 0
                            || eTools.IsMSBrowser
                            || !eFeaturesManager.IsFeatureAvailable(ePref, eConst.XrmFeature.HTMLTemplateEditor)
                        )
                    _iWizardTotalStepNumber--;
            }

            if (!wizardRenderer.Generate())
                return wizardRenderer;

            wizardRenderer.PgContainer.ID = "wizard";

            return wizardRenderer;

        }

        /// <summary>
        /// Création du wizard renderer pour les invitations (++)
        /// </summary>
        /// <param name="pref">préf utilisateur</param>
        /// <param name="nBkmTab">Table des inviations/cible etendu</param>
        /// <param name="nParentFileId">Id de la fiche parente</param>
        ///<param name="nFormularId">Id du formulaire</param>
        ///<param name="nHeight">taille en px de la fenetre de wizard</param>
        ///<param name="nHeightParent">Taille en px du parent</param>      
        /// <returns></returns>
        public static eRenderer CreateFormularXrmWizardRenderer(ePref pref, int nBkmTab, int nParentFileId, int nFormularId, int nHeight, int nHeightParent)
        {
            eFormularXrmWizardRenderer wizardRenderer;

            wizardRenderer = eFormularXrmWizardRenderer.GetFormularXrmWizardRenderer(pref, nBkmTab, nParentFileId, nFormularId, nHeight, nHeightParent);

            if (!wizardRenderer.Generate())
                return wizardRenderer;

            return wizardRenderer;

        }


        /// <summary>
        /// Création du wizard renderer pour les invitations (++)
        /// </summary>
        /// <param name="ePref">préf utilisateur</param>
        /// <param name="nTab">Table des inviations</param>
        /// <param name="nTabFrom">Table de l'évènement</param>
        /// <param name="nParentFileId">Id de la fiche parente</param>
        /// <param name="bDelete">Mode suppressions</param>
        /// <returns></returns>
        public static eRenderer CreateSelectInvitWizardRenderer(ePref ePref, int nTab, int nTabFrom, int nParentFileId, int width, int height, Boolean bDelete)
        {



            ePref.Context.InvitSelectId = new ExtendedDictionary<int, int>();
            eRenderer er = eInvitWizardRenderer.GetInvitWizardRenderer(ePref, nTab, nTabFrom, nParentFileId, width, height, bDelete);
            er.Generate();



            return er;
        }


        /// <summary>
        /// Création du wizard renderer pour les traitement de duplication (++)
        /// </summary>
        /// <param name="ePref">préf utilisateur</param>
        /// <param name="nTab">Table des inviations</param>
        /// <param name="nTabFrom">Table de l'évènement</param>
        /// <param name="nParentFileId">Id de la fiche parente</param>
        /// <param name="bDelete">Mode suppressions</param>
        /// <returns></returns>
        public static eRenderer CreateDuplicationTreatmentWizardRenderer(ePref ePref, int nTab, int width, int height)
        {



            eRenderer er = eDuplicationTreatmentRenderer.GetDuplicationTreatmentWizardRenderer(ePref, nTab, width, height);
            er.Generate();


            return er;
        }


        /// <summary>
        /// DEPRECATED
        /// Retourne la liste des filtre du renderer wizard avec uniquement la liste et le paging de l'étape 1
        /// utilisé uniquement via getPagging.ashx - le manager gérant le pagging pour la liste des filtres pour les invitations (++/xx)
        /// ce pagging a été désactivé à la demande de rma/direction
        /// </summary>
        /// <param name="ePref">préf utilisateur</param>
        /// <param name="nTab">Table des inviations</param>
        /// <param name="nPage">Page des filtre à afficher</param>
        /// <param name="bDelete">Mode suppression</param>
        /// <returns></returns>
        [Obsolete("Le pagging pour ce type de liste a été désactivé à la demande de rma/direction")]
        public static eRenderer CreateSelectInvitFilter(ePref ePref, int nTab, int nPage, int width, int height, Boolean bDelete)
        {
            return eInvitWizardRenderer.GetSelectInvitFilterRenderer(ePref, nTab, nPage, width, height, bDelete);
        }


        /// <summary>
        /// Créé le renderer pour l'assistant Graphiques
        /// </summary>
        /// <param name="ePref">Préférence de l'utilisateur</param>
        /// <param name="nTab">Table Utilisateur ciblée</param>
        /// <param name="reportType">Type de reporting</param>
        /// <param name="report">Modèle de reporting</param>
        /// <returns>Renderer pour l'assistant Reporting</returns>
        public static eRenderer CreateChartWizardRenderer(ePref ePref, int nTab, int width, int height, TypeReport reportType, eReport report = null)
        {
            eChartWizardRenderer wizardRenderer = eChartWizardRenderer.GetChartWizardRenderer(ePref, width, height, reportType, nTab);
            if (report != null)
                wizardRenderer.Report = report;

            if (!wizardRenderer.Generate())
                return wizardRenderer;


            wizardRenderer.PgContainer.ID = "wizard";

            return wizardRenderer;

        }

        /// <summary>
        /// Création d'une liste de Pj
        /// </summary>
        /// <param name="ePref">Préférence utilisateur</param>
        /// <param name="nFileId"></param>
        /// <param name="nTab">Descid de la table à obtenir</param>
        /// <param name="sIdsPj"></param>
        /// <returns></returns>
        public static eRenderer CreatePjListRenderer(ePref ePref, ePJToAdd attachment, string sIdsPj, int nHeight, int nWidth)
        {
            ePjListRenderer ePjListR = ePjListRenderer.GetPjListRenderer(ePref, attachment, nHeight, nWidth);
            ePjListR.InitialPjIds = sIdsPj;

            ePjListR.Generate();
            return ePjListR;
        }

        /// <summary>
        /// Création d'une liste de Pj depuis un template
        /// </summary>
        /// <param name="ePref">Préférence utilisateur</param>
        /// <param name="nFileId"></param>
        /// <param name="nTab">Descid de la table à obtenir</param>
        /// <param name="sIdsPj"></param>
        /// <returns></returns>
        public static eRenderer CreatePjListFromTplRenderer(ePref ePref, ePJToAdd attachment, string sIdsPj, string sViewType, int nHeight, int nWidth)
        {
            ePjListFromTplRenderer ePjListR = ePjListFromTplRenderer.GetPjListFromTplRenderer(ePref, attachment, nHeight, nWidth);
            ePjListR.InitialPjIds = sIdsPj;

            if ((attachment.ParentTab.EdnType == EdnType.FILE_MAIL || attachment.ParentTab.EdnType == EdnType.FILE_SMS) && attachment.FileId > 0 && attachment.IsMailFixed)
            {
                sViewType = "checkedonly";
            }


            ePjListR.ViewType = sViewType;

            ePjListR.Generate();
            return ePjListR;
        }

        /// <summary>
        /// Renderer pour la barre des signets
        /// </summary>
        /// <param name="pref">Préférence utilisateur en cours</param>
        /// <param name="ef">Objet eFile contenant les données à afficher dans les signets</param>
        /// <param name="bFileTabInBkm">indique si une partie des rubriques de la fiche est déplacée en signet (facultatif si fileDetail est différent de null)</param>
        /// <returns></returns>
        public static eRenderer CreateBookmarkBarRenderer(ePref pref, eFile ef, Boolean bFileTabInBkm = false)
        {
            /* CRU : Déplacement du bloc dans une classe Renderer séparée */
            eBookmarkBarRenderer rdr = eBookmarkBarRenderer.CreateBookmarkBarRenderer(pref, ef, bFileTabInBkm);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Retourne un renderer d'erreur
        /// </summary>
        /// <param name="sErrMSG">Message d'erreur pour l'utilisateur</param>
        /// <param name="e">erreur interne</param>
        /// <returns></returns>
        public static eRenderer CreateErrorRenderer(string sErrMSG, Exception e)
        {

            eRenderer ec = eRenderer.CreateRenderer(RENDERERTYPE.UNDEFINED);
            ec.SetError(QueryErrorType.ERROR_NONE, sErrMSG, e);


            return ec;

        }


        /// <summary>
        /// retourne une liste "principale"
        /// </summary>
        /// <param name="ePref">Préférence utilisateir</param>
        /// <param name="nTab">Descid de la table à obtenur</param>
        /// <param name="page">Page de la liste</param>
        /// <param name="row">Nombre de ligne par page</param>
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <returns></returns>
        public static eRenderer CreateMainListRenderer(ePref ePref, int nTab, int page, int row, int height, int width, Boolean bFullList = true)
        {
            eRenderer ec = eListMainRenderer.GetMainListRenderer(ePref, nTab, height, width, page, row, bFullList);
            //Génération du rendu
            ec.Generate();

            return ec;
        }


        /// <summary>
        /// Génération d'un renderer d'une liste de widget disponible pour une table
        /// </summary>
        /// <param name="ePref">pref user</param>
        /// <param name="nTab">tab</param>
        /// <param name="fileId">id de fiche pour mode fiche</param>
        /// <param name="page">numéro de page</param>
        /// <param name="row">ligne par page</param>
        /// <param name="height">hauteur de la fenptre</param>
        /// <param name="width">largeur de fenêtre</param>
        /// <param name="bFullList">Affichage liste entière (pas de pagination)</param>
        /// <returns></returns>
        public static eRenderer CreateWidgetMainListRenderer(ePref ePref, int nTab, int fileId, int page, int row, int height, int width, Boolean bFullList = true)
        {
            // Instanciation
            eListXrmWidgetRenderer elRenderer = eListXrmWidgetRenderer.GetListXrmWidgetRenderer(ePref, nTab, fileId, page, row, height, width, bFullList);

            //Génération du rendu
            elRenderer.Generate();

            return elRenderer;
        }

        /// <summary>
        /// retourne une liste "principale"
        /// </summary>
        /// <param name="ePref">Préférence utilisateir</param>
        /// <param name="nTab">Descid de la table à obtenur</param>
        /// <param name="page">Page de la liste</param>
        /// <param name="row">Nombre de ligne par page</param>
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <returns></returns>
        public static eRenderer CreateFullMainListRenderer(ePref ePref, int nTab, int page, int row, int height, int width)
        {
            eRenderer ec = null;


            TableLite tab = new TableLite(nTab);
            eudoDAL dal = eLibTools.GetEudoDAL(ePref);
            try
            {
                dal.OpenDatabase();
                String err = String.Empty;
                tab.ExternalLoadInfo(dal, out err);
                if (err.Length > 0)
                    throw dal.InnerException ?? new Exception(err);
            }
            finally
            {
                dal?.CloseDatabase();
            }


            switch (tab.EdnType)
            {
                case EdnType.FILE_USER:
                    ec = eFullMainListUserRenderer.GetFullMainListUserRenderer(ePref, height, width, page, row, tab);
                    break;
                case EdnType.FILE_HOMEPAGE:
                    ec = eFullMainListXrmHomePageRenderer.GetFullXrmHomePageRenderer(ePref, height, width, page, row, tab);
                    break;
                case EdnType.FILE_PLANNING:
                    ec = eFullPlanningRenderer.GetFullPlanningRenderer(ePref, height, width, page, row, tab);
                    break;
                default:
                    if (tab.TabType == TableType.RGPDTREATMENTSLOGS)
                        ec = eFullMainRGPDTreatmentLogRenderer.GetFullMainListRenderer(ePref, height, width, page, row, tab);
                    else
                        ec = eFullMainListRenderer.GetFullMainListRenderer(ePref, height, width, page, row, tab);
                    break;
            }


            //Génération du rendu
            ec.Generate();

            return ec;
        }

        /// <summary>
        /// retourne un renderer pour la génération d'une liste de sélection d'invitation (++)
        /// </summary>
        /// <param name="ePref">Préférence utilisateur</param>
        /// <param name="list">Liste à rendre</param>
        /// <param name="nBkm">Descid invitation</param>
        /// <param name="nFilterId">Filtre à utiliser</param>
        /// <param name="page">Numéro de page</param>
        /// <param name="row">Nombre de ligne par page - Il s'agit d'un calcul JS</param>
        /// <param name="height">Hauteur du wizard</param>
        /// <param name="width">Largeur du wizard</param>
        /// <returns></returns>
        public static eRenderer CreateSelectInvitRenderer(ePref ePref, eList list, int nBkm, int nFilterId, int page, int row, int height, int width)
        {

            //
            eRenderer ec = eListSelRenderer.GetSelListRenderer(ePref, list, height, width, page, row);

            //Génération du rendu
            ec.Generate();

            return ec;
        }

        /// <summary>
        /// Retourne un renderder de chart générant le flux XML nécessaire à Fusion Chart
        /// </summary>
        /// <param name="pref">préférence utilisateur</param>
        /// <param name="nChartId">Id du rapport</param>
        /// <param name="nFileId">id de la fiche pour les charts en mode fiche</param>
        /// <returns>Render prêt pour le rendu d'un graphique</returns>
        public static eRenderer CreateChartXML(ePref pref, int nChartId, int nFileId = 0, string sExpressFilterParam = "", eXrmWidgetContext context = null)
        {

            eChartRenderer ec = eChartRenderer.getChartRendererXML(pref, nChartId, nFileId, sExpressFilterParam, context);
            if (ec?.ErrorMsg.Length > 0)
                return ec;
            ec.Generate();

            if (ec.ErrorMsg.Length > 0)
                return ec;

            if (ec.Chart.BcombinedChartReport)
            {
                ec.CombinedChart = eCharts.BuildChart(pref, nChartId, nFileId, sExpressFilterParam, combined: true);
                ec.Generate();
            }


            return ec;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nTabFrom"></param>
        /// <param name="nIdFrom"></param>
        /// <param name="nDescId"></param>
        /// <param name="nFileId"></param>
        /// <returns>Render prêt pour le rendu d'un graphique</returns>
        public static eRenderer CreateStatisticalChartXML(ePref pref, int nTab, int nTabFrom, int nIdFrom, int nDescId, int nField, eCommunChart.TypeAgregatFonction agregat, int nFileId = 0)
        {

            eChartRenderer ec = eSyncFusionChartRenderer.GetStatisticalChartRendererXML(pref, nTab, nTabFrom, nIdFrom, nDescId, nField, agregat, nFileId);
            if (ec.ErrorMsg.Length > 0)
                return ec;

            ec.Generate();

            return ec;

        }


        /// <summary>
        /// Retourne un renderer avec le div conteneur du chart
        /// et les fonction JS nécessaire
        /// </summary>
        /// <param name="pref">Préférence</param>
        /// <param name="nReportId">Id du rapport</param>
        /// <param name="bFullSize">Indique si le chart prend toutes la place disponible dans son conteneur, sans prendre en compte les paramètres (par exemple, homepage)</param>
        /// <returns></returns>
        public static eRenderer CreateChartRenderer(ePref pref, int nReportId, Boolean bFullSize = false)
        {

            eChartRenderer chartRenderer = eChartRenderer.GetChartRenderer(pref, nReportId.ToString(), bFullSize: bFullSize);
            return chartRenderer;
        }


        /// <summary>
        /// Retourne un renderer avec le div conteneur du chart
        /// et les fonction JS nécessaire
        /// </summary>
        /// <param name="pref">Préférence</param>
        /// <param name="reportInfo">rapport</param>
        /// <param name="nReportId">Id du rapport</param>
        /// <param name="bFullSize">Indique si le chart prend toutes la place disponible dans son conteneur, sans prendre en compte les paramètres (par exemple, homepage)</param>
        /// <returns></returns>
        public static eRenderer CreateChartRenderer(ePref pref, eReport reportInfo, int nReportId, Boolean bFullSize = false)
        {

            eChartRenderer chartRenderer = eChartRenderer.GetChartRenderer(pref, reportInfo, nReportId.ToString(), bFullSize: bFullSize);
            return chartRenderer;
        }


        /// <summary>
        /// Retourne un renderer avec le div conteneur du chart
        /// et les fonction JS nécessaire
        /// </summary>
        /// <param name="pref">Préférence</param>
        /// <param name="nReportId">Id du rapport</param>
        /// <param name="bFullSize">Indique si le chart prend toutes la place disponible dans son conteneur, sans prendre en compte les paramètres (par exemple, homepage)</param>
        /// <returns></returns>
        public static eRenderer CreateFiltreEXpressChartRenderer(ePref pref, int nReportId, Boolean bFullSize = false)
        {

            eChartRenderer chartRenderer = eChartRenderer.GetExpressFilterChartRenderer(pref, nReportId, bFullSize);

            return chartRenderer;
        }


        /// <summary>
        /// Retourne un renderer avec le div conteneur des stats
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nTab">Descid de la table</param>
        /// <param name="nDescId">Descid de la colonne pour stats</param>
        /// <param name="nTabFrom">Descid de la table parente</param>
        /// <param name="nIdFrom">ID de la fiche parente</param>
        /// <returns></returns>
        public static eRenderer CreateStatRenderer(ePref pref, int nTab, int nDescId, int nTabFrom = 0, int nIdFrom = 0)
        {

            eSyncFusionStatRenderer sc = new eSyncFusionStatRenderer(pref, nTab, nDescId, nTabFrom, nIdFrom);
            return sc.getStatisticalChartRenderer();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eRenderer CreateUserReportListRenderer(ePref pref)
        {

            return eUserReportListRenderer.GetUserReportList(pref);
        }


        #endregion

        #region RENDERER  FILE

        /// <summary>
        /// Retourne un renderer de type File adapté au eFileType demandé
        /// </summary>
        /// <param name="efType">Type de fiche</param>
        /// <param name="tableLite">Objet TableLite</param>
        /// <param name="ePref">ePref</param>
        /// <param name="param">Dictionnaire de paramètres</param>
        /// <returns></returns>
        public static eRenderer CreateFileRenderer(eConst.eFileType efType, TableLite tableLite, ePref ePref, ExtendedDictionary<string, Object> param)
        {
            eRenderer efRend = null;

            int nFileId = 0;
            int nMailStatut = 0;

            param.TryGetValueConvert("fileid", out nFileId);
            param.TryGetValueConvert("maildraft", out nMailStatut);


            string date = string.Empty;
            string enddate = string.Empty;
            int concernedUser = 0;
            Boolean isPostback = false;
            int nPg = 1;
            Boolean bCroix = true;
            Boolean bPopupDisplay = false;
            bool isBkmFile = false;

            // Paramètres spécifiques template E-mail
            int nWidth = 0;
            int nHeight = 0;
            Boolean bMailForward = false;
            Boolean bEmailing = false;

            Boolean bMailDraft = (EmailStatus)nMailStatut == EmailStatus.MAIL_DRAFT;

            string strMailTo = string.Empty;

            param.TryGetValueConvert("popup", out bPopupDisplay);

            CalendarViewMode viewMode = CalendarViewMode.VIEW_UNDEFIED; // traduction = Mode d'affichage du calendrier non défié !
            string workHourBegin = string.Empty;

            #region Email et planning
            param.TryGetValueConvert("width", out nWidth);
            param.TryGetValueConvert("height", out nHeight);
            #endregion

            #region planning

            param.TryGetValueConvert("date", out date, string.Empty);
            param.TryGetValueConvert("enddate", out enddate, string.Empty);
            param.TryGetValueConvert("concernedUser", out concernedUser);
            param.TryGetValueConvert("ispostback", out isPostback);

            //Mode de calendrier : agenda : tache
            int nViewMode;
            param.TryGetValueConvert("planningviewmode", out nViewMode);
            Enum.TryParse(nViewMode.ToString(), out viewMode);

            param.TryGetValueConvert("workhourbegin", out workHourBegin);

            #endregion

            #region vcard

            param.TryGetValueConvert("npg", out nPg);
            param.TryGetValueConvert("bCroix", out bCroix);
            #endregion

            #region E-mail et E-mailing
            param.TryGetValueConvert("mailforward", out bMailForward);
            param.TryGetValueConvert("maildraft", out bMailDraft);
            param.TryGetValueConvert("emailing", out bEmailing);
            param.TryGetValueConvert("mailto", out strMailTo);
            #endregion

            switch (efType)
            {
                case eConst.eFileType.FILE_CONSULT:
                    // Pour l'affichage d'un template E-mail existant, on utilise le flag "Consultation", inutilisé pour tous les autres types de fichiers
                    // Ce n'est qu'une question de sémantique. Pour un template E-mail, on utilise de toute façon deux renderers dédiés (un pour l'affichage
                    // d'un mail existant, et un autre pour la création d'un mail) qui ne subiront pas les conséquences liées à l'abandon du mode Consultation
                    // TOCHECK SMS
                    if (tableLite.EdnType == EdnType.FILE_MAIL)
                        efRend = eRendererFactory.CreateMailFileRenderer(ePref, tableLite.DescId, nFileId, param, nWidth, nHeight, "", tableLite.EdnType == EdnType.FILE_SMS);
                    //SHA : correction bug #72 765
                    else if (tableLite.EdnType == EdnType.FILE_SMS)
                        efRend = eRendererFactory.CreateEditSmsRenderer(ePref, tableLite.DescId, nFileId, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, bEmailing, param);
                    else
                    {
                        if (tableLite.TabType == TableType.CAMPAIGN && param.ContainsKey("campaignreadonly"))
                        {
                            CAMPAIGN_READONLY_TYPE nReadOnlyType = (CAMPAIGN_READONLY_TYPE)eLibTools.GetNum(param["campaignreadonly"].ToString());

                            //Visualiser le mail/sms envoyé lors de l'emailing (en lecture seul)
                            if (nReadOnlyType == CAMPAIGN_READONLY_TYPE.ONLY_MAIL_FIELDS)
                                efRend = eRendererFactory.CreateConsultMailingRenderer(ePref, tableLite.DescId, nFileId, param, nWidth, nHeight);
                        }
                        else
                        {
                            efRend = eRendererFactory.CreateMainFileRenderer(ePref, tableLite.DescId, nFileId);
                        }
                    }
                    break;
                case eConst.eFileType.FILE_CREA:
                case eConst.eFileType.FILE_MODIF:

                    switch (tableLite.EdnType)
                    {

                        case EdnType.FILE_USER:
                            efRend = eRendererFactory.CreateEditFileUserRenderer(ePref, tableLite.DescId, nFileId, param);
                            break;

                        case EdnType.FILE_MAIL:
                            efRend = eRendererFactory.CreateEditMailRenderer(ePref, tableLite.DescId, nFileId, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, bEmailing, param);
                            break;

                        case EdnType.FILE_SMS:
                            efRend = eRendererFactory.CreateEditSmsRenderer(ePref, tableLite.DescId, nFileId, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, bEmailing, param);
                            break;

                        case EdnType.FILE_PLANNING:

                            Boolean calendarEnabled = ePref.GetPref(tableLite.DescId, ePrefConst.PREF_PREF.CALENDARENABLED).Equals("1");

                            if (calendarEnabled)
                                efRend = eRendererFactory.CreatePlanningFileRenderer(ePref, tableLite.DescId, nFileId, nWidth, nHeight, date, enddate, isPostback, concernedUser, viewMode, param);
                            else
                                efRend = eRendererFactory.CreateEditFileRenderer(ePref, tableLite.DescId, nFileId, param);
                            break;
                        case EdnType.FILE_PJ:
                            efRend = eRendererFactory.CreatePJFileRenderer(ePref, tableLite.DescId, nFileId, param);
                            break;
                        default:
                            switch (tableLite.TabType)
                            {
                                case TableType.CAMPAIGN:
                                    efRend = eRendererFactory.CreateCampaignFileRenderer(ePref, nFileId, param);
                                    break;
                                case TableType.RGPDTREATMENTSLOGS:
                                    efRend = eRendererFactory.CreateRGPDTreatmentLogFileRenderer(ePref, nFileId, param);
                                    break;
                                default:
                                    efRend = eRendererFactory.CreateEditFileRenderer(ePref, tableLite.DescId, nFileId, param);
                                    break;
                            }
                            break;
                    }
                    break;

                case eConst.eFileType.FILE_VCARD:
                    if (tableLite.DescId == 200)
                        efRend = eRendererFactory.CreateVCardRenderer(ePref, nFileId, nPg, bCroix);
                    else
                        //CNA - Affichage de la MiniFiche au survol de xx01 si champ de liaison
                        efRend = eRendererFactory.CreateMiniFileRenderer(ePref, tableLite.DescId, nFileId, bCroix);
                    break;
                case eConst.eFileType.FILE_PRINT:
                    efRend = eRendererFactory.CreateMainFileRenderer(ePref, tableLite.DescId, nFileId);
                    break;
                default:
                    break;
            }

            return efRend;
        }

        public static eRenderer CreateConsultMailingRenderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> dicParams, int nWidth, int nHeight)
        {
            Panel container = new Panel();
            container.CssClass = "mailing_body_container";

            string sError = string.Empty;

            int nType = eConst.eFileType.FILE_CONSULT.GetHashCode();

            string strMailTo = string.Empty;
            bool bMailForward = false;
            bool bMailDraft = false;

            // Paramètres supplémentaires transmis au constructeur de rendu
            ExtendedDictionary<string, Object> param = new ExtendedDictionary<string, Object>();
            eFileTools.eFileContext ef = new eFileTools.eFileContext(new eFileTools.eParentFileId(), ePref.User, nTab, 0);
            param.Add("filecontext", ef);
            param.Add("mailforward", bMailForward);
            param.Add("maildraft", bMailDraft);
            param.Add("mailto", strMailTo);
            param.Add("ntabfrom", nTab);
            param.Add("eFileType", nType);

            eMailing mailing = new eMailing(ePref, nFileId);


            eRenderer efRend;
            if (dicParams.ContainsKey("bSms") && dicParams["bSms"].Equals("1"))
            {
                param.Add("bSms", dicParams["bSms"]);
                efRend = eRendererFactory.CreateConsultSMSMailingRenderer(ePref, nTab, mailing, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, param);
            }
            else
                efRend = eRendererFactory.CreateEditMailingRenderer(ePref, nTab, mailing, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, param);


            /*
            if (!string.IsNullOrEmpty(efRend.ErrorMsg) || efRend.InnerException != null)
            {
                SetError(QueryErrorType.ERROR_NUM_FIELD_NOT_FOUND, string.Concat("Erreur Renderer de MailingWizardRenderer, BuildMailBodyPanel : ", efRend.ErrorMsg, (efRend.InnerException != null ? efRend.InnerException.StackTrace : string.Empty)));
            } 
            */

            return efRend;
        }



        /// <summary>
        /// création du cadre contenant les informations des fiches parentes en en-tête d'une fiche principale
        /// dans le cadre de la fiche complète
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="eFRdr"></param>
        /// <returns></returns>
        public static eRenderer CreateParenttInHeadRenderer(ePref pref, eMasterFileRenderer eFRdr)
        {
            eFileParentInHeadRenderer fhRenderer = new eFileParentInHeadRenderer(pref, eFRdr);
            if (!fhRenderer.Generate())
                return fhRenderer;

            return fhRenderer;
        }

        /// <summary>
        /// Création du cadre contenant les informations des fiches parentes en en-tête d'une fiche principale
        /// Dans le cadre du rafraichissement suite à une modification.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ef">Objet de données eFileHeader</param>
        /// <returns></returns>
        public static eRenderer CreateParenttInHeadRenderer(ePref pref, eFileHeader ef)
        {
            eFileParentInHeadRenderer fhRenderer = new eFileParentInHeadRenderer(pref, ef);

            if (!fhRenderer.Generate())
                return fhRenderer;

            return fhRenderer;
        }

        /// <summary>
        /// Création du cadre contenant les informations des fiches parentes affichées en pied de page pour les templates
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="efRdr"></param>
        /// <returns></returns>
        public static eRenderer CreateParenttInFootRenderer(ePref pref, eMainFileRenderer efRdr)
        {
            eFileParentInFootRenderer fFooterRdr = new eFileParentInFootRenderer(pref, efRdr);
            fFooterRdr.Generate();
            return fFooterRdr;
        }

        /// <summary>
        /// Rendu graphique du cadre de pied de page contenant les informations sur les liaisons parentes
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ef">Objet de données eFileHeader</param>
        /// <returns></returns>
        public static eRenderer CreateParenttInFootRenderer(ePref pref, eFileHeader ef)
        {
            eFileParentInFootRenderer fFooterRdr = new eFileParentInFootRenderer(pref, ef);
            //  fFooterRdr._rType = RENDERERTYPE.FileParentInFoot;
            fFooterRdr.Generate();
            return fFooterRdr;
        }

        /// <summary>
        /// Création du rendu graphique d'une fiche (EVENT) en consultation pleine page
        /// doit être appelé via CreateRenderer
        /// </summary>
        /// <returns>Objet construit et directement utilisable par le manager</returns>
        private static eRenderer CreateMainFileRenderer(ePref ePref, int nTab, int nFileId)
        {
            eMainFileRenderer fileRenderer = new eMainFileRenderer(ePref, nTab, nFileId);

            if (nFileId == 0)
            {
                //Interception des erreur
                fileRenderer.SetError(QueryErrorType.ERROR_NUM_FILE_NOT_FOUND, "Erreur : Fiche introuvable (0) ");
                return fileRenderer;
            }

            if (!fileRenderer.Generate())
                return fileRenderer;

            fileRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CONSULT.GetHashCode().ToString());

            return fileRenderer;
        }

        /// <summary>
        /// Création du rendu graphique d'une fiche (EVENT) en mode impression pleine page
        /// </summary>
        /// <param name="ePref">ePref du contecte</param>
        /// <param name="nTab">Table courante</param>
        /// <param name="nFileId">FileId de la fiche</param>
        /// <param name="nPrintParams">Objet qui définit les paramètres d'impression</param>
        /// <returns>Objet construit et directement utilisable par le manager</returns>
        public static eRenderer CreatePrintFileRenderer(ePref ePref, TableLite tl, int nTab, int nFileId, ePrintParams nPrintParams)
        {
            eRenderer printFileRenderer;
            if (tl.EdnType == EdnType.FILE_PLANNING)
                printFileRenderer = new ePrintPlanningFileRenderer(ePref, nTab, nFileId, nPrintParams);
            else if (tl.EdnType == EdnType.FILE_MAIL || tl.EdnType == EdnType.FILE_SMS)
                printFileRenderer = new ePrintMailFileRenderer(ePref, nTab, nFileId, nPrintParams);
            else
                printFileRenderer = new ePrintFileRenderer(ePref, nTab, nFileId, nPrintParams);

            if (nFileId == 0)
            {
                //Interception des erreur
                printFileRenderer.SetError(QueryErrorType.ERROR_NUM_FILE_NOT_FOUND, "Erreur : Fiche introuvable (0) ");
                return printFileRenderer;
            }

            if (!printFileRenderer.Generate())
                return printFileRenderer;

            printFileRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_PRINT.GetHashCode().ToString());

            return printFileRenderer;
        }



        /// <summary>
        /// Création du rendu graphique d'une fiche EMAIL en consultation
        /// </summary>
        /// <returns>Objet construit et directement utilisable par le manager</returns>
        /// <param name="nWidth">Largeur du renderer</param>
        /// <param name="nHeight">Hauteur du renderer</param>
        /// <param name="dicParams">Dictionnaire de paramètres additionnels</param>
        private static eRenderer CreateMailFileRenderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> dicParams, int nWidth, int nHeight, string strMailTo = "", bool bIsSMS = false)
        {
            eMailFileRenderer mailRenderer = new eMailFileRenderer(ePref, nTab, nFileId, nWidth, nHeight, strMailTo, bIsSMS);
            mailRenderer.PopupDisplay = true; // une fiche E-mail est toujours affichée en popup

            //Tous les paramètres
            mailRenderer.SetDicParams(dicParams);

            if (nFileId == 0)
            {
                //Interception des erreur
                mailRenderer.SetError(QueryErrorType.ERROR_NUM_FILE_NOT_FOUND, "Erreur : Fiche introuvable (0) ");

                return mailRenderer;
            }

            if (!mailRenderer.Generate())
                return mailRenderer;

            mailRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CONSULT.GetHashCode().ToString());

            return mailRenderer;
        }

        /// <summary>
        /// Méthode de génération du rendu d'une annexe sous forme de fiche
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <param name="fileid"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static eRenderer CreatePJFileRenderer(ePref pref, int tab, int fileid, ExtendedDictionary<string, object> param) {
            ePJFileRenderer rdr = new ePJFileRenderer(pref, tab, fileid);

            rdr.SetDicParams(param);

            //Mode Popup
            Boolean bPopupDisplay = false;
            param.TryGetValueConvert("popup", out bPopupDisplay);
            rdr.PopupDisplay = bPopupDisplay;


            if (fileid == 0)
            {
                //Interception des erreur
                rdr.SetError(QueryErrorType.ERROR_NUM_FILE_NOT_FOUND, "Erreur : Fiche introuvable (0) ");

                return rdr;
            }

            if (!rdr.Generate())
                return rdr;

            rdr.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());




            return rdr;

        }
        /// <summary>
        /// Retourne un renderer pour les modes VCard
        /// </summary>
        /// <param name="bCroix">Indique si on affiche ou non la croix de fermeture de la Vcard</param>
        /// <returns></returns>
        private static eRenderer CreateVCardRenderer(ePref pref, int nFileId, int nPage, Boolean bCroix)
        {
            eVCardFileRenderer vCardRenderer = new eVCardFileRenderer(pref, nFileId, nPage);

            if (nFileId == 0)
            {

                vCardRenderer.SetError(QueryErrorType.ERROR_NUM_FILE_NOT_FOUND, "Erreur : Fiche introuvable (0) ");
                return vCardRenderer;
            }

            vCardRenderer.BCroix = bCroix;
            vCardRenderer.Generate();

            return vCardRenderer;
        }



        /// <summary>
        /// Création du pannel de duplication des signets
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eRenderer CreateDuplicationBkmsRenderer(ePref pref, int nTab)
        {

            ExtendedDictionary<string, object> dico = new ExtendedDictionary<string, object>();
            dico.Add("clone", true);
            dico.Add("globaltreat", true);

            eFile myFile = eFileMain.CreateEditMainFile(pref, nTab, 0, dico);
            eRenderer er = eRenderer.CreateRenderer();
            er.PgContainer.Controls.Add(eEditFileLiteRenderer.GetBkmsToClonePanel(myFile, pref, dico));

            return er;

        }


        /// <summary>
        /// Création du rendu graphique d'une fiche (EVENT) en création et modification
        /// </summary>
        /// <param name="ePref">ePref</param>
        /// <param name="nTab">Descid de la table</param>
        /// <param name="nFileId">FileId de la fiche en cours de modification / si 0 on est en création</param>
        /// <param name="param">Dictionaire des paramètres spécifique au renderer
        /// les paramètres possible sont (au 21/03/2013)
        /// "dicvalues" : dictionnaire descid-valeurs de champs de la fiche ouverte. Utilisé pour la création et la gestion des règles sur les modes popup
        /// "popup" : indique si on est en mode popup
        /// "createPpAdrFromPm" : indique la création en série d'un couple PP/ADR depuis une adresse</param>
        /// <returns>
        /// Objet construit et directement utilisable par le manager
        /// </returns>
        private static eRenderer CreateEditFileRenderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> param)
        {
            eEditFileRenderer editFileRenderer = new eEditFileRenderer(ePref, nTab, nFileId);
            //editFileRenderer._rType = RENDERERTYPE.EditFile;

            //Tous les paramètres
            editFileRenderer.SetDicParams(param);

            //Mode Popup
            Boolean bPopupDisplay = false;
            param.TryGetValueConvert("popup", out bPopupDisplay);
            editFileRenderer.PopupDisplay = bPopupDisplay;
            // Signet mode fiche
            bool isBkmFile = false;
            param.TryGetValueConvert("bkmfile", out isBkmFile);
            editFileRenderer.IsBkmFile = isBkmFile;

            //Création de pp depuis pm (avec création d'adresse).
            Boolean bCreatePPADRFromPM = false;
            param.TryGetValueConvert("createPpAdrFromPm", out bCreatePPADRFromPM);
            editFileRenderer.CreatePPADRFromPM = bCreatePPADRFromPM;


            #region Mode résumé
            eMasterFileRenderer.ResumeMode modeResume = eMasterFileRenderer.ResumeMode.Default;

            // Si l'utilisateur a demandé explicitement un mode en cliquant sur le bouton...
            if (param.ContainsKey("modeResume"))
            {
                Boolean bModeResume = false;
                param.TryGetValueConvert("modeResume", out bModeResume);

                modeResume = (bModeResume) ? eMasterFileRenderer.ResumeMode.Yes : eMasterFileRenderer.ResumeMode.No;
            }

            editFileRenderer.ModeResume = modeResume;
            #endregion

            //Génération du renderer. 
            if (!editFileRenderer.Generate())
                return editFileRenderer;

            if (editFileRenderer.File.FileId == 0)
                editFileRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CREA.GetHashCode().ToString());
            else
                editFileRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            return editFileRenderer;
        }


        /// <summary>
        /// Création du rendu graphique d'une fiche (EVENT) en création et modification
        /// </summary>
        /// <param name="ePref">ePref</param>
        /// <param name="nTab">Descid de la table</param>
        /// <param name="nFileId">FileId de la fiche en cours de modification / si 0 on est en création</param>
        /// <param name="param">Dictionaire des paramètres spécifique au renderer
        /// les paramètres possible sont (au 21/03/2013)
        /// "dicvalues" : dictionnaire descid-valeurs de champs de la fiche ouverte. Utilisé pour la création et la gestion des règles sur les modes popup
        /// "popup" : indique si on est en mode popup
        /// "createPpAdrFromPm" : indique la création en série d'un couple PP/ADR depuis une adresse</param>
        /// <returns>
        /// Objet construit et directement utilisable par le manager
        /// </returns>
        private static eRenderer CreateEditFileUserRenderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> param)
        {
            eUserFileRenderer editFileRenderer = new eUserFileRenderer(ePref, nTab, nFileId);
            //editFileRenderer._rType = RENDERERTYPE.EditFile;

            //Tous les paramètres
            editFileRenderer.SetDicParams(param);


            bool bB = false;
            editFileRenderer.DicParams.TryGetValueConvert<bool>("popup", out bB);
            editFileRenderer.PopupDisplay = bB;

            //Génération du renderer. 
            if (!editFileRenderer.Generate())
                return editFileRenderer;

            if (editFileRenderer.File.FileId == 0)
                editFileRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CREA.GetHashCode().ToString());
            else
                editFileRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            return editFileRenderer;
        }

        /// <summary>
        /// Création du rendu graphique d'une fiche Campagne.
        /// </summary>
        /// <param name="ePref">ePref</param>
        /// <param name="nFileId">File ID</param>
        /// <param name="param">Dico de paramètres</param>
        /// <returns></returns>
        public static eRenderer CreateCampaignFileRenderer(ePref ePref, int nFileId, ExtendedDictionary<string, object> param)
        {
            eCampaignFileRenderer campaignRenderer = new eCampaignFileRenderer(ePref, nFileId);

            campaignRenderer.SetDicParams(param);

            //Mode Popup
            Boolean bPopupDisplay = false;
            param.TryGetValueConvert("popup", out bPopupDisplay);
            campaignRenderer.PopupDisplay = bPopupDisplay;


            if (!campaignRenderer.Generate())
                return campaignRenderer;

            campaignRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            return campaignRenderer;
        }

        /// <summary>
        /// Rendu d'une fiche RGPDTreatmentLog
        /// </summary>
        /// <param name="ePref">ePref</param>
        /// <param name="fileId">FileID</param>
        /// <param name="param">Paramètres</param>
        /// <returns></returns>
        public static eRenderer CreateRGPDTreatmentLogFileRenderer(ePref ePref, int fileId, ExtendedDictionary<string, object> param)
        {
            eRGPDTreatmentLogFileRenderer rdr = new eRGPDTreatmentLogFileRenderer(ePref, fileId);

            rdr.SetDicParams(param);
            rdr.PopupDisplay = true;

            if (!rdr.Generate())
                return rdr;

            if (rdr.File.FileId == 0)
                rdr.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CREA.GetHashCode().ToString());
            else
                rdr.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            return rdr;
        }

        /// <summary>
        /// gestionnaire de rendu du mode fiche sans signet
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreateFileLiteRenderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> param = null)
        {
            eEditFileLiteRenderer rdr = new eEditFileLiteRenderer(ePref, nTab, nFileId);

            if (!rdr.Generate())
                return rdr;

            rdr.PgContainer.Attributes.Add("fid", nFileId.ToString());
            rdr.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            return rdr;
        }

        /// <summary>
        /// Renderer pour la partie de la fiche affichée sous les signets.
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreateFilePart2Renderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> param = null)
        {
            eRenderer rdr = eRenderer.CreateRenderer();

            eEditFileLiteRenderer liteRdr = (eEditFileLiteRenderer)eRendererFactory.CreateFileLiteRenderer(ePref, nTab, nFileId, param);

            eTools.TransfertFromTo(liteRdr.BackBoneRdr.PnFilePart2, rdr.PgContainer);

            return rdr;
        }

        /// <summary>
        /// gestionnaire de rendu de la partie haute de la fiche, incorporée dans le squelette global du mode fiche
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreateFilePart1WithBackBoneRenderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> param = null)
        {
            eRenderer rdr;

            if (nTab == TableType.CAMPAIGN.GetHashCode())
            {
                rdr = eRendererFactory.CreateCampaignFileRenderer(ePref, nFileId, param);
                rdr.PgContainer.Attributes.Add("fid", nFileId.ToString());
            }
            else
            {
                rdr = eRendererFactory.CreateFileLiteRenderer(ePref, nTab, nFileId, param);
            }

            return rdr;
        }

        /// <summary>
        /// Création de l'assistant d'import
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>      
        /// <returns></returns>
        public static eRenderer CreateImportWizardRenderer(ePref pref, eImportWizardParam importParams)
        {
            eBaseWizardRenderer renderer = new eImportWizardRenderer(pref, importParams);
            renderer.Generate();
            return renderer;
        }

        /// <summary>
        /// gestionnaire de rendu de la partie haute de la fiche
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreateFilePart1Renderer(ePref ePref, int nTab, int nFileId, ExtendedDictionary<string, Object> param = null)
        {
            eRenderer rdr = eRenderer.CreateRenderer();

            eEditFileLiteRenderer rdrWithBackBone = (eEditFileLiteRenderer)eRendererFactory.CreateFilePart1WithBackBoneRenderer(ePref, nTab, nFileId, param);

            eTools.TransfertFromTo(rdrWithBackBone.BackBoneRdr.PnFilePart1, rdr.PgContainer);

            return rdr;
        }

        /// <summary>
        /// rendu des propriétés de la fiche
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreatePropertiesRenderer(ePref pref, int nTab, int nFileId)
        {
            eFilePropertiesRenderer rdr = new eFilePropertiesRenderer(pref, nTab, nFileId);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }

        /// <summary>
        /// renderer pour récupérer le squelette html du mode fiche.
        /// </summary>
        /// <returns></returns>
        public static eRenderer CreateFileBackBone(int nTab, Boolean bPopup, Boolean bDisplayBookmark, bool isBkmFile)
        {
            eFileBackBoneRenderer rdr = new eFileBackBoneRenderer(nTab, bPopup, bDisplayBookmark, isBkmFile);

            rdr.GenerateBackBone();

            return rdr;
        }

        /// <summary>
        /// gère le rendu du cadre des champs adresse dans le formulaire de création d'un contact
        /// </summary>
        /// <param name="bFromPM"></param>
        /// <param name="dicParams">Paramètres additionnels</param>
        /// <param name="dicValues">Dictionnaire des valeurs des champs envoyé du navigateur client</param>
        /// <param name="ePref">Préférences utilisateur de cnx</param>
        /// <returns></returns>
        public static eRenderer CreateAddressCreationRenderer(ePref ePref, Dictionary<int, string> dicValues = null, Boolean bFromPM = false, Dictionary<string, Object> dicParams = null)
        {
            object myObj = null;
            Boolean bWithoutAdr = false;
            if (dicParams.TryGetValue("withoutadr", out myObj))
                bWithoutAdr = (Boolean)myObj;

            //#37334  - On récupère l'option d'écriture des adresses posta - cas de la création de pp avec changement adresse pro
            Boolean bOverWriteAdr = false;
            if (dicParams.TryGetValue("adrprooverwrite", out myObj))
                bOverWriteAdr = (Boolean)myObj;

            // on simule l'affichage en popup pour ne pas avoir les informations d'en-tête du renderer de base.
            ExtendedDictionary<string, object> dicParam = new ExtendedDictionary<string, object>();
            dicParam.Add("dicvalues", dicValues);
            dicParam.Add("createPpAdrFromPm", bFromPM);
            dicParam.Add("withoutadr", bWithoutAdr);
            dicParam.Add("adrprooverwrite", bOverWriteAdr);
            dicParam.Add("isPPAdrCombined", dicParams["isPPAdrCombined"]);

            eFileTools.eFileContext ef = new eFileTools.eFileContext(new eFileTools.eParentFileId(), ePref.User, TableType.ADR.GetHashCode(), TableType.PP.GetHashCode());


            eFileTools.eFileContext efParent;

            //#60578 - pour le filecontexte de l'adresse, reprend la propriété posteback du parent
            // pour reprendre la non ré exécution des formules du haut sur ce genre de refresh.
            // la valeur du champ est posté et reprise, il ne faut pas la remplacer par celle par défaut
            if (dicParams.ContainsKey("filecontext") && dicParams["filecontext"] != null)
            {
                efParent = (eFileTools.eFileContext)dicParams["filecontext"];
                ef.IsPostBack = efParent.IsPostBack;
            }


            dicParam.Add("filecontext", ef);
            dicParam.Add("popup", true);

            eRenderer addressRenderer = CreateEditFileRenderer(ePref, TableType.ADR.GetHashCode(), 0, dicParam);
            addressRenderer.PgContainer.ID = string.Concat("fileDiv_", TableType.ADR.GetHashCode());
            return addressRenderer;
        }

        /// <summary>
        /// Création du rendu graphique d'une fiche EMAIL en création et modification
        /// </summary>
        /// <param name="ePref">préférence utilisateur</param>
        /// <param name="nTab">Descid de la table</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="nWidth">Largeur du conteneur</param>
        /// <param name="nHeight">Hauteur du conteneur</param>
        /// <param name="strMailTo">Destinataire(s)</param>
        /// <param name="bMailForward">Type transfert</param>
        /// <param name="bMailDraft">Type Draft</param>
        /// <param name="bEmailing">Indique si on est en mode E-mailing ou mail unitaire</param>
        /// <param name="dicParams">Paramètres complémentaires</param>
        /// <returns>objet construit et directement utilisable par le manager</returns>
        public static eRenderer CreateEditMailRenderer(ePref ePref, int nTab, int nFileId, int nWidth, int nHeight, string strMailTo, Boolean bMailForward, Boolean bMailDraft, Boolean bEmailing, ExtendedDictionary<string, Object> dicParams = null)
        {
            eEditMailRenderer editMailRenderer = new eEditMailRenderer(ePref, nTab, nFileId, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, bEmailing);

            editMailRenderer.SetDicParams(dicParams);

            editMailRenderer.PopupDisplay = true; // une fiche E-mail est toujours affichée en popup

            if (!editMailRenderer.Generate())
                return editMailRenderer;

            // Id du container
            //#36216 - En mode forward, il y a un id mais c'est bien une création. Le flag est nécessaire pour eEngine.js
            if (nFileId == 0 || bMailForward)
                editMailRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CREA.GetHashCode().ToString());
            else
                editMailRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            return editMailRenderer;
        }

        /// <summary>
        /// Création du rendu graphique d'une fiche SMS en création et modification
        /// </summary>
        /// <param name="ePref">préférence utilisateur</param>
        /// <param name="nTab">Descid de la table</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="nWidth">Largeur du conteneur</param>
        /// <param name="nHeight">Hauteur du conteneur</param>
        /// <param name="strMailTo">Destinataire(s)</param>
        /// <param name="bMailForward">Type transfert</param>
        /// <param name="bMailDraft">Type Draft</param>
        /// <param name="bEmailing">Indique si on est en mode E-mailing ou mail unitaire</param>
        /// <param name="dicParams">Paramètres complémentaires</param>
        /// <returns>objet construit et directement utilisable par le manager</returns>
        public static eRenderer CreateEditSmsRenderer(ePref ePref, int nTab, int nFileId, int nWidth, int nHeight, string strMailTo, Boolean bMailForward, Boolean bMailDraft, Boolean bEmailing, ExtendedDictionary<string, Object> dicParams = null)
        {
            eEditSmsRenderer editSmsRenderer = new eEditSmsRenderer(ePref, nTab, nFileId, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, bEmailing);

            editSmsRenderer.SetDicParams(dicParams);

            editSmsRenderer.PopupDisplay = true; // une fiche SMS est toujours affichée en popup

            if (!editSmsRenderer.Generate())
                return editSmsRenderer;

            // Id du container
            //#36216 - En mode forward, il y a un id mais c'est bien une création. Le flag est nécessaire pour eEngine.js
            if (nFileId == 0 || bMailForward)
                editSmsRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CREA.GetHashCode().ToString());
            else
                editSmsRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            //SHA : correction bug #72 765, il y'avait un height à 537 px et qui rajoutait un blanc à la fin de la fiche
            editSmsRenderer.PgContainer.Style.Add(HtmlTextWriterStyle.Height, "100%");

            editSmsRenderer.PgContainer.Attributes.Add("sms", "1");

            return editSmsRenderer;
        }


        /// <summary>
        /// Création du rendu graphique d'une fiche campaigne en mode smsing en consult
        /// </summary>
        /// <param name="nWidth">Largeur du conteneur</param>
        /// <param name="nHeight">Hauteur du conteneur</param>
        /// <param name="bMailForward">Type transfert</param>
        /// <param name="bMailDraft">Type Draft</param>
        /// <param name="dicParams">Paramètres complémentaires</param>
        /// <returns>Objet construit et directement utilisable par le manager</returns>
        private static eRenderer CreateConsultSMSMailingRenderer(ePref ePref, int nTab, eMailing mailing, int nWidth, int nHeight, string strMailTo, bool bMailForward, bool bMailDraft, ExtendedDictionary<string, object> param)
        {
            eConsultSmsMailingRenderer consultSmsMailingRenderer;
            consultSmsMailingRenderer = new eConsultSmsMailingRenderer(ePref, nTab, mailing, nWidth, nHeight, strMailTo, bMailForward, bMailDraft);

            consultSmsMailingRenderer.SetDicParams(param);
            consultSmsMailingRenderer.PopupDisplay = true; // ASSISTANT/Consultation MAil est toujours affiché en popup

            if (!consultSmsMailingRenderer.Generate())
                return consultSmsMailingRenderer;

            // Id du container
            if (mailing != null && mailing.Id == 0)
                consultSmsMailingRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CREA.GetHashCode().ToString());
            else
                consultSmsMailingRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            consultSmsMailingRenderer.PgContainer.Attributes.Add("sms", "1");

            return consultSmsMailingRenderer;

        }

        /// <summary>
        /// Création du rendu graphique d'une fiche campaigne en mode Emailing en création/modification
        /// </summary>
        /// <param name="ePref">Préférences user et system</param>
        /// <param name="mailing">Instance de mailing à creer</param>
        /// <param name="nTab">Table du</param>
        /// <param name="nWidth">Largeur du conteneur</param>
        /// <param name="nHeight">Hauteur du conteneur</param>
        /// <param name="bMailForward">Type transfert</param>
        /// <param name="bMailDraft">Type Draft</param>
        /// <param name="dicParams">Paramètres complémentaires</param>
        /// <returns>Objet construit et directement utilisable par le manager</returns>
        public static eRenderer CreateEditMailingRenderer(ePref ePref, int nTab, eMailing mailing, int nWidth, int nHeight, string strMailTo, Boolean bMailForward, Boolean bMailDraft, ExtendedDictionary<string, Object> dicParams = null)
        {
            eEditMailingRenderer editMailingRenderer;

            if (mailing.MailingType == TypeMailing.SMS_MAILING_FROM_BKM || (dicParams != null && dicParams.ContainsKey("bSms") && dicParams["bSms"].ToString() == "1"))
                editMailingRenderer = new eEditSmsMailingRenderer(ePref, nTab, mailing, nWidth, nHeight, strMailTo, bMailForward, bMailDraft);
            else
                editMailingRenderer = new eEditMailingRenderer(ePref, nTab, mailing, nWidth, nHeight, strMailTo, bMailForward, bMailDraft);

            editMailingRenderer.SetDicParams(dicParams);



            editMailingRenderer.PopupDisplay = true; // ASSISTANT/Consultation MAil est toujours affiché en popup

            if (!editMailingRenderer.Generate())
                return editMailingRenderer;

            // Id du container
            if (mailing != null && mailing.Id == 0)
                editMailingRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_CREA.GetHashCode().ToString());
            else
                editMailingRenderer.PgContainer.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            // Pas de la css du mode fiche pour l'asssitant emailing/sms
            editMailingRenderer.PgContainer.CssClass = string.Empty;

            return editMailingRenderer;
        }

        /// <summary>
        /// Création du rendu graphique d'une fiche (Planning) en création et modification
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId">FileId de la fiche en cours de modification / si 0 on est en création</param>
        /// <param name="nHeight">Hauteur de la pop up</param>
        /// <param name="nWidth">Largeur de la pop up</param>
        /// <returns>Objet construit et directement utilisable par le manager</returns>
        private static eRenderer CreatePlanningFileRenderer(ePref ePref, int nTab, int nFileId, int nWidth, int nHeight, string date, string endDate, bool isPostBack, int concernedUser, CalendarViewMode viewMode, ExtendedDictionary<string, Object> dicParams = null)
        {
            ePlanningFileRenderer editPlanningRenderer = new ePlanningFileRenderer(ePref, nTab, nFileId, nWidth, nHeight, date, endDate, isPostBack, concernedUser);

            editPlanningRenderer.SetDicParams(dicParams);
            editPlanningRenderer.ViewMode = viewMode;
            //
            if (!editPlanningRenderer.Generate())
                return editPlanningRenderer;

            //Id et info sur le div contairer

            editPlanningRenderer.PgContainer.Attributes.Add("ftrdr", nFileId == 0 ? eConst.eFileType.FILE_CREA.GetHashCode().ToString() : eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            editPlanningRenderer.PgContainer.Attributes.Add("edntype", editPlanningRenderer.File.ViewMainTable.EdnType.GetHashCode().ToString());

            return editPlanningRenderer;
        }
        #region Formulaire
        /// <summary>
        /// Création du rendu graphique d'un formulaire client
        /// </summary>
        /// <param name="uid">UID de base de données</param>
        /// <param name="appExternalUrl">Url externe du formulaire</param>
        /// <param name="pref">Preferences</param>
        /// <param name="formularId">Id de formulaire source de l'affichage</param>
        /// <param name="evtFileId">Id de l'event lié à ce formulaire</param>
        /// <param name="tplFileId">Id du template ++ lié à ce formulaire</param>
        /// <param name="tranDescId"></param>
        /// <param name="paymentInfos"></param>
        public static eFormularFileRenderer CreateFormularFileRenderer(string uid, string appExternalUrl, ePref pref, int formularId, int evtFileId, int tplFileId = 0, int  tranDescId = -1)
        {
            var FormFile = new eFormular(formularId, pref, evtFileId, tplFileId);
            FormFile.AnalyseAndSerializeFields();
            FormFile.Init();
            FormFile.AlreadyInit = true;
            if (tranDescId > 0)
                FormFile.TranDescId = tranDescId;

            if (FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED && FormFile.Version == FormularVersion.ADV_V2)
                return GenerateFormularFileRenderer(new eFormularAdvFileRenderer(uid, appExternalUrl, pref, FormFile));
            else
                return GenerateFormularFileRenderer(new eFormularFileRenderer(uid, appExternalUrl, pref, FormFile));
        }
        /// <summary>
        /// Création du rendu graphique d'un formulaire client
        /// </summary>
        /// <param name="uid">UID de base de données</param>
        /// <param name="appExternalUrl">Url externe du formulaire</param>
        /// <param name="pref">Preferences</param>
        /// <param name="formFile">Objet métier formulaire source de l'affichage</param>
        public static eFormularFileRenderer CreateFormularFileRenderer(string uid, string appExternalUrl, ePref pref, eFormularFile formFile)
        {
            formFile.AlreadyInit = true;
            return GenerateFormularFileRenderer(new eFormularFileRenderer(uid, appExternalUrl, pref, formFile));
        }
        /// <summary>
        /// Génération de l'objet eFormularFileRenderer déjà instancié
        /// </summary>
        /// <param name="formRenderer">objet eFormularFileRenderer déjà instancié</param>
        /// <returns>objet eFormularFileRenderer généré</returns>
        private static eFormularFileRenderer GenerateFormularFileRenderer(eFormularFileRenderer formRenderer)
        {
            //Génération du renderer. 
            if (!formRenderer.Generate())
                return formRenderer;
            return formRenderer;
        }
        #endregion
        #endregion


        #region RENDERER ADMIN

        /// <summary>
        /// Renderer des options utilisateur
        /// </summary>
        /// <param name="pref">pref de l'utilisateur</param>
        /// <param name="module">Module a générer</param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateUserOptionsRenderer(ePref pref, eUserOptionsModules.USROPT_MODULE module)
        {
            eUserOptionsRenderer rdr = new eUserOptionsRenderer(pref, module);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }

        /// <summary>
        /// Renderer choix de la langue
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateUserOptionsPrefLangRenderer(ePref pref)
        {
            eAdminUsrOptLangRenderer rdr = new eAdminUsrOptLangRenderer(pref);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }

        /// <summary>
        /// Renderer de préférence d'affichage
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateUserOptionsPrefProfileRenderer(ePref pref)
        {
            eUserOptionsPrefProfileRenderer rdr = new eUserOptionsPrefProfileRenderer(pref);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }

        /// <summary>
        /// Renderer choix de la font
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateUserOptionsPrefFontSizeRenderer(ePref pref)
        {
            eUserOptionsPrefFontSizeRenderer rdr = new eUserOptionsPrefFontSizeRenderer(pref);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }


        /// <summary>
        /// Renderer option mru
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsPrefMruModeRenderer CreateUserOptionsPrefMruModeRenderer(ePref pref)
        {
            eUserOptionsPrefMruModeRenderer rdr = new eUserOptionsPrefMruModeRenderer(pref);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Creer un renderer du choix du signature
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateUserOptionsPrefSignRenderer(ePref pref)
        {
            eUserOptionsRenderer rdr = new eAdminUsrOptSignRenderer(pref);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }


        /// <summary>
        /// Creer un renderer du choix du signature pour l'administration (permet de changer la signature d'un autre user)
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateUserOptionsPrefSignAdminRenderer(ePref pref, int nUserId)
        {

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            eUserOptionsRenderer rdr = new eAdminUsrOptSignAdminRenderer(pref, nUserId);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }

        /// <summary>
        /// Creer un renderer du choix du mémo
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nUserid">user à modif</param>
        /// <returns></returns>
        internal static eUserOptionsRenderer CreateUserOptionsPrefMemoRenderer(ePref pref)
        {
            eUserOptionsRenderer rdr = new eAdminUsrOptMemoRenderer(pref, pref.UserId);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }



        /// <summary>
        /// Creer un renderer du choix du mémo depuis l'admin
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nUserid">user à modif</param>
        /// <returns></returns>
        internal static eUserOptionsRenderer CreateUserOptionsPrefAdminMemoRenderer(ePref pref, int nUserid)
        {
            eUserOptionsRenderer rdr = new eAdminUserOptAdminMemoRenderer(pref, nUserid);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }

        /// <summary>
        /// Crée le renderer pour le changement de mot de passe
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateUserOptionsPrefPwdRenderer(ePref pref, int userId, eUserOptionsModules.PREFERENCES_PASSWORD_CONTEXT context)
        {
            eAdminUsrOptPwdRenderer rdr = new eAdminUsrOptPwdRenderer(pref, userId, context);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }

        /// <summary>
        /// Creer un renderer des options du rapport d'exports
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eUserOptionsRenderer CreateAdminUserOptionsExportRenderer(ePref pref)
        {
            eUserOptionsRenderer rdr = new eUserOptionsAdvExportRenderer(pref);

            if (!rdr.Generate())
                return rdr;

            return rdr;
        }


        #endregion


        /// <summary>
        /// Renderer de sous onglet de de l'onglet web hormis l'onglet liste
        /// </summary>
        /// <param name="_pref">les prefs</param>
        /// <param name="specifId">l id de la specif correspiondant au sous onglet web</param>
        /// <param name="nWidth">la largeur de l'iframe</param>
        /// <param name="nHeight">la  hauteur de  l iframe</param>
        /// <returns></returns>
        public static eRenderer CreateWebTabRenderer(ePref _pref, int specifId, int nWidth, int nHeight, Boolean httpsEnabled)
        {
            eRenderer renderer = new eWebTabRenderer(_pref, specifId, nWidth, nHeight, httpsEnabled);
            renderer.Generate();
            return renderer;
        }



        /// <summary>
        /// Renderer de sous onglet de de l'onglet web hormis l'onglet liste
        /// </summary>
        /// <param name="_pref">les prefs</param>
        /// <param name="specifId">l id de la specif correspiondant au sous onglet web</param>
        /// <param name="nWidth">la largeur de l'iframe</param>
        /// <param name="nHeight">la  hauteur de  l iframe</param>
        /// <returns></returns>
        public static eRenderer CreateWebTabNavBarRenderer(ePref _pref, int specifId, int nWidth, int nHeight, Boolean httpsEnabled)
        {
            eRenderer renderer = new eWebTabRenderer(_pref, specifId, nWidth, nHeight, httpsEnabled);
            renderer.Generate();
            return renderer;
        }



        /// <summary>
        /// Créer un renderer de l'assistant d'import cible etendu
        /// </summary>
        /// <param name="_pref">Préference</param>
        /// <param name="nTab">Table principale</param>
        /// <param name="nTabFrom">Table d'ou vient</param>
        /// <param name="nEvtId">evenment auqel seront ratachées les cibles etendues </param>
        /// <param name="nWidth">Largeur de l afenetre</param>
        /// <param name="nHeight">Hauteur de la fenetre</param>
        /// <returns></returns>
        public static eRenderer CreateTargetImportWizardRenderer(ePref _pref, int nTab, int nTabFrom, int nEvtId, int nWidth, int nHeight)
        {
            eRenderer renderer = new eTargetImportRenderer(_pref, nTab, nTabFrom, nEvtId, nWidth, nHeight);
            renderer.Generate();
            return renderer;
        }

        #region RENDERER NOTIFICATIONS

        /// <summary>
        /// Créer un renderer pour la liste des notifications d'un utilisateur
        /// </summary>
        /// <param name="pref">Préference</param>
        /// <param name="nIndexLower">Index notifications début</param>
        /// <param name="nIndexUpper">Index notifications fin</param>
        /// <param name="unreadMode">Mode notifications non lues uniquement</param>
        /// <returns></returns>
        public static eRenderer CreateNotificationListRenderer(ePref pref, int nIndexLower, int nIndexUpper, bool unreadMode = false)
        {
            eRenderer renderer = eNotificationListRenderer.GetNotificationListRenderer(pref, nIndexLower, nIndexUpper, unreadMode);
            renderer.Generate();
            return renderer;
        }

        /// <summary>
        /// Créer un renderer pour les toasters des notifications
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <returns></returns>
        public static eRenderer CreateNotificationToastersRenderer(ePref pref)
        {
            eRenderer renderer = eNotificationToastersRenderer.GetNotificationToastersRenderer(pref);
            renderer.Generate();
            return renderer;
        }

        #endregion

        #region Renderer SELECTIONS

        public static eRenderer CreateSelectionWizardRenderer(ePref pref, int nWidth, int nHeight, int nTab, int nTabSource)
        {
            eRenderer er = eSelectionWizardRenderer.GetSelectionWizardRenderer(pref, nWidth, nHeight, nTab, nTabSource);
            er.Generate();
            return er;
        }


        public static eRenderer CreateSelectionRenderer(ePref pref, int tabID, eList list, int width, int height, int rows, int page)
        {
            eRenderer ec = eListFilteredSelectionRenderer.GetFilteredSelectionListRenderer(pref, tabID, list, height, width, page, rows);
            ec.Generate();
            return ec;
        }

        public static eRenderer CreateSelectionCriteriaRenderer(ePref pref, int tabID)
        {
            eRenderer ec = eSelectionCriteriaRenderer.CreateSelectionCriteriaRenderer(pref, tabID);
            ec.Generate();
            return ec;
        }

        #endregion

        /// <summary>
        /// Retourne un renderer pour les modes MiniFiche
        /// </summary>
        /// <param name="bCroix">Indique si on affiche ou non la croix de fermeture de la Vcard</param>
        /// <returns></returns>
        private static eRenderer CreateMiniFileRenderer(ePref pref, int nTab, int nFileId, Boolean bCroix)
        {
            eMiniFileRenderer miniFileRenderer = new eMiniFileRenderer(pref, nTab, nFileId);

            if (nFileId == 0)
            {
                miniFileRenderer.SetError(QueryErrorType.ERROR_NUM_FILE_NOT_FOUND, "Erreur : Fiche introuvable (0) ");
                return miniFileRenderer;
            }

            miniFileRenderer.BCroix = bCroix;
            miniFileRenderer.Generate();

            return miniFileRenderer;
        }

        public static eRenderer CreatePickADateRenderer(ePref pref, ePickADateRenderer.From from, string sDate)
        {
            ePickADateRenderer rdr = new ePickADateRenderer(pref, from, sDate);
            rdr.Generate();
            return rdr;

        }

        /// <summary>
        /// Créer un menu de droite pour paramétrer une instance d'une page d'accueil
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">descid de la table de la page d'accueil</param>
        /// <param name="fileId">id de la page d'accueil</param>
        /// <param name="height">hauteur de l'ecran</param>
        /// <param name="width">largeur de l'ecran</param>
        /// <param name="part">type de menu : config, content, param</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// Un renderer du menu
        /// </returns>
        public static eRenderer CreateXrmHomeMenuParamManager(ePref pref, int tab, int fileId, int height, int width, MenuPart part, eXrmWidgetContext context)
        {
            eRenderer rdr;

            switch (part)
            {
                case MenuPart.LOAD_WIDGET_CONFIG:
                    rdr = eXrmWidgetFactory.GetMenuWidgetParamRenderer(pref, fileId, true, context);
                    break;
                default:
                    rdr = new eXrmHomeMenuParamRenderer(pref, tab, fileId, height, width, context);
                    break;
            }

            rdr.Generate();

            return rdr;
        }

        /// <summary>
        /// Création d'un rendu de color picker
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static eRenderer CreateColorPickerRenderer(ePref pref, string color = "")
        {
            eColorPickerRenderer rdr = eColorPickerRenderer.CreateColorPickerRenderer(pref, color);
            rdr.Generate();
            return rdr;
        }
    }
}