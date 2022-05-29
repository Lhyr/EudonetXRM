using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.renderer
{
    /// <summary>
    /// Objet permettant de faire un rendu de l'assitant d'import cible etendu
    /// </summary>
    public class eTargetImportRenderer : eRenderer
    {
        /// <summary>
        /// Table de cible Etendu
        /// </summary>
        private Int32 _tabFrom = 0;

        /// <summary>
        /// Evenement de rattachement
        /// </summary>
        private Int32 _evtId = 0;

        /// <summary>
        /// Objet metier de l'import
        /// </summary>
        private eTargetImport _eTargetImport = null;

        /// <summary>
        /// Contsructeur du renderer
        /// </summary>
        /// <param name="_pref">Préference</param>
        /// <param name="nTab">Table principale</param>
        /// <param name="nTabFrom">Table d'ou vient</param>
        /// <param name="nEvtId">evenment auqel seront ratachées les cibles etendues </param>
        /// <param name="nWidth">Largeur de l afenetre</param>
        /// <param name="nHeight">Hauteur de la fenetre</param>
        public eTargetImportRenderer(ePref pref, int nTab, int nTabFrom, int nEvtId, int nWidth, int nHeight)
        {
            this.Pref = pref;
            this._tab = nTab;
            this._tabFrom = nTabFrom;
            this._evtId = nEvtId;
            this._width = nWidth;
            this._height = nHeight;
        }

        /// <summary>
        /// Initialise un eTargetRenderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {
                this._eTargetImport = new eTargetImport(this.Pref);
                this._eTargetImport.LoadTargetInfos(this._tab, this._tabFrom, this._evtId);
            }
            catch (Exception ex)
            {
                _eException = ex;
                _sErrorMsg = String.Concat("eTargetImportRenderer::Init :", ex.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// construit l'iframe
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (_eTargetImport == null)
                return false;

            int idx = 0;
            foreach (KeyValuePair<Int32, String> kv in _eTargetImport.FieldsTargetInfos)
            {
                HtmlGenericControl divColGlobal = new HtmlGenericControl("div");
                divColGlobal.Attributes.Add("class", "fieldItem");
                /*
                HtmlGenericControl divOpacityField = new HtmlGenericControl("div");
                divOpacityField.Attributes.Add("class", "opacityField"); */

                HtmlGenericControl divDatas = new HtmlGenericControl("div");
                divDatas.Attributes.Add("class", "subFieldHead1");
                divDatas.InnerHtml = String.Concat("<span>", kv.Value, "</span>");

                #region props

                divDatas.Attributes.Add("ondblclick", "KeyChange(this.id);");
                divDatas.Attributes.Add("ednkey", "0");
                divDatas.Attributes.Add("title", eResApp.GetRes(Pref, 1669));// "Double cliquez pour affecter une clé de dédoublonnage");
                divDatas.ID = String.Concat("tl_", idx);

                #endregion

                //divColGlobal.Controls.Add(divOpacityField);
                divColGlobal.Controls.Add(divDatas);

                HtmlGenericControl divBottom = new HtmlGenericControl("div");
                divBottom.Attributes.Add("class", "subFieldBottom");
                divBottom.InnerHtml = "&nbsp;";
                divColGlobal.Controls.Add(divBottom);

                //Props div bottom
                divBottom.Attributes.Add("ondblclick", string.Concat("MapOff('", "t_", idx, "');"));
                divBottom.Attributes.Add("onmouseup", string.Concat("MUP('", "t_", idx, "');"));
                divBottom.Attributes.Add("onmousedown", string.Concat("MDN('", "t_", idx, "');"));
                divBottom.Attributes.Add("onmouseover", string.Concat("MOV('", "t_", idx, "');"));
                divBottom.Attributes.Add("onmouseout", string.Concat("MOT('", "t_", idx, "');"));
                divBottom.Attributes.Add("ednorig", "Target");
                divBottom.Attributes.Add("edndescid", kv.Key.ToString());
                divBottom.ID = string.Concat("t_", idx);

                divColGlobal.Attributes.Add("onmouseup", string.Concat("MUP('", "t_", idx, "');"));
                divBottom.Attributes.Add("onmouseup", string.Concat("MUP('", "t_", idx, "');"));
                divDatas.Attributes.Add("onmouseup", string.Concat("MUP('", "t_", idx, "');"));

                idx++;

                _pgContainer.Controls.Add(divColGlobal);
            }

            return true;
        }

        /// <summary>
        /// construit l'iframe de la spécif
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return true;
        }
    }
}
