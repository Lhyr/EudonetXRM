import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';
import { specialCSS, forbiddenFormatHead, capitalizeString } from "../methods/eFileMethods.js?ver=803000";
import { tabFormatForbid, tabFormatBtnLbl, tabFormatBtnSep, wizardFldsMaxNb, vuetifyInput  } from "../methods/eFileConst.js?ver=803000";
import { FieldType, TableType } from "../methods/Enum.min.js?ver=803000";

/**
 * Mixin commune aux files et aux composants directs (mais pas leurs sous-composants).
 * */
export const eFileMixin = {
    data() {
        return {
            vuetifyInput,
            oFileDetailIncrustBkm: null,
            nBtnSize: 22,
            nIconSize: 14,
            nIconSizeMin: 12,
            nIconSizeBig: 16,
            arShortCutIcons: {
                email: 'mdi-email',
                phone: 'mdi-phone',
                geo: 'mdi-map-marker',
                link: 'mdi-link-variant',
                vcard: 'mdi-account'
            },
            shortcutsWhiteList: [FieldType.Geolocation, FieldType.Phone],
            shortCutType: ''
        }
    },
    mixins: [eMotherClassMixin],
    computed: {
        /** libellé caché en admin */
        labelHidden: function () {
            return (this.propDataDetail || this.propDetail).filter(input => {
                if (input.LabelHidden || specialCSS(tabFormatBtnLbl, input.Format))
                    return true;
            })
        },
        /** Aucun libellé en admin */
        noLabel: function () {
            return (this.propDataDetail || this.propDetail).filter(input => {
                if (specialCSS(tabFormatBtnSep, input.Format))
                    return true;
            })
        },
        /** Renvoie les données pour les icones du menu de raccourcis. */
        getShortcutMenuElements: function () {
            let items = [];
            // DataStruct est utilisé dans headFiche, oFileDetailIncrustBkm est utilisé dans tabPinnedFile
            // maisy bcp des composants utilisent DataStruct comme prop ou data, on ne peux pas déclarer prop et data.
            let dataStruct;
            let tooltipPosition;

            switch (this.shortCutType) {
                case 'headFile':
                    dataStruct = this.DataStruct;
                    tooltipPosition = 'left';
                    break;
                case 'pinnedBkm':
                    dataStruct = this.oFileDetailIncrustBkm;
                    tooltipPosition = 'bottom';
                    break;
                default: break;
            }

            let lstStruct = dataStruct?.Structure?.LstStructFields;
            let shortcuts = dataStruct?.Data?.MenuShortcut;

            if (shortcuts) {
                items = Object.keys(shortcuts).flatMap(key => {

                    let val = shortcuts[key];
                    if (key == "SocialNetworkShortcut") {
                        return val?.flatMap(sn => {
                            let { nType, arCrossOver, arButton, oIconSizes } = this.initShortCutFromBack(lstStruct, sn)

                            if (sn.Fields?.length > 0)
                                return {
                                    ...sn,
                                    Fields: arCrossOver,
                                    message: this.getShortCutMessage(arCrossOver)[key],
                                    Button: arButton,
                                    IconSizes: oIconSizes,
                                    Type: nType,
                                    tooltipPosition: tooltipPosition
                                };

                            return;
                        });
                    }
                    else if (
                        key == "VCardShortcut" && (
                            (this.shortCutType == 'headFile' && this.getTab == TableType.PP) ||
                            (this.shortCutType == 'pinnedBkm' && dataStruct?.Structure?.StructFile?.DescId  == TableType.PP)
                        )

                    ) {
                        // insert miniCard
                        let structVCard = lstStruct?.find(structF => structF.IsMiniFileEnabled == true && structF.TargetTab == TableType.PP);
                        if (structVCard) {
                            return {
                                Fields: [structVCard],
                                ...val,
                                IconSizes: {
                                    value: FieldType.Relation,
                                    btnSize: this.nBtnSize,
                                    iconSize: this.nIconSize,
                                    icon: this.arShortCutIcons.vcard,
                                    tooltipPosition: tooltipPosition
                                },
                            };
                        }
                        return;
                    }
                    else {
                        let { nType, arCrossOver, arButton, oIconSizes } = this.initShortCutFromBack(lstStruct, val);

                        if (val.Fields?.length > 0)
                            return {
                                ...val,
                                Fields: arCrossOver,
                                message: this.getShortCutMessage(arCrossOver)[key],
                                Button: arButton,
                                IconSizes: oIconSizes,
                                Type: nType,
                                tooltipPosition: tooltipPosition
                            };

                        return;
                    }
                });
            }

            return items?.filter(itm => itm);
        }
    },
    methods: {
        capitalizeString,
        /** renvoie les classes css du libellé en fonction de l'admin */
        setFileLabelClass: function (input) {
            return {
                'italicLabel': input.Italic,
                'boldLabel': input.Bold,
                'underLineLabel': input.Underline,
                'labelHidden': this.labelHidden.find(i => i.DescId == input.DescId),
                'no-label': this.noLabel.find(i => i.DescId == input.DescId)
            };
        },
        /** Renvoie le type de composant comme classe css */
        getComponentType(input) {
            let eFormat = input.AliasSourceField?.Format || input?.Format;
            return Object.keys(FieldType).find(key => FieldType[key] === eFormat);
        },
        /**
         * initialise les varaibles pour l'affichage de la barre de raccourcis.
         * @param {any} lstStruct
         * @param {any} sn
         */
        initShortCutFromBack: function (lstStruct, sn) {

            let nType = this.getFormatFromStruct(lstStruct, sn);
            let arCrossOver = sn.Fields?.flatMap((fld, id) => {
                return this.getShortCutObjectToDisplay(lstStruct, fld);
            }).filter(n => n);
            let arButton = arCrossOver?.flatMap((fld, id) => {
                let btn = this.getShortcutBtn(fld)[nType]
                if (Array.isArray(btn)) {
                    btn.unshift({ Label: btn[id].title });
                }
                return btn;
            }).filter(n => n);
            let oIconSizes = this.getShortCutIcon(sn)?.find(icon => icon.value == nType);

            return { nType, arCrossOver, arButton, oIconSizes }
        },
        /** Message des infobulles de la barre de raccourcis */
        getShortCutMessage: function (obj) {
            let urlDomainName = this.getUrlDomainName(obj?.find(fld => fld)?.RootURL);

            return {
                HyperLinkShortcut: this.getRes(obj?.length > 1 ? 3139 : 8937),
                EmailShortcut: this.getRes(obj?.length > 1 ? 3140 : 8936),
                PhoneShortcut: this.getRes(obj?.length > 1 ? 3141 : 8935),
                SocialNetworkShortcut: this.getRes(obj?.length > 1 ? 3142 : 3150).replace('<SOCIAL>', this.capitalizeString(urlDomainName)),
                GeoLocationShortcut: this.getRes(8921)
            }
        },
        /**
         * Retourne le format des champs depuis la structure
         * @param {any} lstStruct
         * @param {any} fld
         */
        getFormatFromStruct: function (lstStruct, val) {
            return lstStruct?.find(structF => val.Fields?.map(fld => fld.DescId).includes(structF?.AliasSourceField?.DescId || structF.DescId))?.AliasSourceField?.Format
                || lstStruct?.find(structF => val.Fields?.map(fld => fld.DescId).includes(structF?.AliasSourceField?.DescId || structF.DescId))?.Format;
        },
        /** Icônes des raccourcis */
        getShortCutIcon: function (sn) {
            let arrIcon = [
                { value: FieldType.MailAddress, btnSize: this.nBtnSize, iconSize: this.nIconSizeMin, icon: this.arShortCutIcons.email },
                { value: FieldType.Phone, btnSize: this.nBtnSize, iconSize: this.nIconSizeMin, icon: this.arShortCutIcons.phone },
                { value: FieldType.HyperLink, btnSize: this.nBtnSize, iconSize: this.nIconSizeBig, icon: this.arShortCutIcons.link },
                { value: FieldType.Geolocation, btnSize: this.nBtnSize, iconSize: this.nIconSizeBig, icon: this.arShortCutIcons.geo },
                { value: FieldType.SocialNetwork, btnSize: this.nBtnSize, iconSize: this.nIconSize, icon: sn?.Icon },
                { value: FieldType.Relation, btnSize: this.nBtnSize, iconSize: this.nIconSize, icon: this.arShortCutIcons.vcard }
            ]
            return arrIcon;
        },
        /** Renvoie le nom de domaine de l'url */
        getUrlDomainName(url) {
            try {
                return new URL(url)?.hostname?.replace(this.urlDomainRegex, '');
            } catch (e) {
                return ''
            }
        },
        /**
         * Formatage des objets à afficher dans la shortcut
         * @param {any} lstStruct
         * @param {any} fld
         */
        getShortCutObjectToDisplay: function (lstStruct, fld) {
            let struct = lstStruct?.find(structF => (structF?.AliasSourceField?.DescId || structF.DescId) == fld.DescId)?.AliasSourceField
                || lstStruct?.find(structF => (structF?.AliasSourceField?.DescId || structF.DescId) == fld.DescId)

            // les rubriques telephone & geoloc affichons toujours dans le barre de raccourci
            if (struct?.DISPLAYINACTIONBAR || this.shortcutsWhiteList.includes(struct?.Format))
                return { ...fld, ...struct }

            return null;
        },
        getShortcutBtn: function (button) {
            let geolocCoord = {};
            if (button?.Format == FieldType.Geolocation) {
                let val = button.Value.replace(/[A-Za-z()]/gm, '').trim().split(' ').map(nb => parseFloat(nb.replace(',', '.')));
                geolocCoord = {
                    longitude: val[0],
                    latitude: val[1]
                }
            }

            return {
                [FieldType.Phone]: {
                    text: `${this.getRes(8896)} ${button?.Value}`,
                    action: () => {
                        this.askForAnOption(button)
                    }
                },
                [FieldType.HyperLink]: {
                    text: `${this.getRes(8894)} (${button?.Label})`,
                    action: () => {
                        window.open(button.Value);
                    }
                },
                [FieldType.MailAddress]: {
                    text: `${this.getRes(8897)} (${button?.Value})`,
                    action: () => {
                        var objParentInfo = { parentTab: this.getTab, parentFileId: this.getFileId }
                        top.selectFileMail(getParamWindow().document.getElementById("MLFiles"), button.Value, objParentInfo, TypeMailing.MAILING_UNDEFINED);
                    }
                },
                [FieldType.SocialNetwork]: {
                    text: `${this.getRes(8895)} (${button?.Value})`,
                    action: () => {
                        window.open(button.RootURL != '' ? button.RootURL + button.Value : 'http://' + button.Value)
                    }
                },
                [FieldType.Geolocation]: [
                    {
                        title: button?.Label,
                        text: this.getRes(3144),
                        action: () => {
                            window.open(`https://www.google.com/maps/search/?api=1&query=${geolocCoord.latitude}%2C${geolocCoord.longitude}`)
                        }
                    },
                    {
                        title: button?.Label,
                        text: this.getRes(3145),
                        action: () => {
                            window.open(`https://bing.com/maps/default.aspx?cp=${geolocCoord.latitude}~${geolocCoord.longitude}&lvl=17`)
                        }
                    },
                    {
                        title: button?.Label,
                        text: this.getRes(3146),
                        action: () => {
                            window.open(`https://www.openstreetmap.org/?mlat=${geolocCoord.latitude}&mlon=${geolocCoord.longitude}#map=18/${geolocCoord.latitude}/${geolocCoord.longitude}`)
                        }
                    },
                ]
            }
        }
    }
}