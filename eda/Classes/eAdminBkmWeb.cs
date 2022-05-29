using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminBkmWeb
    {
        private ePref _pref = null;
        private Int32 _iBkm = 0;
        private String _sDefault = "";
        private Int32 _iSpecifId = 0;

        /// <summary>Url du signet Web </summary>
        public String Url { get { return IsSpecif ? Spec.Url : _sDefault; } }

        /// <summary>Paramètre additionnel (specif only) </summary>
        public String UrlParam { get { return Spec.UrlParam; } }

        /// <summary>Lien d'administration de la spécif (specif only)</summary>
        public String UrlAdmin { get { return Spec.UrlAdmin; } }

        /// <summary>Indique si il s'agit d'une spécif ou d'un lien externe</summary>
        public Boolean IsSpecif { get { return _iSpecifId > 0; } }

        /// <summary>specif liée au signet web </summary>
        public eSpecif Spec { get; protected set; }

        /// <summary>Libellés du signet dans les différentes langues disponibles</summary>
        public Dictionary<int, string> Labels;

        /// <summary>Liaison avec PP</summary>
        public Boolean InterPP { get; protected set; }

        /// <summary>Liaison avec PM</summary>
        public Boolean InterPM { get; protected set; }

        /// <summary>Liaison avec un event</summary>
        public Int32 InterEvtDescId { get; protected set; }

        /// <summary>Droit de visu</summary>
        public ePermission ViewPermission { get; protected set; }

        /// <summary>Droit de Mise à jour</summary>
        public ePermission UpdatePermission { get; protected set; }

        /// <summary>Erreur</summary>
        public String Error { get; protected set; }

        public eAdminBkmWeb(ePref p, Int32 bkm)
        {
            _pref = p;
            _iBkm = bkm;
        }


        private void getDescInfos(eudoDAL dal)
        {
            string sError = "";
            DataTableReaderTuned dtr = eSqlDesc.GetBkmWebDTR(dal, _iBkm, out sError);
            if (sError.Length > 0)
            {
                Error = sError;
                return;
            }
            if (!dtr.Read())
            {
                Error = String.Concat("Aucun Enregistrement dans [DESC] ne correspond à ce signet (", _iBkm, ")");
                return;
            }

            _sDefault = dtr.GetString("Default");
            InterPP = dtr.GetBoolean("InterPP");
            InterPM = dtr.GetBoolean("InterPM");
            if (dtr.GetBoolean("InterEvent"))
            {
                Int32 iInterEvtNum = dtr.GetInt32UnSafe("InterEventNum");
                if (iInterEvtNum == 0)
                {
                    InterEvtDescId = 100;
                }
                else
                {
                    InterEvtDescId = (iInterEvtNum + 10) * 100;
                }
            }
            else
            {
                InterEvtDescId = 0;
            }

            UpdatePermission = new ePermission(dtr.GetInt32UnSafe("UpdatePermId"),
                                    (ePermission.PermissionMode)dtr.GetInt32UnSafe("UpdatePermMode"),
                                    dtr.GetInt32UnSafe("UpdatePermLevel"),
                                    dtr.GetString("UpdatePermUser"));

            ViewPermission = new ePermission(dtr.GetInt32UnSafe("ViewPermId"),
                                                (ePermission.PermissionMode)dtr.GetInt32UnSafe("ViewPermMode"),
                                                dtr.GetInt32UnSafe("ViewPermLevel"),
                                                dtr.GetString("ViewPermUser"));


        }
        private void getLabels(eudoDAL dal)
        {
            String sError = "";
            Labels = eSqlRes.GetLabels(dal, _iBkm, out  sError);
        }

        private void getSpecif()
        {
            Int32.TryParse(_sDefault, out _iSpecifId);

            if (_iSpecifId == 0)
                return;
            Spec = eSpecif.GetSpecif(_pref, _iSpecifId);
        }

        public void Generate()
        {
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            try
            {
                dal.OpenDatabase();

                getDescInfos(dal);
                getLabels(dal);
                getSpecif();
            }
            catch (Exception e) { }
            finally
            {
                dal.CloseDatabase();
            }
        }


    }
}