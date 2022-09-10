using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Markdown.Avalonia.Utils
{
    static class InterassemblyUtil
    {
        public static T? InvokeInstanceMethodToGetProperty<T>(string asmNm, string typeNm, string methodNm, params object[] methodArgs) where T : class
        {
            var asm = Assembly.Load(asmNm);
            var setupTp = asm.GetType(typeNm);
            if (setupTp is null)
                throw new NullReferenceException($"Failed to load '{typeNm}' in '{asm.FullName}'.");

            var method = setupTp.GetMethod(methodNm);
            if (method is null)
                throw new NullReferenceException($"'{methodNm}' method dosen't exist in '{typeNm}'.");

            if (method.IsStatic) return null;
            if (method.GetParameters().Length != methodArgs.Length) return null;

            var setupObj = Activator.CreateInstance(setupTp);
            var result = method.Invoke(setupObj, methodArgs);

            return result as T;
        }
    }
}
