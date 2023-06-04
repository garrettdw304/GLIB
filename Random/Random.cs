﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
// higher weight -> rarer
namespace GLIB.Random
{
	public class Random
	{
		public delegate int GetWeight<T>(T element);
		public delegate int RandomProvider(int minInclusive, int maxExclusive);

		public static readonly RandomProvider DEFAULT_RANDOM_PROVIDER = (a, b) => new System.Random((int)DateTime.Now.Ticks).Next(a, b);

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
			int totalWeight = GetTotalWeight(elements, weightGetter);

			int roll = randomProvider(0, totalWeight);

			foreach (T element in elements)
			{
				roll -= weightGetter(element);

				if (roll < 0)
					return element;
			}

			return elements.First();
		}

		private static int GetTotalWeight<T>(ICollection<T> collection, GetWeight<T> weightGetter)
		{
			int totalWeight = 0;
			foreach (T element in collection)
				totalWeight += weightGetter(element);

			return totalWeight;
		}
	}
}
