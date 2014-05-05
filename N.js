/**
 * Created by jm96 on 14-4-30.
 */
//angular js

//N
(function (ns) {
    if (!window[ns]) window[ns] = {};

    //localStorage
    function getData(key, type) {
        var val = window.localStorage.getItem(key);
        if (type === "object") return JSON.parse(val);
        if (type === "boolean") return new Boolean(val);
        if (type === "number") return new Number(val);
        return val;
    }
    window[ns].getData=getData;
    function setData(key, value) {
        var val;
        if (typeof(value) === "object"
            || typeof(value) === "array")
            val = JSON.stringify(value);
        else
            val=value;
        window.localStorage.setItem(key,val);
    }
    window[ns].setData=setData;
    function removeData(key) {
        return window.localStorage.removeItem(key);
    }
    window[ns].removeData=removeData;

})("N");
