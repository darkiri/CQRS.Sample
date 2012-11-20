$(function () {
    var $status = $('#status');
    var connection = $.connection('/notifications');
    connection
        .start()
        .pipe(function() {
            connection.received(function(data) {
                $status.append(data);
            });
        });
});