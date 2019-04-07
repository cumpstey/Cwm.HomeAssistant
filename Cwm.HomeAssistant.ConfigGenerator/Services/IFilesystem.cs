using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cwm.HomeAssistant.Config.Services
{
    public interface IFilesystem
    {
        bool FileExists(string path);

        Task<string> ReadFileAsync(string path);

        Task WriteFileAsync(string path, string content);

        IEnumerable<string> EnumerateFiles(string path, string searchPattern);
    }
}
