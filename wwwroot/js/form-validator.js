function validateRequiredFields(containerSelector = '') {
    let isValid = true;

    // Select all inputs/selects/textareas marked as required
    const $fields = $(`${containerSelector} input[required], ${containerSelector} select[required], ${containerSelector} textarea[required]`);

    $fields.each(function () {
        const $field = $(this);
        const value = $field.val().trim();
        const errorLabelId = $field.attr('id') + '-error';
        let $errorLabel = $('#' + errorLabelId);

        // Create error label if not exists
        if ($errorLabel.length === 0) {
            $errorLabel = $('<label class="error-message" id="' + errorLabelId + '">This value is required.</label>');
            $field.after($errorLabel);
        }

        if (!value) {
            $errorLabel.show();
            isValid = false;
        } else {
            $errorLabel.hide();
        }
    });

    return isValid;
}
function attachInputValidationListeners() {
    $('.input-group input').on('input', function () {
        const inputId = $(this).attr('id');
        const errorLabel = $(`#${inputId}-error`);
        if (errorLabel.length > 0) {
            errorLabel.text('').hide();
        }
    });
}
