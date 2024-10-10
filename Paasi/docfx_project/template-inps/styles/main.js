$(function () {
    var texts = $("#toc li a");
    texts.each(function () {
      $(this).breakWord();
    });
});