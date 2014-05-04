﻿using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class KrovakModified : ProjectionBase
    {

        internal class Inverse : InvertedTransformationBase<KrovakModified, Point2, GeographicCoordinate>
        {
            private readonly ITransformation<Point2, GeographicCoordinate> _invertedCore;

            public Inverse(KrovakModified core) : base(core) {
                Contract.Requires(core != null);
                Contract.Requires(core.Core != null);
                Contract.Requires(core.Core.HasInverse);
                _invertedCore = core.Core.GetInverse();
            }

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(_invertedCore != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var r = value
                    .Difference(Core.FalseProjectedOffset)
                    .Difference(Core.EvaluationPoint);
                var r2 = new Vector2(r.X * r.X, r.Y * r.Y);
                var constant3 = r.X * r.Y * 2.0;
                var constant4 = r2.X - r2.Y;
                var constant5 = ((-3.0 * r2.Y) + r2.X) * r.X;
                var constant6 = ((3.0 * r2.X) - r2.Y) * r.Y;
                var constant7 = constant3 * 2.0 * constant4;
                var constant8 = (r2.X * r2.Y * -6.0) + (r2.X * r2.X) + (r2.Y * r2.Y);
                var delta = new Vector2(
                    Core.C1
                    + (Core.C3 * r.X)
                    - (Core.C4 * r.Y)
                    - (Core.C6 * constant3)
                    + (Core.C5 * constant4)
                    + (Core.C7 * constant5)
                    - (Core.C8 * constant6)
                    + (Core.C9 * constant7)
                    + (Core.C10 * constant8),
                    Core.C2
                    + (Core.C3 * r.Y)
                    + (Core.C4 * r.X)
                    + (Core.C5 * constant3)
                    + (Core.C6 * constant4)
                    + (Core.C8 * constant5)
                    + (Core.C7 * constant6)
                    - (Core.C10 * constant7)
                    + (Core.C9 * constant8)
                );
                var p = value.Difference(Core.FalseProjectedOffset).Add(delta);
                return _invertedCore.TransformValue(p);
            }
        }

        protected readonly Point2 EvaluationPoint;
        protected readonly Vector2 FalseProjectedOffset;
        protected readonly double C1;
        protected readonly double C2;
        protected readonly double C3;
        protected readonly double C4;
        protected readonly double C5;
        protected readonly double C6;
        protected readonly double C7;
        protected readonly double C8;
        protected readonly double C9;
        protected readonly double C10;

        public KrovakModified(
            GeographicCoordinate geographicOrigin,
            double latitudeOfPseudoStandardParallel,
            double azimuthOfInitialLine,
            double scaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid,
            Point2 evaluationPoint,
            double[] constants
        ) : this(new Krovak(
            geographicOrigin,
            latitudeOfPseudoStandardParallel,
            azimuthOfInitialLine,
            scaleFactor,
            Vector2.Zero,
            spheroid
            ), falseProjectedOffset, evaluationPoint, constants)
        {
            Contract.Requires(spheroid != null);
            Contract.Requires(constants != null);
            Contract.Requires(constants.Length == 10);
        }

        private KrovakModified(Krovak core, Vector2 falseProjectedOffset, Point2 evaluationPoint, double[] constants) {
            if (null == core) throw new ArgumentNullException("core");
            if (null == constants) throw new ArgumentNullException("constants");
            if (constants.Length != 10) throw new ArgumentException("10 constants required", "constants");
            Contract.EndContractBlock();

            Core = core;
            FalseProjectedOffset = falseProjectedOffset;
            EvaluationPoint = evaluationPoint;
            C1 = constants[0];
            C2 = constants[1];
            C3 = constants[2];
            C4 = constants[3];
            C5 = constants[4];
            C6 = constants[5];
            C7 = constants[6];
            C8 = constants[7];
            C9 = constants[8];
            C10 = constants[9];
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
        }

        protected Krovak Core { get; private set; }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var p = Core.TransformValue(source);
            var r = p.Difference(EvaluationPoint);
            var r2 = new Vector2(r.X * r.X, r.Y * r.Y);
            var constant3 = r.X * r.Y * 2.0;
            var constant4 = r2.X - r2.Y;
            var constant5 = ((-3.0 * r2.Y) + r2.X) * r.X;
            var constant6 = ((3.0 * r2.X) - r2.Y) * r.Y;
            var constant7 = constant3 * 2.0 * constant4;
            var constant8 = (r2.X * r2.Y * -6.0) + (r2.X * r2.X) + (r2.Y * r2.Y);
            var delta = new Vector2(
                C1
                + (C3 * r.X)
                - (C4 * r.Y)
                - (C6 * constant3)
                + (C5 * constant4)
                + (C7 * constant5)
                - (C8 * constant6)
                + (C9 * constant7)
                + (C10 * constant8),
                C2
                + (C3 * r.Y)
                + (C4 * r.X)
                + (C5 * constant3)
                + (C6 * constant4)
                + (C8 * constant5)
                + (C7 * constant6)
                - (C10 * constant7)
                + (C9 * constant8)
            );

            return p
                .Difference(delta)
                .Add(FalseProjectedOffset);
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            [Pure] get { return Core.HasInverse; }
        }
        
    }
}
