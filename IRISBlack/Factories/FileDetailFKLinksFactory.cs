using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour l'initialisation de la classe FileDetailFKLinks
    /// </summary>
    public class FileDetailFKLinksFactory
    {
        #region Properties
        /// <summary>
        /// Le fichier associé
        /// </summary>
        eFile File { get; set; }
        ePref preferences { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="file">Objet eFile concerné</param>
        private FileDetailFKLinksFactory(ePref pref, eFile file)
        {
            preferences = pref;
            File = file;
        }
        #endregion

        #region static initializers
        /// <summary>
        /// Initialiseur statique de la classe avec tous les paramètres
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="file">Objet eFile concerné</param>
        /// <returns></returns>
        public static FileDetailFKLinksFactory InitFileDetailFKLinksFactory(ePref pref, eFile file)
        {
            return new FileDetailFKLinksFactory(pref, file);
        }
        #endregion
        #region Public
        /// <summary>
        /// Fonction pour créer un FileDetailFKLinksModel
        /// </summary>
        /// <returns></returns>
        public FileDetailFKLinksModel setFileDetailFKLinksModel()
        {
            FileDetailFKLinksModel fKLinks = null;
            eFileTools.eParentFileId parentId = File?.ParentFileId;

            if (parentId != null)
            {

                int? parentPP = parentId.ParentPpId;
                int? parentPM = parentId.ParentPmId;

                #region A réfléchir, pas de sens, ce sont des liaisons multiples
                //try
                //{
                //    if (File.ViewMainTable.TabType == EudoQuery.TableType.PP)
                //        parentPM = int.Parse(eFileTools.LoadPmIdFromPpId(preferences, File.FileId).FirstOrDefault());
                //}
                //catch (Exception)
                //{
                //    parentPM = parentId.ParentPpId ?? 0;
                //}
                //try
                //{
                //    if (File.ViewMainTable.TabType == EudoQuery.TableType.PM)
                //    {
                //        string strPP = eFileTools.LoadAdrFromPmId(preferences, File.FileId)
                //            .SelectMany(eFR => eFileTools.LoadPpIdFromAdrId(preferences, eFR.FileId))
                //            .FirstOrDefault();
                //        if(!string.IsNullOrEmpty(strPP))
                //            parentPP = int.Parse(strPP);
                //    }
                //}
                //catch (Exception)
                //{
                //    parentPP = parentId.ParentPpId ?? 0;
                //}
                #endregion

                fKLinks = new FileDetailFKLinksModel
                {
                    ParentPP = parentPP ?? 0,
                    ParentPM = parentPM ?? 0,
                    ParentAdr = parentId.ParentAdrId ?? 0,
                    ParentEvtDescId = parentId.ParentEvtDescId,
                    ParentEvt = parentId.ParentEvtId ?? 0
                };
            }
            return fKLinks;
        }
        #endregion
    }
}