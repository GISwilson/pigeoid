﻿// TODO: source header

using System;
using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// An angular unit of measure.
	/// </summary>
	public class OgcAngularUnit : OgcUnitBase
	{

		private static readonly OgcAngularUnit DefaultRadianInstance = new OgcAngularUnit("radian", 1, new AuthorityTag("EPSG", "9101"));
		private static readonly OgcAngularUnit DefaultDegreesInstance = new OgcAngularUnit("degree", Math.PI / 180.0, new AuthorityTag("EPSG", "9102"));
		private static readonly OgcAngularUnit DefaultGradsInstance = new OgcAngularUnit("grad", Math.PI / 200.0, new AuthorityTag("EPSG", "9105"));
		private static readonly OgcAngularUnit DefaultArcSecondInstance = new OgcAngularUnit("arc-second", Math.PI / 648000.0, new AuthorityTag("EPSG", "9104"));

		/// <summary>
		/// This is the OGC reference unit for angular measure.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultRadians { get { return DefaultRadianInstance; } }
		/// <summary>
		/// The default degree unit factored against radians.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultDegrees { get { return DefaultDegreesInstance; } }
		/// <summary>
		/// The default arc-second unit factored against radians.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultArcSeconds { get { return DefaultArcSecondInstance; } }
		/// <summary>
		/// The default grad unit factored against radians.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static OgcAngularUnit DefaultGrads { get { return DefaultGradsInstance; } }

		/// <summary>
		/// Constructs a new unit.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="factor">The conversion factor to the base unit.</param>
		public OgcAngularUnit(string name, double factor)
			: base(name, factor) { }

		/// <summary>
		/// Constructs a new unit.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="factor">The conversion factor to the base unit.</param>
		/// <param name="authority">The authority.</param>
		public OgcAngularUnit(string name, double factor, IAuthorityTag authority)
			: base(name, factor, authority) { }

		public override string Type { get { return "angle"; } }

		public override IUnit ReferenceUnit {
			get { return DefaultRadians; }
		}

	}
}
