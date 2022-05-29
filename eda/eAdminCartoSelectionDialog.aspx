<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminCartoSelectionDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminCartoSelectionDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Administrer la recherche par la sélection cartographique</title>
    <style>
        #container {
            margin: 26px;
            overflow: auto;
        }

        #CartoConfig {
            margin-top: 6px;
            width: 100%;
            overflow: auto;
            font-family: Consolas,monaco,monospace;
            color: #202627;
            border: #b9cccf 1px solid;
        }

        #editor_holder {
            display: block;
            overflow: auto;
            height:660px;
        }
    </style>
  
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
   <%-- <link rel="stylesheet" href="https://netdna.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.0.3/css/font-awesome.css">
    <script src="https://cdn.jsdelivr.net/npm/@json-editor/json-editor/dist/jsoneditor.min.js"></script>--%>

</head>
<body>
    <div id="container">      
        <textarea id="CartoConfig" runat="server" ></textarea>
        <div id="editor_holder"></div>
    </div>
    <script>
        //if (JSONEditor) {
        //    // Set the default CSS theme and icon library globally
        //    JSONEditor.defaults.theme = 'bootstrap3';
        //    JSONEditor.defaults.iconlib = 'fontawesome4';
        //    JSONEditor.defaults.editors.object.options.disable_edit_json = true;
        //    JSONEditor.defaults.editors.object.options.disable_properties = true;
        //    //JSONEditor.defaults.editors.object.options.collapsed = true;
        //    var element = document.getElementById('editor_holder');
        //    var editor = new JSONEditor(element, {
        //        schema: {
        //            "definitions": {},
        //            "$schema": "http://json-schema.org/draft-07/schema#",
        //            "$id": "http://example.com/root.json",
        //            "type": "object",
        //            "format": "categories",                  
        //            "title": "Configuration du widget",
        //            "options": {                       
        //                "disable_collapse": true,
        //                "disable_edit_json": true
        //            },
        //            "required": [
        //              "TableSelectionSection",
        //              "MappingSection",
        //              "FilterSection"
        //            ],
        //            "properties": {
        //                "TableSelectionSection": {
        //                    "$id": "#/properties/TableSelectionSection",
        //                    "type": "object",
        //                    "title": "Choix des tables",
        //                    "options": {                               
        //                        "disable_collapse": true,
        //                        "disable_edit_json": true
        //                    },
        //                    "required": [
        //                      "SourceTab",
        //                      "SelectionTab",
        //                      "DestinationTab",
        //                      "RelationDescId",
        //                      "Options"
        //                    ],
        //                    "properties": {
        //                        "SourceTab": {
        //                            "$id": "#/properties/TableSelectionSection/properties/SourceTab",
        //                            "type": "integer",
        //                            "title": "Table source",
        //                            "options": {
        //                                "disable_collapse": true,
        //                                "disable_edit_json": true
        //                            },
        //                            "default": 0,
        //                            "examples": [
        //                              6100
        //                            ]
        //                        },
        //                        "SelectionTab": {
        //                            "$id": "#/properties/TableSelectionSection/properties/SelectionTab",
        //                            "type": "integer",
        //                            "title": "Table sélection",
        //                            "default": 0,
        //                            "examples": [
        //                              6700
        //                            ]
        //                        },
        //                        "DestinationTab": {
        //                            "$id": "#/properties/TableSelectionSection/properties/DestinationTab",
        //                            "type": "integer",
        //                            "title": "Table destination",
        //                            "default": 0,
        //                            "examples": [
        //                              4200
        //                            ]
        //                        },
        //                        "RelationDescId": {
        //                            "$id": "#/properties/TableSelectionSection/properties/RelationDescId",
        //                            "type": "integer",
        //                            "title": "Relation table destination/source",
        //                            "default": 0,
        //                            "examples": [
        //                              4202
        //                            ]
        //                        },
        //                        "Options": {
        //                            "$id": "#/properties/TableSelectionSection/properties/Options",
        //                            "type": "object",
        //                            "title": "Options",                                   
        //                            "options": {
        //                                "disable_collapse": true,
        //                                "disable_edit_json": true
                                       
        //                            },
        //                            "required": [
        //                              "DefaultMapLocationName",
        //                              "FilterResumeDescId",
        //                              "FilterResumeBreakline"
        //                            ],
        //                            "properties": {
        //                                "DefaultMapLocationName": {
        //                                    "$id": "#/properties/TableSelectionSection/properties/Options/properties/DefaultMapLocationName",
        //                                    "type": "string",
        //                                    "title": "Localisation par nom Ville/Région/Pays",
        //                                    "default": "",
        //                                    "examples": [
        //                                      "Courbevoie"
        //                                    ],
        //                                    "pattern": "^(.*)$"
        //                                },
        //                                "FilterResumeDescId": {
        //                                    "$id": "#/properties/TableSelectionSection/properties/Options/properties/FilterResumeDescId",
        //                                    "type": "integer",
        //                                    "title": "Rubrique pour le résumé des critères",
        //                                    "default": 0,
        //                                    "examples": [
        //                                      6702
        //                                    ]
        //                                },
        //                                "FilterResumeBreakline": {
        //                                    "$id": "#/properties/TableSelectionSection/properties/Options/properties/FilterResumeBreakline",
        //                                    "type": "string",
        //                                    "title": "Retour à la ligne",
        //                                    "default": "",
        //                                    "examples": [
        //                                      "\n"
        //                                    ],
        //                                    "pattern": "^(.*)$"
        //                                }
        //                            }
        //                        }
        //                    }
        //                },
        //                "MappingSection": {
        //                    "$id": "#/properties/MappingSection",
        //                    "type": "object",                          
        //                    "title": "Mise en correspondance",
        //                    "required": [
        //                      "Infobox",
        //                      "Card"
        //                    ],
        //                    "options": {
        //                        "compact": true,
        //                        "disable_collapse": true,
        //                        "disable_edit_json": true
        //                    },
        //                    "properties": {
        //                        "Infobox": {
        //                            "$id": "#/properties/MappingSection/properties/Infobox",
        //                            "type": "object",
        //                            "format": "table",
        //                            "title": "Infobulle de position",
        //                            "required": [
        //                              "Geoloc",
        //                              "Image",
        //                              "Title",
        //                              "SubTitle",
        //                              "Fields"
        //                            ],
        //                            "options": {
        //                                "grid_columns":2, 
        //                                "disable_edit_json": true
        //                            },
        //                            "properties": {
        //                                "Geoloc": {
        //                                    "$id": "#/properties/MappingSection/properties/Infobox/properties/Geoloc",
        //                                    "type": "object",
        //                                    "title": null,
        //                                    "required": [
        //                                      "DescId",
        //                                      "ShowLabel"
        //                                    ],
        //                                    "options": {                                                
        //                                        "disable_collapse": true,
        //                                        "disable_edit_json": true
        //                                    },
        //                                    "properties": {
        //                                        "DescId": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/Geoloc/properties/DescId",
        //                                            "type": "integer",
        //                                            "title": "Rubrique géolocalisation",
        //                                            "default": 0,
        //                                            "examples": [
        //                                              6174
        //                                            ]
        //                                        },
        //                                        "ShowLabel": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/Geoloc/properties/ShowLabel",
        //                                            "type": "boolean",
        //                                            "title": "Utiliser le libellé de la rubrique",
        //                                            "default": false,
        //                                            "format": "checkbox",
        //                                            "examples": [
        //                                              true
        //                                            ],
        //                                            "options": {
        //                                                "hidden": true
        //                                            }
        //                                        }
        //                                    }
        //                                },
        //                                "Image": {
        //                                    "$id": "#/properties/MappingSection/properties/Infobox/properties/Image",
        //                                    "type": "object",
        //                                    "title": " ",                                            
        //                                    "required": [
        //                                      "DescId",
        //                                      "ShowLabel"
        //                                    ],
        //                                    "options": {
        //                                        "compact": true,
        //                                        "disable_collapse": true,
        //                                        "disable_edit_json": true
        //                                    },
        //                                    "properties": {
        //                                        "DescId": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/Image/properties/DescId",
        //                                            "type": "integer",
        //                                            "title": "Rubrique de type image",
        //                                            "default": 0,
        //                                            "examples": [
        //                                              6115
        //                                            ]
        //                                        },
        //                                        "ShowLabel": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/Image/properties/ShowLabel",
        //                                            "type": "boolean",
        //                                            "title": "Utiliser le libellé de la rubrique",
        //                                            "default": false,
        //                                            "format": "checkbox",
        //                                            "examples": [
        //                                              true
        //                                            ],
        //                                            "options": {
        //                                                "hidden": true
        //                                            }
        //                                        }
        //                                    }
        //                                },
        //                                "Title": {
        //                                    "$id": "#/properties/MappingSection/properties/Infobox/properties/Title",
        //                                    "type": "object",
        //                                    "title": " ",
        //                                    "required": [
        //                                      "DescId",
        //                                      "ShowLabel"
        //                                    ],
        //                                    "options": {
        //                                        "compact": true,
        //                                        "disable_collapse": true,
        //                                        "disable_edit_json": true
        //                                    },
        //                                    "properties": {
        //                                        "DescId": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/Title/properties/DescId",
        //                                            "type": "integer",
        //                                            "title": "Rubrique titre",
        //                                            "format": "grid",
        //                                            "default": 0,
        //                                            "examples": [
        //                                              6101
        //                                            ]
        //                                        },
        //                                        "ShowLabel": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/Title/properties/ShowLabel",
        //                                            "type": "boolean",
        //                                            "title": "Utiliser le libellé de la rubrique",
        //                                            "default": false,
        //                                            "format": "checkbox",
        //                                            "examples": [
        //                                              true
        //                                            ]
        //                                        }
        //                                    }
        //                                },
        //                                "SubTitle": {
        //                                    "$id": "#/properties/MappingSection/properties/Infobox/properties/SubTitle",
        //                                    "type": "object",
        //                                    "title": " ",                                           
        //                                    "required": [
        //                                      "DescId",
        //                                      "ShowLabel"
        //                                    ],
        //                                    "options": {
        //                                        "compact":true,
        //                                        "disable_collapse": true,
        //                                        "disable_edit_json": true
        //                                    },
        //                                    "properties": {
        //                                        "DescId": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/SubTitle/properties/DescId",
        //                                            "type": "integer",
        //                                            "title": "Rubrique sous-titre",
        //                                            "default": 0,
        //                                            "examples": [
        //                                              6102
        //                                            ]
        //                                        },
        //                                        "ShowLabel": {
        //                                            "$id": "#/properties/MappingSection/properties/Infobox/properties/SubTitle/properties/ShowLabel",
        //                                            "type": "boolean",
        //                                            "title": "Utiliser le libellé de la rubrique",
        //                                            "format":"checkbox",
        //                                            "default": false,
        //                                            "examples": [
        //                                              true
        //                                            ]
        //                                        }
        //                                    }
        //                                },
        //                                "Fields": {
        //                                    "$id": "#/properties/MappingSection/properties/Infobox/properties/Fields",
        //                                    "type": "array",
        //                                    "title": "The Fields Schema",
        //                                    "items": {
        //                                        "$id": "#/properties/MappingSection/properties/Infobox/properties/Fields/items",
        //                                        "type": "object",
        //                                        "title": "The Items Schema",
        //                                        "required": [
        //                                          "DescId",
        //                                          "ShowLabel"
        //                                        ],
        //                                        "properties": {
        //                                            "DescId": {
        //                                                "$id": "#/properties/MappingSection/properties/Infobox/properties/Fields/items/properties/DescId",
        //                                                "type": "integer",
        //                                                "title": "The Descid Schema",
        //                                                "default": 0,
        //                                                "examples": [
        //                                                  6106
        //                                                ]
        //                                            },
        //                                            "ShowLabel": {
        //                                                "$id": "#/properties/MappingSection/properties/Infobox/properties/Fields/items/properties/ShowLabel",
        //                                                "type": "boolean",
        //                                                "title": "The Showlabel Schema",
        //                                                "default": false,
        //                                                "examples": [
        //                                                  true
        //                                                ]
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        },
        //                        "Card": {
        //                            "$id": "#/properties/MappingSection/properties/Card",
        //                            "type": "object",
        //                            "title": "Mini-fiche",
        //                            "required": [
        //                              "ImageStore",
        //                              "Tabs"
        //                            ],
        //                            "options": {                                       
        //                                "disable_edit_json": true
        //                            },
        //                            "properties": {
        //                                "ImageStore": {
        //                                    "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore",
        //                                    "type": "object",
        //                                    "title": "The Imagestore Schema",
        //                                    "required": [
        //                                      "Source",
        //                                      "Fields"
        //                                    ],
        //                                    "properties": {
        //                                        "Source": {
        //                                            "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore/properties/Source",
        //                                            "type": "object",
        //                                            "title": "The Source Schema",
        //                                            "required": [
        //                                              "DescId",
        //                                              "ShowLabel"
        //                                            ],
        //                                            "properties": {
        //                                                "DescId": {
        //                                                    "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore/properties/Source/properties/DescId",
        //                                                    "type": "integer",
        //                                                    "title": "The Descid Schema",
        //                                                    "default": 0,
        //                                                    "examples": [
        //                                                      6100
        //                                                    ]
        //                                                },
        //                                                "ShowLabel": {
        //                                                    "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore/properties/Source/properties/ShowLabel",
        //                                                    "type": "boolean",
        //                                                    "title": "The Showlabel Schema",
        //                                                    "default": false,
        //                                                    "examples": [
        //                                                      false
        //                                                    ]
        //                                                }
        //                                            }
        //                                        },
        //                                        "Fields": {
        //                                            "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore/properties/Fields",
        //                                            "type": "array",
        //                                            "title": "The Fields Schema",
        //                                            "items": {
        //                                                "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore/properties/Fields/items",
        //                                                "type": "object",
        //                                                "title": "The Items Schema",
        //                                                "required": [
        //                                                  "DescId",
        //                                                  "ShowLabel"
        //                                                ],
        //                                                "properties": {
        //                                                    "DescId": {
        //                                                        "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore/properties/Fields/items/properties/DescId",
        //                                                        "type": "integer",
        //                                                        "title": "The Descid Schema",
        //                                                        "default": 0,
        //                                                        "examples": [
        //                                                          6114
        //                                                        ]
        //                                                    },
        //                                                    "ShowLabel": {
        //                                                        "$id": "#/properties/MappingSection/properties/Card/properties/ImageStore/properties/Fields/items/properties/ShowLabel",
        //                                                        "type": "boolean",
        //                                                        "title": "The Showlabel Schema",
        //                                                        "default": false,
        //                                                        "examples": [
        //                                                          true
        //                                                        ]
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                },
        //                                "Tabs": {
        //                                    "$id": "#/properties/MappingSection/properties/Card/properties/Tabs",
        //                                    "type": "array",
        //                                    "format": "tabs-left",
        //                                    "title": "The Tabs Schema",
        //                                    "items": {
        //                                        "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items",
        //                                        "type": "object",
        //                                        "title": "The Items Schema",
        //                                        "required": [
        //                                          "RelationDescId",
        //                                          "Title",
        //                                          "Fields"
        //                                        ],
        //                                        "properties": {
        //                                            "RelationDescId": {
        //                                                "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/RelationDescId",
        //                                                "type": "integer",
        //                                                "title": "The Relationdescid Schema",
        //                                                "default": 0,
        //                                                "examples": [
        //                                                  6100
        //                                                ]
        //                                            },
        //                                            "Title": {
        //                                                "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/Title",
        //                                                "type": "object",
        //                                                "title": "The Title Schema",
        //                                                "required": [
        //                                                  "DescId",
        //                                                  "ShowLabel"
        //                                                ],
        //                                                "properties": {
        //                                                    "DescId": {
        //                                                        "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/Title/properties/DescId",
        //                                                        "type": "integer",
        //                                                        "title": "The Descid Schema",
        //                                                        "default": 0,
        //                                                        "examples": [
        //                                                          6102
        //                                                        ]
        //                                                    },
        //                                                    "ShowLabel": {
        //                                                        "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/Title/properties/ShowLabel",
        //                                                        "type": "boolean",
        //                                                        "title": "The Showlabel Schema",
        //                                                        "default": false,
        //                                                        "examples": [
        //                                                          true
        //                                                        ]
        //                                                    }
        //                                                }
        //                                            },
        //                                            "Fields": {
        //                                                "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/Fields",
        //                                                "type": "array",
        //                                                "title": "The Fields Schema",
        //                                                "items": {
        //                                                    "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/Fields/items",
        //                                                    "type": "object",
        //                                                    "title": "The Items Schema",
        //                                                    "required": [
        //                                                      "DescId",
        //                                                      "ShowLabel"
        //                                                    ],
        //                                                    "properties": {
        //                                                        "DescId": {
        //                                                            "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/Fields/items/properties/DescId",
        //                                                            "type": "integer",
        //                                                            "title": "The Descid Schema",
        //                                                            "default": 0,
        //                                                            "examples": [
        //                                                              6195
        //                                                            ]
        //                                                        },
        //                                                        "ShowLabel": {
        //                                                            "$id": "#/properties/MappingSection/properties/Card/properties/Tabs/items/properties/Fields/items/properties/ShowLabel",
        //                                                            "type": "boolean",
        //                                                            "title": "The Showlabel Schema",
        //                                                            "default": false,
        //                                                            "examples": [
        //                                                              true
        //                                                            ]
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                },
        //                "FilterSection": {
        //                    "$id": "#/properties/FilterSection",
        //                    "type": "object",
        //                    "title": "The Filtersection Schema",
        //                    "required": [
        //                      "Title",
        //                      "Groups"
        //                    ],
        //                    "options": {                                
        //                        "disable_collapse": true,
        //                        "disable_edit_json": true
        //                    },
        //                    "properties": {
        //                        "Title": {
        //                            "$id": "#/properties/FilterSection/properties/Title",
        //                            "type": "object",
        //                            "title": "The Title Schema",
        //                            "required": [
        //                              "Text"
        //                            ],
        //                            "properties": {
        //                                "Text": {
        //                                    "$id": "#/properties/FilterSection/properties/Title/properties/Text",
        //                                    "type": "string",
        //                                    "title": "The Text Schema",
        //                                    "default": "",
        //                                    "examples": [
        //                                      "Critères de sélection"
        //                                    ],
        //                                    "pattern": "^(.*)$"
        //                                }
        //                            }
        //                        },
        //                        "Groups": {
        //                            "$id": "#/properties/FilterSection/properties/Groups",
        //                            "type": "array",
        //                            "format": "tabs-top",
        //                            "title": "The Groups Schema",
        //                            "items": {
        //                                "$id": "#/properties/FilterSection/properties/Groups/items",
        //                                "type": "object",
        //                                "title": "The Items Schema",
        //                                "required": [
        //                                  "Name",
        //                                  "Description",
        //                                  "Filters"
        //                                ],
        //                                "properties": {
        //                                    "Name": {
        //                                        "$id": "#/properties/FilterSection/properties/Groups/items/properties/Name",
        //                                        "type": "object",
        //                                        "title": "The Name Schema",
        //                                        "required": [
        //                                          "Text"
        //                                        ],
        //                                        "properties": {
        //                                            "Text": {
        //                                                "$id": "#/properties/FilterSection/properties/Groups/items/properties/Name/properties/Text",
        //                                                "type": "string",
        //                                                "title": "The Text Schema",
        //                                                "default": "",
        //                                                "examples": [
        //                                                  "Champs caractères"
        //                                                ],
        //                                                "pattern": "^(.*)$"
        //                                            }
        //                                        }
        //                                    },
        //                                    "Description": {
        //                                        "$id": "#/properties/FilterSection/properties/Groups/items/properties/Description",
        //                                        "type": "object",
        //                                        "title": "The Description Schema",
        //                                        "required": [
        //                                          "Text"
        //                                        ],
        //                                        "properties": {
        //                                            "Text": {
        //                                                "$id": "#/properties/FilterSection/properties/Groups/items/properties/Description/properties/Text",
        //                                                "type": "string",
        //                                                "title": "The Text Schema",
        //                                                "default": "",
        //                                                "examples": [
        //                                                  "Ce group permet de tester les champs caractères"
        //                                                ],
        //                                                "pattern": "^(.*)$"
        //                                            }
        //                                        }
        //                                    },
        //                                    "Filters": {
        //                                        "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters",
        //                                        "type": "array",
        //                                        "title": "The Filters Schema",
        //                                        "items": {
        //                                            "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters/items",
        //                                            "type": "object",
        //                                            "title": "The Items Schema",
        //                                            "required": [
        //                                              "CustomLabel",
        //                                              "UseFieldLabel",
        //                                              "SourceDescId",
        //                                              "View",
        //                                              "Unit"
        //                                            ],
        //                                            "properties": {
        //                                                "CustomLabel": {
        //                                                    "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters/items/properties/CustomLabel",
        //                                                    "type": "object",
        //                                                    "title": "The Customlabel Schema",
        //                                                    "required": [
        //                                                      "Text"
        //                                                    ],
        //                                                    "properties": {
        //                                                        "Text": {
        //                                                            "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters/items/properties/CustomLabel/properties/Text",
        //                                                            "type": "string",
        //                                                            "title": "The Text Schema",
        //                                                            "default": "",
        //                                                            "examples": [
        //                                                              "Libellé personnalisé :"
        //                                                            ],
        //                                                            "pattern": "^(.*)$"
        //                                                        }
        //                                                    }
        //                                                },
        //                                                "UseFieldLabel": {
        //                                                    "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters/items/properties/UseFieldLabel",
        //                                                    "type": "boolean",
        //                                                    "title": "The Usefieldlabel Schema",
        //                                                    "default": false,
        //                                                    "examples": [
        //                                                      false
        //                                                    ]
        //                                                },
        //                                                "SourceDescId": {
        //                                                    "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters/items/properties/SourceDescId",
        //                                                    "type": "integer",
        //                                                    "title": "The Sourcedescid Schema",
        //                                                    "default": 0,
        //                                                    "examples": [
        //                                                      6101
        //                                                    ]
        //                                                },
        //                                                "View": {
        //                                                    "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters/items/properties/View",
        //                                                    "type": "string",
        //                                                    "title": "The View Schema",
        //                                                    "default": "",
        //                                                    "examples": [
        //                                                      "default"
        //                                                    ],
        //                                                    "pattern": "^(.*)$"
        //                                                },
        //                                                "Unit": {
        //                                                    "$id": "#/properties/FilterSection/properties/Groups/items/properties/Filters/items/properties/Unit",
        //                                                    "type": "string",
        //                                                    "title": "The Unit Schema",
        //                                                    "default": "",
        //                                                    "examples": [
        //                                                      ""
        //                                                    ],
        //                                                    "pattern": "^(.*)$"
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    });

        //    var CartoConfig = document.getElementById("CartoConfig");
        //    editor.setValue(JSON.parse(CartoConfig.value));
        //}

    </script>
</body>
</html>
