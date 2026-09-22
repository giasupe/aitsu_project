while (true)
{
    string? input = Console.ReadLine();
    if (input is null || input == "/exit")
    {
        break;
    }
    Console.WriteLine("input: " + input);
}