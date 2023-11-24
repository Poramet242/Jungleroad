using System;
using UnityEngine;

namespace Backend
{
    public class ConfirmGameEndResultPackage
    {
        public byte PackageType;

        public ConfirmGameEndResultPackage()
        {
            PackageType = Constant.Package_GameEndResultConfirm;
        }

        public byte[] ToByte()
        {
            Debug.Log($"[ConfirmGameEndResultPackage] ToByte");
            byte[] data = new byte[10];

            data[0] = PackageType;

            return data;
        }
    }
}