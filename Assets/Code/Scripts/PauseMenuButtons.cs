using UnityEngine;

public class PauseMenuButtons : MonoBehaviour
{
  
    public void OnResume()
    {
        PauseManager.Instance.ResumeGame();
    }

   
    public void OnSave()
    {
        PauseManager.Instance.SaveGame();
    }

    public void OnLoad()
    {
        PauseManager.Instance.LoadGame();
    }

 
    public void OnBack()
    {
        PauseManager.Instance.BackToMenu();
    }
}
