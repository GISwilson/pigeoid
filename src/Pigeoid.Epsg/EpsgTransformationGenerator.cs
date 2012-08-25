﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pigeoid.Contracts;
using Vertesaur.Contracts;
using Vertesaur.Search;

namespace Pigeoid.Epsg
{
	public class EpsgTransformationGenerator :
		ITransformationGenerator<ICrs>,
		ITransformationGenerator<EpsgCrs>
	{

		private struct EpsgTransformGraphCost :
			IComparable<EpsgTransformGraphCost>
		{
			public readonly int TotalCount;
			public readonly double TotalAccuracy;

			private EpsgTransformGraphCost(int totalCount, double totalAccuracy) {
				TotalCount = totalCount;
				TotalAccuracy = totalAccuracy;
			}

			public int CompareTo(EpsgTransformGraphCost other) {
				var compareResult = TotalCount.CompareTo(other.TotalCount);
				return 0 != compareResult ? compareResult : TotalAccuracy.CompareTo(other.TotalAccuracy);
			}

			public EpsgTransformGraphCost Add(int additionalCount, double additionalAccuracy) {
				return new EpsgTransformGraphCost(TotalCount + additionalCount, TotalAccuracy + additionalAccuracy);
			}

		}

		// TODO: a custom cost class may be needed because: concatenated operation should be used first and not all operations have accuracy values
		private class EpsgTransformGraph : DynamicGraphBase<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>
		{

			private readonly EpsgArea _fromArea;
			private readonly EpsgArea _toArea;

			public EpsgTransformGraph(EpsgArea fromArea, EpsgArea toArea) {
				_fromArea = fromArea;
				_toArea = toArea;
			}

			private bool AreaIntersectionPasses(EpsgArea area) {
				return null == area
					|| null == _fromArea
					|| null == _toArea
					|| _fromArea.Intersects(area)
					|| _toArea.Intersects(area);
			}

			public override IEnumerable<DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>> GetNeighborInfo(EpsgCrs node, EpsgTransformGraphCost currentCost) {
				var nodeCode = node.Code;
				var results = new List<DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>>();


				var projectionNode = node as EpsgCrsProjected;
				if(null != projectionNode) {
					var projectionOperationInformation = projectionNode.Projection;
					if(null != projectionOperationInformation) {
						results.Add(new DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>(
							projectionNode.BaseCrs, currentCost.Add(1, 0), projectionOperationInformation));
					}
				}

				foreach(var projectionCoordinateReferenceSystem in EpsgCrsProjected.GetProjectionsBasedOn(nodeCode)) {
					if(!AreaIntersectionPasses(projectionCoordinateReferenceSystem.Area))
						continue;

					var projectionOperationInformation = projectionCoordinateReferenceSystem.Projection;
					if(null != projectionOperationInformation && projectionOperationInformation.HasInverse) {
						results.Add(new DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>(
							projectionCoordinateReferenceSystem, currentCost.Add(1, 0), projectionOperationInformation.GetInverse()));
					}
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedForwardReferenced(nodeCode)) {
					var crs = op.TargetCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>(
						crs , currentCost.Add(1, 0), op));
				}
				foreach(var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedReverseReferenced(nodeCode)) {
					if(!op.HasInverse)
						continue;
					var crs = op.SourceCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>(
						crs, currentCost.Add(1, 0), op.GetInverse()));
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetTransformForwardReferenced(nodeCode)) {
					var crs = op.TargetCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>(
						crs, currentCost.Add(1, 0), op));
				}
				foreach(var op in EpsgCoordinateOperationInfoRepository.GetTransformReverseReferenced(nodeCode)) {
					if(!op.HasInverse)
						continue;
					var crs = op.SourceCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DyanmicGraphNodeData<EpsgCrs, EpsgTransformGraphCost, ICoordinateOperationInfo>(
						crs, currentCost.Add(1, 0), op.GetInverse()));
				}

				return results;
			}
		}


		public ITransformation Generate(EpsgCrs from, EpsgCrs to) {
			var graph = new EpsgTransformGraph(from.Area, to.Area);
			var execTimer = new Stopwatch();
			execTimer.Start();
			var path = graph.FindPath(from, to);
			execTimer.Stop();
			var elapsed = execTimer.Elapsed;
			Debug.Write("Generate: " + elapsed);
			return null;// throw new NotImplementedException();
		}

		public ITransformation Generate(ICrs from, ICrs to) {
			if(from is EpsgCrs && to is EpsgCrs) {
				return Generate(from as EpsgCrs, to as EpsgCrs);
			}
			// TODO: if one is not an EpsgCrs we should try making it one (but really it should already have been... so maybe not)
			// TODO: if one is EpsgCrs and the other is not, we need to find the nearest EpsgCrs along the way and use standard methods to get us there
			throw new NotImplementedException("Currently only EpsgCrs to EpsgCrs is supported."); // TODO: just return null if we don't know what to do with it?
		}

	}



}
