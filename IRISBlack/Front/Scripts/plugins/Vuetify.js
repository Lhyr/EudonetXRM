Vue.use(Vuetify);

let oCptStyle = getComputedStyle(document.documentElement);

const opts = {
    icons: {
        iconfont: "mdi"
    },
    theme: {
        options: {
            customProperties: true
        },
        themes: {
            light: {
                primary: oCptStyle?.getPropertyValue('--main-color').trim() || "#bb1515",
                primary93: oCptStyle?.getPropertyValue('--main-color-alpha-93').trim() || "#c02525",
                primary34: oCptStyle?.getPropertyValue('--main-color-alpha-34').trim() || "#bac7ce",
                primary96: oCptStyle?.getPropertyValue('--main-color-alpha-96').trim() || "#be1e1e",
                secondary: oCptStyle?.getPropertyValue('--clair_gris_color-red').trim() || "#757575",
                accent: oCptStyle?.getPropertyValue('--head-mru').trim() || "#82B1FF",
                border: oCptStyle?.getPropertyValue('--border-color').trim() || "#9b0e0e",
                error: oCptStyle?.getPropertyValue('--danger-color').trim() || "#FF5252",
                info: oCptStyle?.getPropertyValue('--info-color').trim() || "#2196F3",
                success: oCptStyle?.getPropertyValue('--success-color').trim() || "#4CAF50",
                warning: oCptStyle?.getPropertyValue('--warning-color').trim() || "#FFC107",
                background: oCptStyle?.getPropertyValue("--background-main-color").trim() || "#f5f5f5",
            }
        }
    },
};

export default new Vuetify(opts)