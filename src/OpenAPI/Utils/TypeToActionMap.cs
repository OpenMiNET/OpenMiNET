using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OpenAPI.Utils
{
	public class TypeToActionMap<TBase>
	{
		private Hashtable ActionMap { get; }
		public TypeToActionMap()
		{
			ActionMap = new Hashtable();
		}

		public bool Register<TType>(Action<TType> action) where TType : TBase
		{
			Type type = typeof(TType);

			if (ActionMap.ContainsKey(type))
			{
				return false;
			}
			ActionMap.Add(type, new Action<TBase>(x => action((TType)x)));

			return true;
		}

		public bool TryInvokeAction<TValue>(TValue argument) where TValue : TBase
		{
			Type type = argument.GetType();

			if (!ActionMap.ContainsKey(type))
			{
				return false;
			}
			((Action<TBase>)ActionMap[type]).Invoke(argument);

			return true;
		}
	}
}
