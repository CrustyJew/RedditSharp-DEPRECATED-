#if !NET462
#pragma warning disable 1591
namespace System
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate)]
    public class SerializableAttribute : Attribute
    {
    }
}
#pragma warning restore 1591
#endif