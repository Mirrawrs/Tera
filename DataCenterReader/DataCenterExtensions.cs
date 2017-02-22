using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tera.Analytics
{
    public static class DataCenterExtensions
    {
        private static XStreamingElement ToXStreamingElement(DataCenterElement element)
        {
            var xElement = new XStreamingElement(element.Name);
            xElement.Add(element.Attributes.Select(attribute => new XAttribute(attribute.Name, attribute.Value)));
            xElement.Add(element.Children.Select(ToXStreamingElement));
            return xElement;
        }

        /// <summary>
        ///     Exports the content of the specified element in XML format.
        /// </summary>
        /// <param name="element">The element to export.</param>
        /// <param name="outputPath">The path of the file that will contain the exported content.</param>
        public static void Export(this DataCenterElement element, string outputPath)
        {
            var xElement = ToXStreamingElement(element);
            using (var file = File.CreateText(outputPath))
            {
                xElement.Save(file);
            }
        }

        /// <summary>
        ///     Exports the contents of the Data Center in XML format.
        /// </summary>
        /// <param name="dataCenter">The Data Center to export.</param>
        /// <param name="outputPath">The path of the directory that will contain the exported content.</param>
        public static void Export(this DataCenter dataCenter, string outputPath)
        {
            var directory = Directory.CreateDirectory(outputPath);
            var groups = dataCenter.Root.Children.GroupBy(child => child.Name);
            foreach (var group in groups)
                if (group.Count() > 1)
                {
                    var groupDirectory = directory.CreateSubdirectory(group.Key);
                    var elementsAndFileNames = group.Select((element, index) => new
                    {
                        element,
                        fileName = $"{element.Name}_{index}.xml"
                    });
                    foreach (var o in elementsAndFileNames)
                    {
                        var fileName = Path.Combine(groupDirectory.FullName, o.fileName);
                        o.element.Export(fileName);
                    }
                }
                else
                {
                    var element = group.Single();
                    var fileName = Path.Combine(directory.FullName, $"{element.Name}.xml");
                    element.Export(fileName);
                }
        }
    }
}