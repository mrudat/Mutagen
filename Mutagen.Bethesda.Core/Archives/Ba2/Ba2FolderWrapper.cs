using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Ba2
{
    class Ba2FolderWrapper : IArchiveFolder
    {
        public IReadOnlyCollection<IArchiveFile> Files { get; }

        public string? Path { get; }

        public Ba2FolderWrapper(string path, IEnumerable<IArchiveFile> files)
        {
            Path = path;
            Files = files.ToList();
        }
    }
}
