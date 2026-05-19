using FileClickerSimple;
using Action = FileClickerSimple.Action;

var fileName = "clicker.txt";
var count = 0;

if (File.Exists(fileName))
{
    count = int.Parse(File.ReadAllText(fileName));
}

while (true)
{
    Console.Clear();
    Console.WriteLine($"Count: {count}");
    Console.WriteLine("Space = +1, R = reset, Esc = avslutt");

    var key = Console.ReadKey(true);

    var result = ClickerService.HandleKey(key.Key, count);

    switch (result.Action)
    {
        case Action.ApplicationExit: 
            return;
        case Action.SaveToFile: 
            File.WriteAllText(fileName, result.Count.ToString());
            break;
    };

    //if (key.Key == ConsoleKey.Escape)
    //{
    //    break;
    //}

    //if (key.Key == ConsoleKey.Spacebar)
    //{
    //    count++;
    //    File.WriteAllText(fileName, count.ToString());
    //}
    //else if (key.Key == ConsoleKey.R)
    //{
    //    count = 0;
    //    File.WriteAllText(fileName, count.ToString());
    //}
}