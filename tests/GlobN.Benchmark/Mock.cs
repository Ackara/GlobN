using System.IO;

namespace Acklann.GlobN
{
    public class Mock
    {
        public static readonly string RootDirectory = Path.Combine(Path.GetTempPath(), nameof(GlobN));
        private static string[] _fileList;

        public static string[] GetFileList()
        {
            if (_fileList?.Length > 0) return _fileList;

            _fileList = Directory.GetFiles(RootDirectory, "*", SearchOption.AllDirectories);
            return _fileList;
        }
    }
}