using OTAPI;
using System;
using Terraria;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.Net;
using Terraria.Net.Sockets;

namespace TerrariaApi.Server.Hooking
{
	internal class NetHooks
	{
		private static HookManager _hookManager;

		public static readonly object syncRoot = new object();

		/// <summary>
		/// Attaches any of the OTAPI Net hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			On.Terraria.Net.NetManager.SendData += OnSendNetData;
			On.Terraria.NetMessage.greetPlayer += OnGreetPlayer;
			On.Terraria.Netplay.OnConnectionAccepted += OnConnectionAccepted;
			On.Terraria.Chat.ChatHelper.BroadcastChatMessage += OnBroadcastChatMessage;

			Hooks.NetMessage.SendData += OnSendData;
			Hooks.MessageBuffer.GetData += OnReceiveData;
			Hooks.NetMessage.SendBytes += OnSendBytes;
			Hooks.MessageBuffer.NameCollision += OnNameCollision;
		}

		private static void OnSendNetData(
			On.Terraria.Net.NetManager.orig_SendData orig,
			Terraria.Net.NetManager self,
			ISocket socket,
			NetPacket packet)
		{
			if (!_hookManager.InvokeNetSendNetData(ref self, ref socket, ref packet))
			{
				orig(self, socket, packet);
			}
		}

		private static void OnGreetPlayer(On.Terraria.NetMessage.orig_greetPlayer orig, int plr)
		{
			if (!_hookManager.InvokeNetGreetPlayer(plr))
			{
				orig(plr);
			}
		}

		private static void OnConnectionAccepted(
			On.Terraria.Netplay.orig_OnConnectionAccepted orig,
			ISocket client)
		{
			int slot = FindNextOpenClientSlot();
			if (slot != -1)
			{
				Netplay.Clients[slot].Reset();
				Netplay.Clients[slot].Socket = client;
			}

			if (FindNextOpenClientSlot() == -1)
			{
				Netplay.StopListening();
			}
		}

		private static void OnBroadcastChatMessage(
			On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessage orig,
			NetworkText text,
			Color color,
			int excludedplayer)
		{
			float r = color.R, g = color.G, b = color.B;

			var cancel = _hookManager.InvokeServerBroadcast(ref text, ref r, ref g, ref b);

			color.R = (byte)r;
			color.G = (byte)g;
			color.B = (byte)b;

			if (!cancel)
			{
				orig(text, color, excludedplayer);
			}
		}

		private static void OnSendData(object sender, Hooks.NetMessage.SendDataEventArgs args)
		{
			if (args.Event == HookEvent.After)
			{
				return;
			}

			int bufferId = args.BufferId;
			int msgType = args.MsgType;
			int remoteClient = args.RemoteClient;
			int ignoreClient = args.IgnoreClient;
			NetworkText text = args.Text;
			int number = args.Number;
			float number2 = args.Number2;
			float number3 = args.Number3;
			float number4 = args.Number4;
			int number5 = args.Number5;
			int number6 = args.Number6;
			int number7 = args.Number7;
			if (_hookManager.InvokeNetSendData(ref msgType, ref remoteClient, ref ignoreClient, ref text, ref number, ref number2, ref number3, ref number4, ref number5, ref number6, ref number7))
			{
				args.Result = HookResult.Cancel;
			}

			args.BufferId = bufferId;
			args.MsgType = msgType;
			args.RemoteClient = remoteClient;
			args.IgnoreClient = ignoreClient;
			args.Text = text;
			args.Number = number;
			args.Number2 = number2;
			args.Number3 = number3;
			args.Number4 = number4;
			args.Number5 = number5;
			args.Number6 = number6;
			args.Number7 = number7;
		}

		private static void OnReceiveData(object sender, Hooks.MessageBuffer.GetDataEventArgs args)
		{
			byte packetId = args.PacketId;
			int readOffset = args.ReadOffset;
			int start = args.Start;
			int length = args.Length;
			if (!Enum.IsDefined(typeof(PacketTypes), (int) packetId))
			{
				args.Result = HookResult.Cancel;
				return;
			}

			if (_hookManager.InvokeNetGetData(ref packetId, args.Instance, ref readOffset, ref length))
			{
				args.Result = HookResult.Cancel;
			}

			args.PacketId = packetId;
			args.ReadOffset = readOffset;
			args.Start = start;
			args.Length = length;
		}

		private static void OnSendBytes(object sender, Hooks.NetMessage.SendBytesEventArgs args)
		{
			int remoteClient = args.RemoteClient;
			byte[] data = args.Data;
			int offset = args.Offset;
			int size = args.Size;
			Terraria.Net.Sockets.SocketSendCallback callback = args.Callback;
			object state = args.State;
			if (_hookManager.InvokeNetSendBytes(Netplay.Clients[remoteClient], data, offset, size))
			{
				args.Result = HookResult.Cancel;
			}

			args.RemoteClient = remoteClient;
			args.Data = data;
			args.Offset = offset;
			args.Size = size;
			args.Callback = callback;
			args.State = state;
		}

		private static void OnNameCollision(object sender, Hooks.MessageBuffer.NameCollisionEventArgs args)
		{
			if (args.Event == HookEvent.After)
			{
				return;
			}

			if (_hookManager.InvokeNetNameCollision(args.Player.whoAmI, args.Player.name))
			{
				args.Result = HookResult.Cancel;
			}
		}

		static int FindNextOpenClientSlot()
		{
			lock (syncRoot)
			{
				for (int i = 0; i < Main.maxNetPlayers; i++)
				{
					if (!Netplay.Clients[i].IsConnected())
					{
						return i;
					}
				}
			}
			return -1;
		}
	}
}
