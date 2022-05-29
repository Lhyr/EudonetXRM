using Com.Eudonet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour alimenter le modele représentant un signet
    /// </summary>
    public class StructureModelForBookmarkFactory : StructureModelForListFactory
    {
        #region propriétés
        eBkmPref bkmPref;
        #endregion

        #region construteurs
        /// <summary>
        /// constructeur pour l'objet
        /// </summary>
        private StructureModelForBookmarkFactory(eList l, ePref p, eBkmPref bkmp) : base(l, p)
        {
            bkmPref = bkmp;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Retourne une instance de la classe.
        /// </summary>
        /// <param name="l">objet list</param>
        /// <param name="pref">objet des prefs</param>
        /// <param name="bkmPref"></param>
        /// <returns></returns>
        public static StructureModelForListFactory InitListDetailModelFactory(eList l, ePref pref, eBkmPref bkmPref)
        {
            return new StructureModelForBookmarkFactory(l, pref, bkmPref);
        }
        #endregion

        #region méthodes protected

        /// <summary>
        /// vérifie s'il existe des filtres express activés par le user sur la liste
        /// </summary>
        protected override void setExpressFilter()
        {
            ExpressFilterActivated = !string.IsNullOrEmpty(bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERCOL));
        }
        /// <summary>
        /// set le type du signet appelé
        /// </summary>
        protected override void setStructType()
        {
            eBookmark bkm = objList as eBookmark;
            StructType = bkm?.BkmEdnType ?? EudoQuery.EdnType.FILE_UNDEFINED;
        }


        #endregion

    }
}