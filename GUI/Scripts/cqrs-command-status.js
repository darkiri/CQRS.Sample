jQuery.fn.flash = function (color, duration) {
    var current = this.css('backgroundColor');
    this.animate({ backgroundColor: 'rgb(' + color + ')' }, duration / 2)
        .animate({ backgroundColor: current }, duration / 2);
}
$(function () {
    var $status = $('#status');
    var connection = $.connection('/notifications');
    connection.start()
        .pipe(function() {
            connection.received(function(data) {
                $status.append(data);
            });
        });
});