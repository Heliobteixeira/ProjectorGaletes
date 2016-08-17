using System;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GLPropertyTree : PropertyTree, Widthable
    {

        public GLPropertyTree(Base parent, int X = 0, int Y = 0)
            : base(parent)
        {
            {
                this.Parent = parent;
                this.SetPosition(X, Y);
                this.Width = 300;
            }

        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
