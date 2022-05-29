using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminPmAdrMapping
    /// </summary>
    public class eAdminPmAdrMapping : eAdminManager
    {
        private Action _action;
        private ePmAddressMapping _mapping;

        enum Action
        {
            Init = 0,
            Refresh = 1,
            AddLine = 2,
            Update = 4
        }
        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        protected override void ProcessManager()
        {
            _action = _requestTools.GetRequestFormEnum<Action>("a");
            if (_action == Action.Init)
            {
                PageRegisters.RegisterFromRoot = true;
                PageRegisters.AddCss("eudoFont");
                PageRegisters.AddCss("eAdminMenu");
                PageRegisters.AddCss("eAdminPmAdrMapping");
                PageRegisters.AddScript("eMain");
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("eModalDialog");
                PageRegisters.RegisterAdminIncludeScript("eAdminPmAdrMapping");
                AddHeadAndBody = true;
            }
            eAdminPmAdrMappingRenderer rdr;
            _mapping = JsonConvert.DeserializeObject<ePmAddressMapping>(_requestTools.GetRequestFormKeyS("m") ?? "");
            try
            {
                if (_action == Action.Update) {
                    eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                    dal.OpenDatabase();
                    try {
                        DescAdvObj obj = DescAdvObj.GetSingle((int)TableType.ADR, DESCADV_PARAMETER.PM_ADR_MAPPING, _mapping.ToJsonString());
                        obj.Save(dal);
                    }
                    catch
                    {
                        throw;
                    }
                    finally {
                        dal.CloseDatabase();
                    }

                    return;
                }
                else if (_action == Action.Refresh)
                {
                    //utilise le mapping transmis à la page lorsque celui-ci n'a pas encore été enregistré
                    rdr = eAdminRendererFactory.CreateAdminPmAdrMappingRenderer(_pref, _mapping, false);
                    rdr.PgContainer.Controls.Clear();
                    rdr.PgContainer.Controls.Add(rdr.GetMappingTable());
                }
                else if (_action == Action.AddLine)
                {
                    rdr = eAdminRendererFactory.CreateAdminPmAdrMappingRenderer(_pref, _mapping, false);
                    rdr.PgContainer.Controls.Clear();
                    System.Web.UI.WebControls.Table tb = new System.Web.UI.WebControls.Table();
                    tb.Rows.Add(rdr.GetMatchingLine(new ePmAddressMapping.FieldMatching()));
                    rdr.PgContainer.Controls.Add(tb);
                }
                else {
                    rdr = eAdminRendererFactory.CreateAdminPmAdrMappingRenderer(_pref); 
                }

                RenderResultHTML(rdr.PgContainer);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
            }
        }
    }
}