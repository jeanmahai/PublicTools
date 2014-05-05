<%@ WebHandler Language="C#" Class="Handler" %>

using System;
using System.Web;

public class Handler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        context.Response.Write("Hello World");
        context.Response.Headers["test"] = "test";
        context.Response.Headers["test2"] = "test2";
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}