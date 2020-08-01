﻿using OTAPI;
using System;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class ServerHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Server hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.Command.StartCommandThread = OnStartCommandThread;
			Hooks.Command.Process = OnProcess;
			Hooks.Net.RemoteClient.PreReset = OnPreReset;
		}

		static HookResult OnStartCommandThread()
		{
			if (Console.IsInputRedirected == true)
			{
				Console.WriteLine("TerrariaServer正在后台运行，已自动忽略命令行输入。");
				return HookResult.Cancel;
			}

			return HookResult.Continue;
		}

		static HookResult OnProcess(string lowered, string raw)
		{
			if (_hookManager.InvokeServerCommand(raw))
			{
				return HookResult.Cancel;
			}
			return HookResult.Continue;
		}

		static HookResult OnPreReset(Terraria.RemoteClient remoteClient)
		{
			if (!Netplay.Disconnect)
			{
				if (remoteClient.IsActive)
				{
					_hookManager.InvokeServerLeave(remoteClient.Id);
				}
				_hookManager.InvokeServerSocketReset(remoteClient);
			}
			return HookResult.Continue;
		}
	}
}
