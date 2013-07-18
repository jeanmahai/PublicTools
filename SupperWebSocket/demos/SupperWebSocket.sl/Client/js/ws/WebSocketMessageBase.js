function WebSocketMessageBase() { }

WebSocketMessageBase.prototype = {
    toJSONString: function () {
        return JSON.stringify(this);
    }
};