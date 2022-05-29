using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Permet de charger le paramétrage de Saml2
    /// </summary>
    public interface ISaml2Settings
    {
        /// <summary>
        /// Initialisation des paramétres
        /// </summary>
        void Init();

        /// <summary>
        /// Conversion des attributs forunis par l'IDP en champ mapppé dans l'application
        /// </summary>
        /// <remarks>
        /// Si certains attributs ont le même nom, le premier element de la même famille sera pris en compte et les autres sont ignorés
        /// </remarks>
        /// <param name="saml2Attributes">attributes de l'adp pour identifier l'utilisateur</param>
        /// <returns>Champs coresspondant aux attribut</returns>
        IEnumerable<eSaml2Field> Convert(IEnumerable<eSaml2Attribute> saml2Attributes);
    }
}