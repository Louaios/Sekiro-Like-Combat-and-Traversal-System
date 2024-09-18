Shader "Quantum Theory/PolyWorld/PolyWorld Terrain Tree" {
	Properties{
		
		[HideInInspector] _TreeInstanceColor("TreeInstanceColor", Vector) = (1,1,1,1)
		[HideInInspector] _TreeInstanceScale("TreeInstanceScale", Vector) = (1,1,1,1)
		[HideInInspector] _SquashAmount("Squash", Float) = 1
	}

		SubShader{
		Tags{ "IgnoreProjector" = "True" "RenderType" = "TreeBark" }
		LOD 200

		CGPROGRAM
#pragma surface surf BlinnPhong vertex:TreeVertBark addshadow nolightmap

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "TerrainEngine.cginc"

	struct Input {
		float2 uv_MainTex;
		fixed4 color : COLOR;
	};

	void TreeVertBark(inout appdata_full v)
	{
		v.vertex.xyz *= _TreeInstanceScale.xyz;
		v.vertex = Squash(v.vertex);
		v.color.rgb = v.color* _TreeInstanceColor.rgb;
	}


	void surf(Input IN, inout SurfaceOutput o) {
	
		o.Albedo = IN.color;

	
		o.Alpha = 1;

	
	}
	ENDCG
	}

		Dependency "BillboardShader" = "Hidden/QuantumTheory/PolyWorld Tree Rendertex A"
}
