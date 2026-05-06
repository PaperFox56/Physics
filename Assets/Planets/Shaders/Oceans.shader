Shader "Planet/OceanDepth"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0.0, 0.3, 0.6, 1.0)
        _DeepWaterColor ("Deep Water Color", Color) = (0.0, 0.05, 0.15, 1.0)
        _DepthMax ("Depth Max", Float) = 100.0
        _FoamColor ("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamIntensity ("Foam Intensity", Range(0, 1)) = 0.5
        _ShorelineWidth ("Shoreline Width", Float) = 1.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.8
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            // Explicitly declare the depth texture (Unity will bind it if enabled)
            sampler2D _CameraDepthTexture;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                UNITY_FOG_COORDS(5)
                SHADOW_COORDS(6)
            };
            
            float4 _WaterColor;
            float4 _DeepWaterColor;
            float _DepthMax;
            float4 _FoamColor;
            float _FoamIntensity;
            float _ShorelineWidth;
            float _Glossiness;
            float _Metallic;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                o.pos = UnityWorldToClipPos(worldPos);
                
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos.xyz);
                
                o.screenPos = ComputeScreenPos(o.pos);
                o.uv = v.uv;
                
                UNITY_TRANSFER_FOG(o, o.pos);
                TRANSFER_SHADOW(o);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Get screen UV coordinates
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                // Sample depth texture
                float rawDepth = tex2D(_CameraDepthTexture, screenUV).r;
                float sceneDepth = LinearEyeDepth(rawDepth);
                float currentDepth = i.screenPos.w;
                
                // Calculate water depth
                float waterDepth = sceneDepth - currentDepth;
                
                // Discard if no terrain underwater
                if (waterDepth <= 0.05)
                {
                    discard;
                }
                
                // Normalize depth
                float depth01 = saturate(waterDepth / _DepthMax);
                
                // Water color based on depth
                float3 waterColor = lerp(_WaterColor.rgb, _DeepWaterColor.rgb, depth01);
                float alpha = lerp(0.6, 0.95, depth01);
                
                // Shoreline foam
                float shoreline = 1.0 - smoothstep(0, _ShorelineWidth, waterDepth);
                shoreline = saturate(shoreline);
                
                // Simple wave foam
                float waveFoam = sin(i.worldPos.x * 0.5 + _Time.y * 2) * cos(i.worldPos.z * 0.5 + _Time.y * 1.5);
                waveFoam = saturate(waveFoam * waveFoam * _FoamIntensity * (1 - depth01));
                
                float foam = max(shoreline, waveFoam);
                
                // Lighting
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(i.viewDir);
                
                float NdotL = saturate(dot(normal, lightDir));
                float3 diffuse = _LightColor0.rgb * NdotL;
                
                float3 halfVec = normalize(lightDir + viewDir);
                float spec = pow(saturate(dot(normal, halfVec)), _Glossiness * 128);
                float3 specular = spec * _LightColor0.rgb * _Metallic;
                
                float3 ambient = ShadeSH9(float4(normal, 1));
                float shadow = SHADOW_ATTENUATION(i);
                
                float3 finalColor = waterColor * (diffuse * shadow + ambient) + specular * shadow;
                finalColor = lerp(finalColor, _FoamColor.rgb, foam);
                
                float fresnel = pow(1.0 - abs(dot(viewDir, normal)), 0.5);
                finalColor += fresnel * _WaterColor.rgb * 0.3;
                
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
        
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                V2F_SHADOW_CASTER;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i);
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
}