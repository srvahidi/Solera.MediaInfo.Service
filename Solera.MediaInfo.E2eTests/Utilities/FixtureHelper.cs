using System.IO;

namespace Solera.MediaInfo.E2eTests.Utilities
{
    public static class FixtureHelper
    {
        public static byte[] GetFixtureBytes(string name) => File.ReadAllBytes(GetFixturePath(name));

        private static string GetFixturePath(string name)
        {
            var location = new DirectoryInfo(Path.GetFullPath("."));
            while (location.Name != "Solera.MediaInfo.E2eTests")
            {
                location = location.Parent;
            }
            return Path.Combine(location.FullName, name);
        }
    }
}
