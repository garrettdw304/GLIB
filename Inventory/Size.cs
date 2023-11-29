using System.Text;

namespace GLIB.Inventory
{
    /// <summary>
    /// A 2 dimensional size structure.
    /// <para/> Used by Inventory to describe the size of an inventory element.
    /// </summary>
    [Serializable]
	public struct Size
	{
        /// <summary>
        /// A reference to a size with both dimensions set to 1.
        /// </summary>
		public static readonly Size ONE = new Size(1, 1);

        /// <summary>
        /// The x dimension of this size.
        /// </summary>
		public int x;
        /// <summary>
        /// The y dimension of this size.
        /// </summary>
		public int y;

		public Size(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

        /// <summary>
        /// Ensures both dimensions are greater than 0.
        /// </summary>
        /// <returns>True if both dimensions are greater than 0, false otherwise.</returns>
		public bool IsPhysical()
		{
			return x > 0 && y > 0;
		}

        /// <summary>
        /// Swaps the x and y dimensions.
        /// </summary>
        /// <param name="isRotated">True if the dimensions should be swapped, false if the dimensions should not be swapped.</param>
        /// <returns>If isRotated is true, returns a size with the dimensions swapped. If isRotated is false, returns the size unswapped.</returns>
		public Size Rotated(bool isRotated = true)
		{
			return isRotated ? new Size(y, x) : this;
		}

        /// <summary>
        /// Parses a size from a string, where a space seperates the values for the x and y dimensions.
        /// </summary>
		public static Size Parse(string str)
		{
			string[] strs = str.Split(' ');
			return new Size(int.Parse(strs[0]), int.Parse(strs[1]));
		}

        /// <summary>
        /// Returns true if the left hand side's x and y match the right hand side's x and y, respectively. Otherwise false.
        /// </summary>
        public static bool operator ==(Size left, Size right)
        {
            return left.x == right.x && left.y == right.y;
        }

        /// <summary>
        /// Returns true if the left hand side's x or y does not match the right hand side's x or y, respectively. Otherwise false.
        /// </summary>
        public static bool operator !=(Size left, Size right)
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
        /// Returns a string representation of this size, including the value of both dimensions.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
			StringBuilder sb = new StringBuilder();
            return sb.Append('<').Append(x).Append(", ").Append(y).Append('>').ToString();
        }
    }
}
