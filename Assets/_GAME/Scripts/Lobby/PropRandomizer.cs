using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class PropRandomizer : MonoBehaviour
    {
        [System.Serializable]
        public class ObjectGroup
        {
            public GameObject[] Options;
        }

        [SerializeField] private Transform _root;
        [SerializeField] private ObjectGroup[] _groups;
        [SerializeField] private bool _autoActivation = false;
        [SerializeField] private bool _randomFlipX = false;
        [SerializeField] private bool _randomFlipZ = false;

        private int _seed;

        public void Init(int customSeed)
        {
            _seed = customSeed;
            Init();
        }

        private void Init()
        {
            System.Random rng = new System.Random(_seed);

            foreach (var group in _groups)
            {
                int chosenIndex = rng.Next(0, group.Options.Length);
                for (int i = 0; i < group.Options.Length; i++)
                {
                    if (group.Options[i] != null)
                        group.Options[i].SetActive(i == chosenIndex);
                }
            }

            var localScale = Vector3.one;
            
            
            if (_randomFlipX && rng.Next(100) > 50)
            {
                localScale.x = -1f;
            }
            if (_randomFlipZ && rng.Next(100) > 50)
            {
                localScale.z = -1f;
            }

            _root.localScale = localScale;
        }

        void Start()
        {
            if(_autoActivation)
            {
                Init();
            }
        }
    }
}