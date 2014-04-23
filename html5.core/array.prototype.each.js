if (!Array.prototype.each) {
    Array.prototype.each = function (fn) {
        if (fn && typeof(fn)=="function") {
            for (var i = 0; i < this.length; i++) {
				fn.bind(this[i])(i);
            }
        }
    };
}

