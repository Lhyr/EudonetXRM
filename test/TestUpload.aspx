<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestUpload.aspx.cs" Inherits="Com.Eudonet.Xrm.TestUpload" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <!--html5demo.css-->
    <style>
        body
        {
            font: normal 16px/20px "Helvetica Neue" , Helvetica, sans-serif;
            background: rgb(237, 237, 236);
            margin: 0;
            margin-top: 40px;
            padding: 0;
        }
        
        section, header, footer
        {
            display: block;
        }
        
        #wrapper
        {
            width: 600px;
            margin: 0 auto;
            background: #fff url(../images/shade.jpg) repeat-x center bottom;
            -moz-border-radius: 10px;
            -webkit-border-radius: 10px;
            border-radius: 10px;
            border-top: 1px solid #fff;
            padding-bottom: 76px;
        }
        
        h1
        {
            padding-top: 10px;
        }
        
        h2
        {
            font-size: 100%;
            font-style: italic;
        }
        
        header, article > *, footer > *
        {
            margin: 20px;
        }
        
        footer > *
        {
            margin: 20px;
            color: #999;
        }
        
        #status
        {
            padding: 5px;
            color: #fff;
            background: #ccc;
        }
        
        #status.fail
        {
            background: #c00;
        }
        
        #status.success
        {
            background: #0c0;
        }
        
        #status.offline
        {
            background: #c00;
        }
        
        #status.online
        {
            background: #0c0;
        }
        
        /*footer #built:hover:after {
          content: '...quickly';
        }
        */
        [contenteditable]:hover:not(:focus)
        {
            outline: 1px dotted #ccc;
        }
        
        abbr
        {
            border-bottom: 0;
        }
        
        abbr[title]
        {
            border-bottom: 1px dotted #ccc;
        }
        
        li
        {
            margin-bottom: 10px;
        }
        
        #promo
        {
            font-size: 90%;
            background: #333 url(../images/learn-js.jpg) no-repeat left center;
            display: block;
            color: #efefef;
            text-decoration: none;
            cursor: pointer;
            padding: 0px 20px 0px 260px;
            border: 5px solid #006;
            height: 160px;
        }
        
        #promo:hover
        {
            border-color: #00f;
        }
        
        #ih5
        {
            font-size: 90%;
            background: #442C0D url(../images/ih5.jpg) no-repeat left center;
            display: block;
            color: #F7FCE4;
            text-decoration: none;
            cursor: pointer;
            padding: 1px 20px 1px 260px;
            border: 5px solid #904200;
        }
        
        #ih5:hover
        {
            border-color: #CF6D3B;
        }
        
        #ffad section
        {
            padding: 20px;
        }
        
        #ffad p
        {
            margin: 10px 10px 10px 100px;
        }
        
        #ffad img
        {
            border: 0;
            float: left;
            display: block;
            margin: 14px 14px 0;
        }
        
        #ffad .definition
        {
            font-style: italic;
            font-family: Georgia,Palatino,Palatino Linotype,Times,Times New Roman,serif;
        }
        
        #ffad .url
        {
            text-decoration: underline;
        }
        
        button, input
        {
            font-size: 16px;
            padding: 3px;
            margin-left: 5px;
        }
        
        #view-source
        {
            display: none;
        }
        
        body.view-source
        {
            margin: 0;
            background: #fff;
            padding: 20px;
        }
        
        body.view-source > *
        {
            display: none;
        }
        
        body.view-source #view-source
        {
            display: block !important;
            margin: 0;
        }
        
        #demos
        {
            width: 560px;
            border-collapse: collapse;
        }
        
        #demos .demo
        {
            padding: 5px;
        }
        
        #demos a
        {
            color: #00b;
            text-decoration: none;
            font-size: 14px;
        }
        
        #demos a:hover
        {
            text-decoration: underline;
        }
        
        #demos tbody tr
        {
            border-top: 1px solid #DCDCDC;
        }
        
        
        #demos .demo p
        {
            margin-top: 0;
            margin-bottom: 5px;
        }
        
        #demos .support
        {
            width: 126px;
        }
        
        .support span
        {
            cursor: pointer;
            overflow: hidden;
            float: left;
            display: block;
            height: 16px;
            width: 16px;
            text-indent: -9999px;
            background-image: url(../images/browsers.gif);
            background-repeat: none;
            margin-right: 5px;
        }
        
        .support span.selected
        {
            outline: 1px dashed #75784C;
        }
        
        .support span.safari
        {
            background-position: 0 0;
        }
        
        .support span.chrome
        {
            background-position: 16px 0;
        }
        
        .support span.firefox
        {
            background-position: 32px 0;
        }
        
        .support span.ie
        {
            background-position: 48px 0;
        }
        
        .support span.opera
        {
            background-position: 64px 0;
        }
        
        .support span.nightly
        {
            opacity: 0.5;
            filter: alpha(opacity=50);
        }
        
        .support span.none
        {
            opacity: 0.1;
            filter: alpha(opacity=10);
        }
        
        #demos .tags
        {
            width: 140px;
        }
        
        .tags span
        {
            font-size: 11px;
            color: #6E724E;
            padding: 2px 5px;
            border: 1px solid #D7D999;
            background: #FFFFC6;
            -moz-border-radius: 10px;
            -webkit-border-radius: 10px;
            border-radius: 10px;
            cursor: pointer;
        }
        
        .tags span:hover, .tags span.selected
        {
            border: 1px solid #75784C;
            background: #FF7;
            color: #333521;
        }
        
        #tags span
        {
            margin-right: 2px;
        }
        
        #tags
        {
            font-size: 14px;
        }
        
        #html5badge
        {
            /*  display: none;*/
            margin-left: -30px;
            border: 0;
        }
        
        #html5badge img
        {
            border: 0;
        }
        
        .support span.yourbrowser
        {
            width: 16px;
            height: 16px;
            background: none;
        }
        
        .support span.yourbrowser.supported
        {
            background: url(../images/yourbrowser.gif) no-repeat top left;
        }
        
        .support span.yourbrowser.not-supported
        {
            background: url(../images/yourbrowser.gif) no-repeat top right;
        }
        
        #carbonads-container
        {
            position: fixed;
            margin-left: 620px;
            margin-top: -2px;
        }
        
        /** Pretty printing styles. Used with prettify.js. */
        pre
        {
            font-size: 14px;
        }
        .str
        {
            color: #080;
        }
        .kwd
        {
            color: #008;
        }
        .com
        {
            color: #800;
        }
        .typ
        {
            color: #606;
        }
        .lit
        {
            color: #066;
        }
        .pun
        {
            color: #660;
        }
        .pln
        {
            color: #000;
        }
        .tag
        {
            color: #008;
        }
        .atn
        {
            color: #606;
        }
        .atv
        {
            color: #080;
        }
        .dec
        {
            color: #606;
        }
    </style>
    <style>
        #holder
        {
            border: 10px dashed #ccc;
            width: 300px;
            min-height: 300px;
            margin: 20px auto;
        }
        #holder.hover
        {
            border: 10px dashed #0c0;
        }
        #holder img
        {
            display: block;
            margin: 10px auto;
        }
        #holder p
        {
            margin: 10px;
            font-size: 14px;
        }
        progress
        {
            width: 100%;
        }
        progress:after
        {
            content: '%';
        }
        .fail
        {
            background: #c00;
            padding: 2px;
            color: #fff;
        }
        .hidden
        {
            display: none !important;
        }
    </style>
    <script language="javascript" type="text/javascript">
        // For discussion and comments, see: http://remysharp.com/2009/01/07/html5-enabling-script/
        /*@cc_on'abbr article aside audio canvas details figcaption figure footer header hgroup mark menu meter nav output progress section summary time video'.replace(/\w+/g, function (n) { document.createElement(n) })@*/

        var addEvent = (function () {
            if (document.addEventListener) {
                return function (el, type, fn) {
                    if (el && el.nodeName || el === window) {
                        el.addEventListener(type, fn, false);
                    } else if (el && el.length) {
                        for (var i = 0; i < el.length; i++) {
                            addEvent(el[i], type, fn);
                        }
                    }
                };
            } else {
                return function (el, type, fn) {
                    if (el && el.nodeName || el === window) {
                        el.attachEvent('on' + type, function () { return fn.call(el, window.event); });
                    } else if (el && el.length) {
                        for (var i = 0; i < el.length; i++) {
                            addEvent(el[i], type, fn);
                        }
                    }
                };
            }
        })();

        (function () {

            var pre = document.createElement('pre');
            pre.id = "view-source"

            // private scope to avoid conflicts with demos
            addEvent(window, 'click', function (event) {
                if (event.target.hash == '#view-source') {
                    // event.preventDefault();
                    if (!document.getElementById('view-source')) {
                        // pre.innerHTML = ('<!DOCTYPE html>\n<html>\n' + document.documentElement.innerHTML + '\n</html>').replace(/[<>]/g, function (m) { return {'<':'&lt;','>':'&gt;'}[m]});
                        var xhr = new XMLHttpRequest();

                        // original source - rather than rendered source
                        xhr.onreadystatechange = function () {
                            if (this.readyState == 4 && this.status == 200) {
                                pre.innerHTML = this.responseText.replace(/[<>]/g, function (m) { return { '<': '&lt;', '>': '&gt;'}[m] });
                                prettyPrint();
                            }
                        };

                        document.body.appendChild(pre);
                        // really need to be sync? - I like to think so
                        xhr.open("GET", window.location, true);
                        xhr.send();
                    }
                    document.body.className = 'view-source';

                    var sourceTimer = setInterval(function () {
                        if (window.location.hash != '#view-source') {
                            clearInterval(sourceTimer);
                            document.body.className = '';
                        }
                    }, 200);
                }
            });

        })();
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <!--
        <section id="wrapper">
            <div id="carbonads-container">
                <div class="carbonad">
                    <div id="azcarbon"></div>
                    <script type="text/javascript">                        var z = document.createElement("script"); z.type = "text/javascript"; z.async = true; z.src = "http://engine.carbonads.com/z/14060/azcarbon_2_1_0_VERT"; var s = document.getElementsByTagName("script")[0]; s.parentNode.insertBefore(z, s);</script>
                </div>
            </div>
            <header>
                <h1>Drag and drop, automatic upload</h1>
            </header>
            -->
        <article>
                <div id="holder">
                </div> 
                <p id="upload" class="hidden"><label>Drag & drop not supported, but you can still upload via this input field:<br><input type="file"></label></p>
                <p id="filereader">File API & FileReader API not supported</p>
                <p id="formdata">XHR2's FormData is not supported</p>
                <p id="progress">XHR2's upload progress isn't supported</p>
                <p>Upload progress: <progress id="uploadprogress" min="0" max="100" value="0">0</progress></p>
                <p>Drag an image from your desktop on to the drop zone above to see the browser both render the preview, but also upload automatically to this server.</p>
            </article>
        <!--
        </section>
        -->
    </div>
    </form>
    <script>

        var holder = document.getElementById('holder'),
    tests = {
        filereader: typeof FileReader != 'undefined',
        dnd: 'draggable' in document.createElement('span'),
        formdata: !!window.FormData,
        progress: "upload" in new XMLHttpRequest
    },
    support = {
        filereader: document.getElementById('filereader'),
        formdata: document.getElementById('formdata'),
        progress: document.getElementById('progress')
    },
    acceptedTypes = {
        'image/png': true,
        'image/jpeg': true,
        'image/gif': true
    },
    progress = document.getElementById('uploadprogress'),
    fileupload = document.getElementById('upload');

        "filereader formdata progress".split(' ').forEach(function (api) {
            if (tests[api] === false) {
                support[api].className = 'fail';
            } else {
                // FFS. I could have done el.hidden = true, but IE doesn't support
                // hidden, so I tried to create a polyfill that would extend the
                // Element.prototype, but then IE10 doesn't even give me access
                // to the Element object. Brilliant.
                support[api].className = 'hidden';
            }
        });

        function previewfile(file) {
            if (tests.filereader === true && acceptedTypes[file.type] === true) {
                var reader = new FileReader();
                reader.onload = function (event) {
                    var image = new Image();
                    image.src = event.target.result;
                    image.width = 250; // a fake resize
                    holder.appendChild(image);
                };

                reader.readAsDataURL(file);
            } else {
                holder.innerHTML += '<p>Uploaded ' + file.name + ' ' + (file.size ? (file.size / 1024 | 0) + 'K' : '');
                console.log(file);
            }
        }

        function readfiles(files) {
            var formData = tests.formdata ? new FormData() : null;
            for (var i = 0; i < files.length; i++) {
                if (tests.formdata) formData.append('file', files[i]);
                previewfile(files[i]);
            }

            // now post a new XHR request
            if (tests.formdata) {
                var xhr = new XMLHttpRequest();
                xhr.open('POST', 'TestUpload.aspx');
                xhr.onload = function () {
                    progress.value = progress.innerHTML = 100;
                };

                if (tests.progress) {
                    xhr.upload.onprogress = function (event) {
                        if (event.lengthComputable) {
                            var complete = (event.loaded / event.total * 100 | 0);
                            progress.value = progress.innerHTML = complete;
                        }
                    }
                }

                //                xhr.send("action=POUET&" + formData);
                xhr.send(formData);
            }
        }

        if (tests.dnd) {
            holder.ondragover = function () { this.className = 'hover'; return false; };
            holder.ondragend = function () { this.className = ''; return false; };
            holder.ondrop = function (e) {
                this.className = '';
                e.preventDefault();
                readfiles(e.dataTransfer.files);
            }
        } else {
            fileupload.className = 'hidden';
            fileupload.querySelector('input').onchange = function () {
                readfiles(this.files);
            };
        }


    </script>
</body>
</html>
