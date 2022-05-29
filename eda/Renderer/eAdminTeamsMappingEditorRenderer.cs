using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.Teams;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// classe de rendu de l'éditeur de mapping teams
    /// </summary>
    public class eAdminTeamsMappingEditorRenderer : eAdminRenderer
    {

        private eTeamsMapping _mapping;
        private eTeamsMappingEditorTools _editorTools;


        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="mapping">mapping tel qu'enregistré en base</param>
        /// <param name="editorTools">objet fournissant toutes les données nécessaires pour proposer les bonnes options à l'admin</param>
        private eAdminTeamsMappingEditorRenderer(ePref pref, eTeamsMapping mapping, eTeamsMappingEditorTools editorTools)
        {
            Pref = pref;
            _mapping = mapping;
            _editorTools = editorTools;
        }


        /// <summary>
        /// méthode statique de construction extérieure
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="mapping">mapping tel qu'enregistré en base</param>
        /// <param name="editorTools">objet fournissant toutes les données nécessaires pour proposer les bonnes options à l'admin</param>
        /// <param name="errorContainer">Erreur à afficher le cas échéant</param>
        public static eAdminTeamsMappingEditorRenderer CreateRenderer(ePref pref, eTeamsMapping mapping, eTeamsMappingEditorTools editorTools, eErrorContainer errorContainer)
        {
            eAdminTeamsMappingEditorRenderer rdr = new eAdminTeamsMappingEditorRenderer(pref, mapping, editorTools);


            //erreur non bloquante
            if (errorContainer != null && errorContainer.IsSet)
            {

                HtmlGenericControl p = new HtmlGenericControl("p");
                p.InnerText = errorContainer.Msg;
                rdr.PgContainer.Controls.Add(p);

                p = new HtmlGenericControl("p");
                p.InnerText = errorContainer.Detail;
                rdr.PgContainer.Controls.Add(p);

                if (eLibTools.IsLocalOrEudoMachine())
                {
                    p = new HtmlGenericControl("p");
                    p.Style.Add(HtmlTextWriterStyle.Display, "none");
                    p.InnerText = errorContainer.DebugMsg;
                    rdr.PgContainer.Controls.Add(p);
                }
            }


            try
            {
                rdr.Generate();
            }
            catch (Exception e)
            {
                rdr._eException = e;
                rdr._sErrorMsg = "An Error has occured while generating Teams Mapping's Editor";
            }


            return rdr;
        }

        protected override bool Build()
        {
            eCheckBoxCtrl cbTeamsEnabledCtrl = new eCheckBoxCtrl(_mapping.Enabled, false);
            PgContainer.Controls.Add(cbTeamsEnabledCtrl);

            //Activer la prise de Rendez - vous Teams
            cbTeamsEnabledCtrl.AddText(eResApp.GetRes(Pref, 3027));
            cbTeamsEnabledCtrl.ID = "cbEnableTeams";
            cbTeamsEnabledCtrl.AddClick();

            PgContainer.Controls.Add(getTriggersPanel());

            PgContainer.Controls.Add(getMeetingPropertiesPanel());

            PgContainer.Controls.Add(getUsersRecipients());

            PgContainer.Controls.Add(getBookmarkRecipients());

            PgContainer.Controls.Add(getTeamsMeetingReferences());

            return base.Build();
        }



        private Panel getTriggersPanel()
        {
            Panel pn = getDefaultadminTeamsMappingPanel();

            //Définir le bouton surlequel créer le rendez - vous Teams
            pn.Controls.Add(getSubtitle(eResApp.GetRes(Pref, 3028)));

            pn.Controls.Add(getTriggersContent());

            return pn;

        }


        private Panel getTriggersContent()
        {
            Panel pn = getDefaultContentPanel();

            //choix du bouton déclencheur de création du rdv Teams (et ultérieurement mise à jour)
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: _editorTools.GetCreateFieldsOptions,
                    iSelectedDescId: _mapping.CreateSaveBtnDescId,
                    sText: eResApp.GetRes(Pref, 3029),  //Créer un Rendez-vous Teams 
                    sID: "Save",
                    bRequired: true

                )
                );
            return pn;

        }




        private Panel getMeetingPropertiesPanel()
        {
            Panel pn = getDefaultadminTeamsMappingPanel();

            pn.Controls.Add(getSubtitle(eResApp.GetRes(Pref, 3030))); //Définir les rubriques alimentant le rendez-vous Teams   

            pn.Controls.Add(getMeetingPropertiesContent());



            return pn;

        }

        private Panel getMeetingPropertiesContent()
        {
            Panel pn = getDefaultContentPanel();

            //début de la réunion
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (_editorTools.GetStartFieldsOptions),
                    iSelectedDescId: _mapping.StartDateDescId,
                    sText: eResApp.GetRes(Pref, 3031),     //Date de début   
                    sID: "Start",
                    bRequired: true
                )
                );
            //fin de la réunion
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (_editorTools.GetEndFieldsOptions),
                    iSelectedDescId: _mapping.EndDateDescId,
                    sText: eResApp.GetRes(Pref, 3032) //    Date de fin 
                    , sID: "End"
                    , bRequired: true
                )
                );
            //titre de la réunion
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (_editorTools.GetTitleFieldsOptions),
                    iSelectedDescId: _mapping.TitleDescId,
                    sText: eResApp.GetRes(Pref, 3033)   //"Titre"  
                    , sID: "Title"
                    , bRequired: true
                )
                );
            //descriptif  de la réunion
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (_editorTools.GetDescriptionFieldsOptions),
                    iSelectedDescId: _mapping.DescriptionDescId,
                    sText: eResApp.GetRes(Pref, 3034)    // "Description"  
                    , sID: "Description"

                )
                );
            return pn;
        }

        private Panel getUsersRecipients()
        {
            Panel pn = getDefaultadminTeamsMappingPanel();

            pn.Controls.Add(getSubtitle("Définir la / les rubrique(s) utilisateurs destinataires"));

            pn.Controls.Add(getUsersRecipientsContent());

            return pn;

        }

        private Panel getUsersRecipientsContent()
        {
            Panel pn = getDefaultContentPanel();

            //on a les rubriques sélectionnées dans l'objet mapping, on récupère les libellés dans l'editorTools
            Dictionary<int, string> dicSelectedUsersFields = _mapping.UserRecipientsDescids.ToDictionary(d => d, d => _editorTools.GetUsersFieldsOptions[d]);


            eRenderer rdr;
            rdr = eFieldsSelectRenderer.CreateFieldsSelectRenderer(Pref, _editorTools.GetUsersFieldsOptions, dicSelectedUsersFields);
            rdr.Generate();
            pn.Controls.Add(rdr.PgContainer);
            rdr.PgContainer.CssClass = "userFields";


            return pn;

        }


        private Panel getBookmarkRecipients()
        {
            Panel pn = getDefaultadminTeamsMappingPanel();

            pn.Controls.Add(getSubtitle("Choisir les destinataires dans les signets"));

            //le modèle est prévu pour prendre en charge plusieurs signets (en anticipation)
            //mais actuellement l'interface et la US n'en prennent qu'un seul en charge
            RecipientFields recipientFields;
            if (_mapping.RecipientsFieldsDescIds?.Count > 0)
                recipientFields = _mapping.RecipientsFieldsDescIds[0];
            else
                recipientFields = new RecipientFields();

            //idem remarque ci-dessus
            eTeamsBookmarkMappingEditorTools bookmarkEditorTool;
            if (_editorTools.BookmarksMappingEditorTools?.Count > 0)
                bookmarkEditorTool = _editorTools.BookmarksMappingEditorTools[0];
            else
                bookmarkEditorTool = new eTeamsBookmarkMappingEditorTools();

            pn.Controls.Add(getBookmarkRecipientsContent(recipientFields, bookmarkEditorTool));

            return pn;

        }
        private Panel getBookmarkRecipientsContent(RecipientFields recipientFields, eTeamsBookmarkMappingEditorTools bookmarkEditorTool)
        {
            Panel pn = getDefaultContentPanel();

            //Choix du signet
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: _editorTools.DicBookmarks,
                    iSelectedDescId: recipientFields.TabDescId,
                    sText: eResApp.GetRes(Pref, 3035),      //"Signet",  
                    sID: "Bookmark"
                )
                );

            //email
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (bookmarkEditorTool.GetMailFieldsOptions),
                    iSelectedDescId: recipientFields.MailDescId,
                    sText: eResApp.GetRes(Pref, 3036), //"Email",  
                    sID: "BkmMail",
                    bRequired: true
                )
                );

            //Nom
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (bookmarkEditorTool.GetNameFieldsOptions),
                    iSelectedDescId: recipientFields.NameDescId,
                    sText: eResApp.GetRes(Pref, 3037),       //"Nom",  
                    sID: "BkmName"
                )
                );

            //Prénom
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (bookmarkEditorTool.GetFirstnameFieldsOptions),
                    iSelectedDescId: recipientFields.FirstNameDescId,
                    sText: eResApp.GetRes(Pref, 3038),      //  "Prénom",  
                    sID: "BkmFirstName"
                )
                );
            return pn;

        }


        private Panel getTeamsMeetingReferences()
        {
            Panel pn = getDefaultadminTeamsMappingPanel();
            //Définir les rubriques qui recevront les références du rendez-vous
            pn.Controls.Add(getSubtitle(eResApp.GetRes(Pref, 3039)));
            pn.Controls.Add(getTeamsMeetingReferencesContent());
            return pn;
        }
        private Panel getTeamsMeetingReferencesContent()
        {
            Panel pn = getDefaultContentPanel();

            //URL pour rejoindre la réunion
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (_editorTools.GetURLFieldsOptions),
                    iSelectedDescId: _mapping.URLMeetingDescid,
                    sText: eResApp.GetRes(Pref, 3040),  // "URL de la réunion",  
                    sID: "URL"
                )
                );
            //fin de la réunion
            pn.Controls.Add(
                getDDLPanel(
                    dicSource: (_editorTools.GetTeamsEventIdFieldsOptions),
                    iSelectedDescId: _mapping.TeamsEventIdDescid,
                    sText: eResApp.GetRes(Pref, 3041),       // "ID Teams"  
                    sID: "TeamsID",
                    bRequired: true
                )
                );

            return pn;

        }


        private Panel getDDLPanel(
            IDictionary<int, string> dicSource,
            int iSelectedDescId,
            string sText,
            string sID,
            bool bRequired = false
            )
        {
            Panel pn = new Panel();
            pn.CssClass = "dvDDLParam";
            pn.ID = $"dv{sID}";

            HtmlGenericControl ctrl = new HtmlGenericControl("div");
            ctrl.Attributes.Add("class", "label");
            ctrl.Controls.Add(new LiteralControl(sText));
            if (bRequired)
            {
                HtmlGenericControl span = new HtmlGenericControl("span") { InnerText = "*" };
                ctrl.Controls.Add(span);
                span.Attributes.Add("class", "required");
            }

            pn.Controls.Add(ctrl);

            DropDownList ddlOptions = new DropDownList();
            pn.Controls.Add(ddlOptions);
            ddlOptions.Attributes.Add("onchange", "nsAdminTeamsMapping.onChangeAction(this);");
            ddlOptions.DataSource = dicSource;
            ddlOptions.DataTextField = "Value";
            ddlOptions.DataValueField = "Key";
            ddlOptions.DataBind();
            ddlOptions.ID = $"ddl{sID}";

            ListItem liNoOption = new ListItem(eResApp.GetRes(Pref, 3042), 0.ToString()); //Aucun élément sélectionné   
            ddlOptions.Items.Insert(0, liNoOption);
            liNoOption.Attributes.Add("class", "noOption");

            ddlOptions.SelectedValue = iSelectedDescId.ToString();

            return pn;
        }

        private Panel getSubtitle(string sText)
        {


            Panel div = new Panel();
            div.Attributes.Add("class", "adminTeamsMappingTitle");

            Panel subDiv = new Panel();
            subDiv.CssClass = "TitleText";
            subDiv.Controls.Add(new LiteralControl(sText)); // Onglet parent
            div.Controls.Add(subDiv);

            //subDiv = new Panel();
            //subDiv.CssClass = "Opener";

            //Panel divOpener = new Panel();
            //divOpener.CssClass = "openerButton";
            //Panel icon = new Panel();
            //icon.CssClass = "icon-chevron-down openerButtonIcon";
            //divOpener.Controls.Add(icon);

            //subDiv.Controls.Add(divOpener);

            div.Controls.Add(subDiv);


            return div;
        }

        private static Panel getDefaultContentPanel()
        {

            Panel pn = new Panel();
            pn.CssClass = "adminTeamsMappingContent";

            return pn;

        }

        private static Panel getDefaultadminTeamsMappingPanel()
        {
            Panel pn = new Panel();
            pn.CssClass = "adminTeamsMapping";
            pn.Attributes.Add("isdisp", "1");


            return pn;
        }


    }
}