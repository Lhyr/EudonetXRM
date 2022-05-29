using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Création d'une liste de droit de traitement
    /// </summary>
    public class eAdminDescTreatmentRightCollection
    {
        /// <summary>Filtre sur "Depuis" (Signet depuis event, PP, PM)</summary>
        public Int32 From = 0;

        /// <summary>Filtre sur "Onglet"</summary>
        public Int32 Tab = 0;

        /// <summary>Filtre sur "Rubrique"</summary>
        public Int32 Field = 0;

        /// <summary>Filtre sur le nom de la fonction</summary>
        public string Function = "";

        /// <summary>Filtre sur "Type de traitement". Ne doit pas être null.</summary>
        public HashSet<eTreatmentType> TreatTypes = new HashSet<eTreatmentType>();

        /// <summary>
        /// Liste des droits
        /// </summary>
        public List<IAdminTreatmentRight> RightsList;


        /// <summary>
        /// Pref user pour les requete/modif
        /// </summary>
        protected ePref _pref;



        /// <summary>
        /// Constructeur standard
        /// </summary>
        /// <param name="pref"></param>
        public eAdminDescTreatmentRightCollection(ePref pref)
        {
            RightsList = new List<IAdminTreatmentRight>();
            _pref = pref;
        }

        /// <summary>
        /// Retourne une liste de droits de traitement en fonction de son type et/ou sa table.
        /// Passé null/0 pour ignorer le critère
        /// </summary>
        /// <returns></returns>
        public virtual void LoadTreamentsList()
        {

            if (_pref == null)
                throw new ArgumentNullException("ePref non fourni");



            eudoDAL eDal = null;
            try
            {
                eDal = eLibTools.GetEudoDAL(_pref);
                eDal.OpenDatabase();

                #region on rajoute les enregistrements potentiellement manquants dans FileDataParam et dans trait
                String sError;

                if (Tab % 100 == 0)
                {
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
                }

                if (Tab % 100 == 0)
                {
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
                }
                #endregion

                StringBuilder sbSQL = new StringBuilder();
                RqParam rq = new RqParam();
                List<String> lstSubRequest = new List<String>();

                //droits de traitements tels qu'ils existaient en V7
                lstSubRequest.Add(TraitSubQuery);

                rq.AddInputParameter("@GlobalRightsLabel", SqlDbType.VarChar, eResApp.GetRes(_pref, 7611));

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
                    rq.AddInputParameter(String.Concat("@", tltmp, "label"), SqlDbType.VarChar, eAdminDescTreatmentRight.GetResFromTreatLocation(_pref, tltmp));
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
                if ((Tab == 0 && !TreatTypes.Contains(eTreatmentType.CATALOG)) || From > 0)
                    Field = -1;

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
                if (Tab == -1)
                {
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
                else if (Tab == 0 && TreatTypes.Contains(eTreatmentType.CATALOG))
                {
                }
                else if (Tab == 0) //tous les onglets => aucune rubrique, sauf si on a choisi le type Catalogue (voire au dessus)
                {
                    sbSQLWhere.Append(" WHERE ( RIGHTS.[DESCID] % 100 = 0 AND RIGHTS.[DESCID] >= 100 ")
                                .Append(" AND   RIGHTS.[DESCID] <  ").Append((int)TableType.HISTO)
                                .Append(" )");

                    //// rq.AddInputParameter("@descid", SqlDbType.Int, Tab);
                }
                else if (Field == 0 && Tab > 0) //Field == 0 : toutes les rubriques
                {
                    sbSQLWhere.Append(" WHERE (RIGHTS.[DESCID] BETWEEN @descid AND @Descid + 99) ");

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



                sbSQL.AppendLine().Append("ORDER BY RIGHTS.TabLabel, RIGHTS.FieldLabel, RIGHTS.FromLabel, RIGHTS.TraitLabel ");
                sbSQL.Replace("@@LNG@@", _pref.User.UserLang);

                rq.SetQuery(sbSQL.ToString());

                sError = String.Empty;
                DataTableReaderTuned dtr = eDal.Execute(rq, out sError);

                if (sError.Length > 0)
                {
                    throw eDal.InnerException;

                }

                if (dtr == null)
                    throw new Exception("Erreur de récupération des traitements");

                try
                {
                    eAdminDescTreatmentRight adminRight = null;
                    while (dtr.Read())
                    {
                        adminRight = new eAdminDescTreatmentRight(_pref);
                        adminRight.TreatLoc = (eLibConst.TREATMENTLOCATION)dtr.GetInt32("TraitLocation");

                        if (adminRight.TreatLoc == eLibConst.TREATMENTLOCATION.Trait)
                        {
                            //Propriétés en provenance de Trait
                            adminRight.SetTrait(nTraitID: dtr.GetEudoNumeric("l_TraitId"), nGlobalTraitId: dtr.GetEudoNumeric("g_TraitId"));

                            //Filtre sur les types choisis
                            if (TreatTypes?.Count > 0 && !TreatTypes.Contains(adminRight.Type))
                                continue;
                        }
                        else
                        {
                            //Filtre sur les types choisis
                            if (TreatTypes?.Count > 0 && !TreatTypes.Contains(eAdminDescTreatmentRight.GetType(adminRight.TreatLoc)))
                                continue;

                            //Propriétés en provenance de DESC
                            adminRight.SetFromDesc(dtr.GetString("FromLabel"), dtr.GetString("FieldLabel"));
                        }

                        //#74 833 KJE: masquer ces fonctionnalités qui ne sont plus utilisées (dernière version V5).
                        // US #4315 - Le droit 122 est désormais utilisé pour "Modifier la zone Assistant"
                        if (adminRight.GlobalTraitId == 121
                            //|| adminRight.GlobalTraitId == 122
                            || adminRight.GlobalTraitId == 123
                            || adminRight.GlobalTraitId == 131
                            || adminRight.GlobalTraitId == 132
                            || adminRight.GlobalTraitId == 133)
                            continue;

                        adminRight.DescID = dtr.GetEudoNumeric("DESCID");
                        if((adminRight.DescID - adminRight.DescID % 100) == 101000)
                        {
                            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                                continue;

                            if (adminRight.GlobalTraitId != 118)
                                continue;
                        }

                        adminRight.TabLabel = dtr.GetString("TabLabel");
                        //if (adminRight.DescID > 0)
                        //{
                        //    adminRight.TabLabel = dtr.GetString("TabLabel");
                        //}
                        //else
                        //{
                        //    adminRight.TabLabel = eResApp.GetRes(_pref, 7611);
                        //}
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
                            // Pas de permission d'import par défaut 
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

        #region static de requete "préfaite"

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
                                                                     "		,CASE WHEN RES.ResId > 0 THEN RES.@@LNG@@ ELSE @GlobalRightsLabel END TabLabel ", Environment.NewLine,
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
                                                                     "	    AND NOT lt.TraitId in (54)", Environment.NewLine, //ex catalogues avancés
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
                                  "                    , 87 ", Environment.NewLine,
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
                                  "   LEFT JOIN RES TABRES ON  TABRES.ResId + case when res.resid=2 then 0 else (RES.ResId % 100) end = RES.ResId  ", Environment.NewLine
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

        /// <summary>
        /// Gets the BKM view pp pm query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="tl">The tl.</param>
        /// <returns></returns>
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

        /// <summary>Requête SQL pour les Droits sur les catalogues (hors relation)</summary>
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
                                  "   WHERE d.POPUP > @popupnone AND (Popupdescid % 100 > 1 OR PopupDescid = d.Descid) "
                    );
        }

        #endregion


    }


}