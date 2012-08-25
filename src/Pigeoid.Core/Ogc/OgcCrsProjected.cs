﻿// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A projected CRS.
	/// </summary>
	public class OgcCrsProjected : OgcNamedAuthorityBoundEntity, ICrsProjected
	{

		private readonly ICrsGeodetic _baseCrs;
		private readonly IUom _unit;
		private readonly ICoordinateOperationInfo _projection;
		private readonly IList<IAxis> _axes;

		/// <summary>
		/// Constructs a new projected CRS.
		/// </summary>
		/// <param name="name">The name of the CRS.</param>
		/// <param name="baseCrs">The CRS this CRS is based on.</param>
		/// <param name="projection">The projection operation.</param>
		/// <param name="linearUnit">The linear unit of the projection.</param>
		/// <param name="axes">The axes of the projected CRS.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsProjected(
			string name,
			ICrsGeodetic baseCrs,
			ICoordinateOperationInfo projection,
			IUom linearUnit,
			IEnumerable<IAxis> axes,
			IAuthorityTag authority
		)
			: base(name, authority) {

			if (null == baseCrs)
				throw new ArgumentNullException("baseCrs");
			if (null == linearUnit)
				throw new ArgumentNullException("linearUnit");
			if (null == projection)
				throw new ArgumentNullException("projection");

			_baseCrs = baseCrs;
			_unit = linearUnit;
			_projection = projection;
			_axes = Array.AsReadOnly(null == axes ? new IAxis[0] : axes.ToArray());
		}

		/// <inheritdoc/>
		public ICrsGeodetic BaseCrs {
			get { return _baseCrs; }
		}

		/// <inheritdoc/>
		public IUom Unit {
			get { return _unit; }
		}

		/// <inheritdoc/>
		public ICoordinateOperationInfo Projection {
			get { return _projection; }
		}

		/// <inheritdoc/>
		public IList<IAxis> Axes {
			get { return _axes; }
		}

		/// <inheritdoc/>
		public IDatumGeodetic Datum {
			get { return null == _baseCrs ? null : _baseCrs.Datum; }
		}

	}
}
