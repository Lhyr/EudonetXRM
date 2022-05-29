/****************************************************************************/
/* Javascript pour la gestion des désinscriptions sur edn.aspx              */
/****************************************************************************/

var nsUnsub = nsUnsub || {};

nsUnsub.ToggleUnsubChoiceChckBx = function (sender) {
    if (sender != null && sender.checked) {

        var disableAllValue = false;
        if (sender.value == "all")
            disableAllValue = false;
        else
            disableAllValue = true;

        var chckbxArr = document.querySelectorAll("input[name='UnsubChoiceChckBx']");
        for (var i = 0; i < chckbxArr.length; ++i) {
            var chckbx = chckbxArr[i];
            if (chckbx != null && (!disableAllValue && chckbx.value != "all") || (disableAllValue && chckbx.value == "all"))
                chckbx.checked = false;
        }
    }
}
