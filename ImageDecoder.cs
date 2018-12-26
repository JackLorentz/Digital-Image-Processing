using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DIP_HW
{
    class ImageDecoder
    {
        public int w = 0;
        public int h = 0;
        //原圖
        private Bitmap image;
        //解壓圖片
        public String manufacturer { get; set; }
        public int version { get; set; }
        public int encoding { get; set; }
        public int bitPerPixel { get; set; }
        public float h_dpi { get; set; }
        public float v_dpi { get; set; }
        public int NPlanes { get; set; }
        public int BytesPerLine { get; set; }
        public int HscreenSize { get; set; }
        public int VscreenSize { get; set; }
        public ListView imageInfoView = new ListView();
        public DataGridView paletteView = new DataGridView();
        public Chart chart = new Chart();
        public GroupBox chartControlBox;
        private CheckBox[] checkBoxes = new CheckBox[4];
        private String[] buttonName = { "Red", "Green", "Blue", "Gray Level" };
        //RGB
        private int[] red_data_set = new int[256];
        private int[] green_data_set = new int[256];
        private int[] blue_data_set = new int[256];
        private int[] gray_level_set = new int[256];
        //調色盤
        private Color[] palette = new Color[256];
        //private List<Color> colors = new List<Color>();

        public void Decoding(Bitmap img)
        {
            this.image = img;
            // 建立資訊表
            this.manufacturer = "Bitmap Compression";
            this.version = 1;
            this.encoding = 1;
            this.bitPerPixel = 0;
            this.w = img.Width;
            this.h = img.Height;
            this.h_dpi = img.HorizontalResolution;
            this.v_dpi = img.VerticalResolution;
            this.NPlanes = 0;
            this.BytesPerLine = 0;
            this.HscreenSize = 0;
            this.VscreenSize = 0;
            // Set the view to show details.
            imageInfoView.Bounds = new Rectangle(new Point(10, 10), new Size(195, 180));
            imageInfoView.View = View.Details;
            // Select the item and subitems when selection is made.
            imageInfoView.FullRowSelect = true;
            // Display grid lines.
            imageInfoView.GridLines = true;

            // Create three items and three sets of subitems for each item.
            ListViewItem item1 = new ListViewItem("Maunfacturer");
            // Place a check mark next to the item.
            item1.SubItems.Add(manufacturer);
            ListViewItem item2 = new ListViewItem("Version");
            item2.SubItems.Add(version.ToString());
            ListViewItem item3 = new ListViewItem("Encoding");
            item3.SubItems.Add(encoding.ToString());
            ListViewItem item4 = new ListViewItem("BitPerPixel");
            item4.SubItems.Add(bitPerPixel.ToString());
            ListViewItem item5 = new ListViewItem("Dimension");
            item5.SubItems.Add(w.ToString() + " x " + h.ToString());
            ListViewItem item6 = new ListViewItem("Resolution");
            item6.SubItems.Add(h_dpi.ToString() + " x " + v_dpi.ToString());
            ListViewItem item7 = new ListViewItem("NPlanes");
            item7.SubItems.Add(NPlanes.ToString());
            ListViewItem item8 = new ListViewItem("BytesPerLine");
            item2.SubItems.Add(BytesPerLine.ToString());
            ListViewItem item9 = new ListViewItem("Screen Size");
            item2.SubItems.Add(HscreenSize.ToString() + " x " + VscreenSize.ToString());


            // Create columns for the items and subitems.
            // Width of -2 indicates auto-size.
            imageInfoView.Columns.Add("Items", -2, HorizontalAlignment.Left);
            imageInfoView.Columns.Add("Contents", -2, HorizontalAlignment.Left);

            //Add the items to the ListView.
            imageInfoView.Items.AddRange(new ListViewItem[] { item1, item2, item3, item4, item5, item6, item7, item8, item9 });
            imageInfoView.BackColor = Color.LightGray;

            //建立調色盤
            int color_index = 0;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color c = image.GetPixel(i, j);
                    if (!is_same_color(c) && color_index < 256)
                    {
                        palette[color_index] = c;
                        color_index++;
                    }
                    //RGB
                    red_data_set[c.R]++;
                    green_data_set[c.G]++;
                    blue_data_set[c.B]++;
                    //Gray level
                    int grayScale = (int)((double)(c.R + c.G + c.B) / (double)3);
                    gray_level_set[grayScale]++;
                }
            }
            paletteView.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
            paletteView.ColumnCount = 16;
            paletteView.RowCount = 16;
            paletteView.AllowUserToResizeRows = false;
            paletteView.AllowUserToResizeColumns = false;
            paletteView.ColumnHeadersVisible = false;
            paletteView.RowHeadersVisible = false;
            paletteView.ReadOnly = true;

            color_index = 0;
            for (int i = 0; i < 16; i++)
            {
                paletteView.Rows[i].Height = 12;
                paletteView.Columns[i].Width = 12;
                for (int j = 0; j < 16; j++)
                {
                    paletteView.Rows[i].Cells[j].Style.BackColor = palette[color_index];
                    color_index++;
                }
            }

            //建立Histogram
            chart.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
            chart.BackColor = Color.LightGray;
            chart.Series.Add("Red");
            chart.Series.Add("Green");
            chart.Series.Add("Blue");
            chart.Series.Add("Gray_Level");
            chart.ChartAreas.Add("RGB");
            chart.Height = 190;
            chart.Width = 190;
            //chart.ChartAreas["RGB"].AxisX.LabelStyle.Enabled = false;
            chart.ChartAreas["RGB"].AxisY.LabelStyle.Enabled = false;
            chart.ChartAreas["RGB"].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas["RGB"].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas["RGB"].BackColor = Color.Gray;
            //Red
            chart.Series["Red"].ChartType = SeriesChartType.Column;
            chart.Series["Red"].Color = Color.Red;
            chart.Series["Red"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Red"].Points.Add(red_data_set[i]);
            }
            //Green
            chart.Series["Green"].ChartType = SeriesChartType.Column;
            chart.Series["Green"].Color = Color.Green;
            chart.Series["Green"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Green"].Points.Add(green_data_set[i]);
            }
            //Blue
            chart.Series["Blue"].ChartType = SeriesChartType.Column;
            chart.Series["Blue"].Color = Color.Blue;
            chart.Series["Blue"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Blue"].Points.Add(blue_data_set[i]);
            }
            //Gray level
            chart.Series["Gray_Level"].ChartType = SeriesChartType.Column;
            chart.Series["Gray_Level"].Color = Color.LightGray;
            chart.Series["Gray_Level"].ChartArea = "RGB";
            for (int i = 0; i < 256; i++)
            {
                chart.Series["Gray_Level"].Points.Add(gray_level_set[i]);
            }
            //建立直方圖選項
            this.chartControlBox = new GroupBox();
            this.chartControlBox.BackColor = Color.LightGray;
            this.chartControlBox.Bounds = new Rectangle(new Point(10, 10), new Size(190, 30));
            for (int i = 0; i < 4; i++)
            {
                checkBoxes[i] = new CheckBox();
                checkBoxes[i].Text = buttonName[i];
                checkBoxes[i].Checked = true;
                checkBoxes[i].Size = new Size(100, 15);
                checkBoxes[i].CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
                this.chartControlBox.Controls.Add(checkBoxes[i]);
            }
            checkBoxes[0].Location = new Point(0, 0);
            checkBoxes[1].Location = new Point(0, 15);
            checkBoxes[2].Location = new Point(100, 0);
            checkBoxes[3].Location = new Point(100, 15);
        }

        private bool is_same_color(Color c)
        {
            if (palette.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < palette.Length; i++)
            {
                if (c.R == palette[i].R && c.G == palette[i].G && c.B == palette[i].B)
                {
                    return true;
                }
            }
            return false;
        }

        private void checkBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxes[0].Checked)
            {
                chart.Series["Red"].Enabled = true;
            }
            else
            {
                chart.Series["Red"].Enabled = false;
            }

            if (checkBoxes[1].Checked)
            {
                chart.Series["Green"].Enabled = true;
            }
            else
            {
                chart.Series["Green"].Enabled = false;
            }

            if (checkBoxes[2].Checked)
            {
                chart.Series["Blue"].Enabled = true;

            }
            else
            {
                chart.Series["Blue"].Enabled = false;
            }

            if (checkBoxes[3].Checked)
            {
                chart.Series["Gray_Level"].Enabled = true;
            }
            else
            {
                chart.Series["Gray_Level"].Enabled = false;
            }
        }
    }
}
