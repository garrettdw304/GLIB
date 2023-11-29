namespace GLIB.Random
{
	// higher weight -> rarer
	public class Random
	{
		/// <summary>
		/// A method that gets the weight of the passed element to be used when selecting a random weighted element.
		/// <para/>higher weight -> less common
		/// </summary>
		/// <returns>An integer representing the weight of the passed element.</returns>
		public delegate int GetWeight<T>(T element);
		/// <summary>
		/// Provides random numbers. Useful if the built in random provider (System.Random) does not meet your needs.
		/// <para/>This functionallity was included because the Unity game engine uses an earlier version of the .NET runtime than this project is developed for, which was causing issues when using System.Random.
		/// </summary>
		/// <param name="minInclusive">The minimum value to be generated, inclusive.</param>
		/// <param name="maxExclusive">The maximum value to be generated, exclusive.</param>
		/// <returns>A random number between minInclusive and maxExclusive.</returns>
		public delegate int RandomProvider(int minInclusive, int maxExclusive);

		private static readonly RandomProvider DEFAULT_RANDOM_PROVIDER = System.Random.Shared.Next;

		/// <summary>
		/// Returns a random element from the collection, where every element has equal chance of being picked.
		/// </summary>
		public static T GetRandomUnweighted<T>(ICollection<T> elements)
		{
			return GetRandom(elements, x => 1);
		}

        /// <summary>
        /// Returns a random element from the collection, where every element has equal chance of being picked.
        /// <para/>This version of GetRandomUnweighted allows the mode of getting random values to be provided.
        /// </summary>
        public static T GetRandomUnweighted<T>(ICollection<T> elements, RandomProvider randomProvider)
		{
			return GetRandom(elements, x => 1, randomProvider);
		}

		/// <summary>
		/// Returns a random element from the collection, chosen using a weighted system.
		/// <para/>Higher weight -> less common
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements">An ICollection of elements to be picked from. MUST BE PRE-SORTED from least to greatest weight value.</param>
		/// <returns>The element that was chosen from elements.</returns>
		public static T GetRandom<T>(ICollection<T> elements) where T : IWeighted
		{
			return GetRandom(elements, x => x.GetWeight(), DEFAULT_RANDOM_PROVIDER);
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
			return GetRandom(elements, weightGetter, DEFAULT_RANDOM_PROVIDER);
		}

		/// <summary>
		/// Returns a random element from the collection, chosen using a weighted system.
		/// <para/>Higher weight -> less common
		/// <para/>This version of GetRandom allows the mode of getting random values to be provided.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements">An ICollection of elements to be picked from. MUST BE PRE-SORTED from least to greatest weight value.</param>
		/// <returns>The element that was chosen from elements.</returns>
		public static T GetRandom<T>(ICollection<T> elements, RandomProvider randomProvider) where T : IWeighted
		{
			return GetRandom(elements, x => x.GetWeight(), randomProvider);
		}

		/// <summary>
		/// Returns a random element from the collection, chosen using a weighted system.
		/// <para/>Higher weight -> less common
		/// <para/>This version of GetRandom allows the mode of getting random values to be provided.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements">An ICollection of elements to be picked from. MUST BE PRE-SORTED from least to greatest weight value.</param>
		/// <returns>The element that was chosen from elements.</returns>
		public static T GetRandom<T>(ICollection<T> elements, GetWeight<T> weightGetter, RandomProvider randomProvider)
		{
			int totalWeight = 0;
			foreach (T element in elements)
				totalWeight += weightGetter(element);

			int roll = randomProvider(0, totalWeight);

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
