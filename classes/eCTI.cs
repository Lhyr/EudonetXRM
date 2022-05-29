using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// 
    /// </summary>
    public class eCTI
    {
        // demande 36826 : MCR/RMA : methode qui execute une requete pour recuperer a partir du phone number  les fiches PM, PP, ADRESS qui sont liees        
        // en entree : _pn : phonenumber
        // en sortie : _error : erreur sur lecture du query
        // public Dictionary<Int32, string>  GetDescFromPhoneNumber(ePrefLite pref, eudoDAL dalCti, string _pn , int _nTab, out String _error)
        static Dictionary<Int32, string> _dicoCTI;

        public bool GetDescFromPhoneNumber(ePref pref, eudoDAL dalCti, string _pn, out String _error)
        {
            _dicoCTI = new Dictionary<int, string>();

            _error = String.Empty;

            DataTableReaderTuned _dtrFields = null;

            try
            {
                #region Construction de la requete CTI

                // String _select = "SELECT [type], [Sstype], [Source], [SourceDescId], [DescId]  FROM FILEMAP_PARTNER WHERE [SourceDescid] = @Sourcedescid ";

                String _select = "SELECT [Desc].[Field],  [Desc].[DescId]  FROM [DESC] where [Format] = @Sourcedescid AND [FILE] IN ('PM','PP','ADDRESS')";

                RqParam rq = new RqParam(_select);
                // rq.AddInputParameter("@Sourcedescid", SqlDbType.Int, _nTab);

                // CONSTANTE TYP_PHONE ?? (__               
                rq.AddInputParameter("@Sourcedescid", SqlDbType.Int, 12);

                #endregion

                //Execution de la requete
                _dtrFields = dalCti.Execute(rq, out _error);

                // dalCti.CloseDatabase();

                int _key;

                if (_dtrFields != null)
                {
                    while (_dtrFields.Read())
                    {
                        _key = _dtrFields.GetEudoNumeric("DescId");
                        string _valuefield = _dtrFields.GetString("Field");
                        _dicoCTI.Add(_key, _valuefield);

                    }
                }

                Action<EudoQuery.EudoQuery> complementaryEudoQueryOptions = delegate(EudoQuery.EudoQuery eq)
                {
                    eq.SetListCol = String.Concat("201;301"); // Contenu de la clause SELECT PP01 et PM01
                    
                    List<WhereCustom> liWhere = new List<WhereCustom>();
                    foreach (KeyValuePair<int,string> phoneFieldDescId in _dicoCTI)
	                {
                        liWhere.Add(new WhereCustom(phoneFieldDescId.Key.ToString(), Operator.OP_CONTAIN, _pn, InterOperator.OP_OR));
                        
                        eq.AddCustomFilter(new WhereCustom(liWhere));
                        
	                }
                    
                };

                eDataFillerGeneric dtfCTI = new eDataFillerGeneric(pref, 200, ViewQuery.CUSTOM);
                dtfCTI.EudoqueryComplementaryOptions = complementaryEudoQueryOptions;

                dtfCTI.Generate();
                // dtfCTI.ListRecords  de type List <eRecord>  collection de eRecord

                // Erreur lors du chargement du filler
                if (dtfCTI.ErrorMsg.Length != 0 || dtfCTI.InnerException != null)
                    throw new Exception(String.Concat(dtfCTI.ErrorMsg, dtfCTI.InnerException == null ? String.Empty : dtfCTI.InnerException.Message));


                return true;
            }
            catch (Exception e)
            {
                _error = e.Message;
                dalCti.CloseDatabase();

                if (_dtrFields != null)
                    _dtrFields.Dispose();
                return false;
            }

        }
    }
}