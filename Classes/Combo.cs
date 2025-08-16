namespace Broadcast.Classes;

internal sealed class Combo : DataGridViewComboBoxCell
{
    public Combo(string?[] items)
    {
        DropDownWidth = 200;
        FlatStyle = FlatStyle.Flat;

        ValueType = typeof(string);

        if (items?.Length > 0)
        {
            Items.AddRange(items);
            Value = items[0];
        }
    }
}