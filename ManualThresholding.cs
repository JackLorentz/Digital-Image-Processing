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
    public partial class ManualThresholding : Form
    {
        private String file_name = "";
        private PcxDecoder pcxDecoder;
        private Bitmap orig_image;

        public ManualThresholding()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //緩存PCX所有bytes
            byte[] array;
            //目前處理原圖 => images[0]
            Bitmap image;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.pcx, *.hpcx, *.tiff) | *.jpg; *.jpeg; *.png; *.pcx; *.hpcx; *.tiff";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            //檔名
            this.file_name = dialog.SafeFileName;
            this.label3.Text = file_name;

            if (dialog.FileName.Contains(".pcx"))
            {
                array = System.IO.File.ReadAllBytes(dialog.FileName);

                //先分析檔頭(128 bytes)
                pcxDecoder = new PcxDecoder(array);
                int width = pcxDecoder.width;
                int height = pcxDecoder.height;
                //分析圖片的pixel
                int index = 0, cnt = 129;
                int size = width * height;
                byte[] imgBuffer = new byte[size];
                while (index < size)
                {
                    //若最高兩位為11則為run-length
                    if ((byte)(array[cnt] & 0xc0) == 0xc0)
                    {
                        int runLength = array[cnt] & 0x3F;
                        byte runValue;
                        //如果前6位均是1, 表示這個只是剛好開頭是11的pixel值
                        if (runLength == 0x3F)
                        {
                            runLength = 1;
                            runValue = array[cnt];
                        }
                        else
                        {
                            cnt++;
                            runValue = array[cnt];
                        }
                        for (int i = 0; i < runLength; i++)
                        {
                            imgBuffer[index] = runValue;
                            index++;
                        }
                    }
                    else
                    {
                        imgBuffer[index] = array[cnt];
                        index++;
                    }
                    cnt++;
                }
                //塗顏色
                //表示彩色
                if (array[68] == 1)
                {
                    image = pcxDecoder.BuildBitmap(imgBuffer, width, height);
                    orig_image = new Bitmap(RGB2Gray(image));
                    pictureBox1.Image = orig_image;
                    pictureBox1.Size = new Size(width, height);
                }
            }
            else if (dialog.FileName.Contains(".hpcx"))
            {
                Huffman huffman = new Huffman();
                array = System.IO.File.ReadAllBytes(dialog.FileName);
                image = huffman.Decoding(array);
                orig_image = new Bitmap(RGB2Gray(image));
                pictureBox1.Image = orig_image;
                pictureBox1.Size = new Size(huffman.w, huffman.h);
            }
            else
            {
                image = (Bitmap)Image.FromFile(dialog.FileName);
                orig_image = new Bitmap(RGB2Gray(image));
                ImageDecoder imageDecoder = new ImageDecoder();
                imageDecoder.Decoding(image);
                pictureBox1.Image = orig_image;
                pictureBox1.Size = new Size(imageDecoder.w, imageDecoder.h);
            }

            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);

            // The Maximum property sets the value of the track bar when
            // the slider is all the way to the right.
            trackBar1.Maximum = 255;
            trackBar1.Minimum = 0;
            // The TickFrequency property establishes how many positions
            // are between each tick-mark.
            trackBar1.TickFrequency = 5;

            // The LargeChange property sets how many positions to move
            // if the bar is clicked on either side of the slider.
            trackBar1.LargeChange = 3;

            // The SmallChange property sets how many positions to move
            // if the keyboard arrows are used to move the slider.
            trackBar1.SmallChange = 2;
        }

        private Bitmap RGB2Gray(Bitmap b)
        {
            int w = b.Width, h = b.Height;
            Rectangle rect = new Rectangle(0, 0, w, h);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData src_bmdata = b.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //將CreateGrayscaleImage灰階影像，並將這個結果交給Bitmap類別的dstBimap
            Bitmap dst = new Bitmap(w, h);

            //將dstBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的dstBimap
            BitmapData dst_bmdata = dst.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            //目的是設兩個起始旗標srcPtr、dstPtr，為srcBmData、dstBmData的掃描行的開始位置
            System.IntPtr srcPtr = src_bmdata.Scan0;
            System.IntPtr dstPtr = dst_bmdata.Scan0;

            //將Bitmap對象的訊息存放到byte中
            int src_bytes = src_bmdata.Stride * h;
            byte[] srcValues​​ = new byte[src_bytes];

            int dst_bytes = dst_bmdata.Stride * h;
            byte[] dstValues​​ = new byte[dst_bytes];

            //複製GRB信息到byte中
            System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcValues​​, 0, src_bytes);
            System.Runtime.InteropServices.Marshal.Copy(dstPtr, dstValues​​, 0, dst_bytes);

            //轉灰階(RGB是反過來的 -> BGR)
            int r, g, bl;
            int i, j, k;
            byte tmp;
            for(i=0; i<h; i++)
            {
                for(j=0; j<w; j++)
                {
                    k = 3 * j;
                    r = srcValues[i * src_bmdata.Stride + k + 2];
                    g = srcValues[i * src_bmdata.Stride + k + 1];
                    bl = srcValues[i * src_bmdata.Stride + k];
                    tmp = (byte)((r + g + bl) / 3);
                    dstValues[i * dst_bmdata.Stride + k + 2] = tmp;
                    dstValues[i * dst_bmdata.Stride + k + 1] = tmp;
                    dstValues[i * dst_bmdata.Stride + k] = tmp;
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(dstValues​​, 0, dstPtr, dst_bytes);

            //解鎖位圖
            b.UnlockBits(src_bmdata);
            dst.UnlockBits(dst_bmdata);

            return dst;
        }

        private double calculate_SNR(Bitmap o, Bitmap n)
        {
            double snr = 0.0, orig_sum = 0.0, noise_sum = 0.0;

            for (int i = 0; i < o.Width; i++)
            {
                for (int j = 0; j < o.Height; j++)
                {
                    Color c = o.GetPixel(i, j);
                    Color new_c = n.GetPixel(i, j);
                    orig_sum += Math.Pow(c.R, 2);
                    noise_sum += Math.Pow(new_c.R - c.R, 2);
                }
            }

            snr = 10 * Math.Log(10, orig_sum / noise_sum);

            return snr;
        }

        private Bitmap thresholding(Bitmap b, int v)
        {
            int w = b.Width, h = b.Height;
            Rectangle rect = new Rectangle(0, 0, w, h);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData src_bmdata = b.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //將CreateGrayscaleImage灰階影像，並將這個結果交給Bitmap類別的dstBimap
            Bitmap dst = new Bitmap(w, h);

            //將dstBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的dstBimap
            BitmapData dst_bmdata = dst.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            //目的是設兩個起始旗標srcPtr、dstPtr，為srcBmData、dstBmData的掃描行的開始位置
            System.IntPtr srcPtr = src_bmdata.Scan0;
            System.IntPtr dstPtr = dst_bmdata.Scan0;

            //將Bitmap對象的訊息存放到byte中
            int src_bytes = src_bmdata.Stride * h;
            byte[] srcValues​​ = new byte[src_bytes];

            int dst_bytes = dst_bmdata.Stride * h;
            byte[] dstValues​​ = new byte[dst_bytes];

            //複製GRB信息到byte中
            System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcValues​​, 0, src_bytes);
            System.Runtime.InteropServices.Marshal.Copy(dstPtr, dstValues​​, 0, dst_bytes);

            //轉灰階
            int gray;
            int i, j, k;
            byte tmp;
            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++)
                {
                    k = 3 * j;
                    gray = srcValues[i * src_bmdata.Stride + k + 2];
                    if(gray > v)
                    {
                        tmp = 255;
                    }
                    else
                    {
                        tmp = 0;
                    }
                    dstValues[i * dst_bmdata.Stride + k + 2] = tmp;
                    dstValues[i * dst_bmdata.Stride + k + 1] = tmp;
                    dstValues[i * dst_bmdata.Stride + k] = tmp;
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(dstValues​​, 0, dstPtr, dst_bytes);

            //解鎖位圖
            b.UnlockBits(src_bmdata);
            dst.UnlockBits(dst_bmdata);

            return dst;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int critical_value = (int)trackBar1.Value;
            this.label6.Text = "臨界值: " + critical_value;
            Bitmap new_image = thresholding(orig_image, critical_value);
            pictureBox2.Image = new_image;
            pictureBox2.Size = new Size(new_image.Width, new_image.Height);
            this.label7.Text = "SNR: " + calculate_SNR(orig_image, new_image).ToString("#0.000");
        }
    }
}