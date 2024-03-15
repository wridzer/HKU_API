using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class HKU_Implementation : MonoBehaviour
{
    private void Start()
    {
        HKUApiWrapper.GetUsers(HKUApiWrapper.MyUsersCallback, IntPtr.Zero);
    }
}
