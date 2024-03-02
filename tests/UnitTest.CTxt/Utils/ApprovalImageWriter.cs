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
        public string? Dir { get; set; }
        public Bitmap Data { get; set; }
        public string? Suffix { get; set; }

        public ApprovalImageWriter(Bitmap image)
        {
            Data = image;
        }
        public ApprovalImageWriter(string dir, Bitmap image, string suffix)
        {
            Dir = dir;
            Data = image;
            Suffix = suffix;
        }

        public virtual string GetApprovalFilename(string basename)
        {
            var name = Suffix is null ? $"{basename}.approved.png" : $"{basename}_{Suffix}.approved.png";
            if (Dir is null)
                return name;

            var basepath = Path.GetDirectoryName(basename);
            return basepath is null ?
                        Path.Combine(Dir, Path.GetFileName(name)) :
                        Path.Combine(basepath, Dir, Path.GetFileName(name));
        }

        public virtual string GetReceivedFilename(string basename)
        {
            var trueBaseName = Path.GetFileName(basename);
            var name = Suffix is null ? $"{trueBaseName}.received.png" : $"{trueBaseName}_{Suffix}.received.png";
            if (Dir is null)
                return name;

            var basepath = Path.GetDirectoryName(basename);
            return basepath is null ?
                        Path.Combine(Dir, name) :
                        Path.Combine(basepath, Dir, name);
        }


        public string WriteReceivedFile(string received)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(received));
            Data.Save(received);
            return received;
        }
    }
}
