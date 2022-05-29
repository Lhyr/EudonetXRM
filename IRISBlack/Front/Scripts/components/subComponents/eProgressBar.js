import { eModalComponentsMixin } from '../../mixins/eModalComponentsMixin.js?ver=803000';

export default {
    name: "eProgressBar",
    data() {
        return {
            startDateUplaod: 0,           
            webImageSize: 158298, // taille de l'image sur le web
            fileSizeMoFront: 0,
            timeTotal: 0,
            newTime: 0,
            vitesseFront: 0,
            totalDownload: 0,
            ext: '',
            getTime : '',
            progress: '',
        };
    },
    components: {
        eIcon: () => import("./eIcon.js")
    },
    mounted() {
        let webImage = document.location.origin + '/' + document.location.pathname.split('/')[1] + '/' + "IRISBlack/Front/Assets/Imgs/default.jpg";  
        // set la date de debut de load du fichier
        this.startDateUplaod = new Date();
        this.startDateUplaod = this.startDateUplaod.getTime();

        // set une image sur le web pour savoir quand elle est upload
        var img = new Image();
        img.src = webImage + '?' + this.startDateUplaod;

        // Une fois upload, on lance la fonction de load (this.end_test_vitesse)
        img.onload = this.end_test_vitesse;
    },
    beforeDestroy: function () {
        clearTimeout(this.getTime) // On clear le timeOut
        clearTimeout(this.progress); // On clear le timeOut
    },
    methods: {
        // Une fois le telechargement de l'image (du web) upload
        end_test_vitesse() {
            // Date de fin d'upload
            let endDateUplaod = new Date();
            endDateUplaod = endDateUplaod.getTime();

            // Vitesse de telechargement de l'utilisateur
            let ms = endDateUplaod - this.startDateUplaod;
            let vitesse = Math.round(this.webImageSize / ms) / 100;
            this.vitesseFront = Math.round(this.webImageSize / ms) / 100;

            let flSize = this.file
                .map(fl => fl.size)
                .reduce((accumulator, currentValue) => accumulator + currentValue);

            // Temps total pour telecharger l'image
            let fileSizeMo = flSize / 1000000;


            // Si le fichier est < 1 Mo on passe en Ko
            if (Math.round(flSize) / 1000000 < 1) {
                this.fileSizeMoFront = Math.round(flSize / 1000);
                this.ext = this.getRes(65);
            } else {
                this.fileSizeMoFront = Math.round(flSize / 1000000);
                this.ext = this.getRes(66)
            }


            let timeTotal = Math.round(fileSizeMo / vitesse) > 1 ? Math.round(fileSizeMo / vitesse) : 1;
            this.timeTotalFront = timeTotal;

            // On créer un interval sur 100 pour calculer 1% = cb de temps ?
            let intervalForOnePerc = (timeTotal * 1000) / 100;

            // On lance la function qui permet de faire progresser la progress bar, on demarre a 0 puis la function incrémentera toute les "intervalForOnePerc"
            this.SetUpdateDownloading();
            this.progressBarSim(0, intervalForOnePerc);

        },

        /**
         * /
         * @param {any} increment // la progress bar de 0 à 100
         * @param {any} intervalForOnePerc // interval de temps calculer en fonction de la taille du fichier
         */
        progressBarSim(increment, intervalForOnePerc) {
            let that = this;

            let bar = this.$refs.progressBarInner;
            let status = this.$refs.status;

            // On incrémente le status de la bar (0% to 100%)
            status.innerHTML = increment + "%";
            // On incrémente la largeur de la bar (0% to 100%)
            bar.style.width = increment + "%";

            // Si l'incrément arrive a 100
            if (increment == 100) {
                status.innerHTML = "100%"; // On set le status a 100
                bar.style.width = "100%"; // On set la largeur de la bar a 100
                clearTimeout(this.progress); // On clear le timeOut

                // Envoie au composent mère une réponse pour indiquer que le telechargement est fini
                setTimeout(function () {
                    that.$emit('callBackfinishLoad');
                }, 1000);

                return;
            } else {
                // Sinon on incrément de 1 l'incrément
                increment++;

                // Et on relance la function avec le nouvelle incrément
                this.progress = setTimeout(function () {
                    that.progressBarSim(increment, intervalForOnePerc)
                }, intervalForOnePerc);
            }

        },
        SetUpdateDownloading() {
            let that = this;

            // On incrément la partie télécharger
            if (this.ext ==  this.getRes(65)) {
                this.totalDownload = Math.round(this.fileSizeMoFront)
            } else {
                if (this.totalDownload >= this.fileSizeMoFront) {
                    this.totalDownload = Math.round(this.fileSizeMoFront)
                } else {
                    this.totalDownload = this.vitesseFront > this.fileSizeMoFront ? this.fileSizeMoFront : Math.round(this.totalDownload + this.vitesseFront)
                }
            }
            
            if (this.timeTotalFront > 0) {
                this.newTime = that.formatTime(that.timeTotalFront)
                that.timeTotalFront = that.timeTotalFront - 1
                this.getTime = setTimeout(function () {
                    that.SetUpdateDownloading(that.timeTotalFront)
                }, 1000);

            } else {
                this.newTime = 0 + "s"
                clearTimeout(this.getTime) // On clear le timeOut
                return
            }
        },
        formatTime(time) {
            // Heures, minutes et secondes
            var hrs = ~~(time / 3600);
            var mins = ~~((time % 3600) / 60);
            var secs = ~~time % 60;

            // sorite -> "1m 01s" or "4h 03m 59s" or "123h 03m 59s"
            var ret = "";
            if (hrs > 0) {
                ret += "" + hrs + "h " + (mins < 10 ? "0" : "");
            } if (mins > 0) {
                ret += "" + mins + "m " + (secs < 10 ? "0" : "");
            }
            ret += "" + secs + "s";
            return ret;
        }

    },
    props: ["file"],
    mixins: [eModalComponentsMixin],
    template: `
    <div class="container_progress">
        <div class="ico_download_progress">
            <span><eIcon>fas fa-file-upload</eIcon></span>
        </div>
        <div class="content_progress">
            <span class="progressName">{{file.name}}</span>
            <div class="progress_loader">
                <div ref="progressBarInner" style="width='0%'" class="progress_bar" id="progressBar" value="0" max="100"></div>
                <span ref="status" id="status"></span>
            </div>
            <span class="progressSize">{{totalDownload}} / {{fileSizeMoFront}} {{ext}}</span>
            <span class="progressTime">{{newTime}}</span>
        </div>
    </div>
`
};