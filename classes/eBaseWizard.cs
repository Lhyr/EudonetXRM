using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer de "base" pour les wizard
    /// </summary>
    public abstract class eBaseWizardRenderer : eRenderer
    {

        #region propriétés & accesseur

        /// <summary>
        /// Nombre d'étape du wizard
        /// </summary>
        protected int _nbStep = 0;

        /// <summary>
        /// Classe de représentation d'une étape
        /// </summary>
        protected class WizStep
        {

            Int32 _nStep;

            /// <summary>
            /// Numéro d'étape
            /// </summary>
            public Int32 Step
            {
                get { return _nStep; }
            }



            String _sLibelle;

            /// <summary>
            /// Libellé d'étape
            /// </summary>
            public String Libelle
            {
                get { return _sLibelle; }           
            }


            Func<Panel> _Action;

            /// <summary>
            /// Méthode a appelé pour l'étape
            /// </summary>
            public Func<Panel> Action
            {
                get { return _Action; }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="step"></param>
            /// <param name="libelle"></param>
            /// <param name="action"></param>
            public WizStep(Int32 step, String libelle, Func<Panel> action)
            {

                _nStep = step;
                _sLibelle = libelle;
                _Action = action;
            }

        }

        /// <summary>
        /// Liste des étapes du wizard
        /// </summary>
        protected List<WizStep> lstStep = new List<WizStep>();


        /// <summary>
        /// Ressource nécéssaire pour le wizard
        /// </summary>
        private eRes _eres;

        /// <summary>
        /// Liste des ressource utilisé par le wizard
        /// </summary>
        protected String _sListRes = String.Empty;


        /// <summary>
        /// Accesseur vers l'objet ressource du wizard
        /// </summary>
        protected eRes WizardRes
        {
            get { return _eres; }
        }

        /// <summary>
        /// Information sur l'historique
        /// </summary>
        public Int32 NB_TOTALSTEP
        {
            get { return _nbStep; }

        }


        #endregion
         

        /// <summary>
        /// Initialisation du renderer  
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            LoadRes();

            

            return true;

        }
 
        /// <summary>
        /// Construit le corps de l'assistant, composé d'un div de param et d'un div par étape d'assistant
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            //header (Etapes)
            Panel Header = this.BuildHeader();

            //body (assistant)
            Panel Editor = this.BuildBody();


            //
            this.PgContainer.Controls.Add(Header);

            //
            this.PgContainer.Controls.Add(Editor);

            return true;
        }

        #region construction Entête

        /// <summary>
        /// Constuit la partie Haute de l'assistant Invitations
        /// Contenant les boutons et libellés des différentes étapes.
        /// </summary>
        /// <returns>Div conteneur de la partie haute de l'assistant</returns>
        private Panel BuildHeader()
        {
            Panel header = new Panel();
            header.ID = "wizardheader";
            header.CssClass = "wizardheader";
            header.Attributes.Add("bkm", _tab.ToString());

            Panel stepGroup = new Panel();
            stepGroup.CssClass = String.Concat("states_placement", _rType == RENDERERTYPE.ChartWizard ? " stpPlcmtChrt" : "");

            Int32 nActiveStep = 1;

            for (Int32 i = 1; i <= NB_TOTALSTEP; i++)
            {
                stepGroup.Controls.Add(BuildHeaderStep(i, i == nActiveStep));

                if (i < NB_TOTALSTEP)
                    stepGroup.Controls.Add(BuildHeaderSeparator());
            }
            header.Controls.Add(stepGroup);
            return header;
        }

        
        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected virtual Panel BuildHeaderStep(Int32 step, Boolean isActive)
        {

            if (!lstStep.Any(a => a.Step == step))
                throw new Exception("Etape du wizard invalide");

            Panel stepBloc = new Panel();
            stepBloc.ID = "step_" + step.ToString();

            Panel numberBloc = new Panel();
            numberBloc.ID = "txtnum_" + step.ToString();


            stepBloc.Attributes.Add("onclick", String.Concat("StepClick('", step, "');"));
            numberBloc.Controls.Add(new LiteralControl(step.ToString()));

            Label lbl = new Label();
            lbl.Text = lstStep.Find(a => a.Step == step).Libelle;
            stepBloc.CssClass = isActive ? "state_grp-current" : "state_grp";
            stepBloc.Controls.Add(numberBloc);
            stepBloc.Controls.Add(lbl);

            return stepBloc;
        }



        /// <summary>
        /// Construit le bloc de séparation entre deux boutons d'étape.
        /// </summary>
        /// <returns>Panel (div) de Séparation entre deux étapes</returns>
        protected Panel BuildHeaderSeparator()
        {
            Panel sepBloc = new Panel();
            sepBloc.CssClass = "state_sep";
            return sepBloc;
        }
         

        #endregion


        #region Construction Body

        /// <summary>
        /// Construit le corps du wizard
        /// </summary>
        /// <returns></returns>
        protected Panel BuildBody()
        {

            Panel wizardBody = new Panel();
            wizardBody.ID = "wizardbody";
            wizardBody.CssClass = "wizardbody";
            wizardBody.Attributes.Add("bkm", _tab.ToString());


            for (int i = 1; i <= NB_TOTALSTEP; i++)
                wizardBody.Controls.Add(BuildBodyStep(i));

            return wizardBody;

        }
        
        /// <summary>
        /// Construction des panel des étapes
        /// </summary>
        /// <param name="step">étape a construire</param>
        /// <returns></returns>
        private Panel BuildBodyStep(Int32 step)
        {


            if (!lstStep.Any(a => a.Step == step))
                throw new Exception("Etape du wizard invalide");

            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = step == 1 ? "editor-on" : "editor-off";
            Label lblFormat = new Label();

            WizStep s = lstStep.Find(a => a.Step == step);

            pEditDiv.Controls.Add(s.Action());


            return pEditDiv;
        }

        #endregion

         
        /// <summary>
        /// Charge la ressource
        /// </summary>
        protected virtual void LoadRes()
        {
            if (_sListRes.Length > 0)
            {


                _eres = new eRes(Pref, _sListRes);
            }

        }


        /// <summary>
        /// Code javascript de lancement du wizard
        /// </summary>
        /// <returns></returns>
        public abstract String GetInitJS();


   
    }
}