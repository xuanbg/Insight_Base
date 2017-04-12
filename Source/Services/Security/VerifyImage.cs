using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

namespace Insight.Base.Services
{
    public class VerifyImage
    {
        // 背景图编号
        private readonly int Number = Params.Random.Next(0, 9);

        // 碎片编号
        private readonly int Index = Params.Random.Next(0, 8);

        // 背景图
        private readonly Image Background;

        // 遮罩图
        private readonly Image Mask;

        /// <summary>
        /// 构造方法
        /// </summary>
        public VerifyImage()
        {
            using (var context = new BaseEntities())
            {
                var name = $"/image/bg{Number}.jpg";
                var bg = context.SYS_VerifyImage.SingleOrDefault(p => p.Type == 1 && p.Name == name);
                if (bg == null) return;

                name = $"/image/mp{Index}.png";
                var mp = context.SYS_VerifyImage.SingleOrDefault(p => p.Type == 0 && p.Name == name);
                if (mp == null) return;

                Background = Util.GetImage(bg.Path);
                Mask = Util.GetImage(mp.Path);
            }
        }

        /// <summary>
        /// 初始化拼图
        /// </summary>
        private byte[] GetPuzzle()
        {
            var width = Background.Width;
            var height = Background.Height;

            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(Background.HorizontalResolution, Background.VerticalResolution);
            bitmap.MakeTransparent();

            var grPhoto = Graphics.FromImage(bitmap);
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
            grPhoto.DrawImage(Background, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
            grPhoto.DrawImage(Mask, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);

            return Util.ImageToByteArray(bitmap);
        }
    }
}
