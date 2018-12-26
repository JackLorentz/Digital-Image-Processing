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
    public partial class Otsu : Form
    {
        public Bitmap image;
        public int flag = 0;
        private int[] statistic = new int[256];

        public Otsu()
        {
            InitializeComponent();
        }

        private void Otsu_Load(object sender, EventArgs e)
        {
            if(flag == 1)
            {
                this.Text = "K-Means";
            }
           
            comboBox1.Items.Clear();
            comboBox1.Items.Add(new ComboboxItem(1, "2"));
            comboBox1.Items.Add(new ComboboxItem(2, "4"));
            comboBox1.Items.Add(new ComboboxItem(3, "8"));
            var selectedObject = comboBox1.Items.Cast<ComboboxItem>().SingleOrDefault(i => i.Value.Equals(1));
            comboBox1.SelectedIndex = comboBox1.FindStringExact(selectedObject.Text.ToString());
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            if (image != null)
            {
                int[] thresholds = new int[256];//chart用
                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        //轉換成灰階,同時統計各值出現次數
                        Color c = image.GetPixel(i, j);
                        int grayScale = (int)((double)(c.R + c.G + c.B) / (double)3);
                        statistic[grayScale]++;
                        image.SetPixel(i, j, Color.FromArgb(grayScale, grayScale, grayScale));
                    }
                }

                Bitmap result = new Bitmap(image.Width, image.Height);
                if (flag == 1)
                {
                    GrayScale[] centers = K_means(statistic, 1);
                    
                    for (int i = 0; i < image.Width; i++)
                    {
                        for (int j = 0; j < image.Height; j++)
                        {
                            int color = 0;
                            Color c = image.GetPixel(i, j);
                            if(c.R < centers[0].color)
                            {
                                color = 0;
                            }
                            else
                            {
                                color = 255;
                            }
                            result.SetPixel(i, j, Color.FromArgb(color, color, color));
                        }
                    }
                    pictureBox1.Image = result;
                    thresholds[centers[0].color] = 4000;
                }
                else
                {
                    List<int> threshold = otsu_method(statistic, 0, statistic.Length, 1);
                    thresholds[threshold[0]] = 4000;

                    for (int i = 0; i < image.Width; i++)
                    {
                        for (int j = 0; j < image.Height; j++)
                        {
                            int color = 0;
                            Color c = image.GetPixel(i, j);
                            if (threshold[0] > c.R)
                            {
                                color = 0;
                            }
                            else
                            {
                                color = 255;
                            }
                            result.SetPixel(i, j, Color.FromArgb(color, color, color));
                        }
                    }
                    pictureBox1.Image = result;
                }
                //
                //
                Form1 form = new Form1();
                double[] snr_rate = form.SNR(image, result);
                this.label2.Text = "SNR : " + snr_rate[0].ToString("#0.000");
                //建立Histogram
                Chart chart = new Chart();
                chart.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
                chart.BackColor = Color.LightGray;
                chart.Series.Add("Gray_Level");
                chart.Series.Add("Threshold");
                chart.ChartAreas.Add("RGB");
                chart.Height = 290;
                chart.Width = 290;
                chart.ChartAreas["RGB"].AxisY.LabelStyle.Enabled = false;
                chart.ChartAreas["RGB"].AxisX.MajorGrid.LineWidth = 0;
                chart.ChartAreas["RGB"].AxisY.MajorGrid.LineWidth = 0;
                chart.ChartAreas["RGB"].BackColor = Color.Gray;
                //Gray level
                chart.Series["Gray_Level"].ChartType = SeriesChartType.Column;
                chart.Series["Gray_Level"].Color = Color.LightGray;
                chart.Series["Gray_Level"].ChartArea = "RGB";
                for (int i = 0; i < 256; i++)
                {
                    chart.Series["Gray_Level"].Points.Add(statistic[i]);
                }
                //Threshold
                chart.Series["Threshold"].ChartType = SeriesChartType.Column;
                chart.Series["Threshold"].Color = Color.Red;
                chart.Series["Threshold"].ChartArea = "RGB";
                for (int i = 0; i < 256; i++)
                {
                    chart.Series["Threshold"].Points.Add(thresholds[i]);
                }
                //顯示直方圖
                chart.Location = new Point(305, 25);
                this.Controls.Add(chart);
                chart.BringToFront();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            ComboboxItem item = comboBox1.Items[comboBox1.SelectedIndex] as ComboboxItem;
            int run = item.Value;
            int[] thresholds = new int[256];

            if(flag == 1)
            {
                int k = (int)Math.Pow(2, run) - 1;
                GrayScale[] centers = K_means(statistic, k);

                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        int color = 0;
                        Color c = image.GetPixel(i, j);
                        if (run == 1)
                        {
                            if (c.R > centers[0].color)
                            {
                                color = 255;
                            }
                            else
                            {
                                color = 0;
                            }
                        }
                        else if (run == 2)
                        {
                            if (c.R <= centers[0].color)
                            {
                                color = 0;
                            }
                            else if (c.R > centers[0].color && c.R <= centers[1].color)
                            {
                                color = centers[1].color;
                            }
                            else if (c.R > centers[1].color && c.R <= centers[2].color)
                            {
                                color = centers[2].color;
                            }
                            else
                            {
                                color = 255;
                            }
                        }
                        else
                        {
                            if (c.R <= centers[0].color)
                            {
                                color = 0;
                            }
                            else if (c.R > centers[0].color && c.R <= centers[1].color)
                            {
                                color = centers[0].color;
                            }
                            else if (c.R > centers[1].color && c.R <= centers[2].color)
                            {
                                color = centers[1].color;
                            }
                            else if (c.R > centers[2].color && c.R <= centers[3].color)
                            {
                                color = centers[2].color;
                            }
                            else if (c.R > centers[3].color && c.R <= centers[4].color)
                            {
                                color = centers[3].color;
                            }
                            else if (c.R > centers[4].color && c.R <= centers[5].color)
                            {
                                color = centers[4].color;
                            }
                            else if (c.R > centers[5].color && c.R <= centers[6].color)
                            {
                                color = centers[6].color;
                            }
                            else
                            {
                                color = 255;
                            }
                        }
                        result.SetPixel(i, j, Color.FromArgb(color, color, color));
                    }
                }
                pictureBox1.Image = result;

                for(int i=0; i<centers.Length; i++)
                {
                    thresholds[centers[i].color] = 4000;
                }
            }
            else
            {
                List<int> threshold = new List<int>();
                threshold = otsu_method(statistic, 0, statistic.Length, run);
                threshold.Sort();
                for (int i = 0; i < threshold.Count; i++)
                {
                    thresholds[threshold[i]] = 4000;
                }

                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        int color = 0;
                        Color c = image.GetPixel(i, j);
                        if (run == 1)
                        {
                            if (c.R > threshold[0])
                            {
                                color = 255;
                            }
                            else
                            {
                                color = 0;
                            }
                        }
                        else if (run == 2)
                        {
                            if (c.R <= threshold[0])
                            {
                                color = 0;
                            }
                            else if (c.R > threshold[0] && c.R <= threshold[1])
                            {
                                color = threshold[1];
                            }
                            else if (c.R > threshold[1] && c.R <= threshold[2])
                            {
                                color = threshold[1];
                            }
                            else
                            {
                                color = 255;
                            }
                        }
                        else
                        {
                            if (c.R <= threshold[0])
                            {
                                color = 0;
                            }
                            else if (c.R > threshold[0] && c.R <= threshold[1])
                            {
                                color = threshold[1];
                            }
                            else if (c.R > threshold[1] && c.R <= threshold[2])
                            {
                                color = threshold[1];
                            }
                            else if (c.R > threshold[2] && c.R <= threshold[3])
                            {
                                color = threshold[3];
                            }
                            else if (c.R > threshold[3] && c.R <= threshold[4])
                            {
                                color = threshold[3];
                            }
                            else if (c.R > threshold[4] && c.R <= threshold[5])
                            {
                                color = threshold[5];
                            }
                            else if (c.R > threshold[5] && c.R <= threshold[6])
                            {
                                color = threshold[5];
                            }
                            else
                            {
                                color = 255;
                            }
                        }
                        result.SetPixel(i, j, Color.FromArgb(color, color, color));
                    }
                }
                pictureBox1.Image = result;
            }
            //
            Form1 form = new Form1();
            double[] snr_rate = form.SNR(image, result);
            this.label2.Text = "SNR : " + snr_rate[0].ToString("#0.000");
            //建立Histogram
            Chart chart = new Chart();
            chart.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
            chart.BackColor = Color.LightGray;
            chart.Series.Add("Gray_Level");
            chart.Series.Add("Threshold");
            chart.ChartAreas.Add("RGB");
            chart.Height = 290;
            chart.Width = 290;
            chart.ChartAreas["RGB"].AxisY.LabelStyle.Enabled = false;
            chart.ChartAreas["RGB"].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas["RGB"].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas["RGB"].BackColor = Color.Gray;
            //Gray level
            chart.Series["Gray_Level"].ChartType = SeriesChartType.Column;
            chart.Series["Gray_Level"].Color = Color.LightGray;
            chart.Series["Gray_Level"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Gray_Level"].Points.Add(statistic[i]);
            }
            //Threshold
            chart.Series["Threshold"].ChartType = SeriesChartType.Column;
            chart.Series["Threshold"].Color = Color.Red;
            chart.Series["Threshold"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Threshold"].Points.Add(thresholds[i]);
            }
            //顯示直方圖
            chart.Location = new Point(305, 25);
            this.Controls.Add(chart);
            chart.BringToFront();

        }

        private List<int> otsu_method(int[] histogram, int l, int r, int cnt)
        {
            List<int> thresholds = new List<int>();
            if (cnt == 0)
            {
                return thresholds;
            }
            else
            {
                int threshold = 0;
                int min_variance = int.MaxValue;

                //找出某區間具有最小變異數
                for (int i = l; i < r; i++)
                {
                    int sum1 = 0, avg1 = 0, total1 = 0;
                    int sum2 = 0, avg2 = 0, total2 = 0;
                    double var1 = 0, var2 = 0;
                    //第一組
                    for (int j = l; j < i; j++)
                    {
                        sum1 += histogram[j] * j;
                        total1 += histogram[j];
                    }
                    avg1 = (int)((double)sum1 / (double)total1);
                    for (int j = l; j < i; j++)
                    {
                        var1 += Math.Pow(j - avg1, 2) * histogram[j];
                    }
                    //第二組
                    for (int j = i; j < r; j++)
                    {
                        sum2 += histogram[j] * j;
                        total2 += histogram[j];
                    }
                    avg2 = (int)((double)sum2 / (double)total2);
                    for (int j = i; j < r; j++)
                    {
                        var2 += Math.Pow(j - avg2, 2) * histogram[j];
                    }
                    //
                    double T = var1 + var2;
                    if (T < min_variance)
                    {
                        min_variance = (int)T;
                        threshold = i;
                    }
                }
                thresholds.Add(threshold);
                List<int> l_thresholds = otsu_method(histogram, l, threshold, cnt - 1);
                List<int> r_thresholds = otsu_method(histogram, threshold, r, cnt - 1);

                for (int i = 0; i < l_thresholds.Count; i++)
                {
                    thresholds.Add(l_thresholds[i]);
                }

                for (int i = 0; i < r_thresholds.Count; i++)
                {
                    thresholds.Add(r_thresholds[i]);
                }

                return thresholds;
            }
        }

        private GrayScale[] K_means(int[] histogram, int k)
        {
            List<int> thresholds = new List<int>();

            List<GrayScale> grayScales = new List<GrayScale>();
            for(int i = 0; i<histogram.Length; i++)
            {
                GrayScale g = new GrayScale();
                g.num = histogram[i];
                g.color = i;
                grayScales.Add(g);
            }
            //括弧內是避免重複的方法
            Random rd = new Random(Guid.NewGuid().GetHashCode());
            GrayScale[] centers = new GrayScale[k];
            int[] tmp = new int[k];
            //產生K個初始中心
            //產生小於256的亂數(grayScales陣列索引)
            tmp[0] = rd.Next(256);
            centers[0] = grayScales[tmp[0]];
            for (int i = 1; i < k; i++)
            {
                int range = rd.Next(1, 50);
                tmp[i] = (tmp[i - 1] + range) % 256;
                centers[i] = grayScales[tmp[i]];
            }
            //
            int convergence_cnt = 0;
            GrayScale[] prev_centers;
            while (convergence_cnt != 3)
            {
                prev_centers = centers;
                //算距離
                for(int i=0; i<grayScales.Count; i++)
                {
                    int min = 0;
                    double min_dist = double.MaxValue;
                    for(int j=0; j<k; j++)
                    {
                        double dist = Math.Sqrt(Math.Pow(grayScales[i].color - centers[j].color, 2) + Math.Pow(grayScales[i].num - centers[j].num, 2));
                        if(dist < min_dist)
                        {
                            min_dist = dist;
                            min = j;
                        }
                    }
                    grayScales[i].cluster = min;
                }
                //重新設中心
                for(int i=0; i<k; i++)
                {
                    int color = 0, num = 0, total = 0;
                    for(int j=0; j<histogram.Length; j++)
                    {
                        if(grayScales[j].cluster == i)
                        {
                            color += grayScales[j].color;
                            num += grayScales[j].num;
                            total++;
                        }
                    }
                    centers[i].color = (int)((double)color / (double)total);
                    centers[i].num = (int)((double)num / (double)total);
                }
                //檢查是否收斂
                bool is_converge = true;
                for(int i=0; i<k; i++)
                {
                    if (!is_centers_equal(centers, prev_centers))
                    {
                        is_converge = false;
                        break;
                    }
                }
                if (is_converge)
                {
                    convergence_cnt++;
                }
            }
            
            return centers;
        }   

        private bool is_centers_equal(GrayScale[] a, GrayScale[] b)
        {
            for(int i=0; i<a.Length; i++)
            {
                if(a[i].color != b[i].color || a[i].num != b[i].num)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class GrayScale
    {
        public int color;
        public int num;
        public int cluster;
        public int cluster_color;
    }
}
