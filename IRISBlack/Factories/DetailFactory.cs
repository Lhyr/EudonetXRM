using System;
using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Core.Model.Teams;
using System.Web;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using System.Threading.Tasks;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory qui permet de créer des modèles pour les 
    /// fiches en mode fiche ou en mode fiche inscrustée.
    /// </summary>
    public class DetailFactory
    {


        #region Properties
        /// <summary>
        /// Le fichier associé
        /// </summary>
        eFile File;
        int Tab { get; set; }
        ePref Pref { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="file">Objet eFile concerné</param>
        /// <param name="_pref"></param>
        /// <param name="nTab"></param>
        private DetailFactory(eFile file, ePref _pref, int nTab)
        {
            File = file;
            Pref = _pref;
            Tab = nTab;
        }

        #endregion

        #region static initializers
        /// <summary>
        /// Initialiseur statique de la classe avec tous les paramètres
        /// </summary>
        /// <param name="file">Objet eFile concerné</param>
        /// <param name="_pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static DetailFactory InitDetailFactory(eFile file, ePref _pref, int nTab)
        {
            return new DetailFactory(file, _pref, nTab);
        }


        #endregion

        #region Public
        /// <summary>
        /// Va retourner les informations demandées au travers d'un fileDetailModel, contenant tous les composants de la page
        /// et leurs données.
        /// </summary>
        /// <param name="File"></param>
        /// <returns></returns>
        public async Task<FileDetailModel> CreateFileDetailModelFromFile()
        {
            //Liste des exceptions mineures qu'on enverra qu'en local ou lors de la sollicitation par une ip de courbevoie
            List<Exception> liExceptions = new List<Exception>();

            // TODO: message adapté
            if (File != null && (File.InnerException != null || File.ErrorMsg.Length > 0))
            {
                if (eLibTools.IsLocalOrEudoMachine())
                {
                    if (File.InnerException != null)
                        throw File.InnerException;

                    if (File.ErrorMsg.Length > 0)
                        throw new EudoException(File.ErrorMsg);
                }
                else
                {
                    throw new EudoException("Une erreur est survenue lors de la génération de la fiche");
                }
            }

            if (File == null || File.ListRecords == null || File.ListRecords.Count == 0)
                throw new EudoException("L'enregistrement est introuvable");

            FileDetailFKLinksModel lnkId;
            RecordModel rm;
            FirmDataFactory fdf;
            FileDetailModel fileDetail;
            FileDetailModel.StructureModel sm;
            IDictionary<int, RGPDModel> rgpd;
            eTeamsMapping teamsMapping;


            var ieFields = File.FldFieldsInfos;
            var ieDescId = ieFields
                .Select(fld => fld.Descid)
                .Union(ieFields.Where(fld => fld.AliasSourceField != null).Select(fld => fld.AliasSourceField.Descid))
                .Union(ieFields.Select(fld => eLibTools.GetTabFromDescId(fld.Descid)));

            try
            {
                lnkId = FileDetailFKLinksFactory.InitFileDetailFKLinksFactory(Pref, File).setFileDetailFKLinksModel();
                rm = RecordFactory.InitRecordFactory(File.Record, File, Pref).ConstructRecordModel() as RecordModel;
                fdf = FirmDataFactory.InitFirmDataFactory(Tab, Pref);
                rgpd = RGPDFactory.initRGPDFactory(Pref).GetRGPDData(ieDescId);
                sm = await StructureModelForFileFactory.InitStructureModelForFileFactory(File).GetStructureModelForFile(rgpd);
                fileDetail = FileDetailFactory.InitFileDetailFactory(Tab, Pref, rm, fdf, sm).setFileDetailModel(lnkId);

            }
            catch (Exception ex)
            {
                throw new EudoException("Une erreur est survenue lors de la génération de la fiche", "", ex);
            }

            eParam eParam = new eParam(Pref);
            string sMajMRU = string.Empty;

            if (!eParam.SetTableMru(Pref, Tab, File.FileId , out sMajMRU))
            {
                liExceptions.Add(new EudoException(sMajMRU, "Une erreur est survenue lors de la mise à jour des MRU"));
            }


            if (liExceptions.Count > 0 && eLibTools.IsLocalOrEudoMachine())
                fileDetail.Errors = liExceptions;

            try
            {
                //attribution de l'action spécifique pour les boutons impliqués dans la création d'event dans teams
                //la méthode ci-dessous renvoie un objet par défaut il n'est donc pas sensé être null
                teamsMapping = eTeamsFactory.GetMapping(Pref, Tab, true);

                if (teamsMapping.Enabled && teamsMapping.IsReady())
                {
                    IFldTypedInfosModel fBtnCreateSave = fileDetail.Structure.LstStructFields.Find(f => f.DescId == teamsMapping.CreateSaveBtnDescId);
                    if (fBtnCreateSave?.Format == FieldType.Button)
                    {
                        ButtonFieldInfos btnCreateSave = (ButtonFieldInfos)fBtnCreateSave;
                        btnCreateSave.SpecificAction = BtnSpecificAction.CreateSaveTeamsEvent;
                    }

                    IFldTypedInfosModel fBtnDelete = fileDetail.Structure.LstStructFields.Find(f => f.DescId == teamsMapping.DeleteBtnDescId);
                    if (fBtnDelete?.Format == FieldType.Button)
                    {
                        ButtonFieldInfos btnDelete = (ButtonFieldInfos)fBtnDelete;
                        btnDelete.SpecificAction = BtnSpecificAction.DeleteTeamsEvent;
                    }

                }


            }
            catch (Exception)
            {
                //si on est en local ou depuis une ip eudo on va vouloir debugguer
                if (eLibTools.IsLocalOrEudoMachine())
                    throw;

                //en release on ne bloque pas tout l'affichage pour ça mais en debug on va vouloir l'erreur aussi
#if DEBUG
                throw;
#endif
            }
            return fileDetail;
        }
        #endregion

    }
}