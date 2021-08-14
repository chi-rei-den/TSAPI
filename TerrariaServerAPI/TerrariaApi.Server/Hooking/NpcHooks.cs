using System;
using Microsoft.Xna.Framework;
using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class NpcHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Npc hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.NPC.SetDefaults += OnSetDefaults;
			On.Terraria.NPC.SetDefaultsFromNetId += OnSetDefaultsFromNetId;
			On.Terraria.NPC.StrikeNPC += OnStrikeNPC;
			On.Terraria.NPC.AI += OnAI;

			Hooks.NPC.Transforming += OnTransforming;
			Hooks.NPC.Spawn += OnSpawn;
			Hooks.NPC.DropLoot += OnDropLoot;
			Hooks.NPC.BossBag += OnBossBag;
			Hooks.NPC.Killed += OnKilled;
		}

		private static void OnSetDefaults(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int type, NPCSpawnParams spawnparams)
		{
			if (!_hookManager.InvokeNpcSetDefaultsInt(ref type, self))
			{
				orig(self, type, spawnparams);
			}
		}

		private static void OnSetDefaultsFromNetId(
			On.Terraria.NPC.orig_SetDefaultsFromNetId orig,
			NPC self,
			int id,
			NPCSpawnParams spawnParams)
		{
			if (!_hookManager.InvokeNpcNetDefaults(ref id, self))
			{
				orig(self, id, spawnParams);
			}
		}

		private static double OnStrikeNPC(
			On.Terraria.NPC.orig_StrikeNPC orig,
			NPC self,
			int damage,
			float knockback,
			int hitDirection,
			bool crit,
			bool noEffect,
			bool fromNet,
			Entity entity)
		{
			var player = entity as Player;
			if (player != null)
			{
				if (!_hookManager.InvokeNpcStrike(self, ref damage, ref knockback, ref hitDirection,
					ref crit, ref noEffect, ref fromNet, player))
				{
					return orig(self, damage, knockback, hitDirection, crit, noEffect, fromNet, entity);
				}
			}
			return 0;
		}

		private static void OnTransforming(object sender, Hooks.NPC.TransformingEventArgs args)
		{
			if (_hookManager.InvokeNpcTransformation(args.Npc.whoAmI))
			{
				args.Result = HookResult.Cancel;
			}
		}

		private static void OnSpawn(object sender, Hooks.NPC.SpawnEventArgs args)
		{
			int index = args.Index;
			if (_hookManager.InvokeNpcSpawn(ref index))
			{
				args.Result = HookResult.Cancel;
			}
			args.Index = index;
		}

		private static void OnDropLoot(object sender, Hooks.NPC.DropLootEventArgs args)
		{
			if (_hookManager.InvokeNpcLootDrop(args))
			{
				args.Result = HookResult.Cancel;
			}
		}

		private static void OnBossBag(object sender, Hooks.NPC.BossBagEventArgs args)
		{
			if (_hookManager.InvokeDropBossBag(args))
			{
				args.Result = HookResult.Cancel;
			}
		}

		private static void OnAI(On.Terraria.NPC.orig_AI orig, NPC self)
		{
			if (!_hookManager.InvokeNpcAIUpdate(self))
			{
				orig(self);
			}
		}

		private static void OnKilled(object sender, Hooks.NPC.KilledEventArgs args)
		{
			_hookManager.InvokeNpcKilled(args.Npc);
		}
	}
}
