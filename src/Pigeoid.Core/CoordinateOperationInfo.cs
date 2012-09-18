﻿using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class CoordinateOperationInfo : ICoordinateOperationInfo
	{

		public CoordinateOperationInfo(string name) : this(name,null) { }

		public CoordinateOperationInfo(string name, IEnumerable<INamedParameter> parameters) : this(name, parameters, true) { }

		public CoordinateOperationInfo(string name, IEnumerable<INamedParameter> parameters, bool hasInverse)
		{
			Name = name;
			Parameters = null == parameters ? new List<INamedParameter>() : parameters.ToList();
			HasInverse = hasInverse;
		}

		public string Name { get; protected set; }

		public List<INamedParameter> Parameters { get; private set; } 

		IEnumerable<INamedParameter> ICoordinateOperationInfo.Parameters { get { return Parameters; } }

		public bool HasInverse { get; protected set; }

		public ICoordinateOperationInfo GetInverse()
		{
			if (HasInverse)
				return new CoordinateOperationInfoInverse(this);

			throw new InvalidOperationException("Operation does not have an inverse.");
		}

		public bool IsInverseOfDefinition { get { return false; } }

	}

}
