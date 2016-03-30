﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gist {
    
    public class HashGrid<T> : System.IDisposable, IEnumerable<T> where T : class {
        System.Func<T, Vector3> _GetPosition;
        LinkedList<T>[] _grid;
        List<T> _points;
        Hash _hash;

        public HashGrid(System.Func<T, Vector3> GetPosition, float cellSize, int nx, int ny, int nz) {
            this._GetPosition = GetPosition;
            this._points = new List<T> ();
            Rebuild (cellSize, nx, ny, nz);
        }

        public void Add(T point) {
            _points.Add (point);
            AddOnGrid(point);
        }
        public void Remove(T point) {
            RemoveOnGrid(point);
            _points.Remove (point);
        }
        public T Find(System.Predicate<T> Predicate) {
            return _points.Find (Predicate);
        }
        public IEnumerable<S> Neighbors<S>(Vector3 center, float distance) where S:T {
            var r2 = distance * distance;
            foreach (var id in _hash.CellIds(center, distance)) {
                var cell = _grid [id];
                foreach (var p in cell) {
                    var s = p as S;
                    if (s == null)
                        continue;
                    
                    var d2 = (_GetPosition (s) - center).sqrMagnitude;
                    if (d2 < r2)
                        yield return s;
                }
            }
        }
		public void Rebuild(float cellSize, int nx, int ny, int nz) {
            _hash = new Hash (cellSize, nx, ny, nz);
            var totalCells = nx * ny * nz;
            if (_grid == null || _grid.Length != totalCells) {
                _grid = new LinkedList<T>[totalCells];
                for (var i = 0; i < _grid.Length; i++)
                    _grid [i] = new LinkedList<T> ();
            }
            Update ();
		}
        public void Update() {
            var limit = _grid.Length;
            for (var i = 0; i < limit; i++)
                _grid [i].Clear ();
            foreach (var p in _points)
                AddOnGrid (p);
        }
        public int[,,] Stat() {
            var counter = new int[_hash.nx, _hash.ny, _hash.nz];
            for (var z = 0; z < _hash.nz; z++)
                for (var y = 0; y < _hash.ny; y++)
                    for (var x = 0; x < _hash.nx; x++)
                        counter [x, y, z] = _grid [_hash.CellId (x, y, z)].Count;
            return counter;
        }

        void AddOnGrid (T point) {
            var id = _hash.CellId (_GetPosition (point));
            var cell = _grid [id];
            cell.AddLast(point);
        }
        void RemoveOnGrid (T point) {
            var id = _hash.CellId (_GetPosition (point));
            var cell = _grid [id];
            cell.Remove (point);
        }

        #region IDisposable implementation
        public void Dispose () {}
        #endregion

        #region IEnumerable implementation
        public IEnumerator<T> GetEnumerator () {
            return _points.GetEnumerator ();
        }
        #endregion

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator () {
            return this.GetEnumerator ();
        }
        #endregion

		public class Hash {
			public readonly Vector3 gridSize;
			public readonly float cellSize;
			public readonly int nx, ny, nz;

			public Hash(float cellSize, int nx, int ny, int nz) {
				this.cellSize = cellSize;
				this.nx = nx;
				this.ny = ny;
				this.nz = nz;
				this.gridSize = new Vector3(nx * cellSize, ny * cellSize, nz * cellSize);
			}
			public IEnumerable<int> CellIds(Vector3 position, float radius) {
                var fromx = CellX (position.x - radius);
                var fromy = CellY (position.y - radius);
                var fromz = CellZ (position.z - radius);
                var widthx = CellX (position.x + radius) - fromx;
                var widthy = CellY (position.y + radius) - fromy;
                var widthz = CellZ (position.z + radius) - fromz;
                if (widthx < 0)
                    widthx += nx;
                if (widthy < 0)
                    widthy += ny;
                if (widthz < 0)
                    widthz += nz;
                
				for (var z = 0; z <= widthz; z++)
					for (var y = 0; y <= widthy; y++)
						for (var x = 0; x <= widthx; x++)
							yield return CellId (x + fromx, y + fromy, z + fromz);
			}
			public int CellId(Vector3 position) {
				return CellId (CellX (position.x), CellY (position.y), CellZ (position.z));
			}
			public int CellId(int x, int y, int z) {
                x = Mod (x, nx);
                y = Mod (y, ny);
                z = Mod (z, nz);
				return x + (y + z * ny) * nx;
			}
			public int CellX(float posX) {
				posX -= gridSize.x * Mathf.CeilToInt (posX / gridSize.x);
				return (int)(posX / cellSize);
			}
			public int CellY(float posY) {
				posY -= gridSize.y * Mathf.CeilToInt (posY / gridSize.y);
				return (int)(posY / cellSize);
			}
			public int CellZ(float posZ) {
				posZ -= gridSize.z * Mathf.CeilToInt (posZ / gridSize.z);
				return (int)(posZ / cellSize);
			}
            public int Mod(int x, int mod) {
                return x - Mathf.FloorToInt ((float)x / mod) * mod;
            }
		}
    }
}