
var watcher = function (doc) {

    that = this;


    //scope surveillé
    var $document = doc;


    this.$Object = {};

    //liste des watcher
    this.watchers = [];



    // Prototype - watcher / digest
    this.$GetWatched = function (sObjName) {

        if (this.$Object.hasOwnProperty(sObjName)) {
            var t = this.$Object[sObjName];
            t.IsSet = t.GetValue() != null;
            return t;
        }
        else {
            var t =
            {
                IsSet: false,
                GetValue: () => { return null; },
                SetValue: () => { }
            };

            return t;
        }
    }

    //ajoute une variable "systeme" à surveiller. Le setter doit explicitement appeller digest. voir getTokenValue/SetToken
    this.$watch = function (myValueGetter, myListener) {

        this.watchers.push(
            {
                getWatchExp: myValueGetter,
                getWatchListner: myListener || function () { }
            }
        );
    };

    //Surveille un input. Ajout un onchange
    this.$watchInput = function (sIdInput, myListener) {
        that = this;


        
        var myInput = $document.getElementById(sIdInput);
        //var myJsonInput = [].slice.call(myInput.attributes).reduce((seed, ee) => { seed[ee.name] = ee.value; return seed }, {  } );

        var myJsonInput = [].slice.call(myInput.attributes).reduce( function(seed, ee){ seed[ee.name] = ee.value; return seed }, {  });



        var myGet = function () {
            return myInput.value;
        }

        var mySet = function (value) {
            myInput.value = value;
            that.$digest();
        }

        this.$Inputs[sIdInput] = {
            GetValue: myGet,
            SetValue: mySet
        }

        var elem = document.getElementById(sIdInput);
        elem.addEventListener("change", function () { that.$digest(); });

        this.$watch(myGet, myListener);

    }


    //Observe un objet arbitraire ()
    this.$watchObj = function (sObjName, sObj, myListener) {

        this.$Object[sObjName] = this.$GetObjectWrap(sObj);

        this.$watch(this.$Object[sObjName] , myListener);
    }


    //retourne un "wrapper" get/set pour un objet "watché"
    this.$GetObjectWrap = function (initval) {

        var _privateVal = initval;


        return {

            IsSet: _privateVal != null,

            GetValue: function () {
                return _privateVal;
            },

            SetValue: function (value) {
                _privateVal = value;
                that.$digest();
            }
        }
    }
     


    //Fonction de parcour pour vérifier les objets ayant changé
    //et appel de leur watcher
    this.$digest = function () {
        var dirty;

        do {

            dirty = false;

            //on parcours tous nons watcher
            for (var i = 0; i < this.watchers.length; i++) {


                var newVal = this.watchers[i].getWatchExp();
                var oldVal = this.watchers[i].oldValue;

                // comparaison via stringfy pour les objects
                if (JSON.stringify(newVal) !== JSON.stringify(oldVal)) {
                    //une valeur a changé !

                    //On prévient le listener
                    this.watchers[i].getWatchListner(newVal, oldVal);

                    //On met à jour l'ancienne valeur
                    this.watchers[i].oldValue = newVal;

                    // on va reboucler pour vérifier les cascades						
                    dirty = true;
                }
            }
        } while (dirty);
    };


}