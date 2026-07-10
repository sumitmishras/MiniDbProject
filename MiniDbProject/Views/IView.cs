namespace MiniDbProject.Views;

public interface IView
{
    string Name { get; }
    Task ShowAsync();
}
