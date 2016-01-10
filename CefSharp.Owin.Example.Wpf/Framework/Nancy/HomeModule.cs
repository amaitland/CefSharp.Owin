using Microsoft.Owin;
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
                var owinContext = new OwinContext(Context.GetOwinEnvironment());

                var model = new { Text = "Welcome to CefSharp.Owin", Method = owinContext.Request.Method };
                return View["home", model];
            };
        }
    }
}
