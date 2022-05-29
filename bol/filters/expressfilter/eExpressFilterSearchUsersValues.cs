using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Gestion de récupération des valeurs pour les filtres express sur les catalogues utilisateurs
    /// </summary>
    public class eExpressFilterSearchUsersValues : eExpressFilterSearchValues
    {

        /// <summary>
        /// Constructeur vide
        /// </summary>
        public eExpressFilterSearchUsersValues() { }




        /// <summary>
        /// Retourne le flux xml des valeurs de filtre express
        /// </summary>        
        /// <returns></returns>
        protected override void generateValues()
        {
            string sSearchValue = _ExpressQuery.SearchValue;


            // on obtient la liste des users à partir du même objet qui est utilisé pour ouvrir les catalogues utilisateurs

            List<eUser.UserListItem> uli;
            eUser objUser = new eUser(_dal,
                    _ExpressQuery.SearchDescId,
                    _uInfos,
                    eUser.ListMode.USERS_ONLY,
                    _uInfos.GroupMode,
                    new List<string>());


            StringBuilder sbError = new StringBuilder();
            uli = objUser.GetUserList(false, true, sSearchValue, sbError);
            if (sbError.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("ExpressFilter (", _ExpressQuery.SearchFieldTab, " - ", _ExpressQuery.FldSearch, ") : Erreur lors de la récupération des utilisateurs : ", sbError));
            }

            XmlAttribute xmlAttrib;
            XmlNode xmlExpressValue;

            // Pour les utilisateurs seul le débute par est géré par eudoquery.
            //debute par
            xmlExpressValue = _xmlResult.CreateElement("element");
            xmlAttrib = _xmlResult.CreateAttribute("value");
            xmlAttrib.Value = string.Concat(EudoQuery.Operator.OP_START_WITH.GetHashCode().ToString(), ";|;", sSearchValue);
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlAttrib = _xmlResult.CreateAttribute("type");
            xmlAttrib.Value = "operator";
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlExpressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangId, 2006), " \"</span>", sSearchValue, "<span class=\"specialItem\">\"</span>");
            _detailsNode.AppendChild(xmlExpressValue);


            foreach (eUser.UserListItem usr in uli)
            {
                if (usr.Hidden)
                    continue;

                xmlExpressValue = _xmlResult.CreateElement("element");

                xmlAttrib = _xmlResult.CreateAttribute("value");

                xmlAttrib.Value = usr.ItemCode;

                xmlExpressValue.Attributes.Append(xmlAttrib);

                xmlExpressValue.InnerText = usr.Libelle;
                _detailsNode.AppendChild(xmlExpressValue);

            }
        }




    }
}