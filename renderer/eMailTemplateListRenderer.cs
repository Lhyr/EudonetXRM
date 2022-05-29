using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu pour la page de liste des template
    /// </summary>
    public class eMailTemplateListRendrer : eActionListRenderer
    {
        private Int32 _nTabTpl = 0;
        private Int32 _nPage = 1;
        eMailingTemplate template;
        private TypeMailTemplate _mtType = TypeMailTemplate.MAILTEMPLATE_UNDEFINED;


 


        /// <summary>
        /// TODO : déplace dans epref les param suplémentaires
        /// </summary>
        IDictionary<eLibConst.CONFIGADV, string> _dicEmailAdvConfig;


        /// <summary>
        /// Retourne une instance de eMailTemplateListRendrer
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="height">Hauteur du renderer</param>
        /// <param name="width">Largeur du renderer</param>
        /// <param name="nTabTpl">DescId du template mail</param>
        /// <param name="nType">Type de modèle de mails à afficher (e-mailing, mail unitaire...)</param>
        /// <returns></returns>
        public static eMailTemplateListRendrer GetMailTemplateListRenderer(ePref pref, Int32 height, Int32 width, Int32 nTabTpl, TypeMailTemplate mtType, IRightTreatment oRightManager, Int32 nPage = 1)
        {
            eMailTemplateListRendrer myRenderer = new eMailTemplateListRendrer(pref, height, width, nTabTpl, mtType, nPage);
            myRenderer._tab = nTabTpl;
            myRenderer._page = nPage;
            myRenderer._rType = RENDERERTYPE.UserMailTemplateList;
            myRenderer.RightManager = oRightManager;

            return myRenderer;
        }

        /// <summary>
        /// Constructeur privé 
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="height">Hauteur du renderer</param>
        /// <param name="width">Largeur du renderer</param>
        /// <param name="nTabTpl">DescId du template mail</param>
        private eMailTemplateListRendrer(ePref pref, Int32 height, Int32 width, Int32 nTabTpl, TypeMailTemplate mtType, Int32 nPage = 1)
            : base(pref)
        {
            _height = height;
            _width = width;
            _nTabTpl = nTabTpl;
            _rType = RENDERERTYPE.UserMailTemplateList;
            _mtType = mtType;
            _nPage = nPage;
            template = new eMailingTemplate(Pref);

            _dicEmailAdvConfig = eLibTools.GetConfigAdvValues(this.Pref,
                new HashSet<eLibConst.CONFIGADV> {
                    eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED,
                    eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD
                });
        }



        /// <summary>
        /// Vérifie les droits de traitement
        /// </summary>
        protected override void InitDrawButtonsAction()
        {
            // BSE 03/10/2016 #49369  Affichage des modèles de mails dans le cas d'un email unitaire
            if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAIL)
            {
                // En fonction des droits, on affiche ou pas le bouton correspondant
                this._drawBtnEdit = RightManager.CanEditItem();
                this._drawBtnRename = RightManager.CanRenameItem();
                this._drawBtnDelete = RightManager.CanDeleteItem();
                this._drawBtnTooltip = false;
                //On n'a pas le bouton dupliquer
                this._drawBtnDuplicate = RightManager.CanDuplicateItem();
            }
            else
            {
                base.InitDrawButtonsAction();
            }


            // Pas de bouton Information/Tooltip
            

            //this._drawBtnPJ = true;

        }

        /// <summary>
        /// Construction de la liste
        /// </summary>
        /// <returns>retourne true si l'opération a réussi</returns>
        protected override bool Init()
        {
            // Récupèration des droits
            if (RightManager == null)
                RightManager = new eRightMailTemplate(Pref);

            // On désactive les boutons actions non necessaire
            _drawBtnTooltip = false;

            _list = eList.CreateMailTemplateList(Pref, _nTabTpl, _mtType, _nPage);

            if (!base.Init())
                return false;

            _drawBtnEdit = _drawBtnEdit && (
           (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAIL
           || _mtType == TypeMailTemplate.MAILTEMPLATE_EMAILING));

            // Pas de paging sur la liste des modèles d'emailing
            if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAILING)
                _rows = _list.ListRecords.Count;
            // Sinon, paging normal
            else
            {
                _rows = _list.RowsByPage;
                _page = _nPage;
            }

            return true;
        }

        /// <summary>
        /// Ajout des controls specifique au template mail
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAILING)
            {
                HiddenField nbTemplates = new HiddenField();
                nbTemplates.ID = String.Concat("NbUserTpl_", TableType.MAIL_TEMPLATE.GetHashCode()); ;
                nbTemplates.Value = _list.ListRecords.Count.ToString();

                this._divHidden.Controls.Add(nbTemplates);
            }

            return base.End();
        }

        /// <summary>
        /// Peuple un panel avec les boutons pour le paging dans la liste des filtres/report
        ///   (uniquement filtre au 07/04/2014)
        /// </summary>
        /// <returns></returns>
        public override void CreatePagingBar(HtmlGenericControl pnTitle)
        {
            base.CreatePagingBar(pnTitle);
        }

        /// <summary>
        /// identifie les paramètres de pagination
        /// Les listes filtre
        /// </summary>
        protected override void SetPagingInfo()
        {
            base.SetPagingInfo();
        }

        protected override bool HasRight(eRecord row)
        {
            if (_dicEmailAdvConfig[eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED] == "1")
            {

                template.Load(row.MainFileid);
                if (template.Body.Contains(" ednc=\"visu\" "))
                    return false;
            }

            this._drawBtnDelete = RightManager.CanDeleteItem();

            return true;
        }


        /// <summary>
        /// Ajout des attributes specifiques sur le bouton d'edition (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionEdit(WebControl webCtrl, eRecord row)
        {
            // #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017
            // grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer la condition _mtType == RENDERERTYPE.EditMailing ci-dessous
            // US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
            webCtrl.Attributes.Add("onclick", String.Concat("javascript:EditMailTemplate(", row.MainFileid + ", ", _mtType.GetHashCode(), ", ", eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.HTMLTemplateEditor) && _mtType == TypeMailTemplate.MAILTEMPLATE_EMAILING ? "true" : "false", ", ", (_dicEmailAdvConfig[eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD] == "1" ? "true" : "false"), ")"));
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de duplication (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDuplicate(WebControl webCtrl, eRecord row)
        {
            //webCtrl.Attributes.Add("onclick", "alert('todo')");
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de rename (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu</param>
        /// <param name="row">record</param>
        protected override void BtnActionRename(WebControl webCtrl, String sElementValueId, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", "alert('todo')");

            String sElementId = sElementValueId.Length > 0 ? sElementValueId : webCtrl.ID;
            webCtrl.Attributes.Add("onclick", String.Concat("renMailTpl('" + sElementId + "', this);"));
            webCtrl.Attributes.Add("ondblclick", String.Concat("stopEvent();return false;"));
        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de tooltip (onclick, ...)
        /// pas de tooltip sur ce rendu
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {
            // webCtrl.Attributes.Add("onmouseover", "shMailTplDesc(event, this);");
            // webCtrl.Attributes.Add("onmouseout", "ht();");
            webCtrl.Attributes.Add("onmouseover", "shTplDescId(" + row.MainFileid + ");");
            webCtrl.Attributes.Add("onmouseout", "ht();");

        }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", "delMailTpl(" + row.MainFileid + ")");
        }



        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, Int32 idxLine)
        {
            base.CustomTableRow(row, trRow, idxLine);

            trRow.Attributes.Add("onclick", string.Concat("selectMailTpl(this);"));
            trRow.Attributes.Add("ondblclick", string.Concat("oTemplate.dblclckUserTemplate(this);return false;"));
            trRow.Attributes.Add("mtid", row.MainFileid.ToString());

            //String error = String.Empty;
            //this.template.LoadAttachments(out error);
            //trRow.Attributes.Add("pjlist", String.Join(";", this.template.ListTemplatePJ));

        }




    }
}