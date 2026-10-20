Shader "VampireLike/ContactShadow"
{
    SubShader
    {
        Tags {"Queue"="Transparent-10" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            struct v2f{float4 pos:SV_POSITION;fixed4 color:COLOR;UNITY_FOG_COORDS(0)};
            v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.color=v.color;UNITY_TRANSFER_FOG(o,o.pos);return o;}
            fixed4 frag(v2f i):SV_Target{fixed4 c=fixed4(.035,.055,.045,i.color.a*.29);UNITY_APPLY_FOG(i.fogCoord,c);return c;}
            ENDCG
        }
    }
}
