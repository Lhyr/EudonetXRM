using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.DataFields;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour construire l'objet record Model.
    /// </summary>
    public class RecordFactory
    {
        eRecord record { get; set; }
        protected eDataFiller eDataFill { get; set; }
        protected ePref preference { get; set; }

        #region constructeurs
        /// <summary>
        /// Constructeur Pour la factory
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="dtf"></param>
        /// <param name="pref"></param>
        protected RecordFactory(eRecord rec, eDataFiller dtf, ePref pref)
        {
            record = rec;
            eDataFill = dtf;
            preference = pref;
        }
        /// <summary>
        /// Constructeur pour les factories qui héritent de celle-ci.
        /// Avec juste le dataFiller et les pref.
        /// </summary>
        /// <param name="dtf"></param>
        /// <param name="pref"></param>
        protected RecordFactory(eDataFiller dtf, ePref pref)
        {
            eDataFill = dtf;
            preference = pref;
        }
        #endregion

        #region Initialisation statique de l'objet
        /// <summary>
        /// Initialisation statique de la classe Recordfactory.
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="dtf"></param>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static RecordFactory InitRecordFactory(eRecord rec, eDataFiller dtf, ePref pref)
        {
            return new RecordFactory(rec, dtf, pref);
        }
        #endregion

        #region public
        /// <summary>
        /// fonction permettant la construction du Model et son retour.
        /// </summary>
        public virtual IRecordModel ConstructRecordModel()
        {
            if (record == null)
                throw new EudoException("L'enregistrement est introuvable");

            // Infos ORM - cf. eRenderer.cs
            OrmMappingInfo ormInfo = eLibTools.OrmLoadAndGetMapWeb(preference);

            #region Liste des champs à exclure (US #2 990 - Tâche #4 858)
            // On exclut les champs situés sous un titre séparateur masqué
            List<eFieldRecord> lDescIdsToExclude = new List<eFieldRecord>();
            // On liste d'abord les titres séparateurs masqués et non masqués, en les triant dans l'ordre d'affichage (Y puis X)
            List<eFieldRecord> lSeparators = record.GetFields
                .Where(f => f.FldInfo.Format == FieldFormat.TYP_TITLE && f.FldInfo.Length == 1)
                .OrderBy(f => f.FldInfo.Y).ThenBy(f => f.FldInfo.X).ToList();
            // Puis, on exclut chaque champ situé après (Y) un titre séparateur masqué, jusqu'au prochain titre séparateur
            for (int i = 0; i < lSeparators.Count; i++)
            {
                // Si le titre séparateur est masqué...
                if (!lSeparators[i].RightIsTableVisible || !lSeparators[i].RightIsVisible)
                {
                    // On masque tous les champs situés en-dessous (Y) s'il est le dernier titre séparateur affiché
                    if (i == lSeparators.Count - 1)
                        lDescIdsToExclude.AddRange(record.GetFields.Where(f => f.FldInfo.Y >= lSeparators[i].FldInfo.Y));
                    // Sinon, on masque tous les champs situés en-dessous (Y) jusqu'au prochain titre séparateur
                    else
                        lDescIdsToExclude.AddRange(record.GetFields.Where(f => f.FldInfo.Y >= lSeparators[i].FldInfo.Y && f.FldInfo.Y < lSeparators[i + 1].FldInfo.Y));
                }
            }
            #endregion

            IRecordModel recMo = new RecordModel
            {
                LstDataFields = record.GetFields
                    .Where(f => f.FldInfo.PermViewAll && !lDescIdsToExclude.Contains(f))
                    .Select(f => DataFieldModelFactory.initDataFieldModelFactory(f, record, ormInfo).GetDataField(preference)),
                MainFileId = record.MainFileid,
                MainFileLabel = record.MainFileLabel,
                RightIsUpdatable = record.RightIsUpdatable,
                RightIsDeletable = record.RightIsDeletable
            };


            recMo.MenuShortcut = new FieldsMenuShortcutModel
            {
                EmailShortcut = new EmailShortcutModel { Icon = "icon-email", Fields = recMo.LstDataFields.OfType<MailDataFieldModel>().Where(dt => !string.IsNullOrEmpty(dt.Value)) },
                GeoLocationShortcut = new GeoLocationShortcutModel { Icon = "icon-map-marker", Fields = recMo.LstDataFields.OfType<GeolocationDataFieldModel>().Where(dt => !string.IsNullOrEmpty(dt.Value)) },
                HyperLinkShortcut = new HyperLinkShortcutModel { Icon = "icon-site_web", Fields = recMo.LstDataFields.OfType<HyperLinkDataFieldModel>().Where(dt => !string.IsNullOrEmpty(dt.Value)) },
                PhoneShortcut = new PhoneShortcutModel { Icon = "icon-phone", Fields = recMo.LstDataFields.OfType<PhoneDataFieldModel>().Where(dt => !string.IsNullOrEmpty(dt.Value)) },
                SocialNetworkShortcut = recMo.LstDataFields.OfType<SocialNetworkDataFieldModel>()
                .Where(dt => !string.IsNullOrEmpty(dt.Value))
                .GroupBy(key => key.Icon, (key, social)
                    => new SocialNetworkShortcutModel { Icon = eFontIcons.GetFontClassName(key), Fields = social }),
                VCardShortcut = new VCardShortcutModel { Icon = "icon-vcard", EncryptedLink = CryptoEudonet.Encrypt(recMo.MainFileId.ToString(), CryptographyConst.KEY_CRYPT_LINK2) }
            };

            #region icone et couleur conditionnelles

            if (record.IsHisto && !String.IsNullOrEmpty(eDataFill.HistoInfo.Icon))
            {
                recMo.BGColor = eDataFill.HistoInfo.BgColor;
                recMo.Icon = eFontIcons.GetFontClassName(eDataFill.HistoInfo.Icon);
                recMo.Color = eDataFill.HistoInfo.Color;
            }
            else if (record.RuleColor.HasRuleColor && !String.IsNullOrEmpty(record.RuleColor.Icon))
            {
                recMo.BGColor = record.RuleColor.BgColor;
                recMo.Icon = eFontIcons.GetFontClassName(record.RuleColor.Icon);
                recMo.Color = record.RuleColor.Color;
            }
            else
            {
                recMo.Icon = eFontIcons.GetFontClassName(eDataFill.ViewMainTable.GetIcon);
                if (!String.IsNullOrEmpty(eDataFill.ViewMainTable.GetIconColor))
                    recMo.Color = eDataFill.ViewMainTable.GetIconColor;
            }
            #endregion

            #region Pièces jointes

            eRecordPJ recPJ = record as eRecordPJ;
            if (recPJ != null)
            {
                CheckPJExistsFactory pjFact = CheckPJExistsFactory.initCheckPJExistsFactory(preference,
                    eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, preference.GetBaseName));

                recMo.PJInfo = new PJUploadInfoModel
                {
                    PjId = record.MainFileid,
                    PJSrcUrl = pjFact.GetSecuredPJURL(recPJ),
                    PJType = recPJ.PjType,
                    PJTabDescID = recPJ.PJTabDescID,
                    PJFileID = recPJ.PJFileID,
                    PJTabType = recPJ.PJTabType,
                    PJCnt = recPJ.PjCnt,
                    PJToolTip = recPJ.ToolTip,
                    IsPJDeletable = recPJ.IsPJDeletable,
                    IsPJUpdatable = recPJ.IsPJUpdatable,
                    IsPJViewable = recPJ.IsPJViewable
                };
            }
            #endregion

            #region US #4315 - Droit de modification de la zone Assistant du nouveau mode Fiche Eudonet X
            eudoDAL dal = eLibTools.GetEudoDAL(preference);
            try
            {
                dal.OpenDatabase();
                recMo.CanUpdateWizardBar = eLibDataTools.IsTreatmentAllowed(dal, preference.User, eDataFill.CalledTabDescId, ProcessRights.PRC_RIGHT_UPDATE_EUDONETX_PROGRESS_AREA); // US #4315
            }
            finally
            {
                dal?.CloseDatabase();
            }
            #endregion

            return recMo;
        }
        #endregion

    }
}