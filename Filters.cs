using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace WinFormsApp2
{

    abstract class Filters
    {
       

        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x,int  y);
        public Bitmap processImage(Bitmap sourceImage)
        {
           
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i=0;i< sourceImage.Width; i++)
            {
                for (int j = 0; i < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage,i+1, j+1));
                }
            }
            return resultImage;
            
        }
        public int Clamp(int Value, int min, int max)
        {
            if (Value < min)
                return min;
            if (Value > max)
                return max;
            return Value;
        }
    }
     class InvertFilter : Filters
    {
        

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor=sourceImage.GetPixel(x, y+1);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }

    }
}
