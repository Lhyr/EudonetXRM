namespace Com.Eudonet.Xrm
{
    /// <className>ILoadQueryString</className>
    /// <summary>Représente les données transmis en QueryString à la page externe de l'appli</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2017-07-31</date>
    public interface ILoadQueryString
    {
        /// <summary>
        /// Indique si la querystring est cohérente
        /// </summary>
        /// <returns>vrai si mauvaise querystring</returns>
        bool InvalidQueryString();
    }
}
