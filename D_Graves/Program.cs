#region

using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace D_Graves
{
    using SharpDX;

    internal class Program
    {
        private const string ChampionName = "Graves";

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell _q, _w, _e, _r;

        private static SpellSlot _smiteSlot;

        private static Spell _smite;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static Items.Item _youmuu, _blade, _bilge;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;

            if (_player.ChampionName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 950F);
            _w = new Spell(SpellSlot.W, 950f);
            _e = new Spell(SpellSlot.E, 450f);
            _r = new Spell(SpellSlot.R, 1500f);

            _q.SetSkillshot(0.25f, 60f, 2000f, false, SkillshotType.SkillshotLine);
            _w.SetSkillshot(0.35f, 150f, 1650f, false, SkillshotType.SkillshotCircle);
            _r.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);

            _youmuu = new Items.Item(3142, 10);
            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);

            if (_player.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner1, 570f);
                _smiteSlot = SpellSlot.Summoner1;
            }
            else if (_player.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner2, 570f);
                _smiteSlot = SpellSlot.Summoner2;
            }

            //D Graves
            _config = new Menu("D-Graves", "D-Graves", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E")).SetValue(false);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEreload", "Use E to Reload")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRE", "Use R if Hit X Enemys")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("MinTargets", "Use R if Hit Enemys >=").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("useRaim", "Use R(Semi-Manual)").SetValue(
                        new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(
                        new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(70, 1, 100)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass!").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            //Last Hit
            _config.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Q LastHit")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseWLH", "W LastHit")).SetValue(false);
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(new MenuItem("Lastmana", "Minimum Mana").SetValue(new Slider(70, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "LastHit!").SetValue(
                        new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            //Lane Clear
            _config.SubMenu("Farm").AddSubMenu(new Menu("LaneClear", "LaneClear"));
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseQL", "Q LaneClear")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("minminions", "Minimum minions to use Q").SetValue(new Slider(3, 1, 6)));
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("UseWL", "W LaneClear")).SetValue(false);
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("minminionsw", "Minimum minions to use W").SetValue(new Slider(3, 1, 5)));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(70, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("ActiveLane", "LaneClear!").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            //Jungle clear
            _config.SubMenu("Farm").AddSubMenu(new Menu("JungleClear", "JungleClear"));
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseQJ", "Q Jungle")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("JungleClear").AddItem(new MenuItem("UseWJ", "W Jungle")).SetValue(false);
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("JungleClear")
                .AddItem(
                    new MenuItem("ActiveJungle", "Jungle key").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));


            //items
            //Offensive
            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Youmuu", "Use Youmuu's")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(60, 1, 100)));

            //Deffensive
            _config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            _config.SubMenu("items").SubMenu("Deffensive").AddSubMenu(new Menu("Cleanse", "Cleanse"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("useqss", "Use QSS/Mercurial Scimitar/Dervish Blade"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("blind", "Blind"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("charm", "Charm"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("fear", "Fear"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("flee", "Flee"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("snare", "Snare"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("taunt", "Taunt"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("suppression", "Suppression"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("stun", "Stun"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("polymorph", "Polymorph"))
                .SetValue(false);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("silence", "Silence"))
                .SetValue(false);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("zedultexecute", "Zed Ult"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("Cleansemode", "Use Cleanse"))
                .SetValue(new StringList(new string[2] { "Always", "In Combo" }));

            //potions
            _config.SubMenu("items").AddSubMenu(new Menu("Potions", "Potions"));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "Use Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "If Mana % <").SetValue(new Slider(35, 1, 100)));

            //Smite 
            _config.AddSubMenu(new Menu("Smite", "Smite"));
            _config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(
                        new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Useblue", "Smite Blue Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("manaJ", "Smite Blue Early if MP% <").SetValue(new Slider(30, 1, 100)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(30, 1, 100)));

            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQM", "Use Q KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseWM", "Use W KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRM", "Use R KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Gap_W", "GapClosers W")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Gap_E", "GapClosers E")).SetValue(true);

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(false);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "Draw smite")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawharass", "Draw Auto Harass").SetValue(true));

            _config.AddToMainMenu();
            Game.PrintChat("<font color='#881df2'>D-Graves by Diabaths</font> Loaded.");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.PrintChat(
                "<font color='#f2f21d'>Do you like it???  </font> <font color='#ff1900'>Drop 1 Upvote in Database </font>");
            Game.PrintChat(
                "<font color='#f2f21d'>Buy me cigars </font> <font color='#ff1900'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_config.Item("useRaim").GetValue<KeyBind>().Active && _r.IsReady())
            {
                var t = TargetSelector.GetTarget(_r.Range + 300, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget()) _r.Cast(t, true, true);
            }

            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (!_config.Item("ActiveCombo").GetValue<KeyBind>().Active
                && (_config.Item("ActiveHarass").GetValue<KeyBind>().Active
                    || _config.Item("harasstoggle").GetValue<KeyBind>().Active)
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();
            }

            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                Laneclear();
            }

            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value)
            {
                JungleClear();
            }

            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active
                && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lastmana").GetValue<Slider>().Value)
            {
                LastHit();
            }

            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }

            _player = ObjectManager.Player;

            Orbwalker.SetAttack(true);
            Usecleanse();
            KillSteal();
            Usepotion();
        }

        public static readonly string[] Smitetype =
           {
                "s5_summonersmiteplayerganker", "s5_summonersmiteduel",
                "s5_summonersmitequick", "itemsmiteaoe", "summonersmite"
            };

        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = _config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var useblue = _config.Item("Useblue").GetValue<bool>();
            var usered = _config.Item("Usered").GetValue<bool>();
            var health = (100 * (_player.Health / _player.MaxHealth)) < _config.Item("healthJ").GetValue<Slider>().Value;
            var mana = (100 * (_player.Mana / _player.MaxMana)) < _config.Item("manaJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            }
            else
            {
                jungleMinions = new string[]
                                    {
                                        "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_RiftHerald",
                                        "SRU_Red", "SRU_Krug", "SRU_Dragon_Air", "SRU_Dragon_Water", "SRU_Dragon_Fire",
                                        "SRU_Dragon_Elder", "SRU_Baron"
                                    };
            }

            var minions = MinionManager.GetMinions(_player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (Obj_AI_Base minion in minions)
                {
                    if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline && minion.Health <= smiteDmg
                        && jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name)))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name))
                        && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && useblue && mana && minion.Health >= smiteDmg
                             && jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue"))
                             && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg
                             && jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red"))
                             && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range) && _config.Item("Gap_W").GetValue<bool>()) _w.Cast(gapcloser.Sender.IsMelee() ? _player : gapcloser.Sender);

            if (_e.IsReady() && gapcloser.Sender.Distance(_player.ServerPosition) <= 200
                && _config.Item("Gap_E").GetValue<bool>())
            {
                _e.Cast(ObjectManager.Player.Position.Extend(gapcloser.Sender.Position, -_e.Range));
            }
        }

        private static void Usecleanse()
        {
            if (_player.IsDead
                || (_config.Item("Cleansemode").GetValue<StringList>().SelectedIndex == 1
                    && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)) return;
            if (Cleanse(_player) && _config.Item("useqss").GetValue<bool>())
            {
                if (_player.HasBuff("zedulttargetmark"))
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140)) Utility.DelayAction.Add(1000, () => Items.UseItem(3140));
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139)) Utility.DelayAction.Add(1000, () => Items.UseItem(3139));
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137)) Utility.DelayAction.Add(1000, () => Items.UseItem(3137));
                }
                else
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140)) Items.UseItem(3140);
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139)) Items.UseItem(3139);
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137)) Items.UseItem(3137);
                }
            }
        }

        private static bool Cleanse(Obj_AI_Hero hero)
        {
            var cc = false;
            if (_config.Item("blind").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Blind))
                {
                    cc = true;
                }
            }

            if (_config.Item("charm").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Charm))
                {
                    cc = true;
                }
            }

            if (_config.Item("fear").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Fear))
                {
                    cc = true;
                }
            }

            if (_config.Item("flee").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Flee))
                {
                    cc = true;
                }
            }

            if (_config.Item("snare").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Snare))
                {
                    cc = true;
                }
            }

            if (_config.Item("taunt").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Taunt))
                {
                    cc = true;
                }
            }

            if (_config.Item("suppression").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Suppression))
                {
                    cc = true;
                }
            }

            if (_config.Item("stun").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Stun))
                {
                    cc = true;
                }
            }

            if (_config.Item("polymorph").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Polymorph))
                {
                    cc = true;
                }
            }

            if (_config.Item("silence").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Silence))
                {
                    cc = true;
                }
            }

            if (_config.Item("zedultexecute").GetValue<bool>())
            {
                if (_player.HasBuff("zedulttargetmark"))
                {
                    cc = true;
                }
            }

            return cc;
        }

        private static void Smiteontarget()
        {
            if (_smite == null) return;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var smiteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Smite);
                var usesmite = _config.Item("smitecombo").GetValue<bool>();
                if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker" && usesmite
                    && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready
                    && hero.IsValidTarget(570))
                {
                    if (!hero.HasBuffOfType(BuffType.Stun) || !hero.HasBuffOfType(BuffType.Slow))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                    }
                    else if (smiteDmg >= hero.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                    }
                }

                if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel" && usesmite
                    && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready
                    && hero.IsValidTarget(570))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
            }
        }

        private static void Combo()
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var useR = _config.Item("UseRC").GetValue<bool>();
            var autoR = _config.Item("UseRE").GetValue<bool>();

            if (_config.Item("smitecombo").GetValue<bool>())
            {
                Smiteontarget();
            }

            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(_q.Range-70)) _q.CastIfHitchanceEquals(t, HitChance.High, true);
            }

            if (useW && _w.IsReady())
            {
                var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_w.Range)) _w.CastIfHitchanceEquals(t, HitChance.High, true);
            }

            if (_r.IsReady() && useR)
            {
                var t = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Physical);
                if (t.IsInvulnerable) return;
                if (_q.IsReady() && t.IsValidTarget(_q.Range)
                    && (_q.GetDamage(t) > t.Health || _player.GetAutoAttackDamage(t, true) > t.Health)) return;
                if (_r.GetDamage(t) - 80 > t.Health && t.IsValidTarget(_r.Range))
                {
                    _r.CastIfHitchanceEquals(t, HitChance.High, true);
                }

                if (autoR)
                {
                    var fuckr = _r.GetPrediction(t, true);
                    if (fuckr.AoeTargetsHitCount >= _config.Item("MinTargets").GetValue<Slider>().Value
                        && t.IsValidTarget(_r.Range)) _r.CastIfHitchanceEquals(t, HitChance.High, true);
                }
            }

            UseItemes();
        }
       
        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var mana = _player.ManaPercent > _config.Item("Harrasmana").GetValue<Slider>().Value;
            if (unit.IsMe)
                if (target.Type == GameObjectType.obj_AI_Hero)
                {
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                        || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && mana))
                    {
                        var useQ = _config.Item("UseQC").GetValue<bool>() || _config.Item("UseQH").GetValue<bool>();
                        var useW = _config.Item("UseWC").GetValue<bool>() || _config.Item("UseWH").GetValue<bool>();
                        if (useQ && _q.IsReady())
                        {
                            var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                            if (t.IsValidTarget(_q.Range - 70)) _q.CastIfHitchanceEquals(t, HitChance.High, true);
                        }

                        if (useW && _w.IsReady())
                        {
                            var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                            if (t.IsValidTarget(_w.Range)) _w.CastIfHitchanceEquals(t, HitChance.High, true);
                        }
                    }

                    var useE = _config.Item("UseEC").GetValue<bool>();
                    var ta = TargetSelector.GetTarget(700, TargetSelector.DamageType.Magical);
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && _e.IsReady())
                    {
                        if (ObjectManager.Player.Position.Extend(Game.CursorPos, 700).CountEnemiesInRange(700) <= 1
                            && useE)
                        {
                            if (!ta.UnderTurret()) _e.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, 450));
                            else if (ta.UnderTurret() && _e.IsReady() && ta.IsValidTarget()
                                     && _q.ManaCost + _e.ManaCost < _player.Mana)
                                if (ta.Health < _q.GetDamage(ta) && ta.IsValidTarget())
                                {
                                    _e.Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, 450));
                                    _q.CastIfHitchanceEquals(ta, HitChance.High, true);
                                }
                        }

                        var useEreload = _config.Item("UseEreload").GetValue<bool>();
                        if (!_player.HasBuff("GravesBasicAttackAmmo2") && _e.IsReady() && useEreload)
                        {
                            var direction = (Game.CursorPos - _player.ServerPosition).To2D().Normalized();
                            for (var step = 0f; step < 360; step += 30)
                            {
                                for (var a = 450; a > 0; a -= 50)
                                {
                                    var currentAngle = step * (float)Math.PI / 90;
                                    var currentCheckPoint = _player.ServerPosition.To2D()
                                                            + a * direction.Rotated(currentAngle);
                                    if (currentCheckPoint.To3D().UnderTurret(true) || currentCheckPoint.To3D().IsWall())
                                    {
                                        return;
                                    }

                                    _e.Cast((Vector3)currentCheckPoint);
                                }
                            }
                        }
                    }
                }
        }

        private static void Harass()
        {
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useW = _config.Item("UseWH").GetValue<bool>();

            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(_q.Range - 70)) _q.CastIfHitchanceEquals(t, HitChance.High, true);
            }

            if (useW && _w.IsReady())
            {
                var t = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget(_w.Range)) _w.CastIfHitchanceEquals(t, HitChance.High, true);
            }
        }

        private static void Laneclear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition,
                _w.Range + _w.Width / 2,
                MinionTypes.All);
            var rangedMinionsW = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition,
                _w.Range + _w.Width + 50,
                MinionTypes.Ranged);
            var minionhitq = _config.Item("minminions").GetValue<Slider>().Value;
            var minionhitw = _config.Item("minminionsw").GetValue<Slider>().Value;
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useWl = _config.Item("UseWL").GetValue<bool>();


            if (_q.IsReady() && useQl)
            {
                var fl2 = _q.GetCircularFarmLocation(allMinionsQ, 200);

                if (fl2.MinionsHit >= minionhitq)
                {
                    _q.Cast(fl2.Position);
                }
                else
                {
                    foreach (var minion in allMinionsQ)
                        if (Orbwalking.InAutoAttackRange(minion)
                            && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                        {
                            _q.Cast(minion);
                        }
                        else if (!Orbwalking.InAutoAttackRange(minion)
                                 && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                        {
                            _q.Cast(minion);
                        }
                }
            }

            if (_w.IsReady() && useWl)
            {
                var fl1 = _w.GetCircularFarmLocation(rangedMinionsW, _w.Width);

                if (fl1.MinionsHit >= minionhitw)
                {
                    _w.Cast(fl1.Position);
                }
                else
                    foreach (var minion in allMinionsW)
                        if (!Orbwalking.InAutoAttackRange(minion)
                            && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.W)) _w.Cast(minion);
            }
        }


        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useW = _config.Item("UseWLH").GetValue<bool>();
            if (allMinions.Count < 3) return;
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }

                if (_w.IsReady() && useW && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.W))
                {
                    _w.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(
                _player.ServerPosition,
                _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useW = _config.Item("UseWJ").GetValue<bool>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && _q.IsReady())
                {
                    _q.Cast(mob);
                }
                if (_w.IsReady() && useW)
                {
                    _w.Cast(mob);
                }
            }
        }

        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iBilge = _config.Item("Bilge").GetValue<bool>();
                var iBilgeEnemyhp = hero.Health
                                    <= (hero.MaxHealth * (_config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBilgemyhp = _player.Health
                                 <= (_player.MaxHealth * (_config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
                var iBlade = _config.Item("Blade").GetValue<bool>();
                var iBladeEnemyhp = hero.Health
                                    <= (hero.MaxHealth * (_config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBlademyhp = _player.Health
                                 <= (_player.MaxHealth * (_config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
                var iYoumuu = _config.Item("Youmuu").GetValue<bool>();

                if (hero.IsValidTarget(450) && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
                {
                    _bilge.Cast(hero);
                }

                if (hero.IsValidTarget(450) && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
                {
                    _blade.Cast(hero);
                }

                if (hero.IsValidTarget(450) && iYoumuu && _youmuu.IsReady())
                {
                    _youmuu.Cast();
                }
            }
        }

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(
                _player.ServerPosition,
                _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            var iusehppotion = _config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = _player.Health
                               <= (_player.MaxHealth * (_config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana
                               <= (_player.MaxMana * (_config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0
                || (mobs.Count > 0 && _config.Item("ActiveJungle").GetValue<KeyBind>().Active))
            {
                if (iusepotionhp && iusehppotion
                    && !(ObjectManager.Player.HasBuff("RegenerationPotion")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                {
                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }

                    if (Items.HasItem(2031) && Items.CanUseItem(2031))
                    {
                        Items.UseItem(2031);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }

                if (iusepotionmp && iusemppotion
                    && !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }

                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if (_q.IsReady() && _config.Item("UseQM").GetValue<bool>())
                {
                    if (_q.GetDamage(hero) > hero.Health && hero.IsValidTarget(_q.Range - 30))
                    {
                        _q.CastIfHitchanceEquals(hero, HitChance.High, true);
                    }
                }
                if (_w.IsReady() && _config.Item("UseWM").GetValue<bool>())
                {
                    if (_w.GetDamage(hero) > hero.Health && hero.IsValidTarget(_w.Range))
                    {
                        _w.CastIfHitchanceEquals(hero, HitChance.High, true);
                    }
                }
                if (_r.IsReady() && _config.Item("UseRM").GetValue<bool>() && hero.IsValidTarget(_r.Range))
                {
                    if (_q.IsReady() && _q.GetDamage(hero) > hero.Health && hero.IsValidTarget(_q.Range)) return;
                    if (!hero.IsInvulnerable && _r.GetDamage(hero) - 80 > hero.Health)
                    {
                        _r.CastIfHitchanceEquals(hero, HitChance.High, true);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var harass = (_config.Item("harasstoggle").GetValue<KeyBind>().Active);
            if (_config.Item("Drawsmite").GetValue<bool>() && _smite != null)
            {
                if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.GreenYellow,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.OrangeRed,
                        "Smite Jungle Off");

                if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker"
                    || _player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel")
                {
                    if (_config.Item("smitecombo").GetValue<bool>())
                    {
                        Drawing.DrawText(
                            Drawing.Width * 0.02f,
                            Drawing.Height * 0.90f,
                            System.Drawing.Color.GreenYellow,
                            "Smite Target On");
                    }
                    else
                        Drawing.DrawText(
                            Drawing.Width * 0.02f,
                            Drawing.Height * 0.90f,
                            System.Drawing.Color.OrangeRed,
                            "Smite Target Off");
                }
            }

            if (_config.Item("Drawharass").GetValue<bool>())
            {
                if (harass)
                {
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.92f,
                        System.Drawing.Color.GreenYellow,
                        "Auto harass Enabled");
                }
                else
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.92f,
                        System.Drawing.Color.OrangeRed,
                        "Auto harass Disabled");
            }
            if (_config.Item("DrawQ").GetValue<bool>() && _q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.GreenYellow);
            }
            if (_config.Item("DrawW").GetValue<bool>() && _w.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.GreenYellow);
            }
            if (_config.Item("DrawE").GetValue<bool>() && _e.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.GreenYellow);
            }

            if (_config.Item("DrawR").GetValue<bool>() && _r.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.GreenYellow);
            }
        }
    }
}