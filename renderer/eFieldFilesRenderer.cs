using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// construit la liste de fichiers accessible pour le champ en cours.
    /// </summary>
    public class eFieldFilesRenderer
    {
        /// <summary>div principale</summary>
        public Panel PgContainer { get; set; }

        /// <summary>chemin d'accès complet au dossier</summary>
        public String FolderPath = "";

        /// <summary>liste des fichiers sélectionnés</summary>
        public List<String> LstSelFiles;

        private Boolean _bMultiple = false;
        private String _sFolder = "";
        private ePref _pref;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="folderType"></param>
        /// <param name="sFolder"></param>
        /// <param name="bMult"></param>
        /// <param name="sLstSelFiles">Fichiers sélectionnés dans le champ</param>
        public eFieldFilesRenderer(ePref pref, eLibConst.FOLDER_TYPE folderType, String sFolder, Boolean bMult, String sLstSelFiles)
        {
            _pref = pref;
            _sFolder = sFolder;
            _bMultiple = bMult;

            List<String> lstSelFiles = new List<string>();
            lstSelFiles.AddRange(sLstSelFiles.Split(';'));
            LstSelFiles = lstSelFiles;

            for (int i = 0; i < LstSelFiles.Count; i++)
            {
                LstSelFiles[i] = LstSelFiles[i].Trim();
            }

            FolderPath = String.Concat(eModelTools.GetPhysicalDatasPath(folderType, _pref.GetBaseName), @"\", _sFolder);

        }



        /// <summary>
        /// charge la liste des fichiers disponibles
        /// </summary>
        public void LoadFiles(HtmlGenericControl divLstFiles, String sMask = "")
        {
            if (Directory.Exists(FolderPath))
            {
                eListFilesProperties filesData = new eListFilesProperties(FolderPath, sMask);
                Int32 idx = 0;
                foreach (FileProperties dataFile in filesData.LstFiles)
                {

                    Panel pn = GetFileLine(dataFile.Name, idx);
                    divLstFiles.Controls.Add(pn);

                    idx++;
                }
                Label l = new Label();
                l.Attributes.Add("ref", "0");
                divLstFiles.Controls.Add(l);
            }
        }

        /// <summary>
        /// Retourne une div contenant le nom du fichier et ses boutons d'actions et de sélection
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Panel GetFileLine(String sFileName, Int32 idx)
        {
            Panel pnFileLine = new Panel();
            Boolean bSelected = LstSelFiles.Contains(sFileName);

            pnFileLine.CssClass = String.Concat("divFldFile list_", idx % 2 == 0 ? "odd" : "even", bSelected ? " eSel" : "");
            if (bSelected)
                pnFileLine.Attributes.Add("sel", "1");

            Panel pnFileName = new Panel();
            pnFileLine.Controls.Add(pnFileName);
            pnFileName.CssClass = "divFName";
            pnFileName.Attributes.Add("idx", idx.ToString());
            pnFileName.Attributes.Add("obg", "1");

            if (_bMultiple)
            {
                eCheckBoxCtrl chk = new eCheckBoxCtrl(bSelected, false);
                chk.AddText(sFileName);
                chk.AddClick("selFile(this.parentElement.parentElement)");
                pnFileName.Controls.Add(chk);

            }
            else
            {
                LiteralControl liFileName = new LiteralControl(sFileName);
                pnFileName.Controls.Add(liFileName);

                pnFileLine.Attributes.Add("onclick", "selFileAdv(this);");
            }

            #region Boutons d'action (renommer supprimer ouvrir)
            Panel pnButtons = new Panel();
            pnFileLine.Controls.Add(pnButtons);
            pnButtons.CssClass = "logo_modifs";

            String sProtectedFileName = sFileName.Replace("'", @"\'");

            //Visualiser
            Panel pnBtn = new Panel();
            pnButtons.Controls.Add(pnBtn);
            pnBtn.ToolTip = eResApp.GetRes(_pref, 1229);
            pnBtn.CssClass = "icon-edn-eye";
            pnBtn.Attributes.Add("onclick", String.Concat("openFile(this.parentElement.parentElement.children[0])"));

            //Renommer
            pnBtn = new Panel();
            pnButtons.Controls.Add(pnBtn);
            pnBtn.ToolTip = eResApp.GetRes(_pref, 86);
            pnBtn.CssClass = "icon-abc";
            pnBtn.Attributes.Add("onclick", String.Concat("oFileNameEditor.onClick(this.parentElement.parentElement.children[0], this);"));


            //Supprimer
            pnBtn = new Panel();
            pnButtons.Controls.Add(pnBtn);
            pnBtn.ToolTip = eResApp.GetRes(_pref, 19);
            pnBtn.CssClass = "icon-delete";
            pnBtn.Attributes.Add("onclick", String.Concat("eConfirm(", eLibConst.MSG_TYPE.QUESTION.GetHashCode(), ",top._res_806, top._res_591 + top._res_7631, null, null, null, function(){delFile(", idx, ");})"));


            #endregion

            return pnFileLine;
        }
    }
}