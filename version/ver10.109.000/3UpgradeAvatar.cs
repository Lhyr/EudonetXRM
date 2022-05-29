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
            String sPhysicalFullPathVCARD = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.VCARD, _pref), @"\");  //Chemin vers VCARD
            String sPhysicalFullPathFile = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref), @"\");  //Chemin vers FILES


            Regex regAvatarFileName = new Regex(@"\\(200|300)_[1-9]+[0-9]*_[0-9]*\.jpg$");
            List<String> lstAvatrFile = System.IO.Directory.GetFiles(sPhysicalFullPathVCARD, "*.jpg").Where(sFileName => regAvatarFileName.IsMatch(sFileName)).ToList();


            List<String> lst = new List<string>();
            foreach (String sFileName in lstAvatrFile)
            {

                System.IO.FileInfo fl = new System.IO.FileInfo(sFileName);

                Int32 nFileId;
                Int32 nTab;

                //
                if (Int32.TryParse(fl.Name.Split('_')[1], out nFileId) && Int32.TryParse(fl.Name.Split('_')[0], out nTab))
                {
                    if (nTab != 200 && nTab != 300)
                        continue;

                    String sSQL;
                    String sBaseName = fl.Name;

                    //déplacement du fichier
                    String sNewPath = String.Concat(sPhysicalFullPathFile.TrimEnd('\\'), "\\", fl.Name);
                    if (!File.Exists(sNewPath))
                    {
                        File.Move(sFileName, sNewPath);
                    }
                    else
                    {
                        //Le fichier existe déjà, on doit le renommer et maj en bdd
                        Int32 nBaseId = 0;
                        while (File.Exists(sNewPath))
                        {
                            nBaseId++;
                            sBaseName = String.Concat(nTab, "_", nFileId, "_", nBaseId, fl.Extension);
                            sNewPath = String.Concat(sPhysicalFullPathFile.TrimEnd('\\'), "\\", sBaseName);
                        }
                        File.Move(sFileName, sNewPath);
                    }


                    if (nTab == 200)
                        sSQL = "UPDATE [PP] SET [PP75] = @FILENAME WHERE [PPID] = @ID";
                    else if (nTab == 300)
                        sSQL = "UPDATE [PM] SET [PM75] = @FILENAME WHERE [PMID] = @ID";
                    else
                        continue;

                    RqParam rq = new RqParam(sSQL);
                    rq.AddInputParameter("@FILENAME", System.Data.SqlDbType.NVarChar, sBaseName);
                    rq.AddInputParameter("@ID", System.Data.SqlDbType.Int, nFileId);


                    String sError;
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