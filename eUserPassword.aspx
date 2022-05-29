<%@ Import Namespace="Com.Eudonet.Xrm" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eUserPassword.aspx.cs"
    EnableSessionState="true" EnableViewState="false" Inherits="Com.Eudonet.Xrm.eUserPassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Eudonet XRM</title>
    <meta http-equiv="Expires" content="0" />
    <meta http-equiv="Cache-Control" content="no-cache" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="shortcut icon" type="image/x-icon" href="themes/default/images/favicon.ico" />
    <link rel="stylesheet" type="text/css" href="themes/default/css/eAdmin.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />
    <link rel="stylesheet" type="text/css" href="themes/default/css/eudoFont.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />
    <script type="text/javascript" language="javascript" src="scripts/eLogin.js?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
</head>

<%= strPageContents %>