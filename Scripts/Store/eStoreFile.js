/*****************************************************************************
* Fonction globale pour les extensions en mode fiche du nouveau store
*****************************************************************************/
"use strict";

var fnStoreFile = function () {

    var modContent = document.querySelector('.store-content-container');
    var storeExtensionDetailHeader = document.querySelector('.store-extension-detail-header')
    var moduleInstalled = modContent ? modContent.classList.contains('installed') : false;
    var moduleBeingInstalled = modContent ? storeExtensionDetailHeader.classList.contains('being-installed') : false;

    // media queries
    //var overflowTest = document.querySelector('.overflow-container');
    var overflowTest = document.querySelector('.mainDiv');
    var overflowHeight = 0;

    if (overflowTest) {
        for (let i = 0; i < overflowTest.children.length; i++)
            overflowHeight += overflowTest.children[i].clientHeight;

        if (overflowHeight > 0 && overflowTest.children.length > 1)
            overflowTest.children[1].style.height = overflowHeight + 'px';
    }

    var laptop = window.matchMedia("(min-width: 1024px) and (max-width: 1500px)");
    var laptopScroll = window.matchMedia("(max-width: 1358px)");
    var tablet = window.matchMedia("(max-width: 1114px)");
    var mobile = window.matchMedia("(max-width: 1024px)");

    // Internet Explorer 6-11
    var isIE = /*@cc_on!@*/false || !!document.documentMode;

    // changer la taille du header au scroll

    var header = document.querySelector('.store-extension-detail-header');

    /*****************************************************************************
 * // Scrollbar pour les fiches des extensions.
 *****************************************************************************/
    /** Si la div permettant le scroll est présente.*/
    if (overflowTest) {

        /**
         * Fonction qui va afficher ou non une entête spécifique au
         * scroll.
         */
        //var fnHeaderAffic = function (sens) {            
        //    if (sens) {
        //        header.classList.add("store-scrolled");
        //    }
        //    else {
        //        header.classList.remove("store-scrolled");
        //    }
        //}

        //overflowTest.addEventListener("scroll", function (e) {
        //    fnHeaderAffic((parseInt(overflowTest.scrollTop) > 0));
        //});
    }

    /** bouton qui permet d'acheter ou d'installer l'application. */
    var btnAction = document.querySelector('.store-btn');
 
    if (btnAction && !isIE) {
        if (btnAction.innerHTML.includes('Annuler') && mobile.matches) {
            btnAction.innerHTML = 'Annuler'
        } else if (btnAction.innerHTML.includes('Annuler') && tablet.matches) {
            btnAction.innerHTML = 'Annuler la demande'
        } else if (btnAction.innerHTML.includes("Finaliser") && mobile.matches) {
            btnAction.innerHTML = 'Finaliser'
        }
    }

    //   autoplay slider

    /* l'autoplay est mis sur pause lorsque l'on positionne la souris sur les chevrons de navigation */

    var storeContentSlider = document.querySelector(".store-content-slider")

    if (storeContentSlider){

        var int = 0;
        var timeout;
        var arrowAfter = document.querySelector(".goto-first");
        var arrowBefore = document.querySelector(".goto-last");
        var arrow = [].slice.call(document.querySelectorAll('.goto-first, .goto-last'));
        var margin = 0
        var stopped = false; //permet de stopper l'autoplay et de le relancer
        //var navLabel = [].slice.call(document.querySelectorAll('.store-navigation label'));
        var navLabel = [].slice.call(document.querySelectorAll('.store-navigation > div[id^="slide"]'));
        var storeTooltip = [].slice.call(document.querySelectorAll('.store-tooltip'));
        var storeTooltipImg = [].slice.call(document.querySelectorAll('.store-tooltip > div > img'));

        navLabel[0].classList.add('store-targeted');

        /* autoplay pour hover sur les chevrons */

        stopAutoPlay(arrow, 'mouseover')
        restartAutoPlay(arrow, 'mouseout')

        stopAutoPlay(arrow, 'touchstart')
        restartAutoPlay(arrow, 'touchend')

        /* autoplay hover sur les images */

        stopAutoPlay(storeTooltipImg, 'mouseover')
        restartAutoPlay(storeTooltipImg, 'mouseout')

        stopAutoPlay(storeTooltipImg, 'touchstart')
        restartAutoPlay(storeTooltipImg, 'touchend')

        /* autoplay hover sur les bullets de nav */

        stopAutoPlay(navLabel, 'mouseover');
        restartAutoPlay(navLabel, 'mouseout');

        stopAutoPlay(navLabel, 'touchstart');
        restartAutoPlay(navLabel, 'touchend');

        /* clic sur le chevron après et avant */

        arrowAfter.addEventListener('click', goToLast);
        arrowBefore.addEventListener('click', goBack);

        /* verifie si le diapo est en pause pour lancer l'autoplay */

        if (stopped == false) {
            advance();
        }

        /* Permet de stopper l'autoplay lorsque l'on met la souris sur les chevrons avant et après */

        function stopAutoPlay(elem,event) {
            if (elem && elem.length > 1)
                for (i = 0; i < elem.length; i++) {
                elem[i].addEventListener(event, function (e) {
                    e.preventDefault();
                    stopped = true;
                });
            }
        }

        /* Permet de relancer l'autoplay lorsque l'on sort la souris des chevrons avant et après */

        function restartAutoPlay(elem,event) {
            if (elem && elem.length > 1)
                for (i = 0; i < elem.length; i++) {
                elem[i].addEventListener(event, function () {
                    stopped = false;
                    advance();
                });
            }
        }



        function labelNavigation(index) {
            navLabel.map(function (x) {
                return x.classList.remove('store-targeted');
            });
            navLabel[index].classList.add('store-targeted');
        }

        function goToLast(e) {
            e.preventDefault();
            if (int < storeTooltip.length - 1) {
                int++;
                margin = margin - 100;
                storeTooltip[0].style.marginLeft = margin + '%';
                labelNavigation(int);
            } else {
                int = 0;
                margin = 0;
                storeTooltip[0].style.marginLeft = '0%';
                labelNavigation(int);
            }
        }

        function goBack() {
            if (int > storeTooltip.length - 1) {
                int = 0;
                margin = 0;
                storeTooltip[0].style.marginLeft = '0%';
                labelNavigation(int);
            } else if (int < 1) {
                int = storeTooltip.length - 1;
                margin = (storeTooltip.length - 1) * -100;
                storeTooltip[0].style.marginLeft = margin + '%';
                labelNavigation(int);
            } else {
                int--;
                margin = margin + 100;
                storeTooltip[0].style.marginLeft = margin + '%';
                labelNavigation(int);
            }
        }

        for (i = 0; i < navLabel.length; i++) {
            navLabel[i].addEventListener('click', changeImage)
        }

        function changeImage(e) {
            e.preventDefault();
            int = navLabel.indexOf(e.target);
            margin = int * -100;
            navLabel.map(function (x) {
                return x.classList.remove('store-targeted');
            });
            e.target.classList.add('store-targeted');
            storeTooltip[0].style.marginLeft = margin + '%';
        }

        /****************************************************
         * Fonction pour lancer l'autoplay: 
         ****************************************************/
        function sliderAutoPlay(int) {
            if (storeTooltip && storeTooltip.length > int)
                if (stopped == false) {
                    margin = margin - 100;
                    storeTooltip[0].style.marginLeft = margin + '%';
                    labelNavigation(int);
                }
            if (stopped == false)
                advance();
        }

        /****************************************************
         * Fonction de calcul de l'autoplay: 
         ****************************************************/
        function advance() {
            if (stopped == false) {
                clearTimeout(timeout);
                timeout = setTimeout(function () {
                    if (int < storeTooltip.length - 1) {
                        if (stopped == false)
                            sliderAutoPlay(int += 1);
                    } else if (int == storeTooltip.length - 1 && stopped == true) {
                        int++;
                    } else if (int == storeTooltip.length - 1 && stopped == false) {
                        int++;
                        sliderAutoPlay(int);
                    } else if (int > storeTooltip.length - 1 && stopped == false) {
                        int = 0;
                        margin = 100;
                        sliderAutoPlay(int);
                    }
                }, 3000);
            }
        }

        // Get the modal - popin du slider
        var modal = document.querySelector(".store-modal");
        var btnLeft = document.querySelector(".store-slide-left");
        var btnRight = document.querySelector(".store-slide-right");

        // Get the image and insert it inside the modal - use its "alt" text as a caption
        var sliderCont = [].slice.call(document.querySelectorAll('.store-img-slider-container'));
        var imgSlider = [].slice.call(document.querySelectorAll('.store-img-slider'));
        var modalIframe = document.querySelector('.store-modal-iframe');
        var modalImg = document.getElementById("img01");
        var captionText = document.getElementById("store-caption");
        var legend = [].slice.call(document.querySelectorAll(".hoverExt"));
        var caption = document.querySelector("#store-caption");

        /* Cache les éléments en mode modal : iframe si image et 
        image si Vidéos */

        function hideElem(visible, invisible) {
            visible.style.display = "block";
            invisible.style.display = "none";
        }

        /****************************************************
         * Change la source des images et la légende en fonction de l'image 
         * du slider
         ****************************************************/

        function changeSrc(imgSrc, imgCpt) {
            modalImg.src = imgSrc.src;
            captionText.innerHTML = imgCpt.alt;
        }

        /****************************************************
         * Change la source des iframes en fonction de l'image 
         * du slider
         ****************************************************/

        function changeIframe(iframeSrc) {
            var ytUrl = iframeSrc.attributes.getNamedItem('ednvid').nodeValue;
            // on ne peut pas mettre 1 vidéo avec watch? il faut 
            // qu'il y ait /embed/ pour 1 iframe
            var iframeUrl = ytUrl.replace("/watch?v=", "/embed/");
            modalIframe.src = iframeUrl;
        }

        /****************************************************
         * Fallback pour IE11 car les images du slider sont en display none
         * sur IE11 et on voit le background du container à la place 
         ****************************************************/
        function imgModalIE() {
            sliderCont.forEach(function (cont) {
                cont.addEventListener('click', function (event) {
                    modal.classList.add('store-opened');
                    modal.focus();
                    if (event.target.children[0].classList.contains('video-content')) {
                        changeIframe(event.target.children[0]);
                        hideElem(modalIframe, modalImg);
                    } else {
                        // Permet de récupérer l'url de bg image 
                        var url = this.style.backgroundImage.slice(4, -1).replace(/["']/g, "");
                        /* Je n'appelle pas la fonction changeSrc car dans ce cas
                        on ne cible pas la Source(src) */
                        modalImg.src = url;
                        hideElem(modalImg, modalIframe);
                    }
                })
            })
        }

        /****************************************************
         * 
         ****************************************************/
        function imgModal() {
            for (i = 0; i < imgSlider.length; i++) {
                imgSlider[i].addEventListener('click', function () {
                    modal.classList.add('store-opened');
                    modal.focus();
                    if (this.classList.contains('video-content')) {
                        changeIframe(this);
                        changeSrc(imgSlider[i], imgSlider[i]);
                        hideElem(modalIframe, modalImg);
                    } else {
                        changeSrc(this, this)
                        hideElem(modalImg, modalIframe);
                    }
                });
            }
        }

        /****************************************************
         * Fallback pour IE11 car les images du slider sont en display none 
         ****************************************************/

        if (isIE) {
            imgModalIE();
        } else {
            imgModal();
        }

        function prev() {
            if (i == 0) {
                i = imgSlider.length - 1;
                if (imgSlider[i].classList.contains('video-content')) {
                    changeSrc(imgSlider[i], imgSlider[i]);
                    changeIframe(imgSlider[i]);
                    hideElem(modalIframe, modalImg);
                } else {
                    changeSrc(imgSlider[i], imgSlider[i]);
                    hideElem(modalImg, modalIframe);
                }
            } else {
                i--;
                if (imgSlider[i].classList.contains('video-content')) {
                    changeSrc(imgSlider[i], imgSlider[i]);
                    changeIframe(imgSlider[i]);
                    hideElem(modalIframe, modalImg);
                } else {
                    hideElem(modalImg, modalIframe);
                    changeSrc(imgSlider[i], imgSlider[i]);
                }
            }
        }

        function next() {
            if (i === imgSlider.length - 1) {
                i = 0;
                if (imgSlider[i].classList.contains('video-content')) {
                    changeSrc(imgSlider[i], imgSlider[i]);
                    changeIframe(imgSlider[i])
                    hideElem(modalIframe, modalImg);
                } else {
                    hideElem(modalImg, modalIframe);
                    changeSrc(imgSlider[i], imgSlider[i]);
                }
            } else {
                i++;
                console.log(i)
                if (imgSlider[i].classList.contains('video-content')) {
                    changeSrc(imgSlider[i], imgSlider[i]);
                    changeIframe(imgSlider[i])
                    hideElem(modalIframe, modalImg);
                } else {
                    hideElem(modalImg, modalIframe);
                    changeSrc(imgSlider[i], imgSlider[i]);
                }
            }
        }

        var i = 0;
        if (btnLeft)
            btnLeft.addEventListener('click', function (e) {
                e.stopPropagation();
                prev();
            });

        if (btnRight)
            btnRight.addEventListener('click', function (e) {
                e.stopPropagation();
                next();
                modalImg.src = imgSlider[i].src;
            });

        // Get the <span> element that closes the modal
        var span = null;
        if (document.getElementsByClassName("store-close") && document.getElementsByClassName("store-close").length > 0)
            span = document.getElementsByClassName("store-close")[0];

        /** Popup modale */
        var divExtension = document.getElementById("myModal");
        divExtension.setAttribute("tabindex", "0");

        // When the user clicks on <span> (x), close the modal
        if (span)
            span.onclick = function () {
                modal.classList.remove('store-opened');
                if (modalIframe)
                    modalIframe.removeAttribute("src");
            }

        if (modalImg)
            modalImg.addEventListener("click", function (e) {
                e.stopPropagation();
            });
        if (caption)
            caption.addEventListener("click", function (e) {
                e.stopPropagation();
            });


        if (divExtension) {
            divExtension.addEventListener("keydown", function (e) {
                if (e.keyCode == '37') {
                    e.stopPropagation();
                    e.preventDefault();

                    // left arrow
                    prev();
                } else if (e.keyCode == '39') {
                    e.stopPropagation();
                    e.preventDefault();

                    // right arrow
                    next();
                } else if (e.key === "Escape") {
                    e.stopPropagation();
                    e.preventDefault();

                    modal.classList.remove('store-opened');

                    if (modalIframe)
                        modalIframe.removeAttribute("src");
                }
            });
        }

        if (modal)
            modal.addEventListener("click", function (e) {
                modal.classList.remove('store-opened');
                e.stopPropagation();
            });

    }

    /* fallback pour les images qui ont la classe object-fit */

    function objectFit(container) {
        if ('objectFit' in document.documentElement.style === false) {

            // Boucle à travers la Collection HTML
            for (var i = 0; i < container.length; i++) {

                // Asign image source to variable
                var imageSource = container[i].querySelector('img').src;

                // Hide image
                if (container[i].querySelector('img'))
                    container[i].querySelector('img').style.display = 'none';
                // Add background-size: cover
                container[i].style.backgroundSize = 'cover';
                // Add background-image: and put image source here
                container[i].style.backgroundImage = 'url("'  + imageSource + '")';
                // Add background-position: center center
                container[i].style.backgroundPosition = 'center center';

                if (container == storeDetailLogoContainer) {
                    container[i].style.width = "90px";
                    container[i].style.height = "80px";
                }
            }
        }
    }


    // assign HTMLCollection with parents of images with objectFit to variable
    var container = document.getElementsByClassName('store-img-slider-container');
    var cardLogoContainer = document.getElementsByClassName('store-card-logo-container');
    var storeDetailLogoContainer = document.getElementsByClassName('store-detail-logo-container');

    objectFit(container)
    objectFit(cardLogoContainer)
    objectFit(storeDetailLogoContainer)

    /** si le module est installé. Bidouillage, car on ajoute le classe installed en CSS. */
    if (moduleInstalled) {

        // Get the element with id="defaultOpen" and click on it
        //SHA : correction bug 71 446
        //document.getElementById("defaultOpen").click();

        /* carrousel extension */

        // Boutons et éléments du carrousel où il y a des event listeners et autres 
        var carousel = document.querySelector("[data-target='carousel']");
        var leftButton = document.querySelector("[data-action='slideLeft']");
        var rightButton = document.querySelector("[data-action='slideRight']");

        // Preparez à limiter la direction dans laquelle le carousel peut
        //bouger
        // et pour contrôler de combien de pixels le carrousel avance à chaque fois
        //afin que le carrousel affiche trois cartes visibles à chaque fois
        // On a besoin de connaître la largeur du carrousel et la marge appliquée sur une carte donnée dans le carrousel
        if (carousel) {
            var card = carousel.querySelector("[data-target='store-card']");
            var carouselWidth = carousel.offsetWidth;
            var cardMarginRight = 0;
            var cardCount = 0;
            var cardCountEven = 0;

            if (card) {
                var cardStyle = card.currentStyle || window.getComputedStyle(card);
                if (cardStyle) {
                    cardMarginRight = Number(cardStyle.marginRight.match(/\d+/g)[0]);
                }
            }

            if (carousel.querySelectorAll("[data-target='store-card']")) {
                // On compte le nombre total de cartes que l'on a 
                cardCount = carousel.querySelectorAll("[data-target='store-card']").length;
                // Rajout pour les carrousel avec un nombre de carte pair
                cardCountEven = carousel.querySelectorAll("[data-target='store-card']").length - 1;
            }


            // Define an offset property to dynamically update by clicking the button controls
            // as well as a maxX property so the carousel knows when to stop at the upper limit
            var offset = 0;

            /* Version Ordinateur - 5 images */

            var maxX = -((cardCount / 5) * carouselWidth +
                           (cardMarginRight * (cardCount / 5)) -
                           carouselWidth - cardMarginRight);

            /* Mediaqueries 1358px - 3 images */

            var maxXResp = -((cardCount / 3) * carouselWidth + (cardMarginRight * (cardCount / 3)) - carouselWidth - cardMarginRight);

            /* Mediaqueries 1024px  - 2 images */
            var maxXRespDeux = -((cardCountEven / 2) * carouselWidth + (cardMarginRight * (cardCountEven / 2)) - carouselWidth - cardMarginRight);

            var moveToRight = function (size, length) {
                //déroule le carrousel si différent de la largeur max et va à au début si égale à la largeur max 
                if (offset !== size) {
                    offset -= carouselWidth + cardMarginRight;
                    carousel.style.transform = "translateX(".concat(offset, "px)");
                } else if (offset == size) {
                    offset += carouselWidth * length + cardMarginRight * length;
                    carousel.style.transform = "translateX(".concat(offset, "px)");
                }
            }



            var moveToLeft = function (length) {
                if (offset !== 0) {
                    //déroule le carrousel si différent 0 et va à la fin si égale à 0 
                    offset += carouselWidth + cardMarginRight;
                    carousel.style.transform = "translateX(".concat(offset, "px)");
                } else if (offset == 0) {
                    offset -= carouselWidth * length + cardMarginRight * length;
                    carousel.style.transform = "translateX(".concat(offset, "px)");
                }
            }

            // Bouton de droite event
            rightButton.addEventListener("click", function () {
                // media querie si l'écran passe en latop
                if (laptop.matches) {
                    moveToRight(maxXResp, 4)
                } else if (mobile.matches) {
                    moveToRight(maxXRespDeux, 6)
                } else {
                    moveToRight(maxX, 2)
                }
            });

            // Bouton de gauche event
            leftButton.addEventListener("click", function () {
                // media querie si l'écran passe en latop - 3 images
                if (laptop.matches) {
                    moveToLeft(4)
                } else if (mobile.matches) {
                    // media querie si l'écran passe en latop - 2 images
                    moveToLeft(6)
                } else {
                    // media querie si l'écran passe en desktop
                    moveToLeft(2)
                }
            });
        }



    }

    //if (moduleInstalled || moduleBeingInstalled){



    //            }




};

/*****************************************************************************/