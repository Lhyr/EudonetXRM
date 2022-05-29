using System;
using System.Collections.Generic;
using System.Data;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /*************************************************************/
    /*   Classe métier de gestion des sélections d'invitation      */
    /* DESCRIPTION  : Classe de gestion des selection            */
    /* CREATION     : SPH LE 10/10/2013                         */
    /************************************************************/
    /// <summary>
    /// Classe métier de gestion des selections
    /// </summary>
    public class eInvitSelection
    {


        private Int32 _nInvitSelectionId = 0;
        private ePref _ePref = null;
        private Int32 _nNbContact = 0;

        private String _sErrorMsg = String.Empty;


        /// <summary>
        /// Erreur sur l'objet
        /// </summary>
        public String ErrorMsg
        {
            get { return _sErrorMsg; }

        }

        /// <summary>
        /// Exception
        /// </summary>
        private Exception _eInnerException = null;

        /// <summary>
        /// Exception intercepée par l'objet
        /// </summary>
        public Exception InnerException
        {
            get { return _eInnerException; }
            set { _eInnerException = value; }
        }

        /// <summary>Nombre de contact différent dans la sélection</summary>
        public Int32 NbContact
        {
            get { return _nNbContact; }
        }

        private Int32 _nNbAddress = 0;


        /// <summary>Nombre d'adresse distinct dans la sélection </summary>
        public Int32 NbAddress
        {
            get { return _nNbAddress; }

        }

        private Int32 _nNbAll = 0;

        /// <summary>
        /// Nombre d'invitation dans la sélection
        /// </summary>
        public Int32 NbAll
        {
            get { return _nNbAll; }

        }

        private Int32 _nTabInvitDescid = 0;

        private Boolean _bAllSelected = false;

        /// <summary>
        /// Indique si toutes les fiches de la sélection en cours ont été cochées
        /// </summary>
        public Boolean AllSelected
        {
            get { return _bAllSelected; }

        }

        /// <summary>Id de la sélection</summary>
        public Int32 InvitSelectionId
        {
            get { return _nInvitSelectionId; }
        }





        /// <summary>
        /// Constructeur de la classe métier des sélections d'invités
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eInvitSelection(ePref pref, Int32 nTab)
        {
            _ePref = pref;
            _nTabInvitDescid = nTab;
        }




        /// <summary>
        /// Ajoute/Retire le contenu d'un filtre à la sélection
        /// </summary>
        /// <param name="dicParam">Dictionnaire de paramètre</param>
        /// <returns></returns>
        public bool SelectAllInvit(ExtendedDictionary<String, String> dicParam)
        {

            eudoDAL edal = eLibTools.GetEudoDAL(_ePref);

            Int32 nFilterId;
            Int32 nParentEvtId;
            Int32 nParentDescId;

            Boolean bAdd = false;
            Boolean bFilterActive = false;
            Boolean bFilterMain = false;

            Boolean bDbl = false;
            Int32 nTypeDbl = 0;

            Int32 nFltTypeConsent = 0;
            Int32 nFltCampaignType = 0;
            Int32 nFltOptIn = 0;
            Int32 nFltOptOut = 0;

            bDbl = (dicParam.ContainsKey("invitselecttypdbl")
                    && Int32.TryParse(dicParam["invitselecttypdbl"].ToString(), out nTypeDbl)
                    && (nTypeDbl == TableType.PP.GetHashCode() || nTypeDbl == TableType.ADR.GetHashCode()));


            dicParam.TryGetValueConvert<Int32>("filterid", out nFilterId);

            //Mode suppression toutes les fiches
            if (nFilterId == -1)
                nFilterId = 0;

            dicParam.TryGetValueConvert<Int32>("parentevtid", out nParentEvtId);
            dicParam.TryGetValueConvert<Int32>("tabfrom", out nParentDescId);

            bAdd = dicParam.ContainsKey("addall") && dicParam["addall"] == "1";
            bFilterActive = dicParam.ContainsKey("fltact") && dicParam["fltact"] == "1";
            bFilterMain = dicParam.ContainsKey("fltprinc") && dicParam["fltprinc"] == "1";

            Boolean bDeleteMode = dicParam.ContainsKey("delete") && dicParam["delete"] == "1";

            dicParam.TryGetValueConvert<Int32>("flttypeconsent", out nFltTypeConsent);
            dicParam.TryGetValueConvert<Int32>("fltcampaigntype", out nFltCampaignType);
            dicParam.TryGetValueConvert<Int32>("fltoptin", out nFltOptIn);
            dicParam.TryGetValueConvert<Int32>("fltoptout", out nFltOptOut);
            bool bFltNoOpt = dicParam.ContainsKey("fltnoopt") && dicParam["fltnoopt"].ToString() == "1";

            try
            {

                edal.OpenDatabase();


                _bAllSelected = bAdd;


                //si bCheck, ajoute toutes les invitations du filtre



                _sErrorMsg = String.Empty;
                EudoQuery.EudoQuery query;


                if (bDeleteMode)
                {
                    query = eLibTools.GetEudoQuery(_ePref, _nTabInvitDescid, ViewQuery.LIST_SEL);
                    query.SetParentDescid = nParentDescId;
                    query.SetParentFileId = nParentEvtId;
                    //demande 86 662 : Anomalie suppression à partir d'un filtre dans un événement 
                    //la selection de fiches archivé pour la suppression
                    query.AddParam("Histo", "1");
                }
                else
                    query = eLibTools.GetEudoQuery(_ePref, 200, ViewQuery.LIST_SEL);

                try
                {
                    if (!String.IsNullOrEmpty(query.GetError))
                    {
                        _sErrorMsg = "eInvitSelection.SelectAllInvit.Init.New - " + Environment.NewLine + query.GetError;
                        return false;
                    }


                    query.SetFilterId = nFilterId;
                    query.SetAddTplTabTreatement = _nTabInvitDescid;
                    query.AddParam("selectInvit", _nInvitSelectionId.ToString());
                    query.AddParam("selectParentEvtId", nParentEvtId.ToString());
                    query.AddParam("selectDeleteMode", bDeleteMode ? "1" : "0");

                    List<WhereCustom> list = new List<WhereCustom>();


                    if (bFilterMain)
                    {
                        list.Add(new WhereCustom(AdrField.PRINCIPALE.GetHashCode().ToString(), Operator.OP_IS_TRUE, ""));

                    }

                    if (bFilterActive)
                    {
                        list.Add(new WhereCustom(AdrField.ACTIVE.GetHashCode().ToString(), Operator.OP_IS_TRUE, ""));

                    }



                    //Filtres consentements
                    List<WhereCustom> listInteractions = eInvitSelectionTools.GetConsentWhereCustom(nFltTypeConsent, nFltCampaignType, nFltOptIn, nFltOptOut, bFltNoOpt);
                    if (listInteractions.Count > 0)
                    {
                        list.Add(new WhereCustom(listInteractions, InterOperator.OP_AND));
                       // query.AddParam("showConsentField", "1");
                    }



                    if (list.Count > 0)
                    {
                        query.AddCustomFilter(new WhereCustom(list));
                    }

                    if (bDbl)
                    {
                        query.AddParam("selectInvitTypeDbl", nTypeDbl.ToString());
                    }




                    // Load la request
                    query.LoadRequest();
                    if (!String.IsNullOrEmpty(query.GetError))
                    {
                        _sErrorMsg = "eInvitSelection.SelectAllInvit.LoadRequest - " + Environment.NewLine + query.GetError;
                        return false;
                    }

                    query.BuildRequest();
                    if (!String.IsNullOrEmpty(query.GetError))
                    {
                        _sErrorMsg = "eInvitSelection.SelectAllInvit.BuildRequest - " + Environment.NewLine + query.GetError;
                        return false;
                    }


                    RqParam RqInsertAll = new RqParam();

                    String sSQLListIds = String.Empty;


          

                    String sFieldInsert = String.Empty;
                    String sSQLCondition = string.Empty;


                    if (bDeleteMode)
                    {
                        

                        sFieldInsert = String.Concat(" [", _nTabInvitDescid, "].[TPLID], NULL, NULL ");
                        sSQLCondition = String.Concat("[", _nTabInvitDescid, "]", ".[TPLID] = [invitselectionfiles].[TPLID]");


                    }
                    else
                    {
                   
                        sFieldInsert = String.Concat(" NULL, [200].[PPID], [200_400].[ADRID] ");

                        sSQLCondition = String.Concat( "[200].[PPID] = [invitselectionfiles].[PPID]   AND [200_400].[ADRID] = [invitselectionfiles].[ADRID] ");
                        

                    }

                    if (bAdd)
                    {
                        //SHA : correction bug #70 428
                        string sTopRandom = "";
                        if (query.GetRandomEnabled)
                            sTopRandom = String.Concat("TOP ", query.GetTopRecord);

                        sSQLListIds = String.Concat("INSERT INTO [invitselectionfiles](selectionid, tplid,  ppid,adrid) SELECT DISTINCT ", sTopRandom, " @seleId, ", sFieldInsert, query.EqBaseQuery, " AND NOT EXISTS (SELECT 1 FROM [invitselectionfiles] WHERE selectionid = @seleid AND ", sSQLCondition, ") ");
                    }
                    else
                    {

                        sSQLListIds = String.Concat(" DELETE FROM [invitselectionfiles]   WHERE [SELECTIONID] IN (SELECT [SELECTIONID] FROM INVITSELECTION WHERE [USERID] = @userid AND [SELECTIONID]=@seleId)  AND EXISTS (SELECT 1  ", query.EqBaseQuery, "AND ",sSQLCondition, ")");

                        RqInsertAll.AddInputParameter("@userid", SqlDbType.Int, _ePref.User.UserId);


                    }
                    RqInsertAll.SetQuery(sSQLListIds);

                    RqInsertAll.AddInputParameter("@seleId", SqlDbType.Int, InvitSelectionId);


                    edal.ExecuteNonQuery(RqInsertAll, out _sErrorMsg);
                    if (!String.IsNullOrEmpty(_sErrorMsg))
                    {
                        _sErrorMsg = String.Concat("eInvitSelection.SelectAllInvit.Execute - ", Environment.NewLine, _sErrorMsg);
                        return false;
                    }

                    UpdateCmpt(edal);

                }
                catch
                {
                    throw;
                }
                finally
                {
                    query.CloseQuery();
                }



            }
            catch
            {
                throw;
            }
            finally
            {
                edal.CloseDatabase();
            }

            return true;
        }



        /// <summary>
        /// Ajoute/Retire une adresse dans la liste de (pré)sélection d'invité (++)
        /// </summary>
        /// <param name="nPPid">PPid du contact</param>
        /// <param name="nAdrId">Adrid de l'invitation</param>
        /// <param name="bCheck">Cochage/décochage</param>
        /// <param name="bDeleteMode">Mode suppresion d'invitations</param>
        /// <returns>Id de la sélection</returns>
        public bool SelectInvit(Int32 nPPid, Int32 nAdrId, Int32 nTplId, Boolean bCheck, Boolean bDeleteMode = false)
        {


            if (InvitSelectionId > 0)
            {
                String sSQL = String.Empty;
                String sSQLCondition = String.Empty;
                String sSQLInsertField = String.Empty;


                if (!bDeleteMode)
                {
                    sSQLInsertField = " ,@adrid,@ppid, NULL ";
                    sSQLCondition = " [ADRID] = @adrid AND [PPID] = @ppid ";
                }
                else
                {
                    sSQLInsertField = " ,NULL,NULL, @tplid ";
                    sSQLCondition = " [TPLID] = @tplid ";
                }

                if (bCheck)
                {
                    //Ajoute à la sélection
                    sSQL = String.Concat("INSERT INTO [INVITSELECTIONFILES] ([SELECTIONID], [ADRID], [PPID], [TPLID]) SELECT @selid", sSQLInsertField, " WHERE NOT EXISTS (SELECT TOP 1 1 FROM [INVITSELECTIONFILES] WHERE [SELECTIONID]= @selid AND ", sSQLCondition, ") ");

                }
                else
                {
                    _bAllSelected = false;
                    //Retire de la sélection
                    sSQL = String.Concat("DELETE FROM [INVITSELECTIONFILES] WHERE [SELECTIONID] = @selid AND ", sSQLCondition);
                }

                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@selid", SqlDbType.Int, InvitSelectionId);

                if (!bDeleteMode)
                {
                    if (nAdrId != 0)
                        rq.AddInputParameter("@adrid", SqlDbType.Int, nAdrId);
                    else
                        rq.AddInputParameter("@adrid", SqlDbType.Int, DBNull.Value);

                    rq.AddInputParameter("@ppid", SqlDbType.Int, nPPid);
                }
                else
                    rq.AddInputParameter("@tplid", SqlDbType.Int, nTplId);

                //Met à jour les compteurs

                eudoDAL edal = eLibTools.GetEudoDAL(_ePref);

                try
                {
                    edal.OpenDatabase();

                    String sError = String.Empty;
                    edal.ExecuteNonQuery(rq, out sError);

                    if (sError.Length > 0)
                    {
                        if (edal.InnerException != null)
                            throw edal.InnerException;
                        else
                            throw new Exception(String.Concat("Erreur de suppression des anciennes sélections :", sError));

                    }

                    UpdateCmpt(edal);
                }
                finally
                {
                    edal.CloseDatabase();
                }
                return true;


            }


            return false;
        }




        /// <summary>
        /// Met à jour les compteurs d'invitations
        /// La connexion doit être ouverte et fermée lors de l'appel
        /// </summary>
        private void UpdateCmpt(eudoDAL edal)
        {


            String sError = String.Empty;
            try
            {

                RqParam rq = new RqParam("SELECT @cntadr=count(distinct adrid), @cntppid=count(distinct ppid), @cntall=count(1)  from invitselection inner join [invitselectionfiles]  on invitselection.selectionid = [invitselectionfiles].selectionid where [invitselection].selectionid=@selid and [userid]=@userid");

                rq.AddOutputParameter("@cntppid", SqlDbType.Int, 16);
                rq.AddOutputParameter("@cntadr", SqlDbType.Int, 16);
                rq.AddOutputParameter("@cntall", SqlDbType.Int, 16);

                rq.AddInputParameter("@userid", SqlDbType.Int, _ePref.User.UserId);
                rq.AddInputParameter("@selid", SqlDbType.Int, InvitSelectionId);
                edal.ExecuteNonQuery(rq, out sError);

                _nNbAddress = (Int32)rq.GetParamValue("@cntadr");
                _nNbContact = (Int32)rq.GetParamValue("@cntppid");
                _nNbAll = (Int32)rq.GetParamValue("@cntall");
            }
            catch
            {
                throw;
            }
            finally
            {

            }


        }

        /// <summary>
        /// Retourne un objet pour manipuler une sélection d'invité
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nTab">Descid de la table d'invitations</param>
        /// <returns></returns>
        public static eInvitSelection GetInvitSelection(ePref pref, Int32 nTab)
        {
            eInvitSelection mySelection = new eInvitSelection(pref, nTab);
            mySelection.GetInvitSelection(nTab);


            return mySelection;
        }


        /// <summary>
        /// Retourne l'id de la sélection d'invité pour la table demandé
        /// Il ne peut y avoir qu'une sélection simultanée par user
        /// </summary>
        /// <param name="nTabDescId">Descid de la table d'invitation</param>
        /// <returns></returns>
        private Int32 GetInvitSelection(Int32 nTabDescId)
        {

            String sError = String.Empty;
            eudoDAL edal;

            //Si la sélection existe déjà, on la retourne
            if (_ePref.Context.InvitSelectId.ContainsKey(nTabDescId))
            {
                _nInvitSelectionId = _ePref.Context.InvitSelectId[nTabDescId];

                edal = eLibTools.GetEudoDAL(_ePref);
                try
                {
                    edal.OpenDatabase();
                    UpdateCmpt(edal);
                }
                catch
                {
                    throw;
                }
                finally
                {

                    edal.CloseDatabase();
                }


                return InvitSelectionId;
            }

            //Sinon, on la crée
            // Il ne peut y avoir qu'une sélection par user, on commence par supprimer toutes les autres sélections du user
            edal = eLibTools.GetEudoDAL(_ePref);
            try
            {
                edal.OpenDatabase();

                //Suppression des filesid
                RqParam rq = new RqParam("DELETE FROM [INVITSELECTIONFILES] WHERE [SELECTIONID] IN  ( SELECT [SELECTIONID] FROM [INVITSELECTION] WHERE [USERID] = @userid)");
                rq.AddInputParameter("@userid", SqlDbType.Int, _ePref.User.UserId);
                edal.ExecuteNonQuery(rq, out sError);

                if (sError.Length > 0 || edal.InnerException != null)
                {
                    if (edal.InnerException != null)
                        throw edal.InnerException;
                    else
                        throw new Exception(String.Concat("Erreur de suppression des anciennes fiches invités sélectionnées :", sError));
                }


                //Suppression des sélections
                rq = new RqParam("DELETE FROM [INVITSELECTION] WHERE [USERID] = @userid");
                rq.AddInputParameter("@userid", SqlDbType.Int, _ePref.User.UserId);
                edal.ExecuteNonQuery(rq, out sError);


                if (sError.Length > 0)
                {
                    if (edal.InnerException != null)
                        throw edal.InnerException;
                    else
                        throw new Exception(String.Concat("Erreur de suppression des anciennes sélections :", sError));

                }

                //Création de la nouvelle séleciton

                rq = new RqParam("INSERT [INVITSELECTION] (userid,tab) VALUES (@userid,@tab); select @ID=scope_identity() ");
                rq.AddInputParameter("@userid", SqlDbType.Int, _ePref.User.UserId);
                rq.AddInputParameter("@tab", SqlDbType.Int, nTabDescId);
                rq.AddOutputParameter("@ID", SqlDbType.Int, 0);


                edal.ExecuteNonQuery(rq, out sError);

                if (sError.Length > 0)
                {
                    if (edal.InnerException != null)
                        throw edal.InnerException;
                    else
                        throw new Exception(String.Concat("Erreur d'enregistrement de la sélection :", sError));

                }

                Int32 nFileID = (Int32)rq.GetParamValue("@ID");


                //Ajoute la sélection au contexte

                _nInvitSelectionId = nFileID;
                _ePref.Context.InvitSelectId.Add(nTabDescId, nFileID);
                return InvitSelectionId;
            }
            catch
            {
                throw;
            }
            finally
            {
                edal.CloseDatabase();
            }
        }

        /// <summary>
        /// Supprime les selections qui ne corespondent pas à l'option ne retenir que les adresses actives ou/et principales
        /// </summary>
        /// <param name="bFilterActive">Adresse active</param>
        /// <param name="bFilterMain">Adresse principale</param>
        public void CleanSelection(Boolean bFilterActive = false, Boolean bFilterMain = false)
        {
            //Si les deux options  adresses actives et principales ne sont pas cochée on fait rien
            if (!bFilterActive && !bFilterMain)
                return;

            String sError = String.Empty;
            eudoDAL edal;

            edal = eLibTools.GetEudoDAL(_ePref);
            try
            {
                edal.OpenDatabase();

                //On supprime les lignes qui ne sont pas actives
                //BSE #50 667 Suppression des adresses non actives OU non principale OU les 2
                String notActive = string.Empty;
                if (bFilterActive)
                    notActive =String.Concat( " AND ( ISNULL([ADDRESS].[ADR11],0) <> '1' " , bFilterMain ? "":")");

                //On supprime les lignes qui ne sont pas principales
                String notMain = string.Empty;
                if (bFilterMain)
                    notMain = String.Concat(bFilterActive ? " OR ": " AND (", " ISNULL([ADDRESS].[ADR12],0) <> '1' )" );


                
                //Suppression des selection d'invit qui n'ont pas d'adresses actives ou/et principales            
                RqParam rq = new RqParam(String.Concat("DELETE FROM [INVITSELECTIONFILES] FROM [INVITSELECTIONFILES] AS [INVIT] INNER JOIN [ADDRESS] ON  [INVIT].[ADRID] = [ADDRESS].[ADRID] WHERE [INVIT].[SELECTIONID] = @SelectionId ", notActive, notMain));
                rq.AddInputParameter("@SelectionId", SqlDbType.Int, this.InvitSelectionId);

                Int32 nbAffectedRow = edal.ExecuteNonQuery(rq, out sError);

                if (sError.Length > 0 || edal.InnerException != null)
                {
                    if (edal.InnerException != null)
                        throw edal.InnerException;
                    else
                        throw new Exception(String.Concat("Erreur de suppression des anciennes fiches invités sélectionnées qui n'ont d'adresse active ou/et principale.", sError));
                }

                UpdateCmpt(edal);
            }
            finally 
            {
                edal.CloseDatabase();
            }
        }
    }
}