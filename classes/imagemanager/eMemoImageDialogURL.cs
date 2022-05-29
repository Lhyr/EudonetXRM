using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe utilisée pour l'ajout d'image via le bouton Parcourir de la fenêtre d'ajout d'image de CKEditor - Backlog #315
    /// </summary>
    public class eMemoImageDialogURL : eAbstractImage
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        protected eMemoImageDialogURL(ePref pref) : base(pref)
        {

        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.MEMO_SETDIALOGURL;
            _imageStorageType = EudoQuery.ImageStorage.STORE_IN_FILE;

            return base.Init();
        }

        /// <summary>
        /// Getter
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eMemoImageDialogURL GetMemoImageDialogURL(ePref pref)
        {
            return new eMemoImageDialogURL(pref);
        }

    }
}