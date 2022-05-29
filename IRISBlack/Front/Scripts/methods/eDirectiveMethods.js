let handleOutsideClick
Vue.directive('closable', {
    bind(el, binding, vnode) {
        handleOutsideClick = (e) => {
            e.stopPropagation()
            const { handler } = binding.value
            if (!el.contains(e.target)) {
                vnode.context[handler]()
            }
        }
        document.addEventListener('mousedown', handleOutsideClick)
        document.addEventListener('touchstart', handleOutsideClick)
    },

    unbind() {
        document.removeEventListener('mousedown', handleOutsideClick)
        document.removeEventListener('touchstart', handleOutsideClick)
    }
})
export default {
    name: "eDirectiveMethods",
};
