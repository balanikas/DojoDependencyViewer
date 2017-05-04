using System.Collections.Generic;

namespace DojoDependencyViewer
{
    class ClassInfo
    {
        public string Name { get; set; }
        public List<string> Dependencies { get; set; } = new List<string>();
    }
}