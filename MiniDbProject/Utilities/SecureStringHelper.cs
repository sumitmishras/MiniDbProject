using System.Runtime.InteropServices;
using System.Security;

namespace MiniDbProject.Utilities;

public static class SecureStringHelper
{
    public static SecureString ToSecureString(string plainText)
    {
        var secure = new SecureString();
        foreach (char c in plainText)
        {
            secure.AppendChar(c);
        }
        secure.MakeReadOnly();
        return secure;
    }

    public static string ToPlainString(SecureString secureString)
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToBSTR(secureString);
            return Marshal.PtrToStringBSTR(ptr) ?? "";
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.ZeroFreeBSTR(ptr);
        }
    }

    public static bool CompareSecureStrings(SecureString s1, SecureString s2)
    {
        IntPtr ptr1 = IntPtr.Zero;
        IntPtr ptr2 = IntPtr.Zero;
        try
        {
            ptr1 = Marshal.SecureStringToBSTR(s1);
            ptr2 = Marshal.SecureStringToBSTR(s2);
            string str1 = Marshal.PtrToStringBSTR(ptr1) ?? "";
            string str2 = Marshal.PtrToStringBSTR(ptr2) ?? "";
            return str1 == str2;
        }
        finally
        {
            if (ptr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(ptr1);
            if (ptr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(ptr2);
        }
    }
}
