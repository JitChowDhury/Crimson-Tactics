using UnityEngine;
using UnityEditor;

public class ObstacleTool : EditorWindow
{
    public ObstacleData obstacleData; //holds the 10 10 grid

    [MenuItem("Tools/Obstacle Tool")]//creates the tool
    public static void ShowWindow()
    {
        GetWindow<ObstacleTool>("Obstacle Tool");//opens the window
    }

    void OnGUI()
    {
        GUILayout.Label("Obstacle Grid Editor", EditorStyles.boldLabel); //adding the tutle

        //creates a box where i need to drop the obstacledata asset
        obstacleData = (ObstacleData)EditorGUILayout.ObjectField("Obstacle Data", obstacleData, typeof(ObstacleData), false);

        if (obstacleData == null)
        {
            EditorGUILayout.HelpBox("Please assign an obstacleData asset to edit.", MessageType.Info);//a message
            return;
        }

        for (int x = 0; x < 10; x++)
        {
            //start a row and put 10 toggle box side by side then in next loop next line and goes on
            EditorGUILayout.BeginHorizontal();
            for (int z = 0; z < 10; z++)
            {
                obstacleData.obstacles[x].row[z] = EditorGUILayout.Toggle(obstacleData.obstacles[x].row[z], GUILayout.Width(40));//40px width buttons
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Save"))
        {
            EditorUtility.SetDirty(obstacleData);//when save is clicked then then it mark it as dirty so unity knows that its changed
            AssetDatabase.SaveAssets();//save the changes
        }
    }
}
