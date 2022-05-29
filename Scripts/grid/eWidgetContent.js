var eWidgetContent = {};

eWidgetContent.init = function (nWidgetType, nWid, nTab, sContext) {

    var widgetWrapper = parent.document.getElementById("widget-wrapper-" + nWid);

    if (widgetWrapper) {
        var width = parseInt(widgetWrapper.style.width);
        var height = parseInt(widgetWrapper.style.height);

        if (nWidgetType == top.XrmWidgetType.Kanban) {
            if (eWidgetKanban) {
                oWidgetKanban = new eWidgetKanban(nWid, nTab, sContext);
                oWidgetKanban.init();
            }
        }
        else if (nWidgetType == top.XrmWidgetType.Editor) {
            setEventListener(document.getElementById("formWidgetContent"), "click", function (event) {
                var target = event.target;
                if (target.tagName == "A") {
                    event.preventDefault();
                    if (target.href)
                        window.open(target.href, "_blank");
                }
            });
        }
        else if (nWidgetType == top.XrmWidgetType.List) {

            if (eListWidget) {

                oListWidget = new eListWidget(nWid, nTab, sContext);
                oListWidget.init();
                oListWidget.resizeList(width, height);
            }
        }

        if (nWidgetType == top.XrmWidgetType.RSS || nWidgetType == top.XrmWidgetType.Editor) {

            var form = document.querySelector("form");
            form.style.width = (width - 10) + "px";
            form.style.height = (height - 10) + "px";
            form.style.overflow = "auto";

        }
    }

    eTools.setWidgetWait(nWid, false);
}