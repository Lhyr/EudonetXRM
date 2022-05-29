using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu des différents types de rubriques disponibles à la création
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminBlockRenderer" />
    public class eAdminFieldsTypesRenderer : eAdminBlockRenderer
    {
        private eAdminFieldsTypesRenderer(ePref pref, eAdminTableInfos tab, String title)
            : base(pref, tab, title, "", idBlock: "partFileFields")
        {

        }
        /// <summary>
        /// Retourne un objet eAdminFieldsTypesRenderer
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tab">Infos de la table</param>
        /// <param name="title">Titre du bloc</param>
        /// <returns></returns>
        public static eAdminFieldsTypesRenderer CreateAdminFieldsTypesRenderer(ePref pref, eAdminTableInfos tab, String title)
        {
            eAdminFieldsTypesRenderer features = new eAdminFieldsTypesRenderer(pref, tab, title);
            return features;
        }

        /// <summary>Construction du bloc "Rubriques" (bloc Mise en page)</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                _panelContent.ID = "FieldsTypesPart";

                HtmlGenericControl h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7680);
                _panelContent.Controls.Add(h5);

                HtmlGenericControl ul = new HtmlGenericControl("ul");
                HtmlGenericControl ctrl;
                //Rubrique
                if (Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                {
                    ctrl = BuildFieldType("icon-rubrique", eResApp.GetRes(Pref, 222));
                    ul.Controls.Add(ctrl);
                    MakeCtrlDraggable(ctrl);
                    ctrl.Attributes.Add("ust", "1"); // ust : unspecified type
                    ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7887));
                }

                //Espace libre
                ctrl = BuildFieldType("icon-ellipsis-h", eResApp.GetRes(Pref, 7290));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add("ondragstart ", "sds(event, this);");
                ctrl.Attributes.Add("ondrag ", "ofm(event);");
                ctrl.Attributes.Add("ondragend ", "ofmu(event);");
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7289));


                //Ligne Libre
                if (_tabInfos.IsCoordEnabled)
                {
                    ctrl = BuildFieldType("icon-ellipsis-h", eResApp.GetRes(Pref, 8283));
                    ul.Controls.Add(ctrl);
                    ctrl.Attributes.Add("ondragstart ", "sds(event, this);");
                    ctrl.Attributes.Add("ondrag ", "ofm(event);");
                    ctrl.Attributes.Add("ondragend ", "ofmu(event);");
                    ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 8282));
                    ctrl.Attributes.Add("wsr", "1"); // wrs : whole space row
                }

                //ligne Blanche
                ctrl = BuildFieldType("icon-minus", eResApp.GetRes(Pref, 8396));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat((int)eAdminUpdateProperty.CATEGORY.DESC, "_", (int)eLibConst.DESC.FORMAT), ((int)FieldFormat.TYP_TITLE).ToString());
                ctrl.Attributes.Add(String.Concat((int)eAdminUpdateProperty.CATEGORY.DESC, "_", (int)eLibConst.DESC.LENGTH), "0");
                ctrl.Attributes.Add(String.Concat((int)eAdminUpdateProperty.CATEGORY.DESCADV, "_", (int)DESCADV_PARAMETER.LABELHIDDEN), "1");
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 8395));
                MakeCtrlDraggable(ctrl);

                //Etiquettes 
                ctrl = BuildFieldType("icon-tag", eResApp.GetRes(Pref, 7293));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_TITLE.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.LENGTH.GetHashCode()), "0");
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7294));
                MakeCtrlDraggable(ctrl);

                //Séparateur 
                ctrl = BuildFieldType("icon-minus", eResApp.GetRes(Pref, 7292));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_TITLE.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.LENGTH.GetHashCode()), "1");
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7291));
                MakeCtrlDraggable(ctrl);




                _panelContent.Controls.Add(ul);

                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7681);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");
                //ul.Controls.Add(BuildFieldType("icon-abc", eResApp.GetRes(Pref, 229))); // Caractère
                //ul.Controls.Add(BuildFieldType("icon-catalog", eResApp.GetRes(Pref, 225))); // Catalogue

                // Caractère
                ctrl = BuildFieldType("icon-font", eResApp.GetRes(Pref, 229));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_CHAR.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7295));
                MakeCtrlDraggable(ctrl);

                // Catalogue
                ctrl = BuildFieldType("icon-folder2", eResApp.GetRes(Pref, 225));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_CHAR.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.POPUP.GetHashCode()), PopupType.DATA.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7296));
                MakeCtrlDraggable(ctrl);

                // Choix multiple
                ctrl = BuildFieldType("icon-list-ul", eResApp.GetRes(Pref, 247));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_CHAR.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.POPUP.GetHashCode()), PopupType.DATA.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.MULTIPLE.GetHashCode()), "1");
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7297));
                MakeCtrlDraggable(ctrl);



                // Mémo
                ctrl = BuildFieldType("icon-pencil-square-o", eResApp.GetRes(Pref, 235));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_MEMO.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7300));
                MakeCtrlDraggable(ctrl);

                // Mémo HTML
                ctrl = BuildFieldType("icon-pencil-square-o", eResApp.GetRes(Pref, 7689));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_MEMO.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.HTML.GetHashCode()), "1");
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7301));
                MakeCtrlDraggable(ctrl);

                _panelContent.Controls.Add(ul);

                //Les relationnels
                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7988);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");


                // Relation 
                ctrl = BuildFieldType("icon-clone", eResApp.GetRes(Pref, 7299));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_CHAR.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.POPUP.GetHashCode()), PopupType.ONLY.GetHashCode().ToString());
                //ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.RELATION.GetHashCode()), "1");
                eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                dal.OpenDatabase();
                String sError;
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.POPUPDESCID.GetHashCode()), eSqlDesc.GetFirstMainFieldDescId(Pref, dal, out sError).ToString());
                dal.CloseDatabase();
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7298));

                MakeCtrlDraggable(ctrl);


                // Alias 
                ctrl = BuildFieldType("icon-clone", eResApp.GetRes(Pref, 7987));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_ALIAS.GetHashCode().ToString());
                MakeCtrlDraggable(ctrl);

                _panelContent.Controls.Add(ul);


                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7682);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");

                // Numérique
                ctrl = BuildFieldType("icon-sort-numeric-asc", eResApp.GetRes(Pref, 236));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_NUMERIC.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7302));
                MakeCtrlDraggable(ctrl);

                // Monétaire 
                ctrl = BuildFieldType("icon-dollar", eResApp.GetRes(Pref, 7303));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_MONEY.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7304));
                MakeCtrlDraggable(ctrl);

                // Pourcentage TODO
                if (eAdminConst.ADMIN_CURRENTVERSION >= (int)eAdminConst.ADMIN_VERSION.V2)
                {
                    ctrl = BuildFieldType("icon-percent", eResApp.GetRes(Pref, 6228));
                    ul.Controls.Add(ctrl);
                }

                // Compteur
                ctrl = BuildFieldType("icon-calculator", eResApp.GetRes(Pref, 6144));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_AUTOINC.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7305));
                MakeCtrlDraggable(ctrl);

                _panelContent.Controls.Add(ul);

                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7687);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");

                // Date
                ctrl = BuildFieldType("icon-calendar", eResApp.GetRes(Pref, 231));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_DATE.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7306));
                MakeCtrlDraggable(ctrl);

                // Heure TODO
                if (eAdminConst.ADMIN_CURRENTVERSION >= (int)eAdminConst.ADMIN_VERSION.V2)
                {
                    ul.Controls.Add(BuildFieldType("icon-clock-o", eResApp.GetRes(Pref, 1515)));
                }

                _panelContent.Controls.Add(ul);

                #region Les cliquables

                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7688);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");

                // Logique
                ctrl = BuildFieldType("icon-check-square", eResApp.GetRes(Pref, 232));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_BIT.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7307));
                MakeCtrlDraggable(ctrl);

                // Bouton
                ctrl = BuildFieldType("icon-square", eResApp.GetRes(Pref, 7693));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_BITBUTTON.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7886));
                MakeCtrlDraggable(ctrl);
                #endregion

                // Score TODO, TODO: RES
                if (eAdminConst.ADMIN_CURRENTVERSION >= (int)eAdminConst.ADMIN_VERSION.V2)
                {
                    ul.Controls.Add(BuildFieldType("icon-star", eResApp.GetRes(Pref, 7691)));
                }

                _panelContent.Controls.Add(ul);

                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7683);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");

                // Téléphone
                ctrl = BuildFieldType("icon-phone3", eResApp.GetRes(Pref, 5138));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_PHONE.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7308));
                MakeCtrlDraggable(ctrl);

                // E-mail 
                ctrl = BuildFieldType("icon-envelope", eResApp.GetRes(Pref, 5142));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_EMAIL.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7309));
                MakeCtrlDraggable(ctrl);

                // Réseau social 
                //if (eAdminConst.ADMIN_CURRENTVERSION >= (int)eAdminConst.ADMIN_VERSION.V2)
                //{
                ctrl = BuildFieldType("icon-share-alt", eResApp.GetRes(Pref, 7690)); // 
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_SOCIALNETWORK.GetHashCode().ToString());
                ctrl.Attributes.Add("title", ""); //TODORES
                MakeCtrlDraggable(ctrl);
                //}

                // Géolocalisation 
                ctrl = BuildFieldType("icon-map-marker", eResApp.GetRes(Pref, 7108));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_GEOGRAPHY_V2.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7310));
                MakeCtrlDraggable(ctrl);

                _panelContent.Controls.Add(ul);

                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7684);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");
                //ul.Controls.Add(BuildFieldType("icon-phone", eResApp.GetRes(Pref, 1216))); // Image
                //ul.Controls.Add(BuildFieldType("icon-stats", eResApp.GetRes(Pref, 1005))); // Graphique
                //ul.Controls.Add(BuildFieldType("icon-email", eResApp.GetRes(Pref, 1543))); // Page web
                //ul.Controls.Add(BuildFieldType("icon-hyperlink", "Lien web")); // TODO: RES 
                //ul.Controls.Add(BuildFieldType("icon-signet-fiche", eResApp.GetRes(Pref, 103))); // Fichier

                // Image
                ctrl = BuildFieldType("icon-picture-o", eResApp.GetRes(Pref, 1216));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_IMAGE.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7311));
                MakeCtrlDraggable(ctrl);


                // Graphique
                ctrl = BuildFieldType("icon-pie-chart", eResApp.GetRes(Pref, 1005));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_CHART.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7312));
                MakeCtrlDraggable(ctrl);

                // Page web
                ctrl = BuildFieldType("icon-cloud", eResApp.GetRes(Pref, 1543));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_IFRAME.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7313));
                MakeCtrlDraggable(ctrl);

                // Lien web
                ctrl = BuildFieldType("icon-link", eResApp.GetRes(Pref, 7314));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_WEB.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7315));
                MakeCtrlDraggable(ctrl);

                // Fichier
                ctrl = BuildFieldType("icon-file-text-o", eResApp.GetRes(Pref, 103));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_FILE.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7317));
                MakeCtrlDraggable(ctrl);

                _panelContent.Controls.Add(ul);

                #region Les automatismes TODO

                //h5 = new HtmlGenericControl("h5");
                //h5.InnerText = eResApp.GetRes(Pref, 7685);
                //_panelContent.Controls.Add(h5);

                //ul = new HtmlGenericControl("ul");
                //if (eAdminConst.ADMIN_CURRENTVERSION >= (int)eAdminConst.ADMIN_VERSION.V2)
                //{
                //    ul.Controls.Add(BuildFieldType("icon-flask", eResApp.GetRes(Pref, 7228)));
                //    ul.Controls.Add(BuildFieldType("icon-stats", "Agrégat")); // TODO: RES
                //    ul.Controls.Add(BuildFieldType("icon-sigma", eResApp.GetRes(Pref, 7692)));
                //    ul.Controls.Add(BuildFieldType("icon-square", eResApp.GetRes(Pref, 7693)));
                //}

                //_panelContent.Controls.Add(ul);
                #endregion

                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 7686);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");

                // Utilisateur
                ctrl = BuildFieldType("icon-user", eResApp.GetRes(Pref, 411));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_USER.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7318));
                MakeCtrlDraggable(ctrl);

                ctrl = BuildFieldType("icon-users", eResApp.GetRes(Pref, 410)); // Groupe
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_GROUP.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 7319));
                MakeCtrlDraggable(ctrl);

                _panelContent.Controls.Add(ul);


                //Les sécurisées
                h5 = new HtmlGenericControl("h5");
                h5.InnerText = eResApp.GetRes(Pref, 2395);
                _panelContent.Controls.Add(h5);

                ul = new HtmlGenericControl("ul");

                //BS: US #765
                // Mot de passe
                ctrl = BuildFieldType("icon-lock", eResApp.GetRes(Pref, 316));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_PASSWORD.GetHashCode().ToString());
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 2394));
                MakeCtrlDraggable(ctrl);

                // Login
                ctrl = BuildFieldType("icon-key", eResApp.GetRes(Pref, 752));
                ul.Controls.Add(ctrl);
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", eLibConst.DESC.FORMAT.GetHashCode()), FieldFormat.TYP_CHAR.GetHashCode().ToString());
                ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESCADV.GetHashCode(), "_", DESCADV_PARAMETER.IS_UNIQUE.GetHashCode()), "1");
                ctrl.Attributes.Add("title", eResApp.GetRes(Pref, 2403));
                MakeCtrlDraggable(ctrl);

                _panelContent.Controls.Add(ul);

                // Liste des rubriques
                eAdminButtonField field = new eAdminButtonField(eResApp.GetRes(Pref, 7694), "buttonFieldsList", eResApp.GetRes(Pref, 7695), "nsAdmin.confFieldsList()");
                field.Generate(_panelContent);

                // Ajout avancé - Uniquement pour les super-administrateurs (#51 573)
                if (Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
                {
                    field = new eAdminButtonField(eResApp.GetRes(Pref, 7588), "buttonFieldsAdvancedAdd", eResApp.GetRes(Pref, 7888), "nsAdminMoveField.addNewField()"); // Ajout avancé - Cliquez ici pour créer une rubrique en précisant son identifiant système
                    field.Generate(_panelContent);
                }


                return true;
            }
            else
            {
                return false;
            }

        }

        internal static void MakeCtrlDraggable(HtmlGenericControl ctrl)
        {
            ctrl.Attributes.Add("ondragstart ", "cds(event, this);");
            ctrl.Attributes.Add("ondrag ", "ofm(event);");
            ctrl.Attributes.Add("ondragend ", "ofmu(event);");
        }

        /// <summary>Construit un "li" contenant l'icône et le libellé du type de rubrique</summary>
        /// <param name="classIcon">Classe CSS de l'icône</param>
        /// <param name="label">Libellé du type</param>
        /// <returns></returns>
        internal static HtmlGenericControl BuildFieldType(String classIcon, String label)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("draggable", "true");
            HtmlGenericControl icon = new HtmlGenericControl("span");
            icon.Attributes.Add("class", classIcon + " fieldIcon");
            li.Controls.Add(icon);
            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerText = label;
            li.Controls.Add(span);

            return li;
        }
    }
}