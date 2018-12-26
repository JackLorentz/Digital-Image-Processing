using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIP_HW
{
    public partial class Bit_Plane_Slicing : Form
    {
        public Bitmap[] bitmaps;
        public Bitmap result;
        private Bitmap grayImage;
        //控制物件
        private string file_name;
        private PcxDecoder pcxDecoder;
        //若是1, 用gray code疊合浮水印
        public int flag = 0;

        public Bit_Plane_Slicing()
        {
            InitializeComponent();
        }

        private void Bit_Plane_Slicing_Load(object sender, EventArgs e)
        {
            if (bitmaps != null)
            {
                pictureBox1.Image = bitmaps[0];
                pictureBox2.Image = bitmaps[1];
                pictureBox3.Image = bitmaps[2];
                pictureBox4.Image = bitmaps[3];
                pictureBox5.Image = bitmaps[4];
                pictureBox6.Image = bitmaps[5];
                pictureBox7.Image = bitmaps[6];
                pictureBox8.Image = bitmaps[7];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //緩存PCX所有bytes
            byte[] array;
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
                    int sum = 0, total = 0;
                    image = pcxDecoder.BuildBitmap(imgBuffer, width, height);
                    Bitmap threshold = new Bitmap(width, height);
                    grayImage = new Bitmap(width, height);
                    //MessageBox.Show(pcxDecoder.width.ToString() + " " + pcxDecoder.height.ToString());
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            Bitmap original = (Bitmap)image;
                            Color color = original.GetPixel(i, j);
                            int grayScale = (color.R + color.G + color.B) / 3;
                            Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);
                            sum += grayScale;
                            grayImage.SetPixel(i, j, newColor);
                            total++;
                        }
                    }
                    int average = (int)((double)sum / (double)total);
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            Color color = grayImage.GetPixel(i, j);
                            int scale = 0;
                            if(color.R >= average)
                            {
                                scale = 255;
                            }
                            else
                            {
                                scale = 0;
                            }
                            Color newColor = Color.FromArgb(scale, scale, scale);
                            threshold.SetPixel(i, j, newColor);
                        }
                    }
                    pictureBox1.Image = threshold;
                    bitmaps[0] = threshold;
                    pictureBox1.Size = new Size(width, height);
                    this.label2.Text = this.file_name;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string name = "";
            if(flag == 0)
            {
                name = "Bit_Stream_";
            }
            else
            {
                name = "Gray_Code_";
            }
            result = new Bitmap(pcxDecoder.width, pcxDecoder.height);
            for (int i = 0; i < pcxDecoder.width; i++)
            {
                for (int j = 0; j < pcxDecoder.height; j++)
                {
                    int gray_color = 0;
                    if (flag == 0)
                    {
                        gray_color = (bitmaps[0].GetPixel(i, j).R & 0x1);
                        gray_color += (bitmaps[1].GetPixel(i, j).R & 0x2);
                        gray_color += (bitmaps[2].GetPixel(i, j).R & 0x4);
                        gray_color += (bitmaps[3].GetPixel(i, j).R & 0x8);
                        gray_color += (bitmaps[4].GetPixel(i, j).R & 0x10);
                        gray_color += (bitmaps[5].GetPixel(i, j).R & 0x20);
                        gray_color += (bitmaps[6].GetPixel(i, j).R & 0x40);
                        gray_color += (bitmaps[7].GetPixel(i, j).R & 0x80);
                    }
                    else
                    {
                        gray_color = (bitmaps[0].GetPixel(i, j).R & 0x1);
                        gray_color ^= (bitmaps[1].GetPixel(i, j).R & 0x2);
                        gray_color ^= (bitmaps[2].GetPixel(i, j).R & 0x4);
                        gray_color ^= (bitmaps[3].GetPixel(i, j).R & 0x8);
                        gray_color ^= (bitmaps[4].GetPixel(i, j).R & 0x10);
                        gray_color ^= (bitmaps[5].GetPixel(i, j).R & 0x20);
                        gray_color ^= (bitmaps[6].GetPixel(i, j).R & 0x40);
                        gray_color ^= (bitmaps[7].GetPixel(i, j).R & 0x80);
                    }
                    result.SetPixel(i, j, Color.FromArgb(gray_color, gray_color, gray_color));
                }
            }
            Form1 form = (Form1)this.Owner;
            double[] snr_rate = form.SNR(grayImage, result);
            form.toolStripStatusLabel4.Text = "SNR : (" + snr_rate[0].ToString("#0.000") + " , "
                + snr_rate[1].ToString("#0.000") + " , "
                + snr_rate[2].ToString("#0.000") + ")";
            form.SNRs.Add(snr_rate);
            //新增分頁
            TabPage tabPage = new TabPage();
            tabPage.BackColor = Color.Gray;
            tabPage.Text = name + form.file_name + form.tab_pages_top;
            //設定圖片
            PictureBox pic = new PictureBox();
            pic.Image = result;
            pic.Size = new Size(pcxDecoder.width, pcxDecoder.height);
            pic.Location = new Point(300, 155);
            pic.MouseClick += new MouseEventHandler(form.pictureBox_MouseClick);
            pic.MouseMove += new MouseEventHandler(form.pictureBox_MouseMove);
            pic.Name = name + this.file_name + form.tab_pages_top;
            form.pictureBoxes.Add(pic);
            //加入圖片至分頁
            tabPage.Controls.Add(pic);
            form.tabControl.Controls.Add(tabPage);
            form.tab_pages_top++;
            this.Close();
        }
    }
}
