using System.IO;
using System.Reflection;
using System.Windows.Markup;
using System.Xml;

namespace MdXamlTest
{
    static class Utils
    {
#if !MIG_FREE
        const string ResourceKey = "MdXaml.Test.Md." ;
#else
        const string ResourceKey = "MdXaml.TestMigfree.Md.";
#endif

        public static string LoadText(string name)
        {

            using (Stream stream = Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream(ResourceKey + name))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string AsXaml(object instance)
        {
            using (var writer = new StringWriter())
            {
                var settings = new XmlWriterSettings { Indent = true };
                using (var xmlWriter = XmlWriter.Create(writer, settings))
                {
                    XamlWriter.Save(instance, xmlWriter);
                }

                writer.WriteLine();
                return writer.ToString();
            }
        }
    }
}
