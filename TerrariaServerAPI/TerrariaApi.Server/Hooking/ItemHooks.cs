﻿using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class ItemHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Item hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.Item.SetDefaults_int_bool += OnSetDefaultsById;
			On.Terraria.Item.netDefaults += OnNetDefaults;

			Hooks.Chest.QuickStack += OnQuickStack;
		}

		private static void OnSetDefaultsById(On.Terraria.Item.orig_SetDefaults_int_bool orig, Item self, int type, bool check)
		{
			if (!_hookManager.InvokeItemSetDefaultsInt(ref type, self))
			{
				orig(self, type, check);
			}
		}

		private static void OnNetDefaults(On.Terraria.Item.orig_netDefaults orig, Item self, int type)
		{
			if (!_hookManager.InvokeItemNetDefaults(ref type, self))
			{
				orig(self, type);
			}
		}

		private static void OnQuickStack(object sender, Hooks.Chest.QuickStackEventArgs args)
		{
			if (_hookManager.InvokeItemForceIntoChest(Main.chest[args.ChestIndex], args.Item, Main.player[args.PlayerId]))
			{
				args.Result = HookResult.Cancel;
			}
		}
	}
}
