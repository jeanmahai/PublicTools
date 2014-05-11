/**
 * Created by jm96 on 14-5-6.
 */
//ie9,chrome 34.0.1847.131 m,ff 29.0
(function () {
    if (!Function.prototype.bind) {
        Function.prototype.bind = function (oThis) {
            if (typeof this !== "function") {
                // closest thing possible to the ECMAScript 5 internal IsCallable function
                throw new TypeError("Function.prototype.bind - what is trying to be bound is not callable");
            }

            var aArgs = Array.prototype.slice.call(arguments, 1),
                fToBind = this,
                fNOP = function () {
                },
                fBound = function () {
                    return fToBind.apply(this instanceof fNOP && oThis
                        ? this
                        : oThis,
                        aArgs.concat(Array.prototype.slice.call(arguments)));
                };

            fNOP.prototype = this.prototype;
            fBound.prototype = new fNOP();

            return fBound;
        };
    }

    angular.module("NProvider", ["ng"]).
        provider("$N", function () {
            function N() {
                this.showLoading = false;
                this.dom = null;
                this.timeout = null;
                this.width = null;
            };
            N.prototype = {
                loading: function (config) {
                    if (this.showLoading) {
                        if (!this.dom) {
                            this.dom = document.createElement("div");
                            this.dom.innerHTML = "<div class='circle'></div><div class='circle1'></div>";
                            this.dom.setAttribute("class", "n-loading")

                            document.body.appendChild(this.dom);
                        }
                        if (this.timeout) {
                            clearTimeout(this.timeout);
                            this.timeout = null;
                        }
                        this.dom.style.right = "0px";
                    }
                },
                loaded: function (response) {
                    if (this.showLoading) {
                        if (!this.width) {
                            if (window.getComputedStyle)
                                this.width = parseInt(window.getComputedStyle(this.dom).width);
                            else if (this.dom.currentStyle) {
                                this.width = parseInt(this.dom.currentStyle.width);
                            }
                        }

                        function hideLoading() {
                            this.dom.style.right = "-" + this.width + "px";
                        };
                        this.timeout = setTimeout(hideLoading.bind(this), 1500);
                    }
                }
            };
            this.$get = function () {
                return new N();
            };
        });
})();