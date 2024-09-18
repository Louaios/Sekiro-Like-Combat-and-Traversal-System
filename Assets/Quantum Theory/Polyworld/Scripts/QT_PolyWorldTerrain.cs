using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class QT_PolyWorldTerrain : MonoBehaviour
{
    #if UNITY_EDITOR

    public string prefabPath; //path to the prefab child
    public string meshPath; //path to the meshes

    [HideInInspector]
    public string PWTName = "None!";
    [HideInInspector]
    public bool terrainGOEnabled = true;
    [HideInInspector]
    public bool PWTGOEnabled = true;
    [HideInInspector]
    public bool quadColor = false;
    [HideInInspector]
    public Material PWTMat;
    [HideInInspector]
    public Vector2[] chunkSizes;
    [HideInInspector]
    public int chunkIndex = 2; //holds the index to which chunksize we want.
    [HideInInspector]
    public float TWidth, TLength, THeight, HFR, CTR; //terrain width, length, and heightmap resolution. control texture resolution
    [HideInInspector]
    public TerrainData terrainData;
   
   // private GameObject PWTerrainGO;    
    public enum ColorVariety
    {
        Normal = 0,
        Less = 1,
        None = 2,
        Most = 3
    }
    public ColorVariety CV; 
    [HideInInspector]
    public float quadWidth, quadLength;
    [HideInInspector]
    public int[] quadsizeBias = new int[] {1,2,4,8,16};
    [HideInInspector]
    public int blurPasses = 1;
    [HideInInspector]
    public int[] BaseMapMip = new int[2]; //holds the mip values for the ColorVariety settings 
    [HideInInspector]
    public bool disableConfirmation = false;
    [HideInInspector]
    public bool deletePWTerrain = true;

    [HideInInspector]
    public int quadsizeBiasIndex = 0;
    [HideInInspector]
    public bool GenerateLODs = true;
    [HideInInspector]
    //values for chunks. Only two as we can't blend more than 2 lods. Last value in the array is teh culling value.
    public float[] LODSizes = new float[] { 0.7f, 0.1f};//.4f, .02f }; //.015f, .03f, .06f, .08f, .15f };
    [HideInInspector]
    public bool Average = true;
    [HideInInspector]
    public bool GenerateMeshColliders = false;

    private Texture2D GenerateBaseMap(int matAmount, Texture2D[] splatDiffuses, float[, ,] splatData)
    {
        int cres = (int)CTR;
        List<float> splatAmount = new List<float>();
        List<Color> sampledColors = new List<Color>();

        //generate the basemap based off the control tex res, even though the heightfield res is more accurate..
        Texture2D basemap = new Texture2D(cres, cres, TextureFormat.ARGB32, true);

        for (int y = 0; y < cres; y++)
        {
            for (int x = 0; x < cres; x++)
            {
                splatAmount.Clear();
                sampledColors.Clear();

                for (int s = 0; s < matAmount; s++)
                {
                    if (splatData[y, x, s] > 0f)
                    {
                        splatAmount.Add(splatData[y, x, s]);

                        float uvx = x / CTR;
                        float uvy = y / CTR;
                        uvx = uvx * splatDiffuses[s].width;
                        uvy = uvy * splatDiffuses[s].height;
                        //now it's 1 to 1 mapped to the terrain. 
                        Vector2 tile = terrainData.splatPrototypes[s].tileSize;
                        float tX = (tile.x / TWidth);
                        float ty = (tile.y / TLength);
                        sampledColors.Add(splatDiffuses[s].GetPixel((int)(uvx / tX), (int)(uvy / ty)));
                    }
                }

                if (sampledColors.Count > 1)
                {
                    Color pixel = sampledColors[0];
                    for (int p = 1; p < sampledColors.Count; p++)
                        pixel = Color.Lerp(pixel, sampledColors[p], splatAmount[p]);
                    basemap.SetPixel(x, y, pixel);
                }
                else
                    basemap.SetPixel(x, y, sampledColors[0]);
            }
        }
        
        basemap.filterMode = FilterMode.Trilinear;
        basemap.wrapMode = TextureWrapMode.Clamp;
        basemap.Apply();

        Color[] mips = basemap.GetPixels(0, 0, basemap.width / BaseMapMip[0], basemap.height / BaseMapMip[0], BaseMapMip[1]);
        Texture2D mip = new Texture2D(basemap.width / BaseMapMip[0], basemap.height / BaseMapMip[0]);
        mip.wrapMode = TextureWrapMode.Clamp;
        mip.filterMode = FilterMode.Trilinear;
        mip.SetPixels(mips);
        

        for (int m = 0; m < blurPasses; m++)
            mip = BlurBaseMap(mip);
       // byte[] bytes = mip.EncodeToPNG();
       // File.WriteAllBytes(Application.dataPath + "/basemap.png", bytes);
        //AssetDatabase.Refresh();
        return mip;
    }

    private Texture2D BlurBaseMap(Texture2D basemap)
    {
        for (int row = 0; row < basemap.height; row++)
        {
            //get current rows of pixels
            Color[] currentRow = basemap.GetPixels(0, row, basemap.width, 1);

            // get row above
            Color[] aboveRow;
            if (row == basemap.height - 1) //if you're at the top row, just use the current row so we're not wrapping
                aboveRow = basemap.GetPixels(0, row, basemap.width, 1);//0, basemap.width, 1);
            else
                aboveRow = basemap.GetPixels(0, row + 1, basemap.width, 1);

            //get row below
            Color[] belowRow;
            if (row == 0) //if you're on the bottom row, get the row at hte top to wrap.
                belowRow = basemap.GetPixels(0, 0, basemap.width, 1);//basemap.height - 1, basemap.width, 1);
            else
                belowRow = basemap.GetPixels(0, row - 1, basemap.width, 1);


            for (int x = 0; x < currentRow.Length; x++)
            {
                //take current pixel, store if off.
                Color currentPixel = currentRow[x];
                //get pixel to the right, left, up and down.

                Color rightPixel;
                if (x == currentRow.Length - 1)
                    rightPixel = currentRow[x];//currentRow[0]; //is this where we're wrapping?
                else
                    rightPixel = currentRow[x + 1];
                Color leftPixel;
                if (x == 0)
                    leftPixel = currentRow[x];//currentRow[currentRow.Length - 1];
                else
                    leftPixel = currentRow[x - 1];

                Color abovePixel = aboveRow[x];
                Color belowPixel = belowRow[x];

                //average em all.
                currentPixel = Color.Lerp(currentPixel, rightPixel, 0.5f);
                currentPixel = Color.Lerp(currentPixel, leftPixel, 0.5f);
                currentPixel = Color.Lerp(currentPixel, abovePixel, 0.5f);
                currentPixel = Color.Lerp(currentPixel, belowPixel, 0.5f);
                currentRow[x] = currentPixel;
            }
            basemap.SetPixels(0, row, basemap.width, 1, currentRow);
        }

        return basemap;
    }
    
    private Color32 GetVertexColor(Texture2D basemap, int x, int y)
    {
        float clampedX = (float)x / HFR;
        float clampedY = (float)y / HFR;
        basemap.mipMapBias = 5;
        clampedX = clampedX * (basemap.width);
        clampedY = clampedY * (basemap.height);

        Color c = basemap.GetPixel((int)clampedX, (int)clampedY);


        byte r = (byte)(c.r * 255f);
        byte g = (byte)(c.g * 255f);
        byte b = (byte)(c.b * 255f);
        byte a = (byte)(c.a * 255f);
        return new Color32(r, g, b, a);

    }

    public void UpdateLODSizes()
    {
        if(this.gameObject.transform.childCount>0)
        {
           
            LODGroup[] LODGs = this.gameObject.transform.GetChild(0).GetComponentsInChildren<LODGroup>();            
            foreach(LODGroup lodg in LODGs)
            {                
                LOD[] lods = lodg.GetLODs();                
                lods[0].screenRelativeTransitionHeight = LODSizes[0];
                lods[1].screenRelativeTransitionHeight = LODSizes[1];               
                lodg.SetLODs(lods);
                lodg.RecalculateBounds();                
            }

           
        }
    }

    //the user sets custom mip level for the basemap when samping vertex colors. 
    public int[] GetTilingValue(ColorVariety cv)
    {
        //value[0] is the divisor for the basemap resolution.
        //value[1] is the mip level. 
        //both have to correspond.
        //2 4 8 16 32 64 128 256
        //1 2 3 4  5  6  7   8
        int[] values = new int[2];
        if (CV == ColorVariety.Normal) //normal is each chunk has normalized uvs.
        {
            values[0] = 2;
            values[1] = 1;
        }
        else if (CV == ColorVariety.None)
        {
            values[0] = 64;
            values[1] = 6;
        }
        else if (CV == ColorVariety.Less)
        {
            values[0] = 8;
            values[1] = 3;

        }
        else
        {
            values[0] = 1;
            values[1] = 0;
        }
        return values;
    }

    public GameObject[] GeneratePolyWorldTerrain(GameObject PWTParent,int currentLOD)
    {
        // SetupTerrainAssociation();
        

        //spawn a save menu foQDer where you'll dump all the meshes
        //create a mesh of quads that will serve as our distribution mesh.
        Vector2 ChunkSize = chunkSizes[chunkIndex];
       
        //get heights
        float[,] HMData = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);        //this is indexed Y,X.  If you want the lower right vertex, the coordinate is 0,31.
        //float[, ,] splatData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight); //get all the splatmap data


        //GameObject PWTParent = new GameObject();
       // PWTParent.name = this.gameObject.name +"-Faceted";
       // PWTParent.transform.position = this.gameObject.transform.position;
        //PWTParent.transform.parent = this.gameObject.transform;

        // determine quad width based on heightfield res and the detail level we chose in the inspector.
        quadWidth = (TWidth / HFR) * quadsizeBias[quadsizeBiasIndex];
        quadLength = (TLength / HFR) * quadsizeBias[quadsizeBiasIndex];
        //Debug.Log("quadWidth: " + quadWidth + "  quadLength: " + quadLength);
        //find out how many quads there should be in our distribution mesh based off the chunksize we chose.
        int numQuadsX = (int)(ChunkSize[0] / quadWidth);
        int numQuadsZ = (int)(ChunkSize[1] / quadLength);
       // Debug.Log("NumquadsX: " + numQuadsX + "   numquadsZ: " + numQuadsZ);
        int numObjsX = (int)(TWidth / ChunkSize[0]);//(int)HFR / numQuadsX;
        int numObjsZ = (int)(TLength / ChunkSize[1]);//(int)HFR / numQuadsZ;
       // Debug.Log("NumObjsX: " + numObjsX + "   numobjsz: " + numObjsZ);
        
        GameObject[] AllChunks = new GameObject[numObjsX * numObjsZ]; //declare the array
        
        int numTris = (numQuadsX * numQuadsZ) * 2;
        int numVerts = numTris * 3;

        //Debug.Log("numTris: " + numTris + "   numverts: " + numVerts);

        //find increments to offset each new gameobject in local space...
        float ObjXOffset = ChunkSize[0];//quadWidth * numQuadsX;
        float ObjZOffset = ChunkSize[1];// numQuadsZ;

        //int matAmount = terrainData.splatPrototypes.Length;

        //Texture2D[] splatDiffuses = new Texture2D[matAmount]; //setup array to hold all the diffuses        
        ////plug in all the diffuse textures we're using on the terrain.
        //for (int s = 0; s < matAmount; s++)
        //    splatDiffuses[s] = terrainData.splatPrototypes[s].texture;

        ////create the basemap for the terrain so our vertices can sample from it.
      
        //Texture2D basemap = GenerateBaseMap(matAmount, splatDiffuses, splatData);

        // byte[] bytes = basemap.EncodeToPNG();
        // File.WriteAllBytes(Application.dataPath + "/basemap.png", bytes);
        //AssetDatabase.Refresh();   

        //DEBUG   

        List<int> BorderVertsIndex = new List<int>(); //holds the vertex indices to all border verts so we can use them as neighbors.
        List<Vector3> BorderVertsPos = new List<Vector3>(); //holds the positions of them.
       // List<int> AvgVertsIndex = new List<int>(); //holds what verts need to be averaged.. the ones that are culled from one lod to the next.
       // List<Vector3> AvgVertsPos = new List<Vector3>(); //positions of em

        int currentGO = 0; //tracks which gameobject index we need to fill.

        float progressBarIncrement = 0;
      
        //loop this mesh creation, instantiation, and GO positioning for as many meshes as we need.
        for (int meshNumZ = 0; meshNumZ < numObjsZ; meshNumZ++)
        {
            for (int meshNumX = 0; meshNumX < numObjsX; meshNumX++)
            {
                EditorUtility.DisplayProgressBar("Converting to PolyWorld Terrain", "Generating Mesh x" + meshNumX + "y" + meshNumZ, progressBarIncrement);
                progressBarIncrement += 1.0f / AllChunks.Length; 

                BorderVertsIndex.Clear();
                BorderVertsPos.Clear();

                GameObject g = new GameObject();                
                MeshFilter mf = g.AddComponent<MeshFilter>();
                MeshRenderer mr = g.AddComponent<MeshRenderer>();
                
                g.name = this.gameObject.name + "_x" + meshNumX + "y" + meshNumZ;
                Mesh dMesh = new Mesh();

                if (GenerateLODs)                
                    g.name+="LOD"+currentLOD;  
               
                    
                dMesh.name = g.name;
                g.transform.parent = PWTParent.transform;
                //declare and build the distribution mesh. Decalre the indices
                int[] dTris = new int[numVerts];
                Vector2[] dUVs = new Vector2[numVerts];
                Vector3[] dVerts = new Vector3[numVerts];
                //Color32[] dVCs = new Color32[numVerts];
                float XOffset, ZOffset;
                int currentVertIndex = 0;
                float uvX, uvY;

                #region Construct quads
                //do column meshes
                for (int z = 0; z < numQuadsZ; z++)
                {
                    ZOffset = (z + 1) * quadLength;
                    int ZHeightIndex = (z + (numQuadsZ * meshNumZ)) * quadsizeBias[quadsizeBiasIndex];

                    //z=3 so you're on the 3rd quad up. 3 + (10 * 2) * 1 = 23

                    //make quads along the x axis and apply all the needed data..
                    for (int x = 0; x < numQuadsX; x++)
                    {
                        int XHeightIndex = (x + (numQuadsX * meshNumX))*quadsizeBias[quadsizeBiasIndex];
                        XOffset = (x + 1) * quadWidth;

                        //we'll construct the mesh relative to the source position of the main terrain, but grab heights of the appropriate chunk.
                        //we'll always offset the mesh in local space to compensate

                        #region get vertex colors -moved to its own function
                        ////sample all the vertex colors through all the splat maps, average, and store them off
                        //Color32 vcLL = GetVertexColor(basemap, XHeightIndex, ZHeightIndex);

                        //Color32 vcUL = GetVertexColor(basemap, XHeightIndex, 1 + ZHeightIndex);
                        //Color32 vcUR = GetVertexColor(basemap, 1 + XHeightIndex, 1 + ZHeightIndex);
                        //Color32 vcLR = GetVertexColor(basemap, 1 + XHeightIndex, ZHeightIndex);


                        ////if we chose to average all the colors by quad..
                        //if (quadColor)
                        //{
                        //    Color32 c1 = Color32.Lerp(vcLL, vcUL, 0.5f);
                        //    Color32 c2 = Color32.Lerp(c1, vcUR, 0.5f);
                        //    Color32 c3 = Color32.Lerp(c2, vcLR, 0.5f);
                        //    dVCs[currentVertIndex] = c3;
                        //    dVCs[currentVertIndex + 1] = c3;
                        //    dVCs[currentVertIndex + 2] = c3;
                        //    dVCs[currentVertIndex + 3] = c3;
                        //    dVCs[currentVertIndex + 4] = c3;
                        //    dVCs[currentVertIndex + 5] = c3;
                        //}
                        //else
                        //{
                        //    Color32 TriL = Color32.Lerp(vcLL, vcUL, 0.5f);
                        //    TriL = Color32.Lerp(TriL, vcUR, 0.5f);
                        //    dVCs[currentVertIndex] = TriL;
                        //    dVCs[currentVertIndex + 1] = TriL;
                        //    dVCs[currentVertIndex + 2] = TriL;

                        //    Color32 TriR = Color32.Lerp(vcUL, vcLR, 0.5f);
                        //    TriR = Color32.Lerp(TriR, vcLL, 0.5f);
                        //    dVCs[currentVertIndex + 3] = TriR;
                        //    dVCs[currentVertIndex + 4] = TriR;
                        //    dVCs[currentVertIndex + 5] = TriR;
                        //}
                        #endregion
                        
                        //LL 
                        dVerts[currentVertIndex] = new Vector3(XOffset - quadWidth, 0, ZOffset - quadLength);
                        dVerts[currentVertIndex].y = HMData[ZHeightIndex, XHeightIndex] * THeight;
                        uvX = (float)x / (float)numQuadsX;
                        uvY = (float)z / (float)numQuadsZ;
                                               
                        //the uvs are mapped 0 to 1 for every chunk. +Meshnum is the offset. *numobj is the tiling.
                        //we have to do one tiling value despite the splats having variable tiling values. 
                        //it's not possible to match since we would have to blend X number of uvs.
                        //dUVs[currentVertIndex] = new Vector2((uvX + meshNumX), (uvY + meshNumZ));

                        dUVs[currentVertIndex] = new Vector2(uvX, uvY);

                            if (z == 0 || x == 0)// && z == numQuadsZ - 1)
                            {
                                BorderVertsIndex.Add(currentVertIndex);
                                BorderVertsPos.Add(dVerts[currentVertIndex]);                                
                            }

                    

                        //UL
                        dVerts[currentVertIndex + 1] = new Vector3(XOffset - quadWidth, 0, ZOffset);
                        dVerts[currentVertIndex + 1].y = HMData[quadsizeBias[quadsizeBiasIndex] + ZHeightIndex, XHeightIndex] * THeight;
                        uvY = ((float)z + 1) / (float)numQuadsZ;
                        dUVs[currentVertIndex + 1] = new Vector2(uvX, uvY);//new Vector2((uvX + meshNumX), (uvY + meshNumZ));
                      
                            if (x == 0|| z == numQuadsZ - 1)
                            {
                                BorderVertsIndex.Add(currentVertIndex + 1);
                                BorderVertsPos.Add(dVerts[currentVertIndex+1]);
                            }

                    
                        //UR
                        dVerts[currentVertIndex + 2] = new Vector3(XOffset, 0, ZOffset);
                        dVerts[currentVertIndex + 2].y = HMData[quadsizeBias[quadsizeBiasIndex] + ZHeightIndex, quadsizeBias[quadsizeBiasIndex] + XHeightIndex] * THeight;
                        uvX = ((float)x + 1) / (float)numQuadsX;
                        dUVs[currentVertIndex + 2] = new Vector2(uvX, uvY);//new Vector2((uvX + meshNumX), (uvY + meshNumZ));
                    
                            if (x == numQuadsX - 1 || z == numQuadsZ - 1)
                            {
                                BorderVertsIndex.Add(currentVertIndex + 2);
                                BorderVertsPos.Add(dVerts[currentVertIndex+2]);                               
                            }

                   

                        //UR just dupe the one above to save time.
                        dVerts[currentVertIndex + 3] = dVerts[currentVertIndex + 2];//new Vector3(XOffset, 0, ZOffset);
                        //dVerts[currentVertIndex + 3].y = dVerts[currentVertIndex + 2];//HMData[quadsizeBias[quadsizeBiasIndex] + ZHeightIndex, quadsizeBias[quadsizeBiasIndex] + XHeightIndex] * THeight;
                        // uvX = ((float)x + 1) / (float)numQuadsX;
                        dUVs[currentVertIndex + 3] = dUVs[currentVertIndex + 2];//new Vector2((uvX + meshNumX), (uvY + meshNumZ));
                     
                            if (x == numQuadsX - 1 || z == numQuadsZ - 1)
                            {
                                BorderVertsIndex.Add(currentVertIndex + 3);
                                BorderVertsPos.Add(dVerts[currentVertIndex+3]);                                
                            }

                     
                            
                        //LR 
                        dVerts[currentVertIndex + 4] = new Vector3(XOffset, 0, ZOffset - quadLength);
                        dVerts[currentVertIndex + 4].y = HMData[ZHeightIndex, quadsizeBias[quadsizeBiasIndex] + XHeightIndex] * THeight;
                        uvY = (float)z / (float)numQuadsZ;
                        dUVs[currentVertIndex + 4] = new Vector2(uvX, uvY);//new Vector2((uvX + meshNumX), (uvY + meshNumZ));
                            if (z == 0 || x==numQuadsX-1)
                            {
                                BorderVertsIndex.Add(currentVertIndex + 4);
                                BorderVertsPos.Add(dVerts[currentVertIndex+4]);
                            }
                        
                        //LL dupe the one above..
                        dVerts[currentVertIndex + 5] = dVerts[currentVertIndex];//new Vector3(XOffset - quadWidth, 0, ZOffset - quadLength);
                        //dVerts[currentVertIndex + 5].y = HMData[ZHeightIndex, XHeightIndex] * THeight;
                        //uvX = (float)x / (float)numQuadsX;
                        //uvY = (float)z / (float)numQuadsZ;
                        dUVs[currentVertIndex + 5] = dUVs[currentVertIndex]; //new Vector2((uvX + meshNumX), (uvY + meshNumZ));
                    
                            if (z == 0 || x == 0)// && z == numQuadsZ - 1)
                            {
                                BorderVertsIndex.Add(currentVertIndex+5);
                                BorderVertsPos.Add(dVerts[currentVertIndex+5]);
                            }
                     
                        currentVertIndex += 6;
                    }
                }
                #endregion
                //break the vertex normals; no shared vertices.
                for (int t = 0; t < numVerts; t++)
                    dTris[t] = t;
                
                //each LOD matches border vertex positions with the next LOD. Doing this will never yield a seamless terrain. 
                #region LOD border averaging
                //now, go through the index of vertices we need to modify and adjust their Y so the lods match up.
                //the vertices on the boundary of the chunk should never be averaged/modified!
                //only average if the number of chunks is >1
                if (numObjsZ > 1 && numObjsX > 1 && GenerateLODs && currentLOD!=1 && Average) //don't average the last lod
                {
                    //we'll check vertex positions against the size of the quads of the next lod. if they're inside, average em.
                    float doubleQW = quadWidth * 2; 
                    float doubleQL = quadLength * 2;

                    for (int i = 0; i < BorderVertsIndex.Count; i++)
                    {
                        //if the vert is inside the next lod's quadsize, average it. Otherwise move on.
                        if(BorderVertsPos[i].x % doubleQW !=0 || BorderVertsPos[i].z % doubleQL !=0)  
                        { 
                            //vert x and z positions are 0 to Chunksize. They are increments of quadWidth (x) and quadLength (z)

                            //grab a vert position. see if there are any neighbors to the left, right, up or down.
                            // there will always only be 2 neighbors.
                            // you know if you're on a corner if there is a neightbor to the left and down, right and down, left and up, or left and down. Never modify them as they're on the edge of chunk.
                            // a corner only has 2 vertex positions. Sides have 3.
                            // you know if you're on a side if there is a neighbor left and right. Modify that vertex, ***but only if it doesn't exist in the next LOD
                            //check to see if it's within the 0 and size of a quad*2?

                            Vector3 chosenVert = BorderVertsPos[i]; //get a vertex position.
                            List<Vector3> VertsSamePos = new List<Vector3>(); //declare a list that will hold vertex positions that are identical
                            List<int> VertsSameIndex = new List<int>(); //do the same but with an index array

                            //first, find the vertices that match the same position.
                            for (int v = 0; v < BorderVertsPos.Count; v++)
                            {
                                if (BorderVertsPos[v] == chosenVert)
                                {
                                    VertsSamePos.Add(BorderVertsPos[v]); //plug em in.
                                    VertsSameIndex.Add(BorderVertsIndex[v]); //store the index
                                }
                            }

                            //if we are on a corner which has only 2 positions, skip the averaging process. Otherwise..
                            if (VertsSamePos.Count > 2)
                            {

                                //create vectors for up, down, left, right, relative to the position + quadsize or quadlength
                                //we do 0 because it really doesn't matter. They all the same positions and the size is 3.

                                //you need to measure X and Z.. nothingi s returned if you're measuring Y because each vertex has a different y val.
                                Vector3 up = new Vector3(VertsSamePos[0].x, VertsSamePos[0].y, VertsSamePos[0].z + quadLength);
                                Vector3 down = new Vector3(VertsSamePos[0].x, VertsSamePos[0].y, VertsSamePos[0].z - quadLength);
                                Vector3 left = new Vector3(VertsSamePos[0].x - quadWidth, VertsSamePos[0].y, VertsSamePos[0].z);
                                Vector3 right = new Vector3(VertsSamePos[0].x + quadWidth, VertsSamePos[0].y, VertsSamePos[0].z);

                                //Vector3[] Neighbors = new Vector3[2]; //any vert will have only two neighbors (assuming all verts were welded)
                                List<Vector3> Neighbors = new List<Vector3>();

                                //find the neighbors.
                                for (int v = 0; v < BorderVertsPos.Count; v++)
                                {
                                    //if any of the BorderVertsPos elements match any of those, grab em and increment our array index to place the other.
                                    //neighbor list isn't big enough. it's grabbing dupe verts. gotta grab em all, weed out the duplicates, then average the result.
                                    if (BorderVertsPos[v].x == up.x && BorderVertsPos[v].z == up.z)
                                    {
                                        Neighbors.Add(BorderVertsPos[v]);
                                        
                                    }
                                    else if (BorderVertsPos[v].x == down.x && BorderVertsPos[v].z == down.z)
                                    {
                                        Neighbors.Add(BorderVertsPos[v]);
                                        

                                    }
                                    else if (BorderVertsPos[v].x == left.x && BorderVertsPos[v].z == left.z)
                                    {
                                        Neighbors.Add(BorderVertsPos[v]);
                                        
                                    }
                                    else if (BorderVertsPos[v].x == right.x && BorderVertsPos[v].z == right.z)
                                    {
                                        Neighbors.Add(BorderVertsPos[v]);
                                        
                                    }
                                }
                                //now remove the duplicates
                                for (int a = 0; a < Neighbors.Count;a++ )
                                {
                                    if (Neighbors[a] == Neighbors[a + 1])
                                    {
                                        Neighbors.RemoveAt(a + 1);
                                        a -= 1;
                                    }
                                    if (Neighbors.Count == 2)
                                        break;
                                }

                               
                                        //average the Y between them to get our new Y position.
                                float average = (Neighbors[0].y + Neighbors[1].y) / 2;

                                        int icounter = 0;
                                        //int[] targetVertIndex = new int[3];
                                        List<int> targetVertIndex = new List<int>();

                                        //int index = BorderVertsPos.IndexOf(VertsSamePos[l]); //just returning the same index over and over..
                                        for (int q = 0; q < BorderVertsPos.Count; q++)
                                        {
                                            if (BorderVertsPos[q] == VertsSamePos[icounter])
                                            {
                                                targetVertIndex.Add(BorderVertsIndex[q]);
                                                icounter++;
                                            }
                                            if (targetVertIndex.Count == 3)
                                            {
                                                dVerts[targetVertIndex[0]].y = average;
                                                dVerts[targetVertIndex[1]].y = average;
                                                dVerts[targetVertIndex[2]].y = average;
                                                break;
                                            }

                                        }
                                       
                            }
                        }
                    }
                }
                #endregion
                

                //setup the indices
                dMesh.vertices = dVerts;
                dMesh.triangles = dTris;
                dMesh.uv = dUVs;
                dMesh.uv2 = dUVs;
              //  dMesh.colors32 = dVCs; //moving the colors to its own function
              
                dMesh.RecalculateNormals();
                //write the mesh to disk!
                AssetDatabase.CreateAsset(dMesh, meshPath+"/"+g.name+".asset");
                mf.sharedMesh = dMesh;
                g.transform.localPosition = new Vector3(ObjXOffset * meshNumX, 0, ObjZOffset * meshNumZ);
                mr.sharedMaterial = PWTMat;
               
                AllChunks[currentGO] = g; //add the gameobject to our array                
                currentGO++; //increment to the next go index

            }

            
        }
        EditorUtility.ClearProgressBar();
        RenderVertexColors(AllChunks);


        
        return AllChunks; //send em back.
    }

    //renders the basemap directly to the vertices of the meshes
    public void RenderVertexColors(GameObject[] allChunks)
    {
        float[, ,] splatData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight); //get all the splatmap data
        int matAmount = terrainData.splatPrototypes.Length;
        Texture2D[] splatDiffuses = new Texture2D[matAmount]; //setup array to hold all the diffuses        
        //plug in all the diffuse textures we're using on the terrain.
        for (int s = 0; s < matAmount; s++)
            splatDiffuses[s] = terrainData.splatPrototypes[s].texture;

        //create the basemap for the terrain so our vertices can sample from it.
        Texture2D basemap = GenerateBaseMap(matAmount, splatDiffuses, splatData);

        Vector2 ChunkSize = chunkSizes[chunkIndex];
        quadWidth = (TWidth / HFR) * quadsizeBias[quadsizeBiasIndex];
        quadLength = (TLength / HFR) * quadsizeBias[quadsizeBiasIndex];
        int numQuadsX = (int)(ChunkSize[0] / quadWidth);
        int numQuadsZ = (int)(ChunkSize[1] / quadLength);       
        int numObjsX = (int)(TWidth / ChunkSize[0]);       

        //process every chunk. this is always only one LOD level at a time which is determined by the Chunksize from the ui
        //foreach(GameObject currentGO in allChunks)
        int meshNumX = 0;
        int meshNumZ = 0;

        for (int g = 0; g < allChunks.Length;g++)
        {
            EditorUtility.DisplayProgressBar("Rendering Vertex Colors", "Faceting Terrain Colors: "+g+" of "+allChunks.Length, g);
            Mesh currentMesh = allChunks[g].GetComponent<MeshFilter>().sharedMesh;
            Color[] dVCs = new Color[currentMesh.vertexCount];
            int currentXQuad = 0;
            int currentZQuad = 0;
            
            for (int x = 0; x < currentMesh.vertexCount; x += 6) //process every quad at a time
            {
                int qsb = quadsizeBias[quadsizeBiasIndex];

                int ZHeightIndex = (currentZQuad + (numQuadsZ * meshNumZ)) * qsb;
                int XHeightIndex = (currentXQuad + (numQuadsX * meshNumX)) * qsb;

                Color32 vcLL = GetVertexColor(basemap, XHeightIndex, ZHeightIndex);
                Color32 vcUL = GetVertexColor(basemap, XHeightIndex, qsb + ZHeightIndex);
                Color32 vcUR = GetVertexColor(basemap, 1 + XHeightIndex, qsb + ZHeightIndex);
                Color32 vcLR = GetVertexColor(basemap, 1 + XHeightIndex, ZHeightIndex);

                if (quadColor)
                {
                    Color32 c1 = Color32.Lerp(vcLL, vcUL, 0.5f);
                    Color32 c2 = Color32.Lerp(c1, vcUR, 0.5f);
                    Color32 c3 = Color32.Lerp(c2, vcLR, 0.5f);
                    dVCs[x] = c3;
                    dVCs[x + 1] = c3;
                    dVCs[x + 2] = c3;
                    dVCs[x + 3] = c3;
                    dVCs[x + 4] = c3;
                    dVCs[x + 5] = c3;
                }
                else
                {
                    Color32 TriL = Color32.Lerp(vcLL, vcUL, 0.5f);
                    TriL = Color32.Lerp(TriL, vcUR, 0.5f);
                    dVCs[x] = TriL;
                    dVCs[x + 1] = TriL;
                    dVCs[x + 2] = TriL;

                    Color32 TriR = Color32.Lerp(vcUL, vcLR, 0.5f);
                    TriR = Color32.Lerp(TriR, vcLL, 0.5f);
                    dVCs[x + 3] = TriR;
                    dVCs[x + 4] = TriR;
                    dVCs[x + 5] = TriR;
                }

                
                currentXQuad++;
                if (currentXQuad % numQuadsX == 0) //last x quad? set it to 0 and goto the next row
                {
                    currentXQuad = 0;
                    currentZQuad++;
                }

                
            }

            meshNumX++;
            if (meshNumX % numObjsX == 0) //if we're on the last meshnumX of the row, set it to 0 and increment meshnumZ so we start on a new row
            {
                meshNumX = 0;
                meshNumZ++;
            }

            currentMesh.colors = dVCs;
        }
        EditorUtility.ClearProgressBar();

        /*
         * 
         *   int ZHeightIndex = (z + (numQuadsZ * meshNumZ)) * quadsizeBias[quadsizeBiasIndex];

                    //z=3 so you're on the 3rd quad up. 3 + (10 * 2) * 1 = 23

                    //make quads along the x axis and apply all the needed data..
                    for (int x = 0; x < numQuadsX; x++)
                    {
                        int XHeightIndex = (x + (numQuadsX * meshNumX))*quadsizeBias[quadsizeBiasIndex];
                        XOffset = (x + 1) * quadWidth;
         * 
         *  Color32 vcLL = GetVertexColor(basemap, XHeightIndex, ZHeightIndex);

                        Color32 vcUL = GetVertexColor(basemap, XHeightIndex, 1 + ZHeightIndex);
                        Color32 vcUR = GetVertexColor(basemap, 1 + XHeightIndex, 1 + ZHeightIndex);
                        Color32 vcLR = GetVertexColor(basemap, 1 + XHeightIndex, ZHeightIndex);


                        //if we chose to average all the colors by quad..
                        if (quadColor)
                        {
                            Color32 c1 = Color32.Lerp(vcLL, vcUL, 0.5f);
                            Color32 c2 = Color32.Lerp(c1, vcUR, 0.5f);
                            Color32 c3 = Color32.Lerp(c2, vcLR, 0.5f);
                            dVCs[currentVertIndex] = c3;
                            dVCs[currentVertIndex + 1] = c3;
                            dVCs[currentVertIndex + 2] = c3;
                            dVCs[currentVertIndex + 3] = c3;
                            dVCs[currentVertIndex + 4] = c3;
                            dVCs[currentVertIndex + 5] = c3;
                        }
                        else
                        {
                            Color32 TriL = Color32.Lerp(vcLL, vcUL, 0.5f);
                            TriL = Color32.Lerp(TriL, vcUR, 0.5f);
                            dVCs[currentVertIndex] = TriL;
                            dVCs[currentVertIndex + 1] = TriL;
                            dVCs[currentVertIndex + 2] = TriL;

                            Color32 TriR = Color32.Lerp(vcUL, vcLR, 0.5f);
                            TriR = Color32.Lerp(TriR, vcLL, 0.5f);
                            dVCs[currentVertIndex + 3] = TriR;
                            dVCs[currentVertIndex + 4] = TriR;
                            dVCs[currentVertIndex + 5] = TriR;
                        }*/



    }


    public void SetupTerrainAssociation()
    {
       // PWT = (QT_PolyWorldTerrain)target;
       // terrainGO = PWT.gameObject;
       // terrainC = this.GetComponent<Terrain>();
        terrainData = this.GetComponent<Terrain>().terrainData;

        TWidth = terrainData.size.x;
        TLength = terrainData.size.z;
        THeight = terrainData.size.y;
        HFR = terrainData.heightmapResolution - 1; //power of 2 +1
        CTR = terrainData.alphamapResolution;

    }

    //determines the measurements of valid chunksizes.
    public Vector2[] GetChunkSizes()
    {
        quadWidth = (TWidth / HFR)*quadsizeBias[quadsizeBiasIndex];
        quadLength = (TLength / HFR)*quadsizeBias[quadsizeBiasIndex];

        float xSize = -1;
        float ySize = -1;

        List<float> xSizes = new List<float>();
        List<float> ySizes = new List<float>();

        //we start at 7 because the values that are lower are just too small.
        for (int x = 2; x <= HFR; x++)
        {
            xSize = TWidth % (quadWidth * x);
            ySize = TLength % (quadLength * x);
            if (xSize == 0f)
                xSizes.Add(x * quadWidth);
            if (ySize == 0f)
                ySizes.Add(x * quadLength);
        }

        Vector2[] finalSizes = new Vector2[xSizes.Count];
        for (int z = 0; z < xSizes.Count; z++)
        {
            finalSizes[z].x = xSizes[z];
            finalSizes[z].y = ySizes[z];
        }
        return finalSizes;
    }
#endif
}
