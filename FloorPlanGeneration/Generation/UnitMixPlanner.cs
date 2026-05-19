using System;
using System.Collections.Generic;
using System.Linq;
using FloorPlanGeneration.Schema;

namespace FloorPlanGeneration.Generation
{
    internal sealed class UnitMixPlanner
    {
        private readonly List<UnitTypeTarget> _targets;

        public UnitMixPlanner(ProgramBrief program)
        {
            _targets = NormalizeTargets(program);
        }

        public IReadOnlyList<UnitTypeTarget> Targets
        {
            get { return _targets; }
        }

        public string ChooseUnitType(double bayArea, Dictionary<string, int> currentCounts, bool weightedVariation, SeededRandom random)
        {
            List<UnitTypeTarget> candidates = _targets
                .Where(t => bayArea >= t.MinArea * 0.75 && bayArea <= t.MaxArea * 1.35)
                .ToList();

            if (candidates.Count == 0)
            {
                candidates = _targets.ToList();
            }

            int totalPlaced = currentCounts.Values.Sum();
            UnitTypeTarget best = null;
            double bestScore = double.NegativeInfinity;

            foreach (UnitTypeTarget target in candidates)
            {
                double desiredArea = (target.MinArea + target.MaxArea) * 0.5;
                double areaFit = 1.0 - Math.Min(1.0, Math.Abs(bayArea - desiredArea) / Math.Max(desiredArea, 1.0));
                double mixNeed = MixNeed(target, currentCounts, totalPlaced);
                double weight = Math.Max(0.1, target.Weight);
                double jitter = weightedVariation ? random.NextDouble() * 0.15 : 0.0;
                double score = (mixNeed * 2.0) + areaFit + Math.Log(weight + 1.0) + jitter;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = target;
                }
            }

            return best == null ? "studio" : best.Type;
        }

        public UnitTypeTarget FindTarget(string type)
        {
            UnitTypeTarget target = _targets.FirstOrDefault(t => string.Equals(t.Type, type, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                return target;
            }

            return _targets[0];
        }

        public double MixMatchScore(IEnumerable<UnitLayout> units)
        {
            Dictionary<string, int> actual = units
                .GroupBy(u => u.Type, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            int targetCountSum = _targets.Sum(t => Math.Max(0, t.TargetCount));
            int actualCount = actual.Values.Sum();
            if (actualCount == 0)
            {
                return 0.0;
            }

            if (targetCountSum > 0)
            {
                double diff = 0.0;
                foreach (UnitTypeTarget target in _targets)
                {
                    int count = actual.ContainsKey(target.Type) ? actual[target.Type] : 0;
                    diff += Math.Abs(count - target.TargetCount);
                }

                return Clamp01(1.0 - (diff / Math.Max(targetCountSum, actualCount)));
            }

            double ratioTotal = _targets.Sum(t => Math.Max(0.0, t.TargetRatio));
            if (ratioTotal <= 0.0)
            {
                return 1.0;
            }

            double ratioDiff = 0.0;
            foreach (UnitTypeTarget target in _targets)
            {
                double expected = Math.Max(0.0, target.TargetRatio) / ratioTotal;
                int count = actual.ContainsKey(target.Type) ? actual[target.Type] : 0;
                double observed = count / (double)actualCount;
                ratioDiff += Math.Abs(expected - observed);
            }

            return Clamp01(1.0 - (ratioDiff * 0.5));
        }

        public bool StrictCountsSatisfied(IEnumerable<UnitLayout> units)
        {
            int targetCountSum = _targets.Sum(t => Math.Max(0, t.TargetCount));
            if (targetCountSum == 0)
            {
                return true;
            }

            Dictionary<string, int> actual = units
                .GroupBy(u => u.Type, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (UnitTypeTarget target in _targets)
            {
                int count = actual.ContainsKey(target.Type) ? actual[target.Type] : 0;
                if (count != target.TargetCount)
                {
                    return false;
                }
            }

            return actual.Values.Sum() == targetCountSum;
        }

        private static double MixNeed(UnitTypeTarget target, Dictionary<string, int> currentCounts, int totalPlaced)
        {
            int placed = currentCounts.ContainsKey(target.Type) ? currentCounts[target.Type] : 0;
            if (target.TargetCount > 0)
            {
                return Math.Max(0.0, target.TargetCount - placed);
            }

            if (target.TargetRatio > 0.0)
            {
                double desiredThroughNextBay = (totalPlaced + 1) * target.TargetRatio;
                return Math.Max(0.0, desiredThroughNextBay - placed);
            }

            return 1.0 / (1.0 + placed);
        }

        private static List<UnitTypeTarget> NormalizeTargets(ProgramBrief program)
        {
            List<UnitTypeTarget> targets = program != null && program.TargetUnitTypes != null
                ? program.TargetUnitTypes.Where(t => !string.IsNullOrWhiteSpace(t.Type)).ToList()
                : new List<UnitTypeTarget>();

            if (targets.Count > 0)
            {
                return targets;
            }

            return new List<UnitTypeTarget>
            {
                new UnitTypeTarget { Type = "studio", MinArea = 32.0, MaxArea = 52.0, TargetRatio = 0.35, Weight = 1.0 },
                new UnitTypeTarget { Type = "one_bed", MinArea = 50.0, MaxArea = 76.0, TargetRatio = 0.45, Weight = 1.0 },
                new UnitTypeTarget { Type = "two_bed", MinArea = 72.0, MaxArea = 105.0, TargetRatio = 0.20, Weight = 0.8 }
            };
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0) return 0.0;
            if (value > 1.0) return 1.0;
            return value;
        }
    }
}
