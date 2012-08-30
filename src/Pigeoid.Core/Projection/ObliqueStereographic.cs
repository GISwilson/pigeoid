﻿// TODO: source header

using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class ObliqueStereographic : ProjectionBase
	{

		private class Inverted : InvertedTransformationBase<ObliqueStereographic,Point2,GeographicCoordinate>
		{

			public Inverted(ObliqueStereographic core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 source) {
				throw new NotImplementedException();
			}
		}


		public ObliqueStereographic(ISpheroid<double> spheroid)
			: base(Vector2.Zero, spheroid) { }

		public override Point2 TransformValue(GeographicCoordinate source) {
			throw new NotImplementedException();
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return true; }
		}

		public override string Name {
			get { return "Oblique Stereographic"; }
		}
	}
}
