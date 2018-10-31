//Our Generic and reusable Javascripts should go here.
var ProjectKeys = ProjectKeys || (ProjectKeys = {});
(function (keys) {

    //use when looking for an index when you only have a key instead of the item
    //for more info - http://stackoverflow.com/questions/6926155/how-to-use-indexof-in-knockoutjs
    keys.arrayFirstIndexOf = function (array, predicate, predicateOwner) {
        for (var i = 0, j = array.length; i < j; i++) {
            if (predicate.call(predicateOwner, array[i])) {
                return i;
            }
        }
        return -1;
    };

    //use in conjunction with bootstrap modal. make sure you have an observable named pickedTemplate on your $root context. You will write your template's name on that observable. The data passed to the template is the valueAccessor you pass to this custom binding.
    ko.bindingHandlers.showModal = {
        init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            var modal = valueAccessor();

            //initialize modal, then clear the observable when the modal closes
            $(element).modal({ show: false, backdrop: 'static' })
                .on("hidden.bs.modal", function () {
                    if (ko.isWriteableObservable(modal)) {
                        modal(null);
                    }
                });

            var templateName = function () {
                var value = bindingContext.$root.pickedTemplate();
                console.log(value);
                return value;
            };

            var templateData = ko.computed({
                read: function () {
                    console.log(modal);
                    return modal;
                },
                disposeWhenNodeIsRemoved: element
            });

            return ko.applyBindingsToNode(element, { template: { if: modal, name: templateName, data: templateData } }, bindingContext);
        },

        update: function (elem, valueAccessor) {
            var value = ko.unwrap(valueAccessor());
            console.log(value);
            $(elem).modal(value ? "show" : "hide");
        }
    };

})(ProjectKeys);

$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;

    return true;
}

function onSavedSuccessfully(msg, callback) {
    // reset original state
    $("#saved").css("left", "42%");
    $("#saved").css("opacity", "1");

    if (msg == "" || msg == null) {
        $("#saved").fadeIn(1000, function () {
            $("#saved").animate({
                opacity: 0,
                left: "-=500"
            }, 800, "easeInQuart", function () {
                $("#saved").hide();
                if (callback != undefined && callback != null) {
                    callback();
                }
            });
        });
    } else {
        alert(msg);
        if (callback != undefined && callback != null) {
            callback();
        }
    }
}

function inIframe() {
    try {
        return window.self !== window.top;
    } catch (e) {
        return true;
    }
}

function setupAjax() {

    $.ajaxSetup({
        cache: false,
        beforeSend: function (xhr) {
            if (typeof showLoading == 'function') {
                showLoading(true);
            } else if (parent.showLoading) {
                parent.showLoading(true);
            }

            setTimeout(function () { showLoading(false); }, 5000);
        },
        complete: function (xhr, status) {
            if (typeof showLoading == 'function') {
                showLoading(false);
            } else if (parent.showLoading) {
                parent.showLoading(false);
            }
        }
    });
}

function defaultAjax() {
    // silently refresh - no loading sign.
    $.ajaxSetup({
        cache: false,
        beforeSend: function (xhr) {
        },
        complete: function (xhr, status) {
        }
    });
}

function showLoading(show) {

    if (show) {
        $("#loader").fadeIn(100);
    } else {
        $("#loader").hide();
    }
}

$(document).ready(function () {
    setupAjax();
});
