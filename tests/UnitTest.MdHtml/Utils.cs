using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Markup;
using System.Xml;
using UnitTest.Base;
using UnitTest.Base.Utils;

namespace UnitTest.MdHtml.Test
{
    public static class Utils
    {
        public static string ReadHtml([CallerMemberName] string fileBaseName = null)
        {
            if (fileBaseName is null)
                throw new ArgumentNullException(nameof(fileBaseName));

            return Util.LoadText(fileBaseName + ".html");
        }

        public static string AsXaml(object result)
        {
            return global::UnitTest.Base.Utils.Util.AsXaml(result);
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
    }
}
