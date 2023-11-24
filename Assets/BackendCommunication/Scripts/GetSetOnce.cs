using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAndGetOnce<T>
{
    private T value;
    private bool getOnce = false;

    public SetAndGetOnce(T v) {
        value = v;
    }

    public T GetOnce() {
        if (getOnce) throw new System.InvalidOperationException("already gotten value once");
        getOnce = true;
        return value;
    }
}
