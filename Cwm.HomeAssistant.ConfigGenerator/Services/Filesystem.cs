using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cwm.HomeAssistant.Config.Services
{
    public class Filesystem : IFileProvider
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public async Task<string> ReadFileAsync(string path)
        {
            return await File.ReadAllTextAsync(path);
        }

        public async Task WriteFileAsync(string path, string contents)
        {
            await File.WriteAllTextAsync(path, contents);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern);
        }
    }
}
