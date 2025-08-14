using System.Text.RegularExpressions;

namespace Broadcast.SubForms
{
    public class SemVer : IComparable<SemVer>
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string[] PreRelease { get; }
        public string[] BuildMetadata { get; }

        private static readonly Regex SemVerRegex = new Regex(
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

        public static SemVer Parse(string version)
        {
            var match = SemVerRegex.Match(version);
            if (!match.Success)
                throw new FormatException($"Invalid SemVer string: {version}");

            int major = int.Parse(match.Groups["major"].Value);
            int minor = int.Parse(match.Groups["minor"].Value);
            int patch = int.Parse(match.Groups["patch"].Value);

            string[] preRelease = match.Groups["prerelease"].Success
                ? match.Groups["prerelease"].Value.Split('.')
                : Array.Empty<string>();

            string[] buildMetadata = match.Groups["build"].Success
                ? match.Groups["build"].Value.Split('.')
                : Array.Empty<string>();

            return new SemVer(major, minor, patch, preRelease, buildMetadata);
        }

        public int CompareTo(SemVer? other)
        {
            if (other == null) return 1;

            int result = Major.CompareTo(other.Major);
            if (result != 0) return result;

            result = Minor.CompareTo(other.Minor);
            if (result != 0) return result;

            result = Patch.CompareTo(other.Patch);
            if (result != 0) return result;

            // Pre-release comparison
            bool thisHasPre = PreRelease.Length > 0;
            bool otherHasPre = other.PreRelease.Length > 0;

            if (!thisHasPre && !otherHasPre) return 0;
            if (!thisHasPre) return 1; // No pre-release means higher precedence
            if (!otherHasPre) return -1;

            for (int i = 0; i < Math.Max(PreRelease.Length, other.PreRelease.Length); i++)
            {
                if (i >= PreRelease.Length) return -1;
                if (i >= other.PreRelease.Length) return 1;

                string a = PreRelease[i];
                string b = other.PreRelease[i];

                bool aIsNum = int.TryParse(a, out int aNum);
                bool bIsNum = int.TryParse(b, out int bNum);

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

        public override string ToString()
        {
            string version = $"{Major}.{Minor}.{Patch}";
            if (PreRelease.Length > 0)
                version += "-" + string.Join(".", PreRelease);
            if (BuildMetadata.Length > 0)
                version += "+" + string.Join(".", BuildMetadata);
            return version;
        }
    }


}
