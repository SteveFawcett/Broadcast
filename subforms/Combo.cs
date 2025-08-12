using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broadcast.subforms
{
    internal sealed class Combo : DataGridViewComboBoxCell
    {
        public Combo(string[] items)
        {
            DropDownWidth = 200;
            FlatStyle = FlatStyle.Flat;
            Items.AddRange(items);
            ValueType = typeof(string);

            if (items?.Length > 0)
                Value = items[0];
        }
    }
}
