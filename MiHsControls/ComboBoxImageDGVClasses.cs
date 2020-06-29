using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiHsControls
{
    /// <summary>
    /// Custom DataGridViewColumn which displays images when not in edit mode, and switches over to combobox for editing
    /// </summary>
    internal class ComboBoxImageColumn : DataGridViewColumn
    {
        public ComboBoxImageColumn(ImageList imageList) : base(new ComboBoxImageCell(imageList))
        {
            DataGridViewCellStyle defaultCellStyle = new DataGridViewCellStyle();
            defaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            defaultCellStyle.NullValue = ErrorBitmapInternal.ErrorBitmap;
            this.DefaultCellStyle = defaultCellStyle;
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override DataGridViewCell CellTemplate {
            get => base.CellTemplate;
            set {
                if (value != null && !value.GetType().IsAssignableFrom(typeof(ComboBoxImageCell)))
                {
                    throw new InvalidCastException("Must be a ComboBoxImageCell");
                }
                base.CellTemplate = value;
            }
        }
    }

    /// <summary>
    /// Specialized DataGridViewComboBoxCell which lets user select from a list of images and displays a selected image when not in edit mode
    /// </summary>
    internal class ComboBoxImageCell : DataGridViewComboBoxCell
    {
        private ImageList imageList;

        public ImageList ImageList
        {
            get => imageList;
            set
            {
                if (imageList != value)
                {
                    imageList = value;
                    // TODO ComboBoxImageDGVClasses/ComboBoxImageCell: update image display when ImageList is set
                }
            }
        }

        public override Type EditType
        {
            get
            {
                return typeof(ComboBoxImageEditingControl);
            }
        }

        //private Type valueType;

        //public override Type ValueType
        //{
        //    get
        //    {
        //        return valueType;
        //    }
        //}

        //public override object DefaultNewRowValue
        //{
        //    get
        //    {
        //        // Use the current date and time as the default value.
        //        return DateTime.Now;
        //    }
        //}

        public ComboBoxImageCell(ImageList imageList) : base()
        {
        }

        /// <summary>
        /// Set the value of the editing control to the current cell value.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="initialFormattedValue"></param>
        /// <param name="dataGridViewCellStyle"></param>
        public override void InitializeEditingControl(int rowIndex, object
                initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

            ComboBoxImageEditingControl ctl = DataGridView.EditingControl as ComboBoxImageEditingControl;
            ctl.ImageList = imageList;
            ctl.SelectedValue = this.Value;
            ctl.DroppedDown = true;
        }
    }

    /// <summary>
    /// Represents a ComboBox which is used as editing control
    /// </summary>
    internal class ComboBoxImageEditingControl : ComboBox, IDataGridViewEditingControl
    {
        public ComboBoxImageEditingControl() : this(null)
        {
        }

        public ComboBoxImageEditingControl(ImageList imageList) : base()
        {
            this.imageList = imageList;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            DrawItem += ComboBox_DrawItem;
        }

        private ImageList imageList;

        public ImageList ImageList
        {
            get => imageList;
            set
            {
                if (!this.DroppedDown && imageList != value)
                {
                    imageList = value;
                }
            }
        }

        public object EditingControlFormattedValue
        {
            get => this.SelectedValue;
            set => this.SelectedValue = value;
        }

        private DataGridView dataGridView;

        public DataGridView EditingControlDataGridView
        {
            get => dataGridView;
            set => dataGridView = value;
        }

        private int rowIndex;

        public int EditingControlRowIndex
        {
            get => rowIndex;
            set => rowIndex = value;
        }

        private bool valueChanged = false;

        public bool EditingControlValueChanged
        {
            get => valueChanged;
            set => valueChanged = value;
        }

        public bool RepositionEditingControlOnValueChange
        {
            get => false;
        }

        public Cursor EditingPanelCursor
        {
            get => base.Cursor;
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.ForeColor = dataGridViewCellStyle.ForeColor;
            this.BackColor = dataGridViewCellStyle.BackColor;
        }

        /// <summary>
        /// Always returns true: all keys are handled by the ComboBox
        /// </summary>
        /// <param name="keyData"></param>
        /// <param name="dataGridViewWantsInputKey"></param>
        /// <returns></returns>
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                case Keys.PageDown:
                case Keys.PageUp:
                    return true;
                default:
                    return !dataGridViewWantsInputKey;
            }
        }

        public void PrepareEditingControlForEdit(bool selectAll)
        {
            // nothing to do
        }

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            if (EditingControlDataGridView != null)
            {
                EditingControlDataGridView.EndEdit();
            }
            base.OnSelectionChangeCommitted(e);
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            valueChanged = true;
            this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            base.OnSelectedValueChanged(e);
        }

        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1) { return; }

            e.DrawBackground();

            Graphics g = e.Graphics;
            Image image = null;
            try
            {
                image = imageList.Images[e.Index];

                g.DrawImage(image, new Rectangle(e.Bounds.Left + (e.Bounds.Width - image.Width) / 2,
                                                   e.Bounds.Top, image.Width, image.Height));
            }
            catch
            {
                image = ErrorBitmapInternal.ErrorBitmap;
                g.DrawImage(image, new Rectangle(e.Bounds.Left + (e.Bounds.Width - image.Width) / 2,
                                                 e.Bounds.Top, image.Width, image.Height));
            }

            bool isSelected = (e.State & DrawItemState.Selected) != 0;

            if (isSelected)
            {
                e.DrawFocusRectangle();
            }
        }

    }
}
