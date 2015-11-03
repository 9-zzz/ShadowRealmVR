using UnityEngine;
using System.Collections;

public class scaler : MonoBehaviour {

	public GameObject beam;
	public float speed;
	public GameObject ps;
	public Vector3 startingscale;

	// Use this for initialization
	void Start () {
		startingscale = transform.localScale;

		transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {

		if (PortGetter.S.cardplaced) {
			ps.SetActive(true);
			transform.localScale = Vector3.Lerp (transform.localScale, startingscale, (speed * Time.deltaTime));
		}

		if (transform.localScale == startingscale)
			beam.SetActive (true);
	}
}
