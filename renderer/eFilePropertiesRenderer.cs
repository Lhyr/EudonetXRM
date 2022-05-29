using System;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer pour les propriété d'une fiche
    ///  - appartient à/modifiée le etc...
    /// </summary>
    public class eFilePropertiesRenderer : eEditFileRenderer
    {

        /// <summary>
        /// Affichage pour les propriétés de la fiche
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        public eFilePropertiesRenderer(ePref pref, Int32 nTab, Int32 nFileId)
            : base(pref, nTab, nFileId)
        {
            _rType = RENDERERTYPE.EditFile;
        }

        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            try
            {
                _myFile = eFileProperties.CreateFileProperties(Pref, _tab, _nFileId);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = _myFile.ErrorMsg;
                    if (_myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eFilePropertiesRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }

        /// <summary>
        /// construction de la fiche
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            try
            {
                _pgContainer.ID = String.Concat("divPty_", _tab);
                _pgContainer.Controls.Add(GetPropertiesTable());
                return true;
            }
            catch (Exception ex)
            {
                _sErrorMsg = "eFilePropertiesRenderer - Build() - Une erreur s'est produite durant la mise en page des propriétés de la fiche.";
                _eException = ex;
                return false;
            }
        }


                /// <summary>
        /// traitement de fin de rendu
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return true;
        }

    }
}