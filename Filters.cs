using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WindowsFormsApp2;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Runtime.Serialization.Formatters;

namespace WindowsFormsApp2
{


    abstract class Filters
    {
        protected virtual Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(x, y);
        }
        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending) return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                                               255 - sourceColor.G,
                                               255 - sourceColor.B);

            return resultColor;


        }


    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel) { this.kernel = kernel; }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }

            return Color.FromArgb(Clamp((int)resultR, 0, 255),
                                   Clamp((int)resultG, 0, 255),
                                   Clamp((int)resultB, 0, 255));
        }
    }

    class BlurFilter : MatrixFilter//размытие
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

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            //определние размера ядра
            int size = 2 * radius + 1;
            //создаем ядро фильтра
            kernel = new float[size, size];
            //коэффициент нормировки ядра
            float norm = 0;
            //расчитываем ядро линейного фильтра
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            //нормируем ядро
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class GrayScaleFilter : Filters//ч-б
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int Intensity = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(Intensity, Intensity, Intensity);

            return resultColor;
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double Intensity = 0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B;
            double k = 20;
            double R = Intensity + 2 * k;
            double G = Intensity + 0.5 * k;
            double B = Intensity - 1 * k;

            return Color.FromArgb(Clamp((int)R, 0, 255),
                                    Clamp((int)G, 0, 255),
                                    Clamp((int)B, 0, 255));
        }
    }

    class Brightness : Filters//яркость
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 20;
            return Color.FromArgb(Clamp(sourceColor.R + k, 0, 255),
                                    Clamp(sourceColor.G + k, 0, 255),
                                    Clamp(sourceColor.B + k, 0, 255));
        }
    }


    class SobelFilterY : MatrixFilter
    {
        public SobelFilterY()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1; kernel[0, 1] = -2; kernel[0, 2] = -1;
            kernel[1, 0] = 0; kernel[1, 1] = 0; kernel[1, 2] = 0;
            kernel[2, 0] = 1; kernel[2, 1] = 2; kernel[2, 2] = 1;
        }
    }

    class SobelFilterX : MatrixFilter
    {
        public SobelFilterX()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1; kernel[0, 1] = 0; kernel[0, 2] = 1;
            kernel[1, 0] = -2; kernel[1, 1] = 0; kernel[1, 2] = 2;
            kernel[2, 0] = -1; kernel[2, 1] = 0; kernel[2, 2] = 1;
        }
    }

    class SharpnessFilter : MatrixFilter//резкость
    {
        public SharpnessFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1; kernel[0, 1] = -1; kernel[0, 2] = -1;
            kernel[1, 0] = -1; kernel[1, 1] = 9; kernel[1, 2] = -1;
            kernel[2, 0] = -1; kernel[2, 1] = -1; kernel[2, 2] = -1;
        }
    }



    class CrowdingFilter : MatrixFilter//тиснение
    {
        public CrowdingFilter()
        {
            int size = 3;
            kernel = new float[size, size];
            int radius = 1;
            float norm = 0;

            kernel[0, 0] = 0; kernel[0, 1] = 1; kernel[0, 2] = 0;
            kernel[1, 0] = 1; kernel[1, 1] = 0; kernel[1, 2] = -1;
            kernel[2, 0] = 0; kernel[2, 1] = -1; kernel[2, 2] = 0;

            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                    norm += kernel[i + radius, j + radius];


            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
    }



    class BorderSelectionSharraY : MatrixFilter//шарра
    {

        public BorderSelectionSharraY()
        {
            int size = 3;
            kernel = new float[size, size];
            kernel[0, 0] = 3; kernel[0, 1] = 10; kernel[0, 2] = 3;
            kernel[1, 0] = 0; kernel[1, 1] = 0; kernel[1, 2] = 0;
            kernel[2, 0] = -3; kernel[2, 1] = -10; kernel[2, 2] = -3;
        }
    }



    class BorderSelectionSharraX : MatrixFilter
    {
        public BorderSelectionSharraX()
        {
            int size = 3;
            kernel = new float[size, size];
            kernel[0, 0] = 3; kernel[0, 1] = 0; kernel[0, 2] = -3;
            kernel[1, 0] = 10; kernel[1, 1] = 0; kernel[1, 2] = -10;
            kernel[2, 0] = 3; kernel[2, 1] = 0; kernel[2, 2] = -3;
        }
    }

    class BorderSelectionPruttaX : MatrixFilter//прюитта
    {
        public BorderSelectionPruttaX()
        {
            int size = 3;
            kernel = new float[size, size];
            kernel[0, 0] = -1; kernel[0, 1] = 0; kernel[0, 2] = 1;
            kernel[1, 0] = -1; kernel[1, 1] = 0; kernel[1, 2] = 1;
            kernel[2, 0] = -1; kernel[2, 1] = 0; kernel[2, 2] = 1;
        }
    }

    class BorderSelectionPruttaY : MatrixFilter
    {
        public BorderSelectionPruttaY()
        {
            int size = 3;
            kernel = new float[size, size];
            kernel[0, 0] = -1; kernel[0, 1] = -1; kernel[0, 2] = -1;
            kernel[1, 0] = 0; kernel[1, 1] = 0; kernel[1, 2] = 0;
            kernel[2, 0] = 1; kernel[2, 1] = 1; kernel[2, 2] = 1;
        }
    }


    class MotionBlur : MatrixFilter//motion blur
    {
        public MotionBlur()
        {
            int size = 9;

            kernel = new float[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    if (i != j) kernel[i, j] = 0;
                    else kernel[i, j] = (float)1 / size;
                }
        }
    }



    class EffectGlass : Filters//стекло
    {
        Random rand = new Random();
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            x = (int)(x + ((float)rand.NextDouble() - 0.5) * 10);
            int idX = Clamp(x, 0, sourceImage.Width - 1);
            y = (int)(y + ((float)rand.NextDouble() - 0.5) * 10);
            int idY = Clamp(y, 0, sourceImage.Height - 1);
            Color sourceColor = sourceImage.GetPixel(idX, idY);

            return Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        }
    }

    class EffectWave1 : Filters//волна 1
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int idX = Clamp(x + (int)(20 * Math.Sin(2 * y * (float)(Math.PI / 60))), 0, sourceImage.Width - 1);
            int idY = Clamp(y, 0, sourceImage.Height - 1);
            Color sourceColor = sourceImage.GetPixel(idX, idY);

            return Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        }
    }

    class EffectWave2 : Filters//волна 2
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int idX = Clamp(x + (int)(20 * Math.Sin(2 * x * (float)(Math.PI / 30))), 0, sourceImage.Width - 1);
            int idY = Clamp(y, 0, sourceImage.Height - 1);
            Color sourceColor = sourceImage.GetPixel(idX, idY);

            return Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        }
    }


    class Transfer : Filters//перенос
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int xnew = x + 50;
            int idX = Clamp(xnew, 0, sourceImage.Width - 1);

            int ynew = y;
            int idY = Clamp(ynew, 0, sourceImage.Height - 1);
            Color sourceColor = sourceImage.GetPixel(idX, idY);

            //int avg = (sourceColor.R + sourceColor.G + sourceColor.B)/3;

            if (xnew < 0 || xnew > (sourceImage.Width - 1) || ynew < 0 || ynew > (sourceImage.Height - 1)) return Color.FromArgb(255, 255, 255);
            //if (avg < 126) return Color.FromArgb(0, 0, 0);
            //else return Color.FromArgb(255, 255, 255);
            else return Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        }
    }


    class Rotate : Filters//поворот
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2;

            int xnew = (int)((x - x0) * Math.Cos((float)Math.PI / 4) - (y - y0) * Math.Sin((float)Math.PI / 4) + x0);
            int idX = Clamp(xnew, 0, sourceImage.Width - 1);

            int ynew = (int)((x - x0) * Math.Sin((float)Math.PI / 4) + (y - y0) * Math.Cos((float)Math.PI / 4) + y0);
            int idY = Clamp(ynew, 0, sourceImage.Height - 1);
            Color sourceColor = sourceImage.GetPixel(idX, idY);

            //int avg = (sourceColor.R + sourceColor.G + sourceColor.B) / 3;

            if (xnew < 0 || xnew > (sourceImage.Width - 1) || ynew < 0 || ynew > (sourceImage.Height - 1)) return Color.FromArgb(255, 255, 255);
            //if (avg < 126) return Color.FromArgb(0, 0, 0);
            //else return Color.FromArgb(255, 255, 255);
            else return Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
        }
    }

    class Morfology : Filters
    {
        protected int MW, MH;
        protected int[,] mask = null;
        protected int del = 1, plus = 0;

        protected void SetMask(int MW, int MH, int[,] mask)
        {
            this.MW = MW;
            this.MH = MH;
            this.mask = mask;
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            SetMask(3, 3, new int[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } });

            for (int i = MW / 2; i < sourceImage.Width - MW / 2; i++)
            {
                worker.ReportProgress(plus + (int)((float)i / resultImage.Width * 100 / del));
                if (worker.CancellationPending) return null;
                for (int j = MH; j < sourceImage.Height - MH / 2; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;

        }
    }

    class Dilation : Morfology//расширение
    {
        public Dilation(int del = 1, int plus = 0)
        {
            this.del = del;
            this.plus = plus;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double avg;
            double avgRes = 0;
            Color sourceColor;
            Color resultColor = Color.FromArgb(0, 0, 0);

            for (int i = -MW / 2; i <= MW / 2; i++)
            {
                for (int j = -MH / 2; j <= MH / 2; j++)
                {
                    sourceColor = sourceImage.GetPixel(i + x, j + y);
                    avg = (sourceColor.R + sourceColor.B + sourceColor.G) / 3;

                    if (mask[i + MW / 2, j + MH / 2] * avg > avgRes)
                        resultColor = sourceColor;

                    avgRes = (resultColor.R + resultColor.G + resultColor.B) / 3;
                }
            }

            return resultColor;
        }
    }




    class Erosion : Morfology//сужение
    {
        public Erosion(int del = 1, int plus = 0)
        {
            this.del = del;
            this.plus = plus;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double avg;
            Color sourceColor;
            Color resultColor = Color.FromArgb(0, 0, 0);
            double avgRes = 255;

            for (int i = -MW / 2; i <= MW / 2; i++)
            {
                for (int j = -MH / 2; j <= MH / 2; j++)
                {
                    sourceColor = sourceImage.GetPixel(i + x, j + y);
                    avg = (sourceColor.R + sourceColor.B + sourceColor.G) / 3;

                    if (mask[i + MW / 2, j + MH / 2] * avg < avgRes)
                        resultColor = sourceColor;

                    avgRes = (resultColor.R + resultColor.G + resultColor.B) / 3;
                }
            }

            return resultColor;
        }
    }


    class Opening : Morfology//открытие
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Filters filters1 = new Dilation(2, 50);
            Filters filters2 = new Erosion(2);

            return filters1.processImage(filters2.processImage(sourceImage, worker), worker);
        }
    }



    class Closing : Morfology//закрытие
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Filters filters1 = new Dilation(2);
            Filters filters2 = new Erosion(2, 50);

            return filters2.processImage(filters1.processImage(sourceImage, worker), worker);
        }
    }

    class Grad : Morfology//grad
    {
        protected Bitmap dilImage, erImage;
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Filters filterD = new Dilation(3);
            Filters filterE = new Erosion(3, 33);
            dilImage = filterD.processImage(sourceImage, worker);
            erImage = filterE.processImage(sourceImage, worker);

            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress(67 + (int)((float)i / resultImage.Width * 33));
                if (worker.CancellationPending) return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(i, j));
                }
            }
            return resultImage;
        }

        protected Color calculateNewPixelColor(int x, int y)
        {

            Color dilColor = dilImage.GetPixel(x, y);
            Color erColor = erImage.GetPixel(x, y);

            int R = dilColor.R - erColor.R;
            int G = dilColor.G - erColor.G;
            int B = dilColor.B - erColor.B;

            return Color.FromArgb(Clamp(R, 0, 255),
                                    Clamp(G, 0, 255),
                                    Clamp(B, 0, 255));
        }
 
    
    
    
    }
}

class GreyworldFilter : Filters//серый мир
{
    protected int avgColorR, avgColorG, avgColorB, avg;

    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
    {
        Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
        int N = sourceImage.Width * sourceImage.Height;
        int sumR = 0;
        int sumG = 0;
        int sumB = 0;

        for (int i = 0; i < sourceImage.Width; i++)
        {
            worker.ReportProgress((int)((float)i / resultImage.Width * 50));
            if (worker.CancellationPending) return null;
            for (int j = 0; j < sourceImage.Height; j++)
            {
                Color sColor = sourceImage.GetPixel(i, j);

                sumR += sColor.R;
                sumG += sColor.G;
                sumB += sColor.B;
            }
        }

        avgColorR = sumR / N;
        avgColorG = sumG / N;
        avgColorB = sumB / N;

        avg = (avgColorR + avgColorG + avgColorB) / 3;


        for (int i = 0; i < sourceImage.Width; i++)
        {
            worker.ReportProgress(50 + (int)((float)i / resultImage.Width * 50));
            if (worker.CancellationPending) return null;
            for (int j = 0; j < sourceImage.Height; j++)
            {
                resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
        }

        return resultImage;
    }

    protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {

        Color sourceColor = sourceImage.GetPixel(x, y);

        int R = sourceColor.R * avg / avgColorR;
        int G = sourceColor.G * avg / avgColorG;
        int B = sourceColor.B * avg / avgColorB;

        return Color.FromArgb(Clamp(R, 0, 255),
                                Clamp(G, 0, 255),
                                Clamp(B, 0, 255));
    }
}

class LinearCorrection : Filters//лин растяжение
{
    protected int YmaxR, YmaxG, YmaxB,
                   YminR, YminG, YminB;
    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
    {
        Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
        Color zeroColor = sourceImage.GetPixel(0, 0);

        YminR = zeroColor.R;
        YmaxR = zeroColor.R;
        YmaxG = zeroColor.G;
        YminG = zeroColor.G;
        YmaxB = zeroColor.B;
        YminB = zeroColor.B;

        for (int i = 0; i < sourceImage.Width; i++)
        {
            worker.ReportProgress((int)((float)i / resultImage.Width * 50));
            if (worker.CancellationPending) return null;
            for (int j = 0; j < sourceImage.Height; j++)
            {
                Color sColor = sourceImage.GetPixel(i, j);

                if (YminR > sColor.R) YminR = sColor.R;
                if (YmaxR < sColor.R) YmaxR = sColor.R;
                if (YminG > sColor.G) YminG = sColor.G;
                if (YmaxG < sColor.G) YmaxG = sColor.G;
                if (YminB > sColor.B) YminB = sColor.B;
                if (YmaxB < sColor.B) YmaxB = sColor.B;
            }
        }

        for (int i = 0; i < sourceImage.Width; i++)
        {
            worker.ReportProgress(50 + (int)((float)i / resultImage.Width * 50));
            if (worker.CancellationPending) return null;
            for (int j = 0; j < sourceImage.Height; j++)
            {
                resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
        }
        return resultImage;
    }

    protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {

        Color sourceColor = sourceImage.GetPixel(x, y);

        int R = (sourceColor.R - YminR) * (255 / (YmaxR - YminR));
        int G = (sourceColor.G - YminG) * (255 / (YmaxG - YminG));
        int B = (sourceColor.B - YminB) * (255 / (YmaxB - YminB));

        return Color.FromArgb(Clamp(R, 0, 255),
                                Clamp(G, 0, 255),
                                Clamp(B, 0, 255));
    }
}

class MedianFilter : Filters//медианный фильтр
{
    private int neighborhoodSize; // размер окрестности 

    public MedianFilter(int neighborhoodSize)
    {
        this.neighborhoodSize = neighborhoodSize;
    }

    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
    {
        int height = sourceImage.Height;
        int width = sourceImage.Width;
        Bitmap resultImage = new Bitmap(width, height);
        int offset = (int)(neighborhoodSize / 2);

        for (int i = 0; i < width; i++)
        {
            worker.ReportProgress((int)((float)i / width * 100));
            if (worker.CancellationPending) return null;

            for (int j = 0; j < height; j++)
            {
                if (i < offset || j < offset || j >= height - offset || i >= width - offset) resultImage.SetPixel(i, j, sourceImage.GetPixel(i, j));
                else resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
        }
        return resultImage;
    }

    protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int[] windowR = new int[neighborhoodSize * neighborhoodSize];
        int[] windowG = new int[neighborhoodSize * neighborhoodSize];
        int[] windowB = new int[neighborhoodSize * neighborhoodSize];
        int count = 0;
        int offset = (int)(neighborhoodSize / 2);
        for (int k = x - offset; k <= x + offset; k++)
        {
            for (int l = y - offset; l <= y + offset; l++)
            {
                Color color = sourceImage.GetPixel(k, l);
                windowR[count] = color.R;
                windowG[count] = color.G;
                windowB[count] = color.B;
                count++;
            }
        }
        Array.Sort(windowR);
        Array.Sort(windowG);
        Array.Sort(windowB);
        int r = windowR[count / 2];
        int g = windowG[count / 2];
        int b = windowB[count / 2];

        return Color.FromArgb(r, g, b);
    }
}
