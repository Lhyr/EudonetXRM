using System;
using System.Collections.Generic;
using System.Data;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Liste retournant uniquement les id des enreigstrement principaux de la liste
    /// </summary>

    public class eAdminListConditions : eListMain
    {


        List<eRules> _lstRules = new List<eRules>();

        ///<summary>
        /// Constructeur standard
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nTab"></param>
        /// <param name="nRows">Nombre de lignes par page</param>
        /// <param name="nPage">Numéro de page à afficher</param>
        private eAdminListConditions(ePref ePref, Int32 nTab, List<eRules> lst)
            : base(ePref, nTab, 0, 1)
        {
            _lstRules = lst;
        }


        #region override
        protected override bool Build()
        {
            _lstRecords = new List<eRecord>();
            foreach (eRules currRules in _lstRules)
            {


                eRecord row = new eRecord();
                _lstRecords.Add(row);

                row.MainFileid = (int)currRules.Type;
                row.ViewTab = _nCalledTab;
                row.CalledTab = _nCalledTab;

                eFieldRecord er = new eFieldRecord();
                er.FldInfo = new Field();
                er.FldInfo.Libelle = "Comportement";
                er.Value = currRules.AllRules.Count > 0 ? "Règle existe" : "Comportement par défaut";
                row.AddField(er);


                eFieldRecord erComp = new eFieldRecord();
                erComp.Value = currRules.Type.ToString();
                erComp.FldInfo = new Field();
                erComp.FldInfo.Libelle = "Comportement";
                row.AddField(erComp);


                eFieldRecord erTab = new eFieldRecord();
                erTab.Value = currRules.Type.ToString();
                erTab.FldInfo = new Field();
                erTab.FldInfo.Libelle = currRules.DescId.ToString();
                row.AddField(erTab);

            }

            return true;
        }


        protected override bool ContinueToFill(DataTableReaderTuned dtr, int nIdx)
        {
            return true;
        }


        protected override bool End()
        {
            return true;
        }



        protected override void EqParamBeforeLoadRequest()
        {

        }


        protected override bool Fill()
        {
            return true;
        }



        protected override bool GetDataReader()
        {
            return true;
        }


        protected override RqParam GetRqParam(string sqlQuery)
        {
            return new RqParam();
        }


        protected override bool Init()
        {
            // Informations génériques

            Table mainTable = new Table();
         
            Field fldRulesLabel = new Field();
 
            return true;
        }


        protected override bool IsFieldDrawable(Field fld)
        {
            return true;
        }

        protected override bool IsFieldFiltered(Field field, string value = null)
        {
            return false;
        }

        protected override bool IsRowVisible(DataTableReaderTuned dtr)
        {
            return true;
        }


        protected override void LoadAllFieldInfo(Field fld)
        {

        }


        protected override void LoadDrawFieldInfo(Field fld)
        {

        }


        protected override void LoadEndInfo()
        {

        }



        protected override void LoadPagingInfo()
        {

        }



        protected override void LoadSpeListPostEndInfo()
        {

        }


        protected override void LoadSpeListPreEndInfo()
        {

        }


        protected override void PostFieldTreatment(eFieldRecord fldRec)
        {

        }


        protected override void PostRowInfo(eRecord row, DataTableReaderTuned dtr)
        {

        }


        /// <summary>
        /// traitement post ligne
        /// </summary>
        /// <param name="row">lignbe en cours</param>
        protected override void PostRowTreatment(eRecord row)
        {
            //pas de traitement dans ce cas
        }


        /// <summary>
        /// Traitement préalalble
        /// </summary>
        /// <param name="dtr">dtr de la liste</param>
        protected override void PreTreatment(DataTableReaderTuned dtr)
        {
            //on ne fait rien
        }
        #endregion
    }
}