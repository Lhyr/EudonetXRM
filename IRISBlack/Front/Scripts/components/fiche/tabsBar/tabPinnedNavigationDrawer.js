import { FieldType } from '../../../methods/Enum.min.js?ver=803000';
import { dynamicFormatChamps } from '../../../../index.js?ver=803000';

export default {
    name: "tabPinnedNavigationDrawer",
    data() {
        return {
            idxBookmark: 0,
            FieldType: FieldType,
            bDisplayLabel: false,
            rightMenuWidth: null,
            memoOpenedFullModeObj: {
                value: false,
                index: null
            }
        };
    },
    components: {
    },
    computed: {
    },
    methods: {
        dynamicFormatChamps
    },
    props: {
        propTabs: {
            type: Array,
            default: []
        }
    },
    template: `
    <div class="pinned-tabs-custom">
        <ul
            id="pinned-tabs-ul"
            ref="pinnedTabsUl"
            class="nav nav-tabs d-flex"
        >
            <li
                v-for="(tab,index) in propTabs"
                :key="tab.DescId"
                :class="{'active': index===idxBookmark,'btnDrawer': true,'pinnedFileBtnDrawer':true}"
            >
                <a
                    class="pinned-tab--btn text--primary"
                    :aria-expanded="index===idxBookmark"
                    data-toggle="tab"
                    :href="'#' + tab.DescId + '_id' "
                >
                    {{tab.Label}}
                </a>
            </li>
        </ul>
        <div class="pinned-tab-content not-draggable">
            <div
                v-for="(input,index) in propTabs"
                :key="input.id"
                :divdescid="input.DescId"
                class="tab-pane"
                v-bind:class="{'active': index===idxBookmark}"
                :id="input.DescId + '_id'"
            >
                <component
                    v-if="input.TableType != FieldType.Logic"
                    :cptFromAside="true"
                    :memoOpenedFullMode="index === memoOpenedFullModeObj.index ? memoOpenedFullModeObj.value : null"
                    :prop-aside="true"
                    :data-input="input"
                    :is="dynamicFormatChamps(input)"
                />
                <div v-else>{{input.Label}}</div>
            </div>
        </div>
    </div>
`
};