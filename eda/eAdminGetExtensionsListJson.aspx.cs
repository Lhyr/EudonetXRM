using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Page permettant de générer le JSON des extensions pour les serveurs déconnectés
    /// </summary>
    public partial class eAdminGetExtensionsListJson : eEudoPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                    throw new EudoAdminInvalidRightException();

                int CurrentPage = 0;
                int ExtensionsPerPage = 22;
                int extensionsCount = 0;
                int extensionsPages = 0;
                //StoreListCriteres Criteres = new StoreListCriteres();

                eAPIExtensionStoreAccess storeAccess = new eAPIExtensionStoreAccess(_pref);                
                List<eAdminExtension> _listExtensions = new List<eAdminExtension>();

                do
                {
                    ++CurrentPage;
                    _listExtensions.AddRange(
                        storeAccess.GetExtensionList(CurrentPage, ExtensionsPerPage, new StoreListCriteres(), out extensionsCount, out extensionsPages)
                        );
                } while ((CurrentPage * ExtensionsPerPage) < extensionsCount);
                string json = JsonConvert.SerializeObject(_listExtensions, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);

                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;

                Response.Charset = null;
                Response.AddHeader("Content-Length", buffer.Length.ToString());
                Response.AddHeader("Content-Disposition", "attachment;filename*=UTF-8''ExtensionList.json");
                Response.ContentType = eLibTools.GetMimeTypeFromExtension("json");
                Response.BinaryWrite(buffer);

                Response.Flush(); // Sends all currently buffered output to the client.
                Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
                HttpContext.Current.ApplicationInstance.CompleteRequest(); // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.

                //Response.End();

                //Response.Clear();
                //Response.ContentType = "text/plain";
                //Response.OutputStream.Write(buffer, 0, buffer.Length);

            }
            catch(EudoAdminInvalidRightException ex)
            {
                Response.Clear();
                Response.Write(ex.Message);
            }
            catch(Exception ex)
            {
                Response.Clear();
                Response.Write(ex.Message + "<br /><br />" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }
    }


}