using System;
using System.Collections.Generic;
namespace Kore.Utils.Core
{
	public static class Service
	{
		[ThreadStatic]
		private static List<IServiceWrapper> serviceWrapperList;
		public static void Set<T>(T instance)
		{
			if (ServiceWrapper<T>.instance != null)
			{
				throw new Exception("An instance of this service class has already been set!");
			}
			ServiceWrapper<T>.instance = instance;
			if (Service.serviceWrapperList == null)
			{
				Service.serviceWrapperList = new List<IServiceWrapper>();
			}
			Service.serviceWrapperList.Add(new ServiceWrapper<T>());
		}
		public static T Get<T>()
		{
			return ServiceWrapper<T>.instance;
		}
		public static bool IsSet<T>()
		{
			return ServiceWrapper<T>.instance != null;
		}
		public static void ResetAll()
		{
			if (Service.serviceWrapperList == null)
			{
				return;
			}
			for (int i = Service.serviceWrapperList.Count - 1; i >= 0; i--)
			{
				Service.serviceWrapperList[i].Unset();
			}
			Service.serviceWrapperList = null;
		}

        public static void Unset<T>()
        {
            ServiceWrapper<T>.instance = default(T);
        }
	}
}
