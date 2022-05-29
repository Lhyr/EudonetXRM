using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// gestion de la mise à jour des spécifs
    /// </summary>
    public class eAdminSpecifManager : eAdminManager
    {

        /// <summary>
        /// gestion de la mise à jour des spécifs
        /// </summary>
        protected override void ProcessManager()
        {


            #region XML Return


            SpecifTreatmentResult result = new SpecifTreatmentResult();

            #endregion

            Int32 nSpecifId;

            try
            {

                //Désérialisation
                eAdminCapsuleSpecif<eAdminUpdateProperty> caps = null;
                try
                {
                    caps = eAdminTools.GetAdminCapsule<eAdminCapsuleSpecif<eAdminUpdateProperty>, eAdminUpdateProperty>(_context.Request.InputStream);
                }
                catch (Exception e)
                {
                    throw new EudoAdminParameterException("Lecture de capsule de mise à jour de spécif impossible.");
                }


                nSpecifId = caps.SpecifId;
                result.SpecifId = nSpecifId;
                result.Tab = caps.DescId;
                bool bNew = caps.SpecifId == 0;


                #region Validation params

                //Récupération des propriétés à maj
                List<SetParam<eLibConst.SPECIFS>> liParams = new List<SetParam<eLibConst.SPECIFS>>();

                Int32 nPpty = 0;
                string sValue = String.Empty;
                bool bMajDisporder = false;
                try
                {
                    foreach (eAdminUpdateProperty pty in caps.ListProperties)
                    {
                        nPpty = pty.Property;
                        sValue = pty.Value;

                        eAdminUpdateProperty.CATEGORY cat = (eAdminUpdateProperty.CATEGORY)pty.Category;
                        switch (cat)
                        {
                            case eAdminUpdateProperty.CATEGORY.SPECIFS:

                                //la modif de disporder est un cas particulier, traité différement
                                if ((eLibConst.SPECIFS)nPpty == eLibConst.SPECIFS.DISPORDER)
                                {
                                    bMajDisporder = true;
                                    eSpecif.MajDisporder(_pref, caps.DescId, caps.SpecifId, Int32.Parse(sValue), true);

                                    continue;
                                }

                                liParams.Add(new SetParam<eLibConst.SPECIFS>((eLibConst.SPECIFS)nPpty, sValue));

                                break;
                            default:
                                liParams.Add(new SetParam<eLibConst.SPECIFS>((eLibConst.SPECIFS)nPpty, sValue));
                                break;
                        }
                    }
                }
                catch (InvalidCastException ex)
                {
                    //paramètres non valide
                    throw new Exception(String.Concat("Le paramètre  [", nPpty, "] valeur [", sValue, "]  n'est pas valide : ", ex.Message));
                }
                catch (Exception ex)
                {
                    // Autre erreurs
                    throw new Exception(String.Concat("Le paramètre  [", nPpty, "] valeur [", sValue, "]  n'est pas valide : ", ex.Message));
                }
                #endregion


                if (bNew)
                {
                    #region Nouvelle spécif

                    //eSpecif specif = new eSpecif();
                    String url = string.Empty;
                    String urlParam = string.Empty;
                    eLibConst.SPECIF_TYPE eType = eLibConst.SPECIF_TYPE.TYP_UNSPECIFIED;
                    eLibConst.SPECIF_SOURCE eSource = eLibConst.SPECIF_SOURCE.SRC_XRM;
                    eLibConst.SPECIF_OPENMODE eOpenMode = eLibConst.SPECIF_OPENMODE.UNSPECIFIED;
                    String sLabel = string.Empty;
                    Int32 nTab = 0;

                    foreach (SetParam<eLibConst.SPECIFS> setparam in liParams)
                    {
                        switch (setparam.Option)
                        {
                            case eLibConst.SPECIFS.URL: url = setparam.Value; break;
                            case eLibConst.SPECIFS.URLPARAM: urlParam = setparam.Value; break;
                            case eLibConst.SPECIFS.SPECIFTYPE: eType = (eLibConst.SPECIF_TYPE)(eLibTools.GetNum(setparam.Value)); break;
                            case eLibConst.SPECIFS.OPENMODE: eOpenMode = (eLibConst.SPECIF_OPENMODE)(eLibTools.GetNum(setparam.Value)); break;
                            case eLibConst.SPECIFS.LABEL: sLabel = setparam.Value; break;
                            case eLibConst.SPECIFS.TAB: nTab = eLibTools.GetNum(setparam.Value); break;
                        }
                    }

                    eSpecif specif = new eSpecif(0, url, urlParam, eType, eSource, eOpenMode, sLabel, nTab, 0);
                    specif.CreateSpecif(_pref);

                    result.SpecifId = specif.SpecifId;
                    result.Success = true;

                    #endregion
                }
                else
                {


                    // Mise à jour des SPECIFS
                    if (liParams.Count > 0)
                    {
                        try
                        {

                            eSpecif eMySpec = eSpecif.GetSpecif(_pref, caps.SpecifId);

                            try
                            {
                                if (eSpecif.UpdateSpecif(_pref, eMySpec, liParams))
                                {
                                    result.Success = true;
                                }
                                else
                                {
                                    result.Success = false;
                                }
                            }
                            catch (Exception e)
                            {

                                result.Success = false;
                                result.ErrorMessage = e.Message;
                            }

                        }
                        catch (Exception e)
                        {
                            result.Success = false;
                            result.ErrorMessage = e.Message;
                        }


                    }
                    else if (!bMajDisporder)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Aucun paramètre fourni.";

                    }
                    result.Success = true;
                }



            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrorMessage = e.Message;

            }

            RenderResult(RequestContentType.TEXT, delegate ()
            {

                return JsonConvert.SerializeObject(result);

            });
        }






    }
}