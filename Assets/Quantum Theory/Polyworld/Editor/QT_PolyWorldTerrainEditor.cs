using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(QT_PolyWorldTerrain))]
public class QT_PolyWorldTerrainEditor : Editor {

    private QT_PolyWorldTerrain PWT;

    private GUIStyle boxStyle;
    Texture WorldIcon;
    Texture UpdateImg;
    Texture UpdateColor;

    public override void OnInspectorGUI()
    {
#if UNITY_IOS
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("iOS Platform Warning:\n\nWhile PolyWorld's content is compatible on iOS, PolyWorld Terrain color\ngeneration is not compatible due to iOS's texture compression scheme.\n\nPlease switch back to PC/Standalone to create PolyWorld terrains,\nthen back to iOS to make your build.");
        GUILayout.EndVertical();
        EditorGUILayout.Space();
#else
        GUILayout.BeginHorizontal();
        GUILayout.Box(WorldIcon, GUILayout.Width(WorldIcon.width), GUILayout.Height(WorldIcon.height), GUILayout.ExpandWidth(true));
        //GUILayout.Label(WorldIcon, GUILayout.Width(WorldIcon.width), GUILayout.Height(WorldIcon.height),GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        PWT.SetupTerrainAssociation();
        PWT.chunkSizes = PWT.GetChunkSizes();
        GUI.color = new Color(1f, 1f, 1f);
        
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Terrain Visibility:");
        GUILayout.BeginHorizontal();
        PWT.terrainGOEnabled = GUILayout.Toggle(PWT.terrainGOEnabled, "Source Terrain",GUI.skin.button,GUILayout.Height(30));      
            
        PWT.gameObject.GetComponent<Terrain>().drawHeightmap= PWT.terrainGOEnabled;

        PWT.PWTGOEnabled = GUILayout.Toggle(PWT.PWTGOEnabled, "PolyWorld Terrain", GUI.skin.button, GUILayout.Height(30));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        if (Selection.activeGameObject.transform.childCount > 0)
        {
            GUILayout.Space(10f);
            Transform goc = Selection.activeGameObject.transform.GetChild(0);
            MeshRenderer[] children = goc.gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer t in children)
                t.enabled = PWT.PWTGOEnabled;

           
                if (GUILayout.Button(UpdateColor, GUILayout.Height(UpdateColor.height), GUILayout.Width(UpdateColor.width), GUILayout.ExpandWidth(true)))
                {
                    MeshFilter[] meshes =Selection.activeGameObject.transform.GetChild(0).GetComponentsInChildren<MeshFilter>();
                    List<GameObject> LOD0 = new List<GameObject>();
                    List<GameObject> LOD1 = new List<GameObject>();
                    foreach (MeshFilter m in meshes)
                    {
                        if (m.name.Contains("LOD0"))
                            LOD0.Add(m.gameObject);
                        else if (m.name.Contains("LOD1"))
                            LOD1.Add(m.gameObject);
                        else
                            LOD0.Add(m.gameObject);
                    }


                    PWT.RenderVertexColors(LOD0.ToArray());
                    if (LOD1.Count > 0)
                    {
                        //set the quadsizebias up one, then put it back down to reflect the top LOD level
                        PWT.quadsizeBiasIndex++;
                        PWT.RenderVertexColors(LOD1.ToArray());
                        PWT.quadsizeBiasIndex--;
                    }
                }
                GUILayout.Space(10f);
            
        }
        else
        {
            PWT.prefabPath = null; //we reset the prefab path since there aren't any polyworld terrains
            PWT.meshPath = null;
        }

        #region Generate Button
        if (GUILayout.Button(UpdateImg, GUILayout.Height(UpdateImg.height), GUILayout.Width(UpdateImg.width), GUILayout.ExpandWidth(true)))
        {
            if (RunChecks())
            {
                if (PWT.deletePWTerrain)
                {
                    if (Selection.activeGameObject.transform.childCount > 0)
                    {
                        for (int c = Selection.activeGameObject.transform.childCount - 1; c >= 0; c--)
                            GameObject.DestroyImmediate(Selection.activeGameObject.transform.GetChild(c).gameObject);
                    }
                }

                int priorBiasIndex = PWT.quadsizeBiasIndex;
                List<GameObject[]> everyLODSet = new List<GameObject[]>(); // a list of gameobject arrays. Each element represents LOD sets. everyLODSet[0] is LOD0, [1] is LOD1, etc.. We do this becasue we don't know how many LOD sets there will be.

                GameObject previousPWT = (GameObject)PrefabUtility.GetPrefabParent(PWT.gameObject.GetComponentInChildren<Transform>().gameObject);

                GameObject PWTParent = new GameObject(); //our main parent.
                PWTParent.name = PWT.gameObject.name + "-Faceted";
                PWTParent.transform.position = PWT.gameObject.transform.position;
                PWTParent.transform.parent = PWT.gameObject.transform;

                

                if (PWT.GenerateLODs)
                {
                    int lodCounter = 0;
                    everyLODSet.Add(PWT.GeneratePolyWorldTerrain(PWTParent, lodCounter));
                    if (PWT.quadsizeBiasIndex != PWT.quadsizeBias.Length - 1) //change ths to a while loop for more lods. don't forget to change UpdateLODSizes() method and LODSizes!
                    {
                        lodCounter++;
                        PWT.quadsizeBiasIndex++;
                        everyLODSet.Add(PWT.GeneratePolyWorldTerrain(PWTParent, lodCounter));                      
                    }
                }
                else
                    everyLODSet.Add(PWT.GeneratePolyWorldTerrain(PWTParent, 0));

                if (PWT.GenerateMeshColliders)
                {
                    PWT.gameObject.GetComponent<TerrainCollider>().enabled = false;

                    for (int x = 0; x < everyLODSet.Count; x++)
                    {
                        foreach (GameObject g in everyLODSet[x])
                            g.AddComponent<MeshCollider>();
                    }
                }
                else
                    PWT.gameObject.GetComponent<TerrainCollider>().enabled = true; 

                //save the meshes to disk. associate them with the right gameobject.
                //WriteAllContent(PWTParent, everyLODSet);

                //go through all the chunks. we do [0] because each element is the same length, and we reference them directly, so it doesn't matter.
                if (PWT.GenerateLODs)
                {
                    //for every chunk..
                    for (int chunkIndex = 0; chunkIndex < everyLODSet[0].Length; chunkIndex++)
                    {
                        //everylodset[0] is lod0, [1] is lod1, etc..                      
                        GameObject LODObject = new GameObject();
                        LODObject.transform.parent = PWTParent.transform; //parent it to the terrain
                        LODObject.transform.position = everyLODSet[0][chunkIndex].transform.position; //move it to the lod0 position
                        LODObject.name = everyLODSet[0][chunkIndex].name;
                        LODObject.name = LODObject.name.Replace("LOD0", "");

                        everyLODSet[0][chunkIndex].transform.parent = LODObject.transform;

                        LODGroup LODG = LODObject.AddComponent<LODGroup>();

                        LOD[] lods = new LOD[everyLODSet.Count];

                        //go through each LOD at the current chunkindex and grab the renderer component
                        for (int LODIndex = 0; LODIndex < everyLODSet.Count; LODIndex++)
                        {                           
                            everyLODSet[LODIndex][chunkIndex].transform.parent = LODObject.transform;
                            Renderer[] renderer = new Renderer[1]; //declare an array to hold the mesh renderer component of the associated lod
                            //grab the renderer component from the LOD->Chunk
                            renderer[0] = everyLODSet[LODIndex][chunkIndex].GetComponent<Renderer>();
                            //plug it into the lod array                           
                            lods[LODIndex] = new LOD(PWT.LODSizes[LODIndex], renderer);
                           // lods[LODIndex].fadeTransitionWidth = 0.5f;
                            
                        }
                      //  LODG.fadeMode = LODFadeMode.CrossFade;
                        LODG.SetLODs(lods);
                    }
                } 
                PWT.quadsizeBiasIndex = priorBiasIndex;
                if (PWT.disableConfirmation == false)
                    EditorUtility.DisplayDialog("Success!", "PolyWorld Terrain has been generated and linked to the current Unity Terrain.", "OK");
                else
                    Debug.Log("PolyWorld Terrain generated and linked to Gameobject: " + Selection.activeGameObject.name);

                if (previousPWT)
                    PrefabUtility.ReplacePrefab(PWTParent, previousPWT);
                else
                    PrefabUtility.CreatePrefab("Assets" + PWT.prefabPath + "/" + PWTParent.gameObject.name + ".prefab", PWTParent);

            }
        }
        #endregion

        GUILayout.EndVertical();
        EditorGUILayout.Space();
        GUILayout.BeginVertical(GUI.skin.box);
        PWT.quadsizeBiasIndex = EditorGUILayout.IntSlider("Quad Size Multiplier:", PWT.quadsizeBiasIndex, 0, 4);
        PWT.chunkIndex = EditorGUILayout.IntSlider("Chunk Size Choice:", PWT.chunkIndex, 0, PWT.chunkSizes.Length - 1);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.BeginHorizontal();
        if (PWT.chunkIndex != PWT.chunkSizes.Length - 1)
            GUILayout.Label("Chunk Size: " + PWT.chunkSizes[PWT.chunkIndex].x + "m X " + PWT.chunkSizes[PWT.chunkIndex].y + "m");
        else
            GUILayout.Label("No chunk pass!");
        float numMeshes = (PWT.TWidth / PWT.chunkSizes[PWT.chunkIndex].x) * (PWT.TLength / PWT.chunkSizes[PWT.chunkIndex].y);
        if (numMeshes > 300)
            GUI.color = new Color(1f, 1f, 0f);
        else
            GUI.color = new Color(1f, 1f, 1f);
        GUILayout.Label("Meshes Generated: " + numMeshes);
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Quad Size: " + PWT.quadWidth + " x " + PWT.quadLength);

        int numQuadsX = (int)(PWT.chunkSizes[PWT.chunkIndex].x / PWT.quadWidth);
        int numQuadsZ = (int)(PWT.chunkSizes[PWT.chunkIndex].y / PWT.quadLength);
        int triCount = (numQuadsX * numQuadsZ) * 2;
        int vertCount = triCount * 3;

        GUI.color = new Color(1f, 1f, 1f);
        GUILayout.Label("Triangles per Chunk: " + triCount);
        if (vertCount > 65536)
            GUI.color = new Color(1f, 0f, 0f);
        else
            GUI.color = new Color(1f, 1f, 1f);
        GUILayout.Label("Vertices per Chunk: " + vertCount);

        GUI.color = new Color(1f, 1f, 1f);
        GUILayout.EndVertical();        
        
        GUILayout.Space(10);
        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Material to Apply:");
        PWT.PWTMat = (Material)EditorGUILayout.ObjectField(PWT.PWTMat, typeof(Material), false);
        EditorGUILayout.EndHorizontal();
        GUI.color = new Color(1f, 1f, 1f);
        PWT.CV = (QT_PolyWorldTerrain.ColorVariety)EditorGUILayout.EnumPopup("Color Variety: ", PWT.CV);
        EditorGUILayout.BeginHorizontal();
        PWT.BaseMapMip = PWT.GetTilingValue(PWT.CV);
        PWT.blurPasses = EditorGUILayout.IntSlider("Blur Amount:", PWT.blurPasses, 0, 8);

        PWT.quadColor = GUILayout.Toggle(PWT.quadColor,"Color by Quad");
        EditorGUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(10);


        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.BeginHorizontal();
        if (PWT.quadsizeBiasIndex == PWT.quadsizeBias.Length - 1)
            GUI.color = new Color(1f, 0f, 0f);
        else
            GUI.color = new Color(1f, 1f, 1f);
        PWT.GenerateMeshColliders = GUILayout.Toggle(PWT.GenerateMeshColliders, "Generate Mesh Colliders");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        PWT.GenerateLODs = GUILayout.Toggle(PWT.GenerateLODs, "Generate LODs");
        //PWT.Average = EditorGUILayout.Toggle("Average Vertices", PWT.Average);
        GUI.color = new Color(1f, 1f, 1f);
        if (PWT.quadsizeBiasIndex == PWT.quadsizeBias.Length - 1)
        {
            PWT.GenerateLODs = false;
            GUILayout.Label("No Valid LODS with Current Settings");
        }
        EditorGUILayout.EndHorizontal();
        if (PWT.GenerateLODs)
        {
            GUILayout.Label("Transition Distances:");
            EditorGUILayout.BeginHorizontal();

            //PWT.LODSizes[0] = Mathf.Clamp01(EditorGUILayout.FloatField("LOD1 %: ", PWT.LODSizes[0]));
           // PWT.LODSizes[1] = Mathf.Clamp(EditorGUILayout.FloatField("Culled %: ", PWT.LODSizes[1]),0,PWT.LODSizes[0]-.01f);
            GUILayout.Label("LOD 1:");
            PWT.LODSizes[0] = EditorGUILayout.Slider(PWT.LODSizes[0], PWT.LODSizes[1], 1);

            GUILayout.Label("Cull:");
            PWT.LODSizes[1] = EditorGUILayout.Slider(PWT.LODSizes[1], 0,PWT.LODSizes[0]);
            EditorGUILayout.EndHorizontal();
            if(Selection.activeGameObject.transform.childCount > 0)
            {
                if(GUILayout.Button("Update LOD Sizes"))                
                    PWT.UpdateLODSizes();                
            }
     
       
        }

        //override this so we don't get multiple terrains in a hierarchy for when we render vertex colors
        PWT.deletePWTerrain =  GUILayout.Toggle(PWT.deletePWTerrain, "Delete Polyworld Terrains on Update");
        PWT.disableConfirmation = GUILayout.Toggle(PWT.disableConfirmation, "Disable Confirmation");
       
        GUILayout.EndVertical();
        if (GUILayout.Button("Help"))
        {
            Application.OpenURL("http://qt-ent.com/PolyWorld/scripts/");
        }


      
        for (int x = 0; x < PWT.terrainData.splatPrototypes.Length; x++)
            SetTextureImporterFormat(true, PWT.terrainData.splatPrototypes[x].texture);
#endif
    }

    private void WriteAllContent(GameObject PWTParent, List<GameObject[]> everyLODSet)
    {
        //write meshes first, associate them, then write the prefab.
        for (int chunkIndex = 0; chunkIndex < everyLODSet[0].Length; chunkIndex++)
        {

        }
    }

    //runs some checks and sets up folders if needed.
    private bool RunChecks()
    {
        float numMeshes = (PWT.TWidth / PWT.chunkSizes[PWT.chunkIndex].x) * (PWT.TLength / PWT.chunkSizes[PWT.chunkIndex].y);
        int numQuadsX = (int)(PWT.chunkSizes[PWT.chunkIndex].x / PWT.quadWidth);
        int numQuadsZ = (int)(PWT.chunkSizes[PWT.chunkIndex].y / PWT.quadLength);
        int triCount = (numQuadsX * numQuadsZ) * 2;
        int vertCount = triCount * 3;
        

        bool passed = false;
        if (PWT.PWTMat == null)
            EditorUtility.DisplayDialog("Material Not Assigned", "Please specify a material in the inspector to apply to the generated PolyWorld Terrain.", "OK");
        else if (PWT.chunkIndex==PWT.chunkSizes.Length-1)
        {
            if (EditorUtility.DisplayDialog("Caution", "Choosing not to chunk the terrain may have consequences for performance on lower-end devices, though it greatly depends on the complexity and physical size of the terrain. Ideally terrains lower than 64x64 may benefit from not chunking.\n\nAre you sure you want to continute?", "Convert", "Cancel"))            
                passed = true;            
        }
        else if (vertCount > 65536)
            EditorUtility.DisplayDialog("Too Many Vertices", "Unity has a limit of 65536 vertices per mesh. The current chunk settings create chunks that are above this vertex count. Please either increase the Quad Size Multiplier or reduce the Chunk Size Choice.", "OK");
        else if (numMeshes > 300)
        {
            if (EditorUtility.DisplayDialog("High Draw Call Count", "With the current Chunk Size Choice, there will be " + numMeshes + " meshes generated.\n\nAre you sure you want to continue?", "Yes", "Cancel"))
                passed = true;
        }
        else
            passed = true;
    
        if(passed)
        {           
            // if the prefabpath is not set, make one. This happens if you've deleted the PW terrain.
            //this will return the full windows path: e:/sourcedata/Unity 5 Projects/PolyWorld/Assets
            if (PWT.prefabPath == null)
            {
                PWT.prefabPath = EditorUtility.SaveFolderPanel("Choose Folder to Save the Prefab and Meshes", "", "");
                AssetDatabase.Refresh();
                if (PWT.prefabPath == "")
                    passed = false;
                else if (!PWT.prefabPath.Contains("Assets"))
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Plase choose a folder within your Assets folder.", "OK");
                    passed = false;
                }
                else
                {
                    Debug.Log(PWT.prefabPath+" is the prefab path.");                 
                    
                    //if the meshpath isn't setup, it means either this is a new mesh or the old one was deleted.
                    if(PWT.meshPath==null)
                    {
                        Debug.Log("Mesh folder does not exist. making..");
                        PWT.prefabPath = PWT.prefabPath.Replace(Application.dataPath, "");
                        Debug.Log(PWT.prefabPath);
                        PWT.meshPath=AssetDatabase.CreateFolder("Assets" + PWT.prefabPath, PWT.gameObject.name + " Mesh Data");
                        PWT.meshPath = AssetDatabase.GUIDToAssetPath(PWT.meshPath); //convert it to a path so we can save the meshes.
                        Debug.Log("Mesh path: " + PWT.meshPath);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.Log(" Mesh folder exists. skipping.");
                    }
                    
                }
            }
        }
        return passed;
    }
    
   
    public static void SetTextureImporterFormat(bool isReadable, Texture2D splatDiffuse)
    {

            
                string assetPath = AssetDatabase.GetAssetPath(splatDiffuse);
                var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (tImporter != null)
                {
                    if(tImporter.isReadable==false)
                    { 
                        tImporter.textureType = TextureImporterType.Default;

                        tImporter.isReadable = isReadable;

                        AssetDatabase.ImportAsset(assetPath);
                        AssetDatabase.Refresh();
                     }
                }


    }

    public void OnEnable()
    {
        WorldIcon = (Texture)Resources.Load("PW_LogoWide");
        UpdateImg = (Texture)Resources.Load("PW_BtnUpdateTerrain");
        UpdateColor = (Texture)Resources.Load("PW_BtnUpdateColor");
        PWT = (QT_PolyWorldTerrain)target;
        PWT.SetupTerrainAssociation(); //initialize all the variables        
    }

}
