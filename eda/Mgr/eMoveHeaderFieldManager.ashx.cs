using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eMoveHeaderFieldManager
    /// </summary>
    public class eMoveHeaderFieldManager : eAdminManager
    {

        protected override void ProcessManager()
        {
            Int32 iOrigDescId = 0, iTab = 0;
            String sOrigCellPos = String.Empty;
            String sDestCellPos = String.Empty;

            if (_requestTools.AllKeys.Contains("tab"))
            {
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out iTab);
            }
            if (_requestTools.AllKeys.Contains("origdescid"))
            {
                Int32.TryParse(_context.Request.Form["origdescid"].ToString(), out iOrigDescId);
            }
            if (_requestTools.AllKeys.Contains("origcellpos"))
            {
                sOrigCellPos = _context.Request.Form["origcellpos"].ToString();
            }
            if (_requestTools.AllKeys.Contains("destcellpos"))
            {
                sDestCellPos = _context.Request.Form["destcellpos"].ToString();
            }

            if (sOrigCellPos == sDestCellPos)
            {
                //Le champ se trouve déjà à l'endroit indiqué
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 1674), eResApp.GetRes(_pref, 6884), ""));
                return;

            }

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                String sError;

                String[] arrOrigCellPos = sOrigCellPos.Split(';');
                String[] arrDestCellPos = sDestCellPos.Split(';');

                Dictionary<String, String> dicPref = _pref.GetPrefDefault(iTab, new List<String> { "HEADER_300", "HEADER_200" });
                String header_300 = dicPref["HEADER_300"];
                String header_200 = dicPref["HEADER_200"];

                String[] arrHeader300 = header_300.Split(';');
                String[] arrHeader200 = header_200.Split(';');
                List<String> listHeader300 = header_300.Split(';').ToList();
                List<String> listHeader200 = header_200.Split(';').ToList();

                if (iOrigDescId % 100 == 0)
                {
                    iOrigDescId++;
                }

                String destRow = arrDestCellPos[0];
                String destCol = arrDestCellPos[1];
                String origRow = arrOrigCellPos[0];
                String origCol = arrOrigCellPos[1];

                // TODO : Gérer le cas ou on fait du drag&drop sur la même colonne
                if (destCol == "0")
                {
                    if (origCol == "0")
                    {
                        // Cas : Déplacement sur la même colonne
                        String destDescId = listHeader300[eLibTools.GetNum(destRow)];
                        if (!String.IsNullOrEmpty(destDescId))
                        {
                            listHeader300[eLibTools.GetNum(destRow)] = iOrigDescId.ToString();
                            listHeader300[eLibTools.GetNum(origRow)] = destDescId;
                        }
                        else
                        {
                            listHeader300[0] = iOrigDescId.ToString();
                        }
                    }
                    else if (origCol == "1")
                    {
                        listHeader200.RemoveAt(eLibTools.GetNum(origRow));
                        if (listHeader300.Count > 0)
                            listHeader300.Insert(eLibTools.GetNum(destRow), iOrigDescId.ToString());
                        else
                            listHeader300.Insert(0, iOrigDescId.ToString());
                    }
                }
                else if (destCol == "1")
                {
                    if (origCol == "1")
                    {
                        // Cas : Déplacement sur la même colonne
                        String destDescId = listHeader200[eLibTools.GetNum(destRow)];
                        if (!String.IsNullOrEmpty(destDescId))
                        {
                            listHeader200[eLibTools.GetNum(destRow)] = iOrigDescId.ToString();
                            listHeader200[eLibTools.GetNum(origRow)] = destDescId;
                        }
                        else
                        {
                            listHeader200[0] = iOrigDescId.ToString();
                        }
                        
                    }
                    else if (origCol == "0")
                    {
                        
                        listHeader300.RemoveAt(eLibTools.GetNum(origRow));
                        if (listHeader200.Count > 0)
                            listHeader200.Insert(eLibTools.GetNum(destRow), iOrigDescId.ToString());
                        else
                            listHeader200.Insert(0, iOrigDescId.ToString());
                    }
                }


                // Nettoyage des pref
                listHeader300.RemoveAll(i => String.IsNullOrEmpty(i));
                listHeader200.RemoveAll(i => String.IsNullOrEmpty(i));

                header_300 = String.Join(";", listHeader300);
                header_200 = String.Join(";", listHeader200);

                List<SetParam<String>> listPref = new List<SetParam<String>>();
                listPref.Add(new SetParam<String>("HEADER_300", header_300));
                listPref.Add(new SetParam<String>("HEADER_200", header_200));
                _pref.SetPrefDefault(iTab, listPref);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                dal.CloseDatabase();
            }
        }
    }
}