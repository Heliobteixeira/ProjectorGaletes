using System;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GLUDScale : NumericUpDown, Widthable
    {


        public GLUDScale(Base parent, int X = 0, int Y = 0)
            : base(parent)
        {
            this.Parent = parent;
            this.Width = 75;
            this.SetPosition(X, Y);
            this.Value = 50;
            this.Max = 400;
            this.Min = 1;
            this.ValueChanged += OnValueChanged;
        }

		void OnValueChanged(Base control, EventArgs args)
        {
            //UnitPrint(String.Format("NumericUpDown: ValueChanged: {0}", ((Control.NumericUpDown)control).Value));
        }
    }
}
