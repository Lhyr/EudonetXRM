using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Render pour la gestion des droits de traitements
    /// </summary>
    public class eAdminAutomationFileRenderer : eAdminAbstractAutomationRenderer
    {

        // Données concerne l'automatisme
        eAdminAutomationFile _automationFile;
        AutomationType _automationType;

        // Rubique sur laquelle l'automatisme est prépositionné à la création
        int _field = 0;

        // L'id de l'automatisme
        Int32 _fileId = 0;

        // l'id du filtre attaché à l'automatisme
        Int32 _filterId = 0;
        String _sModalId;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminAutomationFileRenderer(ePref pref, Int32 nTab, Int32 nField, Int32 nFileId, Int32 width, Int32 height, AutomationType automationType, String modalId) : base(pref, nTab)
        {
            if (Pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            _field = nField;
            _width = width;
            _height = height;
            _rType = RENDERERTYPE.EditFile;
            _fileId = nFileId;
            _sModalId = modalId;
            _automationType = automationType;

        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            _pgContainer.ID = "mainDiv";
            _pgContainer.Height = new Unit(_height);
            _automationFile = eAdminAutomationFile.GetAutomationFile(Pref, _nTab, _field, _fileId, _automationType);

            Int32 filterId;
            if (Int32.TryParse(GetFieldValue(NotificationTriggerField.FILTER_TRIGGER), out filterId))
                _filterId = filterId;


            _pgContainer.Attributes.Add("fileid", _fileId.ToString());
            _pgContainer.Attributes.Add("type", ((int)TypeFilter.NOTIFICATION).ToString());
            _pgContainer.Attributes.Add("tab", _nTab.ToString());
            _pgContainer.Attributes.Add("field", _field.ToString());

            return true;
        }

        /// <summary>
        /// Rempli le select par des rubriques de la table
        /// </summary>
        protected override void FillWithTabFields(HtmlGenericControl select, Int32 targetDescId)
        {
            HtmlGenericControl fieldOption;

            if (_automationFile.TargetFields != null)
            {
                foreach (var fld in _automationFile.TargetFields)
                {
                    fieldOption = new HtmlGenericControl("option");

                    if (fld.Descid == targetDescId)
                        fieldOption.Attributes.Add("selected", "selected");

                    fieldOption.Attributes.Add("value", fld.Descid.ToString());
                    fieldOption.InnerText = String.Concat(_automationFile.Files[fld.Table.DescId], ".", fld.Libelle, " (", fld.Descid, ")");
                    select.Controls.Add(fieldOption);
                }
            }
        }

        /// <summary>
        /// Création du contenu de la popup
        /// </summary>
        protected override void GenerateMainContent()
        {

            _panelSections = new Panel();
            _panelContent.Controls.Add(_panelSections);

            base.GenerateMainContent();
        }


        /// <summary>
        /// Ajoute les attribut systems xrm
        /// </summary>
        /// <param name="field"></param>
        /// <param name="htmlControl"></param>
        protected override void RenderSystemAttributes(NotificationTriggerField field, HtmlGenericControl htmlControl)
        {
            htmlControl.ID = GetClientId(field);

            if (_automationFile == null)
                return;

            eFieldRecord fld = _automationFile.GetField((int)field);

            if (fld == null)
                return;

            htmlControl.Attributes.Add("dbv", fld.Value);
            htmlControl.Attributes.Add("fmt", ((int)fld.FldInfo.Format).ToString());
            htmlControl.Attributes.Add("obligat", fld.FldInfo.Obligat ? "1" : "0");
            htmlControl.Attributes.Add("emax", NotifConst.MAX_LIFE_DURATION_DAYS.ToString());
            htmlControl.Attributes.Add("emin", NotifConst.MIN_LIFE_DURATION_DAYS.ToString());
        }

        /// <summary>
        /// Ajoute les attributs au webcontrol
        /// </summary>
        /// <param name="field">Champ de la table NotificationTriggerField</param>
        /// <param name="control">WebControl</param>
        protected override void RenderSystemAttributes(NotificationTriggerField field, WebControl control)
        {
            control.ID = GetClientId(field);

            if (_automationFile == null)
                return;

            eFieldRecord fld = _automationFile.GetField((int)field);

            if (fld == null)
                return;


            if (control is eCheckBoxCtrl)
            {
                eCheckBoxCtrl check = ((eCheckBoxCtrl)control);
                check.SetChecked(fld.Value == "1");
            }
            control.Attributes.Add("dbv", fld.Value);
            control.Attributes.Add("fmt", ((int)fld.FldInfo.Format).ToString());
            control.Attributes.Add("obligat", fld.FldInfo.Obligat ? "1" : "0");
            control.Attributes.Add("emax", NotifConst.MAX_LIFE_DURATION_DAYS.ToString());
            control.Attributes.Add("emin", NotifConst.MIN_LIFE_DURATION_DAYS.ToString());
        }

        /// <summary>
        /// Récupère la valeur du champs du trigger
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected override string GetFieldValue(NotificationTriggerField field)
        {
            if (_automationFile == null)
                return string.Empty;

            eFieldRecord fld = _automationFile.GetField((int)field);

            if (fld == null || String.IsNullOrEmpty(fld.Value))
                return string.Empty;


            return fld.Value;
        }

        /// <summary>
        /// Récupère la valeur à afficher du champs du trigger
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected override string GetFieldDisplayValue(NotificationTriggerField field)
        {
            if (_automationFile == null)
                return string.Empty;

            eFieldRecord fld = _automationFile.GetField((int)field);
            if (fld == null || String.IsNullOrEmpty(fld.Value))
                return string.Empty;


            return fld.DisplayValue;
        }

        protected override void GenerateHeader()
        {

        }

        protected override void GenerateFooter()
        {

        }

        protected override bool EditEnabled()
        {
            return true;
        }

        protected override void AppendAllCasesContent(Panel panelContent)
        {

            Panel caseContent = new Panel();
            caseContent.ID = "innerCases";
            caseContent.Attributes.Add("class", "inner-cases");
            panelContent.Controls.Add(caseContent);

            AddAutomationConditionsBlock(caseContent);

            // Titre de affiché pour l'automatisme
            AddAutomationShortTextBlock(caseContent);

            // Les abonnés
            AddSubscriberUsersBlock(caseContent);

            // Les abonnés depuis la fiche référence
            AddSubscriberUserFieldsBlock(caseContent);

            // Suppression automatique
            AddDeletingOptionBlock(caseContent);

            // Visuel de l'automatismes
            AddVisualAutomationBlock(caseContent);

            //Mode de diffusion
            AddBroadcastOptionBlock(caseContent);

            // On met le contenu du résulta dans le block dondition
            // AddAutomationConditionsBlock(panelContent, caseContent);

        }

        /// <summary>
        /// Ajoute un block de conditions de déclenchement
        /// </summary>
        /// <param name="caseContent"></param>
        private void AddAutomationConditionsBlock(Panel caseContent)
        {
            // Si
            HtmlGenericControl labelAllCases = new HtmlGenericControl("div");
            labelAllCases.ID = "trigger_filter_ifBlock";
            labelAllCases.Attributes.Add("class", "edaAllCases");
            labelAllCases.InnerText = eResApp.GetRes(Pref, 7444).ToUpper(); // "Si"
            labelAllCases.Attributes.Add("data-active", _filterId > 0 ? "1" : "0");
            caseContent.Controls.Add(labelAllCases);

            // Libellé du filtre
            Panel filterLine = new Panel();
            filterLine.ID = "trigger_filter_ruleBlock";
            caseContent.Controls.Add(filterLine);
            filterLine.CssClass = "inner-cases-line bottom-space";
            filterLine.Attributes.Add("data-active", _filterId > 0 ? "1" : "0");


            BuildFilterCondition(filterLine, _filterId);

            // Alors   
            labelAllCases = new HtmlGenericControl("div");
            labelAllCases.ID = "trigger_filter_thenBlock";
            labelAllCases.Attributes.Add("class", "edaAllCases");
            labelAllCases.Attributes.Add("data-active", _filterId > 0 ? "1" : "0");
            labelAllCases.InnerText = eResApp.GetRes(Pref, 7445).ToUpper(); ; //"Alors";
            caseContent.Controls.Add(labelAllCases);

            //Dans tous les cas
            labelAllCases = new HtmlGenericControl("div");
            labelAllCases.ID = "trigger_all_casesBlock";
            labelAllCases.Attributes.Add("class", "edaAllCases");
            labelAllCases.Attributes.Add("data-active", _filterId > 0 ? "0" : "1");
            labelAllCases.InnerText = eResApp.GetRes(Pref, 7227).ToUpper();
            caseContent.Controls.Add(labelAllCases);
        }

        /// <summary>
        /// Construit un block de condition
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
        private Control BuildFilterCondition(Panel filterLine, int filterId)
        {
            Panel filterControl = new Panel();
            filterLine.Controls.Add(filterControl);
            filterControl.CssClass = "inner-filter-line";
            string content = String.Empty;
            string title = String.Empty;

            if (filterId > 0)
            {
                string err;
                string name;
                string description = AdvFilter.GetDescription(Pref, filterId, out name, out err);

                if (err.Length != 0)
                {
#if DEBUG                    
                    content = err;
#else
                    content = "";
#endif
                }
                else
                {
                    content = name;
                    title = description;
                }
            }

            HtmlGenericControl filterName = new HtmlGenericControl("div");
            filterName.InnerHtml = content;
            filterName.ID = "filterNameBlock";
            filterControl.Controls.Add(filterName);
            filterControl.Attributes.Add("title", title);

            // pen
            HtmlGenericControl pen = new HtmlGenericControl("div");
            pen.Attributes.Add("class", "icon-edn-pen field-btn");
            pen.Attributes.Add("onclick", "oAutomation.editFilter('" + GetClientId(NotificationTriggerField.FILTER_TRIGGER) + "');");
            filterControl.Controls.Add(pen);

            // poubelle
            HtmlGenericControl trash = new HtmlGenericControl("div");
            trash.Attributes.Add("class", "icon-delete field-btn field-btn-second");
            trash.Attributes.Add("onclick", "oAutomation.deleteFilter('" + GetClientId(NotificationTriggerField.FILTER_TRIGGER) + "');");
            trash.Attributes.Add("title", eResApp.GetRes(Pref, 7446));// "Supprimer la condition"
            filterControl.Controls.Add(trash);

            HtmlGenericControl input = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.FILTER_TRIGGER, input);
            input.Attributes.Add("type", "hidden");
            filterControl.Controls.Add(input);

            return filterControl;
        }

        /// <summary>
        /// Ajoute un block de conséquence
        /// </summary>
        /// <param name="content"></param>
        private void AddAutomationShortTextBlock(Panel content)
        {
            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line";
            content.Controls.Add(innerContent);
            // Block conséquence
            // Label
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7447);//"Notification : ";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7448));// "Indiquer le titre de la notification qui s’affiche dans une fenêtre surgissante et dans l’objet du mail, lorsque la notification est adressée par mail"
            label.Attributes.Add("style", "font-weight:bold;");
            label.Attributes.Add("class", "active");
            innerContent.Controls.Add(label);

            HtmlGenericControl valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            innerContent.Controls.Add(valueContainer);

            // Valeur
            HtmlGenericControl value = new HtmlGenericControl("div");
            value.ID = GetClientId(NotificationTriggerResField.SHORT_TITLE);
            value.InnerHtml = _automationFile.Res.Title;
            value.Attributes.Add("class", "field-value");
            valueContainer.Controls.Add(value);

            // le btn 
            HtmlGenericControl pen = new HtmlGenericControl("div");
            pen.Attributes.Add("class", "icon-edn-pen field-btn");
            pen.Attributes.Add("onclick", "oAutomation.openMemo(" + (int)NotificationTriggerResField.SHORT_TITLE + ", '" + HttpUtility.JavaScriptStringEncode(eResApp.GetRes(Pref, 7619)) + "');");
            valueContainer.Controls.Add(pen);

        }

        /// <summary>
        /// Ajoute les utilisateur aponnés a l'automatisme
        /// </summary>
        /// <param name="content"></param>
        private void AddSubscriberUsersBlock(Panel content)
        {

            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line";
            content.Controls.Add(innerContent);
            // Block conséquence
            // Label
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7449);// "Utilisateurs abonnés :";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7450)); //"Sélectionner les utilisateurs et groupes destinataires de la notification"
            innerContent.Controls.Add(label);

            HtmlGenericControl valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            innerContent.Controls.Add(valueContainer);

            // Valeur            
            HtmlGenericControl value = new HtmlGenericControl("div");
            RenderSystemAttributes(NotificationTriggerField.SUBSCRIBERS, value);
            value.InnerHtml = GetFieldDisplayValue(NotificationTriggerField.SUBSCRIBERS);
            value.Attributes.Add("class", "field-value");
            valueContainer.Controls.Add(value);

            // le btn 
            HtmlGenericControl pen = new HtmlGenericControl("div");
            pen.Attributes.Add("class", "icon-catalog field-catalog");
            pen.Attributes.Add("eaction", "LNKCATUSER");
            pen.Attributes.Add("onclick", "oAutomation.openUserCat('" + value.ID + "');");
            innerContent.Controls.Add(pen);

            valueContainer.Controls.Add(pen);

        }

        /// <summary>
        /// Ajoute la rubrique de type utilisateur de la fiche référénce
        /// </summary>
        /// <param name="content"></param>
        private void AddSubscriberUserFieldsBlock(Panel content)
        {

            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line";
            content.Controls.Add(innerContent);
            // Block conséquence
            // Label
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7451);//"Rubriques abonnées : ";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7452));//"Sélectionner les rubriques de type Utilisateurs et Groupes qui sont utilisées pour notifier des utilisateurs liés à la fiche en cours ou à ses fiches parentes");

            innerContent.Controls.Add(label);

            HtmlGenericControl valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            innerContent.Controls.Add(valueContainer);

            // Valeur      
            HtmlGenericControl value = new HtmlGenericControl("div");
            RenderSystemAttributes(NotificationTriggerField.SUBSCRIBERS_USER_FIELD, value);
            value.InnerHtml = GetFieldDisplayValue(NotificationTriggerField.SUBSCRIBERS_USER_FIELD);

            value.Attributes.Add("class", "field-value");
            valueContainer.Controls.Add(value);

            // le btn 
            HtmlGenericControl pen = new HtmlGenericControl("div");
            pen.Attributes.Add("class", "icon-rubrique field-selector");
            pen.Attributes.Add("onclick", "oAutomation.fldSelector(" + (int)NotificationTriggerField.SUBSCRIBERS_USER_FIELD + ");");
            innerContent.Controls.Add(pen);

            valueContainer.Controls.Add(pen);
        }

        /// <summary>
        /// Options de suppression après que l'automatisme ait été exécuté
        /// </summary>
        /// <param name="content"></param>
        private void AddDeletingOptionBlock(Panel content)
        {

            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line";
            content.Controls.Add(innerContent);
            // Block conséquence
            // Label
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7453);//"Suppression automatique : ";
            // "Indiquer le nombre de jours pendants lesquels la notification sera conservée en base.\n 30 jours sont conseillés et 100 jours maximum sont autorisés ");
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7454)
                .Replace("<MIN_DAYS>", NotifConst.RECOMANDED_MIN_LIFE_DURATION_DAYS.ToString())
                .Replace("<MAX_DAYS>", NotifConst.RECOMANDED_MAX_LIFE_DURATION_DAYS.ToString()));

            innerContent.Controls.Add(label);

            HtmlGenericControl valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            innerContent.Controls.Add(valueContainer);

            // Valeur
            HtmlGenericControl value = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.LIFE_DURATION, value);

            String val = GetFieldValue(NotificationTriggerField.LIFE_DURATION);
            if (_automationFile.FileId == 0)
                val = NotifConst.RECOMANDED_MIN_LIFE_DURATION_DAYS.ToString();

            value.Attributes.Add("type", "text");
            value.Attributes.Add("value", val);
            value.Attributes.Add("onchange", "oAutomation.checkDurationValue(this,this.value);");
            value.Attributes.Add("class", "field-value-delete");
            valueContainer.Controls.Add(value);


            HtmlGenericControl days = new HtmlGenericControl("div");
            days.InnerHtml = eResApp.GetRes(Pref, 6897);
            valueContainer.Controls.Add(days);
        }


        /// <summary>
        /// Ajoute le visuel des automatisme : image, picto, avatar etc
        /// </summary>
        /// <param name="content"></param>
        private void AddVisualAutomationBlock(Panel content)
        {
            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line bottom-space";
            content.Controls.Add(innerContent);
            // Block conséquence
            // Label
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7455);//"Utiliser le visuel : ";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7456));//"Indiquer le visuel affiché dans la notification");
            innerContent.Controls.Add(label);


            HtmlGenericControl imageSource = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.IMAGESOURCE, imageSource);
            String imageSourceValue = GetFieldValue(NotificationTriggerField.IMAGESOURCE);
            innerContent.Controls.Add(imageSource);
            imageSource.Attributes.Add("type", "hidden");

            HtmlGenericControl imageField = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.IMAGE, imageField);
            String imageFieldValue = GetFieldValue(NotificationTriggerField.IMAGE);
            innerContent.Controls.Add(imageField);
            imageField.Attributes.Add("type", "hidden");

            HtmlGenericControl valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            valueContainer.Style.Add("z-index", "1");
            innerContent.Controls.Add(valueContainer);

            // menu du visuel
            HtmlGenericControl visualMenu = new HtmlGenericControl("ul");
            valueContainer.Controls.Add(visualMenu);
            visualMenu.ID = "visualMenu";
            visualMenu.Attributes.Add("class", "menu-level-0");
            visualMenu.Attributes.Add("picto-picker", imageSourceValue == "0" ? "on" : "off");
            visualMenu.Attributes.Add("display", "off");
            visualMenu.Attributes.Add("onclick", "oAutomation.clickMenu(event, '" + imageSource.ID + "', '" + imageField.ID + "');");


            // Le niveau 0 est la sélection
            AddMenuItem(visualMenu, "selected-visual", GetSelectedValue(imageSourceValue, imageFieldValue),
                delegate (HtmlGenericControl parentMenuItem0)
                {
                    HtmlGenericControl subMenuItems = new HtmlGenericControl("ul");
                    subMenuItems.Attributes.Add("class", "menu-level-1");
                    parentMenuItem0.Controls.Add(subMenuItems);

                    // Ajout après l'url
                    HtmlGenericControl arrowDown = new HtmlGenericControl("span");
                    arrowDown.ID = "arrow-down";
                    parentMenuItem0.Controls.Add(arrowDown);
                    arrowDown.Attributes.Add("class", "menu-item-arrow icon-caret-down");


                    AddMenuItem(subMenuItems, String.Empty, eResApp.GetRes(Pref, 6819) /*"Pictogramme"*/,
                        delegate (HtmlGenericControl avatar)
                        {
                            HtmlGenericControl subMenuAvatar = new HtmlGenericControl("ul");
                            subMenuAvatar.Attributes.Add("class", "menu-level-2");
                            avatar.Controls.Add(subMenuAvatar);

                            // Ajout après l'url
                            HtmlGenericControl arrow = new HtmlGenericControl("span");
                            avatar.Controls.Add(arrow);
                            arrow.Attributes.Add("class", "menu-item-arrow icon-chevron-right");

                            AddMenuItem(subMenuAvatar, "1", eResApp.GetRes(Pref, 7457) /*"Pictogramme de l'onglet"*/,
                                delegate (HtmlGenericControl pictogramme)
                                {
                                    pictogramme.Attributes.Add("imgsrc", ((int)NotifConst.ImageSource.PICTOGRAMME).ToString());
                                    pictogramme.Attributes.Add("descid", "");
                                    pictogramme.Attributes.Add("lib", eResApp.GetRes(Pref, 7457) /*"Pictogramme de l'onglet"*/);
                                });

                            AddMenuItem(subMenuAvatar, "2", eResApp.GetRes(Pref, 7460) /*"Personnaliser ..."*/,
                                delegate (HtmlGenericControl pictogramme)
                                {
                                    pictogramme.Attributes.Add("imgsrc", ((int)NotifConst.ImageSource.PICTOGRAMME).ToString());
                                    pictogramme.Attributes.Add("descid", "");
                                    pictogramme.Attributes.Add("picto", GetClientId(NotificationTriggerField.ICON));
                                    pictogramme.Attributes.Add("lib", eResApp.GetRes(Pref, 7458) /*"Pictogramme personnalisé"*/);
                                });
                        });

                    AddMenuItem(subMenuItems, String.Empty, eResApp.GetRes(Pref, 7461) /*"Avatar utilisateur"*/,
                        delegate (HtmlGenericControl avatar)
                        {
                            HtmlGenericControl subMenuAvatar = new HtmlGenericControl("ul");
                            subMenuAvatar.Attributes.Add("class", "menu-level-2");
                            avatar.Controls.Add(subMenuAvatar);

                            // Ajout après l'url
                            HtmlGenericControl arrow = new HtmlGenericControl("span");
                            avatar.Controls.Add(arrow);
                            arrow.Attributes.Add("class", "menu-item-arrow icon-chevron-right");

                            // Affichage des rubriques utilisateur simples
                            if (_automationFile.TargetFieldsSingleUser != null)
                            {
                                foreach (eFieldLiteAdmin field in _automationFile.TargetFieldsSingleUser)
                                    AddMenuItem(subMenuAvatar, field.Descid.ToString(), field.Libelle + "   (" + field.Descid.ToString() + ")",
                                        delegate (HtmlGenericControl imageFieldControl)
                                        {
                                            imageFieldControl.Attributes.Add("imgsrc", ((int)NotifConst.ImageSource.AVATAR_USER_FIELD).ToString());
                                            imageFieldControl.Attributes.Add("descid", field.Descid.ToString());
                                            imageFieldControl.Attributes.Add("lib", _automationFile.Files[eLibTools.GetTabFromDescId(field.Descid)] + "." + field.Libelle);
                                        });
                            }
                        });

                    //Demande #66761
                    //On n'affiche pas ce menu s il n'y a pas de rubrique images
                    IEnumerable<eFieldLiteAdmin> listImagefields = _automationFile.Imagefields.Where(f => f.Table.DescId != _automationFile.TargetTabInfos.DescId);
                    if (listImagefields.Count() > 0)
                    {
                        AddMenuItem(subMenuItems, String.Empty, eResApp.GetRes(Pref, 7465) /* "Rubrique Image"*/,
                            delegate (HtmlGenericControl parentMenuItem1)
                            {
                                HtmlGenericControl subMenuImageFields = new HtmlGenericControl("ul");
                                subMenuImageFields.Attributes.Add("class", "menu-level-2");
                                parentMenuItem1.Controls.Add(subMenuImageFields);

                                // Ajout après l'url
                                HtmlGenericControl arrow2 = new HtmlGenericControl("span");
                                parentMenuItem1.Controls.Add(arrow2);
                                arrow2.Attributes.Add("class", "menu-item-arrow icon-chevron-right");

                                //  Affichage des tables
                                foreach (var file in _automationFile.ImageFiles)
                                {
                                    //Demande #66761
                                    //On masque les rubriques images de la propre table, car cela pose problème pour le moment (image pas encore créée au moment de la création de la notif)
                                    //Pour les tables parentes et la table user ca a l'air OK car les fiches sont forcément déjà créées
                                    if (file.Key == _automationFile.TargetTabInfos.DescId)
                                        continue;

                                    AddMenuItem(subMenuImageFields, String.Empty, file.Value,
                                        delegate (HtmlGenericControl parentMenuItem2)
                                        {
                                            HtmlGenericControl subMenuImageFiles = new HtmlGenericControl("ul");
                                            subMenuImageFiles.Attributes.Add("class", "menu-level-3");
                                            parentMenuItem2.Controls.Add(subMenuImageFiles);

                                            // Ajout après l'url
                                            HtmlGenericControl arrow3 = new HtmlGenericControl("span");
                                            parentMenuItem2.Controls.Add(arrow3);
                                            arrow3.Attributes.Add("class", "menu-item-arrow icon-chevron-right");

                                            // Affichage des rubriques
                                            foreach (var field in listImagefields)
                                                if (field.Table.DescId == file.Key)
                                                    AddMenuItem(subMenuImageFiles, field.Descid.ToString(), field.Libelle + "   (" + field.Descid.ToString() + ")",
                                                        delegate (HtmlGenericControl imageFieldControl)
                                                        {
                                                            imageFieldControl.Attributes.Add("imgsrc", ((int)NotifConst.ImageSource.IMAGE_FIELD).ToString());
                                                            imageFieldControl.Attributes.Add("descid", field.Descid.ToString());
                                                            imageFieldControl.Attributes.Add("lib", file.Value + "." + field.Libelle);
                                                        });
                                        });
                                }
                            });
                    }
                }); 
        


            // menu du visuel
            HtmlGenericControl pictoContainer = new HtmlGenericControl("div");
            pictoContainer.ID = "picto-container";
            pictoContainer.Attributes.Add("class", "picto-ctn field-picto");

            HtmlGenericControl picto = new HtmlGenericControl("span");
            RenderSystemAttributes(NotificationTriggerField.ICON, picto);

            String color = GetFieldValue(NotificationTriggerField.COLOR);
            if (String.IsNullOrEmpty(color) || imageSourceValue != "0") // si l'image sourcen'est pas picto on prend la couleur de l'onglet
                color = _automationFile.TargetTabInfos.IconColor;

            String iconKey = GetFieldValue(NotificationTriggerField.ICON);
            if (String.IsNullOrEmpty(iconKey))
                iconKey = _automationFile.TargetTabInfos.Icon;

            eFontIcons.FontIcons icon = eFontIcons.GetFontIcon(iconKey);

            picto.Attributes.Add("class", "picto-btn " + icon.CssName);
            picto.Attributes.Add("picto-color", color);
            picto.Style.Add("color", color);
            picto.Attributes.Add("picto-key", iconKey);
            picto.Attributes.Add("picto-class", icon.CssName);
            picto.Attributes.Add("fld-color", GetClientId(NotificationTriggerField.COLOR));
            picto.Attributes.Add("onclick", "oAutomation.picto(this);");

            pictoContainer.Controls.Add(picto);

            HtmlGenericControl colorInput = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.COLOR, colorInput);
            colorInput.Attributes.Add("type", "hidden");

            pictoContainer.Controls.Add(colorInput);

            valueContainer.Controls.Add(pictoContainer);


        }

        /// <summary>
        /// Retourne la selection du visual
        /// </summary>
        /// <returns></returns>
        private string GetSelectedValue(String sourceValue, String fieldValue)
        {

            if (String.IsNullOrEmpty(sourceValue))
                return eResApp.GetRes(Pref, 7457);//"Pictogramme de l'onglet"; //

            NotifConst.ImageSource source = (NotifConst.ImageSource)Enum.Parse(typeof(NotifConst.ImageSource), sourceValue);
            if (source == NotifConst.ImageSource.AVATAR_USER)
                return eResApp.GetRes(Pref, 370);//"Utilisateur en cours";//

            if (source == NotifConst.ImageSource.PICTOGRAMME)
                return eResApp.GetRes(Pref, 7458);//"Pictogramme personnalisé";//


            if (String.IsNullOrEmpty(fieldValue))
                return eResApp.GetRes(Pref, 7457);//"Pictogramme de l'onglet";//

            Int32 descid;
            if (Int32.TryParse(fieldValue, out descid))
            {
                Int32 tab = eLibTools.GetTabFromDescId(descid);
                switch (source)
                {
                    case NotifConst.ImageSource.AVATAR_USER_FIELD:
                        eFieldLiteAdmin field = _automationFile.TargetFieldsSingleUser?.FirstOrDefault(f => f.Descid == descid);
                        if (field != null)
                            return String.Concat(_automationFile.Files[tab], ".", field.Libelle);
                        return "***(" + descid + " )***";
                    case NotifConst.ImageSource.IMAGE_FIELD:
                        return String.Concat(_automationFile.Files[tab], ".", _automationFile.Imagefields.Find(f => f.Descid == descid).Libelle);
                }
            }

            return eResApp.GetRes(Pref, 7457); //"Pictogramme de l'onglet";//
        }


        /// <summary>
        /// Ajoute une entrée dans le menu
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="itemId"></param>
        /// <param name="itemText"></param>
        /// <param name="callback"></param>
        private void AddMenuItem(HtmlGenericControl menu, String itemId, String itemText, Action<HtmlGenericControl> callback = null)
        {
            HtmlGenericControl item = new HtmlGenericControl("li");

            HtmlGenericControl span = new HtmlGenericControl("span");
            if (!String.IsNullOrEmpty(itemId))
                span.ID = itemId;
            span.InnerHtml = itemText;
            item.Controls.Add(span);
            if (callback != null)
                callback(item);

            menu.Controls.Add(item);
        }

        /// <summary>
        /// Affiche les options de diffusion
        /// </summary>
        /// <param name="content"></param>
        private void AddBroadcastOptionBlock(Panel content)
        {

            #region Diffusion dans XRM aux abonnés

            String val = GetFieldValue(NotificationTriggerField.BROADCAST_TYPE);
            NotifConst.Broadcast broadcast = NotifConst.Broadcast.NONE;
            if (!String.IsNullOrEmpty(val))
                broadcast = (NotifConst.Broadcast)Enum.Parse(typeof(NotifConst.Broadcast), val);


            // Affichage ou pas du block d'edition du corp de mail
            Boolean bBroadCast = false;
            bBroadCast = broadcast.HasFlag(NotifConst.Broadcast.XRM);
            content.Attributes.Add("describe", bBroadCast ? "1" : "0");

            Panel innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line";


            // broadcast
            HtmlGenericControl broadcastField = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.BROADCAST_TYPE, broadcastField);
            broadcastField.Attributes.Add("type", "hidden");
            innerContent.Controls.Add(broadcastField);

            content.Controls.Add(innerContent);
            eCheckBoxCtrl cbo = new eCheckBoxCtrl(bBroadCast, false);
            cbo.AddText(eResApp.GetRes(Pref, 7466), eResApp.GetRes(Pref, 7467));
            cbo.AddClick("oAutomation.broadcast(this, 'describe', '" + broadcastField.ID + "', " + (int)NotifConst.Broadcast.XRM + ");");
            innerContent.Controls.Add(cbo);


            innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line describe";
            content.Controls.Add(innerContent);
            // Label
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7468); //"Descriptif : ";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7469)/*"Indiquer le descriptif de la notification qui s’affiche dans la liste des notifications"*/);
            innerContent.Controls.Add(label);

            HtmlGenericControl valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            innerContent.Controls.Add(valueContainer);

            // Valeur
            HtmlGenericControl value = new HtmlGenericControl("div");
            value.ID = GetClientId(NotificationTriggerResField.SHORT_DESCRIPTION);
            value.InnerHtml = _automationFile.Res.Description;
            value.Attributes.Add("for", "pen_" + ((int)NotificationTriggerResField.SHORT_DESCRIPTION));
            value.Attributes.Add("class", "field-value");
            valueContainer.Controls.Add(value);

            // pen
            HtmlGenericControl pen = new HtmlGenericControl("div");
            pen.ID = "pen_" + (int)NotificationTriggerResField.SHORT_DESCRIPTION;
            pen.Attributes.Add("class", "icon-edn-pen field-btn");
            pen.Attributes.Add("onclick", "oAutomation.openMemo(" + (int)NotificationTriggerResField.SHORT_DESCRIPTION + ", '" + HttpUtility.JavaScriptStringEncode(eResApp.GetRes(Pref, 7620)) + "');");
            valueContainer.Controls.Add(pen);


            #endregion

            #region Diffusion par mail aux abonnés
            bBroadCast = broadcast.HasFlag(NotifConst.Broadcast.MAIL);
            content.Attributes.Add("mail", bBroadCast ? "1" : "0");

            innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line";
            content.Controls.Add(innerContent);
            cbo = new eCheckBoxCtrl(bBroadCast, false);
            cbo.AddText(eResApp.GetRes(Pref, 7470), eResApp.GetRes(Pref, 7471));
            cbo.AddClick("oAutomation.broadcast(this, 'mail', '" + broadcastField.ID + "', " + (int)NotifConst.Broadcast.MAIL + ");");
            innerContent.Controls.Add(cbo);
            #endregion

            #region Diffusion par mail aux autres utilisateurs
            innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line";
            content.Controls.Add(innerContent);


            String otherMails = GetFieldDisplayValue(NotificationTriggerField.SUBSCRIBERS_FREE_EMAIL).Trim();
            content.Attributes.Add("others", otherMails.Length > 0 ? "1" : "0");

            cbo = new eCheckBoxCtrl(otherMails.Length > 0, false);
            cbo.AddText(eResApp.GetRes(Pref, 7472), eResApp.GetRes(Pref, 7473));
            cbo.AddClick("oAutomation.broadcast(this, 'others', '" + broadcastField.ID + "', " + (int)NotifConst.Broadcast.MAIL + ");oAutomation.freeMail(this, '" + GetClientId(NotificationTriggerField.SUBSCRIBERS_FREE_EMAIL) + "')");
            innerContent.Controls.Add(cbo);

            #endregion

            #region Corps de mail mail aux autres utilisateurs
            innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line mail";

            content.Controls.Add(innerContent);

            // Label
            label = new HtmlGenericControl("div");
            label.InnerHtml = eResApp.GetRes(Pref, 7474); //"Corps du mail : ";
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7475)/*"Indiquer le descriptif de la notification qui s’affiche dans la liste des notifications "*/);
            innerContent.Controls.Add(label);

            valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            innerContent.Controls.Add(valueContainer);

            // Valeur
            value = new HtmlGenericControl("div");
            value.ID = GetClientId(NotificationTriggerResField.EMAIL_DESCRIPTION);
            value.InnerHtml = _automationFile.Res.MailBody;
            value.Attributes.Add("class", "field-value");
            value.Attributes.Add("for", "pen_" + ((int)NotificationTriggerResField.EMAIL_DESCRIPTION));
            valueContainer.Controls.Add(value);

            // pen
            pen = new HtmlGenericControl("div");
            pen.ID = "pen_" + ((int)NotificationTriggerResField.EMAIL_DESCRIPTION);
            pen.Attributes.Add("class", "icon-edn-pen field-btn");
            pen.Attributes.Add("onclick", "oAutomation.openMemo(" + (int)NotificationTriggerResField.EMAIL_DESCRIPTION + ", '" + HttpUtility.JavaScriptStringEncode(eResApp.GetRes(Pref, 7621)) + "');");
            valueContainer.Controls.Add(pen);
            #endregion

            #region Autres utilisateurs
            innerContent = new Panel();
            innerContent.CssClass = "inner-cases-line others";
            content.Controls.Add(innerContent);

            // Label
            label = new HtmlGenericControl("div");
            label.Attributes.Add("title", eResApp.GetRes(Pref, 7476)); //"Sélectionner des adresses mail séparées par des ';'");
            label.InnerHtml = eResApp.GetRes(Pref, 7477); //"Autres destinataires par mail : ";
            innerContent.Controls.Add(label);

            valueContainer = new HtmlGenericControl("div");
            valueContainer.Attributes.Add("class", "value-container");
            innerContent.Controls.Add(valueContainer);

            // Valeur
            value = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.SUBSCRIBERS_FREE_EMAIL, value);
            value.Attributes.Add("value", otherMails);
            value.Attributes.Add("type", "text");
            value.Attributes.Add("onchange", "setAttributeValue(this,'dbv', this.value);");

            valueContainer.Controls.Add(value);

            #endregion

        }


        /// <summary>
        /// Génère l'action sur une etape
        /// </summary>
        /// <param name="span"></param>
        protected override void GenerateActionStep(HtmlGenericControl step, string actionNum)
        {
            if (actionNum == "2")
            {
                HtmlGenericControl span = new HtmlGenericControl("span");
                span.ID = "trigger_all_newCondition";
                span.Attributes.Add("class", "autoButtonAdd");
                span.Attributes.Add("data-active", _filterId > 0 ? "0" : "1");
                span.Attributes.Add("onclick", "oAutomation.newCondition('" + GetClientId(NotificationTriggerField.FILTER_TRIGGER) + "');");
                span.InnerHtml = String.Concat("<span class='icon-add active'></span><span>", eResApp.GetRes(Pref, 7478)/*"Nouvelle condition"*/, "</span>");
                step.Controls.Add(span);
            }
        }


        protected override bool End()
        {

            AddCallBackScript(_automationFile.MergeFieldsScriptObject);

            return !base.End();
        }

        /// <summary>
        /// Creation d'une instance de ce renderer
        /// </summary>
        /// <param name="pref">Pref de l'admin </param>
        /// <param name="nTab">Table sur laquelle l'automatisme sera appliqué</param>
        /// <param name="field">descid de la rubrique</param>
        /// <param name="fileId"> file id de l'automatisme: 0 si création</param>
        /// <param name="width">largeur de l'ecran</param>
        /// <param name="height">hauteur de l'ecran</param>
        /// <param name="type">type de l'automatisme</param>
        /// <returns>un renderer</returns>
        public static eRenderer CreateAdminAutomationFileRenderer(ePref pref, int tab, int field, int fileId, int width, int height, AutomationType autoType, String modalId)
        {
            return new eAdminAutomationFileRenderer(pref, tab, field, fileId, width, height, autoType, modalId);
        }

    }
}