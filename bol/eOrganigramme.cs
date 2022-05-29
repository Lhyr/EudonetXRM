using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eOrganigramme</className>
    /// <summary>classe gérant le contenu de l'organigramme</summary>
    /// <authors>GCH</authors>
    /// <date>2014-05-05</date>
    public class eOrganigramme
    {
        /// <summary>Id de la fiche de départ</summary>
        private Int32 _nFileId = 0;
        /// <summary>descid de la table ou l'on souhaite voir l'organigramme</summary>
        private Int32 _nTabId = 0;
        /// <summary>descid du champs de liason</summary>
        private Int32 _nDescId = 0;
        /// <summary>preference de l'utilisateur</summary>
        private ePref _pref = null;
        /// <summary>Objet d'accès à la BDD</summary>
        private eudoDAL _dal = null;
        /// <summary>objet table ou l'on souhaite voir l'organigramme</summary>
        private TableLite _tab = null;
        /// <summary>objet champ du champ de liason</summary>
        private FieldLite _fld = null;
        /// <summary>Elements le plus haut de l'organigramme</summary>
        private eOrganigrammeItem _maintItem = null;
        /// <summary>Retourne le nombre de niveaux d'arborescence</summary>
        private Int32 _nMaxLvl = 0;


        const string V_CTE = "V2 - 410";
             
        #region CONSTRUCTEURS
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">preference de l'utilisateur</param>
        /// <param name="nTabId">id de la table on l'on souhaite voir l'organigramme</param>
        /// <param name="nFileId">Id de la fiche de départ</param>
        /// <param name="IsTopLevel">Indique si l'on souhaite que la fiche indiquée soit la plus haute affichée (c'est à dire sans les parents de cette dernière)</param>
        public eOrganigramme(ePref pref, Int32 nTabId, Int32 nFileId, Boolean IsTopLevel = false)
        {
            _pref = pref;
            _nFileId = nFileId;
            _nTabId = nTabId;
            _dal = eLibTools.GetEudoDAL(_pref);
        }
        #endregion

        #region FCT

        #region FCT publiques
        /// <summary>
        /// 1 En premier
        /// chargement des dépendances de configurations pour construire l'organigramme
        /// </summary>
        public Boolean Load(out String sError)
        {
            sError = String.Empty;
            try
            {
                _nDescId = GetUserValue();
                LoadTableLite(_nTabId, _nDescId);
                CheckSystemIntegrity();
            }
            catch (Exception ex)
            {
                sError = eLibTools.GetExceptionMsg(ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 2 Après le LOAD
        /// Génération de l'arbre d'organigramme
        /// </summary>
        public Boolean Build(out String sError)
        {
            Int32 nHighestFileId = 0;
            sError = String.Empty;
            try
            {
                nHighestFileId = GetHighestId();
                Dictionary<Int32, Int32> listIDS = GetIdsWithLvl(nHighestFileId);
                List<eOrganigrammeItem> listItem = EudoQuery_SetValues(listIDS);

                eOrganigrammeItem a = listItem.Find(z => z.Id == nHighestFileId);
                if (a != null)
                {
                    a.ParentId = 0;
                }

                _maintItem = ReorderChild(listItem);
            }
            catch (Exception ex)
            {
                sError = eLibTools.GetExceptionMsg(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// (3 Après le BUILD)
        /// Récupération du noeud le plus haut de l'organigramme
        /// </summary>
        public eOrganigrammeItem GetMainItem()
        {
            return _maintItem;
        }

        /// <summary>
        /// (3 Après le BUILD)
        /// Retourne le nombre de niveaux d'arborescence
        /// </summary>
        public Int32 GetMaxLevel()
        {
            return _nMaxLvl;
        }
        #endregion

        #region FCT Load
        /// <summary>
        /// Récupération du descid de liaison
        /// </summary>
        /// <returns>le descid de liaison de l'organigramme</returns>
        public Int32 GetUserValue()
        {
            String value = string.Empty;
            string sSQL = string.Concat(
                "SELECT ISNULL( [Enabled], 0 ) AS [Enabled],[Value] "
                , " FROM [UserValue] AS MainUserValue "
                , " WHERE ISNULL( [MainUserValue].[Type], 0 ) = @nType"
                , " AND ISNULL( [MainUserValue].[Tab], 0 ) = @Tab"
                , "  AND ( [MainUserValue].[userid] = @UserId or isnull([MainUserValue].[userid],0) = 0 ) "
                , "AND ISNULL( [Enabled], 0 ) = 1 "
                );

            string sError = string.Empty;
            RqParam rq = new RqParam(sSQL);

            rq.AddInputParameter("@Tab", SqlDbType.Int, _nTabId);
            rq.AddInputParameter("@UserId", SqlDbType.Int, _pref.User.UserId);
            rq.AddInputParameter("@nType", SqlDbType.Int, TypeUserValue.GRAPH_ORG_LINK.GetHashCode());
            _dal.OpenDatabase();
            DataTableReaderTuned dtr = _dal.Execute(rq, out sError);
            try
            {
                if (sError.Length > 0)
                    throw new eOrganigrammeException("rqt", sError);
                if (dtr == null || !dtr.Read())
                    return -1;

                bool enabled = dtr.GetBoolean(0);
                value = dtr.GetString("value");

            }
            catch (eOrganigrammeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new eOrganigrammeException("rqt ex", "Erreur sur GetUserValue - récup des informations en BDD", ex);
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
                _dal.CloseDatabase();
            }
            Int32 nDescId = eLibTools.GetNum(value);
            if (nDescId <= 0)
                throw new eOrganigrammeException("GetNum", "Le descid n'est pas valide.");
            return nDescId;
        }

        /// <summary>
        /// Chargement des informations de la table ou l'on souhaite voir l'organigramme
        ///     et du champs de liason
        /// </summary>
        /// <param name="nTab">descid de la table ou l'on souhaite voir l'organigramme</param>
        /// <param name="nDescId">descid du champs de liason</param>
        private void LoadTableLite(Int32 nTab, Int32 nDescId)
        {
            String sError = String.Empty;
            try
            {
                _dal.OpenDatabase();
                _tab = new EudoQuery.TableLite(nTab);
                _tab.ExternalLoadInfo(_dal, out sError);
                if (sError.Length > 0)
                    throw new eOrganigrammeException("tab.ExternalLoadInfo", sError);
                _fld = new EudoQuery.FieldLite(nDescId);
                _fld.ExternalLoadInfo(_dal, out sError);
                if (sError.Length > 0)
                    throw new eOrganigrammeException("fld.ExternalLoadInfo", sError);
            }
            finally
            {
                _dal.CloseDatabase();
            }
        }

        private void CheckSystemIntegrity()
        {
            if (!checkCTEOrga(GetCTEOrgaHighiestId_Name(_nTabId, _nDescId)))
                CreateCTEHighiest(_tab, _fld);
            if (!checkCTEOrga(GetCTEOrgaIds_Name(_nTabId, _nDescId)))
                CreateCTEOrgaIds(_tab, _fld);
        }

        /// <summary>
        /// Retourne le nom de la CTE retournant l'id du fichier le plus haut de l'arborescence
        /// </summary>
        /// <param name="nTab">descid de la table ou l'on souhaite voir l'organigramme</param>
        /// <param name="nDescId">descid du champs de liason</param>
        /// <returns>nom de la CTE sql</returns>
        private static string GetCTEOrgaHighiestId_Name(Int32 nTab, Int32 nDescId)
        {
            return String.Concat("xfc_getOrgaHighID_", nTab, "_", nDescId);
        }

        /// <summary>
        /// Retourne le nom de la CTE retournant tous les ids organisés de l'arborescence
        /// </summary>
        /// <param name="nTab">descid de la table ou l'on souhaite voir l'organigramme</param>
        /// <param name="nDescId">descid du champs de liason</param>
        /// <returns>nom de la CTE sql</returns>
        private static string GetCTEOrgaIds_Name(Int32 nTab, Int32 nDescId)
        {
            return String.Concat("xfc_getOrgaIDs_", nTab, "_", nDescId);
        }

        /// <summary>
        /// Génère en BDD la CTE retournant l'id du fichier le plus haut de l'arborescence
        /// </summary>
        /// <param name="tab">Table ou l'on souhaite voir l'organigramme</param>
        /// <param name="fld">objet champ du champ de liason</param>
        private void CreateCTEHighiest(TableLite tab, FieldLite fld)
        {
            String sError = String.Empty;

            String sql = String.Concat("",
                "/*  ", V_CTE, " */", Environment.NewLine,
                " CREATE FUNCTION [dbo].[", GetCTEOrgaHighiestId_Name(tab.DescId, fld.Descid), "]", Environment.NewLine,
                " ( @nId as numeric )", Environment.NewLine,
                " RETURNS TABLE", Environment.NewLine,
                " as", Environment.NewLine,
                " RETURN", Environment.NewLine,
                " (", Environment.NewLine,
                "WITH mycte (myDataId,myParentDataId, myLib, myLvl, myPath)", Environment.NewLine,
                "      as", Environment.NewLine,
                "      (", Environment.NewLine,
                "            select distinct ", tab.ShortField, "id, ", fld.RealName, ", ", tab.ShortField, "01, 0 LVL,  cast('>' + CAST([", tab.ShortField, "ID] as varchar(10))  +'>' as varchar(max)) FROM [", tab.TabName, "] WHERE ", tab.ShortField, "id = @nId", Environment.NewLine,
                "            UNION ALL", Environment.NewLine,
                "            select ", tab.ShortField, "id, ", fld.RealName, ", ", tab.ShortField, "01, myLvl + 1 ,   mycte.mypath + '>' +  CAST([", tab.ShortField, "ID] as varchar(10))", Environment.NewLine,
                "            from mycte inner join [", tab.TabName, "] c on c.", tab.ShortField, "id = myParentDataId and CHARINDEX( '>'+ cast([", tab.ShortField, "ID] as varchar(10)) +'>',mycte.mypath)=0 ", Environment.NewLine,
                "      )", Environment.NewLine,
                "    select top 1 *", Environment.NewLine,
                "    from myCTE", Environment.NewLine,
                "    order by myCTE.myLvl desc", Environment.NewLine,
                ")", Environment.NewLine,
                ";"
                );

            try
            {
                _dal.OpenDatabase();
                _dal.ExecuteNonQuery(new RqParam(sql), out sError);
                if (sError.Length > 0)
                    throw new eOrganigrammeException("CREATE FUNCTION", sError);
            }
            finally
            {
                _dal.CloseDatabase();
            }

        }


        /// <summary>
        /// Suprime la cte s'il est plus vielle que la version en cours
        /// </summary>
        /// <param name="sCTEname"></param>
        private void DeleteOldCTE(string sCTEname)
        {
            string sSQL = "select name from  sys.objects obj inner join sys.sql_modules syssql on obj.object_id = syssql.object_id where  obj.name  = @name and definition not like '%" +  V_CTE + "%'";
            string sError = String.Empty;

            DataTableReaderTuned dtr = null;
            try
            {
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@name", SqlDbType.VarChar, sCTEname);
                _dal.OpenDatabase();
                dtr = _dal.Execute(rq, out sError);

                if (sError.Length > 0)
                    throw new eOrganigrammeException("ExecuteQuery", sError);

                if (dtr == null || !dtr.Read())
                    return;



                string sqlDelete = string.Concat("DROP FUNCTION ", sCTEname);

                _dal.ExecuteNonQuery(new RqParam(sqlDelete), out sError);


            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
                _dal.CloseDatabase();
            }
            return;
        }


        /// <summary>
        /// Génère en BDD la CTE retournant tous les ids organisés de l'arborescence
        /// </summary>
        /// <param name="tab">Table ou l'on souhaite voir l'organigramme</param>
        /// <param name="fld">objet champ du champ de liason</param>
        private void CreateCTEOrgaIds(TableLite tab, FieldLite fld)
        {
            String sError = String.Empty;
            String sql = String.Concat("",
                "/*  ", V_CTE, " */", Environment.NewLine,
                " CREATE FUNCTION [dbo].[", GetCTEOrgaIds_Name(tab.DescId, fld.Descid), "]", Environment.NewLine,
                " ( @nId as numeric )", Environment.NewLine,
                " RETURNS TABLE", Environment.NewLine,
                " as", Environment.NewLine,
                " RETURN", Environment.NewLine,
                " (", Environment.NewLine,
                "WITH mycte (myDataId,myParentDataId, myLib, myLvl, mypath)", Environment.NewLine,
                "      as", Environment.NewLine,
                "      (", Environment.NewLine,
                "            select distinct ", tab.ShortField, "id, ", fld.RealName, ", ", tab.ShortField, "01, 0 LVL , cast('>' + CAST([", tab.ShortField, "ID]  as varchar(10))  +'>' as varchar(max)) FROM [", tab.TabName, "] WHERE ", tab.ShortField, "id = @nId", Environment.NewLine,
                "            UNION ALL", Environment.NewLine,
                "            select ", tab.ShortField, "id, ", fld.RealName, ", ", tab.ShortField, "01, myLvl + 1 ,  mycte.mypath + '>' +  CAST([", tab.ShortField, "ID] as varchar(10)) ", Environment.NewLine,
                "            from mycte inner join [", tab.TabName, "] c on c. ", fld.RealName, "  = myDataId and c.", tab.ShortField, "id <> myDataId and CHARINDEX( '>'+ cast([", tab.ShortField, "ID] as varchar(10)) +'>',mycte.mypath) = 0 ", Environment.NewLine,
                "      )", Environment.NewLine,
                "    select *", Environment.NewLine,
                "    from myCTE", Environment.NewLine,
                ")", Environment.NewLine,
                ";"
                );

            try
            {
                _dal.OpenDatabase();
                _dal.ExecuteNonQuery(new RqParam(sql), out sError);
                if (sError.Length > 0)
                    throw new eOrganigrammeException("CREATE FUNCTION", sError);
            }
            finally
            {
                _dal.CloseDatabase();
            }

        }

        #endregion

        #region FCT Build
        /// <summary>
        /// Retourne l'id de la fiche la plus haute de l'arborescence
        /// </summary>
        /// <returns></returns>
        private Int32 GetHighestId()
        {
            String value = string.Empty;
            string sSQL = string.Concat("SELECT myDataId ID from ", GetCTEOrgaHighiestId_Name(_nTabId, _nDescId), "(@FileId)");

            string sError = string.Empty;
            RqParam rq = new RqParam(sSQL);

            rq.AddInputParameter("@FileId", SqlDbType.Int, _nFileId);
            _dal.OpenDatabase();
            DataTableReaderTuned dtr = _dal.Execute(rq, out sError);
            try
            {
                if (sError.Length > 0 || dtr == null || !dtr.Read())
                    throw new eOrganigrammeException("rqt", sError);

                value = dtr.GetString("ID");
            }
            catch (eOrganigrammeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new eOrganigrammeException("rqt ex", "Erreur sur GetUserValue - récup des informations en BDD", ex);
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
                _dal.CloseDatabase();
            }
            Int32 nHighstFileId = eLibTools.GetNum(value);
            if (nHighstFileId <= 0)
                throw new eOrganigrammeException("GetNum", "Le fileid n'est pas valide.");
            return nHighstFileId;
        }

        /// <summary>
        /// Retourne un dictionnaire id de fiches avec id de fiche parente de tous l'organigramme
        /// </summary>
        /// <param name="nMasterFileId">id de la fiche parente</param>
        /// <returns>dictionnaire id de fiches avec id de fiche parente de tous l'organigramme</returns>
        private Dictionary<Int32, Int32> GetIdsWithLvl(Int32 nMasterFileId)
        {
            Dictionary<Int32, Int32> listIdsWithLvl = new Dictionary<Int32, Int32>();
            string sError = string.Empty;
            RqParam rq = new RqParam(string.Concat("SELECT myDataId ID, myLvl LVL from ", GetCTEOrgaIds_Name(_nTabId, _nDescId), "(@FileId)"));
            rq.AddInputParameter("@FileId", SqlDbType.Int, nMasterFileId);
            _dal.OpenDatabase();
            DataTableReaderTuned dtr = _dal.Execute(rq, out sError);
            try
            {
                if (sError.Length > 0)
                    throw new eOrganigrammeException("rqt", sError);
                Int32 nCurrentFileId, nLevel;
                if (dtr != null)
                    while (dtr.Read())
                    {
                        nCurrentFileId = dtr.GetEudoNumeric("ID");
                        nLevel = dtr.GetEudoNumeric("LVL");
                        if (nCurrentFileId > 0)
                            listIdsWithLvl.Add(nCurrentFileId, nLevel);
                    }
            }
            catch (eOrganigrammeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new eOrganigrammeException("rqt ex", "Erreur sur GetUserValue - récup des informations en BDD", ex);
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
                _dal.CloseDatabase();
            }
            return listIdsWithLvl;
        }

        /// <summary>
        /// Génère le contenu de chaques Elements d'organigramme
        /// </summary>
        /// <param name="listIdsWithLvl">dictionnaire id de fiches avec id de fiche parente de tous l'organigramme</param>
        /// <returns>Elements d'organigrammes non trié</returns>
        private List<eOrganigrammeItem> EudoQuery_SetValues(Dictionary<Int32, Int32> listIdsWithLvl)
        {
            List<eOrganigrammeItem> listItem = new List<eOrganigrammeItem>();
            String query_sql = String.Empty;

            List<Field> _query_listFld = null;
            EudoQuery.EudoQuery query = eLibTools.GetEudoQuery(_pref, _nTabId, ViewQuery.CUSTOM);
            try
            {
                List<Int32> listDisplayedListCol = new List<Int32>();
                listDisplayedListCol.Add(_nTabId + 1);

                query.SetListCol = String.Concat(eLibTools.Join<Int32>(";", listDisplayedListCol), ";", _nDescId);
                query.AddCustomFilter(new WhereCustom("MAINID", Operator.OP_IN_LIST, eLibTools.Join<Int32>(";", new List<Int32>(listIdsWithLvl.Keys))));
                if (query.GetError.Length > 0)
                    throw new eOrganigrammeException("query init", query.GetError);
                query.LoadRequest();
                if (query.GetError.Length > 0)
                    throw new eOrganigrammeException("query LoadRequest", query.GetError);
                query.BuildRequest();
                if (query.GetError.Length > 0)
                    throw new eOrganigrammeException("query LoadRequest", query.GetError);
                query_sql = query.EqQuery;
                _query_listFld = query.GetFieldHeaderList;

                _dal.OpenDatabase();
                String sError = String.Empty;
                DataTableReaderTuned dtr = null;
                try
                {
                    dtr = _dal.Execute(new RqParam(query_sql), out sError);
                    if (sError.Length > 0)
                        throw new eOrganigrammeException("query ExecuteQuery", sError);
                    eRecord row;
                    eFieldRecord eFldRow;
                    eOrganigrammeItem currentItem;
                    String sCurrentValue;

                    while (dtr.Read())
                    {
                        // TODOHLA - Faire un datafillergeneric car pas toujours un eRecord !!
                        row = new eRecord();
                        row.FillComplemantaryInfos(query, dtr, _pref);

                        currentItem = new eOrganigrammeItem(row.MainFileid);

                        foreach (Field currentFld in _query_listFld)
                        {
                            eFldRow = eDataFillerTools.GetFieldRecord(_pref, query, dtr, currentFld, row, _pref.User);

                            if (eFldRow.FileId == row.MainFileid && listDisplayedListCol.Contains(currentFld.Descid))
                            {
                                if (_nTabId == TableType.PP.GetHashCode())
                                    sCurrentValue = eFldRow.DisplayValuePPName;
                                else
                                    sCurrentValue = eFldRow.DisplayValue;

                                currentItem.DisplayValue.Add(currentFld.Descid.ToString(), sCurrentValue);
                            }
                            else
                                if (currentFld.Descid == _nDescId)
                                currentItem.ParentId = eLibTools.GetNum(eFldRow.Value);
                        }

                        try
                        {
                            String beginFileNameVCARD = String.Concat(_nTabId, "_", row.MainFileid, "_*");    //Début du nom de fichier : TabId_FileId_
                            String physicalFullPathVCARD = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.VCARD, _pref), @"\");  //Chemin vers VCARD
                            String[] listFile = System.IO.Directory.GetFiles(physicalFullPathVCARD, beginFileNameVCARD);    //Récupération des fichiers correspondant à ce nom de fichier

                            if (listFile.Length > 0)    //Si au moin une image correspondante, on affiche la première qui nous est renvoyé.
                            {
                                FileInfo CurrentVCARDFileName = new System.IO.FileInfo(listFile[0]);
                                currentItem.ImgWebPath = String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.VCARD, _pref.GetBaseName), "/", CurrentVCARDFileName.Name);
                            }

                        }
                        catch { }

                        listItem.Add(currentItem);
                    }
                }
                finally
                {
                    if (dtr != null)
                        dtr.Dispose();
                    _dal.CloseDatabase();
                }
            }
            finally
            {
                if (query != null)
                    query.CloseQuery();
            }
            return listItem;
        }

        /// <summary>
        /// Permet d'obtenir tous les utilisateur/groupes enfants et sous enfants de la liste passé en paramètre sous forme d'une seule liste
        /// </summary>
        private eOrganigrammeItem ReorderChild(List<eOrganigrammeItem> listItem)
        {
            eOrganigrammeItem parentItem = null;
            foreach (eOrganigrammeItem currentItem in listItem)
            {
                if (parentItem == null && currentItem.ParentId <= 0)
                {
                    parentItem = currentItem;
                    break;
                }
            }

            if (parentItem == null)
            {
                parentItem = listItem[0];
                parentItem.ParentId = 0;

                //return parentItem;
            }
            InitAllChield(parentItem, listItem, 1);
            return parentItem;
        }

        /// <summary>
        /// Réordonne les enfants dans le parents en paramètre
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="listItem"></param>
        /// <param name="nLvl"></param>
        private void InitAllChield(eOrganigrammeItem parentItem, List<eOrganigrammeItem> listItem, Int32 nLvl)
        {
            foreach (eOrganigrammeItem currentItem in listItem)
            {
                if (parentItem.Id == currentItem.ParentId
                    && currentItem.ParentId != currentItem.Id
                    )
                {
                    parentItem.SubItem.Add(currentItem);
                    currentItem.Lvl = nLvl;
                    if (nLvl > _nMaxLvl)
                        _nMaxLvl = nLvl;
                    InitAllChield(currentItem, listItem, nLvl + 1);
                }
            }
        }
        #endregion

        #region FCT Génériques
        /// <summary>
        /// Vérificate qu'une CTE existe
        /// </summary>
        /// <param name="sCTEname">Nom de la CTE</param>
        /// <returns>Vrai si la CTE existe en BDD</returns>
        private Boolean checkCTEOrga(String sCTEname)
        {

            DeleteOldCTE(sCTEname);

            String sError = String.Empty;
            String sql = String.Concat("",
                "select 1  from sysobjects where ",
                "([xtype] = 'IF'  )",
                "and [name] = '", sCTEname, "' and id = object_id(N'[dbo].[", sCTEname, "]')"
                );
            DataTableReaderTuned dtr = null;
            try
            {
                _dal.OpenDatabase();
                dtr = _dal.Execute(new RqParam(sql), out sError);
                if (sError.Length > 0)
                    throw new eOrganigrammeException("ExecuteQuery", sError);
                if (dtr == null || !dtr.Read())
                    return false;

            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
                _dal.CloseDatabase();
            }
            return true;
        }
        #endregion

        #endregion
    }

    /// <className>eOrganigrammeItem</className>
    /// <summary>classe représentant le rendu d'un élément d'organigramme</summary>
    /// <authors>GCH</authors>
    /// <date>2014-05-05</date>
    public class eOrganigrammeItem
    {
        /// <summary>Données à afficher (clé : libellé de la rubrique, valeur : contenu de la rubrique)</summary>
        public Dictionary<String, String> DisplayValue { get; set; }
        /// <summary>Liste des Elements enfant de l'éléments en cours</summary>
        public List<eOrganigrammeItem> SubItem { get; set; }
        /// <summary>Id de la fiche correspondant à cet élément</summary>
        public Int32 Id { get; set; }
        /// <summary>Id de la fiche parente de cet élément</summary>
        public Int32 ParentId { get; set; }
        /// <summary>Niveau de cet élément dans l'organigramme</summary>
        public Int32 Lvl { get; set; }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="id">Id de la fiche correspondant à cet élément</param>
        public eOrganigrammeItem(Int32 id)
        {
            DisplayValue = new Dictionary<string, string>();
            SubItem = new List<eOrganigrammeItem>();
            Id = id;
            ParentId = -1;
            ImgWebPath = String.Empty;
        }
        /// <summary>Chemin web de l'Image à afficher dans l'organigramme</summary>
        public String ImgWebPath { get; set; }

    }
    /// <summary>
    /// Objet d'exception
    /// </summary>
    public class eOrganigrammeException : Exception
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="emp"></param>
        /// <param name="sMessage"></param>
        /// <param name="inner"></param>
        public eOrganigrammeException(String emp, String sMessage, Exception inner) : base(String.Concat(emp, " >>> ", sMessage), inner) { }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="emp"></param>
        /// <param name="sMessage"></param>
        public eOrganigrammeException(String emp, String sMessage) : base(String.Concat(emp, " >>> ", sMessage)) { }
    }
}