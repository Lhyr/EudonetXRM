<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" omit-xml-declaration="yes"/>
  <xsl:template match="menu">
    <div id="menuBar"  class="menuBar {@fontsize}" pinned="{RightMenuPinned}" adminmode="{AdminMode}">
      <xsl:if test="Navigateur='1'">
        <div class="switch-new-theme-wrap">
          <input class="switch-new-theme" id="chckNwThm" type="checkbox" onclick="fnChgChckNwThm(this);">
            <xsl:if test="chckNwThm='1'">
              <xsl:attribute name="checked"></xsl:attribute>
            </xsl:if>
          </input>
          <label for="chckNwThm"> </label>
            <span class="switch-new-theme-spn">
              <xsl:attribute name="title" >
                <xsl:value-of select="NwThmTtl" />
              </xsl:attribute>
              <xsl:value-of select="NwThmVal"/>
            </span>
          <!--SHA : backlog #1 647-->
            <img class="switch-new-theme-img" src="themes/default/images/logo-eudonetx.jpg" title="{res/n2378}" alt="{res/n2378}" />
        </div>
      </xsl:if>
      <xsl:if test="NotifsEnabled='1'">
        <div id="divNotifToggleContainer" onclick="notifListToggle(event);">
          <span id="spanNotifToggle" class="icon-bell"></span>
          <xsl:choose>
            <xsl:when test="NotifsUnreadCount > 0">
              <span id="spanNotifCounter" class="notifCounterShow">
                <xsl:value-of select="NotifsUnreadCount"/>
              </span>
            </xsl:when>
            <xsl:otherwise>
              <span id="spanNotifCounter" class="notifCounterHide">
                <xsl:value-of select="NotifsUnreadCount"/>
              </span>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </xsl:if>
      <div id="menuPin" class="icon-menu_btn_acc" userid="{userBlock/userid}">
      </div>
    </div>
    <input id="mapDisplay" type="hidden" value="{CartoDisplay}"></input>
    <div id="Gerard" class="Gerard {@fontsize}">
      <!-- BLOC DU PROFIL -->
      <ul class="encartProfil">
        <li>
          <div class="hAvatar" id="hAvatar" ondragover="UpFilDragOver(this, event);return false;" ondragleave="UpFilDragLeave(this); return false;" ondrop="UpFilDrop(this,event,null,null,1);return false;"
               onclick="doGetImage(this, 'AVATAR');" userid="{userBlock/userid}" tab="101000" fid="{userBlock/userid}">
            <img id="UserAvatar" src="{userBlock/avatarurl}" title="{res/n6180}" />
          </div>
        </li>
        <li class="helloMnu">
          <xsl:value-of select="/menu/res/n6181"/>,
        </li>
        <li class="nickMnu" href="#" title="Userid : {userBlock/userid}&#13;Login : {userBlock/userlogin}&#13;Level : {userBlock/userlevel}&#13;Group : {userBlock/usergroupname}&#13;Mail : {userBlock/usermail}">
          <xsl:value-of select="userBlock/username"/>
        </li>

      </ul>

      <!-- Navigation mode fiche ... -->
      <xsl:for-each select="browsingBloc">
        <div id="rightMenuBrowsing" class="rightMenuBrowsing">
          <xsl:for-each select="browsingEntry">
            <ul>
              <xsl:attribute name="class">
              </xsl:attribute>
              <xsl:call-template name="display_browsingEntry" />
            </ul>
          </xsl:for-each>
        </div>
      </xsl:for-each>

      <!-- BLOC DU TITRE EN MODE FICHE-->
      <xsl:for-each select="blocTitle">
        <div class="rightMenuModFile">
          <xsl:for-each select="entry">
            <ul>
              <li>
                <xsl:call-template name="display_Titre" />
              </li>
            </ul>
          </xsl:for-each>
        </div>
      </xsl:for-each>

      <!-- BLOCS DE MENU -->
      <!--CALENDRIER-->
      <xsl:for-each select="blocMenu">
        <xsl:if test="@calendar='1'">
          <br />
          <br />
          <br />
          <br />
          <div class="CalContainer" day="{@day}" month="{@month}" year="{@year}" wd="{@wd}" calmode="{@calmode}" id="Calendar_{@tab}"></div>
          <br />
          <br />
          <br />
          <br />
        </xsl:if>

        <!-- Punaise permettant d'afficher masquer les liens favoris-->
        <xsl:if test="@favlnk!=''">
          <div id="FavLnk" class="FavLnk">
            <div class="pin"></div>
          </div>
        </xsl:if>

        <!-- TITRE (OPTIONNEL) pour une categorie "communication" ... -->
        <div>
          <xsl:attribute name="id">
            FavLnk<xsl:value-of select="position() + 1"/>
          </xsl:attribute>

          <!-- Si liens favoris en page d'accueil  -->
          <xsl:attribute name="class">
            rightMenuOption <xsl:if test="@favlnk!=''">FavLnkBrd</xsl:if> <xsl:if test="@onclick"> rightMenuOptionClickable</xsl:if> <xsl:if test="@selected='1'"> rightMenuOptionSelected</xsl:if>
          </xsl:attribute>
          <xsl:if test="@title!=''">
            <ul class="rightMenuSubMenuTitle">
              <li>
                <xsl:if test="@tooltip">
                  <xsl:attribute name="title">
                    <xsl:value-of select="@tooltip"/>
                  </xsl:attribute>
                </xsl:if>
                <xsl:if test="@onclick">
                  <xsl:attribute name="onclick">
                    <xsl:value-of select="@onclick"/>
                  </xsl:attribute>
                </xsl:if>
                <span>
                  <xsl:attribute name="class">
                    rightMenuSpan_adjust<xsl:if test="@selected='1'"> rightMenuOptionSelected</xsl:if>
                  </xsl:attribute>
                  <!-- IMAGE -->
                  <xsl:if test="@className">
                    <span>
                      <xsl:attribute name="class">
                        <xsl:value-of select="@className"/>
                      </xsl:attribute>
                    </span>
                  </xsl:if>
                  <span>
                    <xsl:if test="@tooltip">
                      <xsl:attribute name="title">
                        <xsl:value-of select="@tooltip"/>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:value-of select="@title"/>
                  </span>
                  <!-- IMAGE "triangle" -->
                  <span>
                    <xsl:attribute name="class">
                      <xsl:choose>
                        <xsl:when test="@caretClassName">
                          <xsl:value-of select="@caretClassName"/>
                        </xsl:when>
                        <xsl:when test="@caretClassName=''"></xsl:when>
                        <xsl:otherwise>icon-caret-down</xsl:otherwise>
                      </xsl:choose>
                    </xsl:attribute>
                  </span>
                </span>
              </li>
            </ul>
          </xsl:if>

          <!-- ELEMENT DE MENU -->
          <xsl:call-template name="splitblockmenu" />
        </div>
      </xsl:for-each>

      <xsl:for-each select="blocThemes">
        <div class="colorPick" id="colorPick">
          <span class="rightMenuSpan_adjust colorTitle">
            <span class="icon-brush"></span>
            <span>
              <xsl:value-of select="/menu/res/n6853"/>
            </span>
          </span>
          <ul>
            <xsl:for-each select="theme">
              <li>
                <xsl:attribute name="class">
                  <xsl:choose>
                    <xsl:when test="../@endmenutype='favlnk'"></xsl:when>
                    <xsl:otherwise>
                      <xsl:if test="position()>0">themeThumbnail</xsl:if>
                      <!-- Si pas de titre et 1er élément, top arrondis  -->
                      <xsl:if test="position()=1">
                        <xsl:choose>
                          <xsl:when test="../@title"></xsl:when>
                          <xsl:otherwise> rightMenuTop</xsl:otherwise>
                        </xsl:choose>
                      </xsl:if>
                      <xsl:if test="activeTheme=1"> activeTheme</xsl:if>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:attribute>
                <xsl:attribute name="style">
                  background-color: <xsl:value-of select="color"/>;
                </xsl:attribute>
                <xsl:call-template name="display_entry" />
              </li>
            </xsl:for-each>
          </ul>
        </div>
      </xsl:for-each>

      <xsl:call-template name="blockStore" />
      <xsl:call-template name="blockuser" />
    </div>

  </xsl:template>

  <!-- TEMPLATE AFFICHAGE ENTREE -->
  <xsl:template name="display_entry">
    <a>
      <!-- ATTRIBUT DU LIEN -->
      <!--  HREF  -->
      <xsl:if test="count(action)=0">
        <xsl:attribute name="href">
          <xsl:choose>
            <!-- LIEN -->
            <xsl:when test="link">
              <xsl:value-of select="link/@prefix"/>
              <xsl:value-of select="link"/>
            </xsl:when>
            <!-- Seulement l'entrée -->
            <xsl:otherwise>
              <xsl:text>#</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <!-- ONCLICK -->
      <xsl:if test="action">
        <xsl:attribute name="onclick">
          <xsl:value-of select="action"/>
        </xsl:attribute>
      </xsl:if>

      <!-- FONTSIZE -->
      <xsl:if test="mxfont">
        <xsl:attribute name="mxfont">
          <xsl:value-of select="mxfont"/>
        </xsl:attribute>
      </xsl:if>

      <!-- ID -->
      <xsl:if test="id">
        <xsl:attribute name="thid">
          <xsl:value-of select="id"/>
        </xsl:attribute>
      </xsl:if>

      <!-- TARGET -->
      <xsl:if test="link/@target!=''">
        <xsl:attribute name="target">
          <xsl:value-of select="link/@target"/>
        </xsl:attribute>
      </xsl:if>
      <!-- FIN DE ATTRTIBUT -->
      <span>
        <xsl:attribute name="class">
          rightMenuSpan_adjust<xsl:if test="@selected='1'"> rightMenuOptionSelected</xsl:if>
        </xsl:attribute>

        <xsl:attribute name="onmouseover">
          st(event, '<xsl:choose>
            <xsl:when test="tooltip">
              <xsl:value-of select="tooltip"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="label"/>
            </xsl:otherwise>
          </xsl:choose>')
        </xsl:attribute>

        <xsl:attribute name="onmouseout">
          ht()
        </xsl:attribute>


        <!-- IMAGE -->
        <xsl:if test="className">
          <xsl:if test="className!='themeThumbnail'">
            <span class="{className}">

            </span>
          </xsl:if>
        </xsl:if>

        <xsl:if test="color2">
          <span class="themeColor2">
            <xsl:attribute name="style">
              border-top-color: <xsl:value-of select="color2"/>;
            </xsl:attribute>
          </span>
        </xsl:if>

        <!-- LABEL -->
        <span>
          <xsl:choose>
            <xsl:when test="className='themeThumbnail'">
              <xsl:attribute name="class">icon-check</xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="label"/>
            </xsl:otherwise>
          </xsl:choose>
        </span>
      </span>
    </a>
    <xsl:if test="idaction!=''">
      <input type="hidden" >
        <!-- Pour avoir une information sur le droit du traitement :utilisée dans le menu gauche Action -->
        <xsl:attribute name="id">
          <xsl:value-of select="idaction"/>
        </xsl:attribute>
        <xsl:attribute name="sact">
          <xsl:value-of select="straction"/>
        </xsl:attribute>
      </input>
    </xsl:if>
  </xsl:template>

  <xsl:template name="display_Titre">
    <a>
      <!-- ATTRIBUT DU LIEN -->
      <!--  ID  -->
      <xsl:attribute name="id">
        <xsl:choose>
          <!-- JAVASCRIPT -->
          <xsl:when test="idaction">
            <xsl:value-of select="idaction"/>
          </xsl:when>
        </xsl:choose>
      </xsl:attribute>
      <!--  HREF  -->
      <xsl:attribute name="onclick">
        <xsl:choose>
          <!-- JAVASCRIPT -->
          <xsl:when test="action">
            <xsl:value-of select="action"/>
          </xsl:when>
        </xsl:choose>
      </xsl:attribute>
      <!-- INFOBULLE -->
      <xsl:attribute name="title">
        <xsl:choose>
          <xsl:when test="tooltip">
            <xsl:value-of select="tooltip"/>
          </xsl:when>
        </xsl:choose>
      </xsl:attribute>
      <!-- IMAGE -->
      <xsl:if test="className">
        <span class="{className}">
        </span>
      </xsl:if>
      <!-- LABEL -->
      <span class="rightMenuSpan_adjust">
        <xsl:value-of select="label"/>
      </span>
    </a>
  </xsl:template>

  <!-- TEMPLATE AFFICHAGE ENTREE -->
  <xsl:template name="display_browsingEntry">
    <xsl:for-each select="entry">
      <li>
        <a id="{id}" eAction="{eAction}" onclick="{action}" >
          <!-- IMAGE -->
          <xsl:if test="className">
            <span class="{className}" />
          </xsl:if>
        </a>
      </li>

    </xsl:for-each>
  </xsl:template>

  <!-- TEMPLATE AFFICHAGE ENTREE MENU -->
  <xsl:template name="splitblockmenu">
    <xsl:variable name="n" select="../NbFavLnkPerCol"/>
    <xsl:for-each select="entry[position() mod number($n) = 1]">
      <!--<xsl:if test="position()!=1">
        <div class="FavLnkColSep"></div>
      </xsl:if>-->
      <ul>
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when test="../@endmenutype='favlnk'">
              rightMenuFavlnk <xsl:if test="position()!=last()">FavLnkColBord</xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="../@endmenutype"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:for-each select=". | following-sibling::entry[position() &lt; number($n)]">
          <li>
            <xsl:attribute name="class">
              <xsl:if test="@selected='1'">rightMenuOptionSelected</xsl:if>
            </xsl:attribute>
            <xsl:call-template name="display_entry" />
          </li>
        </xsl:for-each>
      </ul>
    </xsl:for-each>
  </xsl:template>

  <!-- TEMPLATE MENU EUDOSTORE-->
  <xsl:template name="blockStore" match="storeBlock">
    <xsl:if test="storeBlock/@showStoreMenu='1'">
      <div id="extension-ui">
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when test="storeBlock/@showStoreFileMenu='1'">
              extension-ui-storeFileMenu
            </xsl:when>
            <xsl:otherwise>
              extension-ui-storeMenu
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <div class="leftNavFilter">
          <div class="infoVersion">
            <xsl:attribute name="title">
              <xsl:if test="storeBlock/infoVersionTitle">
                <xsl:value-of select="storeBlock/infoVersionTitle"/>
              </xsl:if>
            </xsl:attribute>
            <div class="infoActive">
              <span class="secondSpanActive">
                <xsl:if test="storeBlock/infoVersionLabel">
                  <xsl:value-of disable-output-escaping="yes" select="storeBlock/infoVersionLabel"/>
                </xsl:if>
              </span>
            </div>
          </div>
          <xsl:choose>
            <xsl:when test="storeBlock/@showStoreFileMenu='1'">
              <div class="rightMenuOption">
                <ul class="hUserBloc">
                  <li class="btnEscMnu" href="#" onclick="nsAdmin.loadAdminModule('ADMIN_EXTENSIONS');">
                    <span class="rightMenuSpan_adjust">
                      <span class="icon-item_rem"></span>
                      <span>
                        <xsl:if test="storeBlock/backToListLabel">
                          <xsl:value-of select="storeBlock/backToListLabel"/>
                        </xsl:if>
                      </span>
                    </span>
                  </li>
                </ul>
              </div>
            </xsl:when>
            <xsl:otherwise>
              <div class="categories">
                <label class="selectLabel" for="">
                  <xsl:if test="storeBlock/categoryTitleLabel">
                    <xsl:value-of select="storeBlock/categoryTitleLabel"/>
                  </xsl:if>
                </label>
                <div class="custom-select">
                  <select data-category="">
                    <option value="all">
                      <xsl:if test="storeBlock/categoryAllLabel">
                        <xsl:value-of select="storeBlock/categoryAllLabel"/>
                      </xsl:if>
                    </option>
                    <option value="none">
                      <xsl:if test="storeBlock/categoryNoneLabel">
                        <xsl:value-of select="storeBlock/categoryNoneLabel"/>
                      </xsl:if>
                    </option>
                    <xsl:for-each select="storeBlock/storeCategories/category">
                      <option>
                        <xsl:attribute name="value">
                          <xsl:value-of select="value"/>
                        </xsl:attribute>
                        <xsl:value-of select="label"/>
                      </option>
                    </xsl:for-each>
                  </select>
                </div>
              </div>
              <div class="categories coloured">
                <label class="selectLabel" for="">
                  <xsl:if test="storeBlock/offerFilterTitleLabel">
                    <xsl:value-of select="storeBlock/offerFilterTitleLabel"/>
                  </xsl:if>
                </label>


                <!-- Offres -->
                <xsl:for-each select="storeBlock/OfferListe/Offer">
                  <div class="componentTypeCheckBox" title="{current()/@xOfferLabel}">
                    <input class="styled-checkbox" 
                           id="Offre{current()/@xOfferType}" 
                           type="checkbox" 
                           onClick="nsAdminStoreMenu.onCheckOffer(this);"
                           data-offer="{current()/@xOfferCatId}" />
                    
                     <label for="Offre{current()/@xOfferType}">
                     <p><xsl:value-of select="current()/@xOfferType"/></p>
                     <img class="{current()/@xOfferCSS}" src="{current()/@xOfferImg}" alt="{current()/@xOfferAlt}" title="{current()/@xOfferLabel}" />            
                    </label>
                  </div>
                </xsl:for-each>
                
                
     
  
              
              </div>

              <div class="categories coloured">
                <label class="selectLabel" for="">
                  <xsl:if test="storeBlock/otherFilterTitleLabel">
                    <xsl:value-of select="storeBlock/otherFilterTitleLabel"/>
                  </xsl:if>
                </label>
                <div class="componentTypeCheckBox">
                  <xsl:attribute name="title">
                    <xsl:if test="storeBlock/otherFilterFreeTitle">
                      <xsl:value-of select="storeBlock/otherFilterFreeTitle"/>
                    </xsl:if>
                  </xsl:attribute>
                  <input class="styled-checkbox" id="FreeCheck" type="checkbox" onClick="nsAdminStoreMenu.onCheckOtherFilter(this);" data-otherFilter="0" />
                  <label for="FreeCheck">
                    <p>
                      <xsl:if test="storeBlock/otherFilterFreeLabel">
                        <xsl:value-of select="storeBlock/otherFilterFreeLabel"/>
                      </xsl:if>
                    </p>
                  </label>
                </div>
                <div class="componentTypeCheckBox compatible">
                  <xsl:attribute name="title">
                    <xsl:if test="storeBlock/otherFilterCompatibleTitle">
                      <xsl:value-of select="storeBlock/otherFilterCompatibleTitle"/>
                    </xsl:if>
                  </xsl:attribute>
                  <input class="styled-checkbox" id="CompatibleCheck" type="checkbox" onClick="nsAdminStoreMenu.onCheckOtherFilter(this);" data-otherFilter="1" />
                  <label for="CompatibleCheck">
                    <p>
                      <xsl:if test="storeBlock/otherFilterCompatibleLabel">
                        <xsl:value-of select="storeBlock/otherFilterCompatibleLabel"/>
                      </xsl:if>
                    </p>
                  </label>
                </div>
                <div class="componentTypeCheckBox">
                  <xsl:attribute name="title">
                    <xsl:if test="storeBlock/otherFilterNewTitle">
                      <xsl:value-of select="storeBlock/otherFilterNewTitle"/>
                    </xsl:if>
                  </xsl:attribute>
                  <input class="styled-checkbox" id="NewCheck" type="checkbox" onClick="nsAdminStoreMenu.onCheckOtherFilter(this);" data-otherFilter="2" />
                  <label for="NewCheck">
                    <p>
                      <xsl:if test="storeBlock/otherFilterNewLabel">
                        <xsl:value-of select="storeBlock/otherFilterNewLabel"/>
                      </xsl:if>
                    </p>
                  </label>
                </div>
              </div>
              <div class="categories coloured">
                <label class="selectLabel" for="">
                  <xsl:if test="storeBlock/statusFilterTitleLabel">
                    <xsl:value-of select="storeBlock/statusFilterTitleLabel"/>
                  </xsl:if>
                </label>
                <div class="ExtensionsTri">

                  <input type="radio" id="radioStoreStatusAll" name="radioStoreStatus" checked="1" onclick="nsAdminStoreMenu.onChangeStatus(this);" data-status="0" />
                  <label for="radioStoreStatusAll">
                    <xsl:attribute name="title">
                      <xsl:if test="storeBlock/statusAllTitle">
                        <xsl:value-of select="storeBlock/statusAllTitle"/>
                      </xsl:if>
                    </xsl:attribute>
                    <xsl:if test="storeBlock/statusAllLabel">
                      <xsl:value-of select="storeBlock/statusAllLabel"/>
                    </xsl:if>
                  </label>

                  <input type="radio" id="radioStoreStatusInstalled" name="radioStoreStatus" onclick="nsAdminStoreMenu.onChangeStatus(this);" data-status="1" />
                  <label for="radioStoreStatusInstalled">
                    <xsl:attribute name="title">
                      <xsl:if test="storeBlock/statusInstalledTitle">
                        <xsl:value-of select="storeBlock/statusInstalledTitle"/>
                      </xsl:if>
                    </xsl:attribute>
                    <xsl:if test="storeBlock/statusInstalledLabel">
                      <xsl:value-of select="storeBlock/statusInstalledLabel"/>
                    </xsl:if>
                    <span class="rond active-exts icon-check-circle">
                      <xsl:attribute name="title">
                        <xsl:if test="storeBlock/statusIconInstalledTitle">
                          <xsl:value-of select="storeBlock/statusIconInstalledTitle"/>
                        </xsl:if>
                      </xsl:attribute>
                    </span>
                  </label>

                  <input type="radio" id="radioStoreStatusInstalling" name="radioStoreStatus" onclick="nsAdminStoreMenu.onChangeStatus(this);" data-status="2" />
                  <label for="radioStoreStatusInstalling">
                    <xsl:attribute name="title">
                      <xsl:if test="storeBlock/statusInstallingTitle">
                        <xsl:value-of select="storeBlock/statusInstallingTitle"/>
                      </xsl:if>
                    </xsl:attribute>
                    <xsl:if test="storeBlock/statusInstallingLabel">
                      <xsl:value-of select="storeBlock/statusInstallingLabel"/>
                    </xsl:if>
                    <span class="rond attente-exts icon-clock-o">
                      <xsl:attribute name="title">
                        <xsl:if test="storeBlock/statusIconInstallingTitle">
                          <xsl:value-of select="storeBlock/statusIconInstallingTitle"/>
                        </xsl:if>
                      </xsl:attribute>
                    </span>
                  </label>

                  <input type="radio" id="radioStoreStatusProposed" name="radioStoreStatus" onclick="nsAdminStoreMenu.onChangeStatus(this);" data-status="3" />
                  <label for="radioStoreStatusProposed">
                    <xsl:attribute name="title">
                      <xsl:if test="storeBlock/statusProposedTitle">
                        <xsl:value-of select="storeBlock/statusProposedTitle"/>
                      </xsl:if>
                    </xsl:attribute>
                    <xsl:if test="storeBlock/statusProposedLabel">
                      <xsl:value-of select="storeBlock/statusProposedLabel"/>
                    </xsl:if>
                  </label>
                </div>
              </div>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- TEMPLATE BLOC USERS -->
  <xsl:template name="blockuser"  match="userBlock">
    <div class="rightMenuOption">
      <ul class="hUserBloc">
        <xsl:if test="@hideuseroptions='0'">
          <li class="btnEscMnu" href="#" onclick="goTabList(-2);">
            <span class="rightMenuSpan_adjust">
              <span class="icon-buzz"></span>
              <span>
                <xsl:value-of select="/menu/res/n7174"/>
              </span>
            </span>
          </li>
        </xsl:if>
        <xsl:if test="@hideadmin='0'">
          <li class="btnEscMnu" href="#" onclick="nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN)">
            <span class="rightMenuSpan_adjust">
              <span class="icon-cogs2"></span>
              <span>
                <xsl:value-of select="/menu/res/n21"/>
              </span>
            </span>
          </li>
        </xsl:if>
        <li class="btnEscMnu" href="#">
          <xsl:choose>
            <xsl:when test="HelpExtranetLink='1'">
              <a onclick="window.open('eSubmitTokenXRM.aspx?helpextranet=1');">
                <span class="rightMenuSpan_adjust">
                  <span class="icon-lightbulb-o"></span>
                  <span>
                    <xsl:value-of select="/menu/res/n7984"/>
                  </span>
                </span>
                <span class="rightMenuSpan_adjust">
                  <!-- IMAGE -->
                  <xsl:if test="className">
                    <span class="{className}" />
                  </xsl:if>
                </span>
              </a>
            </xsl:when>
            <xsl:otherwise>
              <a href="http://www.eudoweb.com/Emailings/Clients/2014/Outils-XRM/docs/Manuel-XRM-express.pdf" target="_blank">
                <span class="rightMenuSpan_adjust">
                  <span class="icon-lightbulb-o"></span>
                  <span>
                    <xsl:value-of select="/menu/res/n6187"/>
                  </span>
                </span>
                <span class="rightMenuSpan_adjust">
                  <!-- IMAGE -->
                  <xsl:if test="className">
                    <span class="{className}" />
                  </xsl:if>
                </span>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </li>
      </ul>
      <xsl:if test="@sso='0'">
        <ul class="hLinks">
          <li class="decBtn" onclick="doDisco();" title="{menu/res/n6179}">
            <span class="icon-logout"></span>
            <xsl:value-of select="/menu/res/n5008"/>
          </li>
        </ul>
      </xsl:if>
      <xsl:if test="@adfs='1'">
        <ul class="hLinks">
          <li class="decBtn" onclick="doDiscoAdfs();" title="{menu/res/n6179}">
            <span class="icon-logout"></span>
            <xsl:value-of select="/menu/res/n5008"/>
          </li>
        </ul>
      </xsl:if>
    </div>
  </xsl:template>

</xsl:stylesheet>