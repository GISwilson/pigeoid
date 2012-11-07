﻿// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A geocentric CRS.
	/// </summary>
	public class OgcCrsGeocentric : OgcNamedAuthorityBoundEntity, ICrsGeocentric
	{

		private readonly IDatumGeodetic _datum;
		private readonly IUnit _unit;
		private readonly IList<IAxis> _axes;

		/// <summary>
		/// Constructs a new geocentric CRS.
		/// </summary>
		/// <param name="name">The name of the CRS.</param>
		/// <param name="datum">The datum the CRS is based on.</param>
		/// <param name="linearUnit">The linear UoM to use for the CRS.</param>
		/// <param name="axes">The axes which define the space.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsGeocentric(
			string name,
			[NotNull] IDatumGeodetic datum,
			[NotNull] IUnit linearUnit,
			IEnumerable<IAxis> axes,
			IAuthorityTag authority
		) : base(name, authority) {
			if (null == datum)
				throw new ArgumentNullException("datum");
			if (null == linearUnit)
				throw new ArgumentNullException("linearUnit");

			_datum = datum;
			_unit = linearUnit;
			_axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
		}

		/// <inheritdoc/>
		public IDatumGeodetic Datum { get { return _datum; } }

		/// <inheritdoc/>
		public IUnit Unit { get { return _unit; } }

		/// <inheritdoc/>
		public IList<IAxis> Axes{ get { return _axes; } }

	}
}
