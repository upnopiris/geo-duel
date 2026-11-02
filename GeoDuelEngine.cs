// ============================================================================
// GEO-DUEL: PHARAOH & SIAM CYBER EDITION — CORE C# GAME ENGINE
// นวัตกรรมสื่อการเรียนรู้วิชาวิทยาศาสตร์ ป.6 เรื่อง หินและการเปลี่ยนแปลง
// รองรับการใช้งานทั้งใน Unity (2D/3D), .NET 8 Desktop/Web API และ Blazor WebAssembly
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoDuel.Core
{
    public enum CardCategory { Monster, Spell, Trap }
    public enum CardSubType { Normal, Effect, Field, Equip, Continuous, Token }
    public enum MonsterPosition { Attack, Defense }
    public enum DuelPhase { Draw, Main1, Battle, End }

    [Serializable]
    public class GeoTags
    {
        public bool IsCoalOrCarbon { get; set; }
        public bool IsMagmaOrVolcanic { get; set; }
        public bool IsIgneous { get; set; }
        public bool IsSedimentary { get; set; }
        public bool IsMetamorphic { get; set; }
        public bool IsLimestoneOrCarbonate { get; set; }
        public bool IsFossilOrDino { get; set; }
        public bool IsGemOrMineral { get; set; }
        public bool IsMetalOrMagnet { get; set; }
        public bool IsRockFamily => IsIgneous || IsSedimentary || IsMetamorphic || IsGemOrMineral;
    }

    [Serializable]
    public class CardData
    {
        public string Id { get; set; } = "";
        public int Number { get; set; }
        public CardCategory Category { get; set; }
        public CardSubType SubType { get; set; }
        public string Name { get; set; } = "";
        public int Stars { get; set; }
        public string Attribute { get; set; } = "";
        public string Race { get; set; } = "";
        public int BaseAtk { get; set; }
        public int BaseDef { get; set; }
        public string EffectText { get; set; } = "";
        public string ScienceFact { get; set; } = "";
        public string DriveFileId { get; set; } = "";
        public GeoTags Tags { get; set; } = new GeoTags();
    }

    [Serializable]
    public class CardInstance
    {
        public string Uid { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
        public CardData Data { get; set; }
        public int CurrentAtk { get; set; }
        public int CurrentDef { get; set; }
        public int BonusAtk { get; set; }
        public int BonusDef { get; set; }
        public MonsterPosition Position { get; set; } = MonsterPosition.Attack;
        public bool IsFaceDown { get; set; }
        public int SummonedTurn { get; set; }
        public int AttacksLeft { get; set; } = 1;
        public int MaxAttacks { get; set; } = 1;
        public bool ChangedPositionThisTurn { get; set; }
        public bool UsedActiveEffectThisTurn { get; set; }
        public bool HasCrystalShield { get; set; }
        public bool HasGeoHammer { get; set; }
        public bool IsSpellImmune { get; set; }
        public bool SwappedStats { get; set; }
        public bool ZeroDefEquipped { get; set; }
        public int LockedTurns { get; set; }

        public CardInstance(CardData data)
        {
            Data = data;
            CurrentAtk = data.BaseAtk;
            CurrentDef = data.BaseDef;
            MaxAttacks = (data.Id == "M004" || data.Id == "M043") ? 2 : 1;
            AttacksLeft = MaxAttacks;
        }
    }

    [Serializable]
    public class DuelTelemetry
    {
        public int TurnsPlayed { get; set; } = 1;
        public int MonstersSummoned { get; set; }
        public int TributesPerformed { get; set; }
        public int SmartTributeCount { get; set; } // Sacrificed low-ATK monster for high-ATK boss
        public int SpellsActivated { get; set; }
        public int GeologicalCombosTriggered { get; set; }
        public int TrapsTriggered { get; set; }
        public int AttacksDeclared { get; set; }
        public int TypeAdvantageKills { get; set; }
        public int EqualAtkMutualKills { get; set; }
        public int MisplayAttacksIntoHigherDef { get; set; }
        public int TotalDamageDealt { get; set; }
        public List<CardData> CardsUsedInDuel { get; set; } = new List<CardData>();
    }

    [Serializable]
    public class AiEvaluationReport
    {
        public string Rank { get; set; } = "A";
        public int TotalScore100 { get; set; }
        public double ScoreOutOf20 => Math.Round(TotalScore100 / 5.0, 1);
        public int GeoMasteryScore30 { get; set; }
        public int TributeResourceScore25 { get; set; }
        public int CombatMathScore25 { get; set; }
        public int VictorySpeedScore20 { get; set; }
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Improvements { get; set; } = new List<string>();
        public List<string> ScienceTakeaways { get; set; } = new List<string>();
    }

    public class DuelPlayer
    {
        public int PlayerIndex { get; set; }
        public string Name { get; set; } = "";
        public int LifePoints { get; set; } = 8000;
        public List<CardInstance> Deck { get; set; } = new List<CardInstance>();
        public List<CardInstance> Hand { get; set; } = new List<CardInstance>();
        public CardInstance?[] MonsterZones { get; set; } = new CardInstance?[5];
        public CardInstance?[] SpellTrapZones { get; set; } = new CardInstance?[5];
        public List<CardInstance> Graveyard { get; set; } = new List<CardInstance>();
        public List<string> ContinuousEffects { get; set; } = new List<string>();
        public bool NormalSummonedThisTurn { get; set; }

        public int OccupiedMonsterCount => MonsterZones.Count(m => m != null);
        public int FirstEmptyMonsterSlot => Array.FindIndex(MonsterZones, m => m == null);
        public int FirstEmptySpellTrapSlot => Array.FindIndex(SpellTrapZones, s => s == null);
    }

    public class BattleOutcome
    {
        public bool TrapNegatedAttack { get; set; }
        public bool IsMutualDestruction { get; set; }
        public bool AttackerDestroyed { get; set; }
        public bool DefenderDestroyed { get; set; }
        public int DamageToAttacker { get; set; }
        public int DamageToDefender { get; set; }
        public string LogMessage { get; set; } = "";
    }

    // ========================================================================
    // MAIN GEO-DUEL C# ENGINE (WITH USER-SELECTED TRIBUTE & SYNERGY VALIDATION)
    // ========================================================================
    public class GeoDuelMatchEngine
    {
        public DuelPlayer[] Players { get; private set; } = new DuelPlayer[2];
        public int TurnCount { get; private set; } = 1;
        public int ActivePlayerIndex { get; private set; } = 0;
        public DuelPhase CurrentPhase { get; private set; } = DuelPhase.Main1;
        public CardInstance? ActiveFieldSpell { get; private set; }
        public int IceAgeTurnsRemaining { get; private set; }
        public bool IsMatchOver { get; private set; }
        public int WinnerIndex { get; private set; } = -1;
        public string WinReason { get; private set; } = "";
        public DuelTelemetry PlayerTelemetry { get; private set; } = new DuelTelemetry();

        // Direct Geological Parent -> Metamorphic Rock Evolution Map
        public static readonly Dictionary<string, string> MetamorphicEvolutionMap = new()
        {
            { "M002", "M042" }, // หินแกรนิต -> หินไนส์
            { "M030", "M041" }, // หินปูน -> หินอ่อน
            { "M071", "M041" }, // ชอล์ก -> หินอ่อน
            { "M037", "M041" }, // ดินมาร์ล -> หินอ่อน
            { "M025", "M043" }, // หินดินดาน -> หินชนวน
            { "M026", "M045" }, // หินโคลน -> หินชีสต์
            { "M067", "M046" }, // ดินเหนียว -> หินฟิลไลต์
            { "M023", "M044" }, // หินทราย -> หินควอตซ์ไซต์
            { "M021", "M051" }, // หินกรวดมน -> หินเมตาคองเกลอเมอเรต
            { "M032", "M050" }, // ถ่านหิน -> ถ่านหินแอนทราไซต์
            { "M110", "M050" }, // พีทมอส -> ถ่านหินแอนทราไซต์
            { "M111", "M050" }, // ลิกไนต์ -> ถ่านหินแอนทราไซต์
            { "M112", "M050" }  // บิทูมินัส -> ถ่านหินแอนทราไซต์
        };

        public void StartNewMatch(DuelPlayer p0, DuelPlayer p1, bool p0GoesFirst = true)
        {
            Players[0] = p0;
            Players[1] = p1;
            Players[0].LifePoints = 8000;
            Players[1].LifePoints = 8000;
            TurnCount = 1;
            ActivePlayerIndex = p0GoesFirst ? 0 : 1;
            CurrentPhase = DuelPhase.Main1;
            ActiveFieldSpell = null;
            IceAgeTurnsRemaining = 0;
            IsMatchOver = false;
            WinnerIndex = -1;
            WinReason = "";
            PlayerTelemetry = new DuelTelemetry();

            // Deal 5 starting cards to each player
            for (int i = 0; i < 5; i++)
            {
                DrawCard(0);
                DrawCard(1);
            }
            RecalculateAllFieldStats();
        }

        public bool DrawCard(int playerIdx)
        {
            var pl = Players[playerIdx];
            if (pl.Deck.Count == 0)
            {
                FinishMatch(1 - playerIdx, $"กองการ์ดของ {pl.Name} หมดลง (Deck Out)!");
                return false;
            }
            var top = pl.Deck[0];
            pl.Deck.RemoveAt(0);
            pl.Hand.Add(top);
            return true;
        }

        /// <summary>
        /// Summons a monster using explicit player-selected tribute slot indices (NEVER random!).
        /// </summary>
        public (bool Success, string Message) SummonMonsterWithSelectedTributes(
            int playerIdx,
            int handIndex,
            MonsterPosition position,
            List<int> selectedFieldTributeSlots,
            int targetMonsterSlot = -1)
        {
            var pl = Players[playerIdx];
            if (handIndex < 0 || handIndex >= pl.Hand.Count)
                return (false, "ไม่พบการ์ดในมือ");

            var card = pl.Hand[handIndex];
            if (card.Data.Category != CardCategory.Monster)
                return (false, "การ์ดที่เลือกไม่ใช่การ์ดมอนสเตอร์");

            if (pl.NormalSummonedThisTurn)
                return (false, "คุณทำการอัญเชิญปกติครบ 1 ครั้งในเทิร์นนี้แล้ว");

            int requiredTributes = card.Data.Id == "M122" ? 3 : (card.Data.Stars >= 7 ? 2 : (card.Data.Stars >= 5 ? 1 : 0));
            if (selectedFieldTributeSlots.Count != requiredTributes)
                return (false, $"มอนสเตอร์ระดับ {card.Data.Stars} ดาว ต้องเลือกมอนสเตอร์ในสนามเพื่อสังเวย {requiredTributes} ตัวพอดี");

            // Validate and sacrifice the exact monsters chosen by the player
            foreach (int slotIdx in selectedFieldTributeSlots)
            {
                if (slotIdx < 0 || slotIdx >= 5 || pl.MonsterZones[slotIdx] == null)
                    return (false, "ช่องมอนสเตอร์ที่เลือกสังเวยไม่ถูกต้อง");
            }

            foreach (int slotIdx in selectedFieldTributeSlots)
            {
                var sacrificed = pl.MonsterZones[slotIdx]!;
                if (playerIdx == 0)
                {
                    PlayerTelemetry.TributesPerformed++;
                    if (sacrificed.CurrentAtk < card.Data.BaseAtk)
                        PlayerTelemetry.SmartTributeCount++;
                }
                pl.Graveyard.Add(sacrificed);
                pl.MonsterZones[slotIdx] = null;
            }

            int destSlot = (targetMonsterSlot >= 0 && pl.MonsterZones[targetMonsterSlot] == null)
                ? targetMonsterSlot
                : pl.FirstEmptyMonsterSlot;

            if (destSlot == -1)
                return (false, "ช่องมอนสเตอร์บนกระดานเต็มทั้ง 5 ช่องแล้ว");

            pl.Hand.RemoveAt(handIndex);
            card.Position = position;
            card.IsFaceDown = (position == MonsterPosition.Defense);
            card.SummonedTurn = TurnCount;
            card.AttacksLeft = card.MaxAttacks;
            pl.MonsterZones[destSlot] = card;
            pl.NormalSummonedThisTurn = true;

            if (playerIdx == 0)
            {
                PlayerTelemetry.MonstersSummoned++;
                PlayerTelemetry.CardsUsedInDuel.Add(card.Data);
            }

            RecalculateAllFieldStats();
            return (true, $"อัญเชิญ {card.Data.Name} ลงช่องที่ {destSlot + 1} สำเร็จ!");
        }

        /// <summary>
        /// Resolves Monster-vs-Monster Battle with Equal-ATK Mutual Destruction (ตายคู่!).
        /// </summary>
        public BattleOutcome ExecuteMonsterBattle(int attSide, int attSlot, int defSide, int defSlot)
        {
            var outcome = new BattleOutcome();
            RecalculateAllFieldStats();

            var attPl = Players[attSide];
            var defPl = Players[defSide];
            var attacker = attPl.MonsterZones[attSlot];
            var defender = defPl.MonsterZones[defSlot];

            if (attacker == null || defender == null)
                return outcome;

            attacker.AttacksLeft--;
            if (attSide == 0) PlayerTelemetry.AttacksDeclared++;

            // Flip face-down defense monster face-up
            if (defender.IsFaceDown)
                defender.IsFaceDown = false;

            int attVal = attacker.Data.Id == "M094" ? 3500 : attacker.CurrentAtk;
            int defStat = defender.Position == MonsterPosition.Attack ? defender.CurrentAtk : defender.CurrentDef;

            // RULE 1: Equal Attack Power -> BOTH monsters are destroyed (ตายคู่!)
            if (attVal == defStat && (defender.Position == MonsterPosition.Attack || defender.CurrentAtk == attVal))
            {
                outcome.IsMutualDestruction = true;
                outcome.AttackerDestroyed = true;
                outcome.DefenderDestroyed = true;
                DestroyMonster(attSide, attSlot);
                DestroyMonster(defSide, defSlot);
                if (attSide == 0) PlayerTelemetry.EqualAtkMutualKills++;
                outcome.LogMessage = $"พลังเท่ากัน ({attVal} = {defStat})! ตายคู่ลงสุสานทั้งสองฝ่าย!";
                CheckWinConditions();
                return outcome;
            }

            if (defender.Position == MonsterPosition.Attack)
            {
                if (attVal > defStat)
                {
                    int diff = attVal - defStat;
                    defPl.LifePoints = Math.Max(0, defPl.LifePoints - diff);
                    outcome.DefenderDestroyed = true;
                    outcome.DamageToDefender = diff;
                    if (attSide == 0) PlayerTelemetry.TotalDamageDealt += diff;
                    DestroyMonster(defSide, defSlot);
                }
                else
                {
                    int diff = defStat - attVal;
                    attPl.LifePoints = Math.Max(0, attPl.LifePoints - diff);
                    outcome.AttackerDestroyed = true;
                    outcome.DamageToAttacker = diff;
                    if (attSide == 0) PlayerTelemetry.MisplayAttacksIntoHigherDef++;
                    DestroyMonster(attSide, attSlot);
                }
            }
            else
            {
                if (attVal > defStat)
                {
                    outcome.DefenderDestroyed = true;
                    DestroyMonster(defSide, defSlot);
                }
                else if (attVal < defStat)
                {
                    int recoil = defStat - attVal;
                    attPl.LifePoints = Math.Max(0, attPl.LifePoints - recoil);
                    outcome.DamageToAttacker = recoil;
                    if (attSide == 0) PlayerTelemetry.MisplayAttacksIntoHigherDef++;
                }
            }

            CheckWinConditions();
            RecalculateAllFieldStats();
            return outcome;
        }

        public void DestroyMonster(int side, int slot)
        {
            var pl = Players[side];
            var m = pl.MonsterZones[slot];
            if (m == null) return;
            pl.MonsterZones[slot] = null;
            pl.Graveyard.Add(m);
        }

        public void RecalculateAllFieldStats()
        {
            for (int s = 0; s < 2; s++)
            {
                var pl = Players[s];
                if (pl == null) continue;
                foreach (var m in pl.MonsterZones)
                {
                    if (m == null) continue;
                    int atk = m.SwappedStats ? m.Data.BaseDef : m.Data.BaseAtk;
                    int def = m.SwappedStats ? m.Data.BaseAtk : m.Data.BaseDef;
                    if (ActiveFieldSpell?.Data.Id == "S003" && (m.Data.Attribute == "ไฟ" || m.Data.Attribute == "ดิน"))
                        atk += 500;
                    m.CurrentAtk = Math.Max(0, atk + m.BonusAtk);
                    m.CurrentDef = m.ZeroDefEquipped ? 0 : Math.Max(0, def + m.BonusDef);
                }
            }
        }

        public void CheckWinConditions()
        {
            if (Players[0].LifePoints <= 0) FinishMatch(1, "Life Points ของผู้เล่นที่ 1 ลดเหลือ 0");
            else if (Players[1].LifePoints <= 0) FinishMatch(0, "Life Points ของคู่ต่อสู้ลดเหลือ 0");
        }

        private void FinishMatch(int winnerIdx, string reason)
        {
            IsMatchOver = true;
            WinnerIndex = winnerIdx;
            WinReason = reason;
        }

        /// <summary>
        /// 100% Free Built-in Heuristic AI Duel Analyst & Grader (Zero API Cost).
        /// </summary>
        public AiEvaluationReport EvaluateDuelPerformance()
        {
            var rep = new AiEvaluationReport();
            bool won = WinnerIndex == 0;
            int myLp = Players[0].LifePoints;

            // 1. Geological Mastery (30 pts)
            rep.GeoMasteryScore30 = Math.Min(30, 18 + PlayerTelemetry.GeologicalCombosTriggered * 4 + Math.Min(6, PlayerTelemetry.SpellsActivated * 2));
            // 2. Tribute & Resource Efficiency (25 pts)
            rep.TributeResourceScore25 = Math.Min(25, 16 + PlayerTelemetry.SmartTributeCount * 4 + Math.Min(5, PlayerTelemetry.MonstersSummoned));
            // 3. Combat & Math Calculation (25 pts)
            rep.CombatMathScore25 = Math.Max(10, Math.Min(25, 20 + PlayerTelemetry.TypeAdvantageKills * 3 - PlayerTelemetry.MisplayAttacksIntoHigherDef * 3));
            // 4. Victory Speed & LP Preservation (20 pts)
            rep.VictorySpeedScore20 = won ? Math.Min(20, 12 + (myLp >= 2500 ? 5 : 2) + (TurnCount <= 8 ? 3 : 1)) : Math.Min(12, 6 + (TurnCount >= 4 ? 4 : 2));

            rep.TotalScore100 = rep.GeoMasteryScore30 + rep.TributeResourceScore25 + rep.CombatMathScore25 + rep.VictorySpeedScore20;
            rep.Rank = rep.TotalScore100 >= 90 ? "S+" : (rep.TotalScore100 >= 80 ? "S" : (rep.TotalScore100 >= 70 ? "A" : "B"));

            foreach (var c in PlayerTelemetry.CardsUsedInDuel.GroupBy(x => x.Id).Select(g => g.First()).Take(5))
            {
                if (!string.IsNullOrWhiteSpace(c.ScienceFact))
                    rep.ScienceTakeaways.Add($"{c.Name}: {c.ScienceFact}");
            }
            return rep;
        }
    }
}
