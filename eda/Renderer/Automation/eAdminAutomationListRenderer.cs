using System;
using System.Linq;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Render pour la gestion des droits de traitements
    /// </summary>
    public class eAdminAutomationListRenderer : eActionListRenderer
    {

        AutomationType _autoType = AutomationType.ALL;
        int _actifRecordCount = 0;
        int _targetTab = 0;
        int _targetDescId = 0;
        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nDescId"></param>
        private eAdminAutomationListRenderer(ePref pref, Int32 nTab, Int32 nDescId, Int32 width, Int32 height, Int32 nPage, AutomationType automationType) : base(pref)
        {
            if (Pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            Pref = pref;
            _targetTab = nTab;
            _targetDescId = nDescId;
            _tab = (int)TableType.NOTIFICATION_TRIGGER;
            _width = width;
            _height = height;
            _rType = RENDERERTYPE.AutomationList;
            _autoType = automationType;
        }

        /// <summary>
        /// Initilisation du renderer : initialisation de la liste, bouton a afficher, nombre de ligne par page
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _list = eListFactory.CreateAutomationList(Pref, _targetTab, _targetDescId, _page, _autoType);
            _rows = _list.ListRecords.Count;
            _actifRecordCount = _list.ListRecords.Where((r) => {
                eFieldRecord field = r.GetFields.Find((f) => { return f.FldInfo.Descid == (int)NotificationTriggerField.STATUS; });
                return field != null && (field.Value == "1" || field.Value.ToLower() == "true");
            }).Count();

            //Pas de dublication, ni renommage, ni tooltip
            _drawBtnDuplicate = false;
            _drawBtnRename = false;
            _drawBtnTooltip = false;

            return true;
        }


        protected override bool Build()
        {
            _pgContainer.ID = "mainDiv";
            _pgContainer.Attributes.Add("ednType", "automation");
            _pgContainer.CssClass = "tabeul";
            _pgContainer.Attributes.Add("tab", _targetTab.ToString());
            _pgContainer.Attributes.Add("field", _targetDescId.ToString());
            _pgContainer.Attributes.Add("type", "1");
            _pgContainer.Attributes.Add("actif-record-count", _actifRecordCount.ToString());

            return base.Build();
        }


        #region Boutons actions : edit et delete sont activés
        protected override void BtnActionEdit(WebControl webCtrl, eRecord row)
        {
            eFieldRecord fld = row.GetFieldByAlias(TableType.NOTIFICATION_TRIGGER.GetHashCode() + "_" + NotificationTriggerField.NOTIFICATION_TYPE.GetHashCode());
            string type = AutomationType.ALL.GetHashCode().ToString();
            if (fld != null)
            {
                if (fld.Value == "0" || fld.Value == "1" || fld.Value == "2")
                    type = AutomationType.NOTIFICATION.GetHashCode().ToString();
            }

            fld = row.GetFieldByAlias(TableType.NOTIFICATION_TRIGGER.GetHashCode() + "_" + NotificationTriggerField.TRIGGER_TARGET_DESCID.GetHashCode());
            int tab = 0;
            int descid = 0;
            if (fld != null)
            {
                descid = eLibTools.GetNum(fld.Value);
                tab = eLibTools.GetTabFromDescId(descid);
            }
            webCtrl.Attributes.Add("onclick", String.Concat("nsAdminAutomation.edit(", row.MainFileid, ",", tab, ", ", descid, ", ", type, ")"));
        }

        protected override void BtnActionDuplicate(WebControl webCtrl, eRecord row)
        {

        }

        protected override void BtnActionRename(WebControl webCtrl, string sElementValueId, eRecord row)
        {

        }

        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {

        }

        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", String.Concat("nsAdminAutomation.delete(", row.MainFileid, ")"));
        }

        #endregion


        /// <summary>
        /// Creation d'une instance de ce renderer pour afficher la liste des automatismes notifications
        /// </summary>
        /// <param name="pref">pref utilisateur</param>
        /// <param name="nTab">table sur laquelle il y a des automatisme</param>
        /// <param name="fldDescId">descid de la rubrique qui declenche les notifications, 0 pour toutes les rubrique</param>
        /// <param name="width">largeur de la liste</param>
        /// <param name="height">hauteur de la liste</param>
        /// <param name="nPage">numéro de la page</param>
        /// <param name="type">type de l'automatisme</param>
        /// <returns></returns>
        public static eRenderer CreateAdminAutomationListRenderer(ePref pref, Int32 nTab, Int32 fldDescId, Int32 width, Int32 height, Int32 nPage, AutomationType type)
        {
            eRenderer ren = new eAdminAutomationListRenderer(pref, nTab, fldDescId, width, height, nPage, type);
            if (!ren.Generate())
                throw new Exception("Impossible de charger la liste : " + ren.ErrorMsg, ren.InnerException);

            return ren;
        }
    }
}