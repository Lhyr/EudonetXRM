using System.IO;
using System.Net;
using System.Web;
using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Internal;
using EudoExtendedClasses;

namespace Com.Eudonet.Xrm.mgr.external
{
    /// <summary>
    /// Retourne un binary de l'image demandé
    /// </summary>
    public class eImgMgr : eExternalManager, IHttpHandler 
    {


        /// <summary>
        /// Gestion du retourn
        /// </summary>
        protected override void ProcessManager()
        {

            string sHash = _requestTools.GetRequestQSKeyS("hsh") ?? "";

            string sCryto = _requestTools.GetRequestQSKeyS("cc") ?? "";
            string sHsh = _requestTools.GetRequestQSKeyS("hh") ?? "";
            string sBaseName = "";
            string sPjParam = "";

            eLibConst.FOLDER_TYPE ft = eLibConst.FOLDER_TYPE.FILES;
            try
            {
                string sClear = CryptoTripleDES.Decrypt(sCryto, CryptographyConst.KEY_CRYPT_LINK_TRACKING);
                sBaseName = sClear.Split("||")[1];
                sPjParam = sClear.Split("||")[2];
                int nType;

                int.TryParse(sClear.Split("||")[3], out nType);

                ft = eLibTools.GetEnumFromCode<eLibConst.FOLDER_TYPE>(nType, true);
            }
            catch
            {
                sBaseName = "";
                sPjParam = "";
            }

            string sDataPath = eLibTools.GetRootPhysicalDatasPath(_context);
            string sFullFilePath = "";

            if (sBaseName.Length == 0 || sPjParam == "")
            {
                sFullFilePath = Path.Combine(eLibTools.GetRootAppliPath(_context), "themes", "default", "images", "404.png");
                if (string.IsNullOrEmpty(sPjParam))
                    sPjParam = "404.png";
            }
            else
            {
                string dbDatasDir = eLibTools.GetDatasDir(sBaseName.ToString());
                sFullFilePath = Path.Combine(sDataPath, dbDatasDir, ft.ToString(), sPjParam);

                if (!File.Exists(sFullFilePath))
                    sFullFilePath = Path.Combine(eLibTools.GetRootAppliPath(_context), "themes", "default", "images", "404.png");
                
            }



            //envoie le fichier
            if (File.Exists(sFullFilePath))
            {
                _context.Response.Clear();
                _context.Response.ClearContent();
                _context.Response.ClearHeaders();
                
                _context.Response.AddHeader("Content-Disposition", "inline; filename=" + HttpUtility.HtmlEncode(sPjParam) + ";");
                
                _context.Response.AddHeader("Content-Length", new FileInfo(sFullFilePath).Length.ToString());
                string sMimeType = eLibTools.GetMimeTypeFromExtension(Path.GetExtension(sPjParam));
                _context.Response.ContentType = sMimeType; ;
                _context.Response.TransmitFile(sFullFilePath);
                _context.Response.StatusCode = HttpStatusCode.OK.GetHashCode();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            else
            {
                _context.Response.Clear();
                _context.Response.ClearContent();
                _context.Response.ClearHeaders();
                _context.Response.ContentType = "application/octet-stream";
                _context.Response.AddHeader("Content-Disposition", "attachment;filename=404.png;");


              //  _context.Response.AddHeader("Content-Length", new FileInfo(sFullFilePath).Length.ToString());
              //  _context.Response.TransmitFile(sFullFilePath);

                _context.Response.StatusCode = HttpStatusCode.OK.GetHashCode();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }


        /// <summary>
        /// pas de check des infos : la page peut être appellé sans token
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateExternalLoad()
        {
            return false;
        }

        /// <summary>
        /// non reusable
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}