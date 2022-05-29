
export default {
    name: "configComponent",
    data() {
        return {
        };
    },
    props: ["typeBlock", "titleBlock", "descriptionBlock"],
    template: `
        <div>
            <div class="headerRightCard">
                <div class="row">
                    <div class="col-md-10 typeBlock"><h5>{{ typeBlock }}</h5></div>
                    <div class="col-md-2"><button id="close" class="close fas fa-times"></button></div>
                </div>
            </div>
            <div class="contentRightCard">
                <h3 class="titleBlock">{{ titleBlock }}</h3>
                <div class="descriptionBlock"><span>{{ descriptionBlock }}</span></div>
            </div>
        </div>
`,

};