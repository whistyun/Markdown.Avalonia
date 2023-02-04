using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace UnitTest.Base.Utils
{
    public static class Util
    {
        public static string[] GetTextNames()
        {
            var caller = Assembly.GetCallingAssembly();
            var resourceDir = Path.Combine(Path.GetDirectoryName(caller.Location), "Texts");

            return Directory.GetFiles(resourceDir)
                            .Select(path => Path.GetFileName(path))
                            .ToArray();
        }

        public static string LoadText(string name)
        {
            var caller = Assembly.GetCallingAssembly();
            var resourceFile = Path.Combine(Path.GetDirectoryName(caller.Location), "Texts", name);

            using var reader = File.OpenText(resourceFile);

            return reader.ReadToEnd();
        }

        public static string AsXaml(object instance)
        {
            using var writer = new StringWriter();
            var settings = new XmlWriterSettings { Indent = true };
            using (var xmlWriter = XmlWriter.Create(writer, settings))
            {
                var docGen = new BrokenXamlWriter();
                var docObj = docGen.Transform(instance);
                docObj.Save(xmlWriter);

                //XamlServices.Save(xmlWriter, instance);
                //XamlWriter.Save(instance, xmlWriter);
            }

            writer.WriteLine();
            return writer.ToString();

            //using (var writer = new StringWriter())
            //{
            //    var settings = new XmlWriterSettings { Indent = true };
            //    using (var xmlWriter = XmlWriter.Create(writer, settings))
            //    {
            //        XamlServices.Save(xmlWriter, instance);
            //    }
            //
            //    writer.WriteLine();
            //    return writer.ToString();
            //}
        }

        public static string GetRuntimeName()
        {
            var description = RuntimeInformation.FrameworkDescription.ToLower();
            // ".NET Framework"
            // ".NET Core"(for .NET Core 1.0 - 3.1)
            // ".NET Native"
            // ".NET"(for .NET 5.0 and later versions)

            if (description.Contains("framework"))
            {
                return "framework";
            }

            if (description.Contains("core"))
            {
                return "core";
            }

            if (description.Contains("native"))
            {
                return "native";
            }

            return "dotnet";
        }

        public static IEnumerable<T> FindControlsByClassName<T>(Control ctrl, string classNm) where T : Control
        {
            if (ctrl.Classes.Contains(classNm))
                yield return (T)ctrl;

            if (ctrl is Panel panel)
            {
                foreach (var rs in panel.Children.SelectMany(p => FindControlsByClassName<T>(p, classNm)))
                    yield return rs;
            }
        }
    }
}
