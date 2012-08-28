using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ImageServer.UI;
using ImageServer.Common;
using System.Drawing;

namespace ImageServer.UI
{
    /// <summary>
    /// ZoomHandler 的摘要说明
    /// </summary>
    public class ZoomHandler : ImagePatcherHandler
    {
        /// <summary>
        /// 预处理图片
        /// </summary>
        protected override void PreProcess()
        {
            if (Height > 0 && Width > 0)
            {
                if (IsStretch)  // 拉伸
                {
                    Image = Image.Zoom(Width, Height);
                }
                else
                {
                    if (Crop)
                    {
                        Image = Image.Crop(Width, Height, Position);    // 裁剪
                    }
                    else if (Fill)
                    {
                        Image = Image.MaxZoom(Width, Height, BgColor);  // 填充
                    }
                    else
                    {
                        Image = Image.ExpandZoom(Width, Height, BgColor);   // 适应
                    }
                }
            }
            else if (FixedBorder)
            {
                Image = Image.FixedBorderZoom(MaxWidth, MaxHeight); // 限宽/限高
            }
            else
            {
                Image = Image.FixGdi();
            }
        }

        #region 缩放

        /// <summary>
        /// 高度
        /// </summary>
        protected int Height { get { return RequestHelper.GetInt("h", context: context); } }

        /// <summary>
        /// 宽度
        /// </summary>
        protected int Width { get { return RequestHelper.GetInt("w", context: context); } }

        #endregion

        #region 限高或限宽
        /// <summary>
        /// 最长边边长
        /// </summary>
        protected int MaxLength { get { return RequestHelper.GetInt("ml", context: context); } }

        /// <summary>
        /// 限宽宽度
        /// </summary>
        protected int MaxWidth { get { return context.Request["mlb"] == "w" ? MaxLength : 0; } }

        /// <summary>
        /// 限高高度
        /// </summary>
        protected int MaxHeight { get { return context.Request["mlb"] == "h" ? MaxLength : 0; } }
        #endregion

        #region 剪切
        /// <summary>
        /// 是否剪切
        /// </summary>
        protected bool Crop { get { return context.Request["c"] != null; } }

        /// <summary>
        /// 剪切方位
        /// </summary>
        ImageHelper.Position Position { get { return (ImageHelper.Position)RequestHelper.GetInt("pos", context: context); } }
        #endregion

        #region 填充或适应
        /// <summary>
        /// 是否填充满整个输出尺寸或者画面适应输出尺寸
        /// </summary>
        protected bool Fill { get { return RequestHelper.GetInt("max", context: context) == 1; } }
        protected bool FixedBorder { get { return MaxLength > 0; } }
        protected bool IndentZoom { get { return !string.IsNullOrEmpty(context.Request["i"]); } }
        #endregion

        Color BgColor
        {
            get
            {
                int c = RequestHelper.GetInt("b", context: context);
                Color bgc = Color.Transparent;
                switch (c)
                {
                    case 1:
                        bgc = Color.White;
                        break;
                    case 2:
                        bgc = Color.Black;
                        break;
                    default:
                        break;
                }
                return bgc;
            }
        }

        #region 拉伸
        /// <summary>
        /// 是否拉伸
        /// </summary>
        bool IsStretch
        {
            get
            {
                return RequestHelper.GetInt("s", context: context) == 1;
            }
        }
        #endregion
    }
}
