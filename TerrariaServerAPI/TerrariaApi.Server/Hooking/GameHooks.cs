using Microsoft.Xna.Framework;
using OTAPI;

namespace TerrariaApi.Server.Hooking
{
	internal static class GameHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Game hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.Main.Initialize += OnInitialize;
			On.Terraria.Main.Update += OnUpdate;

			Hooks.WorldGen.HardmodeTileUpdate += OnHardmodeTileUpdate;
			Hooks.NPC.MechSpawn += OnNpcMechSpawn;
			Hooks.Item.MechSpawn += OnItemMechSpawn;
		}

		private static void OnInitialize(On.Terraria.Main.orig_Initialize orig, Terraria.Main self)
		{
			HookManager.InitialiseAPI();
			_hookManager.InvokeGameInitialize();
			orig(self);
		}

		private static void OnUpdate(On.Terraria.Main.orig_Update orig, Terraria.Main self, GameTime time)
		{
			_hookManager.InvokeGameUpdate();
			orig(self, time);
			_hookManager.InvokeGamePostUpdate();
		}

		private static void OnHardmodeTileUpdate(object sender, Hooks.WorldGen.HardmodeTileUpdateEventArgs args)
		{
			if (_hookManager.InvokeGameHardmodeTileUpdate(args.X, args.Y, args.Type))
			{
				args.Result = HookResult.Cancel;
			}
		}

		private static void OnNpcMechSpawn(object sender, Hooks.NPC.MechSpawnEventArgs args)
		{
			if (_hookManager.InvokeGameStatueSpawn(
				args.Num2,
				args.Num3,
				args.Num,
				(int)(args.X / 16f),
				(int)(args.Y / 16f),
				args.Type,
				true))
			{
				args.Result = HookResult.Cancel;
			}
		}

		private static void OnItemMechSpawn(object sender, Hooks.Item.MechSpawnEventArgs args)
		{
			if (_hookManager.InvokeGameStatueSpawn(
				args.Num2,
				args.Num3,
				args.Num,
				(int)(args.X / 16f),
				(int)(args.Y / 16f),
				args.Type,
				false))
			{
				args.Result = HookResult.Cancel;
			}
		}
	}
}
