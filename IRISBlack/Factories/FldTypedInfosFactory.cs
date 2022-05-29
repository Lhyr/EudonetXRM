using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;
using System;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// 
    /// </summary>
    public class FldTypedInfosFactory
    {
        Field _field { get; set; }
        ePref _pref { get; set; }
        eDataFiller _dataFiller { get; set; }
        /// <summary>
        /// Données RGPD du champ
        /// </summary>
        RGPDModel _rgpdM { get; set; }

        #region DIctionnaires pour refactoriser les switches ultérieurement
        /// <summary>
        /// TODO : Remplacer les Switch par des dictionnaires pour une meilleure lisibilité.
        /// </summary>
        Dictionary<FieldFormat, FieldType> dicEquiFields { get; set; } = new Dictionary<FieldFormat, FieldType> { };
        /// <summary>
        /// Celui-ci utilisera des lambdas qui retournent le bon constructeur.
        /// </summary>
        Dictionary<FieldType, Func<Field, IFldTypedInfosModel>> dicTypeInst { get; set; } = new Dictionary<FieldType, Func<Field, IFldTypedInfosModel>> { };
        #endregion

        #region constructeurs

        /// <summary>
        /// Constructeur Pour la factory avec juste le Field
        /// </summary>
        /// <param name="_f"></param>
        private FldTypedInfosFactory(Field _f)
        {
            _field = _f;
        }
        /// <summary>
        /// Constructeur Pour la factory
        /// </summary>
        /// <param name="field"></param>
        /// <param name="pref"></param>
        /// <param name="datafiller"></param>
        /// <param name="rgpdM">Données RGPD pour le champ</param>
        private FldTypedInfosFactory(Field field, ePref pref, eDataFiller datafiller, RGPDModel rgpdM)
            : this(field)
        {
            this._pref = pref;
            this._dataFiller = datafiller;
            this._rgpdM = rgpdM;
        }
        #endregion

        #region Initialisation statique de l'objet

        /// <summary>
        /// Initialisation statique de la classe FldTypedInfosFactory avec juste l'objet fields.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static FldTypedInfosFactory InitFldTypedInfosFactory(Field f)
        {
            return new FldTypedInfosFactory(f);
        }
        /// <summary>
        /// Initialisation statique de la classe FldTypedInfosFactory.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="pref"></param>
        /// <param name="dataFiller"></param>
        /// <param name="rgpdM">Paramètres RGPD pour le champ</param>
        /// <returns></returns>
        public static FldTypedInfosFactory InitFldTypedInfosFactory(Field f, ePref pref, eDataFiller dataFiller = null, RGPDModel rgpdM = null)
        {
            return new FldTypedInfosFactory(f, pref, dataFiller, rgpdM);
        }
        #endregion

        #region privée

        #region GetCatalogValues
        /// <summary>
        /// Récupère les valeurs de catalogues disponibles, dont leurs infobulles, pour les transmettre au Front
        /// </summary>
        /// <returns></returns>
        private List<ICatalogValue> GetCatalogValues()
        {
            // Désactivé le 03-09-2021 pour l'instant - La remontée des valeurs de catalogue sur le contrôleur /detail ralentit fortement l'affichage de certains catalogues sur le nouveau mode Fiche
            // A réactiver après optimisation (ex : ne charger ces valeurs que si l'utilisateur les demande en pointant son curseur sur un catalogue)
            bool bGetCatalogValues = false;

            if (!bGetCatalogValues)
                return new List<ICatalogValue>();

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();
                eCatalog catObj = new eCatalog();

                //TODO : ne pas mettre showHidden en dur ?

                // #89 499 - Gestion des catalogues de type ENUM
                if (_field.Popup == PopupType.ENUM)
                {
                    catObj = new eCatalogEnum(_pref, dal, _pref.User, eCatalogEnum.GetCatalogEnumTypeFromField(_field));
                }
                else
                {
                    // #89 203 - Si le catalogue utilise les valeurs d'un autre catalogue, c'est de cet autre catalogue que l'on récupère l'information
                    if (_field.PopupDescId > 0)
                        catObj = new eCatalog(dal, _pref, (PopupType)_field.Popup, _pref.User, _field.PopupDescId, showHiddenValues: true);
                    else
                        catObj = new eCatalog(dal, _pref, (PopupType)_field.Popup, _pref.User, _field.Descid, showHiddenValues: true);
                }

                return CatalogValuesFactory.GetModel(catObj).Values;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dal.CloseDatabase();
            }
        }
        #endregion

        #endregion

        #region public


        /// <summary>
        /// renvoie un objet représentant la structure de champ correctement typé
        /// En paramètre on recoit les res.
        /// </summary>
        /// <param name="getResAdv"></param>
        /// <param name="descAdv"></param>
        /// <returns></returns>
        public IFldTypedInfosModel GetFldTypedInfos(eResInternal getResAdv, DescAdvDataSet descAdv, IDictionary<int, RGPDModel> rgpdData)
        {

            IFldTypedInfosModel fldReturn = null;

            /** léger souci avec le eDataFiller qui ne contient pas le parent.
             * Tout était redirigé vers le champs caractère. */
            switch (_field.Format)
            {
                case FieldFormat.TYP_CHAR:

                    //on identifie ici les liaisons parentes systèmes
                    if (_dataFiller != null
                        && _field.Table.DescId != _dataFiller.ViewMainTable.DescId
                        && _field.Descid == _field.Table.MainFieldDescId
                        )
                    {
                        fldReturn = new SystemRelationFieldInfos(_field, _pref, getResAdv);
                        break;
                    }
                    //on identifie ici les catalogue de type relation
                    else if (_field.Popup == PopupType.SPECIAL)
                    {
                        fldReturn = new RelationFieldInfos(_field, _pref, getResAdv);
                        break;
                    }
                    else if (_field.Popup != PopupType.NONE)
                    {
                        fldReturn = new CatalogFieldInfos(_field, GetCatalogValues());
                        break;
                    }

                    CharFieldInfos fldCharReturn = new CharFieldInfos(_field, _pref);

                    //Pour les tables principales on identifie le champ principal de la fiche
                    // ici on triche un peu pour le mode fiche on identifie pas le champ principal qui n'a pas d'utilité dans le mode fiche
                    List<EdnType> listTableWithMainField = new List<EdnType>() { EdnType.FILE_MAIN, EdnType.FILE_PJ };
                    if (_dataFiller != null && !(_dataFiller is eFile) && listTableWithMainField.Contains(_dataFiller.ViewMainTable.EdnType))
                        fldCharReturn.IsMainField = _field.Descid == _dataFiller.ViewMainTable.MainFieldDescId;

                    fldReturn = fldCharReturn;
                    break;
                case FieldFormat.TYP_DATE:
                    fldReturn = new DateFieldInfos(_field, descAdv);
                    break;
                case FieldFormat.TYP_BIT:
                    fldReturn = new LogicFieldInfos(_field, descAdv);

                    break;
                case FieldFormat.TYP_AUTOINC:
                case FieldFormat.TYP_ID:
                    if (_field.Descid == (int)PJField.FILEID)
                    {
                        fldReturn = new PJRelationFieldInfosModel(_field, _pref, getResAdv);
                        break;
                    }

                    fldReturn = new AutoCountFieldInfos(_field);
                    break;
                case FieldFormat.TYP_MONEY:
                case FieldFormat.TYP_NUMERIC:
                    fldReturn = new NumericFieldInfos(_field, _pref, descAdv, getResAdv);
                    break;
                case FieldFormat.TYP_PJ:
                    fldReturn = new PJFieldInfos(_field);
                    break;
                case FieldFormat.TYP_EMAIL:
                    fldReturn = new MailFieldInfos(_field);
                    break;
                case FieldFormat.TYP_WEB:
                    fldReturn = new HyperLinkFieldInfos(_field);
                    break;
                case FieldFormat.TYP_USER:
                case FieldFormat.TYP_GROUP:
                    fldReturn = new UserFieldInfos(_field);
                    break;
                case FieldFormat.TYP_MEMO:
                    fldReturn = new MemoFieldInfos(_field);
                    break;
                case FieldFormat.TYP_FILE:
                    fldReturn = new FileFieldInfos(_field);
                    break;
                case FieldFormat.TYP_PHONE:
                    fldReturn = new PhoneFieldInfos(_field, _pref.SmsEnabled);
                    break;
                case FieldFormat.TYP_IMAGE:
                    fldReturn = new ImageFieldInfos(_field);
                    break;
                case FieldFormat.TYP_TITLE:
                    if (_field.Length == 1)
                        fldReturn = new SeparatorFieldInfos(_field);
                    else
                        fldReturn = new LabelFieldInfos(_field);
                    break;
                case FieldFormat.TYP_IFRAME:
                    fldReturn = new WebPageFieldInfos(_field);
                    break;
                case FieldFormat.TYP_CHART:
                    fldReturn = new ChartFieldInfos(_field);
                    break;
                case FieldFormat.TYP_BITBUTTON:
                    fldReturn = new ButtonFieldInfos(_field);
                    break;
                case FieldFormat.TYP_ALIAS:
                    fldReturn = new AliasFieldInfos(_field, _pref, getResAdv, descAdv, rgpdData);
                    break;
                case FieldFormat.TYP_SOCIALNETWORK:
                    fldReturn = new SocialNetworkFieldInfos(_field);
                    break;
                case FieldFormat.TYP_GEOGRAPHY_V2:
                    fldReturn = new GeolocationFieldInfos(_field);
                    break;
                case FieldFormat.TYP_ALIASRELATION:
                    fldReturn = new AliasRelationFieldInfos(_field, _pref, getResAdv);
                    break;
                case FieldFormat.TYP_PASSWORD:
                    fldReturn = new PasswordFieldInfos(_field);
                    break;
                case FieldFormat.TYP_HIDDEN:
                    fldReturn = new HiddenFieldInfosModel(_field);
                    break;
                case FieldFormat.TYP_BINARY:
                    fldReturn = new BinaryFieldInfosModel(_field);
                    break;
                default:
                    throw new EudoException(String.Format("Le type de champ {0} n'a pas été reconnu. (Format = {1})", _field.Descid.ToString(), _field.Format));
            }


            bool rFound = false;
            fldReturn.ToolTipText = getResAdv.GetResAdv(new KeyResADV(eLibConst.RESADV_TYPE.TOOLTIP, _field.Descid, _pref.LangId), out rFound);
            if (!rFound)
                fldReturn.ToolTipText = _field.ToolTipText;

            fldReturn.Watermark = getResAdv.GetResAdv(new KeyResADV(eLibConst.RESADV_TYPE.WATERMARK, _field.Descid, _pref.LangId), out rFound);

            fldReturn.RGPD = _rgpdM;

            fldReturn.DISPLAYINACTIONBAR = descAdv.GetAdvInfoValue(_field.Descid, DESCADV_PARAMETER.DISPLAYINACTIONBAR) == "1";


            return fldReturn;
        }

        /// <summary>
        /// renvoie le format "user friendly" du champ
        /// </summary>
        /// <returns></returns>
        public FieldType GetFieldType()
        {

            switch (_field.Format)
            {
                case FieldFormat.TYP_CHAR:

                    if (_field.Popup == PopupType.SPECIAL)
                    {
                        return FieldType.Relation;
                    }
                    else if (_field.Popup != PopupType.NONE)
                    {
                        return FieldType.Catalog;
                    }
                    return FieldType.Character;
                case FieldFormat.TYP_DATE:
                    return FieldType.Date;
                case FieldFormat.TYP_BIT:
                    return FieldType.Logic;
                case FieldFormat.TYP_AUTOINC:
                case FieldFormat.TYP_ID:
                    return FieldType.AutoCount;
                case FieldFormat.TYP_MONEY:
                    return FieldType.Money;
                case FieldFormat.TYP_NUMERIC:
                    return FieldType.Numeric;
                case FieldFormat.TYP_EMAIL:
                    return FieldType.MailAddress;
                case FieldFormat.TYP_WEB:
                    return FieldType.HyperLink;
                case FieldFormat.TYP_USER:
                    return FieldType.User;
                case FieldFormat.TYP_MEMO:
                    return FieldType.Memo;
                case FieldFormat.TYP_FILE:
                    return FieldType.File;
                case FieldFormat.TYP_PHONE:
                    return FieldType.Phone;
                case FieldFormat.TYP_IMAGE:
                    return FieldType.Image;
                case FieldFormat.TYP_TITLE:
                    return (_field.Length == 1) ? FieldType.Separator : FieldType.Label;
                case FieldFormat.TYP_IFRAME:
                    return FieldType.WebPage;
                case FieldFormat.TYP_CHART:
                    return FieldType.Chart;
                case FieldFormat.TYP_BITBUTTON:
                    return FieldType.Button;
                case FieldFormat.TYP_ALIAS:
                    return FieldType.Alias;
                case FieldFormat.TYP_SOCIALNETWORK:
                    return FieldType.SocialNetwork;
                case FieldFormat.TYP_GEOGRAPHY_V2:
                    return FieldType.Geolocation;
                case FieldFormat.TYP_ALIASRELATION:
                    return FieldType.AliasRelation;
                case FieldFormat.TYP_PASSWORD:
                    return FieldType.Password;
                case FieldFormat.TYP_PJ:
                    return FieldType.PJ;
                case FieldFormat.TYP_HIDDEN:
                    return FieldType.Hidden;
                case FieldFormat.TYP_BINARY:
                    return FieldType.Binary;
                default:
                    throw new EudoException(String.Format("Le type de champ {0} n'a pas été reconnu. (Format = {1})", _field.Descid.ToString(), _field.Format));
            }
        }
    }
    #endregion
}