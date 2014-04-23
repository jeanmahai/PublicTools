//ecma262 && javascript 1.8 以上支持Function.bind
//from https://developer.mozilla.org/en-US/docs/JavaScript/Reference/Global_Objects/Function/bind
if (!Function.prototype.bind) {
    Function.prototype.bind = function (oThis) {
        if (typeof this !== "function") {
            throw new TypeError("Function.prototype.bind - what is trying to be bound is not callable");
        }
        var aArgs = Array.prototype.slice.call(arguments, 1),
            fToBind = this,
            fNop = function () { },
            fBound = function () {
                return fToBind.apply(this instanceof fNop && oThis
                                       ? this
                                       : oThis,
                                     aArgs.concat(Array.prototype.slice.call(arguments)));
            };
        fNop.prototype = this.prototype;
        fBound.prototype = new fNop();
        return fBound;
    };
}