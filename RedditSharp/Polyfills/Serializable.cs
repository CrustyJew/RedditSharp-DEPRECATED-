#if !NET462
namespace System
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate)]
    public class SerializableAttribute : Attribute
    {
    }
}
#endif