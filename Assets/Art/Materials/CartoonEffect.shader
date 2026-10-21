Shader "VampireLike/CartoonEffect"
{
    Properties{_Color("Color",Color)=(1,1,1,1) _Tint("Tint",Color)=(1,1,1,1) _Visibility("Visibility",Range(0,1))=1 _Terrain("Terrain",Float)=0 _SrcBlend("Source blend",Float)=5 _DstBlend("Destination blend",Float)=10 _ZWrite("Depth write",Float)=0 _ZTest("Depth test",Float)=4 _SmoothFade("Smooth fade",Float)=1 _Readability("Player focus fade",Float)=1 _AlphaCoverage("MSAA coverage",Float)=0}
    SubShader
    {
        Tags{"RenderType"="Transparent" "Queue"="Transparent"}
        UsePass "VampireLike/CartoonVertex/CARTOONFORWARD"
    }
}
