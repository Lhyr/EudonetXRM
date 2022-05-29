
namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Action du eTargetProcessManager
    /// </summary>
    public enum eImportAction
    {
        // Pas d'action
        DO_NOTHING = 0,

        // Ajout depuis un input
        LOAD_FROM_TEXT = 1,

        // Depuis un fichier txt/csv
        LOAD_FROM_FILE = 2,

        // %  de progress bar
        CHECK_PROGRESS = 3,
    }
}