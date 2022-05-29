using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.list
{

    /// <summary>
    /// renderer pour les eudopart de type liste
    /// </summary>
    public class eHomePageListRenderer : eListMainRenderer
    {
        private Int32 _nBoxOrder = 0;
        private Int32 _tableRemainingWidth = 0;

        /// <summary>Constructeur de l'extrème</summary>
        /// <param name="dal">Objet de connexion à la BDD</param>
        /// <param name="pref">Preferences</param>
        /// <param name="nTargetTab">Table sur laquelle on recherche</param>
        /// <param name="nDispValue"></param>
        /// <param name="nEudoPartWidth">Largeur de l'eudopart</param>
        /// <param name="nEudoPartHeight">Hauteur de l'eudopart</param>
        /// <param name="nBoxOrder">Position de l'eudopart</param>
        public eHomePageListRenderer(eudoDAL dal, ePref pref, Int32 nTargetTab, List<Int32> nDispValue, Int32 nEudoPartWidth, Int32 nEudoPartHeight, Int32 nBoxOrder)
            : base(pref)
        {
            _rType = RENDERERTYPE.Finder;
            _tab = nTargetTab;
            _nBoxOrder = nBoxOrder;

            eHomePageList hpgLst = new eHomePageList(dal, pref, nTargetTab, nDispValue);
            try
            {
                if (!string.IsNullOrEmpty(hpgLst.ErrorMsg))
                {
                    _sErrorMsg = hpgLst.ErrorMsg;
                    return;
                }

                _list = eList.CreateHomePageList(hpgLst);
                if (!string.IsNullOrEmpty(hpgLst.ErrorMsg))
                {
                    _sErrorMsg = hpgLst.ErrorMsg;
                    return;
                }
                _width = nEudoPartWidth;
                _height = nEudoPartHeight;
            }
            finally
            {
                hpgLst = null;
            }
        }

        /// <summary>
        /// Construction des en-tête de colonnes
        /// </summary>
        protected override void Head()
        {
            // Taille calculé automatiquement en fonction de la taille fenêtre pour repartir la taille sur chaque colonne
            _tableRemainingWidth = _width;
            _tableRemainingWidth = _tableRemainingWidth / _nbDisplayCol;

            base.Head();
        }

        /// <summary>
        /// Définit la taille par défaut d'une colonne
        /// </summary>
        /// <param name="libelleMaxLen">taillé max du libellé (avec filtre et tri)</param>
        /// <param name="field">champ à redimensionner</param>
        protected override void InitFieldWidth(string libelleMaxLen, Field field)
        {
            field.Width = _tableRemainingWidth;
        }

        /// <summary>
        /// Génère l'objet _list du renderer
        /// TODO déplace la construction de la liste du constructeur vers ici
        /// </summary>
        /// <returns></returns>
        protected override void GenerateList()
        {
            //
        }


        /// <summary>
        /// rajoute dans la ligne d'en-tête la colonne contenant les icones pour les annexes, taches periodiques...
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void HeaderListIcon(TableRow headerRow)
        {
            return;
        }

        protected int lastValue = 0;

        protected string lastCss = string.Empty;
        /// <summary>
        /// présente les icones pour les annexes, taches periodiques...
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="idxLine"></param>
        /// <param name="sLstRulesCss"></param>
        /// <param name="lIcon"></param>
        override protected void BodyListIcon(eRecord row, TableRow trDataRow, Int32 idxLine, ref string sLstRulesCss, List<string> lIcon)
        {
            if (lastValue != row.MainFileid)
            {
                if (lastCss == "hpgline1")
                    trDataRow.CssClass = "hpgline2";
                else
                    trDataRow.CssClass = "hpgline1";
            }
            else
                trDataRow.CssClass = lastCss;
            lastCss = trDataRow.CssClass;
            lastValue = row.MainFileid;
            trDataRow.ID = string.Concat("row_", _nBoxOrder, "_", idxLine);
        }

        /// <summary>
        /// Pas d'alternance de couleur par ligne
        /// </summary>
        /// <param name="trDataRow"></param>
        /// <param name="idxLine"></param>
        protected override void cssLine(TableRow trDataRow, int idxLine)
        {
        }

        /// <summary>
        /// Traitement de fin de génération
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            base.End();

            _divmt.Style.Add(HtmlTextWriterStyle.Height, string.Concat(_height, "px"));
            //string sSpaceAction = string.Empty;

            #region resize auto de colonnes


            _lenList.Attributes.Add("border", "1px");
            _lenList.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            _lenList.Style.Add(HtmlTextWriterStyle.Left, "-50000px");
            _lenList.CssClass = "mTab mTabLen";

            _lenHeadList.Attributes.Add("border", "1px");
            _lenHeadList.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            _lenHeadList.Style.Add(HtmlTextWriterStyle.Left, "-50000px");
            _lenHeadList.CssClass = "mTab mTabHeadLen";

            TableRow rowLen = new TableRow();
            _lenList.Rows.Add(rowLen);
            TableRow rowHead = new TableRow();
            _lenHeadList.Rows.Add(rowHead);


            TableCell cellLen = null;
            TableCell cellHead = null;
            foreach (KeyValuePair<String, ListColMaxValues> keyValue in _colMaxValues)
            {
                cellLen = new TableCell();
                cellLen.ID = string.Concat("LEN_", keyValue.Key);
                cellLen.Text = keyValue.Value.ColMaxValue;
                cellLen.Attributes.Add("nowrap", string.Empty);
                cellLen.CssClass = keyValue.Value.AdditionalCss;
                rowLen.Cells.Add(cellLen);

                cellHead = new TableCell();
                cellHead.ID = string.Concat("LENH_" + keyValue.Key);
                cellHead.Text = keyValue.Value.HeadValue;
                cellHead.Attributes.Add("nowrap", string.Empty);
                rowHead.Cells.Add(cellHead);
            }

            #endregion

            /*
            //Ajustement taille entête
            TableCell lenLibCell = _lenHeadList.Rows[0].Cells[0];
            lenLibCell.Text = lenLibCell.Text + sSpaceAction;


            TableCell lenColCell = _lenList.Rows[0].Cells[0];
            lenColCell.Text = lenColCell.Text + sSpaceAction;
             * */
            return true;
        }

    }
}