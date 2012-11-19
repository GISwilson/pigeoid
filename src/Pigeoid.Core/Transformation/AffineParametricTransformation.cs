﻿using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class AffineParametricTransformation : ITransformation<Point2>
	{

		private class Inverse : ITransformation<Point2>
		{

			private readonly AffineParametricTransformation _core;
			private readonly double _a0;
			private readonly double _a1;
			private readonly double _a2;
			private readonly double _b0;
			private readonly double _b1;
			private readonly double _b2;

			public Inverse([NotNull] AffineParametricTransformation core){
				_core = core;
				_a0 = ((_core._a2 * _core._b0)-(_core._b2 * _core._a0))/ _core._d;
				_a1 = _core._b2 / _core._d;
				_a2 = -_core._a2 / _core._d;
				_b0 = ((_core._b1 * _core._a0)-(_core._a1 * _core._b0)) / _core._d;
				_b1 = -_core._b1 / _core._d;
				_b2 = _core._a1 / _core._d;
			}

			public Point2 TransformValue(Point2 value) {
				return new Point2(
					_a0 + (_a1 * value.X) + (_a2 * value.Y),
					_b0 + (_b1 * value.X) + (_b2 * value.Y)
				);
			}

			public void TransformValues(Point2[] values){
				for (int i = 0; i < values.Length; i++)
					values[i] = TransformValue(values[i]);
			}

			public IEnumerable<Point2> TransformValues(IEnumerable<Point2> values){
				return values.Select(TransformValue);
			}

			public ITransformation<Point2> GetInverse(){
				return _core;
			}

			ITransformation<Point2, Point2> ITransformation<Point2, Point2>.GetInverse() {
				return _core;
			}

			ITransformation ITransformation.GetInverse() {
				return _core;
			}

			public bool HasInverse {
				get { return true; }
			}
		}

		private readonly double _a0;
		private readonly double _a1;
		private readonly double _a2;
		private readonly double _b0;
		private readonly double _b1;
		private readonly double _b2;
		private readonly double _d;

		public AffineParametricTransformation(double a0, double a1, double a2, double b0, double b1, double b2){
			_a0 = a0;
			_a1 = a1;
			_a2 = a2;
			_b0 = b0;
			_b1 = b1;
			_b2 = b2;
			_d = (a1 * b2) - (a2 * b1);
		}

		public Vector3 A { get { return new Vector3(_a0, _a1, _a2); } }

		public Vector3 B { get { return new Vector3(_b0, _b1, _b2); } }

		public Point2 TransformValue(Point2 value) {
			return new Point2(
				_a0 + (_a1 * value.X) + (_a2 * value.Y),
				_b0 + (_b1 * value.X) + (_b2 * value.Y)
			);
		}

		public void TransformValues([NotNull] Point2[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}

		[NotNull] public IEnumerable<Point2> TransformValues([NotNull] IEnumerable<Point2> values){
			return values.Select(TransformValue);
		}

		public ITransformation<Point2> GetInverse() {
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return new Inverse(this);
		}

		ITransformation<Point2, Point2> ITransformation<Point2, Point2>.GetInverse() {
			return GetInverse();
		}

		ITransformation ITransformation.GetInverse(){
			return GetInverse();
		}

		public bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != _d; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}
	}
}
