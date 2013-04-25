﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pigeoid.Interop
{
    public static class ParameterStandardNames
    {

        [Obsolete]
        private static readonly ReadOnlyCollection<string> GenericNamesCoreList;

        // TODO: I don't know if exposed fields for the names is the best idea.

        /// <summary>
        /// Angle from Rectified to Skew Grid.
        /// </summary>
        public static readonly string NameAngleFromRectifiedToSkewGrid = "Angle from Rectified to Skew Grid";
        /// <summary>
        /// Azimuth of initial line.
        /// </summary>
        public static readonly string NameAzimuthOfInitialLine = "Azimuth of initial line";
        /// <summary>
        /// Azimuth of the center line.
        /// </summary>
        public static readonly string NameAzimuthOfCenterLine = "Azimuth of the center line";
        /// <summary>
        /// Central Meridian.
        /// </summary>
        public static readonly string NameCentralMeridian = "Central Meridian";
        /// <summary>
        /// False Easting.
        /// </summary>
        public static readonly string NameFalseEasting = "False Easting";
        /// <summary>
        /// False Northing.
        /// </summary>
        public static readonly string NameFalseNorthing = "False Northing";
        /// <summary>
        /// Easting at projection center.
        /// </summary>
        public static readonly string NameEastingAtProjectionCenter = "Easting at projection center";
        /// <summary>
        /// Easting of false origin.
        /// </summary>
        public static readonly string NameEastingOfFalseOrigin = "Easting of false origin";
        /// <summary>
        /// Latitude of false origin.
        /// </summary>
        public static readonly string NameLatitudeOfFalseOrigin = "Latitude of false origin";
        /// <summary>
        /// Latitude of first standard parallel.
        /// </summary>
        public static readonly string NameLatitudeOfFirstStandardParallel = "Latitude of first standard parallel";
        /// <summary>
        /// Latitude of natural origin.
        /// </summary>
        public static readonly string NameLatitudeOfNaturalOrigin = "Latitude of natural origin";
        /// <summary>
        /// Latitude of origin.
        /// </summary>
        public static readonly string NameLatitudeOfOrigin = "Latitude of origin";
        /// <summary>
        /// Latitude of projection center.
        /// </summary>
        public static readonly string NameLatitudeOfProjectionCenter = "Latitude of projection center";
        /// <summary>
        /// Latitude of Pseudo Standard Parallel.
        /// </summary>
        public static readonly string NameLatitudeOfPseudoStandardParallel = "Latitude of Pseudo Standard Parallel";
        /// <summary>
        /// Latitude of second standard parallel.
        /// </summary>
        public static readonly string NameLatitudeOfSecondStandardParallel = "Latitude of second standard parallel";
        /// <summary>
        /// Latitude of true scale.
        /// </summary>
        public static readonly string NameLatitudeOfTrueScale = "Latitude of true scale";
        /// <summary>
        /// Longitude of false origin.
        /// </summary>
        public static readonly string NameLongitudeOfFalseOrigin = "Longitude of false origin";
        /// <summary>
        /// Longitude of natural origin.
        /// </summary>
        public static readonly string NameLongitudeOfNaturalOrigin = "Longitude of natural origin";
        /// <summary>
        /// Longitude of projection center.
        /// </summary>
        public static readonly string NameLongitudeOfProjectionCenter = "Longitude of projection center";
        /// <summary>
        /// Northing at projection center.
        /// </summary>
        public static readonly string NameNorthingAtProjectionCenter = "Northing at projection center";
        /// <summary>
        /// Northing of false origin.
        /// </summary>
        public static readonly string NameNorthingOfFalseOrigin = "Northing of false origin";
        /// <summary>
        /// Rectified grid angle.
        /// </summary>
        public static readonly string NameRectifiedGridAngle = "Rectified grid angle";
        /// <summary>
        /// Satellite Height.
        /// </summary>
        public static readonly string NameSatelliteHeight = "Satellite Height";
        /// <summary>
        /// Scale factor at natural origin.
        /// </summary>
        public static readonly string NameScaleFactorAtNaturalOrigin = "Scale factor at natural origin";
        /// <summary>
        /// Scale factor on initial line.
        /// </summary>
        public static readonly string NameScaleFactorOnInitialLine = "Scale factor on initial line";
        /// <summary>
        /// Scale factor on the pseudo standard line.
        /// </summary>
        public static readonly string NameScaleFactorOnPseudoStandardLine = "Scale factor on the pseudo standard line";
        /// <summary>
        /// Standard Parallel.
        /// </summary>
        public static readonly string NameStandardParallel = "Standard Parallel";

        private static readonly HashSet<string> AllNames;
        private static readonly HashSet<string> NormalizedNames;

        static ParameterStandardNames() {
            AllNames = new HashSet<string> {
                NameAngleFromRectifiedToSkewGrid,
                NameAzimuthOfInitialLine,
                NameAzimuthOfCenterLine,
                NameCentralMeridian,
                NameEastingAtProjectionCenter,
                NameEastingOfFalseOrigin,
                NameFalseEasting,
                NameFalseNorthing,
                NameLatitudeOfFalseOrigin,
                NameLatitudeOfFirstStandardParallel,
                NameLatitudeOfNaturalOrigin,
                NameLatitudeOfOrigin,
                NameLatitudeOfProjectionCenter,
                NameLatitudeOfPseudoStandardParallel,
                NameLatitudeOfSecondStandardParallel,
                NameLatitudeOfTrueScale,
                NameLongitudeOfFalseOrigin,
                NameLongitudeOfNaturalOrigin,
                NameLongitudeOfProjectionCenter,
                NameNorthingAtProjectionCenter,
                NameNorthingOfFalseOrigin,
                NameRectifiedGridAngle,
                NameSatelliteHeight,
                NameScaleFactorAtNaturalOrigin,
                NameScaleFactorOnInitialLine,
                NameScaleFactorOnPseudoStandardLine,
                NameStandardParallel
            };
            NormalizedNames = new HashSet<string>(AllNames.Select(NameNormalizedComparerBase.NormalizeBasic), StringComparer.OrdinalIgnoreCase);
        }

        internal static bool IsNormalizedName(string text) {
            return NormalizedNames.Contains(text);
        }

    }
}
