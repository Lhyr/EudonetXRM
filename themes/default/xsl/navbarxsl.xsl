<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html"/>
  <xsl:template match="navbar">
    <div id="navigTab" class="hNavigTab">
      <xsl:call-template name="BoucleNavTab">
        <xsl:with-param  name="debut" select="1"/>
        <xsl:with-param name="fin" select="@nbPageTab" />
      </xsl:call-template>
    </div>
    <div id="nav" class="nav" onkeydown="onNavBarKeyDown(event, null);">
      <ul id="menuNavBar" class="navBar {@fontsize}" nbTab="{@nbPageTab}">
        <!-- MENU DES ONGLETS -->
        <xsl:for-each select="tab">
          <li onmouseover="focusSearch({@descid},event);" onmouseout="unFocusSearch({@descid});" touchend="" id="nvb{@descid}" edntype="{@type}" autocomplete="off">
            <xsl:attribute name="class">
              navEntry <xsl:if test="@ednTabPage&gt;1"> tab-hidden </xsl:if>
            </xsl:attribute>
            <xsl:attribute name="ednTabPage">
              <xsl:value-of select="@ednTabPage"/>
            </xsl:attribute>
            <xsl:if test="@tooltiptextenabled='1' and string-length(@tooltiptext)!=0">
              <xsl:attribute name="title">
                <xsl:value-of select="@tooltiptext"/>
              </xsl:attribute>
            </xsl:if>

            <!--  Titre -->
            <div id="tab_header_{@descid}" >
              <xsl:attribute name="onclick">
                <xsl:choose>
                  <xsl:when test="@type='19' or @type='24'">
                    javascript:
                    <!--  Titre oWebMgr.loadTab -->
                    oGridManager.loadTabGrid( <xsl:value-of select="@descid"/>);
                  </xsl:when>
                  <xsl:otherwise>
                    javascript:onTabSelect( <xsl:value-of select="@descid"/>, true);
                  </xsl:otherwise>
                </xsl:choose>

              </xsl:attribute>

              <xsl:attribute name="class">
                <xsl:choose>
                  <xsl:when test="@active">navTitleActive</xsl:when>
                  <xsl:otherwise>navTitle</xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
              <xsl:value-of select="@label"/>
            </div>

            <!--  Sous Menus -->
            <xsl:if test="admin or @descid > 0">
              <xsl:if test="mrus or actions">
                <ul>
                  <xsl:attribute name="class">
                    <xsl:choose>
                      <xsl:when test="@active">sbMenuActive</xsl:when>
                      <xsl:otherwise>sbMenu</xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                  <!--  MRUs -->
                  <xsl:choose>
                    <xsl:when test="mrus">
                      <xsl:if test="@type!=1">
                        <li class="sbmSearch">
                          <ul>
                            <li class="navSearchInpt"  >
                              <input type="text" id="mru_search_{@descid}" onkeyup="launchNavBarSearch(this.value  , event ,{@descid} );" onclick="focusSearch({@descid}, event);" maxlength="100" autocomplete="off">
                                <!-- #62 037 - le focus/select est désormais géré via focusSearch() et focusOrSelect() sous conditions -->
                                <!--<xsl:attribute name="onfocus">this.select();</xsl:attribute>-->
                              </input>
                            </li>
                            <li id="mru_search_btn_{@descid}" class="icon-magnifier srchFldImg">
                            </li>
                          </ul>
                        </li>
                      </xsl:if>
                      <li class="sbmMRU">

                        <ul id="ul_mru_{@descid}" class="listResult_sMC2">
                        </ul>

                      </li>
                    </xsl:when>
                    <xsl:otherwise>
                      <li class="sbmFakeSearch">
                        <ul>
                          <li class="navSearchInpt"  >
                            <input type="text" id="mru_fakesearch_{@descid}" onclick="focusSearch({@descid}, event);" autocomplete="off">
                              <!-- #62 037 - le focus/select est désormais géré via focusSearch() et focusOrSelect() sous conditions -->
                              <!--<xsl:attribute name="onfocus">this.select();</xsl:attribute>-->
                            </input>
                          </li>
                        </ul>
                      </li>
                    </xsl:otherwise>
                  </xsl:choose>
                  <!--<li class="navSep"></li>-->
                  <xsl:if test="actions">
                    <li class="sbmA">
                      <ul class="Action_sMC">
                        <xsl:apply-templates select="actions"/>
                      </ul>
                    </li>
                  </xsl:if>
                  <!--  ADMIN -->
                  <xsl:choose>
                    <xsl:when test="admin">
                      <li class="sbmBottom">
                        <xsl:attribute name="onclick">
                          <xsl:choose>
                            <xsl:when test="@descid=0">
                              javascript:nsAdmin.loadAdminModule('ADMIN');
                            </xsl:when>
                            <xsl:otherwise>
                              javascript:nsAdmin.loadAdminFile( <xsl:value-of select="@descid"/>);
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:attribute>
                        <p>
                          <span class="icon-cog"></span>
                          <span class="linkAdmin">
                            <a>
                              <xsl:value-of select="admin/@title"/>
                            </a>
                          </span>
                        </p>
                      </li>
                    </xsl:when>
                    <xsl:otherwise>
                      <li class="bottomLine">
                        <p>
                        </p>
                      </li>
                    </xsl:otherwise>
                  </xsl:choose>
                </ul>
              </xsl:if>
            </xsl:if>
          </li>
        </xsl:for-each>
        <!-- BOUTON PLUS -->
        <li class="top-p navEntry has-sub">
          <div class="navTitle" >+</div>
          <ul id="subPLUS">
            <xsl:attribute name="class">
              <xsl:choose>
                <xsl:when test="@nbTabs &gt; 1">sbMenu sbMenuDPlus</xsl:when>
                <xsl:otherwise>sbMenu</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <li class="sbmFakeSearch">
              <ul>
                <li class="navSearchInpt"  >
                  <input type="text" id="mru_fakesearch_PLUS" onclick="focusSearch(0{@descid}, event);" autocomplete="off">
                    <!-- #62 037 - le focus/select est désormais géré via focusSearch() et focusOrSelect() sous conditions -->
                    <!--<xsl:attribute name="onfocus">this.select();</xsl:attribute>-->
                  </input>
                </li>
              </ul>
            </li>
            <!-- ONGLETS -->
            <!-- VUES -->
            <li class="sbmMRU" >
              <ul id="ul_viewtab" >
                <xsl:apply-templates select="plus/views"/>
              </ul>
            </li>
            <!-- lien création de nouvelles vues/choix des onglets-->
            <li class="smbPlus">
              <ul  class ="Action_sMC">
                <!-- Ajouter/modifier liste tab -->
                <xsl:apply-templates select="plus/tabs"/>
              </ul>
            </li>

            <!--  ADMIN -->
            <xsl:choose>
              <xsl:when test="admin">
                <li class="bottomLine">
                  <p>
                    <img alt="{@title}" src="themes/default/images/eMain_admin.png"  width="16" height="16" />
                    <a onclick="alert('ADMIN');">
                      <xsl:value-of select="admin/@title"/>
                    </a>
                  </p>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <li class="bottomLine">
                  <p>
                  </p>
                </li>
              </xsl:otherwise>
            </xsl:choose>
          </ul>
        </li>
      </ul>
    </div>
  </xsl:template>
  <!-- Affichage des boutons d'action -->
  <xsl:template match="action">
    <li class="navAction" onclick="javascript:{action}">
      <xsl:value-of select="label"/>
    </li>
  </xsl:template>
  <xsl:template name="BoucleNavTab">
    <xsl:param name="debut"/>
    <xsl:param name="fin"/>
    <xsl:if test="$fin &gt; 1" >
      <span id="switch{$debut}" class="icon-circle-thin imgInact"></span>
    </xsl:if>
    <xsl:if test="$debut &lt; $fin" >
      <xsl:call-template name="BoucleNavTab">
        <xsl:with-param  name="debut" select="number($debut)+1"/>
        <xsl:with-param name="fin" select="$fin" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>