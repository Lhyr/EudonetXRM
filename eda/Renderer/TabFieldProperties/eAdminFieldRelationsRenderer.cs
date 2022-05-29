using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Linq;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldRelationsRenderer : eAdminBlockRenderer
    {
        private Int32 _descid;
        private eAdminFieldInfos _field;

        private eAdminFieldRelationsRenderer(ePref pref, eAdminFieldInfos field)
            : base(pref, null, eResApp.GetRes(pref, 1117), idBlock: "blockRelations") // Relations
        {
            _descid = field.DescId;
            _field = field;
        }

        /// <summary>
        /// Création du bloc "relation" pour les catalogues spéciaux
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static eAdminFieldRelationsRenderer CreateAdminFieldRelationsRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldRelationsRenderer relations = new eAdminFieldRelationsRenderer(pref, field);
            return relations;
        }

        protected override bool Build()
        {
            base.Build();

            Boolean userAllowed = _field.Format != FieldFormat.TYP_ALIASRELATION
                && _field.IsUserAllowedToUpdate()
                && !eAdminTools.IsSpecialField(_field)
                ;

            eAdminField adminField;

            // Onglet lié
            List<ListItem> items = new List<ListItem>();

            List<eFieldRes> listFields = _field.GetAvailableMainFields();


            List<int> lstHiddenTab = new List<int>();
            if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
            {
           
                lstHiddenTab = eLibTools.GetDescAdvInfo(Pref, listFields.Select(ww => (ww.DescId - ww.DescId % 100)).ToList(),
                    new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT })
                        .Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT && dd.Item2 == "1") != null)
                        .Select(t => t.Key).ToList();



                if (_field.Format == FieldFormat.TYP_ALIASRELATION)
                {
                    if (lstHiddenTab.Contains(_field.AliasRelationTab))
                        lstHiddenTab.Remove(_field.AliasRelationTab);
                }
                else
                {
                    if (lstHiddenTab.Contains(_field.PopupDescId - _field.PopupDescId %100))
                        lstHiddenTab.Remove(_field.PopupDescId - _field.PopupDescId % 100);

                }


            }

            foreach (eFieldRes f in listFields)
            {
                if (lstHiddenTab.Contains(f.DescId - f.DescId % 100))
                    continue;

                items.Add(f.ToListItem());
            }

            if (_field.Format == FieldFormat.TYP_ALIASRELATION)
                adminField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 7703), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.ALIAS_RELATION, items.ToArray(), eResApp.GetRes(Pref, 7931), (_field.AliasRelationTab + 1).ToString(), renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE);
            else
                adminField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 7703), eAdminUpdateProperty.CATEGORY.DESC, (int)eLibConst.DESC.POPUPDESCID, items.ToArray(), eResApp.GetRes(Pref, 7931), _field.PopupDescId.ToString(), renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE);

            adminField.ReadOnly = !userAllowed;
            adminField.Generate(_panelContent);

            if (this._field.Format == FieldFormat.TYP_ALIASRELATION)
                return true;

            // Afficher en signet
            Dictionary<String, String> dic = new Dictionary<String, String>();
            int relationId = Int32.Parse(adminField.Value);
            //Dans le cas ou il existe deja une relation native équivalente, on ne propose pas l'affichage en signet
            String sError = "";

            eudoDAL dal = eLibTools.GetEudoDAL(_ePref);
            bool bIsBkmRelation = false;
            try
            {
                dal.OpenDatabase();
                bIsBkmRelation = false;// listFields.Exists(z => z.DescId == relationId &&  z.HasRel); // eSqlDesc.IsBkmRelation(dal, eLibTools.GetTabFromDescId(relationId), eLibTools.GetTabFromDescId(_descid), _descid, out sError);
                if (sError.Length > 0)
                    throw new EudoException(String.Concat("eSqlDesc.IsBkmRelation : ", sError));
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                dal.CloseDatabase();
            }

            if (bIsBkmRelation && !_field.Relation)
            {
            }
            else if (eLibTools.GetTabFromDescId(relationId) == (int)TableType.PP && _field.Table.InterPP)
            {

            }
            else if (eLibTools.GetTabFromDescId(relationId) == (int)TableType.PM && _field.Table.InterPM)
            {

            }
            else if (eLibTools.GetTabFromDescId(relationId) == _field.Table.InterEVTDescid)
            {

            }
            else
            //SHA : correction bug 72 427
            //KHA US 910 BL 1344:
            if (

                 //Si le champ est déjà relation, on laisse l'option
                 _field.Relation ||

                 // interdiction de PP - PM / PM -PP, incompatible avec address

                 !(

                       //Liste des  cas interdit
                       (_field.Table.TabType == TableType.PP && eLibTools.GetTabFromDescId(relationId) == (int)TableType.PM) // Liaison PP - PM
                    || (_field.Table.TabType == TableType.PM && eLibTools.GetTabFromDescId(relationId) == (int)TableType.PP) // Liaison PM - PP
                )
                )

            {

                dic.Add("1", eResApp.GetRes(Pref, 1460));
                dic.Add("0", eResApp.GetRes(Pref, 1459));
                adminField = new eAdminRadioButtonField(_descid,
                                                            eResApp.GetRes(Pref, 7939),
                                                            eAdminUpdateProperty.CATEGORY.DESC,
                                                            (int)eLibConst.DESC.RELATION, "relInBKM",
                                                            dic,
                                                            value: _field.Relation ? "1" : "0",
                                                            tooltiptext: eResApp.GetRes(Pref, 7940));
                adminField.IsLabelBefore = true;

                adminField.ReadOnly = !userAllowed;
                adminField.Generate(_panelContent);
            }

            // La recherche porte sur...
            dic = new Dictionary<String, String>();
            dic.Add("0", eResApp.GetRes(Pref, 7941));
            dic.Add("1", eResApp.GetRes(Pref, 530));
            adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7942), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.SEARCHALL.GetHashCode(), "rbSearchAll", dic, value: _field.SearchAll ? "1" : "0",
                tooltiptext: eResApp.GetRes(Pref, 7943));
            adminField.IsLabelBefore = true;

            adminField.ReadOnly = !userAllowed;
            adminField.Generate(_panelContent);

            // Option de recherche modifiable ou figée
            dic = new Dictionary<String, String>();
            dic.Add("0", eResApp.GetRes(Pref, 7944));
            dic.Add("1", eResApp.GetRes(Pref, 7945));
            adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7946), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.SEARCHALLBLOCKED.GetHashCode(), "rbSearchAllBlocked", dic, value: _field.SearchAllBlocked ? "1" : "0",
                tooltiptext: eResApp.GetRes(Pref, 7947));
            adminField.IsLabelBefore = true;

            adminField.ReadOnly = !userAllowed;
            adminField.Generate(_panelContent);

            return true;
        }
    }
}