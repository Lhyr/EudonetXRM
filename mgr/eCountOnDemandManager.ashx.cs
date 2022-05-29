using System;
using System.Text;
using System.Threading;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{



    /// <summary>
    /// Compteur de fiches "à la demande"
    /// Pour le mode liste principal
    /// </summary>
    public class eCountOnDemandManager : eEudoManager
    {

        private Boolean _removeDoubles = true;

        private string _sLstTableMerged = "";

        /// <summary>
        /// Type de comptage demandée
        /// </summary>
        public enum TypeCount
        {
            /// <summary>
            /// Non spécifié
            /// </summary>
            UNDEFINED = 0,

            /// <summary>
            /// Pour la pagination en liste
            /// </summary>
            LIST_PAGING = 1,

            /// <summary>
            /// Pour les invités d'une nouvelle campagne
            /// </summary>
            CAMPAING_NEW = 2,

            /// <summary>
            /// Pour les invités d'une campagne existante
            /// </summary>
            CAMPAING_EXIST = 3
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {

            String error = String.Empty;
            TypeCount tp = TypeCount.UNDEFINED;
            if (_requestTools.AllKeys.Contains("operation") && !String.IsNullOrEmpty(_context.Request.Form["operation"]))
            {
                if (!Enum.TryParse(_context.Request.Form["operation"], true, out tp) || !Enum.IsDefined(typeof(TypeCount), tp))
                    tp = TypeCount.UNDEFINED;
            }


            if (_requestTools.AllKeys.Contains("removedoubles") && !String.IsNullOrEmpty(_context.Request.Form["removedoubles"]))
            {
                _removeDoubles = _context.Request.Form["removedoubles"].ToString() == "1";

                if(_removeDoubles && _requestTools.AllKeys.Contains("mergedtabs"))
                {
                    _sLstTableMerged = _context.Request.Form["mergedtabs"].ToString();

                }
            }

            switch (tp)
            {
                case TypeCount.UNDEFINED:
                    Error("Type de compteur non défini.", null);
                    break;
                case TypeCount.LIST_PAGING:
                    CountPaging();
                    break;
                case TypeCount.CAMPAING_NEW:
                    CountNewCampaign();
                    break;
                case TypeCount.CAMPAING_EXIST:
                    CountExistingCampaign();
                    break;
                default:
                    Error("Type de compteur non défini.", null);
                    break;
            }
        }


        /// <summary>
        /// Compte le nombre de mail à envoyer sur une campagne existante
        /// </summary>
        private void CountExistingCampaign()
        {

            Int32 nCampaignId = 0;

            if (_requestTools.AllKeys.Contains("campaignid") && !String.IsNullOrEmpty(_context.Request.Form["campaignid"]) && Int32.TryParse(_context.Request.Form["campaignid"], out nCampaignId))
            {
                try
                {
                    Int32 nbRecipients = eListFactory.GetCountRecipientsCampaign(_pref, nCampaignId, _context.Request.Form);
                    RenderResult(RequestContentType.TEXT, delegate()
                    {
                        return nbRecipients.ToString();
                    });
                }
                catch (eEndResponseException) { }
                catch (ThreadAbortException) { }
                catch (Exception e)
                {
                    Error("Id de campagne invalide.", e);
                }
            }
        }

        /// <summary>
        /// Compte le nombre de mail pour une nouvelle campagne
        /// </summary>
        private void CountNewCampaign()
        {

            try
            {
                Int32 nbRecipients = eListFactory.GetCountRecipientsCampaign(_pref, _context.Request.Form);
                RenderResult(RequestContentType.TEXT, delegate()
                {
                    return nbRecipients.ToString();
                });
            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Error("Erreur sur le comptage de destinataires de camapagne ", e);
            }
        }


        /// <summary>
        /// Compteur pour nombre de fiche en mode liste
        /// </summary>
        private void CountPaging()
        {

            String error = String.Empty;

            // Pas de requête de compteur
            if (String.IsNullOrEmpty(_pref.Context.Paging.SqlCount))
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6608))
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
                return;

            }
            else if (_pref.Context.Paging.HasCount)
            {
                RenderResult(RequestContentType.TEXT, delegate()
                {
                    return String.Concat(
                        eNumber.FormatNumber(_pref, _pref.Context.Paging.NbResult, 0, true), ";",
                        eNumber.FormatNumber(_pref, _pref.Context.Paging.NbPage, 0, false), ";",
                        eNumber.FormatNumber(_pref, _pref.Context.Paging.NbTotalResult, 0, true), ";");
                });
                return;
            }



            if (_pref.Context.Paging.Tab == TableType.PJ.GetHashCode())
            {
                RenderResult(RequestContentType.TEXT, delegate()
            {


                return String.Concat("0;0;0;");
            });
                return;
            }




            String sReturnValue = String.Empty;
            try
            {



                eDataTools.SetCountCurrentList(_pref);


                sReturnValue = eNumber.FormatNumber(_pref, _pref.Context.Paging.NbResult, 0, true);


                // Comptage total actif
                if (!_pref.Context.Paging.NbTotalResult.Equals(_pref.Context.Paging.NbResult))
                    sReturnValue = String.Concat(sReturnValue, " / ", eNumber.FormatNumber(_pref, _pref.Context.Paging.NbTotalResult, 0, true));




            }
            catch (Exception e)
            {
                _pref.Context.Paging.NbResult = 0;
                _pref.Context.Paging.NbTotalResult = 0;
                _pref.Context.Paging.NbPage = 0;



                Error("Erreur lors du comptage", e);


            }
            finally
            {
                // Marque le paging comme ayant un compteur
                _pref.Context.Paging.HasCount = true;
            }

            if (!String.IsNullOrEmpty(error))
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    error,
                    eResApp.GetRes(_pref, 72),
                    error
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            RenderResult(RequestContentType.TEXT, delegate()
            {
                return String.Concat(
                    eNumber.FormatNumber(_pref, _pref.Context.Paging.NbResult, 0, true), ";",
                    eNumber.FormatNumber(_pref, _pref.Context.Paging.NbPage, 0, false), ";",
                    eNumber.FormatNumber(_pref, _pref.Context.Paging.NbTotalResult, 0, true), ";");
            });
        }

        /// <summary>
        /// Génération d'un message d'erreur
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="ex"></param>
        private void Error(String sMsg, Exception ex)
        {


            StringBuilder sDevMsg = new StringBuilder();
            StringBuilder sUserMsg = new StringBuilder();


            //Erreur développeur
            sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append(sMsg).Append(" - ");


            // Detéil de l'exception
            if (ex != null)
                sDevMsg.AppendLine(ex.Message).AppendLine(ex.StackTrace);

            // Message simplifié pour l'utilisateur
            //Une erreur est survenu
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



}