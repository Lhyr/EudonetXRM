using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public partial class eCartoMulti : eEudoPage
    {
        private Operation _operation = 0;
        private Int32 _nTab = 0;

        /// <summary>
        /// Objet métier
        /// </summary>
        private Geocodage infoForGeo = new Geocodage();
        /// <summary>
        /// Dico contenant les informations nécessaire à l'affichage d'un point sur une carte
        /// on boucle dessus pour créer des pushpin
        /// </summary>
        public Dictionary<int, Geocodage> dico = new Dictionary<int, Geocodage>();

        /// <summary>
        /// Nombre maximum d'enregistrements pour la requete
        /// </summary>
        private int topOfCount = 50;
        // descid du champ latitude
        string latitudefield = string.Empty;
        // descid  du champs longitude
        string longitudefield = string.Empty;
        // Message d'erreur
        string _error, request = string.Empty;
        //  Titre pour l'infobulle
        string titre = string.Empty;
        // Champs d'adresse
        string sqlfieldsAdr = string.Empty;
        protected StringBuilder sb = new StringBuilder();
        private enum Operation
        {
            NONE = -1,
            ShowDialog = 0,
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eBingMaps");

            #endregion

            RetrieveParams();
            RunOperation();
        }


        /// <summary>
        /// Récupération des infos postées
        /// </summary>
        private void RetrieveParams()
        {
            Enum.TryParse<Operation>(Request.Form["operation"], out _operation);
            //Table sur laquelle on recherche
            if (_requestTools.AllKeys.Contains("tab"))
                _nTab = eLibTools.GetNum(Request.Form["tab"]);
        }


        /// <summary>
        /// Execute l'action demandée 
        /// </summary>
        private void RunOperation()
        {
            switch (_operation)
            {
                case Operation.ShowDialog:
                    ShowDialog();
                    break;
                default:
                    break;
            }
        }

        public void GetInfos()
        {

        }

        public void ShowDialog()
        {
            String sError = String.Empty;

            h_key.Value = eLibConst.BING_MAPS_KEY;
            Dictionary<Int32, String> dicoMapping = AccessData.GetMappingFromFileMapPartners(_pref, _nTab, out _error);
            bool _forceFrance = false;

            if (_error.Length < 1 && dicoMapping != null)
            {
                latitudefield = dicoMapping[8];
                longitudefield = dicoMapping[9];
                if (dicoMapping[10] != null && dicoMapping[10].Length > 0)
                    topOfCount = eLibTools.GetNum(dicoMapping[10]);

            }
            else
            {
                LaunchError("Impossible de récupérer le Mapping");
            }

            try
            {

                // On va boucler sur les champs adresses pour fabrique une seule chaine a donner à Eudoquery
                for (int i = 2; i < 8; i++)
                {
                    if (dicoMapping[i].Length > 0)
                        sqlfieldsAdr = string.Concat(sqlfieldsAdr, dicoMapping[i], ";");
                }
                if (sqlfieldsAdr.Length > 0)
                    sqlfieldsAdr = sqlfieldsAdr.Substring(0, sqlfieldsAdr.Length - 1);


                if (dicoMapping[7] == string.Empty)
                    _forceFrance = true;
            }
            catch (Exception ex1)
            {
                _error = ex1.Message;
                LaunchError(_error);
            }


            #region Recuperation de la Requête via EudoQuery
            EudoQuery.EudoQuery eq = null;
            try
            {
                eq = eLibTools.GetEudoQuery(_pref, _nTab, EudoQuery.ViewQuery.LIST);
                eq.SetListCol = string.Concat(dicoMapping[1], ";", dicoMapping[8], ";", dicoMapping[9], ";", sqlfieldsAdr);

                eq.SetTopRecord = topOfCount;

                //Fiches marquées
                MarkedFilesSelection ms = null;
                _pref.Context.MarkedFiles.TryGetValue(_nTab, out ms);
                if (ms != null && ms.Enabled)
                    eq.SetDisplayMarkedFile = true;

                //Filtre avancé
                FilterSel FilterSel = null;
                this._pref.Context.Filters.TryGetValue(_nTab, out FilterSel);
                Int32 nFilterId = (FilterSel != null && FilterSel.FilterSelId > 0) ? FilterSel.FilterSelId : 0;
                if (nFilterId > 0)
                    eq.SetFilterId = nFilterId;
                // Filtre sur le champs latitude  different de vide et 0
                //      eq.AddCustomFieldFilter(latitudefield, EudoQuery.Operator.OP_DIFFERENT.GetHashCode(), "0");
                //       eq.AddCustomFieldFilter(latitudefield, EudoQuery.Operator.OP_IS_NOT_EMPTY.GetHashCode(), "");
                //        eq.AddCustomFieldFilter(longitudefield, EudoQuery.Operator.OP_DIFFERENT.GetHashCode(), null);
                // Filtre sur le champs longitude  different de vide et 0
                //     eq.AddCustomFieldFilter(longitudefield, EudoQuery.Operator.OP_DIFFERENT.GetHashCode(), "0");
                //      eq.AddCustomFieldFilter(longitudefield, EudoQuery.Operator.OP_IS_NOT_EMPTY.GetHashCode(), "");
                if (eq.GetError.Length > 0)
                    throw new Exception(string.Concat("eq Init : ", eq.GetError));
                eq.LoadRequest();
                if (eq.GetError.Length > 0)
                    throw new Exception(string.Concat("eq LoadRequest : ", eq.GetError));
                eq.BuildRequest();
                if (eq.GetError.Length > 0)
                    throw new Exception(string.Concat("eq BuildRequest : ", eq.GetError));
                request = eq.EqQuery;

                #region Récupération de la liste de champs voulu

                List<Field> lstFid = new List<Field>();

                EudoQuery.Field fld;
                for (int j = 0; j < dicoMapping.Count; j++)
                {
                    fld = eq.GetFieldHeaderList.Find(delegate (Field _myfid) { return _myfid.Descid == eLibTools.GetNum(dicoMapping[j]); });
                    if (fld != null)
                    {
                        dicoMapping[j] = fld.ValueAlias;

                        lstFid.Add(fld);
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                LaunchError(String.Concat(ex.Message));
            }
            finally
            {
                eq.CloseQuery();
            }

            #endregion

            #region Recuperation des informations en base de données via EudoDal

            DataTableReaderTuned _dtrFields = null;
            eudoDAL edal = null;

            Geocodage geopushpin = null;

            try
            {
                edal = eLibTools.GetEudoDAL(_pref);
                edal.OpenDatabase();

                RqParam rq = new RqParam(request);

                //Execution de la requete
                _dtrFields = edal.Execute(rq, out _error);
                edal.CloseDatabase();
            }
            catch (Exception ex2)
            {
                _error = ex2.Message;
                LaunchError(_error);
                edal.CloseDatabase();
            }

            #endregion

            #region Utilisation des données récupérées et géolocalisation si les coordonnées n'existent pas en base
            try
            {
                int idx = 0;

                if (_dtrFields != null)
                {
                    string _paveAdresse = string.Empty;
                    string _tmplat = string.Empty;
                    string _tmplong = string.Empty;
                    string _aliasLatitude = dicoMapping[8];
                    string _aliasLongitude = dicoMapping[9];
                    string _aliasTitre = dicoMapping[1];
                    string _aliasAdr1 = dicoMapping[2];
                    string _aliasAdr2 = dicoMapping[3];
                    string _aliasAdr3 = dicoMapping[4];
                    string _aliasCP = dicoMapping[5];
                    string _aliasVille = dicoMapping[6];
                    string _aliasPays = dicoMapping[7];

                    String addressLine = String.Empty;
                    while (_dtrFields.Read())
                    {
                        geopushpin = new Geocodage();

                        #region Construction du pavé adresse

                        _paveAdresse = string.Empty;

                        for (int k = 2; k < 8; k++)
                        {
                            if (dicoMapping[k] != string.Empty && _dtrFields.GetString(dicoMapping[k]) != string.Empty)
                            {
                                // En cas de catalogue avancé
                                if (_dtrFields.GetString(dicoMapping[k]).Contains("#$|#$"))
                                    _paveAdresse = string.Concat(_paveAdresse, _dtrFields.GetString(dicoMapping[k]).Split(new string[] { "#$|#$" }, StringSplitOptions.None)[0], ", ");
                                else
                                    _paveAdresse = string.Concat(_paveAdresse, _dtrFields.GetString(dicoMapping[k]), ", ");
                            }
                        }

                        if (_paveAdresse.Length > 3)
                            _paveAdresse = _paveAdresse.Substring(0, _paveAdresse.Length - 2);

                        //     _paveAdresse = string.Concat(dicoMapping[1] == "" ? _dtrFields[_aliasAdr1].ToString() : "", ";", _dtrFields[_aliasAdr2].ToString(), ";", _dtrFields[_aliasAdr3].ToString(), ";", _dtrFields[_aliasCP].ToString(), ";", _dtrFields[_aliasVille].ToString(), ";", _dtrFields[_aliasPays].ToString());
                        #endregion

                        #region Objet "Location" pour la position
                        //addressLine = String.Concat(_dtrFields[_aliasAdr1], " ", _dtrFields[_aliasAdr2], _dtrFields[_aliasAdr3]);
                        //LocationData location = new LocationData(_dtrFields[_aliasPays].ToString(), _dtrFields[_aliasCP].ToString(), _dtrFields[_aliasVille].ToString(), addressLine);

                        /*modif CNA le 07/12/2015*/
                        addressLine = String.Concat(_dtrFields.GetString(_aliasAdr1));
                        if (_aliasAdr2 != string.Empty)
                            addressLine = String.Concat(addressLine, " ", _dtrFields.GetString(_aliasAdr2));
                        if (_aliasAdr3 != string.Empty)
                            addressLine = String.Concat(addressLine, " ", _dtrFields.GetString(_aliasAdr3));

                        LocationData location = new LocationData(_aliasPays != string.Empty ? _dtrFields.GetString(_aliasPays) : "France", _dtrFields.GetString(_aliasCP), _dtrFields.GetString(_aliasVille), addressLine);
                        /*fin modif CNA*/
                        #endregion


                        geopushpin.AdresseHTML = eModelTools.EscapeStrForJS(_paveAdresse);
                        geopushpin.AdresseHTML = Geocodage.MakeHTMLAddress(geopushpin.AdresseHTML);

                        geopushpin.AdresseHTML = System.Web.HttpUtility.HtmlEncode(geopushpin.AdresseHTML);

                        // libellé titre par exemple nom de la société, nom du bien...
                        geopushpin.NameOfLbl = eModelTools.EscapeStrForJS(_dtrFields.GetString(_aliasTitre));
                        geopushpin.NameOfLbl = System.Web.HttpUtility.HtmlEncode(geopushpin.NameOfLbl);

                        //On va créer un objet seulement si les coordonnées lambert sont présente en base
                        if ((_dtrFields.GetString(_aliasLatitude) != "0" && _dtrFields.GetString(_aliasLatitude) != "") && (_dtrFields.GetString(_aliasLongitude) != "0" && _dtrFields.GetString(_aliasLongitude) != ""))
                        {
                            geopushpin.Latitude = _dtrFields.GetString(_aliasLatitude);
                            geopushpin.Longitude = _dtrFields.GetString(_aliasLongitude);

                            dico.Add(idx, geopushpin);
                            idx++;
                        }

                        else // Géolocalisation de l'adresse si les coordonnées ne sont pas en base
                        {
                            _error = string.Empty;

                            BingMapsSearch search = new BingMapsSearch(location);
                            GeocodeInfo result = search.FindLocationByAddress();

                            if (search.Error.Length < 1 && result != null)
                            {
                                geopushpin.Latitude = result.Latitude;
                                geopushpin.Longitude = result.Longitude;

                                //Sauvegarde en base des coordonnées trouvées
                                geopushpin.DescIdTab = eLibTools.GetNum(dicoMapping[0]);
                                AccessData.GetTabNameFromDescId(_pref, ref geopushpin);
                                geopushpin.FileId = _dtrFields.GetEudoNumeric(string.Concat(dicoMapping[0], "_ID"));
                                geopushpin.NameFileLatitude = AccessData.GetFieldNameFromDescId(_pref, eLibTools.GetNum(latitudefield), out _error);
                                geopushpin.NameFileLongitude = AccessData.GetFieldNameFromDescId(_pref, eLibTools.GetNum(longitudefield), out _error);

                                AccessData.UpdateFieldsSQL(_pref, geopushpin, false, out _error);

                                dico.Add(idx, geopushpin);
                                idx++;
                            }
                        }

                    }
                }

                HtmlInputHidden hiddenNBCount = new HtmlInputHidden();
                hiddenNBCount.ID = "h_nbresults";
                hiddenNBCount.Value = dico.Count.ToString();
                divHidden.Controls.Add(hiddenNBCount);
            }
            catch (Exception ex2)
            {
                LaunchError(ex2.Message);
                return;
            }
            #endregion

        }

        public void LaunchError(String error)
        {

            String sDevMsg = string.Concat("Erreur : ", error);

            ErrorContainer = eErrorContainer.GetDevUserError(
               eLibConst.MSG_TYPE.CRITICAL,
               eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
               string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
               eResApp.GetRes(_pref, 72),  //   titre
               string.Concat(sDevMsg));
            LaunchErrorHTML(true);
        }

    }
}