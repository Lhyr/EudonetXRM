using Com.Eudonet.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;


namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de edaDOSListCheck
    /// </summary>
    public class edaDOSListCheck : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string strRemoteAdr = context.Request.ServerVariables["remote_addr"];
            string ipv4 = eLibTools.GetUserIPV4();

            bool blocal = eLibTools.IsLocalMachine(strRemoteAdr)
                || strRemoteAdr.Equals("127.0.0.1") // localhost ipv4
                || strRemoteAdr.Equals("::1") //localhostipv6
                || strRemoteAdr.Equals(context.Request.ServerVariables["LOCAL_ADDR"]);

            bool bEudo = eLibConst.ADR_IP_EUDOWEB.ContainsKey(strRemoteAdr);


            bool reset = (context.Request.QueryString["rc"]?.ToString() ?? "0") == "1";
            bool resetBaned = (context.Request.QueryString["rb"]?.ToString() ?? "0") == "1";
            bool resetTrac = (context.Request.QueryString["rt"]?.ToString() ?? "0") == "1";
            bool resetTracBan = (context.Request.QueryString["rtb"]?.ToString() ?? "0") == "1";


            bool renderJSON = (context.Request.QueryString["json"]?.ToString() ?? "0") == "1";

            List<DoSJSONDico> lst = new List<DoSJSONDico>();

            if (blocal || bEudo)
            {
                var dL = eDoSProtection.GetInstance;
                var dT = eDosProtection.GetInstance();

                 
                if (reset)
                    dL.ResetCall();


                if (resetTrac)
                    dT.Reset();

                if (resetBaned)
                    dL.ResetBanned();
                 
                int nbT = 0;
                if (!renderJSON)
                    context.Response.Write("Tracking : ");
                foreach (var kvp in dT.GetInfo)
                {

                    lst.Add(new DoSJSONDico()
                    {
                        Type = "Tracking",
                        Key = String.Concat(kvp.Key.UserIP, " ", kvp.Key.QueryString),
                        Cmpt = kvp.Value.Cnt_1,
                        Cmpt2 = kvp.Value.Cnt_2,
                        DateInfo = kvp.Value.Date

                    });

                    if (!renderJSON)
                    {
                        context.Response.Write("<br/>");
                        context.Response.Write(String.Concat(nbT, "  ***  ", kvp.Key.UserIP, " ", kvp.Key.QueryString, "-----", kvp.Value.Cnt_1, "/", kvp.Value.Cnt_2));

                        context.Response.Flush();
                    }

                    nbT++;
                }
                

                nbT = 0;
                if (!renderJSON)
                    context.Response.Write(" <br/><br/>CALL :");
                foreach (var kvp in dL.GetDicCall)
                {                   
                    lst.Add(new DoSJSONDico()
                    {
                        Type = "Appel Page",
                        Key = kvp.Key,
                        Cmpt = kvp.Value.Count,
                        Cmpt2 = 0 ,
                        DateInfo = kvp.Value.DateLastCall

                    });

                    if (!renderJSON)
                    {
                        context.Response.Write("<br/>");
                        context.Response.Write(string.Concat(nbT, " *** ", kvp.Key, " ------ ", kvp.Value.Count));
                    }
                    nbT++;
                }

                if (!renderJSON)
                {
                    context.Response.Write("<br/><br/>");
                    context.Response.Write("BANNED :");
                }

                foreach (var kvp in dL.GetDicBanned)
                {
                    lst.Add(new DoSJSONDico()
                    {
                        Type = "Appel Page",
                        Key = kvp.Key,
                        Cmpt = 0,
                        Cmpt2 = 0,
                        DateInfo = kvp.Value

                    });

                    if (!renderJSON)
                    {
                        context.Response.Write("<br>");
                        context.Response.Write(string.Concat(nbT, " *** ", kvp.Key, " ------ ", kvp.Value));

                    }


                    nbT++;

                }

                if (renderJSON)
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(JsonConvert.SerializeObject(lst,Formatting.Indented));
                }
                else
                {



 
                 
                }

            }
        }






        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }


    public class DoSJSONDico
    {
        public string Type;
        public string Key;
        public int Cmpt;
        public int Cmpt2;
        public DateTime DateInfo;
        
    }
}