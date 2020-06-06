using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiHsControls
{
    internal static class ErrorBitmapInternal
    {
        private static Bitmap errorBitmap = null;

        internal static Bitmap ErrorBitmap
        {
            get
            {
                if (errorBitmap == null)
                {
                    errorBitmap = new Bitmap(typeof(DataGridView), "ImageInError.bmp");
                }
                return errorBitmap;
            }
        }
    }
}
