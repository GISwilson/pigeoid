﻿// TODO: source header

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pigeoid.Epsg.Resources
{
	internal abstract class EpsgDynamicLookupBase<TKey, TValue> :
		EpsgLookupBase<TKey, TValue>
		where TValue : class
	{

		private readonly ConcurrentDictionary<TKey, TValue> _lookup;
		private readonly TKey[] _orderedKeys;

		private bool _fullReadPerformed;
		private readonly object _fullReadMutex; // TODO: interlocked may be better

		/// <summary>
		/// Concrete classes must initialize the <c>AllOrderedKeys</c> field from their constructor.
		/// </summary>
		protected EpsgDynamicLookupBase(TKey[] orderedKeys) {
			if(null == orderedKeys)
				throw new ArgumentNullException();

			// TODO: ONLY if in debug, make sure the keys are ordered?

			_lookup = new ConcurrentDictionary<TKey, TValue>();
			_fullReadPerformed = false;
			_fullReadMutex = new object();
			_orderedKeys = orderedKeys;
		}

		public override IEnumerable<TValue> Values {
			get {
				if(!_fullReadPerformed)
					SingleFullRead();

				return _lookup.Values.OrderBy(GetKeyForItem);
			}
		}

		public override IEnumerable<TKey> Keys {
			get { return Array.AsReadOnly(_orderedKeys); }
		}

		public override TValue Get(TKey key) {
			// try to find the item
			TValue item;
			if (_lookup.TryGetValue(key, out item))
				return item;
			// see if we have a key for it
			var i = Array.BinarySearch(_orderedKeys, key);
			if (i < 0)
				return default(TValue);
			// if we do have a key for it, try a get add
			// we do a GetOrAdd instead of TryAdd just in case somebody beat us to it
			return _lookup.GetOrAdd(key, k => Create(k,i));
		}

		protected abstract TValue Create(TKey key, int index);

		protected abstract TKey GetKeyForItem(TValue value);

		private void SingleFullRead() {
			lock(_fullReadMutex) {
				// this is a very heavy operation so we want to be sure it has not been done before
				if (_fullReadPerformed)
					return; // could happen, so we check again to be sure

				for (int i = 0; i < _orderedKeys.Length; i++) {
					// if TryAdd used a delegate we could have use that, but we want to only have one reference for each code
					// call directly on the lookup to avoid another key lookup
					_lookup.GetOrAdd(_orderedKeys[i], k => Create(k, i));
				}
				_fullReadPerformed = true;
			}
		}

	}
}
