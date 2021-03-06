﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Lee_Sin.Misc;
using SharpDX;
using Color = System.Drawing.Color;

namespace Lee_Sin
{
    class EventHandler : LeeSin
    {

        public static void OnCreate(GameObject sender, EventArgs args)
        {
            

            if (!GetBool("wardinsec", typeof(KeyBind)) && !GetBool("starcombo", typeof(KeyBind)) &&
                Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Environment.TickCount - lastcanjump > 2000)
                return;

            if (_processW2 || !W.IsReady() || Player.GetSpell(SpellSlot.W).Name != "BlindMonkWOne" ||
                Player.Spellbook.GetSpell(SpellSlot.Q).Name == "blindmonkwtwo")
                return;
            if (sender.Name.ToLower().Contains("ward"))
            {
                var ward = (Obj_AI_Base)sender;
                var target = TargetSelector.GetTarget(Q.Range + 800, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    target = TargetSelector.GetSelectedTarget() == null ? target : TargetSelector.SelectedTarget;
                }

                if (target == null) return;
                var poss = InsecPos.WardJumpInsecPosition.InsecPos(target, GetValue("fixedwardrange"), true);
                if (sender.Position.Distance(poss.To3D()) < 200)
                {
                    lsatcanjump1 = Environment.TickCount;
                }
            }
            if (sender.Name.ToLower().Contains("ward") && W.IsReady() && sender.IsAlly)
            {
               
                _lastwcasted = Environment.TickCount;
                var ward = (Obj_AI_Base)sender;

                if (ward.IsMe) return;
                W.Cast(ward);
                _created = true;
            }
        }


        #region Ally selector 

        public static void OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            var asec = ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy && a.Distance(Game.CursorPos) < 200 && a.IsValid && !a.IsDead);
            if (asec.Any())
            {
                return;
            }
            if (!_lastClickBool || _clickCount == 0)
            {
                _clickCount++;
                _lastClickPos = Game.CursorPos;
                _lastClickBool = true;
                SelectedAllyAiMinion = null;
                SelectedAllyAiMinionv = new Vector3();
                return;
            }

            if (ClickCounts == 0)
            {
                SelectedAllyAiMinionv = new Vector3();
            }


            if (_lastClickBool && _lastClickPos.Distance(Game.CursorPos) < 100)
            {
                _clickCount++;
                _lastClickBool = false;
            }

            SelectedAllyAiMinion =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        x => x.IsValid && x.Distance(Game.CursorPos, true) < 40000 && x.IsAlly && !x.IsMe)
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();

        }

        public static int ClickCounts { get; set; }

        public static Vector3 SelectedAllyAiMinionv
        { get; set; }

        #endregion

        #region processspellcast,

        public static void OnSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var getresults = BubbaKush.GetPositions(Player, 1125, (byte)GetValue("enemiescount"), HeroManager.Enemies.Where(x => x.Distance(Player) < 1200).ToList());
            if (getresults.Count > 1)
            {
                if (GetBool("xeflash", typeof (bool)))
                {
                    if (GetBool("wardinsec", typeof (KeyBind)) ||
                        Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                        return;

                    var getposition = BubbaKush.SelectBest(getresults, Player);
                    if (args.SData.Name == "BlindMonkRKick")
                    {
                        var poss = getposition;

                        Player.Spellbook.CastSpell(Player.GetSpellSlot("SummonerFlash"), poss, true);
                    }
                }
            }
            if (sender.IsMe)
            {
                switch (args.SData.Name)
                {
                    case "BlindMonkQOne":
                    case "blinkmonkqtwo":
                        _junglelastq = Environment.TickCount;
                        break;

                    case "BlindMonkWOne":
                    case "blindmonkwtwo":
                        _junglelastw = Environment.TickCount;
                        break;

                    case "BlindMonkEOne":
                    case "blindmonketwo":
                        _junglelaste = Environment.TickCount;
                        break;
                }
            }

            if (args.SData.Name.ToLower() == "blindmonkqtwo")
            {
                LeeSin._lastq2casted = Environment.TickCount;
            }

            if (args.SData.Name== "BlindMonkQOne")
            {
                LeeSin._lastq1casted = Environment.TickCount;
            }

            if (args.SData.Name == "BlindMonkRKick")
            {
                lastr = Environment.TickCount;
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    target = TargetSelector.GetSelectedTarget() == null ? target : TargetSelector.SelectedTarget;
                }

                if (target != null)
                {
                   if (Environment.TickCount - LeeSin.lsatcanjump1 > 3000)
                    {                    
                        if (Steps == LeeSin.steps.Flash ||
                            (Environment.TickCount - _lastflashward < 2000 && _wardjumpedtotarget) ||
                            Environment.TickCount - lastflashoverprio < 3000 ||
                            Environment.TickCount - _wardjumpedto < 2000) 
                        {
                            if (GetBool("wardinsec", typeof (KeyBind)) || GetBool("starcombo", typeof (KeyBind)))
                            {
                                var pos = InsecPos.FlashInsecPosition.InsecPos(target, 230);
                                var poss = Player.Position.Extend(target.Position,
                                    +target.Position.Distance(Player.Position) + 230);

                                Player.Spellbook.CastSpell(Player.GetSpellSlot("SummonerFlash"),
                                    !GetBool("wardinsec", typeof (KeyBind)) ? poss : pos, true);
                            }
                        }
                    }
                }
            }

            if (sender.IsMe || sender.IsAlly || !sender.IsChampion()) return;

            switch (args.SData.Name)
            {
                case "MonkeyKingDecoy":
                case "AkaliSmokeBomb":
                    if (sender.Distance(Player) < E.Range)
                        E.Cast();
                    break;
            }
        }

        #endregion

        #region On spell cast

        public static void OnSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.W &&
                (GetBool("wardinsec", typeof(KeyBind)) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear))
            {
                _processw = true;
                _lastprocessw = Environment.TickCount;
            }

            if (args.Slot == SpellSlot.Q)
            {
                _lastqcasted = Environment.TickCount;
            }

            if (args.Slot == SpellSlot.Q && Q2())
            {
                _lastqcasted1 = Environment.TickCount;
            }


            if (args.Slot == Player.GetSpellSlot("summonerflash") && GetBool("wardinsec", typeof(KeyBind)))
            {
                _processr = true;
                _lastprocessr = Environment.TickCount;
                lastflashed = Environment.TickCount;
            }

            if (args.Slot == SpellSlot.R && GetBool("wardinsec", typeof(KeyBind)))
            {

                _processr2 = true;
                _processr2T = Environment.TickCount;
                Playerpos = Player.Position;
            }

            if (args.Slot == SpellSlot.W && (GetBool("wardinsec", typeof(KeyBind)) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear))
            {
                _processW2 = true;
            }

            if (args.Slot == SpellSlot.R && GetBool("wardinsec", typeof(KeyBind)))
            {
                _processr2 = true;
                _processr2T = Environment.TickCount;

            }

        }

        #endregion

        #region #Star

        public static
        Vector2 Star(Obj_AI_Hero target)
        {
            return Player.Position.Extend(target.Position, target.Distance(Player) + 300).To2D();
        }

        #endregion

        public static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "blindmonkqtwo" && args.Target.Type == GameObjectType.obj_AI_Hero)
                {

                }
            }
        }
    }
}