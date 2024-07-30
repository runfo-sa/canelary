using Core.Database;
using Core.Database.ServiceDbModels;
using Core.Events;
using Core.Git;
using Core.Models;
using Core.Services;
using Core.Services.SettingsModel;
using Main.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;

namespace Main.ViewModels
{
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

        public GitTag GitTag { get; set; }
        public string GitTagUri { get; set; } = string.Empty;
        public ObservableCollection<Client> ClientsList { get; set; }
        public ObservableCollection<ModuleAction> ModulesButtons { get; set; }

        public DelegateCommand ChangeThemeCommand { get; private set; }
        public DelegateCommand UpdateClientsCommand { get; private set; }

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

            var tag = Git.GetLastTag();
            if (tag is not null)
            {
                GitTag = (GitTag)tag;
                GitTagUri = Path.Combine(SettingsService.Instance.GitRepo, $"releases/tag/{GitTag.Tag}");
            }

            ChangeThemeCommand = new(ChangeTheme);
            UpdateClientsCommand = new(UpdateClients);

            ClientsList = [.. new ServiceDbContext().EstadoCliente];

            _moduleManager.Run();
            ModulesButtons = [
                .. _moduleManager.Modules
                .Select(m =>
                {
                    var metadata = ModuleMetadata.Parse(m.ModuleName);
                    return new ModuleAction(metadata, new DelegateCommand<string>(LoadModule));
                })
                .Where(m => m.Metadata.AsButton)
                .OrderBy(m => m.Metadata.Position)
            ];
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

        private void ChangeTheme()
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
    }
}
