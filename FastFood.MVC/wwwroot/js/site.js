$(function () {
    $(document).on('submit', '#registerForm', function (e) {
        e.preventDefault();

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: $(this).serialize(),
            success: function (res) {
                if (res.success) {
                    $('#registerModal').modal('hide');
                    location.reload();
                } else {
                    $('#registerModal .modal-content').html(res);
                    $.validator.unobtrusive.parse('#registerForm');
                }
            },
            error: function (xhr) {
                console.error("Error submitting the form:", xhr);
                alert("There was an error processing your request. Please try again.");
            }
        });
    });
});