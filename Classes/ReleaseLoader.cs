namespace Broadcast.Classes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class ReleaseListItem
{
    public string Repo { get; set; }
    public string Version { get; set; }
    public bool IsLatest { get; set; }
    public string ZipName { get; set; }
    public string DownloadUrl { get; set; }
    public string ReadMeUrl { get; set; }
    public override string ToString() => $"{Repo} - {Version}";
}

public class ReleaseInfo
{
    public string Repo { get; set; }
    public string Tag { get; set; }
    public string Name { get; set; }
    public string ReadMeUrl { get; set; }
    public DateTime Published { get; set; }
    public bool IsLatest { get; set; }
    public List<ZipFile> ZipFiles { get; set; }
}

public class ZipFile
{
    public string Name { get; set; }
    public string Url { get; set; }
}

public class ReleaseService
{
    public Dictionary<string, List<ReleaseInfo>> Releases { get; private set; }

    private ReleaseService(Dictionary<string, List<ReleaseInfo>> releases)
    {
        Releases = releases;
    }

    public static async Task<ReleaseService> CreateAsync( string url )
    {
        var loader = new ReleaseLoader();
        var releases = await loader.LoadReleasesAsync( url );
        return new ReleaseService(releases);
    }

    public void print()
    {
        foreach (var repo in Releases.Keys)
        {
            Debug.WriteLine($"Repo: {repo}");
            foreach (var release in Releases[repo])
            {
                Debug.WriteLine($"  - {release.Tag} ({(release.IsLatest ? "Latest" : "Old")})");
                foreach (var zip in release.ZipFiles)
                {
                    Debug.WriteLine($"    • {zip.Name} → {zip.Url}");
                }
            }
        }
    }

    public IEnumerable<ReleaseListItem> GetReleaseItems()
    {
        foreach (var repo in Releases.Keys)
        {
            foreach (var release in Releases[repo])
            {
                foreach (var zip in release.ZipFiles)
                {
                    yield return new ReleaseListItem
                    {
                        Repo = repo,
                        Version = release.Tag,
                        IsLatest = release.IsLatest,
                        ZipName = zip.Name,
                        DownloadUrl = zip.Url,
                        ReadMeUrl = release.ReadMeUrl
                    };
                }
            }
        }
    }
}

public class ReleaseLoader
{
    public async Task<Dictionary<string, List<ReleaseInfo>>> LoadReleasesAsync( string JsonUrl)
    {
        using var httpClient = new HttpClient();
        var json = await httpClient.GetStringAsync(JsonUrl);

        var releases = JsonSerializer.Deserialize<List<ReleaseInfo>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var dict = new Dictionary<string, List<ReleaseInfo>>();

        foreach (var release in releases)
        {
            if (!dict.ContainsKey(release.Repo))
                dict[release.Repo] = new List<ReleaseInfo>();

            dict[release.Repo].Add(release);
        }

        return dict;
    }
}


