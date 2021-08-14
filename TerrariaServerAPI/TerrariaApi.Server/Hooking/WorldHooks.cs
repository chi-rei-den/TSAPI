using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class WorldHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI World hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.Collision.PressurePlate += OnPressurePlate;
			Hooks.WorldGen.Meteor += OnMeteor;

			On.Terraria.IO.WorldFile.SaveWorld_bool_bool += OnSaveWorld;
			On.Terraria.WorldGen.StartHardmode += OnStartHardmode;
			On.Terraria.Main.checkXMas += OnChristmas;
			On.Terraria.Main.checkHalloween += OnHalloween;
			On.Terraria.WorldGen.SpreadGrass += OnSpreadGrass;
		}

		private static void OnPressurePlate(object sender, Hooks.Collision.PressurePlateEventArgs args)
		{
			var npc = args.Entity as NPC;
			if (npc != null)
			{
				if (_hookManager.InvokeNpcTriggerPressurePlate(npc, args.X, args.Y))
				{
					args.Result = HookResult.Cancel;
				}
			}
			else
			{
				var player = args.Entity as Player;
				if (player != null)
				{
					if (_hookManager.InvokePlayerTriggerPressurePlate(player, args.X, args.Y))
					{
						args.Result = HookResult.Cancel;
					}
				}
				else
				{
					var projectile = args.Entity as Projectile;
					if (projectile != null)
					{
						if (_hookManager.InvokeProjectileTriggerPressurePlate(projectile, args.X, args.Y))
						{
							args.Result = HookResult.Cancel;
						}
					}
				}
			}
		}

		private static void OnSaveWorld(
			On.Terraria.IO.WorldFile.orig_SaveWorld_bool_bool orig,
			bool useCloudSaving,
			bool resetTime)
		{
			if (!_hookManager.InvokeWorldSave(resetTime))
			{
				orig(useCloudSaving, resetTime);
			}
		}

		private static void OnStartHardmode(On.Terraria.WorldGen.orig_StartHardmode orig)
		{
			if (!_hookManager.InvokeWorldStartHardMode())
			{
				orig();
			}
		}

		private static void OnMeteor(object sender, Hooks.WorldGen.MeteorEventArgs args)
		{
			if (_hookManager.InvokeWorldMeteorDrop(args.X, args.Y))
			{
				args.Result = HookResult.Cancel;
			}
		}

		private static void OnChristmas(On.Terraria.Main.orig_checkXMas orig)
		{
			if (!_hookManager.InvokeWorldChristmasCheck(ref Terraria.Main.xMas))
			{
				orig();
			}
		}

		private static void OnHalloween(On.Terraria.Main.orig_checkHalloween orig)
		{
			if (!_hookManager.InvokeWorldHalloweenCheck(ref Main.halloween))
			{
				orig();
			}
		}

		private static void OnSpreadGrass(
			On.Terraria.WorldGen.orig_SpreadGrass orig,
			int tileX,
			int tileY,
			int dirt,
			int grass,
			bool repeat,
			byte color)
		{
			if (!_hookManager.InvokeWorldGrassSpread(tileX, tileY, dirt, grass, repeat, color))
			{
				orig(tileX, tileY, dirt, grass, repeat, color);
			}
		}
	}
}
