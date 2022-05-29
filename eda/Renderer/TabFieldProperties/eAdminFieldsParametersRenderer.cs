using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Rendu du bloc d'admin des paramètres de champs
    /// </summary>
    public class eAdminFieldsParametersRenderer : eAdminTabFieldRenderer
    {
        eAdminFieldInfos _field;
        /// <summary>
        /// Est-ce une rubrique système ?
        /// </summary>
        Boolean _isSys;

        private eAdminFieldsParametersRenderer(ePref pref, Int32 nDescid, Boolean isSys)
        {
            Pref = pref;
            DescId = nDescid;
            if (DescId != 0)
                _field = eAdminFieldInfos.GetAdminFieldInfos(pref, DescId);
            _isSys = isSys;
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminFieldsParametersRenderer CreateAdminFieldsParamsRenderer(ePref pref, Int32 nDescid, Boolean isSys)
        {
            return new eAdminFieldsParametersRenderer(pref, nDescid, isSys);
        }

        protected override bool Init()
        {
            return base.Init();
        }


        protected override bool Build()
        {
            // Position du champ dans la grille
            //HtmlInputHidden hidCellpos = new HtmlInputHidden();
            //hidCellpos.ID = "hidCellpos";
            //hidCellpos.Value = _cellPos;
            //_divHidden.Controls.Add(hidCellpos);

            //_pgContainer.Controls.Add(_divHidden);


            if (base.Build())
            {
                try
                {
                    OrmMappingInfo ormInfos = eLibTools.OrmLoadAndGetMapWeb(_ePref);


                    String error = String.Empty;

                    _pgContainer.ID = "paramTab2";
                    _pgContainer.Attributes.Add("class", "paramBlock");
                    _pgContainer.Style.Add(HtmlTextWriterStyle.Display, "none");


                    // Titre
                    HtmlGenericControl title = new HtmlGenericControl("h3");
                    title.ID = "paramTitleTab2";
                    if (DescId == 0)
                    {
                        title.InnerHtml = eResApp.GetRes(Pref, 581);
                    }
                    else
                    {
                        title.InnerHtml = eResApp.GetRes(Pref, 6881);
                    }
                    _pgContainer.Controls.Add(title);


                    Panel block = new Panel();
                    block.CssClass = "paramBlockContent";
                    block.Attributes.Add("sys", _isSys ? "1" : "0");
                    _pgContainer.Controls.Add(block);

                    Panel panel = new Panel();
                    panel.CssClass = "paramPartContent";
                    panel.Attributes.Add("data-active", "1");
                    block.Controls.Add(panel);

                    if (DescId == 0)
                    {
                        HtmlGenericControl text = new HtmlGenericControl();
                        text.InnerHtml = eResApp.GetRes(Pref, 7605);
                        panel.Controls.Add(text);
                    }
                    else
                    {
                        eAdminField adminField;





                        String fieldFormatValue = _field.Format.GetHashCode().ToString();
                        if (_field.Format == FieldFormat.TYP_USER)
                        {
                            eudoDAL eDal = null;
                            try
                            {
                                eDal = eLibTools.GetEudoDAL(Pref);
                                eDal.OpenDatabase();
                                eUserValue uservalueOnlyGP = new eUserValue(eDal, DescId, TypeUserValue.MULTIUSR_GROUP_ONLY, Pref.User);
                                if (uservalueOnlyGP.Build() && uservalueOnlyGP.Enabled)
                                    fieldFormatValue = FieldFormat.TYP_GROUP.GetHashCode().ToString();
                            }
                            finally
                            {
                                if (eDal != null && eDal.IsOpen)
                                    eDal.CloseDatabase();
                            }
                        }

                        #region Type de la rubrique

                        if ((_field.DescId - _field.Table.DescId) < eLibConst.MAX_NBRE_FIELD)
                        {
                            adminField = new eAdminDropdownField(DescId,
                                    eResApp.GetResWithColon(Pref, 105),
                                    eAdminUpdateProperty.CATEGORY.DESC,
                                    eLibConst.DESC.FORMAT.GetHashCode(),
                                    createFieldsTypesListItems(),
                                    value: fieldFormatValue,
                                    renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                                    customLabelCSSClasses: "info");

                            adminField.SetFieldControlID("ddlFieldType");
                            adminField.Generate(panel);
                            DropDownList ddl = (DropDownList)adminField.FieldControl;

                            #region On liste les capsules en attribut dans les options pour pouvoir selectionner celle qui correspond au champ en cours 
                            List<eAdminCapsule<eAdminUpdateProperty>> liCaps = new List<eAdminCapsule<eAdminUpdateProperty>>();
                            List<eAdminCapsule<eAdminUpdateProperty>> liCapsNotNull = new List<eAdminCapsule<eAdminUpdateProperty>>();
                            eAdminCapsule<eAdminUpdateProperty> capsule = null;

                            for (int i = 0; i < ddl.Items.Count; i++)
                            {
                                liCaps.Add(null);

                                ListItem item = ddl.Items[i];
                                if (!String.IsNullOrEmpty(item.Attributes["cplt"]))
                                {
                                    //L'index de la capsule est le même que celui du ListItem dans la DropDownList
                                    liCaps[i] = JsonConvert.DeserializeObject<eAdminCapsule<eAdminUpdateProperty>>(item.Attributes["cplt"]);
                                }

                            }

                            // on se servira de cette liste raccourcie pour trouver la capsule correspondant à notre recherche 
                            // mais on se basera sur liCaps pour récupérer l'index correspondant à l'index de l'item dans la dropdownlist.
                            liCapsNotNull = liCaps.FindAll(delegate (eAdminCapsule<eAdminUpdateProperty> c) { return c != null; });

                            #endregion


                            #region En fonction du format du champ on utilise le delegate adapté

                            switch (_field.Format)
                            {
                                case FieldFormat.TYP_CHAR:
                                    #region Sous catégorie de Caractère

                                    #region on détermine le type de catalogue

                                    PopupType p = PopupType.NONE;

                                    if (_field.PopupType == PopupType.DATA)
                                    {
                                        p = PopupType.DATA;
                                    }
                                    else if (_field.PopupType == PopupType.ONLY && _field.PopupDescId % 100 == 1)
                                    {
                                        p = PopupType.ONLY;
                                    }
                                    else if (_field.PopupType == PopupType.FREE || _field.PopupType == PopupType.ONLY)
                                    {
                                        p = PopupType.ONLY;
                                        if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                                        {
                                            ddl.Attributes.Add("disabled", "1");
                                            ddl.Attributes.Add("title", eResApp.GetRes(Pref, 7660));
                                        }

                                    }
                                    #endregion

                                    //Type relation
                                    if (_field.IsSpecialCatalog)
                                    {

                                        #region Champ de type relation
                                        capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                        {
                                            return (caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                && pty.Property == (int)eLibConst.DESC.POPUP
                                                                && pty.Value == ((int)p).ToString();
                                                        })
                                                    && caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                && pty.Property == (int)eLibConst.DESC.POPUPDESCID
                                                                && int.Parse(pty.Value) % 100 == 1;
                                                        })
                                                            );
                                        });
                                        #endregion
                                    }
                                    else if (_field.PopupType == PopupType.DATA)
                                    {
                                        #region catalogue avancé
                                        if (_field.Multiple)
                                        {
                                            #region Choix multiple

                                            capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                            {
                                                return (caps.ListProperties.Exists(
                                                           delegate (eAdminUpdateProperty pty)
                                                           {
                                                               return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                   && pty.Property == (int)eLibConst.DESC.POPUP
                                                                   && pty.Value == ((int)p).ToString();
                                                           })
                                                       && caps.ListProperties.Exists(
                                                           delegate (eAdminUpdateProperty pty)
                                                           {
                                                               return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                   && pty.Property == (int)eLibConst.DESC.MULTIPLE
                                                                   && pty.Value == "1";
                                                           })
                                                               );
                                            });

                                            #endregion
                                        }
                                        else
                                        {

                                            capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                            {
                                                return (caps.ListProperties.Exists(
                                                           delegate (eAdminUpdateProperty pty)
                                                           {
                                                               return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                   && pty.Property == (int)eLibConst.DESC.POPUP
                                                                   && pty.Value == ((int)p).ToString();
                                                           })
                                                       && caps.ListProperties.Exists(
                                                           delegate (eAdminUpdateProperty pty)
                                                           {
                                                               return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                   && pty.Property == (int)eLibConst.DESC.MULTIPLE
                                                                   && pty.Value == "0";
                                                           })
                                                               );
                                            });
                                        }
                                        #endregion
                                    }
                                    else if (_field.PopupType == PopupType.ONLY || _field.PopupType == PopupType.FREE)
                                    {
                                        #region catalogue v7 historique
                                        //Catalogue v7 - Historique - l'entrée catalogue v7 est unique et lié au popupdescid
                                        // du champ en cours
                                        capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                        {
                                            return (caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                && pty.Property == (int)eLibConst.DESC.POPUP
                                                                && (
                                                                        pty.Value == ((int)PopupType.ONLY).ToString()
                                                                    || pty.Value == ((int)PopupType.FREE).ToString()
                                                                    );
                                                        })
                                                    && caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                && pty.Property == (int)eLibConst.DESC.POPUPDESCID
                                                                && int.Parse(pty.Value) == _field.PopupDescId;
                                                        })
                                                            );
                                        });
                                        #endregion
                                    }
                                    else
                                    {

                                        #region Autre - A prirori, champ caractère "classique"

                                        capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                        {
                                            return (caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                && pty.Property == (int)eLibConst.DESC.POPUP
                                                                && pty.Value == ((int)p).ToString();
                                                        })
                                                    );
                                        });

                                        #endregion
                                    }

                                    //Type Login
                                    if (_field.IsUnique)
                                    {
                                        capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                        {
                                            return (caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESCADV
                                                                && pty.Property == (int)DESCADV_PARAMETER.IS_UNIQUE
                                                                && pty.Value == "1";
                                                        })
                                                    );
                                        });

                                        ddl.Attributes.Add("disabled", "1");
                                    }

                                    #endregion

                                    break;
                                case FieldFormat.TYP_MEMO:
                                    #region Sous catégorie de Memo

                                    capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                    {
                                        return (caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                && pty.Property == (int)eLibConst.DESC.HTML
                                                                && pty.Value == (_field.IsHTML ? "1" : "0");
                                                        })
                                               );
                                    });

                                    #endregion


                                    break;
                                case FieldFormat.TYP_TITLE:

                                    #region Sous catégorie de Titre / Etiquette 

                                    capsule = liCapsNotNull.Find(delegate (eAdminCapsule<eAdminUpdateProperty> caps)
                                    {
                                        return (caps.ListProperties.Exists(
                                                        delegate (eAdminUpdateProperty pty)
                                                        {
                                                            return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                && pty.Property == (int)eLibConst.DESC.LENGTH
                                                                && pty.Value == _field.Length.ToString();
                                                        })
                                               );
                                    });

                                    #endregion

                                    break;
                                default:
                                    break;
                            }


                            if (capsule != null)
                            {
                                ListItem item = ddl.Items[liCaps.IndexOf(capsule)];
                                if (item.Value == fieldFormatValue)
                                    //L'index de la capsule est le même que celui du ListItem dans la DropDownList
                                    ddl.SelectedIndex = liCaps.IndexOf(capsule);
                            }

                            #endregion

                            //A voir si on se limite aux catalogue
                            bool catalogOrm = ormInfos.GetAllMappedDescid.Contains(_field.DescId)

                                &&
                                ((int)_field.PopupType > 2
                                || _field.IsSpecialCatalog
                                );

                            // Le type n'est pas modifiable si le champ est spécial : appartient a la table systeme
                            bool bEnabled =
                                !_field.IsSpecialField()
                                && _field.IsUserAllowedToUpdate()
                                && !catalogOrm;

                            #region Autorisation de transformer les catalogue v7 pour certains champs normalement verrouillée

                            //Désactivé pour l'instant(24/08/2017)
                            if (false && !bEnabled && _field.IsSpecialField())
                            {
                                //Si le champ est un champ "IsSpecialField" mais du type obsolète catalogue,
                                // on autorise sa transformation vers un catalogue avancé/champ texte 

                                // Il ne faut pas l'étendre à tous les champs, par exemple PP03, particule ne doit pas être transformer
                                // De même, les rubriques postal sont pour l'instant bloqué
                                // en autre chose que du texte sans précaution (l'appli considère "en dur" que ce champ doit être du texte brut)
                                if (
                                      (_field.PopupType == PopupType.FREE || _field.PopupType == PopupType.ONLY) // On autorise que la transformation de catalogue v7
                                    && !_field.IsSpecialCatalog // test spécial pour les catalogues relations
                                    && _field.DescId != 203 // Particule est utilisé comme du texe "en dur" Il faut l'exculre explicitement aussi en dur
                                    && !eLibTools.IsCommonPmAdressGlobalPostalField(Pref, _field.DescId) // Il faut exclure les rubriques postales - Elles doivent être mise à jour simulanément entre pm/adresse
                                    )

                                {

                                    // on autorise les transfo
                                    bEnabled = true;

                                    // On limite les type de destinations (catalogue avancé ou texte)
                                    foreach (ListItem itm in ddl.Items)
                                    {
                                        //exclusion des type non char
                                        if (itm.Value != "1")
                                        {
                                            itm.Enabled = false;
                                        }
                                        else
                                        {

                                            if (!String.IsNullOrEmpty(itm.Attributes["cplt"]))
                                            {

                                                //Exclusion des relations
                                                var cptl = JsonConvert.DeserializeObject<eAdminCapsule<eAdminUpdateProperty>>(itm.Attributes["cplt"]);


                                                if (cptl.ListProperties.Exists(
                                                                 delegate (eAdminUpdateProperty pty)
                                                                 {
                                                                     return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                         && pty.Property == (int)eLibConst.DESC.POPUP
                                                                         && pty.Value == "2";
                                                                 })
                                                             && cptl.ListProperties.Exists(
                                                                 delegate (eAdminUpdateProperty pty)
                                                                 {
                                                                     return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                         && pty.Property == (int)eLibConst.DESC.POPUPDESCID
                                                                         && int.Parse(pty.Value) % 100 == 1;
                                                                 }))
                                                {
                                                    itm.Enabled = false;
                                                }


                                                //Que de multiple à multiple
                                                if (_field.Multiple)
                                                {
                                                    if (cptl.ListProperties.Exists(
                                                                delegate (eAdminUpdateProperty pty)
                                                                {
                                                                    return pty.Category == (int)eAdminUpdateProperty.CATEGORY.DESC
                                                                        && pty.Property == (int)eLibConst.DESC.MULTIPLE
                                                                        && pty.Value == "0";
                                                                }))

                                                    {
                                                        itm.Enabled = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            ddl.Enabled = bEnabled;



                        }
                        #endregion

                        bool bIsCampaignSpecialField = (_field.Table.TabType == TableType.CAMPAIGN && _field.IsSpecialField() || _field.Table.TabType == TableType.CAMPAIGNSTATSADV);
                        bool bIsPaymentTransactionField = _field.Table.TabType == TableType.PAYMENTTRANSACTION;
                        bool bIsEventStepSpecialField = _field.Table.IsEventStep && _field.IsSpecialField();
                        bool bIsPJSpecialField = _field.Table.EdnType == EdnType.FILE_PJ && _field.IsSpecialField();

                        bool bLockedField = bIsEventStepSpecialField;

                        // Libellé
                        adminField = new eAdminTextboxField(DescId, eResApp.GetResWithColon(Pref, 7604), eAdminUpdateProperty.CATEGORY.RES, Pref.LangId, value: _field.Labels[Pref.LangId], id: "txtFieldName", readOnly: bLockedField);
                        adminField.Generate(panel);

                        bLockedField = bIsCampaignSpecialField || bIsEventStepSpecialField || bIsPJSpecialField || _field.NoAdmin || bIsPaymentTransactionField;

                        // Administration restreinte
                        if (!bLockedField)
                        {
                            adminField = new eAdminCheckboxField(DescId, eResApp.GetRes(Pref, 7697), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.SUPERADMINONLY.GetHashCode(),
                            value: _field.SuperAdminOnly);
                            adminField.Generate(panel);
                            if (Pref.User.UserLevel < UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
                            {
                                adminField.HideField();
                            }
                        }

                        eAdminRenderer rdr = null;

                        if (_field.Format == FieldFormat.TYP_BIT)
                        {
                            rdr = eAdminRendererFactory.CreateAdminFieldLogicDisplayRenderer(Pref, _field);
                            addRenderer(block, rdr);
                        }

                        bLockedField = bIsEventStepSpecialField;
                        if (!bLockedField)
                        {
                            // Relations
                            if ((_field.Format == FieldFormat.TYP_CHAR && _field.PopupDescId % 100 == 1 && _field.PopupType == PopupType.ONLY)
                                || _field.Format == FieldFormat.TYP_ALIASRELATION)
                            {
                                rdr = eAdminRendererFactory.CreateAdminFieldRelationsRenderer(Pref, _field);
                                addRenderer(block, rdr);
                            }

                            // Bloc caractéristiques
                            rdr = eAdminRendererFactory.CreateAdminFieldFeaturesRenderer(Pref, _field);

                            if (rdr.ErrorMsg.Length > 0)
                                throw rdr.InnerException ?? new Exception(rdr.ErrorMsg);

                            addRenderer(block, rdr);

                            // Bloc mise en page
                            rdr = eAdminRendererFactory.CreateAdminFieldLayoutRenderer(Pref, _field);
                            addRenderer(block, rdr);
                        }

                        //Droits,règles, comportements conditionnels et automatismes
                        // Bloqué pour les champs "system" de campagne
                        bLockedField = bIsCampaignSpecialField || bIsEventStepSpecialField || bIsPJSpecialField || _field.NoAdmin || bIsPaymentTransactionField;
                        if (!bLockedField)
                        {
                            rdr = eAdminRendererFactory.CreateAdminFieldRightsAndRulesRenderer(Pref, _field);
                            addRenderer(block, rdr);
                        }

                        bLockedField = bIsEventStepSpecialField || _field.NoAdmin; ;
                        if (!bLockedField)
                        {
                            //Relation (Rubrique de type Alias)
                            if (_field.Format == FieldFormat.TYP_ALIAS)
                            {
                                rdr = eAdminRendererFactory.CreateAdminFieldAliasRelationRenderer(Pref, _field);
                                addRenderer(block, rdr);
                            }



                            // Catalogue associé
                            if ((DescId - _field.Table.DescId) < eLibConst.MAX_NBRE_FIELD
                                && !_field.CatSystem
                                && ((_field.Format == FieldFormat.TYP_CHAR && _field.PopupType != PopupType.NONE) || _field.Format == FieldFormat.TYP_USER)
                                && (!(_field.Format == FieldFormat.TYP_CHAR && _field.PopupDescId % 100 == 1 && _field.PopupType == PopupType.ONLY)) // pas de catalogue associé pour les catalogue relation
                                )
                            {
                                rdr = eAdminRendererFactory.CreateAdminFieldCatalogRenderer(Pref, _field);
                                addRenderer(block, rdr);
                            }
                        }

                        int iShortid = DescId % 100;
                        //champs non modifiables
                        bLockedField = iShortid == (int)AllField.DATE_CREATE
                            || iShortid == (int)AllField.USER_CREATE
                            || iShortid == (int)AllField.DATE_MODIFY
                            || iShortid == (int)AllField.USER_MODIFY
                            || bIsCampaignSpecialField
                            || bIsPaymentTransactionField
                            || bIsEventStepSpecialField
                            || bIsPJSpecialField
                            || _field.Format == FieldFormat.TYP_PASSWORD
                            || _field.NoAdmin;

                        if (_field.Table.TabType != TableType.PJ && !bLockedField) //les droits de traitement et traçabilité doivent être traités sur une autre us
                        {
                            // Traitements
                            rdr = eAdminRendererFactory.CreateAdminFieldProcessRenderer(Pref, _field);
                            addRenderer(block, rdr);
                            //traçabilité
                            if (_field.Format != FieldFormat.TYP_MEMO &&
                                (_field.Table.TabType == TableType.PM || _field.Table.TabType == TableType.PP || _field.Table.TabType == TableType.ADR || _field.Table.TabType == TableType.EVENT)
                                )
                            {
                                rdr = eAdminRendererFactory.CreateAdminFieldTraceabilityRenderer(Pref, _field);
                                addRenderer(block, rdr);
                            }
                        }

                        bLockedField = bIsEventStepSpecialField;
                        if (!bLockedField)
                        {
                            //Langues et paramètres régionaux
                            rdr = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(Pref, _field);
                            addRenderer(block, rdr);
                        }

                        bLockedField = bIsEventStepSpecialField || _field.NoAdmin || _field.Table.EdnType == EdnType.FILE_PJ;

                        //Qualité des données et RGPD
                        if (!bLockedField && eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminRGPD))
                        {
                            rdr = eAdminRendererFactory.CreateAdminFieldDataQualityRenderer(Pref, _field);
                            addRenderer(block, rdr);
                        }


                    }
                }
                catch (Exception e)
                {
                    _sErrorMsg = String.Concat("eAdminFieldsParametersRenderer.Build() => ", e.Message, Environment.NewLine, e.StackTrace);
                    return false;
                }

                return true;
            }




            return false;
        }


        /// <summary>
        /// Ajoute au bloc le contenu du renderer 
        /// </summary>
        /// <param name="block"></param>
        /// <param name="rdr"></param>
        private void addRenderer(Panel block, eRenderer rdr)
        {


            if (rdr.ErrorMsg.Length > 0)
                throw rdr.InnerException ?? new Exception(rdr.ErrorMsg);


            block.Controls.Add(rdr.PgContainer);
        }

        private ListItem[] createFieldsTypesListItems()
        {
            HashSet<FieldFormat> hsetForbiddenFormats = new HashSet<FieldFormat>();
            if (_field.Format != FieldFormat.TYP_ALIASRELATION)
                hsetForbiddenFormats.Add(FieldFormat.TYP_ALIASRELATION);

            List<ListItem> list = eAdminFieldsParametersRenderer.GetFieldsTypesListItems(Pref, _field, DescId, hsetForbiddenFormats, bDisplayLogin: _field.IsUnique);


            //Gestion historique catalogue v7 - saisie libre ou non  (mais pas relation)      
            // On ajoute une entrée spécifique pour le champ
            if (
                        _field.Format == FieldFormat.TYP_CHAR
                    && !_field.IsSpecialCatalog
                    && (_field.PopupType == PopupType.FREE || _field.PopupType == PopupType.ONLY)
               )
            {

                var caps = new eAdminCapsule<eAdminUpdateProperty>();
                caps.DescId = _field.DescId;
                var pty = new eAdminUpdateProperty()
                {
                    Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                    Property = (int)eLibConst.DESC.POPUP,
                    Value = ((int)_field.PopupType).ToString(),
                };
                caps.ListProperties.Add(pty);
                caps.ListProperties.Add(new eAdminUpdateProperty()
                {
                    Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                    Property = (int)eLibConst.DESC.POPUPDESCID,
                    Value = _field.PopupDescId.ToString()
                });

                ListItem li = new ListItem(eAdminTools.GetCharTypeLabel(Pref, DescId, _field.PopupType, _field.PopupDescId));
                li.Value = ((int)_field.Format).ToString();
                list.Add(li);
                li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
                li.Attributes.Add("disabled", "1");

            }


            return list.ToArray();
        }

        private static ListItem GetListItemForFieldType(ePref pref, FieldFormat format, string tooltip = "", eAdminCapsule<eAdminUpdateProperty> caps = null)
        {
            ListItem item = new ListItem(eAdminTools.GetFieldTypeLabel(pref, format), format.GetHashCode().ToString());
            if (!String.IsNullOrEmpty(tooltip))
                item.Attributes.Add("title", tooltip);
            if (caps != null)
                item.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
            return item;
        }

        private static string GetFieldTypeTooltip(ePref pref, FieldFormat format, bool bCharIsCatalog = false, bool bCharIsRelation = false, bool bCharCatIsMultiple = false, bool bCharIsLogin = false, bool bMemoIsHTML = false, bool bTitleIsSeparator = false)
        {
            string tooltip = String.Empty;
            switch (format)
            {
                case FieldFormat.TYP_CHAR:
                    if (bCharIsCatalog && !bCharIsRelation)
                    {
                        if (bCharCatIsMultiple)
                            tooltip = eResApp.GetRes(pref, 7297);
                        else
                            tooltip = eResApp.GetRes(pref, 7296);
                    }
                    else if (!bCharIsCatalog && bCharIsRelation)
                        tooltip = eResApp.GetRes(pref, 7298);
                    else if (bCharIsLogin)
                        tooltip = eResApp.GetRes(pref, 2402);
                    else
                        tooltip = eResApp.GetRes(pref, 6981);
                    break;

                case FieldFormat.TYP_MEMO:
                    tooltip = bMemoIsHTML ? eResApp.GetRes(pref, 7301) : eResApp.GetRes(pref, 7300);
                    break;

                case FieldFormat.TYP_TITLE:
                    tooltip = bTitleIsSeparator ? eResApp.GetRes(pref, 6979) : eResApp.GetRes(pref, 6980);
                    break;

                case FieldFormat.TYP_HIDDEN: tooltip = eResApp.GetRes(pref, 6978); break;
                case FieldFormat.TYP_BIT: tooltip = eResApp.GetRes(pref, 6992); break;
                case FieldFormat.TYP_DATE: tooltip = eResApp.GetRes(pref, 6990); break;
                case FieldFormat.TYP_EMAIL: tooltip = eResApp.GetRes(pref, 6995); break;
                case FieldFormat.TYP_GEOGRAPHY_V2: tooltip = eResApp.GetRes(pref, 7359); break; // TODO: tooltip manquant pour Géography
                case FieldFormat.TYP_AUTOINC: tooltip = eResApp.GetRes(pref, 6989); break;
                case FieldFormat.TYP_IMAGE: tooltip = eResApp.GetRes(pref, 6999); break;
                case FieldFormat.TYP_NUMERIC: tooltip = eResApp.GetRes(pref, 6987); break;
                case FieldFormat.TYP_MONEY: tooltip = eResApp.GetRes(pref, 6987); break; // TODO: tooltip manquant pour Monétaire
                case FieldFormat.TYP_PHONE: tooltip = eResApp.GetRes(pref, 6994); break;
                case FieldFormat.TYP_USER: tooltip = eResApp.GetRes(pref, 7007); break;
                case FieldFormat.TYP_GROUP: tooltip = eResApp.GetRes(pref, 7007); break;  // TODO: res adaptée pour les champs User n'affichant que les groupes                
                case FieldFormat.TYP_WEB: tooltip = eResApp.GetRes(pref, 6998); break;
                case FieldFormat.TYP_IFRAME: tooltip = eResApp.GetRes(pref, 7003); break;
                case FieldFormat.TYP_CHART: tooltip = eResApp.GetRes(pref, 7002); break;
                case FieldFormat.TYP_BITBUTTON: tooltip = eResApp.GetRes(pref, 7693); break;
                case FieldFormat.TYP_ALIAS: tooltip = eResApp.GetRes(pref, 7987); break;
                case FieldFormat.TYP_ALIASRELATION: tooltip = eResApp.GetRes(pref, 8246); break;
                case FieldFormat.TYP_SOCIALNETWORK: tooltip = eResApp.GetRes(pref, 7690); break;  // TODORES: tooltip manquant pour Reseau Social

                case FieldFormat.TYP_FILE: // TODO: champ non repris en admin XRM d'après les specs
                default: tooltip = String.Empty; break;
            }
            return tooltip;
        }

        /// <summary>
        /// Génère la liste des items représentant les principaux types de champs
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="field"></param>
        /// <param name="descid"></param>
        /// <param name="hsetNotTheseFormats">Formats à ne pas envoyer dans la liste</param>
        /// <param name="bDisplayLogin">Indique si on ajoute le type Login</param>
        /// <returns></returns>
        public static List<ListItem> GetFieldsTypesListItems(ePref pref, eAdminFieldInfos field = null, int descid = 0, IEnumerable<FieldFormat> hsetNotTheseFormats = null, bool bDisplayLogin = false)
        {
            if (hsetNotTheseFormats == null)
                hsetNotTheseFormats = new HashSet<FieldFormat>();


            List<ListItem> list = new List<ListItem>();
            ListItem li;
            eAdminCapsule<eAdminUpdateProperty> caps;
            eAdminUpdateProperty pty;

            if (!hsetNotTheseFormats.Contains(FieldFormat.TYP_HIDDEN))
            {
                //list.Add(GetListItemForFieldType(Pref, FieldFormat.TYP_HIDDEN, eResApp.GetRes(Pref, 6978)));
            }

            if (!hsetNotTheseFormats.Contains(FieldFormat.TYP_TITLE))
            {
                #region Separateur            
                caps = new eAdminCapsule<eAdminUpdateProperty>()
                {
                    DescId = descid
                };
                pty = new eAdminUpdateProperty()
                {
                    Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                    Property = (int)eLibConst.DESC.LENGTH,
                    Value = "1"
                };
                caps.ListProperties.Add(pty);
                li = GetListItemForFieldType(pref, FieldFormat.TYP_TITLE, GetFieldTypeTooltip(pref, FieldFormat.TYP_TITLE, bTitleIsSeparator: true), caps: caps);
                list.Add(li);

                #endregion

                #region Etiquette
                caps = new eAdminCapsule<eAdminUpdateProperty>()
                {
                    DescId = descid
                };
                pty = new eAdminUpdateProperty()
                {
                    Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                    Property = (int)eLibConst.DESC.LENGTH,
                    Value = "0"
                };
                caps.ListProperties.Add(pty);
                li = GetListItemForFieldType(pref, FieldFormat.TYP_TITLE, GetFieldTypeTooltip(pref, FieldFormat.TYP_TITLE, bTitleIsSeparator: false), caps);
                list.Add(li);

                li.Text = eResApp.GetRes(pref, 7293);
                #endregion
            }

            if (!hsetNotTheseFormats.Contains(FieldFormat.TYP_CHAR))
            {
                #region Caractère (sans popup)

                caps = new eAdminCapsule<eAdminUpdateProperty>() { DescId = descid };
                caps.ListProperties.Add(new eAdminUpdateProperty()
                {
                    Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                    Property = (int)eLibConst.DESC.POPUP,
                    Value = ((int)PopupType.NONE).ToString()
                });
                list.Add(GetListItemForFieldType(pref, FieldFormat.TYP_CHAR, GetFieldTypeTooltip(pref, FieldFormat.TYP_CHAR), caps));

                #endregion

                if ((field == null || field.Format == FieldFormat.TYP_CHAR))
                {
                    #region Catalogue

                    caps = new eAdminCapsule<eAdminUpdateProperty>();
                    caps.DescId = descid;
                    pty = new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.POPUP,
                        Value = ((int)PopupType.DATA).ToString(),
                    };
                    caps.ListProperties.Add(pty);
                    caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.MULTIPLE,
                        Value = "0",
                    });


                    li = GetListItemForFieldType(pref, FieldFormat.TYP_CHAR, GetFieldTypeTooltip(pref, FieldFormat.TYP_CHAR, bCharIsCatalog: true), caps);
                    li.Text = eResApp.GetRes(pref, 225);
                    list.Add(li);
                    li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
                    #endregion

                    #region Choix Multiple

                    caps = new eAdminCapsule<eAdminUpdateProperty>();
                    caps.DescId = descid;
                    caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.POPUP,
                        Value = ((int)PopupType.DATA).ToString(),
                    }
                    );
                    caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.MULTIPLE,
                        Value = "1",
                    }
                    );
                    li = GetListItemForFieldType(pref, FieldFormat.TYP_CHAR, GetFieldTypeTooltip(pref, FieldFormat.TYP_CHAR, bCharIsCatalog: true, bCharCatIsMultiple: true), caps);
                    li.Text = eResApp.GetRes(pref, 247);
                    list.Add(li);
                    li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));

                    #endregion

                    #region  Relation

                    caps = new eAdminCapsule<eAdminUpdateProperty>() { DescId = descid };
                    caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.POPUP,
                        Value = ((int)PopupType.ONLY).ToString(),
                    });

                    //on récupère le premier descid de la liste:
                    eudoDAL dal = eLibTools.GetEudoDAL(pref);
                    dal.OpenDatabase();
                    String sError; ;
                    caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.POPUPDESCID,
                        Value = eSqlDesc.GetFirstMainFieldDescId(pref, dal, out sError).ToString()
                    });
                    dal.CloseDatabase();

                    li = GetListItemForFieldType(pref, FieldFormat.TYP_CHAR, GetFieldTypeTooltip(pref, FieldFormat.TYP_CHAR, bCharIsRelation: true), caps);
                    li.Text = eResApp.GetRes(pref, 7299);
                    list.Add(li);
                    li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));

                    #endregion
                }

                #region Login
                if (bDisplayLogin)
                {
                    caps = new eAdminCapsule<eAdminUpdateProperty>();
                    caps.DescId = descid;
                    caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.POPUP,
                        Value = ((int)PopupType.NONE).ToString()
                    });
                    caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESCADV,
                        Property = (int)DESCADV_PARAMETER.IS_UNIQUE,
                        Value = "1",
                    });

                    li = GetListItemForFieldType(pref, FieldFormat.TYP_CHAR, GetFieldTypeTooltip(pref, FieldFormat.TYP_CHAR, bCharIsLogin: true), caps);
                    li.Text = eResApp.GetRes(pref, 752);
                    list.Add(li);
                    li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
                }
                #endregion
            }

            if (!hsetNotTheseFormats.Contains(FieldFormat.TYP_MEMO))
            {
                #region MEMO 
                caps = new eAdminCapsule<eAdminUpdateProperty>() { DescId = descid };
                caps.ListProperties.Add(new eAdminUpdateProperty()
                {
                    Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                    Property = (int)eLibConst.DESC.HTML,
                    Value = "0"
                });
                list.Add(GetListItemForFieldType(pref, FieldFormat.TYP_MEMO, GetFieldTypeTooltip(pref, FieldFormat.TYP_MEMO, bMemoIsHTML: false), caps));

                #endregion

                #region MEMO HTML

                caps = new eAdminCapsule<eAdminUpdateProperty>() { DescId = descid };
                caps.ListProperties.Add(new eAdminUpdateProperty()
                {
                    Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                    Property = (int)eLibConst.DESC.HTML,
                    Value = "1",
                });
                li = GetListItemForFieldType(pref, FieldFormat.TYP_MEMO, GetFieldTypeTooltip(pref, FieldFormat.TYP_MEMO, bMemoIsHTML: true), caps);
                li.Text = eResApp.GetRes(pref, 7689);
                list.Add(li);
                li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
                #endregion
            }

            HashSet<FieldFormat> listOtherFormats = new HashSet<FieldFormat>()
            {
                FieldFormat.TYP_DATE,
                FieldFormat.TYP_BIT,
                FieldFormat.TYP_EMAIL,
                FieldFormat.TYP_PHONE,
                FieldFormat.TYP_WEB,
                FieldFormat.TYP_SOCIALNETWORK,
                FieldFormat.TYP_USER,
                FieldFormat.TYP_GROUP,
                FieldFormat.TYP_IMAGE,
                FieldFormat.TYP_FILE,
                FieldFormat.TYP_IFRAME,
                FieldFormat.TYP_CHART,
                FieldFormat.TYP_NUMERIC,
                FieldFormat.TYP_AUTOINC,
                FieldFormat.TYP_MONEY,
                FieldFormat.TYP_GEOGRAPHY_V2,
                FieldFormat.TYP_BITBUTTON,
                FieldFormat.TYP_ALIAS,
                FieldFormat.TYP_ALIASRELATION,
                FieldFormat.TYP_PASSWORD
            };

            foreach (FieldFormat format in listOtherFormats)
            {
                if (!hsetNotTheseFormats.Contains(format))
                    list.Add(GetListItemForFieldType(pref, format, GetFieldTypeTooltip(pref, format)));
            }

            list = list.OrderBy(item => item.Text).ToList<ListItem>();

            return list;

        }
    }
}