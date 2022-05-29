using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Text;
using Com.Eudonet.Engine;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    public class eUpdateGeoInfos
    {
        Int32 _nTab = 0;
        ePref _pref;
        Dictionary<Int32, String> _dicoMapping;
        eDataFillerGeneric _dtfFilesToUpdate;

        public eUpdateGeoInfos(ePref pref, Int32 nTab)
        {
            _nTab = nTab;
            _pref = pref;
        }


        public Boolean LaunchUpdate(out String sError, out String sMsgInfos)
        {
            sError = "";
            sMsgInfos = "";

            if (!GetMapping(out sError))
                return false;

            if (!GetFilesWithoutGeoInfos(out sError))
                return false;

            if (_dtfFilesToUpdate.ListRecords.Count == 0)
            {
                //TODO : RES
                sMsgInfos = "Aucune fiche à mettre à jour.";
                return true;
            }

            if (!UpdateViaEngine(out sError))
                return false;

            if (sError.Length > 0)
                sMsgInfos = sError;

            return true;
        }

        private Boolean GetMapping(out String sError)
        {
            sError = "";
            _dicoMapping = AccessData.GetMappingFromFileMapPartners(_pref, _nTab, out sError);

            return sError.Length == 0;
        }

        private Boolean GetFilesWithoutGeoInfos(out String sError)
        {
            sError = "";
            StringBuilder sqlfieldsAdr = new StringBuilder();
            sqlfieldsAdr.Append(_nTab + 1);
            for (int i = 2; i < 8; i++)
            {
                if (_dicoMapping[i].Length == 0)
                    continue;

                if (sqlfieldsAdr.Length > 0)
                    sqlfieldsAdr.Append(";");

                sqlfieldsAdr.Append(_dicoMapping[i]);
            }

            _dtfFilesToUpdate = new eDataFillerGeneric(_pref, _nTab, EudoQuery.ViewQuery.CUSTOM);
            _dtfFilesToUpdate.EudoqueryComplementaryOptions =
               delegate (EudoQuery.EudoQuery query)
               {
                   List<WhereCustom> liWc = new List<WhereCustom>();
                   liWc.Add(new WhereCustom(_dicoMapping[8], Operator.OP_IS_EMPTY, "", InterOperator.OP_AND));
                   liWc.Add(new WhereCustom(_dicoMapping[9], Operator.OP_IS_EMPTY, "", InterOperator.OP_AND));
                   liWc.Add(new WhereCustom(_dicoMapping[6], Operator.OP_IS_NOT_EMPTY, "", InterOperator.OP_AND));
                   query.AddCustomFilter(new WhereCustom(liWc, InterOperator.OP_AND));

                   query.SetListCol = sqlfieldsAdr.ToString();
               };

            _dtfFilesToUpdate.Generate();

            if (_dtfFilesToUpdate.ErrorMsg.Length > 0)
            {
                sError = _dtfFilesToUpdate.ErrorMsg;
                return false;
            }

            return true;
        }

        private Boolean UpdateViaEngine(out String sError)
        {
            sError = "";
            StringBuilder sbError = new StringBuilder();
            Boolean bUpdate = false;
            foreach (eRecord rec in _dtfFilesToUpdate.ListRecords)
            {
                #region extraction des données constituant l'adresse
                String sMainFileName = "";
                try
                {
                    sMainFileName = rec.GetFieldByAlias(String.Concat(_nTab, "_", (_nTab + 1))).DisplayValue;
                }
                catch (Exception e)
                {
                }

                String sPays = "";
                try
                {
                    sPays = _dicoMapping[7].Length == 0 ? "France" : rec.GetFieldByAlias(String.Concat(_nTab, "_", _dicoMapping[7])).DisplayValue;
                }
                catch (Exception e)
                {
                    sPays = "France";
                }

                String sCP = "";
                try
                {
                    sCP = rec.GetFieldByAlias(String.Concat(_nTab, "_", _dicoMapping[5])).DisplayValue;
                }
                catch (Exception e)
                {
                }

                String sVille = "";
                try
                {
                    sVille = rec.GetFieldByAlias(String.Concat(_nTab, "_", _dicoMapping[6])).DisplayValue;
                }
                catch (Exception e)
                {
                }

                String sAdr = "";
                try
                {
                    sAdr = String.Concat(_dicoMapping[2].Length > 0 ? rec.GetFieldByAlias(String.Concat(_nTab, "_", _dicoMapping[2])).DisplayValue : ""
                               , _dicoMapping[3].Length > 0 ? rec.GetFieldByAlias(String.Concat(_nTab, "_", _dicoMapping[3])).DisplayValue : ""
                               , _dicoMapping[4].Length > 0 ? rec.GetFieldByAlias(String.Concat(_nTab, "_", _dicoMapping[4])).DisplayValue : ""
                               );
                }
                catch (Exception e)
                {
                }
                #endregion

                #region récupération des coordonnées via bingmap
                LocationData location = new LocationData(sPays, sCP, sVille, sAdr);

                BingMapsSearch search = new BingMapsSearch(location);
                GeocodeInfo result = search.FindLocationByAddress();
                if (search.Error.Length == 0 && result == null)
                {
                    location = new LocationData(sPays, sCP, sVille, "");

                    search.SetLocation(location);
                    result = search.FindLocationByAddress();
                }

                if (search.Error.Length > 0 || result == null)
                {
                    sbError.Append("Une Erreur est survenue lors de la recherche de coordonnées de la fiche ").Append(sMainFileName).AppendLine(": ")
                        .Append(@"\t").AppendLine(search.Error.Length > 0 ? search.Error : "Aucun résultat ne correspond à la recherche");
                    continue;
                }

                #endregion

                #region Mise à jour via Engine

                Engine.Engine engine = eModelTools.GetEngine(_pref, _nTab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                engine.FileId = rec.MainFileid;

                // Champs à mettre à jour
                Int32 iDescidLat = 0, iDescidLong = 0, iGeoloc = 0;
                Int32.TryParse(_dicoMapping[8], out iDescidLat);
                Int32.TryParse(_dicoMapping[9], out iDescidLong);
                Int32.TryParse(_dicoMapping[10], out iGeoloc);
                engine.AddNewValue(iDescidLat, result.Latitude);
                engine.AddNewValue(iDescidLong, result.Longitude);
                if (iGeoloc > 0)
                    engine.AddNewValue(iGeoloc, String.Concat("POINT(", result.Longitude, " ", result.Latitude, ")"));

                engine.EngineProcess(new StrategyCruSimple());

                Engine.Result.EngineResult engineR = engine.Result;
                Boolean bSuccess = engineR.Success;
                if (!engineR.Success)
                {
                    //TODO : gérer l'erreur pour envoyer un message userfriendly au json et envoyer un feedback
                    sbError.Append("Erreur lors de l'enregistrement des coordonnées de la fiche ").Append(sMainFileName).AppendLine(" : ");
                    if (engine.Result.Error != null)
                    {
                        sbError.AppendLine((!String.IsNullOrEmpty(engine.Result.Error.Msg)) ? engine.Result.Error.Msg : engine.Result.Error.DebugMsg);
                    }

                    continue;
                }

                #endregion

                //indique qu'au moins une ligne a été mise à jour
                if (!bUpdate)
                    bUpdate = true;
            }

            sError = sbError.ToString();
            return bUpdate;
        }
    }
}