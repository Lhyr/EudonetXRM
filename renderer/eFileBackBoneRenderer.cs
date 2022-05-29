using EudoQuery;
using System;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe organisant les élément HTML de base du mode fiche
    /// </summary>
    public class eFileBackBoneRenderer : eRenderer
    {
        /// <summary>indique si on affiche le block des bkm (en popup sauf création de contact on ne les affiche pas)</summary>
        private Boolean _bDisplayBookmarkBlock = true;
        private Boolean _bPopup = false;
        private bool _isBkmFile = false;
        private Panel _pnFilePart1;
        private Panel _pnFilePart2;
        private Panel _pnBkmBar;
        private Panel _pnBkmContainer;
        private Panel _pnlDetailsBkms;
        private Panel _pnlBkmFileProps;

        /// <summary>Div englobant la partie haute de la fiche</summary>
        public Panel PnFilePart1
        {
            get { return _pnFilePart1; }
        }

        /// <summary>Div englobant les rubriques exportées dans les signets</summary>
        public Panel PnFilePart2
        {
            get { return _pnFilePart2; }
        }


        /// <summary>Barre des signets</summary>
        public Panel PnBkmBar
        {
            get { return _pnBkmBar; }
        }


        /// <summary>Div renfermant l'ensemble des signets sauf les rubrique fiches exportées dans les signets</summary>
        public Panel PnBkmContainer
        {
            get { return _pnBkmContainer; }
        }

        /// <summary>panel renfermant et la fiche et les signets mais dans le cas du popup exclut l'en-tête</summary>
        public Panel PnlDetailsBkms
        {
            get { return _pnlDetailsBkms; }
        }
        /// <summary>
        /// Panel contenant le lien vers les propriétés de la fiche (dans le cas du signet mode fiche)
        /// </summary>
        public Panel PnlBkmFileProps
        {
            get
            {
                return _pnlBkmFileProps;
            }
        }



        /// <summary>
        /// constructeur pour le sqelette HTML de la fiche
        /// </summary>
        /// <param name="tab">Descid de la table</param>
        /// <param name="bPopup">Indique si on est en popup</param>
        /// <param name="bDisplayBookmarkBlock">Indique si on affiche le bloc de bkms</param>
        /// <param name="isBkmFile">Indique si on est en signet mode fiche</param>
        public eFileBackBoneRenderer(Int32 tab, Boolean bPopup = false, Boolean bDisplayBookmarkBlock = true, bool isBkmFile = false)
        {
            this._tab = tab;
            this._bDisplayBookmarkBlock = bDisplayBookmarkBlock;
            this._bPopup = bPopup;
            _isBkmFile = isBkmFile;
            _rType = RENDERERTYPE.FileBackBone;
        }


        /// <summary>
        /// genère le masque HTML de la fiche
        /// </summary>
        /// <returns></returns>
        public Boolean GenerateBackBone()
        {
            this._pgContainer.ID = String.Concat("fileDiv_", this._tab);
            this._pgContainer.CssClass = "fileDiv";
            this._pgContainer.Attributes.Add("fid", "0");
            this._pgContainer.Attributes.Add("did", this._tab.ToString());


            this._pnFilePart1 = new Panel();
            this._pgContainer.Controls.Add(this._pnFilePart1);
            this._pnFilePart1.ID = "divFilePart1";

            this._pnFilePart2 = new Panel();
            this._pnFilePart2.ID = "FlDtlsBkm";
            this._pnFilePart2.CssClass = "FlDtlsBkm";

            if (this._bPopup)
            {
                //Conteneur du détails de fiche + bookmark uniquement pour l'affichage en popup      
                this._pnlDetailsBkms = new Panel();
                this._pnlDetailsBkms.CssClass = "divDetailsBkms";
                this._pnlDetailsBkms.ID = "divDetailsBkms";

                if (_isBkmFile)
                {
                    _pnlBkmFileProps = new Panel();
                    _pnlBkmFileProps.CssClass = "bkmFilePropsLink";
                    this._pnlDetailsBkms.Controls.Add(_pnlBkmFileProps);
                }

                this._pnlDetailsBkms.Controls.Add(this._pnFilePart1);

                this._pgContainer.Controls.Add(this._pnlDetailsBkms);
            }

            Panel pnBlockBkms = new Panel();
            pnBlockBkms.ID = "blockBkms";
            pnBlockBkms.CssClass = "blockBkms";
            if (this._tab == TableType.USER.GetHashCode())
                pnBlockBkms.Style.Add("display", "none");
            this._pgContainer.Controls.Add(pnBlockBkms);

            if (this._bPopup)
            {
                this._pnlDetailsBkms.Controls.Add(pnBlockBkms);
            }

            this._pnBkmBar = new Panel();
            this._pnBkmBar.ID = String.Concat("bkmBar_", _tab);
            this._pnBkmBar.CssClass = "bkmBar";
            pnBlockBkms.Controls.Add(this._pnBkmBar);

            Panel pnBkmPres = new Panel();
            pnBkmPres.ID = "divBkmPres";
            pnBkmPres.CssClass = "divBkmPres";
            pnBlockBkms.Controls.Add(pnBkmPres);

            //SHA : backlog 938
            if (!this._bPopup)
                pnBkmPres.Controls.Add(this._pnFilePart2);

            //SHA : correction bug #71 235
            if (!_bDisplayBookmarkBlock)
                return true;

            this._pnBkmContainer = new Panel();
            this._pnBkmContainer.ID = "divBkmCtner";
            this._pnBkmContainer.CssClass = "divBkmCtner";
            pnBkmPres.Controls.Add(this._pnBkmContainer);

            return true;
        }
    }
}