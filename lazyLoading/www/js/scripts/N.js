/**
 * Created by jm96 on 14-5-6.
 */
//ie9,chrome 34.0.1847.131 m,ff 29.0
(function () {
    angular.module("NProvider", ["ng"]).
        provider("$N", function () {
            function N() {
                this.showLoading = false;
                this.dom=null;
                this.timeout=null;
                this.width=null;
            };
            N.prototype = {
                loading: function (config) {
                    if (this.showLoading){
                        if(!this.dom){
                            this.dom=document.createElement("div");
                            this.dom.innerHTML="loading";
                            this.dom.setAttribute("class","n-loading")

                            document.body.appendChild(this.dom);
                        }
                        if(this.timeout){
                            clearTimeout(this.timeout);
                            this.timeout=null;
                        }
                        this.dom.style.right="0px";
                    }
                },
                loaded: function (response) {
                    if (this.showLoading){
                        if(!this.width){
                            this.width=parseInt(window.getComputedStyle(this.dom).width);
                        }

                        function hideLoading(){
                            this.dom.style.right="-"+this.width+"px";
                        };
                        this.timeout=setTimeout(hideLoading.bind(this),1500);
                    }
                }
            };
            this.$get = function () {
                return new N();
            };
        });
})();