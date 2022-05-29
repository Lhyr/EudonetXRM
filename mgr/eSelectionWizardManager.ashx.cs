using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;


namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eSelectionWizardManager
    /// </summary>
    public class eSelectionWizardManager : eEudoManager
    {
        XmlNode _baseResultNode;
        int _nTab;
        int _nTabSource;

        /// <summary>ACTION_LOADINVIT = 0</summary>
        const int ACTION_LOADINVIT = 0;
        /// <summary>ACTION_SELECTINVIT = 1</summary>
        const int ACTION_SELECTINVIT = 1;
        /// <summary>ACTION_SELECTALLINVIT = 2</summary>
        const int ACTION_SELECTALLINVIT = 2;

        public enum SELECT_ACTION
        {
            ACTION_LOADINVIT = 0,
            ACTION_SELECTINVIT = 1,
            ACTION_SELECTALLINVIT = 2
        }

        protected override void ProcessManager()
        {
            // BASE DU XML DE RETOUR            
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            _baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(_baseResultNode);

            //Dictionnaire de paramètre
            ExtendedDictionary<String, String> dicParam = new ExtendedDictionary<String, String>();
            LoadCommonParam(dicParam);


            // Paramètres obligatoires


            //Type d'action pour le manager
            Int32 iAction = 0;
            SELECT_ACTION action = SELECT_ACTION.ACTION_LOADINVIT;
            if (!(dicParam.ContainsKey("action") && Int32.TryParse(_context.Request.Form["action"].ToString(), out iAction)))
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "Action"));
                LaunchError();
            }
            else
            {
                action = (SELECT_ACTION)iAction;
            }

            if (!dicParam.ContainsKey("tab"))
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "Tab"));
                LaunchError();
            }



            switch (action)
            {
                case SELECT_ACTION.ACTION_LOADINVIT: GetSelectionList(dicParam);
                    break;
                case SELECT_ACTION.ACTION_SELECTALLINVIT: AddAllToSelection(dicParam); GetSelectionList(dicParam); break;
            }


            XmlNode _successNode = xmlResult.CreateElement("success");
            _baseResultNode.AppendChild(_successNode);
            _successNode.InnerText = "1";

            RenderResult(RequestContentType.XML, delegate() { return xmlResult.OuterXml; });

        }


        /// <summary>
        /// Charge les paramètres communs aux différents appels
        /// </summary>
        /// <param name="dicParam"></param>
        private void LoadCommonParam(ExtendedDictionary<String, String> dicParam)
        {
            //Signet Invitation
            if (_requestTools.AllKeys.Contains("tab"))
            {
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out _nTab);
                dicParam.Add("tab", _context.Request.Form["tab"].ToString());
            }

            //descid Event
            if (_requestTools.AllKeys.Contains("tabsource"))
            {
                Int32.TryParse(_context.Request.Form["tabsource"].ToString(), out _nTabSource);
                dicParam.Add("tabsource", _context.Request.Form["tabsource"].ToString());
            }

            //Largeur disponible pour la liste
            if (_requestTools.AllKeys.Contains("width"))
                dicParam.Add("width", _context.Request.Form["width"].ToString());

            //Hauteur disponible pour la liste
            if (_requestTools.AllKeys.Contains("height"))
                dicParam.Add("height", _context.Request.Form["height"].ToString());

            // Nb de lignes
            if (_requestTools.AllKeys.Contains("rows"))
                dicParam.Add("rows", _context.Request.Form["rows"].ToString());

            //Page demandée
            if (_requestTools.AllKeys.Contains("page"))
                dicParam.Add("page", _context.Request.Form["page"].ToString());

            // Action demandée
            if (_requestTools.AllKeys.Contains("action"))
                dicParam.Add("action", _context.Request.Form["action"].ToString());

            // Filtres éventuels
            if (_requestTools.AllKeys.Contains("filters"))
            {
                dicParam.Add("filters", _context.Request.Form["filters"].ToString());
            }

            // Recharge-t-on la carte ?
            if (_requestTools.AllKeys.Contains("reloadmap"))
            {
                dicParam.Add("reloadmap", _context.Request.Form["reloadmap"].ToString());

            }
                
        }

        


        private void GetSelectionList(ExtendedDictionary<String, String> dicParam)
        {
            //Retourne le flux HTML
            String sError = "";

            eRenderer eRet = eSelectionWizardRenderer.BuildListPart(_pref, dicParam, out sError);

            if (eRet.ErrorMsg.Length == 0 && eRet.InnerException == null)
                RenderResultHTML(eRet.PgContainer, true);
            else
            {
                StringBuilder sDevMsg = new StringBuilder();
                StringBuilder sUserMsg = new StringBuilder();

                sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append(eRet.ErrorMsg);

                if (eRet.InnerException != null)
                    sDevMsg.AppendLine(eRet.InnerException.Message).AppendLine(eRet.InnerException.StackTrace);

                sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));


                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    sUserMsg.ToString(),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    sDevMsg.ToString()

                    );

                LaunchError();
            }
        }


        /// <summary>
        /// Ajoute une adresse à sélection
        /// </summary>
        /// <param name="dicParam">Dictionnaire des parametre pour l'ajout/suppression en masse</param>
        private void AddAllToSelection(ExtendedDictionary<String, String> dicParam)
        {


            eFilteredSelection selection = eFilteredSelection.GetFilteredSelection(_pref, _nTab, _nTabSource);

            if (!selection.SelectAllFiles(dicParam)) // remonte l'exception
                throw new Exception(String.Concat("erreur d'ajout des invitations : ", selection.ErrorMsg), selection.InnerException);


            //_nNbInvit = selection.NbAll;
        }

    }
}