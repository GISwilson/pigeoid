﻿using System;
using JetBrains.Annotations;

namespace Pigeoid.Interop
{
	public class CoordinateOperationNameNormalizedComparer : NameNormalizedComparerBase
	{

		public static readonly CoordinateOperationNameNormalizedComparer Default = new CoordinateOperationNameNormalizedComparer();

		public CoordinateOperationNameNormalizedComparer() : this(null) { }

		public CoordinateOperationNameNormalizedComparer(StringComparer comparer) : base(comparer) { }

		[ContractAnnotation("=>notnull")]
		public override string Normalize(string text) {
			if (null == text)
				return String.Empty;

			text = base.Normalize(text);

			// take the 'orientated' off the end
			if (text.EndsWith("NORTHORIENTATED"))
				text = text.Substring(0, text.Length - 10);

			text = text.Replace("LONGITUDEROTATION", "GEOGRAPHICOFFSET");
			if (text.EndsWith("OFFSETS"))
				text = text.Substring(0, text.Length - 1);

			if (CoordinateOperationStandardNames.IsNormalizedName(text))
				return text;

			var conicFlipText = text.Replace("CONFORMALCONIC", "CONICCONFORMAL");
			if (CoordinateOperationStandardNames.IsNormalizedName(conicFlipText))
				return conicFlipText;

			if(text.EndsWith("AREA")) {
				var areaConicText = text + "CONIC";
				if (CoordinateOperationStandardNames.IsNormalizedName(areaConicText))
					return areaConicText;
			}

			return text;
		}

	}
}
