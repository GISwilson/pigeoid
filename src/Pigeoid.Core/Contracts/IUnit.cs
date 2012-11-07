﻿// TODO: source header

using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A unit of measure.
	/// </summary>
	public interface IUnit
	{
		/// <summary>
		/// The name of the unit.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets the unit type or category for this unit of measure.
		/// </summary>
		string Type { get; }

		IUnitConversionMap<double> ConversionMap { get; }

	}
}
