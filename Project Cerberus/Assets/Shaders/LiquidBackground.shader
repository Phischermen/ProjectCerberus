Shader "Custom/LiquidBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeScale ("Time Scale", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _TimeScale;

            fixed4 frag (v2f i) : SV_Target
            {
                // Scale the time to make the animation slower.
                float time = _Time.y * _TimeScale;
                // Offset the UVs by the current time.
                float2 uv = i.uv + time;
                // Offset the U with a sine wave. Animate the wave with the current time.
                uv.x += sin(uv.y * 10) * sin(time);
                // uv.x += sin(uv.y * 10) * 0.1;
                fixed4 col = tex2D(_MainTex, uv);
                return col;
                //return fixed4(uv.x, uv.y, 0, 1);
            }
            ENDCG
        }
    }
}