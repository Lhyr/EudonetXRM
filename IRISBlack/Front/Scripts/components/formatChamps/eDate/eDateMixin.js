import { updateMethod, verifComponent, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { Lang, FieldType } from '../../../Enum.js?ver=803000';
import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000';
/**
 * Mixin commune aux composants eDate.
 * */
export const eDateMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            messageError: this.getRes(846),
            messageSuccess: this.getRes(2462),
            modif: false,
            bRegExeSuccess: true,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            patternVerif: /^[0-9]{2}\/[0-9]{2}\/[0-9]{4}$/i,
            that: this,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            icon: false,
            focusIn: false,
            CultureInfoDate: null,
            currentDateDB: null,
            bDateRangePicker: false,
            dateDefautFormat: 'DD/MM/YYYY',
            dateDefautFormatEn: 'YYYY/MM/DD',
            timeDefautFormat: 'HH:mm',
            tabDateFormat: ["DD/MM/YYYY HH:mm", "DD-MM-YYYY HH:mm", "YYYY-MM-DD HH:mm", "YYYY/MM/DD HH:mm", "MM/DD/YYYY HH:mm", "dddd D MMMM YYYY HH:mm", "ddd DD/MM/YYYY HH:mm", "ddd D MMM YYYY HH:mm"]
        };
    },
    components: {
        eDateFile: () => import(AddUrlTimeStampJS("./eDate/eDateFile.js")),
        eDateList: () => import(AddUrlTimeStampJS("./eDate/eDateList.js")),
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js")),
    },
    computed: {
        /** retourne le datetime par défaut. */
        getDateTimeDefautFormat() {
            return this.dateDefautFormat + ' ' + this.timeDefautFormat;
        },
        /** retourne le datetime par défaut en anglais. */
        getDateTimeDefautFormatEn() {
            return this.dateDefautFormatEn + ' ' + this.timeDefautFormat;
        },
        /**
         * Retourne la culture à partir de l'identifiant de langue Eudo.
         * @returns {string} la culture
         * */
        cultureMethod() {
            let iUsrLg = this.getUserLangID;
            if (iUsrLg > Lang.length - 1)
                iUsrLg = 0;

            return Lang[iUsrLg];
        },
        IsDisplayReadOnly() {
            return (this.dataInput.ReadOnly)
        },
        getFormat() {

            let InfoDate = this.getInfoDate;

            if (!this.dataInput
                || !this.dataInput.DisplayValue
                || this.dataInput.DisplayValue == "")
                return "";

            this.CultureInfoDate = this.getDateTimeDefautFormat;   // 07/01/2020

            if (this.dataInput.DisplayFormat
                && this.dataInput.DisplayFormat != "") {
                this.CultureInfoDate = dateFormat[this.dataInput.DisplayFormat] || ((InfoDate.value.toUpperCase() || this.dateDefautFormat) + ' ' + this.timeDefautFormat);
            } else if (InfoDate) {
                this.CultureInfoDate = InfoDate.value.toUpperCase() + ' ' + this.timeDefautFormat; // Current Format global
            }

            if (this.dataInput.Value != "") {
                let newFormat = moment(this.dataInput.Value, this.tabDateFormat, this.cultureMethod);

                let returnDate = (newFormat.format(this.timeDefautFormat) == moment().startOf('day').format(this.timeDefautFormat))
                    ? newFormat.format(this.CultureInfoDate.replace(this.timeDefautFormat, ""))
                    : newFormat.format(this.CultureInfoDate);

                return returnDate;
            }

            return "";
        },

        /**
         * Ressort un id pour les input
         * */
        getIdInput() {
            var txtId = this.propDetail ? 'id_input_detail_' + this.dataInput.DescId
                : this.propHead ? 'id_input_head_' + this.dataInput.DescId
                    : this.propAssistant ? 'id_input_assistant_' + this.propAssistantNbIndex + '_' + this.dataInput.DescId
                        : this.propListe ? 'id_input_liste_' + this.propIndexRow + '_' + this.dataInput.DescId
                            : '';

            return txtId;
        },
    },

    watch: {
        "dataInput": function () {
            this.InitDtComponent();
        }
    },
    mounted() {
        this.displayInformationIco();
        let InfoDate = this.getInfoDate;
        this.CultureInfoDate = this.getDateTimeDefautFormat;   // 07/01/2020

        if (this.dataInput.DisplayFormat && this.dataInput.DisplayFormat != "") {
            this.CultureInfoDate = dateFormat[this.dataInput.DisplayFormat] || ((InfoDate.value.toUpperCase() || this.dateDefautFormat) + ' ' + this.timeDefautFormat);
        } else if (InfoDate) {
            this.CultureInfoDate = InfoDate.value.toUpperCase() + ' ' + this.timeDefautFormat; // Current Format global
		} else if (typeof moment.locale == 'function') {
            let newFormat = moment(this.dataInput.DisplayValue, this.tabDateFormat, this.cultureMethod);
            this.CultureInfoDate = newFormat._f;
        }

        var that = this
        var focusDate;

        this.InitDtComponent();

        if (that.propDetail) {
            focusDate = '#id_input_detail_' + that.dataInput.DescId
        } else if (that.propAssistant) {
            focusDate = '#id_input_assistant_' + that.propAssistantNbIndex + '_' + that.dataInput.DescId
        } else if (that.propListe) {
            focusDate = '#id_input_liste_' + that.propIndexRow + '_' + that.dataInput.DescId
        }
        else if (that.propHead) {
            focusDate = '#id_input_head_' + that.dataInput.DescId
        }

        if (this.bDateRangePicker)
            that.setDateRangePicker(focusDate);
        else
            that.setDateTimePicker(focusDate);

    },
    methods: {
        displayInformationIco,
        showInformationIco,
        verifComponent,
        onUpdateCallback,
        showTooltip,
        updateListVal,
        hideTooltip,

        /**
         * Initialise le composant, avec les descid de date de debut, de fin
         * et si c'est un daterangepicker.
         * */
        InitDtComponent() {
            //daterangepicker on rassemble à dispo les dates de début et de fin
            if (this.dataInput.DateEndDescId > 0 && this.dataInput.DateStartDescId == 0)
                this.dataInput.DateStartDescId = this.dataInput.DescId;

            if (this.dataInput.DateStartDescId > 0 && this.dataInput.DateEndDescId == 0)
                this.dataInput.DateEndDescId = this.dataInput.DescId;

            if ((this.propDetail || this.propAssistant || this.propHead) && this.dataInput.DateStartDescId > 0 && this.dataInput.DateEndDescId > 0)
                this.bDateRangePicker = true;
        },

        blurInputValidate(event) {
            var that = this
            let input = this.$refs[this.getIdInput].value;

            if (input != "") {

                if (this.bDateRangePicker)
                    return;

                if (input != that.dataInput.DisplayValue) {

                    let dtNewValue = moment(input, this.CultureInfoDate, this.cultureMethod);

                    if (!dtNewValue.isValid())
                        throw this.getRes(959);
                    updateMethod(that, dtNewValue.format(this.getDateTimeDefautFormat), undefined, undefined, this.dataInput);
                }
            } else if (this.bDateRangePicker && input == "") {
                event.stopPropagation();
                this.bEmptyDisplayPopup = false;
                let additionalData = {
                    'Fields': []
                };


                let objInit = {
                    Format: FieldType.Date,
                    newvalue: input,
                    newdisplay: input,
                }

                let updEndDate = Object.assign({ Descid: that.dataInput.DateEndDescId }, objInit);
                let updStartdDate = Object.assign({ Descid: that.dataInput.DateStartDescId }, objInit);

                additionalData.Fields.push(updEndDate, updStartdDate);

                $(this).val('');
                verifComponent(undefined, event, this.dataInput.Value, this.that, this.dataInput);
                if (!this.bEmptyDisplayPopup) {
                    updateMethod(that, input, undefined, undefined, this.dataInput);
                }
                that.dataInput.DisplayValue = input;
            } else if (!this.bDateRangePicker && input == "") {
                event.stopPropagation();
                this.bEmptyDisplayPopup = false;
                $(this).val('');
                verifComponent(undefined, event, this.dataInput.Value, this.that, this.dataInput);
                if (!this.bEmptyDisplayPopup) {
                    updateMethod(that, input, undefined, undefined, this.dataInput);
                }
                this.dataInput.DisplayValue = input;
                this.dataInput.value = input;
            }
        },

        focus(evt) {
            var that = this;
            //var input;
            //let idInput = that.propDetail ? 'id_input_detail_' : that.propAssistant ? 'id_input_assistant_' + that.propAssistantNbIndex + '_' : that.propListe ? 'id_input_liste_' + that.propIndexRow + '_' : '';

            //input = document.getElementById(this.getIdInput/* + that.dataInput.DescId*/);
            let input = this.$refs[this.getIdInput];
            if (input) {
                input.focus();
                that.openCalendar(evt, 'clicked');
            }
        },
        openCalendar() {
            //if (this.bDateRangePicker)
            //    return;

            var that = this;
            //var papa = this.$parent.$el.querySelector('div[divdescid="' + this.dataInput.DescId + '"]');
            var papa;
            papa = that.propDetail ? that.$parent.$el.querySelector('div[divdescid="' + that.dataInput.DescId + '"]') : that.propAssistant ? that.$parent.$el.querySelector('div[divdescidassitzone="' + that.dataInput.DescId + '_' + that.propAssistantNbIndex + '"]') : that.propListe ? that.$parent.$el.querySelector('#' + 'id_input_liste_' + that.propIndexRow + '_' + that.dataInput.DescId) : that.propHead ? that.$parent.$el.querySelector('div[divdescid="' + that.dataInput.DescId + '"]') : '';
            let position = papa.getBoundingClientRect();
            let sQuerySelector = (this.bDateRangePicker) ? 'div.daterangepicker' : 'div.bootstrap-datetimepicker-widget';
            let dateModal = [...document.getElementsByTagName('body')]
                .map(dc => dc.querySelector(sQuerySelector))
                .find(dc => dc)

            if (!dateModal)
                return false;


            dateModal.style.top = ((position.top + dateModal.offsetHeight > window.innerHeight)
                ? (position.top - 350)
                : (position.top + 55)) + 'px';


            dateModal.style.left = (position.left + 15) + 'px';
        },
        setDateTimePicker(focusDate) {

            var that = this;

            $(focusDate).on("dp.show", function (e) {
                that.openCalendar()
                $('.todayClass').html(that.getRes(143));
                $('.timeTxt').html(that.getRes(851));


                $(".timeTxt").parents("a[data-action]").prop("title", that.getRes(851))

                $(".timeTxt").parents("a[data-action]").click(function () {
                    $(".timeTxt").parents("a[data-action]").prop("title", that.getRes(135))
                    $('.timeTxt').html(that.getRes(135));


                    $(".dateTxt").parents("a[data-action]").prop("title", that.getRes(851))
                    $('.dateTxt').html(that.getRes(851));

                });
            });

            $(focusDate).on("dp.change", function (e) {
                that.currentDateDB = (that.CultureInfoDate)
                    ? e.date.format(that.CultureInfoDate)
                    : e.date.format(this.getDateTimeDefautFormat);
            });

            try {
                $(focusDate).datetimepicker({
                    locale: this.cultureMethod,
                    format: this.CultureInfoDate,
                    widgetParent: '#eDate_' + this._uid,
                    showTodayButton: true,
                    calendarWeeks: true,
                    tooltips: {
                        today: this.getRes(143),
                        clear: this.getRes(2089),
                        close: this.getRes(30),
                        selectMonth: this.getRes(405),
                        prevMonth: this.getRes(136),
                        nextMonth: this.getRes(137),
                        selectYear: this.getRes(406),
                        prevYear: this.getRes(134),
                        nextYear: this.getRes(138),
                        selectDecade: 'Select Decade',
                        prevDecade: 'Previous Decade',
                        nextDecade: 'Next Decade',
                        prevCentury: 'Previous Century',
                        nextCentury: 'Next Century'

                    },
                    icons: {
                        today: 'todayClass',
                        time: 'timeTxt',
                        date: 'dateTxt'
                    }
                });
            } catch (e) {
                console.error(e);
            }


        },
        setDateRangePicker(focusDate) {
            var that = this;

            let dtStart = this.dataInput.DateStartValue != "" ? this.dataInput.DateStartValue : new Date();
            let dtEnd = this.dataInput.DateEndValue != "" ? this.dataInput.DateEndValue : new Date();

            //plus d'info sur le daterangepicker : https://www.daterangepicker.com/
            $(focusDate).daterangepicker({
                parentEl: "body",
                locale: {
                    format: this.getDateTimeDefautFormat, //this.CultureInfoDate,
                    firstDay: 1,
                    applyLabel: this.getRes(219),
                    cancelLabel: this.getRes(29),
                    fromLabel: this.getRes(554),
                    toLabel: this.getRes(553),
                    customRangeLabel: this.getRes(6722),
                    daysOfWeek: this.getRes(885).split(/',?'?/),
                    monthNames: this.getRes(466).split(/',?'?/),
                },
                timePicker24Hour: true,
                startDate: moment(dtStart, this.getDateTimeDefautFormat, this.cultureMethod),
                endDate: moment(dtEnd, this.getDateTimeDefautFormat, this.cultureMethod),
                autoApply: false,
                autoUpdateInput: false,
                timePicker: true,
                showDropdowns: true,
                drops: 'up',
            },
                function (start, end, label) {
                    //daterangepicker on rassemble à dispo les dates de début et de fin
                    let additionalData = {
                        'Fields': []
                    };

                    var dbFormat = that.CultureInfoDate;
                    if (that.CultureInfoDate != this.getDateTimeDefautFormatEn) {
                        dbFormat = this.getDateTimeDefautFormat;
                    }


                    let updEndData = {}
                    updEndData.Descid = that.dataInput.DateEndDescId;
                    updEndData.Format = FieldType.Date;
                    updEndData.NewValue = end.format(dbFormat);
                    updEndData.NewDisplay = end.format(that.CultureInfoDate);

                    // Ajout des infos concernant le champ à mettre à jour
                    // Si le champ a déjà été ajouté dans la liste (erreur de mapping ou autre raison), on met à jour le mapping précédemment ajouté
                    let existingFieldIndex = additionalData.Fields.findIndex(f => f.Descid === updEndData.Descid);
                    if (existingFieldIndex >= 0)
                        additionalData.Fields[existingFieldIndex] = updEndData;
                    else
                        additionalData.Fields.push(updEndData);


                    let updStartdData = {};
                    updStartdData.Descid = that.dataInput.DateStartDescId;
                    updStartdData.Format = FieldType.Date;
                    updStartdData.NewValue = start.format(dbFormat);
                    updStartdData.NewDisplay = start.format(that.CultureInfoDate);

                    existingFieldIndex = additionalData.Fields.findIndex(f => f.Descid === updStartdData.Descid);
                    if (existingFieldIndex >= 0)
                        additionalData.Fields[existingFieldIndex] = updStartdData;
                    else
                        additionalData.Fields.push(updStartdData);


                    if (that.dataInput.DescId == that.dataInput.DateStartDescId) {
                        that.dataInput.DisplayValue = start.format(dbFormat);
                    }
                    if (that.dataInput.DescId == that.dataInput.DateEndDescId) {
                        that.dataInput.DisplayValue = end.format(dbFormat);
                    }

                    updateMethod(that, that.dataInput.DisplayValue, undefined, additionalData, that.dataInput);



                });

            $(focusDate).on('show.daterangepicker', function (ev, picker) {

                //par defaut on charge le daterangepickervers le haut  (drops:"up" dans la déclaration de daterangepicker)
                //mais si on l'élément déclencheur se trouve trop haut sur la page, on affiche vers le haut
                let papa = that.propDetail
                    ? that.$parent.$el.querySelector('div[divdescid="' + that.dataInput.DescId + '"]')
                    : that.propAssistant
                        ? that.$parent.$el.querySelector('div[divdescidassitzone="' + that.dataInput.DescId + '_' + that.propAssistantNbIndex + '"]')
                        : that.propListe
                            ? that.$parent.$el.querySelector('div[divdescid="' + that.dataInput.DescId + '"]')
                            : that.propHead
                                ? that.$parent.$el.querySelector('div[divdescid="' + that.dataInput.DescId + '"]')
                                : '';

                let position = papa.getBoundingClientRect();

                if (position.top - 600 < 0) {
                    picker.drops = "down";
                    picker.move();
                }


            });
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}