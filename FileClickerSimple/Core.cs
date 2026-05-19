namespace FileClickerSimple
{
    // DomainModel
    record HandleKeyResult(int Count, Action Action);

    enum Action {
        NoAction,
        ApplicationExit,
        SaveToFile,
    }

    // ApplicationServices
    internal class ClickerService
    {
        public static HandleKeyResult HandleKey(ConsoleKey key, int count)
        {
            return key switch
            {
                ConsoleKey.Escape => new HandleKeyResult(count, Action.ApplicationExit),
                ConsoleKey.Spacebar => new HandleKeyResult(count + 1, Action.SaveToFile),
                ConsoleKey.R => new HandleKeyResult(0, Action.SaveToFile),
                _ => new HandleKeyResult(count, Action.NoAction)
            };
        }
    }
}
