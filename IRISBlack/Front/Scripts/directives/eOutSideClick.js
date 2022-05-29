Vue.directive('click-outside-iris', {
    bind: function (el, binding, vNode) {
        const handler = (e) => {
            e.stopPropagation();

            if (!el.contains(e.target) && el !== e.target) {
                vNode.context[binding.expression] = false
            }
        }
        el.out = handler
        document.addEventListener('click', handler)
    },
    unbind: function (el, binding) {
        document.removeEventListener('click', el.out)
        el.out = null
    }
});