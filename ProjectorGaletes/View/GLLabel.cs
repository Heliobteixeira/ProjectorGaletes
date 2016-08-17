using System;
using System.Drawing;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GLLabel : Label, Widthable
    {

        public GLLabel(Base parent, string text, int X = 0, int Y = 0) : base(parent)
        {
            {
                this.Parent = parent;
                this.Text = text;
                this.AutoSizeToContents = true;
                this.SetPosition(X, Y);
            }
            
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y+3);

        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
