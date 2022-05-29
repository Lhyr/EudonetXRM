using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu d'un widget (pour les widgets dans iframe)
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eRenderer" />
    public class eWidgetRenderer : eRenderer
    {
        #region Propriétés
        /// <summary>
        /// The widget identifier
        /// </summary>
        protected int _widgetID = 0;
        /// <summary>
        /// The widget parameters
        /// </summary>
        protected eXrmWidgetParam _widgetParams;
        /// <summary>
        /// The widget context
        /// </summary>
        protected eXrmWidgetContext _widgetContext;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="eWidgetRenderer" /> class.
        /// </summary>
        /// <param name="pref">The ePref object</param>
        /// <param name="widgetID">The widget identifier.</param>
        /// <param name="context">The context.</param>
        protected eWidgetRenderer(ePref pref, int widgetID, eXrmWidgetContext context)
        {
            _ePref = pref;
            _widgetID = widgetID;
            _widgetContext = context;
        }

        /// <summary>
        /// Appel l'objet métier
        /// eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {
                _widgetParams = new eXrmWidgetParam(this.Pref, _widgetID);

                return true;
            }
            catch (Exception exc)
            {
                _eException = exc;
                _sErrorMsg = exc.Message;
                return false;
            }
        }
    }
}