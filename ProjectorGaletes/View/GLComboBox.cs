using System;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GLComboBox : ComboBox, Widthable
    {
        public GLComboBox(Base parent, int X = 0, int Y = 0)
            : base(parent)
        {

            {
                // In-Code Item Change
                this.Parent = parent;
                this.SetPosition(X, Y);
                //this.AutoSizeToContents = true;
                //this.Width = 100;

                //MenuItem Triangle = combo.AddItem("Galete 1", "1");
                //this.ItemSelected += OnComboSelect;

            }
        }


		void OnComboSelect(Base control, EventArgs args)
        {
            ComboBox combo = control as ComboBox;
           // UnitPrint(String.Format("ComboBox: OnComboSelect: {0}", combo.SelectedItem.Text));
        }
    }
}
