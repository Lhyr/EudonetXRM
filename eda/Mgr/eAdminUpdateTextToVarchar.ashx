<%@ WebHandler Language="C#" Class="Com.Eudonet.Xrm.eda.eAdminUpdateTextToVarchar" %>
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Transforme les champs de type text vers varchar (ou nvarchar)
    /// </summary>
    public class eAdminUpdateTextToVarchar : eAdminManager
    {
        /// <summary>
        /// Traitement de maj
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                //Doit être admin
                if (_pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                    throw new EudoAdminInvalidRightException();

                //Doit être local ou eudo
                if (!eLibTools.IsLocalOrEudoMachine(_context))
                    throw new EudoAdminInvalidRightException();

                string sConfirm = _requestTools.GetRequestQSKeyS("confirm");
                string sMailConfirm = _requestTools.GetRequestQSKeyS("mail");



                if (string.IsNullOrEmpty(sConfirm) && string.IsNullOrEmpty(sMailConfirm))
                    return;

                if (!string.IsNullOrEmpty(sConfirm))
                {
                    if (
                             _context.Session["updatemail"] != null && _context.Session["updatenotec"] != null &&
                            sConfirm == _context.Session["updatenotec"].ToString() && sMailConfirm == _context.Session["updatemail"].ToString())
                    {
                        //do treatment
                        if (doTreat(sMailConfirm))
                        {


                            _context.Response.Write("Action terminée. Tous les champs de type text / ntext ont été transformés");
                        }
                    }
                    else
                    {
                        _context.Response.Write("Erreur de code");
                    }

                    return;
                }

                if (!string.IsNullOrEmpty(sMailConfirm))
                {
                    _context.Session["updatenotec"] = eLibTools.GetToken(16);
                    sMailConfirm = sMailConfirm.ToLower();

                    if (sMailConfirm.EndsWith("eudonet.com") || sMailConfirm.EndsWith("eudoweb.com"))
                    {
                        if (sendMail(sMailConfirm))
                            _context.Response.Write("Un mail d'instruction vous a été envoyé<br/>");
                        else
                            _context.Response.Write("Action impossible");
                    }
                    else
                        _context.Response.Write("Action impossible - Le mail ne peut être envoyée que sur une adresse eudonet.com.");


                    return;
                }

            }
            catch (EudoAdminInvalidRightException)
            {
                _context.Response.Write("Action impossible : doit être lancé depuis un compte surperadmin sur une ip Eudonet ou depuis la machine locak");
            }
            catch (Exception e)
            {
                if (_pref != null && _pref.User != null && _pref.User.UserLevel > UserLevel.LEV_USR_ADMIN.GetHashCode())
                    _context.Response.Write("Action impossible : " + e.Message);
                else
                    _context.Response.Write("Action impossible");
            }
        }



        private bool doTreat(string smail)
        {
            var l = getlist();
            if (l.Count > 0)
            {
                using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
                {
                    dal.OpenDatabase();
                    bool res = false;
                    string serr = "";
                    try
                    {

                        eLibTools.SetTran(_pref, dal, "TEXT_TO_VARCHAR");


                        string sLog = "*** Lancement de transformation (n)Text to (n)Varchar *** <br/>" + Environment.NewLine;
                        sLog += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " - Utilisateur " + _pref.User.UserLogin + " - " + _pref.UserId + " - email : " + smail + " - code : " + _context.Session["updatenotec"].ToString() + Environment.NewLine;

                        string sBody = "*** Transformation (n)Text to (n)Varchar *** <br/>" + Environment.NewLine;
                        sBody += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " - Utilisateur " + _pref.User.UserLogin + " - " + _pref.UserId + " - email : " + smail + "<br/>" + Environment.NewLine;

                        EngineSender eng = eMailSender.GetMailSender(_pref);

                        string lst = "";
                        foreach (var t in l)
                        {
                            _context.Response.Write("Updating : " + t.Item1 + "." + t.Item2 + "  (" + t.Item3 + ") <br/>");
                            _context.Response.Flush();

                            RqParam rq = new RqParam(String.Concat("ALTER TABLE [", t.Item1, "] ALTER COLUMN [", t.Item2, "] ", t.Item3));
                            int n = dal.ExecuteNonQuery(rq, out serr);
                            if (serr.Length > 0)
                                throw dal.InnerException ?? new Exception(serr);

                            lst += rq.GetSqlCommandText() + Environment.NewLine;
                            sBody += rq.GetSqlCommandText() + "<br/> " + Environment.NewLine;

                        }
                        sBody += " FIN : " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "<br/> " + Environment.NewLine;
                        sBody += "****************************************" + Environment.NewLine;
                        res = true;

                        eng.Subject = "Demande de changement des types Note de (n)text vers (n)varchar";
                        eng.SubjectEncoding = System.Text.Encoding.UTF8;
                        eng.Body = sBody;
                        eng.BodyEncoding = System.Text.Encoding.UTF8;
                        eng.To = new eMailAddressConteneur(smail);
                        eng.Cc = new eMailAddressConteneur("dev@eudonet.com");
                        eng.From = new eMailAddressInfos();

                        eng.From.Mail = "no-reply@eudonet.com";

                        System.Net.Mail.SmtpStatusCode smtpStatusCode;
                        string sReturnMsg = "";
                        string sProcesMsg = "";

                        if (!eng.Send(out smtpStatusCode, out sReturnMsg, out sProcesMsg))
                        {
                            res = false; //Si pas de mail, rollback
                            _context.Response.Write("Action impossible");
                        }


                        sLog += lst;
                        sLog += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        sLog += "******************************" + Environment.NewLine;
                        string datasRootPah = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT);
                        eLibTools.EudoTraceLog(HtmlTools.StripHtml(sLog),
                          datasRootPah,
                            _pref.GetBaseName,
                            _pref.User.UserLogin,
                            "TEXT_TO_VARCHAR_EXECUTEY"
                                );
                    }
                    catch (Exception e)
                    {
                        res = false;
                        if (_pref != null && _pref.User != null && _pref.User.UserLevel > UserLevel.LEV_USR_ADMIN.GetHashCode())
                            _context.Response.Write("Action impossible : " + e.Message);
                        else
                            _context.Response.Write("Action impossible");

                    }
                    finally
                    {

                        eLibTools.CommitOrRollBack(dal, res, "TEXT_TO_VARCHAR");




                        if (dal != null)
                            dal.CloseDatabase();
                    }
                    return res;

                }
            }
            else
            {
                _context.Response.Write("Aucun champ à traiter");
                return false;
            }

            return false;
        }

        private List<Tuple<string, string, string>> getlist()
        {
            List<Tuple<string, string, string>> l = new List<Tuple<string, string, string>>();
            string sQL =
                String.Concat(" select  ", Environment.NewLine,
              " syst.tname  , syst.cname  , ", Environment.NewLine,
                        "case ", Environment.NewLine,
                        "	when syst.ctype = 35 then ' VARCHAR(MAX) '", Environment.NewLine,
                        "	when syst.ctype = 99 then ' NVARCHAR(MAX) '", Environment.NewLine,
                        "	else NULL", Environment.NewLine,
                        " end as ntype ", Environment.NewLine,
                 " from ", Environment.NewLine,
                " [desc] 	inner join", Environment.NewLine,
                 " ( ", Environment.NewLine,
                " select st.name tname,sc.name cname,sty.user_type_id ctype, sty.name ctypename from sys.objects st ", Environment.NewLine,
                " inner join sys.columns sc on st.object_id = sc.object_id", Environment.NewLine,
                " inner join sys.types sty on sty.user_type_id = sc.user_type_id) syst", Environment.NewLine,
                " on syst.cname collate french_ci_ai = [desc].[field]  collate french_ci_ai and syst.tname collate french_ci_ai = [desc].[file]   collate french_ci_ai", Environment.NewLine,
                " where ctype in (35,99)", Environment.NewLine,
                "	and ([File] in('PP','PM','ADDRESS')", Environment.NewLine,
                "	or [File] like 'EVENT%'", Environment.NewLine,
                "	or [File] like 'TEMPLATE%')", Environment.NewLine);


            RqParam rq = new RqParam(sQL);
            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();
                try
                {
                    DataTableReaderTuned dt = dal.Execute(rq);

                    if (dt.HasRows)
                    {
                        while (dt.Read())
                        {
                            l.Add(new Tuple<string, string, string>(dt.GetString(0), dt.GetString(1), dt.GetString(2)));
                        }

                        return l;
                    }

                }
                finally
                {
                    if (dal != null)
                        dal.CloseDatabase();
                }
                return l;

            }
        }
        private bool sendMail(string smail)
        {
            EngineSender eng = eMailSender.GetMailSender(_pref);
            var l = getlist();
            if (l.Count > 0)
            {

                string sBody = string.Concat
                ("<b>**** ATTENTION ****.</b><br/>", Environment.NewLine,
                "<b/>Vous demandez une action qui peut avoir d'importantes conséquences sur l'application.</b> <br/>" + Environment.NewLine,
                "<b>Cette action sera journalisée et vous engage.</b><br/>*************<br/><br/>" + Environment.NewLine,
               "Ne la lancez sur une base de production qu'après l'avoir testé sur une base d'homologation.<br/>" + Environment.NewLine,

               "Les effets peuvent être à la fois sur l'application mais aussi (liste non exhaustive) sur <ul><li>les spécif #net,",
                    "<li>v7" +
                    "<li>ORM" +
                    "<li>Automatismes" + "" +
                    "<li>Outils de synchro.</ul><br/>" + Environment.NewLine);


                _context.Response.Write(sBody + "Un email contenant un lien de confirmation va vous être envoyé.<br/><br/>" + Environment.NewLine);

                string sLst = "";
                foreach (var t in l)
                    sLst += t.Item1 + "." + t.Item2 + "  (" + t.Item3 + ")<br/>" + Environment.NewLine;

                _context.Response.Write(sLst);


                string sLink = eLibTools.GetToken(16);
                _context.Session["updatenotec"] = sLink;
                _context.Session["updatemail"] = smail;


                eng.Subject = "Demande de changement des types Note de (n)text vers (n)varchar";
                eng.SubjectEncoding = System.Text.Encoding.UTF8;
                eng.Body = String.Concat(sBody,
                    Environment.NewLine,
                    "Les champs suivants sont impactés : " + sLst,

                    "Voici le code de confirmation '", sLink, "'  Merci de consulter les architectes/perm dev pour son utilisation.",
                    Environment.NewLine,
                    "Cette action sera journalisé avec l'email et le compte super-admin utilisé."
                    );




                eng.BodyEncoding = System.Text.Encoding.UTF8;

                eng.To = new eMailAddressConteneur(smail);
                eng.From = new eMailAddressInfos();
                eng.From.Mail = "no-reply@eudonet.com";

                System.Net.Mail.SmtpStatusCode smtpStatusCode;
                string sReturnMsg = "";
                string sProcesMsg = "";


                string sLog = "*** Demande de transformation (n)Text to (n)Varchar *** <br/>" + Environment.NewLine;
                sLog += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " - Utilisateur " + _pref.User.UserLogin + " - " + _pref.UserId + " - email : " + smail + " - code : " + sLink + Environment.NewLine;
                sLog += sLst;
                sLog += "******************************" + Environment.NewLine;
                string datasRootPah = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT);
                eLibTools.EudoTraceLog(HtmlTools.StripHtml(sLog),
                  datasRootPah,
                    _pref.GetBaseName,
                    _pref.User.UserLogin,
                    "TEXT_TO_VARCHAR_QUERY"
                        );

                return eng.Send(out smtpStatusCode, out sReturnMsg, out sProcesMsg);

            }
            else
            {
                _context.Response.Write("Aucun champ à traiter <br/>");
                return false;
            }

        }


    }
}