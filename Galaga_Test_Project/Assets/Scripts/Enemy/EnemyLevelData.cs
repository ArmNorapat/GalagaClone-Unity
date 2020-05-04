using System.Collections.Generic;
using UnityEngine;

namespace Gulaga.Enemy
{
    /// <summary>
    /// Define what type of enemy in a row.
    /// </summary>
    [System.Serializable]
    public struct EnemyRowData
    {
        public enum RowEnemyType
        {
            BlueEnemy = 0,
            RedEnemy = 1,
            GreenEnemy = 2
        }

        public RowEnemyType enemyType;
    }

    /// <summary>
    /// How many enemies in a row.
    /// </summary>
    [System.Serializable]
    public struct EnemyRow
    {
        public List<Transform> EnemyPos;
    }

    /// <summary>
    /// Data of enemy in each row for each level.
    /// </summary>
    [CreateAssetMenu(fileName = "ControlData", menuName = "EnemyControlData", order = 1)]
    public class EnemyLevelData : ScriptableObject
    {
        public EnemyRowData[] rows;
    }
}