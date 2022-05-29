using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.xrm.eda;
using EudoQuery;
using Newtonsoft.Json;
using EudoEnum = Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Gestionnaire des demandes de maj de confiadv en admin
    /// </summary>
    public class eAdminConfigAdvManager : eAdminManager
    {
        /// <summary>
        /// traitement de la demande
        /// </summary>
        protected override void ProcessManager()
        {


            eAdminCapsule<eAdminCAdvUpdateProperty> caps = null;
            try
            {
                caps = eAdminTools.GetAdminCapsule<eAdminCapsule<eAdminCAdvUpdateProperty>, eAdminCAdvUpdateProperty>(_context.Request.InputStream);
            }
            catch (Exception e)
            {
                // TODO: gestion d'erreur
                throw e;
            }

            // Parcours des propriétés CONFIG
            List<SetCAdvParam> listConfig = new List<SetCAdvParam>();
            foreach (eAdminCAdvUpdateProperty pty in caps.ListProperties)
            {
                if (pty.Category != eAdminUpdateProperty.CATEGORY.CONFIGADV.GetHashCode())
                    continue;

                EudoEnum.CONFIGADV param = EudoEnum.CONFIGADV.UNDEFINED;
                eLibConst.CONFIGADV_CATEGORY cat = eLibConst.CONFIGADV_CATEGORY.UNDEFINED;

                if (!Enum.TryParse<EudoEnum.CONFIGADV>(pty.Property.ToString(), out param) || param == EudoEnum.CONFIGADV.UNDEFINED)
                    continue;

                if (!Enum.TryParse<eLibConst.CONFIGADV_CATEGORY>(pty.CAdvCategory.ToString(), out cat) || cat == eLibConst.CONFIGADV_CATEGORY.UNDEFINED)
                    continue;

                if (param == EudoEnum.CONFIGADV.COUNTRY && pty.Value == "")
                {
                    try
                    {
                        eLibTools.DeleteConfigAdvValue(_pref, EudoEnum.CONFIGADV.COUNTRY);
                        continue;
                    }
                    catch (Exception e)
                    {
                        LaunchError(
                            eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                e.Message,
                                ""
                            )
                        );
                    }
                }

                //Passage unicode réservé au superadmin+
                if (param == EudoEnum.CONFIGADV.FULL_UNICODE)
                {
                    if (!caps.Confirmed)
                    {
                        eAdminResult res = new eAdminResult();
                        res.NeedConfirm = true;
                        res.Success = true;
                        res.UserErrorMessage = eResApp.GetRes(_pref, 2482); //Cette modification va entrainer des modifications importantes....

                        RenderResult(RequestContentType.TEXT, delegate ()
                        {
                            return JsonConvert.SerializeObject(res);
                        });

                        return;
                    }


                    if (_pref.User.UserLevel < 100)
                        throw new EudoAdminInvalidRightException();



                }
                else if (param == EudoEnum.CONFIGADV.PASSWORD_POLICIES_ALGO)
                {

                    if (pty.Value == "1")
                        pty.Value = PASSWORD_ALGO.ARGON2.GetHashCode().ToString();
                    else
                        pty.Value = PASSWORD_ALGO.MD5.GetHashCode().ToString();

                    if (!caps.Confirmed && PASSWORD_ALGO.ARGON2 == eLibTools.GetEnumFromCode<PASSWORD_ALGO>(pty.Value))
                    {
                        eAdminResult res = new eAdminResult();
                        res.NeedConfirm = true;
                        res.Success = true;
                        res.UserErrorTitle = eResApp.GetRes(_pref, 2607); // Activer le chiffrement renforcé du mot de passe utilisateur
                        res.UserErrorMessage = eResApp.GetRes(_pref, 2606); //L'activation du chiffrement renforcé n'est pas compatible avec les modules EudoDrop, ancien EudoImport, SDK, Synchro Agenda/Contact et 1ère version d'EudoPOP. Le mot de passe des utilisateurs sera renforcé à leur prochaine connexion, leur niveau de sécurité passera à Fort. Les utilisateurs qui ne se sont pas reconnectés et dont le niveau de sécurité est resté à Moyen devront être modifié manuellement afin que leur niveau de sécurité soit renforcé.

                        RenderResult(RequestContentType.TEXT, delegate ()
                        {
                            return JsonConvert.SerializeObject(res);
                        });

                        return;
                    }


                    if (_pref.User.UserLevel < 100)
                        throw new EudoAdminInvalidRightException();


                }
                else if (param == EudoEnum.CONFIGADV.PASSWORD_POLICIES_CASE_SENSITIVE && pty.Value == "1")
                {

                    if (!caps.Confirmed)
                    {
                        eAdminResult res = new eAdminResult();
                        res.NeedConfirm = true;
                        res.Success = true;

                        res.UserErrorMessage = eResApp.GetRes(_pref, 6975).Replace(@"\n", "<br/>"); //Attention, les utilisateurs devront saisir leur mot de passe actuel en majuscule.\nPar ailleurs, les applications hors Eudonet pourraient ne plus fonctionner.\nConfirmez-vous la modification ?

                        RenderResult(RequestContentType.TEXT, delegate ()
                        {
                            return JsonConvert.SerializeObject(res);
                        });

                        return;
                    }

                    if (_pref.User.UserLevel < 100)
                        throw new EudoAdminInvalidRightException();


                }


                listConfig.Add(new SetCAdvParam(param, pty.Value, cat));

            }
            if (listConfig.Count > 0)
            {
                eAdminConfigAdv adminCfgAdv = new eAdminConfigAdv(_pref);
                eAdminResult res = adminCfgAdv.SetParam(listConfig);

                if (!res.Success)
                {
                    LaunchError(
                        eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            res.UserErrorMessage,
                            "",
                            res.DebugErrorMessage
                        )
                    );
                }
                else
                {
                    //Rechargement des préférences
                    _pref.LoadConfigAdv();
                    res.Result.AddRange(caps.ListProperties);
                   
                }

                RenderResult(RequestContentType.TEXT, delegate ()
                {
                    return JsonConvert.SerializeObject(res);
                });
            }


        }

    }
}