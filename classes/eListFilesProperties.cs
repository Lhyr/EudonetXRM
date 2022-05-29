using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe permettant  de lister les fichiers dans un dossier donné
    /// </summary>
    public class eListFilesProperties
    {
        /// <summary>
        /// Liste des fichiers trouvés dans le répertoire donné
        /// </summary>
        public List<FileProperties> LstFiles { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="FolderPath">Dossier dont il faut recenser les fichiers</param>
        /// <param name="sMask">Masque représentant les extensions de fichiers autorisées </param>
        public eListFilesProperties(String FolderPath, String sMask = "")
        {
            if (!Directory.Exists(FolderPath))
            {
                return;
            }
            List<String> allowedExtensions = new List<String>();
            if (!String.IsNullOrEmpty(sMask)) {
                allowedExtensions = sMask.Split(';').ToList<String>();
            }

            LstFiles = new List<FileProperties>();

            IEnumerable<String> lstFilesName = Directory.GetFiles(FolderPath);

            String sFileName = String.Empty;
            Int32 idx = 0;
            FileInfo fi;
            foreach (String completeFileName in lstFilesName)
            {
                fi = new FileInfo(completeFileName);
                if (allowedExtensions.Count > 0)
                {
                    if (!allowedExtensions.Contains(fi.Extension))
                    {
                        continue;
                    }
                }
                LstFiles.Add(new FileProperties(fi));

                idx++;
            }


        }
    }
    /// <summary>
    /// Objet recensant les propriétés utiles sur les fichiers listés
    /// </summary>
    public class FileProperties
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="fi">FileSystemInfo dont on récupère les propriétés</param>
        public FileProperties(FileInfo fi)
        {
            Name = fi.Name;
            FullName = fi.FullName;
            Size = fi.Length;
            Extension = fi.Extension;
        }

        /// <summary>Nom du fichier</summary>
        public String Name { get; private set; }
        /// <summary>Nom complet avec chemin d'accès</summary>
        public String FullName { get; private set; }
        /// <summary>Taille du fichier en octets</summary>
        public long Size { get; private set; }
        /// <summary>Extension</summary>
        public String Extension { get; private set; }
    }

}