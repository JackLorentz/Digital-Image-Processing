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
    public partial class Contrast_Stretching : Form
    {
        public Bitmap image;
        private Bitmap grayImage;
        private int[] gray_transform = new int[256];
        private List<Point> points = new List<Point>();
        private int now_x, now_y;
        private bool is_clear = false;


        public Contrast_Stretching()
        {
            InitializeComponent();
        }

        private void Contrast_Stretching_Load(object sender, EventArgs e)
        {
            int index = 0, vector_x, vector_y, i;
            points.Add(new Point(0, 0));
            points.Add(new Point(64, 32));
            points.Add(new Point(192, 224));
            points.Add(new Point(256, 256));
            for (i=0; i<256; i++)
            {
                if(index < points.Count - 1)
                {
                    if (points[index].X <= i && points[index + 1].X > i)
                    {
                        vector_x = points[index + 1].X - points[index].X;
                        vector_y = points[index + 1].Y - points[index].Y;
                        double[] coef = calculate_line_formula(vector_x, vector_y, points[index + 1].X, points[index + 1].Y);
                        gray_transform[i] = (int)(coef[0] * i + coef[1]);
                    }
                    if(points[index + 1].X - 1 == i)
                    {
                        index++;
                    }
                }
                else
                {
                    gray_transform[i] = i;
                }
            }
            //設定圖表
            this.chart1.BackColor = Color.LightGray;
            this.chart1.Series.Add("constrast stretching");
            this.chart1.ChartAreas.Add("constrast stretching");
            this.chart1.ChartAreas["constrast stretching"].AxisY.Minimum = 0;
            this.chart1.ChartAreas["constrast stretching"].AxisY.Maximum = 256;
            this.chart1.ChartAreas["constrast stretching"].AxisX.Interval = 64;
            this.chart1.ChartAreas["constrast stretching"].AxisX.MajorGrid.LineColor = Color.Silver;
            this.chart1.ChartAreas["constrast stretching"].AxisY.MajorGrid.LineColor = Color.Silver;
            this.chart1.ChartAreas["constrast stretching"].BackColor = Color.DimGray;
            this.chart1.Series["constrast stretching"].ChartType = SeriesChartType.Line;
            this.chart1.Series["constrast stretching"].Color = Color.Coral;
            this.chart1.Series["constrast stretching"].ChartArea = "constrast stretching";
            for(i=0; i<256; i++)
            {
                this.chart1.Series["constrast stretching"].Points.Add(gray_transform[i]);
            }
            //先轉灰階圖
            grayImage = RGB2Gray(image);
            int gray, w = grayImage.Width, h = grayImage.Height;
            Color c;
            Bitmap result = new Bitmap(w, h);
            int j;
            for(j=0; j<h; j++)
            {
                for(i=0; i<w; i++)
                {
                    c = grayImage.GetPixel(i, j);
                    gray = gray_transform[c.R];
                    result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            this.pictureBox1.Image = result;
            this.pictureBox1.Size = new Size(w, h);
            this.label1.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
        }

        private double[] calculate_line_formula(int vector_x, int vector_y, int x, int y)
        {
            double[] coef = new double[2];
            //a(y'-y) = b(x'-x) => y = x * b/-a + x * b/a + y'
            coef[0] = (double)vector_y / (double)vector_x;
            coef[1] = -(double)x * (double)vector_y / (double)vector_x + (double)y;
            return coef;
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

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            //座標
            now_x = e.Location.X;
            now_y = e.Location.Y;
            //轉換成折線圖的座標系統
            if (now_x >= 56 && now_y >= 17 && now_x <= 233 && now_y <= 220)
            {
                now_y = (int)((double)(220 - now_y) * 256.0 / 203.0);
                now_x = (int)((double)(now_x - 56) * 256.0 / 177.0);
                this.toolStripStatusLabel1.Text = "( X , Y ) = " + "(" + now_x + " , " + now_y + ")";
            }
            //刷新
            this.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            is_clear = true;
            this.points.Clear();
            this.points.Add(new Point(0, 0));
            this.chart1.Series["constrast stretching"].Points.Clear();
            this.pictureBox1.Image = null;
            for (int i = 0; i < 256; i++)
            {
                this.chart1.Series["constrast stretching"].Points.Add(i);
            }
        }

        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (is_clear)
            {
                this.points.Add(new Point(now_x, now_y));
                this.points = points.OrderBy(o => o.X).ToList();
                updateChart();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            is_clear = false;
            this.chart1.Series["constrast stretching"].Points.Clear();
            int i, index = 0, vector_x, vector_y;
            points.Add(new Point(256, 256));
            for (i = 0; i < 256; i++)
            {
                if (index < points.Count - 1)
                {
                    if (points[index].X <= i && points[index + 1].X > i)
                    {
                        vector_x = points[index + 1].X - points[index].X;
                        vector_y = points[index + 1].Y - points[index].Y;
                        double[] coef = calculate_line_formula(vector_x, vector_y, points[index + 1].X, points[index + 1].Y);
                        gray_transform[i] = (int)(coef[0] * i + coef[1]);
                    }
                    if (points[index + 1].X - 1 == i)
                    {
                        index++;
                    }
                }
                else
                {
                    gray_transform[i] = i;
                }
            }

            for (i = 0; i < 256; i++)
            {
                this.chart1.Series["constrast stretching"].Points.Add(gray_transform[i]);
            }

            int gray, w = grayImage.Width, h = grayImage.Height;
            Color c;
            Bitmap result = new Bitmap(w, h);
            int j;
            for (j = 0; j < h; j++)
            {
                for (i = 0; i < w; i++)
                {
                    c = grayImage.GetPixel(i, j);
                    gray = gray_transform[c.R];
                    result.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            this.pictureBox1.Image = result;
            this.pictureBox1.Size = new Size(w, h);
            this.label1.Text = "SNR: " + calculate_snr(grayImage, result).ToString("#0.000");
        }

        private void updateChart()
        {
            this.chart1.Series["constrast stretching"].Points.Clear();
            int i, index = 0, vector_x, vector_y;
            for (i = 0; i < 256; i++)
            {
                if (index < points.Count - 1)
                {
                    if (points[index].X <= i && points[index + 1].X > i)
                    {
                        vector_x = points[index + 1].X - points[index].X;
                        vector_y = points[index + 1].Y - points[index].Y;
                        double[] coef = calculate_line_formula(vector_x, vector_y, points[index + 1].X, points[index + 1].Y);
                        gray_transform[i] = (int)(coef[0] * i + coef[1]);
                    }
                    if (points[index + 1].X - 1 == i)
                    {
                        index++;
                    }
                }
            }

            for (i = 0; i < 256; i++)
            {
                if(i < points[points.Count - 1].X)
                {
                    this.chart1.Series["constrast stretching"].Points.Add(gray_transform[i]);
                }
                else
                {
                    this.chart1.Series["constrast stretching"].Points.Add(0);
                }
            }
        }
    }
}
