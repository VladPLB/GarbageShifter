using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level.Runner
{
    public class SpaceRunnerEnemyJoinAndChase : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private SpaceRunnerLevelBuilder level;
        [SerializeField] private int enemyRouteIndex = 0;
        [SerializeField] private Transform model;

        [Header("Move")]
        [SerializeField, Min(0.1f)] private float speed = 28f;
        [SerializeField, Min(0f)] private float rotateLerp = 0.12f;
        [SerializeField, Min(0.05f)] private float waypointReachRadius = 0.8f;

        [Header("Avoid asteroids (runtime steering)")]
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField, Min(0f)] private float avoidanceDetectRadius = 5f;
        [SerializeField, Min(0f)] private float avoidanceStrength = 6f;

        [Header("Spawn")]
        [Tooltip("Спавн на кольце ПОЗАДИ игрока.")]
        [SerializeField] private bool spawnFromRingBehindPlayer = true;

        [Header("Phases")]
        [SerializeField] private bool doJoinOnEnable = true;

        [Tooltip("На сколько сегментов позади игрока держать цель догоняния (чтобы камера стреляла 'назад', а враги влетали из-за спины).")]
        [SerializeField, Min(0f)] private float catchUpBehindSegments = 6f;

        [Tooltip("На каком расстоянии считаем, что враг 'встал на маршрут' и можно переключаться на chase enemyRoutePoint.")]
        [SerializeField, Min(0.1f)] private float catchUpCompleteDistance = 2.2f;

        private enum State
        {
            Join,       // летим по заранее построенным waypoint'ам к точке на маршруте
            CatchUp,    // догоняем "живую" точку на маршруте (позади игрока)
            Chase       // обычное следование за enemyRoutePoint
        }

        private State _state;

        private List<Vector3> _joinPath;
        private int _joinIndex;

        private void OnEnable()
        {
            if (doJoinOnEnable)
                StartPhasedChase();
        }

        public void StartPhasedChase()
        {
            if (level == null)
                level = FindFirstObjectByType<SpaceRunnerLevelBuilder>();

            if (model == null) model = transform;

            if (level == null)
            {
                _state = State.Chase;
                return;
            }

            // 1) Спавнимся позади игрока на кольце (опционально)
            if (spawnFromRingBehindPlayer)
            {
                if (level.TryGetEnemySpawnPointOnRingBehindPlayer(enemyRouteIndex, out var spawnPos))
                    transform.position = spawnPos;
            }

            // 2) Строим join путь к "перехватной" точке на маршруте (позади игрока)
            _joinPath = level.BuildEnemyJoinPathToCatchUpPointFromRingBehindPlayer(enemyRouteIndex, catchUpBehindSegments);
            if (_joinPath == null || _joinPath.Count < 2)
            {
                _state = State.CatchUp;
                return;
            }

            _joinIndex = 1;
            _state = State.Join;
        }

        private void Update()
        {
            if (level == null)
                return;

            if (model == null)
                model = transform;

            switch (_state)
            {
                case State.Join:
                    TickJoin();
                    break;
                case State.CatchUp:
                    TickCatchUp();
                    break;
                case State.Chase:
                    TickChase();
                    break;
            }
        }

        private void TickJoin()
        {
            if (_joinPath == null || _joinIndex >= _joinPath.Count)
            {
                _state = State.CatchUp;
                return;
            }

            Vector3 target = _joinPath[_joinIndex];
            MoveTowardsWithAvoidance(target);

            if (Vector3.Distance(transform.position, target) <= waypointReachRadius)
            {
                _joinIndex++;
                if (_joinIndex >= _joinPath.Count)
                    _state = State.CatchUp;
            }
        }

        private void TickCatchUp()
        {
            // Цель догоняния — точка на основном маршруте ПОЗАДИ игрока и она "едет" вместе с игроком.
            float progress = Mathf.Max(0f, level.PlayerProgress - catchUpBehindSegments);

            if (!level.TrySampleMainPathAtProgress(progress, out var catchPos, out _))
            {
                _state = State.Chase;
                return;
            }

            MoveTowardsWithAvoidance(catchPos);

            if (Vector3.Distance(transform.position, catchPos) <= catchUpCompleteDistance)
            {
                _state = State.Chase;
            }
        }

        private void TickChase()
        {
            var route = level.GetEnemyRoutePoint(enemyRouteIndex);
            if (route == null) return;

            MoveTowardsWithAvoidance(route.position);
        }

        private void MoveTowardsWithAvoidance(Vector3 targetPos)
        {
            Vector3 to = targetPos - transform.position;
            float d = to.magnitude;
            if (d < 0.0001f)
                return;

            Vector3 dir = to / d;

            // простое руление от астероидов (MeshCollider ок — via ClosestPoint)
            Vector3 avoid = ComputeAvoidanceForce(transform.position);
            Vector3 steerDir = (dir + avoid).normalized;

            Vector3 step = steerDir * (speed * Time.deltaTime);
            if (step.magnitude > d) step = to;

            transform.position += step;

            Quaternion desiredRot = Quaternion.LookRotation(steerDir, Vector3.up);
            model.rotation = Quaternion.Slerp(model.rotation, desiredRot, rotateLerp);
        }

        private Vector3 ComputeAvoidanceForce(Vector3 pos)
        {
            if (avoidanceDetectRadius <= 0f || avoidanceStrength <= 0f)
                return Vector3.zero;

            Collider[] hits = Physics.OverlapSphere(pos, avoidanceDetectRadius, obstacleLayerMask, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0)
                return Vector3.zero;

            Vector3 sum = Vector3.zero;
            int count = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                var c = hits[i];
                if (c == null) continue;

                Vector3 closest = c.ClosestPoint(pos);
                Vector3 away = pos - closest;
                float dist = away.magnitude;
                if (dist < 0.0001f) continue;

                float k = Mathf.Clamp01(1f - (dist / avoidanceDetectRadius));
                sum += (away / dist) * (k * k);
                count++;
            }

            if (count == 0) return Vector3.zero;

            Vector3 force = (sum / count) * avoidanceStrength;
            // не даём avoidance полностью “перебить” движение к цели
            return Vector3.ClampMagnitude(force, 0.85f);
        }
    }
}
