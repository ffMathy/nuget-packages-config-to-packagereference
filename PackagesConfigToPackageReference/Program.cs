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
      var packageConfigFiles = Directory.GetFiles(Environment.CurrentDirectory, "packages.config", SearchOption.AllDirectories);
      foreach(var packageConfigFile in packageConfigFiles)
      {
        var folder = Path.GetDirectoryName(packageConfigFile);
        var csprojFile = Directory.GetFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly).Single();

        var csprojXml = XElement.Load(csprojFile);
        var packageConfigXml = XElement.Load(packageConfigFile);

        var packages = packageConfigXml.Elements("package");
        foreach(var package in packages)
        {
          var id = package.Attribute(XName.Get("id")).Value;
          var version = package.Attribute(XName.Get("version")).Value;

          var packageReference = new XElement(
            XName.Get("PackageReference"),
            new XElement(
              XName.Get("Version"), version));
          packageReference.Add(new XAttribute(XName.Get("Include"), id));
          csprojXml.Add(packageReference);
        }

        csprojXml.Save(csprojFile);

        File.WriteAllText(csprojFile,
          File.ReadAllText(csprojFile)
            .Replace(" xmlns=\"\"", ""));
      }
    }
  }
}
