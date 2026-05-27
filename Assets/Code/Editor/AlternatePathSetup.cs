using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AlternatePathSetup
{
    private const string AlternatePathName = "Bottom Entry Path";
    private const int JoinWaypointIndex = 5; // The sixth waypoint in the configured main path.

    [MenuItem("Tools/Tower Defence/Create Bottom Entry Path In Open Scene")]
    private static void CreateBottomEntryPath()
    {
        LevelMananger levelManager = Object.FindFirstObjectByType<LevelMananger>();
        EnemySpawner spawner = Object.FindFirstObjectByType<EnemySpawner>();

        if (levelManager == null || spawner == null)
        {
            Debug.LogError("The open scene needs LevelMananger and EnemySpawner components.");
            return;
        }

        if (levelManager.transform.Find(AlternatePathName) != null)
        {
            Debug.LogWarning("Bottom entry path already exists in the open scene.");
            return;
        }

        SerializedObject levelObject = new SerializedObject(levelManager);
        SerializedProperty paths = levelObject.FindProperty("paths");
        if (paths.arraySize == 0)
        {
            Debug.LogError("Configure Path 0 before creating a bottom entry path.");
            return;
        }

        SerializedProperty sourceWaypoints = paths.GetArrayElementAtIndex(0)
            .FindPropertyRelative("waypoints");
        if (sourceWaypoints.arraySize <= JoinWaypointIndex)
        {
            Debug.LogError("Path 0 does not contain the sixth waypoint.");
            return;
        }

        GameObject pathRoot = new GameObject(AlternatePathName);
        Undo.RegisterCreatedObjectUndo(pathRoot, "Create bottom entry path");
        pathRoot.transform.SetParent(levelManager.transform, false);

        Transform joinWaypoint = sourceWaypoints.GetArrayElementAtIndex(JoinWaypointIndex)
            .objectReferenceValue as Transform;
        Transform mainEntry = sourceWaypoints.GetArrayElementAtIndex(0)
            .objectReferenceValue as Transform;
        if (joinWaypoint == null || mainEntry == null)
        {
            Debug.LogError("Path 0 entry point or sixth waypoint is missing.");
            Object.DestroyImmediate(pathRoot);
            return;
        }

        GameObject bottomEntry = new GameObject("Bottom Entry");
        Undo.RegisterCreatedObjectUndo(bottomEntry, "Create bottom entry waypoint");
        bottomEntry.transform.SetParent(pathRoot.transform, false);
        bottomEntry.transform.position = new Vector3(
            joinWaypoint.position.x,
            -Mathf.Abs(mainEntry.position.y),
            joinWaypoint.position.z);

        Undo.RecordObject(levelManager, "Assign bottom entry path");
        paths.arraySize = Mathf.Max(paths.arraySize, 2);
        SerializedProperty alternateWaypoints = paths.GetArrayElementAtIndex(1)
            .FindPropertyRelative("waypoints");
        alternateWaypoints.arraySize = sourceWaypoints.arraySize - JoinWaypointIndex + 1;
        alternateWaypoints.GetArrayElementAtIndex(0).objectReferenceValue = bottomEntry.transform;
        for (int i = JoinWaypointIndex; i < sourceWaypoints.arraySize; i++)
        {
            alternateWaypoints.GetArrayElementAtIndex(i - JoinWaypointIndex + 1).objectReferenceValue =
                sourceWaypoints.GetArrayElementAtIndex(i).objectReferenceValue;
        }
        levelObject.ApplyModifiedProperties();

        SplitSecondWaveBetweenPaths(spawner);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Created bottom entry path joining the main path at waypoint 6 and assigned both paths in Wave 2.");
    }

    private static void SplitSecondWaveBetweenPaths(EnemySpawner spawner)
    {
        SerializedObject spawnerObject = new SerializedObject(spawner);
        SerializedProperty waves = spawnerObject.FindProperty("waves");
        if (waves.arraySize < 2)
        {
            Debug.LogWarning("Bottom entry path created, but Wave 2 is not configured.");
            return;
        }

        SerializedProperty firstWaveEnemies = waves.GetArrayElementAtIndex(0)
            .FindPropertyRelative("enemies");
        SerializedProperty secondWaveEnemies = waves.GetArrayElementAtIndex(1)
            .FindPropertyRelative("enemies");
        if (firstWaveEnemies.arraySize == 0 || secondWaveEnemies.arraySize == 0)
        {
            Debug.LogWarning("Bottom entry path created, but the first two waves need enemy entries.");
            return;
        }

        Object pathZeroPrefab = firstWaveEnemies.GetArrayElementAtIndex(0)
            .FindPropertyRelative("enemyPrefab")
            .objectReferenceValue;
        Object pathOnePrefab = secondWaveEnemies.GetArrayElementAtIndex(0)
            .FindPropertyRelative("enemyPrefab")
            .objectReferenceValue;

        Undo.RecordObject(spawner, "Split wave between paths");
        for (int i = 0; i < secondWaveEnemies.arraySize; i++)
        {
            bool usesAlternatePath = i % 2 == 1;
            SerializedProperty entry = secondWaveEnemies.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("enemyPrefab").objectReferenceValue =
                usesAlternatePath ? pathOnePrefab : pathZeroPrefab;
            entry.FindPropertyRelative("pathIndex").intValue =
                usesAlternatePath ? 1 : 0;
        }

        spawnerObject.ApplyModifiedProperties();
    }
}
