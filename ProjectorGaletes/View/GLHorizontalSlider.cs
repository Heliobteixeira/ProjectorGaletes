using System;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GLHorizontalSlider : HorizontalSlider, Widthable
    {
        public GLHorizontalSlider(Base parent, float value = 0, int minRange = 0, int maxRange = 100, int X = 0, int Y = 0)
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
