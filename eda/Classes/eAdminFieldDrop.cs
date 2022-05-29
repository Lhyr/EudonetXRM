//using Com.Eudonet.Internal.eda;
//using System;


//namespace Com.Eudonet.Xrm.eda
//{
//    /// <summary>
//    /// Classe de gestion des opérations de s
//    /// uppression de champs
//    /// </summary>
//    public class eAdminFieldDrop
//    {
   

//        /// <summary>
//        /// préférence user
//        /// </summary>
//        private ePref _ePref = null;


//        public eAdminFieldDrop(ePref pref, Int32 nDescid)
//        {

//            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
//                throw new EudoAdminInvalidRightException();

//            eAdminFieldInfos fld = null;
//            _ePref = pref;
//            try
//            {
//                fld = eAdminFieldInfos.GetAdminFieldInfos(_ePref, nDescid);
//                if (fld != null && fld.Error.Length > 0)
//                    throw new Exception(fld.Error);
//            }
//            catch (Exception e)
//            {

//                //exception non gérée
//                throw;
//            }
//        }


//        /// <summary>
//        /// Supprime le champ physiquement de la table
//        /// </summary>
//        /// <param name="fld"></param>
//        /// <returns></returns>
//        private bool DropColumn(eAdminFieldInfos fld)
//        {

//            return true;
//        }

//        /// <summary>
//        /// Nettoie les tables de règles
//        /// </summary>
//        /// <param name="fld"></param>
//        /// <returns></returns>
//        private bool CleanRules(eAdminField fld)
//        {
//            return true;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="fld"></param>
//        /// <returns></returns>
//        private bool CheckReport(eAdminField fld)
//        {

//           // eReport

//            return true;
//        }

//        /// <summary>
//        /// Nettoye Desc/Res
//        /// </summary>
//        /// <param name="fld"></param>
//        /// <returns></returns>
//        private bool CleanMetas(eAdminField fld)
//        {
//            return true;

//        }

//    }
//}