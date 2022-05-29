using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class pour faire un rendu en cas etape invalide - correspond au pattern nullobject
    /// </summary>
    public class eErrorStepRenderer : IWizardStepRenderer
    {        
        /// <summary>
        /// Message à afficher à l'utilisateur en cas cette objet est appelé
        /// </summary>
        private string userMessage;

        /// <summary>
        /// Constructeur de l'etape avec le message 
        /// </summary>
        /// <param name="userMessage"></param>
        public eErrorStepRenderer(string userMessage)
        {
            this.userMessage = userMessage;
        }

        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        public IWizardStepRenderer Init() { return this; }

        /// <summary>
        /// Execute l'opération du rendu
        /// </summary>
        /// <returns></returns>
        public Panel Render()
        {
            Panel ctn = new Panel();
            ctn.Controls.Add(new LiteralControl(userMessage));
            return ctn;
        }
    }
}
