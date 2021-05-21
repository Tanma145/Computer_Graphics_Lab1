using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;


namespace CG1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((double)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
            return resultImage;
        }
        public int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }
    class InventFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }
    class MatrixFilters : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilters() { }
        public MatrixFilters(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0; float resultG = 0; float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighbourColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighbourColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighbourColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighbourColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }
    class BlurFilter : MatrixFilters // Размытие
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }
    class GaussianFilter : MatrixFilters // Фильтр Гаусса
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            // определяем размер ядра
            int size = 2 * radius + 1;
            // создаём ядро фильтра
            kernel = new float[size, size];
            // коэффициент нормировки ядра
            float norm = 0;
            // расчитывает ядро линейного фильтра
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            // нормируем ядро
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }
    class GrayScaleFilter : Filters // Оттенки серого
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int Intensity = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
            return Color.FromArgb(Intensity, Intensity, Intensity);
        }
    }
    class SepiaFilter : Filters // Сепия
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int coeff = 20;
            Color sourceColor = sourceImage.GetPixel(x, y);
            int Intensity = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
            int R = Clamp(Intensity + 2 * coeff, 0, 255);
            int G = Clamp(Intensity + coeff / 2, 0, 255);
            int B = Clamp(Intensity - coeff, 0, 255);
            return Color.FromArgb(R, G, B);
        }
    }
    class BrightnessFilter : Filters // Увеличение яркости
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int constant = 30;
            Color sourceColor = sourceImage.GetPixel(x, y);
            int R = Clamp(sourceColor.R + constant, 0, 255);
            int G = Clamp(sourceColor.G + constant, 0, 255);
            int B = Clamp(sourceColor.B + constant, 0, 255);
            Color resultColor = Color.FromArgb(R, G, B);
            return resultColor;
        }
    }
    class SobelFilter : MatrixFilters // Оператор Собеля
    {
        protected int[,] X = null;
        protected int[,] Y = null;
        public SobelFilter()
        {
            X = new int[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            Y = new int[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        }
        public SobelFilter(int[,] _X, int[,] _Y)
        {
            this.X = _X; this.Y = _Y;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = 1;
            int radiusY = 1;
            float resultRX = 0; float resultGX = 0; float resultBX = 0;
            float resultRY = 0; float resultGY = 0; float resultBY = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighbourColor = sourceImage.GetPixel(idX, idY);
                    resultRX += neighbourColor.R * X[k + radiusX, l + radiusY];
                    resultGX += neighbourColor.G * X[k + radiusX, l + radiusY];
                    resultBX += neighbourColor.B * X[k + radiusX, l + radiusY];
                    resultRY += neighbourColor.R * Y[k + radiusX, l + radiusY];
                    resultGY += neighbourColor.G * Y[k + radiusX, l + radiusY];
                    resultBY += neighbourColor.B * Y[k + radiusX, l + radiusY];
                }
            int resultR = Clamp((int)Math.Sqrt(Math.Pow(resultRX, 2.0) + Math.Pow(resultRY, 2.0)), 0, 255);
            int resultG = Clamp((int)Math.Sqrt(Math.Pow(resultGX, 2.0) + Math.Pow(resultGY, 2.0)), 0, 255);
            int resultB = Clamp((int)Math.Sqrt(Math.Pow(resultBX, 2.0) + Math.Pow(resultBY, 2.0)), 0, 255);
            return Color.FromArgb(Clamp(resultR, 0, 255), Clamp(resultG, 0, 255), Clamp(resultB, 0, 255));
        }
    }
    class SharpnessFilter : MatrixFilters // Увеличение резкости
    {
        public SharpnessFilter()
        {
            kernel = new float[3, 3] { { -1, -1, -1 }, 
                                       { -1,  9, -1 }, 
                                       { -1, -1, -1 } };
        }
    }
    class GrayWorldFilter : Filters // "Серый мир"
    {
        protected int Avg;
        protected int R, G, B;
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color c = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(Clamp(c.R * Avg / R, 0, 255), Clamp(c.G * Avg / G, 0, 255), Clamp(c.B * Avg / B, 0, 255));
            return resultColor;
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            R = 0; G = 0; B = 0; Avg = 0;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);
                    R += sourceColor.R;
                    G += sourceColor.G;
                    B += sourceColor.B;
                }
            }
            R = R / (sourceImage.Width * sourceImage.Height);
            G = G / (sourceImage.Width * sourceImage.Height);
            B = B / (sourceImage.Width * sourceImage.Height);
            Avg = (R + G + B) / 3;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
            return resultImage;
        }
    }
    class AutoContrastFilter : Filters // "Серый мир"
    {
        protected int Avg;
        protected int R, G, B;
        protected int R_max, G_max, B_max;
        protected int R_min, G_min, B_min;
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color c = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb((c.R - R_min) * 255 / (R_max - R_min), (c.G - G_min) * 255 / (G_max - G_min), (c.B - B_min) * 255 / (B_max - B_min));
            return resultColor;
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            R_max =   0; G_max =   0; B_max =   0;
            R_min = 255; G_min = 255; B_min = 255;
            Color sourceColor;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    sourceColor = sourceImage.GetPixel(i, j);
                    //max
                    if (sourceColor.R > R_max)
                        R_max = sourceColor.R;
                    if (sourceColor.G > G_max)
                        G_max = sourceColor.G;
                    if (sourceColor.B > B_max)
                        B_max = sourceColor.B;
                    //min
                    if (sourceColor.R < R_min)
                        R_min = sourceColor.R;
                    if (sourceColor.G < G_min)
                        G_min = sourceColor.G;
                    if (sourceColor.B < B_min)
                        B_min = sourceColor.B;
                }
            }

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
            return resultImage;
        }
    }
    class IdealReflectorFilter : Filters // Идеальный отражатель
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(x, y);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            double Rmax = 0, Gmax = 0, Bmax = 0;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    if (sourceImage.GetPixel(i, j).R > Rmax) Rmax = sourceImage.GetPixel(i, j).R;
                    if (sourceImage.GetPixel(i, j).G > Gmax) Gmax = sourceImage.GetPixel(i, j).G;
                    if (sourceImage.GetPixel(i, j).B > Bmax) Bmax = sourceImage.GetPixel(i, j).B;
                }
            }
            Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / sourceImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    int newR = Clamp((int)(calculateNewPixelColor(sourceImage, i, j).R * 255 / Rmax), 0, 255);
                    int newG = Clamp((int)(calculateNewPixelColor(sourceImage, i, j).G * 255 / Gmax), 0, 255);
                    int newB = Clamp((int)(calculateNewPixelColor(sourceImage, i, j).B * 255 / Bmax), 0, 255);
                    result.SetPixel(i, j, Color.FromArgb(newR, newG, newB));
                }
            }
            return result;
        }
    }
    class MedianFilter : Filters //  Медианный фильтр
    {
        protected int Avg;
        protected int[] newAvg;
        protected const int size = 9;
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            newAvg = new int[size];
            int k = 0;
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    Color currColor = sourceImage.GetPixel(Clamp(x + i, 0, sourceImage.Width - 1), Clamp(y + j, 0, sourceImage.Height - 1));
                    newAvg[k] = (currColor.R + currColor.G + currColor.B) / 3;
                    k++;
                }
            Color sourceColor = sourceImage.GetPixel(x, y);
            Avg = qsort(newAvg, 0, size - 1);
            Color resultColor = Color.FromArgb(Clamp(Avg, 0, 255), Clamp(Avg, 0, 255), Clamp(Avg, 0, 255));
            return resultColor;
        }
        protected int qsort(int[] a, int l, int r)
        {
            int x = a[l + (r - l) / 2], i = l, j = r, temp;
            while (i <= j)
            {
                while (a[i] < x) i++;
                while (a[j] > x) j--;
                if (i <= j)
                {
                    temp = a[i]; a[i] = a[j]; a[j] = temp;
                    i++;
                    j--;
                }
            }
            if (i < r) qsort(a, i, r);
            if (l < j) qsort(a, l, j);
            return a[l + (r - l) / 2];
        }
    }
    class Shift : Filters
    {
        double x0, y0;
        public Shift(double x00, double y00)
        {
            x0 = x00; y0 = y00;
        }
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            int nX = (int)(W + x0);
            int nY = (int)(H + y0);
            if ((nY < 0 || nY > Source.Height - 1) || (nX < 0 || nX > Source.Width - 1))
            {
                sourceColor = Color.Black;
                return sourceColor;
            }
            nX = Clamp(nX, 0, Source.Width - 1);
            nY = Clamp(nY, 0, Source.Height - 1);

            return Source.GetPixel(nX, nY);

        }
    };
    class WavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            int nX = (int) Math.Round(W + 20 *  Math.Sin(2 * Math.PI * H / 60.0));
            if (nX < 0)
                nX = Source.Width + nX;
            if (nX >= Source.Width)
                nX = -Source.Width + nX;

            int nY = H;
            return Source.GetPixel(nX, nY);
        }
    };
    class GlassFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Random rand = new Random(W * H);

            int nX = (int) (W + (rand.NextDouble() - 0.5) * 10);
            if (nX < 0)
                nX = Source.Width + nX;
            if (nX >= Source.Width)
                nX = -Source.Width + nX;

            int nY = (int)(H + (rand.NextDouble() - 0.5) * 10);
            if (nY < 0)
                nY = Source.Height + nY;
            if (nY >= Source.Height)
                nY = -Source.Height + nY;
            return Source.GetPixel(nX, nY);
        }
    };
    class turning : Filters
    {
        double x0, y0, t;
        public turning(double x00, double y00, double t_)
        {
            x0 = x00; y0 = y00; t = t_;
        }
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            x0 = Source.Height / 2;
            y0 = Source.Width / 2;
            int nX = (int)((W - x0) * Math.Acos(t) - (H - y0) * Math.Asin(t) + x0);
            int nY = (int)((W - x0) * Math.Asin(t) + (H - y0) * Math.Acos(t) + y0);
            if ((nY < 0 || nY > Source.Height - 1) || (nX < 0 || nX > Source.Width - 1))
            {
                sourceColor = Color.Black;
                return sourceColor;
            }

            nX = Clamp(nX, 0, Source.Width - 1);
            nY = Clamp(nY, 0, Source.Height - 1);
            return Source.GetPixel(nX, nY);
        }
    };
    class toBin : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            if (sourceColor.R < 127 && sourceColor.G < 127 && sourceColor.R < 127)

                return Color.Black;
            else return Color.White;
        }
    };
}
