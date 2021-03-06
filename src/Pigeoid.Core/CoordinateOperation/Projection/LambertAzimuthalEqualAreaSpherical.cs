﻿using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class LambertAzimuthalEqualAreaSpherical :
        SpheroidProjectionBase,
        IEquatable<LambertAzimuthalEqualAreaSpherical>
    {

        private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualAreaSpherical, Point2, GeographicCoordinate>
        {

            private readonly double _r2;

            public Inverted(LambertAzimuthalEqualAreaSpherical core) : base(core) {
                Contract.Requires(core != null);
                _r2 = core.R * 2.0;
            }

            public override GeographicCoordinate TransformValue(Point2 source) {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                var p = Math.Sqrt((source.X * source.X) + (source.Y * source.Y));
                if (0 == p)
                    return Core.GeographicOrigin;

                var c = 2.0 * Math.Asin(p / (_r2));
                var cosC = Math.Cos(c);
                var sinC = Math.Sin(c);
                var lat = Math.Asin(
                    (cosC * Core._sinLatOrg)
                    + ((source.Y * sinC * Core._cosLatOrg) / p)
                );
                double longitude;
                if (Core.GeographicOrigin.Latitude == HalfPi)
                    longitude = Core.GeographicOrigin.Longitude + Math.Atan2(source.X, -source.Y);
                else if (Core.GeographicOrigin.Latitude == -HalfPi)
                    longitude = Core.GeographicOrigin.Longitude + Math.Atan2(source.X, source.Y);
                else
                    longitude = Core.GeographicOrigin.Longitude + Math.Atan2(
                        source.X * sinC,
                        (p * Core._cosLatOrg * cosC) - (source.Y * Core._sinLatOrg * sinC)
                    );
                return new GeographicCoordinate(lat, longitude);
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }


        public readonly double R;
        public readonly GeographicCoordinate GeographicOrigin;
        private readonly double _sinLatOrg;
        private readonly double _cosLatOrg;

        public LambertAzimuthalEqualAreaSpherical(
            GeographicCoordinate geogOrigin,
            Vector2 falseOffset,
            ISpheroid<double> spheroid
        ) : base(falseOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geogOrigin;

            var oneMinusESq = 1 - ESq;

            if (E == 0) {
                R = MajorAxis;
            }
            else {
                R = MajorAxis * Math.Sqrt(
                    (
                        1.0
                        - (
                            (oneMinusESq / (2.0 * E))
                            * Math.Log((1 - E) / (1 + E))
                        )
                    ) / 2.0
                );
            }

            _sinLatOrg = Math.Sin(geogOrigin.Latitude);
            _cosLatOrg = Math.Cos(geogOrigin.Latitude);
        }


        public override Point2 TransformValue(GeographicCoordinate source) {
            double deltaLon = source.Longitude - GeographicOrigin.Longitude;
            double cosLat = Math.Cos(source.Latitude);
            double cosDeltaLonCosLat = Math.Cos(deltaLon) * cosLat;
            double sinLat = Math.Sin(source.Latitude);
            double rk = R * Math.Sqrt(
                2.0 /
                (1.0 + (_sinLatOrg * sinLat) + (_cosLatOrg * cosDeltaLonCosLat))
            );
            double x = rk * cosLat * Math.Sin(deltaLon);
            double y = rk * ((_cosLatOrg * sinLat) - (_sinLatOrg * cosDeltaLonCosLat));
            return new Point2(x, y);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get { return 0 != R && !Double.IsNaN(R); }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public bool Equals(LambertAzimuthalEqualAreaSpherical other) {
            return !ReferenceEquals(other, null)
                && (
                    GeographicOrigin.Equals(other.GeographicOrigin)
                    && FalseProjectedOffset.Equals(other.FalseProjectedOffset)
                    && Spheroid.Equals(other.Spheroid)
                );
        }

        public override bool Equals(object obj) {
            return null != obj && (
                ReferenceEquals(this, obj)
                || Equals(obj as LambertAzimuthalEqualAreaSpherical));
        }

        public override int GetHashCode() {
            return -GeographicOrigin.GetHashCode() ^ FalseProjectedOffset.GetHashCode();
        }

    }
}
