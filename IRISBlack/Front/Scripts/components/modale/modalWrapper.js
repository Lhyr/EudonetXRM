export default {
    name: "modalWrapper",
    data() {
        return {
          aValues:[]
        };
    },
    props: {
        value: {
          type: Boolean,
          default: () => false,
        },
    },
    computed:{
        updateDialog: {
            get() {
              return this.value
            },
            set(value) {
              this.$emit('input', value)
            }
          },
    },
    template: `
      <v-dialog 
      v-model="updateDialog" 
      scrollable
      v-bind="$attrs" 
      class="modal__wrapper">
          <template v-slot:activator="{ on, attrs }">
            <div 
            v-bind="attrs" 
            v-on="on"
            >
              <slot name="activator" 
              v-bind="attrs"
              v-on="on">
              </slot>
            </div>
          </template>
          <slot name="content" ></slot>
      </v-dialog>
    `,
};