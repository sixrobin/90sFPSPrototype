namespace Doomlike
{
    using UnityEngine;

    public class LogsFolderOpener : MonoBehaviour
    {
        private void Update()
        {
            if (Input.anyKeyDown)
            {
                Manager.ApplicationManager.LogRealtimeSinceStartup();
                Manager.ApplicationManager.OpenPersistentDataPath();
                Manager.ApplicationManager.Quit();
            }
        }
    }
}