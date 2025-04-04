using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;

using AutoRenovatioNS;
using AutoRenovatioNS.Models;

using Core.Database;
using Core.Database.ServiceDbModels;
using Core.Events;
using Core.Services;

using Main.Models;
using Main.Views;

using MaterialDesignThemes.Wpf;

using Theme = Core.Services.SettingsModel.Theme;

namespace Main.ViewModels;

public class MainViewModel : BindableBase
{
    private readonly IModuleManager _moduleManager;
    private readonly IEventAggregator _eventAggregator;

    private int _lastRefreshed = 0;

    public int LastRefreshed
    {
        get => _lastRefreshed;
        set => SetProperty(ref _lastRefreshed, value);
    }

    public static InterTabClient InterTabClientInstance => new();
    public static InterLayoutClient InterLayoutClientInstance => new();

    public static string Version => Assembly.GetExecutingAssembly()
        .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
        .Select(x => x.InformationalVersion)
        .First();

    public ObservableCollection<Client> ClientsList { get; set; }
    public ObservableCollection<ModuleAction> ModulesButtons { get; set; }

    public DelegateCommand ChangeThemeCommand { get; private set; }
    public DelegateCommand UpdateClientsCommand { get; private set; }
    public AsyncDelegateCommand CheckUpdatesCommand { get; private set; }

    public MainViewModel(IModuleManager moduleManager, IEventAggregator eventAggregator)
    {
        _moduleManager = moduleManager;
        _eventAggregator = eventAggregator;

        // Reloj que refresca la lista de clientes cada 30 minutos
        DispatcherTimer refreshTimer = new()
        {
            Interval = TimeSpan.FromMinutes(30)
        };
        refreshTimer.Tick += RefreshTimer;
        refreshTimer.Start();

        // Reloj que actualiza el tiempo pasado desde la ultima actualizacion
        DispatcherTimer updateTime = new()
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        updateTime.Tick += UpdateTime;
        updateTime.Start();

        ChangeThemeCommand = new(SwitchTheme);
        UpdateClientsCommand = new(UpdateClients);
        CheckUpdatesCommand = new(CheckUpdates);

        ClientsList = [.. new ServiceDbContext().EstadoCliente];

        _moduleManager.Run();
        ModulesButtons =
            [.. SettingsService.Instance.Modules
                    .Select(m => new ModuleAction(m, new DelegateCommand<string>(LoadModule)))];
    }

    private void UpdateTime(object? sender, EventArgs args)
    {
        LastRefreshed++;
    }

    private void RefreshTimer(object? sender, EventArgs args)
    {
        UpdateClients();
    }

    private void UpdateClients()
    {
        LastRefreshed = 0;
        ClientsList.Clear();

        var dbContext = new ServiceDbContext();
        foreach (var client in dbContext.EstadoCliente)
        {
            ClientsList.Add(client);
        }
    }

    private static void SwitchTheme()
    {
        SettingsService.Instance.Theme = SettingsService.Instance.Theme switch
        {
            Theme.Dark => Theme.Light,
            Theme.Light => Theme.Dark,
            _ => throw new NotImplementedException()
        };

        App.ChangeTheme(SettingsService.Instance.Theme);
        SettingsService.Save();
    }

    private void LoadModule(string moduleName)
    {
        if (_moduleManager.ModuleExists(moduleName))
        {
            if (!_moduleManager.IsModuleInitialized(moduleName))
            {
                _moduleManager.LoadModule(moduleName);
            }
            _eventAggregator.GetEvent<LoadModuleEvent>().Publish(moduleName);
        }
    }

    private async Task CheckUpdates()
    {
        var updater = new AutoRenovatio(
            new ApplicationInfo("Canelary"),
            new DefaultUpdate(Version),
            SettingsService.Instance.UpdateUrl
        );

        var info = await updater.CheckForUpdatesAsync();
        if (info != null)
        {
            var view = new UpdateDialog()
            {
                DataContext = new UpdateDialogViewModel(updater, info, Version)
            };

            await DialogHost.Show(view, "UpdateDialog");
        }
    }
}