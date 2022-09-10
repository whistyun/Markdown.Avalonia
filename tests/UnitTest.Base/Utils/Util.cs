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
            string resourceKey = caller.GetName().Name + ".Texts.";

            return caller.GetManifestResourceNames()
                         .Where(nm => nm.StartsWith(resourceKey))
                         .Select(nm => nm.Substring(resourceKey.Length))
                         .ToArray();
        }

        public static string LoadText(string name)
        {
            var caller = Assembly.GetCallingAssembly();
            string resourceKey = caller.GetName().Name + ".Texts.";

            using Stream stream = caller.GetManifestResourceStream(resourceKey + name)!;
            using StreamReader reader = new(stream);

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

        public static IEnumerable<T> FindControlsByClassName<T>(IControl ctrl, string classNm) where T : IControl
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
