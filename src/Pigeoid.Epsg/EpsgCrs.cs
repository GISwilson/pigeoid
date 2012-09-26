﻿
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public abstract class EpsgCrs : ICrs
	{

		public static EpsgCrs Get(int code) {
			return EpsgCrsDatumBased.GetDatumBased(code)
				?? EpsgCrsProjected.GetProjected(code)
				?? EpsgCrsCompound.GetCompound(code) as EpsgCrs;
		}

		public static IEnumerable<EpsgCrs> Values {
			get {
				return
					EpsgCrsDatumBased.DatumBasedValues
					.Concat<EpsgCrs>(EpsgCrsProjected.ProjectedValues)
					.Concat<EpsgCrs>(EpsgCrsCompound.CompoundValues)
					.OrderBy(x => x.Code);
			}
		}

		private readonly int _code;
		private readonly string _name;
		private readonly EpsgArea _area;
		private readonly bool _deprecated;

		internal EpsgCrs(int code, string name, EpsgArea area, bool deprecated) {
			_code = code;
			_name = name;
			_area = area;
			_deprecated = deprecated;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

		public EpsgArea Area { get { return _area; } }

		public bool Deprecated { get { return _deprecated; } }

		public override string ToString() {
			return Authority.ToString();
		}

	}
}
