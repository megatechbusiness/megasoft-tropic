if (typeof (bobj.crv.AsyncTransporter) == 'undefined') {
    bobj.crv.AsyncTransporter = {};
}

/**
 * Get a shared AsyncTransporter instance
 */
bobj.crv.AsyncTransporter.getInstance = function() {
    if (!bobj.crv.AsyncTransporter.__instance) {
        bobj.crv.AsyncTransporter.__instance = new function() {
            var iframe = null;
            var RID = 0;
            var transporterURL = bobj.crvUri("../../html/transporter.html");
            var requests = {};
            var isIframeLoaded = false;
            var messageQueue = [];

            function initIframe() {
                /**
                 * Creates iframe on initialization if not existing
                 */
                if (!iframe) {
                    iframe = MochiKit.DOM.createDOM("IFRAME");
                    iframe.style.width = "1px";
                    iframe.style.height = "1px";
                    iframe.style.position = "absolute";
                    iframe.style.top = "-2px";
                    iframe.style.left = "-2px";

                    iframe.frameborder = "0";
                    iframe.scrolling = "no";
                    iframe.allowtransparency = "true";
                    iframe.src = transporterURL;
                    
                    function addIframe() {
                        if (document.body.firstChild)
                            document.body.insertBefore(iframe, document.body.firstChild);
                        else
                            document.body.appendChild(iframe);
                        
                        bobj.connectDOMEvent(iframe, "onload", function() {
                            bobj.disconnectDOMEvent(iframe, "onload", arguments.callee);
                            setIsIFrameLoaded();
                        });   
                    }
                    if(document.body) {
                        addIframe();
                    }
                    else {
                        bobj.connectDOMEvent(window, "onload", function() {
                            addIframe();
                        });
                    }
                }
            }

            function postMessage(method, id, methodArgs) {
                initIframe();
                var message = {
                    method : method,
                    RID : id
                };
                message.methodArgs = methodArgs ? methodArgs : "";

                var messageStr = MochiKit.Base.serializeJSON(message);

                if (!isIframeLoaded) {
                    messageQueue.push(messageStr);
                } else {
                    iframe.contentWindow.postMessage(messageStr, iframe.src);
                }
            }

            function setIsIFrameLoaded() {
                isIframeLoaded = true;
                processQueue();
            }

            /**
             * Processs queue of unsent messages. If iframe is not ready for postMessage, message is queued
             */
            function processQueue() {
                for ( var i = 0; i < messageQueue.length; i++)
                    iframe.contentWindow.postMessage(messageQueue[i], iframe.src);

                messageQueue = [];
            }

            /**
             * Listens for messages on window
             */
            bobj.connectDOMEvent(window, "onmessage", function(ev) {
                if (transporterURL.toLowerCase().indexOf(ev.origin.toLowerCase()) >= 0) {
                    try {
                                                    
                        var rawData = ev.data;
                        var firstBar = rawData.indexOf("|");
                        var secondBar = rawData.indexOf("|", firstBar + 1);
                        var status = parseInt(rawData.substring(0, firstBar));
                        var RID = parseInt(rawData.substring(firstBar + 1, secondBar));
                        var responseText = rawData.substring(secondBar + 1);

                        var request = requests[RID];
                        var response = new function() {
                            this.responseText = responseText;
                        };
                        if (status == 200 || status == 304)
                            request.onsuccess(response);
                        else
                            request.onfail(response);
                    } catch (e) {
                        throw {
                            message : "unable to process async respons",
                            innerException : e
                        };// FIXME localize
                    }
                }
            });

            function sendXDomainRequest(method, url, args, onSuccess, onFail, headers) {                
                var _myID = RID++;
                var _successCB = onSuccess;
                var _failCB = onFail;

                var postFunc = function() { 
                    postMessage(method, _myID, {
                        url : url,
                        args : args,
                        headers : headers
                    });
                };
                
                if (_ie)
                    setTimeout(postFunc,0);
                else
                    postFunc();

                var deferred = new function() {
                    this.cancel = function() {
                        postMessage("cancel", _myID);
                    };

                    this.addCallback = function(fn) {
                        _successCB = fn;
                    };

                    this.addErrback = function(fn) {
                        _failCB = fn;
                    };

                    this.onsuccess = function(req) {
                        if (_successCB)
                            _successCB(req);
                    };

                    this.onfail = function(req) {
                        if (_failCB)
                            _failCB(req);
                    };

                };

                requests[_myID] = deferred;

                return deferred;
            }
            
            function isCrossDomainRequest(url) {
                var xDomainRegex = /^(\w+:)?\/\/([^\/?#]+)/
                var urlSeq = xDomainRegex.exec(url);

                return urlSeq != null && (urlSeq[1] && urlSeq[1] !== location.protocol || urlSeq[2] !== location.host);
            }

            /**
             * @param url
             *            [string] url to post to
             * @param args
             *            [string] params formated as querystring name=value&name=value
             * @param onSuccess
             *            [function] callback
             * @param onFail
             *            [function] callback
             * @param headers
             *            [JSON] Name/Value pair of request headers
             * 
             */
            this.post = function(url, args, onSuccess, onFail, headers) {
                if (isCrossDomainRequest(url))
                    return sendXDomainRequest("POST", url, args, onSuccess, onFail, headers);
                else {
                    var req = MochiKit.Async.getXMLHttpRequest();
                    req.open("POST", url, true);
                    if(headers != null) {
                        for(var hName in headers)
                            req.setRequestHeader(hName, headers[hName]);
                    }
                    var deferred = MochiKit.Async.sendXMLHttpRequest(req, args);
                    if (onSuccess)
                        deferred.addCallback(onSuccess);
                    if (onFail)
                        deferred.addErrback(onFail);

                    return deferred;
                }
            };

            /**
             * @param url
             *            [string] url to post to
             * @param args
             *            [string] params formated as querystring name=value&name=value
             * @param successCB
             *            [function] callback
             * @param failCB
             *            [function] callback
             * @param headers
             *            [JSON] Name/Value pair of request headers
             */
            this.get = function(url, args, onSuccess, onFail, headers) {
                if (isCrossDomainRequest(url))
                    return sendXDomainRequest("GET", url, args, onSuccess, onFail, headers);
                else {
                    var req = MochiKit.Async.getXMLHttpRequest();
                    if (args != null)
                        url += "?" + args;

                    req.open("GET", url, true);
                    
                    if(headers != null) {
                        for(var hName in headers)
                            req.setRequestHeader(hName, headers[hName]);
                    }
                    var deferred = MochiKit.Async.sendXMLHttpRequest(req, args);
                    if (onSuccess)
                        deferred.addCallback(onSuccess);
                    if (onFail)
                        deferred.addErrback(onFail);

                    return deferred;
                }
            };
        };
    }

    return bobj.crv.AsyncTransporter.__instance;
};
