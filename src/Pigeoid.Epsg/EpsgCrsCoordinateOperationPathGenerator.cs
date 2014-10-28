﻿using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{

    internal class EpsgCrsPathSearchNode
    {

        public EpsgCrsPathSearchNode(EpsgCrs crs) {
            Contract.Requires(crs != null);
            Crs = crs;
        }

        public EpsgCrsPathSearchNode(EpsgCrs crs, ICoordinateOperationInfo edgeFromParent, EpsgCrsPathSearchNode parent) {
            Contract.Requires(crs != null);
            Contract.Requires(edgeFromParent != null);
            Contract.Requires(parent != null);
            Crs = crs;
            EdgeFromParent = edgeFromParent;
            Parent = parent;
        }

        public readonly EpsgCrs Crs;
        public readonly ICoordinateOperationInfo EdgeFromParent;
        public readonly EpsgCrsPathSearchNode Parent;

        private void ObjectInvariants() {
            Contract.Invariant(Crs != null);
        }

        public CoordinateOperationCrsPathInfo BuildCoordinateOperationCrsPathInfo() {
            var nodes = new List<ICrs>();
            var operations = new List<ICoordinateOperationInfo>();

            var current = this;
            do {
                nodes.Add(current.Crs);
                if (current.EdgeFromParent == null)
                    break;

                operations.Add(current.EdgeFromParent);
                current = current.Parent;
                Contract.Assume(current != null);
            } while (true/*current != null*/);

            nodes.Reverse();
            operations.Reverse();
            return new CoordinateOperationCrsPathInfo(nodes, operations);
        }

    }

    public class EpsgCrsCoordinateOperationPathGenerator :
        ICoordinateOperationPathGenerator<EpsgCrs>,
        ICoordinateOperationPathGenerator<ICrs>
    {

        public List<Predicate<EpsgCrs>> CrsFilters { get; set; }
        public List<Predicate<ICoordinateOperationInfo>> OpFilters { get; set; }

        public IEnumerable<ICoordinateOperationCrsPathInfo> Generate(ICrs from, ICrs to) {
            var fromEpsg = from as EpsgCrs;
            var toEpsg = to as EpsgCrs;
            if (fromEpsg != null && toEpsg != null)
                return Generate(fromEpsg, toEpsg);
            throw new NotImplementedException();
        }

        public IEnumerable<ICoordinateOperationCrsPathInfo> Generate(EpsgCrs from, EpsgCrs to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationCrsPathInfo>>() != null);

            if (from.Kind == EpsgCrsKind.Compound || from.Kind == EpsgCrsKind.Engineering || from.Kind == EpsgCrsKind.Vertical)
                throw new NotImplementedException(String.Format("Support for the from CRS kind {0} is not yet implemented.", from.Kind));
            if (to.Kind == EpsgCrsKind.Compound || to.Kind == EpsgCrsKind.Engineering || to.Kind == EpsgCrsKind.Vertical)
                throw new NotImplementedException(String.Format("Support for the to CRS kind {0} is not yet implemented.", to.Kind));
            if (from.Code == to.Code)
                throw new NotImplementedException("Empty conversion not yet handled.");

            var startNode = new EpsgCrsPathSearchNode(from);

            Contract.Assume(to is EpsgCrsGeodetic);
            var corePaths = FindAllCorePaths(startNode, (EpsgCrsGeodetic)to);
            return corePaths.Select(node => node.BuildCoordinateOperationCrsPathInfo());
        }

        private IEnumerable<EpsgCrsPathSearchNode> FindAllCorePaths(EpsgCrsPathSearchNode fromNode, EpsgCrsGeodetic toCrs) {
            Contract.Requires(fromNode != null);
            Contract.Requires(fromNode.Crs is EpsgCrsGeodetic);
            Contract.Requires(toCrs != null);

            var earlyResults = new List<EpsgCrsPathSearchNode>();
            var fromCrs = (EpsgCrsGeodetic)fromNode.Crs;
            EpsgCrsPathSearchNode stackSearchNode;

            // construct the hierarchy based on the from CRS
            var fromStack = new List<EpsgCrsPathSearchNode>();
            stackSearchNode = fromNode;
            do {
                fromStack.Add(stackSearchNode);
                var currentCrs = (EpsgCrsGeodetic)stackSearchNode.Crs;
                if (currentCrs.Code == toCrs.Code)
                    earlyResults.Add(stackSearchNode);

                if (!currentCrs.HasBaseOperation)
                    break;

                var baseCrs = currentCrs.BaseCrs;
                if (baseCrs == null)
                    break;

                var fromBaseEdge = currentCrs.GetBaseOperation();
                if (!fromBaseEdge.HasInverse)
                    break; // we have to invert the edge to traverse up the stack

                var toBaseEdge = fromBaseEdge.GetInverse();
                stackSearchNode = new EpsgCrsPathSearchNode(baseCrs, toBaseEdge, stackSearchNode);
            } while (stackSearchNode != null);

            return earlyResults;
        }

    }
}
