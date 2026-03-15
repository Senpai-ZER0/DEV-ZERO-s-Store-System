using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Input;
using ZeroStoreSystem.Config.Models;
using ZeroStoreSystem.Core;
using RichHudFramework.Client;
using ZeroStoreSystem.UI.Admin;

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
        private bool _ctrlBPressedLastTick;
        private int _retryCounter;

        public override void LoadData()
        {
            Instance = this;
            GlobalConfig = new GlobalStoreConfig();
            AdminEditor = new StoreAdminRhfEditor();
            Log.Info("Session loaded.");
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
            EnsureChatHook();
            EnsureRhfInit();
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();

            EnsureChatHook();
            EnsureRhfInit();

            if (AdminEditor != null)
            {
                if (RichHudClient.Registered && !AdminEditor.Ready)
                    TryInitEditor();

                AdminEditor.RefreshAdminAccess();
            }

            HandleCtrlBHotkey();

            // Periodic retry in case RHF comes up later than expected
            _retryCounter++;
            if (_retryCounter >= 120)
            {
                _retryCounter = 0;
                if (AdminEditor != null && RichHudClient.Registered && !AdminEditor.Ready)
                    TryInitEditor();
            }
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

        private void EnsureChatHook()
        {
            if (MyAPIGateway.Utilities != null && !_chatHooked)
            {
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
                _chatHooked = true;
                Log.Info("Chat hook attached.");
            }
        }

        private void EnsureRhfInit()
        {
            if (!_rhfInitRequested)
            {
                _rhfInitRequested = true;
                Log.Info("RHF init requested.");
                RichHudClient.Init("ZERO Store System", OnRichHudReady, OnRichHudReset);
            }
        }

        private void OnRichHudReady()
        {
            Log.Info("RHF client ready.");
            TryInitEditor();
        }

        private void OnRichHudReset()
        {
            Log.Info("RHF client reset.");
            if (AdminEditor != null)
                AdminEditor.Close();
        }

        private void TryInitEditor()
        {
            try
            {
                if (AdminEditor != null && !AdminEditor.Ready)
                {
                    Log.Info("Initializing RHF admin editor.");
                    AdminEditor.Init();
                }
            }
            catch (System.Exception e)
            {
                Log.Error("Failed to initialize RHF admin editor: " + e);
            }
        }

        private void HandleCtrlBHotkey()
        {
            if (MyAPIGateway.Input == null || AdminEditor == null)
                return;

            bool ctrlHeld = MyAPIGateway.Input.IsAnyCtrlKeyPressed();
            bool bPressed = MyAPIGateway.Input.IsKeyPress(MyKeys.B);
            bool comboPressed = ctrlHeld && bPressed;

            if (comboPressed && !_ctrlBPressedLastTick)
            {
                if (!AdminAccess.IsLocalAdminOrHigher())
                {
                    if (MyAPIGateway.Utilities != null)
                        MyAPIGateway.Utilities.ShowMessage("ZERO Store", "Admin access required.");
                }
                else if (!AdminEditor.OpenForLocalAdmin() && MyAPIGateway.Utilities != null)
                {
                    MyAPIGateway.Utilities.ShowMessage("ZERO Store", "RHF editor is not ready yet.");
                }
            }

            _ctrlBPressedLastTick = comboPressed;
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return;

            string msg = messageText.Trim();
            if (!msg.StartsWith("/zstore"))
                return;

            sendToOthers = false;
            Log.Info("Received command: " + msg);

            if (!AdminAccess.IsLocalAdminOrHigher())
            {
                if (MyAPIGateway.Utilities != null)
                    MyAPIGateway.Utilities.ShowMessage("ZERO Store", "Admin access required.");
                return;
            }

            if (AdminEditor == null)
            {
                if (MyAPIGateway.Utilities != null)
                    MyAPIGateway.Utilities.ShowMessage("ZERO Store", "Editor is not initialized yet.");
                return;
            }

            if (!AdminEditor.Ready)
                TryInitEditor();

            if (!AdminEditor.OpenForLocalAdmin() && MyAPIGateway.Utilities != null)
                MyAPIGateway.Utilities.ShowMessage("ZERO Store", "RHF editor is not ready yet.");
        }
    }
}
