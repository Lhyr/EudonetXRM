using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.renderer
{
    /// <summary>
    /// classe de rendu du mode impression d'un email
    /// </summary>
    public class ePrintMailFileRenderer : eMailFileRenderer
    {
        private ePrintParams _nParamsOfPrint = null;

        /// <summary>
        /// Constructeur du renderer
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nTab">The tab.</param>
        /// <param name="nFileId">The file identifier.</param>
        /// <param name="nPrintParams">paramètres d'impression</param>
        /// </summary>
        public ePrintMailFileRenderer(ePref pref, int nTab, int nFileId, ePrintParams nPrintParams) : base(pref, nTab, nFileId, 0, 0, "", false)
        {
            _nParamsOfPrint = nPrintParams;
            _rType = RENDERERTYPE.PrintFile;
        }

        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// Surchage de la classe héritée
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            try
            {
                //Génération d'un objet "métier" de type file
                // TOCHECK SMS
                _myFile = eFileMail.CreateEditMailFile(Pref, _tab, _nFileId, -2);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("ePrintMailFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
                    if (_myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("ePrintMailFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }

        /// <summary>
        /// Ajout du pied
        /// </summary>
        protected override void AddParentInFoot()
        {
            // ajout du pied de page contenant les informations parentes en popup
            eRenderer footRenderer = eRendererFactory.CreateParenttInFootRenderer(Pref, this);
            Panel pgC = null;
            if (footRenderer.ErrorMsg.Length > 0)
                this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
            if (footRenderer != null)
                pgC = footRenderer.PgContainer;
            _backBoneRdr.PgContainer.Controls.Add(footRenderer.PgContainer);
        }

        /// <summary>
        /// Rendu du champ Historique/ et Confirmation de lecture 
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override Boolean RenderFldHistoTrack(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldHisto == null && _fldTracking == null)
                return false;

            // Historisé et confirmation de lecture
            // Ces deux champs devant être affichés côte-à-côte, on utilise les méthodes usuelles pour générer le rendu, puis on récupère les contrôles case à cocher
            // pour les ajouter dans une ligne séparée
            if (_fldHisto.RightIsVisible || (_fldTracking != null && _fldTracking.RightIsVisible))
            {
                List<TableCell> trHisto = RenderField(_fldHisto, String.Empty);
                List<TableCell> trTracking = new List<TableCell>();
                if (_fldTracking != null)
                    trTracking = RenderField(_fldTracking, String.Empty);

                List<TableCell> trRead = new List<TableCell>();
                if (_fldRead != null)
                    trRead = RenderField(_fldRead, String.Empty);


                TableCell tc = new TableCell();
                tc.ColumnSpan = 2;
                //HtmlGenericControl label;
                HtmlGenericControl value;

                if (_fldHisto.RightIsVisible)
                {
                    value = new HtmlGenericControl();
                    value.InnerText = (_fldHisto.DisplayValue == "1") ? eResApp.GetRes(Pref, 58) : eResApp.GetRes(Pref, 59);

                    tc = new TableCell();
                    tc.CssClass = "table_labels";
                    tc.Text = _fldHisto.FldInfo.Libelle;
                    tr.Controls.Add(tc);

                    tc = new TableCell();
                    tc.Controls.Add(value);

                    tr.Controls.Add(tc);
                }

                if (_fldTracking != null && _fldTracking.RightIsVisible)
                {
                    value = new HtmlGenericControl();
                    value.InnerText = (_fldTracking.DisplayValue == "1") ? eResApp.GetRes(Pref, 58) : eResApp.GetRes(Pref, 59);

                    tc = new TableCell();
                    tc.CssClass = "table_labels";
                    tc.Text = _fldTracking.FldInfo.Libelle;
                    tr.Controls.Add(tc);

                    tc = new TableCell();
                    tc.Controls.Add(value);

                    tr.Controls.Add(tc);
                }

                String mailStatusAlias = String.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_STATUS.GetHashCode());
                Int32 iStatus;
                Int32.TryParse(_myFile.Record.GetFieldByAlias(mailStatusAlias).Value, out iStatus);

                if (iStatus == (int)EmailStatus.MAIL_SENT && _fldRead != null && _fldRead.RightIsVisible)
                {
                    value = new HtmlGenericControl();
                    value.InnerText = (_fldRead.DisplayValue == "1") ? eResApp.GetRes(Pref, 58) : eResApp.GetRes(Pref, 59);

                    tc = new TableCell();
                    tc.CssClass = "table_labels";
                    tc.Text = _fldRead.FldInfo.Libelle;
                    tr.Controls.Add(tc);

                    tc = new TableCell();
                    tc.Controls.Add(value);

                    tr.Controls.Add(tc);
                }



                return false;
            }

            return true;
        }

        /// <summary>
        /// Rendu du corps de mail 
        /// </summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected override Boolean RenderFldBody(System.Web.UI.WebControls.Table tab, bool bCkEditor)
        {
            if (_fldBody == null || !_fldBody.RightIsVisible)
                return false;

            TableRow myTr = new TableRow();

            _fldBody.FldInfo.PosRowSpan = 1;

            TableCell myLabel = new TableCell();
            TableCell myValue = new TableCell();

            myLabel.Text = _fldBody.FldInfo.Libelle;

            //Génération du rendu HTML pour chaque champ à afficher
            //C'est dans cette méthode du eRenderer générique que sont gérées les exceptions visuelles à appliquer sur certains champs d'E-mail
            //ex : combobox pour le champ De (From)
            TableCell myValueCell = new TableCell();

            myValueCell.RowSpan = 1;
            myValueCell.ColumnSpan = 1;

            HtmlGenericControl panelBody = new HtmlGenericControl();
            panelBody.InnerHtml = _fldBody.Value;
            myValueCell.Controls.Add(panelBody);

            myTr.Cells.Add(myLabel);
            myTr.Cells.Add(new TableCell());
            myTr.Style.Add("display", "none");

            TableRow myTr2 = new TableRow();
            myValueCell.ColumnSpan = 6; // TOCHECK MAB après maquette QBO
            myTr2.Cells.Add(myValueCell);

            tab.Controls.Add(myTr);
            tab.Controls.Add(myTr2);

            return true;
        }
    }
}