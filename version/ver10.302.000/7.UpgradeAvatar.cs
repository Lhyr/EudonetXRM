using System;
using EudoQuery;
using Com.Eudonet.Xrm;
using Com.Eudonet.Internal;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Com.Eudonet.Core.Model;

public class VersionFile
{
    public static void Upgrade(Object sender)
    {
        eUpgrader upgraderSender = (eUpgrader)sender;


        upgraderSender.AddReferenceAssembly(typeof(System.Web.HttpContext));


        eudoDAL _eDal = upgraderSender.EDal;
        ePrefSQL _pref = upgraderSender.Pref;

        bool bWasOpen = false;
        try
        {
            bWasOpen = _eDal.IsOpen;

            if (!bWasOpen)
                _eDal.OpenDatabase();

            //Enregisrement des avatars, déplacement des fichier
            String sPhysicalFullPathVCARD = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.AVATARS, _pref), @"\");  //Chemin vers VCARD
            String sPhysicalFullPathFile = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref), @"\");  //Chemin vers FILES


            Regex regAvatarFileName = new Regex(@"[0-9]*\.png$");
            List<String> lstAvatrFile = System.IO.Directory.GetFiles(sPhysicalFullPathVCARD, "*.png").Where(sFileName => regAvatarFileName.IsMatch(sFileName)).ToList();
            Int32 nTab;

            nTab = 101000;
            List<String> lst = new List<string>();
            String sError;
            foreach (String sFileName in lstAvatrFile)
            {

                System.IO.FileInfo fl = new System.IO.FileInfo(sFileName);

                Int32 nFileId;

                //
                if (Int32.TryParse(fl.Name.Split('.')[0], out nFileId))
                {
                    string sSQL = "SELECT COUNT(1) FROM [USER] WHERE IsNUll([USER75],'') <> '' AND [USERID] = @ID";


                    RqParam rq = new RqParam(sSQL);
                    rq.AddInputParameter("@ID", System.Data.SqlDbType.Int, nFileId);

                    int nRes = _eDal.ExecuteScalar<Int32>(rq, out sError);

                    if (sError.Length > 0)
                        throw _eDal.InnerException ?? new Exception(sError);

                    //
                    if (nRes != 0)
                        continue;

                    Int32 nBaseId = 0;
                    String sBaseName = sBaseName = String.Concat(101000, "_", nFileId, "_", nBaseId, fl.Extension);

                    //déplacement du fichier
                    String sNewPath = String.Concat(sPhysicalFullPathFile.TrimEnd('\\'), "\\", sBaseName);
                    if (!File.Exists(sNewPath))
                    {
                        File.Copy(sFileName, sNewPath, true);
                    }
                    else
                    {
                        //Le fichier existe déjà, on doit le renommer et maj en bdd

                        while (File.Exists(sNewPath))
                        {
                            nBaseId++;
                            sBaseName = String.Concat(101000, "_", nFileId, "_", nBaseId, fl.Extension);
                            sNewPath = String.Concat(sPhysicalFullPathFile.TrimEnd('\\'), "\\", sBaseName);
                        }
                        File.Copy(sFileName, sNewPath);
                    }


                    if (nTab == 101000)
                        sSQL = "UPDATE [USER] SET [USER75] = @FILENAME WHERE [USERID] = @ID";
                    else
                        continue;


                    rq.AddInputParameter("@FILENAME", System.Data.SqlDbType.NVarChar, sBaseName);
                    rq.AddInputParameter("@ID", System.Data.SqlDbType.Int, nFileId);



                    _eDal.ExecuteNonQuery(rq, out sError);

                }
                else
                {
                    //
                }

            }



        }
        finally
        {
            if (!bWasOpen)
                _eDal.CloseDatabase();
        }
    }
}