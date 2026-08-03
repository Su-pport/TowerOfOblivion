using UnityEngine;

[CreateAssetMenu(menuName = "Game/LevelData", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class LevelInfo
    {
        public int level;       
        public int expRequired;
    }

    public LevelInfo[] levels;
}
