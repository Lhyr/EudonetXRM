import { eExpressFilterMixin } from '../../mixins/eExpressFilterMixin.js?ver=803000';

export default {
    name: "expressFilterDate",
    data() {
        return {
            top: 0,
            choiceRange: 0,
            DisplayFormatFilter: 'DD/MM/YYYY HH:mm:ss',
            that: this,
            componentDate: Object
        };
    },
    mixins: [eExpressFilterMixin],
    template: `
<ul :format="this.propData.datas.Format" v-bind:style="{ top: this.top, left: this.propData.posLeft + 'px' }" ref="filterContent" class="filterContent">
    <li class="data-button-header header-date">
        <ul class="xprss-filter__list">
            <li class="xprss-filter__li" @click="choiceRange = 0">
                <div class='input-group chooseDateParent eChooseDate chooseDateParentComponent' id='chooseDate'>
                    <input style="display:none" type='text' class="form-control" />
                    <span class="input-group-addon">
                        {{getRes(5017)}}
                    </span>
                </div>
            </li>
            <li class="xprss-filter__li" @click="choiceRange = 1">
                <div class='input-group chooseDateParent eBeforeDate chooseDateParentComponent' id='beforeDate'>
                    <input style="display:none" type='text' class="form-control" />
                    <span class="input-group-addon">
                        {{getRes(5015)}}
                    </span>
                </div>
            </li>
            <li class="xprss-filter__li" @click="choiceRange = 3">
                <div class='input-group chooseDateParent eAfterDate chooseDateParentComponent' id='afterDate'>
                    <input style="display:none" type='text' class="form-control" />
                    <span class="input-group-addon">
                        {{getRes(5016)}}
                    </span>
                </div>
            </li>
            <li class="xprss-filter__li" @click="choiceRange = 97">
                <div class='input-group chooseDateParentBetween eBetweenDate chooseDateParentComponent' id='betweenDate'>
                    <input style="display:none" type='text' class="form-control" />
                    <span class="input-group-addon">
                        {{getRes(6675)}}
                    </span>
                </div>
            </li>
        </ul>
    </li>
    <li v-if="this.DataMRU.length > 0" class="data-mru">
        <ul class="xprss-filter__list"> 
            <li class="xprss-filter__li actionItem" :title="dtMru.Value" v-for="dtMru in this.DataMRU" @click="searchFromValue(dtMru.Value)">
                <span>{{dtMru.Value}}</span>
            </li>
        </ul>
    </li>
    <expressFilterFooter/>
</ul>
`,
    beforeDestroy() {
        window.removeEventListener('resize', this.closeDateFilter);
    },
    mounted() {
        let that = this;
        let InfoDate = document.getElementById('eParam').contentWindow.document.getElementById('CultureInfoDate');
        //Tache 2 596 : laisser DisplayFormatFilter = 'DD/MM/YYYY' si non bug au niveau du controleur ePrefManager.ashx
        //Échec de la conversion de la date et/ou de l'heure à partir d'une chaîne de caractères.
        //if (that.propData.datas.DisplayFormat && that.propData.datas.DisplayFormat != "") {
        //    that.DisplayFormatFilter = DisplayFormatFilter
        //} else {
        //    that.DisplayFormatFilter = InfoDate.value.toUpperCase()
        //}

        let startDate = moment().set("hour", 0).set("minute", 0).format(that.DisplayFormatFilter);
        // Initialisation des dates
        for (var i = 0; i < $('.chooseDateParentComponent').length; i++) {
            let id = '#' + $('.chooseDateParentComponent')[i].getAttribute('id')
            $($(id)).daterangepicker({
                "singleDatePicker": id != "#betweenDate",
                "showWeekNumbers": true,
                "timePicker24Hour": true,
                "opens": "right",
                "startDate":startDate,
                "showDropdowns": true,
                "timePicker": true,
                "parentEl": ".header-date",
                "separator": " - ",
                "locale": {
                    "format": that.DisplayFormatFilter,
                    "applyLabel": top._res_219, // Appliquer
                    "cancelLabel": top._res_30, // Fermer
                    "fromLabel": top._res_554, // de
                    "toLabel": top._res_553, // à
                    "weekLabel": top._res_821.substring(0, 1), // S ([S]emaine)
                    "daysOfWeek": [
                        top._res_44.substring(0, 2), // [Di]manche
                        top._res_45.substring(0, 2), // [Lu]undi
                        top._res_46.substring(0, 2), // [Ma]rdi
                        top._res_47.substring(0, 2), // [Me]rcredi
                        top._res_48.substring(0, 2), // [Je]udi
                        top._res_49.substring(0, 2), // [Ve]ndredi
                        top._res_50.substring(0, 2), // [Sa]medi
                    ],
                    "monthNames": [
                        top._res_32, // Janvier
                        top._res_33, // Février
                        top._res_34, // Mars
                        top._res_35, // Avril
                        top._res_36, // Mai
                        top._res_37, // Juin
                        top._res_38, // Juillet
                        top._res_39, // Août
                        top._res_40, // Septembre
                        top._res_41, // Octobre
                        top._res_42, // Novembre
                        top._res_43 // Décembre
                    ],
                    "firstDay": 1
                }
            });

            $(id).on('apply.daterangepicker', function (ev, picker) {
                if (id == "#betweenDate") {
                    let startDate = picker.startDate.format(that.DisplayFormatFilter);
                    let endDate = picker.endDate.format(that.DisplayFormatFilter);
                    let newDate = startDate + '$B#W$' + endDate;
                    that.updatePref(that.choiceRange + ";|;" + newDate);
                } else {
                    let newDate = picker.startDate.format(that.DisplayFormatFilter);                    
                    that.updatePref(that.choiceRange + ";|;" + newDate);
                }
            });

            $(id).data('daterangepicker').container.addClass('chooseDate');
            $(id).data('daterangepicker').container[0].style.display = "none";
            $(id).data('daterangepicker').setStartDate(startDate);

            // Calcule des positions de chaque composent à l'ouverture de celui ci
            $(id).on("show.daterangepicker", function (e) {
                that.componentDate = $(id).data('daterangepicker');
                let widthVisibleFilter = window.innerWidth - 275;
                let widthTab = that.$parent.$parent.$refs.tabContent.clientWidth;
                let diff = widthTab - that.propData.posLeft;
                let dateTypeWidth = $(id).data('daterangepicker').container[0].clientWidth + 13

                //if (widthVisibleFilter < (dateTypeWidth + widthTab) - diff + 260) {
                //    if (this.id != "betweenDate") {
                //        $(id).data('daterangepicker').container.css({ "top": that.top, "left": that.propData.posLeft + 'px', "right": 'auto', "margin": '-15px 0px 0px -246px' })
                //    } else {
                //        $(id).data('daterangepicker').container.css({ "top": that.top, "left": that.propData.posLeft + 'px', "right": 'auto', "margin": '-15px 0px 0px -578px' })
                //    }
                //} else {
                //    $(id).data('daterangepicker').container.css({ "top": that.top, "left": that.propData.posLeft + 'px', "right": 'auto' })
                //}

                let left = 0;
                let dateMargin = 15 // marge du date picker
                let filterContainer = that.$refs.filterContent; // la liste des filtres
                let dateContainer = $(id).data('daterangepicker').container[0]; // le datepicker
                let rightAreaWidth = window.innerWidth - (filterContainer.offsetLeft + filterContainer.offsetWidth); // la place que l'on à droite de la liste des filtres

                // on vérifie que le tableau ne sera pas trop bas si on le place au niveau de la liste déroulante du filtre. Pour cela, on compare la position + la hauteur du calendier pour voir si elle est supérieur à la hauteur de la fenêtre
                if (filterContainer.offsetTop + dateContainer.offsetHeight > window.innerHeight) {
                    that.top = filterContainer.offsetTop - ((filterContainer.offsetTop + dateContainer.offsetHeight + dateMargin) - window.innerHeight);
                }

                //Si on a pas la place à gauche mais que l'on a la place à droite, on le met à la même position X que la liste déroulante + 15px de marge
                if (filterContainer.offsetLeft < dateContainer.offsetWidth
                    && rightAreaWidth > dateContainer.offsetWidth) {
                    left = filterContainer.offsetLeft + filterContainer.offsetWidth + dateMargin
                //Si on a la place à gauche , on le met à la même position X que la liste déroulante + 15px de marge
                } else if (filterContainer.offsetLeft > dateContainer.offsetWidth) {
                    // position du filtre moins la largeur du datepicker, pour qu'il soit à côté
                    left = (filterContainer.offsetLeft - dateContainer.offsetWidth) - dateMargin;
                //si on n'a ni la place à gauche et à droite, on le met au-dessus et centré au milieu. 
                } else {
                    left = dateContainer.offsetWidth / 2;
                    that.top = dateMargin; // normalement à 0 mais on rajoute la marge afin d'éviter d'avoir le datepicker collé au bhaut de la page
                    dateContainer.classList.add('date-above'); // on rajoute 1 classe CSS pour réduite sa hauteur et ne pas masquer la liste des filtres.
                }
                $(id).data('daterangepicker').container.css({ "top": that.top, "left": left + 'px', "margin": '0' });


    

            });

            // Re calcule des positions de chaque composent à chaque fois qu'il y a une modification qui impact le front
            $(id).on("showCalendar.daterangepicker", function (e) {
                $(id).data('daterangepicker').container.css({ "top": that.top, "left": that.propData.posLeft + 'px', "right": 'auto' })
            });
        }
        window.addEventListener('resize', this.closeDateFilter);

    },
    methods: {
        closeDateFilter() {
            if (this.componentDate) {
                this.componentDate.hide()
            }
        }
    }
};