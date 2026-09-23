using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

using var HttpClient = new HttpClient();

var tags = await HttpClient.GetAsync(
    "http://localhost:11434/api/tags");
tags.EnsureSuccessStatusCode();

var tagsList = JsonDocument.Parse(
    await tags.Content.ReadAsStringAsync());

for (int i = 0; i < tagsList.RootElement.GetProperty("models").GetArrayLength(); i++)
{
    var outputModelList = tagsList.RootElement
    .GetProperty("models")[i]
    .GetProperty("name")
    .GetString();
    System.Console.WriteLine($"model: {outputModelList}");
};

while (true)
{
    string? input = Console.ReadLine();

    if (input is null || input == "/exit")
        break;

    var response = await HttpClient.PostAsJsonAsync(
        "http://localhost:11434/api/chat",
        new // 匿名オブジェクトを作成
        {
            model = "gemma4:26b",
            messages = new[] // messagesに配列を入れる
            {
                new // 配列に入れる匿名オブジェクトを作成
                {
                    role = "user",
                    content = input
                }
            },
            stream = false
        });

    System.Console.WriteLine($"status: {(int)response.StatusCode} {response.StatusCode}");

    var json = JsonDocument.Parse(
        await response.Content.ReadAsStringAsync()
    );
    
    var output = json.RootElement
        .GetProperty("message")
        .GetProperty("content")
        .GetString();
    System.Console.WriteLine($"output: {output}");
}