﻿
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgCrsGeodetic : EpsgCrsDatumBased, ICrsGeodetic
	{

		private readonly EpsgDatumGeodetic _geodeticDatum;

		internal EpsgCrsGeodetic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
			: base(code,name,area,deprecated, cs)
		{
			_geodeticDatum = geodeticDatum;
		}

		public override EpsgDatum Datum { get { return _geodeticDatum; } }

		public EpsgDatumGeodetic GeodeticDatum { get { return _geodeticDatum; } }

		IDatumGeodetic ICrsGeodetic.Datum { get { return _geodeticDatum; } }

		public EpsgUnit Unit { get { return CoordinateSystem.Axes.First().Unit; } }

		IUnit ICrsGeodetic.Unit { get { return Unit; } }

		public IList<EpsgAxis> Axes { get { return CoordinateSystem.Axes.ToArray(); } }

		IList<IAxis> ICrsGeodetic.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }
	}

	public class EpsgCrsGeocentric : EpsgCrsGeodetic, ICrsGeocentric
	{
		internal EpsgCrsGeocentric(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
			: base(code, name, area, deprecated, cs, geodeticDatum) {}
	}

	public class EpsgCrsGeographic : EpsgCrsGeodetic, ICrsGeographic
	{
		internal EpsgCrsGeographic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
			: base(code, name, area, deprecated, cs, geodeticDatum) { }
	}

}
