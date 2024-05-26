/* BEGIN EXTERNAL SOURCE */

/* END EXTERNAL SOURCE */
/* BEGIN EXTERNAL SOURCE */

/* END EXTERNAL SOURCE */
/* BEGIN EXTERNAL SOURCE */

    $(document).ready(function() {
        $('#Tags').select2({
            tags: true,
            tokenSeparators: [','],
            ajax: {
                url: '/*******************/hTag SearchTag              delay: 250,
                data: function(params) {
                    return {
                        query: params.term
                    };
                },
                processResults: function(data) {
                    return {
                        results: data.map(function(tag) {
                            return {
                                id: tag.value,
                                text: tag.label
                            };
                        })
                    };
                },
                cache: true
            }
        });

        $('#createProjectForm').on('submit', function(event) {




            event.preventDefault();
            var formData = $(this).serialize();
            $.ajax({
                type: 'POST',
                url: '/*******************/**********************dTag
                data: formData,
                success: function(response) {
    o*********rAddT/*******************/ can proceed with creating the project
                    } else {
                        // Handle error message
                    }
                },
                error: function(xhr, status, error) {
                    // Handle error
                }
            });
        });
    });

/* END EXTERNAL SOURCE */
