﻿// TODO: source header

using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class TransverseMercatorZoned : ProjectionBase
	{

		private class Inverted : InvertedTransformationBase<TransverseMercatorZoned,Point2,GeographicCoord>
		{

			public Inverted(TransverseMercatorZoned core)
				: base(core) { }

			public override GeographicCoord TransformValue(Point2 source) {
				throw new NotImplementedException();
			}
		}

		public TransverseMercatorZoned(ISpheroid<double> spheroid)
			: base(Vector2.Zero, spheroid) { }

		public override Point2 TransformValue(GeographicCoord source) {
			throw new NotImplementedException();
		}

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return true; }
		}

		public override string Name {
			get { return "Transverse Mercator Zoned Grid System"; }
		}
	}
}
