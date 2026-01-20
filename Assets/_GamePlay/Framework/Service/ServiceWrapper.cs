using System;
namespace Kore.Utils.Core
{
	internal class ServiceWrapper<T> : IServiceWrapper
	{
		public static T instance = default(T);
		public void Unset()
		{
			ServiceWrapper<T>.instance = default(T);
		}
	}
}
