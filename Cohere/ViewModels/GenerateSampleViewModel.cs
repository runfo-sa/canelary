using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Data;

using Cohere.Models;

using Core.Database;
using Core.FileTree;
using Core.Helpers;
using Core.Services;
using Core.Services.BackendModel;

using Microsoft.IdentityModel.Tokens;

namespace Cohere.ViewModels;

public class GenerateSampleViewModel : BindableBase, IDialogAware
{
    public static string Title => "Generar Muestra";

    private const string CACHE_FILE = "after_command.cache";
    private const string TO_PNG = "To PNG";

    private readonly IDialogService _dialogService;
    private IFile _labelFile = null!;
    private string? _cachedAfterCommand = null;

    public ListCollectionView Printers { get; } = new(
        PrinterSettings.InstalledPrinters
            .Cast<string>()
            .Append(TO_PNG)
            .ToList()
        );

    public ObservableCollection<ProductoMuestra> ProductsList { get; set; } = [];

    public bool SelectAll
    {
        get;
        set
        {
            SetProperty(ref field, value);
            SelectedAll();
        }
    }

    public bool EnableRecall
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    public string Printer
    {
        get;
        set => SetProperty(ref field, value);
    } = new PrinterSettings().PrinterName;

    public DateTime Fecha
    {
        get;
        set => SetProperty(ref field, value);
    } = DateTime.Now;

    public string AfterCommand
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public DelegateCommand CloseDialogCommand { get; private set; }

    public DialogCloseListener RequestClose { get; }

    public GenerateSampleViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        CloseDialogCommand = new(async () => await ClosingDialog());
        LoadCachedAfterCommand();
        AfterCommand = _cachedAfterCommand ?? string.Empty;
    }

    private void SelectedAll()
    {
        foreach (var item in ProductsList)
        {
            if (item.Enable)
            {
                item.Printable = SelectAll;
            }
        }
    }

    private async Task PrintLabelsAsync(IEnumerable<ProductoMuestra> products)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 4
        };

        await Parallel.ForEachAsync(products, parallelOptions, async (prod, cancellationToken) =>
        {
            if (prod.Printable)
            {
                var label = PreviewServiceProvider
                    .ProvideService(_labelFile.Read())
                    .ParseMetadata()
                    .LoadVariables(prod.Id, Fecha.ToString("yyyyMMdd"));
                var safeSenasa = new string([.. prod.Senasa.Select(c => invalidChars.Contains(c) ? '_' : c)]);
                var printer = Printers.CurrentItem.ToString()!;
                if (printer == TO_PNG)
                {
                    string outputPath = @".\output";

                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                    }

                    var images = await label.Build("12", "3.950x5.950"); // TODO!: Let the user set this parameters
                    GenerateImage(images, $"{outputPath}\\{safeSenasa} - {prod.Id}");
                }
                else
                {
                    PrinterHelper.SendStringToPrinter(printer, label.Content, $"{_labelFile.Name} - {prod.Id}");
                }
            }
        });

        if (!AfterCommand.IsNullOrEmpty())
        {
            RunAfterCommand();
        }
    }

    private static void GenerateImage(List<byte[]?>? imageBytesList, string outputPath)
    {
        if (imageBytesList == null || imageBytesList.Count == 0)
        {
            return;
        }

        try
        {
            int idx = 0;
            foreach (var bytes in imageBytesList)
            {
                idx++;

                if (bytes == null || bytes.Length == 0)
                {
                    continue;
                }

                using MemoryStream ms = new(bytes);
                using Bitmap bitmap = new(ms);
                bitmap.Save($"{outputPath}_{idx}.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError($"{ex.Message} --- {outputPath}");
        }
    }

    private void RunAfterCommand()
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/C {AfterCommand}",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using Process process = Process.Start(startInfo)!;
            process.WaitForExit();
            SaveCachedAfterCommand();
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error executing after command: {ex.Message}");
        }
    }

    private void LoadCachedAfterCommand()
    {
        _cachedAfterCommand = File.Exists(CACHE_FILE) ? File.ReadAllText(CACHE_FILE) : null;
    }

    private void SaveCachedAfterCommand()
    {
        if (!string.IsNullOrEmpty(AfterCommand) && _cachedAfterCommand != AfterCommand)
        {
            File.WriteAllText(CACHE_FILE, AfterCommand);
        }
    }

    private void GenerateRecall(IEnumerable<ProductoMuestra> products)
    {
        var param = new DialogParameters
        {
            { "Products", products }
        };
        _dialogService.ShowDialog("GenerateRecallDialog", param);
    }

    private async Task ClosingDialog()
    {
        if (EnableRecall)
        {
            GenerateRecall(ProductsList.Where(p => p.Printable));
        }
        await PrintLabelsAsync(ProductsList);
        RequestClose.Invoke();
    }

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters.TryGetValue("File", out IFile? file) && file is not null)
        {
            _labelFile = file;

            //using var context = new IdeDbContext();
            //var labelName = Path.GetFileNameWithoutExtension(_labelFile.Name);
        }

        if (parameters.TryGetValue("Products", out IEnumerable<Product>? products) && products is not null)
        {
            var p = products.Select(p => new ProductoMuestra(p));
            foreach (var item in p)
            {
                ProductsList.Add(item);
            }
        }
    }
}