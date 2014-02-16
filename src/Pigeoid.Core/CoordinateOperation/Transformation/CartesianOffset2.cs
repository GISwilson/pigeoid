﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class CartesianOffset2 : ITransformation<Point2>
    {

        public readonly Vector2 Offset;

        public CartesianOffset2(Vector2 offset) {
            Offset = offset;
        }

        public void TransformValues(Point2[] values) {
            for (int i = 0; i < values.Length; i++) {
                TransformValue(ref values[i]);
            }
        }

        private void TransformValue(ref Point2 value) {
            value = value.Add(Offset);
        }

        public CartesianOffset2 GetInverse() {
            Contract.Ensures(Contract.Result<CartesianOffset2>() != null);
            return new CartesianOffset2(Offset.GetNegative());
        }

        ITransformation<Point2> ITransformation<Point2>.GetInverse() {
            return GetInverse();
        }

        public Point2 TransformValue(Point2 value) {
            return value.Add(Offset);
        }

        public IEnumerable<Point2> TransformValues(IEnumerable<Point2> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(Offset.Add);
        }

        public bool HasInverse {
            get { return true; }
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        ITransformation<Point2, Point2> ITransformation<Point2, Point2>.GetInverse() {
            return GetInverse();
        }

        public Type[] GetInputTypes() {
            return new[] {typeof(Point2)};
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(Point2)
                ? new[]{typeof(Point2)}
                : ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            return TransformValue((Point2) value);
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            return values.Select(TransformValue);
        }
    }
}
