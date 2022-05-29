using Com.Eudonet.Internal;
using System;
using System.Threading;
using System.Web.UI;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de gestion des statistiques
    /// </summary>
    public partial class eStatistics : eEudoPage
    {
        /// <summary>ressources utilisées en javascript</summary>
        public String _resAppJS = String.Empty;
        private string _fieldName = String.Empty;

        /// <summary>Nom eudonet du champ tel qu'il doit apparaitre dans la page</summary>
        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        private XmlDocument _xmlChart = new XmlDocument();

        /// <summary>Flux XML utilisé pour construire le fuison chart</summary>
        public String XmlChart
        {
            get
            {
                string _s = _xmlChart.OuterXml.Replace("'", @"\'");
                return _s;
            }
        }

        private Int32 _nTab;
        private Int32 _nTabFrom;
        private Int32 _nIdFrom;
        private Int32 _nOrigineTabFlter;
        private Int32 _nDescId;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// page d'affichage des statistiques
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css
            PageRegisters.AddCss("eReportWizard");
            PageRegisters.AddCss("eModalDialog");
            PageRegisters.AddCss("eStats", "all");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eStatsPrint", "print");
            PageRegisters.AddCss("syncFusion/ej.web.all.min");
            PageRegisters.AddCss("syncFusion/ej.responsive");
            PageRegisters.AddCss("syncFusion/ejgrid.responsive");
            PageRegisters.AddCss("syncFusion/default-responsive");
            #endregion

            #region ajout des js
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eStats");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eCharts");
            PageRegisters.AddScript("jquery.min");
            PageRegisters.AddScript("syncFusion/jsrender.min");
            PageRegisters.AddScript("syncFusion/ej.web.all.min");
            PageRegisters.RawScrip.AppendLine("").Append(string.Concat("top._CombinedZ = '", eLibConst.COMBINED_Z, "';"));
            PageRegisters.RawScrip.AppendLine("").Append(string.Concat("top._CombinedY = '", eLibConst.COMBINED_Y, "';"));
            switch (_pref.LangServId)
            {
                case 0:

                    PageRegisters.AddScript("syncFusion/ej.culture.fr-FR.min");
                    PageRegisters.AddScript("syncFusion/ej.localetexts.fr-FR");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'fr-FR';");
                    break;
                case 1:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                case 2:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.de-DE");
                    PageRegisters.AddScript("syncFusion/ej.culture.de-DE.min");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'de-DE';");
                    break;
                case 3:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                case 4:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.es-ES");
                    PageRegisters.AddScript("syncFusion/ej.culture.es-ES.min");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'es-ES';");
                    break;
                case 5:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                default:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
            }
            #endregion

            #region Variables de session

            Int32 _groupId = _pref.User.UserGroupId;
            Int32 _userLevel = _pref.User.UserLevel;
            String _lang = _pref.Lang;
            Int32 _userId = _pref.User.UserId;
            String _instance = _pref.GetSqlInstance;
            String _baseName = _pref.GetBaseName;

            #endregion

            #region Recuperation des paramètres

            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            _nTabFrom = _requestTools.GetRequestFormKeyI("tabfrom") ?? 0;
            _nIdFrom = _requestTools.GetRequestFormKeyI("idfrom") ?? 0;
            _nDescId = _requestTools.GetRequestFormKeyI("descid") ?? 0;
            _nOrigineTabFlter = _nDescId - (_nDescId % 100);
            //BSE: 58 353
            //if (_nTab != _nOrigineTabFlter)
            //    _nTab = _nOrigineTabFlter;
            #endregion

            try
            {
                eRenderer er = eRendererFactory.CreateStatRenderer(_pref, _nTab, _nDescId, _nTabFrom, _nIdFrom);

                if (er.ErrorMsg.Length == 0)
                {
                    while (er.PgContainer.Controls.Count > 0)
                    {
                        DivGlobal.Controls.Add(er.PgContainer.Controls[0]);
                    }
                }

            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
            catch (Exception ex)
            {
                try
                {
                    LaunchError(eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 6342),
                                eResApp.GetRes(_pref, 1395),
                                String.Concat("Erreur non gérée eStatistics.aspx - Construction de syncfusionChart - : ", Environment.NewLine, ex.ToString())));
                }
                // #95 988 - Quand on appelle un LaunchError, il faut intercepter l'eEndResponseException renvoyée par eRendererXMLHTML.RenderResult(),
                // sinon, elle sera récupérée par Application_* dans Global.asax.cs et renvoyée en erreur 500 brute à IIS.
                // Et donc, sur les serveurs de production, on aura une page d'erreur IIS brute non personnalisée, sans aucun détail de l'erreur pourtant transmise...
                catch (eEndResponseException)
                {

                }
                catch (ThreadAbortException) { }
            }
        }
    }
}