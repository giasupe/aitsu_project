while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();

    if (input is null ||
        input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    Console.WriteLine($"You: {input}");
}
