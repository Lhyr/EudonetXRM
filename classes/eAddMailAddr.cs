using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EudoExtendedClasses;
using EudoQuery;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe métier utilisée par la fenêtre d'ajout de destinataire (pour les mails unitaires)
    /// </summary>
    public class eAddMailAddrData
    {
        private ePref _pref;
        private HashSet<Int32> _lstFields;
        private HashSet<TableType> _hsTabWithFields;

        /// <summary>chaine de caractères recherchée, si renseignée PPID, PMID, ADRID pas pris en compte</summary>
        public String Search = "";
        /// <summary>PP PM ADR liés à la fiche, ne sont pas pris en compte si le champ de recherche est renseigné </summary>
        public Int32 PPID = 0, PMID = 0, ADRID = 0;
        /// <summary>
        /// Type de fichier géré par la fenêtre
        /// </summary>
        public Int32 FileType = EudoQuery.EdnType.FILE_MAIL.GetHashCode();

        /// <summary>
        /// Liste des adresses e-mail retournées
        /// </summary>
        public List<String> DataList { get; private set; }

        /// <summary>
        /// Formats de données autorisés pour les champs recherchés
        /// </summary>
        public List<FieldFormat> RequiredFieldFormats { get; set; }
        /// <summary>
        /// Champs de la table USER recherchés
        /// </summary>
        public List<UserField> RequiredUserFields { get; set; }

        /// <summary>
        /// Classe métier utilisée par la fenêtre d'ajout de destinataire (pour les mails unitaires)
        /// </summary>
        public eAddMailAddrData(ePref pref)
        {
            _pref = pref;
        }

        /// <summary>
        /// Alimentation de la liste des champs utilisables par la fenêtre et sa classe liée
        /// </summary>
        /// <returns>Liste de DescIDs des champs du type indiqué</returns>
        protected HashSet<Int32> getFieldsList()
        {
            if (_lstFields != null)
                return _lstFields;

            _lstFields = new HashSet<Int32>();

            Dictionary<int, string> dic;
            _hsTabWithFields = new HashSet<TableType>();
            TableType[] aTabs = new TableType[3] { TableType.PP, TableType.PM, TableType.ADR };
            foreach (TableType tab in aTabs)
            {
                foreach (FieldFormat requiredFieldFormat in RequiredFieldFormats)
                {
                    dic = eDataTools.GetAllowedDescIdByFormat(_pref, tab.GetHashCode(), requiredFieldFormat);
                    _lstFields.UnionWith(dic.Keys);
                    if (dic.Count > 0)
                        _hsTabWithFields.Add(tab);
                }
            }
            return _lstFields;
        }

        /// <summary>
        /// Récupération de la fiche PM liée
        /// </summary>
        /// <returns></returns>
        private Int32 getPMID()
        {
            if (PMID > 0)
                return PMID;

            if (PPID == 0)
                return 0;

            eDataFillerGeneric dtfPMID = new eDataFillerGeneric(_pref, TableType.PP.GetHashCode(), ViewQuery.CUSTOM);
            dtfPMID.EudoqueryComplementaryOptions =
                         delegate (EudoQuery.EudoQuery eq)
                         {
                             eq.SetListCol = $"201;401;301";
                             eq.SetTopRecord = 1;

                             List<WhereCustom> liWC = new List<WhereCustom>();
                             liWC.Add(new WhereCustom("PPID", Operator.OP_EQUAL, PPID.ToString()));
                             if (ADRID > 0)
                                 liWC.Add(new WhereCustom("ADRID", Operator.OP_EQUAL, ADRID.ToString()));

                             liWC.Add(new WhereCustom(((int)AdrField.ACTIVE).ToString(), Operator.OP_EQUAL, "1"));

                             eq.AddCustomFilter(new WhereCustom(liWC));

                             eq.AddParam("sort", AdrField.PRINCIPALE.GetHashCode().ToString());
                             eq.AddParam("order", SortOrder.DESC.GetHashCode().ToString());
                         };

            dtfPMID.Generate();

            if (dtfPMID.ErrorMsg.Length != 0 || dtfPMID.InnerException != null)
            {
                if (dtfPMID.ErrorType != QueryErrorType.ERROR_NUM_PREF_NOT_FOUND)
                    throw new Exception(String.Concat(dtfPMID.ErrorMsg, dtfPMID.InnerException == null ? String.Empty : dtfPMID.InnerException.Message));
            }

            if (dtfPMID.ListRecords.Count == 0)
                return 0;

            // Recupère l'enregistrement
            eRecord row = dtfPMID.GetFirstRow();
            if (row == null)
                return 0;

            eFieldRecord fld = row.GetFieldByAlias("200_400_301");
            if (fld == null)
                return 0;

            PMID = fld.FileId;

            return PMID;

        }

        /// <summary>
        /// Renvoie la liste des contacts liés à une fiche PM correspondant aux critères donnés
        /// </summary>
        /// <returns>Liste des contacts liés à une fiche PM correspondant aux critères donnés</returns>
        private HashSet<String> getContactCompanyData()
        {
            getFieldsList();

            eDataFillerGeneric dtf = new eDataFillerGeneric(_pref, TableType.PP.GetHashCode(), ViewQuery.CUSTOM);
            dtf.EudoqueryComplementaryOptions =
                         delegate (EudoQuery.EudoQuery eq)
                         {

                             eq.SetListCol = String.Concat($"201;301;401;{(int)AdrField.ACTIVE};", eLibTools.Join<Int32>(";", _lstFields));

                             WhereCustom wc;
                             if (String.IsNullOrEmpty(Search))
                             {
                                 wc = new WhereCustom("PMID", Operator.OP_EQUAL, getPMID().ToString());
                             }
                             else
                             {
                                 List<WhereCustom> liwc = new List<WhereCustom>();
                                 foreach (TableType tab in _hsTabWithFields)
                                 {
                                     if (tab == TableType.ADR)
                                         continue;

                                     Int32 iDescid = tab.GetHashCode() + 1;
                                     liwc.Add(new WhereCustom(iDescid.ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));

                                 }

                                 foreach (Int32 descid in _lstFields)
                                 {
                                     liwc.Add(new WhereCustom(descid.ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));
                                 }
                                 wc = new WhereCustom(liwc, InterOperator.OP_OR);
                             }
                             eq.AddCustomFilter(wc);

                             eq.AddParam("sort", "201");
                             eq.AddParam("order", SortOrder.ASC.GetHashCode().ToString());
                         };

            dtf.Generate();

            if (dtf.ErrorMsg.Length != 0 || dtf.InnerException != null)
            {
                if (dtf.ErrorType != QueryErrorType.ERROR_NUM_PREF_NOT_FOUND)
                    throw new Exception(String.Concat(dtf.ErrorMsg, dtf.InnerException == null ? String.Empty : dtf.InnerException.Message));
            }

            HashSet<String> hsContactData = new HashSet<String>();

            foreach (eRecord rec in dtf.ListRecords)
            {



                String sPPName = "", sPMName = "",sAdrName = "";
                eFieldRecord fldPPName = rec.GetFieldByAlias("200_201");
                if (fldPPName != null)
                    sPPName = fldPPName.DisplayValuePPName;

                eFieldRecord fldPMName = rec.GetFieldByAlias("200_400_301");
                if (fldPMName != null)
                    sPMName = fldPMName.DisplayValue;


                eFieldRecord fldAdrName = rec.GetFieldByAlias("200_401");
                if (fldAdrName != null)
                    sAdrName = fldAdrName.DisplayValue;

                foreach (eFieldRecord f in rec.GetFields)
                {
                    if (RequiredFieldFormats == null || !RequiredFieldFormats.Contains(f.FldInfo.Format))
                        continue;
                    if (String.IsNullOrEmpty(f.Value) || !IsValidValue(f.Value))
                        continue;

                    if (f.FldInfo.Table.DescId == TableType.PM.GetHashCode())
                    {
                        hsContactData.Add(String.Concat(sPMName, " <", f.Value, ">"));
                    }
                    else if (f.FldInfo.Table.DescId == (int)TableType.ADR)
                    {

                        //si adresse mais pas active, on ignore
                        eFieldRecord fldActive = rec.GetFieldByAlias($"200_{(int)AdrField.ACTIVE}");
                        if (fldActive != null && fldActive.Value == "1")
                        {
              

                            hsContactData.Add(String.Concat(sAdrName, " <", f.Value, ">"));
                        }


                    }
                    else
                    {
                        hsContactData.Add(String.Concat(sPPName, " <", f.Value, ">"));
                    }
                }

            }


            return hsContactData;


        }

        /// <summary>
        /// Renvoie la liste des utilisateurs de la base disposant de l'information souhaitée
        /// </summary>
        /// <returns>Liste des utilisateurs de la base disposant de l'information souhaitée</returns>
        private HashSet<String> getUserData()
        {
            HashSet<String> hsUserData = new HashSet<String>();

            if (String.IsNullOrEmpty(Search))
                return hsUserData;

            eDataFillerGeneric dtf = new eDataFillerGeneric(_pref, TableType.USER.GetHashCode(), ViewQuery.CUSTOM);
            dtf.EudoqueryComplementaryOptions =
                delegate (EudoQuery.EudoQuery eq)
                {
                    List<int> requiredCols = new List<int>() { UserField.NAME.GetHashCode() };
                    List<WhereCustom> liwc = new List<WhereCustom>();
                    liwc.Add(new WhereCustom(UserField.NAME.GetHashCode().ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));

                    foreach (UserField requiredUserField in RequiredUserFields)
                    {
                        requiredCols.Add(requiredUserField.GetHashCode());
                        liwc.Add(new WhereCustom(requiredUserField.GetHashCode().ToString(), Operator.OP_CONTAIN, Search, InterOperator.OP_OR));
                    }

                    eq.SetListCol = String.Join(";", requiredCols);

                    eq.AddCustomFilter(new WhereCustom(liwc, InterOperator.OP_OR));
                };

            dtf.Generate();

            if (dtf.ErrorMsg.Length != 0 || dtf.InnerException != null)
            {
                if (dtf.ErrorType != QueryErrorType.ERROR_NUM_PREF_NOT_FOUND)
                    throw new Exception(String.Concat(dtf.ErrorMsg, dtf.InnerException == null ? String.Empty : dtf.InnerException.Message));
            }

            foreach (eRecord rec in dtf.ListRecords)
            {
                String sUserName = "";
                eFieldRecord fldUserName = rec.GetFieldByAlias(String.Concat(TableType.USER.GetHashCode(), "_", UserField.NAME.GetHashCode()));
                if (fldUserName != null)
                    sUserName = fldUserName.Value;

                foreach (UserField requiredUserField in RequiredUserFields)
                {
                    eFieldRecord f = rec.GetFieldByAlias(String.Concat(TableType.USER.GetHashCode(), "_", requiredUserField.GetHashCode()));

                    if (f != null)
                        if (!String.IsNullOrEmpty(f.Value) && IsValidValue(f.Value))
                            hsUserData.Add(String.Concat(sUserName, " <", f.Value, ">"));
                }
            }

            return hsUserData;
        }

        /// <summary>
        /// Lance la recherche
        /// </summary>
        public void LaunchSearch()
        {
            HashSet<String> hsContactData = getContactCompanyData();
            HashSet<String> hsUserData = getUserData();

            hsContactData.UnionWith(hsUserData);

            DataList = hsContactData.ToList<String>();
            DataList.Sort();
        }

        /// <summary>
        /// Vérifie si la valeur du champ parcouru est valide en fonction du ou des formats spécifiés
        /// </summary>
        /// <param name="value">Valeur à valider</param>
        /// <returns>true si valide pour tous les formats demandés, false sinon</returns>
        public bool IsValidValue(string value)
        {
            List<bool> validatedValues = new List<bool>();

            foreach (FieldFormat fieldFormat in RequiredFieldFormats)
            {
                switch (fieldFormat)
                {
                    case FieldFormat.TYP_EMAIL: validatedValues.Add(eLibTools.IsEmailAddressValid(value)); break;
                    case FieldFormat.TYP_PHONE: validatedValues.Add(eLibTools.IsPhoneNumberValid(value)); break;
                }
            }

            // La valeur est considérée comme valide si tous les formats testés ont été validés
            return !validatedValues.Contains(false);
        }
    }
}