﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;
using DotSpatial.Projections;
using Pigeoid.Ogc;
using Pigeoid.Unit;
using Vertesaur;
using System.Collections.Generic;

namespace Pigeoid.Interop.Proj4
{
    public class Proj4SpheroidWrapper : OgcNamedAuthorityBoundEntity, ISpheroidInfo
    {

        private static Spheroid[] AllKnownSpheroids;

        static Proj4SpheroidWrapper() {
            var allSpheroidEnums = (Proj4Ellipsoid[])Enum.GetValues(typeof(Proj4Ellipsoid));
            AllKnownSpheroids = allSpheroidEnums
                .Where(x => x != Proj4Ellipsoid.Custom)
                .Select(known => new Spheroid(known))
                .ToArray();
        }

        public static Spheroid Create(ISpheroidInfo spheroidInfo) {
            if(spheroidInfo == null) throw new ArgumentNullException("spheroidInfo");
            Contract.Ensures(Contract.Result<Spheroid>() != null);

            var aMeters = spheroidInfo.A;
            var bMeters = spheroidInfo.B;

            if (spheroidInfo.AxisUnit != null) {
                var conversion = SimpleUnitConversionGenerator.FindConversion(spheroidInfo.AxisUnit, Proj4LinearUnit.Meter);
                if (conversion != null && !(conversion is UnitUnityConversion)) {
                    aMeters = conversion.TransformValue(aMeters);
                    bMeters = conversion.TransformValue(bMeters);
                }
            }

            Spheroid result;
            if (aMeters == bMeters) {
                var knownMatch = AllKnownSpheroids
                    .Where(k => k.PolarRadius == aMeters)
                    .OrderByDescending(k => SpheroidNameNormalizedComparer.Default.Equals(k.Name, spheroidInfo.Name))
                    .FirstOrDefault();
                if (knownMatch != null)
                    return knownMatch;
                
                result = new Spheroid(Proj4Ellipsoid.Custom);
                result.PolarRadius = aMeters;
            }
            else{
                var knownMatch = AllKnownSpheroids
                    .Where(k => k.EquatorialRadius == aMeters && (k.InverseFlattening == spheroidInfo.InvF || k.PolarRadius == bMeters))
                    .OrderByDescending(k => SpheroidNameNormalizedComparer.Default.Equals(k.Name, spheroidInfo.Name))
                    .FirstOrDefault();
                if (knownMatch != null)
                    return knownMatch;

                result = new Spheroid(Proj4Ellipsoid.Custom);
                result.EquatorialRadius = aMeters;
                // NOTE: do not directly set the InverseFlattening as it is stored as PolarRadius
                result.PolarRadius = bMeters;
            }
            result.Name = spheroidInfo.Name;
            return result;
        }

        public Proj4SpheroidWrapper(Spheroid spheroid)
            : base(spheroid.Name ?? "Unknown", new AuthorityTag("PROJ4", spheroid.Code)) {
            Contract.Requires(spheroid != null);
            Core = spheroid;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
        }

        protected Spheroid Core { get; private set; }

        public IUnit AxisUnit { get { return OgcLinearUnit.DefaultMeter; } }

        public double A {
            get { return Core.EquatorialRadius; }
        }

        public double B {
            get { return Core.PolarRadius; }
        }

        public double E {
            get { return Math.Sqrt(2.0 * InvF - 1.0) / InvF; }
        }

        public double ESecond {
            get { return Math.Sqrt(2.0 * InvF - 1.0) / (InvF - 1.0); }
        }

        public double ESecondSquared {
            get {
                var num = InvF - 1.0;
                return (2.0 * InvF - 1.0) / (num * num);
            }
        }

        public double ESquared {
            get {
                return (2.0 * InvF - 1.0) / (InvF * InvF);
            }
        }

        public double F {
            get { return 1.0 / InvF; }
        }

        public double InvF {
            get { return Core.InverseFlattening; }
        }
    }
}
