namespace WpfControls.CS.Test
{

    using System.Collections.Generic;
    using System.IO;
    using Editors;

    public class FilesystemSuggestionProvider : ISuggestionProvider
    {

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }
            if (filter.Length < 3)
            {
                return null;
            }

            if (filter[1] != ':')
            {
                return null;
            }

            var lst = new List<FileSystemInfo>();
            var dirFilter = "*";
            var dirPath = filter;
            if (!filter.EndsWith("\\"))
            {
                var index = filter.LastIndexOf("\\", System.StringComparison.Ordinal);
                dirPath = filter.Substring(0, index + 1);
                dirFilter = filter.Substring(index + 1) + "*";
            }
            var dirInfo = new DirectoryInfo(dirPath);
            lst.AddRange(dirInfo.GetDirectories(dirFilter));
            lst.AddRange(dirInfo.GetFiles(dirFilter));
            return lst;
        }

    }

}
