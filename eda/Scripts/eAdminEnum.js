var nsAdminEnum = {};

///Type de condition
nsAdminEnum.TypeConditionnal =
{
    /// <summary>
    /// non défini. thrower une execption dans ce cas
    /// </summary>
    Undefined : 0,


    /// <summary>
    /// Condition de modif
    /// </summary>
    Update : 1,

    /// <summary>
    /// Condition de suppression
    /// </summary>
    Delete : 2,

    /// <summary>
    /// Couleurs conditionneles
    /// </summary>
    Color : 3,

    /// <summary>
    /// Condition de visu des entêtes
    /// </summary>
    Header_View : 4,

    /// <summary>
    /// Condition de modifs des entêtes
    /// </summary>
    Header_Update : 5



}

/// <summary>
/// Type de l'automatisme
/// </summary>
nsAdminEnum.AutomationType
{
    /// <summary>
    /// Tous les automatismes
    /// </summary>
    ALL = 0,
    /// <summary>
    /// Notification XRM/Moble ...
    /// </summary>
    NOTIFICATION = 1
}
