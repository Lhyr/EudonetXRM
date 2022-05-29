using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de récupération et manipulation d'images
    /// </summary>
    /// <author>MAB</author>
    public abstract class eImageTools: eAvatarTools
    {

        /// <summary>
        /// Format d'image par défaut pour la génération des images
        /// </summary>
        public static System.Drawing.Imaging.ImageFormat _defaultImgFormat = System.Drawing.Imaging.ImageFormat.Png;


        #region GetDBImage  : Renvoie l'image stockée en base pour le champ (DescId) et la fiche (FileId) spécifiés
        /// <summary>
        /// Renvoie l'image stockée en base pour le champ (DescId) et la fiche (FileId) spécifiés, sans la redimensionner, avec le format par défaut, et ce,
        /// après avoir vérifié les droits d'accès pour l'utilisateur en cours (information renvoyée dans bCanAccessImage).
        /// Renvoie une image générique au lieu de l'image elle-même si l'image est présente en base
        /// </summary>
        /// <param name="pref">Objet Pref pour accès à la base et aux préférences utilisateur</param>
        /// <param name="nDescId">DescId du champ Image à récupérer</param>
        /// <param name="nFileId">FileId de la fiche contenant l'image à récupérer</param>
        /// <param name="bCanAccessImage">Indique si l'utilisateur a, ou non, les droits d'accès à l'image en question</param>
        /// <param name="strError">Messages d'erreur éventuels issus des traitements internes</param>
        /// <returns></returns>
        public static System.Drawing.Image GetDBImage(ePref pref, int nDescId, int nFileId, out bool bCanAccessImage, out string strError)
        {
            Field fieldInfo = new Field();
            return GetDBImage(pref, nDescId, nFileId, 0, 0, ImageFormat.Png, true, out fieldInfo, out bCanAccessImage, out strError);
        }
        /// <summary>
        /// Renvoie l'image stockée en base pour le champ (DescId) et la fiche (FileId) spécifiés, sans la redimensionner, avec le format spécifié, et ce,
        /// après avoir vérifié les droits d'accès pour l'utilisateur en cours (information renvoyée dans bCanAccessImage).
        /// Renvoie une image générique au lieu de l'image elle-même si l'image est présente en base
        /// </summary>
        /// <param name="pref">Objet Pref pour accès à la base et aux préférences utilisateur</param>
        /// <param name="nDescId">DescId du champ Image à récupérer</param>
        /// <param name="nFileId">FileId de la fiche contenant l'image à récupérer</param>
        /// <param name="imageFormat">Format de l'image désiré en sortie (GIF, JPEG, PNG...)</param>
        /// <param name="bCanAccessImage">Indique si l'utilisateur a, ou non, les droits d'accès à l'image en question</param>
        /// <param name="strError">Messages d'erreur éventuels issus des traitements internes</param>
        /// <returns></returns>
        public static System.Drawing.Image GetDBImage(ePref pref, int nDescId, int nFileId, ImageFormat imageFormat, out bool bCanAccessImage, out string strError)
        {
            Field fieldInfo = new Field();
            return GetDBImage(pref, nDescId, nFileId, 0, 0, imageFormat, true, out fieldInfo, out bCanAccessImage, out strError);
        }
        /// <summary>
        /// Renvoie l'image stockée en base pour le champ (DescId) et la fiche (FileId) spécifiés, avec les dimensions et le format spécifiés,
        /// après avoir vérifié les droits d'accès pour l'utilisateur en cours (information renvoyée dans bCanAccessImage).
        /// Renvoie une image générique au lieu de l'image elle-même si l'image est présente en base
        /// </summary>
        /// <param name="pref">Objet Pref pour accès à la base et aux préférences utilisateur</param>
        /// <param name="nDescId">DescId du champ Image à récupérer</param>
        /// <param name="nFileId">FileId de la fiche contenant l'image à récupérer</param>
        /// <param name="nResizeWidth">Largeur de l'image à renvoyer. Renvoie une image proportionnellement redimensionnée à la hauteur si non spécifiée</param>
        /// <param name="nResizeHeight">Hauteur de l'image à renvoyer. Renvoie une image proportionnellement redimensionnée à la largeur si non spécifiée</param>
        /// <param name="bCanAccessImage">Indique si l'utilisateur a, ou non, les droits d'accès à l'image en question</param>
        /// <param name="strError">Messages d'erreur éventuels issus des traitements internes</param>
        /// <returns></returns>
        public static System.Drawing.Image GetDBImage(ePref pref, int nDescId, int nFileId, int nResizeWidth, int nResizeHeight, out bool bCanAccessImage, out string strError)
        {
            Field fieldInfo = new Field();
            return GetDBImage(pref, nDescId, nFileId, nResizeWidth, nResizeHeight, ImageFormat.Png, true, out fieldInfo, out bCanAccessImage, out strError);
        }

        /// <summary>
        /// Renvoie l'image stockée en base pour le champ (DescId) et la fiche (FileId) spécifiés, avec les dimensions et le format spécifiés,
        /// après avoir vérifié les droits d'accès pour l'utilisateur en cours (information renvoyée dans bCanAccessImage).
        /// Renvoie une image générique au lieu de l'image elle-même si l'image est présente en base
        /// </summary>
        /// <param name="pref">Objet Pref pour accès à la base et aux préférences utilisateur</param>
        /// <param name="nDescId">DescId du champ Image à récupérer</param>
        /// <param name="nFileId">FileId de la fiche contenant l'image à récupérer</param>
        /// <param name="nResizeWidth">Largeur de l'image à renvoyer. Renvoie une image proportionnellement redimensionnée à la hauteur si non spécifiée</param>
        /// <param name="nResizeHeight">Hauteur de l'image à renvoyer. Renvoie une image proportionnellement redimensionnée à la largeur si non spécifiée</param>
        /// <param name="imageFormat">Format de l'image désiré en sortie (GIF, JPEG, PNG...)</param>
        /// <param name="bGenericImageOnly">true pour renvoyer une image générique à la place de la vraie image stockée en base</param>
        /// <param name="bCanAccessImage">Indique si l'utilisateur a, ou non, les droits d'accès à l'image en question</param>
        /// <param name="strError">Messages d'erreur éventuels issus des traitements internes</param>
        /// <param name="fieldInfo">Paramètre de sortie : Information sur le champ image</param>
        /// <returns></returns>
        public static System.Drawing.Image GetDBImage(
            ePref pref,
            int nDescId, int nFileId,
            int nResizeWidth, int nResizeHeight,
            ImageFormat imageFormat, bool bGenericImageOnly,
            out Field fieldInfo,
            out bool bCanAccessImage, out string strError)
        {
            strError = String.Empty;
            bCanAccessImage = false;
            fieldInfo = new Field();
            eFieldRecord fieldRecord = new eFieldRecord();
            object oImgData = eLibTools.GetDBImageData(pref, nDescId, nFileId, out fieldInfo, out fieldRecord, out bCanAccessImage, out strError);
            if (strError.Length == 0 && bCanAccessImage)
                return ConvertDBImage(pref, oImgData, nResizeWidth, nResizeHeight, imageFormat, bGenericImageOnly, out strError);
            else
                return null;
        }
        #endregion

        /// <summary>
        /// Indique si la valeur Binary Image passée en paramètre (récupérée par ex. via GetDBImageData) est NULL (pas d'image) ou non (image présente)
        /// </summary>
        /// <param name="oImageData">Objet Binary Image issu de la base de données</param>
        /// <returns></returns>
        public static bool DBImageExists(object oImageData)
        {
            try
            {
                if (oImageData == null || oImageData.GetType() == typeof(System.DBNull))
                    return false;
                // Certaines images peuvent être corrompues en base. On tente alors de les charger dans un objet Bitmap pour vérifier leur validité
                else
                {
                    Bitmap bmpOriginal = new Bitmap(new MemoryStream((Byte[])oImageData));
                    return (bmpOriginal != null);
                }
            }
            catch
            {
                return false; // image corrompue
            }
        }

        /// <summary>
        /// Convertit l'image issue de la base de données en image Bitmap, avec les dimensions et le format spécifiés.
        /// Peut renvoyer une image générique au lieu de l'image elle-même si l'image est présente en base, si demandé via bGenericImageOnly
        /// </summary>
        /// <param name="pref">Objet Pref pour accès à la base et aux préférences utilisateur</param>
        /// <param name="oImageData">Objet Binary Image issu de la base de données</param>
        /// <param name="nResizeWidth">Largeur de l'image à renvoyer. Renvoie une image proportionnellement redimensionnée à la hauteur si non spécifiée</param>
        /// <param name="nResizeHeight">Hauteur de l'image à renvoyer. Renvoie une image proportionnellement redimensionnée à la largeur si non spécifiée</param>
        /// <param name="imageFormat">Format de l'image désiré en sortie (GIF, JPEG, PNG...)</param>
        /// <param name="bGenericImageOnly">true pour renvoyer une image générique à la place de la vraie image stockée en base</param>
        /// <param name="strError">Messages d'erreur éventuels issus des traitements internes</param>
        /// <returns></returns>
        public static System.Drawing.Image ConvertDBImage(
            ePref pref,
            object oImageData,
            int nResizeWidth, int nResizeHeight,
            ImageFormat imageFormat, bool bGenericImageOnly,
            out string strError)
        {
            System.Drawing.Image bitmap = null;
            strError = String.Empty;

            if (!DBImageExists(oImageData))
            {
                // Pas de message d'erreur : il n'y a tout simplement pas d'image en base pour ce DescId et ce FileId
                // Dans ce cas, on renvoie une image générique "grisée"
                try
                {
                    FileInfo fi = new FileInfo(System.Web.HttpContext.Current.Server.MapPath("themes/" + pref.ThemePaths.GetDefaultImageWebPath()));
                    if (fi.Exists)
                        bitmap = new Bitmap(fi.FullName);
                }
                catch
                {
                    bitmap = null;
                }
            }

            else
            {
                if (bGenericImageOnly)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(System.Web.HttpContext.Current.Server.MapPath("themes/" + pref.ThemePaths.GetImageWebPath("/images/ui/picture.png"))); // TODO: image par défaut
                        if (fi.Exists)
                            bitmap = new Bitmap(fi.FullName);
                    }
                    catch
                    {
                        bitmap = null;
                    }
                }
                else
                {
                    try
                    {
                        MemoryStream ms = new MemoryStream((Byte[])oImageData);
                        bitmap = GetThumbnail(ms, nResizeWidth, nResizeHeight);
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        bitmap = null;
                    }
                }
            }

            return bitmap;
        }

        #region Génération de miniatures

        /// <summary>
        /// Crée une miniature de l'image passée en paramètre, écrit le fichier sur le disque, et retourne son emplacement physique
        /// Si un fichier miniature existe déjà, on renvoie directement le chemin vers le fichier existant sans regénérer l'image,
        /// sauf si précisé explicitement en paramètre
        /// </summary>
        /// <param name="srcImg">Chemin de l'image</param>
        /// <param name="width">Largeur de la miniature à générer</param>
        /// <param name="height">Hauteur de la miniature à générer</param>
        /// <param name="bOverrideExisting">Remplace le thumbnail existant</param>
        /// <param name="strError">Message d'erreur éventuel</param>
        /// <returns>Chemin du fichier miniature généré</returns>
        public static string CreateThumbnail(string srcImg, int width, int height, bool bOverrideExisting, out string strError)
        {
            string _sThumbName = srcImg;
            strError = String.Empty;

            try
            {
                string _imgThumb = string.Empty;
                System.IO.FileInfo _fi = new System.IO.FileInfo(srcImg);
                _imgThumb = string.Concat(_fi.FullName.Substring(0, _fi.FullName.Length - _fi.Extension.Length), eLibConst.THUMB_SUFFIX, _fi.Extension);
                _sThumbName = string.Concat(_fi.Name.Substring(0, _fi.Name.Length - _fi.Extension.Length), eLibConst.THUMB_SUFFIX, _fi.Extension);

                if (bOverrideExisting || !System.IO.File.Exists(_imgThumb))
                {
                    try
                    {
                        System.Drawing.Image thumbnailImage = GetThumbnail(srcImg, width, height);
                        System.IO.MemoryStream imageStream = new System.IO.MemoryStream();
                        thumbnailImage.Save(_imgThumb, _defaultImgFormat);
                        imageStream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        strError = String.Concat("Erreur lors de la génération de la miniature (CreateThumbnail - 1) : ", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = String.Concat("Erreur générale lors de la génération de la miniature (CreateThumbnail - 2) : ", ex.Message);
            }

            return _sThumbName;
        }

        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec la taille spécifiée
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="srcImg">Chemin du fichier image à redimensionner</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(string srcImg, int width, int height)
        {
            System.Drawing.Image thumbnail = null;
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(srcImg);
                thumbnail = GetThumbnail(image, _defaultImgFormat, width, height, false);
                image.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur de génération de la miniature (GetThumbnail - 2) : " + ex.Message, ex);
            }
            return thumbnail;
        }
        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec le format et la taille spécifiés
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="srcImg">Chemin du fichier image à redimensionner</param>
        /// <param name="imageFormat">Format de l'image à générer</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(string srcImg, ImageFormat imageFormat, int width, int height)
        {
            System.Drawing.Image thumbnail = null;
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(srcImg);
                thumbnail = GetThumbnail(image, imageFormat, width, height, false);
                image.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur de génération de la miniature (GetThumbnail - 3) : " + ex.Message, ex);
            }
            return thumbnail;
        }
        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec le format et la taille spécifiés
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="srcImg">Chemin du fichier image à redimensionner</param>
        /// <param name="imageFormat">Format de l'image à générer</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <param name="bAllowStretch">Indique si l'agrandissement est autorisé. Si false, l'image ne sera pas redimensionnée si la taille indiquée est supérieure à celle de l'image originale</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(string srcImg, ImageFormat imageFormat, int width, int height, bool bAllowStretch)
        {
            System.Drawing.Image thumbnail = null;
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(srcImg);
                thumbnail = GetThumbnail(image, imageFormat, width, height, bAllowStretch);
                image.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur de génération de la miniature (GetThumbnail - 4) : " + ex.Message, ex);
            }
            return thumbnail;
        }

        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec la taille spécifiée
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="ms">Flux mémoire correspondant à l'image à redimensionner</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(MemoryStream ms, int width, int height)
        {
            System.Drawing.Image thumbnail = null;
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                thumbnail = GetThumbnail(image, _defaultImgFormat, width, height, false);
                image.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur de génération de la miniature (GetThumbnail - 5) : " + ex.Message, ex);
            }
            return thumbnail;
        }
        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec la taille spécifiée
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="ms">Flux mémoire correspondant à l'image à redimensionner</param>
        /// <param name="imageFormat">Format de l'image à générer</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(MemoryStream ms, ImageFormat imageFormat, int width, int height)
        {
            System.Drawing.Image imgToResize = new Bitmap(ms);
            return GetThumbnail(imgToResize, imageFormat, width, height, false);
        }
        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec la taille spécifiée
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="ms">Flux mémoire correspondant à l'image à redimensionner</param>
        /// <param name="imageFormat">Format de l'image à générer</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <param name="bAllowStretch">Indique si l'agrandissement est autorisé. Si false, l'image ne sera pas redimensionnée si la taille indiquée est supérieure à celle de l'image originale</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(MemoryStream ms, ImageFormat imageFormat, int width, int height, bool bAllowStretch)
        {
            System.Drawing.Image imgToResize = new Bitmap(ms);
            return GetThumbnail(imgToResize, imageFormat, width, height, bAllowStretch);
        }

        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec la taille spécifiée
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="imgToResize">Image à redimensionner</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(System.Drawing.Image imgToResize, int width, int height)
        {
            return GetThumbnail(imgToResize, _defaultImgFormat, width, height, false);
        }
        /// <summary>
        /// Génère une miniature de l'image donnée en paramètre avec le format et la taille spécifiés
        /// Si seule une taille (hauteur ou largeur) est spécifiée, le redimensionnement se fait au ratio des dimensions de l'image originale
        /// </summary>
        /// <param name="imgToResize">Image à redimensionner</param>
        /// <param name="imageFormat">Format de l'image à générer</param>
        /// <param name="width">Largeur de l'image souhaitée</param>
        /// <param name="height">Hauteur de l'image souhaitée</param>
        /// <param name="bAllowStretch">Indique si l'agrandissement est autorisé. Si false, l'image ne sera pas redimensionnée si la taille indiquée est supérieure à celle de l'image originale</param>
        /// <returns>L'image redimensionnée</returns>
        public static System.Drawing.Image GetThumbnail(System.Drawing.Image imgToResize, ImageFormat imageFormat, int width, int height, bool bAllowStretch)
        {
            MemoryStream msOutput = new MemoryStream();

            try
            {
                decimal nResizeWidth = width;
                decimal nResizeHeight = height;
                // Calcul de la taille de l'image redimensionnée par aspect ratio si seule une information est passée sur les deux
                decimal nOriginalWidth = imgToResize.Width;
                decimal nOriginalHeight = imgToResize.Height;
                if (nResizeWidth > 0 && nResizeHeight < 1)
                    nResizeHeight = nResizeWidth / (nOriginalWidth / nOriginalHeight);
                if (nResizeHeight > 0 && nResizeWidth < 1)
                    nResizeWidth = nResizeHeight / (nOriginalHeight / nOriginalWidth);

                // Le redimensionnement n'est pas effectué si l'image originale est plus petite (pas d'agrandissement), ou si les coordonnées sont à 0 (pas de redimensionnement)
                // Sauf si indiqué explicitement
                bool bCanStretchImage = bAllowStretch || (nResizeHeight < nOriginalHeight && nResizeWidth < nOriginalWidth);
                if (nResizeWidth > 0 && nResizeHeight > 0 && bCanStretchImage)
                {
                    // Le framework .NET propose GetThumbnailImage pour ce genre de traitements.
                    // Celui-ci n'offrant toutefois pas le choix du format et pouvant planter, notamment si width et height ne lui conviennent pas (= 0 ou autre),
                    // on réalise l'opération sans passer par cette méthode
                    // TODO BENCHMARK : évaluer la pertinence de ce choix question performances
                    //System.Drawing.Image imgResized = imgToResize.GetThumbnailImage(width, height, null, IntPtr.Zero);
                    System.Drawing.Image imgResized = new Bitmap((int)Math.Floor(nResizeWidth), (int)Math.Floor(nResizeHeight));
                    Graphics g = Graphics.FromImage((System.Drawing.Image)imgResized);
                    g.DrawImage((System.Drawing.Image)imgToResize, 0, 0, (int)Math.Floor(nResizeWidth), (int)Math.Floor(nResizeHeight));
                    g.Dispose();
                    imgResized.Save(msOutput, imageFormat);
                    imgResized.Dispose();
                }
                else
                {
                    imgToResize.Save(msOutput, imageFormat);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur de génération de la miniature (GetThumbnail) : " + ex.Message, ex);
            }
            finally
            {
                imgToResize.Dispose();
            }

            System.Drawing.Image imgToReturn = null;
            try
            {
                imgToReturn = new Bitmap(msOutput);
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur de renvoi de la miniature (GetThumbnail) : " + ex.Message, ex);
            }
            finally
            {
                msOutput.Dispose();
            }

            return imgToReturn;
        }


        /// <summary>
        /// retourne le chemin d'accès de la thumbnail en fonction de l'image originelle
        /// </summary>
        /// <param name="sCompleteFilename"></param>
        /// <returns></returns>
        public static string GetThumbNailName(String sCompleteFilename)
        {
            return string.Concat(sCompleteFilename.Substring(0, sCompleteFilename.Length - Path.GetExtension(sCompleteFilename).Length), eLibConst.THUMB_SUFFIX, Path.GetExtension(sCompleteFilename));

        }

        #endregion

        /// <summary>
        /// Retourne le type de stockage de la rubrique image
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="descid"></param>
        /// <returns></returns>
        public static ImageStorage GetStorageType(ePref pref, int descid)
        {
            ImageStorage storage = ImageStorage.STORE_IN_FILE;

            String error = String.Empty;
            eudoDAL eDal = eLibTools.GetEudoDAL(pref);
            DataTableReaderTuned dtr = null;

            try
            {
                eDal.OpenDatabase();

                String query = "SELECT Storage FROM [DESC] WHere DescId = @descid";
                RqParam rqParam = new RqParam(query);
                rqParam.AddInputParameter("@descid", System.Data.SqlDbType.Int, descid);
                dtr = eDal.Execute(rqParam, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception();
                if (dtr.Read())
                {
                    storage = (ImageStorage)(dtr.GetEudoNumeric("Storage"));
                }

            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
                eDal.CloseDatabase();
            }

            return storage;
        }
    }
}