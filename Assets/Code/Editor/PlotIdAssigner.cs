using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlotIdAssigner
{
    [MenuItem("Tools/Tower Defence/Assign Plot IDs In Open Scene")]
    private static void AssignPlotIds()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Plot[] allPlots = UnityEngine.Object.FindObjectsByType<Plot>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        List<Plot> scenePlots = new List<Plot>();

        foreach (Plot plot in allPlots)
        {
            if (plot.gameObject.scene == activeScene)
                scenePlots.Add(plot);
        }

        scenePlots.Sort((left, right) =>
            string.CompareOrdinal(GetHierarchyKey(left.transform), GetHierarchyKey(right.transform)));

        Undo.RecordObjects(scenePlots.ToArray(), "Assign Plot IDs");

        for (int i = 0; i < scenePlots.Count; i++)
        {
            SerializedObject plotObject = new SerializedObject(scenePlots[i]);
            plotObject.FindProperty("plotId").intValue = i;
            plotObject.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(activeScene);
        Debug.Log($"Assigned unique IDs to {scenePlots.Count} plots in {activeScene.name}.");
    }

    private static string GetHierarchyKey(Transform transform)
    {
        string key = $"{transform.GetSiblingIndex():D4}:{transform.name}";
        Transform parent = transform.parent;

        while (parent != null)
        {
            key = $"{parent.GetSiblingIndex():D4}:{parent.name}/{key}";
            parent = parent.parent;
        }

        return key;
    }
}
