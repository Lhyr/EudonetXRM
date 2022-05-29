using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// 
    /// </summary>
    public class eFilteredSelection
    {

        private Int32 _nSelectionId = 0;
        private ePref _ePref = null;
        private Int32 _nNbFiles = 0;

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
        public Int32 NbFiles
        {
            get { return _nNbFiles; }
        }

        private Int32 _nNbAll = 0;

        /// <summary>
        /// Nombre de fiches dans la sélection
        /// </summary>
        public Int32 NbAll
        {
            get { return _nNbAll; }

        }

        private Int32 _nTabSelectionDescid = 0;
        private Int32 _nTabSourceDescid = 0;

        private Boolean _bAllSelected = false;

        /// <summary>
        /// Indique si toutes les fiches de la sélection en cours ont été cochées
        /// </summary>
        public Boolean AllSelected
        {
            get { return _bAllSelected; }

        }




        /// <summary>
        /// Constructeur de la classe métier des sélections de fiches
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nTabSource"></param>
        private eFilteredSelection(ePref pref, Int32 nTab, Int32 nTabSource)
        {
            _ePref = pref;
            _nTabSelectionDescid = nTab;
            _nTabSourceDescid = nTabSource;
        }




        /// <summary>
        /// Ajoute/Retire le contenu d'un filtre à la sélection
        /// </summary>
        /// <param name="dicParam">Dictionnaire de paramètre</param>
        /// <returns></returns>
        public bool SelectAllFiles(ExtendedDictionary<String, String> dicParam)
        {
            String filters = String.Empty;
            int nRows, nPage;

            dicParam.TryGetValue("filters", out filters);
            dicParam.TryGetValueConvert<Int32>("rows", out nRows);
            dicParam.TryGetValueConvert<Int32>("page", out nPage);

            List<WhereCustom> listwc;
            if (!String.IsNullOrEmpty(filters))
                listwc = eSelectionWizardRenderer.GetFiltersWhereCustom(filters);
            else
                listwc = new List<WhereCustom>();

            eList listfilteredsel = eListFactory.CreateListFilteredSelection(_ePref, _nTabSourceDescid, _nTabSelectionDescid, listwc, nRows, nPage);



            eListFilteredSelection fs = (eListFilteredSelection)listfilteredsel;
            String listBaseQuery = fs.GetBaseQuery();

            eudoDAL edal = eLibTools.GetEudoDAL(_ePref);
            

            

            //Int32 nFilterId;
            //Int32 nParentEvtId;
            //Int32 nParentDescId;

            Boolean bAdd = true;

            //dicParam.TryGetValueConvert<Int32>("filterid", out nFilterId);

            //Mode suppression toutes les fiches
            //if (nFilterId == -1)
            //    nFilterId = 0;

            //dicParam.TryGetValueConvert<Int32>("parentevtid", out nParentEvtId);
            //dicParam.TryGetValueConvert<Int32>("tabfrom", out nParentDescId);

            //bAdd = dicParam.ContainsKey("addall") && dicParam["addall"] == "1";

            //Boolean bDeleteMode = dicParam.ContainsKey("delete") && dicParam["delete"] == "1";

            try
            {

                edal.OpenDatabase();

                _sErrorMsg = String.Empty;

                TableLite tl = new TableLite(_nTabSourceDescid);
                tl.ExternalLoadInfo(edal, out _sErrorMsg);
                String idField = tl.FieldId;

                String selectionQuery = String.Concat("SELECT DISTINCT @type, ", idField, ", @userID, @tabID, @sourceTabID ", listBaseQuery, " AND NOT EXISTS (SELECT ",idField," FROM [FILESELECTION] WHERE FileID = ",idField," AND UserId = @userID AND TableID = @tabID AND SelectionType = @type)");

                _bAllSelected = bAdd;

                try
                {
                    RqParam queryInsertAll = new RqParam();

                    String sSQLListIds = String.Empty;

                    String sFieldInsert = String.Empty;
                    String sSQLCondition = string.Empty;

                    //sFieldInsert = String.Concat(" [", _nTabSelectionDescid, "].[TPLID], NULL, NULL ");
                    //sSQLCondition = String.Concat("[", _nTabSelectionDescid, "]", ".[TPLID] = [invitselectionfiles].[TPLID]");


                    if (bAdd)
                    {
                        sSQLListIds = String.Concat("INSERT INTO [FILESELECTION] ([SelectionType], [FileID], [UserID], [TableID], [SourceTableID]) ", selectionQuery);
                    }
                    else
                    {
                        //sSQLListIds = String.Concat(" DELETE FROM [FILESELECTION] WHERE [SELECTIONID] IN (SELECT [SELECTIONID] FROM INVITSELECTION WHERE [USERID] = @userid AND [SELECTIONID]=@seleId)  AND EXISTS (SELECT 1  ", query.EqBaseQuery, "AND ", sSQLCondition, ")");

                        //RqInsertAll.AddInputParameter("@userid", SqlDbType.Int, _ePref.User.UserId);


                    }
                    queryInsertAll.SetQuery(sSQLListIds);
                    queryInsertAll.AddInputParameter("@type", SqlDbType.Int, eConst.SELECTION_TYPE.FILTERED_SEL);
                    queryInsertAll.AddInputParameter("@userID", SqlDbType.Int, _ePref.UserId);
                    queryInsertAll.AddInputParameter("@tabID", SqlDbType.Int, _nTabSelectionDescid);
                    queryInsertAll.AddInputParameter("@sourceTabID", SqlDbType.Int, _nTabSelectionDescid);

                    edal.ExecuteNonQuery(queryInsertAll, out _sErrorMsg);
                    if (!String.IsNullOrEmpty(_sErrorMsg))
                    {
                        _sErrorMsg = String.Concat("eInvitSelection.SelectAllInvit.Execute - ", Environment.NewLine, _sErrorMsg);
                        return false;
                    }

                    //UpdateCmpt(edal);

                }
                catch
                {
                    throw;
                }
                finally
                {
                    //query.CloseQuery();
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
        /// <param name="bDeleteMode">Mode suppresion d'inviations</param>
        /// <returns>Id de la sélection</returns>
        public bool SelectInvit(Int32 fileId, int userId, int tabId, Boolean bCheck, Boolean bDeleteMode = false)
        {


            //if (SelectionId > 0)
            //{
                String sSQL = String.Empty;
                String sSQLCondition = String.Empty;
                String sSQLInsertField = String.Empty;

                if (bCheck)
                {
                    //Ajoute à la sélection
                    sSQL = String.Concat("INSERT INTO [FILESELECTION] ([SELECTIONID], [ADRID], [PPID], [TPLID]) SELECT @selid", sSQLInsertField, " WHERE NOT EXISTS (SELECT TOP 1 1 FROM [INVITSELECTIONFILES] WHERE [SELECTIONID]= @selid AND [TPLID] = @tplid) ");
                }
                else
                {
                    _bAllSelected = false;
                    //Retire de la sélection
                    sSQL = String.Concat("DELETE FROM [FILESELECTION] WHERE [SELECTIONID] = @selid AND ", sSQLCondition);
                }

                RqParam rq = new RqParam(sSQL);
                //rq.AddInputParameter("@selid", SqlDbType.Int, SelectionId);
               // rq.AddInputParameter("@tplid", SqlDbType.Int, nTplId);

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

                    //UpdateCmpt(edal);
                }
                finally
                {
                    edal.CloseDatabase();
                }
                return true;


            //}


            return false;
        }



 

        /// <summary>
        /// Retourne un objet pour manipuler une sélection d'invité
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nTab">Descid de la table d'invitations</param>
        /// <param name="nTabSource"></param>
        /// <returns></returns>
        public static eFilteredSelection GetFilteredSelection(ePref pref, Int32 nTab, Int32 nTabSource)
        {
            eFilteredSelection mySelection = new eFilteredSelection(pref, nTab, nTabSource);            
            return mySelection;
        }

 

        /// <summary>
        /// Supprime les selections qui ne coresspondent pas à l'option ne retenir que les adresses actives ou/et principales
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
                String notActive = string.Empty;
                if (bFilterActive)
                    notActive = " AND [ADDRESS].[ADR11] <> '1' ";

                //On supprime les lignes qui ne sont pas principales
                String notMain = string.Empty;
                if (bFilterMain)
                    notMain = " AND [ADDRESS].[ADR12] <> '1' ";

                //Suppression des selection d'invit qui n'ont pas d'adresses actives ou/et princiaples            
                RqParam rq = new RqParam(String.Concat("DELETE FROM [INVITSELECTIONFILES] FROM [INVITSELECTIONFILES] AS [INVIT] INNER JOIN [ADDRESS] ON  [INVIT].[ADRID] = [ADDRESS].[ADRID] WHERE [INVIT].[SELECTIONID] = @SelectionId", notActive, notMain));
                //rq.AddInputParameter("@SelectionId", SqlDbType.Int, this.SelectionId);

                Int32 nbAffectedRow = edal.ExecuteNonQuery(rq, out sError);

                if (sError.Length > 0 || edal.InnerException != null)
                {
                    if (edal.InnerException != null)
                        throw edal.InnerException;
                    else
                        throw new Exception(String.Concat("Erreur de suppression des anciennes fiches invités sélectionnées qui n'ont d'adresse active ou/et principale.", sError));
                }
            }
            finally
            {
                edal.CloseDatabase();
            }
        }
    }
}