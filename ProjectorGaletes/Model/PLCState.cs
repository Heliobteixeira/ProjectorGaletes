using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectorGaletes
{
    class PLCState
    {
        private int _intangle;
        private int _direction;
        private int _turn;

        public int intangle
        {
            get { return _intangle; }
            set { _intangle = value; }
        }

        public int direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public int turn
        {
            get { return _turn; }
            set { _turn = value; }
        }

        public float angle
        {
            get { return _intangle / 10.0f; }
        }

    }
}
