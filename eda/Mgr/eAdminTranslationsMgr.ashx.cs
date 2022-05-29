using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminTranslationsDialog
    /// </summary>
    public class eAdminTranslationsMgr : eAdminManager
    {
        private eAdminTranslationsRenderer.ACTION _action = eAdminTranslationsRenderer.ACTION.Initial;

        private Int32 _iDescid = 0;
        private Int32 _fileID = 0;

        /// <summary>Diffère du DescId pour les catalogues avancés et les specifs</summary>
        private Int32 _iResid = 0;
        private eAdminTranslation.NATURE _nature;
        private Int32 _iLangId = -1;
        private string _sSearch = "";
        private List<eAdminTranslationsList.OrderBy> _sorts;


        protected override void ProcessManager()
        {

            if (_requestTools.AllKeys.Contains("action"))
            {
                RendererAction();
            }
            else
            {
                UpdateAction();
            }


        }

        private void RendererAction()
        {
            #region récupération des paramètres
            _action = _requestTools.GetRequestFormEnum<eAdminTranslationsRenderer.ACTION>("action");
            _nature = _requestTools.GetRequestFormEnum<eAdminTranslation.NATURE>("nature");
            _iDescid = _requestTools.GetRequestFormKeyI("descid") ?? 0;
            _fileID = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
            _iResid = _requestTools.GetRequestFormKeyI("resid") ?? 0;
            _iLangId = _requestTools.GetRequestFormKeyI("lang") ?? -1;
            _sSearch = _requestTools.GetRequestFormKeyS("search") ?? "";
            String sSorts = _requestTools.GetRequestFormKeyS("sorts");
            if (!String.IsNullOrEmpty(sSorts))
                _sorts = JsonConvert.DeserializeObject<List<eAdminTranslationsList.OrderBy>>(sSorts);
            #endregion

            if (_action == eAdminTranslationsRenderer.ACTION.Initial)
            {
                AddHeadAndBody = true;

                #region ajout des css et js

                PageRegisters.RegisterFromRoot = true;
                PageRegisters.RegisterAdminIncludeScript("eAdminTranslations");
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eMain");
                PageRegisters.AddScript("eList");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("eModalDialog");

                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eList");
                PageRegisters.AddCss("eIcon");
                PageRegisters.AddCss("eButtons");
                PageRegisters.AddCss("eControl");
                PageRegisters.AddCss("eAdmin");
                PageRegisters.AddCss("eAdminMenu");
                PageRegisters.AddCss("eAdminTranslations");

                BodyCssClass = "adminModal bodyWithScroll";
                OnLoadBody = "nsAdminTranslations.initUpdateClick();";

                #endregion
            }


            eAdminTranslationsRenderer trdr = eAdminTranslationsRenderer.CreateAdminTranslationsRenderer(_pref, _action);
            trdr.DescId = _iDescid;
            trdr.Nature = _nature;
            trdr.ResId = _iResid;
            trdr.LangId = _iLangId;
            trdr.Search = _sSearch;
            if (_fileID > 0)
                trdr.FileId = _fileID;
            if (!String.IsNullOrEmpty(sSorts))
            {
                trdr.Sorts = _sorts;
            }

            trdr.Generate();
            if (trdr.ErrorMsg.Length > 0)
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 7657), title: " ", devMsg: trdr.ErrorMsg));


            RenderResultHTML(trdr.PgContainer);

        }

        private void UpdateAction()
        {
            StreamReader sr = new StreamReader(_context.Request.InputStream);
            string s = sr.ReadToEnd();
            String sError = string.Empty;
            eAdminTranslation translation = new eAdminTranslation();
            eAdminResult res = new eAdminResult();
            try
            {
                translation = JsonConvert.DeserializeObject<eAdminTranslation>(s);
                res = translation.Update(_pref);
            }
            catch (JsonReaderException e)
            {
                //throw exc;
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace, Environment.NewLine, "chaine d'entrée : ", s);
                //LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 7657), title: " ", devMsg: sError));

                res.Success = false;
                res.UserErrorMessage = eResApp.GetRes(_pref, 1760);
                res.DebugErrorMessage = sError;
                res.Criticity = 0;
                res.InnerException = e;


            }

            if (!res.Success)
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), res.UserErrorMessage, title: " ", devMsg: res.DebugErrorMessage));


            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(res);
            });

        }

    }
}