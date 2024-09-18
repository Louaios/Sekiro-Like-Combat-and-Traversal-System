// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Quantum Theory/PolyWorld/PolyWorld SurfaceNoise Linear"
{
	Properties
	{
		_OffsetMap("Offset Map", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_Intensity("Intensity", Range( 0 , 1)) = 0.15
		_Tiling("Tiling", Range( 0 , 4)) = 1
		_Speed("Speed", Vector) = (0.5,0,0,0)
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_WaveDirection("Wave Direction", Vector) = (0,2,0,0)
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 2.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _OffsetMap;
		uniform float2 _Speed;
		uniform float _Tiling;
		uniform float _Intensity;
		uniform float3 _WaveDirection;
		uniform float4 _Color;
		uniform float _Metallic;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult29 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner34 = ( _Time.y * _Speed + ( ( _Tiling * 0.1 ) * appendResult29 ));
			v.vertex.xyz += ( ( (-1.0 + (tex2Dlod( _OffsetMap, float4( panner34, 0, 0.0) ).r - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) * ( _Intensity * _WaveDirection ) ) * v.color.a );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = ( _Color * pow( i.vertexColor , 2.2 ) ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=16700
2039;269;1426;825;1969.895;-260.3328;1.375517;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;28;-1898.415,-23.61992;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;27;-1837.235,-115.6278;Float;False;Property;_Tiling;Tiling;3;0;Create;True;0;0;False;0;1;1;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1668.795,-124.923;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;29;-1676.535,4.531479;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-1491.471,8.16748;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;32;-1568.299,252.0436;Float;False;Property;_Speed;Speed;4;0;Create;True;0;0;False;0;0.5,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;31;-1589.118,174.4153;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;34;-1344.28,136.0087;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector3Node;35;-1228.492,883.442;Float;False;Property;_WaveDirection;Wave Direction;7;0;Create;True;0;0;False;0;0,2,0;0,2,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;16;-1317.51,745.1077;Float;False;Property;_Intensity;Intensity;2;0;Create;True;0;0;False;0;0.15;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-1021.096,464.3003;Float;True;Property;_OffsetMap;Offset Map;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;1;-877.9478,-156.8507;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-908.2501,846.3531;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;14;-724.8217,482.4787;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;7;-572.0827,-156.3693;Float;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;2.2;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;21;-649.3992,-402.6664;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-521.0535,699.5442;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-423,134.5;Float;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-271.6965,459.9937;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-422,45.5;Float;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-272.5076,-247.233;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;0;Float;;0;0;Standard;Quantum Theory/PolyWorld/PolyWorld SurfaceNoise Linear;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;27;0
WireConnection;29;0;28;1
WireConnection;29;1;28;3
WireConnection;33;0;30;0
WireConnection;33;1;29;0
WireConnection;34;0;33;0
WireConnection;34;2;32;0
WireConnection;34;1;31;0
WireConnection;13;1;34;0
WireConnection;17;0;16;0
WireConnection;17;1;35;0
WireConnection;14;0;13;1
WireConnection;7;0;1;0
WireConnection;15;0;14;0
WireConnection;15;1;17;0
WireConnection;20;0;15;0
WireConnection;20;1;1;4
WireConnection;22;0;21;0
WireConnection;22;1;7;0
WireConnection;0;0;22;0
WireConnection;0;3;2;0
WireConnection;0;4;3;0
WireConnection;0;11;20;0
ASEEND*/
//CHKSM=962A403D50C0C8E8624D0DCFC968AFE2B235295A