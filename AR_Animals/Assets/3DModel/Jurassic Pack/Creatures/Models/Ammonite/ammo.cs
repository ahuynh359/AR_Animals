using UnityEngine;

public class ammo : MonoBehaviour
{
	public AudioClip Waterflush, Hit_jaw, Hit_head, Hit_tail, Smallstep, Smallsplash, Ammo1, Ammo2, Ammo3;
	Transform Root, Body, Head, Tentacles, Right0, Right1, Right2, Right3, Right4, Right5, Right6, Right7, Right8,
	Left0, Left1, Left2, Left3, Left4, Left5, Left6, Left7, Left8;
	float tentaclesAng, tentaclesPitch, swimRoll, swimPitch, reverse; bool invert, reset;
	const float MAXANG=2, ANGT=0.05f;

	Vector3 dir;
	shared shared;
	AudioSource[] source;
	Animator anm;
	Rigidbody body;

	//*************************************************************************************************************************************************
	//Get components
	void Start()
	{
		Root= transform.Find ("Ammo/root");
		Body= transform.Find ("Ammo/root/body");
		Head= transform.Find ("Ammo/root/body/head");
		Tentacles = transform.Find ("Ammo/root/body/head/tentacles");

		Left0= transform.Find ("Ammo/root/body/head/tentacles/left center0");
		Left1= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1");
		Left2= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1/left center2");
		Left3= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1/left center2/left center3");
		Left4= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1/left center2/left center3/left center4");
		Left5= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1/left center2/left center3/left center4/left center5");
		Left6= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1/left center2/left center3/left center4/left center5/left center6");
		Left7= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1/left center2/left center3/left center4/left center5/left center6/left center7");
		Left8= transform.Find ("Ammo/root/body/head/tentacles/left center0/left center1/left center2/left center3/left center4/left center5/left center6/left center7/left center8");

		Right0 = transform.Find ("Ammo/root/body/head/tentacles/right center0");
		Right1= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1");
		Right2= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1/right center2");
		Right3= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1/right center2/right center3");
		Right4= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1/right center2/right center3/right center4");
		Right5= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1/right center2/right center3/right center4/right center5");
		Right6= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1/right center2/right center3/right center4/right center5/right center6");
		Right7= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1/right center2/right center3/right center4/right center5/right center6/right center7");
		Right8= transform.Find ("Ammo/root/body/head/tentacles/right center0/right center1/right center2/right center3/right center4/right center5/right center6/right center7/right center8");

		source = GetComponents<AudioSource>();
		shared= GetComponent<shared>();
		body=GetComponent<Rigidbody>();
		anm=GetComponent<Animator>();
	}

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 3); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Ammo1; break; case 1: painSnd=Ammo2; break; case 2: painSnd=Ammo3; break; }
		shared.ManageCollision(col, 0.0f, 0.0f, source, painSnd, Hit_jaw, Hit_head, Hit_tail);
	}
	void PlaySound(string name, int time)
	{
		if(time==shared.currframe && shared.lastframe!=shared.currframe)
		{
			switch (name)
			{
			case "Swim": source[1].pitch=Random.Range(0.5f, 0.75f); 
				if(shared.IsOnWater && shared.IsOnGround) source[1].PlayOneShot(Smallsplash, Random.Range(0.25f, 0.5f));
				else if(shared.IsOnWater) source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
				else if(shared.IsOnGround) source[1].PlayOneShot(Smallstep, Random.Range(0.25f, 0.5f));
				shared.lastframe=shared.currframe; break;
			case "Atk":int rnd = Random.Range(0, 2); source[0].pitch=Random.Range(0.9f, 1.1f);
				if(rnd==0) source[0].PlayOneShot(Ammo1, Random.Range(0.1f, 0.25f));
				else source[0].PlayOneShot(Ammo2, Random.Range(0.1f, 0.25f));
				shared.lastframe=shared.currframe; break;
			case "Die": source[0].pitch=Random.Range(0.8f, 1.0f); source[0].PlayOneShot(Ammo3, Random.Range(0.1f, 0.25f));
				shared.lastframe=shared.currframe; shared.IsDead=true; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		if(!shared.IsActive) { body.Sleep(); return; }
		shared.IsJumping=false; shared.IsAttacking=false; shared.OnLevitation=true; shared.IsConstrained=false; reset=false;
		AnimatorStateInfo CurrAnm=anm.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo NextAnm=anm.GetNextAnimatorStateInfo(0);
		dir=-Root.up.normalized;

		//Set Y position
		if(shared.IsInWater)
		{ 
			anm.SetBool("OnGround", false);
			body.mass=1; body.drag=0.5f; body.angularDrag=0.5f;
			if(shared.IsOnWater) body.AddForce(-Vector3.up*Mathf.Lerp(dir.y, 32*transform.localScale.x, 0.25f));
		}
		else
		{
			if(shared.IsOnGround)
			{
				anm.SetBool("OnGround", true);
				body.mass=1; body.drag=1; body.angularDrag=1;
				if(!shared.IsDead) shared.Health-=0.05f;
			}
			else
			{
				shared.IsJumping=true;
				body.AddForce(-Vector3.up*Mathf.Lerp(dir.y, 64*transform.localScale.x, 0.25f));
				body.mass=1; body.drag=0.5f; body.angularDrag=0.5f;
			}
		} 

		//Stopped
		if(NextAnm.IsName("Ammo|IdleA") | CurrAnm.IsName("Ammo|IdleA") |
			CurrAnm.IsName("Ammo|Die") | CurrAnm.IsName("Ammo|DieGround"))
		{
			if(NextAnm.IsName("Ammo|IdleA") | CurrAnm.IsName("Ammo|IdleA"))
			{
				transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
				swimRoll = Mathf.Lerp(swimRoll, 0.0f, 0.01f);
				swimPitch = Mathf.Lerp(swimPitch, (invert?-anm.GetFloat("Pitch"):anm.GetFloat("Pitch"))*90, 0.005f);
			}
			else
			{
				reset=true; shared.OnLevitation=false; shared.IsConstrained=true;
				if(!shared.IsDead) PlaySound("Die", 2);
			}
		}

		//Forward
		else if(NextAnm.IsName("Ammo|Swim") | CurrAnm.IsName("Ammo|Swim"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			swimRoll = Mathf.Lerp(swimRoll, anm.GetFloat("Turn")*24, 0.025f);
			swimPitch = Mathf.Lerp(swimPitch, (invert?-anm.GetFloat("Pitch"):anm.GetFloat("Pitch"))*90, 0.01f);
			if(shared.IsInWater && CurrAnm.normalizedTime > 0.4) body.AddForce((invert?-dir:dir)*2*transform.localScale.x*anm.speed);
			PlaySound("Swim", 5);
		}

		//Running
		else if(NextAnm.IsName("Ammo|SwimFast") | CurrAnm.IsName("Ammo|SwimFast"))
		{
			invert=false;
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			swimRoll = Mathf.Lerp(swimRoll, anm.GetFloat("Turn")*90, 0.025f);
			swimPitch = Mathf.Lerp(swimPitch, anm.GetFloat("Pitch")*90, 0.01f);
			if(shared.IsInWater) body.AddForce(dir*5*transform.localScale.x*anm.speed);
			PlaySound("Swim", 5); PlaySound("Swim", 10);
		}
		
		//Backward/Strafe
		else if(NextAnm.IsName("Ammo|Swim-") | CurrAnm.IsName("Ammo|Swim-"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			swimRoll = Mathf.Lerp(swimRoll, anm.GetFloat("Turn")*24, 0.025f);
			swimPitch = Mathf.Lerp(swimPitch, (invert?-anm.GetFloat("Pitch"):anm.GetFloat("Pitch"))*90, 0.01f);
			if(shared.IsInWater)
			{
				if(anm.GetInteger("Move").Equals(-1)) body.AddForce((invert?dir:-dir)*4*transform.localScale.x*anm.speed);
				else if(anm.GetInteger("Move").Equals(3))  body.AddForce((invert?-dir:dir)*4*transform.localScale.x*anm.speed);
				else if(anm.GetInteger("Move").Equals(10)) body.AddForce((invert?-Root.right.normalized:Root.right.normalized)*4*transform.localScale.x*anm.speed);
				else if(anm.GetInteger("Move").Equals(-10)) body.AddForce((invert?Root.right.normalized:-Root.right.normalized)*4*transform.localScale.x*anm.speed);
			}

			PlaySound("Swim", 5);
		}

		//Attack
		else if(NextAnm.IsName("Ammo|Atk") | CurrAnm.IsName("Ammo|Atk"))
		{
			invert=true; shared.IsAttacking=true;
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			swimRoll = Mathf.Lerp(swimRoll, anm.GetFloat("Turn")*24, 0.025f);
			swimPitch = Mathf.Lerp(swimPitch, -anm.GetFloat("Pitch")*90, 0.025f);

			if(shared.IsInWater)
			{
				if(anm.GetInteger("Move").Equals(-1)) body.AddForce(dir*6*transform.localScale.x*anm.speed);
				else if(anm.GetInteger("Move").Equals(10)) body.AddForce(-Root.right.normalized*6*transform.localScale.x*anm.speed);
				else if(anm.GetInteger("Move").Equals(-10)) body.AddForce(Root.right.normalized*6*transform.localScale.x*anm.speed);
				else body.AddForce(-dir*6*transform.localScale.x*anm.speed);
			}

			PlaySound("Atk", 5); PlaySound("Swim", 10);
		}

		//Impulse
		else if(CurrAnm.IsName("Ammo|IdleC"))
		{
			invert=false;
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			swimRoll = Mathf.Lerp(swimRoll, anm.GetFloat("Turn")*90, 0.025f);
			swimPitch = Mathf.Lerp(swimPitch, anm.GetFloat("Pitch")*90, 0.01f);
			if(shared.IsInWater) body.AddForce( dir*12*transform.localScale.x*anm.speed);

			PlaySound("Atk", 5); PlaySound("Swim", 10);
		}

		//On Ground
		else if(CurrAnm.IsName("Ammo|OnGround"))
		{
			invert=false;
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			anm.SetFloat("Pitch", 0.0f); swimPitch = Mathf.Lerp(swimPitch, 0.0f , 0.01f);
			swimRoll = Mathf.Lerp(swimRoll, 0.0f , 0.025f);

			if(anm.GetInteger("Move").Equals(-1)) body.AddForce(-dir*32*transform.localScale.x*anm.speed);
			else if(!anm.GetInteger("Move").Equals(0)) body.AddForce(dir*32*transform.localScale.x*anm.speed);

			PlaySound("Swim", 5); PlaySound("Swim", 10);
		}
		else if(CurrAnm.IsName("Ammo|Eat")) { invert=true;  reset=true; PlaySound("Atk", 1); swimPitch = Mathf.Lerp(swimPitch, -anm.GetFloat("Pitch")*90, 0.005f); }
		else if(CurrAnm.IsName("Ammo|ToHide") | CurrAnm.IsName("Ammo|ToHide-") ) reset=true;
		else if(CurrAnm.IsName("Ammo|Die-")) { invert=false; PlaySound("Atk", 1);  shared.IsDead=false; }
	}

	void LateUpdate()
	{
		//*************************************************************************************************************************************************
		// Bone rotation
		if(!shared.IsActive) return;

		//Set const varialbes to shared script
		shared.crouch_max=0.0f;
		shared.ang_t=ANGT;
		shared.yaw_max=0.0f;
		shared.pitch_max=0.0f;

		//Save head position befor transformation
		shared.fixedHeadPos=Head.position;

		if(shared.lastHit!=0)	//Taking damage animation
		{
			shared.lastHit--;
			Head.GetChild(0).transform.rotation*= Quaternion.Euler(0, shared.lastHit, 0);
		}
		else if(reset) //Reset
		{
			swimRoll = Mathf.Lerp(swimRoll, 0.0f, 0.01f); anm.SetFloat("Turn", Mathf.Lerp(anm.GetFloat("Turn"), 0.0f, 0.01f ));
			swimPitch = Mathf.Lerp(swimPitch, 0.0f, 0.01f); anm.SetFloat("Pitch", Mathf.Lerp(anm.GetFloat("Pitch"), 0.0f, 0.01f ));	
		}

		//Pitch/roll root
		Root.transform.RotateAround(transform.position, Vector3.up, reverse = Mathf.Lerp(reverse, invert?180f : 0.0f, 0.05f) );
		Root.transform.rotation*= Quaternion.Euler(Mathf.Clamp(-swimPitch, -90, 90), Mathf.Clamp(swimRoll, -25, 25), 0);

		//Tantacles rotation
		if(!shared.IsAttacking && !reset)
		{
			if(Mathf.Abs(swimPitch-(anm.GetFloat("Pitch")*90))<45) tentaclesPitch = Mathf.Lerp(tentaclesPitch, 0.0f, 0.025f);
			else tentaclesPitch = Mathf.Lerp(tentaclesPitch, invert?-anm.GetFloat("Pitch")*8 : anm.GetFloat("Pitch")*8, 0.1f);
			tentaclesAng = Mathf.Lerp(tentaclesAng, anm.GetFloat("Turn")*8, 0.025f);
		}
		else { tentaclesPitch = Mathf.Lerp(tentaclesPitch, 0.0f , 0.1f); 	tentaclesAng = Mathf.Lerp(tentaclesAng, 0.0f, 0.1f); }

		Body.transform.rotation*= Quaternion.Euler(0, 0, tentaclesAng*2);
		Tentacles.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng*4);

		Right0.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right1.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right2.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right3.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right4.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right5.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right6.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right7.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);
		Right8.transform.rotation*= Quaternion.Euler(0, tentaclesPitch, tentaclesAng);

		Left0.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left1.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left2.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left3.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left4.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left5.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left6.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left7.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);
		Left8.transform.rotation*= Quaternion.Euler(-tentaclesPitch, 0, tentaclesAng);

		//Check for ground layer
		shared.GetGroundAlt(false, 0.0f); 

		//*************************************************************************************************************************************************
		// CPU (require "JP script extension" asset)
		if(shared.AI && shared.Health!=0) { shared.AICore(1, 2, 3, 0, 4, 0, 5); }
		//*************************************************************************************************************************************************
		// Human
		else if(shared.Health!=0) { shared.GetUserInputs(1, 2, 3, 0, 4, 0, 5); }
		//*************************************************************************************************************************************************
		//Dead
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }
	}
}










