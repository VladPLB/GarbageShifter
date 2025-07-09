using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    public class MapPathBuilder: MonoBehaviour
{
    [SerializeField]
    private Transform _root;
    [SerializeField]
    private MapLocationPoint _pointPrefab;
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private LineRenderer _lineRendererCompleted;
    [SerializeField]
    private float _lineWidth;
    [SerializeField]
    private int _subdivisions;
    [SerializeField]
    private float _tailLength;
    
    private  Pool<MapLocationItem ,MapManager.LocationType> _pool;
    private LevelZoneData _zoneData;
    private List<MapLocationPoint> _points = new();

    private void BuildPath(int currentLocationIndex, int currentLevelIndex)
    {
        var rawPoints = new List<Vector3>();
        foreach (var loc in _zoneData.Locations)
        {
            rawPoints.Add(loc.uiPosition);
        }
        
        BuildLine(_lineRenderer, rawPoints);
        BuildLine(_lineRendererCompleted, rawPoints, currentLocationIndex+1);

        SetupLocationPoints(rawPoints, currentLocationIndex, currentLevelIndex);
    }

    private void SetupLocationPoints(List<Vector3> rawPoints, int currentLocationIndex, int currentLevelIndex)
    {
        for (int i = 0; i < rawPoints.Count; i++)
        {
            var point = Instantiate(_pointPrefab, _root);
            point.transform.localPosition = rawPoints[i];
            LevelLocation _locationData = _zoneData.Locations[i];
            point.name = $"Point_{_locationData.type}_{i}";
            bool isComplete = i < currentLocationIndex;
            bool isActive = i == currentLevelIndex;
            point.Init(_pool, _locationData, i);
            _points.Add(point);
        }
    }

    private void BuildLine(LineRenderer lineRenderer, List<Vector3> rawPoints, int count = 0)
    {
        var points = new List<Vector3>();
        if (count < 1)
        {
            points = rawPoints.ToList();
        }
        else
        {
            for (int i = 0; i < Mathf.Min(rawPoints.Count,count); i++)
            {
                points.Add(rawPoints[i]);
            }
        }
        lineRenderer.widthCurve = AnimationCurve.Constant(0,1,_lineWidth);
        lineRenderer.useWorldSpace = false;
        var smoothed = GenerateCatmullRomPoints(points, _subdivisions);
        var finalPoints = new List<Vector3>(smoothed);

        //InsertTails(smoothed, finalPoints, count<1);

        lineRenderer.positionCount = finalPoints.Count;
        
        for (int i = 0; i < smoothed.Count; i++)
        {
            lineRenderer.SetPosition(i, smoothed[i]);
        }
    }

    private void InsertTails(List<Vector3> smoothed, List<Vector3> finalPoints, bool isEnd)
    {
        if (smoothed.Count >= 2)
        {
            var dirStart = (smoothed[1] - smoothed[0]).normalized;
            var tailStart = smoothed[0] - dirStart * _tailLength;
            finalPoints.Insert(0, tailStart);
            if(isEnd)
            {
                int last = smoothed.Count - 1;
                var dirEnd = (smoothed[last] - smoothed[last - 1]).normalized;
                var tailEnd = smoothed[last] + dirEnd * _tailLength;
                finalPoints.Add(tailEnd);
            }
        }
    }

    private List<Vector3> GenerateCatmullRomPoints(List<Vector3> points, int subdivisions)
    {
        var result = new List<Vector3>();
        if (points.Count < 2)
        {
            return result;
        }
        
        var extended = new List<Vector3> { points[0] };
        extended.AddRange(points);
        extended.Add(points[points.Count - 1]);
        
        for (int i = 0; i < extended.Count - 3; i++)
        {
            Vector3 p0 = extended[i];
            Vector3 p1 = extended[i + 1];
            Vector3 p2 = extended[i + 2];
            Vector3 p3 = extended[i + 3];

            for (int j = 0; j <= subdivisions; j++)
            {
                float t = j / (float)subdivisions;
                Vector3 pos = 0.5f * (
                    2f * p1 +
                    (-p0 + p2) * t +
                    (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                    (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
                );
                result.Add(pos);
            }
        }

        return result;
    }
    
    public void Init(LevelZoneData zoneData, int targetLocationIndex, int targetLevelIndex)
    {
        _pool = Core.Get<PoolProvider>().MapLocationItem;
        _zoneData = zoneData;
        BuildPath(targetLocationIndex, targetLevelIndex);
    }

    public void Release()
    {
        for (int i = 0; i < _points.Count; i++)
        {
            _points[i].Clear();
            Destroy(_points[i].gameObject);
        }
        _points.Clear();
        _lineRenderer.positionCount = 0;
    }
}
}