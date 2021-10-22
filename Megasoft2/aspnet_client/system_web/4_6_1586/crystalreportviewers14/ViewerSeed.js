
/*
 * SAP_CR_VIEWER_PREFERRED_LOCALE is added dynamically by CRVRemoteFilter in clientapi app. This variable is not needed in InfoView/CMC case as language parameter
 * is always passed ViewerSeed.js
 */
if(typeof(SAP_CR_VIEWER_PREFERRED_LOCALE) == "undefined") {
    var SAP_CR_VIEWER_PREFERRED_LOCALE = "en-US";
}

if (typeof (SAP) == 'undefined') {
    SAP = {
        common : {
            resourceUri : (function() {
                var scripts = document.getElementsByTagName("script");
                for ( var i = 0; i < scripts.length; i++) {
                    var src = scripts[i].getAttribute("src");
                    if (!src)
                        continue;

                    var matches = src.match(/(.*)ViewerSeed\.js/i);

                    if (matches && matches.length)
                        return matches[1];
                }

                return '';
            })(),
            
            productLocale : (function() {
                var scripts = document.getElementsByTagName("script");
                for ( var i = 0; i < scripts.length; i++) {
                    var src = scripts[i].getAttribute("src");
                    if (!src)
                        continue;

                    var matches = src.match(/(.*)ViewerSeed\.js/i);
                    if (matches && matches.length) {
                        matches = src.match(/\w+=\w+/g);
                        if (matches && matches.length) {
                            var parameters = {};
                            for (var j = 0; j < matches.length; j++) {
                                var t = matches[j].split("=");
                                parameters[t[0]] = t[1];
                            }
                            return parameters['language'];
                        }
                    }
                }
                
                return SAP_CR_VIEWER_PREFERRED_LOCALE;
            })(),

            getServletUri : function() {
                return SAP.common.resourceUri + '../CRVRemoteServlet';
            },

            /**
             * sends asynchronous request to servletUrl
             * 
             * @param args
             *            JSON, name value pair
             * @param sucessCB
             *            callback function that's called on sucess
             * @param failureCB
             *            callback function that's called on failure
             */
            ajax : function(args, sucessCB, failureCB) {
                bobj.crv.AsyncTransporter.getInstance().post(SAP.common.getServletUri(), MochiKit.Base.queryString(args),
                        function(response) { /* on success */
                            if (sucessCB)
                                sucessCB(response.responseText);
                        }, function(response) { /* on fail */
                            var responseText = null;
                            if(response instanceof MochiKit.Async.XMLHttpRequestError)
                                responseText = response.req.responseText;
                            else
                                responseText = response.responseText;
                            if (responseText != null && responseText.length > 0 && failureCB)
                                failureCB(responseText);
                        }, {'Content-Type' : 'application/x-www-form-urlencoded; charset=UTF-8', 'Accept' : 'text/html'});
            },
            init : function() {
                crv_config = {
                    isIncludeDefaultCSS : true,
                    useAsync : true,
                    scriptUri : SAP.common.resourceUri + "js/crviewer/",
                    lang : SAP.common.productLocale,
                    useCompressedScripts : true,
                    apiResources : []
                };

                if (crv_config.useCompressedScripts) {
                    crv_config.apiResources = ['../../api-min.js'];
                } else {
                    crv_config.apiResources = ['../API/BOE.js', '../API/Event.js', '../API/Viewer.js', '../API/Parameter.js',
                            '../API/Exceptions.js','../API/CanvasListener.js', '../API/ThemeManager.js', '../API/ActionListener.js', "../API/ReportSource.js"];
                }
                
                document.write('<script language="javascript" src="' + SAP.common.resourceUri + 'js/crviewer/crv.js"></script>');
            },

            instances : {},

            defineNS : function(name, fn) {
                var names = name.split(".");
                var obj = window;
                for ( var i = 0; i < names.length; i++) {
                    if (obj[names[i]] == null) {
                        if (i < (names.length - 1) || fn == undefined)
                            obj[names[i]] = {};
                        else
                            obj[names[i]] = fn;
                    }

                    obj = obj[names[i]];
                }
            }
        }
    };

    SAP.common.init();
}
