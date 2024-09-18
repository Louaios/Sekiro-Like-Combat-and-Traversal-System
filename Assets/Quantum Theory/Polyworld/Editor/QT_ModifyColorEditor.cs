using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;


[CustomEditor(typeof(QT_ModifyColor))]
public class QT_ModifyColorEditor : Editor {

    Texture2D sampleCursor;
    bool hideWireframe = false;
    string status = "";
    public GameObject activeGO, activeRootGO;
    public List<Transform> GOHierarchy;
    public QT_ModifyColor MC;
    private bool isSetup = false;


    //caled when the mouse moves
    void OnSceneGUI()
    {
        if (sampleCursor == null)
        {
            
           string[] guids = AssetDatabase.FindAssets("QT_ModifyColorCursor");
            if (guids.Length > 0)            
                sampleCursor = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(Texture2D));

        }
        else  if (QT_ModifyColorSample.isSampling)
        {
               Cursor.SetCursor(sampleCursor, new Vector2(16, 16), CursorMode.Auto);
               EditorGUIUtility.AddCursorRect(new Rect(0, 0, 2000, 2000), MouseCursor.CustomCursor);
           
        }
       
        Event e = Event.current;

        if (e.type == EventType.MouseDown && QT_ModifyColorSample.isSampling)
        {
            if (e.button == 0)
            {
               //wherever you click, grab the triangleindex of the sourcemesh

                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
	           		RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.gameObject == QT_ModifyColorSample.GMC.gameObject)
                        {
                            //get triangle index from the collider
                            int triIndex = hit.triangleIndex;
                            QT_ModifyColorSample.GMC.AllChannels[QT_ModifyColorSample.channelIndex].sampledColor = QT_ModifyColorSample.GMC.mesh.colors[QT_ModifyColorSample.GMC.mesh.triangles[triIndex * 3]];
                            UpdateTargetVertices(QT_ModifyColorSample.GMC, QT_ModifyColorSample.GMC.AllChannels[QT_ModifyColorSample.channelIndex].sampledColor, QT_ModifyColorSample.channelIndex);
                            QT_ModifyColorSample.isSampling = false;
                            
                        }
                        else
                            Debug.LogWarning("Ray Cast hit "+hit.transform.gameObject.name+". Try again. Hide that GameObject if this becomes a problem.");
                        
                    }
            }

        }
    }


    public override void OnInspectorGUI()
    {
        if (Selection.activeGameObject)
        {
            MC = (QT_ModifyColor)target;
            QT_ModifyColorSample.GMC = MC;

            //do a bunch of checks to make sure it's a mesh/skinned mesh, prefab,and has a collider.
            if (RunInitialSetup(MC))
            {
                if (isSetup == false)
                {
                   
                        //prep the data, put the working temp mesh in the mesh slots so people can see it in the sceneview.
                        if (MC.isMesh)
                        {
                            MC.mesh = MC.sourceMF.sharedMesh;
                            MC.tempMesh = DuplicateMesh(MC.mesh);
                            MC.tempMesh.name = "TEMP ModifyColor Mesh!";
                            SetupInitialColors(MC);
                            MC.sourceMF.sharedMesh = MC.tempMesh;
                            EditorUtility.SetDirty(MC.sourceMF);
                        }
                        else if (MC.isSM)
                        {
                            MC.mesh = MC.sourceSMR.sharedMesh;
                            MC.tempMesh = DuplicateMesh(MC.mesh);
                            MC.tempMesh.name = "TEMP ModifyColor Mesh!";
                            SetupInitialColors(MC);
                            MC.sourceSMR.sharedMesh = MC.tempMesh;
                            EditorUtility.SetDirty(MC.sourceSMR);
                        }
                        isSetup = true;
                }

               
               // if (MC.isPWMesh)
               //     MC.pwMeshOverride = EditorGUILayout.Toggle("New Channel Mapping", MC.pwMeshOverride);

                hideWireframe = EditorGUILayout.Toggle("Hide Wireframe", hideWireframe);
                EditorUtility.SetSelectedWireframeHidden(MC.gameObject.GetComponent<Renderer>(), hideWireframe);
                EditorGUILayout.Space();
                //found a PW mesh
                if (MC.isPWMesh && MC.pwMeshOverride == false)
                    EditorGUILayout.HelpBox("PolyWorld Mesh Detected. Using preexisting channel mapping.", MessageType.Info);

                MC.globalAlpha = EditorGUILayout.Slider("Global Alpha: ", MC.globalAlpha, 0f, 1f);

                

                //setup the UI for each color channel.
                for (int x = 0; x < MC.AllChannels.Length; x++)
                {

                    if (QT_ModifyColorSample.isSampling && QT_ModifyColorSample.channelIndex == x)
                    {
                        GUI.color += new Color(0f, -1f, -1f);
                        status = "Click on a Triangle in Scene View";
                    }
                    else if (MC.AllChannels[x].targetVertices.Count == 0 || MC.AllChannels[x].targetVertices == null)// && MC.isPWMesh == false || MC.AllChannels[x].targetVertices.Count == 0 && MC.pwMeshOverride == true)
                    {
                        GUI.color += new Color(-.5f, 0.5f, -.5f);
                        status = "Channel Clear";
                    }
                    else
                    {
                        GUI.color += new Color(+.5f, 1f, -.5f);
                        status = "Channel in Use";
                    }

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Label("Channel " + (x + 1) + ": " + status);
                    GUI.color = new Color(1f, 1f, 1f);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    MC.tempColors[x] = EditorGUILayout.ColorField("Assign Color: ", MC.tempColors[x]);
                    //if it's not a polyworld mesh, or the override is enabled, let the user sample a color right on the mesh to override.
                    if (!MC.isPWMesh || MC.pwMeshOverride)
                    {
                        if (GUILayout.Button("Sample Color on Mesh"))
                        {
                            if (MC.GetComponent<Collider>().enabled)
                            {
                                //ghetto, I know. Love to know how else to do this.
                                QT_ModifyColorSample.isSampling = !QT_ModifyColorSample.isSampling;
                                QT_ModifyColorSample.channelIndex = x;
                            }
                            else
                                EditorUtility.DisplayDialog("Enable Collider", "Please enable the collider to sample color data from the mesh.", "OK");

                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    MC.tempSmoothness[x] = EditorGUILayout.Slider("Smoothness", MC.tempSmoothness[x],0,1);
                    MC.tempMetallic[x] = EditorGUILayout.Slider("Metallic", MC.tempMetallic[x], 0, 1);

                    MC.AllChannels[x].Color = new float[4] { MC.tempColors[x].r, MC.tempColors[x].g, MC.tempColors[x].b, MC.tempColors[x].a };

                    MC.AllChannels[x].Smoothness = MC.tempSmoothness[x];
                    MC.AllChannels[x].Metallic = MC.tempMetallic[x];
                   

                    //Non Polyworld meshes, or PW meshes with the override, get to sample then tweak the hue/brightness range of the overridden color.
                    if (!MC.isPWMesh || MC.pwMeshOverride)
                    {
                        MC.AllChannels[x].hueRange = EditorGUILayout.Slider("Hue Range", MC.AllChannels[x].hueRange, 0, 1);
                        MC.AllChannels[x].satRange = EditorGUILayout.Slider("Saturation Range", MC.AllChannels[x].satRange, 0, 1);
                        MC.AllChannels[x].valRange = EditorGUILayout.Slider("Brightness Range", MC.AllChannels[x].valRange, 0, 1);  
                        
                        UpdateTargetVertices(MC, MC.AllChannels[x].sampledColor, x);
                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.Space();
                }



                //keep the previous value range. Good for surface variety.
                MC.preserveShading = EditorGUILayout.Toggle("Preserve Shading", MC.preserveShading);
                if (MC.preserveShading)
                {
                    MC.Contrast = EditorGUILayout.Slider("Fade:", MC.Contrast, 0f, 1f);
                    MC.ContrastClamp1 = EditorGUILayout.Slider("Shadows:", MC.ContrastClamp1, 0f, 1f);
                    MC.ContrastClamp2 = EditorGUILayout.Slider("Highlights:", MC.ContrastClamp2, 0f, 1f);
                }
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Load Palette"))
                    LoadPalette(MC);

                if (GUILayout.Button("Save Palette"))
                    SavePalette(MC);

                //apply debug colors to easily see how they are applied.
                if (GUILayout.Button("Use Debug Colors"))
                {
                    MC.tempColors[0] = Color.red;
                    MC.tempColors[1] = Color.green;
                    MC.tempColors[2] = Color.blue;
                    MC.tempColors[3] = Color.yellow;
                    MC.tempColors[4] = Color.magenta;
                    MC.tempColors[5] = Color.white;
                }
                EditorGUILayout.EndHorizontal();
                //clear all data.
                if (GUILayout.Button("Reset Mesh"))
                {

                    QT_ModifyColorSample.isSampling = false;

                    for (int p = 0; p < MC.AllChannels.Length; p++)
                    {
                        //if (!MC.isPWMesh || MC.pwMeshOverride)
                            MC.AllChannels[p].targetVertices.Clear();
                        MC.AllChannels[p].sampledColor = new Color(1, 0, 1, 0.123f);
                        MC.AllChannels[p].hueRange = .5f;
                        MC.AllChannels[p].valRange = .25f;
                        MC.isPWMesh = false;
                    }

                    isSetup = false;    
  

                }

                EditorGUILayout.Space();
                //update the mesh collider on the new game object.
                MC.updateMeshCollider = EditorGUILayout.Toggle("Update Mesh Collider", MC.updateMeshCollider);
                //replace the current prefab with the new gameobject with the new colored mesh.
                MC.Replace = EditorGUILayout.Toggle("Overwrite Prefab", MC.Replace);
                //if we don't replace the prefab, create a new one.
                if (!MC.Replace)
                {
                    MC.newPrefabName = EditorGUILayout.TextField("New Prefab Name:", MC.newPrefabName);
                    if (MC.newPrefabName.Equals(MC.gameObject.transform.root.gameObject.name))
                        EditorGUILayout.HelpBox("New Prefab Name is the same as the current root prefab name. Please providea unique name for the new gameobject.", MessageType.Warning);
                }
                //else
                MC.meshFileName = EditorGUILayout.TextField("Mesh Filename:", MC.meshFileName);


                //if the original value range is empty, setup them up and normalize.
                if (MC.originalValues != null)
                {
                    MC.newValues = new float[MC.tempMesh.colors32.Length];
                    MC.newShading = new float[MC.tempMesh.colors32.Length];

                    for (int x = 0; x < MC.newValues.Length; x++)
                    {
                        MC.newValues[x] = MC.originalValues[x];
                        MC.newShading[x] = MC.originalValues[x];
                    }

                    MC.newValues = MC.RemapFloats(MC.originalValues, MC.Highlights, MC.Shadows);

                    MC.newShading = MC.RemapFloats(MC.originalValues, 0f, 1f);

                    for (int p = 0; p < MC.newShading.Length; p++)
                    {
                        MC.newShading[p] = Mathf.Clamp(MC.newShading[p], MC.ContrastClamp1, MC.ContrastClamp2);
                        MC.newShading[p] += MC.Contrast;
                        MC.newShading[p] = Mathf.Clamp01(MC.newShading[p]);
                    }

                    //copy the originalvcs into the newcolors array to get overwritten by even newer colors.
                    Color[] newColors = new Color[MC.originalVCs.Length];
                    Vector2[] newUV4s = new Vector2[MC.originalVCs.Length];

                    for (int b = 0; b < newColors.Length; b++)                    
                        newColors[b] = MC.originalVCs[b];
                    
                    //in every channel, go through every target vertex and apply the color
                    //we go in reverse order so the colors are layered accordingly.

                    for (int c=MC.AllChannels.Length-1;c>=0;c--)
                    {
                        for (int z = 0; z < MC.AllChannels[c].targetVertices.Count; z++)
                        {
                            int vertIndex = MC.AllChannels[c].targetVertices[z];
                            //lerp between the allchannel color and newcolors via alpha of the allchannels
                            float newR = Mathf.Lerp(newColors[vertIndex].r, MC.AllChannels[c].Color[0], MC.AllChannels[c].Color[3] * MC.globalAlpha);
                            float newG = Mathf.Lerp(newColors[vertIndex].g, MC.AllChannels[c].Color[1], MC.AllChannels[c].Color[3] * MC.globalAlpha);
                            float newB = Mathf.Lerp(newColors[vertIndex].b, MC.AllChannels[c].Color[2], MC.AllChannels[c].Color[3] * MC.globalAlpha);
                            newColors[vertIndex] = new Color(newR, newG, newB);
                            //now to a global fade
                            if (MC.preserveShading)
                                newColors[vertIndex] *= MC.newShading[vertIndex];

                            newUV4s[vertIndex].x = MC.AllChannels[c].Smoothness;
                            newUV4s[vertIndex].y = MC.AllChannels[c].Metallic;
                        }
                    }
                    //assign the new color choices.
                    MC.AssignVCs(newColors);
                    MC.AssignUV4s(newUV4s);
                }

                if (GUILayout.Button("Save Changes to Disk"))
                {
                    //if we input bad stuff
                    if (MC.Replace == false && MC.newPrefabName.Equals(MC.gameObject.transform.root.gameObject.name))
                        EditorUtility.DisplayDialog("Bad Prefab Name", "You have chosen a new prefab name that is the same as the old one. Please give a unique name to the new prefab.", "OK");
                    else
                    {
                        //begin to write all the new data.
                        WriteData();
                        //delete the modifycolor component on the active game object since we don't need it anymore.
                        DestroyImmediate(activeGO.GetComponent<QT_ModifyColor>());
                    }
                }
            }
            if (GUILayout.Button("Help"))
            {
				Application.OpenURL("http://qt-ent.com/PolyWorld/scripts/");
            }
        }
    }

    void CollectGOs(GameObject obj)
    {
        foreach (Transform child in obj.transform) 
        {
            GOHierarchy.Add(child);
            CollectGOs(child.gameObject); 
        }

    }

    //checks to see if it's a mesh and prefab and has a collider.
    private bool RunInitialSetup(QT_ModifyColor MC)
    {
        
        bool pass=true;
        activeGO = Selection.activeGameObject;
        activeRootGO = Selection.activeGameObject.transform.root.gameObject;

        if (PrefabUtility.GetPrefabType(activeRootGO) != PrefabType.PrefabInstance)
        {
            EditorGUILayout.HelpBox(MC.gameObject.name + " is not a Prefab. Please convert this gameobject hierarchy to a prefab before using the Modify Color script.", MessageType.Warning);
            pass = false;
            return pass;
        }
        else
            pass = true;
                 
        MC.sourceMF = activeGO.GetComponent<MeshFilter>();
        if (MC.sourceMF)
            MC.isMesh = true;

        MC.sourceSMR = activeGO.GetComponent<SkinnedMeshRenderer>();
        if (MC.sourceSMR)
            MC.isSM = true;

        if (!MC.isMesh && !MC.isSM) 
        {
            EditorGUILayout.HelpBox(MC.gameObject.name + " does not contain a mesh. Modify Color requires a gameobject with a Meshfilter or SkinnedMeshRenderer component and a mesh.", MessageType.Warning);
            pass = false;
            return pass;
        }
        else
            pass = true;

        if (!MC.GetComponent<MeshCollider>())
        {
            EditorGUILayout.HelpBox("This GameObject temporarily requires a mesh collider to accurately specify color changes.", MessageType.Warning);
            pass = false;
            return pass;
        }
        else
        {
            MC.hasCollider = true;
            pass = true;
        }
        
        if (MC.GetComponent<MeshCollider>().sharedMesh == null)
        {
            EditorGUILayout.HelpBox("The MeshCollider contains no reference to a mesh. Provide one to continue.", MessageType.Warning);
            pass = false;
            return pass;
        }
        else
            pass = true;

        /*
        if (pass)
        {
            GOHierarchy = new List<Transform>();
            //disable the other children because it messes with the modifycolor script.
            CollectGOs(activeRootGO);
            foreach (Transform child in GOHierarchy)
            {
                if (!child.gameObject.name.Equals(activeGO.name) || child.gameObject.activeInHierarchy == true)
                {
                
                    if (!child.GetComponent<MeshFilter>() || !child.GetComponent<SkinnedMeshRenderer>())                    
                        GOHierarchy.Remove(child);                    
                    else
                        child.gameObject.SetActive(false);
                }
                
            }

            

        }*/
        return pass;

    }
    
    //always reverts the mf or smr to prefab state when deselected. This is on purpose to make sure people find that save button.
    public void OnDisable()
    {
        if (activeGO)
        {
            if (activeGO.GetComponent<QT_ModifyColor>())            
                Debug.LogWarning(activeGO.name + " still has a ModifyColor Component enabled. Either write the changes to disk or remove the component.");
            
                QT_ModifyColorSample.isSampling = false;
                QT_ModifyColorSample.GMC = null;
                QT_ModifyColorSample.channelIndex = -1;
                MeshFilter mf = activeGO.GetComponent<MeshFilter>();
                SkinnedMeshRenderer smr = activeGO.GetComponent<SkinnedMeshRenderer>();
                if (mf)
                    PrefabUtility.ResetToPrefabState(mf);
                else if (smr)
                    PrefabUtility.ResetToPrefabState(smr);
               // foreach (Transform child in GOHierarchy)
                 //   child.gameObject.SetActive(false);
                
        }
       
    }

    public void WriteData()
    {
        //get the prefab path, use that as the default path when the save window opens
        string prefabpathFull = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(activeGO));
        string prefabpath = prefabpathFull.Replace(activeGO.name + ".prefab", "");
        string fullPath = EditorUtility.SaveFolderPanel("Choose Location to Write Meshes and Game Objects", prefabpath, "");

        if (fullPath.Length > 0 && fullPath.Contains("Assets"))
        {

            //patch to write previous alpha data for polyworld meshes..gotta find a better place for this.
            Color[] tempalpha = new Color[MC.tempMesh.vertexCount];
            tempalpha = MC.tempMesh.colors;
            for (int c = 0; c < MC.tempMesh.vertexCount; c++)            
                tempalpha[c].a = MC.originalVCs[c].a;
            
            MC.tempMesh.colors = tempalpha;
            MC.isSaving = true;
            MC.tempMesh.name = MC.meshFileName;

            string assetPath = fullPath.Replace(Application.dataPath, "Assets");


            Debug.Log("New Mesh Written To:\n" + fullPath);
            //write mesh
            MC.isSaving = true;
            MeshFilter targetMF = new MeshFilter();
            SkinnedMeshRenderer targetSMR = new SkinnedMeshRenderer();
            MeshCollider targetMC = new MeshCollider();
           
            //instance a copy of the parent prefab that has no modifycolor script and contains original mesh.
            GameObject WorkingGO = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)PrefabUtility.GetPrefabParent(activeRootGO));

            //find the meshfilter/skinnedmeshrenderer from the prefab parent that we're replacing.
            if (WorkingGO.transform.childCount >= 1)
            {                
                Transform[] t = WorkingGO.GetComponentsInChildren<Transform>();
                foreach (Transform trans in t)
                {
                    if (MC.isMesh)
                    {
                        MeshFilter mf = (MeshFilter)trans.GetComponent<MeshFilter>();
              
                        if (mf != null)
                        {
                            if (mf.sharedMesh.name.Equals(MC.mesh.name))
                            {
                                
                                targetMF = mf;
                                targetMC = (MeshCollider)trans.GetComponent<MeshCollider>();
                            
                            }

                        }
                    }
                    if (MC.isSM)
                    {
                        SkinnedMeshRenderer smr = (SkinnedMeshRenderer)trans.GetComponent<SkinnedMeshRenderer>();
                        if (smr != null)
                        {
                            if (smr.sharedMesh.name.Equals(MC.mesh.name))
                            {
                                targetSMR = smr;
                                targetMC = (MeshCollider)trans.GetComponent<MeshCollider>();
                            }
                        }
                    }

                }

            }
            //otherwise, it has no hierarchy.
            else
            {
                if (MC.isMesh)
                    targetMF = WorkingGO.GetComponent<MeshFilter>();
                else if (MC.isSM)
                    targetSMR = WorkingGO.GetComponent<SkinnedMeshRenderer>();
                targetMC = (MeshCollider)WorkingGO.GetComponent<MeshCollider>();
            }



            //if we're replacing the prefab, write all the data and put the mesh where it belongs, then replace the prefab.
            if (MC.Replace)
            {
                AssetDatabase.CreateAsset(MC.tempMesh, assetPath + "/" + MC.meshFileName + ".asset");
                AssetDatabase.Refresh();
                Mesh newMesh = (Mesh)AssetDatabase.LoadAssetAtPath(assetPath + "/" + MC.meshFileName + ".asset", typeof(Mesh));


                if (MC.isMesh)
                    targetMF.sharedMesh = newMesh;
                else if (MC.isSM)
                    targetSMR.sharedMesh = newMesh;
                if (targetMC && MC.updateMeshCollider)
                    targetMC.sharedMesh = newMesh;
                PrefabUtility.ReplacePrefab(WorkingGO, (GameObject)PrefabUtility.GetPrefabParent(activeRootGO), ReplacePrefabOptions.ReplaceNameBased);//(GameObject)PrefabUtility.GetPrefabParent((GameObject)this.transform.root.gameObject), ReplacePrefabOptions.ConnectToPrefab);                
            }
                //otherwise, write a new prefab and plug in the new data.
            else
            {
                // tempMesh.name = newPrefabName;
                WorkingGO.name = MC.newPrefabName;
                AssetDatabase.CreateAsset(MC.tempMesh, assetPath + "/" + MC.meshFileName + ".asset");
                AssetDatabase.Refresh();
                Mesh newMesh = (Mesh)AssetDatabase.LoadAssetAtPath(assetPath + "/" + MC.meshFileName + ".asset", typeof(Mesh));
              
                if (MC.isMesh)
                    targetMF.sharedMesh = newMesh;
                else if (MC.isSM)
                    targetSMR.sharedMesh = newMesh;
                if (targetMC && MC.updateMeshCollider)
                    targetMC.sharedMesh = newMesh;
                PrefabUtility.CreatePrefab(assetPath + "/" + MC.newPrefabName + ".prefab", WorkingGO);
                Debug.Log("New Prefab Written To:\n" + fullPath);
            }
            AssetDatabase.Refresh();
            MC.isSaving = false;
            DestroyImmediate(WorkingGO);
           
            
        }
        else
            EditorUtility.DisplayDialog("Bad Folder", "Please choose a folder within the Assets folder.", "OK");

    }    


    //can't do blendshapes.
    private Mesh DuplicateMesh(Mesh targetmesh)
    {
        Mesh tempMesh = new Mesh();
        tempMesh.name = targetmesh.name;

        tempMesh.vertices = targetmesh.vertices;
        tempMesh.triangles = targetmesh.triangles;
        tempMesh.subMeshCount = targetmesh.subMeshCount;

        for (int x = 0; x < targetmesh.subMeshCount; x++)
        {
            int[] sourceTris = targetmesh.GetTriangles(x);
            tempMesh.SetTriangles(sourceTris, x);
        }
        tempMesh.tangents = targetmesh.tangents;
        tempMesh.normals = targetmesh.normals;
        tempMesh.colors = targetmesh.colors;
        tempMesh.colors32 = targetmesh.colors32;
        tempMesh.uv = targetmesh.uv;
        tempMesh.uv2 = targetmesh.uv2;
        tempMesh.uv4 = targetmesh.uv4;
        if (MC.isSM)
        {
            tempMesh.bindposes = targetmesh.bindposes;
            tempMesh.boneWeights = targetmesh.boneWeights;
        }
        tempMesh.RecalculateBounds();
        tempMesh.RecalculateNormals();

        return tempMesh;
    }

    void SetupInitialColors(QT_ModifyColor MC) //pulls original values from the original mesh
    {

        if (MC.originalValues == null)
        {
            //ifwe dont have orignals, populate THEM
            float h, s, v;
            MC.originalValues = new float[MC.mesh.colors32.Length];
            MC.originalVCs = new Color[MC.mesh.colors.Length];
            MC.originalVC_HSVs = new Vector3[MC.mesh.colors.Length];
           // MC.uv4s = new Vector2[MC.mesh.vertexCount]; //setup the PBR array based on vertex count of mesh

            for (int c = 0; c < MC.originalValues.Length; c++)
            {
                MC.originalVCs[c] = MC.mesh.colors[c];
              //  MC.uv4s[c] = new Vector2(0, 0); //setup initial values to be plastic and rough.

                Color vc = MC.mesh.colors[c];
                //Plug each vertex alpha that is found into their respective channel.
                float VA = MC.originalVCs[c].a * 255;
                //only pw meshes are masked by alpha values.
                if (!MC.pwMeshOverride)
                {
                    if (VA == 3)//3=1, 5=2, 8=3, 10=4. 13=5, 15=6 There will be at least a VA of 1.0 in max applied to a PW mesh.
                    {
                        MC.AllChannels[0].targetVertices.Add(c);
                        MC.isPWMesh = true;
                    }
                    if (VA == 5)
                    {
                        MC.AllChannels[1].targetVertices.Add(c);
                        MC.isPWMesh = true;
                    }
                    if (VA == 8)
                    {
                        MC.AllChannels[2].targetVertices.Add(c);
                        MC.isPWMesh = true;
                    }
                    if (VA == 10)
                    {
                        MC.AllChannels[3].targetVertices.Add(c);
                        MC.isPWMesh = true;
                    }
                    if (VA == 13)
                    {
                        MC.AllChannels[4].targetVertices.Add(c);
                        MC.isPWMesh = true;
                    }
                    if (VA == 15)
                    {
                        MC.AllChannels[5].targetVertices.Add(c);
                        MC.isPWMesh = true;
                    }
                }
                Color.RGBToHSV(vc, out h, out s, out v);
                MC.originalValues[c] = v;
                MC.originalVC_HSVs[c] = new Vector3(h, s, v);
            }

        }

    }

    public void SavePalette(QT_ModifyColor MC)
    {

        string savedFile = EditorUtility.SaveFilePanelInProject("Save Palette", "Palette", "txt", "Save Palette File to Disk");

        if (savedFile.Length > 0 && savedFile.Contains("Assets"))
        {
            StreamWriter writer = File.CreateText(savedFile);

            writer.WriteLine("v1"); //6 lines of color

            for (int x = 0; x < MC.AllChannels.Length; x++)
            {
                writer.WriteLine(MC.AllChannels[x].Color[0] + "," + MC.AllChannels[x].Color[1] + "," + MC.AllChannels[x].Color[2] + "," + MC.AllChannels[x].Color[3] + "," + MC.AllChannels[x].hueRange + "," + MC.AllChannels[x].valRange);
                
            }
            writer.WriteLine("*"); //split between color data and shading data.
            writer.WriteLine(MC.preserveShading+","+MC.Contrast+","+MC.ContrastClamp1+","+MC.ContrastClamp2+","+ MC.pwMeshOverride);
            
            AssetDatabase.Refresh();
            writer.Close();


        }
        else
            EditorUtility.DisplayDialog("Invalid Folder", "Invalid Folder.\n\nPlease specify a folder within your Project.", "OK");

    }

    private void LoadPalette(QT_ModifyColor MC)
    {
        string loadedFile = EditorUtility.OpenFilePanel("Load Palette", Application.dataPath, "txt");
        if (loadedFile.Length > 0)
        {
            //FileStream stream = new FileStream(loadedFile, FileMode.Open);
            StreamReader reader = File.OpenText(loadedFile);//new StreamReader(loadedFile);
            string readme;
            
            int lineCounter = 0;
            while ((readme = reader.ReadLine()) != null)
            {


                string[] line = readme.Split(',');
                
                if (lineCounter >= 1 && lineCounter <= 6)
                {
                    
                    MC.AllChannels[lineCounter-1].Color[0] = float.Parse(line[0]);
                    MC.AllChannels[lineCounter-1].Color[1] = float.Parse(line[1]);
                    MC.AllChannels[lineCounter-1].Color[2] = float.Parse(line[2]);
                    MC.AllChannels[lineCounter-1].Color[3] = float.Parse(line[3]);
                    MC.AllChannels[lineCounter-1].hueRange = float.Parse(line[4]);
                    MC.AllChannels[lineCounter-1].valRange = float.Parse(line[5]);
                }

                if (lineCounter ==8)
                {
                    MC.preserveShading = bool.Parse(line[0]);
                    MC.Contrast = float.Parse(line[1]);
                    MC.ContrastClamp1 = float.Parse(line[2]);
                    MC.ContrastClamp2 = float.Parse(line[3]);
                    MC.pwMeshOverride = bool.Parse(line[4]);
                    break;
                }

                lineCounter++;
            }

            reader.Close();

            for (int x = 0; x < MC.tempColors.Length; x++)
            {
                MC.tempColors[x].r = MC.AllChannels[x].Color[0];
                MC.tempColors[x].g = MC.AllChannels[x].Color[1];
                MC.tempColors[x].b = MC.AllChannels[x].Color[2];
                MC.tempColors[x].a = MC.AllChannels[x].Color[3];                
            }
        }
    }

   
    //updates the list of vertices targeted by the hue and value range.
    public void UpdateTargetVertices(QT_ModifyColor MC, Color targetColor, int channelIndex)
    {
        if (MC.AllChannels[channelIndex].sampledColor == new Color(1, 0, 1, 0.123f)) //our do-nothing color.
        {            
            return;
        }
        else
        {
            MC.AllChannels[channelIndex].targetVertices.Clear();

            float tcHue, tcSat, tcVal;
            Color.RGBToHSV(targetColor, out tcHue, out tcSat, out tcVal);

            float baseHue = (MC.AllChannels[channelIndex].hueRange);// / 2;
            float baseVal = (MC.AllChannels[channelIndex].valRange);// / 2;
            float baseSat = MC.AllChannels[channelIndex].satRange;

            float hueMin = Mathf.Clamp01(tcHue - baseHue);
            float hueMax = Mathf.Clamp01(tcHue + baseHue);
            float valMin = Mathf.Clamp01(tcVal - baseVal);
            float valMax = Mathf.Clamp01(tcVal + baseVal);
            float satMin = Mathf.Clamp01(tcSat - baseSat);
            float satMax = Mathf.Clamp01(tcSat + baseSat);

            for (int c = 0; c < MC.originalVCs.Length; c++)
            {             
                //x =hue, y=sat, z=val
                if (MC.originalVC_HSVs[c].y >= satMin && MC.originalVC_HSVs[c].y <= satMax && MC.originalVC_HSVs[c].z >= valMin && MC.originalVC_HSVs[c].z <= valMax && MC.originalVC_HSVs[c].x >= hueMin && MC.originalVC_HSVs[c].x <= hueMax)
                    MC.AllChannels[channelIndex].targetVertices.Add(c);

            }

            /*
            for (int x = 0; x < MC.AllChannels.Length; x++)
            {
                for (int v = x + 1; v < MC.AllChannels.Length; v++)
                {
                    int counter = 0;
                    while (counter < MC.AllChannels[x].targetVertices.Count) //5 is length of X. 10 is length of v
                    {
                        if (MC.AllChannels[v].targetVertices.Contains(MC.AllChannels[x].targetVertices[counter]))
                        {
                            int targetIndex = MC.AllChannels[v].targetVertices.IndexOf(MC.AllChannels[x].targetVertices[counter]);
                            MC.AllChannels[v].targetVertices.RemoveAt(targetIndex);
                        }
                        counter++;
                    }

                }
            }
             * */


        }
    }

    

}

public static class QT_ModifyColorSample
{
    public static bool isSampling = false;
    public static int channelIndex = -1;   
    public static QT_ModifyColor GMC;
    
}
