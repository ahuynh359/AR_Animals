//using System;
using UnityEngine;
using System.Collections.Generic;


public enum WaterQuality { High = 2, Medium = 1, Low = 0, }

[ExecuteInEditMode]
public class JPWater : MonoBehaviour
{
	[SerializeField]  Material WaterMaterial;
	[SerializeField]  WaterQuality WaterQuality = WaterQuality.High;
	[SerializeField]  bool EdgeBlend = true;
	[SerializeField]  bool GerstnerDisplace = true;
	[SerializeField]  bool DisablePixelLights = true;
	[SerializeField]  int ReflectionSize = 256;
	[SerializeField]  float ClipPlaneOffset = 0.07f;
	[SerializeField]  LayerMask ReflectLayers = -1;
	[SerializeField]  Light DirectionalLight;

	Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
	RenderTexture m_ReflectionTexture;
	int m_OldReflectionTextureSize;
	static bool s_InsideWater;

	[Header("UNDERWATER EFFECT")]
	[SerializeField] bool UnderwaterEffect=true;
	[SerializeField] bool ScreenOverlayFX=true;
	[SerializeField] AudioClip Underwater;
	[SerializeField] Texture[] LightCookie;
	Vector3 defaultLightDir;
	Color32 defaultFogColor;
	float defaultFogStart;
	float defaultFogEnd;
	Color32 underwaterColor;
	[SerializeField]  float UnderwaterFogStart = 0.0f;
	[SerializeField]  float UnderwaterFogEnd = 100.0f;
	float screenWaterY;
	int i=0, j=0; //Used for light cookie animation
	bool enableFX=false;
	[Header("WATER PARTICLES FX")]
	[SerializeField] bool ParticlesEffect=true;
	[SerializeField]  ParticleSystem ripples;
	[SerializeField]  ParticleSystem splash;
	public AudioClip Largesplash;
	float count =0;

	void Update()
	{
		if(WaterMaterial)
		{
			if(WaterQuality > WaterQuality.Medium) WaterMaterial.shader.maximumLOD = 501;
			else if(WaterQuality > WaterQuality.Low) WaterMaterial.shader.maximumLOD = 301;
			else WaterMaterial.shader.maximumLOD = 201;
			
			if(DirectionalLight) WaterMaterial.SetVector("_DirectionalLightDir", DirectionalLight.transform.forward);
	
			if(!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) | !EdgeBlend)
			{
				Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
				Shader.DisableKeyword("WATER_EDGEBLEND_ON");
			}
			else
			{
				Shader.EnableKeyword("WATER_EDGEBLEND_ON");
				Shader.DisableKeyword("WATER_EDGEBLEND_OFF");
				// just to make sure (some peeps might forget to add a water tile to the patches)
				if(Camera.main) Camera.main.depthTextureMode |= DepthTextureMode.Depth;
			}
			
			if(GerstnerDisplace)
			{
				Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
				Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
			}
			else
			{
				Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
				Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
			}
		}
	}

	// This is called when it's known that the object will be rendered by some
	// camera. We render reflections and do other updates here.
	// Because the script executes in edit mode, reflections for the scene view
	// camera will just work!
	void OnWillRenderObject()
	{
		Camera cam = Camera.current;
		if(!WaterMaterial | !cam | s_InsideWater) return;
		// Safeguard from recursive water reflections.
		s_InsideWater = true;
		
		Camera reflectionCamera;
		CreateWaterObjects(cam, out reflectionCamera);
		
		// find out the reflection plane: position and normal in world space
		Vector3 pos = transform.position;
		Vector3 normal = transform.up;
		
		// Optionally disable pixel lights for reflection
		int oldPixelLightCount = QualitySettings.pixelLightCount;
		if(DisablePixelLights) QualitySettings.pixelLightCount = 0;
		
		UpdateCameraModes(cam, reflectionCamera);
		
		// Reflect camera around reflection plane
		float d = -Vector3.Dot(normal, pos) - ClipPlaneOffset;
		Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
		
		Matrix4x4 reflection = Matrix4x4.zero;
		CalculateReflectionMatrix(ref reflection, reflectionPlane);
		Vector3 oldpos = cam.transform.position;
		Vector3 newpos = reflection.MultiplyPoint(oldpos);
		reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
		
		// Setup oblique projection matrix so that near plane is our reflection
		// plane. This way we clip everything below/above it for free.
		Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
		reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
		
		reflectionCamera.cullingMask = ~(1 << 4) & ReflectLayers.value; // never render water layer
		reflectionCamera.targetTexture = m_ReflectionTexture;
		GL.invertCulling = true;
		reflectionCamera.transform.position = newpos;
		Vector3 euler = cam.transform.eulerAngles;
		reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
	
		reflectionCamera.Render();
		reflectionCamera.transform.position = oldpos;
		GL.invertCulling = false;
		GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture);
		
		// Restore pixel light count
		if(DisablePixelLights) QualitySettings.pixelLightCount = oldPixelLightCount;
		s_InsideWater = false;
	}

	// Cleanup all the objects we possibly have created
	void OnDisable()
	{
		if(m_ReflectionTexture)
		{
			DestroyImmediate(m_ReflectionTexture);
			m_ReflectionTexture = null;
		}
		
		foreach (var kvp in m_ReflectionCameras)
		{
			DestroyImmediate((kvp.Value).gameObject);
		}
		m_ReflectionCameras.Clear();
	}

	void UpdateCameraModes(Camera src, Camera dest)
	{
		if(dest == null) return;

		// set water camera to clear the same way as current camera
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;
		

		// update other values to match current camera.
		// even ifwe are supplying custom camera&projection matrices, 
		// some of values are used elsewhere (e.g. skybox uses far plane)
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}
	
	// On-demand create any objects we need for water
	void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera)
	{
		reflectionCamera = null;
			// Reflection render texture
			if(!m_ReflectionTexture | m_OldReflectionTextureSize != ReflectionSize)
			{
				if(m_ReflectionTexture)
				{
					DestroyImmediate(m_ReflectionTexture);
				}
				m_ReflectionTexture = new RenderTexture(ReflectionSize, ReflectionSize, 16);
				m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
				m_ReflectionTexture.isPowerOfTwo = true;
				m_ReflectionTexture.hideFlags = HideFlags.DontSave;
				m_OldReflectionTextureSize = ReflectionSize;
			}
			// Camera for reflection
			m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
			if(!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
			{
				GameObject go = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
				reflectionCamera = go.GetComponent<Camera>();
				reflectionCamera.enabled = false;
				reflectionCamera.transform.position = transform.position;
				reflectionCamera.transform.rotation = transform.rotation;
				reflectionCamera.gameObject.AddComponent<FlareLayer>();
				go.hideFlags = HideFlags.HideAndDontSave;
				m_ReflectionCameras[currentCamera] = reflectionCamera;
			}
	}

	// Given position/normal of the plane, calculates plane in camera space.
	Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * ClipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint(offsetPos);
		Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
	}
	
	// Calculates reflection matrix around the given plane
	static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
		reflectionMat.m01 = (- 2F * plane[0] * plane[1]);
		reflectionMat.m02 = (- 2F * plane[0] * plane[2]);
		reflectionMat.m03 = (- 2F * plane[3] * plane[0]);
		
		reflectionMat.m10 = (- 2F * plane[1] * plane[0]);
		reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
		reflectionMat.m12 = (- 2F * plane[1] * plane[2]);
		reflectionMat.m13 = (- 2F * plane[3] * plane[1]);
		
		reflectionMat.m20 = (- 2F * plane[2] * plane[0]);
		reflectionMat.m21 = (- 2F * plane[2] * plane[1]);
		reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
		reflectionMat.m23 = (- 2F * plane[3] * plane[2]);
		
		reflectionMat.m30 = 0F;
		reflectionMat.m31 = 0F;
		reflectionMat.m32 = 0F;
		reflectionMat.m33 = 1F;
	}


//*************************************************************************************************************************************************
//UNDERWATER & PARTICLES EFFECT 

	void OnGUI ()
	{
		if(!Application.isPlaying | !UnderwaterEffect | !ScreenOverlayFX | !enableFX) return;

		//Screen overlay water FX
		if(screenWaterY>0.0f)
		{
			Camera cam =Camera.main; if(!cam) return;
			Texture2D pic= new Texture2D (2, 2, TextureFormat.ARGB32, false);
			Color UnderWaterScreen = underwaterColor;
			UnderWaterScreen.a=Mathf.Clamp(1.0f-(screenWaterY-1.0f), 0.0f, 0.99f);
			Color[] ScreenCol= new Color[] {UnderWaterScreen, UnderWaterScreen, UnderWaterScreen, UnderWaterScreen};
			pic.SetPixels(ScreenCol);

			GUI.depth=100;
			GUI.DrawTexture(new Rect(0, Screen.height, Screen.width, Screen.height*-screenWaterY), pic);
		}
	}

	void LateUpdate()
	{
		if(!Application.isPlaying | !UnderwaterEffect) return;

		Camera cam =Camera.main; if(!cam) return;
		float d_l = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane)).y;
		float u_l = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, cam.nearClipPlane)).y;
		float d_r = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, cam.nearClipPlane)).y;
		float u_r = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane)).y;
		screenWaterY = Mathf.Clamp( (Mathf.Min(d_l, d_r)-transform.position.y) / (Mathf.Min(d_l, d_r) - Mathf.Min(u_l, u_r)) , -10.0f, 10.0f);


		if(!enableFX)
		{
			if(WaterMaterial)
			{
				defaultLightDir=DirectionalLight.transform.forward;
				defaultFogColor=RenderSettings.fogColor;
				defaultFogStart=RenderSettings.fogStartDistance;
				defaultFogEnd=RenderSettings.fogEndDistance;
				underwaterColor= WaterMaterial.GetColor("_BaseColor");
				enableFX=true;
			}
		}
		else
		{
			if(screenWaterY>1.0f)
			{
				//Play water sound FX
				if(cam.transform.root.GetComponent<AudioSource>())
				{
					cam.transform.root.GetComponent<AudioSource>().clip=Underwater;
					if(cam.transform.root.GetComponent<AudioSource>().isPlaying)
					{ cam.transform.root.GetComponent<AudioSource>().volume =0.5f; cam.transform.root.GetComponent<AudioSource>().pitch=0.5f; } 
					else cam.transform.root.GetComponent<AudioSource>().PlayOneShot(Underwater);
				}

				//Disable flare layer
				if(cam.GetComponent<FlareLayer>()) cam.GetComponent<FlareLayer>().enabled = false; 

				// Setup fog
				RenderSettings.fogColor = Color32.Lerp(RenderSettings.fogColor, underwaterColor, 1.0f);
				RenderSettings.fogStartDistance = UnderwaterFogStart;
				RenderSettings.fogEndDistance = UnderwaterFogEnd;

				//Setup camera background
				cam.clearFlags=CameraClearFlags.SolidColor;
				cam.backgroundColor = underwaterColor;

				//Animate light cookie and rotate light
				if(DirectionalLight && LightCookie.Length>0)
				{
					if(DirectionalLight.transform.forward==defaultLightDir) DirectionalLight.transform.forward=-Vector3.up;
					if(j>2) { if(i==LightCookie.Length) i=0; DirectionalLight.cookie=LightCookie[i]; i++; j=0; } else j++; 
				}
			}

			else if(screenWaterY<=1.0f)
			{
				//Stop water sound FX
				if(cam.transform.root.GetComponent<AudioSource>() && cam.transform.root.GetComponent<AudioSource>().clip==Underwater)
				cam.transform.root.GetComponent<AudioSource>().clip=null;

				//Enable flare layer
				if(cam.GetComponent<FlareLayer>()) cam.GetComponent<FlareLayer>().enabled = true;
			
				// Restore fog
				RenderSettings.fogColor = Color32.Lerp(RenderSettings.fogColor, defaultFogColor, 1.0f);
				RenderSettings.fogStartDistance = defaultFogStart;
				RenderSettings.fogEndDistance = defaultFogEnd; 

				//Restore camera background
				cam.clearFlags=CameraClearFlags.Skybox;
				cam.backgroundColor = defaultFogColor;

				//Disable light cookie and rotate light
				if(DirectionalLight && LightCookie.Length>0)
				{
					if(DirectionalLight.transform.forward==-Vector3.up) DirectionalLight.transform.forward=defaultLightDir;
					Texture blank=null; if(DirectionalLight.cookie) DirectionalLight.cookie =blank;
				}
			}
		}

	}

	void OnTriggerStay(Collider col) { if(!ParticlesEffect) return; WaterParticleFX(col, ripples); }
	void OnTriggerExit(Collider col) { if(!ParticlesEffect) return; WaterParticleFX(col, splash); }
	void OnTriggerEnter(Collider col) { if(ParticlesEffect) WaterParticleFX(col, splash); }

	//Spawn water particle FX
	void WaterParticleFX(Collider col, ParticleSystem particleFx)
	{
		count+=Time.fixedDeltaTime;
		ParticleSystem particle=null; shared creatureScript=null;

		//Check if object has a Rigidbody component
		if(col.transform.root.GetComponent<Rigidbody>())
		{
			//Check his tag (must be a JP Creature with "shared" script attached)
			if(col.transform.root.tag == "Creature")
			{
				creatureScript=col.transform.root.GetComponent<shared>(); //Get creature script
				creatureScript.waterY=transform.position.y; //Set creature current water layer altitude
				if(!creatureScript.IsVisible) return; //Check if creature is visible
				if(particleFx==ripples && count<creatureScript.loop%5) return; //prevent particle overflow
				SkinnedMeshRenderer rend =  creatureScript.rend[0];

				//Check if the creature bounds are in contact with the water surface
				if(rend.bounds.Contains(new Vector3(col.transform.position.x, transform.position.y, col.transform.position.z)))
				{
					//Check if the creature rigidbody are in motion
					if(!creatureScript.anm.GetInteger("Move").Equals(0) |
						(creatureScript.CanFly && creatureScript.OnLevitation) | creatureScript.IsJumping | creatureScript.IsAttacking)
					{
						if(particleFx==splash && (!creatureScript.IsOnGround | creatureScript.IsJumping) )
						{
							col.transform.root.GetComponents<AudioSource>()[1].pitch=Random.Range(0.5f, 0.75f); 
							col.transform.root.GetComponents<AudioSource>()[1].PlayOneShot(Largesplash, Random.Range(0.5f, 0.75f));
						} else particleFx=ripples;
					} else return;

					//The spawn position
					Vector2 pos=new Vector2(rend.bounds.center.x, rend.bounds.center.z);

					//Spawn the particle prefab
					particle=Instantiate(particleFx, new Vector3(pos.x, transform.position.y+0.01f, pos.y), Quaternion.Euler(-90, 0, 0)) as ParticleSystem;
					//Set particle size relative to creature size x
					float size=rend.bounds.size.magnitude/10;
					//particle.transform.localScale=new Vector3(size,size, size);
					particle.transform.localScale=new Vector3(size,size, size);

					//Destroy particle after 3 sec
					DestroyObject(particle.gameObject, 3.0f);
					count=0;
				}
			}
		}
	}

}

