using FileSyncDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiHsControls
{
    /// <summary>
    /// DataGridViewColumn which displays one image converted from an enum value depending on each individual cell integer value
    /// </summary>
    public class ImageEnumColumn : DataGridViewColumn
    {
        public ImageEnumColumn(Type valueType) : base(new ImageEnumCell())
        {
            if (!valueType.IsEnum)
            {
                throw new ArgumentException("Value type must derive from Enum!");
            }
            this.ValueType = valueType;

            DataGridViewCellStyle defaultCellStyle = new DataGridViewCellStyle();
            defaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            defaultCellStyle.NullValue = ErrorBitmapInternal.ErrorBitmap;
            this.DefaultCellStyle = defaultCellStyle;
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DataGridViewCell CellTemplate
        {
            get => base.CellTemplate;
            set
            {
                if (value != null && !value.GetType().IsAssignableFrom(typeof(ImageEnumCell)))
                {
                    throw new InvalidCastException("Must be a ImageListCell");
                }
                base.CellTemplate = value;
            }
        }
    }

    /// <summary>
    /// DataGridViewCell which displays one image converted from an enum value depending on the cell integer value
    /// </summary>
    public class ImageEnumCell : DataGridViewCell
    {
        public override Type EditType
        {
            get
            {
                return null;
            }
        }

        private static Type defaultTypeImage = typeof(Image);

        public override object DefaultNewRowValue
        {
            get
            {
                return ErrorBitmapInternal.ErrorBitmap;
            }
        }

        public override Type FormattedValueType
        {
            get
            {
                return defaultTypeImage;
            }
        }

        [DefaultValue("")]
        public string Description
        {
            get
            {
                return String.Empty;
            }
        }

        private DataGridViewImageCellLayout imageLayout = DataGridViewImageCellLayout.Normal;

        public DataGridViewImageCellLayout ImageLayout {
            get
            {
                return imageLayout;
            }
            set
            {
                imageLayout = value;
            }
        }

        public ImageEnumCell() : base()
        {
        }

        /// <summary>
        /// Paints the cell border, contents and selection/focus rectangle; 
        /// the displayed image depends on the ValueType and the definition of a TypeConverter class 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="clipBounds"></param>
        /// <param name="cellBounds"></param>
        /// <param name="rowIndex"></param>
        /// <param name="elementState"></param>
        /// <param name="value"></param>
        /// <param name="formattedValue"></param>
        /// <param name="errorText"></param>
        /// <param name="cellStyle"></param>
        /// <param name="advancedBorderStyle"></param>
        /// <param name="paintParts"></param>
        protected override void Paint(Graphics g,
                    Rectangle clipBounds,
                    Rectangle cellBounds,
                    int rowIndex,
                    DataGridViewElementStates elementState,
                    object value,
                    object formattedValue,
                    string errorText,
                    DataGridViewCellStyle cellStyle,
                    DataGridViewAdvancedBorderStyle advancedBorderStyle,
                    DataGridViewPaintParts paintParts)
        {
            if (cellStyle == null)
            {
                throw new ArgumentNullException("cellStyle");
            }

            if ((paintParts & DataGridViewPaintParts.Border) != 0)
            {
                PaintBorder(g, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }

            Rectangle valBounds = cellBounds;
            Rectangle borderWidths = BorderWidths(advancedBorderStyle);
            valBounds = applyBorder(borderWidths, valBounds);

            if (valBounds.Width <= 0 || valBounds.Height <= 0)
            {
                return;
            }

            Rectangle imgBounds = valBounds;
            if (cellStyle.Padding != Padding.Empty)
            {
                imgBounds = applyPadding(cellStyle.Padding, imgBounds);
            }

            SolidBrush br = null;
            try
            {
                bool cellSelected = (elementState & DataGridViewElementStates.Selected) != 0;
                br = new SolidBrush((paintParts & DataGridViewPaintParts.SelectionBackground) != 0 && cellSelected ?
                                                                                cellStyle.SelectionBackColor : cellStyle.BackColor);

                if (imgBounds.Width > 0 && imgBounds.Height > 0)
                {
                    Image img = formattedValue as Image;
                    if (img != null)
                    {
                        DataGridViewImageCellLayout imageLayout = this.ImageLayout;
                        if (imageLayout == DataGridViewImageCellLayout.NotSet)
                        {
                            if (this.OwningColumn is DataGridViewImageColumn)
                            {
                                imageLayout = ((DataGridViewImageColumn)this.OwningColumn).ImageLayout;
                                Debug.Assert(imageLayout != DataGridViewImageCellLayout.NotSet);
                            }
                            else
                            {
                                imageLayout = DataGridViewImageCellLayout.Normal;
                            }
                        }

                        if ((paintParts & DataGridViewPaintParts.Border) != 0)
                        {
                            g.FillRectangle(br, valBounds);
                        }

                        if ((paintParts & DataGridViewPaintParts.ContentForeground) != 0)
                        {
                            Rectangle imgBounds2 = applyCenterAlignment(imgBounds, img.Size, imageLayout);

                            Region reg = g.Clip;
                            g.SetClip(Rectangle.Intersect(Rectangle.Intersect(imgBounds2, imgBounds), Rectangle.Truncate(g.VisibleClipBounds)));
                            g.DrawImage(img, imgBounds2);
                            g.Clip = reg;
                        }
                    }
                    else
                    {
                        fillBackGroundAsRequired(g, paintParts, valBounds, br);
                    }
                }
                else
                {
                    fillBackGroundAsRequired(g, paintParts, valBounds, br);
                }

                Point ptCurrentCell = this.DataGridView.CurrentCellAddress;
                if ((paintParts & DataGridViewPaintParts.Focus) != 0 &&
                    ptCurrentCell.X == this.ColumnIndex && ptCurrentCell.Y == rowIndex &&
                    this.DataGridView.Focused)
                {
                    ControlPaint.DrawFocusRectangle(g, valBounds, Color.Empty, br.Color);
                }
            }
            finally
            {
                br.Dispose();
            }

            if (this.DataGridView.ShowCellErrors && (paintParts & DataGridViewPaintParts.ErrorIcon) != 0)
            {
                PaintErrorIcon(g, cellBounds, valBounds, errorText);
            }
        }

        private static Rectangle applyBorder(Rectangle borderWidths, Rectangle valBounds)
        {
            valBounds.Offset(borderWidths.X, borderWidths.Y);
            valBounds.Width -= borderWidths.Right;
            valBounds.Height -= borderWidths.Bottom;
            return valBounds;
        }

        private static Rectangle applyPadding(Padding padding, Rectangle imgBounds)
        {
            imgBounds.Offset(padding.Left, padding.Top);
            imgBounds.Width -= padding.Horizontal;
            imgBounds.Height -= padding.Vertical;
            return imgBounds;
        }

        private static Rectangle applyCenterAlignment(Rectangle imgBounds, Size imgSize, DataGridViewImageCellLayout imageLayout)
        {
            Rectangle imgBounds2;
            switch (imageLayout)
            {
                case DataGridViewImageCellLayout.Normal:
                case DataGridViewImageCellLayout.NotSet:
                    imgBounds2 = new Rectangle(imgBounds.X, imgBounds.Y, imgSize.Width, imgSize.Height);
                    break;
                case DataGridViewImageCellLayout.Zoom:
                    if (imgSize.Width * imgBounds.Height < imgSize.Height * imgBounds.Width)
                    {
                        imgBounds2 = new Rectangle(imgBounds.X, imgBounds.Y,
                            Decimal.ToInt32((decimal)imgSize.Width * imgBounds.Height / imgSize.Height), imgBounds.Height);
                    }
                    else
                    {
                        imgBounds2 = new Rectangle(imgBounds.X, imgBounds.Y,
                            imgBounds.Width, Decimal.ToInt32((decimal)imgSize.Height * imgBounds.Width / imgSize.Width));
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid ImageLayout!");
            }

            imgBounds2.Offset((imgBounds.Width - imgBounds2.Width) / 2,
                              (imgBounds.Height - imgBounds2.Height) / 2);
            return imgBounds2;
        }

        private static void fillBackGroundAsRequired(Graphics g, DataGridViewPaintParts paintParts, Rectangle valBounds, SolidBrush br)
        {
            if ((paintParts & DataGridViewPaintParts.Background) != 0)
            {
                g.FillRectangle(br, valBounds);
            }
        }

    }

}