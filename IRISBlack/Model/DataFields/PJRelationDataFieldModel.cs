using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{

    /// <summary>
    /// Annexes.Fiche
    /// </summary>
    public class PJRelationDataFieldModel : RelationDataFieldModel
    {
        /// <summary>
        /// table à laquelle l'annexe est reliée
        /// </summary>
        ///<remarks>Contrairement aux autres champs de type relation il s'agit ici non pas d'une information structurelle mais d'une information qui varie d'un enregistrement à l'autre</remarks>
        public int PJTargetTab = 0;

        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="f"></param>
        /// <param name="iTargetTab"></param>
        public PJRelationDataFieldModel(eFieldRecord f, int iTargetTab) : base(f)
        {
            PJTargetTab = iTargetTab;
        }
    }
}