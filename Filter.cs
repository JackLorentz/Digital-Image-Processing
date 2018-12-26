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
    public partial class Filter : Form
    {
        public Bitmap image;
        public int flag = 0;

        public Filter()
        {
            InitializeComponent();
        }

        private void Filter_Load(object sender, EventArgs e)
        {
            int[] r_statistic = new int[256];
            int[] g_statistic = new int[256];
            int[] b_statistic = new int[256];

            if (flag == 0)
            {
                this.Text = "Lowpass Spatial Filtering";
            }
            else if(flag == 1)
            {
                this.Text = "Median Filtering (Cross)";
            }
            else if(flag == 2)
            {
                this.Text = "Pseudo Median Filtering";
            }
            else if(flag == 3)
            {
                this.Text = "Basic Highpass Spatial Filtering & Edge Crispening";
            }
            else if(flag == 4)
            {
                this.Text = "High-Boost Filtering";
            }
            else if(flag == 5)
            {
                this.Text = "Robert Cross-Gradient Operator";
            }
            else if(flag == 6)
            {
                this.Text = "Sobel Operator";
            }
            else if(flag == 7)
            {
                this.Text = "Prewitt Operator";
            }
            else if(flag == 8)
            {
                this.Text = "Outlier Filtering";
            }
            else
            {
                this.Text = "Median Filtering (Square)";
            }

            if(flag == 2)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add(new ComboboxItem(0, "MAXMIN"));
                comboBox1.Items.Add(new ComboboxItem(1, "MINMAX"));
                comboBox1.Items.Add(new ComboboxItem(2, "PMED"));
                var selectedObject = comboBox1.Items.Cast<ComboboxItem>().SingleOrDefault(i => i.Value.Equals(0));
                comboBox1.SelectedIndex = comboBox1.FindStringExact(selectedObject.Text.ToString());
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            }
            else if(flag == 3)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add(new ComboboxItem(0, "Mask 1 = 1/9 * {[-1, -1, -1], [-1, 8, -1], [-1, -1, -1]}"));
                comboBox1.Items.Add(new ComboboxItem(1, "Mask 2 = {[0, -1, 0], [-1, 5, -1], [0, -1, 0]}"));
                comboBox1.Items.Add(new ComboboxItem(2, "Mask 3 = {[-1, -1, -1], [-1, 9, -1], [-1, -1, -1]}"));
                comboBox1.Items.Add(new ComboboxItem(3, "Mask 4 = {[1, -2, 1], [-2, 5, -2], [1, -2, 1]}"));
                var selectedObject = comboBox1.Items.Cast<ComboboxItem>().SingleOrDefault(i => i.Value.Equals(0));
                comboBox1.SelectedIndex = comboBox1.FindStringExact(selectedObject.Text.ToString());
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            }
            else if(flag == 4)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add(new ComboboxItem(1, "A = 1"));//標準highpass
                comboBox1.Items.Add(new ComboboxItem(2, "A = 2"));
                comboBox1.Items.Add(new ComboboxItem(3, "A = 3"));
                var selectedObject = comboBox1.Items.Cast<ComboboxItem>().SingleOrDefault(i => i.Value.Equals(1));
                comboBox1.SelectedIndex = comboBox1.FindStringExact(selectedObject.Text.ToString());
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            }
            else if(flag == 5 || flag == 6 || flag == 7)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add(new ComboboxItem(0, "X Direction"));
                comboBox1.Items.Add(new ComboboxItem(1, "Y Direction"));
                comboBox1.Items.Add(new ComboboxItem(2, "Both X Direction and Y Direction"));
                var selectedObject = comboBox1.Items.Cast<ComboboxItem>().SingleOrDefault(i => i.Value.Equals(0));
                comboBox1.SelectedIndex = comboBox1.FindStringExact(selectedObject.Text.ToString());
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            }
            else
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add(new ComboboxItem(3, "3*3"));
                comboBox1.Items.Add(new ComboboxItem(5, "5*5"));
                comboBox1.Items.Add(new ComboboxItem(7, "7*7"));
                var selectedObject = comboBox1.Items.Cast<ComboboxItem>().SingleOrDefault(i => i.Value.Equals(3));
                comboBox1.SelectedIndex = comboBox1.FindStringExact(selectedObject.Text.ToString());
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            }

            if(image != null)
            {
                Bitmap result = new Bitmap(image.Width, image.Height);
                int w = image.Width, h = image.Height;
                if (flag == 0)
                {
                    result = lowpass_filtering(r_statistic, g_statistic, b_statistic, 3);
                    pictureBox1.Image = result;
                }
                else if (flag == 1)
                {
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            List<int> r = new List<int>();
                            List<int> g = new List<int>();
                            List<int> b = new List<int>();
                            Color c;
                            for (int m=1; m<3; m++)
                            {
                                c = image.GetPixel((i + m) % w, j);
                                r.Add(c.R);
                                g.Add(c.G);
                                b.Add(c.B);
                            }
                            for (int m = 1; m < 3; m++)
                            {
                                if (i < m)
                                {
                                    c = image.GetPixel(w + (i - m), j);
                                    r.Add(c.R);
                                    g.Add(c.G);
                                    b.Add(c.B);
                                }
                                else
                                {
                                    c = image.GetPixel(i - m, j);
                                    r.Add(c.R);
                                    g.Add(c.G);
                                    b.Add(c.B);
                                }
                            }
                            for (int m = 1; m < 3; m++)
                            {
                                c = image.GetPixel(i, (j + m) % h);
                                r.Add(c.R);
                                g.Add(c.G);
                                b.Add(c.B);
                            }
                            for (int m = 1; m < 3; m++)
                            {
                                if (j < m)
                                {
                                    c = image.GetPixel(i, h + (j - m));
                                    r.Add(c.R);
                                    g.Add(c.G);
                                    b.Add(c.B);
                                }
                                else
                                {
                                    c = image.GetPixel(i, j - m);
                                    r.Add(c.R);
                                    g.Add(c.G);
                                    b.Add(c.B);
                                }
                            }
                            c = image.GetPixel(i, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                            r.Sort();
                            g.Sort();
                            b.Sort();
                            int R = r[(int)(r.Count / 2)];
                            int G = g[(int)(g.Count / 2)];
                            int B = b[(int)(b.Count / 2)];
                            result.SetPixel(i, j, Color.FromArgb(R, G, B));
                            r_statistic[R]++;
                            g_statistic[G]++;
                            b_statistic[B]++;
                        }
                    }
                    pictureBox1.Image = result;
                }
                else if (flag == 2)
                {
                    for (int i=0; i<w; i++)
                    {
                        for(int j=0; j<h; j++)
                        {
                            List<int> r = new List<int>();
                            List<int> g = new List<int>();
                            List<int> b = new List<int>();

                            Color c = image.GetPixel(i, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                            int x = 0 , y = 0;
                            if (i - 1 < 0)
                            {
                                x = w + (i - 1);
                            }
                            else
                            {
                                x = i - 1;
                            }
                            c = image.GetPixel(x, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            if (j - 1 < 0)
                            {
                                y = h + (j - 1);
                            }
                            else
                            {
                                y = j - 1;
                            }
                            c = image.GetPixel(i, y);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel((i+1)%w, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel(i, (j+1)%h);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            int R = pseudo_median_maxmin(r);
                            int G = pseudo_median_maxmin(g);
                            int B = pseudo_median_maxmin(b);
                            r_statistic[R]++;
                            g_statistic[G]++;
                            b_statistic[B]++;

                            result.SetPixel(i, j, Color.FromArgb(R, G, B));
                        }
                    }
                    pictureBox1.Image = result;
                }
                else if (flag == 3)
                {
                    result = basic_highpass_spatial_filter(r_statistic, g_statistic, b_statistic, 0);
                    pictureBox1.Image = result;
                }
                else if (flag == 4)
                {
                    result = high_boost_filter(r_statistic, g_statistic, b_statistic, 1);
                    pictureBox1.Image = result;
                }
                else if(flag == 5)
                {
                    result = robert_cross_gradient(r_statistic, g_statistic, b_statistic, 0);
                    pictureBox1.Image = result;
                }
                else if(flag == 6)
                {
                    result = sobel_operator(r_statistic, g_statistic, b_statistic, 0);
                    pictureBox1.Image = result;
                }
                else if(flag == 7)
                {
                    result = prewitt_operator(r_statistic, g_statistic, b_statistic, 0);
                    pictureBox1.Image = result;
                }
                else if(flag == 8)
                {
                    result = outlier_filtering(r_statistic, g_statistic, b_statistic, 3);
                    pictureBox1.Image = result;
                }
                else
                {
                    result = median_square_filtering(r_statistic, g_statistic, b_statistic, 3);
                    pictureBox1.Image = result;
                }
                Form1 form = new Form1();
                double[] snr_rate = form.SNR(image, result);
                this.label2.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                    + snr_rate[1].ToString("#0.000") + " , "
                    + snr_rate[2].ToString("#0.000") + ")";
                //建立Histogram
                Chart chart = new Chart();
                chart.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
                chart.BackColor = Color.LightGray;
                chart.Series.Add("Red");
                chart.Series.Add("Green");
                chart.Series.Add("Blue");
                chart.ChartAreas.Add("RGB");
                chart.Height = 290;
                chart.Width = 290;
                chart.ChartAreas["RGB"].AxisY.LabelStyle.Enabled = false;
                chart.ChartAreas["RGB"].AxisX.MajorGrid.LineWidth = 0;
                chart.ChartAreas["RGB"].AxisY.MajorGrid.LineWidth = 0;
                chart.ChartAreas["RGB"].BackColor = Color.Gray;
                //Red level
                chart.Series["Red"].ChartType = SeriesChartType.Column;
                chart.Series["Red"].Color = Color.Red;
                chart.Series["Red"].ChartArea = "RGB";
                for (int i = 0; i < 256; i++)
                {
                    chart.Series["Red"].Points.Add(r_statistic[i]);
                }
                //Green level
                chart.Series["Green"].ChartType = SeriesChartType.Column;
                chart.Series["Green"].Color = Color.Green;
                chart.Series["Green"].ChartArea = "RGB";
                for (int i = 0; i < 256; i++)
                {
                    chart.Series["Green"].Points.Add(g_statistic[i]);
                }
                //Blue level
                chart.Series["Blue"].ChartType = SeriesChartType.Column;
                chart.Series["Blue"].Color = Color.Blue;
                chart.Series["Blue"].ChartArea = "RGB";
                for (int i = 0; i < 256; i++)
                {
                    chart.Series["Blue"].Points.Add(b_statistic[i]);
                }
                //顯示直方圖
                chart.Location = new Point(305, 25);
                this.Controls.Add(chart);
                chart.BringToFront();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int[] r_statistic = new int[256];
            int[] g_statistic = new int[256];
            int[] b_statistic = new int[256];
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(image.Width, image.Height);
            ComboboxItem item = comboBox1.Items[comboBox1.SelectedIndex] as ComboboxItem;
            int size = item.Value;

            if (flag == 0)
            {
                result = lowpass_filtering(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            else if (flag == 1)
            {
                int run = (int)((double)(size * size - 1) / (double)4);
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        List<int> r = new List<int>();
                        List<int> g = new List<int>();
                        List<int> b = new List<int>();
                        Color c;
                        for (int m = 1; m < run; m++)
                        {
                            c = image.GetPixel((i + m) % w, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                        }
                        for (int m = 1; m < run; m++)
                        {
                            if (i < m)
                            {
                                c = image.GetPixel(w + (i - m), j);
                                r.Add(c.R);
                                g.Add(c.G);
                                b.Add(c.B);
                            }
                            else
                            {
                                c = image.GetPixel(i - m, j);
                                r.Add(c.R);
                                g.Add(c.G);
                                b.Add(c.B);
                            }
                        }
                        for (int m = 1; m < run; m++)
                        {
                            c = image.GetPixel(i, (j + m) % h);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                        }
                        for (int m = 1; m < run; m++)
                        {
                            if (j < m)
                            {
                                c = image.GetPixel(i, h + (j - m));
                                r.Add(c.R);
                                g.Add(c.G);
                                b.Add(c.B);
                            }
                            else
                            {
                                c = image.GetPixel(i, j - m);
                                r.Add(c.R);
                                g.Add(c.G);
                                b.Add(c.B);
                            }
                        }
                        c = image.GetPixel(i, j);
                        r.Add(c.R);
                        g.Add(c.G);
                        b.Add(c.B);
                        r.Sort();
                        g.Sort();
                        b.Sort();
                        int R = r[(int)(r.Count / 2)];
                        int G = g[(int)(g.Count / 2)];
                        int B = b[(int)(b.Count / 2)];
                        result.SetPixel(i, j, Color.FromArgb(R, G, B));
                        r_statistic[R]++;
                        g_statistic[G]++;
                        b_statistic[B]++;
                    }
                }
                pictureBox1.Image = result;
            }
            else if (flag == 2)
            {
                if(size == 0)
                {
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            List<int> r = new List<int>();
                            List<int> g = new List<int>();
                            List<int> b = new List<int>();

                            Color c = image.GetPixel(i, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                            int x = 0, y = 0;
                            if (i - 1 < 0)
                            {
                                x = w + (i - 1);
                            }
                            else
                            {
                                x = i - 1;
                            }
                            c = image.GetPixel(x, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            if (j - 1 < 0)
                            {
                                y = h + (j - 1);
                            }
                            else
                            {
                                y = j - 1;
                            }
                            c = image.GetPixel(i, y);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel((i + 1) % w, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel(i, (j + 1) % h);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            int R = pseudo_median_maxmin(r);
                            int G = pseudo_median_maxmin(g);
                            int B = pseudo_median_maxmin(b);
                            r_statistic[R]++;
                            g_statistic[G]++;
                            b_statistic[B]++;

                            result.SetPixel(i, j, Color.FromArgb(R, G, B));
                        }
                    }
                    pictureBox1.Image = result;
                }
                else if(size == 1)
                {
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            List<int> r = new List<int>();
                            List<int> g = new List<int>();
                            List<int> b = new List<int>();

                            Color c = image.GetPixel(i, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                            int x = 0, y = 0;
                            if (i - 1 < 0)
                            {
                                x = w + (i - 1);
                            }
                            else
                            {
                                x = i - 1;
                            }
                            c = image.GetPixel(x, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            if (j - 1 < 0)
                            {
                                y = h + (j - 1);
                            }
                            else
                            {
                                y = j - 1;
                            }
                            c = image.GetPixel(i, y);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel((i + 1) % w, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel(i, (j + 1) % h);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            int R = pseudo_median_minmax(r);
                            int G = pseudo_median_minmax(g);
                            int B = pseudo_median_minmax(b);
                            r_statistic[R]++;
                            g_statistic[G]++;
                            b_statistic[B]++;

                            result.SetPixel(i, j, Color.FromArgb(R, G, B));
                        }
                    }
                    pictureBox1.Image = result;
                }
                else
                {
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            List<int> r = new List<int>();
                            List<int> g = new List<int>();
                            List<int> b = new List<int>();

                            Color c = image.GetPixel(i, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                            int x = 0, y = 0;
                            if (i - 1 < 0)
                            {
                                x = w + (i - 1);
                            }
                            else
                            {
                                x = i - 1;
                            }
                            c = image.GetPixel(x, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            if (j - 1 < 0)
                            {
                                y = h + (j - 1);
                            }
                            else
                            {
                                y = j - 1;
                            }
                            c = image.GetPixel(i, y);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel((i + 1) % w, j);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            c = image.GetPixel(i, (j + 1) % h);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);

                            int R = pseudo_median_pmed(r);
                            int G = pseudo_median_pmed(g);
                            int B = pseudo_median_pmed(b);
                            r_statistic[R]++;
                            g_statistic[G]++;
                            b_statistic[B]++;

                            result.SetPixel(i, j, Color.FromArgb(R, G, B));
                        }
                    }
                    pictureBox1.Image = result;
                }
            }
            else if (flag == 3)
            {
                result = basic_highpass_spatial_filter(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            else if(flag == 4)
            {
                result = high_boost_filter(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            else if(flag == 5)
            {
                result = robert_cross_gradient(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            else if(flag == 6)
            {
                result = sobel_operator(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            else if(flag == 7)
            {
                result = prewitt_operator(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            else if(flag == 8)
            {
                result = outlier_filtering(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            else
            {
                result = median_square_filtering(r_statistic, g_statistic, b_statistic, size);
                pictureBox1.Image = result;
            }
            Form1 form = new Form1();
            double[] snr_rate = form.SNR(image, result);
            this.label2.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            //建立Histogram
            Chart chart = new Chart();
            chart.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
            chart.BackColor = Color.LightGray;
            chart.Series.Add("Red");
            chart.Series.Add("Green");
            chart.Series.Add("Blue");
            chart.ChartAreas.Add("RGB");
            chart.Height = 290;
            chart.Width = 290;
            chart.ChartAreas["RGB"].AxisY.LabelStyle.Enabled = false;
            chart.ChartAreas["RGB"].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas["RGB"].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas["RGB"].BackColor = Color.Gray;
            //Red level
            chart.Series["Red"].ChartType = SeriesChartType.Column;
            chart.Series["Red"].Color = Color.Red;
            chart.Series["Red"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Red"].Points.Add(r_statistic[i]);
            }
            //Green level
            chart.Series["Green"].ChartType = SeriesChartType.Column;
            chart.Series["Green"].Color = Color.Green;
            chart.Series["Green"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Green"].Points.Add(g_statistic[i]);
            }
            //Blue level
            chart.Series["Blue"].ChartType = SeriesChartType.Column;
            chart.Series["Blue"].Color = Color.Blue;
            chart.Series["Blue"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Blue"].Points.Add(b_statistic[i]);
            }
            //顯示直方圖
            chart.Location = new Point(305, 25);
            this.Controls.Add(chart);
            chart.BringToFront();
        }

        private int pseudo_median_maxmin(List<int> c)
        {
            int med = 0;
            int[] tmp = new int[10];
            tmp[0] = Math.Min(Math.Min(c[0], c[1]), c[2]);
            tmp[1] = Math.Min(Math.Min(c[0], c[1]), c[3]);
            tmp[2] = Math.Min(Math.Min(c[0], c[1]), c[4]);
            tmp[3] = Math.Min(Math.Min(c[0], c[2]), c[3]);
            tmp[4] = Math.Min(Math.Min(c[0], c[2]), c[4]);
            tmp[5] = Math.Min(Math.Min(c[0], c[3]), c[4]);
            tmp[6] = Math.Min(Math.Min(c[1], c[2]), c[3]);
            tmp[7] = Math.Min(Math.Min(c[1], c[2]), c[4]);
            tmp[8] = Math.Min(Math.Min(c[1], c[3]), c[4]);
            tmp[9] = Math.Min(Math.Min(c[2], c[3]), c[4]);
            for(int i=0; i<10; i++)
            {
                med = Math.Max(tmp[i], med);
            }
            return med;
        }

        private int pseudo_median_minmax(List<int> c)
        {
            int med = int.MaxValue;
            int[] tmp = new int[10];
            tmp[0] = Math.Max(Math.Max(c[0], c[1]), c[2]);
            tmp[1] = Math.Max(Math.Max(c[0], c[1]), c[3]);
            tmp[2] = Math.Max(Math.Max(c[0], c[1]), c[4]);
            tmp[3] = Math.Max(Math.Max(c[0], c[2]), c[3]);
            tmp[4] = Math.Max(Math.Max(c[0], c[2]), c[4]);
            tmp[5] = Math.Max(Math.Max(c[0], c[3]), c[4]);
            tmp[6] = Math.Max(Math.Max(c[1], c[2]), c[3]);
            tmp[7] = Math.Max(Math.Max(c[1], c[2]), c[4]);
            tmp[8] = Math.Max(Math.Max(c[1], c[3]), c[4]);
            tmp[9] = Math.Max(Math.Max(c[2], c[3]), c[4]);
            for (int i = 0; i < 10; i++)
            {
                med = Math.Min(tmp[i], med);
            }
            return med;
        }

        private int pseudo_median_pmed(List<int> c)
        {
            int min = int.MaxValue, max = 0;
            int[] max_tmp = new int[3];
            int[] min_tmp = new int[3];

            max_tmp[0] = Math.Max(Math.Max(c[0], c[1]), c[2]);
            max_tmp[1] = Math.Max(Math.Max(c[0], c[1]), c[3]);
            max_tmp[2] = Math.Max(Math.Max(c[0], c[1]), c[4]);

            min_tmp[0] = Math.Min(Math.Min(c[0], c[1]), c[2]);
            min_tmp[1] = Math.Min(Math.Min(c[0], c[1]), c[3]);
            min_tmp[2] = Math.Min(Math.Min(c[0], c[1]), c[4]);
            
            for(int i=0; i<3; i++)
            {
                min = Math.Min(max_tmp[i], min);
                max = Math.Max(min_tmp[i], max);
            }

            return (int)((double)(min + max) / (double)2);
        }

        private Bitmap basic_highpass_spatial_filter(int[] r_statistic, int[] g_statistic , int[] b_statistic, int mask)
        {
            Color c;
            int r = 0, g = 0, b = 0;
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(w, h);
            int start_x = 0, start_y = 0;
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<h; j++)
                {
                    if(mask == 0)
                    {
                        start_x = (w + i - 1) % w;
                        start_y = (h + j - 1) % h;
                        for (int m = start_x; m < start_x + 3; m++)
                        {
                            for (int n = start_y; n < start_y + 3; n++)
                            {
                                if (m == i && n == j)
                                {
                                    continue;
                                }
                                c = image.GetPixel(m % w, n % h);
                                r -= c.R;
                                g -= c.G;
                                b -= c.B;
                            }
                        }
                        c = image.GetPixel(i, j);
                        r += c.R * 8;
                        r /= 9;
                        g += c.G * 8;
                        g /= 9;
                        b += c.B * 8;
                        b /= 9;
                    }
                    else if(mask == 1)
                    {
                        c = image.GetPixel((w + i - 1) % w, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (j + 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, j);
                        r += c.R * 5;
                        g += c.G * 5;
                        b += c.B * 5;
                    }
                    else if(mask == 2)
                    {
                        start_x = (w + i - 1) % w;
                        start_y = (h + j - 1) % h;
                        for (int m = start_x; m < start_x + 3; m++)
                        {
                            for (int n = start_y; n < start_y + 3; n++)
                            {
                                if (m == i && n == j)
                                {
                                    continue;
                                }
                                c = image.GetPixel(m % w, n % h);
                                r -= c.R;
                                g -= c.G;
                                b -= c.B;
                            }
                        }
                        c = image.GetPixel(i, j);
                        r += c.R * 9;
                        g += c.G * 9;
                        b += c.B * 9;
                    }
                    else
                    {
                        c = image.GetPixel((w + i - 1) % w, j);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel((i + 1) % w, j);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel(i, (h + j - 1) % h);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel(i, (j + 1) % h);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel(i, j);
                        r += c.R * 5;
                        g += c.G * 5;
                        b += c.B * 5;
                    }
                    
                    if (r < 0)
                    {
                        r = 0;
                    }
                    else if(r > 255)
                    {
                        r = 255;
                    }
                   
                    if (g < 0)
                    {
                        g = 0;
                    }
                    else if (g > 255)
                    {
                        g = 255;
                    }
                    
                    if (b < 0)
                    {
                        b = 0;
                    }
                    else if (b > 255)
                    {
                        b = 255;
                    }
                    r_statistic[r]++;
                    g_statistic[g]++;
                    b_statistic[b]++;
                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return result;
        }
        //Highpass = A * Origin - Lowpass(Average)
        private Bitmap high_boost_filter(int[] r_statistic, int[] g_statistic, int[] b_statistic, int A)
        {
            Color c;
            int r = 0, g = 0, b = 0;
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(w, h);
            int start_x = 0, start_y = 0;
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<h; j++)
                {
                    start_x = (w + i - 1) % w;
                    start_y = (h + j - 1) % h;
                    for (int m=start_x; m<start_x + 3; m++)
                    {
                        for(int n=start_y; n<start_y + 3; n++)
                        {
                            c = image.GetPixel(m % w, n % h);
                            r += c.R;
                            g += c.G;
                            b += c.B;
                        }
                    }
                    r /= 9;
                    g /= 9;
                    b /= 9;
                    c = image.GetPixel(i, j);
                    r = c.R * A - r;
                    g = c.G * A - g;
                    b = c.B * A - b;

                    if (r < 0)
                    {
                        r = 0;
                    }
                    else if (r > 255)
                    {
                        r = 255;
                    }

                    if (g < 0)
                    {
                        g = 0;
                    }
                    else if (g > 255)
                    {
                        g = 255;
                    }

                    if (b < 0)
                    {
                        b = 0;
                    }
                    else if (b > 255)
                    {
                        b = 255;
                    }
                    r_statistic[r]++;
                    g_statistic[g]++;
                    b_statistic[b]++;
                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return result;
        }

        private Bitmap robert_cross_gradient(int[] r_statistic, int[] g_statistic, int[] b_statistic, int flag)
        {
            Color c;
            int r = 0, g = 0, b = 0;
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(w, h);
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<h; j++)
                {
                    if(flag == 0)//X
                    {
                        c = image.GetPixel(i, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    else if(flag == 1)//Y
                    {
                        c = image.GetPixel((i + 1) % w, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    else//X + Y
                    {
                        c = image.GetPixel(i, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    

                    if (r < 0)
                    {
                        r = 0;
                    }
                    else if (r > 255)
                    {
                        r = 255;
                    }

                    if (g < 0)
                    {
                        g = 0;
                    }
                    else if (g > 255)
                    {
                        g = 255;
                    }

                    if (b < 0)
                    {
                        b = 0;
                    }
                    else if (b > 255)
                    {
                        b = 255;
                    }
                    r_statistic[r]++;
                    g_statistic[g]++;
                    b_statistic[b]++;
                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            return result;
        }

        private Bitmap sobel_operator(int[] r_statistic, int[] g_statistic, int[] b_statistic, int flag)
        {
            Color c;
            int r = 0, g = 0, b = 0;
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(w, h);
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<h; j++)
                {
                    if(flag == 0)
                    {
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (h + j - 1) % h);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel(i, (j + 1) % h);
                        r += c.R * 2;
                        g += c.G * 2;
                        b += c.B * 2;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    else if(flag == 1)
                    {
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, j);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, j);
                        r += c.R * 2;
                        g += c.G * 2;
                        b += c.B * 2;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    else
                    {
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (h + j - 1) % h);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel(i, (j + 1) % h);
                        r += c.R * 2;
                        g += c.G * 2;
                        b += c.B * 2;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, j);
                        r -= c.R * 2;
                        g -= c.G * 2;
                        b -= c.B * 2;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, j);
                        r += c.R * 2;
                        g += c.G * 2;
                        b += c.B * 2;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }

                    if (r < 0)
                    {
                        r = 0;
                    }
                    else if (r > 255)
                    {
                        r = 255;
                    }

                    if (g < 0)
                    {
                        g = 0;
                    }
                    else if (g > 255)
                    {
                        g = 255;
                    }

                    if (b < 0)
                    {
                        b = 0;
                    }
                    else if (b > 255)
                    {
                        b = 255;
                    }
                    r_statistic[r]++;
                    g_statistic[g]++;
                    b_statistic[b]++;
                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return result;
        }

        private Bitmap prewitt_operator(int[] r_statistic, int[] g_statistic, int[] b_statistic, int flag)
        {
            Color c;
            int r = 0, g = 0, b = 0;
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (flag == 0)
                    {
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel(i, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    else if (flag == 1)
                    {
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, j);
                        r += c.R;
                        g += c.G ;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                    else
                    {
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel(i, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel(i, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((w + i - 1) % w, (h + j - 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, j);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((w + i - 1) % w, (j + 1) % h);
                        r -= c.R;
                        g -= c.G;
                        b -= c.B;
                        c = image.GetPixel((i + 1) % w, (h + j - 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, j);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        c = image.GetPixel((i + 1) % w, (j + 1) % h);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }

                    if (r < 0)
                    {
                        r = 0;
                    }
                    else if (r > 255)
                    {
                        r = 255;
                    }

                    if (g < 0)
                    {
                        g = 0;
                    }
                    else if (g > 255)
                    {
                        g = 255;
                    }

                    if (b < 0)
                    {
                        b = 0;
                    }
                    else if (b > 255)
                    {
                        b = 255;
                    }
                    r_statistic[r]++;
                    g_statistic[g]++;
                    b_statistic[b]++;
                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return result;
        }

        private Bitmap outlier_filtering(int[] r_statistic, int[] g_statistic, int[] b_statistic, int flag)
        {
            int size = flag;
            Color c;
            int i, j, m, n;
            int r = 0, g = 0, b = 0;
            int w = image.Width, h = image.Height;
            int start_x, start_y;
            Bitmap result = new Bitmap(w, h);
            for(j=0; j<h; j++)
            {
                for(i=0; i<w; i++)
                {
                    start_x = (i - (size / 2) + w) % w;
                    start_y = (j - (size / 2) + h) % h;
                    for(n=j; n<j+size; n++)
                    {
                        for(m=i; m<i+size; m++)
                        {
                            if(i - m != j - n)
                            {
                                c = image.GetPixel(m % w, n % h);
                                r += c.R;
                                g += c.G;
                                b += c.B;
                            }
                        }
                    }
                    c = image.GetPixel(i, j);
                    r = r / (size * size - 1);
                    if(Math.Abs(c.R - r) <= r)
                    {
                        r = c.R;
                    }
                    g = g / (size * size - 1);
                    if (Math.Abs(c.G - r) <= g)
                    {
                        g = c.G;
                    }
                    b = b / (size * size - 1);
                    if (Math.Abs(c.B - b) <= b)
                    {
                        b = c.B;
                    }
                    r_statistic[r]++;
                    g_statistic[g]++;
                    b_statistic[b]++;
                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            return result;
        }

        private Bitmap median_square_filtering(int[] r_statistic, int[] g_statistic, int[] b_statistic, int flag)
        {
            int size = flag;
            int i, j, m, n;
            int start_x, start_y;
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(w, h);
            List<int> r = new List<int>();
            List<int> g = new List<int>();
            List<int> b = new List<int>();
            Color c;
            for(j=0; j<h; j++)
            {
                for(i=0; i<w; i++)
                {
                    start_x = (i - (size / 2) + w) % w;
                    start_y = (j - (size / 2) + h) % h;
                    for(n = start_y; n < start_y+size; n++)
                    {
                        for(m = start_x; m < start_x+size; m++)
                        {
                            c = image.GetPixel(m % w, n % h);
                            r.Add(c.R);
                            g.Add(c.G);
                            b.Add(c.B);
                        }
                    }
                    r.Sort();
                    g.Sort();
                    b.Sort();
                    r_statistic[r[r.Count / 2]]++;
                    g_statistic[g[g.Count / 2]]++;
                    b_statistic[b[b.Count / 2]]++;
                    result.SetPixel(i, j, Color.FromArgb(r[r.Count / 2], g[g.Count / 2], b[b.Count / 2]));
                    r.Clear();
                    g.Clear();
                    b.Clear();
                }
            }
            return result;
        }

        private Bitmap lowpass_filtering(int[] r_statistic, int[] g_statistic, int[] b_statistic, int flag)
        {
            int size = flag;
            int i, j, m, n, r = 0, g = 0, b = 0;
            int start_x, start_y;
            int w = image.Width, h = image.Height;
            Bitmap result = new Bitmap(w, h);
            Color c;
            for (j = 0; j < h; j++)
            {
                for (i = 0; i < w; i++)
                {
                    start_x = (i - (size / 2) + w) % w;
                    start_y = (j - (size / 2) + h) % h;
                    for (n = start_y; n < start_y + size; n++)
                    {
                        for (m = start_x; m < start_x + size; m++)
                        {
                            c = image.GetPixel(m % w, n % h);
                            r += c.R;
                            g += c.G;
                            b += c.B;
                        }
                    }
                    r = r / (size * size);
                    g = g / (size * size);
                    b = b / (size * size);
                    r_statistic[r]++;
                    g_statistic[g]++;
                    b_statistic[b]++;
                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            return result;
        }
    }
}
