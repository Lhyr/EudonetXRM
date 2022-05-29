using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de génération d'un renderer pour la sélection des invités (++)
    /// </summary>
    public class eInvitWizardRenderer : eBaseWizardRenderer
    {
        /// <summary>
        /// Constante temporaire pour activer ou non les catégories de désinscription RGPD
        /// </summary>
        private bool _useNewUnsubscribeMethod = false;

        private int _nTabFrom = 0;
        private int _nParentFileId = 0;

        private int _nPageFilter = 1;

        /// <summary>Objet d'accès aux données</summary>
        public eList _list;

        /// <summary>
        /// Wizard en mode Supression (Xx)
        /// </summary>
        private bool _bIsDeleteMode = false;

        /// <summary>
        /// Cible étendue
        /// </summary>
        private bool _bIsTarget = false;
        private string _sTabName;

        /// <summary>
        /// Indique si une table EventStep a été créé pour la table parente, et donc si les envois récurrents peuvent être utilisés
        /// </summary>
        private bool _eventStepEnabled = false;

        /// <summary>
        /// Catalogue de valeurs Interaction.Type de média
        /// </summary>
        private List<eCatalog.CatalogValue> _catalogMediaType;

        /// <summary>
        /// Catalogue de valeurs Interaction.Type Campagne
        /// </summary>
        private List<eCatalog.CatalogValue> _catalogCampaignType;

        /// <summary>
        /// Dictionnaire des valeurs de catalogues utilisées pour la désinscription
        /// </summary>
        private Dictionary<eTools.InteractionCommonCatalogValuesKeys, int> _dicUnsubscribeCommonCatalogValues;

        /// <summary>
        /// Génère un renderer paramétrés pour l'assistant invitation et le retourne
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="tab">Table des invitations</param>
        /// <param name="nTabFrom">Table de l'évènement</param>
        /// <param name="nParentFileId">Id de la fiche event</param>
        /// <param name="bDelete">Mode suppression</param>
        /// <returns>Renderer contenant l'interface graphique de l'assistant</returns>
        public static eInvitWizardRenderer GetInvitWizardRenderer(ePref ePref, int tab, int nTabFrom, int nParentFileId, int width, int height, bool bDelete)
        {
            return new eInvitWizardRenderer(ePref, tab, nTabFrom, nParentFileId, width, height, bDelete);
        }

        /// <summary>
        /// Initialisation du renderer du wizard de l'invitation des destinataires à partir d'un filtre
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {
                eTableLiteMailing mainTab = eLibTools.GetTableInfo(Pref, _tab, eTableLiteMailing.Factory(Pref));
                _bIsTarget = mainTab.ProspectEnabled;
                _sTabName = mainTab.Libelle;

                _catalogMediaType = GetCatalogValues((int)TableType.INTERACTION, (int)InteractionField.MediaType);
                _dicUnsubscribeCommonCatalogValues = eTools.GetUnsubscribeCommonCatalogValues(Pref);

                string useNewUnsubscribeMethodValue = string.Empty;
                if (eLibTools.GetConfigAdvValues(Pref, new HashSet<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD }).TryGetValue(eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD, out useNewUnsubscribeMethodValue) && useNewUnsubscribeMethodValue == "1")
                    _useNewUnsubscribeMethod = true;

                GetEventStepEnabled();
            }
            catch (Exception)
            {
                // TODO - Gestion d'erreur !
            }

            return base.Init();
        }

        
        private void GetEventStepEnabled()
        {
            //Fonction d'envoi récurrent disponible uniquement si depuis un signet ++ (onglet secondaire avec une liaison vers la table Address) lié à un onglet qui a « Mode opération » coché
            if (!eFeaturesManager.IsFeatureAvailable(this.Pref, eConst.XrmFeature.AdminEventStep))
                return;

            eudoDAL dal = eLibTools.GetEudoDAL(this.Pref);
            try
            {
                dal.OpenDatabase();

                //_nTabFrom = descId de l'onglet parent du signet
                DescAdvDataSet descAdv = new DescAdvDataSet();
                descAdv.LoadAdvParams(dal, new int[] { _nTabFrom }, DESCADV_PARAMETER.EVENT_STEP_ENABLED);
                _eventStepEnabled = descAdv.GetAdvInfoValue(_nTabFrom, DESCADV_PARAMETER.EVENT_STEP_ENABLED, "0") == "1";
            }
            finally
            {
                dal?.CloseDatabase();
            }
        }

        /// <summary>
        /// Constructeur du renderer
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nTabFrom"></param>
        /// <param name="nParentFileId"></param>
        /// <param name="bDelete"></param>
        private eInvitWizardRenderer(ePref ePref, int nTab, int nTabFrom, int nParentFileId, int width, int height, bool bDelete)
        {
            _nbStep = 2;
            _bIsDeleteMode = bDelete;
            Pref = ePref;
            _tab = nTab;
            _nTabFrom = nTabFrom;
            PgContainer.ID = "wizard";
            _nPageFilter = 1;
            _nParentFileId = nParentFileId;
            _width = width;
            _height = height;


            //Liste des ressources nécessaires : 200 : Contact / 400 : Adresse / 411/12 adresse active/principale
            _sListRes = string.Concat("200,400,411,412,", _tab, ",", _nTabFrom);

            lstStep.Add(new WizStep(1, eResApp.GetRes(Pref, 6445), BuildFilterSelection));
            lstStep.Add(new WizStep(2, eResApp.GetRes(Pref, 6400), BuildSelectPPPanel));
        }


        #region Entete Filtre ++

        /// <summary>
        /// retourne le panel contenant les filtres.
        /// </summary>
        /// <returns></returns>
        private Panel BuildFilterSelection()
        {
            eRenderer er = eRendererFactory.CreateInvitFilterListRenderer(Pref, _width, _height, _bIsDeleteMode ? _tab : 200, _bIsDeleteMode);

            if (er.InnerException == null && er.ErrorMsg.Length == 0)
            {
                Panel pnFilterSelection = new Panel();
                Panel pnInstruction = new Panel();

                bool recipFilterActive = _eventStepEnabled && !_bIsDeleteMode;

                //SHA
                #region Ajout des destinataires répondant au filtre sélectionné
                if (recipFilterActive)
                {

                    //informatin table parente

                    eFile objFile = eFileForBkm.CreateFileForBkmBar(_ePref, _nTabFrom, _nParentFileId);
                    if (((eRecordEvent)objFile.Record).OnBreak == 0)
                    {                        
                        HtmlGenericControl ul = new HtmlGenericControl("ul");
                        ul.Attributes.Add("class", "RecipFilter");
                        pnFilterSelection.Controls.Add(ul);

                        HtmlGenericControl li = new HtmlGenericControl("li");
                        ul.Controls.Add(li);
                        RadioButton rbAddImmediateRecipFilter = new RadioButton();
                        li.Controls.Add(rbAddImmediateRecipFilter);
                        rbAddImmediateRecipFilter.ID = "rbAddImmediateRecipFilter";
                        rbAddImmediateRecipFilter.GroupName = "gnInvitRecipFilter";
                        rbAddImmediateRecipFilter.Text = eResApp.GetRes(Pref, 2061);
                        rbAddImmediateRecipFilter.Attributes.Add("onclick", "nsInvitWizard.OnSelectAddFilter(false)");
                        rbAddImmediateRecipFilter.Checked = true;

                        li = new HtmlGenericControl("li");
                        ul.Controls.Add(li);

                        RadioButton addRecurrentRecipFilter = new RadioButton();
                        li.Controls.Add(addRecurrentRecipFilter);
                        addRecurrentRecipFilter.ID = "rbAddRecurrentRecipFilter";
                        addRecurrentRecipFilter.GroupName = "gnInvitRecipFilter";
                        addRecurrentRecipFilter.Text = eResApp.GetRes(Pref, 2062);
                        addRecurrentRecipFilter.Attributes.Add("onclick", "nsInvitWizard.OnSelectAddFilter(true)");
                    }
                    else
                    {
                        HtmlGenericControl ul = new HtmlGenericControl("ul");
                        ul.Attributes.Add("class", "RecipFilter");
                        pnFilterSelection.Controls.Add(ul);

                        HtmlGenericControl li = new HtmlGenericControl("li");
                        ul.Controls.Add(li);
                        li.InnerText = eResApp.GetRes(Pref, 2766); //campagne en pause

                    }
                }

                #endregion


                pnFilterSelection.Controls.Add(pnInstruction);
                pnInstruction.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6444)));
                pnInstruction.CssClass = "instruct";

                pnFilterSelection.Controls.Add(BuildLinkFileRenderer());
                pnFilterSelection.Controls.Add(er.PgContainer);

                er.PgContainer.CssClass = "tabeul";
                er.PgContainer.ID = "mainDiv";
                er.PgContainer.Attributes.Add("edntype", "invit");

                // #33 598 - Redimensionnement des listes ++ - Calcul de la hauteur à donner aux listes en fonction de celle de la fenêtre
                // 25 pixels réservés pour la barre de titre
                // 125 pixels pour la partie haute (rail de navigation + combobox de sélection du fichier de rubriques)
                // 60 pixels d'espace réservé aux boutons de la modal dialog
                // 20 pixels manque cette espace, je ne sais pas d'ou ca viens
                // A ajuster si des options se rajoutent sur la fenêtre par la suite
                int itemListHeight = _height - 25 - 125 - 60 - 20;
                if (recipFilterActive)
                    itemListHeight = itemListHeight - 50;
                er.PgContainer.Style.Add(HtmlTextWriterStyle.Height, string.Concat(itemListHeight, "px"));

                return pnFilterSelection;

            }
            else
            {
                StringBuilder sDevMsg = new StringBuilder();
                StringBuilder sUserMsg = new StringBuilder();

                sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append("Pas de descid");

                if (er.InnerException != null)
                {
                    sDevMsg.AppendLine(er.InnerException.Message).AppendLine(er.InnerException.StackTrace);
                    _eException = er.InnerException;

                }
                sUserMsg.Append(eResApp.GetRes(Pref, 422)).Append("<br>").Append(eResApp.GetRes(Pref, 544));

                _sErrorMsg = sUserMsg.ToString();

                return er.PnlError;


            }

        }





        private HtmlGenericControl BuildLinkFileRenderer()
        {
            //strBodyAttributes = "";
            HtmlGenericControl containerDiv = new HtmlGenericControl("div");


            containerDiv.Attributes.Add("class", "newFilterDiv");
            containerDiv.Attributes.Add("id", "catDivHeadAdv");

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            containerDiv.Controls.Add(ul);

            ul.Attributes.Add("class", "catToolAddLib");

            HtmlGenericControl liLabel = new HtmlGenericControl("li");
            ul.Controls.Add(liLabel);
            liLabel.Attributes.Add("class", "catToolAddLib");

            HtmlGenericControl NFiltr = new HtmlGenericControl("a");
            liLabel.Controls.Add(NFiltr);



            NFiltr.Attributes.Add("Href", string.Concat("javascript:AddNewFilter(", _bIsDeleteMode ? _tab : (int)TableType.PP, ", _aFilterWizarBtns);"));

            NFiltr.Attributes.Add("class", "buttonAdd");

            HtmlGenericControl spanIcn = new HtmlGenericControl("span");
            NFiltr.Controls.Add(spanIcn);
            spanIcn.Attributes.Add("class", "icon-add");


            HtmlGenericControl DivLabel = new HtmlGenericControl("span");
            NFiltr.Controls.Add(DivLabel);
            DivLabel.Attributes.Add("class", "catToolAddLibSp");
            DivLabel.InnerText = eResApp.GetRes(Pref, 6247); //Nouveau Filtre;


            return containerDiv;
        }


        /// <summary>
        /// Création des boutons pour le paging dans la liste des filtres
        /// </summary>
        /// <returns></returns>
        /// 
        [Obsolete("Les fonctionnalités de pagging ont été retirés à la demande de la direction/RMA")]
        private Panel CreateFilterPagingBar(int nPage)
        {
            Panel pnTitle = new Panel();
            pnTitle.CssClass = "paggingFilterWizardList";
            pnTitle.ID = "GetFilterPagging";

            // Paging activé
            HtmlGenericControl divFirst = new HtmlGenericControl("div");
            divFirst.Attributes.Add("Class", "icon-edn-first fRight disable");

            int nbLign = Pref.Context.Paging.NbResult;
            int nbRowByPage = Pref.Context.Paging.RowsByPage;
            int nbPage = 1;

            if (nbRowByPage > 0)
            {
                nbPage = nbLign / nbRowByPage;
                nbPage = nbLign > nbRowByPage * nbPage ? nbPage + 1 : nbPage;

                int pageDprt = nPage;
                if (nPage > 1)
                {
                    divFirst.Attributes.Add("onclick", string.Concat("changePage('", _tab, "', '", 0, "')"));
                    divFirst.Attributes.Add("class", "icon-edn-first fRight");

                }
                else
                {
                    divFirst.Attributes.Add("class", "icon-edn-first fRight disable");
                }


                HtmlGenericControl divPrev = new HtmlGenericControl("div");

                divPrev.Attributes.Add("Class", "icon-edn-prev fRight disable");

                if (nPage > 1)
                {
                    divPrev.Attributes.Add("onclick", string.Concat("changePage('", _tab, "', '", -1, "')"));
                    divPrev.Attributes.Add("class", "icon-edn-prev fRight");

                }
                else
                {
                    divPrev.Attributes.Add("class", "icon-edn-prev fRight disable");
                }


                HtmlGenericControl divNumPage = new HtmlGenericControl("div");
                divNumPage.Attributes.Add("class", "numpagePlsPls");
                divNumPage.Attributes.Add("align", "center");


                HtmlInputText inptNumPage = new HtmlInputText();
                divNumPage.Controls.Add(inptNumPage);
                inptNumPage.ID = string.Concat("nP_", _tab); //Ne pas changer nP il est utilisé dans les script eMAin.js
                inptNumPage.Attributes.Add("onchange", string.Concat("EnterTBPagging('", _tab, "', this.value)"));
                // ="" 
                inptNumPage.Attributes.Add("class", "pagInput");
                inptNumPage.Size = 2;
                inptNumPage.Value = pageDprt.ToString();


                HtmlGenericControl divTotalPage = new HtmlGenericControl("div");
                divTotalPage.Attributes.Add("class", "NbPagePlsPls");


                Panel TotalPage = new Panel();
                //   divTotalPage.Controls.Add(TotalPage);
                //divTotalPage.ID = string.Concat("tNP_", _tab);

                Literal liNbPage = new Literal();
                liNbPage.Text = string.Concat("&nbsp;/&nbsp;", nbPage);
                divTotalPage.Controls.Add(liNbPage);


                HtmlGenericControl divNext = new HtmlGenericControl("div");
                if (pageDprt < nbPage)
                {
                    //divNext.Attributes.Add("onclick", string.Concat("NextPage(", _list.ViewMainTable.DescId.ToString(), ",", inptNumPage.ID, ");"));
                    divNext.Attributes.Add("onclick", string.Concat("changePage('", _tab, "', '", 1, "')"));
                    divNext.Attributes.Add("class", "icon-edn-next fRight");
                }
                else
                {
                    divNext.Attributes.Add("class", "icon-edn-next fRight disable");
                }

                HtmlGenericControl divLast = new HtmlGenericControl("div");
                if (pageDprt < nbPage)
                {
                    divLast.Attributes.Add("onclick", string.Concat("changePage('", _tab, "', '", nbPage + 10, "')"));
                    divLast.Attributes.Add("class", "icon-edn-last fRight");
                }
                else
                {
                    divLast.Attributes.Add("class", "icon-edn-last fRight disable");
                }

                // rajouter pour savoir qu elle est la derniere page caché
                HtmlInputText inptNbPage = new HtmlInputText();
                inptNbPage.Attributes.Add("type", "hidden");
                inptNbPage.ID = "nbP" + _tab; //Ne pas modifier tNP Utilisé dans un script eMAin.js
                inptNbPage.Value = nbPage.ToString();
                divLast.Controls.Add(inptNbPage);


                // comme on est en float:right il faut ajouter les élément de droite à gauche
                pnTitle.Controls.Add(divLast);
                pnTitle.Controls.Add(divNext);
                pnTitle.Controls.Add(divTotalPage);
                pnTitle.Controls.Add(divNumPage);
                pnTitle.Controls.Add(divPrev);
                pnTitle.Controls.Add(divFirst);
            }
            return pnTitle;
        }

        #endregion

        /// <summary>
        /// SHA
        /// Retourne le bloc contenant la fréquence 
        /// et le lien vers la popup de paramètres de planification
        /// </summary>
        /// <returns></returns>
        private Panel BuildFrequencyPanel()
        {
            Panel pnFrequencyPlanif = new Panel();
            pnFrequencyPlanif.ID = "InvitListFilterFrequency";
            pnFrequencyPlanif.Style.Add(HtmlTextWriterStyle.Display, "none");

            Panel pnFrequencyPlanifSub = new Panel();
            pnFrequencyPlanifSub.CssClass = "InvitListFilterSub";
            pnFrequencyPlanif.Controls.Add(pnFrequencyPlanifSub);

            //Fréquence
            Panel pnfreqSubTitle = new Panel();
            pnfreqSubTitle.CssClass = "InvitListFilterSubTitle";
            pnfreqSubTitle.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 1063)));
            pnFrequencyPlanifSub.Controls.Add(pnfreqSubTitle);

            //Lien Paramètres Plannification
            Label lnkSchedule = new Label();
            lnkSchedule.ID = "lnkSchedule";
            lnkSchedule.Text = eResApp.GetRes(Pref, 6888);
            lnkSchedule.Attributes.Add("class", "gofile");
            lnkSchedule.Attributes.Add("onclick", "oInvitWizard.openScheduleParameter()");
            pnFrequencyPlanifSub.Controls.Add(lnkSchedule);

            Label lnkScheduleInfo = new Label();
            lnkScheduleInfo.ID = "lnkScheduleInfo";
            lnkScheduleInfo.Text = "";
            lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.Display, "none");
            lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.Color, "#9c9c9c");
            lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.FontStyle, "italic");
            pnFrequencyPlanifSub.Controls.Add(lnkScheduleInfo);

            return pnFrequencyPlanif;
        }


        /// <summary>
        /// Retourne le div contenant les options de filtres
        /// pour la sélection des invitations (principale, active, griser...)
        /// </summary>
        /// <returns></returns>
        private Panel BuildFilterPPPanel()
        {
            bool bFound;

            Panel pnOtherFilter = new Panel();
            pnOtherFilter.ID = "InvitListFilter";

            if (!_bIsDeleteMode)
            {
                Panel pnlOtherFilterSub = new Panel();
                pnlOtherFilterSub.CssClass = "InvitListFilterSub";
                pnOtherFilter.Controls.Add(pnlOtherFilterSub);

                // Principale/active
                Panel pnlFilterAdrActivPrinc = new Panel();
                pnlFilterAdrActivPrinc.ID = "FilterAdrActivPrinc";
                pnlFilterAdrActivPrinc.CssClass = "FilterAdrActivPrinc";
                pnlOtherFilterSub.Controls.Add(pnlFilterAdrActivPrinc);

                // Ne retenir que les fiches <PREFNAME>
                string sLabelAdr = eResApp.GetRes(Pref, 156).Trim();
                sLabelAdr = string.Concat(sLabelAdr.Replace("<PREFNAME>", WizardRes.GetRes(TableType.ADR.GetHashCode(), out bFound)), " : ");
                pnlFilterAdrActivPrinc.Controls.Add(new LiteralControl(sLabelAdr));

                //Active
                eCheckBoxCtrl radioAdrAct = new eCheckBoxCtrl(false, false);
                radioAdrAct.ID = "radioAdrAct";
                radioAdrAct.AddClick("nsInvitWizard.ChangeFilters();");
                radioAdrAct.AddText(WizardRes.GetRes(AdrField.ACTIVE.GetHashCode(), out bFound));
                pnlFilterAdrActivPrinc.Controls.Add(radioAdrAct);

                //Principale
                eCheckBoxCtrl radioAdrPrinc = new eCheckBoxCtrl(false, false);
                radioAdrPrinc.ID = "radioAdrPrinc";
                radioAdrPrinc.AddClick("nsInvitWizard.ChangeFilters();");
                radioAdrPrinc.AddText(WizardRes.GetRes(AdrField.PRINCIPALE.GetHashCode(), out bFound));
                pnlFilterAdrActivPrinc.Controls.Add(radioAdrPrinc);

                //Ne pas prendre en compte les fiches déjà associé...
                Panel pnlFilterNotDbl = new Panel();
                pnlFilterNotDbl.ID = "FilterNotDbl";
                pnlFilterNotDbl.CssClass = "FilterNotDbl";
                pnlFilterNotDbl.Style.Add(HtmlTextWriterStyle.Display, "none");
                pnlOtherFilterSub.Controls.Add(pnlFilterNotDbl);

                eCheckBoxCtrl chkDoNotDbl = new eCheckBoxCtrl(false, false);
                chkDoNotDbl.ID = "chkDoNotDbl";
                chkDoNotDbl.AddClass("chkDoNotDbl");
                chkDoNotDbl.AddClick("");
                pnlFilterNotDbl.Controls.Add(chkDoNotDbl);

                //Ne pas prendre en compte les fiches <PREFNAME> déjà associées à cette fiche <EVENT>
                string sLabelNotDbl = eResApp.GetRes(Pref, 2613).Trim();
                sLabelNotDbl = sLabelNotDbl.Replace("<PREFNAME>", WizardRes.GetRes(400, out bFound));
                sLabelNotDbl = sLabelNotDbl.Replace("<EVENT>", WizardRes.GetRes(_nTabFrom, out bFound));
                chkDoNotDbl.AddText(sLabelNotDbl);

                //Griser les fiches déjà associé...
                Panel pnlFilterGreyDbl = new Panel();
                pnlFilterGreyDbl.ID = "FilterGreyDbl";
                pnlFilterGreyDbl.CssClass = "FilterGreyDbl";
                pnlOtherFilterSub.Controls.Add(pnlFilterGreyDbl);

                eCheckBoxCtrl radioGreyDbl = new eCheckBoxCtrl(true, false);
                radioGreyDbl.ID = "radioGreyDbl";
                radioGreyDbl.AddClass("radioGreyDbl");
                radioGreyDbl.AddClick("nsInvitWizard.greyElements();");
                pnlFilterGreyDbl.Controls.Add(radioGreyDbl);

                //Création du dropdown
                string sLabelGrey = eResApp.GetRes(Pref, 550).Trim();

                DropDownList select = new DropDownList();
                select.ID = "invitSelectTypDbl";
                select.Items.Add(new ListItem(WizardRes.GetRes(400, out bFound), "400"));
                select.Items.Add(new ListItem(WizardRes.GetRes(200, out bFound), "200"));
                select.CssClass = "invitSelectTypDbl";
                select.Attributes.Add("onchange", "nsInvitWizard.greyElements()");

                string sSelect = eTools.GetControlRender(select);
                sLabelGrey = sLabelGrey.Replace("<PP>", sSelect);
                sLabelGrey = sLabelGrey.Replace("<EVENT>", WizardRes.GetRes(_nTabFrom, out bFound));

                HtmlGenericControl spanFilterGreyDbl = new HtmlGenericControl("span");
                pnlFilterGreyDbl.Controls.Add(spanFilterGreyDbl);
                spanFilterGreyDbl.Controls.Add(new LiteralControl(sLabelGrey));

                //Compteur
                Panel pnlFilterCmpt = new Panel();
                pnlFilterCmpt.ID = "FilterCmpt";
                pnlFilterCmpt.CssClass = "FilterCmpt";

                pnlOtherFilterSub.Controls.Add(pnlFilterCmpt);
                pnlFilterCmpt.Controls.Add(new LiteralControl(string.Concat(eResApp.GetRes(Pref, 437).Trim(), " : ")));

                HtmlGenericControl cmptAdrSpan = new HtmlGenericControl("span");
                cmptAdrSpan.ID = "cmptAdrSpan";
                pnlFilterCmpt.Controls.Add(cmptAdrSpan);

                Label label = new Label();
                label.Text = WizardRes.GetRes(400, out bFound);
                pnlFilterCmpt.Controls.Add(label);

                HtmlGenericControl cmptPPSpan = new HtmlGenericControl("span");
                cmptPPSpan.ID = "cmptPPSpan";
                pnlFilterCmpt.Controls.Add(cmptPPSpan);

                label = new Label();
                label.Text = WizardRes.GetRes(200, out bFound);
                pnlFilterCmpt.Controls.Add(label);

                //Filtres consentements
                if (_useNewUnsubscribeMethod)
                {
                    Panel pnlOtherFilterConsent = new Panel();
                    pnlOtherFilterConsent.CssClass = "InvitListFilterSub";
                    pnOtherFilter.Controls.Add(pnlOtherFilterConsent);

                    Panel pnlFilterTitle = new Panel();
                    pnlFilterTitle.CssClass = "InvitListFilterSubTitle";
                    pnlFilterTitle.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 1846))); //Filtrer sur les consentements
                    pnlOtherFilterConsent.Controls.Add(pnlFilterTitle);

                    //Type de média                    
                    Panel pnlFilterMediaType = new Panel();
                    pnlFilterMediaType.ID = "FilterMediaType";
                    pnlFilterMediaType.CssClass = "FilterMediaType";
                    pnlOtherFilterConsent.Controls.Add(pnlFilterMediaType);

                    pnlFilterMediaType.Controls.Add(new LiteralControl(string.Concat(eResApp.GetRes(Pref, 1839), " :"))); //Type de média

                    DropDownList selectMediaType = new DropDownList();
                    selectMediaType.ID = "invitSelectMediaType";
                    selectMediaType.CssClass = "invitSelectCampaignType";
                    selectMediaType.Attributes.Add("onchange", "nsInvitWizard.ChangeMediaTypeSelect(this);");
                    selectMediaType.Items.Add(new ListItem(eResApp.GetRes(Pref, 8166), "0")); //Aucune valeur
                    foreach (eCatalog.CatalogValue catalogValue in _catalogMediaType)
                    {
                        selectMediaType.Items.Add(new ListItem(catalogValue.DisplayValue, catalogValue.Id.ToString()));
                    }
                    pnlFilterMediaType.Controls.Add(selectMediaType);


                    //Type de campagne
                    Panel pnlFilterCampaignType = new Panel();
                    pnlFilterCampaignType.ID = "FilterCampaignType";
                    pnlFilterCampaignType.CssClass = "FilterCampaignType";

                    pnlOtherFilterConsent.Controls.Add(pnlFilterCampaignType);
                    pnlFilterCampaignType.Controls.Add(new LiteralControl(string.Concat(eResApp.GetRes(Pref, 8713), " :"))); //Type de campagne

                    DropDownList selectCampaignType = new DropDownList();
                    selectCampaignType.ID = "invitSelectCampaignType";
                    selectCampaignType.CssClass = "invitSelectCampaignType";
                    selectCampaignType.Attributes.Add("onchange", "nsInvitWizard.ChangeCampaignTypeSelect(this);");
                    selectCampaignType.Items.Add(new ListItem(eResApp.GetRes(Pref, 8166), "0")); //Aucune valeur
                    pnlFilterCampaignType.Controls.Add(selectCampaignType);

                    //Type consentement
                    HtmlInputHidden hiddenTypeConsent = new HtmlInputHidden();
                    hiddenTypeConsent.ID = "invitHiddenTypeConsent";
                    hiddenTypeConsent.Value = _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.CONSENT].ToString();
                    pnlFilterCampaignType.Controls.Add(hiddenTypeConsent);

                    //Status Consentement
                    Panel pnlFilterConsentStatus = new Panel();
                    pnlFilterConsentStatus.ID = "FilterConsentStatus";
                    pnlFilterConsentStatus.CssClass = "FilterConsentStatus";

                    pnlOtherFilterConsent.Controls.Add(pnlFilterConsentStatus);

                    BuildConsentCheckBox(pnlFilterConsentStatus, "invitChbxOptin", _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTIN].ToString(), eResApp.GetRes(Pref, 8714)); //Opt-in
                    BuildConsentCheckBox(pnlFilterConsentStatus, "invitChbxNoopt", "noopt", eResApp.GetRes(Pref, 1845)); //Aucun consentement enregistré
                    BuildConsentCheckBox(pnlFilterConsentStatus, "invitChbxOptout", _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTOUT].ToString(), eResApp.GetRes(Pref, 8715)); //Opt-out                    
                }
            }

            return pnOtherFilter;
        }

        /// <summary>
        /// Génère une checkbox pour filter les consentements
        /// </summary>
        /// <param name="pnlParent">Panel parent</param>
        /// <param name="id">id de la checkbox</param>
        /// <param name="value">valeur de la checkbox</param>
        /// <param name="label">Libellé de la checkbox</param>
        /// <param name="hidden">Indique la checkbox est masquée</param>
        private void BuildConsentCheckBox(Panel pnlParent, string id, string value, string label, bool hidden = true)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");
            if (!string.IsNullOrEmpty(id))
                span.Attributes.Add("id", string.Concat(id, "Container"));
            string spanClass = "FilterConsentStatusChbx";
            if (hidden)
                spanClass = string.Concat(spanClass, " hidden");
            span.Attributes.Add("class", spanClass);
            pnlParent.Controls.Add(span);

            eCheckBoxCtrl chbx = new eCheckBoxCtrl(false, false);
            chbx.ID = id;
            chbx.Attributes.Add("value", value);
            chbx.AddClick("nsInvitWizard.ChangeFilters();");
            chbx.AddText(label);
            span.Controls.Add(chbx);
        }


        /// <summary>
        /// génère l'écran n°2
        /// </summary>
        /// <returns></returns>
        private Panel BuildSelectPPPanel()
        {
            Panel pnSelectPP = new Panel();
            pnSelectPP.ID = "invitWizardListDiv";
            pnSelectPP.Attributes.Add("edninvitdbl", "400");

            //SHA
            //Panel pour Fréquence et Paramétrer la planification
            Panel pnFrequencyPlanif = BuildFrequencyPanel();
            pnSelectPP.Controls.Add(pnFrequencyPlanif);

            Panel pnList = new Panel();
            pnList.ID = "PPList";
            pnSelectPP.Controls.Add(pnList);

            /*  Ajout des boutons de commande pour les filtres spécifique aux invitations (adresse active/principle/ grisé ligne */
            Panel pnOtherFilter = BuildFilterPPPanel();
            pnSelectPP.Controls.Add(pnOtherFilter);

            return pnSelectPP;
        }

        /// <summary>
        /// Construit la liste des contacts en fonction du filtre précédemment choisi
        /// pour intégration sur l'étape de sélection d'invitation sur un ++/xx
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="dicParam">Dictionnaire des paramètres à utiliser</param>
        /// <param name="sError">Message d'erreur éventuelle</param>
        /// <returns>render non typé contenant les différents panel</returns>
        public static eRenderer BuildPPList(ePref pref, ExtendedDictionary<string, string> dicParam, out string sError)
        {

            eRenderer eGlobalRender = eRenderer.CreateRenderer();

            sError = string.Empty;
            StringBuilder sbError = new StringBuilder();
            int nTab = 0;
            int nBkm = 0;
            int nTabFrom = 0;
            int nFilterId = 0;
            int height = 0;
            int width = 0;
            int nbRows = 0;
            int nPage = 0;
            int nParentEvtId = 0;
            int nFltTypeConsent = 0;
            int nFltCampaignType = 0;
            int nFltOptIn = 0;
            int nFltOptOut = 0;

            string sRootErr = "BuildPPList() - Paramètres invalides : ";


            if (dicParam == null)
            {
                sError = string.Concat(sRootErr, "dictionnaire manquant.");
                return null;
            }

            dicParam.TryGetValueConvert<int>("tab", out nTab);
            dicParam.TryGetValueConvert<int>("tabfrom", out nTabFrom);
            dicParam.TryGetValueConvert<int>("bkm", out nBkm);
            dicParam.TryGetValueConvert<int>("filterid", out nFilterId);

            if (nTab == 0 || nBkm == 0 || nFilterId == 0)
                sbError.Append(sRootErr);

            if (nTab == 0)
            {
                sbError.AppendLine("tab manquant");
            }
            if (nBkm == 0)
            {
                sbError.AppendLine("bkm manquant");
            }
            if (nFilterId == 0)
            {
                sbError.AppendLine("filter manquant");
            }


            if (sbError.Length > 0)
            {
                eGlobalRender.SetError(QueryErrorType.ERROR_NUM_FIELD_NOT_FOUND, sbError.ToString());
                return eGlobalRender;
            }

            dicParam.TryGetValueConvert<int>("height", out height);
            dicParam.TryGetValueConvert<int>("width", out width);
            dicParam.TryGetValueConvert<int>("rows", out nbRows);
            dicParam.TryGetValueConvert<int>("page", out nPage);
            dicParam.TryGetValueConvert<int>("parentevtid", out nParentEvtId);

            bool bFltActiv = dicParam.ContainsKey("fltact");
            bool bFltPrinc = dicParam.ContainsKey("fltprinc");
            bool bDelete = (dicParam.ContainsKey("delete") && dicParam["delete"].ToString() == "1");

            dicParam.TryGetValueConvert<int>("flttypeconsent", out nFltTypeConsent);
            dicParam.TryGetValueConvert<int>("fltcampaigntype", out nFltCampaignType);
            dicParam.TryGetValueConvert<int>("fltoptin", out nFltOptIn);
            dicParam.TryGetValueConvert<int>("fltoptout", out nFltOptOut);
            bool bFltNoOpt = dicParam.ContainsKey("fltnoopt") && dicParam["fltnoopt"].ToString() == "1";

            if (nPage < 1)
                nPage = 1;


            //Création objet LIST
            eList list = eListFactory.CreateListSel(pref, nTab, nBkm, nFilterId, nPage, nbRows, nTabFrom, nParentEvtId, bFltActiv, bFltPrinc, bDelete, nFltTypeConsent, nFltCampaignType, nFltOptIn, nFltOptOut, bFltNoOpt);
            if (list.ErrorMsg.Length > 0 || list.InnerException != null)
            {

                eGlobalRender.SetError(QueryErrorType.ERROR_NUM_FIELD_NOT_FOUND, list.ErrorMsg, list.InnerException);
                return eGlobalRender;

            }

            //--------------- MCR 38712 : debut ajout du nbre de destinataires du filtre en cours
            //Total Raws
            Panel pnCfm = new Panel();
            pnCfm.ID = "CfmDest";
            pnCfm.CssClass = "CfmDest";

            //pnlnbTotalRows.Controls.Add(new LiteralControl(string.Concat(list.NbTotalRows.ToString(), " destinatairesA ")));

            // #56 188 - Nombre de lignes par page personnalisable
            // Ajout du nombre de destinataires total pour ne pas autoriser la saisie d'un nombre de résultats par page supérieur à ce nombre
            HtmlInputHidden cfm = new HtmlInputHidden();
            cfm.ID = "cfm";
            cfm.Value = list.NbTotalRows.ToString();
            pnCfm.Controls.Add(cfm);

            string sLibCfm = string.Concat(eResApp.GetRes(pref, 6442).Replace("<NB>", cfm.Value));
            // pnCfm.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6442)));
            pnCfm.Controls.Add(new LiteralControl(sLibCfm));

            eGlobalRender.PgContainer.Controls.Add(pnCfm);

            //--------------- MCR 38712 : fin ajout du nbre de destinataires du filtre en cours



            // Barre de paging
            // TODO : check erreur sur CreateListSelPagingBar
            Panel pnPaggingBtnBar = eInvitWizardRenderer.CreateListSelPagingBar(nPage, nbRows, pref, list);
            eGlobalRender.PgContainer.Controls.Add(pnPaggingBtnBar);


            // Liste des PP
            eRenderer pnPPList = eRendererFactory.CreateSelectInvitRenderer(pref, list, nBkm, nFilterId, nPage, nbRows, height, width);
            if (pnPPList.ErrorMsg.Length > 0 || pnPPList.InnerException != null)
            {
                eGlobalRender.SetError(QueryErrorType.ERROR_NUM_FIELD_NOT_FOUND, pnPPList.ErrorMsg, pnPPList.InnerException);
                return eGlobalRender;
            }
            eGlobalRender.PgContainer.Controls.Add(pnPPList.PgContainer);


            return eGlobalRender;


        }

        /// <summary>
        /// Création des boutons pour le paging dans la liste des invitations
        /// 
        /// </summary>
        /// <returns></returns>
        private static Panel CreateListSelPagingBar(int nPage, int nbRows, ePref pref, eList list)
        {
            Panel pnTitle = new Panel();
            pnTitle.CssClass = "paggingFilterWizardList";
            pnTitle.ID = "GetLstSelPagging";

            // Paging activé
            HtmlGenericControl divFirst = new HtmlGenericControl("div");
            divFirst.Attributes.Add("Class", "icon-edn-first disable");

            int nbPage = list.NbPage;

            // #56 188 - Pas de numéro de page supérieur au nombre maximal possible
            if (nPage > nbPage)
                nPage = nbPage;

            int pageDprt = nPage;

            if (nPage > 1)
            {
                divFirst.Attributes.Add("onclick", string.Concat("UpdatePPList(1);"));
                divFirst.Attributes.Add("class", "icon-edn-first fRight");
            }
            else
            {
                divFirst.Attributes.Add("class", "icon-edn-first fRight disable");
            }


            HtmlGenericControl divPrev = new HtmlGenericControl("div");

            divPrev.Attributes.Add("Class", "icon-edn-prev fLeft disable");

            if (nPage > 1)
            {
                divPrev.Attributes.Add("onclick", string.Concat("UpdatePPList(", nPage - 1, ");"));
                divPrev.Attributes.Add("class", "icon-edn-prev fRight");

            }
            else
            {
                divPrev.Attributes.Add("class", "icon-edn-prev fRight disable");
            }


            HtmlGenericControl divNumPage = new HtmlGenericControl("div");
            divNumPage.Attributes.Add("class", "numpagePlsPls");
            divNumPage.Attributes.Add("align", "center");


            HtmlInputText inptNumPage = new HtmlInputText();
            divNumPage.Controls.Add(inptNumPage);
            inptNumPage.ID = "nP";
            inptNumPage.Attributes.Add("onchange", string.Concat("UpdatePPList(this.value);"));
            inptNumPage.Attributes.Add("onkeydown", string.Concat("if(event.ctrlKey){UpdatePPList(this.value)};"));
            // ="" 
            inptNumPage.Attributes.Add("class", "pagInput");
            // On dimensionne le champ de saisie en fonction du nombre maximal possibld
            inptNumPage.Size = nbPage.ToString().Length;
            inptNumPage.MaxLength = inptNumPage.Size;
            inptNumPage.Style[HtmlTextWriterStyle.Width] = string.Concat((13 * inptNumPage.Size).ToString(), "px");
            inptNumPage.Value = pageDprt.ToString();


            HtmlGenericControl divTotalPage = new HtmlGenericControl("div");
            divTotalPage.Attributes.Add("class", "NbPagePlsPls");


            Panel TotalPage = new Panel();
            //   divTotalPage.Controls.Add(TotalPage);
            //divTotalPage.ID = string.Concat("tNP_", _tab);

            Literal liNbPage = new Literal();
            liNbPage.Text = string.Concat("&nbsp;/&nbsp;", nbPage);
            divTotalPage.Controls.Add(liNbPage);


            HtmlGenericControl divNext = new HtmlGenericControl("div");
            if (pageDprt < nbPage)
            {
                //divNext.Attributes.Add("onclick", string.Concat("NextPage(", _list.ViewMainTable.DescId.ToString(), ",", inptNumPage.ID, ");"));
                divNext.Attributes.Add("onclick", string.Concat("UpdatePPList(", nPage + 1, ");"));
                divNext.Attributes.Add("class", "icon-edn-next fRight");
            }
            else
            {
                divNext.Attributes.Add("class", "icon-edn-next fRight disable");
            }

            HtmlGenericControl divLast = new HtmlGenericControl("div");
            if (pageDprt < nbPage)
            {
                divLast.Attributes.Add("onclick", string.Concat("UpdatePPList(", nbPage, ");"));
                divLast.Attributes.Add("class", "icon-edn-last fRight");
            }
            else
            {
                divLast.Attributes.Add("class", "icon-edn-last fRight disable");
            }

            // rajouter pour savoir qu elle est la derniere page caché
            HtmlInputText inptNbPage = new HtmlInputText();
            inptNbPage.Attributes.Add("type", "hidden");
            inptNbPage.ID = "nbP";
            inptNbPage.Value = nbPage.ToString();
            divLast.Controls.Add(inptNbPage);

            // #56 188 - Nombre de lignes par page personnalisable
            HtmlGenericControl divNbRows = new HtmlGenericControl("div");
            divNbRows.Attributes.Add("class", "nbrowsPlsPls");
            divNbRows.Attributes.Add("align", "center");

            // Proposition de TGE : limiter à 1000 lignes par page maximum
            int maxRowsAllowed = 1000;
            HtmlInputHidden hMaxRowsAllowed = new HtmlInputHidden();
            hMaxRowsAllowed.ID = "maxRowsAllowed";
            hMaxRowsAllowed.Name = hMaxRowsAllowed.ID;
            hMaxRowsAllowed.Value = maxRowsAllowed.ToString();
            divNbRows.Controls.Add(hMaxRowsAllowed);

            Literal liNbRows = new Literal();
            liNbRows.Text = string.Concat(" - ", eResApp.GetResWithColon(pref, 1338));
            divNbRows.Controls.Add(liNbRows);

            HtmlInputText inptNbRows = new HtmlInputText();
            divNbRows.Controls.Add(inptNbRows);
            inptNbRows.ID = "nR";
            inptNbRows.Attributes.Add("onchange", string.Concat("UpdatePPList(document.getElementById('nP').value, this.value);"));
            inptNbRows.Attributes.Add("onkeydown", string.Concat("if(event.ctrlKey){UpdatePPList(document.getElementById('nP').value, this.value)};"));
            // ="" 
            inptNbRows.Attributes.Add("class", "pagInput pagNbRowsInput");
            // On dimensionne le champ de saisie en fonction du nombre saisi
            inptNbRows.Size = list.NbTotalRows.ToString().Length;
            inptNbRows.MaxLength = inptNbRows.Size;
            inptNbRows.Style[HtmlTextWriterStyle.Width] = string.Concat((13 * inptNbRows.Size).ToString(), "px");

            inptNbRows.Value = nbRows.ToString();

            try
            {
                Panel pnSelFields = new Panel();
                pnSelFields.CssClass = "ivtFieldsSel icon-rubrique";
                pnSelFields.Attributes.Add("onclick", string.Concat("setIvtCol(", ((eListSel)list).InvitationBkm.ToString(), ")"));
                pnTitle.Controls.Add(pnSelFields);

                bool bMaxRowsModeEnabled = nbRows == list.NbTotalRows || nbRows == maxRowsAllowed;
                Panel pnMaxNbRows = new Panel();
                pnMaxNbRows.ID = "ivtMaxNbRows";
                pnMaxNbRows.CssClass = string.Concat("icon-eye ", (bMaxRowsModeEnabled ? "ivtMaxNbRowsActive" : "ivtMaxNbRows"));
                pnMaxNbRows.Attributes.Add("onclick", string.Concat("UpdatePPList(document.getElementById('nP').value, ", (bMaxRowsModeEnabled ? "'default'" : "document.getElementById('cfm').value"), ");"));
                pnMaxNbRows.ToolTip = bMaxRowsModeEnabled ? eResApp.GetRes(pref, 8254) : eResApp.GetRes(pref, 8253); // Afficher moins de lignes / Afficher plus de lignes
                pnTitle.Controls.Add(pnMaxNbRows);

            }
            catch
            {
                throw;
            }

            // comme on est en float:right il faut ajouter les élément de droite à gauche
            pnTitle.Controls.Add(divNbRows);
            pnTitle.Controls.Add(divLast);
            pnTitle.Controls.Add(divNext);
            pnTitle.Controls.Add(divTotalPage);
            pnTitle.Controls.Add(divNumPage);
            pnTitle.Controls.Add(divPrev);
            pnTitle.Controls.Add(divFirst);

            return pnTitle;
        }

        /// <summary>
        /// /// Javascript pour le wizard des selections d'invité
        /// </summary>
        /// <returns></returns>
        public override string GetInitJS()
        {


            string js = string.Concat(
            "   var oInvitWizard ;", Environment.NewLine,
            "   var iCurrentStep = 1;", Environment.NewLine,
            "   var htmlTemplate = null;", Environment.NewLine,
            "   var htmlHeader = null;", Environment.NewLine,
            "   var htmlFooter = null;", Environment.NewLine,
            "   var _activeFilter = 0;", Environment.NewLine,
            "   var iTotalSteps =", 2, " ;", Environment.NewLine,

            "   var _ePopupVNEditor;", Environment.NewLine,
            "   var _eFilterNameEditor;", Environment.NewLine,
            "   var _eReportNameEditor;", Environment.NewLine,
            "   var _activeFilter;", Environment.NewLine,
            "   var _activeFilterTab;", Environment.NewLine,
            "   var _nSelectedFilter;", Environment.NewLine,

              "   var _eCurentSelectedFilter = null;", Environment.NewLine,
              "   var _aFilterWizarBtns ; ", Environment.NewLine,

              " var oIframe;", Environment.NewLine,
            "   function OnDocLoad() { ", Environment.NewLine,
            "       oInvitWizard =  new eInvitWizard(", _tab, ",", _nTabFrom, ",", _nParentFileId, ");", Environment.NewLine,
            "       oInvitWizard.DeleteMode = ", _bIsDeleteMode ? "true" : "false", ";", Environment.NewLine,
            "       oInvitWizard.Target = ", _bIsTarget ? "1" : "0", ";", Environment.NewLine,
            "       oInvitWizard.TabName = '", _sTabName.Replace("\'", "\\'"), "';", Environment.NewLine,
            "       oInvitWizard.HasORM = ", CallOrm.GetOrmMetaInfos(_ePref, _ePref.User, new OrmGetParams() { CachePolicy = OrmMapCachePolicy.SESSION_CACHE_ONLY, ExceptionMode = OrmMappingExceptionMode.SAFE }).GetAllTriggerValidatorDescId.Contains(_tab) ? "1" : "0", Environment.NewLine,
            "       nGlobalActiveTab = ", EudoQuery.TableType.FILTER.GetHashCode(), ";", Environment.NewLine,
            "       initHeadEvents();", Environment.NewLine,
            "       initFilterList();", Environment.NewLine,
            "       Init('invit'); ", Environment.NewLine,
            "       _aFilterWizarBtns = getFilterWizardFromInvitBtns();", Environment.NewLine,
            "         oIframe = wizardIframe; ", Environment.NewLine

            );

            // Sélection initial du filtre "tous"
            if (_bIsDeleteMode)
            {
                js = string.Concat(js, Environment.NewLine,
                    "var oInitSelect = wizardIframe.contentDocument.querySelector(\"tr[eid='104000_-1']\");", Environment.NewLine,
                    "selectLine(oInitSelect)"
                    );
            }


            js = string.Concat(js, "        };", Environment.NewLine);


            return js;


        }

        #region Méthodes héritées de eRenderer, process de création du renderer






        #endregion




        #region OBSOLETE



        /// <summary>
        /// Retourne le wizardbody de l'étape 1 (cad la liste des filtres avec le pagging)
        /// OBSOLETE : La liste des filtres ne sera finalement pas pagginé
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nPage"></param>
        /// <param name="bDelete">Mode suppression du wizard des invitations</param>
        /// <returns></returns>
        public static eInvitWizardRenderer GetSelectInvitFilterRenderer(ePref ePref, int nTab, int nPage, int width, int height, bool bDelete)
        {
            eInvitWizardRenderer er = new eInvitWizardRenderer(ePref, nTab, 0, nPage, width, height, bDelete);

            //réinitialise la sélection d'invitation
            ePref.Context.InvitSelectId = new ExtendedDictionary<int, int>();
            Panel p = er.BuildFilterSelection();
            er._pgContainer = p;
            return er;
        }


        #endregion

    }
}