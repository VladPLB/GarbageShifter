using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    public class MapPathBuilder: MonoBehaviour
{
    [SerializeField]
    private Transform _cameraTransform;
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
        bool isFuturePathEnabled = currentLocationIndex < rawPoints.Count-1;
        bool isPastPathEnabled = currentLocationIndex > 0;
        _lineRenderer.enabled = isFuturePathEnabled;
        if(isFuturePathEnabled)
        {
            BuildLine(_lineRenderer, rawPoints, currentLocationIndex, rawPoints.Count);
        }
        _lineRendererCompleted.enabled = isPastPathEnabled;
        if(isPastPathEnabled)
        {
            BuildLine(_lineRendererCompleted, rawPoints, 0, currentLocationIndex+1);
        }

        SetupLocationPoints(rawPoints, currentLocationIndex, currentLevelIndex);
        
        var cameraPos = rawPoints[currentLocationIndex];
        cameraPos.y = 8f;
        cameraPos.z += 5f;
        _cameraTransform.localPosition = cameraPos;
    }

    private void SetupLocationPoints(List<Vector3> rawPoints, int currentLocationIndex, int currentLevelIndex)
    {
        for (int i = 0; i < rawPoints.Count; i++)
        {
            var point = Instantiate(_pointPrefab, _root);
            point.transform.localPosition = rawPoints[i];
            LevelLocation _locationData = _zoneData.Locations[i];
            point.name = $"Point_{_locationData.type}_{i}";
            point.Init(_pool, _locationData, i);
            point.CurrentProgress(currentLocationIndex, currentLevelIndex);
            _points.Add(point);
        }
    }

    private void BuildLine(LineRenderer lineRenderer, List<Vector3> rawPoints, int start, int end)
    {
        var points = new List<Vector3>();
       // for (int i = start; i < Mathf.Min(rawPoints.Count,end); i++)
       // {
       //     points.Add(rawPoints[i]);
       // }
        for (int i = 0; i < rawPoints.Count; i++)
        {
            points.Add(rawPoints[i]);
        }
        lineRenderer.widthCurve = AnimationCurve.Constant(0,1,_lineWidth);
        lineRenderer.useWorldSpace = false;
        var smoothed = GenerateCatmullRomPoints(points,start,end, _subdivisions);
        var finalPoints = new List<Vector3>(smoothed);

        //InsertTails(smoothed, finalPoints, count<1);

        lineRenderer.positionCount = finalPoints.Count;
        
        for (int i = 0; i < smoothed.Count; i++)
        {
            lineRenderer.SetPosition(i, smoothed[i]);
        }
    }

    private List<Vector3> GenerateCatmullRomPoints(List<Vector3> points, int start, int end, int subdivisions)
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
                if(i>=start && i<end-1)
                {
                    result.Add(pos);
                }
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