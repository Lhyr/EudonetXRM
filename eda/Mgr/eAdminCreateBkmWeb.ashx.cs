using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using Com.Eudonet.Core.Model;
using System.Collections.Generic;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminCreateBkmWeb
    /// </summary>
    public class eAdminCreateBkmWeb : eAdminManager
    {

        protected override void ProcessManager()
        {

            Boolean bInterPP, bInterPM;
            String sLabel;
            String sURL;
            String sError;

            Int32 iParentTab = 0;
            Int32.TryParse(_context.Request.Form["parenttab"], out iParentTab);
            if (iParentTab == 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), "Veuillez réitérer l'opération.", devMsg: String.Concat("pas de parenttab : ", _context.Request.Form["parenttab"])));
            }

            string sType = "";
            if (_requestTools.AllKeys.Contains("subtype") && !String.IsNullOrEmpty(_context.Request.Form["subtype"]))
                sType = _context.Request.Form["subtype"];

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            bInterPP = iParentTab == TableType.PP.GetHashCode();
            bInterPM = iParentTab == TableType.PM.GetHashCode();

            Result r = new Result();

            string sUrl = sType == "1" ? "http://www.eudonet.com" : "specif.aspx";
            sLabel = sType == "2" ? eResApp.GetRes(0, 2578) : "Nouveau Signet Web";

            r.BkmDescid = eSqlDesc.CreateBkmWeb(dal,
                                    bInterPP,
                                    bInterPM,
                                    (bInterPP || bInterPP ? 0 : iParentTab),
                                    sLabel,
                                    sUrl,
                                    sType,
                                    out sError);



            if (sError.Length > 0)
            {
                r.Success = false;
                r.Error = sError;

            }
            else
            {
         
                r.Success = true;
            }

            // Si le signet est créé alors on crée la grille            
            if (r.Success && sType == "2")
                CreateGrid(r);

            RenderResult(RequestContentType.TEXT, () => JsonConvert.SerializeObject(r));

        }

        /// <summary>
        /// Creation de la grille pour le signet grille
        /// </summary>
        /// <param name="bkmDescid"></param>
        /// <param name="dal"></param>
        private void CreateGrid(Result result)
        {
            Engine.Engine eng = eModelTools.GetEngine(_pref, (int)TableType.XRMGRID, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = 0;

            eng.AddParam("DatabaseUid", _pref.DatabaseUid);
            eng.AddNewValue((int)XrmGridField.ParentTab, result.BkmDescid.ToString());
            eng.AddNewValue((int)XrmGridField.DisplayOrder, "1");
            eng.AddNewValue((int)XrmGridField.Title, eResApp.GetRes(_pref, 7977));
            eng.EngineProcess(new StrategyCruSimple());
            EngineResult engResult = eng.Result;


            // Création echouée
            if (engResult.Error != null && (engResult.Error.DebugMsg.Length != 0 || engResult.Error.Msg.Length != 0))
            {
                result.Success = false;
                result.Error = eResApp.GetRes(_pref, 1801);
            }
        }

        public class Result
        {
            public Boolean Success;
            public Int32 BkmDescid;
            public String Error;
        }
    }
}