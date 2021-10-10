using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

public class ImageDeal
{
    public static void DealImage(Bitmap SrcBitmap1, Bitmap SrcBitmap2, string path)
    {
        int width = SrcBitmap2.Width;
        int height = SrcBitmap2.Height;

        using (Bitmap bmp = new Bitmap(width, height))
        {
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.DrawImage(SrcBitmap2, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
            }
            
            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
            BitmapData newData = newBmp.LockBits(new Rectangle(0, 0, newBmp.Width, newBmp.Height),ImageLockMode.WriteOnly,bmp.PixelFormat);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),ImageLockMode.ReadOnly,bmp.PixelFormat);
            BitmapData bmpData2 = SrcBitmap1.LockBits(new Rectangle(0, 0, SrcBitmap1.Width, SrcBitmap1.Height),ImageLockMode.ReadOnly,SrcBitmap1.PixelFormat);

            Parallel.For(0, height, i =>
            Parallel.For(0, width, j =>
            {
                int offset = i * bmpData2.Stride + j * (bmpData2.Stride / bmpData2.Width);
                /* 
                 * 直接在記憶體讀寫像素的RGB值
                 * 其中 Black -> White 的程度代表RGB的值是一致的，所以直接將RGB其中之一的值指定給A即可用黑色程度作為Mask
                 */
                int a = Marshal.ReadByte(bmpData.Scan0, offset + 2);
                int r = Marshal.ReadByte(bmpData2.Scan0, offset + 2);
                int g = Marshal.ReadByte(bmpData2.Scan0, offset + 1);
                int b = Marshal.ReadByte(bmpData2.Scan0, offset);

                Marshal.WriteByte(newData.Scan0, offset, (byte)b);
                Marshal.WriteByte(newData.Scan0, offset + 1, (byte)g);
                Marshal.WriteByte(newData.Scan0, offset + 2, (byte)r);
                Marshal.WriteByte(newData.Scan0, offset + 3, (byte)a);
            }));

            newBmp.UnlockBits(newData);
            bmp.UnlockBits(bmpData);
            SrcBitmap1.UnlockBits(bmpData2);
            newBmp.Save(path);
        }
    }
}