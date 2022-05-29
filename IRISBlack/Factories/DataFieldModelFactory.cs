using Com.Eudonet.Common.DatasDirectory;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model.DataFields;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    public class DataFieldModelFactory
    {
        /// <summary>
        /// Objet eFieldRecord contenant les informations des champs
        /// </summary>
        eFieldRecord f { get; set; }
        /// <summary>
        /// Ligne d'enregistrement dont le champ est issu
        /// </summary>
        eRecord rec { get; set; }
        /// <summary>
        /// Informations concernant le câblage ORM des champs de la base
        /// </summary>
        OrmMappingInfo ormInfo { get; set; }

        #region DIctionnaires pour refactoriser les switches ultérieurement
        /// <summary>
        /// TODO : Mettre un dictionnaire à la place du switch
        /// Celui-ci utilisera de la réflection avec TYpe.getConstructor.
        /// </summary>
        Dictionary<FieldFormat, Func<eFieldRecord, IDataFieldModel>> dicTypeInst { get; set; } = new Dictionary<FieldFormat, Func<eFieldRecord, IDataFieldModel>> { };
        #endregion

        #region constructeurs
        /// <summary>
        /// Constructeur Pour la factory
        /// </summary>
        /// <param name="_f">Objet eFieldRecord contenant les informations des champs</param>
        /// <param name="_rec">Ligne d'enregistrement dont le champ est issu</param>
        /// <param name="_ormInfo">>Informations concernant le câblage ORM des champs de la base</param>
        private DataFieldModelFactory(eFieldRecord _f, eRecord _rec, OrmMappingInfo _ormInfo)
        {
            f = _f;
            rec = _rec;
            ormInfo = _ormInfo;
        }
        #endregion

        #region Initialisation statique de l'objet
        /// <summary>
        /// Initialisation statique de la classe FldTypedInfosFactory.
        /// </summary>
        /// <param name="f">Objet eFieldRecord contenant les informations des champs</param>
        /// <param name="rec">Ligne d'enregistrement dont le champ est issu</param>
        /// <param name="ormInfo">Informations concernant le câblage ORM des champs de la base</param>
        /// <returns></returns>
        public static DataFieldModelFactory initDataFieldModelFactory(eFieldRecord f, eRecord rec = null, OrmMappingInfo ormInfo = null)
        {
            return new DataFieldModelFactory(f, rec, ormInfo);
        }
        #endregion

        #region Public


        /// <summary>
        /// renvoie un objet représentant la structure de champ correctement typé
        /// </summary>
        /// <param name="pref">Objet ePref pour accédeer à des données supplémentaires si besoin</param>
        /// <returns></returns>
        public IDataFieldModel GetDataField(ePref pref)
        {
            IDataFieldModel fldReturn = null;



            switch (f.FldInfo.Format)
            {
                case FieldFormat.TYP_CHAR:
                    //on identifie ici les liaisons parentes systèmes
                    if (rec != null
                        && f.FldInfo.Table.DescId != rec.ViewTab
                        && f.FldInfo.Descid == f.FldInfo.Table.MainFieldDescId
                        )
                    {
                        fldReturn = new SystemRelationDataFieldModel(f);
                        break;
                    }
                    //on identifie ici les catalogue de type relation
                    else if (f.FldInfo.Popup == PopupType.SPECIAL)
                    {
                        fldReturn = new RelationDataFieldModel(f);
                        break;
                    }
                    else if (f.FldInfo.Popup != PopupType.NONE)
                    {
                        fldReturn = new CatalogDataFieldModel(f);
                        break;
                    }
                    fldReturn = new CharDataFieldModel(f);
                    break;
                case FieldFormat.TYP_DATE:
                    fldReturn = new DateDataFieldModel(f);

                    break;
                case FieldFormat.TYP_BIT:
                    fldReturn = new LogicDataFieldModel(f);

                    break;
                case FieldFormat.TYP_AUTOINC:
                case FieldFormat.TYP_ID:
                    if (f.FldInfo.Descid == (int)PJField.FILEID)
                    {
                        fldReturn = new PJRelationDataFieldModel(f, ((eRecordPJ)rec).PJTabDescID);
                        break;
                    }

                    fldReturn = new AutoCountDataFieldModel(f);
                    break;
                case FieldFormat.TYP_MONEY:
                    fldReturn = new MoneyDataFieldModel(f);
                    break;
                case FieldFormat.TYP_PJ:
                case FieldFormat.TYP_NUMERIC:
                    if (f.FldInfo.Descid == (int)CampaignField.RATING)
                        fldReturn = new CampaignScoreFieldModel(f);
                    else
                        fldReturn = new NumericDataFieldModel(f);
                    break;
                case FieldFormat.TYP_EMAIL:
                    fldReturn = new MailDataFieldModel(f);
                    break;
                case FieldFormat.TYP_WEB:
                    fldReturn = new HyperLinkDataFieldModel(f);
                    break;
                case FieldFormat.TYP_USER:
                case FieldFormat.TYP_GROUP:
                    fldReturn = new UserDataFieldModel(f);
                    break;
                case FieldFormat.TYP_MEMO:
                    fldReturn = new MemoDataFieldModel(f);
                    break;
                case FieldFormat.TYP_FILE:
                    fldReturn = new FileDataFieldModel(f);
                    break;
                case FieldFormat.TYP_PHONE:
                    fldReturn = new PhoneDataFieldModel(f);
                    break;
                case FieldFormat.TYP_IMAGE:
                    string root = eLibTools.GetRootPhysicalDatasPath(HttpContext.Current);
                    string path = DatasUtility.GetPhysicalDatasPath(Common.Enumerations.DatasFolderType.FILES, root, pref.GetBaseName);

                    fldReturn = new ImageDataFieldModel(f, path);
                    break;
                case FieldFormat.TYP_TITLE:
                    if (f.FldInfo.Length == 1)
                        fldReturn = new SeparatorDataFieldModel(f);
                    else
                        fldReturn = new LabelDataFieldModel(f);
                    break;
                case FieldFormat.TYP_IFRAME:
                    fldReturn = new WebPageDataFieldModel(f);
                    break;
                case FieldFormat.TYP_CHART:
                    fldReturn = new ChartDataFieldModel(f);
                    break;
                case FieldFormat.TYP_BITBUTTON:
                    fldReturn = new ButtonDataFieldModel(f);
                    break;
                case FieldFormat.TYP_ALIAS:
                    fldReturn = new AliasDataFieldModel(f);
                    break;
                case FieldFormat.TYP_SOCIALNETWORK:
                    fldReturn = new SocialNetworkDataFieldModel(f);
                    break;
                case FieldFormat.TYP_GEOGRAPHY_V2:
                    fldReturn = new GeolocationDataFieldModel(f);
                    break;
                case FieldFormat.TYP_ALIASRELATION:
                    fldReturn = new AliasRelationDataFieldModel(f);
                    break;
                case FieldFormat.TYP_PASSWORD:
                    fldReturn = new PasswordDataFieldModel(f);
                    break;
                case FieldFormat.TYP_HIDDEN:
                    fldReturn = new HiddenDataFieldModel(f);
                    break;
                case FieldFormat.TYP_BINARY:
                    fldReturn = new BinaryDataFieldModel(f);
                    break;
                default:
                    throw new EudoException(String.Format("Le type de champ {0} n'a pas été reconnu. (Format = {1})", f.FldInfo.Descid.ToString(), f.FldInfo.Format));
            }

            // Formules du milieu  de l'ORM (pas les formules du bas)
            fldReturn.HasORMFormula = ormInfo?.GetAllValidatorDescId.Contains(f.FldInfo.Descid) == true;

            return fldReturn;
        }

        #endregion
    }
}