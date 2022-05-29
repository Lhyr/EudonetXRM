using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.counters;
using System.Linq;
using System.Collections.Generic;
using Com.Eudonet.Core.Model.volumetry;
using Com.Eudonet.Internal.counters.structure;
using System.Data.SqlClient;
using System.Data;
using System.Web.Services.Description;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer gérant l'affichage du tableau de bord en administration : licence, compteurs de volumétrie
    /// </summary>
    public class eAdminDashboardRenderer : eAdminModuleRenderer
    {
        private class CounterData
        {
            double monthJanuary = 0;
            double monthFebruary = 0;
            double monthMarch = 0;
            double monthApril = 0;
            double monthMay = 0;
            double monthJune = 0;
            double monthJuly = 0;
            double monthAugust = 0;
            double monthSeptember = 0;
            double monthOctober = 0;
            double monthNovember = 0;
            double monthDecember = 0;
            double yearTotal = 0;
            double yearMaximum = 0;

            public double MonthJanuary
            {
                get
                {
                    return monthJanuary;
                }

                set
                {
                    monthJanuary = value;
                }
            }

            public double MonthFebruary
            {
                get
                {
                    return monthFebruary;
                }

                set
                {
                    monthFebruary = value;
                }
            }

            public double MonthMarch
            {
                get
                {
                    return monthMarch;
                }

                set
                {
                    monthMarch = value;
                }
            }

            public double MonthApril
            {
                get
                {
                    return monthApril;
                }

                set
                {
                    monthApril = value;
                }
            }

            public double MonthMay
            {
                get
                {
                    return monthMay;
                }

                set
                {
                    monthMay = value;
                }
            }

            public double MonthJune
            {
                get
                {
                    return monthJune;
                }

                set
                {
                    monthJune = value;
                }
            }

            public double MonthJuly
            {
                get
                {
                    return monthJuly;
                }

                set
                {
                    monthJuly = value;
                }
            }

            public double MonthAugust
            {
                get
                {
                    return monthAugust;
                }

                set
                {
                    monthAugust = value;
                }
            }

            public double MonthSeptember
            {
                get
                {
                    return monthSeptember;
                }

                set
                {
                    monthSeptember = value;
                }
            }

            public double MonthOctober
            {
                get
                {
                    return monthOctober;
                }

                set
                {
                    monthOctober = value;
                }
            }

            public double MonthNovember
            {
                get
                {
                    return monthNovember;
                }

                set
                {
                    monthNovember = value;
                }
            }

            public double MonthDecember
            {
                get
                {
                    return monthDecember;
                }

                set
                {
                    monthDecember = value;
                }
            }

            public double YearTotal
            {
                get
                {
                    return yearTotal;
                }

                set
                {
                    yearTotal = value;
                }
            }

            public double YearMaximum
            {
                get
                {
                    return yearMaximum;
                }

                set
                {
                    yearMaximum = value;
                }
            }

            public static CounterData operator +(CounterData c1, CounterData c2)
            {
                CounterData c3 = new CounterData();
                c3.MonthJanuary = c1.MonthJanuary + c2.MonthJanuary;
                c3.MonthFebruary = c1.MonthFebruary + c2.MonthFebruary;
                c3.MonthMarch = c1.MonthMarch + c2.MonthMarch;
                c3.MonthApril = c1.MonthApril + c2.MonthApril;
                c3.MonthMay = c1.MonthMay + c2.MonthMay;
                c3.MonthJune = c1.MonthJune + c2.MonthJune;
                c3.MonthJuly = c1.MonthJuly + c2.MonthJuly;
                c3.MonthAugust = c1.MonthAugust + c2.MonthAugust;
                c3.MonthSeptember = c1.MonthSeptember + c2.MonthSeptember;
                c3.MonthOctober = c1.MonthOctober + c2.MonthOctober;
                c3.MonthNovember = c1.MonthNovember + c2.MonthNovember;
                c3.MonthDecember = c1.MonthDecember + c2.MonthDecember;
                c3.YearTotal = c1.YearTotal + c2.YearTotal;
                c3.YearMaximum = c1.YearMaximum + c2.YearMaximum;
                return c3;
            }

            public static CounterData operator -(CounterData c1, CounterData c2)
            {
                CounterData c3 = new CounterData();
                c3.MonthJanuary = c1.MonthJanuary - c2.MonthJanuary;
                c3.MonthFebruary = c1.MonthFebruary - c2.MonthFebruary;
                c3.MonthMarch = c1.MonthMarch - c2.MonthMarch;
                c3.MonthApril = c1.MonthApril - c2.MonthApril;
                c3.MonthMay = c1.MonthMay - c2.MonthMay;
                c3.MonthJune = c1.MonthJune - c2.MonthJune;
                c3.MonthJuly = c1.MonthJuly - c2.MonthJuly;
                c3.MonthAugust = c1.MonthAugust - c2.MonthAugust;
                c3.MonthSeptember = c1.MonthSeptember - c2.MonthSeptember;
                c3.MonthOctober = c1.MonthOctober - c2.MonthOctober;
                c3.MonthNovember = c1.MonthNovember - c2.MonthNovember;
                c3.MonthDecember = c1.MonthDecember - c2.MonthDecember;
                c3.YearTotal = c1.YearTotal - c2.YearTotal;
                c3.YearMaximum = c1.YearMaximum - c2.YearMaximum;
                return c3;
            }

            public static CounterData operator *(CounterData c1, CounterData c2)
            {
                CounterData c3 = new CounterData();
                c3.MonthJanuary = c1.MonthJanuary * c2.MonthJanuary;
                c3.MonthFebruary = c1.MonthFebruary * c2.MonthFebruary;
                c3.MonthMarch = c1.MonthMarch * c2.MonthMarch;
                c3.MonthApril = c1.MonthApril * c2.MonthApril;
                c3.MonthMay = c1.MonthMay * c2.MonthMay;
                c3.MonthJune = c1.MonthJune * c2.MonthJune;
                c3.MonthJuly = c1.MonthJuly * c2.MonthJuly;
                c3.MonthAugust = c1.MonthAugust * c2.MonthAugust;
                c3.MonthSeptember = c1.MonthSeptember * c2.MonthSeptember;
                c3.MonthOctober = c1.MonthOctober * c2.MonthOctober;
                c3.MonthNovember = c1.MonthNovember * c2.MonthNovember;
                c3.MonthDecember = c1.MonthDecember * c2.MonthDecember;
                c3.YearTotal = c1.YearTotal * c2.YearTotal;
                c3.YearMaximum = c1.YearMaximum * c2.YearMaximum;
                return c3;
            }

            public static CounterData operator /(CounterData c1, CounterData c2)
            {
                CounterData c3 = new CounterData();
                c3.MonthJanuary = c1.MonthJanuary / c2.MonthJanuary;
                c3.MonthFebruary = c1.MonthFebruary / c2.MonthFebruary;
                c3.MonthMarch = c1.MonthMarch / c2.MonthMarch;
                c3.MonthApril = c1.MonthApril / c2.MonthApril;
                c3.MonthMay = c1.MonthMay / c2.MonthMay;
                c3.MonthJune = c1.MonthJune / c2.MonthJune;
                c3.MonthJuly = c1.MonthJuly / c2.MonthJuly;
                c3.MonthAugust = c1.MonthAugust / c2.MonthAugust;
                c3.MonthSeptember = c1.MonthSeptember / c2.MonthSeptember;
                c3.MonthOctober = c1.MonthOctober / c2.MonthOctober;
                c3.MonthNovember = c1.MonthNovember / c2.MonthNovember;
                c3.MonthDecember = c1.MonthDecember / c2.MonthDecember;
                c3.YearTotal = c1.YearTotal / c2.YearTotal;
                c3.YearMaximum = c1.YearMaximum / c2.YearMaximum;
                return c3;
            }
        }

        const int _warningThreshold = 95;
        const int _criticalThreshold = 99;

        Panel _pnlContents;
        Panel _pnlContentsLicenseOffer;
        Panel _pnlContentsVolume; // conteneur spécifique des éléments de Volumétrie
        eClientInfos _client = null;
        string _numericValuesClass = "odometer";

        string _baseInfoErrors = String.Empty;
        string _mailSMSInfoErrors = String.Empty;
        string _quotasInfoErrors = String.Empty;
        string _soldeSMSErrors = string.Empty;
        string _soldeEmailErrors = string.Empty;
        string _soldeEStockageErrors = string.Empty;

        #region Initialisation

        #region Valeurs
        int _decimalCount = 2;

        //Tout en Go pour les calculs
        double _sizeDatabaseAttachments = 0;
        double _sizeAttachments = 0;
        double _sizeAvailableStorage = 0;

        //Peut-etre en différentes unités, pour l'affichage
        double _sizeDatabaseAttachmentsDisplay = 0;
        double _sizeAttachmentsDisplay = 0;

        int _counterYear = DateTime.Now.Year;
        CounterData _counterMail = new CounterData();
        CounterData _counterMailCampaign = new CounterData();
        CounterData _counterMailSystem = new CounterData();
        CounterData _counterMailNotification = new CounterData();
        CounterData _counterMailXSPSendMail = new CounterData();
        CounterData _counterMailForgotPassword = new CounterData();
        CounterData _counterMailFeedback = new CounterData();
        CounterData _counterMailTotal = new CounterData();
        CounterData _counterSMS = new CounterData();
        CounterData _counterSMSCampaign = new CounterData();
        CounterData _counterSMSTotal = new CounterData();
        long _counterMailTotalPurchase = 0;
        long _counterSMSTotalPurchase = 0;


        int _nBalanceMail = 0;
        int _nBalanceSMS = 0;
        int _nBalanceDisk = 0;

        // Crédits - TODO VOLUMETRIE
        CounterData _acquiredMailBought = new CounterData();
        CounterData _acquiredMailCredited = new CounterData();
        CounterData _acquiredMailIntervention = new CounterData();
        CounterData _acquiredSMSBought = new CounterData();
        CounterData _acquiredSMSCredited = new CounterData();
        CounterData _acquiredSMSIntervention = new CounterData();

        long _counterMailTotalAcquired = 0;
        long _counterSMSTotalAcquired = 0;

        string _valueDatabaseAttachmentsUnit = String.Empty;
        string _valueAttachmentsUnit = String.Empty;
        string _valueAvailableStorageUnit = String.Empty;
        #endregion

        #region Couleurs
        // Couleurs de base affichées : vert si espace disponible utilisé à moins de threshold%, orange si utilisé à plus de threshold%, rouge si dépassé
        string _colorDatabaseAttachments = "green";
        string _colorAttachments = "green";
        string _colorAvailableStorage = "green";
        string _colorMailTotalBox = "white";
        string _colorMailTotal = "teal";
        string _colorMailCampaign = "teal";
        string _colorMail = "teal";
        string _colorMailSystem = "teal";
        string _colorSMSTotalBox = "white";
        string _colorSMSTotal = "blue";
        string _colorSMSCampaign = "blue";
        string _colorSMS = "blue";
        #endregion

        bool _displayStoragePanel = true;
        bool _displayMailSentPanel = true;
        bool _displayMailAcquiredPanel = true;
        bool _displaySMSSentPanel = true;
        bool _displaySMSAcquiredPanel = true;

        #endregion

        private eAdminDashboardRenderer(ePref pref, int w, int h, int year) : base(pref)
        {
            _width = w;
            _height = h;
            if (year != 0)
                _counterYear = year;
        }

        /// <summary>
        /// Génération du renderer
        /// </summary>
        /// <param name="pref">Objet pref</param>
        /// <param name="w">Largeur</param>
        /// <param name="h">Hauteur</param>
        /// <param name="year">Année</param>
        /// <returns></returns>
        public static eAdminDashboardRenderer CreateAdminDashboardRenderer(ePref pref, int w, int h, int year)
        {
            return new eAdminDashboardRenderer(pref, w, h, year);
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminDashboard))
                return false;

            _client = Pref.ClientInfos;

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            GetDashboardValues();
            AddToJavaScript();

            InitInnerContainer();
            GetRendererContents();

            return true;
        }

        /// <summary>
        /// Appel au Web Service et calcul des valeurs du tableau de bord
        /// </summary>
        private void GetDashboardValues()
        {
            AuditApiDbInformation baseSizeInfos = null;
            AuditApiDbQuotas quotasInfo = null;
            List<AuditApiDbOperations> localMailSmsOperationsInfos = null;

            AuditApiDbGetOperationsResult balanceMail = new AuditApiDbGetOperationsResult();
            AuditApiDbGetOperationsResult balanceSMS = new AuditApiDbGetOperationsResult();
            AuditApiDbGetOperationsResult soldeDiskSpace = new AuditApiDbGetOperationsResult();

            string sError = string.Empty;

            #region Appels Web Service - Si le serveur est paramétré pour autoriser les accès vers l'extérieur, appel du Web Service pour récupérer les données



            IDictionary<eLibConst.SERVERCONFIG, string> serverConfig = eLibTools.GetServerConfigValues(Pref, eLibConst.SERVERCONFIG.AUDIT_SERVICE_TOKEN);
            string auditServiceToken = String.Empty;
            localMailSmsOperationsInfos = new List<AuditApiDbOperations>();


            bool bUseWS = (eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "0"
                && eLibTools.GetServerConfig("ServerWithoutDBSizeInfos", "0") == "0"
                && serverConfig.TryGetValue(eLibConst.SERVERCONFIG.AUDIT_SERVICE_TOKEN, out auditServiceToken));

            //recheche des soldes à la date du jour
            if (bUseWS)
            {
                eAuditAPI auditWS = new eAuditAPI(eLibTools.GetServerConfig("EudoVolumetrieURL", eLibConst.AUDIT_API_URL), Pref.ModeDebug, auditServiceToken);


                //Appel pour avoir les tailles                
                {
                    Action<AuditApiResponseDbInformation> apiActionOnSuccess =
                    delegate (AuditApiResponseDbInformation response)
                    {
                        // Si on a reçu un retour WS avec des informations (BaseName renseigné)
                        if (response.ResultInfos.Error == null ||
                            (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == -1))
                        {
                            baseSizeInfos = response.ResultData.DbInfos;
                        }
                        // Si on a reçu un retour WS sans informations (BaseName vide)
                        else if (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == 404)
                        {
                            _baseInfoErrors = String.Concat(_baseInfoErrors, Environment.NewLine, eResApp.GetRes(Pref, 8099)); // Non Disponible
                        }
                    };

                    Action<string> apiActionOnFailure =
                    delegate (string error)
                    {
                        _baseInfoErrors = error;
                        // En cas d'erreur, on masque l'encart Volumétrie, sauf si on est en mode Debug
                        _displayStoragePanel = Pref.ModeDebug;
                    };

                    auditWS.GetDatabaseInfos(Pref.GetBaseName, apiActionOnSuccess, apiActionOnFailure);
                }

                //Appel pour avoir les quotas
                {
                    Action<AuditApiResponseDbQuotas> apiActionOnSuccess =
                    delegate (AuditApiResponseDbQuotas response)
                    {
                        // Si on a reçu un retour WS avec des informations (BaseName renseigné)
                        if (response.ResultInfos.Error == null ||
                            (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == -1))
                        {
                            quotasInfo = response.ResultData.Quotas;
                        }
                        // Si on a reçu un retour WS sans informations (BaseName vide)
                        else if (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == 404)
                        {
                            _quotasInfoErrors = String.Concat(_quotasInfoErrors, Environment.NewLine, eResApp.GetRes(Pref, 8099)); // Non Disponible
                        }
                    };

                    Action<string> apiActionOnFailure =
                    delegate (string error)
                    {
                        _quotasInfoErrors = error;
                    };

                    auditWS.GetDatabaseQuotas(Pref.GetBaseName, apiActionOnSuccess, apiActionOnFailure);
                }

                //Appel pour avoir le solde email
                {
                    AuditApiDbGetOperationParam paramsFiltre = new AuditApiDbGetOperationParam();

                    paramsFiltre.DataBaseName = Pref.GetBaseName;
                    paramsFiltre.ProductCode = (int)PRODUCT.MAIL;
                    paramsFiltre.ServerName = "";
                    paramsFiltre.DateStartString = DateTime.Now.ToString("yyyy/MM/01");
                    paramsFiltre.DateEndString = DateTime.Now.ToString("yyyy/MM/01");


                    Action<AuditApiResponseDbGetOperation> apiActionOnSuccess =
                    delegate (AuditApiResponseDbGetOperation response)
                    {
                        // Si on a reçu un retour WS avec des informations (BaseName renseigné)
                        if (response.ResultInfos.Error == null ||
                            (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == -1))
                        {
                            if (response.ResultData.InformationsProductConsumption?.Count() > 0)
                            {
                                _nBalanceMail = response.ResultData.InformationsProductConsumption[0].Balance;
                            }
                        }
                        // Si on a reçu un retour WS sans informations (BaseName vide)
                        else if (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == 404)
                        {
                            _soldeEmailErrors = String.Concat(_soldeEmailErrors, Environment.NewLine, eResApp.GetRes(Pref, 8099)); // Non Disponible
                        }
                    };

                    Action<string> apiActionOnFailure =
                    delegate (string error)
                    {
                        _soldeEmailErrors = error;
                    };

                    auditWS.GetCounters(paramsFiltre, apiActionOnSuccess, apiActionOnFailure);
                }

                //Appel pour avoir le solde SMS
                {
                    AuditApiDbGetOperationParam paramsFiltre = new AuditApiDbGetOperationParam();

                    paramsFiltre.DataBaseName = Pref.GetBaseName;
                    paramsFiltre.ProductCode = (int)PRODUCT.SMS;
                    paramsFiltre.ServerName = "";
                    paramsFiltre.DateStartString = DateTime.Now.ToString("yyyy/MM/01");
                    paramsFiltre.DateEndString = DateTime.Now.ToString("yyyy/MM/01");


                    Action<AuditApiResponseDbGetOperation> apiActionOnSuccess =
                    delegate (AuditApiResponseDbGetOperation response)
                    {
                        // Si on a reçu un retour WS avec des informations (BaseName renseigné)
                        if (response.ResultInfos.Error == null ||
                            (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == -1))
                        {
                            if (response.ResultData.InformationsProductConsumption?.Count() > 0)
                            {
                                _nBalanceSMS = response.ResultData.InformationsProductConsumption[0].Balance;
                            }
                        }
                        // Si on a reçu un retour WS sans informations (BaseName vide)
                        else if (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == 404)
                        {
                            _soldeSMSErrors = String.Concat(_soldeSMSErrors, Environment.NewLine, eResApp.GetRes(Pref, 8099)); // Non Disponible
                        }
                    };

                    Action<string> apiActionOnFailure =
                    delegate (string error)
                    {
                        _soldeSMSErrors = error;
                    };

                    auditWS.GetCounters(paramsFiltre, apiActionOnSuccess, apiActionOnFailure);
                }

                //Appel pour avoir le solde DISSPACE
                {
                    AuditApiDbGetOperationParam paramsFiltre = new AuditApiDbGetOperationParam();

                    paramsFiltre.DataBaseName = Pref.GetBaseName;
                    paramsFiltre.ProductCode = (int)PRODUCT.DISKSPACE;
                    paramsFiltre.ServerName = "";
                    paramsFiltre.DateStartString = DateTime.Now.ToString("yyyy/MM/01");
                    paramsFiltre.DateEndString = DateTime.Now.ToString("yyyy/MM/01");


                    Action<AuditApiResponseDbGetOperation> apiActionOnSuccess =
                    delegate (AuditApiResponseDbGetOperation response)
                    {
                        // Si on a reçu un retour WS avec des informations (BaseName renseigné)
                        if (response.ResultInfos.Error == null ||
                            (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == -1))
                        {
                            if (response.ResultData.InformationsProductConsumption?.Count() > 0)
                            {
                                _nBalanceDisk = response.ResultData.InformationsProductConsumption[0].Balance;
                            }
                        }
                        // Si on a reçu un retour WS sans informations (BaseName vide)
                        else if (response.ResultInfos.Error != null && response.ResultInfos.Error.ErrorNum == 404)
                        {
                            _soldeEStockageErrors = String.Concat(_soldeEStockageErrors, Environment.NewLine, eResApp.GetRes(Pref, 8099)); // Non Disponible
                        }
                    };

                    Action<string> apiActionOnFailure =
                    delegate (string error)
                    {
                        _soldeEStockageErrors = error;
                    };

                    auditWS.GetCounters(paramsFiltre, apiActionOnSuccess, apiActionOnFailure);
                }


                // Appel pour avoir les opérations de SMS / E-Mails -- Appel en base - renseigné depuis EudoProcess
                // Il s'agit d'un appel base local mais sans accès WS, cela n'a pas de sens de le faire                
                localMailSmsOperationsInfos = OperationInfos.GetDbOperation(_ePref, out sError, _counterYear);
            }



            #endregion



            #region Calcul des valeurs affichées

            #region - calcul des quantité acheté + régul

            //total des opération d'achat & régulation
            var opAvailabbleMail = localMailSmsOperationsInfos.FindAll(op => op.Operation != null
               && (op.Operation.OperationCode == (int)OPERATION.PURCHASE || op.Operation.OperationCode == (int)OPERATION.REGULATION)
               && op.Operation.ProductCode == (int)PRODUCT.MAIL
              );


            if (opAvailabbleMail.Count > 0)
            {
                //si opération d'achat, ce sont elle que l'on prend
                foreach (var ope in opAvailabbleMail)
                {
                    _counterMailTotalPurchase += ope.Operation.Quantity;
                }
            }
            else
            {
                _counterMailTotalPurchase = quotasInfo?.AnnualEmailCredit ?? 0;
                if (_counterMailTotalPurchase == 0) // par défaut
                    _counterMailTotalPurchase = 100000;
            }

            //Dispo SMS      
            var opAvailabbleSMS = localMailSmsOperationsInfos.FindAll(op => op.Operation != null
               && (op.Operation.OperationCode == (int)OPERATION.PURCHASE || op.Operation.OperationCode == (int)OPERATION.REGULATION)
               && op.Operation.ProductCode == (int)PRODUCT.SMS
               );


            if (opAvailabbleSMS.Count > 0)
            {
                //si opération d'achat, ce sont elle que l'on prend
                foreach (var ope in opAvailabbleSMS)
                {
                    _counterSMSTotalPurchase += ope.Operation.Quantity;
                }
            }
            else
            {
                _counterSMSTotalPurchase = quotasInfo?.AnnualSMSCredit ?? 0;
            }


            //Taille base
            //AABBA tache #3 146 la modification de calcul de stockage 
            // US #935 / Tâche 1383 et associées - Si, pour une raison ou une autre, on obtient 0 (WS inaccessible ou renvoyant 0 faute de données exploitables), on renvoie les valeurs par défaut
            // Confirmation d'ALEB le 30/12/2019 conformément à ce qui était initialement décrit dans la US
            //Dispo SMS         
            var opAvailabbleDisk = localMailSmsOperationsInfos.FindAll(op => op.Operation != null
               && (op.Operation.OperationCode == (int)OPERATION.PURCHASE || op.Operation.OperationCode == (int)OPERATION.REGULATION)
               && op.Operation.ProductCode == (int)PRODUCT.DISKSPACE
              );


            if (opAvailabbleDisk.Count > 0)
            {
                //si opération d'achat, ce sont elle que l'on prend
                foreach (var ope in opAvailabbleDisk)
                {
                    _sizeAvailableStorage += ope.Operation.Quantity;
                }
            }
            else
            {
                _sizeAvailableStorage = quotasInfo?.StorageSpace ?? (Pref.ClientInfos.NbFreeSubscriptions + Pref.ClientInfos.NbPaidSubscriptions); // Capacité de stockage disponible : 1 Go par utilisateur
                if (_sizeAvailableStorage == 0)
                    _sizeAvailableStorage = (Pref.ClientInfos.NbFreeSubscriptions + Pref.ClientInfos.NbPaidSubscriptions);
            }


            #endregion



            #region 1 - Stockage

            #region Si le serveur est paramétré pour autoriser les accès vers l'extérieur, utilisation des données renvoyées par le Web Service

            if (baseSizeInfos != null)
            {
                #region Paramétrage des valeurs - Stockage

                // Unité de base affichée : Mo pour la base et les annexes, Go pour la capacité disponible (because 1 Go par utilisateur)
                _valueDatabaseAttachmentsUnit = eResApp.GetRes(Pref, 66); // Mo (unité)
                _valueAttachmentsUnit = eResApp.GetRes(Pref, 66); // Mo (unité)
                _valueAvailableStorageUnit = eResApp.GetRes(Pref, 1812); // Go (unité);

                // Conversion en Mo (taille renvoyée en octets par le Web Service) et en double (pour garder les décimales)
                _sizeDatabaseAttachmentsDisplay = _sizeDatabaseAttachments = (double)baseSizeInfos.TotalSize / 1024 / 1024;
                _sizeAttachmentsDisplay = _sizeAttachments = (double)baseSizeInfos.AnnexeSize / 1024 / 1024;
                // Conversion en Go pour les calculs
                _sizeDatabaseAttachments = _sizeDatabaseAttachments / 1024;
                _sizeAttachments = _sizeAttachments / 1024;

                // Puis, si la valeur est toujours supérieure à 1024, passage en Go
                if (_sizeDatabaseAttachmentsDisplay >= 1024)
                {
                    _sizeDatabaseAttachmentsDisplay = _sizeDatabaseAttachments;
                    _valueDatabaseAttachmentsUnit = eResApp.GetRes(Pref, 1812); // Go (unité)
                }

                if (_sizeAttachmentsDisplay >= 1024)
                {
                    _sizeAttachmentsDisplay = _sizeAttachments;
                    _valueAttachmentsUnit = eResApp.GetRes(Pref, 1812); // Go (unité)
                }
                // Puis, si la valeur est toujours supérieure à 1024, passage en To (just kidding... or not.)
                // ...

                #endregion

                #region Paramétrage des couleurs - Stockage

                _colorDatabaseAttachments = GetNewColorFromThreshold(_colorDatabaseAttachments, _sizeDatabaseAttachments, _sizeAvailableStorage);
                _colorAttachments = GetNewColorFromThreshold(_colorAttachments, _sizeAttachments, _sizeAvailableStorage);
                _colorAvailableStorage = _colorDatabaseAttachments;

                #endregion
            }

            #endregion

            #region Sinon, utilisation de données locales (si applicable)

            else
            {
                #region Paramétrage des valeurs - Stockage

                // Aucun calcul possible ici, sauf la capacité totale disponible déjà calculée plus haut

                #endregion
            }

            #endregion

            #endregion



            #region 2 -  E-mails et SMS - Achat et consommation

            #region Si le serveur est paramétré pour autoriser les accès vers l'extérieur, utilisation des données renvoyées par le Web Service


            #region Gestion compteur d'achat

            for (int month = 1; month < 13; month++)
            {
                List<AuditApiDbOperations> monthOperationPurchaseEmailSMS = localMailSmsOperationsInfos.FindAll(ope => ope.Operation != null
                && ope.Operation.DateDT.Month == month
                 && ope.Operation.DateDT.Year == _counterYear
                && ope.Operation.OperationCode == (int)OPERATION.PURCHASE);

                if (monthOperationPurchaseEmailSMS != null)
                {
                    foreach (AuditApiDbOperations operationSmsEmail in monthOperationPurchaseEmailSMS)
                    {
                        if (operationSmsEmail.Operation.ProductCode == (int)PRODUCT.MAIL)
                        {
                            ComputeDBInfosVolumeData(_acquiredMailBought, operationSmsEmail.Operation, month);
                            _acquiredMailBought.YearTotal += operationSmsEmail.Operation.Quantity;
                        }


                        if (operationSmsEmail.Operation.ProductCode == (int)PRODUCT.SMS)
                        {
                            ComputeDBInfosVolumeData(_acquiredSMSBought, operationSmsEmail.Operation, month);
                            _acquiredSMSBought.YearTotal += operationSmsEmail.Operation.Quantity;
                        }
                    }

                }

            }

            #endregion


            #endregion

            #region Valeur en base de consommation détailée

            eudoDAL dal = eLibTools.GetEudoDAL(_ePref);
            bool closeDal = false;
            if (!dal.IsOpen)
            {
                dal.OpenDatabase();
                closeDal = true;
            }

            List<eEdnMsgCmpt.CounterVolume> counterVolumeData = GetCounterVolumeData(dal).ToList();

            if (closeDal)
                dal.CloseDatabase();

            ComputeCounterVolumeData(counterVolumeData, localMailSmsOperationsInfos);

            #region Paramétrage des couleurs

            _colorMail = GetNewColorFromThreshold(_colorMail, _counterMail.YearTotal, _counterMailTotalPurchase);
            _colorMailCampaign = GetNewColorFromThreshold(_colorMailCampaign, _counterMailCampaign.YearTotal, _counterMailTotalPurchase);
            _colorMailSystem = GetNewColorFromThreshold(_colorMailSystem, _counterMailSystem.YearTotal, _counterMailTotalPurchase);
            _colorMailTotal = GetNewColorFromThreshold(_colorMailTotal, _counterMailTotal.YearTotal, _counterMailTotalPurchase);
            _colorSMS = GetNewColorFromThreshold(_colorSMS, _counterSMS.YearTotal, _counterSMSTotalPurchase);
            _colorSMSCampaign = GetNewColorFromThreshold(_colorSMSCampaign, _counterSMSCampaign.YearTotal, _counterSMSTotalPurchase);
            _colorSMSTotal = GetNewColorFromThreshold(_colorSMSTotal, _counterSMSTotal.YearTotal, _counterSMSTotalPurchase);



            #endregion


            #endregion

            #endregion

            #endregion
        }

        private void ComputeDBInfosVolumeData(CounterData cd, OperationModel op, int month)
        {
            switch (month)
            {
                case 1:
                    cd.MonthJanuary += op.Quantity;
                    break;
                case 2:
                    cd.MonthFebruary += op.Quantity;
                    break;
                case 3:
                    cd.MonthMarch += op.Quantity;
                    break;
                case 4:
                    cd.MonthApril += op.Quantity;
                    break;
                case 5:
                    cd.MonthMay += op.Quantity;
                    break;
                case 6:
                    cd.MonthJune += op.Quantity;
                    break;
                case 7:
                    cd.MonthJuly += op.Quantity;
                    break;
                case 8:
                    cd.MonthAugust += op.Quantity;
                    break;
                case 9:
                    cd.MonthSeptember += op.Quantity;
                    break;
                case 10:
                    cd.MonthOctober += op.Quantity;
                    break;
                case 11:
                    cd.MonthNovember += op.Quantity;
                    break;
                case 12:
                    cd.MonthDecember += op.Quantity;
                    break;
            }
        }


        private IEnumerable<eEdnMsgCmpt.CounterVolume> GetCounterVolumeData(eudoDAL dal)
        {
            return eEdnMsgCmpt.GetCounterVolume(dal, _counterYear).ToList();
        }


        /// <summary>
        /// Répartition de la liste des compteur pour chaque type de compteur
        /// </summary>
        /// <param name="listCounterVolume">Compteur </param>
        /// <param name="lstOpe">Listye des opértation (régulation)</param>
        private void ComputeCounterVolumeData(IEnumerable<eEdnMsgCmpt.CounterVolume> listCounterVolume, List<AuditApiDbOperations> lstOpe)
        {

            if (lstOpe == null)
                lstOpe = new List<AuditApiDbOperations>();

            _counterMail = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.EMAIL);

            //Campagne mail avec régulation
            _counterMailCampaign = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.EMAIL_CAMPAIGN,
                lstOpe.FindAll(zz => zz.Operation != null && zz.Operation.ProductCode == (int)PRODUCT.MAIL && zz.Operation.OperationCode == (int)OPERATION.REGULATION));

            _counterMailNotification = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.NOTIFICATION);
            _counterMailXSPSendMail = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.XSP_SENDMAIL);
            _counterMailForgotPassword = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.FORGOTPASSWORD);
            _counterMailFeedback = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.FEEDBACK);
            _counterMailSystem = _counterMailNotification + _counterMailXSPSendMail + _counterMailForgotPassword + _counterMailFeedback; // TOCHECK VOLUMETRIE
            _counterMailTotal = _counterMail + _counterMailCampaign + _counterMailSystem;
            _counterSMS = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.SMS);

            //SMS avec régulation
            _counterSMSCampaign = ComputeCounterVolumeData(listCounterVolume, eEdnMsgCmpt.CounterType.SMS_CAMPAIGN, lstOpe.FindAll(zz => zz.Operation != null
                && zz.Operation.ProductCode == (int)PRODUCT.SMS
                && zz.Operation.OperationCode == (int)OPERATION.REGULATION)
                );
            _counterSMSTotal = _counterSMS + _counterSMSCampaign;
        }


        /// <summary>
        /// génération d'un compteur de volumétrie pour le type fourni (répartion par mois de la consommation)
        /// </summary>
        /// <param name="listCounterVolume">liste de toutes les volumétrie</param>
        /// <param name="type">type de consommation (mail, sms, ...)</param>
        /// <param name="lstOpe">opération de régulation (liste par mois)</param>
        /// <returns></returns>
        private CounterData ComputeCounterVolumeData(IEnumerable<eEdnMsgCmpt.CounterVolume> listCounterVolume, eEdnMsgCmpt.CounterType type, List<AuditApiDbOperations> lstOpe = null)
        {
            CounterData counterData = new CounterData();

            IEnumerable<eEdnMsgCmpt.CounterVolume> listCounterVolumeTemp = listCounterVolume.Where(cv => cv.Type == type);

            counterData.YearTotal = listCounterVolumeTemp.Sum(cv => cv.Volume);
            if (counterData.YearTotal > 0)
                counterData.YearMaximum = listCounterVolumeTemp.Max(cv => cv.Volume);

            for (int month = 1; month < 13; ++month)
            {
                eEdnMsgCmpt.CounterVolume counterVolume = listCounterVolumeTemp.FirstOrDefault(cv => cv.Date.Month == month);
                if (counterVolume == null)
                {
                    counterVolume = new eEdnMsgCmpt.CounterVolume();
                    counterVolume.Volume = 0;
                }

                //  if (type == eEdnMsgCmpt.CounterType.EMAIL_CAMPAIGN || type == eEdnMsgCmpt.CounterType.SMS_CAMPAIGN)
                //    counterVolume.Volume += balanceMail?.Regulation ?? 0;

                //si une liste d'opération est fourni, on ajoute la régulation du mois en cours
                if (lstOpe != null && lstOpe.Count > 0)
                {
                    var t = lstOpe.FirstOrDefault(zz => zz.Operation.DateDT.Month == month && zz.Operation.OperationCode == (int)OPERATION.REGULATION);
                    if (t != null)
                        counterVolume.Volume += t.Operation.Quantity;
                }

                switch (month)
                {
                    case 1:
                        counterData.MonthJanuary = counterVolume.Volume;
                        break;
                    case 2:
                        counterData.MonthFebruary = counterVolume.Volume;
                        break;
                    case 3:
                        counterData.MonthMarch = counterVolume.Volume;
                        break;
                    case 4:
                        counterData.MonthApril = counterVolume.Volume;
                        break;
                    case 5:
                        counterData.MonthMay = counterVolume.Volume;
                        break;
                    case 6:
                        counterData.MonthJune = counterVolume.Volume;
                        break;
                    case 7:
                        counterData.MonthJuly = counterVolume.Volume;
                        break;
                    case 8:
                        counterData.MonthAugust = counterVolume.Volume;
                        break;
                    case 9:
                        counterData.MonthSeptember = counterVolume.Volume;
                        break;
                    case 10:
                        counterData.MonthOctober = counterVolume.Volume;
                        break;
                    case 11:
                        counterData.MonthNovember = counterVolume.Volume;
                        break;
                    case 12:
                        counterData.MonthDecember = counterVolume.Volume;
                        break;
                    default:
                        break;
                }
            }

            return counterData;
        }

        /// <summary>
        /// Transmission des valeurs calculées au contexte JS
        /// </summary>
        private void AddToJavaScript()
        {
            #region Constantes et code fixe
            AddCallBackScript(" if( typeof nsAdminDashboard != 'undefined' && nsAdminDashboard != null && typeof nsAdminDashboard.init == 'function' ) { nsAdminDashboard.init('USROPT_MODULE_ADMIN_DASHBOARD');}");
            #endregion

            #region Transmission des valeurs au JavaScript

            // stockage
            AddStorageValueToJS("databaseAttachments", String.Empty, _sizeDatabaseAttachmentsDisplay);
            AddStorageValueToJS("attachments", String.Empty, _sizeAttachmentsDisplay);
            AddStorageValueToJS("database", String.Empty, _sizeDatabaseAttachments - _sizeAttachments);
            AddStorageValueToJS("available", String.Empty, _sizeAvailableStorage);
            AddStorageValueToJS("used", String.Empty, _sizeDatabaseAttachmentsDisplay); // même valeur que database + attachments
            AddStoragePercentToJS("used", String.Empty, (_sizeAvailableStorage > 0 ? _sizeDatabaseAttachments / _sizeAvailableStorage * 100 : 0)); // même valeur que database + attachments

            //envoi mail - valeur
            AddMailValueToJS("sent", "single", _counterMail); // mail unitaire
            AddMailValueToJS("sent", "campaign", _counterMailCampaign); //mail campagne
            AddMailValueToJS("sent", "system", _counterMailSystem); // systeme
            AddMailValueToJS("sent", "total", String.Empty, _counterMailTotal.YearTotal); // total
            AddMailValueToJS("available", String.Empty, String.Empty, _nBalanceMail); //balance

            //Envoi mail - %
            AddMailPercentToJS("sent", "single", "total", (_counterMailTotal.YearTotal != 0) ? ((_counterMail.YearTotal / _counterMailTotal.YearTotal) * 100) : 100); // % unitaire
            AddMailPercentToJS("sent", "campaign", "total", (_counterMailTotal.YearTotal != 0) ? (((_counterMailCampaign.YearTotal / _counterMailTotal.YearTotal) * 100)) : 100); // % campagne
            AddMailPercentToJS("sent", "system", "total", (_counterMailTotal.YearTotal != 0) ? ((_counterMailSystem.YearTotal / _counterMailTotal.YearTotal) * 100) : 100); // % system
            AddMailPercentToJS("sent", "total", String.Empty, _counterMailTotalPurchase > 0 ? _counterMailTotal.YearTotal / _counterMailTotalPurchase : 100); // ?



            // Envoi SMS - Valeur
            AddSMSValueToJS("sent", "single", _counterSMS);
            AddSMSValueToJS("sent", "campaign", _counterSMSCampaign);
            AddSMSValueToJS("sent", "total", String.Empty, _counterSMSTotal.YearTotal);
            AddSMSValueToJS("available", String.Empty, String.Empty, _nBalanceSMS);

            // Envoi SMS  %
            AddSMSPercentToJS("sent", "single", "total", (_counterSMSTotal.YearTotal != 0) ? ((_counterSMS.YearTotal / _counterSMSTotal.YearTotal) * 100) : 100);
            AddSMSPercentToJS("sent", "campaign", "total", (_counterSMSTotal.YearTotal != 0) ? ((_counterSMSCampaign.YearTotal / _counterSMSTotal.YearTotal) * 100) : 100);
            AddSMSPercentToJS("sent", "total", String.Empty, _counterSMSTotalPurchase > 0 ? _counterSMSTotal.YearTotal / _counterSMSTotalPurchase * 100 : 100);  //?


            //Achat Mail - valeur
            AddMailValueToJS("acquired", "bought", _acquiredMailBought); //acheté
            AddMailValueToJS("acquired", "credited", _acquiredMailCredited); //crédité (??)
            AddMailValueToJS("acquired", "intervention", _acquiredMailIntervention); // intervention (??)

            //Achat Mail - %
            AddMailPercentToJS("acquired", "bought", "total", (_counterMailTotalPurchase > 0 && _counterMailTotal.YearTotal != 0) ? ((_acquiredMailBought.YearTotal / _counterMailTotal.YearTotal) * 100) : 100);
            AddMailPercentToJS("acquired", "credited", "total", _counterMailTotalPurchase > 0 ? _acquiredMailCredited.YearTotal / _counterMailTotalPurchase * 100 : 100);
            AddMailPercentToJS("acquired", "intervention", "total", _counterMailTotalPurchase > 0 ? _acquiredMailIntervention.YearTotal / _counterMailTotalPurchase * 100 : 100);

            //Achat SMS - valeur
            AddSMSValueToJS("acquired", "bought", _acquiredSMSBought);
            AddSMSValueToJS("acquired", "credited", _acquiredSMSCredited);
            AddSMSValueToJS("acquired", "intervention", _acquiredSMSIntervention);

            //Achat SMS - %
            AddSMSPercentToJS("acquired", "bought", "total", _counterSMSTotalPurchase > 0 ? _acquiredSMSBought.YearTotal / _counterSMSTotalPurchase * 100 : 100);
            AddSMSPercentToJS("acquired", "credited", "total", _counterSMSTotalPurchase > 0 ? _acquiredSMSCredited.YearTotal / _counterSMSTotalPurchase * 100 : 100);
            AddSMSPercentToJS("acquired", "intervention", "total", _counterSMSTotalPurchase > 0 ? _acquiredSMSIntervention.YearTotal / _counterSMSTotalPurchase * 100 : 100);
            #endregion

            #region Et initialisation côté JS
            AddCallBackScript(String.Concat("nsAdminDashboard.warningThreshold = ", _warningThreshold, ";"));
            AddCallBackScript(String.Concat("nsAdminDashboard.criticalThreshold = ", _criticalThreshold, ";"));
            AddCallBackScript(String.Concat("top._CombinedZ = '", eLibConst.COMBINED_Z, "';"));
            AddCallBackScript(String.Concat("top._CombinedY = '", eLibConst.COMBINED_Y, "';"));
            AddCallBackScript("Odometer.init();");
            AddCallBackScript(String.Concat("Odometer.options = Object.assign(Odometer.options, { selector: '.odometer', format: '(", Pref.NumberSectionsDelimiter, "ddd)", Pref.NumberDecimalDelimiter, "d'});"));
            AddCallBackScript("nsAdminDashboard.setNumericValues();");
            AddCallBackScript("nsAdminDashboard.displayCharts('USROPT_MODULE_ADMIN_DASHBOARD');");
            AddCallBackScript("nsAdminDashboard.updateProgressDescriptionTooltips();");
            #endregion
        }

        /// <summary>
        /// On instancie le container 
        /// </summary>
        private void InitInnerContainer()
        {
            _pnlContents = new Panel();
            _pnlContents.CssClass = "adminCntnt adminCntntDashboard";
            _pnlContents.Style.Add(HtmlTextWriterStyle.Width, String.Concat(_width, "px"));
            _pnlContents.Style.Add(HtmlTextWriterStyle.Height, String.Concat(_height.ToString(), "px"));

            _pnlContentsLicenseOffer = new Panel();
            _pnlContentsLicenseOffer.ID = "content-licenseoffer";
            _pnlContents.Controls.Add(_pnlContentsLicenseOffer);

            // Ajout d'un Panel spécifique pour la volumétrie pour y appliquer des CSS spécifiques par sélecteur d'ID #
            _pnlContentsVolume = new Panel();
            _pnlContentsVolume.ID = "content-volumetrie";
            _pnlContents.Controls.Add(_pnlContentsVolume);

            _pgContainer.Controls.Add(_pnlContents);
        }

        /// <summary>
        /// Gets the renderer contents.
        /// </summary>
        /// <returns></returns>
        private Boolean GetRendererContents()
        {
            Panel targetPanel = null;
            Panel panelLicense = null;
            HtmlGenericControl label = null;

            #region Licence
            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD_OFFERMANAGER.ToString(), eResApp.GetRes(Pref, 8206));
            _pnlContentsLicenseOffer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            // Phrase à afficher
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(eResApp.GetRes(Pref, 8203), _client.ClientOffer.ToString(), _client.ClientProduct.ToString());
            sb.Append("<br />");
            if (_client.NbPaidSubscriptions > 0)
            {
                sb.AppendFormat(eResApp.GetRes(Pref, 8204), _client.NbPaidSubscriptions.ToString());
                if (_client.NbFreeSubscriptions > 0)
                {
                    sb.AppendFormat(" " + eResApp.GetRes(Pref, 8205), _client.NbFreeSubscriptions.ToString());
                }
                else
                {
                    sb.Append(".");
                }
            }

            // Offre de la base
            panelLicense = new Panel();
            panelLicense.CssClass = "panelOffer";
            label = new HtmlGenericControl("label");
            label.Attributes.Add("class", "info");
            label.ID = "labelOfferInfo";
            label.InnerHtml = sb.ToString();
            panelLicense.Controls.Add(label);
            AddButtonOptionField(panelLicense, "btnDiscover", eResApp.GetRes(Pref, 8201), "", onClick: "nsAdmin.sendInfoRequest();");
            targetPanel.Controls.Add(panelLicense);

            // Clé de licence
            panelLicense = new Panel();
            panelLicense.CssClass = "panelOffer";
            AddTextboxOptionField(panelLicense, "txtLicenseKey", eResApp.GetRes(Pref, 8202), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.LICENSEKEY.GetHashCode(), typeof(eLibConst.CONFIGADV), Pref.ClientInfos.LicenseKey,
                EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, customPanelCSSClasses: "fieldInline",
                readOnly: Pref.User.UserLevel < UserLevel.LEV_USR_SUPERADMIN.GetHashCode());
            //AddButtonOptionField(panelLicense, "btnUpgrade", "Demander l'offre supérieure", "");
            targetPanel.Controls.Add(panelLicense);
            #endregion

            #region Volumétrie (US #411 et #412)

            #region Rendu

            section = GetModuleSection(String.Concat(eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD_VOLUME.ToString()), eResApp.GetRes(_ePref, 2330));
            _pnlContentsVolume.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            if (_displayStoragePanel)
            {
                Panel cardBodyContainer = new Panel();
                Panel mainDashboardCardContainer = GetDashboardSection(
                    String.Empty, 12, "info",
                    eResApp.GetRes(_ePref, 2333),
                    String.Format(eResApp.GetRes(Pref, 2334), eDate.ConvertBddToDisplay(Pref.CultureInfo, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0), false, true, true, true)),
                    "box-main box-chart", out cardBodyContainer);

                if (_baseInfoErrors.Trim().Length == 0 && eLibTools.GetServerConfig("ServerWithoutDBSizeInfos", "0") == "0")
                {
                    Panel cardBodyColContainer = new Panel();
                    cardBodyColContainer.CssClass = "col-md-5";
                    cardBodyColContainer.Controls.Add(
                        GetDashboardCard(
                            0, "annex", String.Empty, String.Concat("bg-", _colorAttachments), eResApp.GetRes(Pref, 2332), String.Empty,
                            true, /*_sizeAttachments*/ 0, _decimalCount, _valueAttachmentsUnit, "NumAnnexes",
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty, String.Empty,
                            true
                        )
                    ); // Taille des annexes
                    cardBodyColContainer.Controls.Add(
                        GetDashboardCard(
                            0, "bar-chart-o", String.Empty, String.Concat("bg-", _colorDatabaseAttachments), eResApp.GetRes(Pref, 2331), String.Empty,
                            true, /*_sizeDatabaseAttachments*/ 0, _decimalCount, _valueDatabaseAttachmentsUnit, "NumAnnexesEtBDD",
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty, String.Empty,
                            true
                        )
                    ); // Taille de la base et des annexes
                    cardBodyColContainer.Controls.Add(
                        GetDashboardCard(
                            0, "database2", String.Empty, String.Concat("bg-", _colorAvailableStorage), eResApp.GetRes(Pref, 2333), String.Empty,
                            true, /*_sizeAvailableStorage*/ 0, _decimalCount, _valueAvailableStorageUnit, "NumDispo",
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty, String.Empty,
                            true
                        )
                    ); // Capacité disponible
                    cardBodyColContainer.Controls.Add(GetDashboardCard(
                        0, "database2", String.Concat("bg-", _colorAvailableStorage), String.Empty, eResApp.GetRes(_ePref, 2353), String.Empty,
                        true, /*_sizeDatabaseAttachments*/ 0, _decimalCount, _valueDatabaseAttachmentsUnit, "NumCapaUtilise",
                        true, /*_sizeAvailableStorage*/ 0, _decimalCount, _valueAvailableStorageUnit, "NumDispoBDDUtilise",
                        true, null, _decimalCount, null, "PercCapaUtilise", eResApp.GetRes(_ePref, 2339),
                        false
                    )); // pourcentage du total de la capacité disponible

                    Panel targetChartContainer = new Panel();
                    Panel cardBodyColChartContainer = GetDashboardChartCard(String.Empty, 12, String.Empty, eResApp.GetRes(_ePref, 2330), String.Empty, "circularGauge", out targetChartContainer);

                    cardBodyContainer.Controls.Add(cardBodyColContainer);
                    cardBodyContainer.Controls.Add(cardBodyColChartContainer);
                }
                else if (eLibTools.GetServerConfig("ServerWithoutDBSizeInfos", "0") != "0")
                {
                    cardBodyContainer.Controls.Add(GetDashboardNoBDDSizeInfos(eResApp.GetRes(Pref, 2361)));
                }
                else if (_baseInfoErrors.Trim().Length != 0)
                {
                    cardBodyContainer.Controls.Add(
                        GetDashboardCard(
                            8, "warning", "normal-wrap", "bg-red", _baseInfoErrors, String.Empty,
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty, String.Empty,
                            true
                        )
                    );
                }
                targetPanel.Controls.Add(mainDashboardCardContainer);
            }

            #region Entete sélection de l'année
            targetPanel.Controls.Add(GetDashboardYearSelect());
            #endregion

            if (_displayMailSentPanel || _displayMailAcquiredPanel)
            {
                Panel cardBodyContainer = new Panel();
                Panel mainDashboardCardContainer = GetDashboardSection(
                    "row-md-2-3", 12, "teal", eResApp.GetRes(_ePref, 2366), eResApp.GetRes(_ePref, 1395),
                    "box-main box-chart", out cardBodyContainer);

                if (_mailSMSInfoErrors.Trim().Length == 0)
                {
                    if (_displayMailSentPanel)
                    {
                        cardBodyContainer.Controls.Add(GetDashboardTotalCardWithTowText(
                            12, String.Concat("bg-", _colorMailTotalBox), String.Concat("text-", _colorMailTotal), eResApp.GetRes(_ePref, 2348).Replace("<YEAR>", _counterYear.ToString()),
                            true, _counterMailTotal.YearTotal, _decimalCount, String.Empty, "NumEmailSent",
                            false, eResApp.GetRes(_ePref, 2705).Replace("<DATE>", DateTime.Now.ToString(format: "d")), _nBalanceMail
                        )); // xx% - xx / yy e-mails envoyés
                        cardBodyContainer.Controls.Add(GetDashboardCard(
                            4, "email", String.Concat("bg-", _colorMailCampaign), String.Empty, String.Empty, String.Empty,
                            true,/* _counterMail.YearTotal*/ 0, _decimalCount, String.Format(eResApp.GetRes(_ePref, 2340), eResApp.GetRes(_ePref, 2351)), "NumEmailUnitSent",
                            false, /*_counterMailTotalAvailable*/ 0, _decimalCount, String.Empty, "NumEmailUnitTotal",
                            true, _counterMailTotal.YearTotal > 0 ? ((_counterMail.YearTotal / _counterMailTotal.YearTotal) * 100) : 1, _decimalCount, null, "PercEmailUnitSent", eResApp.GetRes(_ePref, 2713),
                            false
                        )); // du nombre total d'e-mails disponible
                        cardBodyContainer.Controls.Add(GetDashboardCard(
                            4, "email", String.Concat("bg-", _colorMail), String.Empty, String.Empty, String.Empty,
                            true, /*_counterMailCampaign.YearTotal*/ 0, _decimalCount, String.Format(eResApp.GetRes(_ePref, 2340), eResApp.GetRes(_ePref, 2350)), "NumEmailCampaignSent",
                            false, /*_counterMailTotalAvailable*/ 0, _decimalCount, String.Empty, "NumEmailCampaignTotal",
                            true, _counterMailTotal.YearTotal > 0 ? ((_counterMailCampaign.YearTotal / _counterMailTotal.YearTotal) * 100) : 1, _decimalCount, null, "PercEmailCampaignSent", eResApp.GetRes(_ePref, 2713),
                            false
                        )); // du nombre total d'e-mails disponible
                        cardBodyContainer.Controls.Add(GetDashboardCard(
                            4, "email", String.Concat("bg-", _colorMailSystem), String.Empty, String.Empty, String.Empty,
                            true, /*_counterMailSystem.YearTotal*/ 0, _decimalCount, String.Format(eResApp.GetRes(_ePref, 2340), eResApp.GetRes(_ePref, 2352)), "NumEmailSystemSent",
                            false, /*_counterMailTotalAvailable*/ 0, _decimalCount, String.Empty, "NumEmailSystemTotal",
                            true, _counterMailTotal.YearTotal > 0 ? ((_counterMailSystem.YearTotal / _counterMailTotal.YearTotal) * 100) : 1, _decimalCount, null, "PercEmailSystemSent", eResApp.GetRes(_ePref, 2713),
                            false
                        )); // du nombre total d'e-mails disponible

                        Panel targetChartSentContainer = new Panel();
                        Panel cardBodyColChartSentContainer = GetDashboardChartCard(String.Empty, 12, String.Empty, eResApp.GetRes(_ePref, 2344), String.Empty, "emailBarChartEnvoyer", out targetChartSentContainer);
                        targetChartSentContainer.Style.Add(HtmlTextWriterStyle.TextAlign, "center");

                        cardBodyContainer.Controls.Add(cardBodyColChartSentContainer);
                    }

                    if (_displayMailAcquiredPanel)
                    {
                        Panel targetChartAcquiredContainer = new Panel();
                        HtmlGenericControl divContainerPurchased = new HtmlGenericControl("div");
                        cardBodyContainer.Controls.Add(divContainerPurchased);
                        divContainerPurchased.Attributes.Add("class", "purchased-container");
                        divContainerPurchased.Controls.Add(GetDashboardCard(
                            4, "email", string.Concat("bg-", _colorMail), string.Empty, string.Empty, string.Empty,
                            true, 0, _decimalCount, eResApp.GetRes(_ePref, 2485), "NumEmailBought",
                            false, 0, _decimalCount, string.Empty, "NumEmailBought",
                            true, 1,
                            _decimalCount, null, "PercEmailBought", eResApp.GetRes(_ePref, 2486),
                            false));

                        Panel cardBodyColChartAcquiredContainer = GetDashboardChartCard(String.Empty, 12,
                            String.Empty, eResApp.GetRes(_ePref, 2345), String.Empty, "emailBarChartAchat", out targetChartAcquiredContainer);
                        targetChartAcquiredContainer.Style.Add(HtmlTextWriterStyle.TextAlign, "center");



                        cardBodyContainer.Controls.Add(cardBodyColChartAcquiredContainer);
                    }
                }
                else
                {
                    cardBodyContainer.Controls.Add(
                        GetDashboardCard(
                            6, "warning", "red", "bg-red", _mailSMSInfoErrors, String.Empty,
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty, String.Empty,
                            true
                        )
                    );
                }

                targetPanel.Controls.Add(mainDashboardCardContainer);
            }

            if (_displaySMSSentPanel || _displaySMSAcquiredPanel)
            {
                Panel cardBodyContainer = new Panel();
                Panel mainDashboardCardContainer = GetDashboardSection(
                    "row-md-2-3", 12, "blue", eResApp.GetRes(_ePref, 655), eResApp.GetRes(_ePref, 1395),
                    "box-main box-chart", out cardBodyContainer, eResApp.GetRes(_ePref, 2434));

                if (_mailSMSInfoErrors.Trim().Length == 0)
                {
                    if (_displaySMSSentPanel)
                    {
                        cardBodyContainer.Controls.Add(GetDashboardTotalCardWithTowText(
                           12, String.Concat("bg-", _colorSMSTotalBox), String.Concat("text-", _colorSMSTotalBox), eResApp.GetRes(_ePref, 2349).Replace("<YEAR>", _counterYear.ToString()),
                           true, _counterSMSTotal.YearTotal, _decimalCount, String.Empty, "NumSmsSent",
                           false, eResApp.GetRes(_ePref, 2708).Replace("<DATE>", DateTime.Now.ToString(format: "d")), _nBalanceSMS
                       ));// xx% - xx / yy SMS envoyés
                        cardBodyContainer.Controls.Add(GetDashboardCard(
                                4, "sms", String.Concat("bg-", _colorSMSCampaign), String.Empty, String.Empty, String.Empty,
                                true, _counterSMS.YearTotal, _decimalCount, String.Format(eResApp.GetRes(_ePref, 2341), eResApp.GetRes(_ePref, 2350)), "NumSmsCampaignSent",
                                false, _counterSMSTotal.YearTotal > 0 ? ((_counterSMS.YearTotal / _counterSMSTotal.YearTotal) * 100) : 1, _decimalCount, String.Empty, "NumSmsCampaignTotal",
                                true, null, _decimalCount, null, "PercSmsCampaignSent", eResApp.GetRes(_ePref, 2714),
                                false
                            )); // du nombre total de SMS envoyé

                        cardBodyContainer.Controls.Add(GetDashboardCard(
                            4, "sms", String.Concat("bg-", _colorSMS), String.Empty, String.Empty, String.Empty,
                            true, _counterSMSCampaign.YearTotal, _decimalCount, String.Format(eResApp.GetRes(_ePref, 2341), eResApp.GetRes(_ePref, 2351)), "NumSmsUnitSent",
                            false, _counterSMSTotal.YearTotal > 0 ? ((_counterSMSCampaign.YearTotal / _counterSMSTotal.YearTotal) * 100) : 1, _decimalCount, String.Empty, "NumSmsUnitTotal",
                            true, null, _decimalCount, null, "PercSmsUnitSent", eResApp.GetRes(_ePref, 2714),
                            false
                        )); // du nombre total de SMS envoyé


                        Panel targetChartSentContainer = new Panel();
                        Panel cardBodyColChartSentContainer = GetDashboardChartCard(String.Empty, 12, String.Empty, eResApp.GetRes(_ePref, 2346), String.Empty, "smsBarChartEnvoyer", out targetChartSentContainer);
                        targetChartSentContainer.Style.Add(HtmlTextWriterStyle.TextAlign, "center");

                        cardBodyContainer.Controls.Add(cardBodyColChartSentContainer);
                    }

                    if (_displaySMSAcquiredPanel)
                    {
                        Panel targetChartAcquiredContainer = new Panel();
                        HtmlGenericControl divContainerPurchased = new HtmlGenericControl("div");
                        cardBodyContainer.Controls.Add(divContainerPurchased);
                        divContainerPurchased.Attributes.Add("class", "purchased-container");
                        divContainerPurchased.Controls.Add(GetDashboardCard(
                            4, "sms", string.Concat("bg-", _colorSMS), string.Empty, string.Empty, string.Empty,
                            true, 0, _decimalCount, eResApp.GetRes(_ePref, 2487), "NumSMSBought",
                            false, 0, _decimalCount, string.Empty, "NumSMSBought", true, _counterSMSTotalPurchase > 0 ? _acquiredSMSBought.YearTotal / _counterSMSTotalPurchase : 1, _decimalCount, null, "PercSMSBought",
                            eResApp.GetRes(_ePref, 2488),
                            false));


                        Panel cardBodyColChartAcquiredContainer = GetDashboardChartCard(String.Empty, 12, String.Empty, eResApp.GetRes(_ePref, 2347), String.Empty, "smsBarChartAchat", out targetChartAcquiredContainer);
                        targetChartAcquiredContainer.Style.Add(HtmlTextWriterStyle.TextAlign, "center");

                        cardBodyContainer.Controls.Add(cardBodyColChartAcquiredContainer);
                    }
                }
                else
                {
                    cardBodyContainer.Controls.Add(
                        GetDashboardCard(
                            6, "warning", "red", "bg-red", _mailSMSInfoErrors, String.Empty,
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty,
                            false, 0, 0, String.Empty, String.Empty, String.Empty,
                            true
                        )
                    );
                }

                targetPanel.Controls.Add(mainDashboardCardContainer);
            }

            #endregion

            #endregion

            return true;
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueCategory">Catégorie de la valeur : "available", "database", etc.</param>
        /// <param name="valueSubCategory">Sous-Catégorie de la valeur : "jan", "fev", "mar", ...  ou "total"</param>
        /// <param name="value"></param>
        private void AddStorageValueToJS(string valueCategory, string valueSubCategory, double value)
        {
            AddVariableToJS(new string[] { "values", "storage", valueCategory }, valueSubCategory, value);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="valueSubCategory">Sous-Catégorie de la valeur : "jan", "fev", "mar", ...  ou "total"</param>
        /// <param name="value"></param>
        private void AddStoragePercentToJS(string valueCategory, string valueSubCategory, double value)
        {
            AddVariableToJS(new string[] { "percent", "storage", valueCategory }, valueSubCategory, value);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="valueSubCategory">Sous-Catégorie de la valeur : "jan", "fev", "mar", ...  ou "total"</param>
        /// <param name="value"></param>
        private void AddMailValueToJS(string valueSubDomain, string valueCategory, string valueSubCategory, double value)
        {
            AddVariableToJS(new string[] { "values", "mail", valueSubDomain, valueCategory }, valueSubCategory, value);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="valueSubCategory">Sous-Catégorie de la valeur : "jan", "fev", "mar", ...  ou "total"</param>
        /// <param name="value"></param>
        private void AddMailPercentToJS(string valueSubDomain, string valueCategory, string valueSubCategory, double value)
        {
            AddVariableToJS(new string[] { "percent", "mail", valueSubDomain, valueCategory }, valueSubCategory, value);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="valueSubCategory">Sous-Catégorie de la valeur : "jan", "fev", "mar", ...  ou "total"</param>
        /// <param name="value"></param>
        private void AddSMSValueToJS(string valueSubDomain, string valueCategory, string valueSubCategory, double value)
        {
            AddVariableToJS(new string[] { "values", "sms", valueSubDomain, valueCategory }, valueSubCategory, value);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="valueSubCategory">Sous-Catégorie de la valeur : "jan", "fev", "mar", ...  ou "total"</param>
        /// <param name="value"></param>
        private void AddSMSPercentToJS(string valueSubDomain, string valueCategory, string valueSubCategory, double value)
        {
            AddVariableToJS(new string[] { "percent", "sms", valueSubDomain, valueCategory }, valueSubCategory, value);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="values">Valeurs de compteur</param>
        private void AddMailValueToJS(string valueSubDomain, string valueCategory, CounterData values)
        {
            AddVariableToJS(new string[] { "values", "mail", valueSubDomain, valueCategory }, values);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="values">Valeurs de compteur</param>
        private void AddMailPercentToJS(string valueSubDomain, string valueCategory, CounterData values)
        {
            AddVariableToJS(new string[] { "percent", "mail", valueSubDomain, valueCategory }, values);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="values">Valeurs de compteur</param>
        private void AddSMSValueToJS(string valueSubDomain, string valueCategory, CounterData values)
        {
            AddVariableToJS(new string[] { "values", "sms", valueSubDomain, valueCategory }, values);
        }

        /// <summary>
        /// Transmet une valeur Pourcentage calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="valueSubDomain">Sous-Domaine de la valeur : "sent" ou "acquired"</param>
        /// <param name="valueCategory">Catégorie de la valeur : "campaign", "single", "system"</param>
        /// <param name="values">Valeurs de compteur</param>
        private void AddSMSPercentToJS(string valueSubDomain, string valueCategory, CounterData values)
        {
            AddVariableToJS(new string[] { "percent", "sms", valueSubDomain, valueCategory }, values);
        }

        /// <summary>
        /// Transmet les valeurs calculées d'un compteur dans un ensemble de variables JavaScript indexées pour affichage des tableaux de bord
        /// </summary>
        /// <param name="variableMembers">Membres de la variable JS, qui seront construits sous la forme membre1.membre2.membre3... =</param>
        /// <param name="variableValues">Valeurs de compteur</param>
        private void AddVariableToJS(string[] variableMembers, CounterData variableValues)
        {
            AddVariableToJS(variableMembers, "jan", variableValues.MonthJanuary);
            AddVariableToJS(variableMembers, "feb", variableValues.MonthFebruary);
            AddVariableToJS(variableMembers, "mar", variableValues.MonthMarch);
            AddVariableToJS(variableMembers, "apr", variableValues.MonthApril);
            AddVariableToJS(variableMembers, "may", variableValues.MonthMay);
            AddVariableToJS(variableMembers, "jun", variableValues.MonthJune);
            AddVariableToJS(variableMembers, "jul", variableValues.MonthJuly);
            AddVariableToJS(variableMembers, "aug", variableValues.MonthAugust);
            AddVariableToJS(variableMembers, "sep", variableValues.MonthSeptember);
            AddVariableToJS(variableMembers, "oct", variableValues.MonthOctober);
            AddVariableToJS(variableMembers, "nov", variableValues.MonthNovember);
            AddVariableToJS(variableMembers, "dec", variableValues.MonthDecember);
            AddVariableToJS(variableMembers, "total", variableValues.YearTotal);
            AddVariableToJS(variableMembers, "maximum", variableValues.YearMaximum);
        }

        /// <summary>
        /// Transmet une valeur calculée dans une variable JavaScript pour affichage des tableaux de bord
        /// </summary>
        /// <param name="variableMembers">Membres de la variable JS, qui seront construits sous la forme membre1.membre2.membre3... =</param>
        /// <param name="variableKey">Clé de tableau de la variable JS : "jan", "fev", "mar", ...  ou "total"</param>
        /// <param name="variableValue">Valeur de la variable</param>
        private void AddVariableToJS(string[] variableMembers, string variableKey, double variableValue)
        {
            AddCallBackScript(
                String.Concat(
                    "nsAdminDashboard.",
                    String.Join(".", variableMembers.Where(x => !String.IsNullOrEmpty(x)).ToArray()),
                    !String.IsNullOrEmpty(variableKey) ? String.Concat("['", variableKey, "']") : String.Empty,
                    " = ",
                    Double.IsNaN(variableValue) ? "0" : variableValue.ToString().Replace(",", "."),
                    ";"
                )
            ); ;
        }/// <summary>
         /// Renvoie la tuile à afficher dans le tableau de bord lorsqu'il n'y a pas de d'infos sur la base
         /// </summary>
         /// <returns></returns>
        private Panel GetDashboardNoBDDSizeInfos(string sLabel)
        {
            Panel mainContainer = new Panel();
            mainContainer.Attributes.Add("class", "info-header");

            HtmlGenericControl divContainer = new HtmlGenericControl("div");
            divContainer.Attributes.Add("class", "callout callout-noinfo");
            mainContainer.Controls.Add(divContainer);

            HtmlGenericControl label = new HtmlGenericControl("h4");
            label.InnerText = sLabel;
            divContainer.Controls.Add(label);

            return mainContainer;
        }

        /// <summary>
        /// Renvoie la tuile à afficher dans le tableau de bord, contenant la selection de l'année
        /// </summary>
        /// <returns></returns>
        private Panel GetDashboardYearSelect()
        {
            Panel mainContainer = new Panel();
            mainContainer.Attributes.Add("class", "info-header");

            HtmlGenericControl divContainer = new HtmlGenericControl("div");
            divContainer.Attributes.Add("class", "callout callout-info");
            mainContainer.Controls.Add(divContainer);

            HtmlGenericControl label = new HtmlGenericControl("h4");
            label.InnerText = eResApp.GetRes(Pref, 2362);
            divContainer.Controls.Add(label);

            HtmlGenericControl divSelectContainer = new HtmlGenericControl("div");
            divSelectContainer.Attributes.Add("id", "extension-ui");
            divContainer.Controls.Add(divSelectContainer);

            HtmlGenericControl divCustomSelect = new HtmlGenericControl("div");
            divCustomSelect.Attributes.Add("class", "custom-select");
            divSelectContainer.Controls.Add(divCustomSelect);

            HtmlGenericControl select = new HtmlGenericControl("select");
            divCustomSelect.Controls.Add(select);

            int startYear = DateTime.Now.Year;
            int endYear = startYear > 2019 ? startYear - 2 : startYear; //Les stats ne remontent pas avant 2019, année de mise en place du dashboard
            if (endYear > _counterYear)
                endYear = _counterYear;
            for (int year = startYear; year >= endYear; --year)
            {
                HtmlGenericControl option = new HtmlGenericControl("option");
                option.Attributes.Add("value", year.ToString());
                option.InnerText = year.ToString();

                if (year == _counterYear)
                    option.Attributes.Add("selected", "selected");

                select.Controls.Add(option);
            }


            HtmlGenericControl spanArrow = new HtmlGenericControl("span");
            spanArrow.Attributes.Add("class", "pull-right-container");
            divCustomSelect.Controls.Add(spanArrow);

            HtmlGenericControl italic = new HtmlGenericControl("i");
            italic.Attributes.Add("class", "icon-chevron-down");
            spanArrow.Controls.Add(italic);

            return mainContainer;
        }

        /// <summary>
        /// Renvoie une tuile à afficher dans le tableau de bord
        /// Créé pour la US #411 et #412 concernant la volumétrie
        /// </summary>
        /// <param name="widthRatio">Indice de taille de la tuile (1 = petite, 12 = très large)</param>
        /// <param name="icon">Icône</param>
        /// <param name="cardColorClass">Classe de couleur pour le conteneur de la tuile</param>
        /// <param name="containerColorClass">Classe de couleur pour le fond de l'icône</param>
        /// <param name="label">Libellé de la tuile</param>
        /// <param name="value">Valeur de la tuile</param>
        /// <returns>Panneau <div> représentant la tuile</div></returns>
        private Panel GetDashboardCard(int widthRatio, string icon, string cardColorClass, string containerColorClass, string label, string value)
        {
            return GetDashboardCard(
                widthRatio, icon, cardColorClass, containerColorClass, label, value,
                false, 0, 0, String.Empty, String.Empty,
                false, 0, 0, String.Empty, String.Empty,
                false, 0, 0, String.Empty, String.Empty, String.Empty,
                false);
        }

        /// <summary>
        /// Renvoie une tuile à afficher dans le tableau de bord
        /// Créé pour la US #411 et #412 concernant la volumétrie
        /// </summary>
        /// <param name="widthRatio">Indice de taille de la tuile (1 = petite, 12 = très large)</param>
        /// <param name="icon">Icône</param>
        /// <param name="cardColorClass">Classe de couleur pour le conteneur de la tuile</param>
        /// <param name="containerColorClass">Classe de couleur pour le fond de l'icône</param>
        /// <param name="label">Libellé de la tuile</param>
        /// <param name="textValue">Valeur de la tuile</param>
        /// <param name="displayNumericValue">Afficher la valeur numérique ?</param>
        /// <param name="numericValue">Valeur numérique</param>
        /// <param name="numericValueDecimalCount">Nombre de décimales pour la valeur numérique</param>
        /// <param name="numericValueUnit">Unité de la valeur numérique</param>
        /// <param name="numericValueContainerId">Identifiant HTML du conteneur de la valeur numérique</param>
        /// <param name="displayTotalValue">Afficher la valeur totale ?</param>
        /// <param name="totalValue">Valeur totale</param>
        /// <param name="totalValueDecimalCount">Nombre de décimales pour la valeur totale</param>
        /// <param name="totalValueUnit">Unité de la valeur totale</param>
        /// <param name="totalValueContainerId">Identifiant HTML du conteneur de la valeur totale</param>
        /// <param name="displayProgressBar">Afficher la barre de progression ?</param>
        /// <param name="progressValue">Valeur de la barre de progression. Peut être null, auquel cas elle sera calculée à partir de la valeur et du total</param>
        /// <param name="progressValueDecimalCount">Nombre de décimales pour la valeur de la barre de progression</param>
        /// <param name="progressValueUnit">Unité de la valeur de la barre de progression. Si null = %</param>
        /// <param name="progressValueContainerId">Identifiant HTML du conteneur de la valeur de la barre de progression</param>
        /// <param name="progressDescription">Texte à afficher en descriptif de la barre de progression, à la suite des valeurs</param>
        /// <param name="displaySmallUnits">Indique si les unités doivent être affichées avec une balise small (si false = span)</param>
        /// <returns>Panneau <div> représentant la tuile</div></returns>
        private Panel GetDashboardCard(
            int widthRatio, string icon, string cardColorClass, string containerColorClass,
            string label, string textValue,
            bool displayNumericValue, double numericValue, int numericValueDecimalCount, string numericValueUnit, string numericValueContainerId,
            bool displayTotalValue, double totalValue, int totalValueDecimalCount, string totalValueUnit, string totalValueContainerId,
            bool displayProgressBar, double? progressValue, int progressValueDecimalCount, string progressValueUnit, string progressValueContainerId, string progressDescription,
            bool displaySmallUnits
            )
        {
            Panel mainContainer = new Panel();
            if (widthRatio > 0)
            {
                mainContainer.CssClass = String.Concat("col-md-", widthRatio);
            }

            Panel infoBoxContainer = new Panel();
            infoBoxContainer.CssClass = String.Concat("info-box ", cardColorClass);

            HtmlGenericControl infoBoxIconContainer = new HtmlGenericControl("span");
            infoBoxIconContainer.Attributes.Add("class", String.Concat("info-box-icon ", containerColorClass));

            HtmlGenericControl infoBoxIcon = new HtmlGenericControl("i");
            infoBoxIcon.Attributes.Add("class", String.Concat("icon-", icon));

            Panel infoBoxContent = new Panel();
            infoBoxContent.CssClass = "info-box-content";

            HtmlGenericControl infoBoxLabel = new HtmlGenericControl("span");
            string infoBoxLabelclass = "info-box-text";
            infoBoxLabel.InnerHtml = label;
            if (String.IsNullOrEmpty(label))
                infoBoxLabelclass = String.Concat(infoBoxLabelclass, " emptyLabel");
            infoBoxLabel.Attributes.Add("class", infoBoxLabelclass);


            HtmlGenericControl infoBoxValue = new HtmlGenericControl("span");
            infoBoxValue.Attributes.Add("class", "info-box-number");

            if (displayNumericValue)
            {
                HtmlGenericControl numericValueContainer = new HtmlGenericControl("span");
                numericValueContainer.ID = numericValueContainerId;
                numericValueContainer.Attributes.Add("class", _numericValuesClass);
                numericValueContainer.InnerText = numericValueDecimalCount > -1 ? Math.Round(numericValue, numericValueDecimalCount).ToString() : numericValue.ToString();
                HtmlGenericControl numericValueUnitContainer = new HtmlGenericControl(displaySmallUnits ? "small" : "span");
                numericValueUnitContainer.InnerText = String.Concat(" ", numericValueUnit);
                infoBoxValue.Controls.Add(numericValueContainer);
                infoBoxValue.Controls.Add(numericValueUnitContainer);

                if (displayTotalValue)
                {
                    HtmlGenericControl totalValueSeparatorContainer = new HtmlGenericControl("span");
                    totalValueSeparatorContainer.InnerText = " / ";
                    HtmlGenericControl totalValueContainer = new HtmlGenericControl("span");
                    totalValueContainer.ID = totalValueContainerId;
                    totalValueContainer.Attributes.Add("class", _numericValuesClass);
                    totalValueContainer.InnerText = totalValueDecimalCount > -1 ? Math.Round(totalValue, totalValueDecimalCount).ToString() : totalValue.ToString();
                    HtmlGenericControl totalValueUnitContainer = new HtmlGenericControl(displaySmallUnits ? "small" : "span");
                    totalValueUnitContainer.InnerText = String.Concat(" ", totalValueUnit);
                    infoBoxValue.Controls.Add(totalValueSeparatorContainer);
                    infoBoxValue.Controls.Add(totalValueContainer);
                    infoBoxValue.Controls.Add(totalValueUnitContainer);
                }
            }
            else
            {
                if (displayTotalValue)
                    infoBoxValue.InnerHtml = String.Concat(textValue, " / ", totalValue);
                else
                    infoBoxValue.InnerHtml = textValue;
            }

            infoBoxContent.Controls.Add(infoBoxLabel);
            infoBoxContent.Controls.Add(infoBoxValue);

            if (displayProgressBar)
            {
                double nonNullProgressValue = 0;
                if (progressValue == null || !progressValue.HasValue)
                {
                    if (totalValue > 0)
                        nonNullProgressValue = numericValue / totalValue * 100;
                    else
                        nonNullProgressValue = numericValue; // ou 0 ?
                }
                else
                    nonNullProgressValue = (double)progressValue;

                infoBoxContent.CssClass = String.Concat(infoBoxContent.CssClass, " progress-info-box-content");
                Panel progressContainer = new Panel();
                progressContainer.CssClass = "progress";
                Panel progressBarContainer = new Panel();
                progressBarContainer.Style.Add(HtmlTextWriterStyle.Width, String.Concat(nonNullProgressValue, "%").Replace(",", "."));
                progressBarContainer.CssClass = "progress-bar";
                HtmlGenericControl progressDescriptionContainer = new HtmlGenericControl("span");
                progressDescriptionContainer.Attributes.Add("class", "progress-description");
                HtmlGenericControl progressValueContainer = new HtmlGenericControl("span");
                progressValueContainer.ID = progressValueContainerId;
                progressValueContainer.Attributes.Add("class", _numericValuesClass);
                progressValueContainer.InnerText = progressValueDecimalCount > -1 ? Math.Round(nonNullProgressValue, progressValueDecimalCount).ToString() : nonNullProgressValue.ToString();
                HtmlGenericControl progressPercentContainer = new HtmlGenericControl("span");
                progressPercentContainer.InnerText = !String.IsNullOrEmpty(progressValueUnit) ? String.Concat(progressValueUnit, " ") : "% ";
                HtmlGenericControl progressTextContainer = new HtmlGenericControl("span");
                progressTextContainer.InnerText = progressDescription;

                progressDescriptionContainer.Controls.Add(progressValueContainer);
                progressDescriptionContainer.Controls.Add(progressPercentContainer);
                progressDescriptionContainer.Controls.Add(progressTextContainer);
                progressContainer.Controls.Add(progressBarContainer);
                infoBoxContent.Controls.Add(progressContainer);
                infoBoxContent.Controls.Add(progressDescriptionContainer);
            }

            infoBoxIconContainer.Controls.Add(infoBoxIcon);

            if (widthRatio > 0)
            {
                infoBoxContainer.Controls.Add(infoBoxContent);
                mainContainer.Controls.Add(infoBoxIconContainer);
                mainContainer.Controls.Add(infoBoxContainer);
                return mainContainer;
            }
            else
            {
                infoBoxContainer.Controls.Add(infoBoxIconContainer);
                infoBoxContainer.Controls.Add(infoBoxContent);
                return infoBoxContainer;
            }
        }

        /// <summary>
        /// Renvoie une tuile affichant un total à positionner dans le tableau de bord
        /// </summary>
        /// <param name="widthRatio">Indice de taille de la tuile (1 = petite, 12 = très large)</param>
        /// <param name="cardColorClass">Classe de couleur pour le conteneur de la tuile</param>
        /// <param name="numericValueColorClass">Classe de couleur pour les valeurs à mettre en avant</param>
        /// <param name="firstTextValue">Valeur de la tuile</param>
        /// <param name="displayNumericValue">Afficher la valeur numérique ?</param>
        /// <param name="firstNumericValue"></param>
        /// <param name="numericValueDecimalCount">Nombre de décimales pour la valeur numérique</param>
        /// <param name="numericValueContainerId">Identifiant HTML du conteneur de la valeur numérique</param>
        /// <param name="numericValueClass">Classe CSS à appliquer sur la valeur numérique</param>        
        /// <param name="displaySmallUnits">Indique si les unités doivent être affichées avec une balise small (si false = span)</param>
        /// <param name="secondTextValue"></param>
        /// <param name="secondNumericValue"></param>
        /// <returns>Panneau <div> représentant la tuile</div></returns>
        private Panel GetDashboardTotalCardWithTowText(
            int widthRatio, string cardColorClass, string numericValueColorClass,
            string firstTextValue,
            bool displayNumericValue, double firstNumericValue, int numericValueDecimalCount,
            string numericValueContainerId, string numericValueClass,
            bool displaySmallUnits, string secondTextValue, double secondNumericValue
            )
        {
            Panel mainContainer = new Panel();
            if (widthRatio > 0)
            {
                mainContainer.CssClass = String.Concat("col-md-", widthRatio);
            }

            Panel descriptionBlockContainer = new Panel();
            descriptionBlockContainer.CssClass = String.Concat("description-block border-right ", cardColorClass);

            #region Compteurs
            if (displayNumericValue)
            {
                //show first container (email envoyé..)
                HtmlGenericControl headerContainer = new HtmlGenericControl("h5");
                headerContainer.Attributes.Add("class", "description-header");

                HtmlGenericControl numericValueMainContainer = new HtmlGenericControl("span");
                numericValueMainContainer.Attributes.Add("class", numericValueClass);
                HtmlGenericControl numericValueContainer = new HtmlGenericControl("span");
                numericValueContainer.ID = numericValueContainerId;
                numericValueContainer.Attributes.Add("class", String.Concat(numericValueColorClass, " ", _numericValuesClass));
                numericValueContainer.InnerText = numericValueDecimalCount > -1 ? Math.Round(firstNumericValue,
                    numericValueDecimalCount).ToString() : firstNumericValue.ToString();
                HtmlGenericControl numericValueUnitContainer = new HtmlGenericControl(displaySmallUnits ? "small" : "span");
                numericValueUnitContainer.InnerText = String.Concat(" ", firstTextValue);
                numericValueMainContainer.Controls.Add(numericValueContainer);
                numericValueMainContainer.Controls.Add(numericValueUnitContainer);
                headerContainer.Controls.Add(numericValueMainContainer);

                descriptionBlockContainer.Controls.Add(headerContainer);

                //show first container (email restants..)
                HtmlGenericControl headerSecondeContainer = new HtmlGenericControl("h5");
                headerSecondeContainer.Attributes.Add("class", "description-header");

                HtmlGenericControl numericValueSecondeContainer = new HtmlGenericControl("span");
                numericValueSecondeContainer.Attributes.Add("class", numericValueClass);
                HtmlGenericControl numericSecondeValueContainer = new HtmlGenericControl("span");
                numericSecondeValueContainer.ID = numericValueContainerId;
                numericSecondeValueContainer.Attributes.Add("class", String.Concat(numericValueColorClass, " ", _numericValuesClass));
                numericSecondeValueContainer.InnerText = numericValueDecimalCount > -1 ? Math.Round(secondNumericValue,
                    numericValueDecimalCount).ToString() : secondNumericValue.ToString();
                HtmlGenericControl numericSecondeValueUnitContainer = new HtmlGenericControl(displaySmallUnits ? "small" : "span");
                numericSecondeValueUnitContainer.InnerText = String.Concat(" ", secondTextValue);
                numericValueSecondeContainer.Controls.Add(numericSecondeValueContainer);
                numericValueSecondeContainer.Controls.Add(numericSecondeValueUnitContainer);
                headerSecondeContainer.Controls.Add(numericValueSecondeContainer);

                descriptionBlockContainer.Controls.Add(headerSecondeContainer);

            }
            #endregion           

            if (widthRatio > 0)
            {
                mainContainer.Controls.Add(descriptionBlockContainer);
                return mainContainer;
            }
            else
            {
                return descriptionBlockContainer;
            }
        }



        /// <summary>
        /// Renvoie une tuile affichant un total à positionner dans le tableau de bord
        /// </summary>
        /// <param name="widthRatio">Indice de taille de la tuile (1 = petite, 12 = très large)</param>
        /// <param name="icon">Icône</param>
        /// <param name="typeId">Type de donnée affichée pour compléter l'ID de certains conteneurs (ex : "mail", "sms")</param>
        /// <param name="cardColorClass">Classe de couleur pour le conteneur de la tuile</param>
        /// <param name="numericValueColorClass">Classe de couleur pour les valeurs à mettre en avant</param>
        /// <param name="textValue">Valeur de la tuile</param>
        /// <param name="displayNumericValue">Afficher la valeur numérique ?</param>
        /// <param name="numericValue">Valeur numérique</param>
        /// <param name="numericValueDecimalCount">Nombre de décimales pour la valeur numérique</param>
        /// <param name="numericValueUnit">Unité de la valeur numérique</param>
        /// <param name="numericValueContainerId">Identifiant HTML du conteneur de la valeur numérique</param>
        /// <param name="numericValueClass">Classe CSS à appliquer sur la valeur numérique</param>
        /// <param name="displayTotalValue">Afficher la valeur totale ?</param>
        /// <param name="totalValue">Valeur totale</param>
        /// <param name="totalValueDecimalCount">Nombre de décimales pour la valeur totale</param>
        /// <param name="totalValueUnit">Unité de la valeur totale</param>
        /// <param name="totalValueContainerId">Identifiant HTML du conteneur de la valeur totale</param>
        /// <param name="displayProgressValue">Afficher le pourcentage de progression ?</param>
        /// <param name="progressValue">Valeur de la barre de progression. Peut être null, auquel cas elle sera calculée à partir de la valeur et du total</param>
        /// <param name="progressValueDecimalCount">Nombre de décimales pour la valeur de la barre de progression</param>
        /// <param name="progressValueUnit">Unité de la valeur de la barre de progression. Si null = %</param>
        /// <param name="progressValueContainerId">Identifiant HTML du conteneur de la valeur de la barre de progression</param>
        /// <param name="progressDescription">Texte à afficher en descriptif de la barre de progression, à la suite des valeurs</param>
        /// <param name="displaySmallUnits">Indique si les unités doivent être affichées avec une balise small (si false = span)</param>
        /// <returns>Panneau <div> représentant la tuile</div></returns>
        private Panel GetDashboardTotalCard(
            int widthRatio, string icon, string typeId, string cardColorClass, string numericValueColorClass,
            string textValue,
            bool displayNumericValue, double numericValue, int numericValueDecimalCount, string numericValueUnit, string numericValueContainerId, string numericValueClass,
            bool displayTotalValue, double totalValue, int totalValueDecimalCount, string totalValueUnit, string totalValueContainerId,
            bool displayProgressValue, double? progressValue, int progressValueDecimalCount, string progressValueUnit, string progressValueContainerId, string progressDescription,
            bool displaySmallUnits
            )
        {
            Panel mainContainer = new Panel();
            if (widthRatio > 0)
            {
                mainContainer.CssClass = String.Concat("col-md-", widthRatio);
            }

            Panel descriptionBlockContainer = new Panel();
            descriptionBlockContainer.CssClass = String.Concat("description-block border-right ", cardColorClass);

            #region Pourcentage
            if (displayProgressValue)
            {
                double nonNullProgressValue = 0;
                if ((progressValue == null || !progressValue.HasValue) && totalValue > 0)
                    nonNullProgressValue = numericValue / totalValue;
                else
                    nonNullProgressValue = (double)progressValue;
                HtmlGenericControl progressContainer = new HtmlGenericControl("span");
                progressContainer.Attributes.Add("class", String.Concat("description-percentage ", numericValueColorClass));
                progressContainer.ID = String.Concat("description-percentage-", typeId);
                HtmlGenericControl progressContainerIcon = new HtmlGenericControl("i");
                progressContainerIcon.Attributes.Add("class", icon);
                HtmlGenericControl progressValueContainer = new HtmlGenericControl("span");
                progressValueContainer.ID = progressValueContainerId;
                progressValueContainer.Attributes.Add("class", _numericValuesClass);
                progressValueContainer.InnerText = progressValueDecimalCount > -1 ? Math.Round(nonNullProgressValue, progressValueDecimalCount).ToString() : nonNullProgressValue.ToString();
                HtmlGenericControl progressPercentContainer = new HtmlGenericControl("span");
                progressPercentContainer.InnerText = !String.IsNullOrEmpty(progressValueUnit) ? String.Concat(progressValueUnit, " ") : "% ";
                HtmlGenericControl progressTextContainer = new HtmlGenericControl("span");
                progressTextContainer.InnerText = progressDescription;
                progressContainer.Controls.Add(progressContainerIcon);
                progressContainer.Controls.Add(progressValueContainer);
                progressContainer.Controls.Add(progressPercentContainer);
                progressContainer.Controls.Add(progressTextContainer);
                descriptionBlockContainer.Controls.Add(progressContainer);
            }
            #endregion

            #region Compteurs
            if (displayNumericValue)
            {
                HtmlGenericControl headerContainer = new HtmlGenericControl("h5");
                headerContainer.Attributes.Add("class", "description-header");

                HtmlGenericControl numericValueMainContainer = new HtmlGenericControl("span");
                numericValueMainContainer.Attributes.Add("class", numericValueClass);
                HtmlGenericControl numericValueContainer = new HtmlGenericControl("span");
                numericValueContainer.ID = numericValueContainerId;
                numericValueContainer.Attributes.Add("class", String.Concat(numericValueColorClass, " ", _numericValuesClass));
                numericValueContainer.InnerText = numericValueDecimalCount > -1 ? Math.Round(numericValue, numericValueDecimalCount).ToString() : numericValue.ToString();
                HtmlGenericControl numericValueUnitContainer = new HtmlGenericControl(displaySmallUnits ? "small" : "span");
                numericValueUnitContainer.InnerText = String.Concat(" ", numericValueUnit);
                numericValueMainContainer.Controls.Add(numericValueContainer);
                numericValueMainContainer.Controls.Add(numericValueUnitContainer);
                headerContainer.Controls.Add(numericValueMainContainer);

                if (displayTotalValue)
                {
                    HtmlGenericControl totalValueSeparatorContainer = new HtmlGenericControl("span");
                    totalValueSeparatorContainer.InnerText = " / ";
                    HtmlGenericControl totalValueContainer = new HtmlGenericControl("span");
                    totalValueContainer.ID = totalValueContainerId;
                    totalValueContainer.Attributes.Add("class", _numericValuesClass);
                    totalValueContainer.InnerText = totalValueDecimalCount > -1 ? Math.Round(totalValue, totalValueDecimalCount).ToString() : totalValue.ToString();
                    HtmlGenericControl totalValueUnitContainer = new HtmlGenericControl(displaySmallUnits ? "small" : "span");
                    totalValueUnitContainer.InnerText = String.Concat(" ", totalValueUnit);
                    headerContainer.Controls.Add(totalValueSeparatorContainer);
                    headerContainer.Controls.Add(totalValueContainer);
                    headerContainer.Controls.Add(totalValueUnitContainer);
                }

                descriptionBlockContainer.Controls.Add(headerContainer);
            }
            #endregion

            #region Texte de fin
            HtmlGenericControl textContainer = new HtmlGenericControl("span");
            textContainer.Attributes.Add("class", "description-text");
            textContainer.InnerText = textValue;
            descriptionBlockContainer.Controls.Add(textContainer);
            #endregion

            if (widthRatio > 0)
            {
                mainContainer.Controls.Add(descriptionBlockContainer);
                return mainContainer;
            }
            else
            {
                return descriptionBlockContainer;
            }
        }

        /// <summary>
        /// Crée une tuile destinée à recevoir un graphique SyncFusion
        /// </summary>
        /// <param name="rowClass">Classe(s) CSS additionnelles à ajouter sur le conteneur parent de la section (celui transmis en retour de cette méthode)</param>
        /// <param name="widthRatio">Indice de taille de la section (1 = petite, 12 = très large)</param>
        /// <param name="boxColorClass">Couleur de classe CSS à appliquer sur la section (liseré en haut). Exemples : blue, teal, info...</param>
        /// <param name="headerMainText">Texte (ou code HTML) principal à afficher dans l'en-tête</param>
        /// <param name="headerSubText">Sous-texte (ou code HTML) à afficher dans l'en-tête</param>
        /// <param name="chartContainerId">ID du contrôle devant contenir le graphique</param>
        /// <param name="contentsContainer">Elément de type Panel (div) destiné à recevoir le contenu à injecter dans la section</param>
        /// <returns></returns>
        private Panel GetDashboardChartCard(string rowClass, int widthRatio, string boxColorClass, string headerMainText, string headerSubText, string chartContainerId,
        out Panel contentsContainer
        )
        {
            Panel sectionCol = new Panel();
            sectionCol.CssClass = String.Concat("col-md-", widthRatio);
            Panel sectionBox = new Panel();
            sectionBox.CssClass = String.Concat("box-1info box-main-chart box-", boxColorClass);

            #region  En-tête
            Panel sectionBoxHeader = new Panel();
            sectionBoxHeader.CssClass = "box-header ui-sortable-handle";

            #region Texte
            HtmlGenericControl headerIcon = new HtmlGenericControl("i");
            headerIcon.Attributes.Add("class", "icon-bar-chart-o");
            HtmlGenericControl headerMainTextContainer = new HtmlGenericControl("h3");
            headerMainTextContainer.Attributes.Add("class", "box-title");
            headerMainTextContainer.InnerHtml = headerMainText;
            HtmlGenericControl headerSubTextContainer = new HtmlGenericControl("small");
            headerSubTextContainer.InnerHtml = headerSubText;
            headerMainTextContainer.Controls.Add(headerSubTextContainer);
            sectionBoxHeader.Controls.Add(headerIcon);
            sectionBoxHeader.Controls.Add(headerMainTextContainer);
            #endregion

            #region Boîte à outils
            Panel sectionBoxTools = new Panel();
            sectionBoxTools.CssClass = "box-tools pull-right";
            Panel sectionBoxToolsButtons = new Panel();
            sectionBoxToolsButtons.CssClass = "btn-group";
            #region Bouton Menu
            HtmlButton toggleButton = new HtmlButton();
            toggleButton.Attributes.Add("class", "btn btn-box-tool dropdown-toggle");
            toggleButton.Attributes.Add("data-toggle", "dropdown");
            toggleButton.Attributes.Add("aria-expanded", "false");
            HtmlGenericControl toggleButtonIcon = new HtmlGenericControl("i");
            toggleButtonIcon.Attributes.Add("class", "icon-ellipsis-v");
            toggleButton.Controls.Add(toggleButtonIcon);
            sectionBoxToolsButtons.Controls.Add(toggleButton);
            #endregion
            #region Menu
            HtmlGenericControl dropDownMenuList = new HtmlGenericControl("ul");
            dropDownMenuList.Attributes.Add("class", "dropdown-menu");
            dropDownMenuList.Attributes.Add("role", "menu");
            // TODO : LI
            /*
                <li id="print"><a href="#"><i class="icon-file-pdf-o"></i>Générer un PDF</a></li>
                <li><a href="#"><i class="icon-file-image-o"></i>Faire une capture</a></li>
                <li><a href="#"><i class="icon-file-excel-o"></i>Exporter vers Excel</a></li>
                <li class="divider"></li>
                <li class="bolding"><a href="#"><i class="icon-list"></i>Accéder aux données</a></li>
             */
            sectionBoxToolsButtons.Controls.Add(dropDownMenuList);
            #endregion
            //On masque pour l'instant
            //sectionBoxTools.Controls.Add(sectionBoxToolsButtons);
            #endregion

            sectionBoxHeader.Controls.Add(sectionBoxTools);
            sectionBox.Controls.Add(sectionBoxHeader);
            #endregion

            #region Conteneur de graphique
            Panel chartContainer = new Panel();
            chartContainer.CssClass = "box-body box-chart";
            chartContainer.ID = chartContainerId;
            sectionBox.Controls.Add(chartContainer);
            #endregion

            sectionCol.Controls.Add(sectionBox);

            contentsContainer = chartContainer;

            return sectionCol;
        }

        /// <summary>
        /// Crée une sous-section au design spécifique Tableaux de bord, et renvoie en paramètre sortant le conteneur dans lequel insérer le contenu
        /// </summary>
        /// <param name="rowClass">Classe(s) CSS additionnelles à ajouter sur le conteneur parent de la section (celui transmis en retour de cette méthode)</param>
        /// <param name="widthRatio">Indice de taille de la section (1 = petite, 12 = très large)</param>
        /// <param name="boxColorClass">Couleur de classe CSS à appliquer sur la section (liseré en haut). Exemples : blue, teal, info...</param>
        /// <param name="headerMainText">Texte (ou code HTML) principal à afficher dans l'en-tête</param>
        /// <param name="headerSubText">Sous-texte (ou code HTML) à afficher dans l'en-tête</param>
        /// <param name="bodyClass">Classe(s) CSS additionnelles à ajouter au conteneur du corpis de la section</param>
        /// <param name="contentsContainer">Elément de type Panel (div) destiné à recevoir le contenu à injecter dans la section</param>
        /// <param name="helpContent">Ajout d'une infobulle pour les utilisateur sur SMS - demande régression 75 851</param>
        /// <returns></returns>
        private Panel GetDashboardSection(
        string rowClass, int widthRatio, string boxColorClass, string headerMainText, string headerSubText, string bodyClass,
        out Panel contentsContainer, string helpContent = null
        )
        {
            Panel sectionRow = new Panel();
            sectionRow.CssClass = String.Concat("row ", rowClass); // burp
            Panel sectionCol = new Panel();
            sectionCol.CssClass = String.Concat("col-md-", widthRatio);
            Panel sectionBox = new Panel();
            sectionBox.CssClass = String.Concat("box box-", boxColorClass);

            #region  En-tête
            Panel sectionBoxHeader = new Panel();
            sectionBoxHeader.CssClass = "box-header titleHeader ui-sortable-handle";

            #region Texte
            HtmlGenericControl headerSection = new HtmlGenericControl("section");
            headerSection.Attributes.Add("class", "content-header");
            HtmlGenericControl headerMainTextContainer = new HtmlGenericControl("h1");
            headerMainTextContainer.InnerHtml = headerMainText;
            HtmlGenericControl headerSubTextContainer = new HtmlGenericControl("small");
            headerSubTextContainer.InnerHtml = headerSubText;
            headerMainTextContainer.Controls.Add(headerSubTextContainer);
            headerSection.Controls.Add(headerMainTextContainer);
            sectionBoxHeader.Controls.Add(headerSection);
            #endregion

            //ELAIZ: ajout d'une infobulle pour les utilisateur sur SMS - demande régression 75 851

            #region Aide

            if (helpContent != null)
            {
                Panel pnHelpDiv = new Panel();
                //pnHelpDiv.ToolTip = helpContent;
                pnHelpDiv.CssClass = "icon-info-circle sms-help";
                HtmlGenericControl infoTooltip = new HtmlGenericControl("span");
                infoTooltip.Attributes.Add("aria-label", helpContent);
                pnHelpDiv.Controls.Add(infoTooltip);

                sectionBoxHeader.Controls.Add(pnHelpDiv);
            }
            #endregion

            #region Boîte à outils
            Panel sectionBoxTools = new Panel();
            sectionBoxTools.CssClass = "box-tools pull-right";
            Panel sectionBoxToolsButtons = new Panel();
            sectionBoxToolsButtons.CssClass = "btn-group";
            #region Bouton Réduire
            HtmlButton collapseButton = new HtmlButton();
            collapseButton.Attributes.Add("class", "btn btn-box-tool");
            collapseButton.Attributes.Add("type", "button");
            collapseButton.Attributes.Add("onclick", "nsAdminDashboard.displayCollapse(this);");
            HtmlGenericControl collapseButtonIcon = new HtmlGenericControl("i");
            collapseButtonIcon.Attributes.Add("class", "icon-minus2");
            collapseButton.Controls.Add(collapseButtonIcon);
            sectionBoxToolsButtons.Controls.Add(collapseButton);
            #endregion
            sectionBoxTools.Controls.Add(sectionBoxToolsButtons);
            #endregion

            sectionBoxHeader.Controls.Add(sectionBoxTools);
            sectionBox.Controls.Add(sectionBoxHeader);
            #endregion

            #region Corps de la section
            Panel sectionBoxBody = new Panel();
            sectionBoxBody.CssClass = String.Concat("box-body ", bodyClass);
            Panel sectionBoxBodyContents = new Panel();
            sectionBoxBodyContents.CssClass = "content-body";
            sectionBoxBody.Controls.Add(sectionBoxBodyContents);
            sectionBox.Controls.Add(sectionBoxBody);
            #endregion

            sectionCol.Controls.Add(sectionBox);
            sectionRow.Controls.Add(sectionCol);

            contentsContainer = sectionBoxBodyContents;

            return sectionRow;
        }

        /// <summary>
        /// Renvoie la couleur à utiliser pour un compteur selon sa valeur par rapport à la valeur maximale autorisée
        /// </summary>
        /// <param name="currentColor">Couleur actuelle, renvoyée si aucun seuil n'est atteint</param>
        /// <param name="currentSize">Valeur actuelle du compteur</param>
        /// <param name="availableSize">Valeur maximale autorisée</param>
        /// <param name="warningThreshold">Pourcentage à partir duquel afficher l'icône en orange</param>
        /// <param name="criticalThreshold">Pourcentage à partir duquel afficher l'icône en rouge</param>
        /// <returns>Couleur à utiliser</returns>
        string GetNewColorFromThreshold(string currentColor, double currentSize, double availableSize, int warningThreshold = _warningThreshold, int criticalThreshold = _criticalThreshold)
        {
            if (warningThreshold <= 0)
                warningThreshold = _warningThreshold; // sur la maquette : 60
            if (criticalThreshold <= 0)
                criticalThreshold = _criticalThreshold; // sur la maquette : 80

            if (availableSize == 0 && currentSize == 0)
                return currentColor;
            else if (availableSize == 0 && currentSize != 0)
                return "red";
            else if (currentSize > availableSize || ((currentSize / availableSize) * 100 > criticalThreshold))
                return "red";
            else if ((currentSize / availableSize) * 100 > warningThreshold)
                return "yellow";
            else
                return currentColor;
        }
    }
}
