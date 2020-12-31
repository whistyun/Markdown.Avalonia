using ApprovalTests.Core;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnitTest.CTxt.Utils
{
    public class ApprovalImageWriter : IApprovalWriter
    {
        public Bitmap Data { get; set; }

        public ApprovalImageWriter(Bitmap image)
        {
            Data = image;
        }

        public virtual string GetApprovalFilename(string basename)
        {
            return $"{basename}.approved.png";
        }

        public virtual string GetReceivedFilename(string basename)
        {
            return $"{basename}.received.png";
        }


        public string WriteReceivedFile(string received)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(received));
            Data.Save(received);
            return received;
        }
    }
}
