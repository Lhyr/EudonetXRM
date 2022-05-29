using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;
using EudoQuery;

namespace Com.Eudonet.Xrm.renderer
{
    public class eCtiListRender : eListMainRenderer
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
        public eCtiListRender(eudoDAL dal, ePref pref, Int32 nTargetTab, List<Int32> nDispValue, Int32 nEudoPartWidth, Int32 nEudoPartHeight, Int32 nBoxOrder)
            : base(pref)
        {
            _rType = RENDERERTYPE.Finder;
            _tab = nTargetTab;
            _nBoxOrder = nBoxOrder;

            eCtiList hpgLst = new eCtiList(dal, pref, nTargetTab, nDispValue);
                                 
            try
            {
                if (!string.IsNullOrEmpty(hpgLst.ErrorMsg))
                {
                    _sErrorMsg = hpgLst.ErrorMsg;
                    return;
                }

                //  _list = eList.CreateHomePageList(hpgLst);
                _list = hpgLst;

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




       
    }
}