// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Quantum Theory/PolyWorld/PolyWorld SurfaceNoise"
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
			float2 appendResult24 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner28 = ( _Time.y * _Speed + ( ( _Tiling * 0.1 ) * appendResult24 ));
			v.vertex.xyz += ( ( (-1.0 + (tex2Dlod( _OffsetMap, float4( panner28, 0, 0.0) ).r - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) * ( _Intensity * _WaveDirection ) ) * v.color.a );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = ( _Color * i.vertexColor ).rgb;
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
2039;269;1426;825;2295.946;-21.00348;2.047793;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;23;-2194.051,123.756;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;25;-1892.871,13.74815;Float;False;Property;_Tiling;Tiling;3;0;Create;True;0;0;False;0;1;1;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-1624.431,25.45296;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;24;-1632.171,154.9074;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;10;-1544.754,324.7912;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;32;-1523.935,402.4195;Float;False;Property;_Speed;Speed;4;0;Create;True;0;0;False;0;0.5,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-1447.107,158.5434;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;28;-1299.916,286.3846;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;13;-1021.096,464.3003;Float;True;Property;_OffsetMap;Offset Map;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;18;-1522.783,878.0842;Float;False;Property;_WaveDirection;Wave Direction;7;0;Create;True;0;0;False;0;0,2,0;0,2,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;16;-1317.51,745.1077;Float;False;Property;_Intensity;Intensity;2;0;Create;True;0;0;False;0;0.15;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-908.2501,846.3531;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;14;-717.1597,537.6454;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;22;-579.6332,-247.3315;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;1;-760.4095,-3.363168;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-521.0535,699.5442;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-203.974,-91.89807;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-413.5692,36.73241;Float;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-282.8831,495.1934;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-423,134.5;Float;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;0;Float;;0;0;Standard;Quantum Theory/PolyWorld/PolyWorld SurfaceNoise;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;33;0;25;0
WireConnection;24;0;23;1
WireConnection;24;1;23;3
WireConnection;26;0;33;0
WireConnection;26;1;24;0
WireConnection;28;0;26;0
WireConnection;28;2;32;0
WireConnection;28;1;10;0
WireConnection;13;1;28;0
WireConnection;17;0;16;0
WireConnection;17;1;18;0
WireConnection;14;0;13;1
WireConnection;15;0;14;0
WireConnection;15;1;17;0
WireConnection;21;0;22;0
WireConnection;21;1;1;0
WireConnection;20;0;15;0
WireConnection;20;1;1;4
WireConnection;0;0;21;0
WireConnection;0;3;2;0
WireConnection;0;4;3;0
WireConnection;0;11;20;0
ASEEND*/
//CHKSM=7E6B4E9444367A5E7A5D6D6E7B85D1A645E45022