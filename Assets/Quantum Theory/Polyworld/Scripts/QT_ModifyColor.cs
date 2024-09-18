using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[ExecuteInEditMode]
//[AddComponentMenu("Transform/Follow Transform")]
public class QT_ModifyColor : MonoBehaviour {

    public bool Replace = true;
    public string meshFileName = "new mesh";
    public string newPrefabName = ""; //holds the name of newly created prefabs.

    public QT_MCChannels[] AllChannels = new QT_MCChannels[6];
    public Color[] tempColors = new Color[6]; //holds all the temporary colors.  Used for the editorgui and gets converted to float[3]
    [HideInInspector]
    public float[] tempSmoothness = new float[6]; //for smoothness
    [HideInInspector]
    public float[] tempMetallic = new float[6]; //for metallic

    public float globalAlpha = 1f;
    public float Contrast = .5f;
    public float Shadows = 1f;
    public float MidTones = 1f;    
    public float Highlights = 0f;
    public float ContrastClamp1 = 0f;
    public float ContrastClamp2 = 1f;
    public bool preserveShading = true;

    [HideInInspector]
    public Color[] originalVCs;
    [HideInInspector]
    public Vector3[] originalVC_HSVs; //holds a cache of the orignial vertex colors in HSV format.
    [HideInInspector]
    public float[] originalValues;
    [HideInInspector]
    public float[] newValues;
    [HideInInspector]
    public float[] newShading;
    [HideInInspector]
    public bool isPWMesh = false; //whether the mesh is a polyworld-authored mesh which has vertex alpha values specifically placed on the mesh to mask color.
    public bool hasCollider = false;
    public bool isSaving = false;
    public bool pwMeshOverride = false; //manual override for channel mapping on pw meshes.
    public bool updateMeshCollider = true; //automatically update the sharedmesh in a meshcollider.
    public bool isSM, isMesh;
    
    public Mesh mesh; //the original mesh
    public Mesh tempMesh; //the working mesh
    public MeshFilter sourceMF;
    public SkinnedMeshRenderer sourceSMR;

    //[HideInInspector]
    //public Vector2[] uv4s;

    [HideInInspector]
    public bool isPrefab = false;

    public void AssignVCs(Color32[] c)
    {   
        tempMesh.colors32 = c;
        
    }

    public void AssignVCs(Color[] c)
    {
        tempMesh.colors = c;
    }

    public void AssignUV4s(Vector2[] v)
    {
        tempMesh.uv4 = v;
    }


    public void AssignVCs(float[] v)
    {
        Color32[] c32 = new Color32[tempMesh.colors32.Length];
        //c32 is byte
        for (int x = 0; x < tempMesh.colors32.Length; x++)
        {
            c32[x].r = (byte)(v[x] * 255f);
            c32[x].g = (byte)(v[x] * 255f);
            c32[x].b = (byte)(v[x] * 255f);
            c32[x].a = mesh.colors32[x].a;
        }
        tempMesh.colors32 = c32;
    }

   

    void Awake()
    {
        
        for (int x = 0; x < AllChannels.Length; x++)
            AllChannels[x] = new QT_MCChannels();

     tempColors[0] = new Color(.47f, .439f, .372f);
     tempColors[1] = new Color(.309f, .243f, .176f);
     tempColors[2] = new Color(.439f, .372f, .301f);
     tempColors[3] = new Color(.239f, .239f, .239f);
     tempColors[4] = new Color(.384f, .384f, .384f);
     tempColors[5] = new Color(.243f, .2f, .137f);
     //setup some defaults when the script first runs.
     AllChannels[0].Color = new float[4] { .47f, .439f, .372f,1f };
     AllChannels[1].Color = new float[4] { .309f, .243f, .176f,1f };
     AllChannels[2].Color = new float[4] { .439f, .372f, .301f,1f };
     AllChannels[3].Color = new float[4] { .239f, .239f, .239f,1f };
     AllChannels[4].Color = new float[4] { .384f, .384f, .384f,1f };
     AllChannels[5].Color = new float[4] { .243f, .2f, .137f,1f };
     
    }

   
	// Use this for initialization
	void Start () 
    {
       
        this.meshFileName = this.gameObject.name + "-Colored";
        this.newPrefabName = this.gameObject.transform.root.gameObject.name;
	}
	
    

   
    public float[] RemapFloats(float[] v, float high, float low)
    {
        float[] finalVals = new float[v.Length];
        float max = v.Max<float>();
        float min = v.Min<float>();
        float oldRange = (max - min);
        float newRange = (high - low);

        for (int x = 0; x < v.Length; x++)
        {
            finalVals[x] = (((v[x] - min) * newRange) / oldRange) + low;
            Mathf.Clamp01(finalVals[x]);
        }

        return finalVals;

    }

    //cleans up all out crap.
    public void CleanUp()
    {
        tempMesh = null;
        mesh = null;
        for (int x = 0; x < AllChannels.Length; x++)
        {
            AllChannels[x].Color = new float[4] { 0, 0, 0, 1 };
            AllChannels[x].sampledColor = new Color(0, 0, 0, 1);
            AllChannels[x].targetVertices = null;
        }

        
    }
   
    
}

//contains channel information for non pw meshes in modifycolor
public class QT_MCChannels
{
    public float[] Color = new float[4] {0,0,0,1}; //can't serialize color
    public Color sampledColor = new Color(1, 0, 1, 0.123f); //Magenta with a weird alpha value. Checked 
    public List<int> targetVertices = new List<int>();
    public float hueRange = .5f; // 0 to 359
    public float valRange = .25f; //0 to 255, same as saturation
    public float satRange = 0.5f; //test
    public float Smoothness = 0f;
    public float Metallic = 0f;
}
