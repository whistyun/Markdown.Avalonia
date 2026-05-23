using ApprovalTests.Approvers;
using ApprovalTests.Core;
using ApprovalTests.Core.Exceptions;
using Avalonia.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace UnitTest.CTxt.Utils
{
    public class ImageFileApprover : FileApprover
    {
        public ImageFileApprover(IApprovalWriter writer, IApprovalNamer namer, bool normalizeLineEndingsForTextFiles = false)
            : base(writer, namer, normalizeLineEndingsForTextFiles)
        {
        }

        public override ApprovalException? Approve(string approvedPath, string receivedPath)
        {
            if (Path.GetExtension(approvedPath) != ".png")
                return base.Approve(approvedPath, receivedPath);

            if (!File.Exists(approvedPath))
            {
                return new ApprovalMissingException(receivedPath, approvedPath);
            }

            // FIXME: I have no idea to compare bitmap with Avalonia.Media.Imaging
            //        This logic use System.Drawing, So only run on Windows.

            using var approvedImg = SKBitmap.FromImage(SKImage.FromEncodedData(approvedPath));
            using var receivedImg = SKBitmap.FromImage(SKImage.FromEncodedData(receivedPath));

            var approvedByte = approvedImg.Bytes;
            var receivedByte = approvedImg.Bytes;

            if (Compare(receivedByte, approvedByte))
            {
                return null;
            }

            return new ApprovalMismatchException(receivedPath, approvedPath);
        }

        private static bool Compare(ICollection<byte> bytes1, ICollection<byte> bytes2)
        {
            if (bytes1.Count != bytes2.Count)
            {
                return false;
            }

            var e1 = bytes1.GetEnumerator();
            var e2 = bytes2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                if (e1.Current != e2.Current)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
