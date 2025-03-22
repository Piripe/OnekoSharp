using System;
using System.Drawing;

namespace OnekoSharp
{
    internal static class Utils
    {
        public static void DrawOneko(Graphics g, Image sprites, int sprite, Point pos, int size)
        {
            int spriteX = sprite % 8 * 32;
            int spriteY = (int)(sprite / 8d) * 32;
            g.DrawImage(
                sprites, 
                new Rectangle(pos,new Size(size,size)),
                new Rectangle(spriteX,spriteY,32,32),
            GraphicsUnit.Pixel);
        }
        public static int DistanceBetween(Point x, Point y) => (int)Math.Sqrt(Math.Pow(x.X-y.X,2)+Math.Pow(x.Y-y.Y,2));
        public static bool HasFlag(Enum value, Enum flag) => (Convert.ToInt32(value) & Convert.ToInt32(flag)) == Convert.ToInt32(flag);
    }
}
