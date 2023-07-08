using ApprovalTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using UnitTest.Base.Apps;
using UnitTest.Base.Utils;

namespace UnitTest.Base
{
    public class UnitTestBase
    {
        static UnitTestBase()
        {
            var fwNm = Util.GetRuntimeName();
            Approvals.RegisterDefaultNamerCreation(() => new ChangeOutputPathNamer("Out"));

        }

        IDisposable disposable;

        protected string AssetPath;

        public UnitTestBase()
        {
            var asm = Assembly.GetExecutingAssembly();
            AssetPath = Path.GetDirectoryName(asm.Location);
            disposable = App.Start();
        }

        [SetUp]
        public void WaitApplicationStart()
        {
            Debug.Print("Begin WaitApplicationStart");
            while (!App.ApplicationStarted)
                Thread.Sleep(10);
            Debug.Print("End WaitApplicationStart");
        }
    }
}
