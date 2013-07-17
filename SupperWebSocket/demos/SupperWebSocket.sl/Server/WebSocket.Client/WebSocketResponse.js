/// <reference path="WebSocketMessageBase.js" />

function WebSocketResponse() {
    WebSocketMessageBase.call(this);
    this.Handler = "";
    this.Data = "";
}

WebSocketResponse.prototype = new WebSocketMessageBase();