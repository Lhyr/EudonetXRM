using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Permet l'envoie d'un feedback
    /// </summary>
    public class eSendFeedback : eEudoManager
    {

#if DEBUG
        private const int MIN_TIME = 5;
#else
    private const int MIN_TIME = 30;
#endif
        private int _nType = 0;
        private string _sContent = "";

        private FEEDBACK_TYPE FD_TYPE = FEEDBACK_TYPE.UNEDEFINED;

        private eReturnSendFeedBack res = new eReturnSendFeedBack();

        protected override void ProcessManager()
        {

            StreamReader sr = new StreamReader(_context.Request.InputStream);
            //string s = HttpUtility.UrlDecode(sr.ReadToEnd());
            string s = sr.ReadToEnd();


            try
            {
                var def = new { content = "", type = 0 };
                var mySelection = JsonConvert.DeserializeAnonymousType(s, def);

                _nType = mySelection.type;
                _sContent = mySelection.content;
            }
            catch (JsonReaderException exc)
            {
                res.Success = false;
                res.ErrorTitle = eResApp.GetRes(_pref, 416);
                res.ErrorMsg = "";


                RenderResult(RequestContentType.SCRIPT, delegate ()
                {
                    return SerializerTools.JsonSerialize(res);
                });

            }


            FD_TYPE = eLibTools.GetEnumFromCode<FEEDBACK_TYPE>(_nType);

            //Pas de feedbackvide
            if (string.IsNullOrWhiteSpace(_sContent))
            {


                res.Success = false;
                res.ErrorTitle = eResApp.GetRes(_pref, 416);
                res.ErrorMsg = eResApp.GetRes(_pref, 2363).Replace("{RUB}", eResApp.GetRes(_pref, 1501));


                RenderResult(RequestContentType.SCRIPT, delegate ()
                {
                    return SerializerTools.JsonSerialize(res);
                });

                return;
            }

            #region Limite l'intervalle de temps entre chaque envoie de feedback

            if (_context.Session["dtfeedback"] != null)
            {
                long lastFeedback;
                if (long.TryParse(_context.Session["dtfeedback"].ToString(), out lastFeedback))
                {
                    TimeSpan ts = DateTime.Now - new DateTime(lastFeedback);

                    if (ts.TotalSeconds < MIN_TIME)
                    {
                        res.Success = false;
                        res.ErrorTitle = eResApp.GetRes(_pref, 72);
                        res.ErrorMsg = "Merci de patienter avant d'envoyer un nouveau feedback";


                        RenderResult(RequestContentType.SCRIPT, delegate ()
                        {
                            return SerializerTools.JsonSerialize(res);
                        });

                        return;
                    }
                }
            }
            _context.Session["dtfeedback"] = DateTime.Now.Ticks;
            #endregion


            switch (FD_TYPE)
            {

                case FEEDBACK_TYPE.THEME_IRIS:
                    res.Success = true;
                    res.ErrorTitle = "";
                    res.ErrorMsg = "";


                    EngineSender eng = eMailSender.GetMailSender(_pref);


                    eng.Subject = "Commentaire Nouveau thème [" + _pref.GetBaseName + " - " + _pref.User.UserLogin + "]"; ;
                    eng.SubjectEncoding = System.Text.Encoding.UTF8;
                    eng.Body = _sContent;
                    eng.BodyEncoding = System.Text.Encoding.UTF8;

#if DEBUG

                    eng.To = new eMailAddressConteneur("dev@eudonet.com");
                    eng.From = new eMailAddressInfos();
                    eng.From.Mail = "dev@eudonet.com";
#else
                    eng.To = new eMailAddressConteneur("marketing@eudonet.com");
                    eng.From = new eMailAddressInfos();
                    eng.From.Mail = "marketing@eudonet.com";
#endif
                    System.Net.Mail.SmtpStatusCode smtpStatusCode;
                    string sReturnMsg = "";
                    string sProcesMsg = "";

                    bool succes = eng.Send(out smtpStatusCode, out sReturnMsg, out sProcesMsg);
                    if (!succes || (int)smtpStatusCode != 0 && smtpStatusCode != System.Net.Mail.SmtpStatusCode.Ok)
                    {

                        res.Success = false;
                        res.ErrorTitle = eResApp.GetRes(_pref, 72);
                        res.ErrorMsg = eResApp.GetRes(_pref, 2393); //Le message n’a pas pu être envoyé, contacter votre administrateur afin de vérifier la configuration du service de messagerie. 


                        //Si adr eudo ou local, message d'erreur détailé pour admin local ou eudo
                        if (eLibTools.IsLocalOrEudoMachine(_context) && _pref.User.UserLevel >= 99)
                        {
                            //res.ErrorDetailMsg = smtpStatusCode.ToString() + " " + sReturnMsg;
                        }

                    }


                    RenderResult(RequestContentType.SCRIPT, delegate ()
                    {
                        return SerializerTools.JsonSerialize(res);
                    });
                    return;


                default:
                    res.Success = false;
                    res.ErrorTitle = eResApp.GetRes(_pref, 416);
                    res.ErrorMsg = "Type de feedback inconnu";


                    RenderResult(RequestContentType.SCRIPT, delegate ()
                    {
                        return SerializerTools.JsonSerialize(res);
                    });

                    break;
            }

        }



        public enum FEEDBACK_TYPE
        {
            UNEDEFINED = 0,

            THEME_IRIS = 1

        }

    }


    public class eReturnSendFeedBack : JSONReturnGeneric
    {

    }
}