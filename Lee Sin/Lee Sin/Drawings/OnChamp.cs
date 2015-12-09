using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Lee_Sin.Misc;
using Lee_Sin.WardManager;
using SharpDX;
using Color = System.Drawing.Color;

namespace Lee_Sin.Drawings
{
    class OnChamp : LeeSin
    {
        public  static List<Geometry.Polygon.Rectangle> _toList;

        public static List<Geometry.Polygon.Rectangle> NewPoly { get; set; }

        public static void DrawRect()
        {
            for (var a = 0; a < 360f; a ++)
            {
                foreach (var t in HeroManager.Enemies)
                {   
                    var direction = t.Direction.To2D().Perpendicular();
                    var angle = Geometry.DegreeToRadian(a);
                    var rotatedPosition = t.ServerPosition.To2D() + 300*direction.Rotated(angle);
                    var extended = rotatedPosition.Extend(t.ServerPosition.To2D(), rotatedPosition.Distance(t.ServerPosition) + 300);
                    var extend = t.ServerPosition.Extend(rotatedPosition.To3D(), 1100);
                    
                    var s = new Geometry.Polygon.Rectangle(t.ServerPosition, extend, t.BoundingRadius);
                    var targets = HeroManager.Enemies.Where(x => s.IsInside(x.ServerPosition + x.BoundingRadius));
                    if (targets.Count() >= 2)
                    {

                      //  Render.Circle.DrawCircle(extended.To3D(), 100, Color.Blue);
                        if (Player.Distance(extended) < 400)
                        {
                          //  WardJump.WardJumped(extended.To3D(), true, true);
                            Player.Spellbook.CastSpell(Player.GetSpellSlot("SummonerFlash"), extended.To3D(), true);
                        }
                        if (Player.Distance(extended) < 80)
                        {                           
                            R.Cast(t);
                        }
                    }

                }
            }
        }


        public static void OnSpells(EventArgs args) 
        {
            if (Player.IsDead) return;

            if (UltPoly != null && GetBool("rpolygon", typeof(bool)))
            {
                UltPoly.Draw(Color.Red);
            }

           // DrawRect();
          

            if (_rCombo != null && GetBool("rpolygon", typeof(bool))) Render.Circle.DrawCircle((Vector3)_rCombo, 100, Color.Red, 5, true);

            if (GetBool("counthitr", typeof(bool)))
            {
                var getresults = BubbaKush.GetPositions(Player, 1125, (byte)GetValue("enemiescount"), HeroManager.Enemies.Where(x => x.Distance(Player) < 1200).ToList());
                if (getresults.Count > 1)
                {
                    var getposition = BubbaKush.SelectBest(getresults, Player);
                 
                 //   Render.Circle.DrawCircle(getposition, 100, Color.Red, 3, true);
                }
            }


            if (!GetBool("spellsdraw", typeof(bool))) return;
            if (!GetBool("ovdrawings", typeof(bool))) return;
            if (GetBool("qrange", typeof(bool)) && Q.Level > 0)
            {
                var color1 = Q.IsReady() ? Color.DodgerBlue : Color.Red;
                Render.Circle.DrawCircle(Player.Position, Q.Range, color1, 2);
            }

            if (GetBool("wrange", typeof(bool)) && W.Level > 0)
            {
                var colorw = W.IsReady() ? Color.BlueViolet : Color.Red;
                Render.Circle.DrawCircle(Player.Position, W.Range, colorw, 2);
            }

            if (GetBool("erange", typeof(bool)) && E.Level > 0)
            {
                var colore = E.IsReady() ? Color.Plum : Color.Red;
                Render.Circle.DrawCircle(Player.Position, E.Range, colore, 2, true);
            }

            if (GetBool("rrange", typeof(bool)) && R.Level > 0)
            {
                var colorr = R.IsReady() ? Color.LawnGreen : Color.Red;
                Render.Circle.DrawCircle(Player.Position, R.Range, colorr, 2, true);
            }
            var target = HeroManager.Enemies.Where(x => x.Distance(Player) < R.Range && !x.IsDead && x.IsValidTarget(R.Range)).OrderBy(x => x.Distance(Player)).FirstOrDefault();
            if (target == null || Player.IsDead)
            {
                UltPoly = null;
                _ultPolyExpectedPos = null;
                return;
            }

            UltPoly = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(target.Position, 1100), target.BoundingRadius + 20);
            if (GetBool("counthitr", typeof(bool)))
            {
                var counts = HeroManager.Enemies.Where(x => x.Distance(Player) < 1200 && x.IsValidTarget(1200)).Count(h => h.NetworkId != target.NetworkId && UltPoly.IsInside(h.ServerPosition));

                Drawing.DrawText(Drawing.WorldToScreen(Player.Position).X - 50, Drawing.WorldToScreen(Player.Position).Y + 30, Color.Magenta, "Ult Will Hit " + counts);
            }
        }
        
    }
}
