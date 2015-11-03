using UnityEngine;
using System.Collections;

public class Beam : MonoBehaviour
{
	GameObject beamHitParticles;
	ParticleSystem bhp;
	public float beamRotationSpeed = 400.0f;
	public float beamExtendSpeed = 10.0f;
	public float zScaleFactor = 20.0f;
	public float distanceToHitPoint;
	
	void Awake()
	{
		// Make sure the particles system is the first child of THIS extendy beam cube.
		// And that the particle system does NOT Play on Awake!
		//beamHitParticles = transform.parent.GetChild(1).gameObject; // So it doesn't scale the particles, made it a sibling.
		//bhp = beamHitParticles.GetComponent<ParticleSystem>();
	}
	
	void Update()
	{
		RaycastHit hit;
		
		if (Physics.Raycast(transform.position, transform.forward, out hit))
		{
			distanceToHitPoint = Vector3.Distance(transform.position, hit.point);
			
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, (distanceToHitPoint * zScaleFactor)), (beamExtendSpeed * Time.deltaTime)); // Because distance-units != scale-units.
			
			//beamHitParticles.transform.position = hit.point;
			//beamHitParticles.transform.position = Vector3.Lerp(beamHitParticles.transform.position, hit.point, 20*Time.deltaTime); 
			// ^ Ends up being weird because it has to travel to the hit point everytime since in the else I let it stay where it was.
			// If I could keep it on the end of the beam cube, extended or unextended that would work.
		}
		else
		{
			//bhp.Stop(); // Oops we hit nothing, scale the laser back and stop the particles!
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, transform.localScale.z), (beamExtendSpeed * Time.deltaTime));
			// This is screwy but its a nice effect. It messed with the collider though I think... Seems to work, will keep it.
			//transform.localScale = Vector3.one; // Simply just makes it disappear essentially.
		}
		
		transform.Rotate(0, 0, (Time.deltaTime * beamRotationSpeed));
	}
	
	void OnTriggerEnter(Collider other)
	{
		//bhp.Play();
	}
	
}