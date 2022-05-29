using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    public class UserValuesFactory
    {
        /// <summary>
        /// construit la liste des valeurs
        /// </summary>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static List<IUserValue> GetValues(eUser user, UserValuesRequestModel request)
        {
            List<IUserValue> liReturn;
            List<eUser.UserListItem> uli;
            StringBuilder sbError = new StringBuilder();
            if (request.TreeView)
            {
                uli = user.GetUserArbo(request.FullUserList, request.ShowUserOnly, request.SearchPattern, sbError);
            }
            else
            {
                uli = user.GetUserList(request.FullUserList, request.ShowUserOnly, request.SearchPattern, sbError);
            }

            liReturn = uli.Select(uitem => GetValue(uitem)).ToList();

            return liReturn;
        }

        /// <summary>
        /// convertit un useritem en uservaluemodel pour la transmission au front
        /// </summary>
        /// <param name="userItem"></param>
        /// <returns></returns>
        public static IUserValue GetValue(eUser.UserListItem userItem)
        {
            return new UserValuesModel.Value()
            {
                Label = userItem.Libelle,
                Type = (int)userItem.Type,
                Disabled = userItem.Disabled,
                Hidden = userItem.Hidden,
                ItemCode = userItem.ItemCode,
                GroupId = userItem.GroupId,
                GroupLevel = userItem.GroupLevel,
                Selected = userItem.Selected,
                IsChild = userItem.IsChild,
                ChildrensUserListItem = userItem.ChildrensUserListItem.Select(ui => GetValue(ui)).ToList()
            };


        }
        /// <summary>
        /// construit la liste des valeurs
        /// </summary>
        /// <param name="user"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static List<IUserValue> GetMRUValues(eUser user, string search)
        {
            List<IUserValue> liReturn;
            List<eUser.UserListItem> uli;
            StringBuilder sbError = new StringBuilder();
            UserValuesRequestModel request = new UserValuesRequestModel()
            {
                FullUserList = true,
                ShowUserOnly = true,
                SearchPattern = search
            };

            uli = user.GetUserList(request.FullUserList, request.ShowUserOnly, request.SearchPattern, sbError);

            liReturn = uli.Select(uitem => GetValue(uitem)).ToList();

            return liReturn;
        }

    }
}