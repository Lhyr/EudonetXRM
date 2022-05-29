/**
 * constante tableau qui indique, suivant le type d'élément 
 * le message a afficher en tooltip...
 * */
const tabAstuces = {
    "mailaddress": top._res_2503,
    "phone": top._res_2654,
    "sms": top._res_2504,
    "socialnetwork": top._res_2505,
    "hyperLink": top._res_2505,
    "relation": top._res_2506,
    "aliasrelation": top._res_2506,
}


const tabParentForbidden = ["headfiche"]

const stepBarStatus = {
    BackToStep: 0,
    ValidateStep: 1,
    UnknownStep: 2,
    GoToStep: 3
}

export { tabAstuces, tabParentForbidden, stepBarStatus };