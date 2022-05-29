using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools.coremodel;
using Com.Eudonet.Xrm.eda;
using EudoQuery;

namespace Com.Eudonet.Xrm

{
    /// <summary>
    /// Rendu d'une fiche utilisateur
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eMainFileRenderer" />
    public class eUserFileRenderer : eMainFileRenderer
    {




        /// <summary>
        /// Affichage pour la création et la modification
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        public eUserFileRenderer(ePref pref, Int32 nTab, Int32 nFileId)
            : base(pref, nTab, nFileId)
        {

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            _rType = RENDERERTYPE.EditFile;
        }

        /// <summary>
        /// pas de properties pour les users
        /// </summary>
        /// <returns></returns>
        protected override System.Web.UI.WebControls.Table GetPropertiesTable()
        {
            if (PopupDisplay)
                return new System.Web.UI.WebControls.Table();
            else
                return base.GetPropertiesTable();
        }

        protected override Boolean Build()
        {

            bool bHasExtension = false;
            var t = _myFile.GetFileFields;

            /*  masque les champs d'administration des extensions */
            var fExtension = t.Find(d => d.FldInfo.Descid == 101031);

            var fEudoSyncExch = t.Find(d => d.FldInfo.Descid == UserField.EXCH_SYNCHRO.GetHashCode());


            var zz = eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO, Pref);

            if (fEudoSyncExch != null && zz.Infos != null)
            {
                fEudoSyncExch.RightIsVisible = zz.Infos.IsEnabled;
                bHasExtension = bHasExtension || zz.Infos.IsEnabled;



            }

            fExtension.RightIsVisible = bHasExtension;
            if (!bHasExtension)
            {
                var fHab = t.Find(d => d.FldInfo.Descid == 101025);
            }

            return base.Build();
        }


        /// <summary>
        /// champs composant les propriétés : je fais ma liste de courses
        /// </summary>
        /// <returns></returns>
        protected override List<Int32> GetPropertiesFields(ref List<Int32> PtyFieldsDescId)
        {
            if (PtyFieldsDescId == null)
                PtyFieldsDescId = new List<Int32>();



            // Créé par, Créé le
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.DATE_CREATE.GetHashCode());
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.USER_CREATE.GetHashCode());

            // Modifié le, Modifié par
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.DATE_MODIFY.GetHashCode());
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.USER_MODIFY.GetHashCode());


            return PtyFieldsDescId;
        }


        /// <summary>
        /// Field de type char : gestion des cas particulier de User
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldRow"></param>
        /// <param name="ednWebControl"></param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction"></param>
        /// <returns></returns>
        protected override bool RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {

            //Catalogue spécifique : level
            if (fieldRow.FldInfo.Descid == (int)UserField.LEVEL)
                return RenderUserLevelField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            //catalogue spécifique : langue
            if (fieldRow.FldInfo.Descid == (int)UserField.Lang)
                return RenderUserLangField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            //catalogue spécifique : groupe
            if (fieldRow.FldInfo.Descid == (int)UserField.GroupId)
                return RenderUserGroupField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            if (fieldRow.FldInfo.Descid == (int)UserField.Product)
                return RenderUserProductField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);


            if (fieldRow.FldInfo.Descid == (int)UserField.PASSWORD_POLICIES_ALGO)
                return RenderUserSecurityLevelField(row, fieldRow, ednWebControl, sbClass, ref sClassAction);


            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
        }




        /// <summary>
        /// Les champs systèmes de propriété de la fiche ont un rendu de type cell, car ils ne sont pas modifiables
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        protected override EdnWebControl CreateEditEdnControl(eFieldRecord fld)
        {
            //fld spécifique : level / lang
            if (fld.FldInfo.Descid == (int)UserField.LEVEL || fld.FldInfo.Descid == (int)UserField.Lang)
            {
                return new EdnWebControl() { WebCtrl = new TextBox(), TypCtrl = EdnWebControl.WebControlType.TEXTBOX };
            }
            else
                return base.CreateEditEdnControl(fld);

        }



        /// <summary>
        /// Catalogue user level
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldRow"></param>
        /// <param name="ednWebControl"></param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction"></param>
        /// <returns></returns>
        private bool RenderUserLevelField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            eFieldRecord userLevel = row.GetFieldByAlias(String.Concat((int)TableType.USER, "_", (int)UserField.LEVEL));
            int numUserLevel = eLibTools.GetNum(userLevel?.Value ?? "0");

            Dictionary<String, Tuple<bool, string>> dicValues = new Dictionary<string, Tuple<bool, string>>();
            dicValues.Add(((int)UserLevel.LEV_USR_READONLY).ToString(), new Tuple<bool, string>(true, eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_READONLY)));
            dicValues.Add(((int)UserLevel.LEV_USR_1).ToString(), new Tuple<bool, string>(false, "1"));
            dicValues.Add(((int)UserLevel.LEV_USR_2).ToString(), new Tuple<bool, string>(false, "2"));
            dicValues.Add(((int)UserLevel.LEV_USR_3).ToString(), new Tuple<bool, string>(false, "3"));
            dicValues.Add(((int)UserLevel.LEV_USR_4).ToString(), new Tuple<bool, string>(false, "4"));
            dicValues.Add(((int)UserLevel.LEV_USR_5).ToString(), new Tuple<bool, string>(false, "5"));
            dicValues.Add(((int)UserLevel.LEV_USR_ADMIN).ToString(), new Tuple<bool, string>(false, eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_ADMIN)));

            if (Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN || numUserLevel == (int)UserLevel.LEV_USR_SUPERADMIN)
            {
                dicValues.Add(UserLevel.LEV_USR_SUPERADMIN.GetHashCode().ToString(), new Tuple<bool, string>(false, eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_SUPERADMIN)));
            }

            if ((Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN && eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminProduct))
                || numUserLevel == (int)UserLevel.LEV_USR_PRODUCT)
            {
                dicValues.Add(UserLevel.LEV_USR_PRODUCT.GetHashCode().ToString(), new Tuple<bool, string>(false, eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_PRODUCT)));
            }

            string sFunct = (row.RightIsUpdatable && fieldRow.RightIsUpdatable) ? String.Concat("(function(obj){ nsEfileJS.UpdateSelect(obj, null,  ", PopupDisplay ? "0" : "1", ")})") : "";

            //SHA : #71 939 surcharge de GetSelectCombo pour désactiver l'option "Lecture seule"
            HtmlGenericControl sel = eTools.GetSelectCombo(String.Concat("SEL_", ednWebControl.WebCtrl.ID), dicValues, (!(row.RightIsUpdatable && fieldRow.RightIsUpdatable)), "selectadmin", sFunct, row.GetFieldByAlias("101000_101017")?.Value);
            ednWebControl.AdditionalWebCtrl = sel;
            sel.Attributes.Add("inptID", ednWebControl.WebCtrl.ID);

            //base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            sClassAction = "LNKNUM ";

            if (fieldRow.Value == "")
                fieldRow.Value = "1";
            ednWebControl.WebCtrl.Attributes.Add("dbv", fieldRow.Value);
            ednWebControl.WebCtrl.Attributes.Add("value", fieldRow.Value);
            ednWebControl.WebCtrl.Attributes.Add("type", "hidden");

            return true;
        }


        /// <summary>
        /// Champ image en mode fiche
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldRow"></param>
        /// <param name="ednWebControl"></param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction"></param>
        /// <returns></returns>
        private bool RenderUserAvatarField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {


            return false;
        }

        /// <summary>
        /// Langue utilisateur
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldRow"></param>
        /// <param name="ednWebControl"></param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction"></param>
        /// <returns></returns>
        private bool RenderUserLangField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {

            if (fieldRow.Value.Length == 0)
                fieldRow.Value = Pref.User.UserLang;

            Dictionary<String, String> dicValues = eDataTools.GetUsersLang(Pref);



            HtmlGenericControl sel = eTools.GetSelectCombo(String.Concat("SEL_", ednWebControl.WebCtrl.ID), dicValues, (!(row.RightIsUpdatable && fieldRow.RightIsUpdatable)), " selectadmin ", String.Concat("(function(obj){ nsEfileJS.UpdateSelect(obj, null,  ", PopupDisplay ? "0" : "1", ")})"),
                Int32.Parse(row.GetFieldByAlias("101000_101023")?.Value.Substring(row.GetFieldByAlias("101000_101023").Value.Length - 2)).ToString());
            ednWebControl.AdditionalWebCtrl = sel;
            sel.Attributes.Add("inptID", ednWebControl.WebCtrl.ID);


            //base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            //BSE:#52 738 Affecter l'id de la langue au lieu de la valeur
            int nIdLang = 0;
            var arr = dicValues.Where(aa => aa.Value == fieldRow.Value).Select(aa => aa.Key).ToArray();

            if (arr.Length > 0)
                Int32.TryParse(arr[0], out nIdLang);


            sClassAction = "LNKNUM ";
            ednWebControl.WebCtrl.Attributes.Add("dbv", nIdLang.ToString());

            ednWebControl.WebCtrl.Attributes.Add("value", nIdLang.ToString());
            ednWebControl.WebCtrl.Attributes.Add("type", "hidden");
            return true;
        }



        private bool RenderUserGroupField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {


            Dictionary<String, String> dicValues = new Dictionary<string, string>();

            dicValues.Add("0", eResApp.GetRes(Pref, 436));
            dicValues = dicValues.Concat(eGroup.GetGroupIdNameList(Pref)).ToDictionary(a => a.Key, a => a.Value);

            HtmlGenericControl sel = eTools.GetSelectCombo(String.Concat("SEL_", ednWebControl.WebCtrl.ID), dicValues, (!(row.RightIsUpdatable && fieldRow.RightIsUpdatable)), "selectadmin", String.Concat("(function(obj){ nsEfileJS.UpdateSelect(obj, null,  ", PopupDisplay ? "0" : "1", ")})"), row.GetFieldByAlias("101000_101027")?.Value);
            ednWebControl.AdditionalWebCtrl = sel;
            sel.Attributes.Add("inptID", ednWebControl.WebCtrl.ID);


            //base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            sClassAction = "LNKNUM ";
            ednWebControl.WebCtrl.Attributes.Add("dbv", String.IsNullOrEmpty(fieldRow.Value) ? "0" : fieldRow.Value);
            ednWebControl.WebCtrl.Attributes.Add("value", String.IsNullOrEmpty(fieldRow.Value) ? "0" : fieldRow.Value);
            ednWebControl.WebCtrl.Attributes.Add("type", "hidden");
            return true;
        }

        /// <summary>
        /// Renders the user product field.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="fieldRow">The field row.</param>
        /// <param name="ednWebControl">The edn web control.</param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction">Action CSS class</param>
        /// <returns></returns>
        private bool RenderUserProductField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            Dictionary<String, String> dicValues = new Dictionary<string, string>();
            dicValues.Add("0", eResApp.GetRes(Pref, 436));

            List<eProduct> productsList = eProduct.GetProductsList(this.Pref, null);
            foreach (eProduct p in productsList)
            {
                dicValues.Add(p.ProductID.ToString(), p.ProductCode);
            }

            string sFunct = (row.RightIsUpdatable && fieldRow.RightIsUpdatable) ? String.Concat("(function(obj){ nsEfileJS.UpdateSelect(obj, null,  ", PopupDisplay ? "0" : "1", ")})") : "";

            HtmlGenericControl sel = eTools.GetSelectCombo(String.Concat("SEL_", ednWebControl.WebCtrl.ID), dicValues, (!(row.RightIsUpdatable && fieldRow.RightIsUpdatable)), "selectadmin", sFunct, fieldRow.Value);
            ednWebControl.AdditionalWebCtrl = sel;
            sel.Attributes.Add("inptID", ednWebControl.WebCtrl.ID);

            //base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            sClassAction = "LNKNUM ";

            ednWebControl.WebCtrl.Attributes.Add("dbv", fieldRow.Value);
            ednWebControl.WebCtrl.Attributes.Add("value", fieldRow.Value);

            ednWebControl.WebCtrl.Attributes.Add("type", "hidden");
            return true;
        }


        /// <summary>
        /// Renders the user product field.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="fieldRow">The field row.</param>
        /// <param name="ednWebControl">The edn web control.</param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction">Action CSS class</param>
        /// <returns></returns>
        private bool RenderUserSecurityLevelField(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            Dictionary<String, String> dicValues = new Dictionary<string, string>();


            string sValue = fieldRow.Value;
            if (string.IsNullOrEmpty(fieldRow.Value))
            {

                IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv = eLibTools.GetConfigAdvValues(_ePref, eLibConst.CONFIGADV.PASSWORD_POLICIES_ALGO);
                sValue =  eLibTools.GetEnumFromCode<PASSWORD_ALGO>(dicConfigAdv[eLibConst.CONFIGADV.PASSWORD_POLICIES_ALGO], true).GetHashCode().ToString();

            }

            dicValues.Add(sValue, eResApp.GetRes(_ePref, Outils.EnumToResId.GetPassworAlgoResID(eLibTools.GetEnumFromCode<PASSWORD_ALGO>(sValue))));





            HtmlGenericControl sel =
                eTools.GetSelectCombo(String.Concat("SEL_", ednWebControl.WebCtrl.ID),
                dicValues,
                true,
                "selectadmin", "", fieldRow.Value);
            ednWebControl.AdditionalWebCtrl = sel;
            sel.Attributes.Add("inptID", ednWebControl.WebCtrl.ID);

            //base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            sClassAction = "LNKNUM ";

            ednWebControl.WebCtrl.Attributes.Add("dbv", fieldRow.Value);
            ednWebControl.WebCtrl.Attributes.Add("value", fieldRow.Value);

            ednWebControl.WebCtrl.Attributes.Add("type", "hidden");
            return true;
        }

        protected override void AddAddtionnalControlInValueCell(TableCell myValueCell, Field field = null)
        {
            ToggleProductField(myValueCell, field);
        }

        protected override void AddAddtionnalControlInLabelCell(TableCell myValueCell, Field field = null)
        {
            ToggleProductField(myValueCell, field);
        }

        /// <summary>
        /// Masque le champ "Produit" si l'utilisateur n'est pas de niveau 200
        /// </summary>
        /// <param name="myCell">My cell.</param>
        /// <param name="field">The field.</param>
        private void ToggleProductField(TableCell myCell, Field field = null)
        {
            if (field != null)
            {
                if (field.Descid == (int)UserField.Product)
                {
                    eFieldRecord fRecord = _myFile.GetFileFields.FirstOrDefault(f => f.FldInfo.Descid == (int)UserField.LEVEL);
                    if (fRecord != null)
                    {
                        if (fRecord.Value != ((int)UserLevel.LEV_USR_PRODUCT).ToString())
                            myCell.Style.Add("display", "none");
                    }

                }
            }
        }
    }
}