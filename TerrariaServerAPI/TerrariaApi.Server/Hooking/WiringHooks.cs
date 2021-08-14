using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class WiringHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Wiring hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.Wiring.AnnouncementBox += OnAnnouncementBox;
		}

		private static void OnAnnouncementBox(object sender, Hooks.Wiring.AnnouncementBoxEventArgs args)
		{
			if (_hookManager.InvokeWireTriggerAnnouncementBox(
				Wiring.CurrentUser,
				args.X,
				args.Y,
				args.SignId,
				Main.sign[args.SignId].text))
			{
				args.Result = HookResult.Cancel;
			}
		}
	}
}
