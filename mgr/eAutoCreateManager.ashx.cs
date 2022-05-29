using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Génère une fiche à partir du masque de nouvelle fiche fourni par eudoquery.
    /// </summary>
    public class eAutoCreateManager : eEngineMgr
    {
        /// <summary>
        /// contenu du traitement de création automatique de fiche.
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                //Table
                Int32? nTab = _requestTools.GetRequestFormKeyI("tab");
                //Table parente (cas de création depuis un signet)
                Int32 nParentTab = _requestTools.GetRequestFormKeyI("parenttab") ?? 0;
                Int32 nParentFileId = _requestTools.GetRequestFormKeyI(nParentTab.ToString()) ?? 0;

                //Retour d'erreur si tab n'est pas renseigné
                if (nTab == null)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 6237),
                        eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                        eResApp.GetRes(_pref, 72).Replace(" <PARAM> ", " "),
                        "La table dans laquelle le nouvel enregistrement doit être créé n'a pas été précisée."
                        );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }

                Dictionary<Int32, String> dicValues = new Dictionary<Int32, String>();
                ExtendedDictionary<String, Object> dicParams = new ExtendedDictionary<String, Object>();

                Engine.Engine eng = eModelTools.GetEngine(_pref, nTab.Value, eEngineCallContext.GetCallContext(EngineContext.APPLI));

                // Recup des informations des liaisons parentes par leur descid en key directement
                eFileTools.eParentFileId efPrtIdDirect = _requestTools.GetRequestFormDidParent();
                // Recup des informations des liaisons parentes par la clé lnkid
                eFileTools.eParentFileId efPrtIdLnk = _requestTools.GetRequestFormLnkIds();

                // On ne donne pas la priorité au param lnkid
                eFileTools.eParentFileId efPrtId = eFileTools.GetParentFileIdMerge(efPrtIdDirect, efPrtIdLnk);

                if (!efPrtId.IsEmpty)
                {
                    if (efPrtId.HasParentLnk(TableType.PP))
                    {
                        eng.AddTabValue(TableType.PP.GetHashCode(), efPrtId.ParentPpId.Value);
                        dicValues.Add(TableType.PP.GetHashCode(), efPrtId.ParentPpId.Value.ToString());
                    }

                    if (efPrtId.HasParentLnk(TableType.PM))
                    {
                        eng.AddTabValue(TableType.PM.GetHashCode(), efPrtId.ParentPmId.Value);
                        dicValues.Add(TableType.PM.GetHashCode(), efPrtId.ParentPmId.Value.ToString());

                        // On indique que c'est bien une adresse pro
                        dicValues.Add(AdrField.PERSO.GetHashCode(), "0");
                    }

                    if (efPrtId.HasParentLnk(TableType.ADR))
                    {
                        eng.AddTabValue(TableType.ADR.GetHashCode(), efPrtId.ParentAdrId.Value);
                        dicValues.Add(TableType.ADR.GetHashCode(), efPrtId.ParentAdrId.Value.ToString());
                    }

                    if (efPrtId.HasParentLnk(TableType.EVENT))
                    {
                        eng.AddTabValue(efPrtId.ParentEvtDescId, efPrtId.ParentEvtId.Value);
                        dicValues.Add(efPrtId.ParentEvtDescId, efPrtId.ParentEvtId.Value.ToString());
                    }
                }

                //liaison custom
                if (nParentTab > 0 && nParentFileId > 0)
                {
                    int iRelationFieldDescId = _requestTools.GetRequestFormKeyI(String.Format("{0}_spclnk", nParentTab)) ?? 0;
                    if (iRelationFieldDescId > 0)
                        dicValues.Add(iRelationFieldDescId, nParentFileId.ToString());
                }
                //File Context
                eFileTools.eFileContext ef = new eFileTools.eFileContext(efPrtId, _pref.User, nTab.Value, nParentTab);
                dicParams.Add("filecontext", ef);

                dicParams.AddContainsKey("dicvalues", dicValues);

                eFile ghostFile = eFileMain.CreateMainFile(_pref, nTab.Value, 0, EudoQuery.ActiveBkm.NONE.GetHashCode(), dicParams);
                if (ghostFile.ErrorMsg.Length > 0)
                {
                    String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                    if (ghostFile.InnerException != null)
                        sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ghostFile.InnerException.Message, Environment.NewLine, "Exception StackTrace :", ghostFile.InnerException.StackTrace);
                    else
                        sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", ghostFile.ErrorMsg);

                    ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                       String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                       eResApp.GetRes(_pref, 72),  //   titre
                       String.Concat(sDevMsg));

                    LaunchError();
                }

                // #65490 : Cas d'une création d'adresse depuis PM à partir d'un PP existant
                bool bAddAddrFromPMWithExistingPP =
                    ef.Tab == (int)TableType.ADR
                    && ef.TabFrom == (int)TableType.PM
                    && ef.ParentFileId.ParentPpId > 0;

                foreach (eFieldRecord fld in ghostFile.Record.GetFields)
                {
                    if (fld.FldInfo.Table.DescId != nTab)
                        continue;

                    if (fld.FldInfo.Descid == (int)XrmHomePageField.Title)
                    {
                        eng.AddNewValue((int)XrmHomePageField.Title, eResApp.GetRes(_pref, 1560)); // Nouvelle page d'accueil
                        continue;
                    }

                    // Si le champ n'est pas modifiable/visible et qu'il n'y a pas de valeur par défaut et qu'il n'est pas dans le cas de report de champs depuis PM et PP, on ne fait rien
                    if (
                            (!fld.RightIsVisible || !fld.RightIsUpdatable)
                            && (String.IsNullOrEmpty(fld.FldInfo.DefaultValue) && !bAddAddrFromPMWithExistingPP)
                        )
                        continue;

                    if (fld.FldInfo.Format == FieldFormat.TYP_ALIASRELATION)
                        continue;

                    eng.AddNewValue(fld.FldInfo.Descid, fld.Value);
                }



                // TODO - DOIT-ON GERER LES FORMULES DU MILIEU ?
                eng.EngineProcess(new StrategyCruSimple());

                _engResult = eng.Result;

                // TODO Déplacer vers admin Create

                if (_pref.AdminMode)
                {
                    if (nTab.Value == (int)TableType.XRMHOMEPAGE)
                    {
                        // Création des lignes dans RES_FILES pour Title et Tooltip
                        List<eSqlResFiles> listRes = new List<eSqlResFiles>()
                        {
                            new eSqlResFiles(((int)TableType.XRMHOMEPAGE), (int)XrmHomePageField.Title, _engResult.NewRecord.FilesId[0], 0, eResApp.GetRes(_pref, 1560)),
                            new eSqlResFiles(((int)TableType.XRMHOMEPAGE), (int)XrmHomePageField.Tooltip, _engResult.NewRecord.FilesId[0], 0, string.Empty)
                        };
                        if (!eSqlResFiles.UpdateFileResList(_pref, listRes, out _sMsgError))
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", "La mise à jour des traductions a échoué", "", _sMsgError);
                            LaunchError();
                        }

                        // Creation de la grille quand on crée une page d'accueil
                        Engine.Engine engGrid = eModelTools.GetEngine(_pref, (int)TableType.XRMGRID, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                        engGrid.FileId = 0;
                        engGrid.AddParam("DatabaseUid", _pref.DatabaseUid);
                        engGrid.AddParam("ExternalUrl", _pref.AppExternalUrl);
                        engGrid.AddNewValue((int)XrmGridField.Title, eResApp.GetRes(_pref, 7977));
                        engGrid.AddNewValue((int)XrmGridField.ParentTab, ((int)TableType.XRMHOMEPAGE).ToString());
                        engGrid.AddNewValue((int)XrmGridField.ParentFileId, (_engResult.NewRecord.FilesId[0]).ToString());
                        engGrid.AddNewValue((int)XrmGridField.DisplayOrder, "0");
                        engGrid.EngineProcess(new StrategyCruSimple());

                        // Création echouée
                        if (engGrid.Result.Error != null && eng.Result.Error.Msg.Length != 0)
                        {
                            ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, engGrid.Result.Error.Msg, engGrid.Result.Error.Detail, engGrid.Result.Error.Title);
                            LaunchError();
                        }
                        else
                        {
                            // Création des lignes dans RES_FILES pour Title et Tooltip
                            listRes = new List<eSqlResFiles>()
                            {
                                new eSqlResFiles(((int)TableType.XRMGRID), (int)XrmGridField.Title, engGrid.Result.NewRecord.FilesId[0], 0, eResApp.GetRes(_pref, 7977))
                                //new eSqlResFiles(((int)TableType.XRMGRID), (int)XrmGridField.Tooltip, engGrid.Result.NewRecord.FilesId[0], 0, string.Empty),
                            };
                            if (!eSqlResFiles.UpdateFileResList(_pref, listRes, out _sMsgError))
                            {
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", "La mise à jour des traductions a échoué", "", _sMsgError);
                                LaunchError();
                            }
                        }


                    }
                }

            }
            catch (eEndResponseException)
            {


            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 2024).Replace("<PARAM>", ""), string.Concat("Autocreate :", ex.Message), ex.ToString());
                LaunchError();
            }

            DoResponse();
        }
    }
}