using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour construire l'objet record mail Model qui hérite de record model.
    /// </summary>
    public class RecordMailFactory
        : RecordFactory
    {
        eRecordMail recordMail { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rec"></param>
    /// <param name="dtf"></param>
    /// <param name="pref"></param>
    private RecordMailFactory(eRecordMail rec, eDataFiller dtf, ePref pref)
            : base(rec, dtf, pref) {
            recordMail = rec;
        }


        #region Initialisation statique de l'objet
        /// <summary>
        /// Initialisation statique de la classe RecordMailfactory.
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="dtf"></param>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static RecordMailFactory InitRecordMailFactory(eRecordMail rec, eDataFiller dtf, ePref pref)
        {
            return new RecordMailFactory(rec, dtf, pref);
        }
        #endregion

        #region public

        /// <summary>
        /// fonction permettant la construction du Model et son retour.
        /// </summary>
        public override IRecordModel ConstructRecordModel()
        {
            if (recordMail == null)
                throw new EudoException("L'enregistrement est introuvable");

            IRecordModel recMo = base.ConstructRecordModel();
            RecordMailModel recMlMo = GlobalMethodsFactory.ToDerived<IRecordModel, RecordMailModel>(recMo);

            if (recMlMo != null)
                recMlMo.MailStatus = recordMail.MailStatus;

            return recMlMo;


        }
        #endregion

    }
}