using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack
{
    /// <summary>
    /// Classe statique pour le timewatch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public static class StaticTestMethods
    {

        /// <summary>
        /// Statique pour timewatch sur les fonctions.
        /// le type de retour est envoyé en paramètre.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="ret"></param>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public static void TimeWatcherFunction<U>(Func<U> func, U ret) where U : class
        {
            using (var timewatch = eTimeWatcher.InitTimeWatcher())
            {
                ret = func();
            }
        }

        /// <summary>
        /// Statique pour timewatch sur les fonctions.
        /// le type de retour est envoyé en paramètre.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="ret"></param>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public static void TimeWatcherFunction<T, U>(Func<T[], U> func, U ret, params T[] args) where T : class where U : class
        {
            using (var timewatch = eTimeWatcher.InitTimeWatcher())
            {
                ret = func(args);
            }
        }


        /// <summary>
        /// Statique pour timewatch sur les procédures.
        /// pas d'arguments.
        /// </summary>
        /// <param name="proc"></param>
        [Conditional("DEBUG")]
        public static void TimeWatcherAction(Action proc)
        {
            using (var timewatch = eTimeWatcher.InitTimeWatcher())
            {
                proc();
            }
        }

        /// <summary>
        /// Statique pour timewatch sur les procédures.
        /// le type de retour est envoyé en paramètre.
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public static void TimeWatcherAction<T>(Action<T[]> proc, params T[] args) where T : class
        {
            using (var timewatch = eTimeWatcher.InitTimeWatcher())
            {
                proc(args);
            }
        }

    }
}