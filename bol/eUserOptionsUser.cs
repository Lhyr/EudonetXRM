using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de gestion des options utilisateurs de type [USER]
    /// </summary>
    public class eUserOptionsUser : eUserOptions
    {

        /// <summary>
        /// Constructeur standard
        /// </summary>
        /// <param name="pref"></param>
        public eUserOptionsUser(ePref pref)
            : base(pref)
        {

        }

        /// <summary>
        /// Mise à jour de l'option utilisateur
        /// </summary>
        /// <param name="option"></param>
        /// <param name="sOptionValue"></param>
        /// <returns></returns>
        public bool Update(eConst.OPTIONS_USER option, String sOptionValue)
        {
            switch (option)
            {
                case eConst.OPTIONS_USER.LANG:

                    //pour un changement de langue, il faut :
                    // -> récupérer la langue serveur
                    // -> mettre à jour le cookie avec cette langue
                    // -> mettre à jour les pref

                    //Test de validité de la valeur de l'option
                    Int32 nLngUser;
                    if (!Int32.TryParse(sOptionValue, out nLngUser) || nLngUser < 0 || nLngUser > 10)
                        throw new Exception("La langue choisie (" + sOptionValue + ") a une valeur invalide.");

                    Int32 nServerLang;
                    eudoDAL dalDb = eLibTools.GetEudoDAL(Pref);
                    dalDb.OpenDatabase();

                    try
                    {

                        //Maj du cookie
                        if (HttpContext.Current != null)
                            EudoCommonHelper.EudoHelpers.SaveCookie("langue", "LANG_" + nLngUser.ToString().PadLeft(2, '0'), DateTime.MaxValue, HttpContext.Current.Response);


                        if (!eLibTools.GetServerLang(dalDb, nLngUser, out nServerLang))
                            throw new Exception("Problème de récupération de langue serveur pour la langue utilisateur :>" + nLngUser + "<.");
 
                        Pref.LangServId = nServerLang;
                        Pref.Lang = "LANG_" + nLngUser.ToString().PadLeft(2, '0');

                        Pref.User.UserLang = Pref.Lang;
                        Pref.User.UserLangId = Pref.LangId;

                        // maj les ressource vide local avec langue serveur
                        eRes.UpdateEmptyRes(Pref, Pref.Lang, Pref.LangServ);
       



                        //Maj en base
                        String sSQL = "UPDATE [USER] SET LANG=@LANG WHERE USERID=@USERID";


                        RqParam rqUpdt = new RqParam(sSQL);

                        rqUpdt.AddInputParameter("@LANG", System.Data.SqlDbType.Char, "LANG_" + nLngUser.ToString().PadLeft(2, '0'));
                        rqUpdt.AddInputParameter("@USERID", System.Data.SqlDbType.Int, Pref.UserId);


                        String sError = String.Empty;
                        Int32 nNb = dalDb.ExecuteNonQuery(rqUpdt, out sError);

                        if (sError.Length > 0)
                        {
                            if (dalDb.InnerException != null)
                                throw dalDb.InnerException;
                            else
                                throw new Exception("Impossible de mettre à jour la langue utilisateur : " + sError);
                        }



                        return true;

                    }
                    catch (Exception)
                    {
                        
                    }
                    finally
                    {

                        dalDb.CloseDatabase();
                    }



                    break;
                default:
                    throw new NotImplementedException("type d'option utilisateur non géré : " + option.GetHashCode().ToString());

            }


            return false;
        }
    }
}