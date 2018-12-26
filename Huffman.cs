using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;

namespace DIP_HW
{
    class Huffman
    {
        public int w = 0;
        public int h = 0;
        //原圖
        private Bitmap image;
        //解壓圖片
        private Bitmap result;
        public String manufacturer { get; set; }
        public int version { get; set; }
        public int encoding { get; set; }
        public int bitPerPixel { get; set; }
        public int h_dpi { get; set; }
        public int v_dpi { get; set; }
        private Color[] palette = new Color[256];
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
        private List<Color> colors = new List<Color>();
        //每個顏色出現的次數
        private List<int> color_freq = new List<int>();
        //未構成huffman tree的葉子
        private List<Node> nodes = new List<Node>();
        //huffman tree
        private List<Node> tree = new List<Node>();
        //壓縮後的檔案資料
        private int fileSize = 0;
        private List<byte> fileData = new List<byte>();
        //編碼表
        private List<Code> encodingTable = new List<Code>();
        private double avgCodeLength = 0;
        private int longestCodeLength = 0;
        public ListView tableView = new ListView();

        public Bitmap Decoding(byte[] array)
        {
            if (array[0] == 11)
            {
                this.manufacturer = "Huffman Compression";
            }
            this.version = array[1];
            this.encoding = array[2];
            this.bitPerPixel = array[3];
            this.w = array[4] + array[5] * 256;
            this.h = array[6] + array[7] * 256;
            this.h_dpi = array[8] + array[9] * 256;
            this.v_dpi = array[10] + array[11] * 256;
            this.NPlanes = array[12];
            this.BytesPerLine = array[13] + array[14] * 256;
            this.HscreenSize = array[15] + array[16] * 256;
            this.VscreenSize = array[17] + array[18] * 256;
            int entry_size = array[19];
            int table_size = array[20] + array[21] * 256;
            int start_cnt = table_size * entry_size + 22;
            for (int i=22; i<start_cnt; i += 4)
            {
                Code c = new Code();
                int len = array[i];
                int code = array[i + 1] + array[i + 2] * 256 + array[i + 3] * 256 * 256;
                string code_string = "";
                for (int j = 0; j < len; j++)
                {
                    int tmp = (code >> j) & 0x1;
                    code_string = tmp + code_string;
                }
                c.length = len;
                c.code = code;
                c.code_string = code_string;
                encodingTable.Add(c);

                if(len > longestCodeLength)
                {
                    longestCodeLength = len;
                }
            }
            //建立快速解碼表
            Hashtable decodingTable = new Hashtable();
            for(int i=0; i<encodingTable.Count; i++)
            {
                decodingTable.Add(encodingTable[i].code_string, i);
            }
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
            item5.SubItems.Add(w.ToString() + " x " + h.ToString());
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
            imageInfoView.Items.AddRange(new ListViewItem[] { item1, item2, item3, item4, item5, item6, item7, item8, item9 });
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

            int end_cnt = array.Length - encodingTable.Count * 3;
            int palette_index = 0;
            if (version == 6)
            {
                for (int i = end_cnt; i < array.Length; i += 3)
                {
                    this.palette[palette_index] = Color.FromArgb(array[i], array[i + 1], array[i + 2]);
                    this.encodingTable[palette_index].color = palette[palette_index];
                    palette_index++;
                }
            }

            int color_index = 0;
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

            //建立圖片
            List<char> bitStream = new List<char>();
            for(int i=start_cnt; i<end_cnt; i++)
            {
                for(int j=0; j<8; j++)
                {
                    byte bit = (byte)((array[i] >> j) & 0x1);
                    bitStream.Add((char)bit);
                }
            }

            int index = 0, palette_cnt = 0;
            Bitmap result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    string code = "";
                    while (!decodingTable.ContainsKey(code) && code.Length < longestCodeLength)
                    {
                        code += (byte)bitStream[index];
                        index++;
                    }
                    palette_cnt = (int)decodingTable[code];
                    result.SetPixel(y, x, palette[palette_cnt]);
                    //RGB
                    red_data_set[palette[palette_cnt].R]++;
                    green_data_set[palette[palette_cnt].G]++;
                    blue_data_set[palette[palette_cnt].B]++;
                    //Gray level
                    int grayScale = (int)((double)(palette[palette_cnt].R + palette[palette_cnt].G + palette[palette_cnt].B) / (double)3);
                    gray_level_set[grayScale]++;
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

            return result;
        }

        public void Encoding(Bitmap bitmap)
        {
            this.image = bitmap;
            w = image.Width;
            h = image.Height;
            //先建立調色盤和計算各顏色出現頻率
            for (int i=0; i<w; i++)
            {
                for(int j=0; j<h; j++)
                {
                    Color c = image.GetPixel(i, j);
                    if(!is_same_color(c))
                    {
                        colors.Add(c);
                        color_freq.Add(1);
                    }
                }
            }
            //建立huffman tree
            for(int i=0; i<colors.Count; i++)
            {
                Node n = new Node();
                n.color = colors[i];
                n.freq = color_freq[i];
                nodes.Add(n);
            }

            tree = nodes;
            //剩下一個即為根
            while(tree.Count > 1)
            {
                Node n = new Node();
                Node l = new Node();
                Node r = new Node();
                tree = tree.OrderBy(o => o.freq).ToList();
                //建立左子樹
                if (tree[0].left == null && tree[0].right == null)
                {
                    l.color = tree[0].color;
                    l.freq = tree[0].freq;
                }
                //如果該子樹的左右子樹都不是葉子
                else
                {
                    l.left = buildChildTree(tree[0].left);
                    l.right = buildChildTree(tree[0].right);
                    l.freq = l.left.freq + l.right.freq;
                }

                //建立右子樹
                if(tree[1].left == null && tree[1].right == null)
                {
                    r.color = tree[1].color;
                    r.freq = tree[1].freq;
                }
                //如果該子樹的左右子樹都不是葉子
                else
                {
                    r.left = buildChildTree(tree[1].left);
                    r.right = buildChildTree(tree[1].right);
                    r.freq = r.left.freq + r.right.freq;
                }
                n.left = l;
                n.right = r;
                n.freq = n.left.freq + n.right.freq;
                tree.Add(n);
                if (tree.Count > 2)
                {
                    tree.RemoveAt(0);
                    tree.RemoveAt(0);
                }
            }
            //編碼
            int total_nodes = encode(tree[0], 0, 0, 0);
            if (total_nodes == colors.Count)
            {
                //建立顯示表
                // Set the view to show details.
                tableView.Bounds = new Rectangle(new Point(10, 10), new Size(450, 450));
                tableView.View = View.Details;
                // Select the item and subitems when selection is made.
                tableView.FullRowSelect = true;
                // Display grid lines.
                tableView.GridLines = true;
                // Create columns for the items and subitems.
                // Width of -2 indicates auto-size.
                tableView.Columns.Add("Color", -2, HorizontalAlignment.Left);
                tableView.Columns.Add("Length", -2, HorizontalAlignment.Left);
                tableView.Columns.Add("Code", -2, HorizontalAlignment.Left);

                // Create three items and three sets of subitems for each item.
                ListViewItem[] items = new ListViewItem[encodingTable.Count];
                for(int i=0; i<encodingTable.Count; i++)
                {
                    string colorInfo = "(" + encodingTable[0].color.R + " , " + encodingTable[0].color.G + " , " + encodingTable[0].color.B + ")";
                    ListViewItem item = new ListViewItem(colorInfo);
                    item.BackColor = encodingTable[i].color;
                    item.SubItems.Add(encodingTable[i].length.ToString());
                    string code = "";
                    for(int j=0; j<encodingTable[i].length; j++)
                    {
                        int tmp = (encodingTable[i].code >> j) & 0x1;
                        code = tmp.ToString() + code;
                    }
                    encodingTable[i].code_string = code;
                    item.SubItems.Add(encodingTable[i].code_string);
                    items[i] = item;
                }

                //Add the items to the ListView.
                tableView.Items.AddRange(items);
                tableView.BackColor = Color.LightGray;

                //平均碼長
                this.caculateAvgCodeLength();
                //試算推估壓縮後檔案大小
                this.fileSize = 22 + 4 * encodingTable.Count + (int)(this.avgCodeLength * w * h / 8) + 3 * encodingTable.Count;
            }
        }

        public byte[] getFileData()
        {
            byte[] array = new byte[fileData.Count];
            for(int i=0; i<fileData.Count; i++)
            {
                array[i] = fileData[i];
            }

            return array;
        }

        public int getFileSize()
        {
            return fileSize;
        }

        public double getAvgCodeLength()
        {
            return this.avgCodeLength;
        }

        public void makeFile(ToolStripProgressBar pbar)
        {
            pbar.Maximum = fileSize + w * h;
            pbar.Minimum = 0;
            //檔頭:22 bytes(圖片資訊) + 1024 bytes(編碼表)
            //manufacturer 1 byte
            fileData.Add(11);
            //version 1 byte
            fileData.Add(6);
            //encoding 1 byte
            fileData.Add(1);
            //bitPerPixel 1 byte
            fileData.Add(8);
            //width 2 bytes
            fileData.Add((byte)w);
            fileData.Add((byte)(w >> 8));
            //height 2 bytes
            fileData.Add((byte)h);
            fileData.Add((byte)(h >> 8));
            //h_dpi 2 bytes
            fileData.Add(0);
            fileData.Add(0);
            //v_dpi 2 bytes
            fileData.Add(0);
            fileData.Add(0);
            //NPlanes 1 byte
            fileData.Add(1);
            //BytesPerLine 2 bytes
            fileData.Add(0);
            fileData.Add(0);
            //Horizontal Screen Size 2 bytes
            fileData.Add(0);
            fileData.Add(0);
            //Vertical Screen Size 2 bytes
            fileData.Add(0);
            fileData.Add(0);
            //一個編碼表entry大小 1 byte
            fileData.Add(4);
            //編碼表entry數 2 byte
            fileData.Add((byte)encodingTable.Count);
            fileData.Add((byte)(encodingTable.Count >> 8));
            //更新progress bar
            pbar.Increment(22);
            //編碼表
            for (int i = 0; i < encodingTable.Count; i++)
            {
                //一個entry: 4 byte(順序即為顏色編號)
                //碼長
                fileData.Add((byte)encodingTable[i].length);
                fileData.Add((byte)encodingTable[i].code);
                fileData.Add((byte)(encodingTable[i].code >> 8));
                fileData.Add((byte)(encodingTable[i].code >> 16));
                //更新progress bar
                pbar.Increment(4);
            }
            //圖片內容
            string tmp_bitStream = "";
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color c = image.GetPixel(i, j);
                    int k = searchingTable(c);
                    tmp_bitStream += encodingTable[k].code_string;
                    //更新progress bar
                    pbar.Increment(1);
                }
            }
            char[] bitStream = tmp_bitStream.ToCharArray();
            int bit_cnt = 0;
            byte tmp_byte = 0;
            for (int i = 0; i < bitStream.Length; i++)
            {
                if (bit_cnt < 8)
                {
                    int bit = (int)bitStream[i] - 48;
                    tmp_byte += (byte)(bit << bit_cnt);
                    bit_cnt++;
                }
                if (bit_cnt == 8)
                {
                    fileData.Add(tmp_byte);
                    tmp_byte = 0;
                    bit_cnt = 0;
                    //更新progress bar
                    pbar.Increment(1);
                }
            }
            //可能最後還有未滿1個byte的資訊,仍要算一個byte
            fileData.Add(tmp_byte);
            //調色盤
            for (int i = 0; i < encodingTable.Count; i++)
            {
                fileData.Add(encodingTable[i].color.R);
                fileData.Add(encodingTable[i].color.G);
                fileData.Add(encodingTable[i].color.B);
                //更新progress bar
                pbar.Increment(3);
            }
        }

        private void caculateAvgCodeLength()
        {
            int sum = 0;
            for(int i=0; i<encodingTable.Count; i++)
            {
                sum += encodingTable[i].length * encodingTable[i].freq;
            }
            this.avgCodeLength = (double)sum / (double)(w * h);
        }

        private int searchingTable(Color c)
        {
            int index = 0;
            for(int i=0; i<encodingTable.Count; i++)
            {
                if(c.R == encodingTable[i].color.R && c.G == encodingTable[i].color.G && c.B == encodingTable[i].color.B)
                {
                    index = i;
                }
            }
            return index;
        }

        private int encode(Node n, int code, int len, int cnt)
        {
            //不可能只有一邊有葉子的狀況
            if (n.left == null && n.right == null)
            {
                Code c = new Code();
                c.color = n.color;
                c.freq = n.freq;
                c.code = code;
                c.length = len;
                encodingTable.Add(c);

                return cnt + 1;
            }
            else if(n.left != null && n.right != null)
            {
                int left_cnt = encode(n.left, code << 1, len + 1, cnt);
                int right_cnt = encode(n.right, (code << 1) + 1, len + 1, cnt);

                return left_cnt + right_cnt;
            }
            
            return 0;
        }

        private Node buildChildTree(Node orig_node)
        {
            Node new_node = new Node();
            if(orig_node.left == null && new_node.right == null)
            {
                new_node.color = orig_node.color;
                new_node.freq = orig_node.freq; 
            }
            else
            {
                new_node.left = buildChildTree(orig_node.left);
                new_node.right = buildChildTree(orig_node.right);
                new_node.freq = new_node.left.freq + new_node.right.freq;
            }
            return new_node;
        }

        //順便統計顏色數量
        private bool is_same_color(Color c)
        {
            if(colors.Count == 0)
            {
                return false;
            }

            for(int i=0; i<colors.Count; i++)
            {
                if(c.R == colors[i].R && c.G == colors[i].G && c.B == colors[i].B)
                {
                    color_freq[i]++;
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

    class Code
    {
        public Color color;
        public int code;
        public int length;
        public string code_string;
        public int freq;
    }

    class Node
    {
        public Color color;
        public int freq;
        public Node left;
        public Node right;
    }
}
