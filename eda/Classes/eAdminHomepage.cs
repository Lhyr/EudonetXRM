using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminHomepage
    {
        public int Id { get; set; }
        public eConst.HOMEPAGE_TYPE Type { get; private set; }
        public String Label { get; private set; }
        public String Tooltip { get; private set; }
        public String IDUsers { get; private set; }
        public String DisplayUsers { get; private set; }
        public String Content { get; private set; }
        public eAdminHomepage(ePref pref, int id, String label, String userid, String displayUsers, String tooltip = "", String content = "")
        {
            this.Id = id;
            this.Label = label;
            this.IDUsers = userid;
            if (this.IDUsers == "0")
                this.DisplayUsers = eResApp.GetRes(pref, 347);
            else if (String.IsNullOrEmpty(this.IDUsers))
                this.DisplayUsers = eResApp.GetRes(pref, 513);
            else
                this.DisplayUsers = displayUsers;
            this.Tooltip = tooltip;
            this.Content = content;
        }


        /// <summary>
        /// Retourne la liste des objets eAdminHomepage
        /// </summary>
        /// <param name="eDal">eudoDAL</param>
        /// <param name="types">Types de homepages recherchés</param>
        /// <returns></returns>
        public static List<eAdminHomepage> GetHomepagesList(ePref pref, eudoDAL eDal)
        {
            String sError = String.Empty;

            List<eAdminHomepage> list = eSqlHomepage.GetHomepages(pref, eDal, out sError);

            if (!String.IsNullOrEmpty(sError))
                throw new Exception(sError);

            return list;
        }

        /// <summary>
        /// Mise à jour 
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Boolean Save(ePref pref, int id, String prop, String value, out String sError)
        {
            sError = String.Empty;
            Boolean bOk = false;

            eudoDAL eDal = eLibTools.GetEudoDAL(pref);

            try
            {
                eDal.OpenDatabase();

                bOk = eSqlHomepage.UpdateHomepage(eDal, id, prop, value, out sError);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                eDal.CloseDatabase();
            }

            return bOk;
        }



        /// <summary>
        /// Mise à jour 
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static eAdminHomepage GetExpressMesageById(ePref pref, int id, out String sError)
        {
            sError = String.Empty;
            eudoDAL eDal = eLibTools.GetEudoDAL(pref);
            eAdminHomepage hmp = null;
            //hmp = null;
            try
            {
                eDal.OpenDatabase();
                hmp = eSqlHomeExpressMessage.GetExpressMessageByID(pref, id, eDal, out sError);

            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                eDal.CloseDatabase();
            }

            return hmp;
        }

        /// <summary>
        /// Mise à jour des utilisateurs pour une homepage
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="sError"></param>
        public static void SaveUsers(ePref pref, int id, String value, out String sError, eUserOptionsModules.USROPT_MODULE module = eUserOptionsModules.USROPT_MODULE.UNDEFINED)
        {
            sError = String.Empty;

            eudoDAL eDal = eLibTools.GetEudoDAL(pref);

            try
            {
                eDal.OpenDatabase();

                switch (module)
                {

                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:

                        eSqlHomeExpressMessage.UpdateHomepageExpressMessageUsers(eDal, id, value, out sError);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME:
                        //eSqlHomepage.UpdateHomepageUsers(eDal, id, value, out sError);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                        eSqlHomepage.UpdateHomepageUsers(eDal, id, value, out sError);
                        break;
                    default:
                        break;
                }


            }
            catch (Exception e)
            {
                sError = "eAdminHomepage.SaveUsers => " + e.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="sError"></param>
        public void SaveUsersExpresssMessage(ePref pref, out String sError, eUserOptionsModules.USROPT_MODULE module = eUserOptionsModules.USROPT_MODULE.UNDEFINED)
        {


            SaveUsers(pref, this.Id, this.IDUsers, out sError, module);
        }

        /// <summary>
        /// Dupliquer une page d'accueil
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="id"></param>
        /// <param name="sError"></param>
        public static void Clone(ePref pref, int id, out String sError)
        {
            sError = String.Empty;

            eudoDAL eDal = eLibTools.GetEudoDAL(pref);

            try
            {
                eDal.OpenDatabase();

                eSqlHomepage.CloneHomepage(eDal, id, String.Concat(eResApp.GetRes(pref, 7593), " "), out sError);
            }
            catch (Exception e)
            {
                sError = "eAdminHomepage.Clone => " + e.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }

        public static void Delete(ePref pref, int id, eUserOptionsModules.USROPT_MODULE module, out String sError)
        {
            sError = String.Empty;

            eudoDAL eDal = eLibTools.GetEudoDAL(pref);

            try
            {
                eDal.OpenDatabase();
                switch (module)
                {
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME:
                        //eSqlHomepage.DeleteHomepage(eDal, id, out sError);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                        eSqlHomepage.DeleteHomepage(eDal, id, out sError);
                        break;
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                        eSqlHomeExpressMessage.DeleteExpressMessage(eDal, id, out sError);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                sError = "eAdminHomepage.Delete => " + e.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="module"></param>
        /// <param name="sError"></param>
        public void Add(ePref pref, eUserOptionsModules.USROPT_MODULE module, out String sError)
        {
            sError = String.Empty;

            eudoDAL eDal = eLibTools.GetEudoDAL(pref);

            try
            {
                eDal.OpenDatabase();
                switch (module)
                {
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                        eSqlHomeExpressMessage.AddExpressMessage(eDal, this, out sError);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                sError = "eAdminHomepage.ADD => " + e.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="module"></param>
        /// <param name="sError"></param>
        public void Update(ePref pref, eUserOptionsModules.USROPT_MODULE module, out String sError)
        {
            sError = String.Empty;
            eudoDAL eDal = eLibTools.GetEudoDAL(pref);

            try
            {
                eDal.OpenDatabase();
                switch (module)
                {
                    case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                        eSqlHomeExpressMessage.UpdateHomepageExpressMessage(eDal, this, out sError);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                sError = "eAdminHomepage.Update => " + e.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }
    }
}