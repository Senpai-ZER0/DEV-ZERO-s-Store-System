using Sandbox.ModAPI;
using VRage.Game.Components;
using ZeroStoreSystem.Config;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using ZeroStoreSystem.UI.Admin;
using RichHudFramework.Client;

namespace ZeroStoreSystem.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class EconomySession : MySessionComponentBase
    {
        public static EconomySession Instance { get; private set; }
        public GlobalStoreConfig GlobalConfig { get; private set; }
        public StoreAdminRhfEditor AdminEditor { get; private set; }

        private bool _chatRegistered;

        public override void LoadData()
        {
            Instance = this;
            GlobalConfig = GlobalStoreConfigManager.GetDefaultConfig();
            Log.Info("Session loaded.");
        }

        public override void BeforeStart()
        {
            if (MyAPIGateway.Utilities != null && !_chatRegistered && !MyAPIGateway.Utilities.IsDedicated)
            {
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
                _chatRegistered = true;
            }

            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                RichHudClient.Init("ZERO Store System", OnRichHudReady, OnRichHudReset);
            }
        }

        protected override void UnloadData()
        {
            if (_chatRegistered && MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
                _chatRegistered = false;
            }

            if (!MyAPIGateway.Utilities.IsDedicated)
                RichHudClient.Reset();

            if (AdminEditor != null)
            {
                AdminEditor.Close();
                AdminEditor = null;
            }

            Log.Info("Session unloaded.");
            GlobalConfig = null;
            Instance = null;
        }

        private void OnRichHudReady()
        {
            AdminEditor = new StoreAdminRhfEditor();
            AdminEditor.Init();
            Log.Info("RHF admin editor initialized.");

            // Make the editor immediately available in the RHF terminal for admins
            // so opening via chat command remains optional.
            if (AdminAccess.IsLocalAdminOrHigher() && AdminEditor != null)
            {
                try
                {
                    AdminEditor.OpenForLocalAdmin();
                    RichHudFramework.UI.Client.RichHudTerminal.CloseMenu();
                    Log.Info("RHF admin editor registered in terminal for local admin.");
                }
                catch (System.Exception e)
                {
                    Log.Error("Failed to register RHF admin editor automatically: " + e);
                }
            }
        }

        private void OnRichHudReset()
        {
            if (AdminEditor != null)
            {
                AdminEditor.Close();
                AdminEditor = null;
            }

            Log.Info("RHF admin editor reset.");
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return;

            string trimmed = messageText.Trim();
            if (!trimmed.StartsWith("/zstore", System.StringComparison.OrdinalIgnoreCase))
                return;

            sendToOthers = false;

            string[] parts = trimmed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1 || (parts.Length >= 2 && parts[1].Equals("editor", System.StringComparison.OrdinalIgnoreCase)))
            {
                if (AdminEditor != null)
                    AdminEditor.OpenForLocalAdmin();
                else if (MyAPIGateway.Utilities != null)
                    MyAPIGateway.Utilities.ShowMessage("ZERO Store", "RHF editor is not ready yet.");
                return;
            }

            if (parts.Length >= 2 && parts[1].Equals("help", System.StringComparison.OrdinalIgnoreCase))
            {
                if (MyAPIGateway.Utilities != null)
                    MyAPIGateway.Utilities.ShowMessage("ZERO Store", "Commands: /zstore editor");
            }
        }
    }
}
