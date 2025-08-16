using System.Text.RegularExpressions;

namespace Broadcast.Classes;

public class SemVer : IComparable<SemVer>
{
    private static readonly Regex SemVerRegex = new(
        @"^v?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)" +
        @"(?:-(?<prerelease>[0-9A-Za-z\-\.]+))?" +
        @"(?:\+(?<build>[0-9A-Za-z\-\.]+))?$",
        RegexOptions.Compiled);

    public SemVer(int major, int minor, int patch, string[]? preRelease = null, string[]? buildMetadata = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease ?? Array.Empty<string>();
        BuildMetadata = buildMetadata ?? Array.Empty<string>();
    }

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string[] PreRelease { get; }
    public string[] BuildMetadata { get; }

    public int CompareTo(SemVer? other)
    {
        if (other == null) return 1;

        var result = Major.CompareTo(other.Major);
        if (result != 0) return result;

        result = Minor.CompareTo(other.Minor);
        if (result != 0) return result;

        result = Patch.CompareTo(other.Patch);
        if (result != 0) return result;

        // Pre-release comparison
        var thisHasPre = PreRelease.Length > 0;
        var otherHasPre = other.PreRelease.Length > 0;

        if (!thisHasPre && !otherHasPre) return 0;
        if (!thisHasPre) return 1; // No pre-release means higher precedence
        if (!otherHasPre) return -1;

        for (var i = 0; i < Math.Max(PreRelease.Length, other.PreRelease.Length); i++)
        {
            if (i >= PreRelease.Length) return -1;
            if (i >= other.PreRelease.Length) return 1;

            var a = PreRelease[i];
            var b = other.PreRelease[i];

            var aIsNum = int.TryParse(a, out var aNum);
            var bIsNum = int.TryParse(b, out var bNum);

            if (aIsNum && bIsNum)
            {
                result = aNum.CompareTo(bNum);
                if (result != 0) return result;
            }
            else if (aIsNum)
            {
                return -1;
            }
            else if (bIsNum)
            {
                return 1;
            }
            else
            {
                result = string.Compare(a, b, StringComparison.Ordinal);
                if (result != 0) return result;
            }
        }

        return 0;
    }

    public static SemVer Parse(string version)
    {
        var match = SemVerRegex.Match(version);
        if (!match.Success)
            throw new FormatException($"Invalid SemVer string: {version}");

        var major = int.Parse(match.Groups["major"].Value);
        var minor = int.Parse(match.Groups["minor"].Value);
        var patch = int.Parse(match.Groups["patch"].Value);

        var preRelease = match.Groups["prerelease"].Success
            ? match.Groups["prerelease"].Value.Split('.')
            : Array.Empty<string>();

        var buildMetadata = match.Groups["build"].Success
            ? match.Groups["build"].Value.Split('.')
            : Array.Empty<string>();

        return new SemVer(major, minor, patch, preRelease, buildMetadata);
    }

    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (PreRelease.Length > 0)
            version += "-" + string.Join(".", PreRelease);
        if (BuildMetadata.Length > 0)
            version += "+" + string.Join(".", BuildMetadata);
        return version;
    }
}