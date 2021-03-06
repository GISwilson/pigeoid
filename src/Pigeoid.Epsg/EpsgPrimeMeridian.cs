﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.Epsg.Resources;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public class EpsgPrimeMeridian : IPrimeMeridianInfo
    {

        internal EpsgPrimeMeridian(ushort code, string name, double longitude, EpsgUnit unit) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(unit != null);
            Code = code;
            Unit = unit;
            Longitude = longitude;
            Name = name;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(Unit != null);
        }

        public int Code { get; private set; }

        public string Name { get; private set; }

        public double Longitude { get; private set; }

        public EpsgUnit Unit { get; private set; }

        IUnit IPrimeMeridianInfo.Unit { get { return Unit; } }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(Code);
            }
        }

    }
}
