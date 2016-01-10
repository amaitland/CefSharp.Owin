using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Nancy;
using Nancy.Owin;

namespace CefSharp.Owin.Example.Wpf.Framework.Nancy
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = x =>
            {
                var env = Context.GetOwinEnvironment();

                var requestBody = (Stream)env["owin.RequestBody"];
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                var requestMethod = (string)env["owin.RequestMethod"];
                var requestPath = (string)env["owin.RequestPath"];
                var requestPathBase = (string)env["owin.RequestPathBase"];
                var requestProtocol = (string)env["owin.RequestProtocol"];
                var requestQueryString = (string)env["owin.RequestQueryString"];
                var requestScheme = (string)env["owin.RequestScheme"];

                var responseBody = (Stream)env["owin.ResponseBody"];
                var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];

                var model = new { Text = "Welcome to CefSharp.Owin" };
                return View["home", model];
            };
        }
    }
}
