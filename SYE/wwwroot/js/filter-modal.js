/**
 * Settings
 */
var settings = {

    // Ids
    modal: document.querySelector('#modalContent'),
    modalId: '#modalContent',

    // Logging
    outputLog: true,

    // Common Vars
    modalOpen: false

};


/**
 * Show the modal
 */
function showModal() {
    //restrictTabbing();
    settings.modalOpen = true;

    $("body").addClass("modal-open");

    $('#modalContent').addClass("gfc-modal-open");
    if (document.querySelector('#content')) {
        document.querySelector('#content').inert = true;
        document.querySelector('#content').setAttribute('aria-hidden', 'true');
    }
    if (settings.outputLog) console.log("MODAL SHOWN");
}


function restrictTabbing() {
    var tabbable = $("body").find('#modalContent .tabbable');
    var firstTabbable = tabbable.first();
    var lastTabbable = tabbable.last();

    /*redirect last tab to first input*/
    lastTabbable.on('keydown', function (e) {
        if (settings.modalOpen && (e.which === 9 && !e.shiftKey)) {
            e.preventDefault();
            firstTabbable.focus();
        }
    });

    /*redirect first shift+tab to last input*/
    firstTabbable.on('keydown', function (e) {
        if (settings.modalOpen && (e.which === 9 && e.shiftKey)) {
            e.preventDefault();
            lastTabbable.focus();
        }
    });
}


/**
 * Close Modal when Esc key used
 */
$(document).on("keydown", function (evt) {
    if (evt.keyCode === 27) {
        hideModal();
    }
});


/**
 * Hide the modal
 */
function hideModal() {

    $("body").removeClass("modal-open");

    $('#modalContent').removeClass("gfc-modal-open");
    if (document.querySelector('#content')) {
        document.querySelector('#content').inert = false;
        document.querySelector('#content').setAttribute('aria-hidden', 'false');
    }

    $('#content').focus();

    settings.modalOpen = false;
    if (settings.outputLog) console.log("MODAL HIDDEN");
}


/**
 * Starts the sessionCheck once the page has loaded
 */
$(document).ready(function () {
    
    restrictTabbing();
});
