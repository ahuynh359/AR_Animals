
Shader "FX/JPWater" {
Properties {
	[HideInInspector] _ReflectionTex ("Internal Reflection", 2D) = "" {}
		
	_MainTex ("Fallback texture", 2D) = "black" {}
	_ShoreTex ("Shore & Foam texture ", 2D) = "black" {}
	_BumpMap ("Normals ", 2D) = "bump" {}
	
	_DistortParams ("Distortions (Bump waves, Reflection, Fresnel power, Fresnel bias)", Vector) = (1.0, 1.0, 2.0, 1.15)
	_InvFadeParemeter ("Auto blend parameter (Edge, Shore, Distance scale)", Vector) = (0.15, 0.15, 0.5, 1.0)
	
	_AnimationTiling ("Animation Tiling (Displacement)", Vector) = (2.2, 2.2, -1.1, -1.1)
	_AnimationDirection ("Animation Direction (displacement)", Vector) = (1.0, 1.0, 1.0, 1.0)

	_BumpTiling ("Bump Tiling", Vector) = (1.0, 1.0, -2.0, 3.0)
	_BumpDirection ("Bump Direction & Speed", Vector) = (1.0, 1.0, -1.0, 1.0)
	
	_FresnelScale ("FresnelScale", Range (0.15, 4.0)) = 0.75

	_BaseColor ("Base color", COLOR) = ( .54, .95, .99, 0.5)
	_ReflectionColor ("Reflection color", COLOR) = ( .54, .95, .99, 0.5)
	_SpecularColor ("Specular color", COLOR) = ( .72, .72, .72, 1)
	
	_DirectionalLightDir ("Specular light direction", Vector) = (0.0, 0.0, 0.0, 0.0)
	_Shininess ("Shininess", Range (1.0, 1000.0)) = 1000.0
	
	_Foam ("Foam (intensity, cutoff)", Vector) = (0.1, 0.375, 0.0, 0.0)
	
	_GerstnerIntensity("Per vertex displacement", Float) = 1.0
	_GAmplitude ("Wave Amplitude", Vector) = (0.3, 0.35, 0.25, 0.25)
	_GFrequency ("Wave Frequency", Vector) = (1.3, 1.35, 1.25, 1.25)
	_GSteepness ("Wave Steepness", Vector) = (1.0, 1.0, 1.0, 1.0)
	_GSpeed ("Wave Speed", Vector) = (1.2, 1.375, 1.1, 1.5)
	_GDirectionAB ("Wave Direction", Vector) = (0.3, 0.85, 0.85, 0.25)
	_GDirectionCD ("Wave Direction", Vector) = (0.1, 0.9, 0.5, 0.5)
}


CGINCLUDE

	#include "UnityCG.cginc"
	half _GerstnerIntensity;

inline half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength) 
{
	half3 bump = (UnpackNormal(tex2D(bumpMap, coords.xy)) + UnpackNormal(tex2D(bumpMap, coords.zw))) * 0.5;
	half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1, 0, 1);
	return normalize(worldNormal);
} 

inline half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength) 
{
	half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
	bump = bump * 0.5;
	half3 normal = UnpackNormal(bump);
	normal.xy *= bumpStrength;
	return normalize(normal);
} 

inline half3 GetNormal(half4 tf) {
	#ifdef WATER_VERTEX_DISPLACEMENT_ON
		return half3(2, 1, 2) * tf.rbg - half3(1, 0, 1);
	#else
		return half3(0, 1, 0);
	#endif	
}

inline half GetDistanceFadeout(half screenW, half speed) {
	return 1.0f / abs(0.5f + screenW * speed);	
}

half4 GetDisplacement3(half4 tileableUv, half4 tiling, half4 directionSpeed, sampler2D mapA, sampler2D mapB, sampler2D mapC)
{
	half4 displacementUv = tileableUv * tiling + _Time.xxxx * directionSpeed;
	#ifdef WATER_VERTEX_DISPLACEMENT_ON			
		half4 tf = tex2Dlod(mapA, half4(displacementUv.xy, 0.0, 0.0));
		tf += tex2Dlod(mapB, half4(displacementUv.zw, 0.0, 0.0));
		tf += tex2Dlod(mapC, half4(displacementUv.xw, 0.0, 0.0));
		tf *= 0.333333; 
	#else
		half4 tf = half4(0.5, 0.5, 0.5, 0.0);
	#endif
	
	return tf;
}

half4 GetDisplacement2(half4 tileableUv, half4 tiling, half4 directionSpeed, sampler2D mapA, sampler2D mapB)
{
	half4 displacementUv = tileableUv * tiling + _Time.xxxx * directionSpeed;
	#ifdef WATER_VERTEX_DISPLACEMENT_ON			
		half4 tf = tex2Dlod(mapA, half4(displacementUv.xy, 0.0, 0.0));
		tf += tex2Dlod(mapB, half4(displacementUv.zw, 0.0, 0.0));
		tf *= 0.5; 
	#else
		half4 tf = half4(0.5, 0.5, 0.5, 0.0);
	#endif
	
	return tf;
}

inline void ComputeScreenAndGrabPassPos (float4 pos, out float4 screenPos, out float4 grabPassPos) 
{
	#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
	#else
		float scale = 1.0f;
	#endif
	
	screenPos = ComputeScreenPos(pos); 
	grabPassPos.xy = ( float2( pos.x, pos.y*scale ) + pos.w ) * 0.5;
	grabPassPos.zw = pos.zw;
}


inline half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength, half2 perVertxOffset)
{
	half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
	bump = bump * 0.5;
	half3 normal = UnpackNormal(bump);
	normal.xy *= bumpStrength;
	normal.xy += perVertxOffset;
	return normalize(normal);	
}

inline half3 PerPixelNormalLite(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength) 
{
	half4 bump = tex2D(bumpMap, coords.xy);
	bump.xy = bump.wy - half2(0.5, 0.5);
	half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1, 0, 1);
	return normalize(worldNormal);
} 

inline half4 Foam(sampler2D shoreTex, half4 coords, half amount) 
{
	half4 foam = ( tex2D(shoreTex, coords.xy) * tex2D(shoreTex, coords.zw) ) - 0.125;
	foam.a = amount;
	return foam;
}

inline half4 Foam(sampler2D shoreTex, half4 coords) 
{
	half4 foam = (tex2D(shoreTex, coords.xy) * tex2D(shoreTex, coords.zw)) - 0.125;
	return foam;
}

inline half Fresnel(half3 viewVector, half3 worldNormal, half bias, half power)
{
	half facing = clamp(1.0-max(dot(-viewVector, worldNormal), 0.0), 0.0, 1.0);	
	half refl2Refr = saturate(bias+(1.0-bias) * pow(facing, power));	
	return refl2Refr;	
}

inline half FresnelViaTexture(half3 viewVector, half3 worldNormal, sampler2D fresnel)
{
	half facing = saturate(dot(-viewVector, worldNormal));	
	half fresn = tex2D(fresnel, half2(facing, 0.5f)).b;	
	return fresn;
}

inline void VertexDisplacementHQ(	sampler2D mapA, sampler2D mapB, 
									sampler2D mapC, half4 uv, 
									half vertexStrength, half3 normal, 
									out half4 vertexOffset, out half2 normalOffset) 
{	
	half4 tf = tex2Dlod(mapA, half4(uv.xy, 0.0, 0.0));
	tf += tex2Dlod(mapB, half4(uv.zw, 0.0, 0.0));
	tf += tex2Dlod(mapC, half4(uv.xw, 0.0, 0.0));
	tf /= 3.0; 
	
	tf.rga = tf.rga-half3(0.5, 0.5, 0.0);
				
	// height displacement in alpha channel, normals info in rgb
	
	vertexOffset = tf.a * half4(normal.xyz, 0.0) * vertexStrength;							
	normalOffset = tf.rg;
}

inline void VertexDisplacementLQ(	sampler2D mapA, sampler2D mapB, 
									sampler2D mapC, half4 uv, 
									half vertexStrength, half normalsStrength, 
									out half4 vertexOffset, out half2 normalOffset) 
{
	// @NOTE: for best performance, this should really be properly packed!
	
	half4 tf = tex2Dlod(mapA, half4(uv.xy, 0.0, 0.0));
	tf += tex2Dlod(mapB, half4(uv.zw, 0.0, 0.0));
	tf *= 0.5; 
	
	tf.rga = tf.rga-half3(0.5, 0.5, 0.0);
				
	// height displacement in alpha channel, normals info in rgb
	
	vertexOffset = tf.a * half4(0, 1, 0, 0) * vertexStrength;							
	normalOffset = tf.rg * normalsStrength;
}

half4 ExtinctColor (half4 baseColor, half extinctionAmount) 
{
	// tweak the extinction coefficient for different coloring
	 return baseColor - extinctionAmount * half4(0.15, 0.03, 0.01, 0.0);
}

	half3 GerstnerOffsets (half2 xzVtx, half steepness, half amp, half freq, half speed, half2 dir) 
	{
		half3 offsets;
		
		offsets.x =
			steepness * amp * dir.x *
			cos( freq * dot( dir, xzVtx ) + speed * _Time.x); 
			
		offsets.z =
			steepness * amp * dir.y *
			cos( freq * dot( dir, xzVtx ) + speed * _Time.x); 
			
		offsets.y = 
			amp * sin ( freq * dot( dir, xzVtx ) + speed * _Time.x);

		return offsets;			
	}	

	half3 GerstnerOffset4 (half2 xzVtx, half4 steepness, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) 
	{
		half3 offsets;
		
		half4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
		half4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;
		
		half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
		half4 TIME = _Time.yyyy * speed;
		
		half4 COS = cos (dotABCD + TIME);
		half4 SIN = sin (dotABCD + TIME);
		
		offsets.x = dot(COS, half4(AB.xz, CD.xz));
		offsets.z = dot(COS, half4(AB.yw, CD.yw));
		offsets.y = dot(SIN, amp);

		return offsets;			
	}	

	half3 GerstnerNormal (half2 xzVtx, half steepness, half amp, half freq, half speed, half2 dir) 
	{
		half3 nrml = half3(0, 0, 0);
		
		nrml.x -=
			dir.x * (amp * freq) * 
			cos(freq * dot( dir, xzVtx ) + speed * _Time.x);
			
		nrml.z -=
			dir.y * (amp * freq) * 
			cos(freq * dot( dir, xzVtx ) + speed * _Time.x);	

		return nrml;			
	}	
	
	half3 GerstnerNormal4 (half2 xzVtx, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) 
	{
		half3 nrml = half3(0, 2.0, 0);
		
		half4 AB = freq.xxyy * amp.xxyy * dirAB.xyzw;
		half4 CD = freq.zzww * amp.zzww * dirCD.xyzw;
		
		half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
		half4 TIME = _Time.yyyy * speed;
		
		half4 COS = cos (dotABCD + TIME);
		
		nrml.x -= dot(COS, half4(AB.xz, CD.xz));
		nrml.z -= dot(COS, half4(AB.yw, CD.yw));
		
		nrml.xz *= _GerstnerIntensity;
		nrml = normalize (nrml);

		return nrml;			
	}	
	
	void Gerstner (	out half3 offs, out half3 nrml, 
					 half3 vtx, half3 tileableVtx, 
					 half4 amplitude, half4 frequency, half4 steepness, 
					 half4 speed, half4 directionAB, half4 directionCD ) 
	{
		#ifdef WATER_VERTEX_DISPLACEMENT_ON
			offs = GerstnerOffset4(tileableVtx.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);
			nrml = GerstnerNormal4(tileableVtx.xz + offs.xz, amplitude, frequency, speed, directionAB, directionCD);		
		#else
			offs = half3(0, 0, 0);
			nrml = half3(0, 1, 0);
		#endif							
	}

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	// interpolator structs
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 normalInterpolator : TEXCOORD0;
		float4 viewInterpolator : TEXCOORD1;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		float4 grabPassPos : TEXCOORD4;
		UNITY_FOG_COORDS(5)
	};

	struct v2f_noGrab
	{
		float4 pos : SV_POSITION;
		float4 normalInterpolator : TEXCOORD0;
		float3 viewInterpolator : TEXCOORD1;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		UNITY_FOG_COORDS(4)
	};
	
	struct v2f_simple
	{
		float4 pos : SV_POSITION;
		float4 viewInterpolator : TEXCOORD0;
		float4 bumpCoords : TEXCOORD1;
		UNITY_FOG_COORDS(2)
	};

	// textures
	sampler2D _BumpMap;
	sampler2D _ReflectionTex;
	sampler2D _RefractionTex;
	sampler2D _ShoreTex;
	sampler2D_float _CameraDepthTexture;

	// colors in use
	uniform float4 _RefrColorDepth;
	uniform float4 _SpecularColor;
	uniform float4 _BaseColor;
	uniform float4 _ReflectionColor;
	
	// edge & shore fading
	uniform float4 _InvFadeParemeter;

	// specularity
	uniform float _Shininess;
	uniform float4 _DirectionalLightDir;

	// fresnel, vertex & bump displacements & strength
	uniform float4 _DistortParams;
	uniform float _FresnelScale;
	uniform float4 _BumpTiling;
	uniform float4 _BumpDirection;

	uniform float4 _GAmplitude;
	uniform float4 _GFrequency;
	uniform float4 _GSteepness;
	uniform float4 _GSpeed;
	uniform float4 _GDirectionAB;
	uniform float4 _GDirectionCD;
	
	// foam
	uniform float4 _Foam;
	
	// shortcuts
	#define PER_PIXEL_DISPLACE _DistortParams.x
	#define REALTIME_DISTORTION _DistortParams.y
	#define FRESNEL_POWER _DistortParams.z
	#define VERTEX_WORLD_NORMAL i.normalInterpolator.xyz
	#define FRESNEL_BIAS _DistortParams.w
	#define NORMAL_DISPLACEMENT_PER_VERTEX _InvFadeParemeter.z
	
	//
	// HQ VERSION
	//
	
	v2f vert(appdata_full v)
	{
		v2f o;
		
		half3 worldSpaceVertex = mul(unity_ObjectToWorld, (v.vertex)).xyz;
		half3 vtxForAni = (worldSpaceVertex).xzz;

		half3 nrml;
		half3 offsets;
		Gerstner (
			offsets, nrml, v.vertex.xyz, vtxForAni, 						// offsets, nrml will be written
			_GAmplitude, 												// amplitude
			_GFrequency, 												// frequency
			_GSteepness, 												// steepness
			_GSpeed, 													// speed
			_GDirectionAB, 												// direction # 1, 2
			_GDirectionCD												// direction # 3, 4
		);
		
		v.vertex.xyz += offsets;
		
		// one can also use worldSpaceVertex.xz here (speed!), albeit it'll end up a little skewed
		half2 tileableUv = mul(unity_ObjectToWorld, (v.vertex)).xz;
		
		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex - _WorldSpaceCameraPos;

		o.pos = UnityObjectToClipPos(v.vertex);

		ComputeScreenAndGrabPassPos(o.pos, o.screenPos, o.grabPassPos);
		
		o.normalInterpolator.xyz = nrml;
		
		o.viewInterpolator.w = saturate(offsets.y);
		o.normalInterpolator.w = 1;//GetDistanceFadeout(o.screenPos.w, DISTANCE_SCALE);
		
		UNITY_TRANSFER_FOG(o, o.pos);
		return o;
	}

	half4 frag( v2f i ) : SV_Target
	{
		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, VERTEX_WORLD_NORMAL, PER_PIXEL_DISPLACE);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half4 distortOffset = half4(worldNormal.xz * REALTIME_DISTORTION * 10.0, 0, 0);
		half4 screenWithOffset = i.screenPos + distortOffset;
		half4 grabWithOffset = i.grabPassPos + distortOffset;
		
		half4 rtRefractionsNoDistort = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(i.grabPassPos));
		half refrFix = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(grabWithOffset));
		half4 rtRefractions = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(grabWithOffset));
		
		#ifdef WATER_REFLECTIVE
			half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(screenWithOffset));
		#endif

		#ifdef WATER_EDGEBLEND_ON
		if(LinearEyeDepth(refrFix) < i.screenPos.z)
			rtRefractions = rtRefractionsNoDistort;
		#endif
		
		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize ((_DirectionalLightDir.xyz) + viewVector.xyz);
		float nh = max (0, dot (worldNormal, -h));
		float spec = max(0.0, pow (nh, _Shininess));
		
		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
		
		#ifdef WATER_EDGEBLEND_ON
			half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
			depth = LinearEyeDepth(depth);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.w));
			edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
		#endif
		
		// shading for fresnel term
		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);
		
		// base, depth & reflection colors
		half4 baseColor = ExtinctColor (_BaseColor, i.viewInterpolator.w * _InvFadeParemeter.w);
		#ifdef WATER_REFLECTIVE
			half4 reflectionColor = lerp (rtReflections, _ReflectionColor, _ReflectionColor.a);
		#else
			half4 reflectionColor = _ReflectionColor;
		#endif
		
		baseColor = lerp (lerp (rtRefractions, baseColor, baseColor.a), reflectionColor, refl2Refr);
		baseColor = baseColor + spec * (_SpecularColor*_SpecularColor.a);
		
		// handle foam
		half4 foam = Foam(_ShoreTex, i.bumpCoords * 2.0);
		baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y));
		
		baseColor.a = edgeBlendFactors.x;
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}
	
	//
	// MQ VERSION
	//
	
	v2f_noGrab vert300(appdata_full v)
	{
		v2f_noGrab o;
		
		half3 worldSpaceVertex = mul(unity_ObjectToWorld, (v.vertex)).xyz;
		half3 vtxForAni = (worldSpaceVertex).xzz;

		half3 nrml;
		half3 offsets;
		Gerstner (
			offsets, nrml, v.vertex.xyz, vtxForAni, 						// offsets, nrml will be written
			_GAmplitude, 												// amplitude
			_GFrequency, 												// frequency
			_GSteepness, 												// steepness
			_GSpeed, 													// speed
			_GDirectionAB, 												// direction # 1, 2
			_GDirectionCD												// direction # 3, 4
		);
		
		v.vertex.xyz += offsets;
		
		// one can also use worldSpaceVertex.xz here (speed!), albeit it'll end up a little skewed
		half2 tileableUv = mul(unity_ObjectToWorld, v.vertex).xz;
		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex - _WorldSpaceCameraPos;

		o.pos = UnityObjectToClipPos(v.vertex);

		o.screenPos = ComputeScreenPos(o.pos);
		
		o.normalInterpolator.xyz = nrml;
		o.normalInterpolator.w = 1;//GetDistanceFadeout(o.screenPos.w, DISTANCE_SCALE);
		
		UNITY_TRANSFER_FOG(o, o.pos);
		return o;
	}

	half4 frag300( v2f_noGrab i ) : SV_Target
	{
		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, normalize(VERTEX_WORLD_NORMAL), PER_PIXEL_DISPLACE);

		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half4 distortOffset = half4(worldNormal.xz * REALTIME_DISTORTION * 10.0, 0, 0);
		half4 screenWithOffset = i.screenPos + distortOffset;
		
		#ifdef WATER_REFLECTIVE
			half4 rtReflections = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(screenWithOffset));
		#endif
		
		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize (_DirectionalLightDir.xyz + viewVector.xyz);
		float nh = max (0, dot (worldNormal, -h));
		float spec = max(0.0, pow (nh, _Shininess));
		
		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
		
		#ifdef WATER_EDGEBLEND_ON
			half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
			depth = LinearEyeDepth(depth);
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.z));
			edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
		#endif
		
		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);
		
		half4 baseColor = _BaseColor;
		#ifdef WATER_REFLECTIVE
			baseColor = lerp (baseColor, lerp (rtReflections, _ReflectionColor, _ReflectionColor.a), saturate(refl2Refr * 2.0));
		#else
			baseColor = lerp (baseColor, _ReflectionColor, saturate(refl2Refr * 2.0));
		#endif
		
		baseColor = baseColor + spec * _SpecularColor;
		
		baseColor.a = edgeBlendFactors.x * saturate(0.5 + refl2Refr * 1.0);
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}
	
	//
	// LQ VERSION
	//
	
	v2f_simple vert200(appdata_full v)
	{
		v2f_simple o;
		
		half3 worldSpaceVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
		half2 tileableUv = worldSpaceVertex.xz;

		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex-_WorldSpaceCameraPos;
		
		o.pos = UnityObjectToClipPos(v.vertex);
		
		o.viewInterpolator.w = 1;//GetDistanceFadeout(ComputeScreenPos(o.pos).w, DISTANCE_SCALE);
		
		UNITY_TRANSFER_FOG(o, o.pos);
		return o;

	}

	half4 frag200( v2f_simple i ) : SV_Target
	{
		half3 worldNormal = PerPixelNormal(_BumpMap, i.bumpCoords, half3(0, 1, 0), PER_PIXEL_DISPLACE);
		half3 viewVector = normalize(i.viewInterpolator.xyz);

		half3 reflectVector = normalize(reflect(viewVector, worldNormal));
		half3 h = normalize ((_DirectionalLightDir.xyz) + viewVector.xyz);
		float nh = max (0, dot (worldNormal, -h));
		float spec = max(0.0, pow (nh, _Shininess));

		worldNormal.xz *= _FresnelScale;
		half refl2Refr = Fresnel(viewVector, worldNormal, FRESNEL_BIAS, FRESNEL_POWER);

		half4 baseColor = _BaseColor;
		baseColor = lerp(baseColor, _ReflectionColor, saturate(refl2Refr * 2.0));
		baseColor.a = saturate(2.0 * refl2Refr + 0.5);

		baseColor.rgb += spec * _SpecularColor.rgb;
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}
	
ENDCG

Subshader
{
	Tags {"RenderType"="Transparent"}
	Lod 500
	
	ColorMask RGB

	GrabPass { "_RefractionTex" }

	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite On
			Cull Off

			CGPROGRAM
			#pragma target 3.0
		
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
		
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
			#pragma multi_compile WATER_REFLECTIVE WATER_SIMPLE
		
			ENDCG
	}
}
Subshader
{
	Tags {"RenderType"="Transparent"}
	
	Lod 300
	ColorMask RGB
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite On
			Cull Off
		
			CGPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert300
			#pragma fragment frag300
			#pragma multi_compile_fog
		
			#pragma multi_compile WATER_VERTEX_DISPLACEMENT_ON WATER_VERTEX_DISPLACEMENT_OFF
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF
			#pragma multi_compile WATER_REFLECTIVE WATER_SIMPLE
		
			ENDCG
	}
}
Subshader
{
	Tags {"RenderType"="Transparent"}
	
	Lod 200
	ColorMask RGB
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite On
			Cull Off
		
			CGPROGRAM
		
			#pragma vertex vert200
			#pragma fragment frag200
			#pragma multi_compile_fog
		
			ENDCG
	}
}

Fallback "Transparent/Diffuse"
}
