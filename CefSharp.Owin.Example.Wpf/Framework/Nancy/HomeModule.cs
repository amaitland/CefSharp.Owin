using CefSharp.Owin.Example.Wpf.Framework.Nancy.ViewModels;
using Microsoft.Owin;
using Nancy;
using Nancy.ModelBinding;
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

                var inputModel = this.Bind<HomeBindingModel>();

                var model = new HomeViewModel
                {
                    Text = string.Format("Welcome to {0}.{1}", inputModel.Input1, inputModel.Input2),
                    Method = owinContext.Request.Method
                };
                return View["home", model];
            };
        }
    }
}
