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
	}
}
