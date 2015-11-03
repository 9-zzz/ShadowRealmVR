using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO.Ports;

public class PortGetter : MonoBehaviour
{
	public static PortGetter S;

	public bool cardplaced = false;

    public Text screenText;
    SerialPort sp = new SerialPort("COM4", 115200);


	void Awake()
	{
		S = this;
	}

    // Use this for initialization
    void Start()
    {

        sp.Open();
        //sp.ReadTimeout = 100;

        StartCoroutine(waitAndRead(4.0f));

    }

    // Update is called once per frame
    void Update()
    {
		//Debug.Log(sp.ReadLine());
    }

    public IEnumerator waitAndRead(float waitTime)
    {
        while (true)
        {
			yield return new WaitForSeconds(waitTime);

            if (sp.IsOpen)
            {
                if (sp.ReadLine() != null)
                {
                    //Debug.Log("true");
                    Debug.Log(">>> " + sp.ReadLine());
					cardplaced = true;

                    //if (sp.ReadLine().CompareTo("UID Value: 0x04 0x2D 0x9A 0x5A 0xA3 0x40 0x81") == 0)
                       // screenText.text = "YOU PUT THE BLUE EYES WHITE DRAGON ON THE READER!!!";
					//sp.BaseStream.Flush();

				}
				sp.BaseStream.Flush();
			}
			
            //sp.BaseStream.Flush();

            
        }
    }

}
