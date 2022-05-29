using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{

    public class eVCardManager : eEudoManager
    {


        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                String id = "";
                Int32 ppid;
                if (_requestTools.AllKeysQS.Contains("vc"))
                {
                    id = _context.Request.QueryString["vc"].ToString();
                    id = CryptoEudonet.Decrypt(id, CryptographyConst.KEY_CRYPT_LINK2);

                    if (Int32.TryParse(id, out ppid) && ppid > 0)
                    {
                        // nPage = 1 correspond à l'adresse principale adr12 = 1 car
                        // EudoQuery fait un tri sur Adresse principale puis adresse active
                        // L'export se fait sur l'adresse principale c'est isoV7. 
                        eVCardFile vcard = eVCardFile.CreateVCardFiles(_pref, ppid, nPage: 1);

                        String filename;
                        String vcf = BuildVCF(vcard, out filename);

                        Render(vcf, filename);
                    }
                }
            }
            catch (eEndResponseException) { }
            catch (Exception exp)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", exp.Message, Environment.NewLine, "Exception StackTrace :", exp.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)), // Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));
            }
        }

        /// <summary>
        /// Construit le corps de la carte de visite
        /// </summary>
        /// <param name="vcard">objet card de visite</param>
        /// <param name="filename">nome de fichier vcf</param>
        /// <returns></returns>
        private string BuildVCF(eVCardFile vcard, out String filename)
        {
            // Retour a la ligne pour les vCard
            String CRLF = "\\n";
            String sMapping = vcard.GetParam<String>("vCardMapping");
            XmlDocument xmlVCard = new XmlDocument();

            if (string.IsNullOrEmpty(sMapping))
                throw (new Exception("Mapping VCARD manquant."));

            xmlVCard.LoadXml(sMapping);

            eRecord eRecord = null;
            if (vcard.ListRecords.Count > 0)
            {
                eRecord = vcard.ListRecords[0];
            }
            else
            {
                filename = "empty";
                return String.Empty;
            }

            StringBuilder vcf = new StringBuilder();

            //Nom de fichier .vcf
            String lastname = GetDisplayName(eRecord, xmlVCard, "//lastname");
            String firstname = GetDisplayName(eRecord, xmlVCard, "//firstname");
            filename = firstname + " " + lastname;

            // le contenu du vcf
            vcf.Append("BEGIN:VCARD").AppendLine();
            vcf.Append("VERSION:3.0").AppendLine();

            vcf.Append("N:").Append(lastname).Append(";").Append(firstname).AppendLine();
            vcf.Append("FN:").Append(firstname).Append(" ").Append(lastname).AppendLine();
            vcf.Append("TITLE:").Append(GetDisplayName(eRecord, xmlVCard, "//title")).AppendLine();
            vcf.Append("ORG:").Append(GetDisplayName(eRecord, xmlVCard, "//company")).AppendLine();
            //SHA : demande #46 350 : Mettre le téléphone fixe dans Tel. Bureau à la place de Tel. Domicile.
            //vcf.Append("TEL;HOME;VOICE:").Append(GetDisplayName(eRecord, xmlVCard, "//phone")).AppendLine();
            vcf.Append("TEL;WORK;VOICE:").Append(GetDisplayName(eRecord, xmlVCard, "//phone")).AppendLine();
            vcf.Append("TEL;CELL:").Append(GetDisplayName(eRecord, xmlVCard, "//mobile")).AppendLine();
            vcf.Append("ADR;WORK;PREF;QUOTED-PRINTABLE:;;")
                .Append(GetDisplayName(eRecord, xmlVCard, "//address1")).Append(CRLF)
                .Append(GetDisplayName(eRecord, xmlVCard, "//address2")).Append(CRLF)
                .Append(GetDisplayName(eRecord, xmlVCard, "//address3")).Append(";")
                .Append(GetDisplayName(eRecord, xmlVCard, "//city")).Append(";")
                .Append(GetDisplayName(eRecord, xmlVCard, "//country")).Append(";")
                .Append(GetDisplayName(eRecord, xmlVCard, "//postalcode")).AppendLine();
            vcf.Append("EMAIL;PREF;INTERNET:").Append(GetDisplayName(eRecord, xmlVCard, "//email")).AppendLine();
            vcf.Append("NOTE:")
                .Append(eLibTools.RemoveHTML(GetDisplayName(eRecord, xmlVCard, "//body1")).Replace("\n", CRLF)).Append(CRLF)
                .Append(eLibTools.RemoveHTML(GetDisplayName(eRecord, xmlVCard, "//body2")).Replace("\n", CRLF)).Append(CRLF)
                .Append(eLibTools.RemoveHTML(GetDisplayName(eRecord, xmlVCard, "//body3")).Replace("\n", CRLF)).Append(CRLF)
                .Append(eResApp.GetRes(_pref.LangId, 6806)).AppendLine();
            vcf.Append("END:VCARD").AppendLine();

            return vcf.ToString();
        }

        /// <summary>
        /// Récupère la valeur a afficher depuis eRecord en tenant compte du mapping dans le xml
        /// </summary>
        /// <param name="eRecord">eRecord représente la liste des champs de la carte de visite</param>
        /// <param name="xmlVCard">mapping entre le champs vCard et les champs eudonet</param>
        /// <param name="nodeName">noeud xml de mapping</param>
        /// <returns></returns>
        private String GetDisplayName(eRecord eRecord, XmlDocument xmlVCard, string nodeName)
        {
            List<eFieldRecord> lstPpName = new List<eFieldRecord>();
            String vCardFldAlias = eVCardFileRenderer.GetAliasFromDescId(xmlVCard.SelectSingleNode(nodeName));
            eFieldRecord ef = eRecord.GetFieldByAlias(vCardFldAlias);

            //Si pas de mapping définit ou pas de droit de visu, on retourne vide
            if (ef == null || !ef.RightIsVisible)
                return String.Empty;

            return ef.DisplayValue.Replace(";", "\\;"); // en cas de catalogue multiple            
        }

        /// <summary>
        /// Construit une réponse html sous forme d'un fichier vcf
        /// </summary>
        /// <param name="vcf">vCard</param>
        /// <param name="filename">Nom fichier vCard</param>
        private void Render(String vcf, String filename)
        {
            _context.Response.Clear();
            _context.Response.ClearContent();
            _context.Response.ClearHeaders();
            _context.Response.Buffer = true;
            _context.Response.AddHeader("Content-Length", vcf.Length.ToString());
            _context.Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + "_business_card.vcf");
            _context.Response.ContentType = "text/x-vcard";

            //Vu avec RMA : on privilégie outlook avec l'encodage iso-8859-1. (sur android les accents ne sont pas intérpretés correctement)
            _context.Response.ContentEncoding = Encoding.GetEncoding("iso-8859-1");
            
            _context.Response.Write(vcf);
            throw new eEndResponseException();
            //_context.Response.End();
        }
    }
}