
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CG1
{
    public partial class Computer_Graphics_Lab1 : Form
    {
        Bitmap image; //Объект Bitmap
        Bitmap image_prev; //Объект Bitmap
        public Computer_Graphics_Lab1()
        {
            InitializeComponent();
        }
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog(); //Диалог для открытия файла
            dialog.Filter = "Image files|*.png;*.jpg;*.bmp|All files(*.*)|*.*"; //Файлы-изображения
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName); // Проверка на выбор файла
            }
            pictureBox1.Image = image; // Визуализация изображения на форме
            pictureBox1.Refresh();
        }
        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InventFilter filter = new InventFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending == false) //
            {
                image_prev = image;
                image = newImage;
            }
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

        private void button1_Click(object sender, EventArgs e)
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
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void grayScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void увеличениеЯркостиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BrightnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void операторСобеляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void увеличениеРезкостиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayWorldFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }
        private void идеальныйОтражательToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Filters filter = new IdealReflectorFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }
        private void медианныйФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }
        private void матричныеToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void сдвигToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
            Filters filter = new Shift(100, 0);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поворотToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new turning(50,50 , 0.4);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void размытиеToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //Filters filter = new wave();
            //backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Cохранить картинку как..";
                sfd.OverwritePrompt = true;
                sfd.CheckPathExists = true;
                sfd.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All Files(*.*)|*.*";
                sfd.ShowHelp = true;
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox1.Image.Save(sfd.FileName);             
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void binaryTranslationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters Bin =  new toBin();
            backgroundWorker1.RunWorkerAsync(Bin);
        }

        private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] structuring_element = new float[3, 3];

            structuring_element[0, 0] = (float) Convert.ToDouble(textBox1.Text);
            structuring_element[0, 1] = (float) Convert.ToDouble(textBox2.Text);
            structuring_element[0, 2] = (float) Convert.ToDouble(textBox3.Text);
            structuring_element[1, 0] = (float) Convert.ToDouble(textBox4.Text);
            structuring_element[1, 1] = (float) Convert.ToDouble(textBox5.Text);
            structuring_element[1, 2] = (float) Convert.ToDouble(textBox6.Text);
            structuring_element[2, 0] = (float) Convert.ToDouble(textBox7.Text);
            structuring_element[2, 1] = (float) Convert.ToDouble(textBox8.Text);
            structuring_element[2, 2] = (float) Convert.ToDouble(textBox9.Text);

            Morphology dil = new Dilation(structuring_element);
            backgroundWorker1.RunWorkerAsync(dil);
        }

        private void erosinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] structuring_element = new float[3, 3];

            structuring_element[0, 0] = (float)Convert.ToDouble(textBox1.Text);
            structuring_element[0, 1] = (float)Convert.ToDouble(textBox2.Text);
            structuring_element[0, 2] = (float)Convert.ToDouble(textBox3.Text);
            structuring_element[1, 0] = (float)Convert.ToDouble(textBox4.Text);
            structuring_element[1, 1] = (float)Convert.ToDouble(textBox5.Text);
            structuring_element[1, 2] = (float)Convert.ToDouble(textBox6.Text);
            structuring_element[2, 0] = (float)Convert.ToDouble(textBox7.Text);
            structuring_element[2, 1] = (float)Convert.ToDouble(textBox8.Text);
            structuring_element[2, 2] = (float)Convert.ToDouble(textBox9.Text);
            Morphology eros = new Erosion(structuring_element);
            backgroundWorker1.RunWorkerAsync(eros);
        }

        private void openingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] structuring_element = new float[3, 3];

            structuring_element[0, 0] = (float)Convert.ToDouble(textBox1.Text);
            structuring_element[0, 1] = (float)Convert.ToDouble(textBox2.Text);
            structuring_element[0, 2] = (float)Convert.ToDouble(textBox3.Text);
            structuring_element[1, 0] = (float)Convert.ToDouble(textBox4.Text);
            structuring_element[1, 1] = (float)Convert.ToDouble(textBox5.Text);
            structuring_element[1, 2] = (float)Convert.ToDouble(textBox6.Text);
            structuring_element[2, 0] = (float)Convert.ToDouble(textBox7.Text);
            structuring_element[2, 1] = (float)Convert.ToDouble(textBox8.Text);
            structuring_element[2, 2] = (float)Convert.ToDouble(textBox9.Text);
            Morphology open = new Opening(structuring_element);
            backgroundWorker1.RunWorkerAsync(open);
        }

        private void closingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] structuring_element = new float[3, 3];

            structuring_element[0, 0] = (float)Convert.ToDouble(textBox1.Text);
            structuring_element[0, 1] = (float)Convert.ToDouble(textBox2.Text);
            structuring_element[0, 2] = (float)Convert.ToDouble(textBox3.Text);
            structuring_element[1, 0] = (float)Convert.ToDouble(textBox4.Text);
            structuring_element[1, 1] = (float)Convert.ToDouble(textBox5.Text);
            structuring_element[1, 2] = (float)Convert.ToDouble(textBox6.Text);
            structuring_element[2, 0] = (float)Convert.ToDouble(textBox7.Text);
            structuring_element[2, 1] = (float)Convert.ToDouble(textBox8.Text);
            structuring_element[2, 2] = (float)Convert.ToDouble(textBox9.Text);
            Morphology close = new Closing(structuring_element);
            backgroundWorker1.RunWorkerAsync(close);

        }

        private void gradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] structuring_element = new float[3, 3];

            structuring_element[0, 0] = (float)Convert.ToDouble(textBox1.Text);
            structuring_element[0, 1] = (float)Convert.ToDouble(textBox2.Text);
            structuring_element[0, 2] = (float)Convert.ToDouble(textBox3.Text);
            structuring_element[1, 0] = (float)Convert.ToDouble(textBox4.Text);
            structuring_element[1, 1] = (float)Convert.ToDouble(textBox5.Text);
            structuring_element[1, 2] = (float)Convert.ToDouble(textBox6.Text);
            structuring_element[2, 0] = (float)Convert.ToDouble(textBox7.Text);
            structuring_element[2, 1] = (float)Convert.ToDouble(textBox8.Text);
            structuring_element[2, 2] = (float)Convert.ToDouble(textBox9.Text);
            Morphology grad = new Grad(structuring_element);
            backgroundWorker1.RunWorkerAsync(grad);
        }

        private void topHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] structuring_element = new float[3, 3];

            structuring_element[0, 0] = (float)Convert.ToDouble(textBox1.Text);
            structuring_element[0, 1] = (float)Convert.ToDouble(textBox2.Text);
            structuring_element[0, 2] = (float)Convert.ToDouble(textBox3.Text);
            structuring_element[1, 0] = (float)Convert.ToDouble(textBox4.Text);
            structuring_element[1, 1] = (float)Convert.ToDouble(textBox5.Text);
            structuring_element[1, 2] = (float)Convert.ToDouble(textBox6.Text);
            structuring_element[2, 0] = (float)Convert.ToDouble(textBox7.Text);
            structuring_element[2, 1] = (float)Convert.ToDouble(textBox8.Text);
            structuring_element[2, 2] = (float)Convert.ToDouble(textBox9.Text);
            Morphology hat = new TopHat(structuring_element);
            backgroundWorker1.RunWorkerAsync(hat);
        }

        private void blackHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float[,] structuring_element = new float[3, 3];

            structuring_element[0, 0] = (float)Convert.ToDouble(textBox1.Text);
            structuring_element[0, 1] = (float)Convert.ToDouble(textBox2.Text);
            structuring_element[0, 2] = (float)Convert.ToDouble(textBox3.Text);
            structuring_element[1, 0] = (float)Convert.ToDouble(textBox4.Text);
            structuring_element[1, 1] = (float)Convert.ToDouble(textBox5.Text);
            structuring_element[1, 2] = (float)Convert.ToDouble(textBox6.Text);
            structuring_element[2, 0] = (float)Convert.ToDouble(textBox7.Text);
            structuring_element[2, 1] = (float)Convert.ToDouble(textBox8.Text);
            structuring_element[2, 2] = (float)Convert.ToDouble(textBox9.Text);
            Morphology bhat = new BlackHat(structuring_element);
            backgroundWorker1.RunWorkerAsync(bhat);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Filters sharp = new Sharpness();
            //backgroundWorker1.RunWorkerAsync(sharp);

        }

        private void binaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters Bin = new toBin();
            backgroundWorker1.RunWorkerAsync(Bin);
        }

        private void назадToolStripMenuItem_Click(object sender, EventArgs e)
        {
            image = image_prev;
            pictureBox1.Image = image;
            pictureBox1.Refresh();
        }

        private void волныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters Wave = new WavesFilter();
            backgroundWorker1.RunWorkerAsync(Wave);
        }

        private void эффектСтеклаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters Glass = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(Glass);
        }

        private void линейноеРастяжениеГгистограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters autoContrast = new AutoContrastFilter();
            backgroundWorker1.RunWorkerAsync(autoContrast);
        }
    }
}
