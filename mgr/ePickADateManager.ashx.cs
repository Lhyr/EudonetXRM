using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de ePickADateManager
    /// </summary>
    public class ePickADateManager : eEudoManager
    {

        protected override void ProcessManager()
        {
            PageRegisters.RegisterFromRoot = true;            
            PageRegisters.AddCss("eCalendar");
            PageRegisters.AddCss("ePickADate");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("ePickADate");
            PageRegisters.AddScript("eCalendar");
            AddHeadAndBody = true;
            OnLoadBody = "nsPickADate.loadCalendar();";

            String sDate = "";
            Int32 iFrom;
            ePickADateRenderer.From from = ePickADateRenderer.From.None;

            if (_requestTools.AllKeys.Contains("date"))
                sDate = HttpUtility.HtmlDecode(_context.Request.Form["date"].ToString());

            if (_requestTools.AllKeys.Contains("from") && Int32.TryParse(_context.Request.Form["from"].ToString(), out iFrom))
                from = (ePickADateRenderer.From)iFrom;
            
            eRenderer padre = eRendererFactory.CreatePickADateRenderer(_pref, from, sDate);


            RenderResultHTML(padre.PgContainer);


        }

    }
}