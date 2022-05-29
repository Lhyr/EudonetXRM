using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.mgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.IRISBlack.Model;
using static Com.Eudonet.Xrm.IRISBlack.Model.FileDetailModel;
using Com.Eudonet.Common.Cryptography;
using EudoQuery;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour le contrôleur FileDetail
    /// </summary>
    public class FileDetailFactory
    {
        #region Properties
        /// <summary>
        /// Le fichier associé
        /// </summary>
        eFile File;
        int nTab { get; set; }
        ePref _pref { get; set; }
        RecordModel rm { get; set; }
        FirmDataFactory fdf { get; set; }
        StructureModel sm { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="file">Objet eFile concerné</param>
        private FileDetailFactory(eFile file)
        {
            File = file;
        }

        /// <summary>
        /// COnstructeur pour initialiser un FileDetailModel
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="pref"></param>
        /// <param name="obj"></param>
        /// <param name="rec"></param>
        /// <param name="firm"></param>
        /// <param name="strct"></param>
        private FileDetailFactory(int tab, ePref pref, RecordModel rec, FirmDataFactory firm, StructureModel strct)
        {
            nTab = tab;
            _pref = pref;
            rm = rec;
            fdf = firm;
            sm = strct;
        }
        #endregion

        #region static initializers
        /// <summary>
        /// Initialiseur statique de la classe avec tous les paramètres
        /// </summary>
        /// <param name="file">Objet eFile concerné</param>
        /// <returns></returns>
        public static FileDetailFactory InitFileDetailFactory(eFile file)
        {
            return new FileDetailFactory(file);
        }

        /// <summary>
        /// Initialiseur statique de la classe pour initialiser un FileDetailModel
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="pref"></param>
        /// <param name="obj"></param>
        /// <param name="rec"></param>
        /// <returns>FileDetailFactory</returns>
        public static FileDetailFactory InitFileDetailFactory(int tab, ePref pref, RecordModel rec, FirmDataFactory firm, StructureModel strct)
        {
            return new FileDetailFactory(tab, pref, rec, firm, strct);
        }

        #endregion

        #region Public

        /// <summary>
        /// Renvoie le hash à utiliser pour eGotoFile
        /// </summary>
        /// <returns>True ou false</returns>
        public string GetFileHash()
        {
            return HashSHA.GetHashSHA1(String.Concat("EUD0N3T", "tab=", File.ViewMainTable.DescId, "&fid=", File.FileId, "XrM")); // US #6 - pour la génération du lien Partage eGotoFile
        }

        /// <summary>
        /// FOnction pour créer un FileDetailModel
        /// </summary>
        /// <returns></returns>
        public FileDetailModel setFileDetailModel(FileDetailFKLinksModel lnkId)
        {
            sm.WebDataPath = eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.ROOT, _pref.GetBaseName);

            int nRev;

            if (!int.TryParse(eConst.REVISION, out nRev))
            {
                // On convertit la révision au format date (version DEV) en un int exploitable côté JavaScript
                // Int32.MaxValue étant de 2147483647, ça nous laisse la possibilité d'utiliser 4 + 2 + 2 + 2 chiffres maximum jusqu'en 2147
                // On construit donc une String à ce format que l'on reconvertit en Int
                DateTime dRev ;

                if (!(DateTime.TryParseExact(eConst.REVISION, "yyyy/MM/dd-HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dRev)
                    && int.TryParse(dRev.ToString("yyyyMMddhh"), out nRev))) {
                    nRev = int.MaxValue;
                    EudoException eudoEx = new EudoException("FileDetailFactory.setFileDetailModel : Erreur de conversion du numéro de révision !");
                    eLogEvent.Log(eudoEx, System.Diagnostics.EventLogEntryType.Warning);
                } 
            }

            return new FileDetailModel
            {
                Structure = sm,
                Data = rm,
                SireneMapping = fdf.SireneFactory(),
                PredictiveAddressMapping = fdf.DataGouvFactory(),
                FdLinksId = lnkId,
                BaseName = _pref.GetBaseName,
                Revision = nRev,
                actDetail = ActionDetailFactory.initActionDetailFactory(nTab, _pref).getActionDetailModel()
            };
            }
            #endregion
        }
    }