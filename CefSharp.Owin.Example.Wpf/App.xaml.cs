using CefSharp.Wpf;
using Microsoft.Owin.Builder;
using Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

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
            var appFunc = appBuilder.Build<AppFunc>();

            var settings = new CefSettings();
            settings.RegisterOwinSchemeHandlerFactory("owin", appFunc);
            Cef.Initialize(settings);
        }
    }
}
