using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace SiteAdminUtils
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");

            //var culture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            //culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            //Thread.CurrentThread.CurrentCulture = culture;

            //var cultureUi = (CultureInfo)Thread.CurrentThread.CurrentUICulture.Clone();
            //cultureUi.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            //Thread.CurrentThread.CurrentUICulture = cultureUi;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                          XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            base.OnStartup(e);
            DispatcherHelper.Initialize();
        }
    }
}
