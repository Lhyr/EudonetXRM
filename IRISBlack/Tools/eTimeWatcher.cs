using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack
{
    public class eTimeWatcher : IDisposable
    {
        private Stopwatch stopwatch { get; set; }
        private string Message { get; set; }
        private static Stream myFile { get; set; }
        /// <summary>
        /// Initialise un stopwatch
        /// <paramref name="message"/>
        /// </summary>
        private eTimeWatcher(string message)
        {
            Message = message;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        /// <summary>
        /// INitialiseur statique pour la classe
        /// </summary>
        /// <paramref name="message"/>
        /// <returns></returns>
        public static eTimeWatcher InitTimeWatcher(string message = "")
        {
            if (eLibTools.IsLocalOrEudoMachine())
            {
                try
                {
                    InitDebugFile();
                }
                catch (Exception)
                {

                }
                return new eTimeWatcher(message);
            }

            return null;
        }

        /// <summary>
        /// Initialise la sortie du debug en fichier.
        /// </summary>
        [Conditional("DEBUG")]
        private static void InitDebugFile()
        {
            string sTracePath = Path.Combine(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT), eLibConst.EUDO_LOG_PATH + "-TimeWatcher.log");
            myFile = File.Open(sTracePath, FileMode.OpenOrCreate);
            TextWriterTraceListener myTextListener = new TextWriterTraceListener(myFile);
            Trace.Listeners.Add(myTextListener);
        }

        /// <summary>
        /// Permet d'écrire un message dans les logs.
        /// </summary>
        /// <param name="message"></param>
        [Conditional("DEBUG")]
        private void SetWriteToFile(string message)
        {
            // Write output to the file.
            Trace.WriteLine(message);
            Trace.Flush();

            if (myFile != null)
                myFile.Close();
        }

        /// <summary>
        /// Quand on détruit la classe.
        /// </summary>
        public void Dispose()
        {
            if (stopwatch != null)
            {
                stopwatch.Stop();
                StackFrame[] arSf = new StackTrace().GetFrames();
                string error = string.Join(" à ", arSf.Reverse().Select(sf => $"pour {sf.GetMethod().Name} de la classe {sf.GetFileName()} à la ligne {sf.GetFileLineNumber()}"));

                SetWriteToFile($"{Message} le temps d'execution a été de {stopwatch.ElapsedMilliseconds} ms pour {error} ");
            }
        }
    }
}