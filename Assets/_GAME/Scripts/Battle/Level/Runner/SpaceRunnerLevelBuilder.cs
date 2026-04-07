using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level.Runner
{
    public class SpaceRunnerLevelBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform target;

        [SerializeField] 
        private Transform anchorsRoot;
        [SerializeField] 
        private Transform obstaclesRoot;
        
        [SerializeField, Min(0f)] private float playerRouteSpeed = 35f;
        [Header("Enemies")]
        [SerializeField, Min(0)] 
        private int autoCreateEnemyRoutePoints = 6;
        [SerializeField, Min(0f)] 
        private float enemyRouteSpeed = 40f;
        [SerializeField, Min(0)] 
        private int enemyLagSegments = 1;
        [SerializeField]
        private Vector2 enemyOffsetMax = new Vector2(10f, 6f);
        [SerializeField, Range(0f, 1f)]
        private float enemyOffsetSmoothing = 0.8f;
        
        [Header("Enemy spawn ring (around route)")]
        [SerializeField, Min(0f)] 
        private float enemySpawnRingMinRadius = 35f;
        [SerializeField, Min(0f)] 
        private float enemySpawnRingMaxRadius = 55f;

        [Tooltip("Смещение по высоте при спавне из кольца (рандом в диапазоне).")]
        [SerializeField] 
        private Vector2 enemySpawnRingHeightRange = new Vector2(-8f, 8f);

        [Tooltip("На сколько сегментов вперед от текущего прогресса игрока выбирать точку спавна врага.")]
        [SerializeField] 
        private Vector2 enemySpawnAheadSegmentsRange = new Vector2(6f, 14f);
        
        [Tooltip("На сколько сегментов НАЗАД от текущего прогресса игрока выбирать точку спавна (для 'спавна позади').")]
        [SerializeField] 
        private Vector2 enemySpawnBehindSegmentsRange = new Vector2(6f, 14f);

        [Tooltip("Сколько попыток сделать спавн-точку, не попав внутрь астероида.")]
        [SerializeField, Range(1, 30)] 
        private int enemySpawnRingTries = 10;

        [SerializeField, Min(0.1f)] 
        private float enemySpawnClearRadius = 2.0f;

        
        [Header("Join path (enemies enter from outside)")]
        [SerializeField] 
        private LayerMask obstacleLayerMask;
        [SerializeField, Min(0.1f)] 
        private float joinAvoidanceRadius = 2.5f;
        [SerializeField, Min(0f)] 
        private float joinAvoidanceStrength = 4.0f;
        [SerializeField, Min(0)] 
        private int joinAvoidanceRelaxIterations = 3;
        
        [SerializeField, Range(4, 32)] 
        private int joinPathPoints = 10;
        
        [SerializeField, Min(0f)] 
        private float joinCurveBend = 18f;
        
        [Header("Anchors (trajectory)")]
        [SerializeField, Min(3)]
        private int keepAnchorsBehind = 6;

        [SerializeField, Min(8)] private int keepAnchorsAhead = 18;
        [SerializeField, Min(5f)] private float segmentLength = 20f;

        [SerializeField, Min(0f)] private Vector2 _trajectoryOffsetMax = new Vector2(3, 3);
        
        [SerializeField, Range(0f, 1f)]
        private float offsetSmoothing = 0.65f;

        [Header("Rotation")]
        [SerializeField, Range(0f, 30f)]
        private float yawPerSegmentMax = 10f;
        
        [SerializeField, Range(0f, 45f)]
        private float _spineMax = 15f;
        
        [SerializeField, Range(0f, 1f)]
        private float _spineChance = 0.25f;

        [Header("Asteroids (obstacles)")]
        [SerializeField]
        private List<GameObject> asteroidPrefabs = new List<GameObject>();
        
        [SerializeField, Min(0f)]
        private float asteroidsPerSegment = 1.2f;
        
        [SerializeField, Min(0f)]
        private float obstacleSpawnRadius = 14f;
        
        [SerializeField, Min(0f)]
        private float obstacleKeepoutRadius = 3.5f;

        [SerializeField] 
        private Vector2 asteroidScaleRange = new Vector2(0.6f, 1.8f);

        [Header("Asteroids motion (optional)")]
        
        [SerializeField] 
        private Vector2 asteroidSpinDegPerSec = new Vector2(10f, 50f);
        [SerializeField]
        private Vector2 asteroidDriftAmplitude = new Vector2(0.1f, 0.6f);
        [SerializeField]
        private Vector2 asteroidDriftFrequency = new Vector2(0.2f, 0.8f);

        [Header("Debug")] 
        [SerializeField] 
        private bool drawGizmos = true;
        [SerializeField]
        private Color gizmoAnchorColor = new Color(0.3f, 1f, 1f, 0.9f);
        [SerializeField]
        private Color gizmoLinkColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        
        private Transform playerRoutePoint = null;
        private List<Transform> enemyRoutePoints = new List<Transform>();
        private List<Transform> _anchors = new List<Transform>();
        private int _anchorsStartIndex;

        private readonly Queue<GameObject> _spawnedObstacles = new Queue<GameObject>();

        private Vector3 _smoothedLocalOffset;
        private int _anchorIndexCounter;

        private float _playerProgress;
        private List<float> _enemyProgress = new List<float>();
        private List<Vector3> _enemySmoothedOffset = new List<Vector3>();
        private List<int> _enemySeed = new List<int>();

        public float SegmentLength => segmentLength;
        public int FirstAnchorGlobalIndex => _anchorsStartIndex;
        public int LastAnchorGlobalIndex => _anchorsStartIndex + _anchors.Count - 1;
        
        public float PlayerProgress => _playerProgress;


        private void Reset()
        {
            if (anchorsRoot == null)
            {
                var go = new GameObject("[AnchorsRoot]");
                go.transform.SetParent(transform, false);
                anchorsRoot = go.transform;
            }

            if (obstaclesRoot == null)
            {
                var go = new GameObject("[ObstaclesRoot]");
                go.transform.SetParent(transform, false);
                obstaclesRoot = go.transform;
            }
        }

        private void Start()
        {
            EnsureRoots();
            BootstrapInitialAnchors();
            EnsureRoutePointsExist();
            InitEnemyRoutePointState();
        }

        private void Update()
        {
            if (_anchors.Count == 0)
                return;

            EnsureAnchorsAhead();
            CleanupBehind();
            
            TickRoutePoints(Time.deltaTime);
        }

        private void EnsureRoots()
        {
            if (anchorsRoot == null)
            {
                var go = new GameObject("[AnchorsRoot]");
                go.transform.SetParent(transform, false);
                anchorsRoot = go.transform;
            }

            if (obstaclesRoot == null)
            {
                var go = new GameObject("[ObstaclesRoot]");
                go.transform.SetParent(transform, false);
                obstaclesRoot = go.transform;
            }
        }

        private void EnsureRoutePointsExist()
        {
            if (playerRoutePoint == null)
            {
                var go = new GameObject("[RoutePoint_Player]");
                go.transform.SetParent(transform, false);
                playerRoutePoint = go.transform;
            }

            if (enemyRoutePoints == null)
                enemyRoutePoints = new List<Transform>();

            while (enemyRoutePoints.Count < autoCreateEnemyRoutePoints)
            {
                var go = new GameObject($"[RoutePoint_Enemy_{enemyRoutePoints.Count:00}]");
                go.transform.SetParent(transform, false);
                enemyRoutePoints.Add(go.transform);
            }
        }
        
        /// <summary>
        /// Точка спавна на кольце вокруг маршрута ПОЗАДИ игрока (в сегментах).
        /// </summary>
        public bool TryGetEnemySpawnPointOnRingBehindPlayer(int enemyRouteIndex, out Vector3 spawnPos)
        {
            spawnPos = default;

            float behind = Random.Range(enemySpawnBehindSegmentsRange.x, enemySpawnBehindSegmentsRange.y);
            float progress = Mathf.Max(0f, _playerProgress - Mathf.Max(0f, behind));

            EnsureGeneratedForProgress(progress + 2);

            if (!TrySampleMainPath(progress, out var centerPos, out var centerRot))
                return false;

            float minR = Mathf.Min(enemySpawnRingMinRadius, enemySpawnRingMaxRadius);
            float maxR = Mathf.Max(enemySpawnRingMinRadius, enemySpawnRingMaxRadius);
            float height = Random.Range(enemySpawnRingHeightRange.x, enemySpawnRingHeightRange.y);

            Vector3 side = centerRot * Vector3.right;
            Vector3 up = centerRot * Vector3.up;

            for (int i = 0; i < enemySpawnRingTries; i++)
            {
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(minR, maxR);

                Vector3 offset = (Mathf.Cos(ang) * side + Mathf.Sin(ang) * up) * r;
                Vector3 candidate = centerPos + offset + up * height;

                if (!Physics.CheckSphere(candidate, enemySpawnClearRadius, obstacleLayerMask, QueryTriggerInteraction.Ignore))
                {
                    spawnPos = candidate;
                    return true;
                }
            }

            float angFallback = Random.Range(0f, Mathf.PI * 2f);
            float rFallback = Random.Range(minR, maxR);
            spawnPos = centerPos + (Mathf.Cos(angFallback) * side + Mathf.Sin(angFallback) * up) * rFallback + up * height;
            return true;
        }
        
        public Transform GetPlayerRoutePoint() => playerRoutePoint;

        public Transform GetEnemyRoutePoint(int index)
        {
            if (enemyRoutePoints == null || enemyRoutePoints.Count == 0)
                return null;
            if (index < 0 || index >= enemyRoutePoints.Count)
                return null;
            return enemyRoutePoints[index];
        }
        
        /// <summary>
        /// Удобный метод: спавн позади игрока на кольце + join-path на маршрут врага (влет/догон).
        /// </summary>
        public List<Vector3> BuildEnemyJoinPathFromRingBehindPlayer(int enemyRouteIndex, float joinAheadSegments = 2.5f)
        {
            if (!TryGetEnemySpawnPointOnRingBehindPlayer(enemyRouteIndex, out var start))
                return new List<Vector3>();

            return BuildEnemyJoinPath(start, enemyRouteIndex, joinAheadSegments);
        }

        /// <summary>
        /// Строит входной путь (waypoints) из внешней точки в точку на маршруте врага.
        /// Путь - кривая (без NavMesh), затем несколько итераций "раздвига" от астероидов.
        /// </summary>
        public List<Vector3> BuildEnemyJoinPath(
            Vector3 startWorldPos,
            int enemyRouteIndex,
            float joinAheadSegments = 2.5f)
        {
            var route = GetEnemyRoutePoint(enemyRouteIndex);
            if (route == null)
                return new List<Vector3> { startWorldPos };

            // Целимся не в текущую позицию routepoint, а чуть "вперёд" по маршруту,
            // чтобы вход выглядел как вылет на траекторию.
            float targetProgress = _enemyProgress != null && enemyRouteIndex >= 0 && enemyRouteIndex < _enemyProgress.Count
                ? _enemyProgress[enemyRouteIndex] + joinAheadSegments
                : _playerProgress; // fallback

            EnsureGeneratedForProgress(targetProgress + keepAnchorsAhead + 2);

            if (!TrySampleMainPath(targetProgress, out var basePos, out var baseRot))
                basePos = route.position;

            // Учитываем текущий хаотичный оффсет routepoint (чтобы влетать именно в "их" путь)
            Vector3 joinTarget = route.position;
            // Если routepoint уже обновляется тиком — лучше брать его фактическую позицию.
            // Но чтобы вход был "вперёд", слегка подтянем к sample из main path:
            joinTarget = Vector3.Lerp(joinTarget, basePos, 0.65f);

            // Строим квадратичную Безье-кривую: start -> control -> end
            Vector3 dir = (joinTarget - startWorldPos);
            float dist = dir.magnitude;
            Vector3 forward = dist > 0.001f ? dir / dist : (baseRot * Vector3.forward);

            // Контрольная точка: чуть вбок/вверх для "красивого" залёта
            Vector3 side = baseRot * Vector3.right;
            Vector3 up = baseRot * Vector3.up;

            float bendSide = Random.Range(-1f, 1f) * joinCurveBend;
            float bendUp = Random.Range(-0.35f, 0.35f) * joinCurveBend;

            Vector3 control = startWorldPos + forward * (dist * 0.55f) + side * bendSide + up * bendUp;

            int n = Mathf.Max(4, joinPathPoints);
            var path = new List<Vector3>(n);

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                Vector3 p = QuadraticBezier(startWorldPos, control, joinTarget, t);
                path.Add(p);
            }

            // Несколько итераций "релаксации" от препятствий.
            // Края (start/end) фиксируем, двигаем только внутренние точки.
            for (int iter = 0; iter < joinAvoidanceRelaxIterations; iter++)
            {
                for (int i = 1; i < path.Count - 1; i++)
                {
                    Vector3 p = path[i];
                    Vector3 push = ComputeObstaclePush(p, joinAvoidanceRadius, obstacleLayerMask);
                    path[i] = p + push * joinAvoidanceStrength;
                }
            }

            return path;
        }
        
        /// <summary>
        /// Join-path к "перехватной" точке на маршруте, которая находится позади игрока.
        /// Это нужно для фазы догоняния (влет на маршрут сзади).
        /// </summary>
        public List<Vector3> BuildEnemyJoinPathToCatchUpPointFromRingBehindPlayer(
            int enemyRouteIndex,
            float catchUpBehindSegments)
        {
            if (!TryGetEnemySpawnPointOnRingBehindPlayer(enemyRouteIndex, out var start))
                return new List<Vector3>();

            float progress = Mathf.Max(0f, PlayerProgress - Mathf.Max(0f, catchUpBehindSegments));
            if (!TrySampleMainPathAtProgress(progress, out var pos, out _))
                return new List<Vector3> { start };

            // Строим join путь к точке на маршруте (как к обычной цели)
            // Используем BuildEnemyJoinPath(), но там цель = routepoint; здесь хотим точку pos.
            // Поэтому временно вызываем локальный билд на квадратичной Безье к pos.
            return BuildJoinPathToWorldPointAvoidingObstacles(start, pos);
        }

        // --- helper: join to arbitrary world point (same avoidance idea) ---
        private List<Vector3> BuildJoinPathToWorldPointAvoidingObstacles(Vector3 startWorldPos, Vector3 joinTarget)
        {
            Vector3 dir = (joinTarget - startWorldPos);
            float dist = dir.magnitude;
            Vector3 forward = dist > 0.001f ? dir / dist : Vector3.forward;

            // контрольная точка для красивой дуги (ориентируемся от направления)
            Vector3 approxUp = Vector3.up;
            Vector3 side = Vector3.Cross(approxUp, forward);
            if (side.sqrMagnitude < 0.0001f) side = Vector3.right;
            side.Normalize();
            Vector3 up = Vector3.Cross(forward, side).normalized;

            float bendSide = Random.Range(-1f, 1f) * joinCurveBend;
            float bendUp = Random.Range(-0.35f, 0.35f) * joinCurveBend;

            Vector3 control = startWorldPos + forward * (dist * 0.55f) + side * bendSide + up * bendUp;

            int n = Mathf.Max(4, joinPathPoints);
            var path = new List<Vector3>(n);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                Vector3 p = QuadraticBezier(startWorldPos, control, joinTarget, t);
                path.Add(p);
            }

            for (int iter = 0; iter < joinAvoidanceRelaxIterations; iter++)
            {
                for (int i = 1; i < path.Count - 1; i++)
                {
                    Vector3 p = path[i];
                    Vector3 push = ComputeObstaclePush(p, joinAvoidanceRadius, obstacleLayerMask);
                    path[i] = p + push * joinAvoidanceStrength;
                }
            }

            return path;
        }

        
        /// <summary>
        /// Публичный доступ к семплингу основного пути (для догоняния/перехвата).
        /// </summary>
        public bool TrySampleMainPathAtProgress(float progress, out Vector3 pos, out Quaternion rot)
        {
            EnsureGeneratedForProgress(progress + 2);
            return TrySampleMainPath(progress, out pos, out rot);
        }


        private static Vector3 ComputeObstaclePush(Vector3 point, float radius, LayerMask mask)
        {
            Collider[] hits = Physics.OverlapSphere(point, radius, mask, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0)
                return Vector3.zero;

            Vector3 sum = Vector3.zero;
            int count = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                var c = hits[i];
                if (c == null) continue;

                Vector3 closest = c.ClosestPoint(point);
                Vector3 away = (point - closest);
                float d = away.magnitude;
                if (d < 0.0001f) continue;

                float k = Mathf.Clamp01(1f - (d / radius));
                sum += (away / d) * k;
                count++;
            }

            return count > 0 ? (sum / count) : Vector3.zero;
        }


        private void InitEnemyRoutePointState()
        {
            _enemyProgress.Clear();
            _enemySmoothedOffset.Clear();
            _enemySeed.Clear();

            for (int i = 0; i < enemyRoutePoints.Count; i++)
            {
                _enemyProgress.Add(Mathf.Max(0f, _playerProgress - enemyLagSegments));
                _enemySmoothedOffset.Add(Vector3.zero);
                _enemySeed.Add(Random.Range(1, int.MaxValue));
            }
        }

        private void TickRoutePoints(float dt)
        {
            // Двигаем "точку привязки" игрока строго по маршруту.
            _playerProgress += (playerRouteSpeed * dt) / Mathf.Max(0.0001f, segmentLength);

            EnsureGeneratedForProgress(_playerProgress + keepAnchorsAhead + 2);

            if (TrySampleMainPath(_playerProgress, out var pos, out var rot))
                playerRoutePoint.SetPositionAndRotation(pos, rot);

            // Враги: тот же путь, но с хаотичным оффсетом вокруг маршрута.
            for (int i = 0; i < enemyRoutePoints.Count; i++)
            {
                _enemyProgress[i] += (enemyRouteSpeed * dt) / Mathf.Max(0.0001f, segmentLength);

                // Держим врагов "примерно позади" игрока: если догнали — отбрасываем назад.
                float desiredMax = _playerProgress - enemyLagSegments * 0.5f;
                if (_enemyProgress[i] > desiredMax)
                    _enemyProgress[i] = desiredMax;

                float desiredMin = Mathf.Max(0f, _playerProgress - enemyLagSegments * 1.8f);
                if (_enemyProgress[i] < desiredMin)
                    _enemyProgress[i] = desiredMin;

                EnsureGeneratedForProgress(_enemyProgress[i] + keepAnchorsAhead + 2);

                if (!TrySampleMainPath(_enemyProgress[i], out var ePos, out var eRot))
                    continue;

                // Генерируем плавный оффсет (в локале маршрута) с редкими "пересечениями"
                int seed = _enemySeed[i];
                float t = Time.time;

                float nx = Mathf.PerlinNoise(seed * 0.001f, t * 0.22f) * 2f - 1f;
                float ny = Mathf.PerlinNoise(seed * 0.002f, t * 0.27f) * 2f - 1f;

                // Иногда делаем "пересечение": меняем знак бокового смещения
                float cross = Mathf.Sign(Mathf.Sin((t * 0.35f) + seed * 0.00001f));
                Vector3 desiredLocal = new Vector3(nx * enemyOffsetMax.x * cross, ny * enemyOffsetMax.y, 0f);

                _enemySmoothedOffset[i] =
                    Vector3.Lerp(_enemySmoothedOffset[i], desiredLocal, 1f - enemyOffsetSmoothing);

                Vector3 worldOffset = (eRot * Vector3.right) * _enemySmoothedOffset[i].x +
                                      (eRot * Vector3.up) * _enemySmoothedOffset[i].y;

                enemyRoutePoints[i].SetPositionAndRotation(ePos + worldOffset, eRot);
            }

            // Для чистки можно использовать target, но если он не задан — ориентируемся по игроку routepoint.
            if (target == null)
                target = playerRoutePoint;
        }

        private void BootstrapInitialAnchors()
        {
            var startPos = (target != null) ? target.position : transform.position;
            var startRot = (target != null) ? target.rotation : transform.rotation;

            _anchors.Clear();
            _anchorsStartIndex = 0;
            _anchorIndexCounter = 0;
            _smoothedLocalOffset = Vector3.zero;

            var first = CreateAnchor(startPos, startRot);
            _anchors.Add(first);

            for (int i = 0; i < keepAnchorsAhead + keepAnchorsBehind + 4; i++)
                AppendNextAnchorAndContent();
        }

        private void EnsureAnchorsAhead()
        {
            // Обеспечиваем буфер вперёд от текущего прогресса (игрока).
            float ensureTo = _playerProgress + keepAnchorsAhead + 4;
            EnsureGeneratedForProgress(ensureTo);
        }

        private void EnsureGeneratedForProgress(float progress)
        {
            int neededGlobalIndex = Mathf.CeilToInt(progress) + 2; // чтобы интерполяция всегда имела i и i+1
            while (LastAnchorGlobalIndex < neededGlobalIndex)
                AppendNextAnchorAndContent();
        }

        private void CleanupBehind()
        {
            // Держим фиксированное "окно" якорей вокруг текущего прогресса игрока.
            int minIndexToKeep = Mathf.FloorToInt(_playerProgress) - keepAnchorsBehind;
            while (_anchors.Count > 0 && FirstAnchorGlobalIndex < minIndexToKeep)
            {
                var old = _anchors[0];
                _anchors.RemoveAt(0);
                _anchorsStartIndex++;

                if (old != null)
                    Destroy(old.gameObject);
            }

            // Чистка астероидов — по расстоянию (простая, но рабочая).
            if (target == null) return;

            int safety = 512;
            while (_spawnedObstacles.Count > 0 && safety-- > 0)
            {
                var go = _spawnedObstacles.Peek();
                if (go == null)
                {
                    _spawnedObstacles.Dequeue();
                    continue;
                }

                float dist = Vector3.Distance(target.position, go.transform.position);
                if (dist > (segmentLength * (keepAnchorsBehind + keepAnchorsAhead + 8)))
                {
                    _spawnedObstacles.Dequeue();
                    Destroy(go);
                    continue;
                }

                break;
            }
        }

        private void AppendNextAnchorAndContent()
        {
            if (_anchors.Count == 0)
                return;

            Transform prev = _anchors[_anchors.Count - 1];
            Vector3 forward = prev.forward;

            Vector3 desiredLocalOffset = new Vector3(
                Random.Range(-_trajectoryOffsetMax.x, _trajectoryOffsetMax.x),
                Random.Range(-_trajectoryOffsetMax.y, _trajectoryOffsetMax.y),
                0f
            );

            _smoothedLocalOffset = Vector3.Lerp(_smoothedLocalOffset, desiredLocalOffset, 1f - offsetSmoothing);
            Vector3 worldOffset = prev.right * _smoothedLocalOffset.x + prev.up * _smoothedLocalOffset.y;

            Vector3 nextPos = prev.position + forward * segmentLength + worldOffset;

            float yaw = Random.Range(-yawPerSegmentMax, yawPerSegmentMax);
            float bank = (Random.value < _spineChance) ? Random.Range(-_spineMax, _spineMax) : 0f;

            Quaternion yawRot = Quaternion.AngleAxis(yaw, Vector3.up);
            Quaternion bankRot = Quaternion.AngleAxis(bank, Vector3.forward);
            Quaternion nextRot = prev.rotation * yawRot * bankRot;

            var nextAnchor = CreateAnchor(nextPos, nextRot);
            _anchors.Add(nextAnchor);

            SpawnAsteroidsAlongSegment(prev, nextAnchor);
        }

        private Transform CreateAnchor(Vector3 pos, Quaternion rot)
        {
            var go = new GameObject($"Anchor_{_anchorIndexCounter++:0000}");
            if (anchorsRoot != null) go.transform.SetParent(anchorsRoot, true);
            go.transform.SetPositionAndRotation(pos, rot);
            return go.transform;
        }

        private bool TryGetAnchorByGlobalIndex(int globalIndex, out Transform anchor)
        {
            int local = globalIndex - _anchorsStartIndex;
            if (local < 0 || local >= _anchors.Count)
            {
                anchor = null;
                return false;
            }

            anchor = _anchors[local];
            return anchor != null;
        }

        private bool TrySampleMainPath(float progress, out Vector3 pos, out Quaternion rot)
        {
            int i0 = Mathf.FloorToInt(progress);
            int i1 = i0 + 1;
            float t = progress - i0;

            if (!TryGetAnchorByGlobalIndex(i0, out var a0) || !TryGetAnchorByGlobalIndex(i1, out var a1))
            {
                pos = default;
                rot = default;
                return false;
            }

            pos = Vector3.Lerp(a0.position, a1.position, t);
            rot = Quaternion.Slerp(a0.rotation, a1.rotation, t);
            return true;
        }

        private void SpawnAsteroidsAlongSegment(Transform from, Transform to)
        {
            if (asteroidPrefabs == null || asteroidPrefabs.Count == 0)
                return;

            float length = Vector3.Distance(from.position, to.position);
            if (length <= 0.01f)
                return;

            float expected = asteroidsPerSegment;
            int count = Mathf.FloorToInt(expected);
            if (Random.value < (expected - count))
                count++;

            for (int i = 0; i < count; i++)
            {
                float t = Random.Range(0.15f, 0.95f);
                Vector3 p = Vector3.Lerp(from.position, to.position, t);

                Vector3 dir = (to.position - from.position).normalized;
                Vector3 side = Vector3.Cross(dir, Vector3.up);
                if (side.sqrMagnitude < 0.0001f) side = Vector3.Cross(dir, Vector3.right);
                side.Normalize();
                Vector3 up = Vector3.Cross(side, dir).normalized;

                float r = Random.Range(obstacleKeepoutRadius, obstacleSpawnRadius);
                float ang = Random.Range(0f, Mathf.PI * 2f);
                Vector3 radial = (Mathf.Cos(ang) * side + Mathf.Sin(ang) * up) * r;

                Vector3 spawnPos = p + radial;

                var prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count)];
                if (prefab == null) continue;

                var asteroid = Instantiate(prefab, spawnPos, Random.rotation, obstaclesRoot);
                float s = Random.Range(asteroidScaleRange.x, asteroidScaleRange.y);
                asteroid.transform.localScale = asteroid.transform.localScale * s;
                asteroid.SetActive(true);

                var drift = asteroid.GetComponent<Asteroid>();
                if (drift == null) drift = asteroid.AddComponent<Asteroid>();
                drift.SetupRandom(
                    spinDegPerSecRange: asteroidSpinDegPerSec,
                    driftAmplitudeRange: asteroidDriftAmplitude,
                    driftFrequencyRange: asteroidDriftFrequency
                );

                _spawnedObstacles.Enqueue(asteroid);
            }
        }
        
        private Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1f - t;
            return (u * u) * a + (2f * u * t) * b + (t * t) * c;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            if (_anchors == null || _anchors.Count == 0)
                return;

            Gizmos.color = gizmoAnchorColor;
            Transform prev = null;
            for (int i = 0; i < _anchors.Count; i++)
            {
                var a = _anchors[i];
                if (a == null) continue;

                Gizmos.DrawSphere(a.position, 0.6f);

                if (prev != null)
                {
                    Gizmos.color = gizmoLinkColor;
                    Gizmos.DrawLine(prev.position, a.position);
                    Gizmos.color = gizmoAnchorColor;
                }

                prev = a;
            }
        }
    }
}
