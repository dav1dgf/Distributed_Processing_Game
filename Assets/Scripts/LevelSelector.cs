using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelSelector : MonoBehaviour
{
    public void ChangeLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); ;
    }
    public void ChangeLevel(int levelNumber)
    {
        SceneManager.LoadScene(levelNumber);
    }
}
