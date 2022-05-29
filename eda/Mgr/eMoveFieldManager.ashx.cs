
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eMoveFieldManager
    /// </summary>
    public class eMoveFieldManager : eAdminManager
    {

        protected override void ProcessManager()
        {
            Int32 iOrigDescId = 0, iOrigDisporder = 0, iDestDisporder = 0;

            // dans le cas du déplacement d'un champ libre, origdescid devra être le descid de la table
            if (_requestTools.AllKeys.Contains("origdescid"))
            {
                Int32.TryParse(_context.Request.Form["origdescid"].ToString(), out iOrigDescId);
            }
            if (_requestTools.AllKeys.Contains("origdisporder"))
            {
                Int32.TryParse(_context.Request.Form["origdisporder"].ToString(), out iOrigDisporder);
            }
            if (_requestTools.AllKeys.Contains("destdisporder"))
            {
                Int32.TryParse(_context.Request.Form["destdisporder"].ToString(), out iDestDisporder);
            }

            int? iDestX = _requestTools.GetRequestFormKeyI("destx"), iDestY = _requestTools.GetRequestFormKeyI("desty");

            bool bSwap = (_requestTools.GetRequestFormKeyI("modeswap") ?? 0) == 1;
            bool bWholeSpaceRow = _requestTools.GetRequestFormKeyB("wsr") ?? false;
            bool bDropSpaceRow = _requestTools.GetRequestFormKeyB("dsr") ?? false;
            Int32 iTab = eLibTools.GetTabFromDescId(iOrigDescId);


            if (iOrigDescId <= 0 /*|| iOrigDisporder <= nSysLimite || iDestDisporder <= nSysLimite*/)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6882), eResApp.GetRes(_pref, 6883), "", String.Concat("iOrigDescId : ", iOrigDescId, " - iOrigDisporder : ", iOrigDisporder, " - iDestDisporder : ", iDestDisporder)));
                return;
            }

            if (!bDropSpaceRow)
                if (iOrigDisporder == iDestDisporder)
                {
                    //Le champ se trouve déjà à l'endroit indiqué
                    LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 1674), eResApp.GetRes(_pref, 6884), ""));
                    return;

                }

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                String sError = "";


                //  bSwap = false;
                if (bSwap)
                {

                    //
                    eAdminFieldInfos af = eAdminFieldInfos.GetFieldInfosFromDisporder(_pref, iOrigDescId - iOrigDescId % 100, iDestDisporder);

                    if (af != null)
                    {
                        eAdminFieldInfos afOrig = eAdminFieldInfos.GetAdminFieldInfos(dal, _pref, iOrigDescId);

                        if (af.Rowspan == afOrig.Rowspan && af.Colspan == afOrig.Colspan)
                        {

                            eAdminDesc adDest = new eAdminDesc(af.DescId);
                            adDest.SetDesc(eLibConst.DESC.DISPORDER, iOrigDisporder.ToString());
                            adDest.Save(_pref, out sError);
                        }
                        else
                        {
                            //Le champ se trouve déjà à l'endroit indiqué
                            LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 1674), eResApp.GetRes(_pref, 7377), ""));
                            return;
                        }
                    }


                    eAdminDesc ad = new eAdminDesc(iOrigDescId);
                    ad.SetDesc(eLibConst.DESC.DISPORDER, iDestDisporder.ToString());
                    ad.Save(_pref, out sError);



                }
                else if (bWholeSpaceRow)
                {
                    if (iDestY == null)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6882), eResApp.GetRes(_pref, 6883), "", String.Concat("bWholeSpaceRow : ", bWholeSpaceRow, " - iDestY : ", iDestY)));
                        return;
                    }

                    if (bDropSpaceRow)
                        eFileLayout.DropSpaceRow(_pref, dal, iTab, iDestY.Value, out sError);
                    else
                        eFileLayout.InsertSpaceRow(_pref, dal, iTab, iDestY.Value, out sError);

                    if (sError.Length > 0)
                        throw new Exception(sError);

                }
                else
                {
                    eAdminDesc.MoveField admf = new eAdminDesc.MoveField(iOrigDescId, iOrigDisporder, iDestDisporder);

                    if (admf.Shift(dal, out sError) && iOrigDescId % 100 > 0)
                    {
                        eAdminDesc ad = new eAdminDesc(iOrigDescId);

                        ad.SetDesc(eLibConst.DESC.DISPORDER, iDestDisporder.ToString());
                        ad.Save(_pref, out sError);
                    }

                }
                if (!bWholeSpaceRow && eSqlDesc.IsCoordLayoutEnabled(_pref, dal, iTab, out sError))
                    eFileLayout.UpdateFieldsPositions(_pref, iTab, out sError);

                if (sError.Length > 0)
                    throw new Exception(sError);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                dal.CloseDatabase();
            }

        }

    }
}