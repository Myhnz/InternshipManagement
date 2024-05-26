

function checkFormErrors(formId, fieldsToCheck) {
    var formData = $('#' + formId).serialize();
    var dataObject = {};
    $('#' + formId).serializeArray().map(function (x) { dataObject[x.name] = x.value; });

    $.ajax({
        url: '/Admin/CheckErrors',
        type: 'POST',
        data: { dataObject: dataObject, fieldsToCheck: fieldsToCheck },
        success: function (response) {
            $('.text-danger').text('');
            $('.form-control').removeClass('is-invalid');

            $.each(response, function (key, value) {
                $('[name="' + key + '"]').addClass('is-invalid').next('.text-danger').text(value);
            });

            if ($.isEmptyObject(response)) {
                $('#' + formId).unbind('submit').submit();
            }
        }
    });

}