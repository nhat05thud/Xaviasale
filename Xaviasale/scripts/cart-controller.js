var cart = {
    handleRemoveItem: function (e) {
        $(e).parents(".product-cart").remove();
        this.renderNameInput();
    },
    handleRemoveItemInCartSideBar: function (e) {
        $(e).parents(".product-cart").remove();
        this.processRemoveItemInCart($(e).data("id"), $(e).data("color"))
    },
    handleAddtoCart: function (e) {
        this.processAddtoCart($(e).data("id"));
    },
    handleAddtoCartDetail: function (e) {
        var color = $("input[name='ProductColor']").val();
        var quantity = $("#product_content--ajax .button-quantity__layout-vertical input[type='number']").val();
        if (color && color !== "") {
            this.processAddtoCart($(e).data("id"), color, quantity);
        }
        else {
            toastr.warning("Select Color", "Color");
        }
    },
    handleBuyNow: function (e) {
        var color = $("input[name='ProductColor']").val();
        var quantity = $("#product_content--ajax .button-quantity__layout-vertical input[type='number']").val();
        if (color && color !== "") {
            this.processBuyNow($(e).data("id"), color, quantity);
        }
        else {
            toastr.warning("Hãy chọn color", "Color");
        }
    },
    handleAddCoupon: function (e) {
        var color = $("input[name='ProductColor']").val();
        this.processAddCoupon($(e).data("product"), $(e).data("coupon"), color);
    },
    processAddCoupon: function (id, couponid, color) {
        addLoadSpinner();
        $.ajax({
            type: "POST",
            data: { id: id, couponId: couponid, color: color},
            url: "/umbraco/surface/cart/addcoupon",
            success: function (res) {
                if (res.success) {
                    window.location.href = res.redirectUrl
                }
                else {
                    toastr.error(res.responseMessage, res.responseType);
                }
            }
        });
    },
    processAddtoCart: function (id, color, quantity) {
        addLoadSpinner();
        $.ajax({
            type: "POST",
            data: { id: id, color: color, quantity: quantity },
            url: "/umbraco/surface/cart/addtocart",
            success: function (res) {
                removeLoadSpinner();
                if (res.success) {
                    $(".cart-number").text(res.cartNumber);
                    $("#site-nav--cart").html(res.cartAside);
                    if ($("#coupons-section").length > 0) {
                        $("#coupons-section").html(res.couponSection);
                    }
                    toastr.success(res.responseMessage, res.responseType);
                }
                else {
                    toastr.error(res.responseMessage, res.responseType);
                }
            }
        });
    },
    processBuyNow: function (id, color, quantity) {
        addLoadSpinner();
        $.ajax({
            type: "POST",
            data: { id: id, color: color, quantity: quantity },
            url: "/umbraco/surface/cart/buynow",
            success: function (res) {
                window.location.href = res.checkoutUrl;
            }
        });
    },
    processRemoveItemInCart: function (id, color) {
        addLoadSpinner();
        $.ajax({
            type: "POST",
            data: { id: id, color: color },
            url: "/umbraco/surface/cart/removeitemincart",
            success: function (res) {
                removeLoadSpinner();
                if (res.success) {
                    $(".cart-number").text(res.cartNumber);
                    $("#site-nav--cart").html(res.cartAside);
                    $(".cart-drawer").addClass("active");
                    if ($("#coupons-section").length > 0) {
                        $("#coupons-section").html(res.couponSection);
                    }
                    toastr.success(res.responseMessage, res.responseType);
                }
                else {
                    toastr.error(res.responseMessage, res.responseType);
                }
            }
        });
    },
    renderNameInput: function () {
        $(".input-productId").each(function (e) {
            $(this).attr("name", "model.Carts[" + e + "].ProductId");
        });
        $(".input-quantity").each(function (e) {
            $(this).attr("name", "model.Carts[" + e + "].Quantity");
        });
        $(".input-color").each(function (e) {
            $(this).attr("name", "model.Carts[" + e + "].Color");
        });
        $(".input-couponId").each(function (e) {
            $(this).attr("name", "model.Carts[" + e + "].CouponId");
        });
    },
    init: function () {
        this.renderNameInput();
    }
}
cart.init();