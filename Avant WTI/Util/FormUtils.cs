using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Avant.WTI.Util
{
    internal class FormUtils
    {


        private class DropdownItem<T>
        {
            public string Name { get; }
            public T Value { get; }

            public DropdownItem(string Name, T Value)
            {
                this.Name = Name;
                this.Value = Value;
            }
        }

        public static void Combobox_bindItems<T>(ComboBox b, Dictionary<T, string> items)
        {
            List<DropdownItem<T>> comboBoxItems = new List<DropdownItem<T>>();
            foreach (KeyValuePair<T, string> kv in items)
            {
                comboBoxItems.Add(new DropdownItem<T>(kv.Value, kv.Key));
            }
            b.DisplayMember = "Name";
            b.ValueMember = "Value";
            b.DataSource = comboBoxItems;
        }

    }
}
