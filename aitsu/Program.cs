while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();

    if (input == "/exit")
    {
        break;
    }
    Console.WriteLine($"You: {input}");
}