using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// récupération des valeurs de User pour les champs de type User
    /// </summary>
    public class UserValuesController : BaseController
    {

        /// <summary>
        /// renvoie les valeurs d'un catalogue
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Get([FromUri]UserValuesRequestModel r)
        {
            UserValuesModel userValues = new UserValuesModel();

            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref)) {

                eUser user = null;

                //gestion du mode admin, 
                SECURITY_GROUP groupMode = _pref.GroupMode;
                //if (_pref.AdminMode && _pref.User.UserLevel >= 99 && (groupMode == SECURITY_GROUP.GROUP_EXCLUDING || groupMode == SECURITY_GROUP.GROUP_EXCLUDING_READONLY))
                //{
                //    groupMode = SECURITY_GROUP.GROUP_DISPLAY;
                //}
                try
                {
                    dal.OpenDatabase();
                    user = new eUser(dal, r.DescId, _pref.User, eUser.ListMode.USERS_AND_GROUPS, groupMode, showGroupsOnly: r.ShowOnlyGroup);

                    if (r.DisplayProfile)
                    {
                        user.ShowOnlyProfil = r.ShowOnlyProfile;
                        r.TreeView = true;
                    }

                    userValues.UserListSortMode = user.SortMode;
                    userValues.Values = UserValuesFactory.GetValues(user, r);
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                } 
            }

            return Ok(JsonConvert.SerializeObject(userValues));
        }

        // DELETE api/<controller>/5
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}
