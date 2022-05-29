using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer d'options utilisateur - Choix de la langue
    /// </summary>
    public class eAdminUsrOptMemoRenderer : eUserOptionsRenderer
    {

        protected string _sTitle;
        protected string _sSubTitle;
        protected string _sMemoContent;
        protected string _sActionOk;
        protected string _sActionCancel;

        protected string _sEditorType;
        protected string _sToolbarType;
        /// <summary>
        /// #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
        /// Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
        /// </summary>
        protected bool _bEnableTemplateEditor;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminUsrOptMemoRenderer(ePref pref, Int32 nUserId)
            : base(pref, eUserOptionsModules.USROPT_MODULE.PREFERENCES_MEMO, nUserId)
        {
            _sEditorType = "adminusermemo";
            _sToolbarType = "adminusermemo";
            /// #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
            /// Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
            _bEnableTemplateEditor = false;
        }


        public eAdminUsrOptMemoRenderer(ePref pref, eUserOptionsModules.USROPT_MODULE module) : base(pref, module)
        {

        }



        /// <summary>
        /// initialise les var propre au renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {


            if (!eUser.GetFieldValue<String>(Pref, _nUserId, "UserMessage", out _sMemoContent))
                _sMemoContent = String.Empty;

            _sTitle = eUserOptionsModules.GetModuleMiddleTitleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_MEMO, Pref);
            _sSubTitle = eUserOptionsModules.GetModuleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_MEMO, Pref);
            _sActionOk = "setMemo();";
            _sActionCancel = String.Concat("loadUserOption('", eUserOptionsModules.USROPT_MODULE.PREFERENCES.ToString(), "');");

            return true;



        }


        /// <summary>
        /// Construction de la zone de titre
        /// </summary>
        protected virtual void BuildTitle(Panel p)
        {
            #region Les titres
            HtmlGenericControl pnlTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
            pnlTitle.Attributes.Add("class", "adminModalMiddleTitle ");
            pnlTitle.InnerText = _sTitle;
            p.Controls.Add(pnlTitle);


        }


        /// <summary>
        /// Construction de la zone de sous-titre
        /// </summary>
        protected virtual void BuildSubTitle(Panel pnl)
        {
            HtmlGenericControl pnlSubTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
            pnlSubTitle.ID = "memoSubTitle";
            pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
            pnlSubTitle.InnerText = _sSubTitle;
            #endregion

            pnl.Controls.Add(pnlSubTitle);
        }


        /// <summary>
        /// Construction des bouttons si non construit en js
        /// </summary>
        protected virtual void BuildButton(Panel pnl)
        {
            #region Les boutons
            Panel btnPart = new Panel();
            btnPart.CssClass = "adminBtnPart";
            eButtonCtrl btnCancel = new eButtonCtrl(eResApp.GetRes(Pref, 29), eButtonCtrl.ButtonType.GRAY, _sActionCancel); // Annuler

            eButtonCtrl btnValidate = new eButtonCtrl(eResApp.GetRes(Pref, 28), eButtonCtrl.ButtonType.GREEN, _sActionOk); // Valider
            btnPart.Controls.Add(btnValidate);
            btnPart.Controls.Add(btnCancel);
            #endregion

            pnl.Controls.Add(btnPart);
        }


        /// <summary>
        /// Champ mémo
        /// </summary>
        /// <param name="pnl">Panel conteneur</param>
        /// <param name="sMemoContent">Contenu du mémo</param>
        protected virtual void BuildMemoField(Panel pnl)
        {

            HtmlGenericControl pnlMainPart = new HtmlGenericControl("div");

            #region Corps de la signature (CKEditor)

            Panel signBody = new Panel();
            EdnWebControl ednSignBody = new EdnWebControl() { WebCtrl = signBody, TypCtrl = EdnWebControl.WebControlType.PANEL };
            signBody.ID = "BodySignMemoId";
            signBody.Attributes.Add("html", "1"); // Affichage en mode HTML
            signBody.Attributes.Add("inlinemode", "0"); // Pas d'affichage en mode inline
            // type de champ Mémo 
            signBody.Attributes.Add("editortype", _sEditorType);
            signBody.Attributes.Add("toolbartype", _sToolbarType);
            signBody.Attributes.Add("enabletemplateeditor", _bEnableTemplateEditor ? "1" : "0"); // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
            signBody.Style.Add("margin-bottom", "10px"); //BVI espace entre CKEditor et l'option ajouter automatiquement la signature

            // Ajout du code nécessaire pour pouvoir instancier le champ memo coté JS avec initMemoFields
            this.MemoIds.Add(signBody.ID);
            HtmlInputHidden memoDescIds = new HtmlInputHidden();
            memoDescIds.ID = String.Concat("memoIds_", _tab);
            memoDescIds.Value = String.Join(";", _sMemoIds.ToArray());
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _divHidden.ID = String.Concat("hv_", _tab);
            _divHidden.Controls.Add(memoDescIds);

            // Remplit stepBody par la valeur du corps de la signature
            GetHTMLMemoControl(ednSignBody, _sMemoContent);

            // Ajout du conteneur du champ Mémo à son parent
            pnlMainPart.Controls.Add(signBody);
            #endregion

            pnl.Controls.Add(pnlMainPart);
        }


        /// <summary>
        /// Ajout de champ complémentaire
        /// </summary>
        /// <param name="p"></param>
        protected virtual void BuildMainCplt(Panel p)
        {

        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {


            #region Affichage

            Panel pnlContents = new Panel();
            pnlContents.ID = "admntCntnt";
            pnlContents.CssClass = "adminCntnt";


            BuildTitle(_pgContainer);

            BuildSubTitle(pnlContents);

            BuildMemoField(pnlContents);

            BuildMainCplt(pnlContents);

            BuildButton(pnlContents);




            #region Champs cachés
            // On ajoute un div fantôme "fileDiv" avec un attribut permettant à getCurrentView() de savoir sur quel type de page on se trouve
            // On considère l'admin comme un affichage de type Fiche en Modification
            HtmlGenericControl divFakeFileDiv = new HtmlGenericControl("div");
            divFakeFileDiv.Style.Add("visibility", "hidden");
            divFakeFileDiv.Style.Add("display", "none");
            divFakeFileDiv.ID = String.Concat("fileDiv_-1");
            divFakeFileDiv.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());
            #endregion




            // Ajout du contenu à la page
            _pgContainer.Controls.Add(_divHidden);
            _pgContainer.Controls.Add(divFakeFileDiv);
            _pgContainer.Controls.Add(pnlContents);

            #endregion

            return true;
        }
    }


    /// <summary>
    /// renderer de modification d'un champ mémo depuis l'administration des users
    /// réserver à l'admin - ouverture en popup
    /// </summary>
    public class eAdminUserOptAdminMemoRenderer : eAdminUsrOptMemoRenderer
    {
        public eAdminUserOptAdminMemoRenderer(ePref pref, int nUserId) : base(pref, nUserId)
        {
            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
            {
                throw new EudoAdminInvalidRightException();
            }
        }


        /// <summary>
        /// Boutons gérés en js
        /// </summary>
        /// <param name="p"></param>
        protected override void BuildButton(Panel p)
        {
        }

        //Pas de titre princiaple
        protected override void BuildTitle(Panel p)
        {
        }

        protected override void BuildSubTitle(Panel pnl)
        {

        }
    }

}