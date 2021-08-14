using OTAPI;
using System;
using System.Linq;
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

			On.Terraria.Main.startDedInput += OnStart;
			Hooks.Main.CommandProcess += OnCommandProcess;
			On.Terraria.RemoteClient.Reset += OnClientReset;
		}

		private static void OnClientReset(
			On.Terraria.RemoteClient.orig_Reset orig,
			Terraria.RemoteClient remoteClient)
		{
			if (!Netplay.Disconnect)
			{
				if (remoteClient.IsActive)
				{
					_hookManager.InvokeServerLeave(remoteClient.Id);
				}
				_hookManager.InvokeServerSocketReset(remoteClient);
			}
		}

		private static void OnCommandProcess(object sender, Hooks.Main.CommandProcessEventArgs args)
		{
			if (_hookManager.InvokeServerCommand(args.Command))
			{
				args.Result = HookResult.Cancel;
			}
		}

		private static void OnStart(On.Terraria.Main.orig_startDedInput orig)
		{
			if(Environment.GetCommandLineArgs().Any(x => x.Equals("-disable-commands")))
			{
				Console.WriteLine("Command thread has been disabled.");
			}
			else
			{
				orig();
			}
			_hookManager.InvokeGamePostInitialize();
		}
	}
}
