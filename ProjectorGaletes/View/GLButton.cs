using System;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GLButton : Button, Widthable
    {

        public GLButton(Base parent, string text, int X = 0, int Y = 0)
            : base(parent)
        {

            this.Parent = parent;
            this.SetText(text);
            this.SetPosition(X, Y);
            this.AutoSizeToContents = true;
         

        }


    }
}
