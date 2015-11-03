using UnityEngine;
using System.Collections;

public class rainbow : MonoBehaviour
{
	
	Renderer rend;
	Color[] colors = new Color[6];
	
	
	void Awake()
	{
		rend = GetComponent<Renderer>();
		colors[0] = Color.cyan;
		colors[1] = Color.red;
		colors[2] = Color.green;
		colors[3] = new Color(255, 165, 0);
		colors[4] = Color.yellow;
		colors[5] = Color.magenta;
	}
	
	// Use this for initialization
	void Start()
	{
		this.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
		StartCoroutine(waitColorChange());
		//Destroy(gameObject, 5);
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
	
	IEnumerator waitColorChange()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.05f);
			rend.materials[0].SetColor("_EmissionColor", colors[Random.Range(0, colors.Length)]);
		}
	}
	
}