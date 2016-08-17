using System;
using System.Drawing;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GLTextBox : TextBox, Widthable
    {


        public GLTextBox(Base parent, string text, int X=0, int Y=0)
            : base(parent) {

			/* Vanilla Textbox */
			{
                this.Parent = parent;
                this.Width = 75;
                this.SetText(text);
                this.SetPosition(X, Y);
                this.TextChanged += OnEdit;
                this.SubmitPressed += OnSubmit;
			}

		}

        public override void Dispose()
        {
            base.Dispose();
        }

		void OnEdit(Base control, EventArgs args)
        {
            TextBox box = control as TextBox;
            //UnitPrint(String.Format("TextBox: OnEdit: {0}", box.Text));
        }

		void OnSubmit(Base control, EventArgs args)
        {
            TextBox box = control as TextBox;
           // UnitPrint(String.Format("TextBox: OnSubmit: {0}", box.Text));
        }

    }
}
