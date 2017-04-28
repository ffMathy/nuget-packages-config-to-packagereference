using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PackagesConfigToPackageReference
{
    class Program
    {
        static void Main(string[] args)
        {
            var csprojFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj", SearchOption.AllDirectories);
            foreach (var csprojFile in csprojFiles)
            {
                var folder = Path.GetDirectoryName(csprojFile);
                var packagesConfigFile = Path.Combine(folder, "packages.config");

                var csprojXml = XElement.Load(csprojFile);

                if (File.Exists(packagesConfigFile))
                {
                    var packageConfigXml = XElement.Load(packagesConfigFile);

                    var packages = packageConfigXml.Elements("package");
                    foreach (var package in packages)
                    {
                        var id = package.Attribute(XName.Get("id")).Value;
                        var version = package.Attribute(XName.Get("version")).Value;

                        var packageReference = new XElement(
                            XName.Get("PackageReference"),
                            new XElement(
                                XName.Get("Version"), version));
                        packageReference.Add(new XAttribute(XName.Get("Include"), id));

                        var propertyGroupElements = csprojXml.Elements().Where(x => x.Name.LocalName == "ItemGroup");
                        var propertyGroupElement = propertyGroupElements.LastOrDefault();
                        if (propertyGroupElement == null)
                        {
                            propertyGroupElement = new XElement(XName.Get("ItemGroup"));
                            csprojXml.Add(propertyGroupElement);
                        }

                        propertyGroupElement.Add(packageReference);
                    }

                    File.Delete(packagesConfigFile);
                }

                var csprojXmlElements = csprojXml.Elements();
                foreach (var itemGroup in csprojXmlElements.Where(x => x.Name.LocalName == "ItemGroup"))
                {
                    var referenceElementsToRemove = itemGroup
                        .Elements()
                        .Where(x => x.Name.LocalName == "Reference")
                        .Where(x => x
                            .Elements()
                            .SingleOrDefault(y => y.Name.LocalName == "HintPath")
                            ?.Value
                            .Contains("packages") == true);
                    foreach (var element in referenceElementsToRemove)
                    {
                        element.Remove();
                    }
                }

                csprojXml.Save(csprojFile);

                File.WriteAllText(csprojFile,
                  File.ReadAllText(csprojFile)
                    .Replace(" xmlns=\"\"", ""));
            }
        }
    }
}
