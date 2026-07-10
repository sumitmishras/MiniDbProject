using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniDbWpf.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public string AppName { get; } = "Mini Database Creator";
    public string Version { get; } = "v2.0.0";
    public string Description { get; } = "Enterprise-grade database creation and management tool for local SQL Server instances.";
    public string TechStack { get; } = "WPF · .NET 10 · Material Design · MVVM · CommunityToolkit";
}
