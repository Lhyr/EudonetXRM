<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eMainWebTab.aspx.cs" Inherits="Com.Eudonet.Xrm.eMainWebTab"
    EnableSessionState="true" EnableViewState="false" %>

<div id="mainListContent">
    <div id="infos">
        <table class="infos" cellpadding="0" cellspacing="0">
            <tr>                    
                <td>
                    <div id="SubTabMenuCtnr" runat="server" class="subTabDiv">
                    </div>
                </td>              
                <td>
                    <div class="xrm-grid-options" onclick="oGridToolbar.click(event);">
                        <span action="refresh" class="icon-refresh"></span>                                        
                        <span action="options" class="icon-cog"></span> 
                    </div>
                </td>                
            </tr>
        </table>
    </div>   
</div>
<div class="listheader" id="listheader" runat="server"></div>
