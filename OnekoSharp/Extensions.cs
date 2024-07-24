using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnekoSharp
{
    internal static class Extensions
    {
        public static void DrawOneko(this Graphics g, OnekoSprite sprite, Point pos, int size)
        {
            int spriteX = ((int)sprite % 8) * 32;
            int spriteY = (int)((int)sprite / 8d) * 32;
            g.DrawImage(
                Properties.Resources.oneko, 
                new Rectangle(pos,new Size(size,size)),
                new Rectangle(spriteX,spriteY,32,32),
            GraphicsUnit.Pixel);
        }
        public static int DistanceBetween(this Point x, Point y) => (int)Math.Sqrt(Math.Pow(x.X-y.X,2)+Math.Pow(x.Y-y.Y,2));
    }
}
