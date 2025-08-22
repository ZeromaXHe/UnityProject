using System.Collections.Generic;
using UnityEngine;

namespace CatlikeCodings.Prototypes.Maze2
{
    public class MazeCellObject : MonoBehaviour
    {
#if UNITY_EDITOR
        private static List<Stack<MazeCellObject>> _pools;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ClearPools()
        {
            if (_pools == null)
            {
                _pools = new List<Stack<MazeCellObject>>();
            }
            else
            {
                for (var i = 0; i < _pools.Count; i++)
                {
                    _pools[i].Clear();
                }
            }
        }
#endif

        [System.NonSerialized] private Stack<MazeCellObject> _pool;

        public MazeCellObject GetInstance()
        {
            if (_pool == null)
            {
                _pool = new Stack<MazeCellObject>();
#if UNITY_EDITOR
                _pools.Add(_pool);
#endif
            }

            if (_pool.TryPop(out var instance))
            {
                instance.gameObject.SetActive(true);
            }
            else
            {
                instance = Instantiate(this);
                instance._pool = _pool;
            }

            return instance;
        }

        public void Recycle()
        {
            _pool.Push(this);
            gameObject.SetActive(false);
        }
    }
}