using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

namespace Insight.Base.Services
{
    public class VerifyImage
    {
        // 背景图编号
        private readonly int Number = Parameters.Random.Next(0, 9);

        // 碎片编号
        private readonly int Index = Parameters.Random.Next(0, 8);

        // 背景图
        private Image Background;

        // 遮罩图
        private Image Mask;

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

    }
}
