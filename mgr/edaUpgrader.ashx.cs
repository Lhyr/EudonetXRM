using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr.UPDT
{
    /// <summary>
    /// Description résumée de edaUpgrader
    /// </summary>
    public class edaUpgrader : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {

        /// <summary>
        /// traitement de la requete : mise à jour des base demandées
        /// </summary>
        /// <param name="context">Contexte HTTP</param>
        public void ProcessRequest(HttpContext context)
        {


            //Thread.Sleep(60000);
            //  context.Server.ScriptTimeout = 6000; //100 minutes

            string strRemoteAdr = context.Request.ServerVariables["remote_addr"];
            //string ipv4 = eLibTools.GetUserIPV4();

            bool blocal = eLibTools.IsLocalMachine(strRemoteAdr)
                || strRemoteAdr.Equals("127.0.0.1") // localhost ipv4
                || strRemoteAdr.Equals("::1") //localhostipv6
                || strRemoteAdr.Equals(context.Request.ServerVariables["LOCAL_ADDR"]);

            bool bEudoweb = eLibConst.ADR_IP_EUDOWEB.ContainsKey(strRemoteAdr);


            if (!(
                blocal
                //|| bEudo
                )
                )
                context.Response.End();


            String sCptWhere = String.Empty;

            // toutes les bases ou spécif une base
            String sBAse = context.Request.QueryString["b"] ?? "";

            if (sBAse == "")
            {
                context.Response.Write("AUCUNE BASE CHOISIE");
                return;

            }

            if (sBAse != "ALL")
            {
                Regex myBase = new Regex(@"^EUDO_[A-Za-z0-9_-]+$");

                if (!myBase.IsMatch(sBAse))
                {
                    context.Response.Write("AUCUNE BASE CHOISIE");
                    return;

                }
                else
                {

                    sCptWhere = " AND [DIRECTORY] = @BASE ";
                }
            }
            //check local call



            // Application de hotfix
            Boolean hotfix = (context.Request.QueryString["hotfix"] ?? "0") == "1";

            // On supprime après execution
            //Boolean oneshot = (context.Request.QueryString["oneshot"] ?? "0") == "1";


            string sLstBdd = String.Concat("SELECT [UID],[directory], [sqlserverinstancename], Version, sys.* FROM [DATABASES]  db ",
                            " INNER JOIN SYS.DATABASES SYS ON SYS.[NAME] collate french_ci_ai = db.[DIRECTORY] collate french_ci_ai ",
                             " WHERE[VERSION] LIKE '10.%' AND [Version] ", hotfix ? "<=" : "!=", " @VERSION  and state_desc = 'ONLINE' ", sCptWhere);
            RqParam rqpQuery = new RqParam();
            rqpQuery.SetQuery(sLstBdd);
            rqpQuery.AddInputParameter("@VERSION", System.Data.SqlDbType.VarChar, eConst.VERSION);

            rqpQuery.AddInputParameter("@BASE", System.Data.SqlDbType.VarChar, sBAse);

            eudoDAL dalEudoLog = ePrefTools.GetDefaultEudoDal("EUDOLOG");

            Dictionary<String, DBForUpdate> dicBase = new Dictionary<string, DBForUpdate>();

            try
            {
                dalEudoLog.OpenDatabase();

                string sError = String.Empty;
                DataTableReaderTuned dtrEudolog = dalEudoLog.Execute(rqpQuery, out sError);

                if (!String.IsNullOrEmpty(sError))
                    throw dalEudoLog.InnerException ?? new Exception(sError);

                if (dtrEudolog.HasRows)
                    while (dtrEudolog.Read())
                        dicBase[dtrEudolog.GetString(0)] = new DBForUpdate()
                        {
                            BaseName = dtrEudolog.GetString(1),
                            Instance = string.IsNullOrEmpty(dtrEudolog.GetString(2)) ? "." : dtrEudolog.GetString(2)
                        };



                context.Response.Write("/************************   DEBUT MAJ VERSION [" + eConst.VERSION + "] *******************************************************/<br/><br/>");
                context.Response.Flush();

                int nbFailed = 0;
                int nb = 0;
                KeyValuePair<String, DBForUpdate> kvp;
                for (int i = 0; i < dicBase.Count; i++)
                {
                    kvp = dicBase.ElementAt(i);
                    context.Response.Flush();

                    nb++;

                    context.Response.Write("------------------------------------------------------------------------------" + "<br/>" + Environment.NewLine);
                    context.Response.Write(" Base (" + nb + "/" + dicBase.Count + ") à traiter :  [" + kvp.Value.BaseName + "]   <br/> " + Environment.NewLine);

                    eudoDAL currentDB = null;

                    try
                    {
                        //
                        currentDB = ePrefTools.GetDefaultEudoDal(kvp.Value.BaseName, kvp.Value.Instance);
                        currentDB.OpenDatabase();


                        //vérification de la version de la base
                        string sql = "SELECT VERSION FROM [CONFIG] WHERE USERID = 0 ";
                        RqParam rqVersion = new RqParam(sql);

                        string sVersion = currentDB.ExecuteScalar<String>(rqVersion, out sError);
                        if (sError.Length > 0)
                            throw currentDB.InnerException ?? new Exception(sError);



                        context.Response.Write(">>   Version en cours :  " + sVersion + "<br/>" + Environment.NewLine);

                        if (sVersion.StartsWith("7."))
                        {
                            //A partir de la 7.803, on autorisme le passage directe v7->XRM
                            if (String.Compare(sVersion, "7.803.000") >= 0)
                                sVersion = "10.000.000";
                        }

                        // On réexecute les scripts de montée de version si hotfix
                        if (String.Compare(sVersion, eConst.VERSION) < 0 || hotfix)
                        {

                            ePrefSQL myPrefSQL = ePrefTools.GetDefaultPrefSql(kvp.Value.BaseName, kvp.Value.Instance);
                            eUpgrader eUpg = new eUpgrader(myPrefSQL, @".\..\version");
                            try
                            {
                                //Maj
                                eUpg.Process(hotfix);

                                //Intégrité
                                (new eLauncher(myPrefSQL)).Do();

                                context.Response.Write(">>   MISE A JOUR AVEC SUCCES " + "<br/>" + Environment.NewLine);



                                // relance la recherche de version
                                sVersion = currentDB.ExecuteScalar<String>(rqVersion, out sError);

                                if (sError.Length > 0)
                                    throw currentDB.InnerException ?? new Exception(sError);


                                if (hotfix)
                                {
                                    context.Response.Write(">>   Hotfix appliqué avec succès sur la version :  " + eUpg.WorkingVersion + "<br/>" + Environment.NewLine);

                                    // si en oneshot et que c'est le dernier on supprime le repertoire hotfix
                                    //if (oneshot && i == (dicBase.Count - 1))                                  
                                    //    eUpg.deleteHotfixDirectory();

                                }
                                else
                                {
                                    context.Response.Write(">>   Nouvelle Version :  " + sVersion + "<br/>" + Environment.NewLine);
                                }
                            }
                            catch (Exception e)
                            {
                                nbFailed++;

                                context.Response.Write(@"  !!! /ECHEC\  !!! " + "<br/><br/>" + Environment.NewLine);
                                context.Response.Write("  MESSAGE : " + Environment.NewLine);
                                context.Response.Write(e.Message + "<br/>" + "<br/>" + Environment.NewLine + Environment.NewLine);
                                context.Response.Write("  STACKTRACE : " + Environment.NewLine);
                                context.Response.Write(e.StackTrace + "<br/>" + Environment.NewLine);
                                context.Response.Write("------------------------------------------------------------------------------" + "<br/>" + Environment.NewLine);


                                // relance la recherche de version
                                sVersion = currentDB.ExecuteScalar<String>(rqVersion, out sError);

                                if (sError.Length > 0)
                                    throw currentDB.InnerException ?? new Exception(sError);


                                context.Response.Write(">>   DERNIERE VERSION  PASSEE:  " + sVersion + "<br/>" + Environment.NewLine);

                            }
                        }
                        else
                        {

                            if (sVersion.StartsWith("7."))
                                context.Response.Write(">>  BASE EN V7 - LA BASE DOIT ETRE AU MOINS EN 7.803.<br/>" + Environment.NewLine);
                            else
                                context.Response.Write(">>  VERSION A JOUR <br/>" + Environment.NewLine);

                        }
                    }
                    catch (Exception e)
                    {
                        // incrémente le nombre d'échec
                        nbFailed++;

                        context.Response.Write(@"  !!! /ECHEC\  !!! " + "<br/><br/>" + Environment.NewLine);
                        context.Response.Write("  MESSAGE : " + Environment.NewLine);
                        context.Response.Write(e.Message + "<br/>" + "<br/>" + Environment.NewLine + Environment.NewLine);
                        context.Response.Write("  STACKTRACE : " + Environment.NewLine);
                        context.Response.Write(e.StackTrace + "<br/>" + Environment.NewLine);

                    }
                    finally
                    {
                        if (currentDB != null)
                            currentDB.CloseDatabase();

                        context.Response.Write("------------------------------------------------------------------------------" + "<br/><br/>" + Environment.NewLine);
                    }


                    //Si trop de base en erreur, on arrête la montée auto.
                    if (nbFailed > 5)
                    {

                        context.Response.Write("------------------------------------------------------------------------------");
                        context.Response.Write(" TROP DE MONTE DE VERSION EN ERREUR - ARRET DE LA PROCEDURE AUTOMATIQUE       ");
                        context.Response.Write("------------------------------------------------------------------------------");
                        break;
                    }
                }
                context.Response.Write("---------------- FIN MAJ ------------------------------------<br/>");
                //if(dtrEudolog.)


                context.Response.Write("---------------- MAJ DES RESSOURCES ------------------------------------<br/>");

                //http://sph2-pc/xrm.dev/res/resmanu.aspx

            }
            catch (Exception e)
            {
                context.Response.Write(@"  !!! /ECHEC CRITIQUE \  !!! " + "<br/><br/>" + Environment.NewLine);

                context.Response.Write("  MESSAGE : " + Environment.NewLine);
                context.Response.Write(e.Message + "<br/>" + "<br/>" + Environment.NewLine + Environment.NewLine);

                context.Response.Write("  STACKTRACE : " + Environment.NewLine);
                context.Response.Write(e.StackTrace + "<br/>" + Environment.NewLine);

            }
            finally
            {
                if (dalEudoLog != null)
                    dalEudoLog.CloseDatabase();
            }
        }

        /// <summary>
        /// Methodes system
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    internal struct DBForUpdate
    {
        public string BaseName;
        public string Instance;
    }

}