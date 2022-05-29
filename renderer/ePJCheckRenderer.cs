using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu de l'interface de renommage des annexes
    /// </summary>
    public class ePJCheckerRenderer : eRenderer
    {
        List<PJUploadInfo> _filesList = new List<PJUploadInfo>();
        int _fileId = 0;
        string _windowDescription = String.Empty;

        private ePJCheckerRenderer(ePref pref, int tab, int fileid, string windowDescription, List<PJUploadInfo> files)
        {
            _ePref = pref;
            _filesList = files;
            _tab = tab;
            _fileId = fileid;
            _windowDescription = windowDescription;
        }

        /// <summary>
        /// Creates the pj checker renderer.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tab">Table</param>
        /// <param name="fileid">File id</param>
        /// <param name="windowDescription">Message affiché à l'intérieur de la fenêtre</param>
        /// <param name="files">Fichiers</param>
        /// <returns></returns>
        public static ePJCheckerRenderer CreatePJCheckerRenderer(ePref pref, int tab, int fileid, string windowDescription, List<PJUploadInfo> files)
        {
            return new ePJCheckerRenderer(pref, tab, fileid, windowDescription, files);
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            eudoDAL eDal = eLibTools.GetEudoDAL(_ePref);

            try
            {
                eDal.OpenDatabase();

                eRightAttachment rightManager = null;

                try
                {
                    rightManager = new eRightAttachment(_ePref, _tab);
                }
                catch (Exception)
                {
                    // Je conserve la non gestion d'erreur présente avant la refacto. Si erreur, les ReplaceOptDisplayed de chaque pj seront à false et on ne remonte pas l'erreur
                    rightManager = null;
                }

                foreach (PJUploadInfo pjInfo in _filesList)
                {
                    int pjId = 0;

                    // On n'affiche l'option que si la PJ existe déjà pour la fiche en cours
                    bool pjExists = ePJTraitementsLite.PJExistsForFile(eDal, _tab, _fileId, pjInfo.FileName, out pjId);

                    pjInfo.PjId = pjId;

                    if (!pjExists || pjId == 0)
                    {
                        pjInfo.ReplaceOptDisplayed = false;
                        continue;
                    }

                    if (rightManager == null)
                    {
                        pjInfo.ReplaceOptDisplayed = false;
                        continue;
                    }

                    // On n'affiche l'option que si nous avons les droits de modif et de suppression sur les PJ de l'onglet en cours
                    if (!rightManager.CanEditItem() || !rightManager.CanDeleteItem())
                    {
                        pjInfo.ReplaceOptDisplayed = false;
                        continue;
                    }

                    pjInfo.ReplaceOptDisplayed = true;
                }
            }
            finally
            {
                eDal.CloseDatabase();
            }

            return true;
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (!base.Build())
                return false;

            _pgContainer.ID = "pjCheckerContent";

            Panel pQuestion = new Panel();
            pQuestion.CssClass = "titleQuestion";

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-question-circle icon");
            pQuestion.Controls.Add(icon);
            HtmlGenericControl question = new HtmlGenericControl();
            question.Attributes.Add("class", "");
            question.InnerText = _windowDescription;
            pQuestion.Controls.Add(question);

            _pgContainer.Controls.Add(pQuestion);

            Panel pFile, pChoices, pChoice;
            HtmlGenericControl fileLabel;
            TextBox txtSuggestedFile;
            RadioButton rb;

            int count = 0;

            foreach (PJUploadInfo fn in _filesList)
            {
                pFile = new Panel();
                pFile.CssClass = "blockFile";

                fileLabel = new HtmlGenericControl();
                fileLabel.Attributes.Add("class", "labelFilename");
                fileLabel.ID = "labelFn" + count;
                fileLabel.InnerText = fn.FileName;
                fileLabel.Attributes.Add("title", fn.FileName);
                fileLabel.Attributes.Add("data-pjid", fn.PjId.ToString());

                pFile.Controls.Add(fileLabel);

                #region Renommer ou Remplacer et supprimer le fichier existant
                pChoices = new Panel();
                pChoices.CssClass = "blockChoices";

                pChoice = new Panel();

                rb = new RadioButton();
                rb.Text = "Renommer";
                rb.ID = "rbRenameFile" + count;
                rb.GroupName = "rbFile" + count;
                rb.Checked = true;
                pChoice.Controls.Add(rb);
                txtSuggestedFile = new TextBox();
                txtSuggestedFile.Text = fn.SaveAs;
                txtSuggestedFile.CssClass = "txtSuggestedFn";
                txtSuggestedFile.ID = "txtSuggestedFn" + count;
                txtSuggestedFile.Attributes.Add("data-fn", fn.FileName);
                pChoice.Controls.Add(txtSuggestedFile);

                pChoices.Controls.Add(pChoice);


                pChoice = new Panel();
                rb = new RadioButton();
                rb.Text = eResApp.GetRes(_ePref, 8694);
                rb.ID = "rbReplaceFile" + count;
                rb.GroupName = "rbFile" + count;
                if (!fn.ReplaceOptDisplayed)
                {
                    rb.Visible = false;
                }
                pChoice.Controls.Add(rb);
                pChoices.Controls.Add(pChoice);


                #endregion

                pFile.Controls.Add(fileLabel);
                pFile.Controls.Add(pChoices);

                _pgContainer.Controls.Add(pFile);

                count++;

            }

            return true;
        }
    }
}