Shader "VampireLike/CartoonVertex"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) _Tint ("Tint", Color) = (1,1,1,1) _Visibility("Visibility",Range(0,1))=1 _Terrain("Terrain",Float)=0 _EdgeEast("East",Color)=(1,1,1,1) _EdgeWest("West",Color)=(1,1,1,1) _EdgeNorth("North",Color)=(1,1,1,1) _EdgeSouth("South",Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex outlineVert
            #pragma fragment outlineFrag
            #include "UnityCG.cginc"
            float _Terrain,_Visibility;
            struct edgeInput {float4 vertex:POSITION;float3 normal:NORMAL;fixed4 color:COLOR;};
            struct edgeOutput {float4 pos:SV_POSITION;fixed3 color:COLOR;};
            edgeOutput outlineVert(edgeInput v){edgeOutput o;float3 p=mul(unity_ObjectToWorld,v.vertex).xyz;float3 n=UnityObjectToWorldNormal(v.normal);p+=normalize(n)*(_Terrain>.5?0:.014);o.pos=mul(UNITY_MATRIX_VP,float4(p,1));o.color=v.color.rgb*.3;return o;}
            fixed4 outlineFrag(edgeOutput i):SV_Target{clip(_Visibility-frac(dot(floor(i.pos.xy),float2(.5,.25)))-.01);return fixed4(i.color,1);}
            ENDCG
        }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; fixed4 color:COLOR; SHADOW_COORDS(2) UNITY_FOG_COORDS(3) float3 local:TEXCOORD4; UNITY_VERTEX_OUTPUT_STEREO };
            fixed4 _Color, _Tint,_EdgeEast,_EdgeWest,_EdgeNorth,_EdgeSouth;float4 _BlendEdges,_WorldOrigin;float _Terrain,_Flash,_Visibility;
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos=UnityObjectToClipPos(v.vertex); o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
                o.normal=UnityObjectToWorldNormal(v.normal); o.color=v.color*_Color*_Tint;
                o.local=v.vertex.xyz;
                TRANSFER_SHADOW(o); UNITY_TRANSFER_FOG(o,o.pos); return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                clip(_Visibility-frac(dot(floor(i.pos.xy),float2(.5,.25)))-.01);
                float3 n=normalize(i.normal); float nl=dot(n,normalize(_WorldSpaceLightPos0.xyz));
                float shade=smoothstep(-.28,.72,nl); float shadow=SHADOW_ATTENUATION(i);
                float rim=pow(1-saturate(dot(n,normalize(_WorldSpaceCameraPos-i.world))),3)*.08;
                fixed3 baseColor=i.color.rgb;
                baseColor=lerp(baseColor,fixed3(1,1,.86),_Flash);
                if(_Terrain>.5&&i.local.y<.002)
                {
                    float2 global=i.local.xz+_WorldOrigin.xy;
                    float widthX=6+sin(global.y*.24)*1.5+sin(global.y*.61)*.6;
                    float widthZ=6+sin(global.x*.24)*1.5+sin(global.x*.61)*.6;
                    baseColor=lerp(baseColor,_EdgeEast.rgb,smoothstep(16-widthX,16,i.local.x)*_BlendEdges.x);
                    baseColor=lerp(baseColor,_EdgeWest.rgb,smoothstep(16-widthX,16,-i.local.x)*_BlendEdges.y);
                    baseColor=lerp(baseColor,_EdgeNorth.rgb,smoothstep(16-widthZ,16,i.local.z)*_BlendEdges.z);
                    baseColor=lerp(baseColor,_EdgeSouth.rgb,smoothstep(16-widthZ,16,-i.local.z)*_BlendEdges.w);
                    baseColor*=.985+.015*sin(global.x*.18+sin(global.y*.23));
                }
                fixed3 c=baseColor*(.52+.48*shade*lerp(.52,1,shadow))*lerp(fixed3(1,1,1),_LightColor0.rgb,.35)+rim*baseColor;
                fixed4 result=fixed4(c,1); UNITY_APPLY_FOG(i.fogCoord,result); return result;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    Fallback "Diffuse"
}
