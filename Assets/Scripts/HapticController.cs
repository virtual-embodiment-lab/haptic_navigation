using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;


public class HapticController : MonoBehaviour
{
   /* public XRController leftController;*/

  /*  public float defaultAmplitude = 0.2f;
    public float defaultDuration = 0.5f;
    public float defaultFrequency = 0.4f;*/
    public HapticImpulsePlayer hip = new HapticImpulsePlayer();



    [ContextMenu("SendHaptics")]
    /*float amplitude, float duration, float frequency*/
    public void SendHaptics(float amplitude, float duration, float frequency)
    {
        
        hip.SendHapticImpulse(amplitude,duration,frequency);
    }

}
