// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


jQuery("[data-showwhen-questionid]").each(function (s, e) {

    var $this = jQuery(this);
    $this.hide();

    var questionToCheck = $this.data("showwhen-questionid");
    var valToCheck = $this.data("showwhen-value").toString();

    jQuery("[name=" + questionToCheck + "]").on("change", function () {
        var answer = jQuery(this).val();
        showWhen($this, answer, valToCheck);
    });

});

function showWhen($this, answer, valToCheck) {
    if (valToCheck !== "" && answer === valToCheck) {
        $this.show();
    } else {
        $this.hide();
    }
}

function checkForDynamicQuestions() {
    jQuery("[data-showwhen-questionid]").each(function (s, e) {

        var $this = jQuery(this);
        var childId = $this.attr("id");
        var parentQuestionId = $this.data("showwhen-questionid");
        var parentAnswerToCheckFor = $this.data("showwhen-value").toString();
        var parentType = $("[name=" + parentQuestionId + "]").attr("type");
        var parentAnswer;

        switch (parentType) {
            case "radio":
            case "checkbox":
                parentAnswer = jQuery("[name=" + parentQuestionId + "]:checked").val();
                break;
            case "text": parentAnswer = jQuery("[name=" + parentQuestionId + "]").val();
                break;
        }

        if (parentAnswerToCheckFor === parentAnswer) {
            jQuery("#" + childId).show();
        }

    });
}


(function () {

    checkForDynamicQuestions();

    var cookieSeen = getCookie("gfc-cookie-info-seen");
    if (cookieSeen !== "true") {
        jQuery("#global-cookie-message").show();
    }

    jQuery("#cookie-dismiss-button").on("click", function () {
        setCookie("gfc-cookie-info-seen", "true", 365);
        jQuery("#global-cookie-message").hide();
    });

})();


function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(";");
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === " ") c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

//Function to enable javascript window-close button
function windowClose() {
    window.open('', '_parent', '');
    window.close();
}