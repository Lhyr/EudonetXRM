using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu de Signet Web
    /// </summary>
    public class eWebBookmarkRenderer : eRenderer
    {
        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="eWebBkm"></param>
        public eWebBookmarkRenderer(ePref pref, eWebBookmark eWebBkm)
        {
            Pref = pref;
            _rType = RENDERERTYPE.WebBookmark;
            // MCR 39295 : pW:95 pH:95n, mettre  pW:100% (pour utiliser toute la largeur (width de l ecran), pH: 95% : pour eviter le chevauchement avec le menu contextuel : id="rightMenu" 

            try
            {
                Panel pnTitle = eBookmarkRenderer.CreateTitleBar(Pref, eWebBkm.OrigBkm, bZoomButton: true, sDivId: String.Concat("bkm_", eWebBkm.OrigBkm.CalledTabDescId), pW: 100, pH: 95);
                _pgContainer.Controls.Add(pnTitle);
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eWebBookmarkRenderer.constructor()>eBookmarkRenderer.CreateTitleBar : ", Environment.NewLine,
                    "bkm: ", eWebBkm.OrigBkm?.CalledTabDescId.ToString() ?? "null", "sDivId: ", String.Concat("bkm_", eWebBkm.OrigBkm?.CalledTabDescId), ", bZoomButton: true", Environment.NewLine,
                    e.Message, Environment.NewLine,
                    e.StackTrace, Environment.NewLine);
                _eException = e;
            }

            HtmlGenericControl iFrame = eTools.GetFieldIFrame(Pref, eWebBkm.URL, eWebBkm.OrigBkm.ParentFile.CalledTabDescId, eWebBkm.OrigBkm.CalledTabDescId, eWebBkm.OrigBkm.ParentFile.FileId);
            _pgContainer.Controls.Add(iFrame);
            _pgContainer.CssClass = "bkmdiv BkmWeb";
            iFrame.Attributes.Add("class", "BkmWebFrm");

        }
    }
}