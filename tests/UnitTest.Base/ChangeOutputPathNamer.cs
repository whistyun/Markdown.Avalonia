using ApprovalTests.Namers;
using System.IO;

namespace UnitTest.Base
{
    public class ChangeOutputPathNamer : UnitTestFrameworkNamer
    {
        private string dir;


        public override string SourcePath
        {
            get
            {
                var basePath = base.SourcePath;
                return Path.Combine(basePath, dir);
            }
        }

        public ChangeOutputPathNamer(string dir)
        {
            this.dir = dir;
        }
    }
}
