using System;
using System.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME
{
    public static class Extentions
    {

        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }
        public static bool IsNullOrEmpty(this IList list)
        {
            return list == null || list.Count == 0;
        }
        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static T Clone<T>(this T prototype)
        {
            var data = JsonUtility.ToJson(prototype);
            try
            {
                var clonedItem = JsonUtility.FromJson<T>(data);
                return clonedItem;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return prototype;
            }
        }
        
        public static List<T> Shuffle<T>(this IEnumerable<T> list)
        {
            var outList = new List<T>();
            var tmpList = list.ToList();
            int count = tmpList.Count;
            for (int i = 0; i < count; i++)
            {
                var rndIndex = Random.Range(0, tmpList.Count - 1);
                outList.Add(tmpList[rndIndex]);
                tmpList.RemoveAt(rndIndex);
            }
            return outList;
        }

        public static T GetRandomItem<T>(this IEnumerable<T> list)
        {
            if (list.ToList().IsNullOrEmpty())
                return default;
            
            var rndIndex = Random.Range(0, list.Count() - 1);
            return list.ElementAt(rndIndex);
        }
        
        public static List<T> GetRandomItems<T>(this IEnumerable<T> list, int count)
        {
            var tmpList = list.ToList();
            if (tmpList.IsNullOrEmpty())
                return new List<T>();
            
            var outList = new List<T>();
            
            
            for (int i = 0; i < count; i++)
            {
                var rndIndex = Random.Range(0, tmpList.Count - 1);
                outList.Add(tmpList[rndIndex]);
                tmpList.RemoveAt(rndIndex);
                if (tmpList.IsNullOrEmpty())
                {
                    tmpList = list.ToList();
                }
            }
            
            return outList;
        }

        public static Vector3 GetRandomPoint(this Bounds bounds, bool ignoreHeight = true)
        {
            var center = bounds.center;
            var xx = bounds.size.x * .5f;
            var yy = bounds.size.y * .5f;
            var zz = bounds.size.z * .5f;


            var rndX = Random.Range(-xx, xx);
            var rndY = ignoreHeight? 0f: Random.Range(-yy,yy);
            var rndZ = Random.Range(-zz, zz);
            return center + new Vector3(rndX, rndY, rndZ);
        }

        public static Bounds RotateTo(this Bounds bounds, Vector3 direction)
        {
            var center = bounds.center;
            var size = bounds.size;
            
            if (Mathf.RoundToInt(direction.z) == 0)
            {
                size = new Vector3(bounds.size.z, bounds.size.y, bounds.size.x);
                center = new Vector3( bounds.center.z * (direction.x>0?1f:-1f), bounds.center.y, bounds.center.x * (direction.x<0?1f:-1f));
            }
            else
            {
                center.x *= direction.z > 0 ? 1f : -1f;
                center.z *= direction.z > 0 ? 1f : -1f;
            }
            
            return new Bounds(center, size);
        }

        public static int GetRandom(this Vector2Int vector)
        {
            return Random.Range(vector.x, vector.y + 1);
        }
        
    }
}