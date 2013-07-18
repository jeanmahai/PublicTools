/// <reference path="WebSocketMessageBase.js" />

function WebSocketRequest() {
    WebSocketMessageBase.call(this);
    this.Handler = "";
    this.Header = "";
    this.Body = "";
    this.ClientCallbackID = "";
}

WebSocketRequest.prototype = new WebSocketMessageBase();