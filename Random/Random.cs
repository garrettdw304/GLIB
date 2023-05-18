using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// higher weight -> rarer
namespace GLIB.Random
{
	public class Random
	{
		public delegate int GetWeight<T>(T element);

		/// <summary>
		/// Returns a random element from the collection, chosen using a weighted system.
		/// <para/>Higher weight -> less common
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements">An ICollection of elements to be picked from. MUST BE PRE-SORTED from least to greatest weight value.</param>
		/// <returns>The element that was chosen from elements.</returns>
		public static T GetRandom<T>(ICollection<T> elements) where T : IWeighted
		{
			int totalWeight = 0;
			foreach (T element in elements)
			{
				totalWeight += element.GetWeight();
			}

			System.Random random = new System.Random((int)DateTime.Now.Ticks);
			int roll = random.Next(0, totalWeight);

			foreach (T element in elements)
			{
				roll -= element.GetWeight();

				if (roll < 0)
					return element;
			}

			return elements.First();
		}

		/// <summary>
		/// Returns a random element from the collection, chosen using a weighted system.
		/// <para/>Higher weight -> less common
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements">An ICollection of elements to be picked from. MUST BE PRE-SORTED from least to greatest weight value.</param>
		/// <returns>The element that was chosen from elements.</returns>
		public static T GetRandom<T>(ICollection<T> elements, GetWeight<T> weightGetter)
		{
			int totalWeight = 0;
			foreach (T element in elements)
			{
				totalWeight += weightGetter(element);
			}

			System.Random random = new System.Random((int)DateTime.Now.Ticks);
			int roll = random.Next(0, totalWeight);

			foreach (T element in elements)
			{
				roll -= weightGetter(element);

				if (roll < 0)
					return element;
			}

			return elements.First();
		}
	}
}
