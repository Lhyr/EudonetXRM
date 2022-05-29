using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminRightMgr
    /// </summary>
    public class eAdminRightDialogMgr : eAdminManager
    {

        /// <summary>
        /// Représentation d'une demande d'actualisation de filtre
        /// </summary>
        [DataContract]
        public class RefreshFilters
        {
            [DataMember]
            public List<Pair> FromTabs;

            [DataMember]
            public List<Pair> Fields;

            /// <summary>
            /// Raz de la liste des filtre
            /// </summary>
            public RefreshFilters()
            {
                FromTabs = new List<Pair>();
                Fields = new List<Pair>();
            }

            public void AddField(int id, string text)
            {
                Fields.Add(new Pair(id, text));
            }

            public void AddFromTab(int id, string text)
            {
                FromTabs.Add(new Pair(id, text));
            }

        }

        [DataContract]
        public class Pair
        {
            [DataMember]
            public Int32 Id;
            [DataMember]
            public String Text;

            public Pair(int id, string text)
            {
                Id = id;
                Text = text;
            }
        }

        enum ACTION
        {
            RefreshList = 0,
            RefreshFilters = 1
        }

        int _tab = 0;

        /// <summary>
        /// Gestion de la demande => création du renderer aproprié
        /// </summary>
        protected override void ProcessManager()
        {
            ACTION action = ACTION.RefreshList;
            int nLevel = 0, nFrom = 0, nField = 0;

            // Pour l'enregistrement des droits sur une fiche (page d'accueil xrm ou grille)
            int nPageId = 0, nGridId = 0;

            String sUser = "", sFunction = "";
            HashSet<eTreatmentType> hsTreatTypes = new HashSet<eTreatmentType>();
            List<eTreatmentType> lstTreatTypes = new List<eTreatmentType>();

            _tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            nFrom = _requestTools.GetRequestFormKeyI("from") ?? 0;
            nField = _requestTools.GetRequestFormKeyI("field") ?? 0;
            sFunction = _requestTools.GetRequestFormKeyS("fct") ?? "";

            // [dev #55029] droits sur la page d'accueil 
            nPageId = _requestTools.GetRequestFormKeyI("pageid") ?? 0;

            // [dev #55029] droits sur la grille
            nGridId = _requestTools.GetRequestFormKeyI("gridid") ?? 0;

            // Si Tous a été séléctionné
            bool bWithAllTypes = false;
            if (_requestTools.AllKeys.Contains("types") && !String.IsNullOrEmpty(_context.Request.Form["types"]))
            {
                String[] sTypes = _requestTools.GetRequestFormKeyS("types").ToString().Split(';');

                foreach (String s in sTypes)
                {
                    Int32 i = 0;
                    if (!Int32.TryParse(s, out i))
                    {
                        continue;
                    }

                    if (i >= UserLevel.LEV_USR_ADMIN.GetHashCode()) //Options tous les niveaux
                    {
                        bWithAllTypes = true;
                        continue;
                    }

                    eTreatmentType treatType;
                    if (!Enum.TryParse<eTreatmentType>(i.ToString(), out treatType))
                        continue;

                    hsTreatTypes.Add(treatType);
                }
            }

            if (_requestTools.AllKeys.Contains("action"))
                action = _requestTools.GetRequestFormEnum<ACTION>("action");



            if (action == ACTION.RefreshList)
            {
                if (_requestTools.AllKeys.Contains("level") && !String.IsNullOrEmpty(_context.Request.Form["level"]))
                    Int32.TryParse(_context.Request.Form["level"].ToString(), out nLevel);
                if (_requestTools.AllKeys.Contains("users") && !String.IsNullOrEmpty(_context.Request.Form["users"]))
                    sUser = _context.Request.Form["users"].ToString();


                eAdminRightsRenderer renderer = null;
                try
                {
                    ePermission perm = null;
                    // Soit on filtre sur niveau, soit on filtre sur utilisateur
                    if (nLevel > 0 && String.IsNullOrEmpty(sUser))
                    {
                        perm = new ePermission(0, ePermission.PermissionMode.MODE_LEVEL_ONLY, nLevel, sUser);
                        perm.SetPermMode();
                    }
                    else if (nLevel <= 0 && !String.IsNullOrEmpty(sUser))
                    {
                        if (!sUser.Contains("G"))
                        {
                            eUserInfo ui = eUserInfo.GetUserInfo(eLibTools.GetNum(sUser), _pref);
                            nLevel = ui.UserLevel;
                            perm = new ePermission(0, ePermission.PermissionMode.MODE_USER_AND_LEVEL, nLevel, sUser);
                        }
                        else
                        {
                            nLevel = -1; // pas de test sur le niveau
                            perm = new ePermission(0, ePermission.PermissionMode.MODE_USER_ONLY, nLevel, sUser);
                        }

                        perm.SetPermMode();
                    }

                    bool widthGridViewRights = lstTreatTypes.Where((e) => e.GetHashCode() == eTreatmentType.VIEW.GetHashCode()).Count() > 0 || bWithAllTypes;
                    renderer = eAdminRendererFactory.CreateAdminRightsDialogRenderer(_pref, _tab, nPageId, nGridId, widthGridViewRights);


                    renderer.ListOnly = true;
                    renderer.SelectedPermission = perm;
                    renderer.From = nFrom;
                    renderer.Field = nField;
                    renderer.Function = sFunction;
                    if (hsTreatTypes != null && hsTreatTypes.Count > 0)
                        renderer.LstTreatmentTypes = hsTreatTypes;
                    renderer.Generate();

                    if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                    {
                        if (renderer.InnerException != null)
                            throw renderer.InnerException;
                        else
                            throw new Exception(renderer.ErrorMsg);
                    }
                }
                catch (EudoException exc)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6236),
                            exc,
                            eResApp.GetRes(_pref, 72)
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }
                catch (Exception exc)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6236),
                            eResApp.GetRes(_pref, 72),
                            String.Concat("Erreur création du renderer dans eAdminRightsDialog : ", exc.Message)
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }
                finally
                {

                }

                RenderResultHTML(renderer.PgContainer);
            }
            else if (action == ACTION.RefreshFilters)
            {
                RefreshFilters refFlt = new RefreshFilters();
                Dictionary<int, string> dicParents;
                int iPmDescId = (int)TableType.PM;
                int iPpDescId = (int)TableType.PP;
                eRes res = new eRes(_pref, "200,300");
                String sPMLabel = res.GetRes(iPmDescId);
                String sPPLabel = res.GetRes(iPpDescId);

                // tous les onglets
                if (_tab == 0)
                {
                    dicParents = eSqlDesc.LoadParentTabs(_pref, _tab, false, bReturnDescid: true);

                    if (!dicParents.Keys.Contains(iPpDescId))
                        dicParents.Add(iPpDescId, sPPLabel);

                    if (!dicParents.Keys.Contains(iPmDescId))
                        dicParents.Add(iPmDescId, sPMLabel);
                }
                else if (_tab == (int)TableType.XRMHOMEPAGE)
                {
                    dicParents = new Dictionary<int, string>();
                }
                else
                {
                    //Depuis
                    dicParents = eAdminTools.GetListParentTabs(_pref, _tab);
                    if (dicParents.ContainsKey((int)TableType.ADR))
                    {
                        dicParents.Remove((int)TableType.ADR);

                        if (!dicParents.ContainsKey(iPmDescId))
                            refFlt.AddFromTab(iPmDescId, sPMLabel);

                    }

                    //rubrique
                    Dictionary<int, string> dicfields;
                    Dictionary<int, int> dicformat;
                    eSqlDesc.LoadTabFields(_pref, eLibTools.GetTabFromDescId(_tab), out dicfields, out dicformat);

                    foreach (KeyValuePair<int, string> kvp in dicfields)
                    {
                        refFlt.AddField(kvp.Key, kvp.Value);
                    }

                }

                dicParents = dicParents.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                foreach (KeyValuePair<int, string> kvp in dicParents)
                {
                    refFlt.AddFromTab(kvp.Key, kvp.Value);
                }

                RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(refFlt); });
            }
        }

    }
}
