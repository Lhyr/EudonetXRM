using Com.Eudonet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EudoQuery;

namespace Com.Eudonet.Xrm.IRISBlack.Attributs
{
    [AttributeUsage(AttributeTargets.All)]
    sealed class eAuthorizationAttribute : ActionFilterAttribute
    {
        public ePref Pref { get; set; }
        public UserLevel MinUserLevel { get; set; }

        /// <summary>
        /// Instancie un attribut avec le level minimum requis
        /// </summary>
        /// <param name="minLevel"></param>
        public eAuthorizationAttribute(UserLevel minLevel)
        {
            Pref = HttpContext.Current.Session["Pref"] as ePref;
            MinUserLevel = minLevel;
        }

        /// <summary>
        /// Si le level minimum requis n'est pas viable, on génère une erreur.
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            int nUserLevel = Pref?.User?.UserLevel ?? 0;

            if (nUserLevel < (int)MinUserLevel)
                throw new EudoException("Vous n'êtes pas autorisé à effectuer cette opération.");
        }
    }
}