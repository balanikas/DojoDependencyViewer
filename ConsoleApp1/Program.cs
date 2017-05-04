using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DojoDependencyViewer
{
  
    class Program
    {
        private static string _workingDirectory;
        private static string _directory;

        static void Main(string[] args)
        {
            _directory = args[0];
            _workingDirectory = Path.Combine(Path.GetTempPath(), "dojodependencyviewer");

            var classInfos = ParseClasses();

            var data = CreateData(classInfos);

            ClearWorkingDirectory();

            DeployResource("index.html");
            DeployResource("d3.min.js");
            DeployResource("d3.dependencyWheel.js");

            WriteDataToJavascriptFile(data);

            Process.Start(Path.Combine(_workingDirectory, "index.html"));
        }


        static void DeployResource(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DojoDependencyViewer.frontend." + resource))
            {
                using (var fileStream = File.Create(Path.Combine(_workingDirectory, resource)))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        static void ClearWorkingDirectory()
        {
            var directoryInfo = new DirectoryInfo(_workingDirectory);

            if (!Directory.Exists(_workingDirectory))
            {
                Directory.CreateDirectory(_workingDirectory);
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (var directory in directoryInfo.GetDirectories())
            {
                directory.Delete(true);
            }
        }

        static Data CreateData(List<ClassInfo> classInfos)
        {
            var data = new Data {packageNames = classInfos.Select(x => x.Name).ToList()};

            foreach (var classInfo in classInfos)
            {
                var matrixRow = new int[data.packageNames.Count];
                foreach (var dependency in classInfo.Dependencies)
                {
                    var index = data.packageNames.IndexOf(dependency);
                    if (index == -1)
                    {
                        continue;
                    }
                    matrixRow[index] = 1;
                }

                data.matrix.Add(matrixRow.ToList());
            }
            return data;
        }

        static List<ClassInfo> ParseClasses()
        {
            var classInfos = new List<ClassInfo>();
            foreach (var filePath in Directory.EnumerateFiles(_directory, "*.js", SearchOption.AllDirectories).ToList())
            {
                foreach (var fileLine in File.ReadLines(filePath))
                {
                    if (fileLine.Contains("return declare"))
                    {
                        var baseClasses = FindBaseClasses(fileLine);

                        classInfos.Add(new ClassInfo
                        {
                            Name = Path.GetFileNameWithoutExtension(filePath),
                            Dependencies = baseClasses
                        });

                        foreach (var baseClass in baseClasses.Where(x => x != ""))
                        {
                            if (!classInfos.Any(x => x.Name == baseClass))
                            {
                                classInfos.Add(new ClassInfo
                                {
                                    Name = baseClass,
                                    Dependencies = new List<string>()
                                });
                            }

                        }
                        break;
                    }
                }
            }
            return classInfos.OrderBy(x => x.Name).ToList();
        }

        static List<string> FindBaseClasses(string codeLine)
        {
            var baseClasses = Regex.Match(codeLine, @"(?<=\[).+?(?=\])").Value.Split(',');
            for (var index = 0; index < baseClasses.Length; index++)
            {
                baseClasses[index] = baseClasses[index].Trim();
            }

            return baseClasses.ToList();
        }

        static void WriteDataToJavascriptFile(Data chartData)
        {
            File.WriteAllText(Path.Combine(_workingDirectory, "data.js"), "var data = " + JsonConvert.SerializeObject(chartData, Formatting.Indented) + ";");
        }
    }
}
