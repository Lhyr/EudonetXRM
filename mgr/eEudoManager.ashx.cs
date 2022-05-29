using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{

    /// <className>eEudoManager</className>
    /// <summary>Classe parente des manager, se référer à la classe fille
    /// Le mode de session est en read/write : cela verouille les appels concurent
    /// </summary>
    /// <purpose></purpose>
    /// <authors>SPH</authors>
    /// <date>2011-06-18</date>
    public abstract class eEudoManager : eBaseEudoManager, System.Web.SessionState.IRequiresSessionState
    {
    }



    /// <className>eEudoManager avec session ReadOnly</className>
    /// <summary>Classe parente des manager, se référer à la classe fille</summary>
    /// <purpose>Permet de faire un appel sans vérouiller les appel concurent de la même session</purpose>
    /// <authors>SPH</authors>
    /// <date>2018-01-15</date>
    public abstract class eEudoManagerReadOnly : eBaseEudoManager, System.Web.SessionState.IReadOnlySessionState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        protected virtual void ProcessManager(HttpContext context) { }
    }


    /// <className>eRequestReportManager</className>
    /// <summary>Singleton qui enregistre dans une liste triée le temps de traitement d'un appel client</summary>
    /// <purpose>Permet de garder les <see cref="eRequestReportManager._maxItems"/> appels les plus coûteux en temps de traitement</purpose>
    /// <authors>MOU</authors>
    /// <date>2018-01-15</date>
    public sealed class eRequestReportManager
    {
        /// <summary>
        /// Nombre d'appels a garder dans la liste par défaut
        /// </summary>
        private static int MAX_ITEMS = 15;

        /// <summary>
        /// Nombre d'appels a garder dans la liste
        /// </summary>
        private int _maxItems = MAX_ITEMS;

        /// <summary>
        /// Nombre d'appels a garder dans la liste
        /// </summary>
        private bool isActive = false;

        /// <summary>
        /// Objet de synchronisation
        /// </summary>
        private object sync_active = new object();

        /// <summary>
        /// Objet de synchronisation
        /// </summary>
        private object sync_max_items = new object();

        /// <summary>
        /// Objet de synchronisation
        /// </summary>
        private object sync_sorted_set = new object();

        /// <summary>
        /// On garde <see cref="_maxItems"/> appels dans la static avec les infos de la base
        /// </summary>
        private SortedSet<CallInfo> calls;


        /// <summary>
        /// Nombre d'appels a garder dans la liste
        /// </summary>
        public int MaxItems
        {
            get
            {
                int maxItem = MAX_ITEMS;
                lockAndExecOnMaxItems(current => {
                    maxItem = current;
                });

                return maxItem;
            }
            set
            {
                lockAndExecOnMaxItems(current =>
                {
                    _maxItems = value;
                    lockAndExecOnSortedSet(sortedSet => RemoveExtraItems(sortedSet));
                });
            }
        }

        /// <summary>
        /// On synchronise la liste
        /// </summary>
        private void lockAndExecOnMaxItems(Action<int> action)
        {
            lock (sync_max_items)
            {
                action(_maxItems);
            }
        }

        /// <summary>
        /// Le constructeur n'est pas accessible depuis l'exterieur
        /// </summary>
        private eRequestReportManager()
        {
            calls = new SortedSet<CallInfo>(new CallInfoSort());
        }

        /// <summary>
        /// Ajoute le temps au dico
        /// </summary>
        /// <param name="info"></param>
        public void Add(CallInfo info)
        {
            lockAndExecOnSortedSet(sortedSet =>
            {
                if (!isActive)
                    return;

                sortedSet.Add(info);
                RemoveExtraItems(sortedSet);
            });
        }

        /// <summary>
        /// Garde que le nombre d'element défini
        /// </summary>
        private void RemoveExtraItems(SortedSet<CallInfo> sortedSet)
        {
            // On enlève le element au dela de la limite  
            var max = MaxItems;
            while (calls.Count > max)
                sortedSet.Remove(calls.ElementAt(calls.Count - 1));
        }

        /// <summary>
        /// Toute opération sur la liste est synchronisées
        /// </summary>
        /// <param name="action"></param>
        private void lockAndExecOnSortedSet(Action<SortedSet<CallInfo>> action)
        {
            lock (sync_sorted_set)
            {
                action(calls);
            }
        }

        /// <summary>
        /// Activer ou désactiver la trace
        /// </summary>
        /// <returns></returns>
        public void SwitchState(bool isActive)
        {
            // On change d'etat
            lockAndExecOnState(current =>
            {
                this.isActive = isActive;
                if (!this.isActive)
                    lockAndExecOnSortedSet(sortedSet => sortedSet.Clear());
            });
        }

        /// <summary>
        /// Connaitre l'état
        /// </summary>
        /// <returns></returns>
        public bool IsStateActive()
        {
            bool state = false;
            lockAndExecOnState(currentState => state = currentState);
            return state;
        }

        /// <summary>
        /// Toute les opérationq sur l'etat est synchronisées
        /// </summary>
        /// <param name="action"></param>
        private void lockAndExecOnState(Action<bool> action)
        {
            lock (sync_active)
            {
                action(isActive);
            }
        }

        /// <summary>
        /// On retourne la liste clonée
        /// </summary>
        /// <returns></returns>
        public IList<CallInfo> GetCalls()
        {
            IList<CallInfo> finalSortedSet = new List<CallInfo>();
            lockAndExecOnSortedSet(sortedSet => finalSortedSet = new List<CallInfo>(sortedSet));
            return finalSortedSet;
        }

        /// <summary>
        /// L'instance est accessible pour différent thread
        /// </summary>
        public static eRequestReportManager Instance { get { return InternalClass.Instance; } }
              

        /// <summary>
        /// Class interne permet de créer une seule instance eEudoElapsedTime 
        /// en utilisant le mecanisme d'instanciation static qui est ThreadSafe
        /// </summary>
        private class InternalClass
        {
            /// <summary>
            /// Constructeur static appelé une seule fois même dans un context multi-thread
            /// http://csharpindepth.com/Articles/General/Singleton.aspx
            /// Le compilateur ne va pas flaguer le filed instance avec "beforefieldinit"
            /// l'event beforefieldinit on <see cref="Instance"/> est executé après que l'instance de InternalClass est crée et pas l'inverse
            /// http://csharpindepth.com/Articles/General/Beforefieldinit.aspx
            /// </summary>
            static InternalClass() { }

            /// <summary>
            /// Une seule instance eEudoElapsedTime est créée
            /// </summary>
            public static readonly eRequestReportManager Instance = new eRequestReportManager();
        }

        /// <summary>
        /// Comparaison descendante selon la clé
        /// </summary>
        internal class CallInfoSort : IComparer<CallInfo>
        {
            /// <summary>
            /// Comparaisoon entre deux callInfo ^pour garder celui qu'a un temps de réponse max en dédoublonnant
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public int Compare(CallInfo a, CallInfo b)
            {
                // Pour la durée on inverse la comparaison pour afficher en premier les plus couteux
                int v = b.ElapsedTime.CompareTo(a.ElapsedTime);
                if (v > 0 || v < 0)
                    return v;

                v = a.BaseName.CompareTo(b.BaseName);
                if (v > 0 || v < 0)
                    return v;

                v = a.User.CompareTo(b.User);
                if (v > 0 || v < 0)
                    return v;

                v = a.Action.CompareTo(b.Action);
                if (v > 0 || v < 0)
                    return v;

                v = a.Grid.CompareTo(b.Grid);
                if (v > 0 || v < 0)
                    return v;

                v = a.Widget.CompareTo(b.Widget);
                if (v > 0 || v < 0)
                    return v;

                v = a.Report.CompareTo(b.Report);
                if (v > 0 || v < 0)
                    return v;

                return v;
            }
        }

        /// <summary>
        /// Infos sur l'appel
        /// </summary>
        public struct CallInfo
        {
            /// <summary>
            /// Temps ecoulé pour répondre à la requetes
            /// </summary>
            public int ElapsedTime;

            /// <summary>
            /// Nom de la base de données
            /// </summary>
            public string BaseName;

            /// <summary>
            /// Utilisateur connectée
            /// </summary>
            public string User;

            /// <summary>
            /// Action demandé
            /// </summary>
            public string Action;

            /// <summary>
            /// Id de la grille
            /// </summary>
            public int Grid;

            /// <summary>
            /// Id du widget
            /// </summary>
            public int Widget;

            /// <summary>
            /// Id du rapport
            /// </summary>
            public int Report;

            /// <summary>
            /// Url de la demandes
            /// </summary>
            public string Url;
        }
    }

    /// <summary>
    /// Exception d'interuption des manager pour remplacer les responses.end 
    /// Workaround pour le problème sur le serveur ww2.eudonet.com : les response.end crashaient le pool iis (arrêt du pool avec erreur critique non catchable)
    /// </summary>
    public class eEndResponseException : Exception
    {


    }



}