/** tableau des formats interdits. */
const tabFormatForbid = [15, 16, 17, 20, 21, 22];
const tabFormatForbidHeadEdit = [11, 15, 16, 17, 20, 21, 22];
const tabFormatForbidLabel = [11, 15, 16, 18];
const tabFormatBtnLbl = [11, 10, 15, 18];
const tabFormatBtnSep = [11, 15, 16];

// Champs avec des composants Vuetify / Eudofront
const vuetifyInput = [11, 18];

// popup zone assisant et resume, titres et sous-titres
const tabFormatForbidPopup = [15, 16, 17, 20, 21, 22];


/* #79 128/#79 756 - Pour le calcul de la hauteur des champs sur plusieurs lignes */
const DEFAULT_FIELD_HEIGHT = 58;
const DEFAULT_FIELD_MARGIN = 34; // 6 (padding champs) + 18 (label) + 10 (padding-top image);

const summaryFldsMaxNb = 6;

const wizardFldsMaxNb = 9;

const relationFormats = [9, 5]

export { tabFormatForbidLabel, tabFormatForbid, tabFormatForbidHeadEdit, tabFormatForbidPopup, tabFormatBtnLbl, tabFormatBtnSep, DEFAULT_FIELD_HEIGHT, DEFAULT_FIELD_MARGIN, summaryFldsMaxNb, relationFormats, wizardFldsMaxNb, vuetifyInput };