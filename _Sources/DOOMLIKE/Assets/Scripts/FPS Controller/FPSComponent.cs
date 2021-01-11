namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    /// <summary>
    /// Abstract class that every FPS component should extend.
    /// Contains a reference to the related FPSMaster instance.
    /// </summary>
    public abstract class FPSComponent : MonoBehaviour
    {
        protected FPSMaster FPSMaster { get; private set; }

        public void SetFPSMaster(FPSMaster master)
        {
            if (FPSMaster != null)
                ConsoleProLogger.LogMisc($"Overriding FPSMaster on <b>{transform.name}</b>.", gameObject);

            FPSMaster = master;
        }
    }
}