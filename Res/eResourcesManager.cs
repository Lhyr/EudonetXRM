using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classes de gestion des ressources
    /// </summary>
    public static class eResourcesManager
    {
        /// <summary>
        /// Méthode permettant la mise à jour de la table des ressources
        /// </summary>
        /// <param name="sqInstance"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool ResUpdate(string sqInstance, out string err)
        {
            err = string.Empty;

            ePrefSQL pSql = ePrefTools.GetDefaultPrefSql("EUDORES", sqInstance);

            //Connexion à EUDORES
            eudoDAL edalRes = eLibTools.GetEudoDAL(pSql);

            try
            {
                edalRes.OpenDatabase();

                edalRes.StartTransaction(out err);
                if (err.Length != 0)
                    return false;

                try
                {
                    RqParam rqTruncate = new RqParam("TRUNCATE TABLE [RESAPP]");
                    edalRes.AddToTransaction(rqTruncate);
                    edalRes.ExecuteNonQuery(rqTruncate, out err);
                    if (err.Length != 0)
                        throw new Exception(err);

                    //Boucle sur toutes les ressources
                    XmlDocument _res = new XmlDocument();
                    _res.Load(AppDomain.CurrentDomain.BaseDirectory + "\\res\\eResources.xml");


                    XmlReader xmlFile = XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "\\res\\eResources.xml", new XmlReaderSettings());



                    DataSet ds = new DataSet();
                    ds.ReadXml(xmlFile);

                    xmlFile.Close();

                    DataTable dt = ds.Tables[0];
                    //Table res : Attention, nom de colonne case sensitive
                    DataTable dt2 = new DataTable();
                    dt2.Columns.Add("ResId", typeof(System.Int32));
                    dt2.Columns.Add("Id_Lang", typeof(System.Int32));
                    dt2.Columns.Add("Lang", typeof(System.String));

                    //UnPivot
                    foreach (DataRow dr in dt.Rows)
                    {
                        foreach (DataColumn dc in dt.Columns)
                        {
                            if (dc.ColumnName.ToUpper().StartsWith("LANG"))
                            {
                                int nLangId;
                                if (int.TryParse(dc.ColumnName.ToUpper().Replace("LANG_", ""), out nLangId))
                                {
                                    DataRow dr2 = dt2.NewRow();
                                    dr2["ResId"] = dr[0];
                                    dr2["Id_Lang"] = nLangId;
                                    dr2["lang"] = dr[dc];
                                    dt2.Rows.Add(dr2);
                                }
                            }
                        }
                    }


                    edalRes.ExecuteBulkInsert(dt2, "RESAPP", out err);
                    if (err.Length != 0)
                        throw new Exception(err);

                    RqParam rqTruncLang = new RqParam("TRUNCATE TABLE [LANG]");

                    edalRes.AddToTransaction(rqTruncLang, out err);
                    if (err.Length != 0)
                        throw new Exception(err);

                    edalRes.ExecuteNonQuery(rqTruncLang, out err);
                    if (err.Length != 0)
                        throw new Exception(err);

                    RqParam rqLang = new RqParam("INSERT INTO [EUDORES]..[LANG] (id,libelle) select  Id_Lang ,lang from [eudores]..resapp where resid=0 order by resid");

                    edalRes.AddToTransaction(rqLang, out err);
                    if (err.Length != 0)
                        throw new Exception(err);

                    edalRes.ExecuteNonQuery(rqLang, out err);
                    if (err.Length != 0)
                        throw new Exception(err);

                    edalRes.CommitTransaction(out err);
                    if (err.Length != 0)
                        throw new Exception(err);
                }
                catch (Exception exp)
                {
                    err = "Impossible de mettre à jour les ressources : " + exp.ToString();

                    string errRoll = string.Empty;
                    edalRes.RollBackTransaction(out errRoll);
                    if (errRoll.Length > 0)
                        err = string.Concat(err, Environment.NewLine, "RoolBack impossible : ", errRoll);
                }
            }
            finally
            {
                edalRes.CloseDatabase();
            }

            eResApp.Reset(pSql);

            return err.Length == 0;
        }

        /// <summary>
        /// Retourne une requête paramétrée pour insérer une ressource
        /// </summary>
        /// <param name="resNode"></param>
        /// <returns></returns>
        private static IEnumerable<RqParam> SetResApp(XmlNode resNode)
        {

            int nResid = -1;
            if (!int.TryParse(resNode.SelectSingleNode("ID").InnerText, out nResid))
                throw new Exception("Fichier de ressources non valide");


            List<RqParam> lst = new List<RqParam>();

            foreach (XmlNode _fld in resNode.ChildNodes)
            {
                if (_fld.Name.ToUpper().StartsWith("LANG_"))
                {


                    RqParam rqRes = new RqParam("INSERT INTO [RESAPP] (ResId, Id_Lang, Lang) select @resid, @ID_LANG, @LANG");

                    int nLangId;
                    int.TryParse(_fld.Name.ToUpper().Replace("LANG_", ""), out nLangId);

                    string sLang = _fld.InnerText.Trim();
                    if (sLang.Length > 0)
                    {

                        rqRes.AddInputParameter("@RESID", SqlDbType.Int, nResid);
                        rqRes.AddInputParameter("@ID_LANG", SqlDbType.VarChar, nLangId);
                        rqRes.AddInputParameter("@LANG", SqlDbType.VarChar, _fld.InnerText);
                        lst.Add(rqRes);
                    }
                }
            }

            return lst;
        }

        /// <summary>
        /// Retourne un flux d'export des ressources system
        /// </summary>
        /// <param name="_sqInstance">Instance sql du serveur</param>
        /// <param name="err">Message d'erreyr</param>
        /// <returns>Flux format XML des ressources system</returns>
        public static string ResExport(string _sqInstance, out string err)
        {
            err = string.Empty;

            DataTableReaderTuned dtr = null;
            StringBuilder result = new StringBuilder();

            //Connexion à EUDORES            
            eudoDAL dalRes = ePrefTools.GetDefaultEudoDal("EUDORES", _sqInstance);
            dalRes.OpenDatabase();

            try
            {
                dtr = dalRes.Execute(new RqParam("Select * From [APP]"), out err);

                result.AppendLine("<ressources>");
                while (dtr.Read())
                {
                    result.AppendLine("<res>");

                    var colsValues = dtr.GetLineValues();
                    foreach(var item in colsValues)
                    {
                        result.AppendLine("<" + item.Key.ToUpper() + "><![CDATA[" + item.Value.GetString() + "]]></" + item.Key.ToUpper() + ">");
                    }

                    result.AppendLine("</res>");
                }
                result.AppendLine("</ressources>");
            }
            catch { }
            finally
            {
                dtr?.Dispose();
                dalRes.CloseDatabase();
            }

            return result.ToString();
        }
    }


}