using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Com.Eudonet.Xrm.IRISBlack.Tools
{
    /// <summary>
    /// Filtre qui va servir à timer les fonctions Post, Get...
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class eTimeWatcherAttribute : ActionFilterAttribute
    {
        eTimeWatcher eTime;
   
        /// <summary>
        /// Initialise le StopWatch et marque le début de l'execution.
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            eTime = eTimeWatcher.InitTimeWatcher();
        }

        /// <summary>
        /// Appele le dispose du timer, ce qui stop le stopwatch et 
        /// indique le temps entre le début et la fin de l'execution.
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        { 
            eTime.Dispose();
        }
    }
}