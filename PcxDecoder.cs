using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DIP_HW
{
    class PcxDecoder
    {
        //PCX INFO
        private byte[] array;
        public String manufacturer = "Unknown";
        public int version = 0;
        public int encoding = 0;
        public int bitPerPixel = 0;
        public int width = 0, height = 0;
        public int h_dpi = 0, v_dpi = 0;
        public Color[] palette = new Color[256];
        public int NPlanes = 0;
        public int BytesPerLine = 0;
        public int PaletteInfo = 0;
        public int HscreenSize = 0;
        public int VscreenSize = 0;
        //RGB
        private double[] red_data_set = new double[256];
        private double[] green_data_set = new double[256];
        private double[] blue_data_set = new double[256];
        private double[] gray_level_set = new double[256];
        //控制元件
        public ListView imageInfoView = new ListView();
        public DataGridView paletteView = new DataGridView();
        public Chart chart = new Chart();
        public GroupBox chartControlBox;
        private CheckBox[] checkBoxes = new CheckBox[4];
        private String[] buttonName = { "Red", "Green", "Blue", "Gray Level" };

        public object Integer { get; private set; }

        public PcxDecoder(byte[] array)
        {
            this.array = array;
            decoder();
        }

        public int getFileSize()
        {
            return array.Length;
        }

        public void decoder()
        {
            byte m = array[0];
            if (m == 10)
            {
                this.manufacturer = "Zshoft.pcx";
            }

            this.version = array[1];

            this.encoding = array[2];

            this.bitPerPixel = array[3];

            int x_min = array[4] + array[5] * 256;
            int y_min = array[6] + array[7] * 256;
            int x_max = array[8] + array[9] * 256;
            int y_max = array[10] + array[11] * 256;

            this.width = x_max - x_min + 1;
            this.height = y_max - y_min + 1;

            this.h_dpi = array[12] + array[13] * 256;
            this.v_dpi = array[14] + array[15] * 256;

            int index = 0;
            if(version == 5)
            {
                for(int i = array.Length - 768; i<array.Length; i += 3)
                {
                    this.palette[index] = Color.FromArgb(array[i], array[i + 1], array[i + 2]);
                    index++;
                }
            }
            else
            {
                for (int i = 16; i < 64; i += 3)
                {
                    this.palette[index] = Color.FromArgb(array[i], array[i + 1], array[i + 2]);
                    index++;
                }
            }

            this.NPlanes = array[65];

            this.BytesPerLine = array[66] + array[67] * 256;

            this.PaletteInfo = array[68] + array[69] * 256;

            this.HscreenSize = array[70] + array[71] * 256;

            this.VscreenSize = array[72] + array[73] * 256;

            //建立資訊表
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
            item5.SubItems.Add(width.ToString() + " x " + height.ToString());
            ListViewItem item6 = new ListViewItem("Resolution");
            item6.SubItems.Add(h_dpi.ToString() + " x " + v_dpi.ToString());
            ListViewItem item7 = new ListViewItem("NPlanes");
            item7.SubItems.Add(NPlanes.ToString());
            ListViewItem item8 = new ListViewItem("BytesPerLine");
            item8.SubItems.Add(BytesPerLine.ToString());
            ListViewItem item9 = new ListViewItem("Screen Size");
            item9.SubItems.Add(HscreenSize.ToString() + " x " + VscreenSize.ToString());


            // Create columns for the items and subitems.
            // Width of -2 indicates auto-size.
            imageInfoView.Columns.Add("Items", -2, HorizontalAlignment.Left);
            imageInfoView.Columns.Add("Contents", -2, HorizontalAlignment.Left);

            //Add the items to the ListView.
            imageInfoView.Items.AddRange(new ListViewItem[] { item1, item2 , item3 , item4 , item5 , item6 , item7 , item8 , item9 });
            imageInfoView.BackColor = Color.LightGray;

            //建立調色盤
            paletteView.Bounds = new Rectangle(new Point(10, 10), new Size(195, 195));
            paletteView.ColumnCount = 16;
            paletteView.RowCount = 16;
            paletteView.AllowUserToResizeRows = false;
            paletteView.AllowUserToResizeColumns = false;
            paletteView.ColumnHeadersVisible = false;
            paletteView.RowHeadersVisible = false;
            paletteView.ReadOnly = true;

            int color_index = 0;
            for(int i=0; i<16; i++)
            {
                paletteView.Rows[i].Height = 12;
                paletteView.Columns[i].Width = 12;
                for (int j=0; j<16; j++)
                {
                    paletteView.Rows[i].Cells[j].Style.BackColor = palette[color_index];
                    color_index++;
                }

            }
        }

        public Bitmap BuildBitmap(byte[] buffer, int width, int height)
        {
            int index = 0;

            Bitmap image = new Bitmap(width, height);
            for(int y=0; y< height; y++)
            {
                for(int x=0; x< width; x++)
                {
                    image.SetPixel(x, y, palette[buffer[index]]);
                    //RGB
                    red_data_set[palette[buffer[index]].R]++;
                    green_data_set[palette[buffer[index]].G]++;
                    blue_data_set[palette[buffer[index]].B]++;
                    //Gray level
                    double gray_level = (palette[buffer[index]].R + palette[buffer[index]].G + palette[buffer[index]].B) / 3;
                    gray_level_set[(int)gray_level]++;
                    index++;
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
            for(int i=0; i<256; i++)
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
            for (int i=0; i<4; i++)
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

            return image;
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
