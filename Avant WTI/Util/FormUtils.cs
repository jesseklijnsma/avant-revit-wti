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

        /// <summary>
        /// A wrapper for a dropdown item
        /// </summary>
        /// <typeparam name="T">Value Object</typeparam>
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

        /// <summary>
        /// Sets all available options on a combobox to a dictionary of objects and their display value
        /// </summary>
        /// <typeparam name="T">Value Object</typeparam>
        /// <param name="b">Combobox</param>
        /// <param name="items">Dictinary of object and display value</param>
        public static void Combobox_bindItems<T>(ComboBox b, Dictionary<T, string> items)
        {
            // Create a new dropdownitems for each item entry
            List<DropdownItem<T>> comboBoxItems = new List<DropdownItem<T>>();
            foreach (KeyValuePair<T, string> kv in items)
            {
                comboBoxItems.Add(new DropdownItem<T>(kv.Value, kv.Key));
            }
            // Set the value to retrieve as displayvalue from the DropdownItem to 'Name'
            b.DisplayMember = "Name";
            // Set the value to retrieve as value from the DropdownItem to 'Name'
            b.ValueMember = "Value";
            // Bind the list of dropdownitems to the combobox
            b.DataSource = comboBoxItems;
        }

    }
}
