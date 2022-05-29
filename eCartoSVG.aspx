<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eCartoSVG.aspx.cs" Inherits="Com.Eudonet.Xrm.eCartoSVG" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Carte - Répartitions</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <style>
        .map {
            width: 700px;
            margin: auto;
        }
        .mapTooltip {
			position : fixed;
			background-color : #fff;
			moz-opacity:0.70;
			opacity: 0.70;
			filter:alpha(opacity=70);
			border-radius:10px;
			padding : 10px;
			z-index: 1000;
			max-width: 200px;
			display:none;
			color:#343434;
		}
        .areaLegend {
            
        }
        .container {
            max-width: 800px;
            margin:auto;
        }

		body {
			font-family:Helvetica,Arial,sans-serif;
		}
	
		h1 {
			color:#5d5d5d;
			font-size:30px;
		}
    </style>
    <script type="text/javascript">
        $(function () {
            $(".mapcontainer").mapael({
                map: {
                    name: "france_departments",
                    defaultArea: {
                        attrs: {
                            stroke: "#fff",
                            "stroke-width": 1
                        },
                        attrsHover: {
                            "stroke-width": 2
                        },
                        eventHandlers: {
                            //click: function(e, id, mapElem, textElem) {
                            //    var numDepartment = id.replace("department-", "");
                            //    var oExpressFilterMagr = new eUpdater("mgr/eExpressFilterManager.ashx", 0);
                            //    oExpressFilterMagr.ErrorCallBack = function () { }
                            //    oExpressFilterMagr.addParam("tab", "300", "post");
                            //    oExpressFilterMagr.addParam("tabfrom", "300", "post");
                            //    oExpressFilterMagr.addParam("descid", "309", "post");
                            //    oExpressFilterMagr.addParam("q", numDepartment, "post");
                            //    oExpressFilterMagr.addParam("multiple", "0", "post");
                            //    oExpressFilterMagr.send(updateExpressFilterList);
                            //}
                        }
                    }
                },
                legend: {
                    area: {
                        title: "Nombre de fiches Sociétés par département",
                        slices: [
                            {
                                max: 50,
                                attrs: {
                                    fill: "#FFF2B3"
                                },
                                label: "Moins de 50"
                            },
                            {
                                min: 50,
                                max: 150,
                                attrs: {
                                    fill: "#D9E385"
                                },
                                label: "Entre 50 et 150"
                            },
                            {
                                min: 150,
                                max: 300,
                                attrs: {
                                    fill: "#B3C80C"
                                },
                                label: "Entre 150 et 300"
                            },
                            {
                                min: 300,
                                max: 600,
                                attrs: {
                                    fill: "#95AA0F"
                                },
                                label: "Entre 300 et 600"
                            },
                            {
                                min: 600,
                                attrs: {
                                    fill: "#3F5823"
                                },
                                label: "A partir de 600"
                            }
                        ]
                    }
                },
                areas: 
                    <%=_json%>
                
            });
        });
    </script>
</head>
<body class="bodyWithScroll">
    <div class="container">

        <div class="mapcontainer">
            <div class="map">	<span>Carte non disponible</span>

            </div>
            <div class="areaLegend">	<span>Légende non disponible</span>

            </div>
        </div>
    </div>
</body>
</html>
