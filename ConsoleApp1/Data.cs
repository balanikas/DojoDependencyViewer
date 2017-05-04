using System.Collections.Generic;

namespace DojoDependencyViewer
{
    class Data
    {
        public List<string> packageNames { get; set; } = new List<string>();
        public List< List<int>> matrix { get; set; } = new List<List<int>>();
    }
}