﻿// TODO: source header

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public struct GeographicCoordinate :
		IGeographicCoordinate<double>,
		IEquatable<GeographicCoordinate>,
		IComparable<GeographicCoordinate>
	{

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">A coordinate.</param>
		/// <param name="b">A coordinate.</param>
		/// <returns>The result of the operator.</returns>
		[Pure] public static bool operator ==(GeographicCoordinate a, GeographicCoordinate b) {
			return a.Equals(b);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">A coordinate.</param>
		/// <param name="b">A coordinate.</param>
		/// <returns>The result of the operator.</returns>
		[Pure] public static bool operator !=(GeographicCoordinate a, GeographicCoordinate b) {
			return !a.Equals(b);
		}

		/// <summary>
		/// The latitude component of the coordinate.
		/// </summary>
		public readonly double Latitude;

		/// <summary>
		/// The longitude component of the coordinate.
		/// </summary>
		public readonly double Longitude;

		/// <summary>
		/// Creates a new geographical coordinate.
		/// </summary>
		/// <param name="latitude">The latitude.</param>
		/// <param name="longitude">The longitude.</param>
		public GeographicCoordinate(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		/// <inheritdoc/>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		double IGeographicCoordinate<double>.Latitude {
			[Pure] get { return Latitude; }
		}

		/// <inheritdoc/>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		double IGeographicCoordinate<double>.Longitude {
			[Pure] get { return Longitude; }
		}

		/// <inheritdoc/>
		[Pure] public int CompareTo(GeographicCoordinate other) {
			int c = Longitude.CompareTo(other.Longitude);
			return 0 == c ? Latitude.CompareTo(other.Latitude) : c;
		}

		/// <inheritdoc/>
		[Pure] public bool Equals(GeographicCoordinate other) {
// ReSharper disable CompareOfFloatsByEqualityOperator
			return Latitude == other.Latitude && Longitude == other.Longitude;
	// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <inheritdoc/>
		[Pure, ContractAnnotation("null=>false")]
		public override bool Equals(object obj) {
			return obj is GeographicCoordinate && Equals((GeographicCoordinate) obj);
		}

		/// <inheritdoc/>
		[Pure] public override int GetHashCode() {
			return Longitude.GetHashCode();
		}

	}
}
