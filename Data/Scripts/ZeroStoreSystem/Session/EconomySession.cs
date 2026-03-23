using Sandbox.ModAPI;
using VRage.Game.Components;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using RichHudFramework.Client;
using ZeroStoreSystem.UI.Admin;
using ZeroStoreSystem.ShipOffers;
using VRage.Input;

namespace ZeroStoreSystem.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class EconomySession : MySessionComponentBase
    {
        public static EconomySession Instance { get; private set; }
        public GlobalStoreConfig GlobalConfig { get; private set; }
        public StoreAdminRhfEditor AdminEditor { get; private set; }

        private bool _chatHooked;
        private bool _rhfInitRequested;

        public override void LoadData()
        {
            Instance = this;
            StoreItemCatalog.Invalidate();
            ShipStoreOfferCatalog.Invalidate();
            GlobalConfig = new GlobalStoreConfig();
            AdminEditor = new StoreAdminRhfEditor();
            Log.Info("Session loaded.");
        }

        public override void BeforeStart()
        {
            base.BeforeStart();

            if (MyAPIGateway.Utilities != null && !_chatHooked)
            {
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
                _chatHooked = true;
            }

            if (!_rhfInitRequested)
            {
                _rhfInitRequested = true;
                RichHudClient.Init("ZERO Store System", OnRichHudReady, OnRichHudReset);
            }
        }


        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();

            if (AdminEditor != null)
            {
                if (RichHudClient.Registered && !AdminEditor.Ready)
                    AdminEditor.Init();

                AdminEditor.RefreshAdminAccess();
            }

            HandleHotkeys();
        }

        protected override void UnloadData()
        {
            if (_chatHooked && MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
                _chatHooked = false;
            }

            try
            {
                RichHudClient.Reset();
            }
            catch
            {
            }

            Log.Info("Session unloaded.");
            AdminEditor = null;
            GlobalConfig = null;
            Instance = null;
        }


        private void HandleHotkeys()
        {
            if (MyAPIGateway.Input == null || MyAPIGateway.Gui == null)
                return;

            if (!AdminAccess.IsLocalAdminOrHigher())
                return;

            if (!MyAPIGateway.Input.IsAnyCtrlKeyPressed() || !MyAPIGateway.Input.IsNewKeyPressed(MyKeys.B))
                return;

            if (AdminEditor == null)
            {
                Notify("Editor is not initialized yet.");
                return;
            }

            if (!AdminEditor.OpenForLocalAdmin())
                Notify("RHF editor is not ready yet.");
        }

        private static void Notify(string message)
        {
            if (MyAPIGateway.Utilities != null && !string.IsNullOrWhiteSpace(message))
                MyAPIGateway.Utilities.ShowMessage("ZERO Store", message);
        }

        private void OnRichHudReady()
        {
            Log.Info("RHF client ready.");
            if (AdminEditor != null)
                AdminEditor.Init();
        }

        private void OnRichHudReset()
        {
            Log.Info("RHF client reset.");
            if (AdminEditor != null)
                AdminEditor.Close();
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return;

            string msg = messageText.Trim();
            if (!msg.StartsWith("/zstore"))
                return;

            sendToOthers = false;

            if (!AdminAccess.IsLocalAdminOrHigher())
            {
                Notify("Admin access required.");
                return;
            }

            if (AdminEditor == null)
            {
                Notify("Editor is not initialized yet.");
                return;
            }

            if (!AdminEditor.OpenForLocalAdmin())
                Notify("RHF editor is not ready yet.");
        }
    }
}
