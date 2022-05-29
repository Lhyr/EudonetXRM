using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eFilemapPartnerManager
    /// </summary>
    public class eFilemapPartnerManager : eAdminManager
    {
        /// <summary>
        /// Actions possibles du manager
        /// </summary>
        public enum FilemapPartnerAction
        {
            /// <summary>
            /// Indéfini
            /// </summary>
            Undefined = -1,
            /// <summary>
            /// Mise à jour
            /// </summary>
            Update = 0
        }

        /// <summary>
        /// Objet contenant les infos pour la mise à jour
        /// </summary>
        [DataContract]
        public struct UpdateObject
        {
            /// <summary>
            /// The identifier of FilemapPartner
            /// </summary>
            [DataMember]
            public int Id;
            /// <summary>
            /// Type du FilemapPartner
            /// </summary>
            [DataMember]
            public int Type;
            /// <summary>
            /// The source
            /// </summary>
            [DataMember]
            public string Source;
            /// <summary>
            /// Descid source : généralement le descid de la table
            /// </summary>
            [DataMember]
            public int SourceDid;
            /// <summary>
            /// Descid du champ mappé
            /// </summary>
            [DataMember]
            public int Descid;
        }

        public struct ResultObject
        {
            public bool Success;
            public int IdFm;
            public string Error;
        }


        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            int nAction = _requestTools.GetRequestFormKeyI("a") ?? -1;
            FilemapPartnerAction action = (FilemapPartnerAction)nAction;

            int idFm = 0;
            bool success = false;
            string error = string.Empty;

            switch (action)
            {
                case FilemapPartnerAction.Update:

                    string json = _requestTools.GetRequestFormKeyS("o");
                    eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

                    try
                    {
                        UpdateObject upd = JsonConvert.DeserializeObject<UpdateObject>(json);

                        eDal.OpenDatabase();
                        eFilemapPartner obj = eFilemapPartner.CreateFileMapPartner(upd.Type, source: upd.Source, sourceDescid: upd.SourceDid, descid: upd.Descid);
                        idFm = obj.SaveFileMapPartner(eDal, upd.Id);

                        success = true;
                    }
                    catch (Exception exc)
                    {
                        error = exc.Message;
                    }
                    finally
                    {
                        eDal.CloseDatabase();
                    }

                    ResultObject res = new ResultObject
                    {
                        Success = success,
                        IdFm = idFm,
                        Error = error
                    };
                    RenderResult(RequestContentType.SCRIPT, delegate ()
                    {
                        return SerializerTools.JsonSerialize(res);
                    });

                    break;
                default: break;
            }
        }
    }
}