using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DIP_HW
{
    public partial class HistogramProcessing : Form
    {
        public Bitmap image;
        private Bitmap grayImage;

        public HistogramProcessing()
        {
            InitializeComponent();
        }

        private void HistogramProcessing_Load(object sender, EventArgs e)
        {
            if(image != null)
            {
                int[] histogram = new int[256];
                grayImage = new Bitmap(image.Width, image.Height);
                for(int i=0; i<image.Width; i++)
                {
                    for(int j=0; j<image.Height; j++)
                    {
                        Color c = image.GetPixel(i, j);
                        //Color hsv = RGB2HSV(c);
                        int gray = (c.R + c.B + c.G) / 3;
                        histogram[gray]++;
                        grayImage.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                    }
                }
                double[] h = histogramEqualization(grayImage, histogram, grayImage.Width * grayImage.Height);
                Bitmap result = new Bitmap(grayImage.Width, grayImage.Height);
                for(int i=0; i<grayImage.Width; i++)
                {
                    for(int j=0; j<grayImage.Height; j++)
                    {
                        Color c = grayImage.GetPixel(i, j);
                        result.SetPixel(i, j, Color.FromArgb((int)h[c.R], (int)h[c.R], (int)h[c.R]));
                    }
                }
                pictureBox1.Image = result;
                int[] newHistogram = new int[256];
                for(int i=0; i<result.Width; i++)
                {
                    for(int j=0; j<result.Height; j++)
                    {
                        Color c = result.GetPixel(i, j);
                        newHistogram[c.R]++;
                    }
                }
                //
                Form1 form = new Form1();
                double[] snr_rate = form.SNR(grayImage, result);
                this.label1.Text = "SNR : " + snr_rate[0].ToString("#0.000");
                //建立Histogram
                Chart chart = new Chart();
                chart.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
                chart.BackColor = Color.LightGray;
                chart.Series.Add("Histogram Equalization");
                chart.ChartAreas.Add("RGB");
                chart.Height = 256;
                chart.Width = 256;
                chart.ChartAreas["RGB"].AxisY.LabelStyle.Enabled = false;
                chart.ChartAreas["RGB"].AxisX.MajorGrid.LineWidth = 0;
                chart.ChartAreas["RGB"].AxisY.MajorGrid.LineWidth = 0;
                chart.ChartAreas["RGB"].BackColor = Color.Gray;
                //Histogram Equalization
                chart.Series["Histogram Equalization"].ChartType = SeriesChartType.Column;
                chart.Series["Histogram Equalization"].Color = Color.Aquamarine;
                chart.Series["Histogram Equalization"].ChartArea = "RGB";
                for (int i = 0; i < 256; i++)
                {
                    chart.Series["Histogram Equalization"].Points.Add(newHistogram[i]);
                }
                //顯示直方圖
                chart.Location = new Point(305, 25);
                this.Controls.Add(chart);
                chart.BringToFront();
            }
        }

        private double[] histogramEqualization(Bitmap bitmap, int[] histogram, int total)
        {
            double[] h = new double[256];//轉換結果
            double[] p = new double[256];
            double[] cdf = new double[256];
            double cdf_min = double.MaxValue;

            p[0] = histogram[0];
            for(int i=1; i<histogram.Length; i++)
            {
                p[i] = histogram[i];
                cdf[i] = p[i] + cdf[i - 1];
                if(cdf[i] != 0 && cdf[i] < cdf_min)
                {
                    cdf_min = cdf[i];
                }
            }
            
            for (int i=0; i<histogram.Length; i++)
            {
                h[i] = (cdf[i] - cdf_min) / (total - cdf_min) * 255;
            }

            return h;
        }

        private Color RGB2HSV(Color c)
        {
            float r = (float)c.R / (float)255;
            float g = (float)c.G / (float)255;
            float b = (float)c.B / (float)255;

            float h = 0, s = 0, v = 0;
            float min, max, delta, tmp;
            if(r > g)
            {
                tmp = g;
            }
            else
            {
                tmp = r;
            }

            if(tmp > r)
            {
                min = b;
            }
            else
            {
                min = tmp;
            }

            if(r > g)
            {
                tmp = r;
            }
            else
            {
                tmp = g;
            }

            if (tmp > b)
            {
                max = tmp;
            }
            else
            {
                max = b;
            }

            v = max;

            delta = max - min;
            if(max != 0)
            {
                s = delta / max;
            }
            else
            {
                s = 0;
                h = 0;
            }

            if(delta == 0)
            {
                h = 0; 
                return Color.FromArgb((int)h, (int)s, (int)v);
            }
            else if(r == max)
            {
                if(g >= b)
                {
                    h = (g - b) / delta;
                }
                else
                {
                    h = (g - b) / delta + (float)6.0;
                }
            }
            else if(g == max)
            {
                h = (float)2.0 + (b - r) / delta;
            }
            else if(b == max)
            {
                h = (float)4.0 + (r - g) / delta;
            }

            h *= (float)60.0;

            return Color.FromArgb((int)(h * (float)255 / (float)360), (int)s, (int)v);
        }
    }
}
