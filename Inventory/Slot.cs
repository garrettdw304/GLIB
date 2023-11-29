using System.Text;

namespace GLIB.Inventory
{
	/// <summary>
	/// A 2 dimensional slot structure. Stores x and y values for accessing elements of an Inventory.
	/// </summary>
	public struct Slot
	{
		public int x;
		public int y;

		public Slot(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

        /// <summary>
        /// Parses a slot from a string, where a space seperates the x and y values used to construct the slot.
        /// </summary>
		public static Slot Parse(string str)
		{
			string[] strs = str.Split(' ');
			return new Slot(int.Parse(strs[0]), int.Parse(strs[1]));
		}

        /// <summary>
        /// Returns true if the left hand side's x and y match the right hand side's x and y, respectively. Otherwise false.
        /// </summary>
        public static bool operator ==(Slot left, Slot right)
		{
			return left.x == right.x && left.y == right.y;
		}

        /// <summary>
        /// Returns true if the left hand side's x or y does not match the right hand side's x or y, respectively. Otherwise false.
        /// </summary>
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

        /// <summary>
        /// Returns a string representation of this slot, including the value of both dimensions.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            return sb.Append('<').Append(x).Append(", ").Append(y).Append('>').ToString();
        }
    }
}
