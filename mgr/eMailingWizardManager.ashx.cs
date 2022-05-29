using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Merge.Eudonet;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eMailingWizardManager
    /// </summary>
    public class eMailingWizardManager : eEudoManager
    {
        XmlNode _baseResultNode;

        Dictionary<string, int> _dic = new Dictionary<string, int>();

        eCampaignAdditionalFilters _additionalFilters = new eCampaignAdditionalFilters();

        protected override void ProcessManager()
        {
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            _baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(_baseResultNode);
            String sError = "";
            Dictionary<String, String> dicParam = new Dictionary<String, String>();



            var def = new {
                optin = 1,
                optout = 0,
                noconsent = 0,
                valid = 1,
                notverified = 0,
                invalid = 0,
                removeDoubles = 0
            };


            var jsonExtranetParam = def;

            if (_context.Request.Form.AllKeys.Contains("mailfilters"))
                jsonExtranetParam = JsonConvert.DeserializeAnonymousType(_context.Request.Form["mailfilters"], def); ;

            //Ajout le filtre de optin au dictionnaire
            LoadConsentFilterParam();

            int nOptin = 0, nOptout = 0, nNoconsent = 0;



            #region filtre sans consentement

            int nFilterCampaignType = 0;
            if ((Int32.TryParse(_context.Request.Form["fltcampaigntype"], out nFilterCampaignType)) && nFilterCampaignType > 0)
            {
                if ((!Int32.TryParse(_context.Request.Form["fltcampaigntype"], out nFilterCampaignType)) || nFilterCampaignType == 0)
                {

                }
            }


            if (nFilterCampaignType > 0)
            {
                nOptin = countPPList(new eCampaignAdditionalFilters()
                {
                    AddOptin = true,
                    AddNoRegisteredConsent = false,
                    AddOptOut = false,

                    AdressStatus = new AdressStatusParam()
                    {
                        InvalidAdress = jsonExtranetParam.invalid == 1,
                        ValidAdress = jsonExtranetParam.valid == 1,
                        NotVerifiedAdress = jsonExtranetParam.notverified == 1
                    }
                });

                //Ajout le filtre de optout au dictionnaire    LoadConsentFilterParam(dicParam, optoutType);
                nOptout = countPPList(new eCampaignAdditionalFilters()
                {
                    AddOptin = false,
                    AddNoRegisteredConsent = false,
                    AddOptOut = true,

                    AdressStatus = new AdressStatusParam()
                    {
                        InvalidAdress = jsonExtranetParam.invalid == 1,
                        ValidAdress = jsonExtranetParam.valid == 1,
                        NotVerifiedAdress = jsonExtranetParam.notverified == 1
                    }
                });


                //Ajout le filtre de sans consentement au dictionnaire
                nNoconsent = countPPList(new eCampaignAdditionalFilters()
                {
                    AddOptin = false,
                    AddNoRegisteredConsent = true,
                    AddOptOut = false,


                    AdressStatus = new AdressStatusParam()
                    {
                        InvalidAdress = jsonExtranetParam.invalid == 1,
                        ValidAdress = jsonExtranetParam.valid == 1,
                        NotVerifiedAdress = jsonExtranetParam.notverified == 1
                    }
                });
            }
            #endregion


            #region Qualité
            /*
            int nInvalid = countPPList(new eCampaignAdditionalFilters()
            {
  
              AdressStatus = new AdressStatusParam()
                {
                    InvalidAdress =  true,
                    ValidAdress =false,
                    NotVerifiedAdress = false,
                },

                AddOptin = jsonExtranetParam.optin == 1,
                AddNoRegisteredConsent = jsonExtranetParam.noconsent == 1,
                AddOptOut = jsonExtranetParam.optout == 1,
            });


            int nValid = countPPList(new eCampaignAdditionalFilters()
            {
                AdressStatus = new AdressStatusParam()
                {
                    InvalidAdress = false,
                    ValidAdress = true,
                    NotVerifiedAdress = false,
                },

                AddOptin = jsonExtranetParam.optin == 1,
                AddNoRegisteredConsent = jsonExtranetParam.noconsent == 1,
                AddOptOut = jsonExtranetParam.optout == 1,
            });

                        
            int nUnChecked = countPPList(new eCampaignAdditionalFilters()
            {
                AdressStatus = new AdressStatusParam()
                {
                    InvalidAdress = false,
                    ValidAdress = false,
                    NotVerifiedAdress = true,
                },

                AddOptin = jsonExtranetParam.optin == 1,
                AddNoRegisteredConsent = jsonExtranetParam.noconsent == 1,
                AddOptOut = jsonExtranetParam.optout == 1,
            });
            */




            eCampaignXRM ec = eListFactory.GetFilteredCampaign(_pref,
                _context.Request.Form,
                new eCampaignAdditionalFilters()
                {
                    AdressStatus = new AdressStatusParam()
                    {
                        InvalidAdress = true,
                        ValidAdress = true,
                        NotVerifiedAdress = true,
                    },

                    AddOptin = jsonExtranetParam.optin == 1,
                    AddNoRegisteredConsent = jsonExtranetParam.noconsent == 1,
                    AddOptOut = jsonExtranetParam.optout == 1,
                }, _dic);


            int nUnChecked = ec.AdrUncheked;
            int nInvalid = ec.AdrInvalid;
            int nValid = ec.AdrValid;



            #endregion

            #region Total


            eCampaignXRM ecDbl = eListFactory.GetFilteredCampaign(_pref,
               _context.Request.Form,
               new eCampaignAdditionalFilters()
               {

                   AdressStatus = new AdressStatusParam()
                   {
                       InvalidAdress = jsonExtranetParam.invalid == 1,
                       ValidAdress = jsonExtranetParam.valid == 1,
                       NotVerifiedAdress = jsonExtranetParam.notverified == 1
                   },

                   AddOptin = jsonExtranetParam.optin == 1,
                   AddNoRegisteredConsent = jsonExtranetParam.noconsent == 1,
                   AddOptOut = jsonExtranetParam.optout == 1

               }, _dic);


            int nTotalRecepientWithoutDoubles = ecDbl.CountRecipient() - (jsonExtranetParam.removeDoubles == 1 ? ecDbl.AdrDbl : 0);
            int nTotalRecepient = ecDbl.CountRecipient();

            /*

            int nTotalRecepient = countPPList(new eCampaignAdditionalFilters()
            {

                AdressStatus = new AdressStatusParam()
                {
                    InvalidAdress = jsonExtranetParam.invalid == 1,
                    ValidAdress = jsonExtranetParam.valid == 1,
                    NotVerifiedAdress = jsonExtranetParam.notverified == 1
                },

                AddOptin = jsonExtranetParam.optin == 1,
                AddNoRegisteredConsent = jsonExtranetParam.noconsent == 1,
                AddOptOut = jsonExtranetParam.optout == 1,
            });

            int nTotalRecepientWithoutDoubles = countPPList(new eCampaignAdditionalFilters()
            {

                AdressStatus = new AdressStatusParam()
                {
                    InvalidAdress = jsonExtranetParam.invalid == 1,
                    ValidAdress = jsonExtranetParam.valid == 1,
                    NotVerifiedAdress = jsonExtranetParam.notverified == 1
                },

                AddOptin = jsonExtranetParam.optin == 1,
                AddNoRegisteredConsent = jsonExtranetParam.noconsent == 1,
                AddOptOut = jsonExtranetParam.optout == 1,
                RemoveDoubles = jsonExtranetParam.removeDoubles == 1
            });
            */
            #endregion



            #region result
            XmlNode _successNode = xmlResult.CreateElement("success");
            _baseResultNode.AppendChild(_successNode);
            _successNode.InnerText = "1";

            XmlNode _nbElementNode = xmlResult.CreateElement("nOptin");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nOptin.ToString();

            _nbElementNode = xmlResult.CreateElement("nOptout");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nOptout.ToString();

            _nbElementNode = xmlResult.CreateElement("nNoconsent");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nNoconsent.ToString();


            _nbElementNode = xmlResult.CreateElement("invalid");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nInvalid.ToString();


            _nbElementNode = xmlResult.CreateElement("valid");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nValid.ToString();


            _nbElementNode = xmlResult.CreateElement("notchecked");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nUnChecked.ToString();

            _nbElementNode = xmlResult.CreateElement("totalRecepient");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nTotalRecepient.ToString();

            _nbElementNode = xmlResult.CreateElement("totalRecepientWithoutDoubles");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = nTotalRecepientWithoutDoubles.ToString();


            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
            #endregion

        }

        private void LoadConsentFilterParam()
        {

            List<eCatalog.CatalogValue> catalogInteractionStatusConsent = new List<eCatalog.CatalogValue>();
            List<eCatalog.CatalogValue> catalogInteractionTypes = new List<eCatalog.CatalogValue>();
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                catalogInteractionStatusConsent = new eCatalog(dal, _pref, PopupType.DATA, _pref.User, (int)InteractionField.StatusConsent).Values;
                catalogInteractionTypes = new eCatalog(dal, _pref, PopupType.DATA, _pref.User, (int)InteractionField.Type).Values;


                //récupération des différent catalogue et des id nécessaire aux filtres
                _dic["optin"] = eLibTools.GetCatValueIdByDataString("optin", catalogInteractionStatusConsent);
                _dic["optout"] = eLibTools.GetCatValueIdByDataString("optout", catalogInteractionStatusConsent);
                _dic["consent"] = eLibTools.GetCatValueIdByDataString("consent", catalogInteractionTypes);
            }
            finally
            {
                dal.CloseDatabase();
            }


        }

        /// <summary>
        /// pour calculer le nbre de destinataire à partir d'un filtre
        /// </summary>
        /// <param name="filter"> filtre</param>
        /// <returns></returns>
        private int countPPList(eCampaignAdditionalFilters filter)
        {


            int nbRecipients = eListFactory.GetCountRecipientsCampaign(_pref,
                _context.Request.Form,
                filter, _dic);

            return nbRecipients;
        }

    }
}