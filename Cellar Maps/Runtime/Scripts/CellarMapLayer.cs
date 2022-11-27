using IUP.Toolkits.Matrices;
using System;
using UnityEngine;

namespace IUP.Toolkits.CellarMaps
{
    [Serializable]
    public sealed class CellarMapLayer : ISerializationCallbackReceiver, ICellarMapLayer
    {
        public CellarMapLayer(int width, int height)
        {
            Matrix = new Matrix<CellType>(width, height);
        }

        public int Width => Matrix.Width;
        public int Height => Matrix.Height;
        public Matrix<CellType> Matrix { get; private set; }

        /// <summary>
        /// Событие, уведомляющее об изменениях клеток на слое. Первый аргумент - сам слой, второй - 
        /// координаты изменённых клеток.
        /// </summary>
        public event Action CellsChanged;

        [SerializeField] private int _serializableWidth;
        [SerializeReference] private CellType[] _serializableLayer;

        public CellType this[Vector2Int coordinate]
        {
            get => Matrix[coordinate];
            set => SetCellByCoordinate(value, coordinate.x, coordinate.y);
        }
        public CellType this[int x, int y]
        {
            get => Matrix[x, y];
            set => SetCellByCoordinate(value, x, y);
        }

        /// <summary>
        /// Пересоздаёт слой, сбрасывая все значения.
        /// </summary>
        /// <param name="width">Ширина слоя: должна быть больше или равна 1.</param>
        /// <param name="height">Высота слоя: должна быть больше или равна 1.</param>
        /// <exception cref="ArgumentException"></exception>
        public void Recreate(int width, int height)
        {
            if (width < 1)
            {
                throw new ArgumentException("Ширина слоя должна быть больше или равна 1.");
            }
            if (height < 1)
            {
                throw new ArgumentException("Высота слоя должна быть больше или равна 1.");
            }
            Matrix.Recreate(width, height);
        }

        public void Fill(CellType type)
        {
            Matrix.InitAllElements(() => type);
            CellsChanged?.Invoke();
        }

        public void ReplaceWithOther(CellType replace, CellType other)
        {
            Matrix.ForEachElements(
                delegate (ref CellType element)
                {
                    if (element == replace)
                    {
                        element = other;
                    }
                });
            CellsChanged?.Invoke();
        }

        public void Clear()
        {
            Fill(null);
        }

        public void ClearFrom(CellType removedType)
        {
            ReplaceWithOther(removedType, null);
        }

        private void SetCellByCoordinate(CellType type, int x, int y)
        {
            Matrix[x, y] = type;
            CellsChanged?.Invoke();
        }

        public void OnBeforeSerialize()
        {
            _serializableWidth = Width;
            _serializableLayer = Matrix.ToArray();
        }

        public void OnAfterDeserialize()
        {
            int layerHeight = _serializableLayer.Length / _serializableWidth;
            Matrix = new Matrix<CellType>(_serializableWidth, layerHeight);
            Matrix.InitAllElements((int x, int y) => _serializableLayer[y * _serializableWidth + x]);
        }
    }
}
