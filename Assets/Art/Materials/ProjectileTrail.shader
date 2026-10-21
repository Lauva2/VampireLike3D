Shader "VampireLike/ProjectileTrail"
{
    SubShader
    {
        Tags{"Queue"="Transparent+10" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct input{float4 vertex:POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
            struct output{float4 position:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
            output vert(input v){output o;o.position=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
            fixed4 frag(output i):SV_Target{fixed4 c=i.color;c.a*=smoothstep(0,.35,1-abs(i.uv.y*2-1));return c;}
            ENDCG
        }
    }
}
