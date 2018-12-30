using System;
using UnityEngine;

namespace SPEngine
{
	public static class Logging
	{
		public static void Log(string text)
		{
			Debug.Log("[SPEngine] " + text);
		}
		public static void LogFormat(string format, params object[] args)
		{
			Debug.LogFormat("[SPEngine] " + format, args);
		}
		public static void LogWarningFormat(string format, params object[] args)
		{
			Debug.LogWarningFormat("[SPEngine] " + format, args);
		}
		public static void LogException(Exception e)
		{
			Debug.LogException(e);
		}
	}
}
