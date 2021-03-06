﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	[TestFixture]
    public class EpsgCoordinateOperationMethodInfoTest : EpsgDataTestBase<EpsgCoordinateOperationMethodInfo, DbRepository.EpsgCoordinateOperationMethod>
	{

		[Test]
		public void Resources_Match_Db() {

            var assemblyItems = EpsgMicroDatabase.Default.GetOperationMethods();
			var databaseItems = Repository.CoordinateOperationMethods;

			AssertMatches(
				assemblyItems,
				databaseItems,
				new Tester((x, y) => x.Code == y.Code),
				new Tester((x, y) => x.Name == y.Name),
				new Tester((x, y) => x.CanReverse == y.Reverse),
				new Tester((x, y) => AssertEqual(x.ParameterUsage,y.ParamUse.OrderBy(z => z.SortOrder).ToList()))
			);

		}

        private bool AssertEqual(EpsgParameterUsage[] a, IList<DbRepository.EpsgParamUse> b) {
			Assert.AreEqual(a.Length, b.Count);
			for(int i = 0; i < a.Length; i++) {
				var x = a[i];
				var y = b[i];
				Assert.AreEqual(x.ParameterInfo.Code, y.Parameter.Code);
				Assert.AreEqual(x.SignReversal, y.SignReversal ?? false);
			}
			return true;
		}

		[Test]
		public void Operation_Param_Values() {
			foreach(var dbOpMethod in Repository.CoordinateOperationMethods) {
                var assemblyOperationMethod = EpsgMicroDatabase.Default.GetOperationMethod(dbOpMethod.Code);
				Assert.IsNotNull(assemblyOperationMethod);
				foreach(var dbOperation in dbOpMethod.UsedBy) {
					var assemblyParamValues = assemblyOperationMethod.GetOperationParameters(dbOperation.Code);
					var dbParamValues = dbOperation.ParameterValues.Where(x => x.NumericValue != null || x.TextValue != null).ToList();
					Assert.AreEqual(dbParamValues.Count, assemblyParamValues.Count);
					foreach(var dbParamValue in dbParamValues) {
						var assemblyParamValue = assemblyParamValues.Single(x => x.Name == dbParamValue.Parameter.Name);
						var dbValue = (object)dbParamValue.NumericValue ?? dbParamValue.TextValue;
						var dbUom = dbParamValue.Uom;
						Assert.AreEqual(dbValue, assemblyParamValue.Value);
						if(null == dbUom)
							Assert.IsNull(assemblyParamValue.Unit);
						else
							Assert.AreEqual(dbUom.Code, ((EpsgUnit) assemblyParamValue.Unit).Code);
					}
				}
			}
		}
	}
}
