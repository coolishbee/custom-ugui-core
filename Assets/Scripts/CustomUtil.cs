using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUI
{
    public class CustomUtil
    {
        static public void Play(string name)
        {
            Play(name, null);
        }
        static public GameObject Play(string name, GameObject target)
        {
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }
            GameObject go = null;
            Debug.Log("사운드 재생");
            return go;
        }
    }
}