Shader "Custom/NormalShader"
{
    // no Properties block this time!
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // include file that contains UnityObjectToWorldNormal helper function
            #include "UnityCG.cginc"

            struct v2f {
        // we'll output world space normal as one of regular ("texcoord") interpolators
        half3 worldNormal : TEXCOORD0;
        float4 pos : SV_POSITION;
        float4 pos2: TEXCOORD1;
    };

    // vertex shader: takes object space normal as input too
    v2f vert(float4 vertex : POSITION, float3 normal : NORMAL)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(vertex);
        o.pos2 = vertex;
        //o.pos = vertex;
        // UnityCG.cginc file contains function to transform
        // normal from object to world space, use that
        o.worldNormal = UnityObjectToWorldNormal(normal);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        fixed4 c = 0;
    // normal is a 3D vector with xyz components; in -1..1
    // range. To display it as color, bring the range into 0..1
    // and put into red, green, blue components
    //c.rgb = i.worldNormal * 0.5 + 0.5;
    c.rgb = float3(max(0, (sin(i.pos2.y * 20) - 0.9)*5)+(1-(i.worldNormal.y)), max(0, sin(i.pos2.y * 20) - 0.9) + i.worldNormal.y * 5 - 4 + (1 - (i.worldNormal.y)), 0);
    return c;
}
ENDCG
}
    }
}
