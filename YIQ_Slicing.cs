using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIP_HW
{
    public partial class YIQ_Slicing : Form
    {
        public Bitmap image;
        //Y為亮度, I和Q分別代表顏色, 用在彩色有線電視系統的訊號通道上
        private Bitmap y_slice, i_slice, q_slice;

        public YIQ_Slicing()
        {
            InitializeComponent();
        }

        private void YIQ_Slicing_Load(object sender, EventArgs e)
        {
            if(image != null)
            {
                int w = image.Width, h = image.Height;
                Rectangle rect = new Rectangle(0, 0, w, h);

                //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
                BitmapData src_bmdata = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                //將CreateGrayscaleImage灰階影像，並將這個結果交給Bitmap類別的dstBimap
                y_slice = new Bitmap(w, h);
                i_slice = new Bitmap(w, h);
                q_slice = new Bitmap(w, h);

                //將dstBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的dstBimap
                BitmapData y_bmdata = y_slice.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                BitmapData i_bmdata = i_slice.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                BitmapData q_bmdata = q_slice.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
                //目的是設兩個起始旗標srcPtr、dstPtr，為srcBmData、dstBmData的掃描行的開始位置
                System.IntPtr srcPtr = src_bmdata.Scan0;
                System.IntPtr yPtr = y_bmdata.Scan0;
                System.IntPtr iPtr = i_bmdata.Scan0;
                System.IntPtr qPtr = q_bmdata.Scan0;

                //將Bitmap對象的訊息存放到byte中
                int src_bytes = src_bmdata.Stride * h;
                byte[] srcValues​​ = new byte[src_bytes];

                int y_bytes = y_bmdata.Stride * h;
                byte[] yValues​​ = new byte[y_bytes];
                int i_bytes = i_bmdata.Stride * h;
                byte[] iValues​​ = new byte[i_bytes];
                int q_bytes = q_bmdata.Stride * h;
                byte[] qValues​​ = new byte[q_bytes];

                //複製GRB信息到byte中
                System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcValues​​, 0, src_bytes);
                System.Runtime.InteropServices.Marshal.Copy(yPtr, yValues​​, 0, y_bytes);
                System.Runtime.InteropServices.Marshal.Copy(iPtr, iValues​​, 0, i_bytes);
                System.Runtime.InteropServices.Marshal.Copy(qPtr, qValues​​, 0, q_bytes);

                //轉灰階(RGB是反過來的 -> BGR)
                double _y, _i, _q;
                int r, g, b;
                int i, j, k;
                for (i = 0; i < h; i++)
                {
                    for (j = 0; j < w; j++)
                    {
                        k = 3 * j;
                        r = srcValues[i * src_bmdata.Stride + k + 2];
                        g = srcValues[i * src_bmdata.Stride + k + 1];
                        b = srcValues[i * src_bmdata.Stride + k];

                        _y = 0.299 * r + 0.587 * g + 0.114 * b;
                        _i = 0.595716 * r - 0.274453 * g - 0.321263 * b;
                        _q = 0.211456 * r - 0.522591 * g + 0.311135 * b;

                        yValues[i * y_bmdata.Stride + k + 2] = (byte)_y;
                        yValues[i * y_bmdata.Stride + k + 1] = (byte)_y;
                        yValues[i * y_bmdata.Stride + k] = (byte)_y;

                        iValues[i * i_bmdata.Stride + k + 2] = (byte)_i;
                        iValues[i * i_bmdata.Stride + k + 1] = (byte)_i;
                        iValues[i * i_bmdata.Stride + k] = (byte)_i;

                        qValues[i * q_bmdata.Stride + k + 2] = (byte)_q;
                        qValues[i * q_bmdata.Stride + k + 1] = (byte)_q;
                        qValues[i * q_bmdata.Stride + k] = (byte)_q;
                    }
                }
                System.Runtime.InteropServices.Marshal.Copy(yValues​​, 0, yPtr, y_bytes);
                System.Runtime.InteropServices.Marshal.Copy(iValues​​, 0, iPtr, i_bytes);
                System.Runtime.InteropServices.Marshal.Copy(qValues​​, 0, qPtr, q_bytes);

                //解鎖位圖
                image.UnlockBits(src_bmdata);
                y_slice.UnlockBits(y_bmdata);
                i_slice.UnlockBits(i_bmdata);
                q_slice.UnlockBits(q_bmdata);

                this.pictureBox1.Image = y_slice;
                this.pictureBox2.Image = i_slice;
                this.pictureBox3.Image = q_slice;
            }
        }
    }
}
