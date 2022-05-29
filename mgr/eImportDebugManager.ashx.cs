using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.IO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant de créer ou mettre à jour un widget
    /// </summary>
    public class eImportDebugManager : eEudoManager
    {
        /// <summary>
        /// Creation mise à jour des widgets
        /// </summary>
        protected override void ProcessManager()
        {
            string str = string.Empty;
#if DEBUG

            eRequestTools tools = new eRequestTools(_context);
            FileStream FileStream = null;
            StreamReader StreamReader = null;            
            try
            {
                FileStream = File.Open("d:\\datas\\import.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader = new StreamReader(FileStream);
                str = StreamReader.ReadToEnd();
            }
            catch (Exception ex)
            {

                str = ex.Message + " " + ex.StackTrace;
            }
            finally
            {
                FileStream?.Close();
                StreamReader?.Close();
            }

            str = str.Replace("\n", "<br/>");
#endif 
            RenderResult(RequestContentType.TEXT, delegate () { return str; });
        }
    }
}