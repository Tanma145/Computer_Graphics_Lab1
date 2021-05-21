using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace CG1
{
    class Morphology : Filters
    {
        protected float[,] st_elem;
        protected bool Dil;
        protected Morphology() //элемент - квадрат из единиц 
        {
            st_elem = new float[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        public Morphology(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
        }

        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            int radX = st_elem.GetLength(0) / 2;//наш структурныый элемент 3х3, радиус по одной клетке
            int radY = st_elem.GetLength(1) / 2;
            int BITmaxR = 0; int BITmaxG = 0; int BITmaxB = 0; // черный
            int BITminR = 255; int BITminG = 255; int BITminB = 255;//белый
            for (int j = -radY; j <= radY; j++)
                for (int i = -radX; i <= radX; i++)
                {
                    if (Dil) //если расширение
                    {
                        //дополняем все 1 в структурном элементе 
                        if ((st_elem[i + radX, j + radY] == 1) && (Source.GetPixel(W + i, H + j).R > BITmaxR))
                            BITmaxR = Source.GetPixel(W + i, H + j).R;
                        if ((st_elem[i + radX, j + radY] == 1) && (Source.GetPixel(W + i, H + j).G > BITmaxG))
                            BITmaxG = Source.GetPixel(W + i, H + j).G;
                        if ((st_elem[i + radX, j + radY] == 1) && (Source.GetPixel(W + i, H + j).B > BITmaxB))
                            BITmaxB = Source.GetPixel(W + i, H + j).B;

                    }
                    else //сужение
                    {
                        if ((st_elem[i + radX, j + radY] == 1) && (Source.GetPixel(W + i, H + j).R < BITminR))

                            BITminR = Source.GetPixel(W + i, H + j).R;
                        if ((st_elem[i + radX, j + radY] == 1) && (Source.GetPixel(W + i, H + j).G < BITminG))

                            BITminG = Source.GetPixel(W + i, H + j).G;
                        if ((st_elem[i + radX, j + radY] == 1) && (Source.GetPixel(W + i, H + j).B < BITminB))

                            BITminB = Source.GetPixel(W + i, H + j).B;
                    }
                }
            if (Dil)
                return Color.FromArgb(BITmaxR, BITmaxG, BITmaxB);
            else return Color.FromArgb(BITminR, BITminG, BITminB);

        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            int radX = st_elem.GetLength(0) / 2;
            int radY = st_elem.GetLength(1) / 2;
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = radX; i < sourceImage.Width - radX; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                //прерывание процесса 
                if (worker.CancellationPending)
                    return null;
                for (int j = radY; j < sourceImage.Height - radY; j++) // применяем эффект сверху вниз, потом в ширину 
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
            return resultImage;
        }
    };
    class Dilation : Morphology //  расширение
    {
        public Dilation()
        {
            Dil = true;
        }
        public Dilation(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
            Dil = true;
        }
    }
    class Erosion : Morphology //  сужение
    {
        public Erosion()
        {
            Dil = false;
        }
        public Erosion(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
            Dil = false;
        }
    }
    class Opening : Morphology // Морфологическое открытие
    {
        public Opening(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            //сначала сужение, потом расширение
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Err = new Erosion(st_elem);
            Filters Dila = new Dilation(st_elem);
            Bitmap result = Err.processImage(sourceImage, worker);
            result = Dila.processImage(result, worker);//применили два фильтра 
            return result;
        }
    }
    class Closing : Morphology // Морфологическое открытие
    {
        public Closing(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            //сначала расширение, потом сужение
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Err = new Erosion(st_elem);
            Filters Dila = new Dilation(st_elem);
            Bitmap result = Dila.processImage(sourceImage, worker);//применили два фильтра 
            result = Err.processImage(result, worker);

            return result;
        }
    }
    class Grad : Morphology
    {
        public Grad(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Err = new Erosion(st_elem);
            Bitmap resultE = Err.processImage(sourceImage, worker);

            Filters Dila = new Dilation(st_elem);
            Bitmap resultD = Dila.processImage(sourceImage, worker);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                //прерывание процесса 
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    // изображение с расширением (-) изобржение с эрозией, выделяет границы всего, что есть на изображении
                    // с учетом цвета, потому что при чб некоторые границы пропадают 
                    int newR = Clamp(resultD.GetPixel(i, j).R - resultE.GetPixel(i, j).R, 0, 255);
                    int newG = Clamp(resultD.GetPixel(i, j).G - resultE.GetPixel(i, j).G, 0, 255);
                    int newB = Clamp(resultD.GetPixel(i, j).B - resultE.GetPixel(i, j).B, 0, 255);
                    resultImage.SetPixel(i, j, Color.FromArgb(newR, newG, newB));
                }
            }
            return resultImage;

        }
    }

    class TopHat : Morphology
    {
        public TopHat(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Clos = new Closing(st_elem);
            Bitmap resultcL = Clos.processImage(sourceImage, worker);


            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                //прерывание процесса 
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {

                    int newR = Clamp(sourceImage.GetPixel(i, j).R - resultcL.GetPixel(i, j).R, 0, 255);
                    int newG = Clamp(sourceImage.GetPixel(i, j).G - resultcL.GetPixel(i, j).G, 0, 255);
                    int newB = Clamp(sourceImage.GetPixel(i, j).B - resultcL.GetPixel(i, j).B, 0, 255);
                    resultImage.SetPixel(i, j, Color.FromArgb(newR, newG, newB));
                }
            }
            return resultImage;

        }
    }

    class BlackHat : Morphology
    {
        public BlackHat(float[,] st_elem_)
        {
            this.st_elem = st_elem_;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Op = new Opening(st_elem);
            Bitmap result = Op.processImage(sourceImage, worker);


            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                //прерывание процесса 
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {

                    int newR = Clamp(result.GetPixel(i, j).R - sourceImage.GetPixel(i, j).R, 0, 255);
                    int newG = Clamp(result.GetPixel(i, j).G - sourceImage.GetPixel(i, j).G, 0, 255);
                    int newB = Clamp(result.GetPixel(i, j).B - sourceImage.GetPixel(i, j).B, 0, 255);
                    resultImage.SetPixel(i, j, Color.FromArgb(newR, newG, newB));
                }
            }
            return resultImage;

        }
    }
}
