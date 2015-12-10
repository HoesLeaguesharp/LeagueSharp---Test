﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Prediction = Lee_Sin.Prediction;

namespace Lee_Sin.Insec
{
    class InsecTo : LeeSin
    {
        public static void insec()
        {
            #region Target, Slots, Prediction

            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            var target = TargetSelector.GetTarget(Q.Range + 800, TargetSelector.DamageType.Physical);
            if (target != null)
            {
                target = TargetSelector.GetSelectedTarget() == null ? target : TargetSelector.SelectedTarget;
            }
            // if (!R.IsReady() && Environment.TickCount - lastr > 2000) return;
            
            if (target == null) return;

            LastQ(target);
        //   Game.PrintChat(LastQ(target).ToString());
         //   Game.PrintChat((Environment.TickCount - lastq12).ToString());

            var qpred = Q.GetPrediction(target);

            var col = qpred.CollisionObjects;

            var slot = Items.GetWardSlot();
            //   Game.PrintChat(LastQ(target).ToString());
            #endregion

            if (Player.Distance(target) > 500)
            {
                if (Q2() && Q.IsReady() && (R.IsReady() || Environment.TickCount - lastr < 5000))
                {
                    Utility.DelayAction.Add(400, () => Q.Cast());
                }
            }

            if (Q1() && Player.Distance(target) <= Q.Range)
            {
                OnUpdate.CastSpell(Q, target);
            }


            var poss = InsecPos.WardJumpInsecPosition.InsecPos(target, GetValue("fixedwardrange"), true);

            foreach (var min in
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        x => (!x.IsAlly || x.Type == GameObjectType.NeutralMinionCamp) && (!x.Name.ToLower().Contains("turret")) &&
                            (x.Distance(target) < 380 || x.Distance(poss) < 530 || (CanWardFlash(target) && x.Distance(target) < 800))
                             && x.Health > Q.GetDamage(x) + 50 && !x.IsDead &&
                             Q.GetPrediction(x).CollisionObjects.Count == 0 && x.Distance(Player) < Q.Range))
            {
               // minionss = min;
               Render.Circle.DrawCircle(min.Position, 80, Color.Yellow, 5, true);
                if (Q1() && Q.IsReady())
                {
                    Q.Cast(min.Position);
                }
                if (Q1() && Q.IsReady())
                {
                    Q.Cast(min.Position);
                }

                if (Q2() && min.HasBuff("blindmonkqtwo"))
                {
                    Q.Cast();
                }
            }

            if ((Steps == LeeSin.steps.WardJump || Environment.TickCount - _lastwardjump < 1500) && slot != null && W.IsReady() && R.IsReady())
            {
                
                if (target.Position.Distance(Player.Position) < 600)
                {
                    WardManager.WardJump.WardJumped(poss.To3D(), false, false);
                    canwardflash = false;
                    
                }
                else if (CanWardFlash(target))
                {
                    canwardflash = true;
                }
            }

            if (Environment.TickCount - _lastprocessw < 1500 || Steps == LeeSin.steps.Flash ||
                Environment.TickCount - _lastwcasted < 1500)
            {
                if (R.IsReady())
                    R.Cast(target);
            }

            if ((!W.IsReady() || W2()) && !GetBool("prioflash", typeof(bool)) &&
                Environment.TickCount - _lastwcasted > 1000 && LastQ(target))
            {
                lastflashoverprio = Environment.TickCount;
                R.Cast(target);
            }

            #region Determine if we want to flash or ward jump

            if (R.IsReady())
            {
                if (slot != null && (W.IsReady() && Environment.TickCount - _lastprocessw > 1500))
                {
                    if (GetBool("prioflash", typeof(bool)) && Player.GetSpellSlot("summonerflash").IsReady())
                    {
                        Steps = LeeSin.steps.Flash;
                    }
                    else if (Player.Distance(target) < 700)
                    {
                        Steps = LeeSin.steps.WardJump;
                        if (Environment.TickCount - _lastqcasted1 < 600)
                        {
                            canwardflash = false;
                        }
                        else if (CanWardFlash(target))
                        {
                            canwardflash = true;
                        }
                        _lastwardjump = Environment.TickCount;
                    }
                }
                else if (GetBool("useflash", typeof(bool)) && target.Distance(Player) < 400 &&
                         Player.GetSpellSlot("SummonerFlash").IsReady() && (slot == null || !W.IsReady() || W2()) &&
                         (Environment.TickCount - _lastwcasted > 1500 || Environment.TickCount - _lastprocessw > 1500))
                {
                    Steps = LeeSin.steps.Flash;
                }
            }

            var wardtotargetpos = Player.Position.Extend(target.Position, Player.Distance(target) - 250);

            if (!canwardflash) return;

            if (Player.ServerPosition.Distance(target.ServerPosition) < 250 || target.Distance(Player) > 750
                || !CanWardFlash(target) || Environment.TickCount - _lastq1casted < 1000 || target.Buffs.Any(x => x.Name.ToLower().Contains("blindmonkqone")))
                return;

            if (LastQ(target))
            {
                if (!(Player.Distance(target) < 500))
                {
                    WardManager.WardJump.WardJumped(wardtotargetpos, false, false);

                    _wardjumpedto = Environment.TickCount;
                    _wardjumpedtotarget = true;
                    _lastflashward = Environment.TickCount;
                }
                else
                {
                    return;
                }
            }

            #endregion

            #region Q Smite

            var prediction = Prediction.GetPrediction(target, Q.Delay);
           
            var collision = Q.GetCollision(Player.Position.To2D(),
                new List<Vector2> { prediction.UnitPosition.To2D() });

            foreach (var collisions in collision)
            {
                if (collision.Count == 1)
                {
                    if (collision[0].IsMinion && collision[0].IsEnemy)
                    {
                        if (!GetBool("UseSmite", typeof (bool))) return;
                        if (Q.IsReady())
                        {
                            if (collision[0].Distance(Player) < 500)
                            {
                                if (collision[0].Health <= ActiveModes.Smite.GetFuckingSmiteDamage() && Smite.IsReady())
                                {
                                    Q.Cast(prediction.CastPosition);
                                    Player.Spellbook.CastSpell(Smite, collision[0]);
                                }
                            }
                        }
                    }
                }
            }

            #endregion
        }
    }
}
