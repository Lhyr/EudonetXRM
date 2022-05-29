using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.IO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// retourne le flux d'une pj
    /// </summary>
    public partial class getpj : System.Web.UI.Page
    {
        /// <summary>
        /// Page load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

#if DEBUG

#else
            if (!eDosProtection.GetInstance().DemandConnect(Request))
            {
                // Veuillez réessayer plus tard.
                Response.Write(eResApp.GetRes(0, 1773));
                Response.End();
            }
#endif

            string sUid = String.Empty;
            Int32 nPjId = 0;

            try
            {
                if (!String.IsNullOrEmpty(Request.QueryString["pj"]))
                {
                    string sPjParam = Request.QueryString["pj"];

                    //pour les test
                    /*
                    Dictionary<String, String> dicoTest = new Dictionary<string,string>();
                    dicoTest.Add("pjid", "2");
                    dicoTest.Add("uid", "EUDO_TEST");
                    string sTokenTest = ExternalPjTools.EncryptParam(dicoTest);
                    Dictionary<String, String> dico = ExternalPjTools.DecryptParam(sTokenTest);
                    */

                    Dictionary<String, String> dico = ExternalPjTools.DecryptParam(sPjParam);

                    if (dico.ContainsKey("uid"))
                        sUid = dico["uid"];

                    if (dico.ContainsKey("pjid"))
                        Int32.TryParse(dico["pjid"], out nPjId);
                }
            }
            catch (Exception  )
            {
                //throw ex;
            }

            eExternalPJ expj = new eExternalPJ();

            try
            {
                if (String.IsNullOrEmpty(sUid) || nPjId == 0)
                {
                    throw new Exception("Token Invalide");
                }
                else
                {
                    string sError = String.Empty;

                    expj.OpenDatabase();

                    Int32 nId = 0;
                    string sFilename = String.Empty;
                    if (!expj.GetPJFileName(sUid, nPjId, out nId, out sFilename, out sError))
                    {
                        throw new Exception(sError);
                    }

                    if (nId == 0 || String.IsNullOrEmpty(sFilename))
                    {
                        sError = "PJ Invalide.";
                        throw new Exception(sError);
                    }
                    else
                    {
                        //mise à jour de la fiche
                        if (!expj.UpdateAccessDateAndCounter(nId, out sError))
                        {
                            throw new Exception(sError);
                        }

                        //récupère dossier racine
                        string sDatasPath = String.Empty;
                        if (!expj.GetRootDatasPath(out sDatasPath, out sError))
                        {
                            throw new Exception(sError);
                        }

                        //génère dossier complet
                        if (String.IsNullOrEmpty(sDatasPath))
                        {
                            sError = "DatasPath non configuré sur ce serveur.";
                            throw new Exception(sError);
                        }
                        else
                        {
                            string sFullFilePath = Path.Combine(sDatasPath, sUid, sFilename);

                            //envoie le fichier
                            if (File.Exists(sFullFilePath))
                            {
                                Response.Clear();
                                Response.ClearContent();
                                Response.ClearHeaders();
                                Response.ContentType = "application/octet-stream";
                                Response.AddHeader("Content-Disposition", "attachment; filename=\"" + HttpUtility.HtmlEncode(sFilename) + "\";");
                                Response.AddHeader("Content-Length", new FileInfo(sFullFilePath).Length.ToString());
                                Response.TransmitFile(sFullFilePath);
                                Response.StatusCode = HttpStatusCode.OK.GetHashCode();
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                            }
                            else
                            {
                                sError = "Fichier non trouvé";
                                throw new Exception(sError);
                            }
                        }
                    }
                }
            }
            catch (eEndResponseException) { Response.End(); }
            catch (Exception ex)
            {
                //throw ex;
                Response.StatusCode = HttpStatusCode.NotFound.GetHashCode();
                Response.StatusDescription = ex.Message;
                Response.SuppressContent = true;
            }
            finally
            {
                expj.CloseDatabase();
            }
        }
    }
}