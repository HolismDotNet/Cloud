namespace Infra;

public class BlobProxyController : HolismController
{
    public static HttpClient http = new HttpClient();

    [HttpGet]
    public async Task<IActionResult> Get(string url)
    {
        url.Ensure().IsUrl($"{url} is not a valid URL");
        var response = await http.GetAsync(url);
        var stream = await response.Content.ReadAsStreamAsync();
        if (stream == null)
        {
            return NotFound();
        }
        return File(stream, response.Content.Headers.ContentType.MediaType);
    }
}