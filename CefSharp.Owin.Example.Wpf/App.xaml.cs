using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Owin;
using Owin.Builder;

namespace CefSharp.Owin.Example.Wpf
{
    //Shorthand for Owin pipeline func
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var appBuilder = new AppBuilder();
            appBuilder.UseNancy();
            var appFunc = (AppFunc)appBuilder.Build(typeof (AppFunc));

            var settings = new CefSettings();
            settings.RegisterOwinSchemeHandlerFactory("owin", appFunc);
            Cef.Initialize(settings);
        }
    }
}
