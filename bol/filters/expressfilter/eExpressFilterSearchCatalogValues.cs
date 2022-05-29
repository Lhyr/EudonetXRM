using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Gestion de récupération des valeurs pour les filtres express sur les catalogues non utilisateur et non spéciaux
    /// </summary>
    public class eExpressFilterSearchCatalogValues : eExpressFilterSearchValues
    {

        /// <summary>
        /// Retourne le flux xml des valeurs de filtre express
        /// </summary>   
        protected override void generateValues()
        {

            string sFilterValue = _ExpressQuery.SearchValue;


            eCatalog catalog = new eCatalog(_dal, null, _ExpressQuery.FldSearch.Popup, _uInfos, _ExpressQuery.FldSearch.PopupDescId, false, -1, searchpattern: sFilterValue, isSnapshot: false);

            XmlAttribute xmlAttrib;
            XmlNode xmlExpressValue;

            //debute par
            xmlExpressValue = _xmlResult.CreateElement("element");
            xmlAttrib = _xmlResult.CreateAttribute("value");
            xmlAttrib.Value = string.Concat(EudoQuery.Operator.OP_START_WITH.GetHashCode().ToString(), ";|;", sFilterValue);
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlAttrib = _xmlResult.CreateAttribute("type");
            xmlAttrib.Value = "operator";
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlExpressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangServerId, 2006), " \"</span>", sFilterValue, "<span class=\"specialItem\">\"</span>");
            _detailsNode.AppendChild(xmlExpressValue);

            //Contient
            xmlExpressValue = _xmlResult.CreateElement("element");
            xmlAttrib = _xmlResult.CreateAttribute("value");
            xmlAttrib.Value = string.Concat(EudoQuery.Operator.OP_CONTAIN.GetHashCode().ToString(), ";|;", sFilterValue);
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlAttrib = _xmlResult.CreateAttribute("type");
            xmlAttrib.Value = "operator";
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlExpressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangServerId, 2009), " \"</span>", sFilterValue, "<span class=\"specialItem\">\"</span>");
            _detailsNode.AppendChild(xmlExpressValue);

            foreach (eCatalog.CatalogValue catalogValue in catalog.Values)
            {
                xmlExpressValue = _xmlResult.CreateElement("element");

                xmlAttrib = _xmlResult.CreateAttribute("value");
                xmlAttrib.Value = catalogValue.DbValue;

                xmlExpressValue.Attributes.Append(xmlAttrib);
                xmlExpressValue.InnerText = catalogValue.DisplayValue;

                _detailsNode.AppendChild(xmlExpressValue);
            }
        }
    }
}