using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.synchroExchange;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu de la fenêtre de mapping des champs d'EudoSync Exchange Agendas version E2017/Office 365
    /// </summary>
    public class eAdminStoreSynchroExchangeMappingRenderer : eAdminRenderer
    {
        IEnumerable<eFieldSynchroExchange> _tabFields;

        // Mapping
        SynchroExchangeMappingParam _mappingParam;

        int _organizerDescid = 0;

        int _attendeesDescid = 0;

        int _startDescid = 0;

        int _endDescid = 0;

        int _subjectDescid = 0;

        int _locationDescid = 0;

        int _bodyDescid = 0;

        int _sensitivityDescid = 0;


        private eAdminStoreSynchroExchangeMappingRenderer(ePref pref, int tab) : base()
        {
            _ePref = pref;
            _tab = tab;
        }
        /// <summary>
        /// Creates the admin extension synchro exchange mapping renderer.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <returns></returns>
        public static eAdminStoreSynchroExchangeMappingRenderer CreateAdminExtensionSynchroExchangeMappingRenderer(ePref pref, int tab)
        {
            eAdminStoreSynchroExchangeMappingRenderer rdr = new eAdminStoreSynchroExchangeMappingRenderer(pref, tab);
            return rdr;
        }

        /// <summary>
        /// Initialisation du mapping : chargement des listes de champs disponibles
        /// </summary>
        /// <returns>true si initialisation effectuée, false sinon</returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (_tab == 0) return false;

                IDictionary<eLibConst.CONFIGADV, string> dicConfigADV =
                    eLibTools.GetConfigAdvValues(Pref, new HashSet<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.SYNC365_MAPPING });

                // Chargement de la liste des champs de la table Planning sélectionnée
                _tabFields = RetrieveFields.GetDefault(_ePref)
                    .AddOnlyThisTabs(new int[] { _tab })
                    .ResultFieldsInfo(eFieldSynchroExchange.FactoryTable(_ePref), eFieldSynchroExchange.Factory(_ePref)).OrderBy(f => f.Libelle);

                string mapping = dicConfigADV[eLibConst.CONFIGADV.SYNC365_MAPPING];
                if (mapping.Length > 0)
                {
                    _mappingParam = JsonConvert.DeserializeObject<SynchroExchangeMappingParam>(mapping);

                    if (_mappingParam != null)
                    {
                        _organizerDescid = _mappingParam.OrganizerDescid;
                        _attendeesDescid = _mappingParam.AttendeesDescid;
                        _startDescid = _mappingParam.StartDescid;
                        _endDescid = _mappingParam.EndDescid;
                        _subjectDescid = _mappingParam.SubjectDescid;
                        _locationDescid = _mappingParam.LocationDescid;
                        _bodyDescid = _mappingParam.BodyDescid;
                        _sensitivityDescid = _mappingParam.SensitivityDescid;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Génération du rendu
        /// </summary>
        /// <returns>true si rendu généré, false sinon</returns>
        protected override bool Build()
        {
            _pgContainer.ID = "syncMappingContent";

            HtmlInputHidden hidden = new HtmlInputHidden();
            hidden.ID = "hidMapping";
            hidden.Attributes.Add("dsc", string.Format("{0}|{1}|{2}",
                (int)eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.SYNC365_MAPPING.GetHashCode(), (int)eLibConst.CONFIGADV_CATEGORY.SYNCHRO_OFFICE365));
            _pgContainer.Controls.Add(hidden);

            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.ORGANIZER, _organizerDescid);
            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.ATTENDEES, _attendeesDescid);
            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.DATE_START, _startDescid);
            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.DATE_END, _endDescid);
            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.SUBJECT, _subjectDescid);
            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.LOCATION, _locationDescid);
            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.DESCRIPTION, _bodyDescid);
            BuildDropdownListForMapping(_pgContainer, SynchroExchangeMapping.ExchangeField.CONFIDENTIAL, _sensitivityDescid);

            return true;

        }

        /// <summary>
        /// Création de la liste des champs Eudonet mappables pour un champ Exchange donné
        /// </summary>
        /// <param name="container">Panel conteneur</param>
        /// <param name="field">Champ source à mapper (Exchange)</param>
        /// <param name="descid">Descid sélectionné</param>
        void BuildDropdownListForMapping(Panel container, SynchroExchangeMapping.ExchangeField field, int descid)
        {
            IEnumerable<eFieldSynchroExchange> fields;

            Panel pField = new Panel();
            pField.CssClass = "field";

            HtmlGenericControl htmlLabel = new HtmlGenericControl("label");
            htmlLabel.InnerText = SynchroExchangeMapping.GetMappingFieldLabel(Pref, field);
            pField.Controls.Add(htmlLabel);
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddl" + field.ToString();
            ddl.Attributes.Add("data-source", field.ToString());

            // #65 175 - Ajout des champs selon la liste des formats autorisés + interdiction des catalogues liés
            fields = _tabFields.Where(
                f => (
                    (SynchroExchangeMapping.GetMappingFieldAllowedDescIds(f, field).Count == 0 || SynchroExchangeMapping.GetMappingFieldAllowedDescIds(f, field).Contains(f.Descid % 100)) && // #65 658 - Le DescID du champ correspond éventuellement au DescID système prédéfini
                    SynchroExchangeMapping.GetMappingFieldAllowedFormats(field).Contains(f.Format) && // le format du champ figure parmi ceux autorisés
                    (f.Popup == PopupType.NONE || f.Popup == PopupType.FREE || f.Popup == PopupType.ONLY || f.Popup == PopupType.DATA) && // catalogue simple uniquement (cf. #65 658) ou avancé (#65 584)
                    (f.Multiple == false || f.Multiple && f.Descid % 100 == AllField.TPL_MULTI_OWNER.GetHashCode()) && // pas de catalogue à choix multiple, sauf pour A faire par/Participants (TPL92)
                    (f.PopupDescId == 0 || f.PopupDescId == f.Descid) && // Le catalogue n'utilise pas les rubriques d'un autre catalogue
                    (f.BoundDescid == 0 || f.BoundDescid == f.Descid) && // Ce champ n'est pas directement lié (enfant) à un autre champ (parent)
                    _tabFields.Where(fBound => fBound.BoundDescid == f.Descid)?.Count() == 0 // Ce champ n'est pas utilisé comme liaison (parent) d'un autre champ (enfant)
                )
            );

            List<ListItem> itemsList = fields.Select(f => new ListItem(f.Libelle, f.Descid.ToString())).ToList();
            itemsList.Insert(0, new ListItem(eResApp.GetRes(_ePref, 6211), "0"));
            ddl.Items.AddRange(itemsList.ToArray());

            if (descid > 0)
            {
                ddl.SelectedValue = descid.ToString();
                // #65 175 - Si le champ mappé inscrit en base ne figure pas ou plus parmi la liste de champs valides, on matérialise l'erreur de mapping en colorant
                // la combobox
                if (fields?.ToList().Find(f => f.Descid == descid) == null)
                    ddl.CssClass = String.Concat(ddl.CssClass, " mappingConflict");
            }

            pField.Controls.Add(ddl);

            container.Controls.Add(pField);
        }
    }
}