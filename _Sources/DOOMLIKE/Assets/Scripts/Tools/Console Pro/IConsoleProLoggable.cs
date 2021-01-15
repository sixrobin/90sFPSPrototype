namespace Doomlike
{
    public interface IConsoleProLoggable
    {
        bool ConsoleProMuted { get; }

        string ConsoleProPrefix { get; }
    }
}