﻿using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    /// <summary>
    /// A Mercator projection, 1SP or 2SP.
    /// </summary>
    public class Mercator :
        ProjectionBase,
        IEquatable<Mercator>
    {

        private class Inverted : InvertedTransformationBase<Mercator, Point2, GeographicCoordinate>
        {

            public Inverted(Mercator core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 coordinate) {
                var x = (
                        Math.Atan(Math.Pow(
                            Math.E,
                            (Core.FalseProjectedOffset.Y - coordinate.Y) / Core.Ak
                        )) * -2.0
                    )
                    + HalfPi;
                return new GeographicCoordinate(
                    (Math.Sin(2.0 * x) * Core.LatLineCoefficient1)
                    + (Math.Sin(4.0 * x) * Core.LatLineCoefficient2)
                    + (Math.Sin(6.0 * x) * Core.LatLineCoefficient3)
                    + (Math.Sin(8.0 * x) * Core.LatLineCoefficient4)
                    + x,
                    (
                        (coordinate.X - Core.FalseProjectedOffset.X)
                        / Core.Ak
                    ) + Core.CentralMeridian
                );
            }

        }

        public static Mercator ConstructVariantC(
            GeographicCoordinate falseOrigin,
            Vector2 offsetAtFalseOrigin,
            ISpheroid<double> spheroid
        ) {
            Contract.Requires(spheroid != null);
            var sinLat = Math.Sin(falseOrigin.Latitude);
            var eSinLat = spheroid.E * sinLat;
            var eHalf = spheroid.E / 2.0;
            var scaleFactor = CalculateScaleFactor(falseOrigin.Latitude, spheroid.ESquared);
            var m = spheroid.A
                * scaleFactor
                * Math.Log(
                    Math.Tan((falseOrigin.Latitude / 2.0) + QuarterPi)
                    * Math.Pow((1.0 - eSinLat) / (1.0 + eSinLat), eHalf)
                );
            var falseProjectedOffset = new Vector2(
                offsetAtFalseOrigin.X,
                offsetAtFalseOrigin.Y - m);
            return new Mercator(
                falseOrigin.Longitude,
                scaleFactor,
                falseProjectedOffset,
                spheroid);
        }

        private static double CalculateScaleFactor(double latitude, double eSquared) {
            var sinLat = Math.Sin(latitude);
            return Math.Cos(latitude)
                / Math.Sqrt(1.0 - (sinLat * sinLat * eSquared));
        }

        protected readonly double CentralMeridian;
        protected readonly double ScaleFactor;
        protected readonly double Ak;
        protected readonly double LatLineCoefficient1;
        protected readonly double LatLineCoefficient2;
        protected readonly double LatLineCoefficient3;
        protected readonly double LatLineCoefficient4;

        /// <summary>
        /// Constructs a Mercator projection from 2 standard parallels.
        /// </summary>
        /// <param name="geographicOrigin">The geographic origin.</param>
        /// <param name="falseProjectedOffset">The false projected offset.</param>
        /// <param name="spheroid">The spheroid.</param>
        public Mercator(
            GeographicCoordinate geographicOrigin,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : this(
            geographicOrigin.Longitude,
            CalculateScaleFactor(geographicOrigin.Latitude, spheroid.ESquared),
            falseProjectedOffset,
            spheroid
        ) {
            Contract.Requires(spheroid != null);
        }

        /// <summary>
        /// Constructs a Mercator projection from 1 standard parallel.
        /// </summary>
        /// <param name="centralMeridian">The central meridian.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="falseProjectedOffset">The false projected offset.</param>
        /// <param name="spheroid">The spheroid.</param>
        public Mercator(
            double centralMeridian,
            double scaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            CentralMeridian = centralMeridian;
            ScaleFactor = scaleFactor;
            Ak = MajorAxis * scaleFactor;
            var e4 = ESq * ESq;
            var e6 = e4 * ESq;
            var e8 = e6 * ESq;
            LatLineCoefficient1 = (ESq / 2.0) + (5 / 24.0 * e4) + (e6 / 12.0) + (13.0 / 360.0 * e8);
            LatLineCoefficient2 = (7.0 / 48.0 * e4) + (29.0 / 240.0 * e6) + (811.0 / 11520.0 * e8);
            LatLineCoefficient3 = (7.0 / 120.0 * e6) + (81.0 / 1120.0 * e8);
            LatLineCoefficient4 = (4279 / 161280.0 * e8);
        }

        public override Point2 TransformValue(GeographicCoordinate coordinate) {
            var eSinLat = Math.Sin(coordinate.Latitude) * E;
            return new Point2(
                ((coordinate.Longitude - CentralMeridian) * Ak) + FalseProjectedOffset.X,
                (Math.Log(
                    Math.Tan((coordinate.Latitude / 2.0) + QuarterPi)
                    * Math.Pow((1.0 - eSinLat) / (1.0 + eSinLat), EHalf)
                ) * Ak) + FalseProjectedOffset.Y
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if(!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get { return 0 != Ak && !Double.IsNaN(Ak); }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(Mercator other) {
            if (ReferenceEquals(this, other))
                return true;
            return !ReferenceEquals(other, null)
                && (
                // ReSharper disable CompareOfFloatsByEqualityOperator
                    CentralMeridian == other.CentralMeridian
                    && ScaleFactor == other.ScaleFactor
                // ReSharper restore CompareOfFloatsByEqualityOperator
                    && FalseProjectedOffset.Equals(other.FalseProjectedOffset)
                    && Spheroid.Equals(other.Spheroid)
                )
            ;
        }

        public override bool Equals(object obj) {
            return Equals(obj as Mercator);
        }

        public override int GetHashCode() {
            return -CentralMeridian.GetHashCode() ^ -ScaleFactor.GetHashCode();
        }

    }
}
