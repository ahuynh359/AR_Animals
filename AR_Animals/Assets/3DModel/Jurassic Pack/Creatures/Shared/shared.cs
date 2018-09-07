using UnityEngine;
using System.Collections.Generic;

//*************************************************************************************************************************************************
//SHARED CREATURES STUFF
//*************************************************************************************************************************************************

public class shared : MonoBehaviour 
{
	manager manager=null;
	[Header("RESOURCES")]
	public ParticleSystem blood;
	public Texture[] skin, eyes;
	public enum skinselect {SkinA, SkinB, SkinC};
	public enum eyesselect {Type0, Type1, Type2, Type3, Type4, Type5, Type6, Type7, Type8, Type9, Type10, Type11, Type12, Type13, Type14, Type15};

	[Space (10)] [Header("SETTINGS")]
	public skinselect BodySkin;
	public eyesselect EyesSkin;
	[Range(0.1f, 2.0f)] public float AnimSpeed=1.0f;
	[Range(0.0f, 100.0f)] public float Health=100;
	[Range(0.0f, 100.0f)] public float Water=100;
	[Range(0.0f, 100.0f)] public float Food=100;
	[Range(0.0f, 100.0f)] public float Fatigue=100;
	[Range(1, 10)] public float DamageMultiplier;
	[Range(1, 10)] public float ArmorMultiplier;

	[Space (10)] [Header("ARTIFICIAL INTELLIGENCE")]
	public bool AI=false;
	const string PathHelp=
	"Use gameobjects as waypoints to define a path for this creature by \n"+
	"taking into account the priority between autonomous AI and its path.";
	const string WaypointHelp=
	"Place your waypoint gameobject in a reacheable position.\n"+
	"Don't put a waypoint in air if the creature are not able to fly";
	const string PriorityPathHelp=
	"Using a priority of 100% will disable all autonomous AI for this waypoint\n"+
	"Obstacle avoid AI and custom targets search still enabled";
	const string TargetHelp=
	"Use gameobjects to assign a custom enemy/friend for this creature\n"+
	"Can be any kind of gameobject e.g : player, other creature.\n"+
	"The creature will include friend/enemy goals in its search. \n"+
	"Enemy: triggered if the target is in range. \n"+
	"Friend: triggered when the target moves away.";
	const string MaxRangeHelp=
	"If MaxRange is zero, range is infinite. \n"+
	"Creature will start his attack/tracking once in range.";
	//Path editor
	[Space (10)] [Tooltip(PathHelp)] public List<_PathEditor> PathEditor;
	[HideInInspector] public int nextPath=0;
	[HideInInspector] public enum PathType { Walk, Run };
	[System.Serializable] public struct _PathEditor
	{
		[Tooltip(WaypointHelp)] public GameObject _Waypoint;
		public PathType _PathType;
		[Tooltip(PriorityPathHelp)] [Range(1, 100)] public int Priority;
	}
	//Target editor
	[Space (10)] [Tooltip(TargetHelp)]  public List< _TargetEditor> TargetEditor;
	[HideInInspector] public enum TargetType { Enemy, Friend };
	[System.Serializable] public struct _TargetEditor
	{
		public GameObject _GameObject;
		public TargetType _TargetType;
		[Tooltip(MaxRangeHelp)]
		public int MaxRange;
	}

	[HideInInspector] public LODGroup lod;
	[HideInInspector] public Rigidbody body;
	[HideInInspector] public Animator anm;
	[HideInInspector] public SkinnedMeshRenderer[] rend;
	[HideInInspector] public bool IsActive, IsVisible, IsDead, IsOnGround, IsOnWater, IsInWater, IsConstrained, OnLevitation, CanAttack, CanFly, CanSwim, CanJump, IsCrouching, IsJumping, IsAttacking, HasSideAttack;
	[HideInInspector] public float currframe, lastframe, lastHit, delta, spineX_T, spineY_T, crouch_T, crouch_max, yaw_max, pitch_max, ang_t;
	[HideInInspector] public float  posY, terrainY, waterY=-65536, behaviorCount, rndAngle, withersSize;
	[HideInInspector] public int rndMove, rndIdle, loop;
	[HideInInspector] public string behavior, regime, specie;
	[HideInInspector] public GameObject objectTGT=null, lookTGT=null, objectCOL=null;
	[HideInInspector] public Vector3 vectorTGT=Vector3.zero, fixedHeadPos=Vector3.zero, scale=Vector3.zero;
	[HideInInspector] const int enemyMaxRange=50, waterMaxRange=200, foodMaxRange=200, friendMaxRange=200, preyMaxRange=200;

//***********************************************************************************************************************************************************************************************************
//STARTUP VALUES
	void Start()
	{
		manager=Camera.main.GetComponent<manager>();
		regime=transform.GetChild(0).tag;
		specie=transform.GetChild(0).name;
		lod=GetComponent<LODGroup>();
		anm=GetComponent<Animator>();
		body=GetComponent<Rigidbody>();
		body.maxDepenetrationVelocity=1.0f;
		rend=GetComponentsInChildren<SkinnedMeshRenderer>();
		SetScale(transform.localScale.x);
		SetMaterials(BodySkin.GetHashCode(), EyesSkin.GetHashCode());
		loop=Random.Range(0, 100);
		if(anm.parameters[0].name=="Attack") CanAttack=true;
		if(anm.parameters[1].name.Equals("Pitch")) CanFly=true;
		else if(anm.parameters[2].name.Equals("Pitch")) CanSwim=true;
		else if(anm.parameters[1].name.Equals("OnGround")) CanJump=true;
	}
//***********************************************************************************************************************************************************************************************************
//ENABLE / DISABLE AI
	public void SetAI(bool UseAI) { AI=UseAI; if(!AI) { vectorTGT=Vector3.zero; objectTGT=null; objectCOL=null; behaviorCount=0; } }
//***********************************************************************************************************************************************************************************************************
//CHANGE SKIN
	#ifUNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if(rend.Length==0 | rend[0]==null) rend=GetComponentsInChildren<SkinnedMeshRenderer>();

		if(rend[0].sharedMaterials[0].mainTexture!=skin[BodySkin.GetHashCode()] |
			rend[0].sharedMaterials[1].mainTexture!=eyes[EyesSkin.GetHashCode()])
		{ 
			foreach (SkinnedMeshRenderer element in rend)
			{
				Material a = element.sharedMaterial; element.sharedMaterial=new Material(a);
				element.sharedMaterials[0].mainTexture=skin[BodySkin.GetHashCode()];
				if(element.sharedMaterials.Length>1) element.sharedMaterials[1].mainTexture = eyes[EyesSkin.GetHashCode()];
			}
		}
	}
	#endif
	public void SetMaterials(int bodyindex, int eyesindex)
	{
		BodySkin= (skinselect) bodyindex; EyesSkin= (eyesselect) eyesindex;
		foreach (SkinnedMeshRenderer element in rend)
		{
			element.materials[0].mainTexture = skin[bodyindex];
			if(element.materials.Length>1) element.materials[1].mainTexture = eyes[eyesindex];
		}
	}
//***********************************************************************************************************************************************************************************************************
//SET SCALE
	public void SetScale(float resize)
	{
		transform.localScale=new Vector3(resize, resize, resize); //creature size
		withersSize = (transform.GetChild(0).GetChild(0).position-transform.position).magnitude; //At the withers altitude
		scale = rend[0].bounds.extents; //bounding box scale
	}
//***********************************************************************************************************************************************************************************************************
//COUNTERS
	void Update()
	{
		//Is creature currently visible or selected by manager ?
		if(transform.gameObject==manager.creaturesList[manager.selected].gameObject | rend[0].isVisible | rend[1].isVisible | rend[2].isVisible |
			(Camera.main.transform.position-transform.position).magnitude< 100)
		{ IsVisible=true; IsActive=true; }
		else
		{ 
			IsVisible=false;
			//Realtime game ? If not, turn off creature activity
			if(!manager.RealtimeGame) { IsActive=false; anm.cullingMode=AnimatorCullingMode.CullCompletely; return; } else
			{ IsActive=true; anm.cullingMode=AnimatorCullingMode.CullUpdateTransforms; }
		}

		//Get current animation frame
		if(currframe==15f | anm.GetAnimatorTransitionInfo(0).normalizedTime>0.5) { currframe=0.0f; lastframe=-1; }
		else currframe = Mathf.Round((anm.GetCurrentAnimatorStateInfo (0).normalizedTime % 1.0f) * 15f);

		//Manage health bar
		if(Health>0)
		{
			if(loop>=100)	
			{
				if((Water==0 | Food==0 | Fatigue==0)) Health-=0.1f; //decrease health
				else if(Health<100) Health+=0.1f; 
				if(anm.GetInteger("Move")!=0) //decrease needs
				{ 
					Food-=Random.Range(0.0f, 0.5f);
					if(!CanSwim) { Fatigue-=Random.Range(0.0f, 0.5f); Water-=Random.Range(0.0f, 0.5f); }
					else { Fatigue=100; Water=100; }
				}
				loop=0;
			} else loop ++;
		}
		else
		{
			Water=0; Food=0; Fatigue=0; behavior="Dead";
			if(behaviorCount>0) behaviorCount=0;
			else if(behaviorCount==-5000)
			{
				//Delete from list and destroy gameobject
				if(manager.selected>=manager.creaturesList.IndexOf(transform.gameObject)) { if(manager.selected>0) manager.selected--; }
				manager.creaturesList.Remove(transform.gameObject); Destroy(transform.gameObject);
			}
			else behaviorCount--;
		}

		//Clamp all parameters
		Health=Mathf.Clamp(Health, 0, 100); Water=Mathf.Clamp(Water, 0, 100); Food=Mathf.Clamp(Food, 0, 100); Fatigue=Mathf.Clamp(Fatigue, 0, 100);
	}

///***********************************************************************************************************************************************************************************************************
// KEYBOARD / MOUSE AND JOYSTICK INPUT (MANAGER ONLY) (allow to control all JP creatures)
	public void GetUserInputs(int idle1=0, int idle2=0, int idle3=0, int idle4=0, int eat=0, int drink=0, int sleep=0, int rise=0)
	{
		if(behavior=="Sleep" && anm.GetInteger("Move")!=0) behavior="Player";
		else if(behaviorCount<=0) { objectTGT=null; behavior="Player"; behaviorCount=0; } else behaviorCount--;

		// Current camera manager target ?
		if(manager.UseManager && transform.gameObject==manager.creaturesList[manager.selected].gameObject && manager.CameraMode!=0)
		{
			//Run key
			bool run; if(Input.GetKey(KeyCode.LeftShift)) run= true; else run=false;

			//Attack key
			if(CanAttack) { if(Input.GetKey(KeyCode.Mouse0)) anm.SetBool ("Attack", true); else anm.SetBool ("Attack", false); }

			//Crouch key (require "JP script extension" asset)
			if(IsOnGround && Input.GetKey(KeyCode.LeftControl)) { crouch_T = crouch_max*transform.localScale.x; IsCrouching=true; }
			else IsCrouching=false;

			//Fly/swim up/down key
			if(CanFly | CanSwim)
			{
				if(Input.GetKey(KeyCode.Mouse1))
				{
					if(Input.GetAxis("Mouse X")!=0)//Turn 
					anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), Mathf.Clamp(Input.GetAxisRaw("Mouse X")*3.0f,-1.0f, 1.0f), ang_t));
					else anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), 0, ang_t));

					if(Input.GetAxis("Mouse Y")!=0 && anm.GetInteger("Move")==3) //Pitch with mouse if is moving
					anm.SetFloat("Pitch", Mathf.LerpAngle(anm.GetFloat("Pitch"), Mathf.Clamp(Input.GetAxisRaw("Mouse Y")*3.0f,-1.0f, 1.0f), ang_t));
					else if(Input.GetKey(KeyCode.LeftControl)) anm.SetFloat ("Pitch", 1.0f);
					else if(Input.GetKey(KeyCode.Space)) anm.SetFloat ("Pitch", -1.0f);
					else anm.SetFloat("Pitch", Mathf.LerpAngle(anm.GetFloat("Pitch"), 0, ang_t));
				}
				else
				{
					if(Input.GetKey(KeyCode.LeftControl)) anm.SetFloat ("Pitch", 1.0f);
					else if(Input.GetKey(KeyCode.Space)) anm.SetFloat ("Pitch", -1.0f);
					else anm.SetFloat ("Pitch", 0.0f);
				}
			}

			//Jump key
			if(CanJump && IsOnGround && Input.GetKey(KeyCode.Space)) anm.SetInteger ("Move", 3);

			//Moving keys
			else if(Input.GetAxis("Horizontal")!=0 | Input.GetAxis("Vertical")!=0)
			{
				if(CanSwim | (CanFly&&!IsOnGround)) //Flying/swim type
				{
					if(Input.GetKey(KeyCode.Mouse1))
					{
						if(Input.GetAxis("Vertical")<0) anm.SetInteger ("Move", -1); //Backward
						else if(Input.GetAxis("Vertical")>0) anm.SetInteger ("Move", 3); //Forward
						else if(Input.GetAxis("Horizontal")>0) anm.SetInteger ("Move", -10); //Strafe-
						else if(Input.GetAxis("Horizontal")<0) anm.SetInteger ("Move", 10); //Strafe+
						else anm.SetInteger ("Move", 0);
					}
					else
					{
						if(run) anm.SetInteger ("Move", CanSwim?2:1); else  anm.SetInteger ("Move", CanSwim?1:2); 
						delta = Mathf.DeltaAngle(manager.transform.eulerAngles.y, transform.eulerAngles.y-Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))*Mathf.Rad2Deg);
						anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), Mathf.Clamp(-delta/15, -1.0f, 1.0f), ang_t)); //Turn
					}
				}
				else //Default
				{
					if(HasSideAttack) // Tail attack
					{
						if(Input.GetAxis("Vertical")<0 && !run) anm.SetInteger ("Move", 1); //Forward
						else if(Input.GetAxis("Vertical")>0) anm.SetInteger ("Move", 2); //Run
						else if(Input.GetAxis("Horizontal")>0) anm.SetInteger ("Move", -10); //Strafe-
						else if(Input.GetAxis("Horizontal")<0) anm.SetInteger ("Move", 10); //Strafe+
					}
					else if(Input.GetKey(KeyCode.Mouse1))
					{
						if(Input.GetAxis("Vertical")>0 && !run) anm.SetInteger ("Move", 1); //Forward
						else if(Input.GetAxis("Vertical")>0) anm.SetInteger ("Move", 2); //Run
						else if(Input.GetAxis("Vertical")<0) anm.SetInteger ("Move", -1);	//Backward
						else if(Input.GetAxis("Horizontal")>0) anm.SetInteger ("Move", -10); //Strafe-
						else if(Input.GetAxis("Horizontal")<0) anm.SetInteger ("Move", 10); //Strafe+
						anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), Input.GetAxis("Mouse X"), ang_t)); //Turn
					}
					else
					{
						delta = Mathf.DeltaAngle(manager.transform.eulerAngles.y, transform.eulerAngles.y-Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))*Mathf.Rad2Deg);
						if(!run)
						{
							if(delta>135) anm.SetInteger ("Move", -10); //Turn
							else if(delta<-135) anm.SetInteger ("Move", 10); //Turn
							else anm.SetInteger ("Move", 1); //Walk
						}
						else
						{
							if(delta>135 | delta<-135) anm.SetInteger ("Move", 1); //Walk
							else anm.SetInteger ("Move", 2); //Run
						}
						anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), Mathf.Clamp(-delta/45, -1.0f, 1.0f), ang_t)); //Turn
					}
				}
			}
			//Stoped
			else
			{
				if(CanSwim | (CanFly && !IsOnGround)) //Flying/Swim
				{
					if(CanSwim && anm.GetFloat("Pitch")!=0 && !Input.GetKey(KeyCode.Mouse1))
					{
						if(run) anm.SetInteger ("Move", 2); else anm.SetInteger ("Move", 1);
					} else anm.SetInteger ("Move", 0);

					anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), 0.0f, ang_t));
				}
				else //Terrestrial
				{
					if(HasSideAttack) //Tail attack
					{	
						delta = Mathf.DeltaAngle(manager.transform.eulerAngles.y, transform.eulerAngles.y);
						if(delta>-135 && delta<0 && anm.GetBool("Attack")) anm.SetInteger ("Move", 10);
						else if(delta<135 && delta>0 && anm.GetBool("Attack")) anm.SetInteger ("Move", -10); 
						else anm.SetInteger ("Move", 0);
						anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), 0.0f, ang_t));
					}
					else if(Input.GetKey(KeyCode.Mouse1))
					{
						if(Input.GetAxis("Mouse X")>0) anm.SetInteger ("Move", -10); //Strafe- 
						else if(Input.GetAxis("Mouse X")<0) anm.SetInteger ("Move", 10); //Strafe+
						else anm.SetInteger ("Move", 0);
						anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), Mathf.Clamp(Input.GetAxis("Mouse X"), -1.0f, 1.0f), ang_t));
					}
					else { anm.SetInteger ("Move", 0); anm.SetFloat("Turn", Mathf.LerpAngle(anm.GetFloat("Turn"), 0.0f, ang_t)); } //Stop
				}
			}

			//Idles
			if(Input.GetKey(KeyCode.E))
			{
				if(Input.GetKeyDown(KeyCode.E))
				{
					int idles_lenght=0; if(idle1>0) idles_lenght++; if(idle2>0) idles_lenght++; if(idle3>0) idles_lenght++; if(idle4>0) idles_lenght++; //idles to play
					rndIdle = Random.Range(1, idles_lenght+1);
				}
				if(rndIdle==1) anm.SetInteger ("Idle", idle1);
				else if(rndIdle==2) anm.SetInteger ("Idle", idle2);
				else if(rndIdle==3) anm.SetInteger ("Idle", idle3);
				else if(rndIdle==4) anm.SetInteger ("Idle", idle4);
			}
			else if(Input.GetKey(KeyCode.F)) //Eat / Drink
			{
				if(vectorTGT==Vector3.zero) FindPlayerFood(); //looking for food
				//Drink
				if(IsOnWater)
				{
					anm.SetInteger ("Idle", drink);
					if(Water<100) { behavior="Drink"; Water+=0.05f; }
					if(Health<25) Health+=0.05f;
					if(Input.GetKeyUp(KeyCode.F)) vectorTGT=Vector3.zero;
					else vectorTGT=transform.position;
				}
				//Eat
				else if(vectorTGT!=Vector3.zero)
				{
					anm.SetInteger ("Idle", eat); behavior="Eat";
					if(Food<100) Food+=0.05f;
					if(Water<25) Water+=0.05f;
					if(Health<25) Health+=0.05f;
					if(Input.GetKeyUp(KeyCode.F)) vectorTGT=Vector3.zero;
				}
				//nothing found
				else manager.message=1;
			}
			//Sleep/Sit
			else if(Input.GetKey(KeyCode.Q))
			{ 
				anm.SetInteger("Idle", sleep);
				if(anm.GetInteger("Move")!=0) anm.SetInteger ("Idle", 0);
			}
			//Rise
			else if(rise!=0 && Input.GetKey(KeyCode.Space)) anm.SetInteger ("Idle", rise);
			else { anm.SetInteger ("Idle", 0); vectorTGT=Vector3.zero; }
			
			if(anm.GetCurrentAnimatorStateInfo(0).IsName(specie+"|Sleep") && Fatigue<100)
			{ behavior="Sleep";  Fatigue+=0.05f; if(Health<100) Health+=0.05f; }
		}
		// Not current camera target, reset parameters
		else if(AnimSpeed>0)
		{
			anm.SetFloat ("Turn", 0.0f); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", 0);
			if(CanAttack) anm.SetBool ("Attack", false);
			if(CanFly | CanSwim) anm.SetFloat ("Pitch", 0.0f);
		}
	}


//***********************************************************************************************************************************************************************************************************
//TARGET LOOKING (Randomly look around and crouch or target looking)
	public void TargetLooking(float spineX, float spineY, float crouch)
	{

		if(objectTGT)
		{
			if(behavior.EndsWith("Hunt") | behavior.Equals("Battle") |  behavior.EndsWith("Contest") | behavior.Equals("Eat")  )
			lookTGT=objectTGT.gameObject;
			else if(lookTGT && loop==0) lookTGT=null;
		} else if(lookTGT && loop==0) lookTGT=null;


		if(lookTGT) // Target
		{
			Quaternion dir;
			if(lookTGT.tag.Equals("Creature")) dir= Quaternion.LookRotation(lookTGT.GetComponent<Rigidbody>().worldCenterOfMass-fixedHeadPos);
			else dir= Quaternion.LookRotation(lookTGT.transform.position-fixedHeadPos);
			spineX_T=yaw_max*-(Mathf.DeltaAngle(dir.eulerAngles.y, transform.eulerAngles.y)/(180-yaw_max));
			spineY_T=pitch_max*(Mathf.DeltaAngle(dir.eulerAngles.x, transform.eulerAngles.x)/(90-pitch_max));
		}
		else // No target
		{
			// Is turning ? Rotate spine to direction
			if(Mathf.RoundToInt((anm.GetFloat("Turn")*100))!=0) { spineX_T=anm.GetFloat("Turn")*yaw_max; spineY_T =0.0f; }
			// Randomly look around
			else
			{
				if(Mathf.RoundToInt(spineX*100)==Mathf.RoundToInt(spineX_T*100)) spineX_T = Random.Range(-yaw_max, yaw_max);
				if(Mathf.RoundToInt(spineY*100)==Mathf.RoundToInt(spineY_T*100)) spineY_T = Random.Range(-pitch_max/2, pitch_max/2);
			}
		}

		//Crouch anim (require "JP script extension" asset)
		if(anm.GetInteger("Move")==0 && !IsCrouching) { crouch_T=Mathf.PingPong(Time.time/4 , (crouch_max*transform.localScale.x)/2); }
		else if(!IsCrouching) crouch_T=0.0f;

		if(IsOnGround && !OnLevitation && !IsAttacking && IsCrouching) anm.speed=AnimSpeed/1.5f; //Crouch speed modifer
		else anm.speed = AnimSpeed;//Default speed

	}

//***********************************************************************************************************************************************************************************************************
// FIND PLAYER FOOD
bool FindPlayerFood()
{
		//Find carnivorous food (looking for a dead creature in range)
		if(regime.Equals("Carnivorous"))
		{
			foreach (GameObject element in manager.creaturesList.ToArray())
			{
				if((element.transform.position-fixedHeadPos).magnitude>scale.z) continue; //not in range
				shared otherCreature= element.GetComponent<shared>(); //Get other creature script
				if(otherCreature.IsDead) { objectTGT=otherCreature.gameObject; vectorTGT = otherCreature.body.worldCenterOfMass; return true; } // meat found
			}
		}
		else
		{
			//Find herbivorous food (looking for trees/details on terrain in range )
			if(manager.terrain)
			{
				//Large creature, look for trees
				if(withersSize>8) 
				{
					Vector3 V1=Vector3.zero;  float i=0; RaycastHit hit;
					while(i<360)
					{
						V1=transform.position+(Quaternion.Euler(0, i, 0)*Vector3.forward*(scale.y*2));
						if(Physics.Linecast(V1+Vector3.up*withersSize, transform.position+Vector3.up*withersSize, out hit, manager.treeLayer))
						{ vectorTGT = hit.point; return true; } //tree found
						else { i++; V1=Vector3.zero; } // not found, continue
					}
				}
				//Look for grass detail
				else
				{
					TerrainData data=manager.terrain.terrainData;
					int res= data.detailResolution, layer=0;
					float x = ((transform.position.x - manager.terrain.transform.position.x) / data.size.z * res), y = ((transform.position.z - manager.terrain.transform.position.z) / data.size.x * res);

					for(layer=0; layer<data.detailPrototypes.Length; layer++)
					{
						if(data.GetDetailLayer( (int) x,  (int) y, 1, 1, layer) [ 0, 0]>0)
						{
							vectorTGT.x=(data.size.x/res)*x+manager.terrain.transform.position.x;
							vectorTGT.z=(data.size.z/res)*y+manager.terrain.transform.position.z;
							vectorTGT.y = manager.terrain.SampleHeight( new Vector3(vectorTGT.x, 0, vectorTGT.z)); 
							objectTGT=null; return true; 
						}
					}
				}
			}
		}

		objectTGT=null; vectorTGT=Vector3.zero; return false; //nothing found...
}

//***********************************************************************************************************************************************************************************************************
// MANAGE COLLISIONS, DAMAGES AND BLOOD FX
	public void ManageCollision(Collision col, float pitch_max, float crouch_max, AudioSource[] source, AudioClip pain, AudioClip Hit_jaw, AudioClip Hit_head, AudioClip Hit_tail)
	{
		//Exemple weapon collision
		if(col.gameObject.name.Equals("MyWeapon"))
		{
			SpawnBlood(col, source, pain, scale.z); //spawn blood, play creature pain sound
			Health-=  Mathf.Clamp( 20 - (scale.z*ArmorMultiplier), 0.1f, 100); //Take damages 20hp - armor value
			source[1].PlayOneShot(Hit_head, 1.0f); //hit sound
			Destroy(col.gameObject); //destroy projectil
			objectTGT=manager.transform.root.gameObject; // set the player gameobject to target
			lookTGT=objectTGT.gameObject; //look at target
			if(CanAttack) behavior="Battle"; else behavior="ToFlee"; // attack if he can, else flee
			behaviorCount=1000; //duration of this behavior
		}

		//Creatures
		else if(col.transform.root.tag.Equals("Creature"))
		{
			shared otherCreature =col.gameObject.GetComponent<shared>(); //Get other creature script

			if(objectTGT==col.transform.root.gameObject) objectCOL=objectTGT;

			//A Creature attack me
			if(lastHit==0 && otherCreature.IsAttacking)
			{

				float baseDamages=otherCreature.scale.z*otherCreature.DamageMultiplier;
				if(col.collider.gameObject.name.Equals("jaw0")) //bite damage
				{
					SpawnBlood(col, source, pain, scale.z); source[1].PlayOneShot(Hit_jaw, 1.0f);
					Health-= Mathf.Clamp( 5+baseDamages - (scale.z*ArmorMultiplier), 0.1f, 100);
				}
				else if(col.collider.gameObject.name.Equals("head")) //hit by head
				{
					body.AddExplosionForce(250, col.contacts[0].point, otherCreature.scale.z);
					SpawnBlood(col, source, pain, scale.z); source[1].PlayOneShot(Hit_head, 1.0f);
					if(regime.Equals("Carnivorous")) Health-= Mathf.Clamp( 10+baseDamages - (scale.z*ArmorMultiplier), 0.1f, 100);
					else Health-= Mathf.Clamp( baseDamages - (scale.z*ArmorMultiplier), 0.1f, 100);
				}
				else  if(!col.collider.gameObject.name.Equals("root")) //other
				{
					body.AddExplosionForce(250, col.contacts[0].point, otherCreature.scale.z);
					SpawnBlood(col, source, pain, scale.z); source[1].PlayOneShot(Hit_tail, 1.0f);
					if(regime.Equals("Carnivorous")) Health-= Mathf.Clamp( 15+baseDamages - (scale.z*ArmorMultiplier), 0.1f, 100);
					else Health-= Mathf.Clamp( baseDamages - (scale.z*ArmorMultiplier), 0.1f, 100);
				 }
			}
			//A carnivorous creature of a different species touches me
			if(objectTGT!=otherCreature.gameObject && otherCreature.regime=="Carnivorous" &&
				otherCreature.specie!=specie && !otherCreature.behavior.Equals("Hunt"))
			{
				behaviorCount=1000; objectTGT=otherCreature.gameObject;
				if(CanAttack) behavior="Battle"; else behavior="ToFlee";
			}
			//A creature touches me, look at
			else if(!lookTGT) lookTGT=otherCreature.gameObject;

			//Player attack, triggers  behavior on other creature
			if(!AI && IsAttacking)
			{
				objectTGT=otherCreature.gameObject; otherCreature.objectTGT=transform.gameObject;
				behaviorCount=1000; otherCreature.behaviorCount=1000;
				if(otherCreature.specie==specie) { behavior="Contest"; otherCreature.behavior="Contest"; }
				else if(otherCreature.CanAttack) { behavior="Battle"; otherCreature.behavior="Battle"; }
				else { behavior="Battle"; otherCreature.behavior="ToFlee"; }
			}

		}

		//Other, is not the ground, not a target... avoid
		else if(objectCOL==null && behavior.StartsWith("To") && col.gameObject!=objectTGT &&
					col.contacts[0].point.y>(transform.position.y+withersSize/4))  objectCOL=col.gameObject;
	}

	//Spawn blood particle at contact position, play creature pain sound
	void SpawnBlood(Collision col, AudioSource[] source, AudioClip pain, float size)
	{
		if(lastHit==0)
		{
			source[0].pitch=Random.Range(1.0f, 1.5f); source[0].PlayOneShot(pain, 1.0f); //pain sound
			lastHit=64;  //counter, prevent overflow
		} 	
		ParticleSystem particle=null; particle = Instantiate(blood, col.contacts[0].point, Quaternion.Euler(-90, 0, 0))as ParticleSystem; //spawn particle
		particle.transform.localScale=new Vector3(size/10, size/10, size/10); //particle size
		DestroyObject(particle.gameObject, 1.0f); //destroy particle
	}

//***********************************************************************************************************************************************************************************************************
//GET GROUND / WATER ALTITUDE (get Terrain collider or walkable/water layer altitude and normal, return y position)
	public void GetGroundAlt(bool quadruped, float crouch)
	{
		Vector3 normal=Vector3.zero; RaycastHit hit;

		//Use raycast, can walk on any kind of collider with "walkable'' layer
		if(manager.UseRaycast) 
		{
			if(Physics.Raycast(transform.position+Vector3.up*withersSize, -Vector3.up, out hit, withersSize*2, manager.walkableLayer))
			{ terrainY = hit.point.y; normal=hit.normal; }  else terrainY=-65536;
		}
		// Unity "Terrain collider" only
		else
		{
			terrainY=manager.terrain.SampleHeight(transform.position)+manager.terrain.GetPosition().y;
			float res=manager.terrain.terrainData.heightmapResolution;
			float x = ((transform.position.x -manager.terrain.transform.position.x) / manager.terrain.terrainData.size.x ) * res;
			float y = ((transform.position.z - manager.terrain.transform.position.z) / manager.terrain.terrainData.size.z ) * res;
			if(x>res | x<0 | y>res | y<0) posY=transform.position.y;
			else normal=manager.terrain.terrainData.GetInterpolatedNormal(x/res, y/res);
		}

		//Freeze Rigidbody/disable collision
		if(!IsDead)
		{
		 	body.detectCollisions=true;
			if(IsConstrained) body.constraints=RigidbodyConstraints.FreezeAll; else
			body.constraints=RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		}
		else
		{ 
			body.detectCollisions=false;
			body.constraints=RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		}

		//Is in water
		if(transform.position.y<waterY)
		{
			if((transform.position.y-waterY)>-body.centerOfMass.y-transform.localScale.x) { IsOnWater=true; IsInWater=false; } //On water
			else { IsOnWater=false; IsInWater=true; } //Underwater
		}
		//Not In/on water
		else { IsOnWater=false; IsInWater=false; }

		//Not on ground
		if((transform.position.y-terrainY)>transform.localScale.x)
		{
			IsOnGround=false;
			if(transform.position.y<waterY) IsInWater=true; else IsInWater=false;
			transform.rotation=Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), 0.02f);
		}
		//On ground
		else
		{
			if(IsConstrained | OnLevitation)
			{
				IsOnGround=true;
				transform.rotation=Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal), 0.02f);
			}
			else
			{
				IsOnGround=true;
				if(normal!=Vector3.zero)
				{
					Quaternion Normal=Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal);
					float pitch = Mathf.Clamp(Mathf.DeltaAngle(Normal.eulerAngles.x, 0.0f), -20, 20), roll = Mathf.Clamp(Mathf.DeltaAngle(Normal.eulerAngles.z, 0.0f), -8, 8);
					transform.rotation=Quaternion.Lerp(transform.rotation, Quaternion.Euler(-pitch, transform.eulerAngles.y, -roll), 0.02f);
				}
			}
		}

		if(IsInWater && !IsDead) posY=waterY-body.centerOfMass.y;
		else if(IsOnGround) posY=terrainY;
		else
		{
			posY=transform.position.y;
			transform.rotation=Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), 0.02f);
		}
	}


//***********************************************************************************************************************************************************************************************************
//
// FEET IINVERSE KINEMATICS
//
//***********************************************************************************************************************************************************************************************************
//***********************************************************************************************************************************************************************************************************
//QUADRUPED
	public void QuadIK(Transform RArm1, Transform RArm2, Transform RArm3, Transform LArm1, Transform LArm2, Transform LArm3,
										Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform LLeg1, Transform LLeg2, Transform LLeg3)
	{
		if(manager.UseIK) manager.message=2; manager.UseIK=false; return;
	}
//***********************************************************************************************************************************************************************************************************
//SMALL BIPED
	public void SmallBipedIK(Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform RLeg4,
													Transform LLeg1, Transform LLeg2, Transform LLeg3, Transform LLeg4)
	{
		if(manager.UseIK) manager.message=2; manager.UseIK=false; return;
	}
//***********************************************************************************************************************************************************************************************************
//LARGE BIPED
	public void LargeBipedIK(Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform RLeg4,
												Transform LLeg1, Transform LLeg2, Transform LLeg3, Transform LLeg4)
	{
		if(manager.UseIK) manager.message=2; manager.UseIK=false; return;
	}
//***********************************************************************************************************************************************************************************************************
//CONVEX QUADRUPED
	public void ConvexQuadIK(Transform RArm1, Transform RArm2, Transform RArm3, Transform LArm1, Transform LArm2, Transform LArm3,
													Transform RLeg1, Transform RLeg2, Transform RLeg3,Transform LLeg1, Transform LLeg2, Transform LLeg3)
	{
		if(manager.UseIK) manager.message=2; manager.UseIK=false; return;
	}
//***********************************************************************************************************************************************************************************************************
//FLYING
	public void FlyingIK(Transform RArm1, Transform RArm2, Transform RArm3, Transform LArm1, Transform LArm2, Transform LArm3,
										Transform RLeg1, Transform RLeg2, Transform RLeg3, Transform LLeg1, Transform LLeg2, Transform LLeg3)
	{
		if(manager.UseIK) manager.message=2; manager.UseIK=false; return;
	}

//***********************************************************************************************************************************************************************************************************
//
// ARTIFICIAL INTELLIGENCE
//
//***********************************************************************************************************************************************************************************************************


//***********************************************************************************************************************************************************************************************************
//AI CORE (AI entry point for all JP creatures)
	public void AICore(int idle1=0, int idle2=0, int idle3=0, int idle4=0, int eat=0, int drink=0, int sleep=0)
	{
		 if(AI) manager.message=2; AI=false; return;
	}
}


