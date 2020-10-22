// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Run these functions once page is loaded:
$(document).ready(function () {
    focusElementIfExists("#error-summary-container");
});

//Functions

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

//Function to enable the 'status' message to be announced
//Rationale: this data is already on the page but the content must be updated for teh status message to be announced properly, as per DAC audit
function announceStatusMessages() {
    var statusMessages = document.querySelectorAll("[role='status']");
    for (let i = 0; i < statusMessages.length; i++) {
        //Set just the inner HTML:
        statusMessages[i].innerHTML = statusMessages[i].innerHTML;
    }
}

//Function to return user focus to checkbox they clicked
function reselectCheckbox(domId) {
    $("#" + domId).focus();
    //setTimeout(function () { element.focus(); }, 1);
}

//filter functions
function setFilterButtons() {
    if (isFilterActive()) {
        $("#btn--filter-clear").show();
    } else {
        $("#btn--filter-clear").hide();
    }
    $("#btn--filter").hide();
}
function doSubmitIfDesktop(checkboxElem, formName, checkboxClicked) {
    if (checkboxElem.checked) {
        $("#btn--filter-clear").show();//don't really need this
    }
    if (!isMobile()) {
        //This is a tablet or desktop so apply the filter
        //Set 'selectedCheckbox' option to the id of what they picked before
        if (checkboxClicked != null) {
            $("#checkboxClicked").val(checkboxClicked);
        }
        $("form#" + formName).submit();
    }
}
function isMobile() {
    if (window.matchMedia("(max-width: 767px)").matches) {
        // The viewport is less than 768 pixels wide
        //This is a mobile device
        return true;
    } else {
        // The viewport is at least 768 pixels wide
        //This is a tablet or desktop
        return false;
    }
}
function clearFiltersAndSubmit(formName) {
    var num = $("#FacetsModal_Count").val();
    var index;
    for (index = 0; index < num; index++) {
        var elem = $("#Facets_" + index.toString() + "__Selected");
        elem.prop("checked", false);
    }
    $("form#" + formName).submit();
}
function isFilterActive() {
    var returnVal = false;
    var num = $("#FacetsModal_Count").val();
    var index;
    for (index = 0; index < num; index++) {
        var elem = $("#Facets_" + index.toString() + "__Selected");
        if (elem.attr("checked")) {
            returnVal = true;
        }
    }
    return returnVal;
}
// functions for the form
function checkForJs() {
    //looks for a text area with a class of auto-expand
    //and attaches a js function
    $("textarea.auto-expand").each(function () {
        var textBox = document.getElementById(this.name);

        textBox.onkeyup = function () {
            var existingHeight = $(this).height();
            var fontsize = 15;
            var charsBuffer = 75
            var defaultScrollHeight = 210;
            var lineBuffer = 0;//we don't need this now but set it to zero in case we need it later

            if (isMobile()) {
                if (existingHeight > defaultScrollHeight) {
                    $(this).height(defaultScrollHeight);//shrink back to default 
                }
                return;
            }

            var rows = parseInt(countRows(textBox));
            var chars = textBox.value.length;
            //console.log(rows);
            if (chars>=charsBuffer) {
                $(this).height(defaultScrollHeight + ((rows - lineBuffer) * fontsize));//add one row
            } else {
                if (existingHeight > defaultScrollHeight) {
                    $(this).height(defaultScrollHeight);//shrink back to default 
                }
            }
        }
    });
}
function countRows(textArea) {
    var text = textArea.value;
    //determine what the fontsize will be
    var fontsize = 15;
    //get number of characters that can fit in a row
    var charsperrow = textArea.clientWidth / fontsize;
    //get any hard returns
    var hardreturns = text.split(/\r|\r\n|\n/);
    var rows = hardreturns.length;
    //loop through returns and calculate soft returns
    for (var i = 0, len = rows; i < len; i++) {
        var line = hardreturns[i];
        var softreturns = Math.round(line.length / charsperrow);
        //if softreturns is greater than 0, minus by 1 (hard return already counted)
        softreturns = Math.round(softreturns > 0 ? (softreturns - 1) : 0);
        rows += softreturns;
    }
    return Math.round(rows)-1;
}

function focusElementIfExists(selector) {
    var element = document.querySelector(selector);

    if (element != null) {
        element.focus();
    }
}