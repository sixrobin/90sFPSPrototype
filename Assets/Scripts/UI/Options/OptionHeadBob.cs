﻿namespace Doomlike.UI
{
    using UnityEngine;

    public class OptionHeadBob : OptionRaycasterValues
    {
        public override void Init()
        {
        }

        public override string FormatValueDisplay(string display)
        {
            return $"{display}<b>%</b>";
        }

        public override void OnValueChanged(string value)
        {
            if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bobMult))
            {
                Debug.LogError($"Could not parse {value} to a float value!");
                return;
            }

            Manager.ReferencesHub.FPSMaster.FPSHeadBob.SetPercentage(bobMult);
        }
    }
}