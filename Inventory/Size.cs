﻿namespace GLIB.Inventory
{
	[Serializable]
	public struct Size
	{
		public static readonly Size ONE = new Size(1, 1);

		public int x;
		public int y;

		public Size(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public bool IsPhysical()
		{
			return x > 0 && y > 0;
		}

		public Size Rotated(bool isRotated = true)
		{
			return isRotated ? new Size(y, x) : this;
		}

		public static Size Parse(string str)
		{
			string[] strs = str.Split(' ');
			return new Size(int.Parse(strs[0]), int.Parse(strs[1]));
		}
	}
}
