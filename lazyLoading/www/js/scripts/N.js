/**
 * Created by jm96 on 14-5-6.
 */
(function () {
    angular.module("NProvider", []).
        provider("$N", function () {
            function N() {
                this.showLoading = false;
            };
            N.prototype = {
                loading: function (config) {
                    if (this.showLoading)
                        console.info("loading " + config.url);
                },
                loaded: function (response) {
                    if (this.showLoading)
                        console.info("loaded " + response.config.url);
                }
            };
            this.$get = function () {
                return new N();
            };
        });
})();