

const tabUrl = {
    "bing": {
        url: "https://www.bing.com/api/v6/Places/AutoSuggest",
        params: {
            q: "",
            appid: "F2DD9E3AA45F7512D9C6CA9A150CBA7F76556B81",
            setlang: "fr-FR",
            setmkt: "fr-FR",
            types: "place,address",
            structuredaddress: true,
            count: "10",
            clientid: "36B5745F03F1621511B278AA07F1614A"
        }
    },
    "BingLocation": {
        url: "https://dev.virtualearth.net/REST/v1/Locations",
        params: {
            q: "",
            o: "json",
            culture: 'fr-FR',
            key: "Aia9V-TFKUb44CNZsVp_oxYGgszFUgksJal8-_IW1SSbodepQ4didGSMVp4UiSwR",
            maxResults: "1"
        }
    },
    "sirene": {
        url: "",
        params: {

        }
    },
    "datagouv": {
        url: "https://api-adresse.data.gouv.fr/search/",
        params: {
            limit: 15,
            q: ""
        }
    }
};

class eAutoCompletionHelper {
    /**
     * 
     * @param {any} eAxiosHelper
     * @param {any} e
     * @param {any} params
     * @param {any} provider
     * @param {any} dataInput
     * @param {any} address
     */
    constructor(eAxiosHelper, e, params, provider, dataInput, address) {
        this.address = address ? address : '';
        this.typeProvider = provider;
        this.eAxios = eAxiosHelper;
        this.event = e;
        this.params = params;
        this.dataInput = dataInput;

        if (this.event.target.value == "")
            return;

        this.objAutoCompletion = [];
    }


    async loadAutoCompletion() {
        try {
            this.json = await this.eAxios.GetAsync({
                params: this.params,
                responseType: 'json'
            });
        } catch (e) {
            throw e;
        }
    }

    retrieveJson() {
        if (!this.json)
            return;

        switch (this.typeProvider) {
            case "bing": return this.bingJson();
            case "BingLocation": return this.bingLocationJson();
            case "sirene": return this.sireneJson();
            case "datagouv": return this.datagouvJson();
            default: return;
        }
    }

    bingLocationJson() {
        return {
            streetName: this.address.streetName ? this.address.streetName : '',
            postalCode: this.address.postalCode ? this.address.postalCode : '',
            city: this.address.city ? this.address.city : '',
            label: this.address.label ? this.address.label : '',
            region: this.address.region ? this.address.region : '',
            country: this.address.country ? this.address.country : '',
            geography: this.json.resourceSets[0].resources[0].geocodePoints[0] ? 'POINT(' + this.json.resourceSets[0].resources[0].geocodePoints[0].coordinates[1] + ' ' + this.json.resourceSets[0].resources[0].geocodePoints[0].coordinates[0] + ')' : ''
        };
    }

    bingJson() {
        if (!this.json.value)
            return;
        return this.json.value.map(a => {
            if (a._type == "Place") {
                return {
                    event: this.event,
                    _type: a._type ? a._type : '',
                    city: a.address.addressLocality ? a.address.addressLocality : '',
                    addressSubregion: a.address.addressSubregion ? a.address.addressSubregion : '',
                    region: a.address.addressRegion ? a.address.addressRegion : '',
                    country: a.address.addressCountry ? a.address.addressCountry : '',
                    countryIso: a.address.countryIso ? a.address.countryIso : '',
                    label: a.address.text ? a.address.text : ''
                };
            } else {
                return {
                    event: this.event,
                    _type: a._type ? a._type : '',
                    streetName: a.streetAddress ? a.streetAddress : '',
                    city: a.addressLocality ? a.addressLocality : '',
                    addressSubregion: a.addressSubregion ? a.addressSubregion : '',
                    region: a.addressRegion ? a.addressRegion : '',
                    postalCode: a.postalCode ? a.postalCode : '',
                    country: a.addressCountry ? a.addressCountry : '',
                    countryIso: a.countryIso ? a.countryIso : '',
                    label: a.text ? a.text : '',
                    formattingRuleId: a.formattingRuleId ? a.formattingRuleId : ''
                };
            }

        });
    }

    sireneJson() {
        if (!this.json.ResultData)
            return;
        return this.json.ResultData.Rows.map(a => {

            return {
                event: this.event, ...a
            };
        });
    }
    GetCountry(departmentNumber) {
        switch (departmentNumber) {
            case "971": return "Guadeloupe";
            case "972": return "Martinique";
            case "973": return "Guyane française";
            case "974": return "Réunion";
            case "975": return "Saint-Pierre-et-Miquelon";
            case "976": return "Mayotte";
            case "977": return "Saint-Barthélemy";
            case "978": return "Saint-Martin";
            case "984": return "Terres australes et antarctiques françaises";
            case "986": return "Wallis-et-Futuna";
            case "987": return "Polynésie française";
            case "988": return "Nouvelle-Calédonie";
            case "989": return "Clipperton"; // oui, même s'il n'y a plus personne là-bas depuis 1917 - https://fr.wikipedia.org/wiki/Oubli%C3%A9s_de_Clipperton :)
            default: return "France";
        }
    }

    datagouvJson() {
        if (!this.json.features)
            return;
        return this.json.features.map(a => {
            let infos = [];
            let region = "";
            let department = "";
            let departmentNumber = "";

            if (a.properties.context != "") {
                infos = a.properties.context.split(',');
                if (infos.length == 3) {
                    region = infos[2].trim();
                }
                if (infos.length >= 2) {
                    department = infos[1].trim();
                }
                if (infos.length >= 1) {
                    departmentNumber = infos[0].trim();
                }
            }
            return {
                event: this.event,
                streetName: a.properties.name ? a.properties.name : '',
                postalCode: a.properties.postcode ? a.properties.postcode :'',
                city: a.properties.city ? a.properties.city : '',
                label: a.properties.label ? a.properties.label : '',
                region: region ? region : '',
                department: department ? department : '',
                departmentNumber: departmentNumber ? departmentNumber : '',
                country: departmentNumber ? this.GetCountry(departmentNumber) : '',
                cityCode: a.properties.citycode ? a.properties.citycode : '',
                houseNumber: a.properties.housenumber ? a.properties.housenumber : '',
                geography: a.geometry ? 'POINT(' + a.geometry.coordinates[0] + ' ' + a.geometry.coordinates[1] + ')' : ''
            }
        })
    }
}

export { eAutoCompletionHelper, tabUrl };