using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using Com.Eudonet.Xrm.classes;
using EudoQuery;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using System.Globalization;
using System.Linq;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page de tracking de l'application Eudonet
    /// Particularité : cette page est appelée depuis l'extérieur sans authentification !
    /// </summary>
    public partial class edn : eExternalPage<LoadQueryStringMailing>
    {
        /// <summary>
        /// Constante temporaire pour activer ou non les catégories de désinscription RGPD
        /// </summary>
        private bool _useNewUnsubscribeMethod = false;

        /// <summary>
        /// Titre désinscription
        /// </summary>
        protected string UnsubTitle { get; set; }

        /// <summary>
        /// Label bouton désinscription
        /// </summary>
        protected string UnsubButtonLabel { get; set; }

        /// <summary>
        /// Dictionnaire des valeurs de catalogues utilisées pour la désinscription
        /// </summary>
        private Dictionary<eTools.InteractionCommonCatalogValuesKeys, int> _dicUnsubscribeCommonCatalogValues;

        /// <summary>
        /// Identifiant de la campagne
        /// </summary>
        protected int _campaignId = 0;

        /// <summary>
        /// Identifiant de la 
        /// </summary>
        protected int _userLangId = -1;

        /// <summary>
        /// Css spécifique au panel
        /// </summary>
        public string Css { get; set; } = string.Empty;

        /// <summary>
        /// LangId de l'uilisateur connecté
        /// </summary>
        public int UserLangId
        {
            get
            {
                if (_userLangId != -1)
                    return _userLangId;
                else
                    return _pref.User.UserLangServerId;
            }
        }

        /// <summary>
        /// Type d'external page
        /// </summary>
        protected override eExternal.ExternalPageType PgTyp { get { return eExternal.ExternalPageType.TRACKING; } }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessPage()
        {
            #region ajout des css

            PageRegisters.AddCss("eTrack");

            #endregion

            #region ajout des js

            PageRegisters.AddScript("eTrack");

            #endregion

            ClientScript.GetPostBackEventReference(this, string.Empty);

            try
            {
                ExtPgTrace("Debut edn.aspx");

                List<Exception> listEx = new List<Exception>();

                string mailTabName = string.Empty;

                try
                {
                    // Chargement de  MailTabName / CampaignId
                    LoadInfosEdn(out mailTabName);
                }
                catch (Exception ex)
                {
                    listEx.Add(ex);
                }

                bool campaignExpired = false;

                StatsUpdate stats = GetStatsUpdate(mailTabName);

                string useNewUnsubscribeMethodValue = String.Empty;
                if (eLibTools.GetConfigAdvValues(_pref, new HashSet<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD }).TryGetValue(eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD, out useNewUnsubscribeMethodValue) && useNewUnsubscribeMethodValue == "1")
                    _useNewUnsubscribeMethod = true;

                // #28178 - Pour les confirmations de lecture, il n'y pas forcément de  campagne (mail unitaire)
                if (_campaignId == 0 && DataParam.ParamData.Category != MailingDataLinkCategory.READ)
                {
                    throw new TrackExp(string.Concat("Erreur de recherche de campage : CampaignId du mail n°", DataParam.ParamData.MailId, " non trouvé. "), false);
                }

                RequestBrowserInfo browInfo = new RequestBrowserInfo() { BrowserInfo = Request.Browser, UserAgent = Request.UserAgent };

                try
                {
                    // Mail Lu - Pour n'importe quel action, on enregistre la lecture du mail si elle n'est pas déjà faite
                    stats.UpdateRead(DataParam.ParamData, browInfo);
                }
                catch (Exception ex)
                {
                    listEx.Add(ex);
                }

                // On affiche le mail même si l'évenement est terminé
                // On permet la désinscription même si l'évenement est terminé
                if (DataParam.ParamData.Category != MailingDataLinkCategory.VISU
                    && DataParam.ParamData.Category != MailingDataLinkCategory.UNSUB
                    && DataParam.ParamData.Category != MailingDataLinkCategory.READ)
                {
                    try
                    {
                        if (stats.TrackIsExpired())
                        {
                            campaignExpired = true;

                            if (DataParam.ParamData.Category == MailingDataLinkCategory.LNK)
                            {
                                throw new StatsExp("Campage expiré - redirection directe vers l'url.", null);
                            }
                            else
                            {
                                _finishCmd.Add(delegate (HttpResponse resp)
                                {
                                    // Campagne Mail expiré
                                    resp.Write(eResApp.GetRes(GetLangServId(), 1774));
                                });
                            }
                        }
                    }
                    catch (StatsExp)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        listEx.Add(ex);
                    }
                }

                if (!campaignExpired)
                {


                    switch (DataParam.ParamData.Category)
                    {
                        case MailingDataLinkCategory.FORMU:
                            #region Lien formulaire
                            int nTplFileIdFormu = -1;
                            int nEvtFileIdFormu = -1;
                            ExternalUrlParamMailingTrackForm lnkFrm = (ExternalUrlParamMailingTrackForm)DataParam.ParamData;
                            // Informe la case à coché ou le compteur de la table main (Invit ++, PP, PM, etc.)
                            UpdateMainTable(lnkFrm, mailTabName, out nTplFileIdFormu, out nEvtFileIdFormu);

                            // CampaignStats & TrackMail & TrackMailLog
                            stats.UpdateTrackClick(lnkFrm, eLibTools.GetUserIPV4(Request));

                            // Rediriger vers Liens de la page de formulaire
                            _finishCmd.Add(delegate (HttpResponse resp)
                            {
                                resp.Redirect(GetLinkFormular(nEvtFileIdFormu, nTplFileIdFormu, lnkFrm.FormId), true);
                            });
                            break;
                        #endregion

                        case MailingDataLinkCategory.LNK:
                            #region Lien url
                            int nTplFileIdLnk = -1;
                            int nEvtFileIdLnk = -1;
                            ExternalUrlParamMailingTrackLink lnk = (ExternalUrlParamMailingTrackLink)DataParam.ParamData;

                            try
                            {
                                // Informe la case à coché ou le compteur de la table main (Invit ++, PP, PM, etc.)
                                UpdateMainTable(lnk, mailTabName, out nTplFileIdLnk, out nEvtFileIdLnk);

                                // CampaignStats & TrackMail & TrackMailLog
                                stats.UpdateTrackClick(lnk, eLibTools.GetUserIPV4(Request));
                            }
                            catch (Exception ex)
                            {
                                listEx.Add(ex);
                            }
                            // Redirection
                            _finishCmd.Add(delegate (HttpResponse resp)
                            {
                                if (Request.Headers != null && Request.Headers.Get("X-EXTERNALTRACKING") == "1")
                                {
                                    resp.Clear();
                                    resp.ClearContent();
                                    resp.ContentType = "text/plain";
                                    resp.Write("1");
                                    resp.End();

                                }
                                //BSE #49 729 Si le lien ne contient pas :// , on rajoute HTTP:// (vu la complexité du contrôle des protocoles,on gère que HTTP)
                                else if (lnk.Url.Contains("://"))
                                    resp.Redirect(lnk.Url, true);
                                else
                                    resp.Redirect(string.Concat("http://", lnk.Url), true);
                            });
                            break;
                        #endregion

                        case MailingDataLinkCategory.READ:
                            #region Image de lecture

                            ExternalUrlParam read = DataParam.ParamData;

                            // Retourne contenu vide de type image
                            _finishCmd.Add(delegate (HttpResponse resp)
                            {
                                if (Request.Headers != null && Request.Headers.Get("X-EXTERNALTRACKING") == "1")
                                {
                                    resp.Clear();
                                    resp.ClearContent();
                                    resp.ContentType = "text/plain";
                                    resp.Write("1");
                                    resp.End();

                                }
                                else
                                {
                                    resp.Clear();
                                    resp.ClearContent();
                                    resp.ClearHeaders();
                                    resp.ContentType = "image/jpeg";
                                }
                            });
                            break;
                        #endregion

                        case MailingDataLinkCategory.UNSUB:
                            #region Lien de désinscription
                            ExternalUrlParamMailing unsub = DataParam.ParamData;

                            //Chargement de la langue a afficher
                            List<eAdminLanguage> listLang = eAdminLanguage.Load(_pref, true);
                            LoadLang(listLang);

                            //Hack pour charger le catalogue dans la bonne langue
                            if (UserLangId < 10)
                                _pref.Lang = "LANG_0" + UserLangId.ToString();
                            else
                                _pref.Lang = "LANG_" + UserLangId.ToString();

                            LoadExternalInfosUnsub uInfo = GetLoadExternalInfosUnsub(mailTabName);

                            // Erreur silencieuse : on envoie un feedback mais on continue le traitement pour la mise à jour des stats
                            if (uInfo.SilentExp != null)
                                foreach (Exception infExp in uInfo.SilentExp)
                                    _msgFeedback.Add(eLibTools.GetExceptionMsg(infExp));

                            bool unsubscrib = _requestTools.AllKeys.Contains("re");

                            #region enregistrement de la désinscription

                            // Enregistre la désinscription
                            if (unsubscrib)
                            {
                                if (!_useNewUnsubscribeMethod)
                                {
                                    int categoryId = 0;
                                    if (uInfo.FldCampCategory != null
                                        && _requestTools.GetRequestFormKeyS("UnsubChoice") != null
                                        && _requestTools.GetRequestFormKeyS("UnsubChoice").ToLower() != "all")
                                        categoryId = eLibTools.GetNum(uInfo.FldCampCategory.Value);

                                    stats.UpdateUnsubValid(uInfo.FldMailAValue.Mail, categoryId);

                                    //Lance la csp personnalisé si elle existe
                                    try
                                    {
                                        StatsUpdate.LaunchCspManageBounce(_dalClient, uInfo.FldMailAValue.Mail, "UNSUBSCRIBE");
                                    }
                                    catch (Exception e)
                                    {
                                        listEx.Add(e);
                                    }
                                }
                                else
                                {
                                    if (_requestTools.GetRequestFormKeyS("UnsubChoiceChckBx") != null && (uInfo.PpId != 0 || !String.IsNullOrEmpty(uInfo.FldMailAValue.Mail)))
                                    {
                                        string sUnsubscribeCategories = _requestTools.GetRequestFormKeyS("UnsubChoiceChckBx");
                                        string[] aUnsubscribeCategories = sUnsubscribeCategories.Split(',');
                                        HashSet<int> listUnsubscribeCategories = new HashSet<int>();
                                        foreach (string sUnsubCategory in aUnsubscribeCategories)
                                        {
                                            int nUnsubCategory = 0;
                                            if (int.TryParse(sUnsubCategory, out nUnsubCategory))
                                                listUnsubscribeCategories.UnionWith(new HashSet<int> { nUnsubCategory });
                                        }

                                        List<eCatalog.CatalogValue> listCatMediaType = GetMediaTypes();
                                        List<eCatalog.CatalogValue> listCatCampaignType = GetUnsubscribeCategories();

                                        List<int> listAdrIds = new List<int>();

                                        if (DataParam.ParamData.ParamType != Common.Enumerations.EudonetMailingBuildParamType.EXTRANET)
                                            listAdrIds = GetAddressIds(uInfo.PpId, uInfo.FldMailAValue.Mail).Where(n => n != 0).ToList();
                                        else
                                            listAdrIds.Add(DataParam.ParamData.MailId);


                                        if (listAdrIds.Count == 0)
                                            listAdrIds.Add(0);

                                        Dictionary<int, OptListContainer> dicoAdrOpt = new Dictionary<int, OptListContainer>();
                                        foreach (int adrId in listAdrIds)
                                        {
                                            OptListContainer optListContainer = GetOptInOutCategories(uInfo.PpId, new List<int> { adrId });
                                            dicoAdrOpt.Add(adrId, optListContainer);
                                        }

                                        foreach (eCatalog.CatalogValue catMediaType in listCatMediaType)
                                        {
                                            if (catMediaType.IsDisabled)
                                                continue;

                                            List<eCatalog.CatalogValue> sublistCatCampaignType = listCatCampaignType.Where(c => c.ParentId == catMediaType.Id && !c.IsDisabled).ToList();
                                            if (sublistCatCampaignType.Count == 0)
                                                continue;

                                            foreach (eCatalog.CatalogValue catCampaignType in sublistCatCampaignType)
                                            {
                                                foreach (int adrId in listAdrIds)
                                                {
                                                    OptListContainer optListContainer = dicoAdrOpt[adrId];

                                                    bool archiveOptIn = false;

                                                    //Coché et non opt-in/déjà opt-out
                                                    if (listUnsubscribeCategories.Contains(catCampaignType.Id) &&
                                                        !optListContainer.ListOptIn.Contains(catCampaignType.Id))
                                                    {
                                                        //archivage opt-out
                                                        //creation opt-in
                                                        archiveOptIn = false;
                                                    }

                                                    //Décoché et non opt-out/déjà opt-in
                                                    else if (!listUnsubscribeCategories.Contains(catCampaignType.Id) &&
                                                        !optListContainer.ListOptOut.Contains(catCampaignType.Id))
                                                    {
                                                        //archivage opt-in
                                                        //creation opt-out
                                                        archiveOptIn = true;
                                                    }
                                                    //Coché et déjà opt-in, on ne fait rien
                                                    //Décoché et déjà opt-out, on ne fait rien
                                                    else
                                                    {
                                                        continue;
                                                    }

                                                    bool insertOptIn = !archiveOptIn;

                                                    string error;
                                                    int nbRow = ArchiveOptInOptOut(uInfo.PpId, adrId, catMediaType.Id, catCampaignType.Id, archiveOptIn, out error);
                                                    if (error.Length != 0)
                                                    {
                                                        _msgFeedback.Add(String.Format("Erreur lors de l'archivage des interactions : {0}", error));
                                                        continue;
                                                    }

                                                    if (!InsertOptInOptOut(uInfo.PpId, adrId, catMediaType.Id, catCampaignType.Id, insertOptIn, out error))
                                                    {
                                                        _msgFeedback.Add(String.Format("Erreur lors de la création des interactions : {0}", error));
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            // Proposition de désinsription ou Confirmation de la prise en compte de la demande
                            GenerateUnsub(uInfo, unsubscrib, listLang);
                            break;
                        #endregion

                        case MailingDataLinkCategory.VISU:
                            #region Lien de visu
                            ExternalUrlParamMailingVisu visu = (ExternalUrlParamMailingVisu)DataParam.ParamData;

                            // Clic sur le lien de visu
                            stats.UpdateViewClick();

                            // Construction du mail pour la visu
                            GenerateMail(visu, mailTabName);
                            break;
                            #endregion
                    }
                }

                if (listEx.Count > 0)
                {
                    foreach (var cEx in listEx)
                    {
                        string expMsg = eLibTools.GetExceptionMsg(cEx, 3);
                        _anError = true;
                        _msgFeedback.Add(expMsg);
                    }

                    // Permet la redirection même en cas d'erreur en annulant l'arrêt de la page
                    if (DataParam.ParamData.Category == MailingDataLinkCategory.LNK && _finishCmd?.Count > 0)
                        _anError = false;
                }
            }
            catch (StatsExp exp)
            {
                string expMsg = eLibTools.GetExceptionMsg(exp, 3);
                _anError = true;


                /* - En cas d'erreur, on redirige quand même vers le lien*/
                if (DataParam.ParamData.Category == MailingDataLinkCategory.LNK)
                {
                    // On annule donc l'arrêt de la page 
                    _anError = false;

                    ExternalUrlParamMailingTrackLink mylnk = (ExternalUrlParamMailingTrackLink)DataParam.ParamData;
                    _finishCmd.Add(delegate (HttpResponse resp)
                    {
                        if (Request.Headers != null && Request.Headers.Get("X-EXTERNALTRACKING") == "1")
                        {
                            resp.Clear();
                            resp.ClearContent();
                            resp.ContentType = "text/plain";
                            resp.Write("1");
                            resp.End();
                        }
                        //BSE #49 729 Si le lien ne contient pas :// , on rajoute HTTP:// (vu la complexité du contrôle des protocoles, on gère que HTTP)
                        else if (mylnk.Url.Contains("://"))
                            resp.Redirect(mylnk.Url, true);
                        else
                            resp.Redirect(string.Concat("http://", mylnk.Url), true);
                    });
                }
                else
                {
                    _msgFeedback.Add(expMsg);
                    ExtPgTrace(expMsg);
                }
            }
        }


        /// <summary>
        /// retourne un LoadExternalInfosUnsub pour les traitemetn de désinscrtiption
        /// </summary>
        /// <param name="mailTabName"></param>
        /// <returns></returns>
        protected virtual LoadExternalInfosUnsub GetLoadExternalInfosUnsub(string mailTabName)
        {
            return new LoadExternalInfosUnsub(_dalClient, _pref, DataParam.ParamData, mailTabName, _campaignId, _pref.User.UserLogin.StartsWith("EDN_"));
        }

        /// <summary>
        /// Retourne un objet pour la maj des informations de click
        /// </summary>
        /// <param name="sMailTabName"></param>
        /// <returns></returns>
        protected virtual StatsUpdate GetStatsUpdate(string sMailTabName)
        {
            return new StatsUpdate(_dalClient, _campaignId, _pref.User.UserId, sMailTabName);
        }

        /// <summary>
        /// Charge les tokens du tracking de la queryString
        /// </summary>
        protected override void LoadQueryString()
        {
            DataParam = new LoadQueryStringMailing(_pageQueryString.UID, _pageQueryString.Cs, _pageQueryString.P);
        }

        /// <summary>
        /// Chargement de  MailTabName / CampaignId
        /// </summary>
        /// <param name="mailTabName"></param>
        protected virtual void LoadInfosEdn(out string mailTabName)
        {
            string error = string.Empty;
            mailTabName = string.Empty;

            int mailTabDescId = DataParam.ParamData.MailTabDescId;
            int mailId = DataParam.ParamData.MailId;

            #region MailTabName

            mailTabName = eLibTools.GetTabNameFromDescId(_dalClient, mailTabDescId, out error);

            if (string.IsNullOrEmpty(mailTabName) || error.Length != 0)
                throw new TrackExp(string.Concat("Nom de la table email non trouvé. ", (error.Length != 0) ? error : string.Empty));

            #endregion

            #region CampaignId
            if (DataParam.ParamData.ParamType != Common.Enumerations.EudonetMailingBuildParamType.EXTRANET)
            {
                string sql = new StringBuilder()
                        .Append("SELECT isnull([TPL").Append(MailField.DESCID_MAIL_CAMPAIGNID.GetHashCode()).Append("], 0)")
                        .Append(" FROM [").Append(mailTabName).Append("]")
                        .Append(" WHERE [TplId] = @mailid")
                        .ToString();

                RqParam rq = new RqParam(sql);
                rq.AddInputParameter("@mailid", SqlDbType.Int, mailId);

                _campaignId = _dalClient.ExecuteScalar<int>(rq, out error);

                if (_campaignId == 0 && DataParam.ParamData.Category == MailingDataLinkCategory.READ)
                {
                    DataParam.IsUnitMail = true;
                    _campaignId = -1;
                    return;

                }

                if (error.Length != 0 || _campaignId == 0)
                    throw new TrackExp(string.Concat("Erreur de recherche de campage : CampaignId du mail n°", mailId, " non trouvé. ", (error.Length != 0) ? error : string.Empty), false);
            }
            else
            {
                DataParam.IsUnitMail = true;
                _campaignId = -1;
            }
            #endregion
        }


        /// <summary>
        /// Pour lien de visu
        /// </summary>
        /// <param name="visuParam"></param>
        /// <param name="mailTabName"></param>
        private void GenerateMail(ExternalUrlParamMailingVisu visuParam, string mailTabName)
        {
            RendType = ExternalPageRendType.TRACK_VISU;
            eUsrFrm usrFrm = new eUsrFrm(_pref, _dalClient, _pageQueryString.UID, _campaignId, eLibTools.GetAppUrl(Request));

            string css = string.Empty;
            string visuPnlInnerHtml = string.Empty;
            string pageTitle = string.Empty;
            usrFrm.GenerateMail(visuParam, mailTabName, out css, out visuPnlInnerHtml, out pageTitle);

            this.Css = css;
            visuPnl.InnerHtml = visuPnlInnerHtml;
            PageTitle = pageTitle;
        }


        /// <summary>
        /// Pour formulaire
        /// </summary>
        /// <param name="nParentFileId"></param>
        /// <param name="nTplFileId"></param>
        /// <param name="nFormId"></param>
        /// <returns></returns>
        private string GetLinkFormular(int nParentFileId, int nTplFileId, int nFormId)
        {
            FormularBuildParam fbp = new FormularBuildParam();
            fbp.Uid = _pageQueryString.UID;
            fbp.AppExternalUrl = eLibTools.GetAppUrl(Request);
            fbp.FormularId = nFormId;
            fbp.ParentFileId = nParentFileId;
            fbp.TplFileId = nTplFileId;
            return string.Concat(ExternalUrlTools.GetLinkFormular(fbp), ComplementaryRedirectUrl());
        }


        /// <summary>
        /// Méthode appelé sur un lien de type confirmation de lecture
        /// </summary>
        /// <param name="lnk"></param>
        protected virtual void UpdateRead(ExternalUrlParamMailingTrack lnk)
        {

            return;
        }


        /// <summary>
        /// Informe la case à cocher ou le compteur de la table main (Invit ++, PP, PM, etc.)
        /// </summary>
        /// <param name="lnk"></param>
        /// <param name="mailTabName"></param>
        /// <param name="nTplFileId"></param>
        /// <param name="nEvtFileId"></param>
        protected virtual void UpdateMainTable(ExternalUrlParamMailingTrack lnk, string mailTabName, out int nTplFileId, out int nEvtFileId)
        {
            nTplFileId = 0;
            nEvtFileId = 0;
            // Vérification de l'utilisateur
            if (_pref.User.UserLogin.StartsWith("EDN_"))
            {
                eUsrFrm usrFrm = new eUsrFrm(_pref, _dalClient, _pageQueryString.UID, _campaignId, eLibTools.GetAppUrl(Request));
                string sError = string.Empty;
                usrFrm.UpdateMainTable(lnk, mailTabName, out nTplFileId, out nEvtFileId, out sError);
                if (sError.Length > 0)
                    _msgFeedback.Add(sError);
            }
            else
            {
                _msgFeedback.Add("L'utilisateur qui met à jour doit être un utilisateur 'EDN_...'");
            }
        }


        /// <summary>
        /// Génération de la page de désinscription
        /// </summary>
        /// <param name="unsubInfos">Infos de la campagne</param>
        /// <param name="unsubscribeOk">Indique si l'utilisateur a cliqué sur se désinscrire</param>
        /// <param name="listLang">Liste des langues pour générer la dropdownlist</param>
        protected virtual void GenerateUnsub(LoadExternalInfosUnsub unsubInfos, bool unsubscribeOk, List<eAdminLanguage> listLang)
        {
            if (!_useNewUnsubscribeMethod)
            {
                PageTitle = eResApp.GetRes(UserLangId, 6595); // Désinscription
                UnsubTitle = eResApp.GetRes(UserLangId, 1829); //Confirmation de désinscription
                UnsubButtonLabel = eResApp.GetRes(UserLangId, 1787); //Se désinscrire 
            }
            else
            {
                PageTitle = eResApp.GetRes(UserLangId, 1847); //Paramètres d'abonnements
                UnsubTitle = eResApp.GetRes(UserLangId, 1848); //Vos abonnements
                UnsubButtonLabel = eResApp.GetRes(UserLangId, 28); //Valider
            }

            // Si le mail est vide, on indique à l'utilisateur qu'il ne peux se désinscrire
            if (unsubInfos.FldMailAValue == null || unsubInfos.FldMailAValue.Mail.Length == 0)
            {
                RendType = ExternalPageRendType.ERROR;
                // Votre mail n'a pas pu être trouvé. La désinscription ne peux être effectuée. Merci de contacter : 
                errPnl.InnerHtml = eResApp.GetRes(UserLangId, 1775);
                return;
            }

            string mailSyntaxeHtml = string.Concat("<span id='unsubMail' class='ednUnsubMail'>", HttpUtility.HtmlEncode(unsubInfos.FldMailAValue), "</span>");

            if (unsubscribeOk)
            {
                RendType = ExternalPageRendType.TRACK_UNSUB_VALID;

                HtmlGenericControl lbl = new HtmlGenericControl("label");
                if (!_useNewUnsubscribeMethod)
                {
                    // 1776 : Le désabonnement de votre mail <MAIL> a bien été pris en compte.
                    lbl.InnerHtml = eResApp.GetRes(UserLangId, 1776).Replace("<MAIL>", mailSyntaxeHtml);
                }
                else
                {
                    if (DataParam.ParamData.ParamType == Common.Enumerations.EudonetMailingBuildParamType.EXTRANET)
                    {
                        if (string.IsNullOrEmpty(unsubInfos.FldMailAValue.Mail) || unsubInfos.FldMailAValue.Mail == "-")
                            lbl.InnerHtml = eResApp.GetRes(UserLangId, 2955);
                        else
                            lbl.InnerHtml = eResApp.GetRes(UserLangId, 1849).Replace("<MAIL>", mailSyntaxeHtml);
                    }
                    else
                        lbl.InnerHtml = eResApp.GetRes(UserLangId, 1849).Replace("<MAIL>", mailSyntaxeHtml);
                }
                unsubValid.Controls.Add(lbl);
            }
            else
            {
                RendType = ExternalPageRendType.TRACK_UNSUB_CHOICE;

                if (!_useNewUnsubscribeMethod)
                {
                    HtmlGenericControl lbl = new HtmlGenericControl("label");
                    // 1777 : Votre adresse e-mail : <MAIL> va être désinscrite.
                    lbl.InnerHtml = eResApp.GetRes(UserLangId, 1777).Replace("<MAIL>", mailSyntaxeHtml);
                    unsubMsg.Controls.Add(lbl);

                    bool drawChoiceCategory = unsubInfos.FldCampCategory != null && unsubInfos.FldCampCategory.DisplayValue.Length != 0;

                    // Nous ajoutons un param pour indiquer le post du formulaire
                    unsubChoice.Controls.Add(new HtmlInputHidden() { ID = "re", Value = "ab" });

                    if (drawChoiceCategory)
                    {
                        HtmlGenericControl p;
                        string radioLbl = string.Empty;

                        p = new HtmlGenericControl("p");
                        unsubChoice.Controls.Add(p);
                        // 1780 : Choix de la désinscription :
                        p.Controls.Add(new Literal() { Text = eResApp.GetRes(UserLangId, 1780) });

                        string categorySyntaxeHtml = string.Concat("<span class='ednUnsubCat'>", unsubInfos.FldCampCategory.DisplayValue, "</span>");

                        p = new HtmlGenericControl("p");
                        p.Attributes.Add("class", "ednUnsubRad");
                        unsubChoice.Controls.Add(p);
                        // 1778 : Je voudrais me désinscrire des campagnes de type : 
                        radioLbl = string.Concat(eResApp.GetRes(UserLangId, 1778), " ", categorySyntaxeHtml);
                        AddUnsubRadioBtn(p, "unsubCategory", "Category", true, false, radioLbl);

                        p = new HtmlGenericControl("p");
                        p.Attributes.Add("class", "ednUnsubRad");
                        unsubChoice.Controls.Add(p);
                        // 1779 : Je souhaite me désinscrire de toutes les campagnes mail.
                        radioLbl = eResApp.GetRes(UserLangId, 1779);
                        AddUnsubRadioBtn(p, "unsubAll", "All", false, false, radioLbl);
                    }
                }
                else
                {
                    HtmlGenericControl lbl = new HtmlGenericControl("label");
                    //8696: Quel dommage de ne plus vous compter parmi nos destinataires. Si vous êtes sûr(e) de votre choix, choisissez de quelles communications vous souhaitez vous désabonner pour l'adresse <MAIL> :
                    lbl.InnerHtml = eResApp.GetRes(UserLangId, 8696).Replace("<MAIL>", mailSyntaxeHtml);

                    if (DataParam.ParamData.ParamType == Common.Enumerations.EudonetMailingBuildParamType.EXTRANET)
                    {
                        if (string.IsNullOrEmpty(unsubInfos.FldMailAValue.Mail) || unsubInfos.FldMailAValue.Mail == "-")
                            lbl.InnerHtml = eResApp.GetRes(UserLangId, 2954);
                        else
                            lbl.InnerHtml = eResApp.GetRes(UserLangId, 8696).Replace("<MAIL>", mailSyntaxeHtml);
                    }
                    else
                        lbl.InnerHtml = eResApp.GetRes(UserLangId, 8696).Replace("<MAIL>", mailSyntaxeHtml);

                    unsubMsg.Controls.Add(lbl);

                    // Nous ajoutons un param pour indiquer le post du formulaire
                    unsubChoice.Controls.Add(new HtmlInputHidden() { ID = "re", Value = "ab" });

                    HtmlGenericControl p;
                    string radioLbl = string.Empty;

                    p = new HtmlGenericControl("p");
                    unsubChoice.Controls.Add(p);

                    if (unsubInfos.PpId != 0 || !String.IsNullOrEmpty(unsubInfos.FldMailAValue.Mail))
                    {
                        List<eCatalog.CatalogValue> listCatMediaType = GetMediaTypes();
                        List<eCatalog.CatalogValue> listCatCampaignType = GetUnsubscribeCategories();
                        List<int> listAdrId = GetAddressIds(unsubInfos.PpId, unsubInfos.FldMailAValue.Mail);
                        OptListContainer listOptCategories = GetOptInOutCategories(unsubInfos.PpId, listAdrId);

                        foreach (eCatalog.CatalogValue catMediaType in listCatMediaType)
                        {
                            if (catMediaType.IsDisabled)
                                continue;

                            List<eCatalog.CatalogValue> sublistCatCampaignType = listCatCampaignType.Where(c => c.ParentId == catMediaType.Id && !c.IsDisabled).ToList();
                            if (sublistCatCampaignType.Count == 0)
                                continue;

                            p = new HtmlGenericControl("p");
                            p.InnerText = catMediaType.DisplayValue;
                            unsubChoice.Controls.Add(p);

                            foreach (eCatalog.CatalogValue catCampaignType in sublistCatCampaignType)
                            {
                                p = new HtmlGenericControl("p");
                                p.Attributes.Add("class", "ednUnsubRad");
                                unsubChoice.Controls.Add(p);
                                string id = String.Concat("unsub", catCampaignType.Id.ToString());
                                string value = catCampaignType.Id.ToString();
                                bool isChecked = listOptCategories.ListOptIn.Contains(catCampaignType.Id);
                                AddUnsubChckBx(p, id, value, catCampaignType.DisplayValue, isChecked);
                            }
                        }

                        //Se désinscrire de toutes les campagnes
                        {
                            p = new HtmlGenericControl("p");
                            p.Attributes.Add("class", "ednUnsubAllRad");
                            unsubChoice.Controls.Add(p);
                            string id = String.Concat("unsubAll");
                            string value = "all";
                            AddUnsubChckBx(p, id, value, eResApp.GetRes(UserLangId, 1835));
                        }
                    }
                }

                //Creation dropdown selection langue, alimentation et selection de la langue navigateur
                //HtmlSelect selectLang = new HtmlSelect();
                //selectLang.ID = "selectLang";
                selectLang.Items.Clear();
                selectLang.Attributes.Add("onchange", "reloadLang();");
                ListItem item = null;
                bool selected = false;

                foreach (eAdminLanguage kv in listLang)
                {
                    item = new ListItem(kv.Label, kv.Id.ToString());
                    selectLang.Items.Add(item);
                    if (kv.SysId == UserLangId && !selected)
                    {
                        item.Selected = true;
                        selected = true;
                    }
                }

                unsubLang.Controls.Add(selectLang);
            }
        }

        private void LoadLang(List<eAdminLanguage> listLang)
        {
            //On recupere la langue navigateur et on recherche la correspondance avec le syslang si premier chargement
            if (!IsPostBack)
            {
                CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentUICulture;

                //Si correspondance trouvé on recupere le syslang pour l'affichage, sinon on charge la langue par defaut défini dans le maplang
                if (eLibConst.CultureInfoToSysLang.ContainsKey(ci.Name) && listLang.Exists(item => item.SysId == eLibConst.CultureInfoToSysLang[ci.Name].GetHashCode()))
                {
                    _userLangId = eLibConst.CultureInfoToSysLang[ci.Name].GetHashCode();
                }
                else
                {
                    //Langue par defaut du map lang
                    _userLangId = listLang.Where(item => item.IsDefault).FirstOrDefault().SysId;
                }
            }
            else //Dans le cas d'un postback sur changement de la ddl ou alors submit
            {
                if (string.IsNullOrEmpty(Request["__EVENTARGUMENT"]))
                {
                    _userLangId = Int32.Parse(Session["_userLangId"].ToString());
                }
                else //Cas du changement de ddl
                {
                    _userLangId = Int32.Parse(Request["__EVENTARGUMENT"]);

                }
            }
            Session["_userLangId"] = _userLangId;
        }

        #region Boîte à outils

        private void AddUnsubRadioBtn(Control parent, string id, string value, bool isChecked, bool isDisabled, string label)
        {
            HtmlInputRadioButton radioBtn = new HtmlInputRadioButton();
            radioBtn.Name = "UnsubChoice";
            radioBtn.Value = value;
            radioBtn.ID = id;
            radioBtn.Checked = isChecked;
            radioBtn.Disabled = isDisabled;
            parent.Controls.Add(radioBtn);

            HtmlGenericControl lblLibRadio = new HtmlGenericControl("label");
            lblLibRadio.InnerHtml = label;
            lblLibRadio.Attributes.Add("for", id);
            parent.Controls.Add(lblLibRadio);
        }

        private void AddUnsubChckBx(Control parent, string id, string value, string label, bool isChecked = false)
        {
            HtmlGenericControl checkBox = new HtmlGenericControl("input");
            checkBox.Attributes.Add("type", "checkbox");
            checkBox.Attributes.Add("id", id);
            checkBox.Attributes.Add("name", "UnsubChoiceChckBx");
            checkBox.Attributes.Add("value", value);
            if (isChecked)
                checkBox.Attributes.Add("checked", "checked");
            checkBox.Attributes.Add("onchange", "nsUnsub.ToggleUnsubChoiceChckBx(this);");
            parent.Controls.Add(checkBox);

            HtmlGenericControl lblLibChckBx = new HtmlGenericControl("label");
            lblLibChckBx.InnerHtml = label;
            lblLibChckBx.Attributes.Add("for", id);
            parent.Controls.Add(lblLibChckBx);
        }

        #endregion

        #region Catégories de désinscription
        /// <summary>
        /// Récupère les valeurs du catalogue Interaction.Type de média
        /// </summary>
        /// <returns></returns>
        private List<eCatalog.CatalogValue> GetMediaTypes()
        {
            FieldLite typeMediaField = null;

            IEnumerable<FieldLite> listFields = RetrieveFields.GetEmpty(null)
                                            .AddOnlyThisTabs(new int[] { (int)TableType.INTERACTION })
                                            .AddOnlyThisDescIds(new int[] { (int)InteractionField.MediaType })
                                            .SetExternalDal(_dalClient)
                                            .ResultFieldsInfo(FieldLite.Factory());

            if (listFields.Any())
                typeMediaField = listFields.First();

            if (typeMediaField == null)
                throw new Exception("GetMediaTypes error : impossible de récupérer les infos de la rubrique Type de Média");

            eCatalog _catalog = new eCatalog(_dalClient, _pref, typeMediaField.Popup, _pref.User, typeMediaField.PopupDescId, langId: _userLangId);
            return _catalog.Values;
        }

        /// <summary>
        /// Récupère les valeurs du catalogue Interaction.Type de campagne
        /// </summary>
        /// <returns></returns>
        private List<eCatalog.CatalogValue> GetUnsubscribeCategories()
        {
            FieldLite typeCampaignField = null;

            IEnumerable<FieldLite> listFields = RetrieveFields.GetEmpty(null)
                                            .AddOnlyThisTabs(new int[] { (int)TableType.INTERACTION })
                                            .AddOnlyThisDescIds(new int[] { (int)InteractionField.TypeCampaign })
                                            .SetExternalDal(_dalClient)
                                            .ResultFieldsInfo(FieldLite.Factory());

            if (listFields.Any())
                typeCampaignField = listFields.First();

            if (typeCampaignField == null)
                throw new Exception("GetUnsubscribeCategories error : impossible de récupérer les infos de la rubrique Type de Campagne");

            eCatalog _catalog = new eCatalog(_dalClient, _pref, typeCampaignField.Popup, _pref.User, typeCampaignField.PopupDescId, langId: _userLangId);
            return _catalog.Values;
        }

        /// <summary>
        /// Initialise le dictionnaire _dicUnsubscribeCommonCatalogValues
        /// </summary>
        private void InitDicoUnsubscribeCommonCatalogValues()
        {
            _dicUnsubscribeCommonCatalogValues = eTools.GetUnsubscribeCommonCatalogValues(_pref);
        }

        /// <summary>
        /// Classe pour englober les catégories des consentements (opt-in et opt-out)
        /// </summary>
        private class OptListContainer
        {
            /// <summary>Liste des catégories en opt-in</summary>
            public HashSet<int> ListOptIn { get; set; }
            /// <summary>Liste des catégories en opt-out</summary>
            public HashSet<int> ListOptOut { get; set; }

            /// <summary>
            /// Constructeur par défaut
            /// </summary>
            public OptListContainer()
            {
                ListOptIn = new HashSet<int>();
                ListOptOut = new HashSet<int>();
            }
        }

        /// <summary>
        /// Récupère les id du catalogues Interaction.Type de campagne des fiches Interactions au statut "Opt-in" et "Opt-Out" relié à des fiches PP/ADDRESS ayant comme l'email passé en paramètre
        /// </summary>
        /// <param name="ppId">id fiche parente PP</param>
        /// <param name="listAdrId">liste id fiches parentes ADDRESS</param>
        /// <returns></returns>
        private OptListContainer GetOptInOutCategories(int ppId, IEnumerable<int> listAdrId)
        {
            OptListContainer listContainer = new OptListContainer();

            //On récupére les valeurs des catalogues pour les filtres
            if (_dicUnsubscribeCommonCatalogValues == null)
                InitDicoUnsubscribeCommonCatalogValues();

            //On écupérer les fiches Interaction de type opt-in non archivées
            eDataFillerGeneric filler = new eDataFillerGeneric(_pref, (int)TableType.INTERACTION, ViewQuery.CUSTOM);
            filler.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
            {
                //Liste des colonnes                
                HashSet<string> listCols = new HashSet<string>() {
                    ((int)InteractionField.Type).ToString()
                    ,((int)InteractionField.StatusConsent).ToString()
                    ,((int)InteractionField.TypeCampaign).ToString()
                };
                eq.SetListCol = String.Join(";", listCols);

                //Filtres Parents
                //PP
                List<WhereCustom> listFiltersParents = new List<WhereCustom>();
                WhereCustom wcFilterPP = new WhereCustom("PPID", Operator.OP_EQUAL, ppId.ToString(), InterOperator.OP_AND);
                listFiltersParents.Add(wcFilterPP);

                //ADR
                List<WhereCustom> listFiltersADR = new List<WhereCustom>();
                foreach (int adrId in listAdrId.Where(n => n != 0))
                {
                    WhereCustom wcFilterADR = new WhereCustom("ADRID", Operator.OP_EQUAL, adrId.ToString(), InterOperator.OP_OR);
                    listFiltersADR.Add(wcFilterADR);
                }
                if (listFiltersADR.Count == 0)
                {
                    WhereCustom wcFilterADR = new WhereCustom("ADRID", Operator.OP_IS_EMPTY, "", InterOperator.OP_OR);
                    listFiltersADR.Add(wcFilterADR);
                }
                WhereCustom wcFiltersADR = new WhereCustom(listFiltersADR, InterOperator.OP_AND);
                listFiltersParents.Add(wcFiltersADR);

                //Préparation des filtres
                List<WhereCustom> listFilters = new List<WhereCustom>();

                WhereCustom filterArchived = new WhereCustom(((int)InteractionField.Archive).ToString(), Operator.OP_DIFFERENT, "1", InterOperator.OP_AND);
                listFilters.Add(filterArchived);

                WhereCustom filterConsent = new WhereCustom(((int)InteractionField.Type).ToString(), Operator.OP_EQUAL, _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.CONSENT].ToString(), InterOperator.OP_AND);
                listFilters.Add(filterConsent);

                WhereCustom filtersParents = new WhereCustom(listFiltersParents, InterOperator.OP_AND);
                listFilters.Add(filtersParents);

                eq.AddCustomFilter(new WhereCustom(listFilters));
            };

            filler.Generate();

            if (filler.ErrorMsg.Length != 0 || filler.InnerException != null)
                throw new Exception(String.Concat("GetOptInOutCategories : ", filler.ErrorMsg, filler.InnerException == null ? String.Empty : filler.InnerException.Message));

            Field fldTypeCampaign = filler.FldFieldsInfos.Find(f => f.Descid == (int)InteractionField.TypeCampaign);
            Field fldStatusConsent = filler.FldFieldsInfos.Find(f => f.Descid == (int)InteractionField.StatusConsent);

            //On récupére les type de campagne des fiches interactions
            List<int> listTypeCampain = new List<int>();
            foreach (eRecord rec in filler.ListRecords)
            {
                string sTypeValue = rec.GetFieldByAlias(fldTypeCampaign.Alias).Value;
                string sStatusValue = rec.GetFieldByAlias(fldStatusConsent.Alias).Value;
                int nTypeValue = 0;
                int nStatusValue = 0;
                if (Int32.TryParse(sTypeValue, out nTypeValue) && Int32.TryParse(sStatusValue, out nStatusValue))
                {
                    if (nStatusValue == _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTIN])
                        listContainer.ListOptIn.UnionWith(new HashSet<int> { nTypeValue });
                    else if (nStatusValue == _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTOUT])
                        listContainer.ListOptOut.UnionWith(new HashSet<int> { nTypeValue });
                }
            }

            return listContainer;
        }

        /// <summary>
        /// Récupère les id des adresses ayant un champ email identique à la valeur passée en paramètre
        /// </summary>
        /// <param name="ppId">Id PP</param>
        /// <param name="email">valeur email recherchée</param>
        /// <returns></returns>
        private List<int> GetAddressIds(int ppId, string email)
        {
            List<int> listAdrIds = new List<int>();

            IEnumerable<eFieldLiteWithLib> listEmailFields = RetrieveFields.GetDefault(_pref)
                .AddOnlyThisTabs(new int[] { (int)TableType.ADR })
                .AddOnlyThisFormats(new FieldFormat[] { FieldFormat.TYP_EMAIL })
                .ResultFieldsInfo(eFieldLiteWithLib.Factory(_pref));

            if (listEmailFields.Count() > 0)
            {
                eDataFillerGeneric filler = new eDataFillerGeneric(_pref, (int)TableType.ADR, ViewQuery.CUSTOM);
                filler.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
                {

                    //Liste des colonnes                
                    HashSet<string> listCols = new HashSet<string>() {
                        ((int)PPField.NOM).ToString()
                        ,((int)AdrField.PERSO).ToString()
                        };
                    eq.SetListCol = String.Join(";", listCols);

                    //Préparation des filtres
                    List<WhereCustom> listFilters = new List<WhereCustom>();

                    WhereCustom wcFiltersParents = new WhereCustom("PPID", Operator.OP_EQUAL, ppId.ToString(), InterOperator.OP_AND);
                    listFilters.Add(wcFiltersParents);

                    List<WhereCustom> listFiltersEmail = new List<WhereCustom>();
                    foreach (eFieldLiteWithLib emailField in listEmailFields)
                    {
                        WhereCustom wcFilterEmail = new WhereCustom(emailField.Descid.ToString(), Operator.OP_EQUAL, email, InterOperator.OP_OR);
                        listFiltersEmail.Add(wcFilterEmail);
                    }
                    WhereCustom wcFiltersEmail = new WhereCustom(listFiltersEmail, InterOperator.OP_AND);
                    listFilters.Add(wcFiltersEmail);

                    eq.AddCustomFilter(new WhereCustom(listFilters));
                };

                filler.Generate();

                if (filler.ErrorMsg.Length != 0 || filler.InnerException != null)
                    throw new Exception(String.Concat("GetAddressIds : ", filler.ErrorMsg, filler.InnerException == null ? String.Empty : filler.InnerException.Message));

                //On récupére les id des adresses
                foreach (eRecord rec in filler.ListRecords)
                {
                    if (rec.MainFileid != 0 && !listAdrIds.Contains(rec.MainFileid))
                        listAdrIds.Add(rec.MainFileid);
                }
            }

            return listAdrIds;
        }

        /// <summary>
        /// Archive les fiches Interactions OptIn
        /// </summary>
        /// <param name="ppId">Id PP</param>
        /// <param name="adrId">Id ADDRESS</param>
        /// <param name="mediaType">Type de média de la campagne</param>
        /// <param name="campaignType">Catégorie de la campagne</param>
        /// <param name="archiveOptIn">Archive les opt-in si true, Archive les opt-out si false</param>
        /// <param name="sError">erreur</param>
        /// <returns></returns>
        private int ArchiveOptInOptOut(int ppId, int adrId, int mediaType, int campaignType, bool archiveOptIn, out string sError)
        {
            sError = String.Empty;

            if ((ppId == 0 && adrId == 0) || mediaType == 0 || campaignType == 0)
            {
                sError = "Archivage impossible, des informations sont manquantes";
                return 0;
            }

            //On récupére les valeurs des catalogues pour les filtres
            try
            {
                if (_dicUnsubscribeCommonCatalogValues == null)
                    InitDicoUnsubscribeCommonCatalogValues();
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                return 0;
            }

            /*
            UPDATE [INTERACTION]
            SET [TPL04] = 1
            WHERE isnull([TPL04], 0) <> 1
            AND [TPL06] = @catalogValueConsent
            AND [TPL08] = @catalogValueStatus
            AND [TPL17] = @mediaType
            AND [TPL09] = @campaignType
            AND isnull([Ppid], 0) = @ppId
            AND isnull([AdrId], 0) = @adrId
            */

            StringBuilder sql = new StringBuilder()
                    .Append("UPDATE [INTERACTION]")
                    .Append(" SET [TPL").Append(GetFieldIdString((int)InteractionField.Archive)).Append("] = @Archiver")
                    .Append(" WHERE isnull([TPL").Append(GetFieldIdString((int)InteractionField.Archive)).Append("], 0) <> @Archiver")
                    .Append(" AND isnull([Ppid], 0) = @ppId")
                    .Append(" AND isnull([AdrId], 0) = @adrId")
                    .Append(" AND [TPL").Append(GetFieldIdString((int)InteractionField.Type)).Append("] = @catalogValueConsent")
                    .Append(" AND [TPL").Append(GetFieldIdString((int)InteractionField.StatusConsent)).Append("] = @catalogValueStatus")
                    .Append(" AND [TPL").Append(GetFieldIdString((int)InteractionField.MediaType)).Append("] = @mediaType")
                    .Append(" AND [TPL").Append(GetFieldIdString((int)InteractionField.TypeCampaign)).Append("] = @campaignType");


            RqParam rq = new RqParam(sql.ToString());
            rq.AddInputParameter("@Archiver", SqlDbType.Bit, true);
            rq.AddInputParameter("@ppId", SqlDbType.Int, ppId);
            rq.AddInputParameter("@adrId", SqlDbType.Int, adrId);
            rq.AddInputParameter("@catalogValueConsent", SqlDbType.Int, _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.CONSENT]);
            int statusOpt = _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTOUT];
            if (archiveOptIn)
                statusOpt = _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTIN];
            rq.AddInputParameter("@catalogValueStatus", SqlDbType.Int, statusOpt);
            rq.AddInputParameter("@mediaType", SqlDbType.Int, mediaType);
            rq.AddInputParameter("@campaignType", SqlDbType.Int, campaignType);

            return _dalClient.ExecuteNonQuery(rq, out sError);
        }

        /// <summary>
        /// Retourne le numero d'un champ sous la forme 00
        /// </summary>
        /// <param name="descid">DescId du champ</param>
        /// <returns></returns>
        private string GetFieldIdString(int descid)
        {
            return (descid % 100).ToString("00");
        }

        /// <summary>
        /// Crée une fiche de désinscription (table Interaction, type : consentement, opt-out)
        /// </summary>
        /// <param name="ppId">Id PP</param>
        /// <param name="adrId">Id ADDRESS</param>
        /// <param name="mediaType">Type de média de la campagne</param>
        /// <param name="campaignType">Catégorie de la campagne</param>
        /// <param name="insertOptIn">Crée les opt-in si true, Crée les opt-out si false</param>
        /// <param name="sError">erreur</param>
        /// <returns></returns>
        private bool InsertOptInOptOut(int ppId, int adrId, int mediaType, int campaignType, bool insertOptIn, out string sError)
        {
            sError = String.Empty;

            if ((ppId == 0 && adrId == 0) || mediaType == 0 || campaignType == 0)
            {
                sError = "Création impossible, des informations sont manquantes";
                return false;
            }

            //On récupére les valeurs des catalogues pour les filtres
            try
            {
                if (_dicUnsubscribeCommonCatalogValues == null)
                    InitDicoUnsubscribeCommonCatalogValues();
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                return false;
            }


            Engine.Engine eng = eModelTools.GetEngine(_pref, (int)TableType.INTERACTION, eEngineCallContext.GetCallContext(ppId > 0 ? EngineContext.FORMULAR_REGISTRED : EngineContext.FORMULAR_ANONYMOUS));

            if (ppId != 0)
                eng.AddTabValue((int)TableType.PP, ppId);

            if (adrId != 0)
                eng.AddTabValue((int)TableType.ADR, adrId);

            eng.AddNewValue((int)InteractionField.Date, DateTime.Now.Date.ToString());
            eng.AddNewValue((int)InteractionField.Type, _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.CONSENT].ToString());
            eng.AddNewValue((int)InteractionField.MediaType, mediaType.ToString());
            eng.AddNewValue((int)InteractionField.TypeCampaign, campaignType.ToString());
            eng.AddNewValue((int)InteractionField.ConsentObtainedBy, _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.WEBFORM].ToString());

            int statusOpt = _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTOUT];
            if (insertOptIn)
                statusOpt = _dicUnsubscribeCommonCatalogValues[eTools.InteractionCommonCatalogValuesKeys.OPTIN];
            eng.AddNewValue((int)InteractionField.StatusConsent, statusOpt.ToString());

            eng.EngineProcess(new StrategyCruSimple());
            EngineResult result = eng.Result;

            if (!result.Success)
            {
                StringBuilder sb = new StringBuilder("InsertOptInOptOut error: ");
                if (result.Error.IsSet)
                    sb.Append(result.Error);
                else
                    sb.AppendLine("Error inconnu !");

                sError = sb.ToString();

                return false;
            }

            return true;
        }

        #endregion
    }
}