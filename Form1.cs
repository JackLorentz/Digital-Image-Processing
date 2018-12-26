using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DIP_HW
{
    public partial class Form1 : Form
    {
        
        //元件
        public List<PictureBox> pictureBoxes = new List<PictureBox>();
        public TabControl tabControl = new TabControl();
        //控制物件
        public event EventHandler LoadCompleted;
        public string file_name;
        private int selected_pictureBox = 0;//控制目前點選圖片
        public int tab_pages_top = 0;
        private int pic_x = 300, pic_y = 155;
        private int now_x, now_y;
        private bool is_first_open = true;
        public List<double[]> SNRs = new List<double[]>();
        private PcxDecoder pcxDecoder;
        //裁切工具
        private Graphics g;
        private Pen p = new Pen(Color.Black);
        private bool is_crop_rectangle = false;
        private bool is_crop_circle = false;
        private int start_x, start_y;
        private int end_x = 0, end_y = 0;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.OnLoadCompleted(EventArgs.Empty);
        }

        protected virtual void OnLoadCompleted(EventArgs e)
        {
            var handler = LoadCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void 檔案FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox1 = new PictureBox();
            pictureBox1.Location = new Point(this.pic_x, this.pic_y);
            //緩存PCX所有bytes
            byte[] array;
            //目前處理原圖 => images[0]
            Bitmap image;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.pcx, *.hpcx, *.tiff) | *.jpg; *.jpeg; *.png; *.pcx; *.hpcx; *.tiff" ;
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            //檔名
            this.file_name = dialog.SafeFileName;

            if (dialog.FileName.Contains(".pcx")) {
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
                    if((byte)(array[cnt] & 0xc0) == 0xc0)
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
                        for(int i=0; i<runLength; i++)
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
                if(array[68] == 1)
                {
                    image = pcxDecoder.BuildBitmap(imgBuffer, width, height);
                    pictureBox1.Image = image;
                    pictureBox1.Size = new Size(width, height);
                    pictureBox1.Name = this.file_name + this.tab_pages_top;
                    // 把圖片加入控制
                    this.pictureBoxes.Add(pictureBox1);
                    //啟動其他功能
                    this.toolStripButton1.Enabled = true;
                    this.toolStripDropDownButton2.Enabled = true;
                    this.toolStripDropDownButton3.Enabled = true;
                    this.toolStripDropDownButton4.Enabled = true;
                    this.toolStripDropDownButton5.Enabled = true;
                    this.toolStripDropDownButton6.Enabled = true;
                    this.toolStripDropDownButton7.Enabled = true;
                    this.toolStripDropDownButton8.Enabled = true;
                    this.saveFileToolStripMenuItem.Enabled = true;
                    this.fractalImageEncodingToolStripMenuItem.Enabled = true;
                    this.fractalImageDecodingToolStripMenuItem.Enabled = true;

                    //顯示ListView
                    ListView listView = pcxDecoder.imageInfoView;
                    listView.Location = new Point(875, 30);
                    this.Controls.Add(listView);
                    listView.BringToFront();
                    //顯示直方圖
                    Chart chart = pcxDecoder.chart;
                    chart.Location = new Point(875, 223);
                    this.Controls.Add(chart);
                    chart.BringToFront();
                    GroupBox groupBox = pcxDecoder.chartControlBox;
                    groupBox.Location = new Point(875, 420);
                    this.Controls.Add(groupBox);
                    groupBox.BringToFront();
                    //顯示調色盤
                    DataGridView dataGridView = pcxDecoder.paletteView;
                    dataGridView.Location = new Point(875, 460);
                    this.Controls.Add(dataGridView);
                    dataGridView.BringToFront();
                    //SNR
                    double[] snr_rate = new double[3];
                    this.SNRs.Add(snr_rate);
                    //新增分頁
                    TabPage tabPage = new TabPage();
                    tabPage.BackColor = Color.Gray;
                    tabPage.Text = this.file_name;
                    //加入顯示座標與顏色
                    pictureBox1.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
                    pictureBox1.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
                    pictureBox1.MouseDown += new MouseEventHandler(cutting_MouseDown);
                    pictureBox1.MouseUp += new MouseEventHandler(cutting_MouseUp);
                    //加入圖片至分頁
                    if (this.is_first_open)
                    {
                        tabControl.Location = new Point(0, 25);
                        tabControl.Size = new Size(850, 630);
                        this.Controls.Add(tabControl);
                        this.is_first_open = false;
                    }
                    tabPage.Controls.Add(pictureBox1);
                    this.tabControl.Controls.Add(tabPage);
                    this.tab_pages_top++;
                }

            }
            else if (dialog.FileName.Contains(".hpcx"))
            {
                Huffman huffman = new Huffman();
                array = System.IO.File.ReadAllBytes(dialog.FileName);

                pictureBox1.Image = huffman.Decoding(array);
                pictureBox1.Size = new Size(huffman.w, huffman.h);
                pictureBox1.Name = this.file_name + this.tab_pages_top;
                // 把圖片加入控制
                this.pictureBoxes.Add(pictureBox1);
                //啟動其他功能
                this.toolStripButton1.Enabled = true;
                this.toolStripDropDownButton2.Enabled = true;
                this.toolStripDropDownButton3.Enabled = true;
                this.toolStripDropDownButton4.Enabled = true;
                this.toolStripDropDownButton5.Enabled = true;
                this.toolStripDropDownButton6.Enabled = true;
                this.toolStripDropDownButton7.Enabled = true;
                this.toolStripDropDownButton8.Enabled = true;
                this.saveFileToolStripMenuItem.Enabled = true;
                this.fractalImageEncodingToolStripMenuItem.Enabled = true;
                this.fractalImageDecodingToolStripMenuItem.Enabled = true;

                //顯示ListView
                ListView listView = huffman.imageInfoView;
                listView.Location = new Point(875, 30);
                this.Controls.Add(listView);
                listView.BringToFront();
                //顯示直方圖
                Chart chart = huffman.chart;
                chart.Location = new Point(875, 223);
                this.Controls.Add(chart);
                chart.BringToFront();
                GroupBox groupBox = huffman.chartControlBox;
                groupBox.Location = new Point(875, 420);
                this.Controls.Add(groupBox);
                groupBox.BringToFront();
                //顯示調色盤
                DataGridView dataGridView = huffman.paletteView;
                dataGridView.Location = new Point(875, 460);
                this.Controls.Add(dataGridView);
                dataGridView.BringToFront();
                //SNR
                double[] snr_rate = new double[3];
                this.SNRs.Add(snr_rate);
                //新增分頁
                TabPage tabPage = new TabPage();
                tabPage.BackColor = Color.Gray;
                tabPage.Text = this.file_name;
                //加入顯示座標與顏色
                pictureBox1.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
                pictureBox1.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
                pictureBox1.MouseDown += new MouseEventHandler(cutting_MouseDown);
                pictureBox1.MouseUp += new MouseEventHandler(cutting_MouseUp);
                //加入圖片至分頁
                if (this.is_first_open)
                {
                    tabControl.Location = new Point(0, 25);
                    tabControl.Size = new Size(850, 630);
                    this.Controls.Add(tabControl);
                    this.is_first_open = false;
                }
                tabPage.Controls.Add(pictureBox1);
                this.tabControl.Controls.Add(tabPage);
                this.tab_pages_top++;
            }
            else
            {
                image = (Bitmap)Image.FromFile(dialog.FileName);
                ImageDecoder imageDecoder = new ImageDecoder();
                imageDecoder.Decoding(image);
                pictureBox1.Image = image;
                pictureBox1.Size = new Size(imageDecoder.w, imageDecoder.h);
                pictureBox1.Name = this.file_name + this.tab_pages_top;
                // 把圖片加入控制
                this.pictureBoxes.Add(pictureBox1);
                //啟動其他功能
                this.toolStripButton1.Enabled = true;
                this.toolStripDropDownButton2.Enabled = true;
                this.toolStripDropDownButton3.Enabled = true;
                this.toolStripDropDownButton4.Enabled = true;
                this.toolStripDropDownButton5.Enabled = true;
                this.toolStripDropDownButton6.Enabled = true;
                this.toolStripDropDownButton7.Enabled = true;
                this.toolStripDropDownButton8.Enabled = true;
                this.saveFileToolStripMenuItem.Enabled = true;
                this.fractalImageEncodingToolStripMenuItem.Enabled = true;
                this.fractalImageDecodingToolStripMenuItem.Enabled = true;

                //顯示ListView
                ListView listView = imageDecoder.imageInfoView;
                listView.Location = new Point(875, 30);
                this.Controls.Add(listView);
                listView.BringToFront();
                //顯示直方圖
                Chart chart = imageDecoder.chart;
                chart.Location = new Point(875, 223);
                this.Controls.Add(chart);
                chart.BringToFront();
                GroupBox groupBox = imageDecoder.chartControlBox;
                groupBox.Location = new Point(875, 420);
                this.Controls.Add(groupBox);
                groupBox.BringToFront();
                //顯示調色盤
                DataGridView dataGridView = imageDecoder.paletteView;
                dataGridView.Location = new Point(875, 460);
                this.Controls.Add(dataGridView);
                dataGridView.BringToFront();
                //SNR
                double[] snr_rate = new double[3];
                this.SNRs.Add(snr_rate);
                //新增分頁
                TabPage tabPage = new TabPage();
                tabPage.BackColor = Color.Gray;
                tabPage.Text = this.file_name;
                //加入顯示座標與顏色
                pictureBox1.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
                pictureBox1.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
                pictureBox1.MouseDown += new MouseEventHandler(cutting_MouseDown);
                pictureBox1.MouseUp += new MouseEventHandler(cutting_MouseUp);
                //加入圖片至分頁
                if (this.is_first_open)
                {
                    tabControl.Location = new Point(0, 25);
                    tabControl.Size = new Size(850, 630);
                    this.Controls.Add(tabControl);
                    this.is_first_open = false;
                }
                tabPage.Controls.Add(pictureBox1);
                this.tabControl.Controls.Add(tabPage);
                this.tab_pages_top++;
            }
        }

        public void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //座標
            now_x = e.Location.X;
            now_y = e.Location.Y;
            this.toolStripStatusLabel1.Text = "( X, Y ) = " + "(" + now_x + " , " + now_y + ")";
            //顏色
            Bitmap image = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            if (now_x < image.Width && now_y < image.Height)
            {
                Color color = image.GetPixel(now_x, now_y);
                this.toolStripStatusLabel2.Text = "( R , G , B ) = ( " + color.R + " , " + color.G + " , " + color.B + " )";
                this.toolStripStatusLabel3.BackColor = Color.FromArgb(color.R, color.G, color.B);

                if (is_crop_rectangle || is_crop_circle && g != null)
                {
                    end_x = e.Location.X;
                    end_y = e.Location.Y;
                    //g.DrawRectangle(p, start_x, start_y, end_x, end_y);
                }
            }
            //刷新
            this.Invalidate();
        }

        public void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            PictureBox clickedPictureBox = sender as PictureBox;
            for (int i = 0; i < pictureBoxes.Count; i++)
            {
                if (clickedPictureBox.Name == pictureBoxes[i].Name)
                {
                    this.selected_pictureBox = i;
                    if(i == 0)
                    {
                        this.toolStripStatusLabel4.Text = "SNR : ( ---, ---, --- )";
                    }
                    else
                    {
                        this.toolStripStatusLabel4.Text = "SNR : (" + SNRs[this.selected_pictureBox][0].ToString("#0.000") + " , "
                            + SNRs[this.selected_pictureBox][1].ToString("#0.000") + " , "
                            + SNRs[this.selected_pictureBox][2].ToString("#0.000") + ")";
                    }
                   
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //原圖不能刪
            if (this.tab_pages_top > 1)
            {
                this.tab_pages_top--;
                this.tabControl.Controls.RemoveAt(this.tab_pages_top);
                this.selected_pictureBox = 0;
                this.toolStripStatusLabel4.Text = "SNR = ( ---, ---, ---)";
            }
        }

        private void 灰階效果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int new_w = pictureBoxes[this.selected_pictureBox].Width;
            int new_h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap newImage = new Bitmap(new_w, new_h);
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            for (int i=0; i< new_w; i++)
            {
                for(int j=0; j< new_h; j++)
                {
                    Color color = original.GetPixel(i, j);
                    int grayScale = (color.R + color.G + color.B) / 3;
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    newImage.SetPixel(i, j, newColor);
                }
            }
            double[] snr_rate = SNR(original, newImage);
            this.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , " 
                + snr_rate[1].ToString("#0.000") + " , " 
                + snr_rate[2].ToString("#0.000") + ")";
            this.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = "Gray_level_" + this.file_name + this.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = newImage;
            pic.Size = new Size(new_w, new_h); 
            pic.Location = new Point(this.pic_x, this.pic_y);
            pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
            pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
            pic.Name = "Gray_level_" + this.file_name + this.tab_pages_top;
            this.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            this.tabControl.Controls.Add(tabPage);
            this.tab_pages_top++;
        }

        private void negativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int new_w = pictureBoxes[this.selected_pictureBox].Width;
            int new_h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap newImage = new Bitmap(new_w, new_h);
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            for (int i = 0; i < new_w; i++)
            {
                for (int j = 0; j < new_h; j++)
                {
                    Color color = original.GetPixel(i, j);
                    int r = 255 - color.R;
                    int g = 255 - color.G;
                    int b = 255 - color.B;
                    Color newColor = Color.FromArgb(r, g, b);
                    newImage.SetPixel(i, j, newColor);
                }
            }
            double[] snr_rate = SNR(original, newImage);
            this.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            this.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = "Negative_" + this.file_name + this.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = newImage;
            pic.Size = new Size(new_w, new_h);
            pic.Location = new Point(this.pic_x, this.pic_y);
            pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
            pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
            pic.Name = "Negative_" + this.file_name + this.tab_pages_top;
            this.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            this.tabControl.Controls.Add(tabPage);
            this.tab_pages_top++;
        }

        private void linearInterplationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            int new_w = w * 2;
            int new_h = h * 2;
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X -= w / 2;
            loc.Y -= h / 2;
            Bitmap newImage = new Bitmap(new_w, new_h);
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            int u = 0, v = 0, r = 0, g = 0, b = 0;
             for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                    Color c1 = original.GetPixel(i, j);
                    newImage.SetPixel(u, v, c1);

                    Color c2;
                    if (i+1 < w)
                    {
                        c2 = original.GetPixel(i + 1, j);
                    }
                    else
                    {
                        c2 = c1;
                    }
                    r = (c2.R + c1.R) / 2;
                    g = (c2.G + c1.G) / 2;
                    b = (c2.B + c1.B) / 2;
                    newImage.SetPixel(u + 1, v, Color.FromArgb(r, g, b));

                    Color c3;
                    if (j + 1 < h)
                    {
                        c3 = original.GetPixel(i, j + 1);
                    }
                    else
                    {
                        c3 = c1;
                    }
                    r = (c3.R + c1.R) / 2;
                    g = (c3.G + c1.G) / 2;
                    b = (c3.B + c1.B) / 2;
                    newImage.SetPixel(u, v + 1, Color.FromArgb(r, g, b));

                    Color c4;
                    if (i + 1 < w && j + 1 < h)
                    {
                        c4 = original.GetPixel(i + 1, j + 1);
                    }
                    else
                    {
                        c4 = c1;
                    }
                    r = (c4.R + c1.R) / 2;
                    g = (c4.G + c1.G) / 2;
                    b = (c4.B + c1.B) / 2;
                    newImage.SetPixel(u + 1, v + 1, Color.FromArgb(r, g, b));
                    v += 2;
                }
                v = 0;
                u += 2;
            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size(new_w, new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        private void simpleDuplicationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            int new_w = (int)(w * 0.5);
            int new_h = (int)(h * 0.5);
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X += new_w / 2;
            loc.Y += new_h / 2;
            Bitmap newImage = new Bitmap(new_w, new_h);
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            int u = 0, v = 0;
            for (int i = 0; i < new_w; i++)
            {
                for (int j = 0; j < new_h; j++)
                {
                    Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                    Color c1 = original.GetPixel(u, v);

                    Color c2;
                    if (u + 1 < w)
                    {
                        c2 = original.GetPixel(u + 1, v);
                    }
                    else
                    {
                        c2 = original.GetPixel(u, v);
                    }

                    Color c3;
                    if (v + 1 < h)
                    {
                        c3 = original.GetPixel(u, v + 1);
                    }
                    else
                    {
                        c3 = original.GetPixel(u, v);
                    }

                    Color c4;
                    if (u + 1 < w && v + 1 < h)
                    {
                        c4 = original.GetPixel(u + 1, v + 1);
                    }
                    else
                    {
                        c4 = original.GetPixel(u, v);
                    }
                    int r = (c1.R + c2.R + c3.R + c4.R) / 4;
                    int g = (c1.G + c2.G + c3.G + c4.G) / 4;
                    int b = (c1.B + c2.B + c3.B + c4.B) / 4;
                    newImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                    v += 2;
                }
                v = 0;
                u += 2;
            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size(new_w, new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        private void linearInterpolationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int new_w = (int)(pictureBoxes[this.selected_pictureBox].Width * 0.5);
            int new_h = (int)(pictureBoxes[this.selected_pictureBox].Height * 0.5);
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X += new_w / 2;
            loc.Y += new_h / 2;
            Bitmap newImage = new Bitmap(new_w, new_h);
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            int u = 0, v = 0;
            for (int i = 0; i < new_w; i++)
            {
                for (int j = 0; j < new_h; j++)
                {
                    Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                    Color color = original.GetPixel(u, v);
                    newImage.SetPixel(i, j, color);
                    v += 2;
                }
                v = 0;
                u += 2;
            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size(new_w, new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        private void holeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            //每次旋轉30度
            //角度轉弧度
            double rad1 = Math.PI * 30 / 180.0;
            double rad2 = Math.PI * 60 / 180.0;
            double new_w = w * Math.Cos(rad1) + h * Math.Cos(rad2);
            double new_h = w * Math.Cos(rad2) + h * Math.Cos(rad1);
            //新圖片位置
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X -= (int)((new_w - (double)w) / (double)2);
            loc.Y -= (int)((new_h - (double)h) / (double)2);
            Bitmap newImage = new Bitmap((int)new_w, (int)new_h);
            //先填背景顏色
            for (int i = 0; i < newImage.Height; i++)
            {
                for (int j = 0; j < newImage.Width; j++)
                {
                    newImage.SetPixel(j, i, Color.Gray);
                }
            }
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString())
            //x和y是原圖的座標系統
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double u = 0, v = 0;//新座標系統位置
                    double new_x, new_y;
                    Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                    Color color = original.GetPixel(x, y);
                    //先找出旋轉位置
                    new_x = x * Math.Cos(rad1) - y * Math.Sin(rad1);
                    new_y = x * Math.Sin(rad1) + y * Math.Cos(rad1);
                    //在尋找這個原圖位置在新座標系統的位置
                    u = new_x + h * Math.Cos(rad2);
                    v = new_y;
                    if ((int)u < (int)new_w && (int)v < (int)new_h)
                    {
                        newImage.SetPixel((int)u, (int)v, color);
                    }
                }
               
            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size((int)new_w, (int)new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        private void withoutHoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            //每次旋轉30度
            //角度轉弧度
            double rad1 = Math.PI * 30 / 180.0;
            double rad2 = Math.PI * 60 / 180.0;
            double new_w = w * Math.Cos(rad1) + h * Math.Cos(rad2);
            double new_h = w * Math.Cos(rad2) + h * Math.Cos(rad1);
            //新圖片位置
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X -= (int)((new_w - (double)w) / (double)2);
            loc.Y -= (int)((new_h - (double)h) / (double)2);
            //補洞位置
            bool[,] holeLoc = new bool[(int)new_w, (int)new_h];
            Bitmap newImage = new Bitmap((int)new_w, (int)new_h);
            //先填背景顏色
            for (int i = 0; i < newImage.Height; i++)
            {
                for (int j = 0; j < newImage.Width; j++)
                {
                    newImage.SetPixel(j, i, Color.Gray);
                    holeLoc[i, j] = true;
                }
            }
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString())
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //x和y是原圖的座標系統
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double u = 0, v = 0;//新座標系統位置
                    double new_x, new_y;
                    Color color = original.GetPixel(x, y);
                    //先找出旋轉位置
                    new_x = x * Math.Cos(rad1) - y * Math.Sin(rad1);
                    new_y = x * Math.Sin(rad1) + y * Math.Cos(rad1);
                    //在尋找這個原圖位置在新座標系統的位置
                    u = new_x + h * Math.Cos(rad2);
                    v = new_y;
                    if ((int)u < (int)new_w && (int)v < (int)new_h)
                    {
                        newImage.SetPixel((int)u, (int)v, color);
                        holeLoc[(int)u, (int)v] = false;
                    }
                }

            }
            //補洞
            for(int v = 1; v < (int)new_h; v++)
            {
                for(int u = 1; u < (int)new_w; u++)
                {
                    if(holeLoc[u, v] && !holeLoc[u, v-1] && !holeLoc[u-1, v])
                    {
                        double x = 0, y = 0;
                        double original_x, original_y;
                        x = u - h * Math.Cos(rad2);
                        y = v;
                        original_x = x * Math.Cos(-rad1) - y * Math.Sin(-rad1);
                        original_y = x * Math.Sin(-rad1) + y * Math.Cos(-rad1);
                        Color color = original.GetPixel((int)original_x, (int)original_y);
                        newImage.SetPixel(u, v, color);
                    }
                }
            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size((int)new_w, (int)new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        private void holeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            //每次旋轉30度
            //角度轉弧度
            double rad1 = Math.PI * 30 / 180.0;
            double rad2 = Math.PI * 60 / 180.0;
            double new_w = w * Math.Cos(rad1) + h * Math.Cos(rad2);
            double new_h = w * Math.Cos(rad2) + h * Math.Cos(rad1);
            //新圖片位置
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X -= (int)((new_w - (double)w) / (double)2);
            loc.Y -= (int)((new_h - (double)h) / (double)2);
            Bitmap newImage = new Bitmap((int)new_w, (int)new_h);
            //先填背景顏色
            for (int i = 0; i < newImage.Height; i++)
            {
                for (int j = 0; j < newImage.Width; j++)
                {
                    newImage.SetPixel(j, i, Color.Gray);
                }
            }
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString())
            //x和y是原圖的座標系統
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double u = 0, v = 0;//新座標系統位置
                    double new_x, new_y;
                    Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                    Color color = original.GetPixel(x, y);
                    //先找出旋轉位置
                    new_x = x * Math.Cos(-rad1) - y * Math.Sin(-rad1);
                    new_y = x * Math.Sin(-rad1) + y * Math.Cos(-rad1);
                    //在尋找這個原圖位置在新座標系統的位置
                    u = new_x; 
                    v = new_y + w * Math.Cos(rad2);
                    if((int)u < (int)new_w && (int)v < (int)new_h)
                    {
                        newImage.SetPixel((int)u, (int)v, color);
                    }
                }

            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size((int)new_w, (int)new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        private void withoutHoleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            //每次旋轉30度
            //角度轉弧度
            double rad1 = Math.PI * 30 / 180.0;
            double rad2 = Math.PI * 60 / 180.0;
            double new_w = w * Math.Cos(rad1) + h * Math.Cos(rad2);
            double new_h = w * Math.Cos(rad2) + h * Math.Cos(rad1);
            //新圖片位置
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X -= (int)((new_w - (double)w) / (double)2);
            loc.Y -= (int)((new_h - (double)h) / (double)2);
            //補洞位置
            bool[,] holeLoc = new bool[(int)new_w, (int)new_h];
            Bitmap newImage = new Bitmap((int)new_w, (int)new_h);
            //先填背景顏色
            for (int i = 0; i < newImage.Height; i++)
            {
                for (int j = 0; j < newImage.Width; j++)
                {
                    newImage.SetPixel(j, i, Color.Gray);
                    holeLoc[i, j] = true;
                }
            }
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString())
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //x和y是原圖的座標系統
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double u = 0, v = 0;//新座標系統位置
                    double new_x, new_y;
                    Color color = original.GetPixel(x, y);
                    //先找出旋轉位置
                    new_x = x * Math.Cos(-rad1) - y * Math.Sin(-rad1);
                    new_y = x * Math.Sin(-rad1) + y * Math.Cos(-rad1);
                    //在尋找這個原圖位置在新座標系統的位置
                    u = new_x;
                    v = new_y + w * Math.Cos(rad2);
                    if ((int)u < (int)new_w && (int)v < (int)new_h)
                    {
                        newImage.SetPixel((int)u, (int)v, color);
                        holeLoc[(int)u, (int)v] = false;
                    }
                }

            }
            //補洞
            for (int v = 1; v < (int)new_h; v++)
            {
                for (int u = 1; u < (int)new_w; u++)
                {
                    if (holeLoc[u, v] && !holeLoc[u, v - 1] && !holeLoc[u - 1, v])
                    {
                        double x = 0, y = 0;
                        double original_x, original_y;
                        x = u;
                        y = v - w * Math.Cos(rad2);
                        original_x = x * Math.Cos(rad1) - y * Math.Sin(rad1);
                        original_y = x * Math.Sin(rad1) + y * Math.Cos(rad1);
                        Color color = original.GetPixel((int)original_x, (int)original_y);
                        newImage.SetPixel(u, v, color);
                    }
                }
            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size((int)new_w, (int)new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        private void transparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Transparent transparentForm = new Transparent();
            transparentForm.Owner = this;
            transparentForm.Show(this);
        }

        private void bitStreamingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bit_Plane_Slicing form = new Bit_Plane_Slicing();
            Bitmap[] bitmaps = new Bitmap[8];

            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap grayImage = new Bitmap(w, h);
            for (int i = 0; i < 8; i++)
            {
                bitmaps[i] = new Bitmap(w, h);
            }
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color color = original.GetPixel(i, j);
                    int grayScale = (color.R + color.G + color.B) / 3;
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    grayImage.SetPixel(i, j, newColor);
                }
            }
            //切片
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<h; j++)
                {
                    int bitPlane = 0;
                    Color c = grayImage.GetPixel(i, j);
                    int[] planes = new int[8];
                    for (int k = 0; k < 8; k++) planes[k] = 0;
                    //
                    //byte[] colors = System.BitConverter.GetBytes(c.R);
                    //0 bit plane 
                    bitPlane = c.R & 0x1;
                    if(bitPlane == 1)
                    {
                        planes[0] = 255;
                    }
                    //1 bit plane 
                    bitPlane = c.R & 0x2;// 00000010
                    if (bitPlane == 0x2)
                    {
                        planes[1] = 255;
                    }
                    //2 bit plane 
                    bitPlane = c.R & 0x4;// 00000100
                    if (bitPlane == 0x4)
                    {
                        planes[2] = 255;
                    }
                    //3 bit plane
                    bitPlane = c.R & 0x8;// 00001000
                    if (bitPlane == 0x8)
                    {
                        planes[3] = 255;
                    }
                    //4 bit plane
                    bitPlane = c.R & 0x10;// 00010000
                    if (bitPlane == 0x10)
                    {
                        planes[4] = 255;
                    }
                    //5 bit plane
                    bitPlane = c.R & 0x20;// 00100000
                    if (bitPlane == 0x20)
                    {
                        planes[5] = 255;
                    }
                    //6 bit plane
                    bitPlane = c.R & 0x40;// 01000000
                    if (bitPlane == 0x40)
                    {
                        planes[6] = 255;
                    }
                    //7 bit plane
                    bitPlane = c.R & 0x80;// 10000000
                    if (bitPlane == 0x80)
                    {
                        planes[7] = 255;
                    }
                    for(int k=0; k<8; k++)
                    {
                        bitmaps[k].SetPixel(i, j, Color.FromArgb(planes[k], planes[k], planes[k]));
                    }
                }
            }
            //
            form.flag = 0;
            form.bitmaps = bitmaps;
            form.Owner = this;
            form.Show(this);
        }

        private void grayCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bit_Plane_Slicing form = new Bit_Plane_Slicing();
            Bitmap[] bitmaps = new Bitmap[8];

            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap grayImage = new Bitmap(w, h);
            for (int i = 0; i < 8; i++)
            {
                bitmaps[i] = new Bitmap(w, h);
            }
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                    Color color = original.GetPixel(i, j);
                    int grayScale = (color.R + color.G + color.B) / 3;
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    grayImage.SetPixel(i, j, newColor);
                }
            }
            //切片
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int bitPlane = 0;
                    Color c = grayImage.GetPixel(i, j);
                    int[] planes = new int[8];
                    for (int k = 0; k < 8; k++) planes[k] = 0;
                    //
                    //byte[] colors = System.BitConverter.GetBytes(c.R);
                    //0 bit plane 
                    bitPlane = c.R & 0x1;
                    if (bitPlane == 1)
                    {
                        planes[0] = 255;
                    }
                    //1 bit plane 
                    bitPlane = c.R & 0x2;// 00000010
                    if (bitPlane == 0x2)
                    {
                        planes[1] = 255;
                    }
                    //2 bit plane 
                    bitPlane = c.R & 0x4;// 00000100
                    if (bitPlane == 0x4)
                    {
                        planes[2] = 255;
                    }
                    //3 bit plane
                    bitPlane = c.R & 0x8;// 00001000
                    if (bitPlane == 0x8)
                    {
                        planes[3] = 255;
                    }
                    //4 bit plane
                    bitPlane = c.R & 0x10;// 00010000
                    if (bitPlane == 0x10)
                    {
                        planes[4] = 255;
                    }
                    //5 bit plane
                    bitPlane = c.R & 0x20;// 00100000
                    if (bitPlane == 0x20)
                    {
                        planes[5] = 255;
                    }
                    //6 bit plane
                    bitPlane = c.R & 0x40;// 01000000
                    if (bitPlane == 0x40)
                    {
                        planes[6] = 255;
                    }
                    //7 bit plane
                    bitPlane = c.R & 0x80;// 10000000
                    if (bitPlane == 0x80)
                    {
                        planes[7] = 255;
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        bitmaps[k].SetPixel(i, j, Color.FromArgb(planes[k], planes[k], planes[k]));
                    }
                }
            }
            //
            form.flag = 1;
            form.bitmaps = bitmaps;
            form.Owner = this;
            form.Show(this);
        }

        private void otsuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Otsu otsu = new Otsu();
            otsu.flag = 0;
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            otsu.image = new Bitmap(original);
            otsu.Owner = this;
            otsu.Show(this);
        }

        private void kmeansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Otsu k_means = new Otsu();
            k_means.flag = 1;
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            k_means.image = new Bitmap(original);
            k_means.Owner = this;
            k_means.Show(this);
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            is_crop_rectangle = true;
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            is_crop_circle = true;
        }

        private void cutting_MouseDown(object sender, MouseEventArgs e)
        {
            if (is_crop_rectangle || is_crop_circle)
            {
                start_x = e.Location.X;
                start_y = e.Location.Y;
                end_x = start_x;
                end_y = start_y;
                g = this.pictureBoxes[this.selected_pictureBox].CreateGraphics();
            }
        }

        private void lowpassSpatialFilteringToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 0;
            filter.image = new Bitmap(original);
            filter.Owner = this;
            filter.Show(this);
        }

        private void medianFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 1;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(3000);
        }

        private void pseudoMedianFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 2;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image files (*.hpcx) | *.hpcx";
            saveFileDialog.Title = "Save an Output File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                HuffmanForm huffmanForm = new HuffmanForm();
                Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                huffmanForm.saveFileDialog = saveFileDialog;
                //先建立編碼表
                Huffman huffman = new Huffman();
                huffman.Encoding(original);
                huffmanForm.listView = huffman.tableView;
                huffmanForm.avg_length = huffman.getAvgCodeLength();
                //建立好編碼表後開始建立檔案
                huffman.makeFile(this.toolStripProgressBar1);
                //輸出檔案
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, huffman.getFileData());
                //huffmanForm.toolStripStatusLabel3.Text = "壓縮比: " + ((double)pcxDecoder.getFileSize() / (double)huffman.getFileSize()).ToString("#0.00");
                //顯示壓縮資訊
                huffmanForm.Owner = this;
                huffmanForm.Show(this);
            }
        }

        private void fractalImageEncodingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image files (*.fractal) | *.fractal";
            saveFileDialog.Title = "Save an Output File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
               
                //System.IO.File.WriteAllBytes(saveFileDialog.FileName, buffer);
            }*/
            FractalImageCoding fic = new FractalImageCoding();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            byte[] buffer = fic.Encoding(original, this.toolStripProgressBar1);
            MessageBox.Show("Compression Completed");
        }

        private void fractalImageDecodingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "Image files (*.fractal) | *.fractal";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                fractalDecodingForm fdf = new fractalDecodingForm();
                fdf.fic_array = System.IO.File.ReadAllBytes(dialog.FileName);
                fdf.Owner = this;
                fdf.Show(this);
            }
        }
        //Highpass Filter
        private void lowpassSpatialFilteringToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 3;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void highBoostFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 4;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void derivativeFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 5;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void sobelOperatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 6;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void prewittOperatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 7;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void adaptiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int w = pictureBoxes[this.selected_pictureBox].Width;
            int h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap grayImage = new Bitmap(w, h);
            Bitmap threshold = new Bitmap(w, h);
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            Color color, newColor;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    color = original.GetPixel(i, j);
                    int grayScale = (color.R + color.G + color.B) / 3;
                    newColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    grayImage.SetPixel(i, j, newColor);
                }
            }
            //windows大小為7*7
            double avg;
            int sum = 0, start_x = 0, start_y = 0;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    start_x = (w + i - 3) % w;
                    start_y = (h + j - 3) % h;
                    for (int m = start_x; m < start_x + 7; m++)
                    {
                        for (int n = start_y; n < start_y + 7; n++)
                        {
                            color = grayImage.GetPixel(m % w, n % h);
                            sum += color.R;
                        }
                    }
                    avg = (double)sum / (double)64;
                    color = grayImage.GetPixel(i, j);
                    if (color.R > avg)
                    {
                        threshold.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    }
                    else
                    {
                        threshold.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                    sum = 0;
                    start_x = 0;
                    start_y = 0;
                }
            }
            //
            double[] snr_rate = SNR(original, threshold);
            this.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            this.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = "Thresholding_" + this.file_name + this.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = threshold;
            pic.Size = new Size(w, h);
            pic.Location = new Point(this.pic_x, this.pic_y);
            pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
            pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
            pic.Name = "Threaholding_" + this.file_name + this.tab_pages_top;
            this.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            this.tabControl.Controls.Add(tabPage);
            this.tab_pages_top++;
        }

        private void manualModificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManualThresholding MT = new ManualThresholding();
            MT.Owner = this;
            MT.Show(this);
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int new_w = pictureBoxes[this.selected_pictureBox].Width;
            int new_h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap newImage = new Bitmap(new_w, new_h);
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            Color c;
            int i, j;
            for(j=0; j<new_h; j++)
            {
                for(i=0; i<new_w; i++)
                {
                    if(j < new_h / 2)
                    {
                        c = original.GetPixel(i, (new_h - 1) - j);
                        newImage.SetPixel(i, j, c);
                    }
                    else
                    {
                        c = original.GetPixel(i, j);
                        newImage.SetPixel(i, j, c);
                    }
                }
            }
            double[] snr_rate = SNR(original, newImage);
            this.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            this.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = "Gray_level_" + this.file_name + this.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = newImage;
            pic.Size = new Size(new_w, new_h);
            pic.Location = new Point(this.pic_x, this.pic_y);
            pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
            pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
            pic.Name = "Gray_level_" + this.file_name + this.tab_pages_top;
            this.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            this.tabControl.Controls.Add(tabPage);
            this.tab_pages_top++;
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int new_w = pictureBoxes[this.selected_pictureBox].Width;
            int new_h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap newImage = new Bitmap(new_w, new_h);
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            Color c;
            int i, j;
            for (j = 0; j < new_h; j++)
            {
                for (i = 0; i < new_w; i++)
                {
                    if (i < new_w / 2)
                    {
                        c = original.GetPixel((new_w - 1) - i, j);
                        newImage.SetPixel(i, j, c);
                    }
                    else
                    {
                        c = original.GetPixel(i, j);
                        newImage.SetPixel(i, j, c);
                    }
                }
            }
            double[] snr_rate = SNR(original, newImage);
            this.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            this.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = "Gray_level_" + this.file_name + this.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = newImage;
            pic.Size = new Size(new_w, new_h);
            pic.Location = new Point(this.pic_x, this.pic_y);
            pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
            pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
            pic.Name = "Gray_level_" + this.file_name + this.tab_pages_top;
            this.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            this.tabControl.Controls.Add(tabPage);
            this.tab_pages_top++;
        }

        private void degreeDiagnalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int new_w = pictureBoxes[this.selected_pictureBox].Width;
            int new_h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap newImage = new Bitmap(new_w, new_h);
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            Color c;
            int i, j;
            for (j = 0; j < new_h; j++)
            {
                for (i = 0; i < new_w; i++)
                {
                    if(i < (new_h - 1) - j && j < (new_w - 1) - i)
                    {
                        c = original.GetPixel((new_h - 1) - j, (new_w - 1) - i);
                        newImage.SetPixel(i, j, c);
                    }
                    else
                    {
                        c = original.GetPixel(i, j);
                        newImage.SetPixel(i, j, c);
                    }
                }
            }
            double[] snr_rate = SNR(original, newImage);
            this.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            this.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = "Gray_level_" + this.file_name + this.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = newImage;
            pic.Size = new Size(new_w, new_h);
            pic.Location = new Point(this.pic_x, this.pic_y);
            pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
            pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
            pic.Name = "Gray_level_" + this.file_name + this.tab_pages_top;
            this.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            this.tabControl.Controls.Add(tabPage);
            this.tab_pages_top++;
        }

        private void degreeDiagnalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int new_w = pictureBoxes[this.selected_pictureBox].Width;
            int new_h = pictureBoxes[this.selected_pictureBox].Height;
            Bitmap newImage = new Bitmap(new_w, new_h);
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            Color c;
            int i, j;
            for (j = 0; j < new_h; j++)
            {
                for (i = 0; i < new_w; i++)
                {
                    if (i > j)
                    {
                        c = original.GetPixel(j, i);
                        newImage.SetPixel(i, j, c);
                    }
                    else
                    {
                        c = original.GetPixel(i, j);
                        newImage.SetPixel(i, j, c);
                    }
                }
            }
            double[] snr_rate = SNR(original, newImage);
            this.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            this.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = "Gray_level_" + this.file_name + this.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = newImage;
            pic.Size = new Size(new_w, new_h);
            pic.Location = new Point(this.pic_x, this.pic_y);
            pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
            pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
            pic.Name = "Gray_level_" + this.file_name + this.tab_pages_top;
            this.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            this.tabControl.Controls.Add(tabPage);
            this.tab_pages_top++;
        }

        private void outlierFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 8;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void medianFilteringSquareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter filter = new Filter();
            Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            filter.flag = 9;
            filter.image = original;
            filter.Owner = this;
            filter.Show(this);
        }

        private void histogramEqualizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistogramProcessing form = new HistogramProcessing();
            Bitmap original = new Bitmap((Bitmap)pictureBoxes[this.selected_pictureBox].Image);
            form.image = original;
            form.Owner = this;
            form.Show(this);
        }

        private void histogramSpecificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Histogram_Specification hs = new Histogram_Specification();
            hs.Owner = this;
            hs.Show(this);
        }

        private void contrastStretchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Contrast_Stretching cs = new Contrast_Stretching();
            cs.image = new Bitmap((Bitmap)pictureBoxes[this.selected_pictureBox].Image);
            cs.Owner = this;
            cs.Show(this);
        }

        private void yIQSlicingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            YIQ_Slicing yiq = new YIQ_Slicing();
            yiq.image = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
            yiq.Owner = this;
            yiq.Show(this);
        }

        private void cutting_MouseUp(object sender, MouseEventArgs e)
        {
            if (is_crop_circle)
            {
                int w = pictureBoxes[this.selected_pictureBox].Width;
                int h = pictureBoxes[this.selected_pictureBox].Height;
                Bitmap newImage = new Bitmap(w, h);
                Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
                double r = Math.Sqrt(Math.Pow(end_x - start_x, 2) + Math.Pow(end_y - start_y, 2)) / (double)2;
                double center_x = (double)(start_x + end_x) / (double)2;
                double center_y = (double)(start_y + end_y) / (double)2;
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        Color color = original.GetPixel(i, j);
                        if (Math.Sqrt(Math.Pow(i - center_x, 2) + Math.Pow(j - center_y, 2)) < r)
                        {
                            newImage.SetPixel(i, j, color);
                        }
                        else
                        {
                            newImage.SetPixel(i, j, Color.Gray);   
                        }
                    }
                }
                //新增分頁
                TabPage tabPage = new TabPage();
                tabPage.BackColor = Color.Gray;
                tabPage.Text = "Crop_" + this.file_name + this.tab_pages_top;
                //設定圖片
                PictureBox pic = new PictureBox();
                pic.Image = newImage;
                pic.Size = new Size(w, h);
                pic.Location = new Point(this.pic_x, this.pic_y);
                pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
                pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
                pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
                pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
                pic.Name = "Crop_" + this.file_name + this.tab_pages_top;
                this.pictureBoxes.Add(pic);
                //加入圖片至分頁
                tabPage.Controls.Add(pic);
                this.tabControl.Controls.Add(tabPage);
                this.tab_pages_top++;
                this.is_crop_circle = false;
                this.g.Dispose();
            }
            if (is_crop_rectangle)
            {
                int w = pictureBoxes[this.selected_pictureBox].Width;
                int h = pictureBoxes[this.selected_pictureBox].Height;
                Bitmap newImage = new Bitmap(w, h);
                Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        Color color = original.GetPixel(i, j);
                        if (j > start_y && j < end_y && i > start_x && i < end_x)
                        {
                            newImage.SetPixel(i, j, color);
                        }
                        else
                        {
                            newImage.SetPixel(i, j, Color.Gray);
                        }
                    }
                }
                //新增分頁
                TabPage tabPage = new TabPage();
                tabPage.BackColor = Color.Gray;
                tabPage.Text = "Crop_" + this.file_name + this.tab_pages_top;
                //設定圖片
                PictureBox pic = new PictureBox();
                pic.Image = newImage;
                pic.Size = new Size(w, h);
                pic.Location = new Point(this.pic_x, this.pic_y);
                pic.MouseClick += new MouseEventHandler(pictureBox_MouseClick);
                pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
                pic.MouseDown += new MouseEventHandler(cutting_MouseDown);
                pic.MouseUp += new MouseEventHandler(cutting_MouseUp);
                pic.Name = "Crop_" + this.file_name + this.tab_pages_top;
                this.pictureBoxes.Add(pic);
                //加入圖片至分頁
                tabPage.Controls.Add(pic);
                this.tabControl.Controls.Add(tabPage);
                this.tab_pages_top++;
                this.is_crop_rectangle = false;
                this.g.Dispose();
            }
        }

        private void rGBSlicingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap redImage = new Bitmap(pcxDecoder.width, pcxDecoder.height);
            Bitmap blueImage = new Bitmap(pcxDecoder.width, pcxDecoder.height);
            Bitmap greenImage = new Bitmap(pcxDecoder.width, pcxDecoder.height);

            for (int i = 0; i < pcxDecoder.width; i++)
            {
                for (int j = 0; j < pcxDecoder.height; j++)
                {
                    Bitmap original = (Bitmap)pictureBoxes[0].Image;
                    Color color = original.GetPixel(i, j);
                    Color newColor = Color.FromArgb(color.R, 0, 0);
                    redImage.SetPixel(i, j, newColor);
                    newColor = Color.FromArgb(0, color.B, 0);
                    blueImage.SetPixel(i, j, newColor);
                    newColor = Color.FromArgb(0, 0, color.G);
                    greenImage.SetPixel(i, j, newColor);
                }
            }

            RGB_Slicing rgb_form = new RGB_Slicing();
            PictureBox[] pic = new PictureBox[3];
            int x = 50;
            for(int i=0; i<3; i++)
            {
                pic[i] = new PictureBox();
                pic[i].Location = new Point(x, 41);
                pic[i].Size = new Size(pcxDecoder.width, pcxDecoder.height);
                x += 286;
            }

            pic[0].Image = redImage;
            pic[1].Image = blueImage;
            pic[2].Image = greenImage;

            for (int i = 0; i < 3; i++)
            {
                rgb_form.Controls.Add(pic[i]);  
            }
            rgb_form.Show(this);
        }

        private void simpleDuplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int new_w = pictureBoxes[this.selected_pictureBox].Width * 2;
            int new_h = pictureBoxes[this.selected_pictureBox].Height * 2;
            Point loc = pictureBoxes[this.selected_pictureBox].Location;
            loc.X -= pictureBoxes[this.selected_pictureBox].Width / 2;
            loc.Y -= pictureBoxes[this.selected_pictureBox].Height / 2;
            Bitmap newImage = new Bitmap(new_w, new_h);
            //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
            int u = 0, v = 0;
            for (int i = 0; i < pictureBoxes[this.selected_pictureBox].Width; i++)
            {
                for (int j = 0; j < pictureBoxes[this.selected_pictureBox].Height; j++)
                {
                    Bitmap original = (Bitmap)pictureBoxes[this.selected_pictureBox].Image;
                    Color color = original.GetPixel(i, j);
                    newImage.SetPixel(u, v, color);
                    newImage.SetPixel(u+1, v, color);
                    newImage.SetPixel(u, v+1, color);
                    newImage.SetPixel(u+1, v+1, color);
                    v += 2;
                }
                v = 0;
                u += 2;
            }
            //設定圖片
            pictureBoxes[this.selected_pictureBox].Image = newImage;
            pictureBoxes[this.selected_pictureBox].Size = new Size(new_w, new_h);
            pictureBoxes[this.selected_pictureBox].Location = loc;
        }

        public double[] SNR(Bitmap orig, Bitmap n)
        {
            double[] SNR_rate = new double[3];
            double r_orig_sum = 0.0, g_orig_sum = 0.0, b_orig_sum = 0.0;
            double r_noise_sum = 0.0, g_noise_sum = 0.0, b_noise_sum = 0.0;

            for(int i=0; i<orig.Width; i++)
            {
                for(int j=0; j<orig.Height; j++)
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
