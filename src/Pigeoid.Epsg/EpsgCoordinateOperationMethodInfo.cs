﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
    public class EpsgCoordinateOperationMethodInfo : ICoordinateOperationMethodInfo
    {

        /*[Obsolete]
        private class EpsgCoordinateOperationMethodParamInfoLookUp
        {

            [Obsolete]
            private class OpParamValueDynamicLookUp : EpsgDynamicLookUpBase<ushort, OpParamValueInfo>
            {

                private readonly EpsgCoordinateOperationMethodParamInfoLookUp _parent;
                private readonly int _valueDataOffset;
                private readonly int _opDataSize;

                internal OpParamValueDynamicLookUp(EpsgCoordinateOperationMethodParamInfoLookUp parent, ushort[] opCodes)
                    : base(opCodes) {
                    Contract.Requires(parent != null);
                    Contract.Requires(parent._paramUsage != null);
                    Contract.Requires(opCodes != null);
                    _parent = parent;
                    _valueDataOffset = sizeof(byte) // usage count
                        + ((sizeof(ushort) + sizeof(byte)) * _parent._paramUsage.Length) // usage data
                        + sizeof(ushort); // op count
                    _opDataSize = sizeof(ushort) // op code
                        + ((sizeof(ushort) * 2) * _parent._paramUsage.Length); // values
                }

                [ContractInvariantMethod]
                private void CodeContractInvariants() {
                    Contract.Invariant(_parent != null);
                    Contract.Invariant(_parent._paramUsage != null);
                }

                protected override OpParamValueInfo Create(ushort key, int index) {
                    Contract.Ensures(Contract.Result<OpParamValueInfo>() != null);
                    using (var numberLookUp = new EpsgNumberLookUp())
                    using (var readerTxt = EpsgDataResource.CreateBinaryReader(ParamTextValueFileName))
                    using (var reader = EpsgDataResource.CreateBinaryReader(_parent._paramDatFileName)) {
                        reader.BaseStream.Seek(
                            _valueDataOffset // header and usage data
                            + (index * _opDataSize) // previous op data
                            + sizeof(ushort) // current opCode
                            , SeekOrigin.Begin);
                        var paramValues = new List<INamedParameter>(_parent._paramUsage.Length);
                        Contract.Assume(Contract.ForAll(_parent._paramUsage, pu => pu != null)); // invariant not found from EpsgCoordinateOperationMethodParamInfoLookUp._paramUsage
                        for (int i = 0; i < _parent._paramUsage.Length; i++) {
                            var valueCode = reader.ReadUInt16();
                            var uomCode = reader.ReadUInt16();
                            if (valueCode != 0xffff) {
                                var paramName = _parent._paramUsage[i].Parameter.Name;
                                EpsgUnit uom;
                                if (uomCode == UInt16.MaxValue) {
                                    uom = null;
                                }
                                else {
                                    uom = EpsgUnit.Get(uomCode);
                                    Contract.Assume(uom != null);
                                }
                                paramValues.Add(CreateParameter(valueCode, paramName, uom, readerTxt, numberLookUp));
                            }
                        }
                        return new OpParamValueInfo(key, paramValues.ToArray());
                    }
                }

                protected override ushort GetKeyForItem(OpParamValueInfo value) {
                    return (ushort)value.OpCode;
                }
            }

            private const string ParamTextValueFileName = "params.txt";
            private const int FixedLookUpThreshold = 8;

            private static INamedParameter CreateParameter(ushort valueCode, string paramName, EpsgUnit unit, BinaryReader readerTxt, EpsgNumberLookUp numberLookUp) {
                Contract.Requires(!String.IsNullOrEmpty(paramName));
                Contract.Requires(unit != null);
                Contract.Requires(readerTxt != null);
                Contract.Requires(numberLookUp != null);
                Contract.Ensures(Contract.Result<INamedParameter>() != null);
                return ((valueCode & 0xc000) == 0x8000)
                    ? new NamedParameter<string>(paramName,
                        EpsgTextLookUp.GetString((ushort)(valueCode & 0x7fff), readerTxt),
                        unit) as INamedParameter
                    : new NamedParameter<double>(paramName, numberLookUp.Get(valueCode), unit);
            }

            private static EpsgFixedLookUpBase<ushort, OpParamValueInfo> CreateFullLookUp(BinaryReader reader, int opCount, ParamUsage[] paramUsage) {
                Contract.Requires(reader != null);
                Contract.Requires(paramUsage != null);
                Contract.Requires(Contract.ForAll(paramUsage, pu => pu != null));
                Contract.Ensures(Contract.Result<EpsgFixedLookUpBase<ushort, OpParamValueInfo>>() != null);
                var lookUpDictionary = new SortedDictionary<ushort, OpParamValueInfo>();
                var paramValues = new List<INamedParameter>();
                using (var numberLookUp = new EpsgNumberLookUp())
                using (var readerTxt = EpsgDataResource.CreateBinaryReader(ParamTextValueFileName)) {
                    for (int opIndex = 0; opIndex < opCount; opIndex++) {
                        var opCode = reader.ReadUInt16();
                        paramValues.Clear();
                        for (int paramIndex = 0; paramIndex < paramUsage.Length; paramIndex++) {
                            var valueCode = reader.ReadUInt16();
                            var uomCode = reader.ReadUInt16();
                            if (valueCode != 0xffff) {
                                var paramName = paramUsage[paramIndex].Parameter.Name;
                                EpsgUnit uom;
                                if (uomCode == UInt16.MaxValue) {
                                    uom = null;
                                }
                                else {
                                    uom = EpsgUnit.Get(uomCode);
                                    Contract.Assume(uom != null);
                                }
                                paramValues.Add(CreateParameter(valueCode, paramName, uom, readerTxt, numberLookUp));
                            }
                        }
                        lookUpDictionary.Add(opCode, new OpParamValueInfo(opCode, paramValues.ToArray()));
                    }
                }
                return new EpsgFixedLookUpBase<ushort, OpParamValueInfo>(lookUpDictionary);
            }

            private readonly ushort _coordinateOperationCode;
            private readonly ParamUsage[] _paramUsage;
            private readonly EpsgLookUpBase<ushort, OpParamValueInfo> _valueLookUp;
            private readonly string _paramDatFileName;

            public EpsgCoordinateOperationMethodParamInfoLookUp(ushort coordinateOperationCode) {
                _coordinateOperationCode = coordinateOperationCode;
                _paramDatFileName = "param" + _coordinateOperationCode + ".dat";
                Contract.Assume(!String.IsNullOrEmpty(_paramDatFileName));
                using (var reader = EpsgDataResource.CreateBinaryReader(_paramDatFileName)) {
                    _paramUsage = new ParamUsage[reader.ReadByte()];
                    for (int i = 0; i < _paramUsage.Length; i++) {
                        var paramInfo = EpsgParameterInfo.Get(reader.ReadUInt16());
                        Contract.Assume(paramInfo != null);
                        var signRev = 0x01 == reader.ReadByte();
                        _paramUsage[i] = new ParamUsage(paramInfo, signRev);
                    }
                    Contract.Assume(Contract.ForAll(_paramUsage, pu => pu != null));
                    var opCount = reader.ReadUInt16();
                    if (opCount <= FixedLookUpThreshold) {
                        _valueLookUp = CreateFullLookUp(reader, opCount, _paramUsage);
                    }
                    else {
                        var opCodes = new ushort[opCount];
                        var opSkip = _paramUsage.Length * (sizeof(ushort) * 2);
                        for (int i = 0; i < opCodes.Length; i++) {
                            opCodes[i] = reader.ReadUInt16();
                            reader.BaseStream.Seek(opSkip, SeekOrigin.Current);
                        }
                        _valueLookUp = new OpParamValueDynamicLookUp(this, opCodes);
                    }
                }
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_paramUsage != null);
                Contract.Invariant(Contract.ForAll(_paramUsage, pu => pu != null));
                Contract.Invariant(_valueLookUp != null);
                Contract.Invariant(!String.IsNullOrEmpty(_paramDatFileName));
            }

            public ReadOnlyCollection<ParamUsage> ParameterUsage {
                get {
                    Contract.Ensures(Contract.Result<ReadOnlyCollection<ParamUsage>>() != null);
                    return Array.AsReadOnly(_paramUsage);
                }
            }

            public OpParamValueInfo GetParameterValueInfo(int operationCode) {
                return operationCode < 0 || operationCode > UInt16.MaxValue
                    ? null
                    : _valueLookUp.Get((ushort)operationCode);
            }
        }

        [Obsolete]
        public class ParamUsage
        {

            internal ParamUsage(EpsgParameterInfo parameter, bool signRev) {
                Contract.Requires(parameter != null);
                Parameter = parameter;
                SignReversal = signRev;
            }

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(Parameter != null);
            }

            public EpsgParameterInfo Parameter { get; private set; }

            public bool SignReversal { get; private set; }

        }

        [Obsolete]
        public class OpParamValueInfo
        {
            private readonly ushort _opCode;

            internal OpParamValueInfo(ushort opCode, INamedParameter[] values) {
                Contract.Requires(values != null);
                _opCode = opCode;
                Values = Array.AsReadOnly(values);
            }

            [ContractInvariantMethod]
            private void ObjectInvariants() {
                Contract.Invariant(Values != null);
            }

            public int OpCode { get { return _opCode; } }
            public ReadOnlyCollection<INamedParameter> Values { get; private set; }
        }*/

        [Obsolete]
        internal static readonly EpsgDataResourceReaderOperationMethod Reader = new EpsgDataResourceReaderOperationMethod();

        [Obsolete]
        public static EpsgCoordinateOperationMethodInfo Get(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? Reader.GetByKey(unchecked((ushort)code))
                : null;
        }

        [Obsolete]
        public static IEnumerable<EpsgCoordinateOperationMethodInfo> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationMethodInfo>>() != null);
                return Reader.ReadAllValues();
            }
        }

        private readonly ushort _code;
        
        [Obsolete]
        private readonly EpsgDataResourceReaderParameterValues _paramValuesReader;
        
        //[Obsolete]
        //private readonly Lazy<EpsgCoordinateOperationMethodParamInfoLookUp> _paramData;

        internal EpsgCoordinateOperationMethodInfo(ushort code, string name, bool canReverse) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _code = code;
            Name = name;
            CanReverse = canReverse;
            _paramValuesReader = new EpsgDataResourceReaderParameterValues(code);
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(_paramValuesReader != null);
        }

        public int Code {
            get {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return _code;
            }
        }
        public string Name { get; private set; }
        public bool CanReverse { get; private set; }

        public EpsgParameterUsage[] ParameterUsage {
            get {
                Contract.Ensures(Contract.Result<EpsgParameterUsage[]>() != null);
                return _paramValuesReader.GetParameterUsages();
            }
        }

        public List<INamedParameter> GetOperationParameters(int operationCode) {
            if (operationCode <= 0 || operationCode > UInt16.MaxValue)
                return null;
            return _paramValuesReader.ReadParameters(unchecked((ushort)operationCode));
        }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

        public override string ToString() {
            return Code.ToString() + " " + Name;
        }

    }
}
