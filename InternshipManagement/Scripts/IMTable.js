
const alertMessages = {
    user: {
        delete: {
            confirmTitle: 'Bạn chắc chứ?',
            confirmText: 'Bạn sẽ không thể hoàn tác điều này!',
            successTitle: 'Đã xóa!',
            successText: 'Các người dùng đã chọn đã bị xóa.',
            errorTitle: 'Lỗi!',
            errorText: 'Đã xảy ra lỗi khi xóa người dùng.'
        },
        edit: {
            errorTitle: 'Lỗi!',
            errorText: 'Đã xảy ra lỗi khi tìm nạp dữ liệu người dùng.'
        },
        viewDetails: {
            errorTitle: 'Lỗi!',
            errorText: 'Đã xảy ra lỗi khi tìm nạp dữ liệu người dùng.'
        },
        create: {
            errorTitle: 'Lỗi!',
            errorText: 'Đã xảy ra lỗi khi tải biểu mẫu tạo người dùng mới.'
        }
    }
    // Add more table types if needed
};

// Initialize table
function initializeTable($table, $deleteBtn, $editBtn, $viewDetailsBtn, $selectedCount) {
    $table.bootstrapTable();

    // Enable/disable buttons based on row selection
    $table.on('check.bs.table uncheck.bs.table check-all.bs.table uncheck-all.bs.table', function () {
        var selections = $table.bootstrapTable('getSelections');
        var selectedCount = selections.length;

        // Toggle visibility of buttons based on selection count
        $deleteBtn.toggle(selectedCount > 0);
        $editBtn.toggle(selectedCount === 1);
        $viewDetailsBtn.toggle(selectedCount === 1);

        // Update selected count display
        $selectedCount.text("(" + selectedCount + ")");
    });
}

// Delete button click handler
function handleDelete($table, $deleteBtn, $selectedCount, deleteUrl, tableType) {
    const messages = alertMessages[tableType].delete;
    $deleteBtn.click(function () {
        var ids = $.map($table.bootstrapTable('getSelections'), function (row) {
            return row.Username;
        });

        if (ids.length > 0) {
            // Show confirmation dialog
            Swal.fire({
                title: messages.confirmTitle,
                text: messages.confirmText,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Có, xóa!'
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: deleteUrl,
                        type: 'POST',
                        data: JSON.stringify(ids),
                        contentType: 'application/json; charset=utf-8',
                        success: function () {
                            $table.bootstrapTable('refresh');
                            Swal.fire(
                                messages.successTitle,
                                messages.successText,
                                'success'
                            );
                            // Reset selected count display after deletion
                            $selectedCount.text('0');
                        },
                        error: function () {
                            Swal.fire(
                                messages.errorTitle,
                                messages.errorText,
                                'error'
                            );
                        }
                    });
                }
            });
        }
    });
}


// Edit button click handler
function handleEdit($table, $editBtn, editUrl, editModalSelector, tableType) {
    const messages = alertMessages[tableType].edit;
    $editBtn.click(function () {
        var selectedUser = $table.bootstrapTable('getSelections')[0];
        if (selectedUser) {
            $.ajax({
                url: editUrl,
                type: 'GET',
                data: { username: selectedUser.Username },
                success: function (data) {
                    $(editModalSelector + ' .modal-content').html(data);
                    $(editModalSelector).modal('show');
                },
                error: function () {
                    Swal.fire({
                        title: messages.errorTitle,
                        text: messages.errorText,
                        icon: "error"
                    });
                }
            });
        }
    });
}


// View Details button click handler
function handleViewDetails($table, $viewDetailsBtn, detailUrl, viewModalSelector, tableType) {
    const messages = alertMessages[tableType].viewDetails;
    $viewDetailsBtn.click(function () {
        var selectedUser = $table.bootstrapTable('getSelections')[0];
        if (selectedUser) {
            $.ajax({
                url: detailUrl,
                type: 'GET',
                data: { username: selectedUser.Username },
                success: function (data) {
                    $(viewModalSelector + ' .modal-content').html(data);
                    $(viewModalSelector).modal('show');
                },
                error: function () {
                    Swal.fire({
                        title: messages.errorTitle,
                        text: messages.errorText,
                        icon: "error"
                    });
                }
            });
        }
    });
}

// Create button click handler
function handleCreate($createBtn, createUrl, createModalSelector, tableType) {
    const messages = alertMessages[tableType].create;
    $createBtn.click(function () {
        $.ajax({
            url: createUrl,
            type: 'GET',
            success: function (data) {
                $(createModalSelector + ' .modal-content').html(data);
                $(createModalSelector).modal('show');
            },
            error: function () {
                Swal.fire({
                    title: messages.errorTitle,
                    text: messages.errorText,
                    icon: "error"
                });
            }
        });
    });
}

// Custom AJAX request function
function ajaxRequest(params, ajaxUrl) {
    $.get(ajaxUrl + '?' + $.param(params.data)).then(function (res) {
        params.success(res);
    });
}

    function indexFormatter(value, row, index) {
        var pageSize = $table.bootstrapTable('getOptions').pageSize;
        var pageNumber = $table.bootstrapTable('getOptions').pageNumber;
        return pageSize * (pageNumber - 1) + index + 1;
    }