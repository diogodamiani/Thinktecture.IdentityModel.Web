var stsEndpoint = 'https://identity.thinktecture.com/idsrv/issue/oauth2';
var scope = 'https://roadie/webservicesecurity/rest/';
var serviceEndpoint = 'https://roadie/webservicesecurity/rest/identity';
var authN;
var token;

$(document).ready(function () {
    authN = new thinktectureIdentityModel.BrokeredAuthentication(stsEndpoint, scope);
});

function login(username, password) {
    $('#idpToken').val('');
    $('#claims').empty();

    var un = $('#username').val();
    var pw = $('#password').val();

    authN.getIdpToken(un, pw, idpTokenAvailable);
};

function idpTokenAvailable(idpToken) {
    $('#idpToken').val(idpToken);
    token = idpToken;
};

function getIdentity() {
    $('#claims').empty();
    getIdentityClaimsFromService();
};

function getIdentityClaimsFromService() {
    authHeader = authN.createAuthenticationHeader(token);

    $.ajax({
        type: 'GET',
        cache: false,
        url: serviceEndpoint,
        beforeSend: function (req) {
            req.setRequestHeader('Authorization', authHeader);
        },
        success: function (result) {
            //alert(result[0].Value);

            $.each(result, function (key, val) {
                $('#claims').append($('<li>' + val.Value + '</li>'))
            });
        },
        error: function (error) {
            alert('Error: ' + error.responseText);
        }
    });
}