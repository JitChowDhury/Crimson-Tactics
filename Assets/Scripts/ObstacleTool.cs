using UnityEngine;
using UnityEditor;

public class ObstacleTool : EditorWindow
{
    public ObstacleData obstacleData;//reference to the object

    [MenuItem("Tools/Obstacle Tool")]
    //opens the windoww
    public static void ShowWindow()
    {
        GetWindow<ObstacleTool>("Obstacle Tool");
    }

    //its called by unity everyt frame while editor is open
    void OnGUI()
    {
        GUILayout.Label("Obstacle Grid Editor", EditorStyles.boldLabel);//displays the title

        //asset can be manually changed
        obstacleData = (ObstacleData)EditorGUILayout.ObjectField(
            "Obstacle Data",
            obstacleData,
            typeof(ObstacleData),
            false
        );
        //safety check
        if (obstacleData == null)
        {
            EditorGUILayout.HelpBox("Assign an ObstacleData asset to edit.", MessageType.Info);
            return;
        }

        for (int x = 0; x < 10; x++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int z = 0; z < 10; z++)
            {   //draws one toggle per tile
                //directly edits the scriptable object data
                obstacleData.obstacles[x].row[z] =
                    EditorGUILayout.Toggle(obstacleData.obstacles[x].row[z], GUILayout.Width(25));

            }
            EditorGUILayout.EndHorizontal();
        }
        //save button
        if (GUILayout.Button("Save"))
        {
            //marks the scriptable object as modified
            EditorUtility.SetDirty(obstacleData);
            AssetDatabase.SaveAssets();
        }
    }
}
