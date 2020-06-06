using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiHsControls
{
    /// <summary>
    /// Abstract class prevents columns erroneously created by VS Designer by moving column creation to InitLayout
    /// </summary>
    public abstract class DataGridViewWLazyColumnCreation : DataGridView
    {
        public DataGridViewWLazyColumnCreation() : base()
        {
        }

        protected override void InitLayout()
        {
            base.InitLayout();
            if (!DesignMode)
            {
                createColumns();
            }
        }

        protected abstract void createColumns();
    }
}
