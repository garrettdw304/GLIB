﻿using System.Text;

namespace GLIB.Inventory
{
	/// <summary>
	/// An inventory data structure that stores non-resizable multi-celled elements. Elements can be rotated 90 degrees, which is indicated via a boolean where true means the element is rotated 90 degrees. Rotated element's width and height are flipped.
	/// </summary>
	/// <typeparam name="E">The type of element to be stored in this Inventory.</typeparam>
	public class Inventory<E> where E : class
	{
		/// <summary>
		/// A method that calculates the size of an element. The size of an element CANNOT change while inside of the inventory (or the inventory will be corrupted), and as good practice should not change ever.
		/// </summary>
		/// <param name="element">The element to get the size of.</param>
		/// <returns>The size of the element.</returns>
        public delegate Size SizeOf(E element);

		/// <summary>
		/// The width of this inventory's grid.
		/// </summary>
		public readonly int Width;
		/// <summary>
		/// The height of this inventory's grid.
		/// </summary>
		public readonly int Height;

		/// <summary>
		/// The grid used for keeping track of the cells an element is taking up in this inventory.
		/// </summary>
		private readonly int[,] grid;
		/// <summary>
		/// The elements stored in this inventory.
		/// </summary>
		private readonly Dictionary<int, Element> elements;

		/// <summary>
		/// The SizeOf delegate to be used for finding the size of elements.
		/// </summary>
		protected SizeOf sizeOf;
		/// <summary>
		/// The inventory's filter. Items added to the inventory are passed through this filter and are only permitted to be added if the filter returns true.
		/// </summary>
		private Predicate<E> filter;

		/// <summary>
		/// Called when elements have been added to the inventory.
		/// </summary>
		public event Action<IReadOnlyCollection<IReadOnlyElement>> ElementsAdded;
		/// <summary>
		/// Called when elements have been moved in the inventory, including an element's rotation.
		/// </summary>
		public event Action<IReadOnlyCollection<IReadOnlyElement>> ElementsMoved;
		/// <summary>
		/// Called when elements have been removed from the inventory.
		/// </summary>
		public event Action<IReadOnlyCollection<IReadOnlyElement>> ElementsRemoved;

		/// <summary>
		/// Creates a new inventory instance with a grid the size of width by height.
		/// <para/>If filter is left null, a default filter allowing all items through is used.
		/// </summary>
		/// <param name="width">The width (x) of the inventory.</param>
		/// <param name="height">The height (y) of the inventory.</param>
		/// <param name="sizeOf">The SizeOf delegate used to get the size of elements.</param>
		/// <param name="filter">The filter to apply to items being added to the inventory. If left null, a default filter allowing all items through is used.</param>
		public Inventory(int width, int height, SizeOf sizeOf, Predicate<E> filter = null)
		{
			Width = width;
			Height = height;
			grid = new int[width, height];
			elements = new Dictionary<int, Element>();
			this.sizeOf = sizeOf;
			this.filter = filter != null ? filter : x => true;
		}

        #region public
        /// <summary>
        /// Adds an element to this inventory in the first slot it can fit, accounting for both rotations.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>True if the element was added successfully. False if the element was not added because it did not pass the filter or there was no space for it.</returns>
        /// <exception cref="InvalidSizeException"></exception>
        public bool Add(E element)
		{
			if (!filter(element))
				return false;

			Size size = sizeOf(element);
			if (!size.IsPhysical())
				throw new InvalidSizeException("The values of the element's size must be greater than 0.");

			if (Contains(element))
				return false;

			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
				{
					Slot slot = new Slot(x, y);
					if (Add(element, slot, false))
						return true;
					else if (Add(element, slot, true))
						return true;
				}

			return false;
		}

        /// <summary>
        /// Adds an element to this inventory at the specified slot, rotated accordingly.
        /// </summary>
        /// <param name="element">The element to add to this inventory.</param>
        /// <param name="slot">The slot to add this element at.</param>
        /// <param name="rotated">The rotation of the element.</param>
        /// <returns>True if the element passed the filter and could fit into the space specified, false o.w.</returns>
        /// <exception cref="InvalidSizeException"></exception>
        public bool Add(E element, Slot slot, bool rotated)
		{
			if (!filter(element))
				return false;

			Size size = sizeOf(element);
			if (!size.IsPhysical())
				throw new InvalidSizeException("The values of the element's size must be greater than 0.");

			if (Contains(element))
				return false;

			if (rotated)
				size = size.Rotated();

			if (!WillFit(slot, size))
				return false;

			int dictKey = GetNewDictKey();

			Element newEl = new Element(element, slot, rotated, dictKey);

            elements.Add(dictKey, newEl);

			SetSlots(slot, size, dictKey);

			ElementsAdded?.Invoke((IReadOnlyCollection<IReadOnlyElement>)newEl.Copy().Yield());

			return true;
		}

		public bool Move(E element, Slot newSlot, bool rotated)
		{
			Size sizeOfEl = sizeOf(element);
			Size newSize = sizeOfEl.Rotated(rotated);
			if (!newSize.IsPhysical())
				throw new Exception("The dimentions of the element's size must be greater than 0.");

			if (!Contains(element))
				throw new Exception("The element is not present in the inventory.");
			Element oldEl = elements.Values.First(x => x.Value == element);
			Size oldSize = sizeOfEl.Rotated(oldEl.Rotated);
			Slot oldSlot = oldEl.Slot;
			int dictKey = oldEl.DictKey;

			if (!WillFit(newSlot, newSize, dictKey))
				return false;

			SetSlots(oldSlot, oldSize, 0);
			SetSlots(newSlot, newSize, dictKey);
			oldEl.Slot = newSlot;
			oldEl.Rotated = rotated;

			ElementsMoved?.Invoke((IReadOnlyCollection<IReadOnlyElement>)oldEl.Copy().Yield());
			return true;
		}

		public bool FilterAllows(E element)
		{
			return filter(element);
		}

		public void SetFilter(Predicate<E> filter = null)
		{
			if (filter == null)
				filter = x => true;

			else
				this.filter = filter;
		}

		public E Get(Slot slot)
		{
			if (grid[slot.x, slot.y] == 0)
				return null;

			return elements[grid[slot.x, slot.y]].Value;
		}

		public E GetOne()
		{
			return IsEmpty() ? null : elements.Values.ElementAt(0).Value;
		}

		public List<E> GetAll()
		{
			List<E> list = new List<E>();
			foreach (Element e in elements.Values)
				list.Add(e.Value);

			return list;
		}

		public Element GetElement(E element)
		{
			foreach (Element e in elements.Values)
				if (e.Value == element)
					return e.Copy();

			return null;
		}

		public List<Element> GetAllElements()
		{
			List<Element> toReturn = new();

			foreach (Element e in elements.Values)
				toReturn.Add(e.Copy());

			return toReturn;
		}

		public bool Remove(E element)
		{
			foreach (Element e in elements.Values)
				if (e.Value == element)
				{
					Remove(e.Slot);
					return true;
				}

			return false;
		}

		public E Remove(Slot slot)
		{
			Element element = elements[grid[slot.x, slot.y]];

			elements.Remove(element.DictKey);

			SetSlots(element.Slot, sizeOf(element.Value).Rotated(element.Rotated));

			ElementsRemoved?.Invoke((IReadOnlyCollection<IReadOnlyElement>)element.Yield());

			return element.Value;
		}

		public bool Contains(E element)
		{
			foreach (Element e in elements.Values)
				if (e.Value == element)
					return true;

			return false;
		}

		public bool Contains(Predicate<E> predicate)
		{
			foreach (Element e in elements.Values)
				if (predicate(e.Value))
					return true;

			return false;
		}

		public Slot Find(E element)
		{
			foreach (Element e in elements.Values)
				if (e.Value == element)
					return e.Slot;

			return new Slot(-1, -1);
		}

		public Slot Find(Predicate<E> predicate)
		{
			foreach (Element e in elements.Values)
				if (predicate(e.Value))
					return e.Slot;

			return new Slot(-1, -1);
		}

		public bool TryAddAll(IEnumerable<E> elements)
		{
			List<Element> addedElements = new List<Element>();
			foreach (E element in elements)
			{
				if (!AddSilently(element, out Element added))
				{
					foreach (Element e in addedElements)
						RemoveSilently(e.Value);

					return false;
				}

				addedElements.Add(added);
			}

			ElementsAdded?.Invoke((IReadOnlyCollection<IReadOnlyElement>)addedElements.Select(x => x.Copy()));

			return true;
		}

		public void Clear()
		{
			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
					grid[x, y] = 0;

			List<Element> elsRemoved = elements.Values.ToList();
			elements.Clear();

			ElementsRemoved?.Invoke(elsRemoved);
		}

		public bool IsEmpty()
		{
			return !elements.Any();
		}

		public bool IsRotated(E element)
		{
			return elements.Values.First(x => x.Value == element).Rotated;
		}

        /// <summary>
		/// Tries to add/move the item to the slot indicated, and returns true if it would be added, false if not.
        /// </summary>
        /// <returns>
        /// Returns true if:
        /// <para/>1. There are no items in the slots this item would take up.
        /// <para/>2. The filter will allow the item into the inventory.
        /// <para/>Else false.
		/// </returns>
        public bool TestAdd(Slot slot, E element, bool rotate = false)
		{
			Size size = sizeOf(element);
			int ignore = GetDictKey(element);
			return filter(element) && WillFit(slot, rotate ? size.Rotated() : size, ignore);
		}
		#endregion public

		#region private
		private void SetSlots(Slot slot, Size size, int dictKey = 0)
		{
			for (int y = slot.y; y < slot.y + size.y; y++)
				for (int x = slot.x; x < slot.x + size.x; x++)
					grid[x, y] = dictKey;
		}

		private bool WillFit(Slot slot, Size size, int ignore = 0)
		{
			if (slot.y + size.y - 1 >= Height || slot.x + size.x - 1 >= Width)
				return false;

			for (int y = slot.y; y < slot.y + size.y; y++)
				for (int x = slot.x; x < slot.x + size.x; x++)
					if (grid[x, y] != 0 && grid[x, y] != ignore)
						return false;

			return true;
		}

		private int GetDictKey(E e)
		{
			foreach (Element e2 in elements.Values)
				if (e2.Value == e)
					return e2.DictKey;

			return 0;
		}

		private int GetNewDictKey()
		{
			for (int i = 1; i < int.MaxValue; i++)
				if (!elements.ContainsKey(i))
					return i;

			throw new IndexOutOfRangeException();
		}

		private bool AddSilently(E element, out Element added)
		{
			added = null;
			if (!filter(element))
				return false;

			Size size = sizeOf(element);
			if (!size.IsPhysical())
				throw new Exception("The dimentions of the element's size must be greater than 0.");

			if (Contains(element))
				throw new Exception("The element is already present in the inventory.");

			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
				{
					Slot slot = new Slot(x, y);
					if (AddSilently(element, slot, false, out added))
						return true;
					else if (AddSilently(element, slot, true, out added))
						return true;
				}

			return false;
		}

		private bool AddSilently(E element, Slot slot, bool rotated, out Element added)
		{
			added = null;

			if (!filter(element))
				return false;

			Size size = sizeOf(element);
			if (!size.IsPhysical())
				throw new Exception("The dimentions of the element's size must be greater than 0.");

			if (Contains(element))
				throw new Exception("The element is already present in the inventory.");

			if (rotated)
				size = size.Rotated();

			if (!WillFit(slot, size))
				return false;

			int dictKey = GetNewDictKey();

			added = new Element(element, slot, rotated, dictKey);

            elements.Add(dictKey, added);

			SetSlots(slot, size, dictKey);

            return true;
		}

		private bool RemoveSilently(E element)
		{
			foreach (Element e in elements.Values)
				if (e.Value == element)
				{
					RemoveSilently(e.Slot);
					return true;
				}

			return false;
		}

		private E RemoveSilently(Slot slot)
		{
			Element element = elements[grid[slot.x, slot.y]];

			elements.Remove(element.DictKey);

			SetSlots(element.Slot, sizeOf(element.Value).Rotated(element.Rotated));

			return element.Value;
		}
		#endregion private

		public class Element : IReadOnlyElement
		{
			public E Value { get; }
			public int DictKey { get; }

			public Slot Slot { get; set; }
			public bool Rotated { get; set; }

			public Element(E value, Slot slot, bool rotated, int dictKey)
			{
				Value = value;
				DictKey = dictKey;
				Slot = slot;
				Rotated = rotated;
			}

			public Element Copy()
			{
				return new Element(Value, Slot, Rotated, DictKey);
			}

            internal IEnumerable<Element> Yield()
			{
				yield return this;
			}
		}

		/// <summary>
		/// Exposes the getters of Element's properties.
		/// </summary>
		public interface IReadOnlyElement
		{
			public E Value { get; }
			public int DictKey { get; }
			public Slot Slot { get; }
			public bool Rotated { get; }
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
					sb.Append(grid[x, y] + " ");

				sb.Remove(sb.Length - 1, 1);
				sb.Append('\n');
			}

			sb.Remove(sb.Length - 1, 1);

			return sb.ToString();
		}
	}
}
