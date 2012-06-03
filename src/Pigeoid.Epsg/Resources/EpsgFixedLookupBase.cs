﻿using System;
using System.Collections.Generic;

namespace Pigeoid.Epsg.Resources
{
	internal class EpsgFixedLookupBase<TKey, TValue> :
		EpsgLookupBase<TKey, TValue>
		where TValue : class
	{

		protected readonly SortedDictionary<TKey, TValue> _lookup;

		/// <summary>
		/// Concrete classes must initialize the <c>Lookup</c> field from their constructor.
		/// </summary>
		internal EpsgFixedLookupBase(SortedDictionary<TKey, TValue> lookup) {
			if(null == lookup)
				throw new ArgumentNullException();

			_lookup = lookup;
		}

		public override TValue Get(TKey key) {
			TValue item;
			_lookup.TryGetValue(key, out item);
			return item;
		}

		public override IEnumerable<TKey> Keys {
			get { return _lookup.Keys; }
		}

		public override IEnumerable<TValue> Values {
			get { return _lookup.Values; }
		}

	}
}
