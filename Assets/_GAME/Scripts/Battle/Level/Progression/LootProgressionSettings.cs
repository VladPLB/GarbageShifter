using _GAME.Scripts.Inventory;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level.Progression
{
    // Drop of materials (Tools, Scraps) and Licenses by location grade
    [CreateAssetMenu(fileName = "LootProgressionSettings", menuName = "Scriptable/Battle/LootProgression", order = 3)]
    public class LootProgressionSettings : ScriptableObject
    {
        [Header("Grade (max dropped rank)")]
        [Tooltip("Grade grows once per this amount of locations")]
        [SerializeField, Min(1)] private int _gradeLocations = 30;

        [Header("Licenses")]
        [Tooltip("Per enemy kill chance (0..1) of a license of the current grade rank")]
        [SerializeField, Range(0f, 1f)] private float _licenseBaseChance = 0.002f;
        [Tooltip("Each grade above a rank adds this part of the base chance for that rank")]
        [SerializeField, Min(0f)] private float _licenseChanceGrowthPerGrade = 1f;
        [Tooltip("Last level of every N-th location gives a guaranteed license of the grade rank")]
        [SerializeField, Min(1)] private int _guaranteedLicensePeriod = 10;

        [Header("Materials (per enemy kill)")]
        [SerializeField, Range(0f, 1f)] private float _materialDropChance = 1f;
        [SerializeField, Range(0f, 1f)] private float _toolShare = 0.4f;
        [SerializeField] private Vector2Int _toolAmount = new(2, 4);
        [SerializeField] private Vector2Int _scrapAmount = new(4, 8);
        [Tooltip("Amount multiplier per rank: amount * falloff^rank")]
        [SerializeField, Range(0.05f, 1f)] private float _rankAmountFalloff = 0.6f;
        [Tooltip("Each grade above a rank increases its amount by this part")]
        [SerializeField, Min(0f)] private float _amountGrowthPerGrade = 0.25f;
        [Tooltip("Each grade above a rank increases its drop weight by this part (lower ranks drop more often)")]
        [SerializeField, Min(0f)] private float _lowerRankWeightGrowth = 1f;

        [Header("Drop above the grade (for owned better equipment)")]
        [Tooltip("Per enemy kill chance (0..1) of Tools of the first rank above the grade, if the player owns equipment of that rank")]
        [SerializeField, Range(0f, 1f)] private float _ownedRankToolChance = 0.05f;
        [Tooltip("Per enemy kill chance (0..1) of a License of the first rank above the grade, if the player owns equipment of that rank")]
        [SerializeField, Range(0f, 1f)] private float _ownedRankLicenseChance = 0.002f;
        [Tooltip("Chance multiplier for every next rank above the grade")]
        [SerializeField, Range(0f, 1f)] private float _ownedRankChanceFalloff = 0.5f;

        public float MaterialDropChance => _materialDropChance;
        public float ToolShare => _toolShare;

        public int GetGrade(int location)
        {
            return Mathf.Clamp(location / _gradeLocations, 0, (int)ItemRank.Unique);
        }

        public bool IsGuaranteedLicenseLocation(int location)
        {
            return (location + 1) % _guaranteedLicensePeriod == 0;
        }

        public float GetLicenseChance(ItemRank rank, int grade)
        {
            var gradesAbove = grade - (int)rank;
            if (gradesAbove < 0)
                return 0f;

            return _licenseBaseChance * (1f + _licenseChanceGrowthPerGrade * gradesAbove);
        }

        public float GetOwnedRankToolChance(ItemRank rank, int grade) => GetOwnedRankChance(_ownedRankToolChance, rank, grade);
        public float GetOwnedRankLicenseChance(ItemRank rank, int grade) => GetOwnedRankChance(_ownedRankLicenseChance, rank, grade);

        private float GetOwnedRankChance(float chance, ItemRank rank, int grade)
        {
            var ranksAbove = (int)rank - grade;
            if (ranksAbove <= 0)
                return 0f;

            return chance * Mathf.Pow(_ownedRankChanceFalloff, ranksAbove - 1);
        }

        public ItemRank GetRandomMaterialRank(int grade)
        {
            var total = 0f;
            for (int r = 0; r <= grade; r++)
                total += GetMaterialWeight(r, grade);

            var roll = Random.value * total;
            for (int r = 0; r <= grade; r++)
            {
                roll -= GetMaterialWeight(r, grade);
                if (roll <= 0f)
                    return (ItemRank)r;
            }
            return (ItemRank)grade;
        }

        public int GetMaterialAmount(bool isTool, ItemRank rank, int grade)
        {
            var range = isTool ? _toolAmount : _scrapAmount;
            var multiplier = Mathf.Pow(_rankAmountFalloff, (int)rank) * (1f + _amountGrowthPerGrade * (grade - (int)rank));
            var amount = Random.Range(range.x, range.y + 1) * multiplier;
            return Mathf.Max(1, Mathf.RoundToInt(amount));
        }

        private float GetMaterialWeight(int rank, int grade)
        {
            return 1f + _lowerRankWeightGrowth * (grade - rank);
        }
    }
}
