using Unity.AI.Navigation;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelPathFinder
    {
        private NavMeshSurface _navMeshSurface;

        public LevelPathFinder()
        {
            _navMeshSurface = Object.FindObjectOfType<NavMeshSurface>();
        }

        public void Rebuild()
        {
            _navMeshSurface.BuildNavMesh();
        }
    }
}