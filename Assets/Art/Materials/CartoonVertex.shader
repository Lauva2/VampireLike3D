Shader "VampireLike/CartoonVertex"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) _Tint ("Tint", Color) = (1,1,1,1) _Visibility("Visibility",Range(0,1))=1 _Terrain("Terrain",Float)=0 _EdgeEast("East",Color)=(1,1,1,1) _EdgeWest("West",Color)=(1,1,1,1) _EdgeNorth("North",Color)=(1,1,1,1) _EdgeSouth("South",Color)=(1,1,1,1) _SrcBlend("Source blend",Float)=1 _DstBlend("Destination blend",Float)=0 _ZWrite("Depth write",Float)=1 _ZTest("Depth test",Float)=4 _SmoothFade("Smooth fade",Float)=0 _AlphaCoverage("MSAA coverage",Float)=1 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGINCLUDE
        #include "UnityCG.cginc"
        UNITY_INSTANCING_BUFFER_START(ActorProperties)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
            UNITY_DEFINE_INSTANCED_PROP(float, _Flash)
            UNITY_DEFINE_INSTANCED_PROP(float, _Emission)
            UNITY_DEFINE_INSTANCED_PROP(float, _Visibility)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Occlusion)
            UNITY_DEFINE_INSTANCED_PROP(float, _FxStyle)
            UNITY_DEFINE_INSTANCED_PROP(float, _FxProgress)
            UNITY_DEFINE_INSTANCED_PROP(float, _Readability)
            UNITY_DEFINE_INSTANCED_PROP(float, _Hostile)
            UNITY_DEFINE_INSTANCED_PROP(float, _Hazard)
            UNITY_DEFINE_INSTANCED_PROP(float, _DangerActive)
        UNITY_INSTANCING_BUFFER_END(ActorProperties)
        #define ACTOR_VALUE(name) UNITY_ACCESS_INSTANCED_PROP(ActorProperties, name)
        float _SmoothFade;
        float VisibilityClip(float3 world)
        {
            float4 occlusion=ACTOR_VALUE(_Occlusion);
            float visibility=ACTOR_VALUE(_Visibility);
            if(occlusion.w>.5)
            {
                float4 a=mul(UNITY_MATRIX_VP,float4(world,1)),b=mul(UNITY_MATRIX_VP,float4(occlusion.xyz,1));
                float2 delta=a.xy/a.w-b.xy/b.w;delta.x*=_ScreenParams.x/_ScreenParams.y;
                float coverage=smoothstep((1-visibility)*.25-.006,(1-visibility)*.25+.006,length(delta));
                clip(coverage-.01);return coverage;
            }
            clip(visibility-.005);return 1;
        }
        float3 EffectPosition(float3 p,float4 part)
        {
            if(ACTOR_VALUE(_FxStyle)<.5)return p;
            float t=saturate(ACTOR_VALUE(_FxProgress));float3 center=part.yzw;
            if(part.x<.5)return center+(p-center)*max(.03,sin(saturate(t*1.8)*3.14159));
            if(part.x<1.5)return p*(.18+t*1.82);
            if(part.x<2.5){float3 flight=center*float3(1+t*2.0,1,1+t*2.0);flight.y+=sin(t*3.14159)*.35-t*t*.5;return flight+(p-center)*(1-t*.7);}
            return p+float3(sin(p.y*8+t*23)*.025,0,0);
        }
        ENDCG
        Pass
        {
            Cull Front
            AlphaToMask On
            CGPROGRAM
            #pragma vertex outlineVert
            #pragma fragment outlineFrag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            float _Terrain;
            struct edgeInput {float4 vertex:POSITION;float3 normal:NORMAL;fixed4 color:COLOR;float4 part:TEXCOORD1;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct edgeOutput {float4 pos:SV_POSITION;fixed3 color:COLOR;float3 world:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            edgeOutput outlineVert(edgeInput v){edgeOutput o;UNITY_SETUP_INSTANCE_ID(v);UNITY_TRANSFER_INSTANCE_ID(v,o);float3 p=mul(unity_ObjectToWorld,float4(EffectPosition(v.vertex.xyz,v.part),1)).xyz;float3 n=UnityObjectToWorldNormal(v.normal);p+=normalize(n)*(_Terrain>.5?0:.014);o.pos=mul(UNITY_MATRIX_VP,float4(p,1));o.world=p;o.color=v.color.rgb*.3;return o;}
            fixed4 outlineFrag(edgeOutput i):SV_Target{UNITY_SETUP_INSTANCE_ID(i);float coverage=VisibilityClip(i.world);return fixed4(i.color,coverage);}
            ENDCG
        }
        Pass
        {
            Name "CARTOONFORWARD"
            Tags { "LightMode"="ForwardBase" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            AlphaToMask [_AlphaCoverage]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR;float4 part:TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; fixed4 color:COLOR; SHADOW_COORDS(2) UNITY_FOG_COORDS(3) float3 local:TEXCOORD4; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };
            fixed4 _Color,_EdgeEast,_EdgeWest,_EdgeNorth,_EdgeSouth;float4 _BlendEdges,_WorldOrigin;float _Terrain;
            float _OrganicTerrain;
            float4 _PlayerFocus;
            fixed4 _RegionCenter,_Region00,_Region01,_Region02,_Region10,_Region12,_Region20,_Region21,_Region22;
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                v.vertex.xyz=EffectPosition(v.vertex.xyz,v.part);
                o.pos=UnityObjectToClipPos(v.vertex); o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
                o.normal=UnityObjectToWorldNormal(v.normal); o.color=v.color*_Color*ACTOR_VALUE(_Tint);
                o.local=v.vertex.xyz;
                TRANSFER_SHADOW(o); UNITY_TRANSFER_FOG(o,o.pos); return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float coverage=VisibilityClip(i.world);
                float3 n=normalize(i.normal); float nl=dot(n,normalize(_WorldSpaceLightPos0.xyz));
                float shade=smoothstep(-.28,.72,nl); float shadow=SHADOW_ATTENUATION(i);
                float rim=pow(1-saturate(dot(n,normalize(_WorldSpaceCameraPos-i.world))),3)*.08;
                fixed3 baseColor=i.color.rgb;
                baseColor=lerp(baseColor,fixed3(1,1,.86),ACTOR_VALUE(_Flash));
                baseColor=lerp(baseColor,fixed3(1,.10,.23),ACTOR_VALUE(_Hostile)*.48);
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
                    if(_OrganicTerrain>.5)
                    {
                        float2 p=i.local.xz+float2(sin(global.y*.105)+sin(global.y*.247)*.35,sin(global.x*.113)+cos(global.x*.213)*.35)*4.8;
                        float2 weights=smoothstep(0,32,abs(p));
                        fixed3 eastWest=p.x>0?_Region21.rgb:_Region01.rgb;
                        fixed3 northSouth=p.y>0?_Region12.rgb:_Region10.rgb;
                        fixed3 diagonal=p.x>0?(p.y>0?_Region22.rgb:_Region20.rgb):(p.y>0?_Region02.rgb:_Region00.rgb);
                        baseColor=lerp(lerp(_RegionCenter.rgb,eastWest,weights.x),lerp(northSouth,diagonal,weights.x),weights.y);
                    }
                }
                fixed3 c=baseColor*(.52+.48*shade*lerp(.52,1,shadow))*lerp(fixed3(1,1,1),_LightColor0.rgb,.35)+rim*baseColor;
                float3 halfDirection=normalize(normalize(_WorldSpaceLightPos0.xyz)+normalize(_WorldSpaceCameraPos-i.world));
                float gloss=i.color.a;float highlight=pow(saturate(dot(n,halfDirection)),lerp(12,64,gloss))*gloss*.18*shadow;
                c+=highlight*_LightColor0.rgb;
                c+=baseColor*(ACTOR_VALUE(_Emission)+ACTOR_VALUE(_FxStyle)*.14*pow(1-saturate(ACTOR_VALUE(_FxProgress)),2));
                float focus=lerp(1,lerp(.32,1,smoothstep(.7,2.4,length(i.world.xz-_PlayerFocus.xz))),ACTOR_VALUE(_Readability)*_PlayerFocus.w);
                float dangerFocus=lerp(1,lerp(lerp(.18,.42,ACTOR_VALUE(_DangerActive)),1,1-smoothstep(3,7,length(i.world.xz-_PlayerFocus.xz))),ACTOR_VALUE(_Hazard)*_PlayerFocus.w);
                fixed4 result=fixed4(c,lerp(1,ACTOR_VALUE(_Visibility),_SmoothFade)*focus*dangerFocus*coverage); UNITY_APPLY_FOG(i.fogCoord,result); return result;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    Fallback "Diffuse"
}
