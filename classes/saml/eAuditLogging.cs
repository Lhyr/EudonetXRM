using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Data;
using EudoExtendedClasses;
using static Com.Eudonet.Xrm.eTools;
using System.IO;
using System.Text;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Permet de logguer la connexion via saml2
    /// </summary>
    public class eAuditLogging
    {
        string _directory;
        string _direction;
        string _userlogin;
        string _sessionid;

        /// <summary>
        /// 0:info
        /// 1:warn
        /// 2:error
        /// </summary>
        int _level;

        string _tracePath;
        string _filename = "SAML.log";

        /// <summary>
        /// Récup de eudoDal pour EudoTrait
        /// </summary>
        public eAuditLogging(string sessionid, string userlogin, string directory, int level = 2)
        {
            _directory = directory;
            _level = level;
            _tracePath = Path.Combine(eTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT), eLibConst.EUDO_LOG_PATH, _directory);

            _userlogin = userlogin;
            if (string.IsNullOrEmpty(_userlogin))
                _userlogin = "UNKNWON";

            _sessionid = sessionid;
            if (string.IsNullOrEmpty(_sessionid))
                _sessionid = "NOTSET";
        }

        /// <summary>
        /// Log des informations sur la connexion ADFS
        /// </summary>
        /// <param name="message"></param>
        /// <param name="nLogLevel">Type de log</param>
        public void Log(string message, TypeLogExternalAuth nLogLevel)
        {

            message = string.Concat("[", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "] - ", _sessionid, " - ", nLogLevel.ToString(), " - ", _direction.ToUpper(), " - ", _userlogin.ToUpper(), " - ", message);

            if (!Directory.Exists(_tracePath))
                Directory.CreateDirectory(_tracePath);

            if (!String.IsNullOrEmpty(message))
                message = String.Concat(message, Environment.NewLine);

            try { File.AppendAllText(Path.Combine(_tracePath, _filename), message); } catch { }
        }
        /// <summary>
        /// Lis les infos de trace
        /// </summary>
        public string ReadTrace()
        {
            if (File.Exists(Path.Combine(_tracePath, _filename)))
            {
                try
                {

                    StringBuilder lines = new StringBuilder();
                    
                    string line;
                    using (StreamReader file = new StreamReader(Path.Combine(_tracePath, _filename)))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            if (!line.StartsWith("["))
                                continue;

                            if (line.Length > 200)
                                lines.Insert(0, $"{line.Substring(0, 200)}\n");
                            else
                                lines.Insert(0, $"{line}\n");
                        }
                    }

                    return lines.ToString();

                }
                catch (Exception e)
                {
                    return $"TraceFile : {e.Message}";
                }
            }

            return string.Empty;
        }


        public void Info(string message, string direction = "---")
        {
            if (_level > 0)
                return;

            _direction = direction;
            Log(message, eTools.TypeLogExternalAuth.INFO);
        }
        public void Warn(string message, string direction = "---")
        {
            if (_level > 1)
                return;

            _direction = direction;
            Log(message, eTools.TypeLogExternalAuth.WARNING);
        }

        public void Error(string message, string direction = "---")
        {
            if (_level > 2)
                return;

            _direction = direction;
            Log(message, eTools.TypeLogExternalAuth.ERROR);
        }
    }
}