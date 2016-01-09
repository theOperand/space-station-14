﻿using SS14.Client.Graphics.Sprite;
using SS14.Client.GameObjects;
using SS14.Client.Interfaces.Resource;
using SS14.Shared;
using SS14.Shared.GameObjects;
using SS14.Shared.GO;
using SS14.Shared.Maths;
using System;
using System.Drawing;
using SFML.Window;
using SFML.Graphics;
using SS14.Client.Graphics;
using Color = SFML.Graphics.Color;

namespace SS14.Client.Services.UserInterface.Components
{
    public struct PartHp
    {
        public int currHP;
        public int maxHP;
    }

    internal class HealthScannerWindow : GuiComponent
    {
        private readonly Sprite _arml;
        private readonly Sprite _armr;
        private readonly Sprite _background;
        private readonly Sprite _chest;
        private readonly Sprite _groin;
        private readonly Sprite _head;
        private readonly Sprite _legl;
        private readonly Sprite _legr;
        private readonly TextSprite _overallHealth;
        private readonly IResourceManager _resourceManager;
        private readonly UserInterfaceManager _uiMgr;

        private readonly Entity assigned;

        private bool dragging;

        public HealthScannerWindow(Entity assignedEnt, Vector2 mousePos, UserInterfaceManager uiMgr,
                                   IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            assigned = assignedEnt;
            _uiMgr = uiMgr;

            _overallHealth = new TextSprite("hpscan" + assignedEnt.Uid.ToString(), "",
                                            _resourceManager.GetFont("CALIBRI"));
            _overallHealth.Color = new SFML.Graphics.Color(34, 139, 34);

            _background = _resourceManager.GetSprite("healthscan_bg");

            _head = _resourceManager.GetSprite("healthscan_head");
            _chest = _resourceManager.GetSprite("healthscan_chest");
            _arml = _resourceManager.GetSprite("healthscan_arml");
            _armr = _resourceManager.GetSprite("healthscan_armr");
            _groin = _resourceManager.GetSprite("healthscan_groin");
            _legl = _resourceManager.GetSprite("healthscan_legl");
            _legr = _resourceManager.GetSprite("healthscan_legr");

            Position = new Point((int) mousePos.X, (int) mousePos.Y);

            Setup();
            Update(0);
        }

        private Color GetColor(int curr, int max)
        {
            float healthPct = curr/(float) max;

            if (healthPct > 0.75) return new Color(128, 255, 128);
            else if (healthPct > 0.50) return new Color(255, 255, 0);
            else if (healthPct > 0.25) return new Color(192, 64, 0);
            else if (healthPct > 0) return new Color(255, 0, 0);
            else return new Color(64, 64, 64);
        }

        private void Setup()
        {
            ComponentReplyMessage reply = assigned.SendMessage(this, ComponentFamily.Damageable,
                                                               ComponentMessageType.GetCurrentLocationHealth,
                                                               BodyPart.Head);

            Color temp = GetColor((int) reply.ParamsList[1], (int) reply.ParamsList[2]);

            if (reply.MessageType == ComponentMessageType.CurrentLocationHealth)
                _head.Color = temp;

            reply = assigned.SendMessage(this, ComponentFamily.Damageable, ComponentMessageType.GetCurrentLocationHealth,
                                         BodyPart.Torso);
            if (reply.MessageType == ComponentMessageType.CurrentLocationHealth)
                _chest.Color = temp;

            reply = assigned.SendMessage(this, ComponentFamily.Damageable, ComponentMessageType.GetCurrentLocationHealth,
                                         BodyPart.Left_Arm);
            if (reply.MessageType == ComponentMessageType.CurrentLocationHealth)
                _arml.Color = temp;

            reply = assigned.SendMessage(this, ComponentFamily.Damageable, ComponentMessageType.GetCurrentLocationHealth,
                                         BodyPart.Right_Arm);
            if (reply.MessageType == ComponentMessageType.CurrentLocationHealth)
                _armr.Color = temp;

            reply = assigned.SendMessage(this, ComponentFamily.Damageable, ComponentMessageType.GetCurrentLocationHealth,
                                         BodyPart.Groin);
            if (reply.MessageType == ComponentMessageType.CurrentLocationHealth)
                _groin.Color = temp;

            reply = assigned.SendMessage(this, ComponentFamily.Damageable, ComponentMessageType.GetCurrentLocationHealth,
                                         BodyPart.Left_Leg);
            if (reply.MessageType == ComponentMessageType.CurrentLocationHealth)
                _legl.Color = temp;

            reply = assigned.SendMessage(this, ComponentFamily.Damageable, ComponentMessageType.GetCurrentLocationHealth,
                                         BodyPart.Right_Leg);
            if (reply.MessageType == ComponentMessageType.CurrentLocationHealth)
                _legr.Color = temp;

            if (assigned.HasComponent(ComponentFamily.Damageable))
            {
                var comp = (HealthComponent) assigned.GetComponent(ComponentFamily.Damageable);
                _overallHealth.Text = comp.GetHealth().ToString() + " / " + comp.GetMaxHealth().ToString();
            }
        }

        public override sealed void Update(float frameTime)
        {
            _background.Position = new Vector2(Position.X, Position.Y); 

            _head.Position = new Vector2 (Position.X, Position.Y);
            _chest.Position = new Vector2(Position.X, Position.Y);
            _arml.Position = new Vector2(Position.X, Position.Y); 
            _armr.Position = new Vector2(Position.X, Position.Y); 
            _groin.Position = new Vector2(Position.X, Position.Y);
            _legl.Position = new Vector2(Position.X, Position.Y); 
            _legr.Position = new Vector2(Position.X, Position.Y);

            _overallHealth.Position = new Vector2(Position.X + 86, Position.Y + 29);

            var bounds = _background.GetLocalBounds();
            ClientArea = new Rectangle(Position, new Size((int)bounds.Width, (int)bounds.Height));
        }

        public override void Render()
        {
            _background.Draw();

            _head.Draw();
            _chest.Draw();
            _arml.Draw();
            _armr.Draw();
            _groin.Draw();
            _legl.Draw();
            _legr.Draw();

            _overallHealth.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            if (dragging) Position = new Point( e.X, e.Y);
        }

        public override bool MouseDown(MouseButtonEventArgs e)
        {
            if (ClientArea.Contains(new Point((int) e.X, (int) e.Y)))
            {
                var insidePos = new Vector2((int) e.X , (int) e.Y );
                if ((insidePos - new Vector2(189, 9)).Length <= 5)
                {
                    _uiMgr.RemoveComponent(this);
                    Dispose();
                }
                else if (insidePos.Y <= 18) dragging = true;
                return true;
            }
            return false;
        }

        public override bool MouseUp(MouseButtonEventArgs e)
        {
            if (dragging)
            {
                dragging = false;
                return true;
            }
            return false;
        }
    }
}