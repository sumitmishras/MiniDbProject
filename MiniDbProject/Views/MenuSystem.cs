using MiniDbProject.Constants;
using MiniDbProject.Helpers;
using MiniDbProject.Models;

namespace MiniDbProject.Views;

public class MenuSystem
{
    private readonly List<MenuItem> _items = new();
    private int _selectedIndex;
    private string _title = "";

    public MenuSystem(string title)
    {
        _title = title;
    }

    public void AddItem(string id, string label, string? description = null, ConsoleKey? shortcut = null, string? icon = null)
    {
        _items.Add(new MenuItem
        {
            Id = id,
            Label = label,
            Description = description,
            ShortcutKey = shortcut,
            Icon = icon
        });
    }

    public void AddSeparator()
    {
        _items.Add(new MenuItem { IsSeparator = true });
    }

    public async Task<string?> ShowAsync()
    {
        ConsoleKey key;
        do
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayBanner(AppConstants.AppName, AppConstants.AppVersion, AppConstants.Author);
            ConsoleRenderer.DrawMenuTitle(_title);

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].IsSeparator)
                {
                    ConsoleHelper.WriteLineColored($"    {AppConstants.Colors.Dim}{new string('\u2500', 50)}{AppConstants.Colors.Reset}");
                    continue;
                }

                string prefix = i == _selectedIndex ? $"{AppConstants.Colors.Highlight}>{AppConstants.Colors.Reset} " : "  ";
                string icon = _items[i].Icon ?? " ";
                string shortcut = _items[i].ShortcutKey.HasValue ? $" [{_items[i].ShortcutKey.Value}]" : $" [{i + 1}]";

                ConsoleHelper.WriteColored($"  {prefix}{AppConstants.Colors.Primary}{icon}{AppConstants.Colors.Reset}");
                if (i == _selectedIndex)
                {
                    ConsoleHelper.WriteColored($"{AppConstants.Colors.Bold}{AppConstants.Colors.White}{_items[i].Label}{AppConstants.Colors.Reset}");
                }
                else
                {
                    ConsoleHelper.WriteColored($"{AppConstants.Colors.Default}{_items[i].Label}{AppConstants.Colors.Reset}");
                }

                if (!string.IsNullOrEmpty(_items[i].Description))
                {
                    ConsoleHelper.WriteColored($"  {AppConstants.Colors.Dim}- {_items[i].Description}{AppConstants.Colors.Reset}");
                }
                ConsoleHelper.WriteLineColored("");
            }

            ConsoleRenderer.DrawFooter("Use \u2191\u2193 to navigate, Enter to select, ESC to go back");

            try
            {
                key = Console.ReadKey(true).Key;
            }
            catch
            {
                return null;
            }

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
                    while (_items[_selectedIndex].IsSeparator)
                        _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
                    break;
                case ConsoleKey.DownArrow:
                    _selectedIndex = (_selectedIndex + 1) % _items.Count;
                    while (_items[_selectedIndex].IsSeparator)
                        _selectedIndex = (_selectedIndex + 1) % _items.Count;
                    break;
                case ConsoleKey.Enter:
                    return _items[_selectedIndex].Id;
                case ConsoleKey.Escape:
                    return null;
                default:
                    if (key >= ConsoleKey.D1 && key <= ConsoleKey.D9)
                    {
                        int num = key - ConsoleKey.D1;
                        int idx = 0;
                        int count = 0;
                        foreach (var item in _items)
                        {
                            if (!item.IsSeparator)
                            {
                                if (count == num && idx < _items.Count)
                                {
                                    _selectedIndex = idx;
                                    return _items[_selectedIndex].Id;
                                }
                                count++;
                            }
                            idx++;
                        }
                    }
                    break;
            }
        } while (true);
    }
}
