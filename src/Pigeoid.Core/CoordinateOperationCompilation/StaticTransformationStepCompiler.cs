﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Interop;
using Pigeoid.Ogc;
using Pigeoid.Projection;
using Pigeoid.Transformation;
using Pigeoid.Unit;
using Vertesaur;
using Vertesaur.Contracts;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperationCompilation
{
	class StaticTransformationStepCompiler : StaticCoordinateOperationCompiler.IStepOperationCompiler
	{

		private class TransformationCompilationParams
		{

			public TransformationCompilationParams(
				[NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParams,
				[NotNull] NamedParameterLookup parameterLookup,
				string operationName)
			{
				StepParams = stepParams;
				ParameterLookup = parameterLookup;
				OperationName = operationName;
			}

			public StaticCoordinateOperationCompiler.StepCompilationParameters StepParams;
			public NamedParameterLookup ParameterLookup;
			public string OperationName;

		}

		private static readonly StaticTransformationStepCompiler DefaultValue = new StaticTransformationStepCompiler();
		public static StaticTransformationStepCompiler Default { get { return DefaultValue; } }

		private static IUnit ExtractUnit(ICrs crs) {
			var geodetic = crs as ICrsGeodetic;
			if (null != geodetic)
				return geodetic.Unit;
			return null;
		}

		private static bool ConvertIfVaild(IUnit from, IUnit to, ref double value) {
			if (null == from || null == to)
				return false;
			var conv = SimpleUnitConversionGenerator.FindConversion(from, to);
			if (null == conv)
				return false;

			value = conv.TransformValue(value);
			return true;
		}

		private static bool TryGetDouble(INamedParameter parameter, IUnit unit, out double value) {
			if (!NamedParameter.TryGetDouble(parameter, out value))
				return false;

			ConvertIfVaild(parameter.Unit, unit, ref value);
			return true;
		}

		private static bool TryCrateVector3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, out Vector3 result) {
			return TryCrateVector2(xParam, yParam, zParam, null, out result);
		}

		private static bool TryCrateVector2(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, IUnit linearUnit, out Vector3 result) {
			double x, y, z;
			if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y) && NamedParameter.TryGetDouble(yParam, out z)) {
				if (null != linearUnit) {
					ConvertIfVaild(xParam.Unit, linearUnit, ref x);
					ConvertIfVaild(yParam.Unit, linearUnit, ref y);
					ConvertIfVaild(zParam.Unit, linearUnit, ref z);
				}
				result = new Vector3(x, y, z);
				return true;
			}
			result = Vector3.Invalid;
			return false;
		}

		private static bool TryCreateGeographicCoordinate(INamedParameter latParam, INamedParameter lonParam, out GeographicCoordinate result) {
			double lat, lon;
			if (NamedParameter.TryGetDouble(latParam, out lat) && NamedParameter.TryGetDouble(lonParam, out lon)) {
				ConvertIfVaild(latParam.Unit, OgcAngularUnit.DefaultRadians, ref lat);
				ConvertIfVaild(lonParam.Unit, OgcAngularUnit.DefaultRadians, ref lon);
				result = new GeographicCoordinate(lat, lon);
				return true;
			}
			result = default(GeographicCoordinate);
			return false;
		}

		private static ISpheroidInfo ExtractSpheroid(ICrs crs) {
			var geodetic = crs as ICrsGeodetic;
			if (null == geodetic)
				return null;
			var datum = geodetic.Datum;
			if (null == datum)
				return null;
			return datum.Spheroid;
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreatePositionVectorTransformation(TransformationCompilationParams opData) {
			var xTransParam = new KeywordNamedParameterSelector("XAXIS", "TRANS");
			var yTransParam = new KeywordNamedParameterSelector("YAXIS", "TRANS");
			var zTransParam = new KeywordNamedParameterSelector("ZAXIS", "TRANS");
			var xRotParam = new KeywordNamedParameterSelector("XAXIS", "ROT");
			var yRotParam = new KeywordNamedParameterSelector("YAXIS", "ROT");
			var zRotParam = new KeywordNamedParameterSelector("ZAXIS", "ROT");
			var scaleParam = new KeywordNamedParameterSelector("SCALE");

			if (!opData.ParameterLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam))
				return null;

			Vector3 translation, rotation;
			double scale;

			if (!TryCrateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
				return null;
			if (!TryCrateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, out rotation))
				return null;
			if (!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
				return null;

			ConvertIfVaild(scaleParam.Selection.Unit, ScaleUnitPartsPerMillion.Value, ref scale);

			var helmert = new Helmert7Transformation(translation, rotation, scale);

			if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric)
				return new StaticCoordinateOperationCompiler.StepCompilationResult(opData.StepParams, opData.StepParams.RelatedOutputCrsUnit, helmert);

			var spheroidFrom = ExtractSpheroid(opData.StepParams.RelatedInputCrs) ?? ExtractSpheroid(opData.StepParams.RelatedOutputCrs);
			if (null == spheroidFrom)
				return null;

			var spheroidTo = ExtractSpheroid(opData.StepParams.RelatedOutputCrs) ?? spheroidFrom;

			ITransformation transformation = new Helmert7GeographicTransformation(spheroidFrom, helmert, spheroidTo);
			var unitConversion = CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
			if(null != unitConversion)
				transformation = new ConcatenatedTransformation(new[] { unitConversion, transformation});

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				OgcAngularUnit.DefaultRadians,
				transformation);
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeographicOffset(TransformationCompilationParams opData) {
			var latParam = new KeywordNamedParameterSelector("LAT");
			var lonParam = new KeywordNamedParameterSelector("LON");
			opData.ParameterLookup.Assign(latParam, lonParam);

			ITransformation transformation = null;
			if (latParam.IsSelected && lonParam.IsSelected) {
				GeographicCoordinate offset;
				if (TryCreateGeographicCoordinate(latParam.Selection, lonParam.Selection, out offset))
					transformation = new GeographicOffset(offset);
			}
			else if (latParam.IsSelected) {
				double value;
				if(TryGetDouble(latParam.Selection, OgcAngularUnit.DefaultRadians, out value))
					transformation = new GeographicOffset(new GeographicCoordinate(value, 0));
			}
			else if (lonParam.IsSelected) {
				double value;
				if(TryGetDouble(lonParam.Selection, OgcAngularUnit.DefaultRadians, out value))
					transformation = new GeographicOffset(new GeographicCoordinate(0, value));
			}

			if(null == transformation)
				return null; // no parameters

			var unitConversion = CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
			if (null != unitConversion)
				transformation = new ConcatenatedTransformation(new[] { unitConversion, transformation });

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				OgcAngularUnit.DefaultRadians,
				transformation);
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeocentricTranslation(TransformationCompilationParams opData) {
			var xParam = new KeywordNamedParameterSelector("XAXIS", "X");
			var yParam = new KeywordNamedParameterSelector("YAXIS", "Y");
			var zParam = new KeywordNamedParameterSelector("ZAXIS", "Z");
			if (!opData.ParameterLookup.Assign(xParam, yParam, zParam))
				return null;

			Vector3 delta;
			if (!TryCrateVector3(xParam.Selection, yParam.Selection, zParam.Selection, out delta))
				return null;

			if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric) {
				// assume the units are correct
				return new StaticCoordinateOperationCompiler.StepCompilationResult(
					opData.StepParams,
					opData.StepParams.RelatedOutputCrsUnit ?? opData.StepParams.RelatedInputCrsUnit ?? opData.StepParams.InputUnit,
					new GeocentricTranslation(delta));
			}

			var inputSpheroid = opData.StepParams.RelatedInputSpheroid;
			if (null == inputSpheroid)
				return null;
			var outputSpheroid = opData.StepParams.RelatedOutputSpheroid;
			if (null == outputSpheroid)
				return null;
			ITransformation transformation = new GeographicGeocentricTranslation(inputSpheroid, delta, outputSpheroid);
			var conv = CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
			if (null != conv)
				transformation = new ConcatenatedTransformation(new[] {conv, transformation});
			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				OgcAngularUnit.DefaultRadians,
				transformation);

		}

		private readonly INameNormalizedComparer _coordinateOperationNameComparer;

		public StaticTransformationStepCompiler(INameNormalizedComparer coordinateOperationNameComparer = null){
			_coordinateOperationNameComparer = coordinateOperationNameComparer ?? CoordinateOperationNameNormalizedComparer.Default;
		}

		public StaticCoordinateOperationCompiler.StepCompilationResult Compile(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
			return CompileInverse(stepParameters)
				?? CompileForwards(stepParameters);
		}

		[CanBeNull] private static ITransformation CreateCoordinateUnitConversion([NotNull] IUnit from, [NotNull] IUnit to) {
			if (!UnitEqualityComparer.Default.Equals(from, to)) {
				var conv = SimpleUnitConversionGenerator.FindConversion(from, to);
				if (null != conv) {
					if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("LENGTH", from.Type)) {
						return new LinearElementTransformation(conv);
					}
					if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("ANGLE", from.Type)) {
						return new AngularElementTransformation(conv);
					}
				}
			}
			return null;
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileForwards([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters){
			var forwardCompiledStep = CompileForwardToTransform(stepParameters);
			if (null == forwardCompiledStep)
				return null;

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				forwardCompiledStep.OutputUnit,
				forwardCompiledStep.Transformation
			);
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileInverse([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters){
			if (!stepParameters.CoordinateOperationInfo.IsInverseOfDefinition || !stepParameters.CoordinateOperationInfo.HasInverse)
				return null;

			var inverseOperationInfo = stepParameters.CoordinateOperationInfo.GetInverse();
			if (null == inverseOperationInfo)
				return null;

			var expectedOutputUnits = ExtractUnit(stepParameters.RelatedOutputCrs)
				?? ExtractUnit(stepParameters.RelatedInputCrs);

			var forwardCompiledStep = CompileForwardToTransform(new StaticCoordinateOperationCompiler.StepCompilationParameters(
				inverseOperationInfo,
				expectedOutputUnits,
				stepParameters.RelatedOutputCrs,
				stepParameters.RelatedInputCrs
			));

			var forwardCompiledOperation = forwardCompiledStep.Transformation;
			if (!forwardCompiledOperation.HasInverse)
				return null;

			var inverseCompiledOperation = forwardCompiledOperation.GetInverse();
			var resultTransformation = inverseCompiledOperation;

			// make sure that the input units are correct
			var unitConversion = CreateCoordinateUnitConversion(stepParameters.InputUnit, forwardCompiledStep.OutputUnit);
			if(null != unitConversion)
				resultTransformation = new ConcatenatedTransformation(new[]{unitConversion, resultTransformation});

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				expectedOutputUnits,
				resultTransformation
			);
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileForwardToTransform([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
			string operationName = null;
			IEnumerable<INamedParameter> parameters = null;

			var parameterizedOperation = stepParameters.CoordinateOperationInfo as IParameterizedCoordinateOperationInfo;
			if (null != parameterizedOperation) {
				if (null != parameterizedOperation.Method)
					operationName = parameterizedOperation.Method.Name;
				parameters = parameterizedOperation.Parameters;
			}

			if (null == operationName)
				operationName = stepParameters.CoordinateOperationInfo.Name;

			if (null == operationName)
				return null;

			var parameterLookup = new NamedParameterLookup(parameters ?? Enumerable.Empty<INamedParameter>());
			var compilationParams = new TransformationCompilationParams(stepParameters, parameterLookup, operationName);

			var normalizedName = _coordinateOperationNameComparer.Normalize(compilationParams.OperationName);

			if (normalizedName.StartsWith("POSITIONVECTORTRANSFORMATION"))
				return CreatePositionVectorTransformation(compilationParams);
			if (normalizedName.Equals("GEOGRAPHICOFFSET"))
				return CreateGeographicOffset(compilationParams);
			if (normalizedName.StartsWith("GEOCENTRICTRANSLATION"))
				return CreateGeocentricTranslation(compilationParams);
			return null;
		}

	}
}
