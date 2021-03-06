﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    public class AngularElementTransformation :
        ITransformation<Vector2>,
        ITransformation<Vector3>,
        ITransformation<GeographicCoordinate>,
        ITransformation<GeographicHeightCoordinate>
    {

        private readonly ITransformation<double> _core;

        public AngularElementTransformation(ITransformation<double> core) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            _core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_core != null);
        }

        public Type[] GetInputTypes() {
            return new [] {
                typeof(GeographicHeightCoordinate),
                typeof(GeographicCoordinate),
                typeof(Vector3),
                typeof(Vector2)
            };
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof (GeographicHeightCoordinate)
                || inputType == typeof (GeographicCoordinate)
                || inputType == typeof (Vector3)
                || inputType == typeof (Vector2)
                ? new[] {inputType}
                : ArrayUtil<Type>.Empty;
        }

        public object TransformValue(object value) {
            if (value is GeographicHeightCoordinate)
                return TransformValue((GeographicHeightCoordinate) value);
            if (value is GeographicCoordinate)
                return TransformValue((GeographicCoordinate) value);
            if (value is Vector3)
                return TransformValue((Vector3) value);
            if (value is Vector2)
                return TransformValue((Vector2) value);
            throw new InvalidOperationException();
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }

        public bool HasInverse {
            [Pure] get { return _core.HasInverse; }
        }

        public AngularElementTransformation GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<AngularElementTransformation>() != null);
            return new AngularElementTransformation(_core.GetInverse());
        }


        ITransformation<Vector2> ITransformation<Vector2>.GetInverse() {
            return GetInverse();
        }
        ITransformation<Vector2, Vector2> ITransformation<Vector2, Vector2>.GetInverse() {
            return GetInverse();
        }
        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }
        ITransformation<Vector3> ITransformation<Vector3>.GetInverse() {
            return GetInverse();
        }
        ITransformation<Vector3, Vector3> ITransformation<Vector3, Vector3>.GetInverse() {
            return GetInverse();
        }
        ITransformation<GeographicCoordinate> ITransformation<GeographicCoordinate>.GetInverse() {
            return GetInverse();
        }
        ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
            return GetInverse();
        }
        ITransformation<GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }
        ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        public Vector2 TransformValue(Vector2 value) {
            return new Vector2(
                _core.TransformValue(value.X),
                _core.TransformValue(value.Y));
        }
        public Vector3 TransformValue(Vector3 value) {
            return new Vector3(
                _core.TransformValue(value.X),
                _core.TransformValue(value.Y),
                _core.TransformValue(value.Z));
        }
        public GeographicCoordinate TransformValue(GeographicCoordinate value) {
            return new GeographicCoordinate(
                _core.TransformValue(value.Latitude),
                _core.TransformValue(value.Longitude));
        }
        public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate value) {
            return new GeographicHeightCoordinate(
                _core.TransformValue(value.Latitude),
                _core.TransformValue(value.Longitude),
                value.Height);
        }

        public void TransformValues(Vector2[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }
        public void TransformValues(Vector3[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }
        public void TransformValues(GeographicCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }
        public void TransformValues(GeographicHeightCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }

        public IEnumerable<Vector2> TransformValues(IEnumerable<Vector2> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Vector2>>() != null);
            return values.Select(TransformValue);
        }
        public IEnumerable<Vector3> TransformValues(IEnumerable<Vector3> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Vector3>>() != null);
            return values.Select(TransformValue);
        }
        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(TransformValue);
        }
        public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

    }
}
