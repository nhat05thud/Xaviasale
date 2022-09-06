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
            toastr.warning("Hãy chọn color", "Color");
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
    processAddtoCart: function (id, color, quantity) {
        $(".loading_div").show();
        var $this = this;
        $.ajax({
            type: "POST",
            data: { id: id, color: color, quantity: quantity },
            url: "/umbraco/surface/cart/addtocart",
            success: function (res) {
                $(".loading_div").hide();
                if (res.success) {
                    $(".cart-number").text(res.cartNumber);
                    $("#site-nav--cart").html(res.cartAside);
                    //$(".cart-drawer").addClass("active");
                    toastr.success(res.responseMessage, res.responseType);
                }
                else {
                    toastr.error(res.responseMessage, res.responseType);
                }
            }
        });
    },
    processBuyNow: function (id, color, quantity) {
        $(".loading_div").show();
        var $this = this;
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
        $(".loading_div").show();
        var $this = this;
        $.ajax({
            type: "POST",
            data: { id: id, color: color },
            url: "/umbraco/surface/cart/removeitemincart",
            success: function (res) {
                $(".loading_div").hide();
                if (res.success) {
                    $(".cart-number").text(res.cartNumber);
                    $("#site-nav--cart").html(res.cartAside);
                    $(".cart-drawer").addClass("active");
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
    },
    init: function () {
        this.renderNameInput();
    }
}
cart.init();