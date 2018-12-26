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
    public partial class Transparent : Form
    {
        //控制物件
        private string file_name;
        private PcxDecoder pcxDecoder;
        private Bitmap transparentImage;
        private Bitmap backGroundImage;
        private double transparent_rate = 0;

        public Transparent()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //緩存PCX所有bytes
            byte[] array;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.pcx) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.pcx";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.file_name = dialog.SafeFileName;

            if (dialog.FileName.Contains(".pcx"))
            {
                array = System.IO.File.ReadAllBytes(dialog.FileName);
                String result = "";
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
                    backGroundImage = pcxDecoder.BuildBitmap(imgBuffer, width, height);
                    pictureBox1.Image = backGroundImage; ;
                    pictureBox1.Size = new Size(width, height);
                    this.label1.Text = this.file_name;
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //緩存PCX所有bytes
            byte[] array;
            //目前處理原圖 => images[0]
            Bitmap image;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.pcx) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.pcx";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.file_name = dialog.SafeFileName;

            if (dialog.FileName.Contains(".pcx"))
            {
                array = System.IO.File.ReadAllBytes(dialog.FileName);
                String result = "";
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
                    Bitmap sumImage = new Bitmap(width, height);
                    transparentImage = pcxDecoder.BuildBitmap(imgBuffer, width, height);
                    this.label2.Text = this.file_name;
                    for (int i = 0; i < transparentImage.Width; i++)
                    {
                        for (int j = 0; j < transparentImage.Height; j++)
                        {
                            Color c1 = backGroundImage.GetPixel(i, j);
                            Color c2 = transparentImage.GetPixel(i, j);
                            int r = (int)(transparent_rate * (float)c1.R + (1 - transparent_rate) * (float)c2.R);
                            int g = (int)(transparent_rate * (float)c1.G + (1 - transparent_rate) * (float)c2.G);
                            int b = (int)(transparent_rate * (float)c1.B + (1 - transparent_rate) * (float)c2.B);
                            sumImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                        }
                    }
                    pictureBox2.Image = sumImage;
                    pictureBox2.Size = new Size(width, height);
                    this.trackBar1.Enabled = true;
                }
            }

            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);

            // The Maximum property sets the value of the track bar when
            // the slider is all the way to the right.
            trackBar1.Maximum = 100;
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

        private void trackBar1_Scroll(object sender, System.EventArgs e)
        {
            transparent_rate = (double)trackBar1.Value / (double)100;
            int w = transparentImage.Width, h = transparentImage.Height;
            Rectangle rect = new Rectangle(0, 0, w, h);

            //將srcBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的srcBimap
            BitmapData t_bmdata = transparentImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData b_bmdata = backGroundImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //將CreateGrayscaleImage灰階影像，並將這個結果交給Bitmap類別的dstBimap
            Bitmap dst = new Bitmap(w, h);

            //將dstBitmap鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的dstBimap
            BitmapData dst_bmdata = dst.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //位元圖中第一個像素數據的地址。它也可以看成是位圖中的第一個掃描行
            //目的是設兩個起始旗標srcPtr、dstPtr，為srcBmData、dstBmData的掃描行的開始位置
            System.IntPtr t_Ptr = t_bmdata.Scan0;
            System.IntPtr b_Ptr = b_bmdata.Scan0;
            System.IntPtr dstPtr = dst_bmdata.Scan0;

            //將Bitmap對象的訊息存放到byte中
            int t_bytes = t_bmdata.Stride * h;
            byte[] t_Values​​ = new byte[t_bytes];
            int b_bytes = b_bmdata.Stride * h;
            byte[] b_Values​​ = new byte[b_bytes];

            int dst_bytes = dst_bmdata.Stride * h;
            byte[] dstValues​​ = new byte[dst_bytes];

            //複製GRB信息到byte中
            System.Runtime.InteropServices.Marshal.Copy(t_Ptr, t_Values​​, 0, t_bytes);
            System.Runtime.InteropServices.Marshal.Copy(b_Ptr, b_Values​​, 0, b_bytes);
            System.Runtime.InteropServices.Marshal.Copy(dstPtr, dstValues​​, 0, dst_bytes);

            //轉灰階(RGB是反過來的 -> BGR)
            double r1, g1, b1, r2, g2, b2;
            int i, j, k;
            byte tmp_r, tmp_g, tmp_b;
            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++)
                {
                    k = 3 * j;
                    //fowardground
                    r1 = t_Values[i * t_bmdata.Stride + k + 2];
                    g1 = t_Values[i * t_bmdata.Stride + k + 1];
                    b1 = t_Values[i * t_bmdata.Stride + k];
                    //background
                    r2 = b_Values[i * b_bmdata.Stride + k + 2];
                    g2 = b_Values[i * b_bmdata.Stride + k + 1];
                    b2 = b_Values[i * b_bmdata.Stride + k];

                    tmp_r = (byte)((1.0 - transparent_rate) * r1 + transparent_rate * r2);
                    tmp_g = (byte)((1.0 - transparent_rate) * g1 + transparent_rate * g2);
                    tmp_b = (byte)((1.0 - transparent_rate) * b1 + transparent_rate * b2);

                    dstValues[i * dst_bmdata.Stride + k + 2] = tmp_r;
                    dstValues[i * dst_bmdata.Stride + k + 1] = tmp_g;
                    dstValues[i * dst_bmdata.Stride + k] = tmp_b;
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(dstValues​​, 0, dstPtr, dst_bytes);

            //解鎖位圖
            transparentImage.UnlockBits(t_bmdata);
            backGroundImage.UnlockBits(b_bmdata);
            dst.UnlockBits(dst_bmdata);

            pictureBox2.Image = dst;
            this.label8.Text = trackBar1.Value + "%";
            double[] snr_rate = SNR(transparentImage, dst);
            this.label9.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
        }

        private double[] SNR(Bitmap orig, Bitmap n)
        {
            double[] SNR_rate = new double[3];
            double r_orig_sum = 0.0, g_orig_sum = 0.0, b_orig_sum = 0.0;
            double r_noise_sum = 0.0, g_noise_sum = 0.0, b_noise_sum = 0.0;

            for (int i = 0; i < orig.Width; i++)
            {
                for (int j = 0; j < orig.Height; j++)
                {
                    Color c = orig.GetPixel(i, j);
                    Color new_c = n.GetPixel(i, j);
                    r_orig_sum += Math.Pow(c.R, 2);
                    g_orig_sum += Math.Pow(c.G, 2);
                    b_orig_sum += Math.Pow(c.B, 2);

                    r_noise_sum += Math.Pow(new_c.R - c.R, 2);
                    g_noise_sum += Math.Pow(new_c.G - c.G, 2);
                    b_noise_sum += Math.Pow(new_c.B - c.B, 2);
                }
            }
            SNR_rate[0] = 10 * Math.Log(10, r_orig_sum / r_noise_sum);
            SNR_rate[1] = 10 * Math.Log(10, g_orig_sum / g_noise_sum);
            SNR_rate[2] = 10 * Math.Log(10, b_orig_sum / b_noise_sum);
            return SNR_rate;
        }
    }
}
