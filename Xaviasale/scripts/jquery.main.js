(function ($) {
    $(function () {
        myfunload();
        initOwlCarousel();
        loadRecentlyProd();
        initCoptRealtime(30, 90);
        initCountDownDate();
    });
})(jQuery);
function addLoadSpinner() {
    $("body").append("<div id='product-loading'></div>");
}
function removeLoadSpinner() {
    $("#product-loading").remove();
}
//function===============================================================================================
/*=============================fun=========================================*/
var prevNowPlaying = null;
if (prevNowPlaying) {
    clearInterval(prevNowPlaying);
}
prevNowPlaying = setInterval(function () {
    var currentView = $(".copt-realtime-visitors__number").text();
    var randomInt = getRandomArbitrary(1, 5);
    var randomNum = getRandomArbitrary(1, 10);
    if (parseInt(currentView) < 10 || randomNum % 2 == 0) {
        $(".copt-realtime-visitors__number").text(parseInt(currentView) + randomInt);
    }
    else {
        $(".copt-realtime-visitors__number").text(parseInt(currentView) - randomInt);
    }
}, 3000);
function initCoptRealtime(min, max) {
    $(".copt-realtime-visitors__number").text(getRandomArbitrary(min, max));
}
function getRandomArbitrary(min, max) {
    return Math.floor(Math.random() * (max - min) + min);
}
function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min;
}
function myfunload() {
    $(".wrap-content p").each(function () {
        if ($(this).html() === "") {
            $(this).html("&nbsp;");
        }
    });
}
function initCountDownDate() {
    var dt = new Date();
    var toTime = 0;
    if (dt.getHours() >= 20) {
        toTime = new Date().setHours(24, 0, 0, 0);
        counDownDate(toTime);
        return;
    }
    if (dt.getHours() >= 16) {
        toTime = new Date().setHours(20, 0, 0, 0);
        counDownDate(toTime);
        return;
    }
    if (dt.getHours() >= 12) {
        toTime = new Date().setHours(16, 0, 0, 0);
        counDownDate(toTime);
        return;
    }
    if (dt.getHours() >= 8) {
        toTime = new Date().setHours(12, 0, 0, 0);
        counDownDate(toTime);
        return;
    }
    if (dt.getHours() >= 4) {
        toTime = new Date().setHours(8, 0, 0, 0);
        counDownDate(toTime);
        return;
    }
    if (dt.getHours() >= 0) {
        toTime = new Date().setHours(4, 0, 0, 0);
        counDownDate(toTime);
        return;
    }
}
function counDownDate(countDownDate) {
    var x = setInterval(function () {
        var now = new Date().getTime();
        var distance = countDownDate - now;

        var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        var seconds = Math.floor((distance % (1000 * 60)) / 1000);

        $(".copt-countdown-timer__digit").text((hours < 10 ? "0" + hours : hours) + ":" + (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds));
        if ($(".copt-countdown-timer__digit").is(":hidden")) {
            $(".copt-countdown-timer__digit").show();
        }
        if (distance < 0) {
            clearInterval(x);
            $(".copt-countdown-timer").remove();
        }
    }, 1000);
}
function initOwlCarousel() {
    $(".main-banner").owlCarousel({
        items: 1,
        loop: true,
        nav: false,
        dots: true,
        autoplay: true,
        autoplaytimeout: 10000,
        autoplayHoverPause: true
    });
    $(".review-carousel").owlCarousel({
        loop: false,
        margin: 30,
        nav: true,
        dots: false,
        autoplay: false,
        autoplaytimeout: 10000,
        navText: ["<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><path d=\"M16.17,3.79429978 C16.6087613,4.23306111 16.6087613,4.94443379 16.17,5.38319512 L16.1694477,5.38374707 L9.54469299,11.9992998 L16.1694477,18.6148525 C16.6085138,19.0533088 16.6090082,19.7646813 16.1705519,20.2037475 L16.17,20.2042998 C15.7309336,20.6433662 15.0190664,20.6433662 14.58,20.2042998 L6.375,11.9992998 L14.58,3.79429978 C15.0190664,3.35523341 15.7309336,3.35523341 16.17,3.79429978 Z\"></path></svg>", "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><path d=\"M7.829071,3.79429978 C7.39030967,4.23306111 7.39030967,4.94443379 7.829071,5.38319512 C7.82925505,5.38337917 7.82943916,5.38356315 7.82962333,5.38374707 L14.454378,11.9992998 L14.454378,11.9992998 L7.82962333,18.6148525 C7.39055717,19.0533088 7.39006277,19.7646813 7.82851906,20.2037475 C7.82870297,20.2039316 7.82888696,20.2041157 7.829071,20.2042998 C8.26813738,20.6433662 8.98000463,20.6433662 9.419071,20.2042998 L17.624071,11.9992998 L17.624071,11.9992998 L9.419071,3.79429978 C8.98000463,3.35523341 8.26813738,3.35523341 7.829071,3.79429978 Z\"></path></svg>"],
        responsive: {
            0: {
                items: 1
            },
            767: {
                items: 2
            },
            993: {
                items: 3
            }
        }
    });

    $(".sell-carousel").owlCarousel({
        loop: false,
        margin: 15,
        nav: true,
        dots: false,
        lazyLoad: true,
        autoplay: false,
        autoplaytimeout: 10000,
        navText: ["<svg class='svg-24' fill='#BCBCBC'> <path d='M15.41 16.09l-4.58-4.59 4.58-4.59L14 5.5l-6 6 6 6z'></path></svg>", "<svg class='svg-24' fill='#BCBCBC'><path d='M8.59 16.34l4.58-4.59-4.58-4.59L10 5.75l6 6-6 6z'></path></svg>"],
        onInitialized: function (event) {
            $(event.target).find(".owl-item .item").height($(event.target).find(".owl-item .item").height() + $(event.target).find("button.upsell-widget-product__add-cart").height() + 30);
        },
        responsive: {
            0: {
                items: 1
            },
            1024: {
                items: 4
            },
            1280: {
                items: 6
            }
        }
    });
    initOwlCarouselWithThumb();
}
function initOwlCarouselWithThumb() {
    var slidesPerPage = 5;

    $("#sync1").owlCarousel({
        items: 1,
        slideSpeed: 2000,
        nav: false,
        autoplay: false,
        dots: false,
        loop: false,
        responsiveRefreshRate: 200,
    }).on('changed.owl.carousel', syncPosition);

    $("#sync2").on('initialized.owl.carousel', function () {
        $("#sync2").find(".owl-item").eq(0).addClass("current");
    }).owlCarousel({
        margin: 15,
        items: slidesPerPage,
        dots: false,
        nav: false,
        smartSpeed: 200,
        slideSpeed: 500,
        slideBy: slidesPerPage,
        responsiveRefreshRate: 100
    }).on('changed.owl.carousel', syncPosition2);

    $("#sync2").on("click", ".owl-item", function (e) {
        e.preventDefault();
        var number = $(this).index();
        $("#sync1").data('owl.carousel').to(number, 300, true);
    });
}

function syncPosition(el) {
    var count = el.item.count - 1;
    var current = Math.round(el.item.index - (el.item.count / 2) - .5);

    if (current < 0) {
        current = count;
    }
    if (current > count) {
        current = 0;
    }
    $("#sync2").find(".owl-item").removeClass("current").eq(current).addClass("current");
    var onscreen = $("#sync2").find('.owl-item.active').length - 1;
    var start = $("#sync2").find('.owl-item.active').first().index();
    var end = $("#sync2").find('.owl-item.active').last().index();

    if (current > end) {
        $("#sync2").data('owl.carousel').to(current, 100, true);
    }
    if (current < start) {
        $("#sync2").data('owl.carousel').to(current - onscreen, 100, true);
    }
}
function syncPosition2(el) {
    var number = el.item.index;
    $("#sync1").data('owl.carousel').to(number, 100, true);
}

function loadRecentlyProd() {
    if (localStorage.getItem("prod_recently")) {
        $.ajax({
            type: "POST",
            data: { ids: localStorage.getItem("prod_recently") },
            url: "/umbraco/surface/product/loadrecentlyproducts",
            success: function (res) {
                if (res) {
                    $("#recently-viewed-and-featured-products").html(res);
                    initOwlCarousel();
                }
                else {
                    toastr.error("Error", "Error");
                }
            }
        });
    }
}
/**===========**/
$(".cart-container").click(function (e) {
    e.preventDefault();
    $(".cart-drawer").addClass("active");
});
$(document).on("click", ".cart-drawer-icon-close", function (e) {
    e.preventDefault();
    $(".cart-drawer").removeClass("active");
});
$(".search-icon").click(function (e) {
    e.preventDefault();
    $(".search-modal").addClass("fadeIn");
});
$(".search-modal__close, .search-modal__overlay").click(function (e) {
    e.preventDefault();
    $(".search-modal").removeClass("fadeIn");
});
$(".mobile-nav").click(function (e) {
    e.preventDefault();
    $(".mobile-menu").addClass("active");
});
$(".mobile-nav-menu__header-close, .popover-left__overlay").click(function (e) {
    e.preventDefault();
    $(".mobile-menu").removeClass("active");
});
$(".mobile-nav-menu__item .icon-toggle").click(function (e) {
    $(this).parents(".mobile-nav-menu__item").find(".mobile-nav-menu__sublist").toggleClass("active");
});
$("#requestForm, #rv_add_form").on('submit', function () {
    setTimeout(function () {
        if (!$("input, textarea, select").hasClass("input-validation-error")) {
            $(".loading_div").css("display", "block");
        }
    });
});
$("#newsletterForm").on('submit', function () {
    if (!$("input, textarea, select").hasClass("input-validation-error")) {
        $(".loading_div").css("display", "block");
    }
});
function onSuccess() {
    $(".loading_div").css("display", "none");
    $("#divUpdateMessage").removeClass("alert alert-danger").addClass("alert alert-success");
    if ($(".rv-widget__form--container").length > 0) {
        $(".rv-widget__form--container").hide();
        $(".rv-widget__add-review .rv-widget__btn").show();
        swal("Success!", "Your review has been sent to Administrator!", "success");
    }
}
function onFailure() {
    $(".loading_div").css("display", "none");
    $("#divUpdateMessage").addClass("alert alert-danger");
    if ($(".rv-widget__form--container").length > 0) {
        swal("Error!", "An error has occurred!", "error");
    }
}
$("#sort-product--ajax").on("change", function (e) {
    $(".loading_div").css("display", "block");
    $.ajax({
        type: "POST",
        data: { lang: $("#currentculture").val(), sort: $(this).val() },
        url: "/umbraco/surface/product/loadproducts",
        success: function (res) {
            if (res) {
                setTimeout(function () {
                    $("#loadAjaxProductBySort").html(res);
                    $(".loading_div").css("display", "none");
                }, 100);
            }
            else {
                toastr.error("Error", "Error");
                $(".loading_div").css("display", "none");
            }
        }
    });
});
$(".rv-widget__add-review").click(function () {
    $(this).children(".rv-widget__btn").hide();
    $(".rv-widget__form--container").stop(true, false, true).slideToggle(300);
});
$(document).on("click", ".rv-widget__form-close", function () {
    $(".rv-widget__add-review .rv-widget__btn").show();
    $(".rv-widget__form--container").stop(true, false, true).slideToggle(300);
});
$(document).on("click", ".cart-drawer .cart-drawer-overlay", function () {
    $(".cart-drawer").removeClass("active");
});
$(document).on("mouseover", ".rv-widget__form-star .review-icon-symbols", function () {
    var reviewText = "Wonderful";
    $(this).parent().attr("data-rating", ($(this).prevAll().length + 1));
    $(".rv-widget__form-feeling").removeClass((index, className) => (className.match(/(^|\s)rv-widget__form-feeling-\S+/g) || []).join(' '));
    switch ($(this).prevAll().length + 1) {
        case 1:
            reviewText = "Awful";
            break;
        case 2:
            reviewText = "Bad";
            break;
        case 3:
            reviewText = "Normal";
            break;
        case 4:
            reviewText = "Good";
            break;
        case 5:
            reviewText = "Wonderful";
            break;
    };
    $(".rv-widget__form-feeling").addClass("rv-widget__form-feeling-" + ($(this).prevAll().length + 1));
    $(".rv-widget__form-feeling").text(reviewText);
    $("input[type='hidden']#Star").val($(this).prevAll().length + 1);
    $(this).prevAll().length + 1 >= 4 ? $(".rv-widget__form-noti").hide() : $(".rv-widget__form-noti").show();
});
$("#rv_add_form input, #rv_add_form textarea").on("keyup", function () {
    textcounter($(this).parents(".rv-widget__group-field"), $(this).val().length);
});
$(".rv-widget__control-suggest").on("click", function () {
    var text = $(".rv-widget__group-field textarea.rv-widget__input").val();
    $(".rv-widget__group-field textarea.rv-widget__input").val(text + " " + $(this).text());
    textcounter($(this).parents(".rv-widget__group-field").next(), $(this).parents(".rv-widget__group-field").next().find(".rv-widget__input").val().length + $(this).text().length);
});
$(document).on("click", "#cart .button-quantity__layout-vertical .button-quantity", function () {
    var input = $(this).parent().find(".input-quantity");
    var value = input.val() !== "" ? input.val() : 0;
    if ($(this).hasClass("increase")) {
        input.val(parseInt(value) + 1);
    }
    else {
        if (value > 1) {
            input.val(parseInt(value) - 1);
        }
    }
});
function textcounter(el, length) {
    var text = el.find(".rv-widget__form-character span").text();
    var textSplit = text.split("/");
    textSplit[0] = length;
    el.find(".rv-widget__form-character span").text(textSplit.join("/"));
}
function gotopage(url) {
    window.location.href = url;
}
$(document).on("click", ".upsell-widget-product__add-cart",function () {
    cart.handleAddtoCart(this);
});
$(document).on("click", ".product-cart__remove",function () {
    cart.handleRemoveItemInCartSideBar(this);
});
$(document).on("click", ".upsell-quantity__discount-button .upsell-quantity__add-cart", function () {
    $("button.upsell-quantity__add-cart").prop('disabled', false);
    $(this).prop('disabled', true);
    $(".upsell-button-dual-ring").remove();
    $(this).prepend("<div class='upsell-absolute upsell-button-dual-ring'></div>");
    cart.handleAddCoupon(this);
});

/*******======================*******/
$(window).load(function () {
    if ($(this).scrollTop() > 300) {
        $('#sticky-bar').addClass('active');
    }
});
$(window).scroll(function () {
    if ($(this).scrollTop() > 300) {
        $('#sticky-bar').addClass('active');
    }
    else {
        $('#sticky-bar').removeClass('active');
    }
});