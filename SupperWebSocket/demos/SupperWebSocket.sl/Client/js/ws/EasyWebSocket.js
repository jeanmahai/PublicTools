/// <reference path="WebSocketMessageBase.js" />
/// <reference path="WebSocketRequest.js" />
/// <reference path="WebSocketResponse.js" />

function ClientCallback(fn) {
    this.id = Math.round(Math.random() * 1000000);
    this.callback = fn;
}

function emptyFn() {
}

function EasyWebSocket(ws, onopen, onclose, onerror) {
    var me = this;

    this.socket = new WebSocket(ws);
    this.callbacks = [];
    this.onopend = onopen;
    this.onclose = onclose;
    this.onerror = onerror;

    this.socket.onclose = this.onclose || emptyFn;
    this.socket.onerror = this.onerror || emptyFn;
    this.socket.onopen = this.onopend || emptyFn;
    this.socket.onmessage = function (evt) {
        var _response;
        try {
            _response = JSON.parse(evt.data);
        }
        catch (ex) {
            console.warn("返回的数据不是JSON格式,不能转换");
            _response = evt.data;
        }
        if (!_response) return;
        if (_response.Handler) {
            var _callback = me.findCallback(_response.Handler);
            if (_callback && _response.Data) {
                _callback(_response.Data);
                me.removeCallback(_response.Handler);
                return;
            }
            else if (_response.RemoteHandler) {
                eval(_response.RemoteHandler);
                return;
            }
        }
        else if (_response.RemoteHandler) {
            eval(_response.RemoteHandler);
            return;
        }
        console.warn("无处理对象,data=" + _response);
    };
}

EasyWebSocket.prototype = {
    send: function (message, callback) {
        var handler = new ClientCallback(callback);
        if (message instanceof WebSocketRequest) {
            message.ClientCallbackID = handler.id;
            this.socket.send(message.toJSONString());
        } else {
            this.socket.send(message);
        }
        this.callbacks.push(handler);
    },
    findCallback: function (id) {
        var i;
        var callbacks = this.callbacks;
        var len = callbacks.length;
        for (i = 0; i < len; i++) {
            if (callbacks[i].id == id) {
                return callbacks[i].callback;
            }
        }
        return null;
    },
    removeCallback: function (id) {
        var i;
        var callbacks = this.callbacks;
        var len = callbacks.length;
        for (i = 0; i < len; i++) {
            if (callbacks[i].id == id) {
                callbacks[i] = callbacks[callbacks.length - 1];
                callbacks.pop();
            }
        }
        console.info("callback length=" + callbacks.length);
    }
};