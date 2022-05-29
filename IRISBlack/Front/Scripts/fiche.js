
// Date range picker asideRight fiche


$('#reservation').daterangepicker(
    {
        ranges:{
            'Aujourd\'hui': [moment(), moment()],
            'Hier'   : [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'Les 7 derniers jours ' : [moment().subtract(6, 'days'), moment()],
            'Les 30 derniers jours': [moment().subtract(29, 'days'), moment()],
            'Ce mois-ci'  : [moment().startOf('month'), moment().endOf('month')],
            'Le mois dernier'  : [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        },
        startDate: moment().subtract(29, 'days'),
        endDate: moment(),
        "locale": {
            "format": "DD/MM/YYYY",
            "separator": " - ",
            "applyLabel": "Valider",
            "cancelLabel": "Annuler",
            "fromLabel": "De",
            "toLabel": "à",
            "customRangeLabel": "Personnalisé",
            "daysOfWeek": [
                "Dim",
                "Lun",
                "Mar",
                "Mer",
                "Jeu",
                "Ven",
                "Sam"
            ],
            "monthNames": [
                "Janvier",
                "Février",
                "Mars",
                "Avril",
                "Mai",
                "Juin",
                "Juillet",
                "Août",
                "Septembre",
                "Octobre",
                "Novembre",
                "Décembre"
            ],
            "firstDay": 1
        }
    },
    function (start, end) {
        $('#reservation').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'))
    }
)


// Note button asideRight fiche
$(function () {
    $('.textarea').wysihtml5({
        placeholder:'',
        toolbar: {
            "font-styles": false,
            "emphasis": true, // Italics, bold, etc.
            "lists": false, // (Un)ordered lists, e.g. Bullets, Numbers.
            "html": false, // Button which allows you to edit the generated HTML.
            "link": false, // Button to insert a link.
            "image": false, // Button to insert an image.
            "color": true, // Button to change color of font
            "blockquote": false, // Blockquote
        }
    });
})

// Impression fiche 
function impressionFiche() {
    window.open("impressionFiche.php");
}

// recherche dans la liste d'un signet 
$("#searchExemple1").keyup(function() {
    var value = this.value.toLowerCase().trim();
    $("#exemple1 tr").each(function(index) {
        if (!index) return;
        $(this).find("td").each(function() {
            var id = $(this).text().toLowerCase().trim();
            var matchedIndex = id.indexOf(value);
            var not_found = (matchedIndex == -1);
            $(this).closest('tr').toggle(!not_found);
            return not_found;
        });
    });
});


// chat
function removeElem(elem){
    var name = document.getElementsByClassName("name");
    for(var i = 0; i < name.length; i++){
        if(name[i].innerHTML == elem.parentElement.parentElement.children[0].children[0].innerHTML){
            name[i].parentElement.parentElement.parentElement.classList.remove("elemOpen")
        }
    }
    elem.parentElement.parentElement.parentElement.parentElement.remove();
    calculChatBox();
}

var direct_chat_messages = document.getElementsByClassName("direct-chat-messages");
for(var i = 0; i < direct_chat_messages.length; i++){
    direct_chat_messages[i].scrollTop = direct_chat_messages[i].scrollHeight
} 

function focusBoxChat(e){
    var boxChat = document.getElementsByClassName("direct-chat");

    for(var i = 0; i < boxChat.length; i++ ){
        boxChat[i].classList.remove("box-primary");
        boxChat[i].classList.add("box-default");
    }
    e.children[0].classList.remove("box-default");
    e.children[0].classList.add("box-primary");

}
$('#toggle').on('click', function () {
    if($('.rowChatBox').hasClass("control-open-menu-chat")){
        $('.rowChatBox').removeClass('control-open-menu-chat')
    }else{
        $('.rowChatBox').addClass('control-open-menu-chat')
    }

});

// active filtre favoris
function changefiltre1(elem){
    var cible = document.getElementsByClassName("count-filtre1");
    var val = elem.childNodes[0];
    var cibleVal = cible[0].childNodes[4];    
    cibleVal.nodeValue = "(" + val.nodeValue + ")";
}
function changefiltre2(elem){
    var cible = document.getElementsByClassName("count-filtre2");
    var val = elem.childNodes[0];
    var cibleVal = cible[0].childNodes[4];    
    cibleVal.nodeValue = "(" + val.nodeValue + ")";
}

// change color background annexe rightBarFiche
function dragEnter(event) {
    event.target.style.backgroundColor = "#ecf0f5";
}

function dragLeave(event) {
    event.target.style.backgroundColor = "#fff";

}



// change vue signet 'list' to 'carte'

// document.getElementById("count-span-list").style.display = "none";
// document.getElementById("count-span-list-equip").style.display = "none";
// document.getElementById("liste").style.display = "none";
// // document.getElementById("incruster").style.display = "none";
// document.getElementById("liste-equipement").style.display = "none";
// document.getElementById("incruster-equipement").style.display = "none";

// function Liste() {
//     document.getElementById("fiche").style.display = "none";
//     // document.getElementById("incruster").style.display = "none";
//     document.getElementById("count-action-carte").style.display = "none";
//     document.getElementById("liste").style.display = "block";
//     document.getElementById("count-span-list").style.display = "inherit";
//     countfinal();
// }

// function Fiche() {
//     document.getElementById("count-span-list").style.display = "none";
//     document.getElementById("liste").style.display = "none";
//     // document.getElementById("incruster").style.display = "none";
//     document.getElementById("fiche").style.display = "block";
//     // document.getElementById("count-action-carte").style.display = "inherit";
//     checked();
// }

// function ficheIncruster() {
//     document.getElementById("fiche").style.display = "none";
//     document.getElementById("liste").style.display = "none";
//     // document.getElementById("incruster").style.display = "block";
// }

// function Liste_equipement() {
//     document.getElementById("fiche-equipement").style.display = "none";
//     document.getElementById("incruster-equipement").style.display = "none";
//     document.getElementById("count-action-carte-equip").style.display = "none";
//     document.getElementById("liste-equipement").style.display = "block";
//     document.getElementById("count-span-list-equip").style.display = "inherit";
//     //countfinal_equip();
// }

// function Fiche_equipement() {
//     document.getElementById("count-span-list-equip").style.display = "none";
//     document.getElementById("liste-equipement").style.display = "none";
//     document.getElementById("incruster-equipement").style.display = "none";
//     document.getElementById("fiche-equipement").style.display = "block";
//     document.getElementById("count-action-carte-equip").style.display = "inherit";
//     checked_equip();
// }
// Fiche();
// Fiche_equipement();

// function goBack(){
//     document.querySelector('#fiche').style.display = "block";
//     document.getElementById("count-span-list-equip").style.display = "none";
//     document.getElementById("liste").style.display = "none";
//     document.getElementById("incruster").classList.remove('inc-file-visible');
//     document.getElementById("liste-equipement").style.display = "none";
//     document.getElementById("incruster-equipement").style.display = "none";
// }

// focus input red or green
function myFunction(x) {
    if(x.value == ""){
        x.className += " input-danger";
        setTimeout(function(){
            x.classList.remove("input-danger");
        }, 1000); 
    }else{
        x.className += " input-success";
        setTimeout(function(){
            x.classList.remove("input-success");
        }, 1000);
    }
}

// au scroll affiche le menu en haut
$(window).scroll(function() {
    if ($(document).scrollTop() > 50) {
        $('.content-header').addClass('color-change');
    } else {
        $('.content-header').removeClass('color-change');
    }
});


//popover hover sur le nom dans une liste d'un signet une modal s'affiche sur la droite
popOver();
function popOver(){
    $(function(){
        $('[rel="popover"]').popover({
            container: 'body',
            html: true,
            content: function () {
                var clone = $($(this).data('popover-content')).clone(true).removeClass('hide');
                return clone;
            }
        }).click(function(e) {
            e.preventDefault();
        });

        $('[rel="popover"]').popover({
            animation:false
        }).on('hide.bs.popover', function () {
            if ($(".popover:hover").length) {
                return false;
            }                
        });
        $('body').on('mouseleave', '.popover', function(){
            $('.popover').popover('hide');
        });
    });
}

popOver2();
function popOver2(){
    $(function(){
        $('[rel="popover_2"]').popover({
            container: 'body',
            html: true,
            placement: function (context, source){
                if(window.matchMedia("(min-width:993px)").matches) {
                    if(source.parentNode.childNodes[1].innerHTML == "Filtres favoris" || source.parentNode.childNodes[1].innerHTML == "Rapport favoris" || source.parentNode.childNodes[1].innerHTML == "Publipostage favoris" || source.parentNode.childNodes[1].innerHTML == "Export favoris" || source.parentNode.childNodes[1].innerHTML == "Graphique favoris"){
                        return "right";
                    }else{
                        return "left";
                    }
                }else{
                    var position = $(source).position();
                    if (position.top < 200){ 
                        return "bottom";
                    }else{
                        return "top";
                    }
                }
            },
            content: function () {
                var clone = $($(this).data('popover-content')).clone(false).removeClass('hide');
                return clone;
            }
        }).click(function(e) {
            e.preventDefault();
        });
    });
}

// init select option
$(function () {
    $('.select2').select2()
})


//init des datepicker
$(function () {


    //Date picker
    $('#datepicker').datepicker({
        autoclose: true,
        language: 'fr'
    })

    $('#datepicker-2').datepicker({
        autoclose: true,
        language: 'fr'
    })

    $('#datepicker-3').datepicker({
        autoclose: true,
        language: 'fr'
    })

    $('#datepicker-4').datepicker({
        autoclose: true,
        language: 'fr'
    })
})

var today = new Date();


//popover de l'en-tête
$(document).ready(function(){
    $('[data-toggle="popover"]').popover();   
});


// active steps bar
var steps_1 = document.getElementById("steps_1");
var steps_2 = document.getElementById("steps_2");
var steps_3 = document.getElementById("steps_3");

function steps1() {            
    if(steps_1.className == "steps__item steps__item--done steps__item--first"){
        steps_1.className = "steps__item steps__item--active steps__item--first";
        steps_2.className = "steps__item steps__item--done";
        steps_3.className = "steps__item steps__item--done";
    }
}

function steps2() {
    if(steps_2.className == "steps__item steps__item--done"){
        steps_2.className = "steps__item steps__item--active";
        steps_1.className = "steps__item steps__item--done steps__item--first";
        steps_3.className = "steps__item steps__item--done";

    }
}

function steps3() {
    if(steps_3.className == "steps__item steps__item--done"){
        steps_3.className = "steps__item steps__item--active";
        steps_1.className = "steps__item steps__item--done steps__item--first";
        steps_2.className = "steps__item steps__item--done";

    }
}

// modal type alertify
alertify.defaults.theme.ok = "btn btn-success";
alertify.defaults.theme.cancel = "btn btn-danger";
alertify.defaults.theme.input = "form-control";
function ModalListPropertyFiche(){
    var innerProperty = document.getElementById("innerHeaderProperty");
    var innerContentProperty = document.getElementById("content_listeProperty");
    document.getElementById("content_listeProperty").style.display = "block";
    if(!alertify.Property){
        alertify.dialog('Property',function(){
            return{
                main:function(message){
                    this.message = message;
                    this.elements.root.classList.add("alertProperty");
                },
                setup:function(){
                    return {
                        buttons:[{
                            text: "OK",
                            key:27/*Esc*/,
                            className: alertify.defaults.theme.ok,
                        }],
                        focus: {
                            element:0
                        },
                        options: {
                            title: innerProperty
                        }
                    };
                },
                prepare:function(){
                    this.setContent(this.message);
                },
                callback:function(closeEvent){

                }
            }});
    }
    alertify.Property(innerContentProperty);

    alertify.Property().set({
        basic: false,
        transition:'zoom',
        movable:false,
        closable: true,
        maximizable: true,
        resizable: false,
        labels: {
            ok:this.getRes(944),
            cancel:this.getRes(30)
        },

    }).show();
}


// GridStack Fiche
/*
$(function() {
    var options = {
        float: true,
        width: 12,
        height: 999,
        animate: true,
        always_show_resize_handle: true,
        cellHeight: 5,
        verticalMargin: 10,
        horizontalMargin: 10,
        placeholder_class: 'grid-stack-placeholder',
        acceptWidgets: '.grid-stack-item'
    };

    $('.grid-stack').gridstack(_.defaults(options));
    $('.grid-stack-item').draggable({cancel: ".not-draggable" });
    var droppables = [{
        x: 0,
        y: 0,
        width: 1,
        height: 1
    }];
});

$('.grid-stack').on('gsresizestop', function(event, elem) {
    var newHeight = $(elem).attr('data-gs-height');
});

$('.grid-stack-item').on('resize', function(el, width, height) {
    let MyHeight = height
    let element = el.target;
    let note = el.target.getElementsByClassName('note-steps');
    if(note.length > 0){
        for(var i = 0; i < note.length; i++){
            note[i].style.maxHeight = MyHeight - 180 + 'px';
        }
    }

});
*/

//NavBar run left run right
var NavBarSub = document.getElementById("NavBarSub");
NavBarSub.style.right = initRight + "px";
function runRight(){
    if(NavBarSub.style.right == "0px"){
        initRight = 0;
        iniTab = -1;
    }
    var NavBar = document.getElementById("NavBar");
    var rubonNav = document.getElementsByClassName("rubonNav");
    var AdditionWidht = 0;
    for(var i = 0; i < rubonNav.length; i++){
        if($(rubonNav[i]).hasClass("hiddenLi")){

        }
        else{
            AdditionWidht += rubonNav[i].clientWidth;
        }
    }
    if(iniTab < NavBarSub.children.length && AdditionWidht > NavBar.clientWidth){
        iniTab += 1;
        initRight += NavBarSub.children[iniTab].offsetWidth;
        NavBarSub.style.right = initRight + "px";
        NavBarSub.children[iniTab].classList.add("hiddenLi");

    }else{
        return false;
    }
}

function runLeft(){
    if(NavBarSub.style.right != 0 + "px"){
        var hiddenLi = document.getElementsByClassName("hiddenLi");
        var initTest = hiddenLi.length -1;
        NavBarSub.children[initTest].classList.remove("hiddenLi");
        initRight -= NavBarSub.children[initTest].offsetWidth;
        NavBarSub.style.right = initRight + "px";
        iniTab -= 1;

    }else{
        return false;
    }
}

// initNavBar Calcul
var initRight = 0;
var iniTab = -1;
var NavBar = document.getElementById("NavBar");
var arrowNavBarLeft = document.getElementsByClassName("arrowNavBarLeft")[0];
var arrowNavBarRight = document.getElementsByClassName("arrowNavBarLeft")[1];

if(NavBar.offsetWidth > window.innerWidth - 400){
    //NavBar.style.width = window.innerWidth - 520 + 'px';
}else{
    //arrowNavBarLeft.style.display = "none";
    //arrowNavBarRight.style.display = "none";
}
InitresizeNav();
function InitresizeNav(){
    resizeChatBoxOpen();
    var NavBarSub = document.getElementById("NavBarSub");
    var logo = document.getElementById("logo");
    var navBarCustom = document.getElementById("navBarCustom");
    var contenairNavBar  = document.getElementById("contenairNavBar")
    var widthNavBar = contenairNavBar.offsetWidth - logo.offsetWidth - navBarCustom.offsetWidth;
    NavBar.style.width = widthNavBar -72 + 'px';

    var hiddenLi = document.getElementsByClassName("hiddenLi");
    var initValue = -1;
    for(var i = 0; i < hiddenLi.length; i++){
        initValue += 1;
    }

    if(hiddenLi[initValue]){
        var rightTransform = NavBarSub.style.right.substr(0, NavBarSub.style.right.length-2);

        if(NavBarSub.clientWidth - rightTransform + hiddenLi[initValue].clientWidth < NavBar.clientWidth){
            runLeft();
        }
    }
    setTimeout(function(){ 
        var navUlOut = document.getElementById("navUlOut");
        var navUl = document.getElementById("navUl");
        if(NavBar.offsetWidth > NavBarSub.offsetWidth ){
            var hiddenLi = document.getElementsByClassName("hiddenLi");
            for(var i = 0; i < hiddenLi.length; i++){
                hiddenLi[i].classList.remove("hiddenLi");
            }
            NavBarSub.style.right = "0px"
            arrowNavBarLeft.style.display = "none";
            arrowNavBarRight.style.display = "none";
            navUlOut.style.display = "none";
            navUl.style.display = "block";

        }else{
            arrowNavBarLeft.style.display = "block";
            arrowNavBarRight.style.display = "block";
            navUlOut.style.display = "block";
            navUl.style.display = "none";
        }
    }, 0);
}

window.onresize = InitresizeNav;


// date pour la steps line




