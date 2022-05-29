using System;
using Com.Eudonet.Internal;
using System.Text;
using EudoQuery;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eMailSuggestMgr
    /// </summary>
    public class eMailSuggestMgr : eEudoManager
    {
        protected override void ProcessManager()
        {
            Int32 _pmid = 0, _ppid = 0, _adrid = 0;
            String _nomEmail = "", _prenomEmail = "";


            //Recuperation param
            if (_requestTools.AllKeys.Contains("pmid") && !String.IsNullOrEmpty(_context.Request.Form["pmid"]))
                Int32.TryParse(_context.Request.Form["pmid"].ToString(), out _pmid);
            if (_requestTools.AllKeys.Contains("ppid") && !String.IsNullOrEmpty(_context.Request.Form["ppid"]))
                Int32.TryParse(_context.Request.Form["ppid"].ToString(), out _ppid);
            if (_requestTools.AllKeys.Contains("adrid") && !String.IsNullOrEmpty(_context.Request.Form["adrid"]))
                Int32.TryParse(_context.Request.Form["adrid"].ToString(), out _adrid);
            if (_requestTools.AllKeys.Contains("nomEmail") && !String.IsNullOrEmpty(_context.Request.Form["nomEmail"]))
                _nomEmail = _context.Request.Form["nomEmail"].ToString();
            if (_requestTools.AllKeys.Contains("prenomEmail") && !String.IsNullOrEmpty(_context.Request.Form["prenomEmail"]))
                _prenomEmail = _context.Request.Form["prenomEmail"].ToString();

            //Chargement de la liste des emails depuis la base en fonction des params
            List<eFieldRecord> lstAdrEmail = new List<eFieldRecord>();
            if (_pmid > 0) //Creation PP
            {
                lstAdrEmail = eFileTools.LoadAdrFromPmId(_pref, _pmid);
            }
            else if (_ppid > 0) // Modification PP
            {
                List<string> lstPmId = eFileTools.LoadPmIdFromPpId(_pref, _ppid);
                if(lstPmId.Count > 0)
                    lstAdrEmail = eFileTools.LoadAdrFromListPmId(_pref, lstPmId);
            }
            else if (_adrid > 0) //Modification adress en modal
            {
                List<string> lstPpId = eFileTools.LoadPpIdFromAdrId(_pref, _adrid);
                if(lstPpId.Count > 0 && Int32.TryParse(lstPpId.FirstOrDefault(), out _ppid))
                {
                    List<string> lstPmId = eFileTools.LoadPmIdFromPpId(_pref, _ppid);
                    if (lstPmId.Count > 0)
                        lstAdrEmail = eFileTools.LoadAdrFromListPmId(_pref, lstPmId);
                }
            }

            //Tranformation des emails de type alias, suppression des emails invalides et recuperation de la liste des noms de domaine
            List<String> lstDomainName = new List<String>();
            eMailAddressConteneur emailConteneur = null;
            foreach (eFieldRecord fldMail in lstAdrEmail)
            {
                emailConteneur = new eMailAddressConteneur(fldMail.DisplayValue);
                lstDomainName.AddRange(emailConteneur.LstAddress.Select(x => x.MailDomain));
            }
                
            //Construction squelette XML
            _xmlResult = new XmlDocument();
            XmlNode maintNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(maintNode);
            XmlNode rootNode = _xmlResult.CreateElement("mails");
            _xmlResult.AppendChild(rootNode);

            //Si il existe des noms de domaine on peut effectuer la suggestion d email
            if(lstDomainName.Count > 0)
            {
                //Recupere du nom de domaine le plus utilisé
                string sDomainName = lstDomainName.GroupBy(s => s)
                            .OrderByDescending(s => s.Count())
                            .First().Key;
                //On créer les noeuds HTML avec les suggestions
                foreach (String s in getSuggestedEmail(sDomainName.ToLower(), _nomEmail.ToLower(), _prenomEmail.ToLower()))
                {
                    XmlNode xmlMail = _xmlResult.CreateElement("mail");
                    xmlMail.InnerText = s;
                    rootNode.AppendChild(xmlMail);
                }
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });

        }

        /// <summary>
        /// Retourne la liste des suggestions d'adresses mailsl
        /// </summary>
        /// <param name="domainName">Le nom de domaine a utiliser</param>
        /// <param name="nom">Le nom du contact</param>
        /// <param name="prenom">Le prenom du contact</param>
        /// <returns>Une liste de string contenant les adresses mails a suggerer</returns>
        private List<String> getSuggestedEmail(string domainName, string nom, string prenom)
        {
            List<String> listSuggestedEmail = new List<String>();
            string sFirstLetter = string.Empty;
            domainName = eLibTools.RemoveDiacritics(domainName).Replace(" ", String.Empty);
            //Construction de la liste des combinaisons possibles
            if (!String.IsNullOrEmpty(nom))
            {
                nom = eLibTools.RemoveDiacritics(nom).Replace(" ", String.Empty);
                listSuggestedEmail.Add(nom + domainName);
            }
            if (!String.IsNullOrEmpty(prenom))
            {
                prenom = eLibTools.RemoveDiacritics(prenom).Replace(" ", String.Empty);
                //Gestion des prenoms composés
                sFirstLetter = prenom.First().ToString();
                if (prenom.Contains("-") && prenom.IndexOf("-") < prenom.Length)
                    sFirstLetter = prenom.First().ToString() +  (prenom.ElementAt(prenom.IndexOf("-") + 1)).ToString();
                listSuggestedEmail.Add(prenom + domainName);
            }
            if (!String.IsNullOrEmpty(prenom) && !String.IsNullOrEmpty(nom))
            {
                listSuggestedEmail.Add(prenom + "." + nom + domainName);
                listSuggestedEmail.Add(prenom + nom + domainName);
                listSuggestedEmail.Add(nom + "." + prenom + domainName);
                listSuggestedEmail.Add(sFirstLetter + "." + nom + domainName);
                listSuggestedEmail.Add(sFirstLetter + nom + domainName);
            }

            return listSuggestedEmail;
        }

    }
}