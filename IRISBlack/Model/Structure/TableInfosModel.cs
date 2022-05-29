using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// Model de données pour les tables.
    /// </summary>
    public class TableInfosModel
    {
        /// <summary>
        /// indique si la liaison interevt doit être mise à blanc par défaut lors de la création 
        /// </summary>
        public bool NoDefaultLink100 { get; set; }
        /// <summary>
        /// indique si la liaison PP doit être mise à blanc par défaut lors de la création 
        /// </summary>
        public bool NoDefaultLink200 { get; set; }
        /// <summary>
        /// indique si la liaison PM doit être mise à blanc par défaut lors de la création 
        /// </summary>
        public bool NoDefaultLink300 { get; set; }
        /// <summary>
        /// Liaison Event activée
        /// </summary>
        public bool InterEvent { get; set; }
        /// <summary>
        /// Liaison Event parente obligatoire 
        /// </summary>
        public bool InterEventNeeded { get; set; }
        /// <summary>
        /// Liaison PP activée
        /// </summary>
        public bool InterPP { get; set; }
        /// <summary>
        /// Liaison PP obligatoire
        /// </summary>
        public bool InterPPNeeded { get; set; }
        /// <summary>
        /// Liaison PM obligatoire
        /// </summary>
        public bool InterPM { get; set; }
        /// <summary>
        /// Liaison PM obligatoire
        /// </summary>
        public bool InterPMNeeded { get; set; }
        /// <summary>
        /// Descid de l'event lié
        /// </summary>
        public int InterEVTDescid { get; set; }
        /// <summary>
        /// La limite de caractère que l'on va mettre pour avant recherche.
        /// </summary>
        public int SearchLimit { get; set; }
        /// <summary>
        /// Doit-on reprendre en cascade PMPP en cas de liaison
        /// </summary>
        public bool NoCascadePMPP  { get; set; }
        /// <summary>
        /// Doit-on reprendre en cascade PPPM en cas de liaison
        /// </summary>
        public bool NoCascadePPPM { get; set; }
    }
}