﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public class EpsgCrsGeodetic : EpsgCrsDatumBased, ICrsGeodetic
    {

        internal EpsgCrsGeodetic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
            : base(code, name, area, deprecated, cs) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
            GeodeticDatum = geodeticDatum;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(GeodeticDatum != null);
        }

        public override EpsgDatum Datum {
            get {
                Contract.Ensures(Contract.Result<EpsgDatum>() != null);
                return GeodeticDatum;
            }
        }

        public EpsgDatumGeodetic GeodeticDatum { get; private set; }

        IDatumGeodetic ICrsGeodetic.Datum { get { return GeodeticDatum; } }

        public EpsgUnit Unit {
            get {
                Contract.Ensures(Contract.Result<EpsgUnit>() != null);
                var axes = CoordinateSystem.Axes;
                if (axes.Count == 0)
                    throw new NotImplementedException();
                Contract.Assume(axes[0] != null);
                return axes[0].Unit;
            }
        }

        IUnit ICrsGeodetic.Unit { get { return Unit; } }

        public IList<EpsgAxis> Axes {
            get {
                Contract.Ensures(Contract.Result<IList<EpsgAxis>>() != null);
                return CoordinateSystem.Axes.ToArray();
            }
        }

        IList<IAxis> ICrsGeodetic.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }
    }

    public class EpsgCrsGeocentric : EpsgCrsGeodetic, ICrsGeocentric
    {
        internal EpsgCrsGeocentric(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
            : base(code, name, area, deprecated, cs, geodeticDatum) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
        }
    }

    public class EpsgCrsGeographic : EpsgCrsGeodetic, ICrsGeographic
    {
        internal EpsgCrsGeographic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum)
            : base(code, name, area, deprecated, cs, geodeticDatum) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
        }
    }

}
