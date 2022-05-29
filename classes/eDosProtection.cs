using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eDosProtection</className>
    /// <summary>Classe de gestion de sécurité deny of service</summary>
    /// <purpose>Il peut y avoir qu'une seule instance de cette classe</purpose>
    /// <authors>HLA</authors>
    /// <date>2014-02-11</date>
    public class eDosProtection
    {
        private const int MAX_BY_30S = 15;
        private const int MAX_BY_30M = 600;

        /// <summary>
        /// Pour eviter de charger le serveur, on active les trace seulement pour 10 minutes si la demande à été faite
        /// </summary>
        private const int TRACE_TIMEOUT = 600;

        private static eDosProtection _instance = null;
        private static object _syncLock = new object();

        private Dictionary<eDosProtectionKey, eDosProtectionData> _dicValues;
        private static object _syncLockDico = new object();

        private static object _syncLockTrace = new object();
        private bool _dosTrace = false;
        private long _dosTraceDateTicksActiv = DateTime.MinValue.Ticks;

        private eDosProtection()
        {
            this._dicValues = new Dictionary<eDosProtectionKey, eDosProtectionData>();

#if DEBUG
            ActiveTraceMode(true);
#endif
        }

        /// <summary>
        /// Retourne l'instance unique de eDosProtection
        /// </summary>
        /// <returns></returns>
        public static eDosProtection GetInstance()
        {
            if (null == _instance)
            {
                lock (_syncLock)
                {
                    if (null == _instance)
                        _instance = new eDosProtection();
                }
            }

            return _instance;
        }

        /// <summary>Active ou désactive le mode de tace</summary>
        public void ActiveTraceMode(bool active)
        {
            // Force le log
            LocalTrace("DOS {trace_mode} >> change to : " + active, true);
            LocalTrace(string.Concat("DOS {limits} >> ban si plus de ", MAX_BY_30S, " cliques en 30 secondes ",
                 "ou si plus de ", MAX_BY_30M, " cliques en 30 minutes"), true);

            // Active ou pas le log
            lock (_syncLockTrace)
            {
                this._dosTrace = active;
                this._dosTraceDateTicksActiv = DateTime.Now.Ticks;
            }
        }


        /// <summary>
        /// Retourne les Infos de DoS
        /// </summary>
        public IEnumerable<KeyValuePair<eDosProtectionKey, eDosProtectionData>> GetInfo
        {
            get
            {
                return _dicValues;
            }

        }


        /// <summary>
        /// Vide le dictionnaire de suivi du DoS
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            try
            {
                _dicValues.Clear();
                return true;
            }
            catch
            {

                return false;
            }
        }


        /// <summary>
        /// Stop les demandes multiples
        /// </summary>
        /// <param name="request">context de la page</param>
        /// <returns>indique si la personne a atteint une des limites</returns>
        public bool DemandConnect(HttpRequest request)
        {
            DateTime dt = DateTime.Now;
            eDosProtectionData data;
            eDosProtectionKey key = eDosProtectionKey.GetKey(request);

            LocalTrace("DOS {keygen} >> UserId : " + key.UserIP + " / QueryString : " + key.QueryString + " / HashCode : " + key.GetHashCode());

            if (!_dicValues.TryGetValue(key, out data))
            {
                lock (_syncLockDico)
                {
                    if (!_dicValues.TryGetValue(key, out data))
                    {
                        data = new eDosProtectionData(dt);
                        _dicValues.Add(key, data);

                        LocalTrace("DOS {add} >> " + data.ToString());

                        return true;
                    }
                }
            }

            TimeSpan dtDiff = dt - data.Date;
            data.Date = dt;

            if (dtDiff.TotalSeconds < 30)
                data.Cnt_1++;
            else
                data.Cnt_1 = 0;

            if (dtDiff.TotalMinutes < 30)
                data.Cnt_2++;
            else
                data.Cnt_2 = 0;

            LocalTrace("DOS {val} >> " + data.ToString());

            // Les limites sont MAX_BY_30S cliques de moins de 30 secondes et MAX_BY_30M cliques de moins de 30 minutes
            if (data.Cnt_1 >= MAX_BY_30S || data.Cnt_2 >= MAX_BY_30M)
            {
                LocalTrace("DOS {limit} >> reached");

                return false;
            }

            return true;
        }

        private void LocalTrace(string msg, bool forceTrace = false)
        {
            if (forceTrace || IsTraceActive())
                eModelTools.EudoTraceLog(msg, prefix: "dos");
        }

        private bool IsTraceActive()
        {
            bool traceActive = false;
            bool expireTrace = false;

            lock (_syncLockTrace)
            {
                if (_dosTrace && _dosTraceDateTicksActiv > 0)
                {
                    TimeSpan ts = DateTime.Now - new DateTime(_dosTraceDateTicksActiv);
                    if (ts.TotalSeconds > TRACE_TIMEOUT)
                        expireTrace = true;
                    else
                        traceActive = true;
                }
            }

            // On désactive la trace si le timeout est atteint
            if (expireTrace)
                ActiveTraceMode(false);

            return traceActive;
        }
    }

    /// <className>eDosProtectionKey</className>
    /// <summary>Classe de gestion de clé de sécurité</summary>
    /// <purpose>Permet de génerer une clé en fonction du HttpRequest et de gerer la comparaison d'objet</purpose>
    /// <authors>HLA</authors>
    /// <date>2014-02-11</date>
    public class eDosProtectionKey
    {
        /// <summary>IP de l'utilisateur</summary>
        public string UserIP { get; set; }
        /// <summary>Paramètres de la page</summary>
        public string QueryString { get; set; }

        private eDosProtectionKey()
        {

        }

        /// <summary>
        /// Clé de comparaison
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCodeGenerator.GenericHashCode(UserIP, QueryString);
        }

        /// <summary>
        /// Test d'égalité
        /// </summary>
        /// <param name="obj">object à comparer</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            eDosProtectionKey pKey = obj as eDosProtectionKey;
            if (pKey == null)
                return false;

            return UserIP == pKey.UserIP && QueryString == pKey.QueryString;
        }

        /// <summary>
        /// Réécriture
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Retourne une nouvelle clé en fonction du context
        /// </summary>
        /// <param name="request">context de la page</param>
        /// <returns></returns>
        public static eDosProtectionKey GetKey(HttpRequest request)
        {
            return new eDosProtectionKey() { QueryString = request.QueryString.ToString(), UserIP = eLibTools.GetUserIPV4(request) };
        }
    }

    /// <className>eDosProtectionData</className>
    /// <summary>Classe de gestion de données de sécurité par clé</summary>
    /// <purpose>On retrouve les données necessaire à la gestion de la sécurité</purpose>
    /// <authors>HLA</authors>
    /// <date>2014-02-11</date>
    public class eDosProtectionData
    {
        /// <summary>Date de dernier clique</summary>
        public DateTime Date { get; set; }
        /// <summary>Compteur de clique 1</summary>
        public int Cnt_1 { get; set; }
        /// <summary>Compteur de clique 2</summary>
        public int Cnt_2 { get; set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="date">date de départ</param>
        public eDosProtectionData(DateTime date)
        {
            this.Date = date;
            this.Cnt_1 = 0;
            this.Cnt_2 = 0;
        }

        /// <summary>
        /// Réécriture
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder()
                .Append("Date : ").Append(this.Date).Append(" // ")
                .Append("Cnt_1 : ").Append(this.Cnt_1).Append(" // ")
                .Append("Cnt_2 : ").Append(this.Cnt_2)
                .ToString();
        }
    }
}