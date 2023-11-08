namespace Doomlike
{
    using UnityEngine;

    public class PrototypeEndTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            ConsoleProLogger.LogMisc("Triggering Prototype End Trigger.");
            GetComponent<Collider>().enabled = false;

            Manager.ReferencesHub.FPSMaster.FPSUIController.Hide();
            Manager.ReferencesHub.FPSMaster.DisableAllComponents();
            Manager.ReferencesHub.FPSMaster.FPSCamera.CamRampsController.FadeToWhiteScreenGrayscale(1f, 4f, 1.5f, () =>
            {
                Manager.ReferencesHub.PrototypeEndCanvas.SetActive(true);
            });
        }
    }
}