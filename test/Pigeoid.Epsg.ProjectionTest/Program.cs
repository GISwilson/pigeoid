﻿using System.Linq;

namespace Pigeoid.Epsg.ProjectionTest
{
	class Program
	{
		static void Main(string[] args) {

			/*var txGen = new EpsgTransformationGenerator();
			var from = EpsgCrs.Get(4326);
			var to = EpsgCrs.Get(3857);
			var tx = txGen.Generate(from, to);*/
			var localCrs = EpsgCrs.Values.OfType<EpsgCrsEngineering>().First();
			;
		}
	}
}
