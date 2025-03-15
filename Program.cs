using PuppeteerSharp;
using System.Text.Json;

Console.Write("URL: ");
string url = Console.ReadLine();
string chrome = @"C:\Program Files\Google\Chrome\Application\Chrome.exe";

BrowserFetcher browserFetcher = new();
await browserFetcher.DownloadAsync();

await using var browser = await Puppeteer.LaunchAsync(
    new LaunchOptions
    {
        Headless = true,
        ExecutablePath = chrome
    }
);

var hashsetRequested = new HashSet<string>();

var links = await Get(url, browser);
hashsetRequested.Add(url);

foreach (var link in links)
{
    Console.WriteLine(link);
}

async Task<List<string>> Get(string url, IBrowser browser)
{
    List<string> links = new List<string>();
    Console.WriteLine("Solicitud a: " + url);

    await using var page = await browser.NewPageAsync();
    var res = await page.GoToAsync(url);

    if (res.Status == System.Net.HttpStatusCode.OK)
    {
        var result = await page.EvaluateFunctionAsync<JsonElement>("()=>{" +
            "const a = document.querySelectorAll('a');" +
            "const res = [];" +
            "for(let i=0; i<a.length; i++)" +
            "   res.push(a[i].href);" +
            "return res;" +
            "}");

        if (result.ValueKind == JsonValueKind.Array)
        {
            foreach (var e in result.EnumerateArray())
            {
                links.Add(e.GetString());
            }
        }
    }

    return links;
}
