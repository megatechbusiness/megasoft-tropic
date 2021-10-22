SAP.common.defineNS("SAP.BOE", {
    logon : function(cmsName, username, password, authentication, sucessCB, failureCB) {
        var args = {
            'ServletTask' : 'Authenticate',
            'username' : username,
            'password' : password,
            'cmsName' : cmsName,
            'auth' : authentication
        };

        function onSucess(responseText) {
            var json = MochiKit.Base.evalJSON(responseText);
            if (sucessCB && json)
                sucessCB(json.session);
        }

        function onFailure(responseText) {
            if (failureCB)
                failureCB(responseText);
        }

        SAP.common.ajax(args, onSucess, onFailure);

    }
});