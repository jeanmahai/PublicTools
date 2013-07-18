/// <reference path="WebSocketMessageBase.js" />

function WebSocketResponse() {
    WebSocketMessageBase.call(this);
    this.Handler = "";
    this.Data = "";
    this.RemoteHandler = "";
}

WebSocketResponse.prototype = new WebSocketMessageBase();