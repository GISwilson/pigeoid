﻿using System;
using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    [ContractClass(typeof(LambertConicBaseCodeContracts))]
    public abstract class LambertConicBase : ProjectionBase
    {

        /// <summary>
        /// The geographic origin of the projection.
        /// </summary>
        protected readonly GeographicCoordinate GeographicOrigin;

        protected LambertConicBase(
            GeographicCoordinate geographicOrigin,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geographicOrigin;
        }

        public abstract override Point2 TransformValue(GeographicCoordinate source);

        public abstract override ITransformation<Point2, GeographicCoordinate> GetInverse();

        public abstract override bool HasInverse { [Pure] get; }

    }

    [ContractClassFor(typeof(LambertConicBase))]
    internal class LambertConicBaseCodeContracts : LambertConicBase
    {

        internal LambertConicBaseCodeContracts() : base(
            default(GeographicCoordinate),
            default(Vector2),
            default(ISpheroid<double>)) { }

        public override Point2 TransformValue(GeographicCoordinate source) {
            throw new System.NotImplementedException();
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if(!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            Contract.EndContractBlock();
            throw new NotImplementedException();
        }

        public override bool HasInverse {
            [Pure] get { throw new System.NotImplementedException(); }
        }
    }

}
