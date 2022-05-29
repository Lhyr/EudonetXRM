using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu des bookmark
    /// </summary>
    public class eNotesBookmarkRenderer : eRenderer
    {
        eBookmark _bkm = null;
        String _value = String.Empty;
        Boolean _isHtml = false;
        Boolean _readOnly = false;
        Int32 _nbRows = 0;
        Boolean _isForPrint = false;

        #region CONSTRUCTEUR



        /// <summary>
        /// Création d'un BookmarkRenderer à partir d'un eBookmark chargé
        /// </summary>
        /// <param name="ePref">Préférence de l'utilisateur</param>
        /// <param name="eBkm">Objet bookmark dont il faut faire le rendu</param>
        public eNotesBookmarkRenderer(ePref ePref, eBookmark eBkm, Boolean isForPrint = false)
        {
            Pref = ePref;
            _tab = eBkm.CalledTabDescId - (eBkm.CalledTabDescId % 100); // TODO: à vérifier avec SPH/KHA
            _bkm = eBkm;
            _rType = RENDERERTYPE.NotesBookmark;
            _isForPrint = isForPrint;
            try
            {

                if (eBkm.ParentFile.Record == null)
                {
                    _sErrorMsg = "Pas de fiche";
                    return;
                }

                eFieldRecord field = eBkm.ParentFile.Record.GetFieldByAlias(String.Concat(eBkm.ParentFile.CalledTabDescId, "_", eBkm.CalledTabDescId));
                _value = field.Value;
                _isHtml = RenderMemoFieldIsHtml(field);
                _nbRows = field.FldInfo.PosRowSpan;
                _readOnly = field.FldInfo.ObligatReadOnly || field.FldInfo.ReadOnly || !field.RightIsUpdatable;
            }
            catch (Exception ex)
            {
                _eException = ex;
                _sErrorMsg = ex.Message;

            }
        }


        // TODO: Création d'un BookmarkRenderer à partir d'un descid ?

        #endregion

        #region ACCESSEURS

        /// <summary>Correspond au descid du champ Notes correspondant au signet</summary>
        public virtual Int32 VirtualMainTableDescId
        {

            get
            {
                return _tab;
            }
        }

        #endregion

        /// <summary>
        /// Construit l'objet et le remplit
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            try
            {
                _pgContainer.Controls.Add(eBookmarkRenderer.CreateTitleBar(Pref, _bkm));
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eBookmarkFileRenderer.Build()>eBookmarkRenderer.CreateTitleBar : ", Environment.NewLine,
                    "bkm: ", _bkm?.CalledTabDescId.ToString() ?? "null", Environment.NewLine,
                    e.Message, Environment.NewLine,
                    e.StackTrace, Environment.NewLine);
                _eException = e;
                return false;
            }

            FillContainer();

            Body();

            return true;

        }


        /// <summary>
        /// Surcharge du End() standard
        /// Dans ce cas, ne fait rien
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return true;
        }

        #region Méthodes interne à la génération de liste

        /// <summary>
        /// 
        /// </summary>
        protected void FillContainer()
        {
            Panel divmt = new Panel();
            divmt.ID = String.Concat("div", VirtualMainTableDescId);
            divmt.CssClass = "divmTab";
            _pgContainer.Controls.Add(divmt);

            String mainTableId = String.Concat("mt_", VirtualMainTableDescId);

            // Div de champ caché
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _divHidden.ID = String.Concat("hv_", VirtualMainTableDescId);

            //CSS ICON STANDARD
            // MAB - TODO
            String sCSSStdIcon = String.Empty;
            //sCSSStdIcon = String.Concat("background:url(themes/", Pref.Theme, "/images/iFileIcon/", _list.ViewMainTable.GetIcon, ") center center no-repeat  !important ");
            HtmlInputHidden inptDefIconCss = new HtmlInputHidden();
            inptDefIconCss.ID = "ICON_DEF_" + VirtualMainTableDescId;
            inptDefIconCss.Attributes.Add("etype", "css");
            inptDefIconCss.Attributes.Add("ecssname", String.Concat("iconDef_", _tab));
            inptDefIconCss.Attributes.Add("ecssclass", sCSSStdIcon);
            _divHidden.Controls.Add(inptDefIconCss);

            // MIN_COL_WIDTH - TODO - A SUP
            HtmlInputHidden inputMinColWidth = new HtmlInputHidden();
            inputMinColWidth.ID = "minColWidth";
            inputMinColWidth.Value = eConst.MIN_COL_WIDTH.ToString();
            _divHidden.Controls.Add(inputMinColWidth);

            //Ajout du div caché
            _pgContainer.Controls.Add(_divHidden);
        }

        /// <summary>
        /// Construction du contennu de la table de la liste
        /// </summary>
        protected virtual void Body()
        {
            if (!_isForPrint)
            {
                // Contrôles HTML : valeur des Notes (textarea) et div conteneur
                HtmlTextArea memoEditorValueControl = new HtmlTextArea();
                memoEditorValueControl.ID = String.Concat("eBkmMemoEditorValue_", _bkm.CalledTabDescId);
                memoEditorValueControl.Style.Add("display", "none");
                memoEditorValueControl.InnerText = _value; // #36751 CRU/SPH : On fait un InnerText à la place d'un InnerHTML pour ne pas décoder la valeur


                HtmlGenericControl memoEditorContainerControl = new HtmlGenericControl("div");
                memoEditorContainerControl.ID = String.Concat("eBkmMemoEditorContainer_", _bkm.CalledTabDescId);
                // Ajout des attributs pour le fonctionnement de eUpdater/eEngine depuis eMemoEditor.update()
                memoEditorContainerControl.Attributes.Add("ename", String.Concat("eBkmMemoEditorContainer_", _bkm.CalledTabDescId));
                memoEditorContainerControl.Attributes.Add("did", _bkm.CalledTabDescId.ToString());
                memoEditorContainerControl.Attributes.Add("fid", _bkm.ParentFileId.ToString());
                memoEditorContainerControl.Attributes.Add("html", (_isHtml ? "1" : "0"));
                memoEditorContainerControl.Attributes.Add("frominnertext", "1"); //en fonction du mode d'ouverture (popup, fiche...), l'init du ck editor est différente. 
                if (!_isHtml)
                    memoEditorContainerControl.Attributes.Add("nbrows", _nbRows.ToString()); // #37575 CRU : Ajout info nombre lignes paramétré
                if (_readOnly)
                {
                    memoEditorContainerControl.Attributes.Add("readonly", "readonly");
                    memoEditorContainerControl.Attributes.Add("ero", "1");
                }
                // Ajout des contrôles au conteneur de signet
                _pgContainer.Controls.Add(memoEditorValueControl);
                _pgContainer.Controls.Add(memoEditorContainerControl);
            }
            else
            {
                #region Impression
                Panel panel = new Panel();
                HtmlGenericControl p = new HtmlGenericControl("p");
                p.Attributes.Add("class", "memoText");
                if (_isHtml)
                {
                    //p.InnerHtml = _value;
                    // [BUG XRM] Impression fiche avec champ Mémo HTML - #49286
                    p.InnerText = HttpUtility.HtmlDecode(HtmlTools.StripHtml(_value));
                }
                else
                    p.InnerText = _value;
                panel.Controls.Add(p);

                _pgContainer.Controls.Add(panel);
                #endregion
            }

            _pgContainer.Attributes.Add("memobkm", "1");

            // L'exécution du JS instanciant eMemoEditor sera prise en charge par updateBkmList()
        }

        #endregion

    }
}