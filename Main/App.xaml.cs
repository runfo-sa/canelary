using AdonisUI;
using Core.Logger;
using Core.Services;
using Core.Services.SettingsModel;
using Core.View;
using ICSharpCode.AvalonEdit.Highlighting;
using Main.ViewModels;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace Main
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            // Enviamos las exepciones no capturadas a una funcion
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ResolveException);

            // Asignamos la región a utilizar para el formato de números y fechas
            Thread.CurrentThread.CurrentCulture = new CultureInfo(SettingsService.Instance.Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(SettingsService.Instance.Culture);
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)
                )
            );

            // Carga el archivo que especifica el syntax highlighting para el lenguaje ZPL
            // Lo hacemos aca para tener que hacerlo una sola vez en el ciclo de vida del programa
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly
                .GetManifestResourceNames()
                .Single(str => str.EndsWith("ZPL.xshd"));

            var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new System.Xml.XmlTextReader(stream!);
            HighlightingManager.Instance.RegisterHighlighting(
                "ZPL",
                [],
                ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(
                    reader,
                    HighlightingManager.Instance
                )
            );

            ChangeTheme(SettingsService.Instance.Theme);

            return Container.Resolve<Views.Main>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<Settings>();
            containerRegistry.RegisterDialog<About>();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register<Views.Main, MainViewModel>();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = @".\\Modules" };
        }

        public static void ResolveException(object? sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;
            ExceptionPopUp popUp = new(ex.GetBaseException().Message);
            Logger.Log($"Message: {ex.GetBaseException().Message}{Environment.NewLine}Stack Trace:{Environment.NewLine}{ex.GetBaseException().StackTrace}");
            popUp.ShowDialog();
        }

        public static void ChangeTheme(Theme theme)
        {
            ResourceLocator.SetColorScheme(Current.Resources,
                theme == Theme.Dark ? ResourceLocator.DarkColorScheme : ResourceLocator.LightColorScheme);
        }
    }
}