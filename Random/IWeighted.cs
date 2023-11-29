using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLIB.Random
{
	/// <summary>
	/// An element who has a weight associated with it. Used for weighted loot systems and the like for selecting random elements who have different chances of being picked.
	/// <para/> higher weight -> less common
	/// </summary>
	public interface IWeighted
	{
		/// <summary>
		/// Gets the weight of this object.
		/// </summary>
		public int GetWeight();
	}
}
