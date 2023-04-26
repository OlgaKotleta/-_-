using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        Bitmap image;
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ///Диалог для открытия файла
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
            }
            pictureBox1.Image = image;
            pictureBox1.Refresh();
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertFilter filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
                image = newImage;


        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;



        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();

        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрГауссаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filters);


        }

        private void чернобелыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filters);


        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Brightness();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SobelFilterX();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void yToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SobelFilterY();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            
            if (saveFileDialog1.FileName != "")
            {
                
                System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog1.OpenFile();
                
                
                
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        this.pictureBox1.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case 2:
                        this.pictureBox1.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        this.pictureBox1.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Gif);
                        break;

                    case 4:
                        this.pictureBox1.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }

                fs.Close();
            }
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new CrowdingFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void xToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filters = new BorderSelectionSharraX();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void yToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Filters filters = new BorderSelectionSharraY();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void хToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new BorderSelectionPruttaX();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void уToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new BorderSelectionPruttaY();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void motionBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new MotionBlur();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void стеклоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new EffectGlass();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Filters filters = new EffectWave1();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Filters filters = new EffectWave2();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void переносToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Transfer();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void поворотToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Rotate();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new GreyworldFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new LinearCorrection();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void медианныйФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new MedianFilter(5);
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void сужениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Erosion();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Dilation();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void открытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Opening();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void закрытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Closing();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void gradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Grad();
            backgroundWorker1.RunWorkerAsync(filters);
        }
    }
    }
