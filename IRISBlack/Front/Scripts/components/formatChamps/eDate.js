import { sizeForm, updateMethod, focusInput, RemoveBorderSuccessError, verifComponent, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType, Lang, dateFormat, FieldType, Month, MonthShort, Weekdays, WeekDaysShort, WeekdaysMin, LongDateFormat } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eDate",
    data() {
        return {
            messageError: this.getRes(846),
            messageSuccess: this.getRes(2462),
            modif: false,
            bRegExeSuccess: true,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            bShow: false,
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
            timeDefautFormat: 'HH:mm:ss',
            tabDateFormat: ["DD/MM/YYYY HH:mm", "DD-MM-YYYY HH:mm", "YYYY-MM-DD HH:mm", "YYYY/MM/DD HH:mm", "MM/DD/YYYY HH:mm", "dddd D MMMM YYYY HH:mm", "ddd DD/MM/YYYY HH:mm", "ddd D MMM YYYY HH:mm"]
        };
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js"))
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
        /**
         * Renvoie la valeur à afficher dans le champ Date, avec le format souhaité
         * */
        getFormat() {


            if (!this.dataInput?.DisplayValue?.length > 0)
                return "";

            //KHA déplacé dans le created. inutile de le recalculer à chaque fois
            //let InfoDate = this.getInfoDate;
            //this.CultureInfoDate = this.getDateTimeDefautFormat;   // 07/01/2020

            //if (this.dataInput.DisplayFormat
            //    && this.dataInput.DisplayFormat != "") {
            //    this.CultureInfoDate = dateFormat[this.dataInput.DisplayFormat] || ((InfoDate.toUpperCase() || this.dateDefautFormat) + ' ' + this.timeDefautFormat);
            //} else if (InfoDate) {
            //    this.CultureInfoDate = InfoDate.toUpperCase() + ' ' + this.timeDefautFormat; // Current Format global
            //}

            if (this.dataInput.Value != "") {
                let newFormat = moment(this.dataInput.Value, this.tabDateFormat, this.cultureMethod);
                //NHA regression 85002: Nouveau mode fiche]Affichage heure champ date intervalle.
                let indexHhmm = this.CultureInfoDate?.indexOf("HH:mm");
                let bValueMidnight = this.dataInput?.Value?.endsWith('00:00:00');
                let bEndValueMidnight = this.dataInput?.DateEndValue?.endsWith('00:00:00');
                let bStartValueMidnight = this.dataInput?.DateStartValue?.endsWith('00:00:00');
                //NHA regression 85998 : Affichage date et heure 00:00
                // On retire l'affichage de l'heure si :
                // - on affiche pas un intervalle de dates
                if (!(bEndValueMidnight && bStartValueMidnight)) {
                    // - si on est pas en train d'afficher le calendrier
                    if (!this.bShow) {
                        // - qu'on affiche une date à minuit,
                        // - que le format d'affichage comporte les heures,
                        if (bValueMidnight && indexHhmm > -1) {
                            this.CultureInfoDate = this.CultureInfoDate?.substring(0, indexHhmm - 1)
                            indexHhmm = -1; // remise à jour de l'index qui a changé, puisque l'information a été supprimée
                        }
                    }
                    // Sinon, dans tous les autres cas, il faut afficher l'heure (date autre que minuit OU calendrier affiché)
                    // On vérifie alors que le format d'affichage comprenne bien HH:mm, sinon, on le remet à jour
                    if (indexHhmm === -1 && (!bValueMidnight || this.bShow))
                        this.CultureInfoDate = this.CultureInfoDate + " HH:mm";
                }
               
                let returnDate = "";
                if (newFormat.format(this.timeDefautFormat) == moment().startOf('day').format(this.timeDefautFormat))
                    returnDate = newFormat.format(this.CultureInfoDate.replace(this.timeDefautFormat, ""));
                else
                    returnDate = newFormat.format(this.CultureInfoDate);
                return returnDate;
            }

            return "";
        },

        /* US 2151 Modifier UX Intervalle de date - Infobulle */
        getFormatRange() {
            if (this.dataInput.DateStartValue != "") {
                let newFormatStart = moment(this.dataInput.DateStartValue, this.tabDateFormat, this.cultureMethod);
                let newFormatEnd = moment(this.dataInput.DateEndValue, this.tabDateFormat, this.cultureMethod);

                let returnDateRangeStart = (newFormatStart.format(this.timeDefautFormat) == moment().startOf('day').format(this.timeDefautFormat))
                    ? newFormatStart.format(this.CultureInfoDate.replace(this.timeDefautFormat, ""))
                    : newFormatStart.format(this.CultureInfoDate);

                let returnDateRangeEnd = (newFormatEnd.format(this.timeDefautFormat) == moment().startOf('day').format(this.timeDefautFormat))
                    ? newFormatEnd.format(this.CultureInfoDate.replace(this.timeDefautFormat, ""))
                    : newFormatEnd.format(this.CultureInfoDate);

                return returnDateRangeStart + ' ' + ' - ' + ' ' + returnDateRangeEnd;
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
        getValueColor() {
            return this.dataInput.ValueColor != "" ? `color:${this.dataInput.ValueColor}` : "";
        }
    },

    watch: {
        "dataInput": function () {
            this.InitDtComponent();
        }
    },
    created() {
        //moment.locale('fr')
        if (this.cultureMethod == 'fr')
            moment.locale(this.cultureMethod, {
                months: Month,
                monthsShort: MonthShort,
                monthsParseExact: true,
                weekdays: Weekdays,
                weekdaysShort: WeekDaysShort,
                weekdaysMin: WeekdaysMin,
                weekdaysParseExact: true,
                longDateFormat: LongDateFormat,
                dayOfMonthOrdinalParse: /\d{1,2}(er|e)/,
                ordinal: function (number) {
                    return number + (number === 1 ? 'er' : 'e');
                },
                meridiemParse: /PD|MD/,
                isPM: function (input) {
                    return input.charAt(0) === 'M';
                },
                // In case the meridiem units are not separated around 12, then implement
                // this function (look at locale/id.js for an example).
                // meridiemHour : function (hour, meridiem) {
                //     return /* 0-23 hour, given meridiem token and hour 1-12 */ ;
                // },
                meridiem: function (hours, minutes, isLower) {
                    return hours < 12 ? 'PD' : 'MD';
                },
                week: {
                    dow: 1, // Monday is the first day of the week.
                    doy: 4  // Used to determine first week of the year.
                }
            });
        let InfoDate = this.getInfoDate;
        this.CultureInfoDate = this.getDateTimeDefautFormat;   // 07/01/2020

        if (this.dataInput.DisplayFormat && this.dataInput.DisplayFormat != "") {
            this.CultureInfoDate = dateFormat[this.dataInput.DisplayFormat] || ((InfoDate.toUpperCase() || this.dateDefautFormat) + ' ' + this.timeDefautFormat);
        } else if (InfoDate) {
            this.CultureInfoDate = InfoDate.toUpperCase() + ' ' + this.timeDefautFormat; // Current Format global
		} else if (typeof moment.locale == 'function') {
            let newFormat = moment(this.dataInput.DisplayValue, this.tabDateFormat, this.cultureMethod);
            this.CultureInfoDate = newFormat._f;
        } 			

    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
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
        sizeForm,
        displayInformationIco,
        showInformationIco,
        verifComponent,
        onUpdateCallback,
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

            if ((this.propDetail || this.propAssistant || this.propHead) && this.dataInput.DateStartDescId > 0 && this.dataInput.DateEndDescId > 0) {
                this.bDateRangePicker = true;


                let rangedData = $('.rangeData');
                for (var i = 0; i < rangedData.length; i++) {
                    let elem = rangedData[i].id;
                    let drp = $('#' + elem).data('daterangepicker');
                    if (drp) {
                        drp.startDate = moment(this.dataInput.DateStartValue, this.getDateTimeDefautFormat, this.cultureMethod);
                        drp.endDate = moment(this.dataInput.DateEndValue, this.getDateTimeDefautFormat, this.cultureMethod);
                        drp.updateView();
                        drp.updateCalendars();
                    }
                }
            }
        },

        blurInputValidate(event) {
            var that = this
            let input = this.$refs[this.getIdInput].value;

            if (input != ""
                || (event?.relatedTarget != 'undefined' && (event?.relatedTarget == document.querySelector('.monthselect')
                    || event?.relatedTarget == document.querySelector('.yearselect')
                    ))
                ) {

                if (this.bDateRangePicker || this.preventBlurInputValidate) {
                    return;
                }

                if (input != that.getFormat) {
                        
                    let newV = input;    
                    let dtNewValue = moment(input, this.CultureInfoDate, this.cultureMethod);
                    
                    if (!dtNewValue.isValid() && input) {
                        throw this.getRes(959);
                    }

                    // verifier si la valeur est date ou pas, sinon, envoie une vide valeur 
                    if (dtNewValue.isValid()) {
                        newV = dtNewValue.format(this.getDateTimeDefautFormat);
                    }
                        
                    updateMethod(that, newV, undefined, undefined, this.dataInput);
                }
            //ELAIZ - demande 87 610 - on vérifie que la date a changée (dataInput.Value) car sinon le blur se déclenche lorsque l'on sélectionne une date même si on a pas changé de date 
            } else if (this.bDateRangePicker && input == "" && this.dataInput.Value != "") {
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

                verifComponent(undefined, event, this.dataInput.Value, this.that, this.dataInput, additionalData);
                if (!this.bEmptyDisplayPopup) {
                    updateMethod(that, input, undefined, additionalData, this.dataInput);
                }
                that.dataInput.DisplayValue = input;
            } else if (!this.bDateRangePicker && input == "") {
                event.stopPropagation();
                this.bEmptyDisplayPopup = false;
                $(this).val('');
                verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput);
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

        //Quand t'on scroll on ferme le calandrier du dateTimePicker
        closeCalendar(focusDate) {
            if (this.bShow)
                $(focusDate).data("DateTimePicker").hide();
        },

        openCalendar() {
            //if (this.bDateRangePicker)
            //    return;
            this.bShow = true;
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
            if (this.propListe) {
                dateModal.style.top = ((position.top + dateModal.offsetHeight > window.innerHeight)
                    ? (position.top)
                    : (position.top)) + 'px';

                dateModal.style.left = (position.left) + 'px';
            }
            papa.focus();
        },
        setDateTimePicker(focusDate) {
            var that = this;
            //let contentWrap = document.getElementById("mainContentWrap");
            let contentWrap = this.$root?.$children?.find(fiche => fiche.$options.name == 'App').$children?.find(fiche => fiche.$options.name == 'fiche')?.$refs?.mainContentWrap;

            $(focusDate).on("dp.show", function (e) {

                //NHA regression 85998 : Affichage date et heure 00:00
                // Si on a masqué l'affichage des heures parce que la valeur est définie sur 00:00, il faut les réafficher dans le calendrier pour pouvoir saisir les heures
                // Le datetimepicker Bootstrap ne disposant pas d'événement beforeShow, on ne peut le faire qu'au show.
                // Or, à ce moment-là, l'affichage est déjà calculé, et il est trop tard pour modifier le format d'affichage.
                // Il faut donc masquer puis réafficher le composant pour prendre cette modification en compte.
                let indexHhmm = that.CultureInfoDate?.indexOf("HH:mm");
                if (indexHhmm === -1) {
                    that.CultureInfoDate = that.CultureInfoDate + " HH:mm";
                    that.preventBlurInputValidate = true; // on empêche le onBlur de se déclencher à l'appel de hide plus bas
                    $(focusDate).datetimepicker('format', that.CultureInfoDate);
                    $(focusDate).datetimepicker('viewDate', moment(that.dataInput.Value, that.CultureInfoDate));
                    $(focusDate).datetimepicker('hide');
                    $(focusDate).datetimepicker('show');
                    // Correctif pour la demande 88812 - Sur Firefox, le fait de fermer le composant datetimepicker ('hide'), enlève le focus au champs et empêche ainsi de refermer le 
                    // calendrier par la suite. Du coup, il faut remettre le focus par la suite car sinon le seul moyen de fermer le calendrier est de re-cliquer sur le champs une fois ouvert
                    // J'ai mis un setTimeout car sinon le focus ne se fait pas, probablement car la méthode datetimepicker('hide') n'a pas fini au moment où l'on fait appel à focus()
                    // Mettre des awaits sur les methodes hide et show ne fonctionne pas non plus, ni faire 1 focus dans un $nextTick ou faire un await $nextTick
                    // On cible uniquement firefox, qui est le navigateur qui pose problème.
                    let browser = new getBrowser();
                    if (browser.isFirefox) {
                        //this correspond au champ
                        setTimeout(() => {
                            this.focus();
                        }, 500)
                    }
                    return; // l'appel à show() ci-dessus va re-déclencher le on("dp.show"), il faut donc sortir pour ne pas exécuter 2 fois le code ci-dessous
                }
                else {
                	// Une fois que l'on a plus besoin de bloquer le onBlur, on le signifie
                    that.preventBlurInputValidate = false;
                }

                that.openCalendar();
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

            $(focusDate).on("dp.hide", function (e) {
                that.bShow = false;
            });
        
            $(focusDate).on("dp.change", function (e) {
                that.currentDateDB = (that.CultureInfoDate)
                    ? moment(e.date, that.CultureInfoDate)
                    : e.date?.format(this.getDateTimeDefautFormat); 
            });

            contentWrap?.addEventListener('scroll', (e) => {
                if (typeof that.closeCalendar == 'function')
                    that.closeCalendar(focusDate);
            });

            try {
                $(focusDate).datetimepicker({
                    //debug : true,
                    locale: this.cultureMethod,
                    format: this.CultureInfoDate,
                    widgetParent: !this.propListe ? '#eDate_' + this._uid : 'body',
                    showTodayButton: true,
                    calendarWeeks: true,
                    useCurrent: false,
                    viewDate: moment(new Date()).hours(0).minutes(0).seconds(0).milliseconds(0),
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


            let daysOfWeekTabSubing = [];
            let daysOfWeekTab = this.getRes(885).split(/',?'?/);
            daysOfWeekTab.forEach(day => day != '' ? daysOfWeekTabSubing.push(day.substring(0, 3)) : '');


            let monthNamestabSubing = [];
            let monthNamesTab = this.getRes(466).split(/',?'?/);
            monthNamesTab.forEach(month => month != '' ? monthNamestabSubing.push(month) : '');





            let dtStart = this.dataInput.DateStartValue != "" ? this.dataInput.DateStartValue : new Date();
            let dtEnd = this.dataInput.DateEndValue != "" ? this.dataInput.DateEndValue : new Date();


            /* Function qui permet de trigger le click du date start */
            $.fn.onStartDateChosen = function (cb) {
                var t, el;
                $(this).on('show.daterangepicker', function (ev, picker) {
                    let ParentId = $(focusDate)[0].parentElement.parentElement.id;
                    var sd = $('#' + ParentId + ' td.start-date').data('title');
                    t = setInterval(() => {
                        if (sd == $('#' + ParentId + ' td.start-date').data('title')) return;
                        sd = $('#' + ParentId + ' td.start-date').data('title');
                        if (!el || el !== sd) {
                            el = sd;
                            cb(ev, picker);
                        }
                    }, 10);
                });
                /* A la fermeture du calendrier */
                $(this).on('hide.daterangepicker', function (ev, picker) {
                    let inputEnd = $('[divdescid=' + that.dataInput.DateEndDescId + ']');
                    for (var i = 0; i < inputEnd.length; i++) {
                        inputEnd[i].classList.remove('selectedDate')
                    }
                    let inputStart = $('[divdescid=' + that.dataInput.DateStartDescId + ']');
                    for (var i = 0; i < inputStart.length; i++) {
                        inputStart[i].classList.remove('selectedDate')
                    }
                    clearInterval(t)
                });
            }


            /* Function qui permet de trigger le click du date end */
            $.fn.onEndDateChosen = function (cb) {
                var t, el;
                $(this).on('show.daterangepicker', function (ev, picker) {
                    let ParentId = $(focusDate)[0].parentElement.parentElement.id;
                    var sd = $('#' + ParentId + ' td.end-date').data('title');
                    t = setInterval(() => {
                        if (sd == $('#' + ParentId + ' td.end-date').data('title')) return;
                        sd = $('#' + ParentId + ' td.end-date').data('title');
                        if (!el || el !== sd) {
                            if (sd != undefined) {
                                el = sd;
                                cb(ev, picker);
                            }
                            return;
                        }
                    }, 10);
                });
                /* A la fermeture du calendrier */
                $(this).on('hide.daterangepicker', function (ev, picker) {

                    let inputEnd = $('[divdescid=' + that.dataInput.DateEndDescId + ']');
                    for (var i = 0; i < inputEnd.length; i++) {
                        inputEnd[i].classList.remove('selectedDate')
                    }

                    let inputStart = $('[divdescid=' + that.dataInput.DateStartDescId + ']');
                    for (var i = 0; i < inputStart.length; i++) {
                        inputStart[i].classList.remove('selectedDate')
                    }
                    clearInterval(t);
                });
            }



            //plus d'info sur le daterangepicker : https://www.daterangepicker.com/
            $(focusDate).daterangepicker({
                parentEl: '#eDate_' + this._uid,
                locale: {
                    format: this.getDateTimeDefautFormat, //this.CultureInfoDate,
                    firstDay: this.cultureMethod == 'en' ? 0 : 1,
                    applyLabel: this.getRes(219),
                    cancelLabel: this.getRes(29),
                    fromLabel: this.getRes(554),
                    toLabel: this.getRes(553),
                    customRangeLabel: this.getRes(6722),
                    daysOfWeek: daysOfWeekTabSubing,
                    monthNames: monthNamestabSubing,
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

                    updateMethod(that, that.dataInput.DisplayValue, undefined, additionalData, that.dataInput);

                    //NHA regression 85002: Nouveau mode fiche]Affichage heure champ date intervalle.
                    if (that.dataInput.DescId == that.dataInput.DateStartDescId) {
                        that.dataInput.DisplayValue = start.format(dbFormat);
                        that.$root.$children.find(f => f.$options.name == 'App').$children.find(fiche => fiche.$options.name == 'fiche')
                            .DataStruct.Structure.LstStructFields
                            .find(f => f.DescId == that.dataInput.DateEndDescId).DateEndValue = end.format(that.CultureInfoDate.endsWith('ss') ? that.CultureInfoDate : that.CultureInfoDate.concat(':ss'));

                        that.$root.$children.find(f => f.$options.name == 'App').$children.find(fiche => fiche.$options.name == 'fiche')
                            .DataStruct.Structure.LstStructFields
                            .find(f => f.DescId == that.dataInput.DateEndDescId).DateStartValue = start.format(that.CultureInfoDate.endsWith('ss') ? that.CultureInfoDate : that.CultureInfoDate.concat(':ss'));
                    }
                    if (that.dataInput.DescId == that.dataInput.DateEndDescId) {
                        that.dataInput.DisplayValue = end.format(dbFormat);

                        that.$root.$children.find(f => f.$options.name == 'App').$children.find(fiche => fiche.$options.name == 'fiche')
                            .DataStruct.Structure.LstStructFields
                            .find(f => f.DescId == that.dataInput.DateStartDescId).DateEndValue = end.format(that.CultureInfoDate.endsWith('ss') ? that.CultureInfoDate : that.CultureInfoDate.concat(':ss'));

                        that.$root.$children.find(f => f.$options.name == 'App').$children.find(fiche => fiche.$options.name == 'fiche')
                            .DataStruct.Structure.LstStructFields
                            .find(f => f.DescId == that.dataInput.DateStartDescId).DateStartValue = start.format(that.CultureInfoDate.endsWith('ss') ? that.CultureInfoDate : that.CultureInfoDate.concat(':ss'));
                    }

                });


            /* callBack du trigger au click sur la date start */
            $(focusDate).onStartDateChosen(function (picker) {

                /* On ajoute une class AUX groupe input de la date start */
                let inputStart = $('[divdescid=' + that.dataInput.DateStartDescId + ']');
                for (var i = 0; i < inputStart.length; i++) {
                    inputStart[i].classList.add('selectedDate')
                }

                /* On enleve une class AUX groupe input de la date end */
                let inputEnd = $('[divdescid=' + that.dataInput.DateEndDescId + ']');
                for (var i = 0; i < inputEnd.length; i++) {
                    inputEnd[i].classList.remove('selectedDate')
                }
            })

            /* CallBack du trigger au click sur la date end */
            $(focusDate).onEndDateChosen(function (picker) {

                /* On enleve une class AUX groupe input de la date start */
                let inputStart = $('[divdescid=' + that.dataInput.DateStartDescId + ']');
                for (var i = 0; i < inputStart.length; i++) {
                    inputStart[i].classList.remove('selectedDate')
                }

                /* On ajoute une class AUX groupe input de la date end */
                let inputEnd = $('[divdescid=' + that.dataInput.DateEndDescId + ']');
                for (var i = 0; i < inputEnd.length; i++) {
                    inputEnd[i].classList.add('selectedDate')
                }

            })

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
                let doc = document.getElementById("mainContentWrap")

                if (position.left + 565 > doc.clientWidth) {
                    picker.container[0].classList.remove('opensright')
                    picker.container[0].classList.add('opensleft')
                    picker.opens = "left";
                    picker.move();
                } else {
                    picker.container[0].classList.remove('opensleft')
                    picker.container[0].classList.add('opensright')
                    picker.opens = "right";
                    picker.move();
                }

                if (position.top - 500 < 0) {
                    picker.drops = "down";
                    picker.move();
                } else {
                    picker.drops = "up";
                    picker.move();
                }

            });
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex", "propResumeEdit"],
    mixins: [eFileComponentsMixin],
    template: `
<div :id="'eDate_' + this._uid" :class="['globalDivComponent', !CultureInfoDate.endsWith('HH:mm') ? 'dateInputWithoutHour' : '']">
    <div ref="date"        
        v-if="!propListe" 
        :class="[focusIn ? 'focusIn': '', IsDisplayReadOnly? 'headReadOnly read-only' : '','ellips input-group hover-input', bEmptyDisplayPopup ? 'display-alert' : '']">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->
        <input 
            v-if="!IsDisplayReadOnly"
            :size="sizeForm(getFormat)" 
            :placeholder="dataInput.Watermark" 
            :field="'field'+dataInput.DescId" 
            :value="getFormat"
            :style="getValueColor"
            @focus="focusIn = true" 
            autocomplete="off" 
            @blur="blurInputValidate($event); focusIn = false"
            @click="openCalendar($event, 'focused')" 
            :id="[getIdInput]"
            :ref="getIdInput"
            type="text" 
            :class="[dataInput.DateStartValue ? 'rangeData' : '', 'form-control input-line fname id_' + dataInput.DescId]"
        >
        <span class="readOnly dateInput text-truncate" v-else>{{getFormat}}</span>
        <span @click="focus($event)" class="input-group-addon">
            <a href="#!" class="hover-pen" >
                <i :class="[
                    (IsDisplayReadOnly && !icon)?'mdi mdi-lock'
                    :(IsDisplayReadOnly && icon)?'fas fa-calendar-plus'
                    :(!IsDisplayReadOnly && icon)?'fas fa-calendar-plus'
                    :'fas fa-pencil-alt']" >
                </i>
            </a>
        </span>

        <!-- Message d'erreur après la saisie dans le champs -->
		        <eAlertBox v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
			        <p>{{getRes(2471)}}</p>
		        </eAlertBox>

    </div>

    <div ref="date" v-if="propListe" :class="[propListe ? 'listRubriqueDate' : '', 'ellips input-group hover-input', focusIn ? 'focusIn' : '', IsDisplayReadOnly ? 'read-only' : '']">

        <!-- Si le champ date est modifiable -->
        <div @click="[!IsDisplayReadOnly ?  openCalendar($event): '']" type="text" :class="[IsDisplayReadOnly ? 'readOnlyDate' : '', 'ellipsDiv form-control input-line fname']">
            <!-- <div @mouseout="icon = false" @mouseover="icon = true" class="targetIsTrue" :style="{ color: dataInput.ValueColor}">{{getFormat}}
            </div> -->

            <input :placeholder="dataInput.Watermark" :field="'field'+dataInput.DescId" 
                :value="getFormat" @focus="focusIn = true" autocomplete="off" 
                @click="openCalendar($event, 'focused')"
                @blur="blurInputValidate($event); focusIn = false"
                :id="[getIdInput]" 
                :ref="getIdInput"
                v-if="!IsDisplayReadOnly"  type="text" :class="'form-control input-line fname id_' + dataInput.DescId">
            <span  @mouseout="icon = false" @mouseover="icon = true" class="readOnly dateInput" v-else>{{getFormat}}</span>

        </div>

        <!-- Icon -->
        <span @click="focus($event);focusIn = true;" class="input-group-addon">
            <a href="#!" class="hover-pen" >
                <i :class="[
                    (IsDisplayReadOnly && !icon)?'mdi mdi-lock'
                    :(IsDisplayReadOnly && icon)?'mdi mdi-lock'
                    :(!IsDisplayReadOnly && icon)?'fas fa-calendar-plus'
                    :'fas fa-pencil-alt']" >
                </i>
            </a>
        </span>
       
    </div>

</div>
`
};