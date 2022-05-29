using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{



    /// <summary>
    /// Pour les listes users admin.
    /// </summary>
    public class eListMainUser : eListMain
    {
        bool _bEudoSyncEnabled;

        /// <summary>
        /// Constructeur protected avec vérif niveau admin
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nRows"></param>
        /// <param name="nPage"></param>
        protected eListMainUser(ePref ePref, Int32 nTab, Int32 nRows, Int32 nPage) : base(ePref, nTab, nRows, nPage)
        {
            if (ePref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            var zz = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO, Pref);
            _bEudoSyncEnabled = zz.Infos.IsEnabled;
        }


        /// <summary>
        /// Getter pour le constructeur
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nRows"></param>
        /// <param name="nPage"></param>
        /// <returns></returns>
        public static eListMainUser GetListMainUser(ePref ePref, Int32 nTab, Int32 nRows, Int32 nPage)
        {
            return new eListMainUser(ePref, nTab, nRows, nPage);
        }


        protected override bool Build()
        {


            if (base.Build())
            {
                if (!_bEudoSyncEnabled)
                {
                    var f = _fldFieldsInfos.Find(d => d.Descid == 101020);
                    if (f != null)
                        _fldFieldsInfos.Remove(f);
                }

                return true;

            }

            return false;
        }


        /// <summary>
        /// Traitement de fin de ligne : gestion des droits spécifiques sur certains champ
        /// en fonction du niveau de l'utilisateur
        /// </summary>
        /// <param name="row"></param>
        protected override void PostRowTreatment(eRecord row)
        {
            base.PostRowTreatment(row);


            //interdiction de modif un user d'un level > au sien
            String sUlevel = row.GetFieldByAlias(string.Concat((int)TableType.USER, "_", (int)UserField.LEVEL))?.Value ?? "";
            int nLevel = 0;
            Int32.TryParse(sUlevel, out nLevel);
            if (nLevel > Pref.User.UserLevel)
            {
                row.RightIsUpdatable = false;
                row.RightIsDeletable = false;

                foreach (var f in row.GetFields)
                {
                    f.RightIsUpdatable = false;
                }

                return;
            }

            //Sinon acces ok
            row.RightIsUpdatable = true;

            //Champ par champ
            foreach (var f in row.GetFields)
            {
                //Tous les champs bit sauf disable si user = user connecté
                if (f.FldInfo.Format == FieldFormat.TYP_BIT
                    && (f.FldInfo.Descid != (int)UserField.UserDisabled || f.FileId != Pref.User.UserId)
                    && (f.FldInfo.Descid != (int)UserField.IsProfile || f.FileId != Pref.User.UserId)
                    )
                {
                    if ((f.FldInfo.Descid == (int)UserField.UserDisabled || f.FldInfo.Descid == (int)UserField.UserHidden) && 
                        
                        ( row is eRecordUser && ((eRecordUser)row).IsProfil))
                    {
                        f.RightIsUpdatable = false;
                    }
                    else
                        f.RightIsUpdatable = true;

                }
                else
                    f.RightIsUpdatable = false;
            }

            // interdit de se supprimer
            if (row.MainFileid != Pref.UserId)
                row.RightIsDeletable = true;
        }
    }
}