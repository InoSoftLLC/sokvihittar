$(function() {
    $('.search-box button').click(search);
    $('.search-results .tabs a').click(selectTab);
    
    $('.search-results .tabs li:first-of-type').addClass('selected');
});

function search() {
    var button = $('.search-box button');
    var text = $('.search-box #search').val();
    var limit = $('.search-box #crawlerNumber').val();
    var firstTab;
    button.text('SEARCHING...');
    
    $.ajax({
        type: "GET",
        url: '/api/Search/',
        data: { text: text, limit: limit },
        success: function (data) {
            //$('.result-list').html(data[0]);
        },
        error: function() {
            
        }
    });
}

function selectTab(event) {
    var tab = $(event.target);
    var results = $('.search-results');
    
    if (!tab.hasClass('item')) {
        tab = tab.parents('.item');
    }
    var tabName = tab.children('span.name').text();
    
    results.find('#' + tabName).show().siblings().hide();
    tab.parent('li').addClass('selected').siblings().removeClass('selected');
}