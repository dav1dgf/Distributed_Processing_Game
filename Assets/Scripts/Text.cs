using UnityEngine;
using TMPro;

public class Text : MonoBehaviour
{
    public TMP_Text myText;  // Reference to the TextMeshPro component

    void Start()
    {
        // Set initial text
        myText.text = "Hello, World!";
    }

    void Update()
    {
        // Change text dynamically (optional)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myText.text = "Space key pressed!";
        }
    }

    void SetText(string s)
    {
        myText.text = s;
    }
}
