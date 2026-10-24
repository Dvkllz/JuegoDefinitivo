Shader "Nebula/GradientSky"
{
    Properties
    {
        _Top ("Zenith", Color) = (0.035, 0.075, 0.15, 1)
        _Horizon ("Horizon", Color) = (0.23, 0.43, 0.46, 1)
        _Bottom ("Below", Color) = (0.06, 0.15, 0.22, 1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };
            float4 _Top, _Horizon, _Bottom;
            v2f vert(appdata_base v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.dir=v.vertex.xyz; return o; }
            fixed4 frag(v2f i) : SV_Target
            {
                float y=normalize(i.dir).y;
                return y>0 ? lerp(_Horizon, _Top, pow(saturate(y),0.55)) : lerp(_Horizon, _Bottom, saturate(-y*2));
            }
            ENDCG
        }
    }
    Fallback Off
}
