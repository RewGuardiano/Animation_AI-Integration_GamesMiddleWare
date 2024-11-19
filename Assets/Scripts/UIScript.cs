using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScript : MonoBehaviour
{

    public TextMeshProUGUI Spawnbutton;

    [SerializeField] public GameObject Snowball;
    // Start is called before the first frame update
  

    public void InstantiateBall()
    {
        Instantiate(Snowball);
    }
}
