﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Lee_Sin.ActiveModes;
using Lee_Sin.Misc;

namespace Lee_Sin 
{
    class OnUpdate : LeeSin
    {
        public static void OnUpdated(EventArgs args)
        {
            ProcessHandler.ProcessHandlers();

            if (Player.IsRecalling() || MenuGUI.IsChatOpen) return;

            if (GetBool("smiteenable", typeof(KeyBind)))
            {
                ActiveModes.Smite.AutoSmite();
            }
            if (GetBool("wardjump", typeof(KeyBind)))
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                WardManager.WardJump.WardJumped(Player.Position.Extend(Game.CursorPos, 590), true);
            }

            if (GetBool("wardinsec", typeof(KeyBind)))
            {
                Insec.InsecTo.insec();
            }

            if (GetBool("starcombo", typeof(KeyBind)))
            {
                ActiveModes.Star.StarCombo();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ActiveModes.ComboMode.Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear.Lane();
                    LaneClear.Lane2();
                    JungleClear.Jungle();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass.Harassed();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LaneClear.LastHit();
                    break;
            }

            AutoUlt.AutoUlti();
        }
    }
}
