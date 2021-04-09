$.getJSON('/Node/MapNodes', function (nodes) {
    nodes.forEach(applyNode);
});

function applyNode(node) {
    var path = loadPath(node.identifier);

    var last = path[path.length - 1];
    var title = last.children('.title');

    if (!title.attr('href')) {
        title.attr('href', `/Node/Details?id=${node.id}`);
    }

    var level = node.levelLog?.level || 0;
    title.toggleClass('warn', level == 2);
    title.toggleClass('error', level == 3);

    for (var i = path.length - 1; i >= 0; i--) {
        var node = path[i];
        checkLevel(node);
    }
}
function loadPath(identifier) {
    var parts = identifier.split('.');

    var parent = $('.map');
    var path = [parent];

    for (const part of parts) {
        var node = parent.find('.node').filter(function () {
            return $(this).children('.title').filter(function () {
                return $(this).text() == part;
            }).length > 0;
        });

        if (node.length <= 0) {
            var html = $('.map template').html();
            node = $(html);

            var title = node.find('.title');
            title.text(part);

            node.appendTo(parent);
        }

        path.push(node);
        parent = node;
    }

    return path;
}
function checkLevel(node) {
    var warned = node.has('.warn').length > 0;
    var errored = node.has('.error').length > 0;

    node.toggleClass('warn', warned && !errored);
    node.toggleClass('error', errored);
}

var connection = new signalR.HubConnectionBuilder().withUrl("/nodeHub").build();

connection.on("NodeAsync", applyNode);

connection.start().then(function () {
    console.log('signalR started');
}).catch(function (err) {
    return console.error(err);
});