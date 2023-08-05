﻿using System.Text;

namespace GLIB.Inventory
{
	public struct Slot
	{
		public int x;
		public int y;

		public Slot(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static Slot Parse(string str)
		{
			string[] strs = str.Split(' ');
			return new Slot(int.Parse(strs[0]), int.Parse(strs[1]));
		}

		public static bool operator ==(Slot left, Slot right)
		{
			return left.x == right.x && left.y == right.y;
		}

		public static bool operator !=(Slot left, Slot right)
		{
			return left.x != right.x || left.y != right.y;
		}

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            return sb.Append('<').Append(x).Append(", ").Append(y).Append('>').ToString();
        }
    }
}
