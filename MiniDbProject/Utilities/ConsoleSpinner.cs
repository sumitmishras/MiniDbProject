using MiniDbProject.Constants;

namespace MiniDbProject.Utilities;

public class ConsoleSpinner : IDisposable
{
    private readonly string[] _sequence = { "\u258F", "\u258E", "\u258D", "\u258C", "\u258B", "\u258A", "\u2589", "\u2588" };
    private readonly string _message;
    private readonly ConsoleColor _color;
    private int _counter;
    private bool _isRunning;
    private Thread? _thread;
    private readonly int _left;
    private readonly int _top;

    public ConsoleSpinner(string message = "", ConsoleColor color = ConsoleColor.Cyan)
    {
        _message = message;
        _color = color;
        try
        {
            _left = Console.CursorLeft;
            _top = Console.CursorTop;
        }
        catch
        {
            _left = 0;
            _top = 0;
        }
    }

    public void Start()
    {
        _isRunning = true;
        _thread = new Thread(Spin)
        {
            IsBackground = true
        };
        _thread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _thread?.Join(200);
        ClearLine();
    }

    private void Spin()
    {
        while (_isRunning)
        {
            try
            {
                Console.SetCursorPosition(_left, _top);
                Console.ForegroundColor = _color;
                Console.Write($" {_sequence[_counter % _sequence.Length]} {_message}  ");
                Console.ResetColor();
                _counter++;
                Thread.Sleep(100);
            }
            catch { break; }
        }
    }

    private void ClearLine()
    {
        try
        {
            if (_left < 0 || _top < 0) return;
            Console.SetCursorPosition(_left, _top);
            int width = 80;
            try { width = Console.WindowWidth; } catch { }
            Console.Write(new string(' ', Math.Max(1, width - _left - 1)));
            Console.SetCursorPosition(_left, _top);
        }
        catch { }
    }

    public void Dispose()
    {
        Stop();
    }
}
