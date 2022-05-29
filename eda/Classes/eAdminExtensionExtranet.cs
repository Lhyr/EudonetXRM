using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.extranet;
using Com.Eudonet.Internal.eda;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionExtranet : eAdminExtension
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionExtranet(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Liste des extranet paramétré
        /// </summary>
        public List<eExtranetParam> lstExtranet = new List<eExtranetParam>();

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionExtranet(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }

        /// <summary>
        /// Indique si l'intégralité du mode Fiche de l'extension (notamment l'onglet Paramètres) doit être rafraîchi après activation/désactivation
        /// </summary>
        public override bool NeedsFullRefreshAfterEnable
        {
            get { return true; }
        }

        /// <summary>
        /// Indique si l'extension comporte des paramètres à afficher dans l'onglet Paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get {
             
                return true; 
            }
        }

        /// <summary>
        /// Processus exécuté après (dés)activation
        /// </summary>
        /// <param name="bEnable">Doit-on activer ou désactiver l'extension ?</param>
        /// <param name="sError">Erreur éventuellement survenue</param>
        /// <returns></returns>
        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            sError = "";
            return true;
        }


        /// <summary>
        /// Mis à jour des param de l'extension en fonction d'un changement de status
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="oldStatus"></param>
        protected override void UpdateParamChangeStatus(eExtension ext, EXTENSION_STATUS oldStatus)
        {
            if (ext.Status == oldStatus || lstExtranet.Count == 0)
                return;

            if (ext.Status == EXTENSION_STATUS.STATUS_DISABLED || ext.Status == EXTENSION_STATUS.STATUS_DISABLED)
            {
                foreach (var extranet in lstExtranet)
                {
                    extranet.DateDisabled = DateTime.Now;
                    extranet.DateModify =   DateTime.Now;
                    extranet.UpdatededBy = _pref.UserId;
                    extranet.IsActive = false;
                }

                ext.Param = JsonConvert.SerializeObject(new { ext = lstExtranet });
            }

            //Pour l'instant, l'activation de l'extension active les extranet
            if(ext.Status == EXTENSION_STATUS.STATUS_READY)
            {
                foreach (var extranet in lstExtranet)
                {
                    extranet.DateActivation = DateTime.Now;
                    extranet.DateModify = DateTime.Now;
                    extranet.UpdatededBy = _pref.UserId;
                    extranet.IsActive = true;
                  //  extranet.Token =   eExtranetToken.GetExtranetToken(_pref, extranet.Id);
                }
                ext.Param = JsonConvert.SerializeObject(new { ext = lstExtranet });
            }
        }

        /// <summary>
        /// Indique s'il faut sauvegarder l'extension sous jacente (cf SetDefaultParam)
        /// </summary>
        public bool NeedSaveRegistredExt = false;

        /// <summary>
        /// Désérialisation du parm
        /// </summary>
        /// <param name="newExt"></param>
        public override void SetDefaultParam(eExtension newExt)
        {
            if (newExt.Param != "{}") //extension paramétrée
            {
                try
                {
                    var lst = new { ext = new List<eExtranetParam>() };
                    lstExtranet = JsonConvert.DeserializeAnonymousType(newExt.Param, lst).ext;


                    if (lstExtranet.Count == 0)
                    {
                        lstExtranet = new List<eExtranetParam>()
                            {
                                    eExtranetParam.GetNewExtranetParam(_pref)
                            };

                        NeedSaveRegistredExt = true;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Paramétrage des extranets corrompu");
                }
            }
            else
            {
                //extension non paramétrée, on génère une entrée extranet par défaut 
                lstExtranet = new List<eExtranetParam>()
                    {
                        eExtranetParam.GetNewExtranetParam(_pref)
                    };

                NeedSaveRegistredExt = true;
            }




            newExt.Param = JsonConvert.SerializeObject(new { ext = lstExtranet });
        }



        /// <summary>
        /// Activation/Désactivation de l'extension - Action sur tables autre que "Extension"
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string sError)
        {
            sError = "";
            return true;
        }

        /// <summary>
        /// Vérification de l'activation de l'extension
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            sError = "";
            if (this.Infos == null)
                return false;

            return this.Infos.Status == EXTENSION_STATUS.STATUS_READY;
        }




        /// <summary>
        /// Renvoie les informations à afficher dans l'encart supérieur de la fiche Extension
        /// </summary>
        /// <returns></returns>
        public override List<HtmlGenericControl> GetModuleInfo()
        {
            return new List<HtmlGenericControl>();
        }

    }
}