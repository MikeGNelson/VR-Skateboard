Shader "Custom/QuadrentShader"
{
    Properties
    {
        _Horizontalbounding("Horizontal Bounding", Float) = 0.5
        _Bounding("Bounding", Float) = 0.5
        _Hor("Horizontal", Float) = 0.5
        _Vert("Vertical", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            float _Horizontalbounding;
            float _Bounding;
            float _Hor;
            float _Vert;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float r = 0.0;
                float g = 0.0;
                float b = 0.0;

                float dim = 20.0;
                float halfDim = dim / 2.0;
                float horAbs = abs(_Hor);
                float vertAbs = abs(_Vert);
                bool horGreaterThanBound = _Hor > _Horizontalbounding;
                bool vertGreaterThanBound = _Vert > _Bounding;

                // Calculate x and y based on UV coordinates
                float x = uv.x * dim;
                float y = uv.y * dim;

                // Right or Left
                if ((x >= halfDim && horGreaterThanBound) || (x < halfDim && _Hor < _Horizontalbounding))
                {
                    g += horAbs;
                }

                // Top or Bottom
                if ((y >= halfDim && vertGreaterThanBound) || (y < halfDim && _Vert < _Bounding))
                {
                    g += vertAbs;
                }

                g = clamp(g, 0.0, 1.0); // Ensure g is within the range [0, 1]

                return fixed4(r, g, b, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
