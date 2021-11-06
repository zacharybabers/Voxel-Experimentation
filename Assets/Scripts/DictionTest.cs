using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DictionTest : MonoBehaviour
{

    public Dictionary<int, string> Chicken;
    // Start is called before the first frame update
    void Start()
    {
        Chicken = new Dictionary<int, string>();

        for (int i = 0; i < 10; i++)
        {
            Chicken.Add(i, "");
        }

        for (int i = 0; i < 10; i++)
        {
            print(Chicken.ElementAt(i).Key);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
