using Microsoft.Extensions.DependencyInjection;

namespace MauiBasics;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    // Tạo trang SAU InitializeComponent để StaticResource trong XAML đã tồn tại.
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_services.GetRequiredService<AppShell>());
}
