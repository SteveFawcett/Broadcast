using System.Diagnostics;

namespace Broadcast.Classes;

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;


public static class StringDownloader
{
    public static async Task<string?> DownloadStringAsync(string url)
    {
        using var httpClient = new HttpClient();

        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error downloading string: {ex.Message}");
            return null;
        }
    }
}

public static class FileDownloader
{
    public static async Task DownloadFileAsync(string fileUrl, string destinationPath)
    {
        using var httpClient = new HttpClient();

        try
        {
            var response = await httpClient.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();

            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);

            Debug.WriteLine($"✅ Downloaded to: {destinationPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed to download file: {ex.Message}");
        }
    }
}
