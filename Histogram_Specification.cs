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
using System.Windows.Forms.DataVisualization.Charting;

namespace DIP_HW
{
    public partial class Histogram_Specification : Form
    {
        //開檔
        private double origin_snr, desire_snr, result_snr;
        //histogram specification
        private double[] desired_transformation;
        private Bitmap target;

        public Histogram_Specification()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.chart1.Visible = false;
            this.chart1.Series.Clear();
            this.chart1.ChartAreas.Clear();
            this.chart3.Visible = false;
            this.chart3.Series.Clear();
            this.chart3.ChartAreas.Clear();
            this.pictureBox1.Image = null;
            this.pictureBox3.Image = null;
            //緩存PCX所有bytes
            byte[] array;
            //目前處理原圖 => images[0]
            Bitmap image, grayImage, result;
            //Gray Scale統計
            int[] gray_statistic = new int[256];
            int[] new_statistic = new int[256];

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.pcx, *.hpcx, *.tiff) | *.jpg; *.jpeg; *.png; *.pcx; *.hpcx; *.tiff";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            //檔名
            this.label1.Text = "ORIGIN: " + dialog.SafeFileName;

            if (dialog.FileName.Contains(".pcx"))
            {
                array = System.IO.File.ReadAllBytes(dialog.FileName);
                //先分析檔頭(128 bytes)
                PcxDecoder pcxDecoder = new PcxDecoder(array);
                int w = pcxDecoder.width;
                int h = pcxDecoder.height;
                //分析圖片的pixel
                int index = 0, cnt = 129;
                int size = w * h;
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
                    image = pcxDecoder.BuildBitmap(imgBuffer, w, h);
                    grayImage = RGB2Gray(image);
                    target = new Bitmap(grayImage);
                    //做直方圖均衡化
                    result = new Bitmap(w, h);
                    image_channel_statisic(grayImage, gray_statistic);
                    double[] transform = histogramEqualization(grayImage, gray_statistic, w * h);
                    int i, j;
                    Color c;
                    for(j=0; j<h; j++)
                    {
                        for(i=0; i<w; i++)
                        {
                            c = grayImage.GetPixel(i, j);
                            int gray = (int)transform[c.R];
                            new_statistic[gray]++;
                            result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                        }
                    }
                    //
                    pictureBox1.Image = result;
                    pictureBox1.Size = new Size(w, h);
                    this.label3.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
                }

            }
            else if (dialog.FileName.Contains(".hpcx"))
            {
                Huffman huffman = new Huffman();
                array = System.IO.File.ReadAllBytes(dialog.FileName);
                image = huffman.Decoding(array);
                grayImage = RGB2Gray(image);
                target = new Bitmap(grayImage);
                int w = grayImage.Width, h = grayImage.Height;
                //做直方圖均衡化
                result = new Bitmap(w, h);
                image_channel_statisic(grayImage, gray_statistic);
                double[] transform = histogramEqualization(grayImage, gray_statistic, w * h);
                int i, j;
                Color c;
                for (j = 0; j < h; j++)
                {
                    for (i = 0; i < w; i++)
                    {
                        c = grayImage.GetPixel(i, j);
                        int gray = (int)transform[c.R];
                        new_statistic[gray]++;
                        result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                    }
                }
                //
                pictureBox1.Image = result;
                pictureBox1.Size = new Size(w, h);
                this.label3.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
            }
            else
            {
                image = (Bitmap)Image.FromFile(dialog.FileName);
                grayImage = RGB2Gray(image);
                target = new Bitmap(grayImage);
                int w = grayImage.Width, h = grayImage.Height;
                //做直方圖均衡化
                result = new Bitmap(w, h);
                image_channel_statisic(grayImage, gray_statistic);
                double[] transform = histogramEqualization(grayImage, gray_statistic, w * h);
                int i, j;
                Color c;
                for (j = 0; j < h; j++)
                {
                    for (i = 0; i < w; i++)
                    {
                        c = grayImage.GetPixel(i, j);
                        int gray = (int)transform[c.R];
                        new_statistic[gray]++;
                        result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                    }
                }
                pictureBox1.Image = result;
                pictureBox1.Size = new Size(w, h);
                this.label3.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
            }

            //設定圖表
            this.chart1.Visible = true;
            this.chart1.BackColor = Color.LightGray;
            this.chart1.Series.Add("Origin");
            this.chart1.ChartAreas.Add("Origin");
            this.chart1.ChartAreas["Origin"].AxisY.Minimum = 0;
            this.chart1.ChartAreas["Origin"].AxisY.Maximum = 4000;
            this.chart1.ChartAreas["Origin"].AxisX.Interval = 32;
            this.chart1.ChartAreas["Origin"].AxisX.MajorGrid.LineColor = Color.Silver;
            this.chart1.ChartAreas["Origin"].AxisY.MajorGrid.LineColor = Color.Silver;
            this.chart1.ChartAreas["Origin"].BackColor = Color.DimGray;
            this.chart1.Series["Origin"].ChartType = SeriesChartType.Column;
            this.chart1.Series["Origin"].Color = Color.Aqua;
            this.chart1.Series["Origin"].ChartArea = "Origin";
            for (int i = 0; i < 256; i++)
            {
                this.chart1.Series["Origin"].Points.Add(new_statistic[i]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.chart2.Visible = false;
            this.chart2.Series.Clear();
            this.chart2.ChartAreas.Clear();
            this.chart3.Visible = false;
            this.chart3.Series.Clear();
            this.chart3.ChartAreas.Clear();
            this.pictureBox2.Image = null;
            this.pictureBox3.Image = null;
            //緩存PCX所有bytes
            byte[] array;
            //目前處理原圖 => images[0]
            Bitmap image, grayImage, result;
            // Gray Scale統計
            int[] gray_statistic = new int[256];
            int[] new_statistic = new int[256];

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.pcx, *.hpcx, *.tiff) | *.jpg; *.jpeg; *.png; *.pcx; *.hpcx; *.tiff";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            //檔名
            this.label2.Text = "DESIRE: " + dialog.SafeFileName;

            if (dialog.FileName.Contains(".pcx"))
            {
                array = System.IO.File.ReadAllBytes(dialog.FileName);
                //先分析檔頭(128 bytes)
                PcxDecoder pcxDecoder = new PcxDecoder(array);
                int w = pcxDecoder.width;
                int h = pcxDecoder.height;
                //分析圖片的pixel
                int index = 0, cnt = 129;
                int size = w * h;
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
                    image = pcxDecoder.BuildBitmap(imgBuffer, w, h);
                    grayImage = RGB2Gray(image);
                    //做直方圖均衡化
                    result = new Bitmap(w, h);
                    image_channel_statisic(grayImage, gray_statistic);
                    double[] transform = histogramEqualization(grayImage, gray_statistic, w * h);
                    this.desired_transformation = transform;
                    int i, j;
                    Color c;
                    for (j = 0; j < h; j++)
                    {
                        for (i = 0; i < w; i++)
                        {
                            c = grayImage.GetPixel(i, j);
                            int gray = (int)transform[c.R];
                            new_statistic[gray]++;
                            result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                        }
                    }
                    //
                    pictureBox2.Image = result;
                    pictureBox2.Size = new Size(w, h);
                    this.label4.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
                }
            }
            else if (dialog.FileName.Contains(".hpcx"))
            {
                Huffman huffman = new Huffman();
                array = System.IO.File.ReadAllBytes(dialog.FileName);
                image = huffman.Decoding(array);
                grayImage = RGB2Gray(image);
                int w = grayImage.Width, h = grayImage.Height;
                //做直方圖均衡化
                result = new Bitmap(w, h);
                image_channel_statisic(grayImage, gray_statistic);
                double[] transform = histogramEqualization(grayImage, gray_statistic, w * h);
                this.desired_transformation = transform;
                int i, j;
                Color c;
                for (j = 0; j < h; j++)
                {
                    for (i = 0; i < w; i++)
                    {
                        c = grayImage.GetPixel(i, j);
                        int gray = (int)transform[c.R];
                        new_statistic[gray]++;
                        result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                    }
                }
                //
                pictureBox2.Image = result;
                pictureBox2.Size = new Size(w, h);
                this.label4.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
            }
            else
            {
                image = (Bitmap)Image.FromFile(dialog.FileName);
                grayImage = RGB2Gray(image);
                int w = grayImage.Width, h = grayImage.Height;
                //做直方圖均衡化
                result = new Bitmap(w, h);
                image_channel_statisic(grayImage, gray_statistic);
                double[] transform = histogramEqualization(grayImage, gray_statistic, w * h);
                this.desired_transformation = transform;
                int i, j;
                Color c;
                for (j = 0; j < h; j++)
                {
                    for (i = 0; i < w; i++)
                    {
                        c = grayImage.GetPixel(i, j);
                        int gray = (int)transform[c.R];
                        new_statistic[gray]++;
                        result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                    }
                }
                pictureBox1.Image = result;
                pictureBox1.Size = new Size(w, h);
                this.label4.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
            }

            //設定圖表
            this.chart2.Visible = true;
            this.chart2.BackColor = Color.LightGray;
            this.chart2.Series.Add("Desire");
            this.chart2.ChartAreas.Add("Desire");
            this.chart2.ChartAreas["Desire"].AxisY.Minimum = 0;
            this.chart2.ChartAreas["Desire"].AxisY.Maximum = 4000;
            this.chart2.ChartAreas["Desire"].AxisX.Interval = 32;
            this.chart2.ChartAreas["Desire"].AxisX.MajorGrid.LineColor = Color.Silver;
            this.chart2.ChartAreas["Desire"].AxisY.MajorGrid.LineColor = Color.Silver;
            this.chart2.ChartAreas["Desire"].BackColor = Color.DimGray;
            this.chart2.Series["Desire"].ChartType = SeriesChartType.Column;
            this.chart2.Series["Desire"].Color = Color.Coral;
            this.chart2.Series["Desire"].ChartArea = "Desire";
            for (int i = 0; i < 256; i++)
            {
                this.chart2.Series["Desire"].Points.Add(new_statistic[i]);
            }
        }

        private void image_channel_statisic(Bitmap image, int[] gray_statistic)
        {
            int i, j;
            Color c;
            int w = image.Width, h = image.Height;
            for(j=0; j<h; j++)
            {
                for(i=0; i<w; i++)
                {
                    c = image.GetPixel(i, j);
                    gray_statistic[c.R]++;
                }
            }
        }

        private double[] histogramEqualization(Bitmap bitmap, int[] histogram, int total)
        {
            double[] h = new double[256];//轉換結果
            double[] p = new double[256];
            double[] cdf = new double[256];
            double cdf_min = double.MaxValue;

            p[0] = histogram[0];
            for (int i = 1; i < histogram.Length; i++)
            {
                p[i] = histogram[i];
                cdf[i] = p[i] + cdf[i - 1];
                if (cdf[i] != 0 && cdf[i] < cdf_min)
                {
                    cdf_min = cdf[i];
                }
            }

            for (int i = 0; i < histogram.Length; i++)
            {
                h[i] = (cdf[i] - cdf_min) / (total - cdf_min) * 255;
            }

            return h;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int[] new_statistic = new int[256];
            int w = target.Width, h = target.Height;
            Bitmap result = new Bitmap(w, h);
            int i, j;
            Color c;
            for (j = 0; j < h; j++)
            {
                for (i = 0; i < w; i++)
                {
                    c = target.GetPixel(i, j);
                    int gray = (int)desired_transformation[c.R];
                    new_statistic[gray]++;
                    result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }

            //
            pictureBox3.Image = result;
            pictureBox3.Size = new Size(w, h);
            this.label5.Text = "SNR: " + calculate_snr(target, result).ToString("#0.000");
            //設定圖表
            this.chart3.Visible = true;
            this.chart3.BackColor = Color.LightGray;
            this.chart3.Series.Add("Result");
            this.chart3.ChartAreas.Add("Result");
            this.chart3.ChartAreas["Result"].AxisY.Minimum = 0;
            this.chart3.ChartAreas["Result"].AxisY.Maximum = 4000;
            this.chart3.ChartAreas["Result"].AxisX.Interval = 32;
            this.chart3.ChartAreas["Result"].AxisX.MajorGrid.LineColor = Color.Silver;
            this.chart3.ChartAreas["Result"].AxisY.MajorGrid.LineColor = Color.Silver;
            this.chart3.ChartAreas["Result"].BackColor = Color.DimGray;
            this.chart3.Series["Result"].ChartType = SeriesChartType.Column;
            this.chart3.Series["Result"].Color = Color.YellowGreen;
            this.chart3.Series["Result"].ChartArea = "Result";
            for (int k = 0; k < 256; k++)
            {
                this.chart3.Series["Result"].Points.Add(new_statistic[k]);
            }
        }

        private double calculate_snr(Bitmap orig, Bitmap n)
        {
            double snr;
            Color c, new_c;
            double orig_sum = 0.0;
            double noise_sum = 0.0;

            for (int i = 0; i < orig.Width; i++)
            {
                for (int j = 0; j < orig.Height; j++)
                {
                    c = orig.GetPixel(i, j);
                    new_c = n.GetPixel(i, j);
                    orig_sum += Math.Pow(c.R, 2);
                    noise_sum += Math.Pow(new_c.R - c.R, 2);
                }
            }
            snr = 10 * Math.Log(10, orig_sum / noise_sum);
            return snr;
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
            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++)
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
    }
}
