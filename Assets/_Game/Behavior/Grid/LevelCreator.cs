using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class LevelCreator : MonoBehaviour
{
    public LevelData LevelData;

    private const string PATH = "Assets/_Game/Levels/Level.asset";

    [ContextMenu("Save Level Data")]
    public void SaveData()
    {
        LevelData ld = LevelData;
        if (ld == null)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("You can't create a new LevelData in the editor. The game needs to be running so that the cell data can be read from");
                return;
            }

            ld = ScriptableObject.CreateInstance<LevelData>();
        }

        bool bSuccess = true;

        GridManager.Instance.PopulateGridData(ref ld);
        GetSpawnerData(ref ld);
        bSuccess &= GetHQPosition(ref ld); //make sure we don't short circuit

        if (!bSuccess)
        {
            Debug.LogError("There was some problem with the level. Not writing it.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(ld);
        if (string.IsNullOrEmpty(path))
        {
            path = AssetDatabase.GenerateUniqueAssetPath(PATH);
            AssetDatabase.CreateAsset(ld, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = ld;

        if (LevelData == null)
        {
            LevelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        }
    }

    private bool GetHQPosition(ref LevelData ld)
    {
        HQ[] hqs = FindObjectsByType<HQ>(FindObjectsSortMode.None);
        if (hqs.Length != 1)
        {
            Debug.LogError("There are either too many HQs or there isn't one. Fix this");
            return false;
        }
        ld.HQCoordinates = hqs[0].Coordinates;
        return true;
    }

    private void GetSpawnerData(ref LevelData ld)
    {
        List<SpawnerData> spawnerDatas = new();

        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        foreach (EnemySpawner spawner in spawners)
        {
            spawnerDatas.Add(new SpawnerData
            {
                EnemyCount = spawner.enemyNum,
                WaveNum    = spawner.waveNum,
                Position   = spawner.transform.position,
                Rotation   = spawner.transform.rotation
            });
        }

        ld.Spawners = spawnerDatas.ToArray();
    }
}
#endif //UNITY_EDITOR