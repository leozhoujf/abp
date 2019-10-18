var abp = abp || {};

(function () {

    abp.appPath = abp.appPath || '/';

    abp.auth = abp.auth || {};

    abp.auth.tokenName = 'Abp.Token';
    abp.auth.tokenExpireDateName = 'Abp.TokenExpireDate';
    abp.auth.tokenTypeName = 'Abp.TokenType';
    abp.auth.tokenHeaderName = 'Authorization';

    abp.auth.setToken = function (authToken, expireDate) {
        localStorage.setItem(abp.auth.tokenName, authToken);
        localStorage.setItem(abp.auth.tokenExpireDateName, expireDate);
    };

    abp.auth.getToken = function() {
        var expireDate = localStorage.getItem(abp.auth.tokenExpireDateName);
        if (new Date(expireDate) > Date.now()) {
            return localStorage.getItem(abp.auth.tokenName);
        }
        abp.auth.clearToken();
        return null;
    };

    abp.auth.setTokenType = function (type) {
        localStorage.setItem(abp.auth.tokenTypeName, type);
    };

    abp.auth.getTokenType = function() {
        return localStorage.getItem(abp.auth.tokenTypeName);
    };

    abp.auth.clearToken = function() {
        localStorage.removeItem(abp.auth.tokenName);
        localStorage.removeItem(abp.auth.tokenExpireDateName);
    };

    /* Swagger */
    abp.swagger = abp.swagger || {};

    function loginUserInternal(tenantId, configObject, callback) {
        var usernameOrEmailAddress = document.getElementById('userName').value;
        if (!usernameOrEmailAddress) {
            alert('Username or Email Address is required, please try with a valid value !');
            return false;
        }

        var password = document.getElementById('password').value;
        if (!password) {
            alert('Password is required, please try with a valid value !');
            return false;
        }

        var xhr = new XMLHttpRequest();

        xhr.onreadystatechange = function () {
            if (xhr.readyState === XMLHttpRequest.DONE) {
                var responseJson = JSON.parse(xhr.responseText);
                if (xhr.status === 200) {
                    var expiresDate = new Date();
                    expiresDate.setSeconds(expiresDate.getSeconds() + responseJson.expires_in);
                    abp.auth.setToken(responseJson.access_token, expiresDate);
                    abp.auth.setTokenType(responseJson.token_type);
                    callback();
                } else {
                    alert(responseJson.error + "\n" + responseJson.error_description);
                }
            }
        };

        xhr.open('POST', configObject.Authority + '/connect/token', true);
        if (tenantId !== null) {
            xhr.setRequestHeader(configObject.TenantKey, tenantId);
        }

        var data = new FormData();
        data.append("client_id", configObject.SwaggerClientId);
        data.append("client_secret", configObject.SwaggerClientSecret);
        data.append("grant_type", "password");
        data.append("scope", configObject.SwaggerScope);
        data.append("username", usernameOrEmailAddress);
        data.append("password", password);
        xhr.send(data);
    };

    abp.swagger.login = function (configObject, callback) {
        //Get TenantId first
        var tenancyName = document.getElementById('tenancyName').value;

        if (tenancyName) {
            var xhrTenancyName = new XMLHttpRequest();
            xhrTenancyName.onreadystatechange = function () {
                if (xhrTenancyName.readyState === XMLHttpRequest.DONE && xhrTenancyName.status === 200) {
                    var responseJson = JSON.parse(xhrTenancyName.responseText);
                    if (responseJson.tenantId !== null) { // Tenant exists and active.
                        loginUserInternal(responseJson.tenantId, configObject, callback); // Login for tenant    
                    } else {
                        alert('There is no such tenant or tenant is not active !');
                    }
                }
            };

            xhrTenancyName.open('GET', '/api/abp/multi-tenancy/tenants/by-name/' + tenancyName, true);
            xhrTenancyName.send();
        } else {
            loginUserInternal(null, configObject, callback); // Login for host
        }
    };

    abp.swagger.logout = function () {
        abp.auth.clearToken();
    }

    abp.swagger.closeAuthDialog = function() {
        if (document.getElementById('abp-auth-dialog')) {
            document.getElementsByClassName("swagger-ui")[1].removeChild(document.getElementById('abp-auth-dialog'));
        }
    };

    abp.swagger.openAuthDialog = function(configObject, loginCallback) {
        abp.swagger.closeAuthDialog();

        var abpAuthDialog = document.createElement('div');
        abpAuthDialog.className = 'dialog-ux';
        abpAuthDialog.id = 'abp-auth-dialog';

        document.getElementsByClassName("swagger-ui")[1].appendChild(abpAuthDialog);

        // -- backdrop-ux
        var backdropUx = document.createElement('div');
        backdropUx.className = 'backdrop-ux';
        abpAuthDialog.appendChild(backdropUx);

        // -- modal-ux
        var modalUx = document.createElement('div');
        modalUx.className = 'modal-ux';
        abpAuthDialog.appendChild(modalUx);

        // -- -- modal-dialog-ux
        var modalDialogUx = document.createElement('div');
        modalDialogUx.className = 'modal-dialog-ux';
        modalUx.appendChild(modalDialogUx);

        // -- -- -- modal-ux-inner
        var modalUxInner = document.createElement('div');
        modalUxInner.className = 'modal-ux-inner';
        modalDialogUx.appendChild(modalUxInner);

        // -- -- -- -- modal-ux-header
        var modalUxHeader = document.createElement('div');
        modalUxHeader.className = 'modal-ux-header';
        modalUxInner.appendChild(modalUxHeader);

        var modalHeader = document.createElement('h3');
        modalHeader.innerText = 'Authorize';
        modalUxHeader.appendChild(modalHeader);

        // -- -- -- -- modal-ux-content
        var modalUxContent = document.createElement('div');
        modalUxContent.className = 'modal-ux-content';
        modalUxInner.appendChild(modalUxContent);

        modalUxContent.onkeydown = function(e) {
            if (e.keyCode === 13) {
                //try to login when user presses enter on authorize modal
                abp.swagger.login(configObject, loginCallback);
            }
        };

        //Inputs
        createInput(modalUxContent, 'tenancyName', 'Tenancy Name (Leave empty for Host)');
        createInput(modalUxContent, 'userName', 'Username or email address');
        createInput(modalUxContent, 'password', 'Password', 'password');

        //Buttons
        var authBtnWrapper = document.createElement('div');
        authBtnWrapper.className = 'auth-btn-wrapper';
        modalUxContent.appendChild(authBtnWrapper);

        //Authorize button
        var authorizeButton = document.createElement('button');
        authorizeButton.className = 'btn modal-btn auth authorize button';
        authorizeButton.innerText = 'Login';
        authorizeButton.style.marginRight = '10px';
        authorizeButton.onclick = function() {
            abp.swagger.login(configObject, loginCallback);
        };
        authBtnWrapper.appendChild(authorizeButton);

        //Close button
        var closeButton = document.createElement('button');
        closeButton.className = 'btn modal-btn auth btn-done button';
        closeButton.innerText = 'Close';
        closeButton.onclick = abp.swagger.closeAuthDialog;
        authBtnWrapper.appendChild(closeButton);
    };

    function createInput(container, id, title, type) {
        var wrapper = document.createElement('div');
        wrapper.className = 'wrapper';
        container.appendChild(wrapper);

        var label = document.createElement('label');
        label.innerText = title;
        wrapper.appendChild(label);

        var section = document.createElement('section');
        section.className = 'block-tablet col-10-tablet block-desktop col-10-desktop';
        wrapper.appendChild(section);

        var input = document.createElement('input');
        input.id = id;
        input.type = type ? type : 'text';
        input.style.width = '100%';

        section.appendChild(input);
    };

})();
