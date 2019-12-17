using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Explosion : MonoBehaviour
{
    private float deathCount;
    public Text deathText;
    // Start is called before the first frame update
    void Start()
    {
        deathCount = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += new Vector3(0.3f, 0f, 0.3f) * Time.deltaTime;

        deathText.text = "Deaths: " + deathCount.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Agent"))
        {
            Destroy(other.gameObject);
            deathCount++;
            Debug.Log(deathCount);
        }
    }
}
