﻿using System;
using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{

    public class GeocentricTransformationGeographicWrapper : GeographicGeocentricTransformationBase
    {

        private class Inverse : GeographicGeocentricTransformationBase
        {
            public static Inverse BuildInverse(GeocentricTransformationGeographicWrapper coreWrapper) {
                Contract.Requires(coreWrapper != null);
                Contract.Requires(coreWrapper.GeocentricToGeographic.HasInverse);
                Contract.Requires(coreWrapper.GeographicToGeocentric.HasInverse);
                Contract.Ensures(Contract.Result<Inverse>() != null);
                var geographicToGeocentric = coreWrapper.GeocentricToGeographic.GetInverse();
                Contract.Assume(coreWrapper.GeographicToGeocentric.HasInverse);
                var geocentricToGeographic = coreWrapper.GeographicToGeocentric.GetInverse();
                return new Inverse(geographicToGeocentric, geocentricToGeographic, coreWrapper);
            }

            private readonly GeocentricTransformationGeographicWrapper _coreWrapper;
            private readonly ITransformation<Point3> _inverseOperation;

            private Inverse(GeographicGeocentricTransformation geographicGeocentric, GeocentricGeographicTransformation geocentricGeographic, GeocentricTransformationGeographicWrapper coreWrapper)
                : base(geographicGeocentric, geocentricGeographic) {
                Contract.Requires(coreWrapper != null);
                Contract.Requires(geographicGeocentric != null);
                Contract.Requires(geocentricGeographic != null);
                _coreWrapper = coreWrapper;
                _inverseOperation = coreWrapper.GeocentricCore.GetInverse();
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_coreWrapper != null);
                Contract.Invariant(_inverseOperation != null);
            }

            public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
                return GeocentricToGeographic.TransformValue2D(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
                return GeocentricToGeographic.TransformValue(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
                return GeocentricToGeographic.TransformValue(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
                return GeocentricToGeographic.TransformValue2D(_inverseOperation.TransformValue(GeographicToGeocentric.TransformValue(value)));
            }

            public override ITransformation GetInverse() {
                return _coreWrapper;
            }

            public override bool HasInverse {
                get { return true; }
            }

        }

        private readonly ITransformation<Point3> _core;

        public GeocentricTransformationGeographicWrapper(GeographicGeocentricTransformation geographicGeocentric, GeocentricGeographicTransformation geocentricGeographic, ITransformation<Point3> core)
            : base(geographicGeocentric, geocentricGeographic) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.Requires(geographicGeocentric != null);
            Contract.Requires(geocentricGeographic != null);
            _core = core;
        }

        public GeocentricTransformationGeographicWrapper(ISpheroid<double> fromSpheroid, ISpheroid<double> toSpheroid, ITransformation<Point3> core)
            : base(fromSpheroid, toSpheroid) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.Requires(fromSpheroid != null);
            Contract.Requires(toSpheroid != null);
            _core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_core != null);
        }

        public ITransformation<Point3> GeocentricCore {
            get {
                Contract.Ensures(Contract.Result<ITransformation<Point3>>() != null);
                return _core;
            }
        }

        public override GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicHeightCoordinate TransformValue3D(GeographicCoordinate value) {
            return GeocentricToGeographic.TransformValue(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override GeographicCoordinate TransformValue2D(GeographicHeightCoordinate value) {
            return GeocentricToGeographic.TransformValue2D(_core.TransformValue(GeographicToGeocentric.TransformValue(value)));
        }

        public override bool HasInverse { get { return base.HasInverse && _core.HasInverse; } }

        public override ITransformation GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation>() != null);
            return Inverse.BuildInverse(this);
        }
    }
}
