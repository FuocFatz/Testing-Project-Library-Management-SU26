using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LibraryManagement.Data;
using LibraryManagement.Services;
using LibraryManagement.ViewModels;

namespace LibraryManagement;

public partial class App : Application
{
    public static LibraryDbContextFactory DbFactory { get; private set; } = null!;
    public static LibraryService LibraryService { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        DbFactory = new LibraryDbContextFactory(configuration.GetConnectionString("DefaultConnection")!);
        LibraryService = new LibraryService(DbFactory);

        try
        {
            await DatabaseInitializer.InitializeAsync(DbFactory);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể kết nối SQL Server.\\n\\nChi tiết: {ex.Message}\\n\\nHãy kiểm tra connection string trong appsettings.json.",
                "Library Management - Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }
}
