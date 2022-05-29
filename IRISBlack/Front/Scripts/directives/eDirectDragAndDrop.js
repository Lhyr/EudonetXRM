export default {
    bind: function (el, binding, vnode) {
        el.setAttribute('draggable', 'true');
        /*
         *   drag – a dragged item is dragged
         *   dragstart – we start dragging a draggable element
         *   dragend – a drag ends (e.g. we let go of the mouse)
         *   dragenter – wen a dragged item enters a droppable element
         *   dragleave – a dragged item leaves a droppable element
         *   dragover – a dragged item is moved over a droppable element (calls every hundred milliseconds or so)
         *   drop – a dragged item is dropped on a droppable element
         */
        el.handleDragStart = evt => {
            evt.dataTransfer.dropEffect = 'move';
            evt.dataTransfer.effectAllowed = 'move';
            // Need to set to something or else drag doesn't start
            evt.dataTransfer.setData('dragTarget', evt.target.innerText);
        };

        el.handleDragEnter = evt => {
            // update css only draged element enter into the li
            if (evt.target.matches("li")) {
                evt.target.style.borderLeft = '2px dashed #bb1515';
            }
        };

        el.handleDragLeave = evt => {
            // remove the css when draged element leave the element
            if (evt.target.matches("li") && !evt.currentTarget.contains(evt.relatedTarget)) {
                evt.target.style.borderLeft = '';
            }
        };

        el.handleDrop = evt => {
            evt.preventDefault();
            if (evt.stopPropagation) {
                // stops the browser from redirecting.
                evt.stopPropagation();
            }
            // remove the css when drag & drop event finish, it is possible to drop the draged element on the li or a element
            if (evt.target.matches("li")) {
                evt.target.style.borderLeft = '';
            } else {
                evt.target.parentNode.style.borderLeft = '';
            }
            if (evt.target.parentNode.parentNode.classList.contains("dropzone") || evt.target.parentNode.classList.contains("dropzone")) {
                let source = vnode.context[binding.value];
                let dragId = source.findIndex(x => x.Label == evt.dataTransfer.getData('dragTarget'));
                let dropId = source.findIndex(x => x.Label == evt.target.innerText);
                source.splice(dropId, 0, source.splice(dragId, 1)[0]);

                const event = new CustomEvent('dropEvent', {
                    detail: {},
                    bubbles: true
                });
                el.dispatchEvent(event);
            }

            return false;
        };

        el.addEventListener("dragstart", el.handleDragStart);

        el.addEventListener("dragenter", el.handleDragEnter);

        el.addEventListener("dragleave", el.handleDragLeave);

        el.addEventListener("drop", el.handleDrop);

    },

    unbind: function (el) {
        el.removeAttribute('draggable');
        el.removeEventListener('dragstart', el.handleDragStart);
        el.removeEventListener('dragenter', el.handleDragEnter);
        el.removeEventListener('dragleave', el.handleDragLeave);
        el.removeEventListener('drop', el.handleDrop);
    }
}