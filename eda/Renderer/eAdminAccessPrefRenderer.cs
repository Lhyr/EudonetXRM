using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminAccessPrefRenderer : eAdminModuleRenderer
    {



        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminAccessPrefRenderer(ePref pref, int nWidth, int nHeight)
            : base(pref)
        {
            Pref = pref;
            _width = nWidth - 30;
            _height = nHeight - 30;
        }


        public static eAdminAccessPrefRenderer CreateAdminAccessPrefRenderer(ePref pref, int nWidth, int nHeight, bool popup = false, int nUserId = 0)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();
            eAdminAccessPrefRenderer rdr;
            if (popup)
                rdr = new eAdminAccessPrefPopupRender(pref, nWidth, nHeight, nUserId);
            else
                rdr = new eAdminAccessPrefRenderer(pref, nWidth, nHeight);

            rdr.Generate();

            return rdr;
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                if(!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminAccess_Preferences))
                {

                    PgContainer.ID = "adminAccessPref";
                    Panel pnl = new Panel();
                    HtmlGenericControl p = new HtmlGenericControl("p");
                    p.InnerText = eResApp.GetRes(Pref, 8926);
                    p.Attributes.Add("style", "margin-top:10% ; font-size:122% ");
                    pnl.Controls.Add(p);
                    PgContainer.Controls.Add(pnl);
                }
                else
                {
                    return true;
                }                
            }
            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {

            PgContainer.ID = "adminAccessPref";
            Panel pnl;

            // CNA - Demande #49558 - Masquer cette section            
            // SPH - Demande #54214  - refaire cette section
            pnl = GetModuleSection("groupDefinePref", eResApp.GetRes(Pref, 7710)); // Se connecter en tant que profil
            LoadConnectAs(pnl.Controls[pnl.Controls.Count - 1] as Panel);
            PgContainer.Controls.Add(pnl);

            pnl = GetModuleSection("groupDuplicatePref", eResApp.GetRes(Pref, 7711)); // Répliquer les préférences
            LoadCopyPrefSection(pnl.Controls[pnl.Controls.Count - 1] as Panel);
            PgContainer.Controls.Add(pnl);


            pnl = GetModuleSection("groupResetPref", eResApp.GetRes(Pref, 7709)); // Réinitialiser les préférences
            LoadResetPrefSection(pnl.Controls[pnl.Controls.Count - 1] as Panel);
            PgContainer.Controls.Add(pnl);

            return true;
        }

        /// <summary>
        /// Charge la section de réinitialisation des pref
        /// </summary>
        /// <param name="panel"></param>
        private void LoadResetPrefSection(Panel panel)
        {
            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = eResApp.GetRes(Pref, 7712);
            panel.Controls.Add(p);

            eAdminButtonField btn = eAdminButtonField.GetEAdminButtonField(new eAdminButtonParams()
            {
                Label = eResApp.GetRes(Pref, 7713),
                ID = "btnResetPref",
                OnClick = "nsAdminPref.resetPref()"
            });

            btn.Generate(panel);
        }


        /// <summary>
        /// Création du bloc "se connecter en tant que profil"
        /// </summary>
        /// <param name="panel"></param>
        protected virtual void LoadConnectAs(Panel panel)
        {

            HtmlGenericControl p = new HtmlGenericControl("p");
            //p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7876)));

            p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8052))); //Cette fonctionnalité permet de vous connecter...

            p.Attributes.Add("class", "copyPrefP");
            panel.Controls.Add(p);

            HtmlGenericControl infoUL = new HtmlGenericControl("ul");
            infoUL.Attributes.Add("class", "copyPrefLabelUl");






            panel.Controls.Add(infoUL);


            HtmlGenericControl liCatDstSrc = new HtmlGenericControl("li");
            infoUL.Controls.Add(liCatDstSrc);
            HtmlGenericControl ulCatDstSrc = new HtmlGenericControl("ul");
            liCatDstSrc.Controls.Add(ulCatDstSrc);
            ulCatDstSrc.Attributes.Add("class", "copyPrefSrcDslUL");

            HtmlGenericControl infoSrc = new HtmlGenericControl("li");
            ulCatDstSrc.Controls.Add(infoSrc);
            infoSrc.Attributes.Add("class", "copyPref");

            HtmlGenericControl labelSrc = new HtmlGenericControl("span");


            labelSrc.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8050)));//Profil

            labelSrc.Attributes.Add("class", "copyPrefLabelField");
            infoSrc.Controls.Add(labelSrc);

            HtmlGenericControl txtSource = new HtmlGenericControl("input");
            infoSrc.Controls.Add(txtSource);
            txtSource.Attributes.Add("readonly", "1");
            txtSource.Attributes.Add("id", "EDA_CNX_AS");
            txtSource.Attributes.Add("type", "text");
            txtSource.Attributes.Add("eaction", "LNKCATUSER");
            txtSource.Attributes.Add("class", " readonly LNKCATUSER edit edaPrefInputField");
            txtSource.Attributes.Add("ednformat", ((int)FieldFormat.TYP_USER).ToString());

            txtSource.Attributes.Add("ednvalue", "");



            HtmlGenericControl btnSrc = new HtmlGenericControl("span");
            btnSrc.Attributes.Add("class", "rIco icon-catalog btn");
            btnSrc.Attributes.Add("onclick", "nsAdminPref.showUserCat(this, 'EDA_CNX_AS',0,1 ,1)");
            infoSrc.Controls.Add(btnSrc);


            eAdminButtonField btn = new eAdminButtonField(eResApp.GetRes(Pref, 8051), "btnResetPref", onclick: "nsAdminPref.cnxAs()");
            btn.Generate(panel);


        }

        /// <summary>
        /// Charge la section de réinitialisation des pref
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="nUserId">Utilisateur "cible" préselectioné</param>
        protected virtual void LoadCopyPrefSection(Panel panel, int nUserId = -1)
        {
            HtmlGenericControl p = new HtmlGenericControl("p");
            p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7876)));
            p.Attributes.Add("class", "copyPrefP");
            panel.Controls.Add(p);

            HtmlGenericControl infoUL = new HtmlGenericControl("ul");
            infoUL.Attributes.Add("class", "copyPrefLabelUl");
            HtmlGenericControl infoIL1 = new HtmlGenericControl("li");
            infoIL1.Attributes.Add("class", "copyPrefLabelLI");
            infoIL1.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7877))); // - d'un utilisateur vers un ou plusieurs utilisateurs,


            HtmlGenericControl infoIL2 = new HtmlGenericControl("li");
            infoIL2.Attributes.Add("class", "copyPrefLabelLI");
            infoIL2.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7878))); // - d'un profil vers...

            infoUL.Controls.Add(infoIL1);
            infoUL.Controls.Add(infoIL2);
            panel.Controls.Add(infoUL);


            HtmlGenericControl liCatDstSrc = new HtmlGenericControl("li");
            infoUL.Controls.Add(liCatDstSrc);
            HtmlGenericControl ulCatDstSrc = new HtmlGenericControl("ul");
            liCatDstSrc.Controls.Add(ulCatDstSrc);
            ulCatDstSrc.Attributes.Add("class", "copyPrefSrcDslUL");

            HtmlGenericControl infoSrc = new HtmlGenericControl("li");
            ulCatDstSrc.Controls.Add(infoSrc);
            infoSrc.Attributes.Add("class", "copyPref");

            HtmlGenericControl labelSrc = new HtmlGenericControl("span");
            labelSrc.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7373)));
            labelSrc.Attributes.Add("class", "copyPrefLabelField");
            infoSrc.Controls.Add(labelSrc);

            HtmlGenericControl txtSource = new HtmlGenericControl("input");
            infoSrc.Controls.Add(txtSource);
            txtSource.Attributes.Add("readonly", "1");
            txtSource.Attributes.Add("id", "EDA_CPREF_SRC");
            txtSource.Attributes.Add("type", "text");
            txtSource.Attributes.Add("eaction", "LNKCATUSER");
            txtSource.Attributes.Add("class", " readonly LNKCATUSER edit edaPrefInputField");
            txtSource.Attributes.Add("ednformat", ((int)FieldFormat.TYP_USER).ToString());

            if (nUserId >= 0)
            {
                eUserProfileInfo ui = eUserProfileInfo.GetUserProfileInfo(nUserId, Pref);
                txtSource.Attributes.Add("ednvalue", ui.UserId.ToString());
                txtSource.Attributes.Add("value", ui.UserDisplayName);
            }
            else
            {
                txtSource.Attributes.Add("ednvalue", "");
            }

            HtmlGenericControl btnSrc = new HtmlGenericControl("span");
            btnSrc.Attributes.Add("class", "rIco icon-catalog btn");
            btnSrc.Attributes.Add("onclick", "nsAdminPref.showUserCat(this, 'EDA_CPREF_SRC',0,1)");
            infoSrc.Controls.Add(btnSrc);




            HtmlGenericControl infoCible = new HtmlGenericControl("li");
            ulCatDstSrc.Controls.Add(infoCible);
            infoCible.Attributes.Add("class", "copyPref");

            HtmlGenericControl labelDst = new HtmlGenericControl("span");
            labelDst.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7879)));
            labelDst.Attributes.Add("class", "copyPrefLabelField");
            infoCible.Controls.Add(labelDst);

            HtmlGenericControl txtDst = new HtmlGenericControl("input");
            infoCible.Controls.Add(txtDst);
            txtDst.Attributes.Add("readonly", "1");
            txtDst.Attributes.Add("id", "EDA_CPREF_DST");
            txtDst.Attributes.Add("type", "text");
            txtDst.Attributes.Add("eaction", "LNKCATUSER");
            txtDst.Attributes.Add("class", " readonly LNKCATUSER edit edaPrefInputField");
            txtDst.Attributes.Add("ednformat", ((int)FieldFormat.TYP_USER).ToString());

            if (nUserId >= 0)
            {
                eUserInfo ui = eUserInfo.GetUserInfo(nUserId, Pref);
                txtDst.Attributes.Add("ednvalue", ui.UserId.ToString());
                txtDst.Attributes.Add("value", ui.UserDisplayName);
            }
            else
            {
                txtDst.Attributes.Add("ednvalue", "");
            }


            HtmlGenericControl btnDst = new HtmlGenericControl("span");
            btnDst.Attributes.Add("class", "rIco icon-catalog btn");
            btnDst.Attributes.Add("onclick", "nsAdminPref.showUserCat(this, 'EDA_CPREF_DST',1,1)");
            infoCible.Controls.Add(btnDst);

            if (nUserId <= 0)
            {
                eAdminButtonField btn = new eAdminButtonField(eResApp.GetRes(Pref, 7880), "btnResetPref", onclick: "nsAdminPref.copyPref()");
                btn.Generate(panel);
            }
        }
    }


    public class eAdminAccessPrefPopupRender : eAdminAccessPrefRenderer
    {

        private int _nUserId;

        public eAdminAccessPrefPopupRender(ePref pref, int nWidth, int nHeight, int nUserId) : base(pref, nWidth, nHeight)
        {
            _nUserId = nUserId;
        }

        protected override bool Build()
        {
            PgContainer.ID = "adminAccessPref";
            Panel pnl;

            pnl = GetModuleSection("groupDuplicatePref", eResApp.GetRes(Pref, 7711), bCollapsable: false); // Répliquer les préférences
            LoadCopyPrefSection(pnl.Controls[pnl.Controls.Count - 1] as Panel, _nUserId);
            PgContainer.Controls.Add(pnl);

            return true;
        }
    }
}


