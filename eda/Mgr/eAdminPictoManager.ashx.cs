using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Newtonsoft.Json;
using System;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminPictoManager
    /// </summary>
    public class eAdminPictoManager : eAdminManager
    {

        protected override void ProcessManager()
        {
            int nTab = 0;
            int nDescId = 0;            
            String color = String.Empty;
            String icon = String.Empty;
            String error = String.Empty;
            eFontIcons.FontIcons font;
            String fontClassName = String.Empty;

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                nTab = eLibTools.GetNum(_context.Request.Form["tab"].ToString());
            else
            {
                throw new Exception("Paramètre 'tab' nécessaire à la mise à jour.");
            }

            if (_requestTools.AllKeys.Contains("descid") && !String.IsNullOrEmpty(_context.Request.Form["descid"]))
                nDescId = eLibTools.GetNum(_context.Request.Form["descid"].ToString());
            if (_requestTools.AllKeys.Contains("color") && !String.IsNullOrEmpty(_context.Request.Form["color"]))
                color = _context.Request.Form["color"].ToString();
            if (_requestTools.AllKeys.Contains("icon") && !String.IsNullOrEmpty(_context.Request.Form["icon"])) {
                icon = _context.Request.Form["icon"].ToString();
                font = eFontIcons.GetFontIcon(icon);
                fontClassName = font.CssName;
            }
                

            #region Enregistrement
            Boolean success = false;
            eAdminResult adminResult = new eAdminResult();
            try
            {
                if (nDescId == 0)
                    nDescId = nTab;
                eAdminDesc desc = new eAdminDesc(nDescId);
                desc.SetDesc(eLibConst.DESC.ICON, icon);
                desc.SetDesc(eLibConst.DESC.ICONCOLOR, color);
                adminResult = desc.Save(_pref, out error);

            }
            catch (Exception exc) {
                error = String.Concat("Erreur lors de l'enregistrement du picto : ", exc.Message);
            }

            #endregion

            #region Résultat
            eAdminPictoResult result = new eAdminPictoResult()
            {
                Success = adminResult.Success,
                Error = adminResult.UserErrorMessage,
                PictoClassName = fontClassName,
                PictoColor = color
            };


            RenderResult(RequestContentType.TEXT, delegate()
            {
                return JsonConvert.SerializeObject(result);
            });
            #endregion
            
        }


        public class eAdminPictoResult
        {
            public Boolean Success;
            public String Error;
            public String PictoClassName;
            public String PictoColor;
        }

    }
}