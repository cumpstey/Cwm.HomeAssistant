using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cwm.HomeAssistant.Config.Services
{
    public class DummyFilesystem : IFileProvider
    {
        #region Fields

        private IDictionary<string, string> _filesystem = new Dictionary<string, string>();

        #endregion

        #region Filesystem methods

        public bool FileExists(string path)
        {
            return _filesystem.ContainsKey(path);
        }

        public Task<string> ReadFileAsync(string path)
        {
            if(_filesystem.ContainsKey(path))
            {
                return Task.FromResult(_filesystem[path]);
            }

            var tcs = new TaskCompletionSource<string>();
            tcs.SetException(new FileNotFoundException("File not found", path));
            return tcs.Task;
        }

        public Task WriteFileAsync(string path, string contents)
        {
            _filesystem[path] = contents;
            return Task.CompletedTask;
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            // Convert the search pattern to something that _anything else at all_
            // can understand.
            var searchRegex = searchPattern.Replace(".", @"\.")
                                           .Replace("?", @"[^\\]")
                                           .Replace("*", @"[^\\]{0,}");
            var pathRegex = path.TrimEnd('\\').Replace(@"\", @"\\");
            var fullRegex= $@"^{pathRegex}\\{searchRegex}$";
            return _filesystem.Keys.Where(key => Regex.IsMatch(key, fullRegex));
        }

        #endregion
    }
}
