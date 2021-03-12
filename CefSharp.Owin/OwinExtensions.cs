using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CefSharp.Owin
{
    //Shorthand for Owin pipeline func
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class OwinExtensions
    {
        public static void RegisterOwinSchemeHandlerFactory(this CefSettingsBase settings, string schemeName, AppFunc appFunc)
        {
            settings.RegisterScheme(new CefCustomScheme { SchemeName = schemeName, SchemeHandlerFactory = new OwinSchemeHandlerFactory(appFunc) });
        }
    }
}
