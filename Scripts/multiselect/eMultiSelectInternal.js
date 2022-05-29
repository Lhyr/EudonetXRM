
/// Objet permettant de gérer la selection de plusieurs valeurs 
/// à l'interieur de l'iframe eMultiSelect.aspx
var oMultiSelectInternal = (function () {
    
    function selectedItems()
    {
        var items = [];
        var ids = "";
        var targetContainer = document.getElementById("TabSelectedList");
        if (targetContainer)
        {
            var elements = [].slice.call(targetContainer.querySelectorAll("div[item]"));            
            for (var i = 0; i < elements.length; i++)
            {
                ids += getAttributeValue(elements[i], "item") + ";"
                items.push({
                    'id': getAttributeValue(elements[i], "item"),
                    'title': GetText(elements[i])
                });
            }
        }

        return { 'items': items, 'ids': ids };
    }

    // fonctions publiques
    return {
        GetSelectedItems: function () { return selectedItems();}      
    };
}());