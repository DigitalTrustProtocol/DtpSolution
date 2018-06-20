///<reference path="../typings/globals/jquery/index.d.ts" />

var TrustchainService = (function () {
    function TrustchainService(settings) {
        this.settings = settings;
    }

    TrustchainService.prototype.Query = function (targets) {
        var query = this.BuildQuery(targets);
        return this.PostData('/api/graph/Query', JSON.stringify(query));
    }

    TrustchainService.prototype.BuildQuery = function (targets) {
        var subjects = [];
        var scope = "";
        for (var key in targets) {
            var target = targets[key];
            var subject = { address: target.address };
            subjects.push(subject);
            scope = target.scope;
        }

        if (typeof scope === 'string')
            scope = { value: scope };

        var obj = {
            "issuers": this.settings.publicKeyHash,
            "subjects": subjects,

            // Scope is used to filter on trust resolvement. It can be any text
            "scope": (scope) ? scope : undefined, // The scope could also be specefic to country, area, products, articles or social medias etc.

            // Claim made about the subject. The format is specified by the version property in the header section.
            "types": [
                "binarytrust.tc1"
            ],
            "level": 0,
            //"flags": "LeafsOnly"
        }
        return obj;
    }

    TrustchainService.prototype.GetTrustById = function (id) {
        var url = '/api/trust/get/' + id; // id = encoded byte array

        return this.GetData(url);
    }

    TrustchainService.prototype.GetSimilarTrust = function (trust) {
        var url = '/api/trust/get/?issuer=' + trust.issuer.address + '&subject=' + trust.subject.address + '&type=' + encodeURIComponent(trust.type) + '&scopevalue=' + encodeURIComponent((trust.scope) ? trust.scope.value : "");

        return this.GetData(url);
    }


    TrustchainService.prototype.GetTrustTemplate = function (subject, alias) {
        var url = '/api/trust/build?issuer=' + settings.publicKeyHash + '&subject=' + subject + '&alias=' + alias;

        return this.GetData(url);
    }

    TrustchainService.prototype.PostTrustTemplate = function (trust) {
        return this.PostData('/api/trust/build', JSON.stringify(trust));
    }

    TrustchainService.prototype.PostTrustTemplate = function (package) {
        return this.PostData('/api/package/build', JSON.stringify(package));
    }

    TrustchainService.prototype.PostTrust = function (trust) {
        return this.PostData('/api/trust/add', JSON.stringify(trust));
    }

    TrustchainService.prototype.GetData = function (query) {
        var deferred = $.Deferred();
        var self = this;
        var url = this.settings.infoserver + query;

        $.ajax({
            type: "GET",
            url: url,
            contentType: 'application/json; charset=utf-8',
        }).done(function (msg, textStatus, jqXHR) {
            resolve = msg;
            deferred.resolve(resolve);
        }).fail(function (jqXHR, textStatus, errorThrown) {
            self.TrustServerErrorAlert(jqXHR, textStatus, errorThrown, self.settings.infoserver);
            deferred.fail();
        });

        return deferred.promise();
    }


    TrustchainService.prototype.PostData = function (query, data) {
        var deferred = $.Deferred();
        var self = this;
        var url = this.settings.infoserver + query;

        $.ajax({
            type: "POST",
            url: url,
            data: data,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json'
        }).done(function (msg, textStatus, jqXHR) {
            resolve = msg;
            deferred.resolve(resolve);
        }).fail(function (jqXHR, textStatus, errorThrown) {
            self.TrustServerErrorAlert(jqXHR, textStatus, errorThrown, self.settings.infoserver);
            deferred.fail();
        });

        return deferred.promise();
    }

    TrustchainService.prototype.TrustServerErrorAlert = function (jqXHR, textStatus, errorThrown, server) {
        if (jqXHR.status == 404 || errorThrown == 'Not Found') {
            var msg = 'Error 404: Server ' + server + ' was not found.';
            //alert('Error 404: Server ' + server + ' was not found.');
            console.log(msg);
        }
        else {
            var msg = textStatus + " : " + errorThrown;
            if (jqXHR.responseJSON && jqXHR.responseJSON.ExceptionMessage)
                msg = jqXHR.responseJSON.ExceptionMessage;

            alert(msg);
        }
    }

    return TrustchainService;
}())